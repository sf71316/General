using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;

namespace General.Crypto.Password
{
    /// <summary>
    /// Argon2id 密碼雜湊公用函式，輸出 PHC 標準字串格式：
    /// <c>$argon2id$v=19$m=19456,t=2,p=1$&lt;salt&gt;$&lt;hash&gt;</c>
    /// </summary>
    /// <remarks>
    /// <para>參數依據 OWASP Password Storage Cheat Sheet；salt 16 bytes、hash 32 bytes 依據 RFC 9106。</para>
    /// <para>
    /// 本類別「不」實作 pepper。OWASP 要求 pepper 必須與雜湊分開存放於 KMS/HSM，
    /// 屬於部署層決策；如需 pepper，請於呼叫端先行 HMAC 後再傳入，例如
    /// <c>Hash(Convert.ToBase64String(hmacSha384(password, pepper)))</c>。
    /// </para>
    /// </remarks>
    public static class PasswordHasher
    {
        /// <summary>Salt 長度（bytes），RFC 9106 建議值。</summary>
        public const int SaltSize = 16;

        /// <summary>雜湊輸出長度（bytes），RFC 9106 建議值。</summary>
        public const int HashSize = 32;

        /// <summary>PHC 演算法識別字。</summary>
        private const string Algorithm = "argon2id";

        /// <summary>Argon2 版本號 0x13 (19)，即 Argon2 1.3 規格。</summary>
        private const int Version = 19;

        #region Public Method

        /// <summary>
        /// 以 Argon2id 雜湊密碼，回傳 PHC 格式字串（可直接存入資料庫）。
        /// </summary>
        /// <param name="password">明文密碼，以 UTF-8 編碼，無長度上限。</param>
        /// <param name="strength">運算強度，預設為 OWASP 建議基準。</param>
        /// <exception cref="ArgumentNullException">password 為 null。</exception>
        /// <exception cref="ArgumentException">password 為空字串。空密碼不得儲存，應由呼叫端的密碼政策先行攔截。</exception>
        public static string Hash(string password, Argon2idStrength strength = Argon2idStrength.Default)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (password.Length == 0) throw new ArgumentException("密碼不得為空字串。", "password");

            Argon2idParameters parameters = Argon2idParameters.FromStrength(strength);
            byte[] salt = GenerateSalt();
            byte[] hash = ComputeHash(password, salt, parameters);

            return Encode(parameters, salt, hash);
        }

        /// <summary>
        /// 驗證密碼是否符合先前產生的 PHC 字串。
        /// </summary>
        /// <remarks>
        /// 一律以 <paramref name="phcString"/> 內記載的參數重算，而非查詢目前的強度設定表，
        /// 因此日後調整參數表時舊有雜湊仍可正常驗證。
        /// 格式不正確或無法解析時回傳 false，不拋出例外，以免登入流程因髒資料中斷。
        /// null 或空字串密碼一律回傳 false；空密碼無法通過 <see cref="Hash(string, Argon2idStrength)"/>，
        /// 故不可能存在對應的雜湊值。
        /// </remarks>
        public static bool Verify(string password, string phcString)
        {
            if (string.IsNullOrEmpty(password)) return false;

            Argon2idParameters parameters;
            byte[] salt;
            byte[] expected;
            if (!TryDecode(phcString, out parameters, out salt, out expected)) return false;

            byte[] actual = ComputeHash(password, salt, parameters, expected.Length);
            return FixedTimeEquals(actual, expected);
        }

        /// <summary>
        /// 判斷既有雜湊是否應以新的強度重算（登入成功後的無痛升級）。
        /// </summary>
        /// <remarks>
        /// 比對的是實際的 m/t/p 數值而非強度代號，因此調整參數表後亦能正確觸發升級。
        /// 無法解析的字串一律回傳 true，代表需要重新雜湊。
        /// </remarks>
        public static bool NeedsRehash(string phcString, Argon2idStrength desired = Argon2idStrength.Default)
        {
            Argon2idParameters current;
            byte[] salt;
            byte[] hash;
            if (!TryDecode(phcString, out current, out salt, out hash)) return true;

            if (salt.Length != SaltSize || hash.Length != HashSize) return true;

            Argon2idParameters target = Argon2idParameters.FromStrength(desired);
            return current.MemorySize != target.MemorySize
                || current.Iterations != target.Iterations
                || current.Parallelism != target.Parallelism;
        }

