using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace General.Data.ColumnMapper
{
    public interface IColumnMapping<T> : IPropertyMapping<T>
    {
        string ColumnName { get; set; }
        DbType DbType { get; set; }
    }

}
