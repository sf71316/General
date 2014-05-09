using General.Service.OssUINotifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Service
{
    public class OssUINotification:IUINotifications
    {
        UINotificationsWS _service;
        IServiceConfiguration _config;
        public OssUINotification(IServiceConfiguration config)
        {
            this._config = Config;
        }

        public void LaunchClientApplication(ExternalId externalId, string applicationUrl)
        {
            this._service.LaunchClientApplication(externalId, applicationUrl);
        }
        public void Dispose()
        {
            this._service.Dispose();
        }

        public void Initialize()
        {
            if (this._service == null)
            {
                this._service = new UINotificationsWS();
                this._service.Url = this.Config.Url;
                this._service.Credentials = this.Config.Credential;
            }
        }

        public IServiceConfiguration Config
        {
            get { return this._config; }
        }

       
    }
}
