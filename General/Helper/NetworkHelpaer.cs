using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Web;

namespace General
{
    public static class NetworkHelpaer
    {
        /// <summary>
        /// Get Client IP
        /// </summary>
        /// <returns></returns>
        public static string ClientIP()
        {
            HttpContext context = HttpContext.Current;
            string _ip=context.Request.ServerVariables["REMOTE_ADDR"].ToString();
            if (string.IsNullOrEmpty(_ip))
            {
                if (context.Request.ServerVariables["HTTP_VIA"] != null)
                {
                    _ip = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
                }
            }
            return _ip;
        }
        /// <summary>
        /// This  Method run by winfrom
        /// </summary>
        /// <returns></returns>
        public static string[] GetPhysicalAddresses()
        {
            var result = new List<string>();
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in interfaces)
            {
                PhysicalAddress address = adapter.GetPhysicalAddress();
                byte[] bytes = address.GetAddressBytes();
                if (bytes.Length == 0) continue;

                var mac = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    mac.Append(bytes[i].ToString("X2"));
                    if (i != bytes.Length - 1) mac.Append("-");
                }

                result.Add(mac.ToString());
            }
            return result.ToArray();
        }
    }
}
