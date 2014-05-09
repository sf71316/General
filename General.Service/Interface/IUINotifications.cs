
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace General.Service
{
    public interface IUINotifications : IDisposable, IServiceCommon
    {
        void LaunchClientApplication(General.Service.OssUINotifications.ExternalId externalId, string applicationUrl);
    }
}
