using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using General;
using System.Reflection;
using System.Linq.Expressions;

namespace General.Data.Test
{
    [TestClass]
    public class DAO_Test
    {

        [TestMethod]
        public void Dapper_ORM_Select_Test()
        {
            var m = new TestData();
            var c=   m.GetData();
        }
        [TestMethod]
        public void Query_Test()
        {
            QueryTranslator q = new QueryTranslator();
            Expression<Func<Entity, bool>> expr = p => (p.Field1 == "1" || p.Field1 == "2") && p.PK == "3";
            q.Translate(expr);
            q.ToWhere();
        }

    }
}
