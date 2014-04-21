using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using General.Service.OssDiagnostics;

namespace General.Service.MSTV.OSS
{
    public abstract class OssDiagnostics : IServiceConfiguration, General.Service.Services.MSTV.OSS.IOssDiagnostics
    {
        DiagnosticsNotificationsWSSoap _diagnostics;
        ossDiagnosticsNotificationsSoap _diagnostics2;
        public OssDiagnostics()
        {
            this._diagnostics = new DiagnosticsNotificationsWSSoap();
            this._diagnostics2 = new ossDiagnosticsNotificationsSoap();
        }
        public DeviceState ReadDeviceState(string ExternalID)
        {
            Device device = new Device();
            device.ExternalID = ExternalID;
            return this._diagnostics2.ReadDeviceState(device);
        }
        public void SendRebootRequestToDevice(RebootRequest request)
        {
            this._diagnostics.SendRebootRequestToDevice(request.DeviceExternalID,request.Message,
                request.Timeout,request.Reson,request.ForceReboot,request.Any);
        }
        public string Url
        {
            get { return this._diagnostics.Url; }
            set { this._diagnostics.Url =this._diagnostics2.Url= value; }
        }

        public NetworkCredential Credential
        {
            get { return this._diagnostics.Credentials as NetworkCredential; }
            set { this._diagnostics.Credentials =this._diagnostics2.Credentials= value; }
        }

        public void Dispose()
        {
            if (this != null)
            {
                this._diagnostics.Dispose();
                this._diagnostics2.Dispose();
            }
        }
    }
}
