using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;
using General.Service.BSS;
using General.Service.COSS;
using General.Service.OssDiagnostics;

namespace General.Service
{
    internal sealed class DefaultAdapter : MSTVService
    {
        public DefaultAdapter()
        {
            ServiceConfigure config = new ServiceConfigure();
            this._bss = new PrincipalManagementWS(config.PrincipalManagementConfigure);
            this._coss=new CossWS(config.CossConfigure);
            this._notification=new NotificationProxyWS(config.NotificationConfigure);
            this._ossDiagnostics = new OssDiagnosticsWS(config.OssDiagnosticsConfigure);
            this._ossconfiguration = new OssConfigurationWS(config.OssConfigConfigure);
            this._osschannel = new OssChannelWS(config.OssChannelConfigure);
            this._ossepg = new OssEpgWS(config.OssEpgConfigure);
            this._billing = new BillingWS(config.BillingConfigure);
            this._uinotifications = new OssUINotification(config.OssUInotificationConfigure);
        }

    }
}
