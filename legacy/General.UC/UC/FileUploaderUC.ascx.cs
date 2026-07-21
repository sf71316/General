using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Text;
using System.Web.Script.Serialization;
using General.Entity;
using General;
using General.CC.FileUploader;

namespace General.UC
{
    public partial class FileUploaderUC : System.Web.UI.UserControl
    {
        #region Event   
           protected void Page_Init(object sender, EventArgs e)
           {
              this.Uploadify1.ReceivedData += new Uploadify.ReceivedJSONDataHandler(FileUploader1_ReceivedData);
           }

           void FileUploader1_ReceivedData(object sender, string JsonData)
           {
               FileEntity list = JsonData.JsonDeserialize<FileEntity>();
               
           }
           protected void Page_Load(object sender, EventArgs e)
           {
              
           }

        #endregion

          

     
    }
}