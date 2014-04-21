using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using General.Service.OssDiagnostics;

namespace General.Service
{
    public interface IOssDiagnostics : IDisposable, IServiceCommon
    {
        /// <summary>
        /// 讀取Device 使用狀態
        /// </summary>
        /// <param name="ExternalID"></param>
        /// <returns></returns>
        DeviceState ReadDeviceState(string ExternalID);
        /// <summary>
        /// 發出重新開機要求給Device
        /// </summary>
        /// <param name="request"></param>
        void SendRebootRequestToDevice(RebootRequest request);
        /// <summary>
        /// 發出診斷報告要求給Device
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="callbackurl"></param>
        void SendDiagnosticRequestToDevice(string deviceid, string callbackurl);
    }
}
