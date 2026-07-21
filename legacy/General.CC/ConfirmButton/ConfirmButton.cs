using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace General.CC.ConfirmButton
{
    [ToolboxBitmap(typeof(System.Web.UI.WebControls.Button)), DefaultProperty("Text"),
    ToolboxData("<{0}:ConfirmButton runat=\"server\"></{0}:ConfirmButton>")]
    public class ConfirmButton : Button
    {
        #region Property
        [DefaultValue(typeof(bool), "是否要求客戶端確認訊息")]
        public bool RequestConfirm
        {
            get
            {
                if (ViewState["Confirm"] != null)
                    return ViewState["Confirm"].ToString() == bool.TrueString;
                else
                    return false;
            }
            set
            {
                ViewState["Confirm"] = value;
            }
        }
           [DefaultValue(typeof(bool), "端確認訊息")]
        public string ConfirmMessage
        {
            get
            {
                return this._confirmmessage;
            }
            set
            {
                this._confirmmessage = value;
            }
        }
        private string _confirmmessage;
          [DefaultValue(typeof(bool), "提交後是否禁用按鈕")]
        public bool DisableButton
        {
            get
            {
                return this._disablebtn;
            }
            set
            {
                this._disablebtn = value;
            }

        }
          private bool _disablebtn;
        #endregion

        public ConfirmButton()
        {
            this.Load += ConfirmButton_Load;
        }
        void ConfirmButton_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Request["__EVENTTARGET"] == "confirm")
            {
                ConfirmArgs arg = new ConfirmArgs();
                arg.DialogResult = HttpContext.Current.Request["__EVENTARGUMENT"] == bool.TrueString;
                this.OnComfirm(arg);
            }
        }
        protected virtual void OnComfirm(ConfirmArgs e)
        {
            if (this.Confirm != null)
            {

                this.Confirm(this, e);

            }
            RequestConfirm = false;
        }
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if (this.RequestConfirm)
            {
                ClientScriptManager cs = Page.ClientScript;
                cs.RegisterStartupScript(this.GetType(), "confirm",
                  string.Format(@"{1}if(confirm('{0}'))
                    {{__doPostBack('confirm','True');}}
                    else{{__doPostBack('confirm','False');}}", 
                                                             ConfirmMessage,
                                                             (this.DisableButton)?"":""), true);
            }
            base.Render(writer);
        }
        public event EventHandler<ConfirmArgs> Confirm;
    }
}
