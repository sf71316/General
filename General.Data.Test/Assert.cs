using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using General;
using System.Reflection;
using System.Linq.Expressions;
using System.Linq;

namespace General.Data.Test
{
    [TestClass]
    public class DAO_Test
    {

        [TestMethod]
        public void Dapper_ORM_Select_Test()
        {
            var m = new TestData();
            var c = m.GetData();
        }
        [TestMethod]
        public void Single_Query_Test()
        {
            QueryTranslator q = new QueryTranslator();
            Expression<Func<Entity, bool>> expr = p => new string[] { "A", "B" }.Contains(p.Field1);
            q.Translate(expr);
            var ss = q.ToWhere();
            Assert.IsTrue("WHERE ([Field1] IN (@T1_Field1_0,@T1_Field1_1))" == ss);
        }
        [TestMethod]
        public void Single_Query_Test_2()
        {
            QueryTranslator q = new QueryTranslator();
            Expression<Func<Entity, bool>> expr = p => new string[] { "A", "B" }.Contains(p.Field1) && p.PK == "test";
            q.Translate(expr);
            var ss = q.ToWhere();
            Assert.IsTrue("WHERE ([Field1] IN (@T1_Field1_0,@T1_Field1_1))([PK]  =  @T1_PK_0)" == ss);
        }
        [TestMethod]
        public void Single_Query_Test_3()
        {
            QueryTranslator q = new QueryTranslator();
            Expression<Func<Entity, bool>> expr = p =>
                p.FK.ToString() != "" &&
                new string[] { "A", "B" }.Contains(p.Field1) && p.PK == "test";
            q.Translate(expr);
            var ss = q.ToWhere();
           // Assert.IsTrue("WHERE ([Field1] IN (@T1_Field1_0,@T1_Field1_1))([PK]  =  @T1_PK_0)" == ss);
        }
    }
}
