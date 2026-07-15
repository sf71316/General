using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data.ColumnMapper
{
    public sealed class PropertyMapping<T> : IPropertyMapping<T>
    {
        IMappedType<T> IPropertyMapping<T>.MappedType { get; set; }

        string IPropertyMapping<T>.PropertyName { get; set; }
    }
}
