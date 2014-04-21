using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
    internal sealed class DeleteCommandBuilder : CommandBuilderBase, IDeleteCommand
    {
        private DeleteCommandBuilder(string tableName, IDACAdapter dao)
            : base(dao)
        {
            this._tablename = tableName;
        }
        public static DeleteCommandBuilder GetCommand(string tableName, IDACAdapter dao)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                return new DeleteCommandBuilder(tableName, dao);
            }
            return null;
        }
        public bool Delete(ICondition c)
        {
            string query = this.CombineCondition(c, this._tablename, "");
            string queryPattern = string.Format(@"DELETE  FROM {0} {1}",
                                                    this._tablename,
                                                    string.IsNullOrEmpty(query) ? "WHERE " + query : "");
            this._dao.CommandText = queryPattern;
            return this._dao.Execute();
        }

        public void Delete<T>(List<T> cs) where T : ICondition
        {
            foreach (ICondition item in cs)
            {
                string query = this.CombineCondition(item, this._tablename, "");
                string queryPattern = string.Format(@"DELETE  FROM {0} {1}",
                                                    this._tablename,
                                                    string.IsNullOrEmpty(query) ? "WHERE " + query : "");
                 this._dao.CommandText = queryPattern;
                 this._dao.Execute();
            }
        }

        
    }
}
