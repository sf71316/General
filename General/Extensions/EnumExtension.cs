using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace General
{
    public static class EnumExtension
    {
        public static Attribute GetAttribute(this Enum obj, Type typeAttribute)
        {
            FieldInfo[] Fields = obj.GetType().GetFields();
            foreach (FieldInfo item in Fields)
            {
                Attribute t = Attribute.GetCustomAttribute(item, typeAttribute);
                if (t != null)
                {
                    if (item.Name == obj.ToString())
                        return t;
                }
            }
            return null;
        }
        public static T GetAttribute<T>(this Enum obj) where T : Attribute, new()
        {
            FieldInfo[] Fields = obj.GetType().GetFields();
            foreach (FieldInfo item in Fields)
            {

                T t = (T)Attribute.GetCustomAttribute(item, typeof(T));
                if (t != null)
                {
                    if (item.Name == obj.ToString())
                        return (T)t;
                }
            }
            return null;
        }
    }
}
