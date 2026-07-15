# SQLConditionConverter

Expression Tree 驅動的 SQL 查詢建構工具，可將 C# Lambda 表達式轉譯為 SQL WHERE 條件和 SELECT 欄位。設計為**不綁定特定 SQL 結構**，無論是單一資料表查詢或多表 JOIN 都能使用。

## 命名空間

```csharp
using General.Data.SQLConditionConverter;
using General.Data.SQLConditionConverter.Interfaces;
```

---

## QueryConditionTranslator\<T\>

將 Lambda 表達式轉譯為 SQL WHERE 條件字串與參數化的 `QueryParameters`。

### 基本使用

```csharp
var translator = new QueryConditionTranslator<Product>();
translator
    .AddCondition(p => p.Price > 100)
    .AddCondition(p => p.Name != null);

string where = translator.Translate();
// "(Price > @c1) AND (Name IS NOT NULL)"

QueryParameters parameters = translator.Parameters;
// parameters[0]: { Name = "@c1", Value = 100, DbType = Decimal }
```

### 支援的運算子

| 運算子 | C# 表達式 | SQL 輸出 |
|---|---|---|
| 等於 | `p => p.Status == 1` | `Status = @c1` |
| 不等於 | `p => p.Status != 1` | `Status <> @c1` |
| 大於/小於 | `p => p.Price > 100` | `Price > @c1` |
| 大於等於/小於等於 | `p => p.Price >= 100` | `Price >= @c1` |
| IS NULL | `p => p.Name == null` | `Name IS NULL` |
| IS NOT NULL | `p => p.Name != null` | `Name IS NOT NULL` |
| AND | `p => p.A == 1 && p.B == 2` | `A = @c1 AND B = @c2` |
| OR | `p => p.A == 1 \|\| p.B == 2` | `A = @c1 OR B = @c2` |
| LIKE (Contains) | `p => p.Name.Contains("test")` | `Name LIKE @c1` (值 = `%test%`) |
| LIKE (StartsWith) | `p => p.Name.StartsWith("A")` | `Name LIKE @c1` (值 = `A%`) |
| LIKE (EndsWith) | `p => p.Name.EndsWith("Z")` | `Name LIKE @c1` (值 = `%Z`) |
| IN (陣列) | `p => arr.Contains(p.Type)` | `Type IN (@c1,@c2,@c3)` |
| IN (new 陣列) | `p => new[]{1,2}.Contains(p.Type)` | `Type IN (@c1,@c2)` |
| IN (List) | `p => list.Contains(p.Type)` | `Type IN (@c1,@c2)` |

> **IN 特殊處理**：當集合元素超過 2100 個時，會自動改為 inline literal 而非參數化，以避免 SQL Server 參數數量上限。空集合會輸出 `(1=0)` 避免產生無效 SQL。

### 多條件

多次 `AddCondition` 之間以 `AND` 連接，每個條件各自用括號包裹：

```csharp
translator
    .AddCondition(p => p.Status > 0 || p.Name != null)  // 內部用 OR
    .AddCondition(p => p.Type == 1);

// "(Status > @c1 OR Name IS NOT NULL) AND (Type = @c2)"
```

### 文字條件 (TextCondition)

用於無法以 Expression 表達的複雜 SQL（子查詢、EXISTS 等），可自帶參數：

```csharp
translator.AddTextCondition("EXISTS (SELECT 1 FROM Orders WHERE UserID = @p1)",
    new QueryParameter { Name = "@p1", Value = 100, DbType = DbType.Int32 });
```

文字條件與 Expression 條件之間也以 `AND` 連接。多個文字條件之間同樣以 `AND` 連接。

### 反向比較

Expression 中值寫在左邊、欄位寫在右邊時，會自動交換為正確順序：

```csharp
int val = 5;
translator.AddCondition(p => val == p.Status);
// 輸出: "Status = @c1"，而非 "@c1 = Status"

translator.AddCondition(p => null == p.Name);
// 輸出: "Name IS NULL"
```

