using Microsoft.TV.TVControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace General.MPF
{
    public abstract class BasePage:Page
    {
        protected TVClientContext _tvc;
        protected string AppTheme
        {
            get
            {
                if (this.Resolution == DisplayResolution.HD)
                {
                   return  "HD";
                }
                else
                {
                    return "SD";
                }
            }
        }
        protected virtual bool DebugMode {
            get
            {
                return false;
            }
        }
        protected DisplayResolution Resolution
        {
            get
            {
                if (this._tvc.DisplayFormat.ToString().Contains("HD"))
                {
                    return DisplayResolution.HD;
                }
                else
                {
                    return DisplayResolution.SD;
                }

            }
        }
        protected string Language
        {
            get
            {
                return this._tvc.AcceptLanguage;
            }
        }
        protected string DeviceId
        {
            get
            {
                if (DebugMode)
                    return System.Configuration.ConfigurationManager.AppSettings["Test.DeviceID"];
                else
                    return this._tvc.DeviceId;
            }
        }
        protected virtual void Page_PreInit(object sender, EventArgs e)
        {
            _tvc = this.PageInformation(this.Context);
            this.Theme = this.AppTheme;
        }
        protected virtual void Page_Init(object sender, EventArgs e)
        {
            Response.ContentType = "text/xml";
        }
        protected virtual void Page_LoadComplete(object sender, EventArgs e)
        {

        }
     
    }
}
