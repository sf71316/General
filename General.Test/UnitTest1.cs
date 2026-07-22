using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using General.Data.SQLConditionConverter;
using General.Data.SQLConditionConverter.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace General.Test
{
    #region Test Entities

    public enum TestCategories
    {
        C1, C2, C3, C4, C5, C6,
    }

    public class TestEntity
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public TestCategories Categories { get; set; }
    }

    public class TestEntityWithMapping
    {
        [ColumnMapping(ColumnName = "user_name", AliasName = "t")]
        public string Name { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
    }

    public class SkipNullEntity
    {
        public string Name { get; set; }
        public int Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    [SkipNullMapping]
    public class SkipNullWithAttrEntity
    {
        public string Name { get; set; }
        public int Status { get; set; }
        public decimal Amount { get; set; }
    }

    public class SkipNullMappingAttribute : Attribute, IConditionMappingAttribute<SkipNullWithAttrEntity>
    {
        public Dictionary<Expression<Func<SkipNullWithAttrEntity, object>>, string> MappingCollection { get; set; }
            = new Dictionary<Expression<Func<SkipNullWithAttrEntity, object>>, string>();
        public bool IsSkipNullOrEmpty { get; set; }

        public void Mapping()
        {
            IsSkipNullOrEmpty = true;
        }
    }

    public class LengthEntity
    {
        [ColumnMapping(ColumnName = "user_name", Length = 50)]
        public string Name { get; set; }

        [ColumnMapping(ColumnName = "code", Length = 10)]
        public string Code { get; set; }

        // 有 ColumnMapping 但未宣告 Length → Size 應為 0
        [ColumnMapping(ColumnName = "memo")]
        public string Memo { get; set; }

        // 完全無 ColumnMapping → Size 應為 0
        public string Description { get; set; }

        public int Status { get; set; }
    }

    // For QuerySelectBuilder tests
    public class SelectEntity
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public int Status { get; set; }
    }

    public class SelectEntityWithAttr
    {
        [ColumnMapping(ColumnName = "user_id")]
        public int ID { get; set; }

        [ColumnMapping(ColumnName = "user_name", AliasName = "u")]
        public string Name { get; set; }

        [ColumnMapping(ColumnName = "order_amount", AliasName = "o")]
        public decimal Amount { get; set; }

        public int Status { get; set; }
    }

    [JoinMapping]
    public class JoinSearchEntity
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public decimal OrderAmount { get; set; }
        public int Status { get; set; }
    }

    public class JoinMappingAttribute : Attribute, IConditionMappingAttribute<JoinSearchEntity>
    {
        public Dictionary<Expression<Func<JoinSearchEntity, object>>, string> MappingCollection { get; set; }
            = new Dictionary<Expression<Func<JoinSearchEntity, object>>, string>();
        public bool IsSkipNullOrEmpty { get; set; }

        public void Mapping()
        {
            MappingCollection[p => p.ID] = "u.ID";
            MappingCollection[p => p.UserName] = "u.Name";
            MappingCollection[p => p.OrderAmount] = "o.Amount";
            MappingCollection[p => p.Status] = "u.Status";
        }
    }

    #endregion

    [TestClass]
    public class UnitTest1
    {
        #region Issue #1/#2: Translate() double AND and missing spaces

        [TestMethod]
        public void Translate_MultipleExpressions_NoDoubleAND()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Status > 0);
            translator.AddCondition(p => p.Type == 1);
            var result = translator.Translate();

            // Should have exactly one " AND " between two conditions, no double AND
            Assert.IsFalse(result.Contains("AND  AND"), $"Double AND found: {result}");
            Assert.IsTrue(result.Contains(" AND "), $"Missing AND: {result}");
        }

        [TestMethod]
        public void Translate_TextConditionOnly_NoLeadingAND()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddTextCondition("Status = 1");
            var result = translator.Translate();

            // Should not start with AND
            Assert.IsFalse(result.TrimStart().StartsWith("AND"), $"Starts with AND: {result}");
            Assert.IsTrue(result.Contains("(Status = 1)"), $"Missing text condition: {result}");
        }

        [TestMethod]
        public void Translate_ExpressionAndTextCondition_SingleANDBetween()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Status > 0);
            translator.AddTextCondition("Type = 1");
            var result = translator.Translate();

            // Should have exactly one AND between expression and text, not double
            Assert.IsFalse(result.Contains("AND  AND"), $"Double AND found: {result}");
            Assert.IsTrue(result.Contains(") AND (Type = 1)"), $"Unexpected format: {result}");
        }

        [TestMethod]
        public void Translate_MultipleTextConditions_SpacedAND()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddTextCondition("A = 1");
            translator.AddTextCondition("B = 2");
            var result = translator.Translate();

            // Should have " AND " (with spaces) between text conditions
            Assert.IsTrue(result.Contains("(A = 1) AND (B = 2)"), $"Missing spaced AND: {result}");
            Assert.IsFalse(result.Contains(")AND("), $"Missing space in AND: {result}");
        }

        #endregion

        #region Issue #3: setVariable SkipNullOrEmpty logic - non-string types should not be skipped

        [TestMethod]
        public void Translate_SkipNullOrEmpty_IntValueNotSkipped()
        {
            var translator = new QueryConditionTranslator<SkipNullWithAttrEntity>();
            translator.AddCondition(p => p.Status == 5);
            var result = translator.Translate();

            // Int value 5 should NOT be skipped even with SkipNullOrEmpty enabled
            Assert.IsTrue(result.Contains("Status"), $"Status field was skipped: {result}");
            Assert.IsTrue(translator.Parameters.Count > 0, "Parameters should contain the int value");
            Assert.AreEqual(5, translator.Parameters.First().Value);
        }

        [TestMethod]
        public void Translate_SkipNullOrEmpty_DecimalValueNotSkipped()
        {
            var translator = new QueryConditionTranslator<SkipNullWithAttrEntity>();
            translator.AddCondition(p => p.Amount > 100m);
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("Amount"), $"Amount field was skipped: {result}");
            Assert.IsTrue(translator.Parameters.Count > 0, "Parameters should contain the decimal value");
        }

        [TestMethod]
        public void Translate_SkipNullOrEmpty_EmptyStringSkipped()
        {
            string emptyVal = "";
            var translator = new QueryConditionTranslator<SkipNullWithAttrEntity>();
            translator.AddCondition(p => p.Name == emptyVal);
            var result = translator.Translate();

            // Empty string should be skipped
            Assert.IsFalse(translator.Parameters.Any(p => p.Value is string s && s == ""),
                "Empty string should be skipped");
        }

        [TestMethod]
        public void Translate_SkipNullOrEmpty_NonEmptyStringNotSkipped()
        {
            string val = "test";
            var translator = new QueryConditionTranslator<SkipNullWithAttrEntity>();
            translator.AddCondition(p => p.Name == val);
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("Name"), $"Name field was skipped: {result}");
            Assert.IsTrue(translator.Parameters.Any(p => (string)p.Value == "test"),
                "Non-empty string should be in parameters");
        }

        #endregion

        #region Issue #4: setVariable null NullReferenceException

        [TestMethod]
        public void Translate_NullMemberValue_NoException()
        {
            string nullVal = null;
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Name == nullVal);
            var result = translator.Translate();

            // Should produce IS NULL, not throw NRE
            Assert.IsTrue(result.Contains("NULL"), $"Should contain NULL: {result}");
        }

        [TestMethod]
        public void Translate_EqualNull_ProducesISNULL()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Description == null);
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("IS NULL"), $"Should use IS NULL: {result}");
            Assert.IsFalse(result.Contains("IS NOT NULL"), $"Should not be IS NOT NULL: {result}");
        }

        [TestMethod]
        public void Translate_NotEqualNull_ProducesISNOTNULL()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Description != null);
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("IS NOT NULL"), $"Should use IS NOT NULL: {result}");
        }

        #endregion

        #region Issue #5: Contains/IN empty collection and performance

        [TestMethod]
        public void Translate_EmptyArrayContains_ProducesFalseCondition()
        {
            var emptyArr = new int[] { };
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => emptyArr.Contains(p.Type));
            var result = translator.Translate();

            // Empty IN should produce (1=0) instead of invalid SQL
            Assert.IsTrue(result.Contains("1=0"), $"Empty IN should produce 1=0: {result}");
        }

        [TestMethod]
        public void Translate_ArrayContains_ProducesIN()
        {
            var arr = new int[] { 1, 2, 3, 4 };
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => arr.Contains(p.Type));
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("Type IN ("), $"Should contain IN clause: {result}");
            Assert.AreEqual(4, translator.Parameters.Count, "Should have 4 parameters");
        }

        [TestMethod]
        public void Translate_NewArrayContains_ProducesIN()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => new int[] { 10, 20, 30 }.Contains(p.Type));
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("Type IN ("), $"Should contain IN clause: {result}");
            Assert.AreEqual(3, translator.Parameters.Count);
        }

        [TestMethod]
        public void Translate_LargeArrayContains_UsesInlineLiterals()
        {
            var largeArr = Enumerable.Range(0, 2200).ToArray();
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => largeArr.Contains(p.Type));
            var result = translator.Translate();

            // >2100 items should use inline literals, no parameters
            Assert.IsTrue(result.Contains("Type IN ("), $"Should contain IN clause: {result}");
            Assert.AreEqual(0, translator.Parameters.Count,
                "Large array should use inline literals, not parameters");
        }

        [TestMethod]
        public void Translate_EmptyListContains_ProducesFalseCondition()
        {
            var emptyList = new List<int>();
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => emptyList.Contains(p.Type));
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("1=0"), $"Empty list should produce 1=0: {result}");
        }

        #endregion

        #region Issue #6: switchRL and IsNullConstant after switch

        [TestMethod]
        public void Translate_ReversedComparison_CorrectOrder()
        {
            int val = 5;
            var translator = new QueryConditionTranslator<TestEntity>();
            // Value on left, field on right: val == p.Status
            translator.AddCondition(p => val == p.Status);
            var result = translator.Translate();

            // Should produce "Status = @c1", not "@c1 = Status"
            Assert.IsTrue(result.Contains("Status"), $"Should contain field name: {result}");
            Assert.IsTrue(translator.Parameters.Any(p => (int)p.Value == 5));
        }

        [TestMethod]
        public void Translate_ReversedNullComparison_ProducesISNULL()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            // null on the left side: null == p.Description
            translator.AddCondition(p => null == p.Description);
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("IS NULL"), $"Reversed null should produce IS NULL: {result}");
        }

        [TestMethod]
        public void Translate_MemberVariableOnLeft_Switched()
        {
            string searchName = "test";
            var translator = new QueryConditionTranslator<TestEntity>();
            // Variable on left, field on right
            translator.AddCondition(p => searchName == p.Name);
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("Name"), $"Should contain field Name: {result}");
            Assert.IsTrue(translator.Parameters.Any(p => (string)p.Value == "test"));
        }

        #endregion

        #region Issue #7: checkEncoding - Chinese and Unicode characters

        [TestMethod]
        public void Translate_ChineseStringValue_UsesDbTypeString()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Name == "測試中文");
            var result = translator.Translate();

            // Chinese text should produce DbType.String (NVarChar)
            var param = translator.Parameters.FirstOrDefault();
            Assert.IsNotNull(param);
            Assert.AreEqual(DbType.String, param.DbType,
                "Chinese characters should use DbType.String (NVarChar)");
        }

        [TestMethod]
        public void Translate_AsciiStringValue_UsesDbTypeAnsiString()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Name == "hello");
            var result = translator.Translate();

            var param = translator.Parameters.FirstOrDefault();
            Assert.IsNotNull(param);
            Assert.AreEqual(DbType.AnsiString, param.DbType,
                "ASCII characters should use DbType.AnsiString (VarChar)");
        }

        [TestMethod]
        public void Translate_JapaneseStringValue_UsesDbTypeString()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Name == "こんにちは");
            var result = translator.Translate();

            var param = translator.Parameters.FirstOrDefault();
            Assert.IsNotNull(param);
            Assert.AreEqual(DbType.String, param.DbType,
                "Japanese characters should use DbType.String (NVarChar)");
        }

        #endregion

        #region Issue #8: Translate reusability (Reset)

        [TestMethod]
        public void Translate_CalledTwice_ResultsNotAccumulated()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Status == 1);

            var result1 = translator.Translate();
            var result2 = translator.Translate();

            Assert.AreEqual(result1, result2,
                $"Second call should produce same result.\nFirst:  {result1}\nSecond: {result2}");
            Assert.AreEqual(translator.Parameters.Count,
                1, "Parameters should be reset between calls");
        }

        [TestMethod]
        public void Translate_CalledTwice_ParameterCountConsistent()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Status == 1);
            translator.AddCondition(p => p.Type == 2);

            translator.Translate();
            var paramCount1 = translator.Parameters.Count;

            translator.Translate();
            var paramCount2 = translator.Parameters.Count;

            Assert.AreEqual(paramCount1, paramCount2,
                "Parameter count should be consistent between Translate calls");
        }

        #endregion

        #region LIKE (Contains/StartsWith/EndsWith)

        [TestMethod]
        public void Translate_StringContains_ProducesLIKE()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Name.Contains("test"));
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("LIKE"), $"Should contain LIKE: {result}");
            var param = translator.Parameters.First();
            Assert.AreEqual("%test%", param.Value, "Contains should wrap with %");
        }

        [TestMethod]
        public void Translate_StartsWith_ProducesLIKE()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Name.StartsWith("abc"));
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("LIKE"), $"Should contain LIKE: {result}");
            var param = translator.Parameters.First();
            Assert.AreEqual("abc%", param.Value, "StartsWith should append %");
        }

        [TestMethod]
        public void Translate_EndsWith_ProducesLIKE()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Name.EndsWith("xyz"));
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("LIKE"), $"Should contain LIKE: {result}");
            var param = translator.Parameters.First();
            Assert.AreEqual("%xyz", param.Value, "EndsWith should prepend %");
        }

        #endregion

        #region ColumnMappingAttribute

        [TestMethod]
        public void Translate_ColumnMappingAttribute_UsesMapping()
        {
            var translator = new QueryConditionTranslator<TestEntityWithMapping>();
            translator.AddCondition(p => p.Name == "hello");
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("t.user_name"),
                $"Should use mapped column name t.user_name: {result}");
        }

        #endregion

        #region Complex conditions

        [TestMethod]
        public void Translate_OrCondition_ProducesOR()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Status > 0 || p.Description != null);
            var result = translator.Translate();

            Assert.IsTrue(result.Contains(" OR "), $"Should contain OR: {result}");
        }

        [TestMethod]
        public void Translate_AndOrCombined_CorrectStructure()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Status > 0 || p.Description != null);
            translator.AddCondition(p => p.Type == 1);
            var result = translator.Translate();

            // Two expressions joined by AND, first one has OR inside
            Assert.IsTrue(result.Contains(" OR "), $"Should contain OR: {result}");
            Assert.IsTrue(result.Contains(" AND "), $"Should contain AND: {result}");
        }

        [TestMethod]
        public void Translate_ComparisonOperators_AllWork()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Status >= 1);
            translator.AddCondition(p => p.Type <= 10);
            translator.AddCondition(p => p.Amount < 100m);
            var result = translator.Translate();

            Assert.IsTrue(result.Contains(" >= "), $"Should contain >=: {result}");
            Assert.IsTrue(result.Contains(" <= "), $"Should contain <=: {result}");
            Assert.IsTrue(result.Contains(" < "), $"Should contain <: {result}");
        }

        #endregion

        #region TextCondition with parameters

        [TestMethod]
        public void Translate_TextConditionWithParams_ParametersAdded()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddTextCondition("Status = @p1",
                new QueryParameter { Name = "@p1", Value = 1, DbType = DbType.Int32 });
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("(Status = @p1)"), $"Unexpected result: {result}");
            Assert.IsTrue(translator.Parameters.Any(p => p.Name == "@p1"));
        }

        #endregion

        #region QueryParameter.Size (參數長度)

        [TestMethod]
        public void QueryParameter_DefaultSize_IsZero()
        {
            var parameter = new QueryParameter { Name = "@p1", Value = "abc", DbType = DbType.AnsiString };

            Assert.AreEqual(0, parameter.Size, "未指定時 Size 應為 0（由 provider 自行推導）");
        }

        [TestMethod]
        public void Translate_TextConditionWithSize_SizePreserved()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddTextCondition("Name = @p1",
                new QueryParameter { Name = "@p1", Value = "abc", DbType = DbType.AnsiString, Size = 50 });
            translator.Translate();

            var param = translator.Parameters.Single(p => p.Name == "@p1");
            Assert.AreEqual(50, param.Size, "呼叫端指定的 Size 應原封不動保留");
            Assert.AreEqual("abc", param.Value);
            Assert.AreEqual(DbType.AnsiString, param.DbType);
        }

        [TestMethod]
        public void Translate_MultipleTextConditionParams_EachKeepsOwnSize()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddTextCondition("Name = @p1",
                new QueryParameter { Name = "@p1", Value = "abc", DbType = DbType.AnsiString, Size = 50 });
            translator.AddTextCondition("Description = @p2 AND Status = @p3",
                new QueryParameter { Name = "@p2", Value = "測試", DbType = DbType.String, Size = 100 },
                new QueryParameter { Name = "@p3", Value = 1, DbType = DbType.Int32 });
            translator.Translate();

            Assert.AreEqual(50, translator.Parameters.Single(p => p.Name == "@p1").Size);
            Assert.AreEqual(100, translator.Parameters.Single(p => p.Name == "@p2").Size);
            Assert.AreEqual(0, translator.Parameters.Single(p => p.Name == "@p3").Size,
                "未指定 Size 的參數應維持 0");
        }

        [TestMethod]
        public void Translate_CalledTwice_TextConditionSizeNotLostOrDuplicated()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Status == 1);
            translator.AddTextCondition("Name = @p1",
                new QueryParameter { Name = "@p1", Value = "abc", DbType = DbType.AnsiString, Size = 50 });

            translator.Translate();
            var firstCount = translator.Parameters.Count;
            Assert.AreEqual(50, translator.Parameters.Single(p => p.Name == "@p1").Size);

            translator.Translate();

            Assert.AreEqual(firstCount, translator.Parameters.Count,
                "重複 Translate() 不應重複累加 text condition 參數");
            Assert.AreEqual(1, translator.Parameters.Count(p => p.Name == "@p1"),
                "@p1 不應出現兩次");
            Assert.AreEqual(50, translator.Parameters.Single(p => p.Name == "@p1").Size,
                "重複 Translate() 後 Size 仍應保留");
        }

        [TestMethod]
        public void Translate_EqualCondition_AutoParameterSizeIsZero()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Name == "hello");
            translator.Translate();

            Assert.IsTrue(translator.Parameters.All(p => p.Size == 0),
                "翻譯器自動產生的參數 Size 應為 0（未指定）");
        }

        [TestMethod]
        public void Translate_LikeCondition_AutoParameterSizeIsZero()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Name.Contains("test"));
            translator.Translate();

            Assert.IsTrue(translator.Parameters.All(p => p.Size == 0),
                "LIKE 自動產生的參數 Size 應為 0（未指定）");
        }

        [TestMethod]
        public void Translate_InCondition_AutoParameterSizeIsZero()
        {
            var arr = new[] { "a", "bb", "ccc" };
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => arr.Contains(p.Name));
            translator.Translate();

            Assert.AreEqual(3, translator.Parameters.Count);
            Assert.IsTrue(translator.Parameters.All(p => p.Size == 0),
                "IN 自動產生的參數 Size 應為 0（未指定）");
        }

        [TestMethod]
        public void Translate_MixedAutoAndTextConditionParams_OnlyTextConditionHasSize()
        {
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Name == "hello");
            translator.AddTextCondition("Description = @p1",
                new QueryParameter { Name = "@p1", Value = "abc", DbType = DbType.AnsiString, Size = 200 });
            translator.Translate();

            Assert.AreEqual(200, translator.Parameters.Single(p => p.Name == "@p1").Size);
            Assert.IsTrue(translator.Parameters.Where(p => p.Name != "@p1").All(p => p.Size == 0),
                "自動產生的參數不應被指定 Size");
        }

        #endregion

        #region ColumnMappingAttribute.Length (欄位長度 → 參數 Size)

        [TestMethod]
        public void Translate_EqualWithLength_ParameterSizeFromAttribute()
        {
            var translator = new QueryConditionTranslator<LengthEntity>();
            translator.AddCondition(p => p.Name == "abc");
            var result = translator.Translate();

            var param = translator.Parameters.Single();
            Assert.AreEqual(50, param.Size, "= 條件應套用宣告的 Length");
            Assert.IsTrue(result.Contains("user_name"), $"應使用對應欄位名: {result}");
        }

        [TestMethod]
        public void Translate_DifferentColumns_EachUsesOwnLength()
        {
            var translator = new QueryConditionTranslator<LengthEntity>();
            translator.AddCondition(p => p.Name == "abc");
            translator.AddCondition(p => p.Code == "x1");
            translator.Translate();

            // 依產生順序：第一個參數對應 Name(50)、第二個對應 Code(10)
            Assert.AreEqual(50, translator.Parameters[0].Size);
            Assert.AreEqual(10, translator.Parameters[1].Size);
        }

        [TestMethod]
        public void Translate_ColumnMappingWithoutLength_SizeIsZero()
        {
            var translator = new QueryConditionTranslator<LengthEntity>();
            translator.AddCondition(p => p.Memo == "abc");
            translator.Translate();

            Assert.AreEqual(0, translator.Parameters.Single().Size,
                "有 ColumnMapping 但未宣告 Length 時 Size 應為 0");
        }

        [TestMethod]
        public void Translate_NoColumnMapping_SizeIsZero()
        {
            var translator = new QueryConditionTranslator<LengthEntity>();
            translator.AddCondition(p => p.Description == "abc");
            translator.Translate();

            Assert.AreEqual(0, translator.Parameters.Single().Size,
                "無 ColumnMapping 時 Size 應為 0");
        }

        [TestMethod]
        public void Translate_ComparisonOperatorWithLength_ParameterSizeFromAttribute()
        {
            var translator = new QueryConditionTranslator<LengthEntity>();
            translator.AddCondition(p => p.Code != "zz");
            translator.Translate();

            Assert.AreEqual(10, translator.Parameters.Single().Size,
                "<> 條件也應套用宣告的 Length");
        }

        [TestMethod]
        public void Translate_ReversedComparisonWithLength_ParameterSizeFromAttribute()
        {
            string search = "abc";
            var translator = new QueryConditionTranslator<LengthEntity>();
            // 變數在左、欄位在右 → 交換後仍應正確取到欄位長度
            translator.AddCondition(p => search == p.Name);
            translator.Translate();

            Assert.AreEqual(50, translator.Parameters.Single().Size,
                "反轉比較 (變數在左) 也應套用宣告的 Length");
        }

        [TestMethod]
        public void Translate_InClauseWithLength_AllParametersUseLength()
        {
            var arr = new[] { "a", "bb", "ccc" };
            var translator = new QueryConditionTranslator<LengthEntity>();
            translator.AddCondition(p => arr.Contains(p.Name));
            var result = translator.Translate();

            Assert.AreEqual(3, translator.Parameters.Count);
            Assert.IsTrue(translator.Parameters.All(p => p.Size == 50),
                "IN 的每個參數都應套用宣告的 Length");
            Assert.IsTrue(result.Contains("user_name IN ("), $"應對應欄位名: {result}");
        }

        [TestMethod]
        public void Translate_LikeContainsWithLength_SizeIsZero()
        {
            var translator = new QueryConditionTranslator<LengthEntity>();
            translator.AddCondition(p => p.Name.Contains("abc"));
            var result = translator.Translate();

            var param = translator.Parameters.Single();
            Assert.AreEqual(0, param.Size,
                "LIKE (Contains) 不套用 Length，避免萬用字元造成截斷");
            Assert.AreEqual("%abc%", param.Value);
            Assert.IsTrue(result.Contains("LIKE"), $"應為 LIKE: {result}");
        }

        [TestMethod]
        public void Translate_LikeStartsWithLength_SizeIsZero()
        {
            var translator = new QueryConditionTranslator<LengthEntity>();
            translator.AddCondition(p => p.Name.StartsWith("abc"));
            translator.Translate();

            Assert.AreEqual(0, translator.Parameters.Single().Size,
                "LIKE (StartsWith) 不套用 Length");
        }

        [TestMethod]
        public void Translate_MixedLengthColumns_EachParameterCorrect()
        {
            var translator = new QueryConditionTranslator<LengthEntity>();
            translator.AddCondition(p => p.Name == "abc");        // Length 50
            translator.AddCondition(p => p.Description == "xyz");  // 無 mapping → 0
            translator.AddCondition(p => p.Code == "c1");          // Length 10
            translator.Translate();

            Assert.AreEqual(50, translator.Parameters[0].Size);
            Assert.AreEqual(0, translator.Parameters[1].Size);
            Assert.AreEqual(10, translator.Parameters[2].Size);
        }

        [TestMethod]
        public void Translate_CalledTwice_LengthSizeConsistent()
        {
            var translator = new QueryConditionTranslator<LengthEntity>();
            translator.AddCondition(p => p.Name == "abc");

            translator.Translate();
            var first = translator.Parameters.Single().Size;
            translator.Translate();
            var second = translator.Parameters.Single().Size;

            Assert.AreEqual(50, first);
            Assert.AreEqual(first, second, "重複 Translate() 後 Size 應一致");
        }

        #endregion

        #region 完整 SQL 字串精確比對 (lambda → 完整 WHERE 子句)

        [TestMethod]
        public void FullSql_SingleComparison_ExactMatch()
        {
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => p.Status > 0);

            Assert.AreEqual("(Status > @c1)", t.Translate());
            Assert.AreEqual("@c1", t.Parameters.Single().Name);
            Assert.AreEqual(0, t.Parameters.Single().Value);
        }

        [TestMethod]
        public void FullSql_TwoConditionsAnded_ExactMatchAndParamOrder()
        {
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => p.Status > 0);
            t.AddCondition(p => p.Type == 1);

            Assert.AreEqual("(Status > @c1) AND (Type = @c2)", t.Translate());
            // 參數命名依產生順序遞增
            Assert.AreEqual("@c1", t.Parameters[0].Name);
            Assert.AreEqual("@c2", t.Parameters[1].Name);
        }

        [TestMethod]
        public void FullSql_EqualNull_ExactMatch()
        {
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => p.Description == null);

            Assert.AreEqual("(Description IS NULL)", t.Translate());
            Assert.AreEqual(0, t.Parameters.Count, "IS NULL 不應產生參數");
        }

        [TestMethod]
        public void FullSql_NotEqualNull_ExactMatch()
        {
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => p.Description != null);

            Assert.AreEqual("(Description IS NOT NULL)", t.Translate());
            Assert.AreEqual(0, t.Parameters.Count);
        }

        [TestMethod]
        public void FullSql_OrInsideSingleCondition_ExactMatch()
        {
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => p.Status > 0 || p.Description != null);

            Assert.AreEqual("(Status > @c1 OR Description IS NOT NULL)", t.Translate());
        }

        [TestMethod]
        public void FullSql_InClauseFromArrayLiteral_ExactMatch()
        {
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => new int[] { 1, 2, 3 }.Contains(p.Type));

            Assert.AreEqual("((Type IN (@c1,@c2,@c3)))", t.Translate());
            Assert.AreEqual(3, t.Parameters.Count);
            Assert.AreEqual("@c1", t.Parameters[0].Name);
            Assert.AreEqual("@c3", t.Parameters[2].Name);
        }

        [TestMethod]
        public void FullSql_InClauseFromVariable_ExactMatch()
        {
            var arr = new[] { 10, 20 };
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => arr.Contains(p.Type));

            Assert.AreEqual("((Type IN (@c1,@c2)))", t.Translate());
        }

        [TestMethod]
        public void FullSql_EmptyIn_ExactMatch()
        {
            var empty = new int[] { };
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => empty.Contains(p.Type));

            Assert.AreEqual("((1=0))", t.Translate());
            Assert.AreEqual(0, t.Parameters.Count);
        }

        [TestMethod]
        public void FullSql_LikeContains_ExactMatch()
        {
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => p.Name.Contains("test"));

            Assert.AreEqual("((Name LIKE @c1))", t.Translate());
            Assert.AreEqual("%test%", t.Parameters.Single().Value);
        }

        [TestMethod]
        public void FullSql_LikeStartsWith_ExactMatch()
        {
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => p.Name.StartsWith("abc"));

            Assert.AreEqual("((Name LIKE @c1))", t.Translate());
            Assert.AreEqual("abc%", t.Parameters.Single().Value);
        }

        [TestMethod]
        public void FullSql_ColumnMappingAlias_ExactMatch()
        {
            var t = new QueryConditionTranslator<TestEntityWithMapping>();
            t.AddCondition(p => p.Name == "hello");

            Assert.AreEqual("(t.user_name = @c1)", t.Translate());
        }

        [TestMethod]
        public void FullSql_ExpressionPlusTextCondition_ExactMatch()
        {
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => p.Status > 0);
            t.AddTextCondition("Type = 1");

            Assert.AreEqual("(Status > @c1) AND (Type = 1)", t.Translate());
        }

        [TestMethod]
        public void FullSql_ReversedComparison_ExactMatch()
        {
            int v = 5;
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => v == p.Status);

            // 變數在左、欄位在右 → 交換後欄位在左
            Assert.AreEqual("(Status = @c1)", t.Translate());
            Assert.AreEqual(5, t.Parameters.Single().Value);
        }

        [TestMethod]
        public void FullSql_MultipleComparisonOperators_ExactMatch()
        {
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => p.Status >= 1);
            t.AddCondition(p => p.Amount < 100m);

            Assert.AreEqual("(Status >= @c1) AND (Amount < @c2)", t.Translate());
        }

        [TestMethod]
        public void FullSql_CalledTwice_IdenticalOutput()
        {
            var t = new QueryConditionTranslator<TestEntity>();
            t.AddCondition(p => p.Status > 0);
            t.AddCondition(p => p.Type == 1);

            var first = t.Translate();
            var second = t.Translate();

            Assert.AreEqual("(Status > @c1) AND (Type = @c2)", first);
            Assert.AreEqual(first, second, "重複呼叫應產生完全相同的 SQL（含參數命名）");
        }

        #endregion

        #region Enum contains

        [TestMethod]
        public void Translate_EnumArrayContains_ProducesIN()
        {
            var cats = new[] { TestCategories.C1, TestCategories.C2, TestCategories.C3 };
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => cats.Contains(p.Categories));
            var result = translator.Translate();

            Assert.IsTrue(result.Contains("Categories IN ("), $"Should contain IN: {result}");
            Assert.AreEqual(3, translator.Parameters.Count);
        }

        #endregion

        #region Integration test (original test from root UnitTest1.cs)

        [TestMethod]
        public void Translate_OriginalIntegrationTest_Succeeds()
        {
            var arr = Enumerable.Range(0, 2100).Select(x => (TestCategories)x);
            var translator = new QueryConditionTranslator<TestEntity>();
            translator.AddCondition(p => p.Status > 0 || p.Description != null);
            translator.AddCondition(p => new int[] { 1, 2, 3, 4 }.Contains(p.Type));
            translator.AddCondition(p => arr.Contains(p.Categories));

            var condition = translator.Translate();

            Assert.IsNotNull(condition);
            Assert.IsTrue(condition.Length > 0);
            Assert.IsTrue(condition.Contains(" AND "));
            Assert.IsTrue(condition.Contains(" OR "));
            Assert.IsTrue(condition.Contains("Type IN ("));
            Assert.IsTrue(condition.Contains("Categories IN ("));
        }

        #endregion
    }

    [TestClass]
    public class QuerySelectBuilderTests
    {
        #region Basic Column selection

        [TestMethod]
        public void Build_NoColumns_ReturnsStar()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            var result = builder.Build();
            Assert.AreEqual("*", result);
        }

        [TestMethod]
        public void Build_SingleColumn_ReturnsPropertyName()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder.Column(p => p.Name);
            var result = builder.Build();
            Assert.AreEqual("Name", result);
        }

        [TestMethod]
        public void Build_MultipleColumns_CommaSeparated()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder.Column(p => p.ID).Column(p => p.Name).Column(p => p.Amount);
            var result = builder.Build();
            Assert.AreEqual("ID, Name, Amount", result);
        }

        [TestMethod]
        public void Build_ColumnWithAlias_ProducesAS()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder.Column(p => p.Name, alias: "DisplayName");
            var result = builder.Build();
            Assert.AreEqual("Name AS DisplayName", result);
        }

        [TestMethod]
        public void Build_ColumnWithTableAlias_ProducesDotNotation()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder.Column(p => p.Name, tableAlias: "u");
            var result = builder.Build();
            Assert.AreEqual("u.Name", result);
        }

        [TestMethod]
        public void Build_ColumnWithTableAliasAndAlias_ProducesBoth()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder.Column(p => p.ID, tableAlias: "u", alias: "UserID");
            var result = builder.Build();
            Assert.AreEqual("u.ID AS UserID", result);
        }

        #endregion

        #region ColumnMappingAttribute

        [TestMethod]
        public void Build_WithColumnMappingAttribute_UsesColumnName()
        {
            var builder = new QuerySelectBuilder<SelectEntityWithAttr>();
            builder.Column(p => p.ID);
            var result = builder.Build();
            // ID has ColumnMapping(ColumnName="user_id") with no AliasName
            Assert.AreEqual("user_id", result);
        }

        [TestMethod]
        public void Build_WithColumnMappingAttribute_UsesAliasName()
        {
            var builder = new QuerySelectBuilder<SelectEntityWithAttr>();
            builder.Column(p => p.Name);
            var result = builder.Build();
            // Name has ColumnMapping(ColumnName="user_name", AliasName="u")
            Assert.AreEqual("u.user_name", result);
        }

        [TestMethod]
        public void Build_WithColumnMappingAttribute_TableAliasOverrides()
        {
            var builder = new QuerySelectBuilder<SelectEntityWithAttr>();
            builder.Column(p => p.Name, tableAlias: "x");
            var result = builder.Build();
            // Override AliasName "u" with "x"
            Assert.AreEqual("x.user_name", result);
        }

        [TestMethod]
        public void Build_WithColumnMappingAttribute_NoMappingProperty()
        {
            var builder = new QuerySelectBuilder<SelectEntityWithAttr>();
            builder.Column(p => p.Status);
            var result = builder.Build();
            // Status has no ColumnMappingAttribute, use property name
            Assert.AreEqual("Status", result);
        }

        #endregion

        #region IConditionMappingAttribute

        [TestMethod]
        public void Build_WithConditionMapping_UsesMapping()
        {
            var builder = new QuerySelectBuilder<JoinSearchEntity>();
            builder.Column(p => p.UserName);
            var result = builder.Build();
            // JoinMappingAttribute maps UserName => "u.Name"
            Assert.AreEqual("u.Name", result);
        }

        [TestMethod]
        public void Build_WithConditionMapping_TableAliasOverrides()
        {
            var builder = new QuerySelectBuilder<JoinSearchEntity>();
            builder.Column(p => p.OrderAmount, tableAlias: "o2");
            var result = builder.Build();
            // JoinMappingAttribute maps OrderAmount => "o.Amount", override table alias to "o2"
            Assert.AreEqual("o2.Amount", result);
        }

        #endregion

        #region From (group select)

        [TestMethod]
        public void Build_FromGroup_AllColumnsWithSameAlias()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder.From("u", p => p.ID, p => p.Name);
            var result = builder.Build();
            Assert.AreEqual("u.ID, u.Name", result);
        }

        [TestMethod]
        public void Build_FromMultipleGroups_DifferentAliases()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder
                .From("u", p => p.ID, p => p.Name)
                .From("o", p => p.Amount, p => p.CreatedOn);
            var result = builder.Build();
            Assert.AreEqual("u.ID, u.Name, o.Amount, o.CreatedOn", result);
        }

        [TestMethod]
        public void Build_FromWithColumnMappingAttribute_OverridesTableAlias()
        {
            var builder = new QuerySelectBuilder<SelectEntityWithAttr>();
            builder.From("x", p => p.ID, p => p.Name);
            var result = builder.Build();
            // ID: ColumnMapping(ColumnName="user_id") → x.user_id
            // Name: ColumnMapping(ColumnName="user_name", AliasName="u") → override to x.user_name
            Assert.AreEqual("x.user_id, x.user_name", result);
        }

        #endregion

        #region RawColumn

        [TestMethod]
        public void Build_RawColumn_PassedThrough()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder.RawColumn("COUNT(*) AS Total");
            var result = builder.Build();
            Assert.AreEqual("COUNT(*) AS Total", result);
        }

        [TestMethod]
        public void Build_MixedColumnsAndRaw_CombinedCorrectly()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder
                .Column(p => p.ID, tableAlias: "u")
                .RawColumn("COUNT(*) AS Total");
            var result = builder.Build();
            Assert.AreEqual("u.ID, COUNT(*) AS Total", result);
        }

        #endregion

        #region ColumnAll

        [TestMethod]
        public void Build_ColumnAll_ReturnsStar()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder.ColumnAll();
            var result = builder.Build();
            Assert.AreEqual("*", result);
        }

        [TestMethod]
        public void Build_ColumnAllWithAlias_ReturnsAliasedStar()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder.ColumnAll("u");
            var result = builder.Build();
            Assert.AreEqual("u.*", result);
        }

        [TestMethod]
        public void Build_ColumnAllWithOtherColumns_Combined()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder
                .ColumnAll("u")
                .Column(p => p.Amount, tableAlias: "o");
            var result = builder.Build();
            Assert.AreEqual("u.*, o.Amount", result);
        }

        #endregion

        #region Reset

        [TestMethod]
        public void Build_AfterReset_ReturnsNewSelection()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder.Column(p => p.ID).Column(p => p.Name);
            Assert.AreEqual("ID, Name", builder.Build());

            builder.Reset().Column(p => p.Amount);
            Assert.AreEqual("Amount", builder.Build());
        }

        #endregion

        #region Mixed usage (Column + From + Raw + ColumnAll)

        [TestMethod]
        public void Build_MixedAllApis_CombinedCorrectly()
        {
            var builder = new QuerySelectBuilder<SelectEntity>();
            builder
                .Column(p => p.ID, tableAlias: "u", alias: "UserID")
                .From("o", p => p.Amount, p => p.CreatedOn)
                .RawColumn("DATEDIFF(DAY, o.CreatedOn, GETDATE()) AS DaysSince");
            var result = builder.Build();
            Assert.AreEqual(
                "u.ID AS UserID, o.Amount, o.CreatedOn, DATEDIFF(DAY, o.CreatedOn, GETDATE()) AS DaysSince",
                result);
        }

        #endregion

        #region Integration: QuerySelectBuilder + QueryConditionTranslator

        [TestMethod]
        public void Integration_SelectAndWhere_ComposeSql()
        {
            var select = new QuerySelectBuilder<JoinSearchEntity>()
                .Column(p => p.UserName)
                .Column(p => p.OrderAmount);

            var where = new QueryConditionTranslator<JoinSearchEntity>();
            where.AddCondition(p => p.Status > 0);

            var selectSql = select.Build();
            var whereSql = where.Translate();

            var sql = $"SELECT {selectSql} FROM Users u JOIN Orders o ON u.ID = o.UserID WHERE {whereSql}";

            Assert.IsTrue(sql.Contains("SELECT u.Name, o.Amount"), $"SELECT part wrong: {sql}");
            Assert.IsTrue(sql.Contains("WHERE"), $"Missing WHERE: {sql}");
            Assert.IsTrue(where.Parameters.Count > 0, "Should have parameters");
        }

        [TestMethod]
        public void Integration_SelectWithFromAndCondition()
        {
            var select = new QuerySelectBuilder<SelectEntity>()
                .From("u", p => p.ID, p => p.Name)
                .From("o", p => p.Amount)
                .RawColumn("COUNT(*) AS OrderCount");

            var where = new QueryConditionTranslator<SelectEntity>();
            where.AddCondition(p => p.Status == 1);
            where.AddCondition(p => p.Name.Contains("test"));

            var sql = $"SELECT {select.Build()} FROM Users u JOIN Orders o ON u.ID = o.UserID WHERE {where.Translate()}";

            Assert.IsTrue(sql.StartsWith("SELECT u.ID"), $"Wrong start: {sql}");
            Assert.IsTrue(sql.Contains("COUNT(*) AS OrderCount"), $"Missing raw: {sql}");
            Assert.IsTrue(sql.Contains("LIKE"), $"Missing LIKE: {sql}");
        }

        #endregion
    }
}
