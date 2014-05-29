﻿using System;
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

        long ReadStartingDataForDetailedDiagnostics(string deviceId, DateTime time, ref Guid guid, ref string url);

        ClientEvents[] ReadDetailedDiagnosticsData(Guid deviceExternalId, string url, long startNum, uint numEvents);

        void UpdateDetailedDiagnosticsState(string deviceExternalId, bool enable);

        void UpdateDetailedDiagnosticsStateByDate(string deviceId, DateTime until);
        string ReadClientFilters(string deviceId);

        void UpdateClientFilters(string deviceId, ClientFilter[] filters);
        void SendDataRefreshNotification(string[] groupExternalIds);
    }
}
