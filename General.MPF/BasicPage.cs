using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web.Caching;

namespace General.MPF
{
    
    public abstract class BasicPage:BasePage
    {
       SessionModel _session;
       protected override void Page_PreInit(object sender, EventArgs e)
       {
           base.Page_PreInit(sender, e);
           this.Theme = this.AppTheme;
           _session = new SessionModel(this.Context);
       }


       private bool SessionExist
       {
           get
           {
               return _session.Exist;
           }
       }
       public UserSession MPFSession
       {
           get
           {
               return _session.GetUserData(this.DeviceId);
           }
       }
    }
}
