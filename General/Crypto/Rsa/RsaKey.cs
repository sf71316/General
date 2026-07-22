using System;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace General.Crypto.Rsa
{
    /// <summary>
    /// RSA 金鑰。可產生新金鑰或自 PEM 載入，並匯出為標準 PEM 格式。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 匯出格式為 PKCS#8（私鑰，<c>BEGIN PRIVATE KEY</c>）與
    /// SubjectPublicKeyInfo（公鑰，<c>BEGIN PUBLIC KEY</c>），
    /// 可與 OpenSSL、Java、Python 等互通。載入時亦接受 PKCS#1
    /// （<c>BEGIN RSA PRIVATE KEY</c>）。
    /// </para>
    /// <para>
    /// 金鑰材料與運算皆以 BouncyCastle 實作，不使用 BCL 的
    /// <c>System.Security.Cryptography.RSA</c>。原因是 .NET Framework 上
    /// <c>RSA.Create()</c> 會回傳 <c>RSACryptoServiceProvider</c>，
    /// 它不支援 OAEP-SHA256 與 PSS；而支援這些的 <c>RSACng</c> 在
    /// netstandard2.0 無法取得。改用 BouncyCastle 可確保各執行環境行為一致。
    /// </para>
    /// </remarks>
    public sealed class RsaKey : IDisposable
    {
        /// <summary>可接受的最小金鑰長度（bits）。低於此長度視為不安全。</summary>
        public const int MinimumKeySizeBits = 2048;

        /// <summary>產生金鑰時的預設長度（bits）。NIST 建議 2030 年後採用 3072。</summary>
        public const int DefaultKeySizeBits = 3072;

        private RsaKeyParameters _publicKey;
        private RsaPrivateCrtKeyParameters _privateKey;
        private bool _disposed;

        private RsaKey(RsaKeyParameters publicKey, RsaPrivateCrtKeyParameters privateKey)
        {
            _publicKey = publicKey;
            _privateKey = privateKey;
        }

        #region Property

        /// <summary>是否含私鑰。僅有公鑰時無法解密或簽章。</summary>
        public bool HasPrivateKey
        {
            get { return _privateKey != null; }
        }

        /// <summary>金鑰長度（bits）。</summary>
        public int KeySizeBits
        {
            get
            {
                ThrowIfDisposed();
                return _publicKey.Modulus.BitLength;
            }
        }

        #endregion

        #region Factory

        /// <summary>產生新的 RSA 金鑰對。</summary>
        /// <param name="keySizeBits">金鑰長度，預設 <see cref="DefaultKeySizeBits"/>。</param>
        /// <exception cref="ArgumentOutOfRangeException">長度小於 <see cref="MinimumKeySizeBits"/>。</exception>
        public static RsaKey Generate(int keySizeBits = DefaultKeySizeBits)
        {
            if (keySizeBits < MinimumKeySizeBits)
                throw new ArgumentOutOfRangeException("keySizeBits",
                    "金鑰長度至少須為 " + MinimumKeySizeBits + " bits。");

            RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
            generator.Init(new KeyGenerationParameters(new SecureRandom(), keySizeBits));
            AsymmetricCipherKeyPair pair = generator.GenerateKeyPair();

            return FromPrivate((RsaPrivateCrtKeyParameters)pair.Private);
        }

        /// <summary>
        /// 自 PEM 字串載入金鑰。公鑰與私鑰皆可，會自動判別；
        /// 私鑰支援 PKCS#8 與 PKCS#1 兩種格式。
        /// </summary>
        /// <exception cref="ArgumentNullException">pem 為 null。</exception>
        /// <exception cref="CryptographicException">PEM 格式無效、非 RSA 金鑰，或長度不足。</exception>
        public static RsaKey FromPem(string pem)
        {
            if (pem == null) throw new ArgumentNullException("pem");

            object parsed;
            try
            {
                using (StringReader reader = new StringReader(pem))
                {
                    parsed = new PemReader(reader).ReadObject();
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException("PEM 格式無效。", ex);
            }

            if (parsed == null)
                throw new CryptographicException("PEM 格式無效。");

            AsymmetricCipherKeyPair keyPair = parsed as AsymmetricCipherKeyPair;
            if (keyPair != null) parsed = keyPair.Private;

            RsaPrivateCrtKeyParameters privateKey = parsed as RsaPrivateCrtKeyParameters;
            if (privateKey != null) return FromPrivate(privateKey);

            RsaKeyParameters publicKey = parsed as RsaKeyParameters;
            if (publicKey != null && !publicKey.IsPrivate)
            {
                EnsureKeySize(publicKey);
                return new RsaKey(publicKey, null);
            }

            throw new CryptographicException("PEM 內容不是 RSA 金鑰。");
        }

        private static RsaKey FromPrivate(RsaPrivateCrtKeyParameters privateKey)
        {
            RsaKeyParameters publicKey =
                new RsaKeyParameters(false, privateKey.Modulus, privateKey.PublicExponent);

            EnsureKeySize(publicKey);
            return new RsaKey(publicKey, privateKey);
        }

        private static void EnsureKeySize(RsaKeyParameters key)
        {
            int bits = key.Modulus.BitLength;
            if (bits < MinimumKeySizeBits)
                throw new CryptographicException(
                    "金鑰長度為 " + bits + " bits，低於允許的最小值 " + MinimumKeySizeBits + " bits。");
        }

        #endregion

        #region Export

        /// <summary>匯出公鑰為 SubjectPublicKeyInfo PEM（<c>-----BEGIN PUBLIC KEY-----</c>）。</summary>
        public string ToPublicPem()
        {
            ThrowIfDisposed();

            return WritePem(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(_publicKey));
        }

        /// <summary>匯出私鑰為 PKCS#8 PEM（<c>-----BEGIN PRIVATE KEY-----</c>）。</summary>
        /// <exception cref="InvalidOperationException">此金鑰不含私鑰。</exception>
        public string ToPrivatePem()
        {
            ThrowIfDisposed();

            if (_privateKey == null)
                throw new InvalidOperationException("此金鑰僅含公鑰，無法匯出私鑰。");

            // 直接把 AsymmetricKeyParameter 交給 PemWriter 會輸出 PKCS#1
            // （BEGIN RSA PRIVATE KEY），需經 Pkcs8Generator 才是 PKCS#8。
            return WritePem(new Pkcs8Generator(_privateKey, null));
        }

        private static string WritePem(object structure)
        {
            using (StringWriter writer = new StringWriter())
            {
                new PemWriter(writer).WriteObject(structure);
                return writer.ToString();
            }
        }

        #endregion

        #region Internal Accessor

        /// <summary>供同組件的加密／簽章類別取用公鑰參數。</summary>
        internal RsaKeyParameters PublicKeyParameters
        {
            get
            {
                ThrowIfDisposed();
                return _publicKey;
            }
        }

        /// <summary>供同組件的解密／簽章類別取用私鑰參數；僅有公鑰時為 null。</summary>
        internal RsaPrivateCrtKeyParameters PrivateKeyParameters
        {
            get
            {
                ThrowIfDisposed();
                return _privateKey;
            }
        }

        #endregion

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException("RsaKey");
        }

        /// <summary>釋放金鑰參照。</summary>
        public void Dispose()
        {
            if (_disposed) return;

            _publicKey = null;
            _privateKey = null;
            _disposed = true;
        }
    }
}
