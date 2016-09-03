using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using System.Text;

namespace General.Data.Test
{
    public class TestData : DataBase
    {
        public TestData()
        {
            this.GenerateDataBase("Data Source=MINISERVER\\SQL2012;Initial Catalog=BugNET_sf71316;User Id=sa;Password = 123456; ", SQLType.MSSQL);
        }
        public bool Test()
        {
            using (var conn = this.Provider.GenerateConnection())
            {
                try
                {
                    conn.Open();
                    return true;
                }catch(Exception e )
                {
                    return false;
                }
            }
        }

    }
}
