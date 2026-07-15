using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace General.Data.ColumnMapper
{
    public interface IMappedType
    {
        Type MappedType { get; }

        bool ColumnHasBeenMapped(string columnName);
        bool DbTypesHasBeenMapped(string columnName);

        IDictionary<string, string> MappedColumns { get; }
        IDictionary<string, DbType> MappedDbTypes { get; }
    }
    public interface IMappedType<T> : IMappedType
    {
        void DefineColumnMapping(IColumnMapping<T> columnMapping);
        void DefineDbMapping(IColumnMapping<T> columnMapping);
    }
}
