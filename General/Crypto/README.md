# AesEncryptor — AES-256-GCM 可逆加密

用於需要**還原原文**的場景（例如儲存第三方 API 金鑰、身分證字號、需要解密後比對的欄位）。

> 使用者密碼**不屬於**這個場景 —— 密碼應該用單向雜湊，請見
> [`Password/README.md`](Password/README.md) 的 `PasswordHasher`。

- 命名空間：`General.Crypto`
- 目標框架：netstandard2.0
- 相依套件：`BouncyCastle.Cryptography` 2.6.2

---

## 1. 快速上手

### 產生並保存金鑰（首次部署）

```csharp
byte[] key = AesEncryptor.GenerateKey();            // 32 bytes，AES-256
string forStorage = Convert.ToBase64String(key);    // 存進 KMS / 環境變數
```

### 加解密

```csharp
using General.Crypto;

byte[] key = Convert.FromBase64String(Environment.GetEnvironmentVariable("APP_ENC_KEY"));

using (var enc = new AesEncryptor(key))
{
    string cipher = enc.Encrypt("要保護的內容");   // → Base64 字串，直接存進資料庫
    string plain  = enc.Decrypt(cipher);
}
```

### 以密碼衍生金鑰

當金鑰必須由使用者輸入的密碼產生時（例如加密匯出的備份檔）：

```csharp
byte[] salt = AesEncryptor.GenerateSalt();   // 16 bytes，須與密文一同保存

using (var enc = AesEncryptor.FromPassword(password, salt))
{
    string cipher = enc.Encrypt(content);
}
```

採 PBKDF2-HMAC-SHA256、**600,000** 次迭代（OWASP 建議值）。
**salt 與 iterations 必須保存**，否則無法還原金鑰。相同的
`password + salt + iterations` 必然衍生出相同金鑰。

---

## 2. 密文格式

```
Base64( version(1) ‖ nonce(12) ‖ ciphertext(n) ‖ tag(16) )
```

| 欄位 | 長度 | 說明 |
|---|---|---|
| version | 1 byte | 格式版本，目前為 `1`。日後更換演算法時據此分辨舊密文 |
| nonce | 12 bytes | 96-bit，GCM 標準建議值。**每次加密隨機產生** |
| ciphertext | n bytes | 與明文等長 —— GCM 是串流模式，**不做區塊填充** |
| tag | 16 bytes | 128-bit 驗證標籤 |

**額外開銷固定為 29 bytes**（Base64 後約 40 字元）。

### 資料庫欄位長度估算

```
欄位長度 ≈ ceil((明文 bytes + 29) / 3) × 4
```

| 明文 | 建議欄位 |
|---|---|
| 50 bytes 以內（身分證、電話） | `VARCHAR(128)` |
| 255 bytes 以內 | `VARCHAR(512)` |
| 不定長度 | `VARCHAR(MAX)` / `TEXT` |

密文只含 Base64 字元集，**用 `VARCHAR` 即可，不需要 `NVARCHAR`**。

---

## 3. ⚠️ 金鑰管理（最重要的一節）

加密演算法再強，金鑰外洩就全部歸零。

| 該做 | 不該做 |
|---|---|
| 金鑰存於 KMS / HSM / 環境變數 | ❌ 寫死在原始碼 |
| 設定檔加密後才進版控 | ❌ 明文放在 `Web.config` 並提交 |
| 不同環境用不同金鑰 | ❌ 開發與正式共用一把 |
| 規劃輪替流程 | ❌ 從此不換 |

### 金鑰輪替

密文開頭的 version 位元組**不含金鑰識別資訊**。若需要支援多把金鑰並行
（輪替期間新舊資料共存），請在呼叫端自行加上金鑰版本欄位，例如：

```sql
ALTER TABLE Secrets ADD KeyVersion TINYINT NOT NULL DEFAULT 1;
```

解密時依 `KeyVersion` 挑選對應金鑰，背景批次逐步以新金鑰重新加密。

---

## 4. ⚠️ GCM 的 nonce 絕不可重複

這是 GCM 唯一但極其致命的地雷：**同一把金鑰下若有兩筆訊息用了相同的 nonce**，
攻擊者不只能還原這兩筆明文，還能**偽造出可通過驗證的任意密文**。
其後果遠比 CBC 模式重複使用 IV 嚴重。

本實作的防護：

- nonce **一律由內部隨機產生**，沒有任何 API 能讓呼叫端指定或固定 nonce
- 每次呼叫 `Encrypt()` 都產生新的 nonce

