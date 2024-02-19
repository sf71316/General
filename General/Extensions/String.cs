using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Runtime.Caching;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Threading;

namespace General
{
    public static class StringExtensions
    {
        private static string XML_SERIALIZE_CACHE_NAME = "XMLSerializeCache";
        private static ConcurrentDictionary<Type, XmlSerializer> GetXMLSerializeCache()
        {
            ObjectCache cache = MemoryCache.Default;
            bool NotOverTime = Monitor.TryEnter(cache, 6000);
            try
            {
                if (!cache.Contains(XML_SERIALIZE_CACHE_NAME))
                {
                    CacheItemPolicy policy = new CacheItemPolicy();
                    policy.SlidingExpiration = TimeSpan.FromMinutes(10);
                    cache.Add(XML_SERIALIZE_CACHE_NAME, new ConcurrentDictionary<Type, XmlSerializer>(), policy);
                }
            }
            finally
            {
                if (NotOverTime)
                {
                    Monitor.Exit(cache);
                }
            }

            return cache.Get(XML_SERIALIZE_CACHE_NAME) as ConcurrentDictionary<Type, XmlSerializer>;
        }
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
        public static T Deserialize<T>(this string value, bool useCache = false) where T : new()
        {
            return Deserialize<T>(value, string.Empty, false, useCache);
        }
        public static T Deserialize<T>(this string value, bool InitEntity, bool useCache = false) where T : new()
        {
            return Deserialize<T>(value, string.Empty, InitEntity, useCache);
        }
        public static T DeserializeWithoutException<T>(this string value) where T : new()
        {

            try
            {
                return Deserialize<T>(value, "", false);
            }
            catch
            {
                return default(T);
            }



        }
        public static T Deserialize<T>(this string value, string xmlPath, bool InitEntity, bool UseCache = false) where T : new()
        {
            XmlSerializer ser = null;
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(value);
            XmlNodeReader reader;
            if (string.IsNullOrEmpty(xmlPath))
                reader = new XmlNodeReader(xdoc.DocumentElement);
            else
                reader = new XmlNodeReader(xdoc.SelectSingleNode(xmlPath));
            if (UseCache)
            {
                var cache = GetXMLSerializeCache();
                if (!cache.TryGetValue(typeof(T), out ser))
                {
                    ser = new XmlSerializer(typeof(T));
                    cache.TryAdd(typeof(T), ser);
                }

            }
            else
            {
                ser = new XmlSerializer(typeof(T));
            }
            object obj = ser.Deserialize(reader);

            return (T)obj;
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
        public static T JsonDeserialize<T>(this string value) where T : new()
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

    }
}
