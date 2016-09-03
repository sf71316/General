﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;

namespace General.Data
{
    public  abstract class DataBase : IDisposable
    {
        private bool disposed = false;

        private IDapperProvider _dapper;
        protected IDACAdapter Provider { get; private set; }

        public DataBase()
        {
        }

        public DataBase(string ConfigKey)
        {
            Provider = new DefaultProvider(ConfigKey);
        }
        protected void GenerateDataBase(string connectstring,SQLType type)
        {
            Provider = new DefaultProvider(connectstring, type);
        }
        public DataBase(IDACAdapter adapter)
        {
            this.Provider = adapter;
        }

        #region Data Base Method

        protected void CreateCommand()
        {
            this.Provider.CreateCommand();
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

        protected DataRow DataRow()
        {
            return this.Provider.DataRow();
        }

        protected DataSet DataSet()
        {
            return this.Provider.DataSet();
        }

        protected DataTable DataTable()
        {
            return this.Provider.DataTable();
  
        }
        protected DbDataReader DataReader()
        {
            return this.Provider.Reader();

        }
        //[Obsolete("此方法已過時，請使用Dapper")]
        //protected T GetEntity<T>() where T : IEntity
        //{
        //    return this.Provider.GetEntity<T>();
        //}
        //[Obsolete("此方法已過時，請使用Dapper")]
        //protected IEnumerable<TEntity> GetEntities<TEntity>()
        //    where TEntity : IEntity
        //{
        //    return this.Provider.GetEntities<TEntity>();
        //}

        protected object ExecuteNonQuery()
        {
            return this.Provider.ExecuteNonQuery();
        }

        protected object Value
        {
            get { return this.Provider.Value; }
        }

        #endregion Data Base Method

        #region Event

        protected virtual void Provider_Initialize(object sender, DbEventArgs e)
        {
        }

        protected virtual void Provider_Initialized(object sender, DbEventArgs e)
        {
        }

        #endregion Event

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