# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test

```bash
# Build and test the whole solution — the dotnet CLI works now that the .NET 3.5 projects
# have been moved out to legacy/ (see "Legacy Projects" below)
dotnet build General.sln
dotnet test General.sln

# Run a single test by name filter
dotnet test General.Test/General.Test.csproj --filter "Name~Translate_EqualNull"

# msbuild still works and is required for anything under legacy/
msbuild General.sln /p:Configuration=Debug
```

`General.Test` is the actively maintained test project (MSTest v2, net472); it covers the SQL Condition Converter in `General/Data/SQLConditionConverter/`.

## Architecture

This is a multi-project .NET class library solution providing shared infrastructure for data access, cryptography, notifications, validation, and ASP.NET web controls.

### Mixed Target Frameworks & Project Styles

- **General** (core library): .NET Standard 2.0, SDK-style csproj with PackageReference (Newtonsoft.Json, SystemWebAdapters, System.DirectoryServices, Konscious Argon2, BouncyCastle)
- **General.Test**: net472, SDK-style, MSTest v2 — references General
- **General.Data** (ORM): .NET Framework 4.0, legacy csproj, uses Dapper 1.50 via packages.config
- **General.DataExpress** (lightweight DAC): .NET Framework 4.0, legacy csproj

### Project Map (projects in General.sln)

| Project | Folder | Purpose |
|---|---|---|
| General | `General/` | Core library: AES-GCM encryption, RSA encryption/signing, Argon2id password hashing, mail/Skype notification, SQL condition translator (new version), column mapper |
| General.Test | `General.Test/` | MSTest v2 tests for General (SQLConditionConverter, PasswordHasher, AesEncryptor, Rsa) |
| General.Data | `Gerenal.Data/` | Full ORM layer with Dapper, Expression Tree → SQL WHERE translator, CRUD command builders |
| General.DataExpress | `General.DataLw/` | Lightweight ADO.NET-only data access (no Dapper) |

### Retired Projects (deleted)

Six .NET 3.5-era projects were retired and **deleted from the working tree**: `General.CC`
(ASP.NET custom controls), `General.UC` (ASP.NET web app), `Gerneral.Helper` (validation
utilities), `General.Data.Test` (MSTest v1), `General.Log`, and `General.Log.Test`.

They were first moved to `legacy/` (commit `3716746`), then deleted. To recover any of them,
check out the tree at that commit — the full history is preserved:

```bash
git show 3716746 --stat                    # see what was there
git checkout 3716746 -- legacy/General.CC  # restore one project
```

They also carried known-vulnerable dependencies that are now gone from the working tree:
log4net 2.0.3 (CVE-2018-1285 XXE), jQuery 2.1.0/1.9.1 (four XSS CVEs), a 2007-era jQuery UI
datepicker, and a Flash `uploadify.swf` (CVE-2018-9173). Do not resurrect these files without
updating those dependencies first.

`General.Extension/` also exists on disk and is not in the solution.

### Key Design Patterns

**Expression Tree → SQL Translation (two versions)**:
- **Old** (`Gerenal.Data/Base/QueryTranslator.cs`): `QueryTranslator : ExpressionVisitor` — outputs `Queue<QueryCondition>` → WHERE string with `@T1_FieldName_0` parameters. Null values are auto-skipped for dynamic queries.
- **New** (`General/Data/SQLConditionConverter/`): `TranslatorBase<T>` (ExpressionVisitor) + `QueryConditionTranslator<T>`. Fluent API: `AddCondition(expr)` / `AddTextCondition(sql, params QueryParameter[])` → `Translate()` (re-callable; state resets between calls) → parameters exposed via `Parameters`. Behaviors to preserve:
  - `== null` / `!= null` → `IS NULL` / `IS NOT NULL` (also when operands are reversed, e.g. `null == p.X`)
  - `Contains`/`StartsWith`/`EndsWith` → LIKE with `%` wrapping in the parameter value
  - Collection `Contains` → `IN (...)`; empty collection → `(1=0)`; >2100 items → inline literals instead of parameters (SQL Server parameter limit)
  - String parameter `DbType` auto-detected: ASCII → `AnsiString` (VarChar), non-ASCII (CJK etc.) → `String` (NVarChar)
  - Column aliasing via `[ColumnMapping(ColumnName=..., AliasName=...)]` on properties, or a class-level attribute implementing `IConditionMappingAttribute<T>` (also controls `IsSkipNullOrEmpty` — skips null/empty *string* conditions only, never value types)
