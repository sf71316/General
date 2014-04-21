using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using General.Service.OssConfiguration;


namespace General.Service
{
    public interface IOssConfiguration : IDisposable, IServiceCommon
    {
        /// <summary>
        /// 啟動群組更新
        /// </summary>
        /// <param name="groupExternalId"></param>
        void StartGroupUpgrade(ExternalId groupExternalId);
        /// <summary>
        /// 停止群組更新
        /// </summary>
        /// <param name="groupExternalId"></param>
        void StopGroupUpgrade(ExternalId groupExternalId);
    }
}
