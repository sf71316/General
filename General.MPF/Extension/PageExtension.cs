using Microsoft.TV.TVControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace General.MPF
{
    public static class PageExtension
    {
        public static TVClientContext PageInformation(this Page page, HttpContext context)
        {
            return new TVClientContext(context);
        }
        public static TVClientContext PageInformation(this UserControl uc, HttpContext context)
        {
            return new TVClientContext(context);
        }
    }
}
