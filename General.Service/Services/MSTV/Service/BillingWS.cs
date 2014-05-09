using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using General.Service.Billing;

namespace General.Service
{
    public class BillingWS:IBilling
    {
        BillingRecordManagement management;
        IServiceConfiguration _config;
        public BillingWS(IServiceConfiguration Config)
        {
            this._config = Config;
        }
        public void DeleteBillingRecord(BillingRecord record, DateTime startdate, DateTime enddate)
        {
            this.management.DeleteBillingRecord(record, startdate, enddate);
        }
        public BillingRecord[] ReadBillingRecord(BillingRecord record, DateTime startdate, DateTime enddate)
        {
            return  this.management.ReadBillingRecord(record, startdate, enddate);
        }

        public void UpdateBillingRecordStatus(BillingRecord record, DateTime startdate, DateTime enddate)
        {
            this.management.UpdateBillingRecordStatus(record, startdate, enddate);
        }
        public void Dispose()
        {
            management.Dispose();
        }

        public void Initialize()
        {
            if (this.management == null )
            {
                this.management = new BillingRecordManagement();
                this.management.Credentials = this._config.Credential;
                this.management.Url = this._config.Url;
            }
        }



        public IServiceConfiguration Config
        {
            get { return this._config; }
        }
    }
}
