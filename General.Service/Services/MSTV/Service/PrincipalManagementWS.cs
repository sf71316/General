using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;
using General.Service.BSS;
using System.Threading;

namespace General.Service
{
    public sealed class PrincipalManagementWS:IPrincipalManagement
    {

        PrincipalManagementInterfaceSoap _bssWS = null;
        PrincipalManagementSoap _bssWS2 = null;
         IServiceConfiguration _config;
        public PrincipalManagementWS(IServiceConfiguration Config)
        {
            this._config = Config;
        }
        #region BSS
        public BSS.Device ReadDevice(string externalID)
        {
            
            BSS.Device[] array = this._bssWS2.ReadDevice(externalID);
            if (array != null && array.Length > 0)
            {
                return array[0];
            }
            return null;
        }
        public BSS.Device[] ReadDevices(string externalID)
        {
            return  this._bssWS2.ReadDevice(externalID);
        }
        public BSS.Device ReadDeviceByGuid(Guid deviceID)
        {
            
            return this._bssWS2.ReadDeviceByGuid(deviceID);
        }

        public void UpdateDeviceVersion(BSS.Device device)
        {
            
            this._bssWS2.UpdateDeviceVersion(device);
        }
        public void UpdateAccountValue(string accoundExternalid, AccountValue[] values)
        {
            this._bssWS2.UpdateAccountValues(accoundExternalid, values);
        }
        public AccountValue[] ReadAllAccountValues(string accoundExternalid)
        {
          return   this._bssWS2.ReadAllAccountValues(accoundExternalid);
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
                throw ex;
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
            this._bssWS.AddGroupMemberships(externalId, array);
        }

