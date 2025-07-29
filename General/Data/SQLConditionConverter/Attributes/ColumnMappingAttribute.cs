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

    }
}
