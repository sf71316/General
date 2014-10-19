using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data.Dapper
{
    internal abstract class DapperCommandBuilder : ICommandBuilder
    {
        public DapperCommandBuilder()
        {
            this.Translator = new QueryTranslator();
            
        }
        public DapperCommandBuilder(string tablename,IDapperProvider dapper):this()
        {
            this.TableName = tablename;
            this.Dapper = dapper;
        }
        public IQueryTranslator Translator { get; set; }
        protected IDapperProvider Dapper { get;  set; }
        public string TableName { get; set; }
        public bool UseTableAlias
        {
            get {
                return
                    this.Translator.UseTableAlias;
            }
            set
            {
                this.Translator.UseTableAlias = value;
            }
        }
        public event EventHandler<ErrorArg> Exception;

        protected void OnException(string message)
        {
            if (this.Exception != null)
            {
                this.Exception(this, new ErrorArg { 
                    Message = message 
                });
            }
        }
        protected void OnException(string message, Exception e)
        {
            this.Exception(this, new ErrorArg
            {
                Message = message,
                exception=e
            });
        }

        public static ISelectCommand GetSelectCommandBuilder(string tablename,IDapperProvider provider)
        {
            return new DapperSelectCommandBuilder(tablename, provider);
        }
        public static IInsertCommand GetInsertCommandBuilder(string tablename, IDapperProvider provider)
        {
            return new DapperInsertCommandBuilder(tablename, provider);
        }
        public static IUpdateCommand GetUpdateCommandBuilder(string tablename, IDapperProvider provider)
        {
            return new DapperUpdateCommandBuilder(tablename, provider);
        }
        public static IDeleteCommand GetDeleteCommandBuilder(string tablename, IDapperProvider provider)
        {
            return new DapperDeleteCommandBuilder(tablename, provider);
        }
    }
    internal class ErrorArg:EventArgs
    {
        public Exception exception { get; set; }

        public string Message { get; set; }
    }
}
