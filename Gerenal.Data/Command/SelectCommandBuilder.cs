using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace General.Data
{
    internal sealed class SelectCommandBuilder : CommandBuilderBase, ISelectCommand
    {
        public SelectCommandBuilder(string tableName, IDACAdapter dao)
            : base(dao)
        {
            this._tablename = tableName;
        }
        public DataTable Select(ICondition c)
        {
            string query = this.CombineCondition(c, this._tablename, "");
            string queryPattern =string.Format(@"SELECT * FROM {0} {1}",
                                                    this._tablename,
                                                    string.IsNullOrEmpty(query)?"WHERE "+query:"");
            this._dao.CommandText = queryPattern;
            return this._dao.DataTable();
        }
    }
}
