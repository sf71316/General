using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace General.Data
{
   internal sealed class InsertCommandBuilder:CommandBuilderBase,IInsertCommand
    {
       private InsertCommandBuilder(string tableName, IDACAdapter dao)
            : base(dao)
        {
                this._tablename = tableName;
        }
       public static InsertCommandBuilder GetCommand(string tableName, IDACAdapter dao)
       {
           if (!string.IsNullOrEmpty(tableName))
           {
               return new InsertCommandBuilder(tableName, dao);
           }
           return null;
       }
       private void InsertAction(ICommandEntity e)
       {
           string queryPattern = "INSERT INTO {0} ({1}) VALUES({2})";
           PropertyInfo[] props = e.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
           List<string> fieldList = new List<string>();
           List<string> valueList = new List<string>();
           foreach (PropertyInfo prop in props)
           {
               string fieldName = prop.Name;
               DataSourceColumnAttribute attr = prop.GetInstancetAttribute<DataSourceColumnAttribute>();
               if (attr != null)
               {
                   fieldName = !String.IsNullOrEmpty(attr.FieldName) ? attr.FieldName : fieldName;
                   if (attr.IsAutoKey || attr.IsCustomField)
                       continue;
               }
               object v = prop.GetValue(e, null);

               if (v != null)
               {
                   string parameterName = String.Format("{0}_{1}", _tablename, fieldName);
                   fieldList.Add(fieldName);
                   valueList.Add(String.Format("@{0}", parameterName));
                   this._dao.AddParameter(parameterName, v);

               }
           }
           string query =
               String.Format(queryPattern,
               this._tablename,
               String.Join(", ", fieldList.ToArray()),
               String.Join(", ", valueList.ToArray()));
           this._dao.CommandText = query;
       }
       public bool Insert(ICommandEntity e)
       {
           this._dao.ClearParameter();
           this.InsertAction(e);
           return this._dao.Execute();
       }
       [STAThread]
       public object Insert(ICommandEntity e, bool IsReturnID)
       {
           this._dao.ClearParameter();
           this.InsertAction(e);
           if (IsReturnID)
           {
               this._dao.CommandText += "SELECT @@IDENTITY AS 'Identity'";
           }
           return this._dao.ExecuteNonQuery();
       }

       public R Insert<R>(ICommandEntity e)
       {
           this._dao.ClearParameter();
           this.InsertAction(e);
           return (R)this._dao.ExecuteNonQuery();
       }

       public void Insert<T>(List<T> es) where T : ICommandEntity
       {
           foreach (var item in es)
           {
                this._dao.ClearParameter();
                this.InsertAction(item);
                this._dao.ExecuteNonQuery();
           }
       }
    }
}
