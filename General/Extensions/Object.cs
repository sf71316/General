using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Caching;
using System.Runtime.ConstrainedExecution;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Threading;

namespace General
{
    public static class ObjectExtensions
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
        public static T ConvertTo<T>(this object value)
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="omitXmlDeclaration">True 不設定 False 設定 XML 宣告</param>
        /// <returns></returns>
        public static string Serialize(this object value, bool omitXmlDeclaration,
            bool indent = false, bool useCache = false)
        {

            XmlSerializer xs = null;
            if (useCache)
            {
                var cache = GetXMLSerializeCache();
                if (!cache.TryGetValue(value.GetType(), out xs))
                {
                    xs = new XmlSerializer(value.GetType(), "");
                    cache.TryAdd(value.GetType(), xs);
                }
            }
            else
            {
                xs = new XmlSerializer(value.GetType(), "");
            }
            //  StringBuilder sb = new StringBuilder();
            XmlWriterSettings xws = new XmlWriterSettings()
            {
                Encoding = new UTF8Encoding(false),
                OmitXmlDeclaration = omitXmlDeclaration,
                Indent = indent,

            };
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter xw = XmlWriter.Create(ms, xws))
                {
                    var emptyNs = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

                    xs.Serialize(xw, value, emptyNs);
                    xw.Flush();
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }



        }
        public static string Serialize(this object value, bool useCache = false)
        {
            return Serialize(value, true, useCache);
        }
        public static string JsonSerialize(this object value)
        {
            return JsonConvert.SerializeObject(value);
        }

    }
}
