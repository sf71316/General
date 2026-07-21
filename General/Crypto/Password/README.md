# PasswordHasher — Argon2id 密碼雜湊

以 Argon2id 雜湊使用者密碼，輸出 **PHC 標準字串格式**。參數依據
[OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
與 [RFC 9106](https://datatracker.ietf.org/doc/html/rfc9106)。

- 命名空間：`General.Crypto.Password`
- 目標框架：netstandard2.0
- 相依套件：`Konscious.Security.Cryptography.Argon2` 1.3.1

---

## 1. 快速上手

### 註冊 / 變更密碼

```csharp
using General.Crypto.Password;

string phc = PasswordHasher.Hash(password);
// 直接把 phc 整串存進資料庫，不需要另外存 salt 欄位
user.PasswordHash = phc;
```

### 登入驗證

```csharp
if (!PasswordHasher.Verify(inputPassword, user.PasswordHash))
{
    return LoginResult.Fail();
}
```

### 登入成功後的無痛升級（建議實作）

日後把預設強度調高時，讓使用者在下次登入時自動換成新參數，
不必強制全體重設密碼。這是 OWASP 建議的 work factor 升級做法。

```csharp
if (PasswordHasher.Verify(inputPassword, user.PasswordHash))
{
    if (PasswordHasher.NeedsRehash(user.PasswordHash))
    {
        user.PasswordHash = PasswordHasher.Hash(inputPassword);
        repository.Update(user);
    }
    return LoginResult.Success();
}
```

### 非同步版本

Argon2 是 CPU-bound 運算，非同步版本僅是 `Task.Run` 包裝，用途是避免阻塞
ASP.NET 請求執行緒，**不會**讓運算變快。

```csharp
string phc = await PasswordHasher.HashAsync(password);
bool ok    = await PasswordHasher.VerifyAsync(inputPassword, user.PasswordHash);
```

---

## 2. 儲存格式（PHC）

```
$argon2id$v=19$m=19456,t=2,p=1$8Nwu9oxOQIJNUV+u/zSTNA$smcx8jHtTXU9zSHEhAY2gjU6zPJDpnVaJsxzMxo/jRE
└───┬───┘└─┬─┘└──────┬───────┘└──────────┬──────────┘└─────────────────┬─────────────────────────┘
  演算法   版本    成本參數              salt (16 bytes)              hash (32 bytes)
```

| 欄位 | 說明 |
|---|---|
| `argon2id` | 演算法識別字 |
| `v=19` | Argon2 版本 0x13，即 1.3 規格 |
| `m` / `t` / `p` | 記憶體 (KiB) / 迭代次數 / 平行度 |
| salt / hash | Base64，依 PHC 規範**去除尾端 `=` 填充** |

採用 PHC 而非自訂格式的理由：

1. **參數自帶** — `Verify()` 一律以字串內記載的 m/t/p 重算，而非查詢目前的強度設定表。
   日後調高預設強度，資料庫裡的舊雜湊仍能正常驗證，不會全體登入失敗。
2. **跨語言相容** — PHP `password_verify()`、Python `argon2-cffi`、Go `argon2id`
   等函式庫都認得同一格式，日後系統改寫語言不必轉換密碼。

---

## 3. 資料庫欄位建議

### `VARCHAR(128)`

```sql
-- SQL Server
PasswordHash VARCHAR(128) NOT NULL

-- MySQL
password_hash VARCHAR(128) NOT NULL
```

**長度依據**：目前實際輸出為 **97～98 字元**。

| 組成 | 長度 |
|---|---|
| `$argon2id$v=19$` | 15 |
| `$m=131072,t=3,p=1` | 最長 18 |
| `$` + salt (16 bytes → Base64 去填充) | 1 + 22 |
| `$` + hash (32 bytes → Base64 去填充) | 1 + 43 |
| **合計** | **最長 100** |

開到 128 是留餘裕給日後調整參數（例如記憶體加到 7 位數、或改用更長的 hash），
不必再做欄位異動。**不要**開剛好 100，也不必開 `MAX`。

### 為何用 `VARCHAR` 而非 `NVARCHAR`

PHC 字串只含 Base64 字元集與 `$,=` 等 ASCII 符號，
用 `NVARCHAR` 只會讓儲存空間加倍且無任何好處。

### 不需要 salt 欄位

salt 已編碼在字串內，**不要**另外開 `PasswordSalt` 欄位。

---

## 4. 強度選擇與效能

| 強度 | m (記憶體) | t (迭代) | p (平行度) | 實測 Hash | 實測 Verify |
|---|---|---|---|---|---|
| `Default` | 19456 KiB (19 MiB) | 2 | 1 | 69 ms | 65 ms |
| `High` | 65536 KiB (64 MiB) | 2 | 1 | 158 ms | 156 ms |
| `Highest` | 131072 KiB (128 MiB) | 3 | 1 | 455 ms | 435 ms |

> 實測環境：Windows 11 / net472，各取 5 次平均，僅供相對比較。
> 部署前應在實際伺服器上重測。

### 怎麼選

- **`Default` 已符合 OWASP 建議基準**，一般系統直接用預設值即可。
  這裡的「Default」不是「低強度選項」，請勿因為名稱而誤以為不安全。
- `High` / `Highest` 是在 OWASP 基準**之上**再加碼，屬本專案自訂，
  適用於高價值帳號（管理者、金流）。
- 注意 **記憶體是併發瓶頸**：`Highest` 在 100 個並行登入時會瞬間吃掉
  約 12.8 GB 記憶體。高流量站台請以 `Default` 為主，並在登入端加上速率限制。

### OWASP 的參數表怎麼讀

OWASP 列出的五組參數（46MiB/t=1、19MiB/t=2、12MiB/t=3、9MiB/t=4、7MiB/t=5）
是**防禦強度等效**的替代方案，讓你依機器 RAM 挑一組，
**不是**由弱到強的等級。本專案的 `Default` 採用其中的 19MiB/t=2/p=1。

---

## 5. 本函式不含 pepper

`PasswordHasher` **刻意不實作 pepper**。

**原因**：OWASP 要求 pepper 必須與雜湊值**分開存放**於金鑰保管庫（KMS）或
硬體安全模組（HSM）。這是部署層的決策，若把 pepper 參數塞進這支公用函式，
極易誘導開發者把金鑰硬編碼在原始碼或 `Web.config` 裡 —— 那不但沒有提升安全性，
反而製造出「以為有保護」的錯覺。

**若確實需要 pepper**，請在呼叫端先行 HMAC 再傳入：

```csharp
// pepper 從 KMS / HSM / 環境變數取得，絕不可寫死在程式碼或設定檔中
byte[] pepper = KeyVault.GetSecret("password-pepper");

static string ApplyPepper(string password, byte[] pepper)
{
    using (var hmac = new HMACSHA384(pepper))
    {
        byte[] mac = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(mac);
    }
}

// 註冊
string phc = PasswordHasher.Hash(ApplyPepper(password, pepper));

// 驗證
bool ok = PasswordHasher.Verify(ApplyPepper(inputPassword, pepper), user.PasswordHash);
```

⚠️ 一旦導入 pepper，**輪替金鑰時所有既有雜湊都會失效**，
必須事先規劃輪替策略（例如在雜湊旁存放 pepper 版本號），再決定是否採用。

---

## 6. 錯誤處理行為

| 情境 | 行為 |
|---|---|
| `Hash(null)` | 丟 `ArgumentNullException` |
| `Hash("")` | 丟 `ArgumentException`（空密碼不得儲存） |
| `Hash(password, (Argon2idStrength)99)` | 丟 `ArgumentOutOfRangeException` |
| `Verify(null, phc)` | 回 `false` |
| `Verify("", phc)` | 回 `false` |
| `Verify(password, null)` | 回 `false` |
| `Verify(password, "格式錯誤的字串")` | 回 `false` |
| `NeedsRehash("格式錯誤的字串")` | 回 `true`（視為需重新雜湊） |

**設計原則**：`Verify()` 在任何輸入下都不拋出例外。
資料庫欄位可能存著舊格式、空值或髒資料，若因此拋出例外，
等於讓攻擊者只要送出特製輸入就能癱瘓登入 API。
反之 `Hash()` 的參數錯誤屬於程式邏輯瑕疵，應盡早失敗以利發現。

其他行為：

- 密碼一律以 **UTF-8** 編碼，**無長度上限**（bcrypt 的 72 bytes 截斷問題在此不存在），
  完整支援中文、日文、emoji 等 Unicode 字元。
- 雜湊比對使用**定時比較**（constant-time），避免時序攻擊。
  netstandard2.0 無 `CryptographicOperations.FixedTimeEquals`，故於 `PasswordHasher` 內自行實作。

---

## 7. 參考來源

- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [RFC 9106 — Argon2 Memory-Hard Function](https://datatracker.ietf.org/doc/html/rfc9106)
- [PHC string format 規範](https://github.com/P-H-C/phc-string-format/blob/master/phc-sf-spec.md)
- [Konscious.Security.Cryptography.Argon2](https://github.com/kmaragon/Konscious.Security.Cryptography)
- 實作參考：[黑暗執行緒 — Argon2id 密碼雜湊](https://blog.darkthread.net/blog/argon2id-password-hasher/)
  （本實作在其基礎上改為 PHC 格式、同步為主 API，並補上 netstandard2.0 相容處理）

---

## 8. 測試

```bash
dotnet test General.Test/General.Test.csproj --filter "FullyQualifiedName~PasswordHasherTests"
```

測試涵蓋：round-trip 驗證、salt 隨機性、PHC 格式與無填充 Base64、
三種強度的參數編碼、16 種格式異常輸入、`NeedsRehash` 升級判斷、
Unicode 與 1000 字元長密碼、非同步版本一致性。
