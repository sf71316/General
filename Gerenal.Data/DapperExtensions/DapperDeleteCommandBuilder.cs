using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace General.Data.Dapper
{
    internal class DapperDeleteCommandBuilder : DapperCommandBuilder, IDeleteCommand
    {
        public DapperDeleteCommandBuilder(string tablename, IDapperProvider dapper)
            : base(tablename, dapper)
        {

        }
        public int Delete(Expression expr)
        {
            DynamicParameters paramer = new DynamicParameters();
            DbTypeConverter tconvert = new DbTypeConverter();
            StringBuilder sql = new StringBuilder();
            if (string.IsNullOrEmpty(TableName))
            {
                this.OnException("Not set table name");
                return 0;
            }
            if (expr == null)
            {
                this.OnException("Not set condition");
                return 0;
            }
            this.Translator.Clear();
            this.Translator.Translate(expr);
            sql.AppendFormat("DELETE FROM {0} {1}", this.TableName, this.Translator.ToWhere());
            foreach (var item in this.Translator.Parameters)
            {
                paramer.Add(item.Key, item.Value, tconvert.Get(item.Value.GetType()), null, null);
            }
            return this.Dapper.Execute(sql.ToString(), paramer);
        }

        public int Delete<T>(Expression<Func<T, bool>> expr)
        {
            return this.Delete<T>(expr);
        }
    }
}
