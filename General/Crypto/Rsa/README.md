# RSA — 非對稱加密與數位簽章

用於**加密方與解密方不是同一個人**的場景。若加解密都在同一個系統內完成
（加密資料庫欄位、設定檔），請直接用 [`AesEncryptor`](../README.md) ——
RSA 在那種情境只會更慢、更複雜，沒有任何好處。

- 命名空間：`General.Crypto.Rsa`
- 目標框架：netstandard2.0
- 相依套件：`BouncyCastle.Cryptography` 2.6.2

| 類別 | 用途 |
|---|---|
| `RsaKey` | 金鑰產生、PEM 載入與匯出 |
| `RsaEncryptor` | 混合加密（RSA-OAEP 包裝 AES 金鑰 + AES-256-GCM 加密資料） |
| `RsaSigner` | 數位簽章（RSASSA-PSS + SHA-256） |

---

## 1. 什麼時候該用 RSA

| 情境 | 適用 |
|---|---|
| 前端／邊緣系統只能加密，私鑰離線保管 | ✅ `RsaEncryptor` |
| 與外部廠商交換加密資料 | ✅ `RsaEncryptor`（須配合對方規格） |
| License 授權碼、API 請求簽章、檔案來源驗證 | ✅ `RsaSigner` |
| 加密資料庫欄位、設定檔 | ❌ 用 `AesEncryptor` |

> 若是全新設計且無相容包袱，現代建議其實是 **ECDSA / ECDH (P-256)** ——
> 金鑰小得多、運算快很多、安全強度相當。RSA 的優勢只剩「舊系統都認得」。

---

## 2. 快速上手

### 產生金鑰

```csharp
using General.Crypto.Rsa;

using (var key = RsaKey.Generate())        // 預設 3072-bit
{
    File.WriteAllText("private.pem", key.ToPrivatePem());   // 只給解密／簽章端
    File.WriteAllText("public.pem",  key.ToPublicPem());    // 可公開散布
}
```

### 加密（只需公鑰）

```csharp
using (var key = RsaKey.FromPem(File.ReadAllText("public.pem")))
using (var enc = new RsaEncryptor(key))
{
    string cipher = enc.Encrypt("要保護的內容");   // 無長度上限
}
```

### 解密（需私鑰）

```csharp
using (var key = RsaKey.FromPem(File.ReadAllText("private.pem")))
using (var enc = new RsaEncryptor(key))
{
    string plain = enc.Decrypt(cipher);
}
```

### 簽章與驗證

```csharp
// 簽章端（持私鑰）
using (var key = RsaKey.FromPem(privatePem))
using (var signer = new RsaSigner(key))
{
    string signature = signer.Sign(licenseData);
}

// 驗證端（只需公鑰）
using (var key = RsaKey.FromPem(publicPem))
using (var verifier = new RsaSigner(key))
{
    if (!verifier.Verify(licenseData, signature)) return false;
}
```

> `RsaEncryptor` 與 `RsaSigner` **不會**Dispose 傳入的 `RsaKey`，
> 金鑰的生命週期由呼叫端掌握。

---

## 3. 為什麼是混合加密

RSA 能直接加密的資料量極小：

| 金鑰長度 | OAEP-SHA256 明文上限 |
|---|---|
| RSA-2048 | **190 bytes** |
| RSA-3072 | 318 bytes |
| RSA-4096 | 446 bytes |

**RSA 不是用來加密資料的，是用來加密「金鑰」的。** 因此 `RsaEncryptor`
採標準的信封加密（envelope encryption）：

```
1. 產生一把全新的隨機 AES-256 金鑰
2. 用 AES-256-GCM 加密資料      ← 交給 AesEncryptor，無長度上限
3. 用 RSA-OAEP-SHA256 包裝那把 AES 金鑰
4. 輸出 Base64(version(1) ‖ wrappedKeyLength(2) ‖ wrappedKey ‖ AesEncryptor 密文)
```

好處是資料同時受 GCM 的完整性保護 —— 密文遭竄改解密會直接失敗。
每次加密都會產生全新的 AES 金鑰與 nonce，相同明文不會產生相同密文。

---

## 4. 演算法與參數

| 項目 | 設定 | 說明 |
|---|---|---|
| 加密 padding | **RSA-OAEP**，SHA-256 + MGF1-SHA256 | 不開放選擇 |
| 簽章 padding | **RSASSA-PSS**，SHA-256、salt 32 bytes | 不開放選擇 |
| 預設金鑰長度 | **3072** bits | NIST 建議 2030 年後的強度 |
| 最低金鑰長度 | **2048** bits | 更短的金鑰載入時直接丟例外 |
| 對稱層 | AES-256-GCM | 沿用 `AesEncryptor` |

