using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace General.Data
{
    public static class EnumExtension
    {
        public static Attribute GetAttribute(this Enum e, Type typeAttribute)
        {
            FieldInfo[] Fields = e.GetType().GetFields();
            foreach (FieldInfo item in Fields)
            {
                Attribute t = Attribute.GetCustomAttribute(item, typeAttribute);
                if (t != null)
                {
                    if (item.Name == e.ToString())
                        return t;
                }
            }
            return null;
        }
        public static T GetAttribute<T>(this Enum e) where T : Attribute, new()
        {
            FieldInfo[] Fields = e.GetType().GetFields();
            foreach (FieldInfo item in Fields)
            {

                T t = (T)Attribute.GetCustomAttribute(item, typeof(T));
                if (t != null)
                {
                    if (item.Name == e.ToString())
                        return (T)t;
                }
            }
            return null;
        }
    }
}
