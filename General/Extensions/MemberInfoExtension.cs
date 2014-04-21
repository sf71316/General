using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace General
{
    public static class MemberInfoExtension
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
