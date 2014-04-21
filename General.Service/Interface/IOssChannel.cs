using General.Service.OssChannel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Service
{
    public interface IOssChannel:IDisposable, IServiceCommon
    {
        void SetGlobalSetting(GlobalSettingKey key, string value);
        string GetGlobalSetting(GlobalSettingKey key);
    }
}
