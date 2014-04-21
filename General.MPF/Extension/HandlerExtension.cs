using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace General.MPF
{
    public static class HandlerExtension
    {
        public static UserSession GetUserData(this HttpContext context, string DeviceId)
        {
            SessionModel session = new SessionModel(context);
            return session.GetUserData(DeviceId);
        }
    }
}
