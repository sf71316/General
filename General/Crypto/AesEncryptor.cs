using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace General.Crypto
{
    /// <summary>
    /// AES-256-GCM 可逆加密。輸出為單一 Base64 字串：
    /// <c>Base64(version(1) ‖ nonce(12) ‖ ciphertext(n) ‖ tag(16))</c>
    /// </summary>
    /// <remarks>
    /// <para>
    /// GCM 屬於 AEAD（帶驗證的加密），密文遭竄改時解密會直接失敗，
    /// 不需要另外搭配 MAC。nonce 由內部每次隨機產生並附於密文，呼叫端無從指定，
    /// 以杜絕「重複使用 nonce」這個 GCM 最致命的誤用。
    /// </para>
    /// <para>
    /// netstandard2.0 沒有原生的 <c>System.Security.Cryptography.AesGcm</c>
    /// （需 netstandard2.1 以上，而 .NET Framework 不支援 netstandard2.1），
    /// 故以 BouncyCastle 實作 GCM。
    /// </para>
    /// <para>
    /// 本類別為執行緒安全：金鑰建立後不再變動，每次加解密都使用各自的 cipher 實例。
    /// </para>
    /// </remarks>
    public sealed class AesEncryptor : IDisposable
    {
        /// <summary>金鑰長度（bytes）。32 bytes = AES-256。</summary>
        public const int KeySize = 32;

        /// <summary>Nonce 長度（bytes）。96-bit 為 GCM 標準建議值。</summary>
        public const int NonceSize = 12;

        /// <summary>驗證標籤長度（bytes）。128-bit 為完整強度。</summary>
        public const int TagSize = 16;

        /// <summary>金鑰衍生用的 salt 長度（bytes）。</summary>
        public const int SaltSize = 16;

        /// <summary>PBKDF2 預設迭代次數，採 OWASP 對 PBKDF2-HMAC-SHA256 的建議值。</summary>
        public const int DefaultIterations = 600000;

        /// <summary>格式版本。日後更換演算法時據此分辨舊密文。</summary>
        private const byte FormatVersion = 1;

        /// <summary>密文解析失敗一律回報此訊息，不區分失敗原因，避免成為攻擊者的 oracle。</summary>
        private const string DecryptFailedMessage = "密文無效或已遭竄改。";

        private byte[] _key;
        private bool _disposed;

        #region Constructor / Factory

        /// <summary>
        /// 以既有的 32 bytes 金鑰建立加密器。
        /// </summary>
        /// <exception cref="ArgumentNullException">key 為 null。</exception>
        /// <exception cref="ArgumentException">key 長度不是 <see cref="KeySize"/>。</exception>
        public AesEncryptor(byte[] key)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (key.Length != KeySize)
                throw new ArgumentException("金鑰長度必須為 " + KeySize + " bytes（AES-256）。", "key");

            _key = (byte[])key.Clone();
        }

        /// <summary>
        /// 以密碼衍生金鑰建立加密器（PBKDF2-HMAC-SHA256）。
        /// </summary>
        /// <param name="password">來源密碼。</param>
        /// <param name="salt">至少 <see cref="SaltSize"/> bytes，須與密文一同保存或另行管理。</param>
        /// <param name="iterations">迭代次數，預設 <see cref="DefaultIterations"/>。</param>
        /// <remarks>
        /// 相同的 password + salt + iterations 必然衍生出相同金鑰，
        /// 因此 salt 與 iterations 必須由呼叫端妥善保存，否則無法解密。
        /// </remarks>
        public static AesEncryptor FromPassword(string password, byte[] salt, int iterations = DefaultIterations)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (salt == null) throw new ArgumentNullException("salt");
            if (salt.Length < SaltSize)
                throw new ArgumentException("salt 長度至少須為 " + SaltSize + " bytes。", "salt");
            if (iterations < 1)
                throw new ArgumentOutOfRangeException("iterations", "迭代次數必須大於 0。");

            byte[] key = DeriveKey(password, salt, iterations);
            try
            {
                return new AesEncryptor(key);
            }
            finally
            {
                Array.Clear(key, 0, key.Length);
            }
        }

        /// <summary>產生密碼學安全的隨機金鑰，供首次部署使用。</summary>
        public static byte[] GenerateKey()
        {
            return GenerateRandom(KeySize);
        }

        /// <summary>產生供 <see cref="FromPassword"/> 使用的隨機 salt。</summary>
        public static byte[] GenerateSalt()
        {
            return GenerateRandom(SaltSize);
        }

        #endregion

        #region Encrypt / Decrypt

        /// <summary>加密字串，回傳 Base64 密文。明文以 UTF-8 編碼。</summary>
        /// <exception cref="ArgumentNullException">plainText 為 null。</exception>
        public string Encrypt(string plainText)
        {
            if (plainText == null) throw new ArgumentNullException("plainText");

            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(plainText)));
        }

        /// <summary>解密 Base64 密文，回傳 UTF-8 字串。</summary>
        /// <exception cref="CryptographicException">密文無效、遭竄改，或金鑰不符。</exception>
        public string Decrypt(string cipherText)
        {
            if (cipherText == null) throw new ArgumentNullException("cipherText");

            byte[] raw;
            try
            {
                raw = Convert.FromBase64String(cipherText);
            }
            catch (FormatException)
            {
                // 與其他失敗原因回報相同訊息
                throw new CryptographicException(DecryptFailedMessage);
            }

            return Encoding.UTF8.GetString(Decrypt(raw));
        }

        /// <summary>加密位元組陣列，回傳 version ‖ nonce ‖ ciphertext ‖ tag。</summary>
        /// <exception cref="ArgumentNullException">plain 為 null。</exception>
        public byte[] Encrypt(byte[] plain)
        {
            if (plain == null) throw new ArgumentNullException("plain");
            ThrowIfDisposed();

            byte[] nonce = GenerateRandom(NonceSize);

            GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
            cipher.Init(true, new AeadParameters(new KeyParameter(_key), TagSize * 8, nonce));

            byte[] sealedBytes = new byte[cipher.GetOutputSize(plain.Length)];
            int position = cipher.ProcessBytes(plain, 0, plain.Length, sealedBytes, 0);
            cipher.DoFinal(sealedBytes, position);

            byte[] result = new byte[1 + NonceSize + sealedBytes.Length];
            result[0] = FormatVersion;
            Buffer.BlockCopy(nonce, 0, result, 1, NonceSize);
            Buffer.BlockCopy(sealedBytes, 0, result, 1 + NonceSize, sealedBytes.Length);
            return result;
        }

        /// <summary>解密 version ‖ nonce ‖ ciphertext ‖ tag 格式的位元組陣列。</summary>
        /// <exception cref="CryptographicException">密文無效、遭竄改，或金鑰不符。</exception>
        public byte[] Decrypt(byte[] cipherBytes)
        {
            if (cipherBytes == null) throw new ArgumentNullException("cipherBytes");
            ThrowIfDisposed();

            // 至少要有 version + nonce + tag；密文本身可以是 0 bytes（加密空字串）
            if (cipherBytes.Length < 1 + NonceSize + TagSize)
                throw new CryptographicException(DecryptFailedMessage);
            if (cipherBytes[0] != FormatVersion)
                throw new CryptographicException(DecryptFailedMessage);

            byte[] nonce = new byte[NonceSize];
            Buffer.BlockCopy(cipherBytes, 1, nonce, 0, NonceSize);

            int sealedLength = cipherBytes.Length - 1 - NonceSize;
            byte[] sealedBytes = new byte[sealedLength];
            Buffer.BlockCopy(cipherBytes, 1 + NonceSize, sealedBytes, 0, sealedLength);

            GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
            cipher.Init(false, new AeadParameters(new KeyParameter(_key), TagSize * 8, nonce));

            byte[] plain = new byte[cipher.GetOutputSize(sealedLength)];
            try
            {
                int position = cipher.ProcessBytes(sealedBytes, 0, sealedLength, plain, 0);
                cipher.DoFinal(plain, position);
            }
            catch (InvalidCipherTextException)
            {
                // 驗證標籤不符 —— 密文遭竄改或金鑰錯誤
                throw new CryptographicException(DecryptFailedMessage);
            }
            return plain;
        }

        #endregion

        #region Helper

        private static byte[] DeriveKey(string password, byte[] salt, int iterations)
        {
            Pkcs5S2ParametersGenerator generator = new Pkcs5S2ParametersGenerator(new Sha256Digest());
            generator.Init(Encoding.UTF8.GetBytes(password), salt, iterations);
            KeyParameter parameter = (KeyParameter)generator.GenerateDerivedMacParameters(KeySize * 8);
            return parameter.GetKey();
        }

        private static byte[] GenerateRandom(int length)
        {
            byte[] buffer = new byte[length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }
            return buffer;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException("AesEncryptor");
        }

        #endregion

        /// <summary>釋放資源，並將記憶體中的金鑰內容清零。</summary>
        public void Dispose()
        {
            if (_disposed) return;

            if (_key != null)
            {
                Array.Clear(_key, 0, _key.Length);
                _key = null;
            }
            _disposed = true;
        }
    }
}
