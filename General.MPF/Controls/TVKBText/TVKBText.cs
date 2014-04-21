using Microsoft.TV.TVControls;
using Microsoft.TV.TVControls.Actions;
using Microsoft.TV.TVControls.Collections;
using Microsoft.TV.TVControls.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace General.MPF.Controls
{
    public class TVKBText : TVText
    {
        private  string CONTROL_ID;
        public TVKBText()
        {
            this.PreRender += Page_PreRender;
            this.Load += TVKBText_Load;
            
        }       
        protected void TVKBText_Load(object sender, EventArgs e)
        {
            CONTROL_ID = string.Format("{0}_{1}",
    Path.GetFileNameWithoutExtension(this.Page.Request.PhysicalPath).ToLower(), this.ClientID);
            if (this.EnableKeyboard && !string.IsNullOrEmpty(this.KBContainerID))
            {
                IKBContainer _container;
                if (Page.Master == null)
                    _container = Page.FindControl(this.KBContainerID) as IKBContainer;
                else
                {
                    Control _control = null;
                    foreach (Control item in Page.Master.Controls)
                    {
                        _control = this.FindControl<Control>(item, this.KBContainerID);
                        if (_control != null)
                            break;
                    }

                    _container = _control as IKBContainer;
                }

                if (_container != null)
                {
                    this.KBDatasourceID = _container.DatasourceID;
                    this.KBLayoutID = _container.LayoutID;
                }
            }
            if (this.EnableSaveState)
            {
                this.LoadState();
            }
        }

        private void LoadState()
        {
            if(this.Page.Cache[CONTROL_ID]!=null)
                this.Text = this.Page.Cache[CONTROL_ID].ToString();
        }
        private void SaveState()
        {
            SubmitAction submit = new SubmitAction("_savestate","",
                string.Format("savehandler.ashx?id={0}&cid={1}",CONTROL_ID,this.ID),
                SubmitMethod.Get,this.ID);
            this.OnBlur = new BlurEvent(submit);
        }
        protected void Page_PreRender(object e, EventArgs arg)
        {
            if (this.EnableKeyboard)
            {
                //Onclick Add Event
                
                ShowAction _showkb = new ShowAction(string.Format("{0}_showkb", this.ID), this.KBLayoutID);
                SubmitAction _bindkb =
                    new SubmitAction(string.Format("{0}_bindpage", this.ID), this.KBDatasourceID,
                        string.Format("{0}?t={1}&c={2}",
                        this.KBPagePath, this.ClientID, this.TVPageID), SubmitMethod.Get, "");

                //Change Background Back Button 
                TVActions _actions = this.FindControl(this.TVActionID) as TVActions;
                TVLabel _lblbg = new TVLabel(string.Format("{0}_bg", this.ID), this.Background.ToString());
                if (_actions != null)
                {
                    ScriptAction _changeBack = new ScriptAction("changeBack");
                    _changeBack.Name = "ControlBgBackAction_" + this.ClientID;
                    _changeBack.Data = this.ClientID;
                    _actions.Actions.Add(_changeBack);

                    FocusAction _FocusAction = new FocusAction("Focus_" + this.ClientID, this.ClientID);
                    _actions.Actions.Add(_FocusAction);

                    _lblbg.IsVisible = false;
                    if(this.Controls.OfType<Control>().All(p=>p.ID!=_lblbg.ID))
                            this.Controls.Add(_lblbg);
                }
                // Change Button Background
                ScriptAction _changeBg = new ScriptAction("changeBg");
                _changeBg.Name = "ScriptAction0";
                _changeBg.Data = this.ClientID;
                ActionsList _list = new ActionsList();
                _list.Add(_changeBg);
                _list.Add(_showkb);
                _list.Add(_bindkb);
                if (this.OnClick == null ||
                    !(this.OnClick.Actions.Contains(_showkb) && this.OnClick.Actions.Contains(_bindkb)
                    && this.OnClick.Actions.Contains(_changeBg)
                    ))
                    this.OnClick = new ClickEvent(_list);
            }
            if (this.EnableKBPage)
            {
                string _msg = (!string.IsNullOrEmpty(this.Message)) ? "&m=" + this.Message : "";
                NavigateAction _nav = new NavigateAction(this.ID+"_Nav0",
                    string.Format("{0}?t={1}{2}", this.KBPagePath, this.CONTROL_ID, _msg));
                this.OnClick = new ClickEvent(_nav);
            }
            if (this.EnableSaveState)
                this.SaveState();
        }

        
        private T FindControl<T>(Control control, string id) where T : Control
        {
            foreach (Control item in control.Controls)
            {
                if (item.ID == id || item.ClientID == id)
                {
                    return (T)item;
                }
                Control child = this.FindControl<T>(item, id);
                if (child != null)
                    return (T)child;
            }
            return null;
        }

        #region Property
        public bool EnableKeyboard { get; set; }
        public bool EnableKBPage { get; set; }
        public bool EnableSaveState { get; set; }
        public string TVActionID { get; set; }
        public string KBLayoutID { get; set; }
        public string KBDatasourceID { get; set; }
        public string KBPagePath { get; set; }
        public string KBContainerID { get; set; }
        public string TVPageID { get; set; }
        public string Message { get; set; }
        #endregion
    }
}
