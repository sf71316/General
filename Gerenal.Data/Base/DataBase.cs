using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace General.Data
{
    public abstract class DataBase : IDisposable
    {
        private bool disposed = false;
        protected IDACAdapter Provider { get; private set; }
        protected IDapperProvider Dapper { get; private set; }
        private IInsertCommand _insertcmd;
        private IUpdateCommand _updatecmd;
        private IDeleteCommand _deletecmd;

        public DataBase()
        {
        }

        public DataBase(string ConfigKey)
        {
            Provider = new DefaultProvider(ConfigKey);
            this.Dapper = new DapperProvider(Provider.Connection);
            this.InitializeCommand();
        }
        public DataBase(IDACAdapter adapter)
        {
            this.Provider = adapter;
            this.Dapper = new DapperProvider(adapter.Connection);
            this.InitializeCommand();
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
        [Obsolete("此方法已過時，請使用Dapper")]
        protected T GetEntity<T>() where T : IEntity
        {
            return this.Provider.GetEntity<T>();
        }
        [Obsolete("此方法已過時，請使用Dapper")]
        protected IEnumerable<TEntity> GetEntities<TEntity>()
            where TEntity : IEntity
        {
            return this.Provider.GetEntities<TEntity>();
        }

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

 
        #region Method
        private void InitializeCommand()
        {
            //   var attr = this.GetType().GetCustomAttributes(typeof(TableMappingAttribute), true).FirstOrDefault() as TableMappingAttribute;
            TableMappingAttribute attr = this.GetType().GetInstancetAttribute<TableMappingAttribute>();
            if (attr != null)
            {
                this._insertcmd = InsertCommandBuilder.GetCommand(attr.TableName, this.Provider);
                this._updatecmd = UpdateCommandBuilder.GetCommand(attr.TableName, this.Provider);
                this._deletecmd = DeleteCommandBuilder.GetCommand(attr.TableName, this.Provider);
            }
        }

        #endregion Method

        #region IInsertCommand

        protected bool Insert(ICommandEntity e)
        {
            return this._insertcmd.Insert(e);
        }

        protected object Insert(ICommandEntity e, bool IsReturnID)
        {
            return this._insertcmd.Insert(e, IsReturnID);
        }

        protected R Insert<R>(ICommandEntity e)
        {
            return this._insertcmd.Insert<R>(e);
        }

        protected void Insert<T>(List<T> es) where T : ICommandEntity
        {
            this._insertcmd.Insert<T>(es);
        }

        #endregion IInsertCommand

        #region IUpdateCommand

        protected bool Update(ICommandEntity e)
        {
            return this._updatecmd.Update(e);
        }

        protected bool Update(ICommandEntity e, ICondition c)
        {
            return this._updatecmd.Update(e, c);
        }

        #endregion IUpdateCommand

        #region IDeleteCommand

        protected bool Delete(ICondition c)
        {
            return this._deletecmd.Delete(c);
        }

        protected void Delete<T>(List<T> cs) where T : ICondition
        {
            this._deletecmd.Delete<T>(cs);
        }

        #endregion IDeleteCommand

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