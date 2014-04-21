using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Service
{
    public class OssEpgWS:IOssEpg
    {
        private OssEpg.OssEpgWebService service;
        IServiceConfiguration _config;
        public OssEpgWS(IServiceConfiguration Config)
        {
            this._config = Config;
        }
        public string ReadEPGByDateRange(DateTime starttime, DateTime endtime)
        {
            return System.Text.Encoding.Default.GetString(
                this.service.ReadEPGByDateRange(starttime, endtime));
        }

        public void Dispose()
        {
            if (this.service != null)
                this.service.Dispose();
        }

        public void Initialize()
        {
            if (this.service == null)
            {
                this.service = new OssEpg.OssEpgWebService();
                this.service.Credentials = this._config.Credential;
                this.service.Url = this._config.Url;
            }
        }
    }
}
