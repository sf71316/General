using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace General.Data
{
    public static class MemberInfoExtensions
    {
        public static T GetInstancetAttribute<T>(this MemberInfo obj) where T : Attribute, new()
        {
            try
            {
                
                return (T)Attribute.GetCustomAttribute(obj, typeof(T));
            }
            catch
            {
                return null;
            }
        }
    }
}
