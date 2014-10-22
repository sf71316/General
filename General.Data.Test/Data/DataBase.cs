using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using System.Text;

namespace General.Data.Test
{
    [TableMapping("VendorComment")]
    public class TestData:General.Data.DataBase
    {
        public TestData():base("Db")
        {
            
        }
        public IEnumerable<Entity> GetData()
        {


            return this.Select().From("Table1").
                Query<Entity>();
            //return this.Select().From("VendorComment").Where<VendorCommentEntity>(p=>p.Active==1).
            //    Query<VendorCommentEntity>();
        }
        
    }
}
