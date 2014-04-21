using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO.Compression;
using System.IO;
using System.Reflection;
using System.Web.Caching;
using System.Text.RegularExpressions;


namespace General.CC.DatePicker
{
    
    /// <summary>
    /// Module that handles compression of JavaScript resources using
    /// GZip and some basic code optimizations that strips full line
    /// comments and whitespace from the beginning and end of lines.
    /// 
    /// This module should be used in conjunction with
    /// ClientScriptProxy.RegisterClientScriptResource which sets
    /// up the proper URL formatting required for this module to
    /// handle requests. Format is:
    /// 
    /// wwScriptCompression.ashx?r=ResourceName&t=FullAssemblyName
    /// 
    /// The type parameter can be omitted if the resource lives
    /// in this assembly.
    /// <remarks> 
    /// * JS resources should have \r\n breaks
    /// </remarks>
    /// </summary>
    public class wwScriptCompressionModule : IHttpModule
    {
    /// <summary>
    /// Global flag that is set when the module is first loaded
    /// and allows code to check whether the module is loaded.
    /// </summary>
        public static bool wwScriptCompressionModuleActive = false;

        public void Init(HttpApplication context)
        {
            wwScriptCompressionModuleActive = true;
            context.PostResolveRequestCache += new EventHandler(this.PostResolveRequestCache);
        }
        public void Dispose()
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PostResolveRequestCache(object sender, EventArgs e)
        {
            HttpContext Context = HttpContext.Current;
            HttpRequest Request = Context.Request;

            // *** Skip over anything we don't care about immediately
            if (!Request.Url.LocalPath.ToLower().Contains("wwsc.axd"))
                return;

            HttpResponse Response = Context.Response;
            string AcceptEncoding = Request.Headers["Accept-Encoding"];

            // *** Start by checking whether GZip is supported by client
            bool UseGZip = false;
            if (!string.IsNullOrEmpty(AcceptEncoding) && 
                AcceptEncoding.ToLower().IndexOf("gzip") > -1 )
                UseGZip = true;

            // *** Create a cachekey and check whether it exists
            string CacheKey = Request.QueryString.ToString() + UseGZip.ToString();
            
            byte[] Output = Context.Cache[CacheKey] as byte[];
            if (Output != null)
            {
                // *** Yup - read cache and send to client
                SendOutput(Output, UseGZip);
                return;
            }

            // *** Retrieve information about resource embedded
            // *** Values are base64 encoded
            string ResourceTypeName = Request.QueryString["t"];
            if (!string.IsNullOrEmpty(ResourceTypeName))
                ResourceTypeName = Encoding.ASCII.GetString(Convert.FromBase64String(ResourceTypeName));

            string Resource = Request.QueryString["r"];
            if (string.IsNullOrEmpty(Resource))
            {
                SendErrorResponse("Invalid Resource");
                return;
            }
            Resource = Encoding.ASCII.GetString(Convert.FromBase64String(Resource));
            
            // *** Try to locate the assembly that houses the Resource
            Assembly ResourceAssembly = null;
 
            // *** If no type is passed use the current assembly - otherwise
            // *** run through the loaded assemblies and try to find assembly
            if (string.IsNullOrEmpty(ResourceTypeName))
                ResourceAssembly = this.GetType().Assembly;
            else
            {
                ResourceAssembly = this.FindAssembly(ResourceTypeName);
                if (ResourceAssembly == null)
                {
                    SendErrorResponse("Invalid Type Information");
                    return;
                }
            }

            // *** Load the script file as a string from Resources
            string Script = "";
            using (Stream st = ResourceAssembly.GetManifestResourceStream(Resource))
            {                
                StreamReader sr = new StreamReader(st);
                Script = sr.ReadToEnd();
            }

            // *** Optimize the script by removing comment lines and stripping spaces
            if (!Context.IsDebuggingEnabled)            
                Script = OptimizeScript(Script);                
                        
            // *** Now we're ready to create out output
            // *** Don't GZip unless at least 8k
            if (UseGZip && Script.Length > 8092)
                Output = GZipMemory(Script);
            else
            {
                Output = Encoding.ASCII.GetBytes(Script);
                UseGZip = false;
            }

            // *** Add into the cache
            Context.Cache.Add(CacheKey, Output, null, DateTime.UtcNow.AddDays(1), TimeSpan.Zero, CacheItemPriority.High,null);
            
            // *** Write out to Response object with appropriate Client Cache settings
            this.SendOutput(Output, UseGZip);
        }

        
        /// <summary>
        /// Returns an error response to the client. Generates a 404 error
        /// </summary>
        /// <param name="Message"></param>
        private void SendErrorResponse(string Message)
        {
            if (!string.IsNullOrEmpty(Message))
                Message = "Invalid Web Resource";

            HttpContext Context = HttpContext.Current;

            Context.Response.StatusCode = 404;
            Context.Response.StatusDescription = Message;
            Context.Response.End();
        }

