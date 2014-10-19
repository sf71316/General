using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using System.Text;

namespace General.Data.Test
{
    [TableMapping("Table1")]
    public class TestData:General.Data.DataBase
    {
        public TestData():base("Db")
        {
            
        }

    }
}
