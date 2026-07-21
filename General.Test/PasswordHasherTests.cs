using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using General.Crypto.Password;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace General.Test
{
    [TestClass]
    public class PasswordHasherTests
    {
        private const string Password = "P@ssw0rd!";

        #region Hash / Verify

        [TestMethod]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            string phc = PasswordHasher.Hash(Password);

            Assert.IsTrue(PasswordHasher.Verify(Password, phc));
        }

        [TestMethod]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            string phc = PasswordHasher.Hash(Password);

            Assert.IsFalse(PasswordHasher.Verify("p@ssw0rd!", phc));
            Assert.IsFalse(PasswordHasher.Verify(Password + " ", phc));
            Assert.IsFalse(PasswordHasher.Verify(string.Empty, phc));
        }

        [TestMethod]
        public void Hash_SamePasswordTwice_ProducesDifferentResults()
        {
            string first = PasswordHasher.Hash(Password);
            string second = PasswordHasher.Hash(Password);

            Assert.AreNotEqual(first, second, "salt 應為隨機，兩次雜湊結果不得相同");
            Assert.IsTrue(PasswordHasher.Verify(Password, first));
            Assert.IsTrue(PasswordHasher.Verify(Password, second));
        }

        [TestMethod]
        public void Hash_EmptyPassword_Throws()
        {
            Assert.ThrowsException<ArgumentException>(() => PasswordHasher.Hash(string.Empty));
        }

        [TestMethod]
        public void Verify_EmptyPassword_ReturnsFalseWithoutThrowing()
        {
            string phc = PasswordHasher.Hash(Password);

            // 空密碼不得使登入流程拋出例外
            Assert.IsFalse(PasswordHasher.Verify(string.Empty, phc));
        }

        [TestMethod]
        public void Hash_NullPassword_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => PasswordHasher.Hash(null));
        }

        #endregion

        #region PHC Format

        [TestMethod]
        public void Hash_ProducesPhcFormat()
        {
            string phc = PasswordHasher.Hash(Password);

            // $argon2id$v=19$m=19456,t=2,p=1$<salt>$<hash>
            Assert.IsTrue(
                Regex.IsMatch(phc, @"^\$argon2id\$v=19\$m=19456,t=2,p=1\$[A-Za-z0-9+/]+\$[A-Za-z0-9+/]+$"),
                "實際輸出：" + phc);
        }

        [TestMethod]
        public void Hash_Base64SegmentsAreUnpadded()
        {
            string phc = PasswordHasher.Hash(Password);

            // 只檢查 salt 與 hash 兩段；前面的 v=19、m=、t=、p= 本來就含 '='
            string[] parts = phc.Split('$');
            Assert.AreEqual(6, parts.Length, "實際輸出：" + phc);
            Assert.IsFalse(parts[4].Contains("="), "salt 不應含尾端填充：" + parts[4]);
            Assert.IsFalse(parts[5].Contains("="), "hash 不應含尾端填充：" + parts[5]);
            Assert.AreEqual(22, parts[4].Length, "16 bytes 去填充後應為 22 字元");
            Assert.AreEqual(43, parts[5].Length, "32 bytes 去填充後應為 43 字元");
        }

        [TestMethod]
        public void Hash_EachStrength_EncodesOwnParametersAndRoundTrips()
        {
            AssertStrength(Argon2idStrength.Default, "m=19456,t=2,p=1");
            AssertStrength(Argon2idStrength.High, "m=65536,t=2,p=1");
            AssertStrength(Argon2idStrength.Highest, "m=131072,t=3,p=1");
        }

        private static void AssertStrength(Argon2idStrength strength, string expectedCosts)
        {
            string phc = PasswordHasher.Hash(Password, strength);

            StringAssert.Contains(phc, expectedCosts);
            Assert.IsTrue(PasswordHasher.Verify(Password, phc), strength + " 應可驗證通過");
            Assert.IsFalse(PasswordHasher.Verify("wrong", phc), strength + " 錯誤密碼應失敗");
        }

        [TestMethod]
        public void Hash_UndefinedStrength_Throws()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => PasswordHasher.Hash(Password, (Argon2idStrength)99));
        }

        #endregion

        #region Malformed Input

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("abc")]
        [DataRow("$argon2id$v=19$m=19456,t=2,p=1$c2FsdA")]                       // 段數不足
        [DataRow("$argon2id$v=19$m=19456,t=2,p=1$c2FsdA$aGFzaA$extra")]          // 段數過多
        [DataRow("argon2id$v=19$m=19456,t=2,p=1$c2FsdA$aGFzaA")]                 // 缺前置 $
        [DataRow("$argon2i$v=19$m=19456,t=2,p=1$c2FsdA$aGFzaA")]                 // 演算法不符
        [DataRow("$argon2id$v=16$m=19456,t=2,p=1$c2FsdA$aGFzaA")]                // 版本不符
        [DataRow("$argon2id$v=xx$m=19456,t=2,p=1$c2FsdA$aGFzaA")]                // 版本非數字
        [DataRow("$argon2id$v=19$m=19456,t=2$c2FsdA$aGFzaA")]                    // 成本參數不足
        [DataRow("$argon2id$v=19$m=19456,t=2,x=1$c2FsdA$aGFzaA")]                // 成本參數標籤錯誤
        [DataRow("$argon2id$v=19$m=0,t=2,p=1$c2FsdA$aGFzaA")]                    // 成本參數為 0
        [DataRow("$argon2id$v=19$m=-1,t=2,p=1$c2FsdA$aGFzaA")]                   // 成本參數為負
        [DataRow("$argon2id$v=19$m=19456,t=2,p=1$!!!!$aGFzaA")]                  // salt 非 Base64
        [DataRow("$argon2id$v=19$m=19456,t=2,p=1$c2FsdA$!!!!")]                  // hash 非 Base64
        [DataRow("$argon2id$v=19$m=19456,t=2,p=1$$aGFzaA")]                      // salt 為空
        [DataRow("$argon2id$v=19$m=19456,t=2,p=1$c2FsdA$")]                      // hash 為空
        public void Verify_MalformedPhcString_ReturnsFalseWithoutThrowing(string phc)
        {
            Assert.IsFalse(PasswordHasher.Verify(Password, phc));
        }

        [TestMethod]
        public void Verify_NullPassword_ReturnsFalse()
        {
            string phc = PasswordHasher.Hash(Password);

            Assert.IsFalse(PasswordHasher.Verify(null, phc));
        }

        [TestMethod]
        public void Verify_TamperedHash_ReturnsFalse()
        {
            string phc = PasswordHasher.Hash(Password);
            char last = phc[phc.Length - 1];
            string tampered = phc.Substring(0, phc.Length - 1) + (last == 'A' ? 'B' : 'A');

            Assert.IsFalse(PasswordHasher.Verify(Password, tampered));
        }

        #endregion

        #region NeedsRehash

        [TestMethod]
        public void NeedsRehash_SameStrength_ReturnsFalse()
        {
            string phc = PasswordHasher.Hash(Password);

            Assert.IsFalse(PasswordHasher.NeedsRehash(phc, Argon2idStrength.Default));
        }

        [TestMethod]
        public void NeedsRehash_HigherStrengthDesired_ReturnsTrue()
        {
            string phc = PasswordHasher.Hash(Password, Argon2idStrength.Default);

            Assert.IsTrue(PasswordHasher.NeedsRehash(phc, Argon2idStrength.High));
            Assert.IsTrue(PasswordHasher.NeedsRehash(phc, Argon2idStrength.Highest));
        }

        [TestMethod]
        public void NeedsRehash_LowerStrengthDesired_ReturnsTrue()
        {
            string phc = PasswordHasher.Hash(Password, Argon2idStrength.High);

            Assert.IsTrue(PasswordHasher.NeedsRehash(phc, Argon2idStrength.Default),
                "參數不一致即應重算，不論高低");
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("abc")]
        [DataRow("$md5$deadbeef")]
        public void NeedsRehash_MalformedPhcString_ReturnsTrue(string phc)
        {
            Assert.IsTrue(PasswordHasher.NeedsRehash(phc));
        }

        [TestMethod]
        public void NeedsRehash_NonStandardSaltOrHashSize_ReturnsTrue()
        {
            // salt 4 bytes、hash 4 bytes，長度不符 RFC 9106 建議值
            const string phc = "$argon2id$v=19$m=19456,t=2,p=1$c2FsdA$aGFzaA";

            Assert.IsTrue(PasswordHasher.NeedsRehash(phc));
        }

        #endregion

        #region Unicode

        [DataTestMethod]
        [DataRow("中文密碼測試")]
        [DataRow("パスワード")]
        [DataRow("пароль")]
        [DataRow("🔐🗝️😀")]
        [DataRow("  前後空白  ")]
        public void Hash_UnicodePassword_CanRoundTrip(string password)
        {
            string phc = PasswordHasher.Hash(password);

            Assert.IsTrue(PasswordHasher.Verify(password, phc));
            Assert.IsFalse(PasswordHasher.Verify(password + "x", phc));
        }

        [TestMethod]
        public void Hash_LongPassword_HasNoLengthLimit()
        {
            // bcrypt 有 72 bytes 上限，Argon2 沒有；此處以 1000 字元驗證不被截斷
            string password = new string('a', 1000);
            string phc = PasswordHasher.Hash(password);

            Assert.IsTrue(PasswordHasher.Verify(password, phc));
            Assert.IsFalse(PasswordHasher.Verify(new string('a', 999), phc),
                "長密碼不得被截斷，否則 999 與 1000 字元會被視為相同");
        }

        #endregion

        #region Async

        [TestMethod]
        public async Task HashAsync_ResultIsVerifiableBySyncVerify()
        {
            string phc = await PasswordHasher.HashAsync(Password);

            Assert.IsTrue(PasswordHasher.Verify(Password, phc));
        }

        [TestMethod]
        public async Task VerifyAsync_MatchesSyncBehavior()
        {
            string phc = PasswordHasher.Hash(Password);

            Assert.IsTrue(await PasswordHasher.VerifyAsync(Password, phc));
            Assert.IsFalse(await PasswordHasher.VerifyAsync("wrong", phc));
        }

        #endregion
    }
}
