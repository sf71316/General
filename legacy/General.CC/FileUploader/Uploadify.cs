using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing.Design;
using System.Drawing;

namespace General.CC.FileUploader
{
   
    [ToolboxData("<{0}:Uploadify runat=server></{0}:Uploadify>")]
    [ToolboxBitmap(typeof(Uploadify), "FileUploader.Uploadify.png")]
    public class Uploadify : WebControl, ICallbackEventHandler
    {
        #region Property
        [Category("控制項引用資源")]
        [Description("")]
        [DefaultValue(""), Bindable(true), UrlProperty]
        [Editor("System.Web.UI.Design.UrlEditor, System.Design", typeof(UITypeEditor))]
        public string jQueryPath
        {
            get;
            set;
        }
        [Category("控制項引用資源")]
        [Description("")]
        [DefaultValue(""), Bindable(true), UrlProperty]
        [Editor("System.Web.UI.Design.UrlEditor, System.Design", typeof(UITypeEditor))]
        public bool LoadjQuery
        {
            get { return this._isloadjquery; }
            set { this._isloadjquery = value; }
        }
        private bool _isloadjquery = true;
        [Category("控制項引用資源")]
        [Description("")]
        [DefaultValue(""), Bindable(true), UrlProperty]
        [Editor("System.Web.UI.Design.UrlEditor, System.Design", typeof(UITypeEditor))]
        public string FileUploadJsPath
        {
            get;
            set;
        }
        [Category("控制項引用資源")]
        [Description("")]
        [DefaultValue(""), Bindable(true), UrlProperty]
        [Editor("System.Web.UI.Design.UrlEditor, System.Design", typeof(UITypeEditor))]
        public string CssPath
        {
            get;
            set;
        }
        [Category("控制項引用資源")]
        [Description("上傳處理程序*必填")]
        [DefaultValue(""), Bindable(true), UrlProperty]
        [Editor("System.Web.UI.Design.UrlEditor, System.Design", typeof(UITypeEditor))]
        public string UploadHandler
        {
            get;
            set;
        }

        [Category("控制項設定")]
        [Description("是否多檔選擇")]
        [DefaultValue(false), Bindable(true)]
        public bool MultiUpload
        {
            get { return this._ismultiupload; }
            set { this._ismultiupload = value; }
        }
        private bool _ismultiupload = false;
        [Category("控制項設定")]
        [Description("是否自動上傳")]
        [DefaultValue(true), Bindable(true)]
        public bool AutoUpload
        {
            get { return this._autoupload; }
            set { this._autoupload = value; }
        }
        private bool _autoupload = true;
        [Category("控制項設定")]
        [Description("按鈕文字")]
        [DefaultValue("Select Files...."), Bindable(true)]
        public string ButtonText
        {
            get { return this._buttontext; }
            set { this._buttontext = value; }
        }
        private string _buttontext = "Select Files....";
        [Category("控制項設定")]
        [Description("按鈕文字")]
        [DefaultValue("Upload"), Bindable(true)]
        public string UploadButtonText
        {
            get { return this._uploadbuttontext; }
            set { this._uploadbuttontext = value; }
        }
        private string _uploadbuttontext = "Upload";
        [Category("控制項設定")]
        [Description("檔案上傳完成事件")]
        [DefaultValue(""), Bindable(false)]
        public string OnUploadSuccess
        {
            get { return this._onUploadSuccess; }
            set { this._onUploadSuccess = value; }
        }
        private string _onUploadSuccess = @"function(file, data, response){ReturnData(data,null);}";
        [Category("控制項設定")]
        [Description("上傳次數限制")]
        [DefaultValue(0), Bindable(true)]
        public int UploadLimit {
            get { return this._uploadlimit; }
            set { this._uploadlimit = value; }
        }
        private int _uploadlimit = 0;
        [Category("控制項設定")]
        [Description("*.jpg;*.gif")]
        [DefaultValue(""), Bindable(true)]
        public string Extension { get; set; }
        #endregion
        #region Event   
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ClientScriptProxy scriptProxy = ClientScriptProxy.Current;

            // *** Register resources
            this.RegisterResources(scriptProxy);
            StringBuilder sbOptions = new StringBuilder(512);
            sbOptions.AppendLine("{");
            sbOptions.AppendLine(string.Format("'multi':{0},", this.MultiUpload.ToString().ToLower()));
            if(this.UploadLimit!=0)
                sbOptions.AppendLine(string.Format("'uploadLimit':'{0}',", this.UploadLimit));
            sbOptions.AppendLine(string.Format("'auto':{0},", this.AutoUpload.ToString().ToLower()));
            if(!string.IsNullOrEmpty(this.ButtonText))
                  sbOptions.AppendLine(string.Format("'buttonText':'{0}',", this.ButtonText));
            if (!string.IsNullOrEmpty(this.Extension))
                sbOptions.AppendLine(string.Format("'fileTypeExts':'{0}',", this.Extension));
            sbOptions.AppendLine(string.Format("'onUploadSuccess' :{0},", this.OnUploadSuccess));
            sbOptions.AppendLine(string.Format("'swf':'{0}',",
                scriptProxy.GetWebResourceUrl(this, typeof(ControlResources), ControlResources.FILE_UPLOADER_SWF_RESOURCE)));
            if (!string.IsNullOrEmpty(this.UploadHandler))
                sbOptions.AppendLine(string.Format("'uploader':'{0}'", this.ResolveUrl(this.UploadHandler)));
            else
                throw new Exception("UploadHandler Not Null");
            sbOptions.Append("}");

