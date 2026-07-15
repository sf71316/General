using System;
using System.Collections.Generic;
using System.Text;

namespace General.Notify
{
    internal static class ConfigHelper
    {
        public static string SkypeNotificationAPIUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["SkypeNotificationAPIUrl"];
            }
        }
    }
}
