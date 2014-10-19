using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace General.Data
{

    internal class DapperProvider : IDapperProvider 
    {
        IDbConnection cnn;
        public DapperProvider(IDbConnection connection)
        {
            cnn = connection;
        }

        public int Execute(string sql, object parameters)
        {
            return cnn.Execute(sql, parameters, null, null, null);
        }
        public int Execute(string sql, object parameters, CommandType type)
        {
            return this.cnn.Execute(sql, parameters, null, null, type);
        }
        public int Execute(string sql, object parameters, IDbTransaction transaction, int? commandTimeout, CommandType? commandType)
        {
            return this.cnn.Execute(sql, parameters, transaction, commandTimeout, commandType);
        }
        public IEnumerable<IDictionary<string, object>> Query(string sql, object parameters)
        {
            return this.cnn.Query(sql, parameters);
        }
        public IEnumerable<T> Query<T>(string sql, object parameters)
        {
            return cnn.Query<T>(sql, parameters, null, true, null, null);
        }
        public IEnumerable<T> Query<T>(string sql, object parameters, IDbTransaction transaction)
        {
            return cnn.Query<T>(sql, parameters, transaction, true, null, null);
        }

        public IEnumerable<T> Query<T>(string sql, object parameters, CommandType commandType)
        {
            return cnn.Query<T>(sql, parameters, null, true, null, commandType);
        }

        public IEnumerable<T> Query<T>(string sql, object parameters, IDbTransaction transaction, CommandType commandType)
        {
            return cnn.Query<T>(sql, parameters, transaction, true, null, commandType);
        }
        public IDbConnection Connection
        {
            get
            {
                return cnn;
            }
            set
            {
                cnn = value;
            }
        }
    }
}