**為什麼 padding 不開放選擇**：PKCS#1 v1.5 加密有
[Bleichenbacher 攻擊](https://en.wikipedia.org/wiki/Adaptive_chosen-ciphertext_attack)，
不該再被使用。把它列為選項只會讓人選錯。

### 與其他語言／工具互通

參數對應如下，已實測與 OpenSSL 雙向互通：

| 我方 | OpenSSL 對應參數 |
|---|---|
| `RsaEncryptor` 的 OAEP | `-pkeyopt rsa_padding_mode:oaep -pkeyopt rsa_oaep_md:sha256 -pkeyopt rsa_mgf1_md:sha256` |
| `RsaSigner` 的 PSS | `-sigopt rsa_padding_mode:pss -sigopt rsa_pss_saltlen:32`（搭配 `-sha256`） |

驗證我方簽章：

```bash
openssl dgst -sha256 -verify public.pem \
  -sigopt rsa_padding_mode:pss -sigopt rsa_pss_saltlen:32 \
  -signature signature.bin message.bin
```

> 注意：`RsaEncryptor` 的**信封格式是本專案自訂的**，外部系統無法直接解讀整包密文。
> 可互通的是金鑰格式、OAEP 與 PSS 這三者。若需與外部系統交換完整密文，
> 請雙方先議定信封格式。

---

## 5. 金鑰格式

| 匯出 | 格式 | PEM 標頭 |
|---|---|---|
| `ToPublicPem()` | SubjectPublicKeyInfo | `-----BEGIN PUBLIC KEY-----` |
| `ToPrivatePem()` | PKCS#8 | `-----BEGIN PRIVATE KEY-----` |

`FromPem()` 除上述兩種外，也接受 PKCS#1 私鑰
（`-----BEGIN RSA PRIVATE KEY-----`），因為舊版 OpenSSL 常產出此格式。

### ⚠️ 私鑰保管

| 該做 | 不該做 |
|---|---|
| 私鑰存於 KMS / HSM / 受限權限的檔案 | ❌ 提交進版控 |
| 只把 `public.pem` 散布出去 | ❌ 把 `private.pem` 一起發佈 |
| 不同環境用不同金鑰對 | ❌ 開發與正式共用 |

**加密端只需要公鑰。** 若某個系統只負責加密或驗簽，就不要把私鑰放上去 ——
這正是採用非對稱加密的意義所在。

---

## 6. 錯誤處理行為

| 情境 | 行為 |
|---|---|
| `RsaKey.Generate(< 2048)` | `ArgumentOutOfRangeException` |
| `RsaKey.FromPem(null)` | `ArgumentNullException` |
| PEM 格式無效／非 RSA 金鑰／長度不足 | `CryptographicException` |
| `ToPrivatePem()` 但只有公鑰 | `InvalidOperationException` |
| `Decrypt()` 但只有公鑰 | `InvalidOperationException` |
| `Sign()` 但只有公鑰 | `InvalidOperationException` |
| 密文遭竄改／金鑰錯誤／格式錯誤 | `CryptographicException`（訊息一律相同） |
| 簽章不符／格式錯誤／非 Base64／null | `Verify()` 回 **`false`**，不丟例外 |
| Dispose 後再使用 | `ObjectDisposedException` |

設計原則：

- **「只有公鑰卻要求私鑰操作」是程式邏輯錯誤**，用 `InvalidOperationException`
  與密文錯誤明確區分開來，方便開發時及早發現。
- **解密失敗一律回報相同訊息**，不區分「OAEP 解碼失敗」「格式錯誤」「版本不符」，
  避免差異化的錯誤訊息成為攻擊者的 oracle。呼叫端亦不應把例外細節回傳給外部使用者。
- **`Verify()` 在任何輸入下都不丟例外**，避免攻擊者以特製輸入癱瘓驗證端。

其他行為：

- 字串一律以 **UTF-8** 編碼
- PSS 每次簽章都帶入隨機 salt，**相同資料的兩次簽章值會不同**，兩者都能通過驗證。
  切勿以「比對簽章字串是否相等」來做驗證，必須呼叫 `Verify()`

---

## 7. 實作說明

金鑰材料與所有 RSA 運算皆以 BouncyCastle 實作，未使用 BCL 的
`System.Security.Cryptography.RSA`。

原因是 .NET Framework 上 `RSA.Create()` 會回傳 `RSACryptoServiceProvider`，
它**不支援 OAEP-SHA256 與 PSS**（只支援 OAEP-SHA1 與 PKCS#1 v1.5）；
而支援這些的 `RSACng` 在 netstandard2.0 無法取得。改用 BouncyCastle 可確保
各執行環境行為完全一致，不會出現「在 .NET 8 正常、在 .NET Framework 拋例外」。

---

## 8. 參考來源

- [RFC 8017 — PKCS #1 v2.2（OAEP 與 PSS）](https://datatracker.ietf.org/doc/html/rfc8017)
- [NIST SP 800-57 — 金鑰長度建議](https://csrc.nist.gov/publications/detail/sp/800-57-part-1/rev-5/final)
- [OWASP Cryptographic Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cryptographic_Storage_Cheat_Sheet.html)
- [BouncyCastle for .NET](https://github.com/bcgit/bc-csharp)

---

## 9. 測試

```bash
dotnet test General.Test/General.Test.csproj --filter "FullyQualifiedName~RsaTests"
```

測試涵蓋：金鑰產生與長度下限、PEM 匯出入 round-trip（PKCS#8 / SPKI / PKCS#1）、
公鑰與私鑰能力區分、混合加密突破 190 bytes 限制（512 KB 資料）、Unicode、
密文竄改偵測、錯誤金鑰、PSS 簽章的隨機性與驗證、簽章與資料竄改偵測、
以 PEM 傳遞金鑰的完整跨端流程、併發操作。
