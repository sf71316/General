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
        public DefaultProvider(string connectstring, SQLType type)
           : base(connectstring, type)
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
                    obj = reader[0];
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

        public override DbConnection GenerateConnection()
        {
            var conn = this._provider.CreateConnection();
            conn.ConnectionString = this.settings.ConnectionString;
            return conn;
        }

        public override DbCommand GenerateCommand()
        {
            return this.Connection.CreateCommand();
        }

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