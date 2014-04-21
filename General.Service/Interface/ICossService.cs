using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Service
{
    public interface ICossService : IDisposable,IServiceCommon
    {
        string AccountDetails(string acc);
        string AddPackage(string pkgName);
        string BossGateway(string cmd);
        string CreateAccount(string acc);
        string devCluster(string cm);
        string GetAccountbyDeviceExternalId(string dev);
        string GetDupSG(string accPlusSG);
        string getMSWDSeq();
        string GetSSLBasicPackage();
        void log_getAccDetails(string acc);
        string log_GetDeviceSG(string dev);
        void log_GetUser(string acc);
        string QuanTuen(string acc);
        string[] ReadAccountSubscriberGroups(string acc);
        string RemovePackage(string pkgName);
        string removeUser(string acc);
        string setMaster(string acc);
        string setUser(string acc);
        string sgMapping(string id);
        string sgToDevice(string pkgName);
        string stbToPurchase(string accREC);
        string XMLsgMapping2coss(string id);
        string ynDev(string rc);
    }
}
