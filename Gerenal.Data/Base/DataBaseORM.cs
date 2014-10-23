using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
   public  class DataBase<T>:DataBase
    {
       public string TableName { get; set; }
       public DataBase()
       {
           TableMappingAttribute attr = typeof(T).GetInstancetAttribute<TableMappingAttribute>();
           this.TableName = attr.TableName;

       }
    }
}
