using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Data;

namespace General.Data
{
    internal abstract class CommandBuilderBase
    {
        protected IDACAdapter _dao;
        protected string _tablename;
        public CommandBuilderBase(IDACAdapter dao)
        {
            this._dao = dao;
        }
        protected string CombineCondition(ICondition c, string tableName, string surfix)
        {
            if (c != null)
            {
                List<string> conditionList = new List<string>();
                PropertyInfo[] props = c.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo prop in props)
                {
                    object v = prop.GetValue(c, null);

                    if (v != null)
                    {
                        DataSourceColumnAttribute attr = null;
                        if (prop.IsDefined(typeof(DataSourceColumnAttribute), true))
                            attr = prop.GetInstancetAttribute<DataSourceColumnAttribute>();

                        // 如果標記為自定義欄位則略過組合條件
                        if (attr != null && attr.IsCustomField)
                            continue;

                        string fieldName = attr != null && !String.IsNullOrEmpty(attr.FieldName) ? attr.FieldName : prop.Name;
                        string parameterName = String.Format("condition_{0}{1}", fieldName, surfix);

                        if (prop.PropertyType.IsGenericType &&
                            prop.PropertyType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
                        {
                            string condition = this.CombineCondition((System.Collections.IList)v, fieldName, tableName, surfix);
                            if (!String.IsNullOrEmpty(condition))
                                conditionList.Add(condition);
                        }
                        else
                        {
                            conditionList.Add(String.Format(" ({0}{1} = @{2}) ",
                                !String.IsNullOrEmpty(tableName) ? tableName + "." : "",
                                fieldName,
                                parameterName));
                            this._dao.AddParameter(parameterName, v);
                        }
                    }
                }

                return conditionList.Count > 0 ? " WHERE " + String.Join(" AND ", conditionList.ToArray()) : "";
            }

            return "";
        }
        protected string CombineCondition(IList list, string fieldName, string tableName, string surfix)
        {
            if (list != null && list.Count > 0)
            {
                string result = String.Empty;
                string parameterName = String.Empty;
                if (list.Count == 1)
                {
                    parameterName = String.Format("condition_{0}", fieldName);
                    result = String.Format(" ({0}{1} = @{2}{3} ",
                        !String.IsNullOrEmpty(tableName) ? tableName + "." : "",
                        fieldName, parameterName, surfix);
                    this._dao.AddParameter(parameterName, list[0]);

                }
                else
                {
                    List<string> parasList = new List<string>();
                    for (int index = 0; index < list.Count; index++)
                    {
                        parameterName = String.Format("condition_{0}{1}{2}", fieldName, surfix, index);
                        parasList.Add("@" + parameterName);
                        this._dao.AddParameter(parameterName, list[0]);
                    }
                    result = String.Format(" ({0}{1} IN ({2})) ",
                        !String.IsNullOrEmpty(tableName) ? tableName + "." : "",
                        fieldName,
                        String.Join(",", parasList.ToArray()));
                }

                return result;
            }
            return "";
        }
          
    }
}
