using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace General.Data.Dapper
{
    internal class DapperSelectCommandBuilder<T> : DapperCommandBuilder, ISelectCommand<T>
    {
        Expression _expr;
        string _fieldnames;
        string _tablenames;
        string _orderby;
        StringBuilder sql;
        public DapperSelectCommandBuilder( IDapperProvider dapper)
            : base( dapper)
        {
            this.sql = new StringBuilder();
        }
      
        public IEnumerable<T> Query()
        {
            DynamicParameters paramer = new DynamicParameters();
            DbTypeConverter tconvert = new DbTypeConverter();


            if (string.IsNullOrEmpty(this._tablenames))
            {
                this.OnException("Not set table name");
            }
            else
            {
                this.Translator.Translate(this._expr);
                this.sql.AppendFormat("SELECT {0} FROM {1} {2}",
                    this._fieldnames,
                    this._tablenames,
                    this.Translator.ToWhere());
                if (!string.IsNullOrEmpty(this._orderby))
                {
                    this.sql.AppendFormat(" ORDER BY {0}",this._orderby);
                }
                foreach (var item in this.Translator.Parameters)
                {
                    paramer.Add(item.Key, item.Value, tconvert.Get(item.Value.GetType()), null, null);
                }
                string sql = this.sql.ToString();
                this.Clear();
                return this.Dapper.Query<T>(sql, paramer);
            }
            this.Clear();
            return null;
        }
        private void Clear()
        {
            this._expr = null;
            this._orderby=this._tablenames=this._fieldnames = string.Empty;
            this.sql = new StringBuilder();
        }
        public ISelectQuery<T> OrderBy(string fieldname)
        {
            this._orderby = fieldname;
            return this;
        }

        public ISelectQuery<T> Where(Expression expr)
        {
            throw new NotImplementedException();
        }

        public ISelectQuery<T> Where(Expression<Func<T, bool>> expr)
        {
            throw new NotImplementedException();
        }

        public ISelectQuery<T> Where<T2>(Expression<Func<T, T2, bool>> expr)
        {
            throw new NotImplementedException();
        }

        public ISelectQuery<T> Where<T2, T3>(Expression<Func<T, T2, T3, bool>> expr)
        {
            throw new NotImplementedException();
        }

        public ISelectQuery<T> Where<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expr)
        {
            throw new NotImplementedException();
        }

        public ISelectCommand<T> Select(Expression<Func<T, object>> selector)
        {
            throw new NotImplementedException();
        }

        public ISelectCommand<T> From(string tablename)
        {
            throw new NotImplementedException();
        }
    }
}
