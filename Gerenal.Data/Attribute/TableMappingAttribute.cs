using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data
{
     [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TableMappingAttribute:Attribute
    {
         public string TableName { get; private set; }
         public TableMappingAttribute()
         {

         }
         public TableMappingAttribute(string TableName)
         {
             this.TableName = TableName;
         }
    }
}
