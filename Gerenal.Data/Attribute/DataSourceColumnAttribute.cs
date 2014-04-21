using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public  sealed class DataSourceColumnAttribute : Attribute
    {
        public DataSourceColumnAttribute()
        {

        }
        // This is a positional argument
        public DataSourceColumnAttribute(string fileName)
        {
            this.FieldName = fileName;
        }
        public DataSourceColumnAttribute(bool isCustomField)
        {
            this.IsCustomField = isCustomField;
        }
        #region Property
        public string FieldName { get; set; }
        public bool IsCustomField { get; set; }
        public bool IsAutoKey { get; set; }
        #endregion

    }
}