        /// <summary>
        /// Sends the output to the client using appropriate cache settings.
        /// Content should be already encoded and ready to be sent as binary.
        /// </summary>
        /// <param name="Output"></param>
        /// <param name="UseGZip"></param>
        private void SendOutput(byte[] Output, bool UseGZip)
        {
            HttpResponse Response = HttpContext.Current.Response;

            Response.ContentType = "application/x-javascript";
            if (UseGZip)
                Response.AppendHeader("Content-Encoding", "gzip");

            if (!HttpContext.Current.IsDebuggingEnabled)
            {
                Response.ExpiresAbsolute = DateTime.UtcNow.AddYears(1);
                Response.Cache.SetLastModified(DateTime.UtcNow);
                Response.Cache.SetCacheability(HttpCacheability.Public);
            }

            Response.BinaryWrite(Output);
            Response.End();
        }


        /// <summary>
        /// Very basic script optimization to reduce size:
        /// Remove any leading white space and any lines starting
        /// with //. 
        /// </summary>
        /// <param name="Script"></param>
        /// <returns></returns>
        public static string OptimizeScript(string Script)
        {
            JavaScriptMinifier min = new JavaScriptMinifier();
            return min.MinifyString(Script);

            // *** Remove all instances of /* .... */ spanning multiple lines if necessary
            //string optimized = Regex.Replace(Script, @"/\*.*?\*/\s*\n*", "", RegexOptions.Singleline);
            
            //string optimized = Script;

            // *** Remove comments and whitespace, line-by-line:
            // "^\s*//.*$\n?" - Single-line comment, also look for leading whitespace and trailing newline
            // "^\s*$\n" - Blank line (with or without whitespace) and trailing newline
            // "^\s+" - Leading whitespace
            // "\s+$" - Trailing whitespace
            //return Regex.Replace(optimized, @"^\s*//.*$\n?|^\s*$\n|^\s+|\s+$", "", RegexOptions.Multiline);

#if (false)  // old code
            Script = Script.Replace("\r\n", "\n");
            string[] Lines = Script.Split(new string[1] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            foreach (string Line in Lines)
            {
                string LineContent = Line.Trim();

                // *** Remove full comment lines
                if (LineContent.StartsWith("//"))
                    continue;
                sb.Append(LineContent + "\n");
            }
            return sb.ToString();
#endif
        }



        /// <summary>
        /// Finds an assembly in the current loaded assembly list
        /// </summary>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        private Assembly FindAssembly(string TypeName)
        {
            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                string fn = ass.FullName;
                if (ass.FullName == TypeName)
                    return ass;
            }

            return null;
        }
        
        /// <summary>
        /// Takes a binary input buffer and GZip encodes the input
        /// </summary>
        /// <param name="Buffer"></param>
        /// <returns></returns>
        public static byte[] GZipMemory(byte[] Buffer)
        {
            MemoryStream ms = new MemoryStream();

            GZipStream GZip = new GZipStream(ms, CompressionMode.Compress);

            GZip.Write(Buffer, 0, Buffer.Length);
            GZip.Close();

            byte[] Result = ms.ToArray();
            ms.Close();

            return Result;
        }


        /// <summary>
        /// Takes a string input and GZip encodes the input
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static byte[] GZipMemory(string Input)
        {
            return GZipMemory(Encoding.ASCII.GetBytes(Input));
        }
        
        //public static byte[] GZipStream(Stream stream)
        //{
        //    MemoryStream ms = new MemoryStream();

        //    GZipStream GZip = new GZipStream(ms, CompressionMode.Compress);
            
        //    int LastBytes = -1;
        //    while (LastBytes != 0)
        //    {
        //        LastBytes = stream.Read(
        //    }

        //    GZip.Write(Buffer, 0, Buffer.Length);
        //    GZip.Close();

        //    byte[] Result = ms.ToArray();
        //    ms.Close();

        //    return Result;
        //}


    }
}
