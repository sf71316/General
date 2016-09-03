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
        Expression _selector;
        string _orderby;
        StringBuilder sql;
        public DapperSelectCommandBuilder(IDapperProvider dapper)
            : base(dapper)
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
                this.Translator.Clear();
                this.Translator.Translate(this._expr);
                this.sql.AppendFormat("SELECT {0} FROM {1} {2}",
                    this._fieldnames,
                    this._tablenames,
                    this.Translator.ToWhere());
                if (!string.IsNullOrEmpty(this._orderby))
                {
                    this.sql.AppendFormat(" ORDER BY {0}", this._orderby);
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
            this._orderby = this._tablenames = this._fieldnames = string.Empty;
            this.sql = new StringBuilder();
        }

        private void where(Expression expr)
        {
            this._selector = expr;
        }
        public ISelectQuery<T> OrderBy(string fieldname)
        {
            this._orderby = fieldname;
            return this;
        }

        public ISelectQuery<T> Where(Expression expr)
        {
            this.where(expr);
            return this;
        }

        public ISelectQuery<T> Where(Expression<Func<T, bool>> expr)
        {
            return this.Where(expr);
        }

        public ISelectCommand<T> Select(Expression<Func<T, object>> selector)
        {
            this._selector = selector;
            return this;
        }

        public ISelectCommand<T> From(string tablename)
        {
            this._tablenames = tablename;
            return this;
        }
    }
}