            StringBuilder sbStartupScript = new StringBuilder(400);
            sbStartupScript.AppendLine("$( function() {");
            sbStartupScript.AppendLine(string.Format("$('#{0}_file').uploadify({1});", this.ClientID, sbOptions.ToString()));
            sbStartupScript.AppendLine("});");

            scriptProxy.RegisterStartupScript(this.Page, typeof(ControlResources), "_fp" + this.ID,
                sbStartupScript.ToString(), true);
        }
        protected override void RenderContents(HtmlTextWriter output)
        {
          
          //  output.Write("<tr><td>");
            output.Write("<div class='file'>");
            output.Write(string.Format(@"<input type=""file"" name=""file_upload"" id=""{0}_file"" />",this.ClientID));
            output.Write("</div>");
          //  output.Write("</td>");
          //  output.Write("</tr>");
            if (!this.AutoUpload)
            {
                output.Write("<div class='button'>");
                //output.Write("<tr><td>");
                output.Write(string.Format(@"<input type=""button"" class='uploadify-button-upload' name=""file_upload_button"" value=""{1}"" id=""{0}_button"" onclick=""javascript:$('#{0}_file').uploadify('upload','*')"" />",
                                                                this.ClientID, this.UploadButtonText));
                //output.Write("</td></tr>");
                output.Write("</div>");
            }
        
        }
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.WriteLine("<div class='panel' >");
           // writer.WriteLine("<table class='panel' style=''>");
           // base.RenderBeginTag(writer);
        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {
           // base.RenderEndTag(writer);
           // writer.WriteLine("</table>");
            writer.WriteLine("<div>");
        }
        public delegate void ReceivedJSONDataHandler(object sender, string JsonData);
        public event ReceivedJSONDataHandler ReceivedData;
        private void OnReceived(string json)
        {
            if (this.ReceivedData != null)
            {
                ReceivedData(this, json);
            }
        }
        #endregion
        #region Private Method
        private void RegisterResources(ClientScriptProxy scriptProxy)
        {
            // *** Make sure jQuery is loaded
            if (this.LoadjQuery)
            {
                if(string.IsNullOrEmpty(this.jQueryPath))
                    ControlResources.LoadjQuery(this.Page);
                else
                    scriptProxy.RegisterClientScriptInclude(this.Page, typeof(ControlResources),
                     "_jqueryjs", this.ResolveUrl(this.jQueryPath));
            }
         

            // *** Load jQuery uploadify Scripts
            if (string.IsNullOrEmpty(this.FileUploadJsPath))
                scriptProxy.RegisterClientScriptResource(this.Page, typeof(ControlResources),
                    ControlResources.FILE_UPLOADER_SCRIPT_RESOURCE);
            else
                scriptProxy.RegisterClientScriptInclude(this.Page, typeof(ControlResources),
                                    "__fileuploader",
                                    this.ResolveUrl(this.FileUploadJsPath));
            
            // *** Load the related CSS reference into the page
            string css = "";
            if (string.IsNullOrEmpty(this.CssPath))
                css = scriptProxy.GetWebResourceUrl(this.Page, typeof(ControlResources),
                                             ControlResources.FILE_UPLOADER_CSS_RESOURCE);
            else
                css = this.ResolveUrl(this.CssPath);
            // *** Register Calendar CSS 'manually'
            scriptProxy.RegisterClientScriptBlock(this.Page, typeof(ControlResources), "_calcss",
                string.Format(@"<link href='{0}' type=""text/css"" rel=""stylesheet"" />",css), false);

            //CallBackHandlerEventRegister
            string cbReference1 = this.Page.ClientScript.GetCallbackEventReference(this, "arg",
            "ReturnData", "");
            string callbackScript1 = "function ReturnData(arg, context) {" +
            cbReference1 + "; }";

            scriptProxy.RegisterClientScriptBlock(this.Page, typeof(ControlResources), "_fp", callbackScript1,true);
        }
        #endregion

        #region CallabckEventHandler
        public string GetCallbackResult()
        {
            return string.Empty;
        }

        public void RaiseCallbackEvent(string eventArgument)
        {
            this.OnReceived(eventArgument);
        }
        #endregion
      
    }
}
