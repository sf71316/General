using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.IO;
using System.Reflection;

[assembly: TagPrefix("General.CC", "cc")]
[assembly: WebResource("General.CC.FileUploader.Resources.jquery.uploadify.js", "text/javascript")]
[assembly: WebResource("General.CC.FileUploader.Resources.uploadify.swf", "application/x-shockwave-flash")]
[assembly: WebResource("General.CC.FileUploader.Resources.uploadify.css", "text/css")]
namespace General.CC.FileUploader
{
    internal class ControlResources
    {
        public const string FILE_UPLOADER_JQUERY_RESOURCE_URL = "https://ajax.googleapis.com/ajax/libs/jquery/1.7.1/jquery.min.js";
        public const string FILE_UPLOADER_SCRIPT_RESOURCE = "General.CC.FileUploader.Resources.jquery.uploadify.js";
        public const string FILE_UPLOADER_CSS_RESOURCE = "General.CC.FileUploader.Resources.uploadify.css";
        public const string FILE_UPLOADER_SWF_RESOURCE = "General.CC.FileUploader.Resources.uploadify.swf";

        /// <summary>
        /// Loads the appropriate jScript library out of the scripts directory
        /// </summary>
        /// <param name="control"></param>
        public static void LoadjQuery(Control control, string jQueryUrl)
        {
            ClientScriptProxy p = ClientScriptProxy.Current;
            //p.RegisterClientScriptResource(control, typeof(ControlResources), ControlResources.PROTOTYPE_SCRIPT_RESOURCE);
            p.RegisterClientScriptInclude(control, typeof(ControlResources), "jQuery_GOOGLE", ControlResources.FILE_UPLOADER_JQUERY_RESOURCE_URL);
            return;
        }

        /// <summary>
        /// Loads the jQuery component uniquely into the page
        /// </summary>
        /// <param name="control"></param>
        /// <param name="jQueryUrl">Optional Url to the jQuery Library. NOTE: Should also have a .min version in place</param>
        public static void LoadjQuery(Control control)
        {
            LoadjQuery(control, null);
        }

        /// <summary>
        /// Simplified Helper function that is used to add script files to the page
        /// This version adds scripts to the top of the page in the 'normal' position
        /// immediately following the form tag.
        /// </summary>
        /// <param name="script"></param>
        public static void IncludeScriptFile(Control control, string scriptFile)
        {
            scriptFile = control.ResolveUrl(scriptFile);
            ClientScriptProxy.Current.RegisterClientScriptInclude(control, typeof(ControlResources),
                                          Path.GetFileName(scriptFile).ToLower(),
                                          scriptFile);
        }

        /// <summary>
        /// Simplified Helper function that is used to add script files to the page
        /// This version adds scripts to the bottom of the page just before the
        /// Form tag is ended. This ensures that other libraries have loaded
        /// earlier in the page.
        /// 
        /// This may be required for 'manually' adding script code that relies
        /// on other dependencies. For example a jQuery library that depends
        /// on jQuery or wwScriptLIbrary that is implicitly loaded.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="script"></param>
        public static void IncludeScriptFileBottom(Control control, string scriptFile)
        {
            scriptFile = control.ResolveUrl(scriptFile);
            ClientScriptProxy.Current.RegisterClientScriptBlock(control, typeof(ControlResources),
                            Path.GetFileName(scriptFile).ToLower(),
                            "<script src='" + scriptFile + "' type='text/javascript'></script>",
                            false);
        }


        /// <summary>
        /// Returns a string resource from a given assembly.
        /// </summary>
        /// <param name="assembly">Assembly reference (ie. typeof(ControlResources).Assembly) </param>
        /// <param name="ResourceName">Name of the resource to retrieve</param>
        /// <returns></returns>
        public static string GetStringResource(Assembly assembly, string ResourceName)
        {
            Stream st = assembly.GetManifestResourceStream(ResourceName);
            StreamReader sr = new StreamReader(st);
            string content = sr.ReadToEnd();
            st.Close();
            return content;
        }

        /// <summary>
        /// Returns a string resource from the from the ControlResources Assembly
        /// </summary>
        /// <param name="ResourceName"></param>
        /// <returns></returns>
        public static string GetStringResource(string ResourceName)
        {
            return GetStringResource(typeof(ControlResources).Assembly, ResourceName);
        }

    }
}

