using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace General.Data.Dapper
{
    internal class DapperSelectCommandBuilder : DapperCommandBuilder, ISelectCommand
    {
        Expression _expr;
        string _fieldnames;
        string _tablenames;
        string _orderby;
        StringBuilder sql;
        public DapperSelectCommandBuilder(string tablename, IDapperProvider dapper)
            : base(tablename, dapper)
        {
            this.sql = new StringBuilder();
        }
        public ISelectQuery Where(Expression expr)
        {
            this._expr = expr;
            return this;
        }

        public ISelectQuery Where<T1>(Expression<Func<T1, bool>> expr)
        {
            return this.Where(expr);
        }

        public ISelectQuery Where<T1, T2>(Expression<Func<T1, T2, bool>> expr)
        {
            return this.Where(expr);
        }

        public ISelectQuery Where<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> expr)
        {
            return this.Where(expr);
        }

        public ISelectQuery Where<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> expr)
        {
            return this.Where(expr);
        }

        public ISelectCommand Select(string field = "*")
        {
            this._fieldnames = field;
            return this;
        }

        public ISelectCommand From(string tablename)
        {
            this._tablenames = tablename;
            return this;
        }

        public IEnumerable<T> Query<T>()
        {
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
                return this.Dapper.Query<T>(this.sql.ToString(), null);
            }
            return null;
        }

        public ISelectQuery OrderBy(string fieldname)
        {
            this._orderby = fieldname;
            return this;
        }
    }
}