**仍需注意的量級限制**：96-bit 隨機 nonce 在同一把金鑰下，
約 **2³² 筆訊息**（約 43 億）後碰撞機率開始不可忽略。
一般應用綽綽有餘；若是超大量寫入的系統，請規劃金鑰輪替，
勿讓單一金鑰累積超過此量級。

---

## 5. 為什麼用 BouncyCastle 而不是原生 `AesGcm`

`System.Security.Cryptography.AesGcm` 需要 **netstandard2.1 以上**，
而 .NET Framework **完全不支援 netstandard2.1**。本專案的 `General` 是
netstandard2.0、`General.Test` 是 net472，改 TFM 會直接把 .NET Framework
的使用端斷掉。

`Microsoft.Bcl.Cryptography` 亦**不會**在 netstandard2.0 下提供 `AesGcm`（已實測確認）。

因此改用 BouncyCastle 的 `GcmBlockCipher` —— 得到的是同一套標準演算法，
無傳遞相依套件。

> 日後若 `General` 升級到 .NET 8 之類的目標框架，可改用原生 `AesGcm`
> 並移除此相依；密文格式不需要變動，兩者輸出完全相容。

---

## 6. 錯誤處理行為

| 情境 | 行為 |
|---|---|
| `new AesEncryptor(null)` | `ArgumentNullException` |
| 金鑰長度不是 32 bytes | `ArgumentException` |
| `Encrypt(null)` / `Decrypt(null)` | `ArgumentNullException` |
| 密文遭竄改（任一位元組） | `CryptographicException` |
| 金鑰錯誤 | `CryptographicException` |
| 密文被截斷 / 長度不足 | `CryptographicException` |
| 版本位元組不符 | `CryptographicException` |
| 不是合法 Base64 | `CryptographicException` |
| Dispose 後再使用 | `ObjectDisposedException` |

**所有解密失敗都回報完全相同的訊息**（`密文無效或已遭竄改。`），
刻意不區分「標籤驗證失敗」「格式錯誤」「版本不符」。
差異化的錯誤訊息會成為攻擊者的 oracle —— 舊版 `CryptoProvider` 正是
因為 CBC 模式下缺乏驗證、且 padding 錯誤會拋出可辨識的例外，而存在
padding oracle 風險。

其他行為：

- 字串一律以 **UTF-8** 編碼
- 建構式會**複製**金鑰陣列，呼叫端事後清空自己的副本不影響加密器
- `Dispose()` 會將記憶體中的金鑰內容**清零**
- **執行緒安全**：金鑰建立後不再變動，每次加解密使用各自的 cipher 實例，
  同一個實例可安全地跨執行緒共用

---

## 7. 與舊版 `CryptoProvider` 的差異

舊的 `CryptoProvider` / `AESCrypto` / `RijndaelCrypto` / `CryptoEum` 已**移除**。
舊實作存在下列問題：

| 問題 | 新實作的處理 |
|---|---|
| CBC 模式無任何完整性驗證 → padding oracle | GCM 為 AEAD，內建驗證 |
| IV 由呼叫端管理，易誤用為固定 IV | nonce 內部隨機產生，不對外開放 |
| `Mode` 屬性可被設為 ECB | 不提供該屬性 |
| `Dispose()` 未使用即呼叫會 `NullReferenceException` | 已修正，並有測試涵蓋 |
| 每次加解密洩漏 `MemoryStream` / `CryptoStream` | 不再持有 stream 欄位 |
| 共用可變欄位，非執行緒安全 | 無共用可變狀態 |
| `GetModule()` 未知型別回傳 `null` | 不再有工廠方法 |
| `RijndaelManaged` 已過時（SYSLIB0022） | 移除 |

舊密文**無法**用新實作解密（演算法與格式皆不同）。若有既有資料需要遷移，
請保留舊程式碼的副本，以「舊實作解密 → 新實作加密」的方式批次轉換。

---

## 8. 參考來源

- [NIST SP 800-38D — GCM 規範](https://csrc.nist.gov/publications/detail/sp/800-38d/final)
- [OWASP Cryptographic Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cryptographic_Storage_Cheat_Sheet.html)
- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)（PBKDF2 迭代次數）
- [BouncyCastle for .NET](https://github.com/bcgit/bc-csharp)

---

## 9. 測試

```bash
dotnet test General.Test/General.Test.csproj --filter "FullyQualifiedName~AesEncryptorTests"
```

測試涵蓋：round-trip（含 Unicode、1 MB 大型資料、空值）、nonce 唯一性、
密文版面配置、**逐位元組竄改偵測**、截斷、錯誤金鑰、錯誤訊息一致性、
金鑰長度驗證、金鑰複製語意、`FromPassword` 衍生一致性、Dispose 行為、
500 執行緒併發。
