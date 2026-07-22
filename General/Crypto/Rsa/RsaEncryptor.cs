using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;

namespace General.Crypto.Rsa
{
    /// <summary>
    /// RSA 混合加密（envelope encryption）。以 RSA-OAEP-SHA256 包裝一把隨機的
    /// AES-256 金鑰，資料本身則交由 <see cref="AesEncryptor"/> 以 AES-256-GCM 加密。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 格式：<c>Base64(version(1) ‖ wrappedKeyLength(2) ‖ wrappedKey ‖ AesEncryptor 密文)</c>
    /// </para>
    /// <para>
    /// RSA 本身只能加密極少量資料（RSA-2048 搭配 OAEP-SHA256 僅 190 bytes），
    /// 因此不直接用 RSA 加密資料，而是用它包裝對稱金鑰 —— 這是標準做法，
    /// 既無長度限制，資料也同時受 GCM 的完整性保護。
    /// </para>
    /// <para>
    /// 加密只需公鑰，解密需私鑰。每次加密都會產生全新的 AES 金鑰。
    /// </para>
    /// </remarks>
    public sealed class RsaEncryptor : IDisposable
    {
        /// <summary>格式版本。日後更換演算法時據此分辨舊密文。</summary>
        private const byte FormatVersion = 1;

        /// <summary>解密失敗一律回報此訊息，不區分原因，避免成為攻擊者的 oracle。</summary>
        private const string DecryptFailedMessage = "密文無效或已遭竄改。";

        private readonly RsaKey _key;
        private bool _disposed;

        /// <summary>
        /// 建立加密器。傳入的 <see cref="RsaKey"/> 由呼叫端負責釋放，本類別不會將其 Dispose。
        /// </summary>
        /// <exception cref="ArgumentNullException">key 為 null。</exception>
        public RsaEncryptor(RsaKey key)
        {
            if (key == null) throw new ArgumentNullException("key");
            _key = key;
        }

        #region Encrypt

        /// <summary>加密字串，回傳 Base64 密文。明文以 UTF-8 編碼，無長度上限。</summary>
        /// <exception cref="ArgumentNullException">plainText 為 null。</exception>
        public string Encrypt(string plainText)
        {
            if (plainText == null) throw new ArgumentNullException("plainText");

            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(plainText)));
        }

        /// <summary>加密位元組陣列，僅需公鑰。</summary>
        /// <exception cref="ArgumentNullException">plain 為 null。</exception>
        public byte[] Encrypt(byte[] plain)
        {
            if (plain == null) throw new ArgumentNullException("plain");
            ThrowIfDisposed();

            byte[] aesKey = AesEncryptor.GenerateKey();
            try
            {
                byte[] wrappedKey = WrapKey(aesKey);
                byte[] payload;
                using (AesEncryptor aes = new AesEncryptor(aesKey))
                {
                    payload = aes.Encrypt(plain);
                }

                byte[] result = new byte[1 + 2 + wrappedKey.Length + payload.Length];
                result[0] = FormatVersion;
                result[1] = (byte)(wrappedKey.Length >> 8);
                result[2] = (byte)(wrappedKey.Length & 0xFF);
                Buffer.BlockCopy(wrappedKey, 0, result, 3, wrappedKey.Length);
                Buffer.BlockCopy(payload, 0, result, 3 + wrappedKey.Length, payload.Length);
                return result;
            }
            finally
            {
                Array.Clear(aesKey, 0, aesKey.Length);
            }
        }

        #endregion

        #region Decrypt

        /// <summary>解密 Base64 密文，回傳 UTF-8 字串。需私鑰。</summary>
        /// <exception cref="InvalidOperationException">金鑰不含私鑰。</exception>
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
                throw new CryptographicException(DecryptFailedMessage);
            }

            return Encoding.UTF8.GetString(Decrypt(raw));
        }

        /// <summary>解密位元組陣列。需私鑰。</summary>
        /// <exception cref="InvalidOperationException">金鑰不含私鑰。</exception>
        /// <exception cref="CryptographicException">密文無效、遭竄改，或金鑰不符。</exception>
        public byte[] Decrypt(byte[] cipherBytes)
        {
            if (cipherBytes == null) throw new ArgumentNullException("cipherBytes");
            ThrowIfDisposed();

            // 以公鑰解密屬於程式邏輯錯誤，不是攻擊，故與密文錯誤分開回報
            if (!_key.HasPrivateKey)
                throw new InvalidOperationException("此金鑰僅含公鑰，無法解密。");

            if (cipherBytes.Length < 3)
                throw new CryptographicException(DecryptFailedMessage);
            if (cipherBytes[0] != FormatVersion)
                throw new CryptographicException(DecryptFailedMessage);

            int wrappedLength = (cipherBytes[1] << 8) | cipherBytes[2];
            if (wrappedLength <= 0 || 3 + wrappedLength > cipherBytes.Length)
                throw new CryptographicException(DecryptFailedMessage);

            byte[] wrappedKey = new byte[wrappedLength];
            Buffer.BlockCopy(cipherBytes, 3, wrappedKey, 0, wrappedLength);

            int payloadLength = cipherBytes.Length - 3 - wrappedLength;
            byte[] payload = new byte[payloadLength];
            Buffer.BlockCopy(cipherBytes, 3 + wrappedLength, payload, 0, payloadLength);

            byte[] aesKey;
            try
            {
                aesKey = UnwrapKey(wrappedKey);
            }
            // CryptoException 涵蓋 InvalidCipherTextException（OAEP 解碼失敗）與
            // DataLengthException（長度與模數不符）；ArgumentException 涵蓋其餘輸入問題
            catch (Exception ex) when (ex is CryptoException || ex is ArgumentException)
            {
                throw new CryptographicException(DecryptFailedMessage);
            }

            try
            {
                if (aesKey.Length != AesEncryptor.KeySize)
                    throw new CryptographicException(DecryptFailedMessage);

                using (AesEncryptor aes = new AesEncryptor(aesKey))
                {
                    return aes.Decrypt(payload);
                }
            }
            finally
            {
                Array.Clear(aesKey, 0, aesKey.Length);
            }
        }

        #endregion

        #region RSA-OAEP

        /// <summary>
        /// 以 RSA-OAEP-SHA256（MGF1 亦為 SHA-256）包裝／解開對稱金鑰。
        /// 此組合等同 .NET 的 <c>RSAEncryptionPadding.OaepSHA256</c> 與 OpenSSL 的
        /// <c>-pkeyopt rsa_padding_mode:oaep -pkeyopt rsa_oaep_md:sha256</c>。
        /// </summary>
        private static OaepEncoding CreateOaep()
        {
            return new OaepEncoding(new RsaEngine(), new Sha256Digest(), new Sha256Digest(), null);
        }

        private byte[] WrapKey(byte[] aesKey)
        {
            OaepEncoding engine = CreateOaep();
            engine.Init(true, _key.PublicKeyParameters);
            return engine.ProcessBlock(aesKey, 0, aesKey.Length);
        }

        private byte[] UnwrapKey(byte[] wrappedKey)
        {
            OaepEncoding engine = CreateOaep();
            engine.Init(false, _key.PrivateKeyParameters);
            return engine.ProcessBlock(wrappedKey, 0, wrappedKey.Length);
        }

        #endregion

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException("RsaEncryptor");
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
