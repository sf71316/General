using General.Data.Dapper;
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

        #region   ORM Method
        protected object Insert(object e)
        {
            return this._insertcmd.Insert(e);
        }
        protected int Delete(Expression expr)
        {
            return this._deletecmd.Delete(expr);
        }
        protected int Delete<T>(Expression<Func<T, bool>> expr)
        {
            return this._deletecmd.Delete<T>(expr);
        }
        protected int Update(object e, Expression expr)
        {
            return this._updatecmd.Update(e, expr);
        }
        protected int Update<T>(object e, Expression<Func<T, bool>> expr)
        {
            return this._updatecmd.Update<T>(e, expr);
        }
        //protected ISelectQuery Where(Expression expr)
        //{
        //    return this._selectcmd.Where(expr);
        //}

        //protected ISelectQuery Where<T1>(Expression<Func<T1, bool>> expr)
        //{
        //    return this._selectcmd.Where<T1>(expr);
        //}

        //protected ISelectQuery Where<T1, T2>(Expression<Func<T1, T2, bool>> expr)
        //{
        //    return this._selectcmd.Where<T1, T2>(expr);
        //}

        //protected ISelectQuery Where<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> expr)
        //{
        //    return this._selectcmd.Where<T1, T2, T3>(expr);
        //}

        //protected ISelectQuery Where<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> expr)
        //{
        //    return this._selectcmd.Where<T1, T2, T3, T4>(expr);
        //}

        protected ISelectCommand Select(string field = "*")
        {
            return this._selectcmd.Select(field);
        }

        //protected ISelectCommand From(string tablename)
        //{
        //    return this._selectcmd.From(tablename);
        //}

        //protected IEnumerable<T> Query<T>()
        //{
        //    return this._selectcmd.Query<T>();
        //}

        //protected ISelectQuery OrderBy(string fieldname)
        //{
        //    return this.OrderBy(fieldname);
        //}

        
        #endregion

        protected ISelectCommand SelectCommand
        {
            get
            {
                return this._selectcmd;
            }
        }
        protected IInsertCommand InsertCommand
        {
            get
            {
                return this._insertcmd;
            }
        }
        protected IUpdateCommand UpdateCommand
        {
            get
            {
                return this._updatecmd;
            }
        }
        protected IDeleteCommand DeleteCommand
        {
            get
            {
                return this._deletecmd;
            }
        }

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