### 重複使用

同一個 Translator 實例可重複呼叫 `Translate()`，每次呼叫會自動重置內部狀態：

```csharp
translator.AddCondition(p => p.Status == 1);

string result1 = translator.Translate();  // "(Status = @c1)"
string result2 = translator.Translate();  // "(Status = @c1)" — 結果相同，參數重建
```

### 參數長度 (Size / Length)

`QueryParameter` 具備 `Size` 屬性，用於指定參數長度。為 SQL Server 提供**固定**的參數長度可讓相同查詢重用執行計畫（否則會因傳入值長度不同而被視為不同簽章、產生多份計畫）。`Size = 0` 表示未指定，由 ADO.NET provider 依 `DbType` 與實際值自行推導。

長度有兩個來源：

**1. 由 `ColumnMappingAttribute.Length` 自動填入**

在 Property 上宣告 `Length`，翻譯器會自動把該欄位相關參數的 `Size` 設為宣告值：

```csharp
public class UserEntity
{
    [ColumnMapping(ColumnName = "user_name", Length = 50)]
    public string Name { get; set; }

    [ColumnMapping(ColumnName = "code", Length = 10)]
    public string Code { get; set; }
}

var where = new QueryConditionTranslator<UserEntity>();
where.AddCondition(p => p.Name == "abc");   // 參數 Size = 50
where.AddCondition(p => p.Code != "x1");    // 參數 Size = 10
```

套用範圍：`=`、`<>`、`>`、`<` 等比較，以及 `IN (...)`（每個元素都套用）。反向比較（值在左、欄位在右）交換後也正確套用。

> **LIKE 例外**：`Contains`/`StartsWith`/`EndsWith` 產生的參數**不套用** `Length`（`Size` 維持 0）。因為 LIKE 的值會包上萬用字元（`Contains("abc")` → `%abc%`，比原值多 2 字元），若套用欄位長度會在搜尋字串接近上限時被 ADO.NET 靜默截斷，導致查詢結果錯誤。

未宣告 `Length`、透過 `IConditionMappingAttribute` 映射、或無 `ColumnMapping` 的欄位，`Size` 一律維持 0。

**2. 由呼叫端於 `AddTextCondition` 自訂**

文字條件的參數由呼叫端自行設定 `Size`，翻譯器不會覆寫：

```csharp
translator.AddTextCondition("Name = @p1",
    new QueryParameter { Name = "@p1", Value = "abc", DbType = DbType.AnsiString, Size = 50 });
```

### TraceMode

建構時傳入 `true` 啟用 Trace 模式，會透過 `Debug.WriteLine` 輸出 Expression 轉譯過程：

```csharp
var translator = new QueryConditionTranslator<Product>(endableTraceMode: true);
```

---

## QuerySelectBuilder\<T\>

以 Expression 建構 SQL SELECT 欄位清單，支援欄位映射、table alias 覆蓋和原始 SQL。

### 基本使用

```csharp
var select = new QuerySelectBuilder<Product>();
select
    .Column(p => p.ID)
    .Column(p => p.Name)
    .Column(p => p.Price);

string columns = select.Build();
// "ID, Name, Price"
```

未選取任何欄位時，`Build()` 回傳 `*`。

### API 一覽

#### Column — 單一欄位

```csharp
// 基本
.Column(p => p.Name)                                    // "Name"

// 指定 table alias
.Column(p => p.Name, tableAlias: "u")                   // "u.Name"

// 指定 output alias (AS)
.Column(p => p.Name, alias: "DisplayName")               // "Name AS DisplayName"

// 同時指定 table alias 和 output alias
.Column(p => p.ID, tableAlias: "u", alias: "UserID")     // "u.ID AS UserID"
```

#### From — 群組選取

同一個 table alias 下批次選取多個欄位，減少重複指定 alias：

