using System;
using System.Web;
using System.Web.Caching;

namespace General.MPF.Controls
{
    public class SaveStateHandler : IHttpHandler
    {
        
        #region IHttpHandler Members

        public bool IsReusable
        {
         
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string _id = context.Request.QueryString["id"];
            string _value = context.Request.QueryString[context.Request.QueryString["cid"]];
            if (!string.IsNullOrEmpty(_id))
            {
                if (context.Cache.Get(_id) == null)
                {
                    context.Cache.Add(_id, _value, null,
                    Cache.NoAbsoluteExpiration,
                  new TimeSpan(0, 10, 0), CacheItemPriority.High, null);
                }
                else
                {
                    context.Cache.Insert(_id, _value, null,
                  Cache.NoAbsoluteExpiration,
                new TimeSpan(0, 10, 0), CacheItemPriority.High, null);
                }
            }
        }

        #endregion
    }
}
