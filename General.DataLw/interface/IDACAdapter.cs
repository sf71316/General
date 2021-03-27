using System;
using System.Data;
using System.Configuration;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
namespace General.Data
{
    public interface IDACAdapter : IDbInfo, IDisposable
    {
        void AddParameter(string name, object value);
        void AddParameter(string name, object value, DbType? type);
        void ClearParameter();
        DbCommand CreateCommand();
        void InitCommand();
        string CommandText { get; set; }
        DataRow GetDataRow();
        DataSet GetDataSet();
        DataTable GetDataTable();
        DbDataReader GetReader();
        object ExecuteNonQuery();
        bool Execute();
        object GetValue();

    }
}
