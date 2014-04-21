using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using General.Service.PrincipalManagement;
using System.Net;

namespace General.Service
{
    public abstract class PrincipalManagementBase : IService,IPrincipalManagement
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
        public DeviceValue[] ReadAllDeviceValues(string deviceExternalID)
        {
            return this._service2.ReadAllDeviceValues(deviceExternalID);
        }
        public GroupMembership[] GetGroupMemberships(PrincipalIdentifier principalId)
        {
            return this._service.GetGroupMemberships(principalId);
        }
        public   void Dispose()
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


       
    }
}
