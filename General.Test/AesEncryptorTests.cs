using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using General.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace General.Test
{
    [TestClass]
    public class AesEncryptorTests
    {
        private const string PlainText = "Hello World!!";

        private static AesEncryptor NewEncryptor()
        {
            return new AesEncryptor(AesEncryptor.GenerateKey());
        }

        #region Round Trip

        [TestMethod]
        public void Encrypt_Decrypt_RoundTrips()
        {
            using (var enc = NewEncryptor())
            {
                string cipher = enc.Encrypt(PlainText);

                Assert.AreNotEqual(PlainText, cipher);
                Assert.AreEqual(PlainText, enc.Decrypt(cipher));
            }
        }

        [TestMethod]
        public void Encrypt_EmptyString_RoundTrips()
        {
            using (var enc = NewEncryptor())
            {
                Assert.AreEqual(string.Empty, enc.Decrypt(enc.Encrypt(string.Empty)));
            }
        }

        [TestMethod]
        public void Encrypt_EmptyByteArray_RoundTrips()
        {
            using (var enc = NewEncryptor())
            {
                byte[] result = enc.Decrypt(enc.Encrypt(new byte[0]));

                Assert.AreEqual(0, result.Length);
            }
        }

        [DataTestMethod]
        [DataRow("中文內容測試")]
        [DataRow("パスワード")]
        [DataRow("🔐🗝️😀")]
        [DataRow("  前後空白  ")]
        [DataRow("line1\r\nline2\tend")]
        public void Encrypt_Unicode_RoundTrips(string text)
        {
            using (var enc = NewEncryptor())
            {
                Assert.AreEqual(text, enc.Decrypt(enc.Encrypt(text)));
            }
        }

        [TestMethod]
        public void Encrypt_LargePayload_RoundTrips()
        {
            byte[] plain = new byte[1024 * 1024];
            new Random(42).NextBytes(plain);

            using (var enc = NewEncryptor())
            {
                CollectionAssert.AreEqual(plain, enc.Decrypt(enc.Encrypt(plain)));
            }
        }

        [TestMethod]
        public void Decrypt_WithSeparateInstanceSharingKey_Succeeds()
        {
            byte[] key = AesEncryptor.GenerateKey();
            string cipher;

            using (var writer = new AesEncryptor(key)) cipher = writer.Encrypt(PlainText);
            using (var reader = new AesEncryptor(key)) Assert.AreEqual(PlainText, reader.Decrypt(cipher));
        }

        #endregion

        #region Nonce / Format

        [TestMethod]
        public void Encrypt_SamePlaintextTwice_ProducesDifferentCiphertext()
        {
            using (var enc = NewEncryptor())
            {
                Assert.AreNotEqual(enc.Encrypt(PlainText), enc.Encrypt(PlainText),
                    "nonce 應為隨機，相同明文不得產生相同密文");
            }
        }

        [TestMethod]
        public void Encrypt_NoncesAreUnique()
        {
            const int rounds = 200;
            var nonces = new HashSet<string>();

            using (var enc = NewEncryptor())
            {
                for (int i = 0; i < rounds; i++)
                {
                    byte[] raw = enc.Encrypt(Encoding.UTF8.GetBytes(PlainText));
                    nonces.Add(Convert.ToBase64String(raw.Skip(1).Take(AesEncryptor.NonceSize).ToArray()));
                }
            }

            Assert.AreEqual(rounds, nonces.Count, "每次加密都必須使用全新的 nonce");
        }

        [TestMethod]
        public void Encrypt_LayoutIsVersionNonceCiphertextTag()
        {
            byte[] plain = Encoding.UTF8.GetBytes(PlainText);

            using (var enc = NewEncryptor())
            {
                byte[] raw = enc.Encrypt(plain);

                Assert.AreEqual(1, raw[0], "第一個位元組應為格式版本");
                Assert.AreEqual(1 + AesEncryptor.NonceSize + plain.Length + AesEncryptor.TagSize, raw.Length,
                    "GCM 為串流模式，密文長度應等於明文長度，不做區塊填充");
            }
        }

        [TestMethod]
        public void Encrypt_ResultIsValidBase64()
        {
            using (var enc = NewEncryptor())
            {
                Convert.FromBase64String(enc.Encrypt(PlainText));
            }
        }

        #endregion

        #region Tamper Detection

        [TestMethod]
        public void Decrypt_TamperedCiphertext_Throws()
        {
            using (var enc = NewEncryptor())
            {
                byte[] raw = enc.Encrypt(Encoding.UTF8.GetBytes(PlainText));

                // 逐一翻轉每個位元組的最低位元，每一種竄改都必須被偵測到
                for (int i = 0; i < raw.Length; i++)
                {
                    byte[] tampered = (byte[])raw.Clone();
                    tampered[i] ^= 0x01;

                    Assert.ThrowsException<CryptographicException>(() => enc.Decrypt(tampered),
                        "位元組 " + i + " 遭竄改卻未被偵測");
                }
            }
        }

        [TestMethod]
        public void Decrypt_TruncatedCiphertext_Throws()
        {
            using (var enc = NewEncryptor())
            {
                byte[] raw = enc.Encrypt(Encoding.UTF8.GetBytes(PlainText));
                byte[] truncated = raw.Take(raw.Length - 1).ToArray();

                Assert.ThrowsException<CryptographicException>(() => enc.Decrypt(truncated));
            }
        }

        [TestMethod]
        public void Decrypt_WrongKey_Throws()
        {
            string cipher;
            using (var writer = NewEncryptor()) cipher = writer.Encrypt(PlainText);

            using (var other = NewEncryptor())
            {
                Assert.ThrowsException<CryptographicException>(() => other.Decrypt(cipher));
            }
        }

        [TestMethod]
        public void Decrypt_FailuresShareIdenticalMessage()
        {
            // 錯誤訊息不得洩漏失敗原因，否則構成 oracle
            using (var enc = NewEncryptor())
            using (var other = NewEncryptor())
            {
                byte[] raw = enc.Encrypt(Encoding.UTF8.GetBytes(PlainText));

                byte[] tampered = (byte[])raw.Clone();
                tampered[raw.Length - 1] ^= 0xFF;

                byte[] badVersion = (byte[])raw.Clone();
                badVersion[0] = 99;

                var messages = new List<string>();
                messages.Add(Capture(() => enc.Decrypt(tampered)));
                messages.Add(Capture(() => enc.Decrypt(badVersion)));
                messages.Add(Capture(() => enc.Decrypt(new byte[5])));
                messages.Add(Capture(() => enc.Decrypt("!!!不是 Base64!!!")));
                messages.Add(Capture(() => other.Decrypt(Convert.ToBase64String(raw))));

                Assert.AreEqual(1, messages.Distinct().Count(),
                    "所有解密失敗都必須回報相同訊息，實際收到：" + string.Join(" | ", messages.Distinct()));
            }
        }

        private static string Capture(Action action)
        {
            try
            {
                action();
                Assert.Fail("預期應拋出例外");
                return null;
            }
            catch (CryptographicException ex)
            {
                return ex.Message;
            }
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("abc")]
        [DataRow("!!!!")]
        [DataRow("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        public void Decrypt_MalformedInput_ThrowsCryptographicException(string cipher)
        {
            using (var enc = NewEncryptor())
            {
                Assert.ThrowsException<CryptographicException>(() => enc.Decrypt(cipher));
            }
        }

        #endregion

        #region Key Handling

        [TestMethod]
        public void GenerateKey_ReturnsCorrectSizeAndIsRandom()
        {
            byte[] first = AesEncryptor.GenerateKey();
            byte[] second = AesEncryptor.GenerateKey();

            Assert.AreEqual(AesEncryptor.KeySize, first.Length);
            CollectionAssert.AreNotEqual(first, second);
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(16)]
        [DataRow(31)]
        [DataRow(33)]
        public void Constructor_WrongKeySize_Throws(int size)
        {
            Assert.ThrowsException<ArgumentException>(() => new AesEncryptor(new byte[size]));
        }

        [TestMethod]
        public void Constructor_NullKey_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AesEncryptor(null));
        }

        [TestMethod]
        public void Constructor_CopiesKey_SoCallerMutationHasNoEffect()
        {
            byte[] key = AesEncryptor.GenerateKey();
            using (var enc = new AesEncryptor(key))
            {
                string cipher = enc.Encrypt(PlainText);
                Array.Clear(key, 0, key.Length);   // 呼叫端清空自己的副本

                Assert.AreEqual(PlainText, enc.Decrypt(cipher), "加密器應持有金鑰的獨立副本");
            }
        }

        #endregion

        #region FromPassword

        [TestMethod]
        public void FromPassword_SamePasswordAndSalt_ProducesInteroperableKeys()
        {
            byte[] salt = AesEncryptor.GenerateSalt();
            const int iterations = 1000;   // 測試用低迭代，避免拖慢測試
            string cipher;

            using (var a = AesEncryptor.FromPassword("p@ssw0rd", salt, iterations))
                cipher = a.Encrypt(PlainText);

            using (var b = AesEncryptor.FromPassword("p@ssw0rd", salt, iterations))
                Assert.AreEqual(PlainText, b.Decrypt(cipher));
        }

        [TestMethod]
        public void FromPassword_DifferentSalt_ProducesDifferentKey()
        {
            const int iterations = 1000;
            string cipher;

            using (var a = AesEncryptor.FromPassword("p@ssw0rd", AesEncryptor.GenerateSalt(), iterations))
                cipher = a.Encrypt(PlainText);

            using (var b = AesEncryptor.FromPassword("p@ssw0rd", AesEncryptor.GenerateSalt(), iterations))
                Assert.ThrowsException<CryptographicException>(() => b.Decrypt(cipher));
        }

        [TestMethod]
        public void FromPassword_DifferentPassword_ProducesDifferentKey()
        {
            byte[] salt = AesEncryptor.GenerateSalt();
            const int iterations = 1000;
            string cipher;

            using (var a = AesEncryptor.FromPassword("p@ssw0rd", salt, iterations))
                cipher = a.Encrypt(PlainText);

            using (var b = AesEncryptor.FromPassword("P@ssw0rd", salt, iterations))
                Assert.ThrowsException<CryptographicException>(() => b.Decrypt(cipher));
        }

        [TestMethod]
        public void FromPassword_UnicodePassword_RoundTrips()
        {
            byte[] salt = AesEncryptor.GenerateSalt();
            const int iterations = 1000;
            string cipher;

            using (var a = AesEncryptor.FromPassword("密碼🔐", salt, iterations))
                cipher = a.Encrypt(PlainText);

            using (var b = AesEncryptor.FromPassword("密碼🔐", salt, iterations))
                Assert.AreEqual(PlainText, b.Decrypt(cipher));
        }

        [TestMethod]
        public void FromPassword_InvalidArguments_Throw()
        {
            byte[] salt = AesEncryptor.GenerateSalt();

            Assert.ThrowsException<ArgumentNullException>(() => AesEncryptor.FromPassword(null, salt));
            Assert.ThrowsException<ArgumentNullException>(() => AesEncryptor.FromPassword("pw", null));
            Assert.ThrowsException<ArgumentException>(() => AesEncryptor.FromPassword("pw", new byte[8]));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => AesEncryptor.FromPassword("pw", salt, 0));
        }

        [TestMethod]
        public void GenerateSalt_ReturnsCorrectSizeAndIsRandom()
        {
            byte[] first = AesEncryptor.GenerateSalt();

            Assert.AreEqual(AesEncryptor.SaltSize, first.Length);
            CollectionAssert.AreNotEqual(first, AesEncryptor.GenerateSalt());
        }

        #endregion

        #region Dispose / Thread Safety

        [TestMethod]
        public void Dispose_ThenUse_ThrowsObjectDisposedException()
        {
            var enc = NewEncryptor();
            string cipher = enc.Encrypt(PlainText);
            enc.Dispose();

            Assert.ThrowsException<ObjectDisposedException>(() => enc.Encrypt(PlainText));
            Assert.ThrowsException<ObjectDisposedException>(() => enc.Decrypt(cipher));
        }

        [TestMethod]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var enc = NewEncryptor();
            enc.Dispose();
            enc.Dispose();
        }

        [TestMethod]
        public void Dispose_WithoutAnyUse_DoesNotThrow()
        {
            // 舊的 CryptoProvider 在這個情境會拋 NullReferenceException
            using (NewEncryptor()) { }
        }

        [TestMethod]
        public void ConcurrentUse_IsThreadSafe()
        {
            using (var enc = NewEncryptor())
            {
                var results = new string[500];
                Parallel.For(0, results.Length, i =>
                {
                    string text = PlainText + i;
                    results[i] = enc.Decrypt(enc.Encrypt(text));
                });

                for (int i = 0; i < results.Length; i++)
                {
                    Assert.AreEqual(PlainText + i, results[i]);
                }
            }
        }

        #endregion

        #region Null Arguments

        [TestMethod]
        public void NullArguments_Throw()
        {
            using (var enc = NewEncryptor())
            {
                Assert.ThrowsException<ArgumentNullException>(() => enc.Encrypt((string)null));
                Assert.ThrowsException<ArgumentNullException>(() => enc.Encrypt((byte[])null));
                Assert.ThrowsException<ArgumentNullException>(() => enc.Decrypt((string)null));
                Assert.ThrowsException<ArgumentNullException>(() => enc.Decrypt((byte[])null));
            }
        }

        #endregion
    }
}
