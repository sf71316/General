using General.Service.OssChannel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Service
{
    public class OssChannelWS:IOssChannel
    {
        IServiceConfiguration _config;
        ChannelManagement channel;
        public OssChannelWS(IServiceConfiguration config)
        {
            this._config = config;
        }
        public void SetGlobalSetting(GlobalSettingKey key, string value)
        {
            
            this.channel.SetGlobalSetting(key, value);
        }
        public string GetGlobalSetting(GlobalSettingKey key)
        {
            return this.channel.GetGlobalSetting(key);
        }
        public void Dispose()
        {
            this.channel.Dispose();
        }

        public void Initialize()
        {
            if (this.channel==null)
                this.channel = new ChannelManagement();
        }


        public IServiceConfiguration Config
        {
            get { return this._config; }
        }
    }
}