        /// <summary>
        /// <see cref="Hash(string, Argon2idStrength)"/> 的非同步包裝。
        /// Argon2 為 CPU-bound 運算，此處以背景執行緒執行以避免阻塞呼叫端執行緒。
        /// </summary>
        public static Task<string> HashAsync(string password, Argon2idStrength strength = Argon2idStrength.Default, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Hash(password, strength), cancellationToken);
        }

        /// <summary>
        /// <see cref="Verify(string, string)"/> 的非同步包裝。
        /// </summary>
        public static Task<bool> VerifyAsync(string password, string phcString, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Verify(password, phcString), cancellationToken);
        }

        #endregion

        #region Hash

        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private static byte[] ComputeHash(string password, byte[] salt, Argon2idParameters parameters, int hashSize = HashSize)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            using (Argon2id argon2 = new Argon2id(passwordBytes))
            {
                argon2.Salt = salt;
                argon2.MemorySize = parameters.MemorySize;
                argon2.Iterations = parameters.Iterations;
                argon2.DegreeOfParallelism = parameters.Parallelism;
                return argon2.GetBytes(hashSize);
            }
        }

        /// <summary>
        /// 定時比較，避免因提早結束而洩漏資訊的時序攻擊。
        /// netstandard2.0 無 CryptographicOperations.FixedTimeEquals，故自行實作。
        /// </summary>
        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null) return false;
            if (left.Length != right.Length) return false;

            int diff = 0;
            for (int i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }
            return diff == 0;
        }

        #endregion

        #region PHC Encode / Decode

        private static string Encode(Argon2idParameters parameters, byte[] salt, byte[] hash)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('$').Append(Algorithm);
            sb.Append("$v=").Append(Version.ToString(CultureInfo.InvariantCulture));
            sb.Append("$m=").Append(parameters.MemorySize.ToString(CultureInfo.InvariantCulture));
            sb.Append(",t=").Append(parameters.Iterations.ToString(CultureInfo.InvariantCulture));
            sb.Append(",p=").Append(parameters.Parallelism.ToString(CultureInfo.InvariantCulture));
            sb.Append('$').Append(ToBase64Unpadded(salt));
            sb.Append('$').Append(ToBase64Unpadded(hash));
            return sb.ToString();
        }

        private static bool TryDecode(string phcString, out Argon2idParameters parameters, out byte[] salt, out byte[] hash)
        {
            parameters = default(Argon2idParameters);
            salt = null;
            hash = null;

            if (string.IsNullOrEmpty(phcString)) return false;

            // 格式：$argon2id$v=19$m=..,t=..,p=..$salt$hash → 前置 '$' 使 Split 產生 6 段，第 0 段為空字串
            string[] parts = phcString.Split('$');
            if (parts.Length != 6) return false;
            if (parts[0].Length != 0) return false;
            if (!string.Equals(parts[1], Algorithm, StringComparison.Ordinal)) return false;

            int version;
            if (!TryParseTagged(parts[2], "v", out version)) return false;
            if (version != Version) return false;

            string[] costs = parts[3].Split(',');
            if (costs.Length != 3) return false;

            int memorySize, iterations, parallelism;
            if (!TryParseTagged(costs[0], "m", out memorySize)) return false;
            if (!TryParseTagged(costs[1], "t", out iterations)) return false;
            if (!TryParseTagged(costs[2], "p", out parallelism)) return false;
            if (memorySize <= 0 || iterations <= 0 || parallelism <= 0) return false;

            if (!TryFromBase64Unpadded(parts[4], out salt)) return false;
            if (!TryFromBase64Unpadded(parts[5], out hash)) return false;
            if (salt.Length == 0 || hash.Length == 0) return false;

            parameters = new Argon2idParameters(memorySize, iterations, parallelism);
            return true;
        }

        /// <summary>解析 "m=19456" 這類 &lt;tag&gt;=&lt;int&gt; 片段。</summary>
        private static bool TryParseTagged(string segment, string tag, out int value)
        {
            value = 0;
            if (segment == null) return false;
            if (segment.Length <= tag.Length + 1) return false;
            if (!segment.StartsWith(tag + "=", StringComparison.Ordinal)) return false;

            return int.TryParse(segment.Substring(tag.Length + 1), NumberStyles.None, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>PHC 規範的 Base64 不含尾端 '=' 填充。</summary>
        private static string ToBase64Unpadded(byte[] value)
        {
            return Convert.ToBase64String(value).TrimEnd('=');
        }

        private static bool TryFromBase64Unpadded(string value, out byte[] result)
        {
            result = null;
            if (string.IsNullOrEmpty(value)) return false;

            int padding = value.Length % 4;
            if (padding == 1) return false;
            if (padding > 0) value += new string('=', 4 - padding);

            try
            {
                result = Convert.FromBase64String(value);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        #endregion

        /// <summary>Argon2id 成本參數組合。</summary>
        private struct Argon2idParameters
        {
            /// <summary>記憶體用量（KiB）。</summary>
            public readonly int MemorySize;

            /// <summary>迭代次數。</summary>
            public readonly int Iterations;

            /// <summary>平行度。</summary>
            public readonly int Parallelism;

            public Argon2idParameters(int memorySize, int iterations, int parallelism)
            {
                this.MemorySize = memorySize;
                this.Iterations = iterations;
                this.Parallelism = parallelism;
            }

            public static Argon2idParameters FromStrength(Argon2idStrength strength)
            {
                switch (strength)
                {
                    case Argon2idStrength.Default:
                        return new Argon2idParameters(19456, 2, 1);
                    case Argon2idStrength.High:
                        return new Argon2idParameters(65536, 2, 1);
                    case Argon2idStrength.Highest:
                        return new Argon2idParameters(131072, 3, 1);
                    default:
                        throw new ArgumentOutOfRangeException("strength");
                }
            }
        }
    }
}
