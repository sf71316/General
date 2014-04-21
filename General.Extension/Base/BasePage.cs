using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace General
{
    public abstract class BasePage : Page
    {
        private List<string> _displayformat;
        protected virtual void Page_PreInit(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.Page.Request.Headers["UA-DisplayFormat"]))
                _displayformat = this.Page.Request.Headers["UA-DisplayFormat"].Split(' ').ToList();
            this.Theme = AppTheme;
        }
        protected virtual void Page_Init(object sender, EventArgs e)
        {
            Response.ContentType = "text/xml";

        }

        #region Private Method

        private string AppTheme
        {
            get
            {
                //TODO 強制使用HD模式

                if (this.Resolution == DisplayResolution.HD)
                    return "HD";
                else
                    return "SD";//"SD";
            }
        }
        #endregion

        #region Protected Method
        protected DisplayResolution Resolution
        {
            get
            {
                if (_displayformat != null)
                {
                    return (_displayformat[0] == "sd") ?
                        DisplayResolution.SD :
                             DisplayResolution.HD;
                }
                else
                {
                    return DisplayResolution.SD;
                }
            }
        }
        protected DisplayScan Scan
        {
            get
            {
                if (_displayformat != null)
                {
                    return (_displayformat[1] == "interlaced") ?
                        DisplayScan.Interlaced :
                        DisplayScan.Progressive;
                }
                else
                {
                    return DisplayScan.Interlaced;
                }
            }
        }
        protected bool IsWideScreen
        {
            get
            {
                if (_displayformat != null)
                {
                    return (_displayformat[2] == "wide") ? true : false;
                }
                else
                {
                    return false;
                }
            }
        }
        protected string Language
        {
            get
            {
                return Page.Request.Headers.Get("Accept-Language");
            }
        }
        protected string DeviceId
        {
            get
            {
                return this.Page.Request.Headers.Get("UA-DeviceId");
            }
        }
        #endregion
    }
}
