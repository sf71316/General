using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;

namespace General.Service
{
    public class ServiceConfigure
    {
        public NetworkCredential Credential { get; set; }
        public ServiceConfigure()
        {
           Credential= new NetworkCredential(ConfigurationManager.AppSettings["LoginAccount"],
                                                          ConfigurationManager.AppSettings["LoginPwd"],
                                                          ConfigurationManager.AppSettings["LoginDomain"]);
           this.PrincipalManagementConfigure = new ServiceConfiguration();
           this.CossConfigure = new ServiceConfiguration();
           this.NotificationConfigure = new ServiceConfiguration();
           this.OssConfigConfigure = new ServiceConfiguration();
           this.OssDiagnosticsConfigure = new ServiceConfiguration();
           this.OssChannelConfigure = new ServiceConfiguration();
            this.OssEpgConfigure = new ServiceConfiguration();
            this.OssUInotificationConfigure = new ServiceConfiguration();
            this.BillingConfigure = new ServiceConfiguration();
            

           this.PrincipalManagementConfigure.Credential = 
           this.CossConfigure.Credential = this.NotificationConfigure.Credential =
           this.OssDiagnosticsConfigure.Credential=this.OssConfigConfigure.Credential = 
           this.OssChannelConfigure.Credential= this.BillingConfigure.Credential=
           this.OssEpgConfigure.Credential = this.OssUInotificationConfigure.Credential= this.Credential;
            
           this.PrincipalManagementConfigure.Url = ConfigurationManager.AppSettings["BSSUrl"];
           this.OssConfigConfigure.Url = ConfigurationManager.AppSettings["OssConfigUrl"];
           this.NotificationConfigure.Url = ConfigurationManager.AppSettings["NotificationWSProxyUrl"];
           this.CossConfigure.Url = ConfigurationManager.AppSettings["MSTVService"];
           this.OssDiagnosticsConfigure.Url = ConfigurationManager.AppSettings["OSSDiagnosticsUrl"];
           this.OssChannelConfigure.Url = ConfigurationManager.AppSettings["OSSChannelUrl"];
           this.OssEpgConfigure.Url = ConfigurationManager.AppSettings["OssEpgUrl"];
           this.BillingConfigure.Url = ConfigurationManager.AppSettings["BSSBillingUrl"];
           this.OssUInotificationConfigure.Url = ConfigurationManager.AppSettings["OssUInotificationUrl"];
        }
        #region property
        public IServiceConfiguration PrincipalManagementConfigure { get; set; }
        public IServiceConfiguration OssConfigConfigure { get; set; }
        public IServiceConfiguration OssDiagnosticsConfigure { get; set; }
        public IServiceConfiguration CossConfigure { get; set; }
        public IServiceConfiguration NotificationConfigure { get; set; }
        public IServiceConfiguration OssChannelConfigure { get; set; }

        public IServiceConfiguration OssEpgConfigure { get; set; }
        public IServiceConfiguration BillingConfigure { get; set; }
        public IServiceConfiguration OssUInotificationConfigure { get; set; }
        #endregion

        internal sealed class ServiceConfiguration : IServiceConfiguration
        {
            public string Url { get; set; }
            public NetworkCredential Credential { get; set; }
        }
    }
}
