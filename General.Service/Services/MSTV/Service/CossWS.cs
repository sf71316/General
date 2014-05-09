using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using General.Service.COSS;

namespace General.Service
{
    public sealed class CossWS:ICossService
    {
        //Service1 _mstvWS = null;
        Service1 _mstvWS = null;
        IServiceConfiguration _config;
        public CossWS(IServiceConfiguration Config)
        {
            this._config = Config;
          
        }
        #region MSTV Subscribe
        public string AccountDetails(string acc)
        {
            return this._mstvWS.AccountDetails(acc);
        }

        public string AddPackage(string pkgName)
        {
            return this._mstvWS.AddPackage(pkgName);
        }

        public string BossGateway(string cmd)
        {
            return this._mstvWS.BossGateway(cmd);
        }

        public string CreateAccount(string acc)
        {
            return this._mstvWS.CreateAccount(acc);
        }

        public string GetAccountbyDeviceExternalId(string dev)
        {
            return this._mstvWS.GetAccountbyDeviceExternalId(dev);
        }

        public string GetDupSG(string accPlusSG)
        {
            return this._mstvWS.GetDupSG(accPlusSG);

        }

        public string GetSSLBasicPackage()
        {
            return this._mstvWS.GetSSLBasicPackage();
        }

        public string QuanTuen(string acc)
        {
            return this._mstvWS.QuanTuen(acc);

        }

        public string[] ReadAccountSubscriberGroups(string acc)
        {
            return this._mstvWS.ReadAccountSubscriberGroups(acc);
        }

        public string RemovePackage(string pkgName)
        {
            return this._mstvWS.RemovePackage(pkgName);
        }

        public string XMLsgMapping2coss(string id)
        {
            return this._mstvWS.XMLsgMapping2coss(id);
        }

        public string devCluster(string cm)
        {
            return this._mstvWS.devCluster(cm);
        }

        public string getMSWDSeq()
        {
            return this._mstvWS.getMSWDSeq();
        }

        public string log_GetDeviceSG(string dev)
        {
            return this._mstvWS.log_GetDeviceSG(dev);
        }

        public void log_GetUser(string acc)
        {
            this._mstvWS.log_GetDeviceSG(acc);
        }

        public void log_getAccDetails(string acc)
        {
            this._mstvWS.log_getAccDetails(acc);
        }

        public string removeUser(string acc)
        {
            return this._mstvWS.removeUser(acc);
        }

        public string setMaster(string acc)
        {
            return this._mstvWS.setMaster(acc);
        }

        public string setUser(string acc)
        {
            return this._mstvWS.setUser(acc);
        }

        public string sgMapping(string id)
        {
            return this._mstvWS.sgMapping(id);
        }

        public string sgToDevice(string pkgName)
        {
            return this._mstvWS.sgToDevice(pkgName);
        }

        public string stbToPurchase(string accREC)
        {
            return this._mstvWS.stbToPurchase(accREC);
        }

        public string ynDev(string rc)
        {
            return this._mstvWS.ynDev(rc);

        }

        #endregion

        public void Dispose()
        {
            if(this._mstvWS!=null)
            this._mstvWS.Dispose();
        }


        public void Initialize()
        {
            if (this._mstvWS == null)
            {
                //this._mstvWS = new Service1();
                this._mstvWS = new Service1();
                this._mstvWS.Url = this._config.Url;
                this._mstvWS.Credentials = this._config.Credential;
            }
        }


        public IServiceConfiguration Config
        {
            get { return this._config; }
        }
    }
}