        public void RemoveGroupMemberships(PrincipalIdentifier principalId, GroupPrincipalExternalId[] groupIds)
        {
            this._bssWS.RemoveGroupMemberships(principalId, groupIds);
        }
        public DeviceValue[] ReadAllDeviceValues(string deviceExternalID)
        {
            return this._bssWS2.ReadAllDeviceValues(deviceExternalID);
        }
        public DeviceValue ReadAllDeviceValues(string deviceExternalID, string ClientSettingKey)
        {
            DeviceValue[] all = this.ReadAllDeviceValues(deviceExternalID);
            var value = all.Where(p => p.Key.ToLower() == ClientSettingKey.ToLower());
            if (value.Count() > 0)
            {
                return value.ElementAt<DeviceValue>(0);
            }
            else
            {
                return null;
            }
        }
        public GroupMembership[] GetGroupMemberships(PrincipalIdentifier principalId)
        {
            return this._bssWS.GetGroupMemberships(principalId);
        }
        public Dictionary<T, GroupMembership[]> BatchGetGroupMemberships<T>(IList<T> collection) where T:PrincipalIdentifier,new()
        {
            SubscriberGroupThreadPool<T> pool = new SubscriberGroupThreadPool<T>(this);

            foreach (var item in collection)
            {
                pool.Execute(item);
            }
            pool.WaitAllDone();
            return pool.Raw;
        }
        Dictionary<string, GroupMembership[]> IPrincipalManagement.BatchGetGroupMembershipsByString<T>(IList<T> collection)
        {
            SubscriberGroupThreadPool<T> pool = new SubscriberGroupThreadPool<T>(this);

            foreach (var item in collection)
            {
                pool.Execute(item);
            }
            pool.WaitAllDone();
            return pool.Result;
        }
        public string[] GetGroupMembershipsByDevice(string deviceid)
        {
            DevicePrincipalExternalId identifier = new DevicePrincipalExternalId();
            identifier.Id = deviceid;
            GroupMembership[] groups = this._bssWS.GetGroupMemberships(identifier);
            if (groups != null)
            {
                return groups.Select(p => { return p.GroupExternalId.Id; }).ToArray();
            }
            return null;
        }
        public string[] GetGroupMembershipsByAccount(string accountid)
        {
            
            AccountPrincipalExternalId identifier = new AccountPrincipalExternalId();
            identifier.Id = accountid;
            GroupMembership[] groups = this._bssWS.GetGroupMemberships(identifier);
            if (groups != null)
            {
                return groups.Select(p => { return p.GroupExternalId.Id; }).ToArray();
            }
            return null;
        }
        public void DeleteAllDeviceValue(string ExternalID)
        {
            this._bssWS2.DeleteAllDeviceValues(ExternalID);
        }
        public Account[] ReadAccount(string externalId)
        {
            Account a = new Account();
            return this._bssWS2.ReadAccount(externalId);
        }
        public Account ReadFirstAccount(string externalId)
        {
            
            Account[] acc = this._bssWS2.ReadAccount(externalId);
            if (acc.Length > 0)
            {
                return acc[0];
            }
            else
                return null;
        }
        public Device ReadFirstDevice(string externalId)
        {
            Device[] device = this._bssWS2.ReadDevice(externalId);
            if (device.Length > 0)
            {
                return device[0];
            }
            else
                return null;
        }
        public void UpdateAccountCreditLimit(Account acc)
        {
            this._bssWS2.UpdateAccountCreditLimit(acc);
        }
        public void UpdateDeviceCreditLimit(string ExternalID,int CreditLimit)
        {
            Device device = this.ReadFirstDevice(ExternalID);
            Account acc = new Account();
            acc.CreditLimit = CreditLimit;
            acc.ExternalID = device.AccountExternalId;
            this._bssWS2.UpdateAccountCreditLimit(acc);
        }
        public void UpdateDeviceValues(string deviceExternalId, DeviceValue[] values)
        {
            this._bssWS2.UpdateDeviceValues(deviceExternalId, values);
        }
        public void UpdateDeviceValuesAndNotify(string deviceExternalId, DeviceValue[] values)
        {
            this._bssWS2.UpdateDeviceValuesAndNotify(deviceExternalId, values);
        }
        public void CreateAccount(Account Account)
        {
            this._bssWS2.CreateAccount(Account);
        }
        public PinStatus UpdateDevicePinByType(Guid Guid, string oldpin, string newpin, short st)
        {
           return this._bssWS2.UpdateDevicePinByType(Guid,oldpin,newpin,st);
        }
        public void AddGroupMemberships(PrincipalIdentifier principalid, GroupPrincipalExternalId[] groupids)
        {
            
            this._bssWS.AddGroupMemberships(principalid, groupids);
        }
        public SubscriberGroup[] ReadAccountGroups(Account account)
        {
            return this._bssWS2.ReadAccountGroups(account);
        }
        public void UpdateAccountUsers(Account Account, bool remove)
        {
            this._bssWS2.UpdateAccountUsers(Account, remove);
        }
        public User[] ReadUser(User user)
        {
            return this._bssWS2.ReadUser(user);
        }
        public void UpdateAccountDevices(Account Account, bool remove)
        {
            this._bssWS2.UpdateAccountDevices(Account, remove);
        }
        public void UpdateAccountStatus(Account Account)
        {
            this._bssWS2.UpdateAccountStatus(Account);
        }
        public SubscriberGroup[] ReadDeviceGroups(Device Device)
        {
            return this._bssWS2.ReadDeviceGroups(Device);
        }
        public void SetGlobalValue(string key, string value)
        {
            this._bssWS.SetGlobalValue(key, value);
        }
        public UserstoreNameValue[] GetGlobalValue(string key)
        {
           return this._bssWS.GetGlobalValue(key);
        }
        public void RemoveGlobalValue(string key)
        {
            this._bssWS.RemoveGlobalValue(key);
        }
        public void RemoveDeviceValues(DevicePrincipalExternalId deviceExternalId, string[] keyName)
        {
            this._bssWS.RemoveDeviceValues(deviceExternalId, keyName);
        }
        public void RemoveDeviceValues(string deviceExternalId, string[] keyName)
        {
            
            DevicePrincipalExternalId DeviceId = new DevicePrincipalExternalId();
            DeviceId.Id = deviceExternalId;
            this.RemoveDeviceValues(DeviceId, keyName);
        }
        public bool DeviceIsExist(string deviceExternalId)
        {
            try
            {
                return this._bssWS2.ReadDevice(deviceExternalId).Length > 0;
            }
            catch
            {
                return false;
            }

        }
        public bool UserIsExist(string AccountExternalId)
        {

            try
            {
                return this._bssWS2.ReadUser(new User { AccountExternalId = AccountExternalId }).Length > 0;
            }
            catch
            {
                return false;
            }
        }
        public bool AccountIsExist(string AccountExternalId)
        {

            try
            {
                return this._bssWS2.ReadAccount(AccountExternalId).Length > 0;
            }
            catch
            {
                return false;
            }
        }
        public void DeleteAccount(string externalId)
        {
            this._bssWS2.DeleteAccount(externalId);
        }
        #endregion

        public void Dispose()
        {
            if(this._bssWS!=null)
            this._bssWS.Dispose();
            if (this._bssWS2 != null)
            this._bssWS2.Dispose();
        }


        public void Initialize()
        {
            if (this._bssWS == null || this._bssWS2 == null)
            {
                this._bssWS = new PrincipalManagementInterfaceSoap();
                this._bssWS2 = new PrincipalManagementSoap();
                this._bssWS.Url = this._bssWS2.Url = this._config.Url;
                this._bssWS.Credentials = this._bssWS2.Credentials = this._config.Credential;
            }
        }

        #region  Private Method 

        #endregion







        
    }
}
