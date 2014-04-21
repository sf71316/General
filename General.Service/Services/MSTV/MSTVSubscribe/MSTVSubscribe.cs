using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using General.Service.COSS;


namespace General.Service.MSTV.COSS
{
    public abstract class MSTVSubscribeBase : IServiceConfiguration, General.Service.Services.MSTV.MSTVSubscribe.IMSTVSubscribeBase
    {
        Service1 _service;
        public MSTVSubscribeBase()
        {
            this._service = new Service1();
            
        }

        public string AccountDetails(string acc)
        {
            return this._service.AccountDetails(acc);
        }

        public string AddPackage(string pkgName)
        {
            return this._service.AddPackage(pkgName);
        }

        public string BossGateway(string cmd)
        {
            return this._service.BossGateway(cmd);
        }

        public string CreateAccount(string acc)
        {
            return this._service.CreateAccount(acc);

        }

        public string GetAccountbyDeviceExternalId(string dev)
        {
            return this._service.GetAccountbyDeviceExternalId(dev);
        }

        public string GetDupSG(string accPlusSG)
        {
            return this._service.GetDupSG(accPlusSG);

        }

        public string GetSSLBasicPackage()
        {
            return this._service.GetSSLBasicPackage();
        }

        public string QuanTuen(string acc)
        {
            return this._service.QuanTuen(acc);

        }

        public string[] ReadAccountSubscriberGroups(string acc)
        {
            return this._service.ReadAccountSubscriberGroups(acc);
        }

        public string RemovePackage(string pkgName)
        {
            return this._service.RemovePackage(pkgName);
        }

        public string XMLsgMapping2coss(string id)
        {
            return this._service.XMLsgMapping2coss(id);
        }

        public string devCluster(string cm)
        {
            return this._service.devCluster(cm);
        }

        public string getMSWDSeq()
        {
            return this._service.getMSWDSeq();
        }

        public string log_GetDeviceSG(string dev)
        {
            return this._service.log_GetDeviceSG(dev);
        }

        public void log_GetUser(string acc)
        {
             this._service.log_GetDeviceSG(acc);
        }

        public void log_getAccDetails(string acc)
        {
            this._service.log_getAccDetails(acc);
        }

        public string removeUser(string acc)
        {
            return this._service.removeUser(acc);
        }

        public string setMaster(string acc)
        {
            return this._service.setMaster(acc);
        }

        public string setUser(string acc)
        {
            return this._service.setUser(acc);
        }

        public string sgMapping(string id)
        {
            return this._service.sgMapping(id);
        }

        public string sgToDevice(string pkgName)
        {
            return this._service.sgToDevice(pkgName);
        }

        public string stbToPurchase(string accREC)
        {
            return this._service.stbToPurchase(accREC);
        }

        public string ynDev(string rc)
        {
            return this._service.ynDev(rc);

        }
        public void Dispose()
        {
            if (this != null)
            {
                GC.SuppressFinalize(this);
            }
        }



        public string Url
        {
            get
            {
                
                return this._service.Url;
            }
            set
            {
                this._service.Url = value;
            }
        }

        public NetworkCredential Credential
        {
            get
            {
                return this._service.Credentials as NetworkCredential;
            }
            set
            {
                 this._service.Credentials = value;
            }
        }
    }
}
