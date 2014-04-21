using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using General.CC;
using System.Web.UI.HtmlControls;
using System.IO;
using General.Entity;
using General;

namespace General.UC.Handler.FileUploader
{
    public class GeneralUploader:General.CC.FileUploader.UploadHandlerBase
    {
        public GeneralUploader()
        {
            this.SavePath = "~/Attach/Temp/";
        }
        public override void ProcessRequest(HttpContext context)
        {
            FileEntity e = new FileEntity();
            if (context.Request.Files.Count > 0)
            {
               
                for (int i = 0; i < context.Request.Files.Count; i++)
		    	{
                    HttpPostedFile file = context.Request.Files[i];
                    Guid PK= Guid.NewGuid();
                    string filename =string.Format("{0}{1}",PK,
                                                                        Path.GetExtension(file.FileName));
                    e.FileName = file.FileName;
                    e.FileGrid = PK;
                    e.FilePath = string.Format("{0}{1}", this.SavePath, filename);
                    e.NewFileName = filename;
                    file.SaveAs(context.Server.MapPath(e.FilePath));
                    
                }
            }

          context.Response.Write(e.JsonSerialize());
        }
    }
}