using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.IO;

namespace General.CC.FileUploader
{
    public abstract class UploadHandlerBase : IHttpHandler, IRequiresSessionState
    {
        public virtual bool IsReusable
        {
            get { return true; }
        }
        public string SavePath { get; set; }
        private void Init(HttpContext context)
        {
            if (!Directory.Exists(context.Server.MapPath(this.SavePath)))
            {
                Directory.CreateDirectory(context.Server.MapPath(this.SavePath));
            }
        }
        public virtual void ProcessRequest(HttpContext context)
        {
            Init(context);
        }
    }
}
