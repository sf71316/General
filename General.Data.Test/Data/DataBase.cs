using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using System.Text;

namespace General.Data.Test
{
    [TableMapping("General_Log")]
    public class TestDataBase:General.Data.DataBase
    {
        public TestDataBase():base("Db")
        {
            
        }
    
        public Entity GetEntity()
        {
            this.CommandText = "SELECT * FROm General_Log where LogID='9'";
            return this.GetEntity<Entity>();
        }
        public List<Entity> GetCollection()
        {
            this.CommandText = "SELECT * FROm General_Log";
            List<Entity> collection=this.GetEntities<Entity>().ToList();
            return collection;
        }

        public IEnumerable<Entity> DapperTest(int count)
        {
            
            return this.Dapper.Query<Entity>("SELECT TOP " + count + " * FROM AccountPackage",
                null);
        }
        public IEnumerable<Entity> DefaultTest(int count)
        {
            this.CommandText = "SELECT TOP " + count + " * FROM AccountPackage";
            return this.GetEntities<Entity>();
        }
        public void AddTestDataforDefault()
        {
            for (int i = 0; i < 50000; i++)
            {
                var e = new Entity
                {
                    AccountID=Guid.NewGuid().ToString("N"),
                    PackageID=1
                };
                this.CommandText = "INSERT INTO AccountPackage(AccountID,PackageID) Values(@AccountID,@PackageID)";
                this.ClearParameter();
                this.AddParameter("AccountID", e.AccountID);
                this.AddParameter("PackageID", e.PackageID);
                this.ExecuteNonQuery();
            }
        }
        public void AddTestDataforDapper()
        {
            for (int i = 0; i < 50000; i++)
            {
                var e = new Entity
                {
                    AccountID = Guid.NewGuid().ToString("N"),
                    PackageID = 1
                };

                this.Dapper.Execute("INSERT INTO AccountPackage(AccountID,PackageID) Values(@AccountID,@PackageID)",e);
            }
        }
    }
}
