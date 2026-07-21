namespace General.Crypto.Password
{
    /// <summary>
    /// Argon2id 運算強度。
    /// 參數依據 OWASP Password Storage Cheat Sheet 與 RFC 9106。
    /// </summary>
    public enum Argon2idStrength
    {
        /// <summary>OWASP 建議基準：m=19456 (19 MiB), t=2, p=1。</summary>
        Default = 0,

        /// <summary>高於 OWASP 基準：m=65536 (64 MiB), t=2, p=1。</summary>
        High = 1,

        /// <summary>最高：m=131072 (128 MiB), t=3, p=1。</summary>
        Highest = 2,
    }
}