- **`QuerySelectBuilder<T>`** (same folder): builds SELECT column lists — `Column(expr, tableAlias, alias)`, `From(alias, exprs...)`, `RawColumn(sql)`, `ColumnAll(alias)`, `Reset()`; honors the same mapping attributes.

**ORM Command Builder** (`Gerenal.Data/DapperExtensions/`):
Abstract `DapperCommandBuilder` with four implementations (Select/Insert/Update/Delete). `DataBase` abstract class auto-initializes all four via `[TableMapping("table")]` attribute. Subclasses get fluent API: `Select("*").Where<T>(expr).Query<T>()`.

**Provider abstraction**:
`IDACAdapter` → `ProviderBase` (reads `<connectionStrings>`) → `DefaultProvider` (uses `DbProviderFactory` for DB-neutral ADO.NET).

**Dapper type mapping**: `FallbackTypeMapper` + `ColumnAttributeTypeMapper<T>` lets Dapper recognize `[Column(Name="xxx")]` attributes with fallback to default mapping.

**ExpressionFactory** (`Gerenal.Data/Base/ExpressionFactory.cs`): Compiles property setter delegates via Expression Trees with static cache (double-checked locking) to avoid runtime reflection.

**Asymmetric crypto** (`General/Crypto/Rsa/`): `RsaKey` (generate / PEM import-export, PKCS#8 +
SPKI out, PKCS#1 also accepted in), `RsaEncryptor` (envelope encryption — RSA-OAEP-SHA256 wraps a
fresh AES-256 key, `AesEncryptor` encrypts the data, so there is no 190-byte limit), `RsaSigner`
(RSASSA-PSS + SHA-256). Padding is not configurable — PKCS#1 v1.5 encryption is Bleichenbacher-
vulnerable and is deliberately unreachable. **All RSA operations go through BouncyCastle, not the
BCL**: on .NET Framework `RSA.Create()` returns `RSACryptoServiceProvider`, which supports neither
OAEP-SHA256 nor PSS, and `RSACng` is unavailable on netstandard2.0. Verified interoperable with
OpenSSL in both directions. Requesting a private-key operation on a public-only key throws
`InvalidOperationException` (a programming error), while ciphertext problems throw
`CryptographicException` with a uniform message. See `General/Crypto/Rsa/README.md`.

**Reversible encryption** (`General/Crypto/AesEncryptor.cs`): AES-256-GCM via BouncyCastle, output
`Base64(version(1) ‖ nonce(12) ‖ ciphertext ‖ tag(16))`. Nonce is always generated internally — there
is deliberately no API to supply one, because nonce reuse under GCM is catastrophic. All decryption
failures throw `CryptographicException` with an **identical** message so the error cannot be used as
an oracle. Native `AesGcm` is unreachable here (needs netstandard2.1, which .NET Framework does not
support), hence BouncyCastle. The old `CryptoProvider`/`AESCrypto`/`RijndaelCrypto`/`CryptoEum` were
removed — they used unauthenticated CBC and were vulnerable to padding oracle attacks. See
`General/Crypto/README.md`.

**Password hashing** (`General/Crypto/Password/`): `PasswordHasher` — Argon2id via Konscious, PHC
string output (`$argon2id$v=19$m=19456,t=2,p=1$salt$hash`). `Verify()` re-derives using the
parameters embedded in the stored string, so changing the strength table never invalidates existing
hashes; `NeedsRehash()` drives upgrade-on-login. See `General/Crypto/Password/README.md` for the
DB column recommendation (`VARCHAR(128)`), strength/perf table, and why pepper is intentionally
not implemented.

### General.Data vs General.DataExpress

Both share namespace `General.Data` but **cannot be referenced together** (namespace collision). DataExpress (`General.DataLw/`, "Lw" = Lightweight) is a minimal ADO.NET-only version without Dapper or ORM. Their `IDACAdapter` interfaces are incompatible — DataExpress uses `GetDataRow()`/`GetDataSet()` naming vs the older `DataRow()`/`DataSet()`.

### Folder Name Typos (Historical)

- `Gerenal.Data/` (not "General") — contains `General.Data.csproj`
- `legacy/Gerneral.Helper/` (not "General") — contains `Gerneral.Helper.csproj`

These are intentional historical directory names; do not rename them.
