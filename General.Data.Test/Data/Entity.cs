using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data.Test
{
    
    public class Entity:IEntity,ICommandEntity
    {
        [Column(FieldName="PK",AutoKey=true,Key=true)]
        public int Key { get; set; }
        public Guid FK { get; set; }
        [Column(FieldName = "Field1")]
        public int Name { get; set; }
 
    }
}
