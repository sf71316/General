using System;
using General.Service.BSS;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using General.Service.OssChannel;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using General.Service.Billing;
using System.Diagnostics;


namespace General.Service.Test
{

    [TestClass]
    public class ServiceTest
    {

        MSTVService service = MSTVService.DefaultService();

        [TestMethod]
        public void TestMethod3()
        {
          
        }
        [TestMethod]
        public void TestReboot()
        {
         

        }
     
        [TestMethod]
        public void TestBSS_RemoveDeviceValues()
        {
      
            
            this.service.PrincipalManagement.RemoveDeviceValues(new DevicePrincipalExternalId
            {
                Id = "00803F1A0D18"
            }, new string[] { "ClientMenus" });
        }

        [TestMethod]
        public void Set_DeviceValue_STB()
        {
            var devicevalue = this.service.PrincipalManagement.ReadAllDeviceValues("TATUNG STB-2520(GS)_195083").First(p => p.Key == "EnhancedHdmiInterfaceSecurity");
            this.service.PrincipalManagement.UpdateDeviceValuesAndNotify("TATUNG STB-2520(GS)_195083",
                new DeviceValue[] { 
                new DeviceValue{Key="EnhancedHdmiInterfaceSecurity",Value="0"}
                });
            devicevalue = this.service.PrincipalManagement.ReadAllDeviceValues("TATUNG STB-2520(GS)_195083").First(p => p.Key == "EnhancedHdmiInterfaceSecurity");
         
        }
        [TestMethod]
        public void Read_GetGlobalValue()
        {
            var d = DateTime.UtcNow;
            DeviceValue dv = this.service.PrincipalManagement.ReadAllDeviceValues("ac6fbb07a793", "DetuneTimeoutHours");
         //   UserstoreNameValue[] value = this.service.PrincipalManagement.GetGlobalValue("DetuneTimeoutHours");

        }
        [TestMethod]
        public void Billing_Test()
        {
           // DateTime dtStart = DateTime.Now.AddHours(-2).ToUniversalTime();
           // DateTime dtEnd = DateTime.Now.ToUniversalTime();
           //var c= this.service.Billing.ReadBillingRecord(new Billing.BillingRecord(), dtStart, dtEnd);
           //var e1 = c.Where(p => p.Status == BillingEventStatus.UnRead);
           
        }
        [TestMethod]
        public void OSS_EPG_List_Test()
        {
            var s = this.service.OssEpg.ReadEPGByDateRange(
                DateTime.Now,DateTime.Now.AddHours(5));
            
        }
        [TestMethod]
        public void OSS_MultiThread_GetSG_TEST()
        {
            Stopwatch sw = new Stopwatch();
            int count = 100;
            Account[] account = this.service.PrincipalManagement.ReadAccount(null);
            var _account = account.Take(count).Select(p =>new AccountPrincipalExternalId { Id = p.ExternalID });
            sw.Start();
            var _sgmapping = this.service.PrincipalManagement.BatchGetGroupMemberships < AccountPrincipalExternalId>(_account.ToList());
            sw.Stop();
            Debug.WriteLine("Test Spend "+sw.ElapsedMilliseconds+"ms");
            Assert.IsTrue(_sgmapping.Count == count);
        }
        [TestMethod]
        public void OSS_SingleThread_GetSG_TEST()
        {
            Stopwatch sw = new Stopwatch();
            int count = 100;
            Dictionary<PrincipalIdentifier, GroupMembership[]> _collection = 
                new Dictionary<PrincipalIdentifier, GroupMembership[]>();
            Account[] account = this.service.PrincipalManagement.ReadAccount(null).Take(count).ToArray();
            sw.Start();
            foreach (var item in account)
            {
                AccountPrincipalExternalId id=new AccountPrincipalExternalId{Id=item.ExternalID};
                var sg = this.service.PrincipalManagement.GetGroupMemberships(id);
                _collection.Add(id, sg);
            }
            sw.Stop();
            Debug.WriteLine("Test Spend " + sw.ElapsedMilliseconds + "ms");
            Assert.IsTrue(_collection.Count == count);
        }
        [TestMethod]
        public void OSS_GetSG_Diff_Raw()
        {
            Stopwatch sw = new Stopwatch();
            int count = 1000;
            Account[] account = this.service.PrincipalManagement.ReadAccount(null);
            var _account = account.Take(count).Select(p => new AccountPrincipalExternalId { Id = p.ExternalID });
            sw.Start();
            var _sgmapping = this.service.PrincipalManagement.BatchGetGroupMemberships<AccountPrincipalExternalId>(_account.ToList());
            sw.Stop();
            Debug.WriteLine("Test Spend " + sw.ElapsedMilliseconds + "ms");
            Assert.IsTrue(_sgmapping.Count == count);
        }
        [TestMethod]
        public void OSS_GetSG_Diff_String()
        {
            Stopwatch sw = new Stopwatch();
            int count = 1000;
            Account[] account = this.service.PrincipalManagement.ReadAccount(null);
            var _account = account.Take(count).Select(p => new AccountPrincipalExternalId { Id = p.ExternalID });
            sw.Start();
            var _sgmapping = this.service.PrincipalManagement.BatchGetGroupMembershipsByString<AccountPrincipalExternalId>(_account.ToList());
            sw.Stop();
            Debug.WriteLine("Test Spend " + sw.ElapsedMilliseconds + "ms");
            Assert.IsTrue(_sgmapping.Count == count);
        }
    }

}
