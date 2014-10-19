using General.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace General.Data
{
    public abstract class DataBase : IDisposable
    {
        private bool disposed = false;
        private ISelectCommand _selectcmd;
        private IInsertCommand _insertcmd;
        private IUpdateCommand _updatecmd;
        private IDeleteCommand _deletecmd;
        private IDapperProvider _dapper;
        protected IDACAdapter Provider { get; private set; }
        protected IDapperProvider Dapper
        {
            get
            {
                if (this._dapper==null)
                    this._dapper = new DapperProvider(Provider.Connection);
                return this._dapper;
            }
        }
    

        public DataBase()
        {
        }

        public DataBase(string ConfigKey)
        {
            Provider = new DefaultProvider(ConfigKey);
            this.InitializeCommand();
        }
        public DataBase(IDACAdapter adapter)
        {
            this.Provider = adapter;
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
 
        #region Method
        private void InitializeCommand()
        {
            //   var attr = this.GetType().GetCustomAttributes(typeof(TableMappingAttribute), true).FirstOrDefault() as TableMappingAttribute;
            TableMappingAttribute attr = this.GetType().GetInstancetAttribute<TableMappingAttribute>();
            if (attr != null)
            {
                this._insertcmd = DapperCommandBuilder.GetInsertCommandBuilder(attr.TableName, this.Dapper);
                this._updatecmd = DapperCommandBuilder.GetUpdateCommandBuilder(attr.TableName, this.Dapper);
                this._deletecmd = DapperCommandBuilder.GetDeleteCommandBuilder(attr.TableName, this.Dapper);
                this._selectcmd = DapperSelectCommandBuilder.GetSelectCommandBuilder(attr.TableName, this.Dapper);
            }
        }

        #endregion Method

        #region Property
        protected ISelectCommand Select { get { return this._selectcmd; } }
        protected IInsertCommand Insert { get { return this._insertcmd; } }
        protected IUpdateCommand Update { get { return this._updatecmd; } }
        protected IDeleteCommand Delete { get { return this._deletecmd; } }
        #endregion
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