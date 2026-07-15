using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace General.Data.ColumnMapper
{
    public sealed class MappedType<T> : IMappedType<T>
    {



        Type IMappedType.MappedType { get { return typeof(T); } }

        public IDictionary<string, string> MappedColumns { get; } = new Dictionary<string, string>();

        public IDictionary<string, DbType> MappedDbTypes { get; } = new Dictionary<string, DbType>();

        void IMappedType<T>.DefineColumnMapping(IColumnMapping<T> columnMapping)
        {
            if (MappedColumns.ContainsKey(columnMapping.PropertyName))
                MappedColumns[columnMapping.PropertyName] = columnMapping.ColumnName;
            else
                MappedColumns.Add(columnMapping.PropertyName, columnMapping.ColumnName);
        }
        void IMappedType<T>.DefineDbMapping(IColumnMapping<T> columnMapping)
        {
            if (MappedDbTypes.ContainsKey(columnMapping.PropertyName))
                MappedDbTypes[columnMapping.PropertyName] = columnMapping.DbType;
            else
                MappedDbTypes.Add(columnMapping.PropertyName, columnMapping.DbType);
        }
        bool IMappedType.DbTypesHasBeenMapped(string PropertyName)
        {
            return MappedDbTypes.ContainsKey(PropertyName);
        }
        bool IMappedType.ColumnHasBeenMapped(string PropertyName)
        {
            return MappedColumns.ContainsKey(PropertyName);
        }

    }
}
