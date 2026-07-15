using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data.SQLConditionConverter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnMappingAttribute : Attribute
    {
        public string AliasName { get; set; }
        public string ColumnName { get; set; }
        /// <summary>
        /// 欄位長度。0 表示未宣告；用於填入 QueryParameter.Size 以利執行計畫重用。
        /// LIKE (Contains/StartsWith/EndsWith) 不套用此長度，避免萬用字元造成截斷。
        /// </summary>
        public int Length { get; set; }

    }
}
