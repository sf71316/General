using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;

namespace General.Data
{
    public abstract class DataBase : IDisposable
    {
        private bool disposed = false;
        protected IDACAdapter Provider { get; private set; }

        public DataBase()
        {
        }

        public DataBase(string ConfigKey)
        {
            Provider = new DefaultProvider(ConfigKey);
        }
        public DataBase(IDbInfo dbinfo)
        {
            this.Provider.Connection = dbinfo.Connection;
            this.Provider.Adapter = dbinfo.Adapter;

        }

        #region Data Base Method

        protected DbCommand CreateCommand()
        {
            return this.Provider.CreateCommand();
        }

        protected void AddParameter(string name, object value)
        {
            this.Provider.AddParameter(name, value);
        }

        protected void AddParameter(string name, object value, DbType? type)
        {
            this.Provider.AddParameter(name, value, type);
        }

        protected void ClearParameter()
        {
            this.Provider.ClearParameter();
        }

        protected DbCommand Command
        {
            get { return this.Provider.Command; }
        }

        protected string CommandText
        {
            get
            {
                return this.Provider.CommandText;
            }
            set
            {
                this.Provider.CommandText = value;
            }
        }

        protected DataRow GetDataRow()
        {
            return this.Provider.GetDataRow();
        }

        protected DataSet DataSet()
        {
            return this.Provider.GetDataSet();
        }

        protected DataTable DataTable()
        {
            return this.Provider.GetDataTable();

        }
        protected DbDataReader GetReader()
        {
            return this.Provider.GetReader();

        }

        protected object ExecuteNonQuery()
        {
            return this.Provider.ExecuteNonQuery();
        }

        protected object GetValue()
        {
            return this.Provider.GetValue();
        }
        protected T GetValue<T>()
        {
            try
            {
                T t = (T)this.Provider.GetValue();
                return t;
            }
            catch
            {
                return default(T);
            }
        }

        #endregion Data Base Method


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                if (disposing)
                {
                    this.Provider.Dispose();
                }
            }
            disposed = true;
        }

        ~DataBase()
        {
            Dispose(false);

        }

        public static IDACAdapter GetDbInstance(string config)
        {
            return new DbInstance(config).Provider;
        }


    }
}