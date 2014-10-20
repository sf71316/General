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
            var m = new TestData();
            m.GetData();
        }

    }
}
