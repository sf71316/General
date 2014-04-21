using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using General.Service.BSS;
using System.Web.Services.Protocols;

namespace General.Service.MSTV.BSS
{
    public abstract class PrincipalManagementBase : IServiceConfiguration, General.Service.Services.MSTV.BSS.IPrincipalManagementBase
    {
        PrincipalManagementInterfaceSoap _service;
        PrincipalManagementSoap _service2;
        public PrincipalManagementBase()
        {
            this._service = new PrincipalManagementInterfaceSoap();
            this._service2 = new PrincipalManagementSoap();
        }
        public Device ReadDeviceByGuid(Guid deviceID)
        {
            return this._service2.ReadDeviceByGuid(deviceID);
        }
        public void AddGroupsToAccount(string accountExternalId, string[] groupExternalIds)
        {
            AccountPrincipalExternalId accountPrincipalExternalId = new AccountPrincipalExternalId();
            accountPrincipalExternalId.Id = accountExternalId;
            try
            {
                this.AddGroupsToPrincipal(accountPrincipalExternalId, groupExternalIds);
            }
            catch (SoapException ex)
            {
            //    if (ex.Message.Contains("Server did not recognize the value of HTTP Header SOAPAction"))
            //    {
            //        string[] accountExternalIds = new string[]
            //{
            //    accountExternalId
            //};
            //        for (int i = 0; i < groupExternalIds.Length; i++)
            //        {
            //            this.AddAccountsToGroup(groupExternalIds[i], accountExternalIds);
            //        }
            //    }
                throw;
            }
        }
        public void AddGroupsToPrincipal(PrincipalIdentifier externalId, string[] groupExternalIds)
        {
            GroupPrincipalExternalId[] array = new GroupPrincipalExternalId[groupExternalIds.Length];
            for (int i = 0; i < groupExternalIds.Length; i++)
            {
                array[i] = new GroupPrincipalExternalId();
                array[i].Id = groupExternalIds[i];
            }
            this._service.AddGroupMemberships(externalId, array);
        }

        public Device ReadDevice(string externalID)
        {
            Device[] array =this._service2.ReadDevice(externalID);
            if (array != null && array.Length > 0)
            {
                return array[0];
            }
            return null;
        }
        public DeviceValue[] ReadAllDeviceValues(string deviceExternalID)
        {
            return this._service2.ReadAllDeviceValues(deviceExternalID);
        }
        public GroupMembership[] GetGroupMemberships(PrincipalIdentifier principalId)
        {
            return this._service.GetGroupMemberships(principalId);
        }
        public Account[] ReadAccount(string externalId)
        {
            Account a = new Account();
            return this._service2.ReadAccount(externalId);
        }
        public Account ReadFirstAccount(string externalId)
        {
            Account[] acc=this._service2.ReadAccount(externalId);
            if (acc.Length > 0)
            {
                return acc[0];
            }
            else
                return null;
        }
        public void UpdateAccountCreditLimit(Account acc)
        {
            this._service2.UpdateAccountCreditLimit(acc);
        }
        public void UpdateDeviceVersion(Device device)
        {
            this._service2.UpdateDeviceVersion(device);
        }
        public void UpdateDeviceValue(string deviceExternalId,DeviceValue[] values)
        {
            this._service2.UpdateDeviceValues(deviceExternalId, values);
        }
        public void UpdateDeviceValuesAndNotify(string deviceExternalId,DeviceValue[] values)
        {
            this._service2.UpdateDeviceValuesAndNotify( deviceExternalId, values);
        }
        public  void Dispose()
        {
            if (this != null)
                this._service.Dispose();
        }
        public string Url {
            get { return this._service.Url; }
            set { this._service.Url = this._service2.Url = value; }
        }

        public NetworkCredential Credential
        {
            get { return this._service.Credentials as NetworkCredential; }
            set { this._service.Credentials = this._service2.Credentials = value; }
        }
        public PrincipalManagementInterfaceSoap Method
        {
            get
            {
                return this._service;
            }
        }
        public PrincipalManagementSoap Method2
        {
            get
            {
                return this._service2;
            }
        }
       
    }
}
