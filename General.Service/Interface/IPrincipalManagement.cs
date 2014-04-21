using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using General.Service.BSS;

namespace General.Service
{
    public interface IPrincipalManagement : IDisposable, IServiceCommon
    {
        /// <summary>
        /// 將Subscriber Groups 加入帳號
        /// </summary>
        /// <param name="accountExternalId"></param>
        /// <param name="groupExternalIds"></param>
        void AddGroupsToAccount(string accountExternalId, string[] groupExternalIds);
        /// <summary>
        /// 將Subscriber Groups 加入用戶類別(ex Device,Account)
        /// </summary>
        /// <param name="externalId"></param>
        /// <param name="groupExternalIds"></param>
        void AddGroupsToPrincipal(PrincipalIdentifier externalId, string[] groupExternalIds);
        /// <summary>
        /// 從用戶類別中取得Subscriber Groups  資料
        /// </summary>
        /// <param name="principalId"></param>
        /// <returns></returns>
        GroupMembership[] GetGroupMemberships(PrincipalIdentifier principalId);
        /// <summary>
        /// 從Device中取得Subscriber Groups  資料
        /// </summary>
        /// <param name="deviceid"></param>
        /// <returns></returns>
        string[] GetGroupMembershipsByDevice(string deviceid);
        /// <summary>
        /// 從Account中取得Subscriber Groups 資料
        /// </summary>
        /// <param name="accountid"></param>
        /// <returns></returns>
        string[] GetGroupMembershipsByAccount(string accountid);
        /// <summary>
        /// 取得Account資料
        /// </summary>
        /// <param name="externalId"></param>
        /// <returns></returns>
        Account[] ReadAccount(string externalId);
        /// <summary>
        /// 取得Device 中全部DeviceValue
        /// </summary>
        /// <param name="deviceExternalID"></param>
        /// <returns></returns>
        DeviceValue[] ReadAllDeviceValues(string deviceExternalID);
        DeviceValue ReadAllDeviceValues(string deviceExternalID, string ClientSettingKey);
        /// <summary>
        /// 取得Device 資料
        /// </summary>
        /// <param name="externalID"></param>
        /// <returns></returns>
        General.Service.BSS.Device ReadDevice(string externalID);
        /// <summary>
        /// 取得Device 資料
        /// </summary>
        /// <param name="externalID"></param>
        /// <returns></returns>
        BSS.Device[] ReadDevices(string externalID);
        /// <summary>
        /// 取得Device 資料(該方法不帶LastReportedVersion、RequestedVersion、ResolvedVersion屬性)
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        General.Service.BSS.Device ReadDeviceByGuid(Guid deviceID);
        /// <summary>
        /// 取得Account 資料(第一組)
        /// </summary>
        /// <param name="externalId"></param>
        /// <returns></returns>
        Account ReadFirstAccount(string externalId);
        /// <summary>
        /// 取得CreditLimit 值
        /// </summary>
        /// <param name="acc"></param>
        void UpdateAccountCreditLimit(Account acc);
        /// <summary>
        /// 更新DeviceValue
        /// </summary>
        /// <param name="deviceExternalId"></param>
        /// <param name="values"></param>
        void UpdateDeviceValues(string deviceExternalId, DeviceValue[] values);
        /// <summary>
        /// 更新DeviceValue 並通知Device
        /// </summary>
        /// <param name="deviceExternalId"></param>
        /// <param name="values"></param>
        void UpdateDeviceValuesAndNotify(string deviceExternalId, DeviceValue[] values);
        /// <summary>
        /// 更新 Device Version資料
        /// </summary>
        /// <param name="device"></param>
        void UpdateDeviceVersion(General.Service.BSS.Device device);
        /// <summary>
        /// 從用戶類別中移除Subscriber Groups 
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="groupIds"></param>
        void RemoveGroupMemberships(PrincipalIdentifier principalId, GroupPrincipalExternalId[] groupIds);
        /// <summary>
        /// 取得Device資料(第一組)
        /// </summary>
        /// <param name="externalId"></param>
        /// <returns></returns>
        Device ReadFirstDevice(string externalId);
        /// <summary>
        /// 更新CreditLimit
        /// </summary>
        /// <param name="ExternalID"></param>
        /// <param name="CreditLimit"></param>
        void UpdateDeviceCreditLimit(string ExternalID, int CreditLimit);
        /// <summary>
        /// 移除所有DeviceValue
        /// </summary>
        /// <param name="ExternalID"></param>
        void DeleteAllDeviceValue(string ExternalID);
        /// <summary>
        /// 更新用戶AccountValue
        /// </summary>
        /// <param name="accoundExternalid"></param>
        /// <param name="values"></param>
        void UpdateAccountValue(string accoundExternalid, AccountValue[] values);
        /// <summary>
        /// 讀取用戶所有AccountValue
        /// </summary>
        /// <param name="accoundExternalid"></param>
        AccountValue[] ReadAllAccountValues(string accoundExternalid);
        /// <summary>
        /// 加入用戶
        /// </summary>
        /// <param name="Account"></param>
        void CreateAccount(Account Account);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Guid"></param>
        /// <param name="oldpin"></param>
        /// <param name="newpin"></param>
        /// <param name="st"></param>
        /// <returns></returns>
        PinStatus UpdateDevicePinByType(Guid Guid, string oldpin, string newpin, short st);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="principalid"></param>
        /// <param name="groupids"></param>
        void AddGroupMemberships(PrincipalIdentifier principalid,GroupPrincipalExternalId[] groupids);

        SubscriberGroup[] ReadAccountGroups(Account account);
        SubscriberGroup[] ReadDeviceGroups(Device Device);
        void UpdateAccountUsers(Account Account, bool remove);
        User[] ReadUser(User user);
        void UpdateAccountDevices(Account Account, bool remove);
        void UpdateAccountStatus(Account Account);
        void SetGlobalValue(string key, string value);
        UserstoreNameValue[] GetGlobalValue(string key);
        void RemoveGlobalValue(string key);
        void RemoveDeviceValues(DevicePrincipalExternalId deviceExternalId ,string[] keyName);
        void RemoveDeviceValues(string deviceExternalId, string[] keyName);
        bool DeviceIsExist(string deviceExternalId);
        bool UserIsExist(string AccountExternalId);
        bool AccountIsExist(string AccountExternalId);
        void DeleteAccount(string externalId);
        /// <summary>
        /// Use Multi-thread improve performance for get principalids sg
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        Dictionary<T, GroupMembership[]> BatchGetGroupMemberships<T>(IList<T> collection) where T : PrincipalIdentifier,new();
        Dictionary<string, GroupMembership[]> BatchGetGroupMembershipsByString<T>(IList<T> collection) where T : PrincipalIdentifier, new();
    }
}
