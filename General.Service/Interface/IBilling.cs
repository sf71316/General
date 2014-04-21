using General.Service.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Service
{
    public interface IBilling : IDisposable, IServiceCommon
    {
        void DeleteBillingRecord(BillingRecord record,DateTime startdate,DateTime enddate);
        BillingRecord[] ReadBillingRecord(BillingRecord record, DateTime startdate, DateTime enddate);
        void UpdateBillingRecordStatus(BillingRecord record, DateTime startdate, DateTime enddate);
    }
}
