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
        public IEnumerable<VendorCommentEntity> GetData()
        {


            return this.Select().From("Table1").Where<Entity>(p => p.PK=="1" && p.Field1=="" || p.FK=="222").
                Query<VendorCommentEntity>();
            //return this.Select().From("VendorComment").Where<VendorCommentEntity>(p=>p.Active==1).
            //    Query<VendorCommentEntity>();
        }
        
    }
}
