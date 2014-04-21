using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace General.Data
{
    internal sealed class UpdateCommandBuilder : CommandBuilderBase, IUpdateCommand
    {

        private UpdateCommandBuilder(string tableName, IDACAdapter dao)
            : base(dao)
        {
            this._tablename = tableName;
        }
        public static UpdateCommandBuilder GetCommand(string tableName, IDACAdapter dao)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                return new UpdateCommandBuilder(tableName, dao);
            }
            return null;
        }
        public bool Update(ICommandEntity e)
        {
            return this.Update(e, null);
        }

        public bool Update(ICommandEntity e, ICondition c)
        {
            string queryPattern = "UPDATE {0} SET {1} {2} ";
            string condition = this.CombineCondition(c, this._tablename,"");
            if (!string.IsNullOrEmpty(condition))
            {
                PropertyInfo[] props = e.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                List<string> fieldList = new List<string>();
                foreach (PropertyInfo prop in props)
                {
                    if (prop.IsDefined(typeof(DataSourceColumnAttribute), true))
                    {
                        DataSourceColumnAttribute attr = prop.GetInstancetAttribute<DataSourceColumnAttribute>();
    
                        if (attr.IsCustomField || attr.IsAutoKey)
                            continue;
                        object v = prop.GetValue(e, null);

                        if (v != null)
                        {
                            string fieldName = !String.IsNullOrEmpty(attr.FieldName) ? attr.FieldName : prop.Name;
                            string parameterName = String.Format("{0}_{1}",this._tablename, fieldName);

                            fieldList.Add(String.Format("{0} = @{1}", fieldName, parameterName));
                            this._dao.AddParameter(parameterName, v);

                        }
                    }
                }
                string query =
                      String.Format(queryPattern,
                      this._tablename,
                      String.Join(", ", fieldList.ToArray()),
                      condition);
                this._dao.CommandText = query;
                return this._dao.Execute();
            }
            return false;
        }
    }
}
