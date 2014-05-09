using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using General.Service.OssDiagnostics;

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
       public long ReadStartingDataForDetailedDiagnostics(string deviceId,DateTime time,ref Guid guid, ref string url)
       {
           return this._diagnosticsWS.ReadStartingDataForDetailedDiagnostics(deviceId, time,ref guid,ref url);
       }
       public ClientEvents[] ReadDetailedDiagnosticsData(Guid deviceExternalId,string url,long startNum,uint numEvents)
       {
           return this._diagnosticsWS.ReadDetailedDiagnosticsData(deviceExternalId, url, startNum, numEvents);
       }
       public void UpdateDetailedDiagnosticsState(string deviceExternalId,bool enable)
       {
           this._diagnosticsWS.UpdateDetailedDiagnosticsState(deviceExternalId,enable);
       }
       public void UpdateDetailedDiagnosticsStateByDate(string deviceId,DateTime until)
       {
           this._diagnosticsWS.UpdateDetailedDiagnosticsStateByDate(deviceId, until);
       }
       public string ReadClientFilters(string deviceId)
       {
           return this._diagnosticsWS.ReadClientFilters(deviceId);
       }
       public void UpdateClientFilters(string deviceId, ClientFilter[] filters)
       {
           this._diagnosticsWS.UpdateClientFilters(deviceId, filters);
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


       public IServiceConfiguration Config
       {
           get { return this._config; }
       }
    }
}
