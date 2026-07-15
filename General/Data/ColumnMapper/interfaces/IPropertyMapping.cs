using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data.ColumnMapper
{
    public interface IPropertyMapping<T>
    {
        IMappedType<T> MappedType { get; set; }

        string PropertyName { get; set; }
    }
}
