using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using System.Text;

namespace General.Data.Test
{
    public class TestData : General.Data.DataBase<Entity>
    {
        public TestData():base("Db")
        {
            
        }
        public IEnumerable<Entity> GetData()
        {
            //var c=from p in this._selectcmd
            //      where p.
            return null;
            //return this.Select().From("Table1").
            //    Query<Entity>();
            //return this.Select().From("VendorComment").Where<VendorCommentEntity>(p=>p.Active==1).
            //    Query<VendorCommentEntity>();
        }
        
    }
}
