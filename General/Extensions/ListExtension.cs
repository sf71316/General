using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace General
{
    /// <summary>
    /// List擴充方法
    /// </summary>
   public static class ListExtension
    {
       /// <summary>
       /// List 轉成分割字串
       /// </summary>
       /// <param name="list"></param>
       /// <param name="spliestring"></param>
       /// <returns></returns>
       public static string ToString(this List<string> list, string spliestring)
       {
           return string.Join(spliestring, list.ToArray());
       }
 
    }
}
