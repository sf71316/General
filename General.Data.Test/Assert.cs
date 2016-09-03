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
           // var m = new TestData();
          //  var c=   m.GetData();
        }
        [TestMethod]
        public void Dapper_Inject_Connectstring()
        {
            var m = new TestData();
            Assert.IsTrue(m.Test());
            //  var c=   m.GetData();
        }
    }
}
