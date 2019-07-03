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
        public override DataSet GetDataSet()
        {
            _adapter.SelectCommand = this._cmd;
            DataSet ds = new DataSet();
            this._adapter.Fill(ds);
            return ds;
        }

        public override DataTable GetDataTable()
        {
            DataSet ds = this.GetDataSet();
            if (ds != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            else
                return null;
        }

        public override DataRow GetDataRow()
        {
            DataTable dt = this.GetDataTable();
            if (dt != null && dt.Rows.Count > 0)
                return dt.Rows[0];
            else
                return null;
        }

        public override object GetValue()
        {

            object obj = null;
            DbDataReader reader = this.GetReader();
            this.Open();
            if (reader.Read())
            {
                obj = reader[0];
            }
            this.Close();
            return obj;

        }

        public override DbDataReader GetReader()
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

        public override DbCommand CreateCommand()
        {
            return this._conn.CreateCommand();
        }
        public override void InitCommand()
        {
            this._cmd = this.CreateCommand();
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
            if (this._adapter != null)
                this._adapter.Dispose();
            if (this._cmd != null)
                this._cmd.Dispose();
            if (this._conn != null)
                this._conn.Dispose();
            if (this._provider != null)
                this._provider = null;
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