using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using General.Service.OssDiagnostics;
using General.Service.OssUINotifications;

namespace General.Service
{
   public sealed class OssDiagnosticsWS:IOssDiagnostics
    {
       DiagnosticsNotificationsWSSoap _diagnosticsWS=null;
       ossDiagnosticsNotificationsSoap _diagnosticsWS2 = null;
       IServiceConfiguration _config;
       public OssDiagnosticsWS(IServiceConfiguration Config)
       {
           this._config = Config;
       }
       #region OssDiagnostics

       public DeviceState ReadDeviceState(string ExternalID)
       {
           
           OssDiagnostics.Device device =
               new OssDiagnostics.Device();
           device.ExternalID = ExternalID;
           return this._diagnosticsWS2.ReadDeviceState(device);
       }
       public void SendRebootRequestToDevice(RebootRequest request)
       {
           this._diagnosticsWS.SendRebootRequestToDevice(request.DeviceExternalID, request.Message,
               request.Timeout, request.Reson, request.ForceReboot, request.Any);
       }
       public void SendDiagnosticRequestToDevice(string deviceid,string callbackurl)
       {
           
           this._diagnosticsWS.SendDiagnosticRequestToDevice(deviceid, callbackurl);
       }
       #endregion

       public void Dispose()
       {
           if(this._diagnosticsWS!=null)
           this._diagnosticsWS.Dispose();
           if (this._diagnosticsWS2 != null)
           this._diagnosticsWS2.Dispose();
       }

       public void Initialize()
       {
           if (this._diagnosticsWS == null && this._diagnosticsWS2 == null )
           {
               this._diagnosticsWS = new DiagnosticsNotificationsWSSoap();
               this._diagnosticsWS2 = new ossDiagnosticsNotificationsSoap();
             this._diagnosticsWS.Url = this._diagnosticsWS2.Url = this._config.Url;
              this._diagnosticsWS.Credentials = 
                   this._diagnosticsWS2.Credentials = this._config.Credential;
           }
       }
    }
}
