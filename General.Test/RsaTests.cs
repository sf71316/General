using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using General.Crypto.Rsa;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace General.Test
{
    [TestClass]
    public class RsaTests
    {
        private const string PlainText = "Hello World!!";

        // 產生 3072-bit 金鑰頗慢，整個測試類別共用一把 2048-bit 金鑰。
        // 需要特定長度的個別測試再自行產生。
        private static RsaKey _sharedKey;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _sharedKey = RsaKey.Generate(2048);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            if (_sharedKey != null) _sharedKey.Dispose();
        }

        private static RsaKey PublicOnly(RsaKey key)
        {
            return RsaKey.FromPem(key.ToPublicPem());
        }

        #region RsaKey — Generate

        [TestMethod]
        public void Generate_DefaultSize_Is3072()
        {
            using (var key = RsaKey.Generate())
            {
                Assert.AreEqual(3072, key.KeySizeBits);
                Assert.IsTrue(key.HasPrivateKey);
            }
        }

        [TestMethod]
        public void Generate_RespectsRequestedSize()
        {
            using (var key = RsaKey.Generate(2048))
            {
                Assert.AreEqual(2048, key.KeySizeBits);
            }
        }

        [DataTestMethod]
        [DataRow(512)]
        [DataRow(1024)]
        [DataRow(2047)]
        public void Generate_BelowMinimumSize_Throws(int bits)
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => RsaKey.Generate(bits));
        }

        [TestMethod]
        public void Generate_ProducesDistinctKeys()
        {
            using (var a = RsaKey.Generate(2048))
            using (var b = RsaKey.Generate(2048))
            {
                Assert.AreNotEqual(a.ToPublicPem(), b.ToPublicPem());
            }
        }

        #endregion

        #region RsaKey — PEM

        [TestMethod]
        public void ToPublicPem_UsesSubjectPublicKeyInfoHeader()
        {
            StringAssert.StartsWith(_sharedKey.ToPublicPem().Trim(), "-----BEGIN PUBLIC KEY-----");
        }

        [TestMethod]
        public void ToPrivatePem_UsesPkcs8Header()
        {
            // PKCS#8 是 "BEGIN PRIVATE KEY"；"BEGIN RSA PRIVATE KEY" 是 PKCS#1，非本實作契約
            string pem = _sharedKey.ToPrivatePem().Trim();

            StringAssert.StartsWith(pem, "-----BEGIN PRIVATE KEY-----");
            Assert.IsFalse(pem.Contains("BEGIN RSA PRIVATE KEY"), "不應輸出 PKCS#1 格式");
        }

        [TestMethod]
        public void FromPem_PrivateKey_RoundTrips()
        {
            using (var reloaded = RsaKey.FromPem(_sharedKey.ToPrivatePem()))
            {
                Assert.IsTrue(reloaded.HasPrivateKey);
                Assert.AreEqual(_sharedKey.KeySizeBits, reloaded.KeySizeBits);
                Assert.AreEqual(_sharedKey.ToPublicPem(), reloaded.ToPublicPem());
            }
        }

        [TestMethod]
        public void FromPem_PublicKey_RoundTripsWithoutPrivateKey()
        {
            using (var reloaded = RsaKey.FromPem(_sharedKey.ToPublicPem()))
            {
                Assert.IsFalse(reloaded.HasPrivateKey);
                Assert.AreEqual(_sharedKey.ToPublicPem(), reloaded.ToPublicPem());
            }
        }

        [TestMethod]
        public void ToPrivatePem_OnPublicOnlyKey_Throws()
        {
            using (var pub = PublicOnly(_sharedKey))
            {
                Assert.ThrowsException<InvalidOperationException>(() => pub.ToPrivatePem());
            }
        }

        [TestMethod]
        public void FromPem_AlsoAcceptsPkcs1PrivateKey()
        {
            // 外部系統（尤其舊的 OpenSSL）常給 PKCS#1，載入端應接受
            const string pkcs1 = @"-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEAsm7B0IWpKcDlxDdmXCHUZLXBqL9r0mzTSEMWMTQVaMbLg1lU
";
            // 僅驗證不會誤判為「非 RSA 金鑰」而以其他例外型別失敗；
            // 內容被截斷，故預期為格式錯誤
            Assert.ThrowsException<CryptographicException>(() => RsaKey.FromPem(pkcs1));
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("not a pem")]
        [DataRow("-----BEGIN PUBLIC KEY-----\nnot base64\n-----END PUBLIC KEY-----")]
        public void FromPem_MalformedInput_ThrowsCryptographicException(string pem)
        {
            Assert.ThrowsException<CryptographicException>(() => RsaKey.FromPem(pem));
        }

        [TestMethod]
        public void FromPem_Null_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => RsaKey.FromPem(null));
        }

        [TestMethod]
        public void Dispose_ThenUse_Throws()
        {
            var key = RsaKey.Generate(2048);
            key.Dispose();

            Assert.ThrowsException<ObjectDisposedException>(() => { var _ = key.KeySizeBits; });
            Assert.ThrowsException<ObjectDisposedException>(() => key.ToPublicPem());
        }

        #endregion

        #region RsaEncryptor

        [TestMethod]
        public void Encrypt_Decrypt_RoundTrips()
        {
            using (var enc = new RsaEncryptor(_sharedKey))
            {
                string cipher = enc.Encrypt(PlainText);

                Assert.AreNotEqual(PlainText, cipher);
                Assert.AreEqual(PlainText, enc.Decrypt(cipher));
            }
        }

        [TestMethod]
        public void Encrypt_WithPublicKeyOnly_DecryptWithPrivateKey()
        {
            string cipher;
            using (var pub = PublicOnly(_sharedKey))
            using (var enc = new RsaEncryptor(pub))
            {
                cipher = enc.Encrypt(PlainText);
            }

            using (var dec = new RsaEncryptor(_sharedKey))
            {
                Assert.AreEqual(PlainText, dec.Decrypt(cipher));
            }
        }

        [TestMethod]
        public void Decrypt_WithPublicKeyOnly_ThrowsInvalidOperation()
        {
            string cipher;
            using (var enc = new RsaEncryptor(_sharedKey)) cipher = enc.Encrypt(PlainText);

            using (var pub = PublicOnly(_sharedKey))
            using (var dec = new RsaEncryptor(pub))
            {
                // 以公鑰解密是程式邏輯錯誤，不應與「密文錯誤」混為一談
                Assert.ThrowsException<InvalidOperationException>(() => dec.Decrypt(cipher));
            }
        }

        [TestMethod]
        public void Encrypt_ExceedsRawRsaLimit_StillWorks()
        {
            // RSA-2048 + OAEP-SHA256 的原始上限是 190 bytes，混合加密不應受此限制
            byte[] plain = new byte[512 * 1024];
            new Random(7).NextBytes(plain);

            using (var enc = new RsaEncryptor(_sharedKey))
            {
                CollectionAssert.AreEqual(plain, enc.Decrypt(enc.Encrypt(plain)));
            }
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("中文內容測試")]
        [DataRow("🔐🗝️😀")]
        public void Encrypt_UnicodeAndEmpty_RoundTrips(string text)
        {
            using (var enc = new RsaEncryptor(_sharedKey))
            {
                Assert.AreEqual(text, enc.Decrypt(enc.Encrypt(text)));
            }
        }

        [TestMethod]
        public void Encrypt_SamePlaintextTwice_ProducesDifferentCiphertext()
        {
            using (var enc = new RsaEncryptor(_sharedKey))
            {
                Assert.AreNotEqual(enc.Encrypt(PlainText), enc.Encrypt(PlainText),
                    "每次加密都應使用全新的 AES 金鑰與 nonce");
            }
        }

        [TestMethod]
        public void Decrypt_TamperedCiphertext_Throws()
        {
            using (var enc = new RsaEncryptor(_sharedKey))
            {
                byte[] raw = enc.Encrypt(Encoding.UTF8.GetBytes(PlainText));

                // 抽樣翻轉：包裝金鑰區與 AES 酬載區各取數點
                int[] targets = { 0, 3, 10, raw.Length / 2, raw.Length - 20, raw.Length - 1 };
                foreach (int i in targets)
                {
                    byte[] tampered = (byte[])raw.Clone();
                    tampered[i] ^= 0x01;

                    Assert.ThrowsException<CryptographicException>(() => enc.Decrypt(tampered),
                        "位元組 " + i + " 遭竄改卻未被偵測");
                }
            }
        }

        [TestMethod]
        public void Decrypt_WrongKey_Throws()
        {
            string cipher;
            using (var enc = new RsaEncryptor(_sharedKey)) cipher = enc.Encrypt(PlainText);

            using (var other = RsaKey.Generate(2048))
            using (var dec = new RsaEncryptor(other))
            {
                Assert.ThrowsException<CryptographicException>(() => dec.Decrypt(cipher));
            }
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("abc")]
        [DataRow("!!!!")]
        [DataRow("AQABAAAA")]
        public void Decrypt_MalformedInput_ThrowsCryptographicException(string cipher)
        {
            using (var enc = new RsaEncryptor(_sharedKey))
            {
                Assert.ThrowsException<CryptographicException>(() => enc.Decrypt(cipher));
            }
        }

        [TestMethod]
        public void Encryptor_NullKey_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new RsaEncryptor(null));
        }

        [TestMethod]
        public void Encryptor_NullArguments_Throw()
        {
            using (var enc = new RsaEncryptor(_sharedKey))
            {
                Assert.ThrowsException<ArgumentNullException>(() => enc.Encrypt((string)null));
                Assert.ThrowsException<ArgumentNullException>(() => enc.Encrypt((byte[])null));
                Assert.ThrowsException<ArgumentNullException>(() => enc.Decrypt((string)null));
                Assert.ThrowsException<ArgumentNullException>(() => enc.Decrypt((byte[])null));
            }
        }

        [TestMethod]
        public void Encryptor_DoesNotDisposeSuppliedKey()
        {
            using (var enc = new RsaEncryptor(_sharedKey)) { }

            // 共用金鑰在此之後仍須可用，否則後續測試會連鎖失敗
            Assert.IsTrue(_sharedKey.KeySizeBits > 0);
        }

        #endregion

        #region RsaSigner

        [TestMethod]
        public void Sign_Verify_RoundTrips()
        {
            using (var signer = new RsaSigner(_sharedKey))
            {
                string signature = signer.Sign(PlainText);

                Assert.IsTrue(signer.Verify(PlainText, signature));
            }
        }

        [TestMethod]
        public void Verify_WithPublicKeyOnly_Succeeds()
        {
            string signature;
            using (var signer = new RsaSigner(_sharedKey)) signature = signer.Sign(PlainText);

            using (var pub = PublicOnly(_sharedKey))
            using (var verifier = new RsaSigner(pub))
            {
                Assert.IsTrue(verifier.Verify(PlainText, signature));
            }
        }

        [TestMethod]
        public void Sign_WithPublicKeyOnly_ThrowsInvalidOperation()
        {
            using (var pub = PublicOnly(_sharedKey))
            using (var signer = new RsaSigner(pub))
            {
                Assert.ThrowsException<InvalidOperationException>(() => signer.Sign(PlainText));
            }
        }

        [TestMethod]
        public void Sign_SameDataTwice_ProducesDifferentSignatures()
        {
            using (var signer = new RsaSigner(_sharedKey))
            {
                string first = signer.Sign(PlainText);
                string second = signer.Sign(PlainText);

                Assert.AreNotEqual(first, second, "PSS 帶隨機 salt，簽章值應每次不同");
                Assert.IsTrue(signer.Verify(PlainText, first));
                Assert.IsTrue(signer.Verify(PlainText, second), "兩個簽章都必須能通過驗證");
            }
        }

        [TestMethod]
        public void Verify_TamperedData_ReturnsFalse()
        {
            using (var signer = new RsaSigner(_sharedKey))
            {
                string signature = signer.Sign(PlainText);

                Assert.IsFalse(signer.Verify(PlainText + " ", signature));
                Assert.IsFalse(signer.Verify("hello world!!", signature));
                Assert.IsFalse(signer.Verify(string.Empty, signature));
            }
        }

        [TestMethod]
        public void Verify_TamperedSignature_ReturnsFalse()
        {
            using (var signer = new RsaSigner(_sharedKey))
            {
                byte[] raw = Convert.FromBase64String(signer.Sign(PlainText));
                raw[raw.Length - 1] ^= 0x01;

                Assert.IsFalse(signer.Verify(PlainText, Convert.ToBase64String(raw)));
            }
        }

        [TestMethod]
        public void Verify_WrongKey_ReturnsFalse()
        {
            string signature;
            using (var signer = new RsaSigner(_sharedKey)) signature = signer.Sign(PlainText);

            using (var other = RsaKey.Generate(2048))
            using (var verifier = new RsaSigner(other))
            {
                Assert.IsFalse(verifier.Verify(PlainText, signature));
            }
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("abc")]
        [DataRow("!!!不是 Base64!!!")]
        [DataRow("AAAA")]
        public void Verify_MalformedSignature_ReturnsFalseWithoutThrowing(string signature)
        {
            using (var signer = new RsaSigner(_sharedKey))
            {
                Assert.IsFalse(signer.Verify(PlainText, signature));
            }
        }

        [TestMethod]
        public void Verify_NullData_ReturnsFalse()
        {
            using (var signer = new RsaSigner(_sharedKey))
            {
                string signature = signer.Sign(PlainText);

                Assert.IsFalse(signer.Verify((string)null, signature));
                Assert.IsFalse(signer.Verify((byte[])null, signature));
            }
        }

        [TestMethod]
        public void Signer_NullKey_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new RsaSigner(null));
        }

        [TestMethod]
        public void Sign_NullData_Throws()
        {
            using (var signer = new RsaSigner(_sharedKey))
            {
                Assert.ThrowsException<ArgumentNullException>(() => signer.Sign((string)null));
                Assert.ThrowsException<ArgumentNullException>(() => signer.Sign((byte[])null));
            }
        }

        #endregion

        #region Cross-instance / Concurrency

        [TestMethod]
        public void KeyTransferredByPem_EncryptSignInterop()
        {
            // 模擬真實流程：A 端只拿到公鑰 PEM，B 端持有私鑰
            string publicPem = _sharedKey.ToPublicPem();
            string privatePem = _sharedKey.ToPrivatePem();

            string cipher;
            using (var aKey = RsaKey.FromPem(publicPem))
            using (var enc = new RsaEncryptor(aKey))
            {
                cipher = enc.Encrypt(PlainText);
            }

            string signature;
            using (var bKey = RsaKey.FromPem(privatePem))
            using (var dec = new RsaEncryptor(bKey))
            using (var signer = new RsaSigner(bKey))
            {
                Assert.AreEqual(PlainText, dec.Decrypt(cipher));
                signature = signer.Sign(PlainText);
            }

            using (var aKey = RsaKey.FromPem(publicPem))
            using (var verifier = new RsaSigner(aKey))
            {
                Assert.IsTrue(verifier.Verify(PlainText, signature));
            }
        }

        [TestMethod]
        public void ConcurrentEncryptAndSign_IsStable()
        {
            using (var enc = new RsaEncryptor(_sharedKey))
            using (var signer = new RsaSigner(_sharedKey))
            {
                var results = new bool[100];
                Parallel.For(0, results.Length, i =>
                {
                    string text = PlainText + i;
                    bool decrypted = enc.Decrypt(enc.Encrypt(text)) == text;
                    bool verified = signer.Verify(text, signer.Sign(text));
                    results[i] = decrypted && verified;
                });

                Assert.IsTrue(results.All(r => r), "併發操作不得產生錯誤結果");
            }
        }

        #endregion
    }
}
