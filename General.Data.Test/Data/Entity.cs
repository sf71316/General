using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Data.Test
{
    public class Entity:IEntity,ICommandEntity
    {
        public string AccountID { get; set; }
        public int PackageID { get; set; }
        public DateTime OrderDate { get; set; }
        public int IsRemove { get; set; }
        public DateTime RemoveDate { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}
