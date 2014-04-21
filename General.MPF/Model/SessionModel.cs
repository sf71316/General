using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace General.MPF
{
    public class SessionModel
    {
        private HttpContext _context;
        public SessionModel(HttpContext context)
        {
            this._context = context;
            this.InitSession();
        }
        private void InitSession()
        {
            if (!Exist)
            {
                this._context.Cache.Add("MPFSession", new MPFSession(), null,
                         Cache.NoAbsoluteExpiration,
                            new TimeSpan(0, 30, 0), CacheItemPriority.NotRemovable, null);
            }
        }
        public MPFSession MPFSession
        {
            get
            {
                return _context.Cache["MPFSession"] as MPFSession;
            }
        }
        public bool Exist
        {
            get
            {
                return this.MPFSession != null;
            }
        }
        public UserSession GetUserData(string DeviceId)
        {
            MPFSession session = MPFSession;
            if (session == null)
                InitSession();
            if (!session.ContainsKey(DeviceId))
                session.Add(DeviceId, new UserSession());
            return session[DeviceId] as UserSession;
        }
    }
}
