using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using General;
using System.Reflection;

namespace General.Data.Test
{
    [TestClass]
    public class DAO_Test
    {

          [TestMethod]
        public void Dapper_ORM_Select_Test()
        {
            new TestDataBase().DapperTest(10000);
        }
          [TestMethod]
          public void Default_ORM_Select_Test()
          {
              new TestDataBase().DefaultTest(10000);
          }
           [TestMethod]
          public void Dapper_ORM_Add_Test()
          {
              new TestDataBase().AddTestDataforDapper();
          }
          [TestMethod]
          public void Default_ORM_Add_Test()
          {
              new TestDataBase().AddTestDataforDefault();
          }
    }
}
