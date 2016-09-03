using General.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace General.Data
{
   public abstract  class DataBase<T>:DataBase
    {
       public string TableName { get; set; }
       protected ISelectCommand<T> _selectcmd;
       protected IInsertCommand _insertcmd;
       protected IUpdateCommand _updatecmd;
       protected IDeleteCommand _deletecmd;
       public DataBase(string conName):base(conName)
       {
           TableMappingAttribute attr = typeof(T).GetInstancetAttribute<TableMappingAttribute>();
           this.TableName = attr.TableName;
           //this._insertcmd = DapperCommandBuilder.GetInsertCommandBuilder(this.Dapper);
           //this._updatecmd = DapperCommandBuilder.GetUpdateCommandBuilder(this.Dapper);
           //this._deletecmd = DapperCommandBuilder.GetDeleteCommandBuilder(this.Dapper);
           //this._selectcmd = DapperCommandBuilder.GetSelectCommandBuilder<T>(this.Dapper);
       }

     
    }
}
