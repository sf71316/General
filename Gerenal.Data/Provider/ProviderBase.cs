using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace General.Data
{
    internal abstract class ProviderBase : IDisposable, IDACAdapter
    {
        protected DbCommand _cmd;
        protected DbConnection _conn;
        protected DbProviderFactory _provider;
        protected DbDataAdapter _adapter;
        protected ConnectionStringSettings settings;
         public ProviderBase()
        {
           
        }

         public ProviderBase(string connection)
        {
            settings = ConfigurationManager.ConnectionStrings[connection];
            this.Init();
        }
        protected void Init()
        {
            if (settings != null)
            {
                _provider = DbProviderFactories.GetFactory(settings.ProviderName);
                _conn = _provider.CreateConnection();
                _conn.ConnectionString = settings.ConnectionString;
                _cmd = _provider.CreateCommand();
                _cmd.Connection = _conn;
                _adapter = _provider.CreateDataAdapter();
                this.ProviderName = this.settings.ProviderName;
               
            }
            else
            {
                throw new NoNullAllowedException("找不到該連線設定");
            }
        }

        public string ProviderName { get; set; }
        public DbConnection Connection
        {
            get { return this._conn; }
            set { this._conn = value; }
        }

        public DbDataAdapter Adapter
        {
            get { return this._adapter; }
        }

        public DbCommand Command
        {
            get
            {
                return this._cmd;
            }
        }

        #region IDACAdapter
        public abstract void AddParameter(string name, object value);

        public abstract void AddParameter(string name, object value, System.Data.DbType? type);

        public abstract void ClearParameter();

        public abstract void CreateCommand();

        public abstract string CommandText{get;set;}

        public abstract DataRow DataRow();

        public abstract DataSet DataSet();

        public abstract DataTable DataTable();

        public abstract DbDataReader Reader();

        public abstract object ExecuteNonQuery();

        public abstract bool Execute();

        public abstract object Value{get;}
        public virtual void Dispose()
        {
            if (this != null)
            {
                _provider = null;
                _conn.Dispose();
                _cmd.Dispose();
                _adapter.Dispose();
            }
        }
        #endregion

    }
}
