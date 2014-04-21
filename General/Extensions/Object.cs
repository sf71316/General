using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;

namespace General
{
    public static class ObjectExtensions
    {
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
        public static string Serialize(this object value, bool omitXmlDeclaration)
        {
          
                XmlSerializer xs = new XmlSerializer(value.GetType(), "");
              //  StringBuilder sb = new StringBuilder();
                XmlWriterSettings xws = new XmlWriterSettings()
                {
                    Encoding=new UTF8Encoding(false),
                    OmitXmlDeclaration = omitXmlDeclaration,
                    Indent = true
                };
                MemoryStream ms = new MemoryStream();
                using (XmlWriter xw = XmlWriter.Create(ms, xws))
                {
                   var emptyNs = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                   xs.Serialize(xw, value, emptyNs);
                    xw.Flush();
                    return Encoding.UTF8.GetString(ms.ToArray());
                }

            
      

        }
        public static string Serialize(this object value)
        {
            return Serialize(value, true);
        }
        public static string JsonSerialize(this object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}