```csharp
// 單一群組
.From("u", p => p.ID, p => p.Name, p => p.Status)
// "u.ID, u.Name, u.Status"

// 多群組 — 多表 JOIN 場景
.From("u", p => p.ID, p => p.Name)
.From("o", p => p.Amount, p => p.CreatedOn)
// "u.ID, u.Name, o.Amount, o.CreatedOn"
```

#### RawColumn — 原始 SQL

用於聚合函數或無法以 Expression 表達的欄位：

```csharp
.RawColumn("COUNT(*) AS Total")
.RawColumn("DATEDIFF(DAY, o.CreatedOn, GETDATE()) AS DaysSince")
```

#### ColumnAll — 全選

```csharp
.ColumnAll()          // "*"
.ColumnAll("u")       // "u.*"
```

#### Reset — 清除重用

```csharp
var builder = new QuerySelectBuilder<Product>();
builder.Column(p => p.ID).Column(p => p.Name);
string r1 = builder.Build();  // "ID, Name"

builder.Reset().Column(p => p.Price);
string r2 = builder.Build();  // "Price"
```

#### 混合使用

所有 API 可自由組合：

```csharp
var select = new QuerySelectBuilder<SearchEntity>()
    .Column(p => p.ID, tableAlias: "u", alias: "UserID")
    .From("o", p => p.Amount, p => p.CreatedOn)
    .RawColumn("COUNT(*) AS OrderCount")
    .ColumnAll("d");

select.Build();
// "u.ID AS UserID, o.Amount, o.CreatedOn, COUNT(*) AS OrderCount, d.*"
```

---

## 欄位名稱映射

`QueryConditionTranslator` 和 `QuerySelectBuilder` 共用同一套映射機制，解析優先順序為：

1. **IConditionMappingAttribute\<T\>** — Entity class 上的 Attribute，透過 `Mapping()` 方法定義集中映射
2. **ColumnMappingAttribute** — Property 上的 Attribute，定義 `ColumnName` 和 `AliasName`（table alias）
3. **Property 名稱** — 如果以上都未定義，直接使用 C# 屬性名稱

### 方式一：ColumnMappingAttribute（適合單表或固定別名）

```csharp
public class UserEntity
{
    [ColumnMapping(ColumnName = "user_id")]
    public int ID { get; set; }

    [ColumnMapping(ColumnName = "user_name", AliasName = "u")]
    public string Name { get; set; }

    public int Status { get; set; }  // 無 Attribute，使用 "Status"
}
```

```csharp
var select = new QuerySelectBuilder<UserEntity>();
select.Column(p => p.ID);     // "user_id"
select.Column(p => p.Name);   // "u.user_name"
select.Column(p => p.Status); // "Status"

var where = new QueryConditionTranslator<UserEntity>();
where.AddCondition(p => p.Name == "test");
where.Translate(); // "u.user_name = @c1"
```

### 方式二：IConditionMappingAttribute（適合多表 JOIN，集中管理映射）

```csharp
[OrderSearchMapping]
public class OrderSearchEntity
{
    public int ID { get; set; }
    public string UserName { get; set; }
    public decimal OrderAmount { get; set; }
    public int Status { get; set; }
}

public class OrderSearchMappingAttribute : Attribute, IConditionMappingAttribute<OrderSearchEntity>
{
    public Dictionary<Expression<Func<OrderSearchEntity, object>>, string> MappingCollection { get; set; }
        = new Dictionary<Expression<Func<OrderSearchEntity, object>>, string>();
    public bool IsSkipNullOrEmpty { get; set; }

    public void Mapping()
    {
        // Key: Entity property, Value: 實際 SQL 欄位（含 table alias）
        MappingCollection[p => p.ID] = "u.ID";
        MappingCollection[p => p.UserName] = "u.Name";
        MappingCollection[p => p.OrderAmount] = "o.Amount";
        MappingCollection[p => p.Status] = "u.Status";

        IsSkipNullOrEmpty = true;  // 啟用後，null 和空字串的條件會自動跳過
    }
}
```

