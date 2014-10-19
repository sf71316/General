using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
//using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace General.Data
{
    internal class DefaultProvider : ProviderBase
    {

        public DefaultProvider()
        {
          
        }

        public DefaultProvider(string connection)
            : base(connection)
        {
          
        }
     
        public override object ExecuteNonQuery()
        {
            this.Open();
            object obj = this._cmd.ExecuteNonQuery();
           this.Close();
            return obj;
        }
        public override bool Execute()
        {
            this.Open();
            object msg = this._cmd.ExecuteNonQuery();
            if (msg != null)
            {
                int r = 0;
                if (int.TryParse(msg.ToString(), out r))
                {
                    return (r > 0) ? true : false;
                }
            }
             this.Close();
            return false;
        }
        public override DataSet DataSet()
        {
            _adapter.SelectCommand = this._cmd;
            DataSet ds = new DataSet();
            this._adapter.Fill(ds);
            return ds;
        }

        public override DataTable DataTable()
        {
            DataSet ds = this.DataSet();
            if (ds != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            else
                return null;
        }

        public override DataRow DataRow()
        {
            DataTable dt = this.DataTable();
            if (dt != null && dt.Rows.Count > 0)
                return dt.Rows[0];
            else
                return null;
        }

        public override object Value
        {
            get
            {
                this.Open();
                object obj = null;
                DbDataReader reader = this.Reader();
                if (reader.Read())
                {
                    obj= reader[0];
                }
                this.Close();
                return obj;
            }
        }

        public override DbDataReader Reader()
        {
            return this._cmd.ExecuteReader();
        }


        public override void AddParameter(string name, object value)
        {
            this.AddParameter(name, value, null);
        }

        public override void AddParameter(string name, object value, DbType? type)
        {
            DbParameter parameter = this._cmd.CreateParameter();
            parameter.ParameterName = name;
            if (value != null)
            {
                parameter.Value = value;
                if (type != null)
                    parameter.DbType = type.Value;
            }
            else
                parameter.Value = DBNull.Value;
            this._cmd.Parameters.Add(parameter);
        }

        public override void ClearParameter()
        {
            if (this._cmd != null)
            {
                this._cmd.Parameters.Clear();
            }
        }

        public override void CreateCommand()
        {
            this._cmd = this._conn.CreateCommand();
        }

        public override   T GetEntity<T>() 
        {
            this.Open();
            DbDataReader reader = this._cmd.ExecuteReader();
            T entity = entity = Activator.CreateInstance<T>();
            
            if (reader.Read())
            {
                this.GetEntity<T>(reader, entity);
            }

             this.Close();
            return entity;
        }

        public override  IEnumerable<TEntity> GetEntities<TEntity>() 
        {
            this.Open();
            DbDataReader reader = this._cmd.ExecuteReader();
            var collection = new List<TEntity>();
            while (reader.Read())
            {
                TEntity entity = Activator.CreateInstance<TEntity>();
                this.GetEntity<TEntity>(reader, entity);

                collection.Add(entity);
            }
             this.Close();
            return collection;
        }

        private void Close()
        {
            if (this._conn.State != ConnectionState.Closed)
                this._conn.Close();
        }

        private void Open()
        {
            if (this._conn.State == ConnectionState.Closed)
                this._conn.Open();
        }

        public override void Dispose()
        {
          
        }


        #region Prviate Method

        private void GetEntity<T>(DbDataReader reader, T t) where T : IEntity
        {
            PropertyInfo[] properties = t.GetType().GetProperties(BindingFlags.Public | 
                                                                                                                BindingFlags.Instance | 
                                                                                                                BindingFlags.NonPublic);
            foreach (PropertyInfo item in properties)
            {
                //TODO 目前未實作Attribute套用機制
                ITypeConverter typeConverter = TypeConverterFactory.GetConvertType(item.PropertyType);
                ColumnAttribute _columnAttr =
                    item.GetInstancetAttribute<ColumnAttribute>();
                string _column;
                if (_columnAttr == null)
                {
                    _column = item.Name;
                }
                else
                {
                    if (_columnAttr.Ignore)
                        continue;
                    else
                        _column = _columnAttr.FieldName;
                }
                if (reader.HasColumn(_column))
                {
                    if (item.PropertyType.IsEnum)
                    {
                        EnumConverter _enumconverter = typeConverter as EnumConverter;
                        item.SetValue(t,
                         _enumconverter.Convert(item.PropertyType,
                                                                        reader.GetValue(reader.GetOrdinal(_column))), null);
                    }
                    else
                    {
                        item.SetValue(t,
                         typeConverter.Convert(reader.GetValue(reader.GetOrdinal(_column))), null);
                    }
                    
                    //   item.SetValue(entity, reader.GetValue(reader.GetOrdinal(item.Name)), null);
                }
            }
        }

        #endregion Prviate Method

        public override string CommandText
        {
            get
            {
                return this._cmd.CommandText;
            }
            set
            {
                this._cmd.CommandText = value;
            }
        }

      
      
    }
}