using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace General.CC.DatePicker
{
    /// <summary>
    /// ASP.NET jQuery DatePicker Control Wrapper
    /// by Rick Strahl
    /// http://www.west-wind.com/
    /// 
    /// License: Free
    /// 
    /// based on jQuery UI DatePicker client control by Marc Grabanski    
    /// http://marcgrabanski.com/code/ui-datepicker/
    /// 
    /// Simple DatePicker control that uses jQuery UI DatePicker to pop up 
    /// a date picker. 
    /// 
    /// Important Requirements:
    /// scripts/jquery.js   
    /// scripts/ui.datepicker.js
    /// scripts/ui.datepicker.css
    /// 
    /// Resources are embedded into the assembly so you don't need
    /// to reference or distribute anything. You can however override
    /// each of these resources with relative URL based resources.
    /// 修改適用於繁體中文版
    /// </summary>
    [ToolboxBitmap(typeof(System.Web.UI.WebControls.Calendar)), DefaultProperty("Text"),
    ToolboxData("<{0}:DatePicker runat=\"server\"></{0}:DatePicker>")]
    public class DatePicker : TextBox
    {
        public DatePicker()
        {
            this.Width = Unit.Pixel(80);
            this._OnClientSelect = " function() {}";
        }

        /// <summary>
        /// The currently selected date
        /// </summary>
        [DefaultValue(typeof(DateTime), ""),
        Category("Date Selection")]
        public DateTime? SelectedDate
        {
            get
            {
                DateTime defaultDate = DateTime.Now;//預設值為當日日期
                if (DateTime.TryParse(this.Text, out defaultDate))
                {
                    return defaultDate;
                }
                else
                    return null;
            }
            set
            {
                if (!value.HasValue)
                    this.Text = "";
                else
                {
                    string dateFormat = this.DateFormat;
                    if (dateFormat == "Auto")
                        dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                    this.Text = value.Value.ToString(dateFormat);
                }
            }
        }


        /// <summary>
        /// Determines how the datepicking option is activated
        /// </summary>
        [Description("Determines how the datepicking option is activated")]
        [Category("Date Selection"), DefaultValue(typeof(DatePickerDisplayModes), "ImageButton")]
        public DatePickerDisplayModes DisplayMode
        {
            get { return _DisplayMode; }
            set { _DisplayMode = value; }
        }
        private DatePickerDisplayModes _DisplayMode = DatePickerDisplayModes.ImageButton;



        /// <summary>
        /// Url to a Calendar Image or WebResource to use the default resource image.
        /// Applies only if the DisplayMode = ImageButton
        /// </summary>
        [Description("Url to a Calendar Image or WebResource to use the default resource image")]
        [Category("Resources"), DefaultValue("WebResource")]
        public string ButtonImage
        {
            get { return _ButtonImage; }
            set { _ButtonImage = value; }
        }
        private string _ButtonImage = "WebResource";

        /// <summary>
        /// The CSS that is used for the calendar
        /// </summary>
        [Category("Resources"), Description("The CSS that is used for the calendar or empty. WebResource loads from resources."),
         DefaultValue("WebResource")]
        public string CalendarCss
        {
            get { return _CalendarCss; }
            set { _CalendarCss = value; }
        }
        private string _CalendarCss = "WebResource";


        /// <summary>
        /// Location for the calendar JavaScript
        /// </summary>
        [Description("Location for the calendar JavaScript or empty for none. WebResource loads from resources")]
        [Category("Resources"), DefaultValue("WebResource")]
        public string CalendarJs
        {
            get { return _CalendarJs; }
            set { _CalendarJs = value; }
        }
        private string _CalendarJs = "WebResource";


        /// <summary>
        /// Location of jQuery library. Use WebResource for loading from resources
        /// </summary>
        [Description("Location of jQuery library or empty for none. Use WebResource for loading from resources")]
        [Category("Resources"), DefaultValue("WebResource")]
        public string jQueryJs
        {
            get { return _jQueryJs; }
            set { _jQueryJs = value; }
        }
        private string _jQueryJs = "WebResource";


        /// <summary>
        /// Determines the Date Format used. Auto uses CurrentCulture. Format: MDY/  month, date,year separator
        /// </summary>
        [Description("Determines the Date Format used. Auto uses CurrentCulture. Format: MDY/  month, date,year separator")]
        [Category("Date Selection"), DefaultValue("Auto")]
        public string DateFormat
        {
            get { return _DateFormat; }
            set { _DateFormat = value; }
        }
        private string _DateFormat = "Auto";

        /// <summary>
        /// Minumum allowable date. Leave blank to allow any date
        /// </summary>
        [Description("Minumum allowable date")]
        [Category("Date Selection"), DefaultValue(typeof(DateTime?), null)]
        public DateTime? MinDate
        {
            get { return _MinDate; }
            set { _MinDate = value; }
        }
        private DateTime? _MinDate = null;
        [Description("不載入jQuery API")]
        [Category("Date Selection"), DefaultValue(false)]
        public bool DisablejQuery
        {
            get { return _DisablejQuery; }
            set { _DisablejQuery = value; }
        }
        private bool _DisablejQuery = false;
        /// <summary>
        /// Maximum allowable date. Leave blank to allow any date.
        /// </summary>
        [Description("Maximum allowable date. Leave blank to allow any date.")]
        [Category("Date Selection"), DefaultValue(typeof(DateTime?), null)]
        public DateTime? MaxDate
        {
            get { return _MaxDate; }
            set { _MaxDate = value; }
        }
        private DateTime? _MaxDate = null;


        /// <summary>
        /// Client event handler fired when a date is selected
        /// </summary>
        [Description("Client event handler fired when a date is selected")]
        [Category("Date Selection"), DefaultValue("")]
        public string OnClientSelect
        {
            get { return _OnClientSelect; }
            set { _OnClientSelect = value; }
        }
        private string _OnClientSelect = "";


        /// <summary>
        /// Determines where the Close icon is displayed. True = top, false = bottom.
        /// </summary>
        [Description("Determines where the Close icon is displayed. True = top, false = bottom.")]
        [Category("Date Selection"), DefaultValue(true)]
        public bool CloseAtTop
        {
            get { return _CloseAtTop; }
            set { _CloseAtTop = value; }
        }
        private bool _CloseAtTop = true;


        /// <summary>
        /// Code that embeds related resources (.js and css)
        /// </summary>
        /// <param name="scriptProxy"></param>
        protected void RegisterResources(ClientScriptProxy scriptProxy)
        {
            // *** Make sure jQuery is loaded
            if (this.jQueryJs == "WebResource" && !DisablejQuery)
                ControlResources.LoadjQuery(this.Page);
            else if (!string.IsNullOrEmpty(this.jQueryJs))
                scriptProxy.RegisterClientScriptInclude(this.Page, typeof(ControlResources),
                     "_jqueryjs", this.ResolveUrl(this.jQueryJs));

            // *** Load the calandar script
            string script = this.CalendarJs;

            // *** Load jQuery Calendar Scripts
            if (script == "WebResource")
                scriptProxy.RegisterClientScriptResource(this.Page, typeof(ControlResources),
                    ControlResources.JQUERY_CALENDAR_SCRIPT_RESOURCE);
            else if (!string.IsNullOrEmpty(script))
                scriptProxy.RegisterClientScriptInclude(this.Page, typeof(ControlResources),
                                    "__jqueryCalendar",
                                    this.ResolveUrl(script));

            // *** Load the related CSS reference into the page
            script = this.CalendarCss;
            if (script == "WebResource")
                script = scriptProxy.GetWebResourceUrl(this.Page, typeof(ControlResources),
                                             ControlResources.JQUERY_CALENDAR_CSS_RESOURCE);
            else if (!string.IsNullOrEmpty(script))
                script = this.ResolveUrl(this.CalendarCss);

            // *** Register Calendar CSS 'manually'
            string css = @"<link href=""" + script +
                         @""" type=""text/css"" rel=""stylesheet"" />";
            scriptProxy.RegisterClientScriptBlock(this.Page, typeof(ControlResources), "_calcss", css, false);
        }

        /// <summary>
        /// Most of the work happens here for generating the hook up script code
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // *** MS AJAX aware script management
            ClientScriptProxy scriptProxy = ClientScriptProxy.Current;

            // *** Register resources
            this.RegisterResources(scriptProxy);

            string dateFormat = this.DateFormat;

            if (string.IsNullOrEmpty(dateFormat) || dateFormat == "Auto")
            {
                // *** Try to create a data format string from culture settings
                // *** this code will fail if culture can't be mapped on server hence the empty try/catch
                try
                {
                    dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                }
                catch { }
            }

            dateFormat = dateFormat.ToLower().Replace("yyyy", "yy");

            // *** Capture and map the various option parameters
            StringBuilder sbOptions = new StringBuilder(512);
            sbOptions.Append("{");

            string onSelect = this.OnClientSelect;

            if (this.DisplayMode == DatePickerDisplayModes.Button)
                sbOptions.Append("showOn: 'button',");
            else if (this.DisplayMode == DatePickerDisplayModes.ImageButton)
            {
                string img = this.ButtonImage;
                if (img == "WebResource")
                    img = scriptProxy.GetWebResourceUrl(this, typeof(ControlResources), ControlResources.CALENDAR_ICON_RESOURCE);
                else
                    img = this.ResolveUrl(this.ButtonImage);

                sbOptions.Append("showOn: 'button', buttonImageOnly: true, buttonImage: '" + img + "',buttonText: 'Select date',");
            }
            else if (this.DisplayMode == DatePickerDisplayModes.Inline)
            {
                scriptProxy.RegisterHiddenField(this, this.ClientID, this.Text);
                onSelect = this.ClientID + "OnSelect";
            }

            if (!string.IsNullOrEmpty(onSelect))
                sbOptions.Append("onSelect: " + onSelect + ",");

            if (this.MaxDate.HasValue)
                sbOptions.Append("maxDate: " + wwWebUtils.EncodeJsDate(MaxDate.Value) + ",");

            if (this.MinDate.HasValue)
                sbOptions.Append("minDate: " + wwWebUtils.EncodeJsDate(MinDate.Value) + ",");

            if (!this.CloseAtTop)
                sbOptions.Append("closeAtTop: false,");

            sbOptions.Append("dateFormat: '" + dateFormat + "'}");


            // *** Write out initilization code for calendar
            StringBuilder sbStartupScript = new StringBuilder(400);
            sbStartupScript.AppendLine("jQuery(document).ready( function() {");


            if (this.DisplayMode != DatePickerDisplayModes.Inline)
                sbStartupScript.AppendLine("var cal = jQuery('#" + this.ClientID + "').datepicker(" + sbOptions.ToString() + ");");
            else
            {
                sbStartupScript.AppendLine("var cal = jQuery('#" + this.ClientID + "Div').datepicker(" + sbOptions.ToString() + ");");
                sbStartupScript.AppendLine("var dp = jQuery.datepicker;");

                if (this.SelectedDate.HasValue && this.SelectedDate.Value > new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                    sbStartupScript.AppendLine("dp.setDateFor(cal[0],new Date('" + this.Text + "'));");

                sbStartupScript.AppendLine("dp.reconfigureFor(cal[0]);");

                // *** Assign value to hidden form var on selection
                scriptProxy.RegisterStartupScript(this, typeof(ControlResources), this.UniqueID + "OnSelect",
                    "function  " + this.ClientID + "OnSelect(dateStr)\r\n" +
                    "{\r\n" +
                    ((string.IsNullOrEmpty(this.OnClientSelect)) ? this.OnClientSelect + "(dateStr);\r\n" : "") +
                    "jQuery('#" + this.ClientID + "')[0].value = dateStr;\r\n}\r\n", true);
            }

            sbStartupScript.AppendLine("} );");
            scriptProxy.RegisterStartupScript(this.Page, typeof(ControlResources), "_cal" + this.ID,
                 sbStartupScript.ToString(), true);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        public override void RenderControl(HtmlTextWriter writer)
        {
            if (this.DisplayMode != DatePickerDisplayModes.Inline)
                base.RenderControl(writer);
            else
            {
                writer.Write("<div id='" + this.ClientID + "Div'></div>");
            }

            if (HttpContext.Current == null)
            {
                if (this.DisplayMode == DatePickerDisplayModes.Button)
                {
                    writer.Write(" <input type='button' value='...' style='width: 20px; height: 20px;' />");
                }
                else if ((this.DisplayMode == DatePickerDisplayModes.ImageButton))
                {
                    string img;
                    if (this.ButtonImage == "WebResource")
                        img = this.Page.ClientScript.GetWebResourceUrl(this.GetType(), ControlResources.CALENDAR_ICON_RESOURCE);
                    else
                        img = this.ResolveUrl(this.ButtonImage);

                    writer.AddAttribute(HtmlTextWriterAttribute.Src, img);
                    writer.AddAttribute("hspace", "2");
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                }
            }
        }
    }


    public enum DatePickerDisplayModes
    {
        Button,
        ImageButton,
        AutoPopup,
        Inline
    }
}
