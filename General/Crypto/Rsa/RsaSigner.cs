using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Signers;

namespace General.Crypto.Rsa
{
    /// <summary>
    /// RSA 數位簽章，採 RSASSA-PSS + SHA-256。
    /// </summary>
    /// <remarks>
    /// <para>簽章需私鑰，驗證只需公鑰。</para>
    /// <para>
    /// 簽章用於證明資料的「來源」與「未被竄改」，但**不提供機密性** ——
    /// 資料本身仍是明文。若同時需要保密，請搭配 <see cref="RsaEncryptor"/>。
    /// </para>
    /// <para>
    /// PSS 每次簽章都會帶入隨機 salt，因此對相同資料重複簽章會得到不同的簽章值，
    /// 這是正常且符合規範的行為，兩者都能通過驗證。
    /// </para>
    /// </remarks>
    public sealed class RsaSigner : IDisposable
    {
        /// <summary>PSS 的 salt 長度（bytes）。取雜湊長度，與 .NET 及 OpenSSL 的預設一致。</summary>
        private const int SaltLength = 32;

        private readonly RsaKey _key;
        private bool _disposed;

        /// <summary>
        /// 建立簽章器。傳入的 <see cref="RsaKey"/> 由呼叫端負責釋放，本類別不會將其 Dispose。
        /// </summary>
        /// <exception cref="ArgumentNullException">key 為 null。</exception>
        public RsaSigner(RsaKey key)
        {
            if (key == null) throw new ArgumentNullException("key");
            _key = key;
        }

        #region Sign

        /// <summary>對位元組資料簽章，回傳 Base64 簽章值。需私鑰。</summary>
        /// <exception cref="ArgumentNullException">data 為 null。</exception>
        /// <exception cref="InvalidOperationException">金鑰不含私鑰。</exception>
        public string Sign(byte[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            ThrowIfDisposed();

            if (!_key.HasPrivateKey)
                throw new InvalidOperationException("此金鑰僅含公鑰，無法簽章。");

            PssSigner signer = CreateSigner();
            signer.Init(true, _key.PrivateKeyParameters);
            signer.BlockUpdate(data, 0, data.Length);
            return Convert.ToBase64String(signer.GenerateSignature());
        }

        /// <summary>對字串簽章（以 UTF-8 編碼）。需私鑰。</summary>
        /// <exception cref="ArgumentNullException">text 為 null。</exception>
        /// <exception cref="InvalidOperationException">金鑰不含私鑰。</exception>
        public string Sign(string text)
        {
            if (text == null) throw new ArgumentNullException("text");

            return Sign(Encoding.UTF8.GetBytes(text));
        }

        #endregion

        #region Verify

        /// <summary>
        /// 驗證簽章是否吻合。只需公鑰。
        /// </summary>
        /// <remarks>
        /// 任何形式的失敗（簽章不符、格式錯誤、非 Base64、null）一律回傳 false 而不拋出例外，
        /// 以免攻擊者藉由特製輸入癱瘓驗證端。
        /// </remarks>
        public bool Verify(byte[] data, string signature)
        {
            ThrowIfDisposed();

            if (data == null || string.IsNullOrEmpty(signature)) return false;

            byte[] raw;
            try
            {
                raw = Convert.FromBase64String(signature);
            }
            catch (FormatException)
            {
                return false;
            }

            try
            {
                PssSigner signer = CreateSigner();
                signer.Init(false, _key.PublicKeyParameters);
                signer.BlockUpdate(data, 0, data.Length);
                return signer.VerifySignature(raw);
            }
            catch (Exception ex) when (ex is CryptographicException || ex is CryptoException || ex is ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// RSASSA-PSS，雜湊與 MGF1 皆為 SHA-256、salt 長度 32 bytes。
        /// 此組合等同 .NET 的 <c>RSASignaturePadding.Pss</c> 搭配 SHA-256。
        /// </summary>
        private static PssSigner CreateSigner()
        {
            return new PssSigner(new RsaEngine(), new Sha256Digest(), SaltLength);
        }

        /// <summary>驗證字串的簽章（以 UTF-8 編碼）。只需公鑰。</summary>
        public bool Verify(string text, string signature)
        {
            if (text == null) return false;

            return Verify(Encoding.UTF8.GetBytes(text), signature);
        }

        #endregion

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException("RsaSigner");
        }

        /// <summary>
        /// 標記為已釋放。傳入的 <see cref="RsaKey"/> 屬於呼叫端，此處不會將其 Dispose。
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
        }
    }
}
