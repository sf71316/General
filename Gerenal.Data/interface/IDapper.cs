using System;
using System.Collections.Generic;
using System.Data;
namespace General.Data
{
   public interface IDapperProvider
    {
       int Execute(string sql, object parameters);
       int Execute(string sql, object parameters, CommandType type);
       int Execute(string sql, object param, IDbTransaction transaction, int? commandTimeout, CommandType? commandType);
       IEnumerable<T> Query<T>(string sql, object parameters);
       IEnumerable<T> Query<T>(string sql, object param, IDbTransaction transaction);
       IEnumerable<T> Query<T>(string sql, object param, CommandType commandType);
       IEnumerable<T> Query<T>(string sql, object param, IDbTransaction transaction, CommandType commandType);
       IEnumerable<IDictionary<string, object>> Query(string sql, object parameters);
    }
}
