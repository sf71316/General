using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public  sealed class ColumnAttribute : Attribute
    {
        public ColumnAttribute()
        {

        }

        #region Property
        public string Name { get; set; }
        public bool Ignore { get; set; }
        public bool Key { get; set; }
        public bool AutoKey { get; set; }
        public bool IsNull { get; set; }
        #endregion

    }
}
