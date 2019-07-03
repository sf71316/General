using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using System.Reflection;

namespace General
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
        public static int? ParseInt(this string str)
        {
            int i = 0;
            if (int.TryParse(str, out i))
            {
                return i;
            }
            else
            {
                return null;
            }
        }
        public static Guid? ParseGuild(this string str)
        {
            try
            {
                return new Guid(str);
            }
            catch
            {
                return null;
            }
        }
        public static T Deserialize<T>(this string value) where T : new()
        {
            return Deserialize<T>(value, string.Empty,false);
        }
        public static T Deserialize<T>(this string value, bool InitEntity) where T : new()
        {
            return Deserialize<T>(value, string.Empty, InitEntity);
        }
        public static T Deserialize<T>(this string value,string xmlPath,bool InitEntity) where T : new()
        {
            XmlDocument xdoc = new XmlDocument();

            try
            {
              
                xdoc.LoadXml(value);
                XmlNodeReader reader ;
                if(string.IsNullOrEmpty(xmlPath))
                    reader = new XmlNodeReader(xdoc.DocumentElement);
                else
                    reader = new XmlNodeReader(xdoc.SelectSingleNode(xmlPath));
                XmlSerializer ser = new XmlSerializer(typeof(T));
                object obj = ser.Deserialize(reader);

                return (T)obj;
            }
            catch(Exception ex)
            {
                if(InitEntity)
                  return  Activator.CreateInstance<T>();
                else
                    return default(T);
            }

        }
        public static T TryParse<T>(this string value)
        {
            List<MethodInfo> methods = typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Public).
                Where(p => p.Name.ToLower() == "tryparse").ToList();
            if (methods.Count > 0)
            {
                try
                {
                    MethodInfo m = methods[0];
                    object[] parameters = new object[] { value, null };
                    bool result = (bool)m.Invoke(null, parameters);
                    if (result)
                    {
                        return (T)parameters[1];
                    }
                }
                catch
                {
                    return default(T);
                }
            }
            return default(T);
        }
        public static bool IsParse<T>(this string value)
        {
            List<MethodInfo> methods = typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Public).
                Where(p => p.Name.ToLower() == "tryparse").ToList();
            if (methods.Count > 0)
            {
                MethodInfo m = methods[0];
                object[] parameters = new object[] { value, null };
                bool result = (bool)m.Invoke(null, parameters);
                return result;
            }
            return false;
        }
        public static T JsonDeserialize<T>(this string value)where T:new ()
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
