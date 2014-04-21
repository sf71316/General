using System;
using System.Data;
using System.Configuration;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
namespace General.Data
{
    public interface IDACAdapter :  IDisposable
    {
        void AddParameter(string name, object value);
        void AddParameter(string name, object value,DbType? type);
        void ClearParameter();
        void CreateCommand();
        string CommandText { get; set; }
        DataRow DataRow();
        DataSet DataSet();
        DataTable DataTable();
        DbDataReader Reader();
        T GetEntity<T>() where T : IEntity;
        IEnumerable<TEntity> GetEntities<TEntity>()
            where TEntity : IEntity;
        object ExecuteNonQuery();
        bool Execute();
        object Value { get; }
        DbCommand Command { get; }
        DbDataAdapter Adapter { get; }
        DbConnection Connection { get; }
    }
}