```csharp
// SELECT 和 WHERE 共用同一份映射
var select = new QuerySelectBuilder<OrderSearchEntity>()
    .Column(p => p.UserName)       // "u.Name"
    .Column(p => p.OrderAmount);   // "o.Amount"

var where = new QueryConditionTranslator<OrderSearchEntity>();
where.AddCondition(p => p.Status > 0);      // "u.Status > @c1"
where.AddCondition(p => p.UserName == "A");  // "u.Name = @c2"
```

### table alias 覆蓋

無論使用哪種映射方式，`Column()` 和 `From()` 的 `tableAlias` 參數都可以**覆蓋** Attribute 中定義的 table alias：

```csharp
// OrderSearchEntity 映射 UserName => "u.Name"
select.Column(p => p.UserName, tableAlias: "u2");  // "u2.Name"

// ColumnMappingAttribute 定義 AliasName = "u"
select.Column(p => p.Name, tableAlias: "x");       // "x.user_name"
```

### SkipNullOrEmpty

在 `IConditionMappingAttribute` 中設定 `IsSkipNullOrEmpty = true` 後，`QueryConditionTranslator` 會自動跳過值為 `null` 或空字串的條件，適合用在動態查詢表單：

```csharp
// 假設 entity 的 UserName 為 null、Status 為 5
where.AddCondition(p => p.UserName == entity.UserName);  // 被跳過
where.AddCondition(p => p.Status == entity.Status);      // "u.Status = @c1"
```

> 注意：此設定只影響 `QueryConditionTranslator`，不影響 `QuerySelectBuilder`。

---

## 完整範例：多表 JOIN 查詢

```csharp
// 1. 定義查詢參數 Entity 及映射
[OrderSearchMapping]
public class OrderSearchEntity
{
    public int UserID { get; set; }
    public string UserName { get; set; }
    public decimal OrderAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public int Status { get; set; }
}

// 2. 建構 SELECT
var select = new QuerySelectBuilder<OrderSearchEntity>()
    .Column(p => p.UserID)
    .Column(p => p.UserName, alias: "Name")
    .Column(p => p.OrderAmount)
    .Column(p => p.OrderDate)
    .RawColumn("COUNT(*) OVER() AS TotalCount");

// 3. 建構 WHERE
var where = new QueryConditionTranslator<OrderSearchEntity>();
where.AddCondition(p => p.Status > 0);
where.AddCondition(p => p.OrderAmount >= 1000m);
where.AddCondition(p => p.UserName.Contains("test"));

// 4. 組合 SQL
string sql = $@"
SELECT {select.Build()}
FROM Users u
JOIN Orders o ON u.ID = o.UserID
WHERE {where.Translate()}
ORDER BY o.OrderDate DESC";

// 5. 取得參數
QueryParameters parameters = where.Parameters;
// 可直接用於 Dapper、ADO.NET 等
```

輸出的 SQL：

```sql
SELECT u.UserID, u.Name AS Name, o.Amount, o.OrderDate, COUNT(*) OVER() AS TotalCount
FROM Users u
JOIN Orders o ON u.ID = o.UserID
WHERE (u.Status > @c1) AND (o.Amount >= @c2) AND ((u.Name LIKE @c3))
ORDER BY o.OrderDate DESC
```

---

## 類別與介面參考

| 類別/介面 | 說明 |
|---|---|
| `QueryConditionTranslator<T>` | WHERE 條件轉譯器 |
| `QuerySelectBuilder<T>` | SELECT 欄位建構器 |
| `QueryParameters` | 參數集合 (`List<QueryParameter>`) |
| `QueryParameter` | 單一參數（Name, Value, DbType, Size） |
| `ColumnMappingAttribute` | Property 層級欄位映射（ColumnName, AliasName, Length） |
| `IConditionMappingAttribute<T>` | Entity 層級集中映射介面 |
| `IQueryConditionExtractor` | Translator 的公開介面（Parameters, Translate） |
