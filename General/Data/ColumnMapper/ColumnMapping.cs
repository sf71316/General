using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace General.Data.ColumnMapper
{
    public sealed class ColumnMapping<T> : IColumnMapping<T>
    {
        public DbType DbType { get; set; }
        string IColumnMapping<T>.ColumnName { get; set; }

        IMappedType<T> IPropertyMapping<T>.MappedType { get; set; }

        string IPropertyMapping<T>.PropertyName { get; set; }
    }
}
