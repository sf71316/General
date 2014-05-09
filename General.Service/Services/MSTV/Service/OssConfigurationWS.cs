using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Service
{
    public sealed class OssConfigurationWS:IOssConfiguration
    {
        IServiceConfiguration _config;
        OssConfiguration.Configuration _ConfigWS = null;
        public OssConfigurationWS(IServiceConfiguration Config)
        {
            this._config = Config;
        }
        public void StartGroupUpgrade(OssConfiguration.ExternalId groupExternalId)
        {
            this._ConfigWS.StartGroupUpgrade(groupExternalId);
        }

        public void StopGroupUpgrade(OssConfiguration.ExternalId groupExternalId)
        {
            this._ConfigWS.StopGroupUpgrade(groupExternalId);
        }

        public void Dispose()
        {
            if (this._ConfigWS != null)
                this._ConfigWS.Dispose();
        }

        public void Initialize()
        {
            if (this._ConfigWS == null)
            {
                
                this._ConfigWS = new OssConfiguration.Configuration();
                this._ConfigWS.Url = _config.Url;
                this._ConfigWS.Credentials = _config.Credential;
            }
        }
        public IServiceConfiguration Config
        {
          get { return this._config; } 
        }
    }
}
