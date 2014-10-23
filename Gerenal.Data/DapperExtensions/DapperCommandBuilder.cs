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
        public DapperCommandBuilder(IDapperProvider dapper)
            : this()
        {
            this.Dapper = dapper;
        }
        public IQueryTranslator Translator { get; set; }
        protected IDapperProvider Dapper { get; set; }
        public string TableName { get; set; }
        public bool UseTableAlias
        {
            get
            {
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
                this.Exception(this, new ErrorArg
                {
                    Message = message
                });
            }
        }
        protected void OnException(string message, Exception e)
        {
            this.Exception(this, new ErrorArg
            {
                Message = message,
                exception = e
            });
        }

        public static ISelectCommand<T> GetSelectCommandBuilder<T>(IDapperProvider provider)
        {
            return new DapperSelectCommandBuilder<T>(provider);
        }
        public static IInsertCommand GetInsertCommandBuilder(IDapperProvider provider)
        {
            return new DapperInsertCommandBuilder(provider);
        }
        public static IUpdateCommand GetUpdateCommandBuilder(IDapperProvider provider)
        {
            return new DapperUpdateCommandBuilder(provider);
        }
        public static IDeleteCommand GetDeleteCommandBuilder(IDapperProvider provider)
        {
            return new DapperDeleteCommandBuilder(provider);
        }
    }
    internal class ErrorArg : EventArgs
    {
        public Exception exception { get; set; }

        public string Message { get; set; }
    }
}
