using System;
using System.Web;
using System.Reflection;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Drawing;
using System.Text;

namespace General.CC.DatePicker
{
    /// <summary>
    /// Summary description for wwWebUtils.
    /// </summary>
    internal class wwWebUtils
    {
        /// <summary>
        /// Finds a Control recursively. Note finds the first match and exits
        /// </summary>
        /// <param name="ContainerCtl">The top level container to start searching from</param>
        /// <param name="IdToFind">The ID of the control to find</param>
        /// <returns></returns>
        public static Control FindControlRecursive(Control Root, string Id)
        {
            return FindControlRecursive(Root, Id, false);
        }

        /// <summary>
        /// Finds a Control recursively. Note finds the first match and exits
        /// </summary>
        /// <param name="ContainerCtl">The top level container to start searching from</param>
        /// <param name="IdToFind">The ID of the control to find</param>
        /// <param name="AlwaysUseFindControl">If true uses FindControl to check for hte primary Id which is slower, but finds dynamically generated control ids inside of INamingContainers</param>
        /// <returns></returns>
        public static Control FindControlRecursive(Control Root, string Id, bool AlwaysUseFindControl)
        {
            if (AlwaysUseFindControl)
            {
                Control ctl = Root.FindControl(Id);
                if (ctl != null)
                    return ctl;
            }
            else
            {
                if (Root.ID == Id)
                    return Root;
            }

            foreach (Control Ctl in Root.Controls)
            {
                Control FoundCtl = FindControlRecursive(Ctl, Id, AlwaysUseFindControl);
                if (FoundCtl != null)
                    return FoundCtl;
            }

            return null;
        }

        /// <summary>
        /// Returns a fully qualified HTTP path from a partial path starting out with a ~.
        /// Same syntax that ASP.Net internally supports but this method can be used
        /// outside of the Page framework
        /// </summary>
        /// <param name="RelativePath"></param>
        /// <returns></returns>
        public static string ResolveUrl(string RelativePath)
        {
            if (RelativePath == null)
                return null;

            if (RelativePath.IndexOf(":") != -1)
                return RelativePath;

            // *** Fix up image path for ~ root app dir directory
            if (RelativePath.StartsWith("~"))
            {
                if (HttpContext.Current != null)
                    return HttpContext.Current.Request.ApplicationPath + RelativePath.Substring(1);
                else
                    // *** Assume current directory is the base directory
                    return RelativePath.Substring(1);
            }

            return RelativePath;
        }


        /// <summary>
        /// Returns a resource string. Shortcut for HttpContext.GetGlobalResourceObject.
        /// </summary>
        /// <param name="ClassKey"></param>
        /// <param name="ResourceKey"></param>
        /// <returns></returns>
        public static string GRes(string ClassKey, string ResourceKey)
        {
            string Value = HttpContext.GetGlobalResourceObject(ClassKey, ResourceKey) as string;
            if (string.IsNullOrEmpty(Value))
                return ResourceKey;

            return Value;
        }
        /// <summary>
        /// Returns a resource string. Shortcut for HttpContext.GetGlobalResourceObject.
        /// 
        /// Defaults to "Resources" as the ResourceSet (ie. Resources.xx.resx)
        /// </summary>
        /// <param name="ResourceKey"></param>
        /// <returns></returns>
        public static string GRes(string ResourceKey)
        {
            return GRes("Resources", ResourceKey);
        }

        /// <summary>
        /// Returns a JavaScript Encoded string from a Global Resource
        /// </summary>
        /// <param name="ClassKey"></param>
        /// <param name="ResourceKey"></param>
        /// <returns></returns>
        public static string GResJs(string ClassKey, string ResourceKey)
        {
            string Value = GRes(ClassKey, ResourceKey) as string;
            return EncodeJsString(Value);
        }

        /// <summary>
        /// Returns a JavaScript Encoded string from a Global Resource
        /// Defaults to the "Resources" resource set.
        /// </summary>
        /// <param name="ResourceKey"></param>
        /// <returns></returns>
        public static string GResJs(string ResourceKey)
        {
            return GResJs("Resources", ResourceKey);
        }

        /// <summary>
        /// Encodes a string to be represented as a string literal. The format
        /// is essentially a JSON string.
        /// 
        /// The string returned includes outer quotes 
        /// Example Output: "Hello \"Rick\"!\r\nRock on"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string EncodeJsString(string s)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\"");
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");

            return sb.ToString();
        }

        /// <summary>
        /// Converts a date to a JavaScript date string in UTC format
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string EncodeJsDate(DateTime date)
        {
            return "new Date(\"" + date.ToString("yyyy/MM/dd HH:mm:ss") + " UTC" + "\")";
        }


        ///// <summary>
        ///// This method returns a fully qualified server Url which includes
        ///// the protocol, server, port in addition to the server relative Url.
        ///// 
        ///// It work like Page.ResolveUrl, but adds these to the beginning.
        ///// This method is useful for generating Urls for AJAX methods
        ///// </summary>
        ///// <param name="ServerUrl">Any Url, either App relative or fully qualified</param>
        ///// <param name="ServerControl">Required Page or Server Control so this method can call ResolveUrl.
        ///// This parameter should be of the current form or a control on the form.</param>
        ///// <returns></returns>
        //public static string ResolveServerUrl(string ServerUrl)
        //{
        //    if (ServerUrl.ToLower().StartsWith("http"))
        //        return ServerUrl;

        //    // *** Start by fixing up the Url an Application relative Url
        //    string Url = ResolveUrl(ServerUrl);

        //    Uri ExistingUrl = HttpContext.Current.Request.Url;
        //    Url = ExistingUrl.Scheme + "://" + ExistingUrl.Authority + Url;

        //    return Url;
        //}

        ///// <summary>
        ///// Returns the executing ASPX, ASCX, MASTER page for a control instance.
        ///// Path is returned app relative without a leading slash
        ///// </summary>
        ///// <param name="Ctl"></param>
        ///// <returns></returns>
        //public static string GetControlAppRelativePath(Control Ctl)
        //{
        //    return Ctl.TemplateControl.AppRelativeVirtualPath.Replace("~/", "");
        //}

    }
}
