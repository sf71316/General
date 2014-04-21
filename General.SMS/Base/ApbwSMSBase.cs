using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using General;
using System.IO;
using System.Net.Security;

namespace General.SMS
{
    public abstract class ApbwSMSBase : IDisposable
    {
        private HttpWebRequest request;
        private HttpWebResponse response;
        private readonly string API_URI = "http://121.254.96.2/XSMSAP/api/{0}";
        protected string Command;
        protected T RequestService<T>(string content)
            where T : class,new()
        {
            Stream RequestStream = null;
            StreamReader ResponseReader = null;
            request = WebRequest.Create(string.Format(this.API_URI, this.Command)) as HttpWebRequest;
            byte[] bytes;
            bytes = System.Text.Encoding.UTF8.GetBytes(content);
            request.Method = "POST";
            request.ContentLength = bytes.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Timeout = 10 * 1000; //time out 10秒
            //取得Request的stream
            RequestStream = request.GetRequestStream();
            //將資料寫入Stream中
            RequestStream.Write(bytes, 0, bytes.Length);
            //關閉此Stream
            RequestStream.Close();
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                ResponseReader = new StreamReader(response.GetResponseStream());
                T t = ResponseReader.ReadToEnd().Deserialize<T>();
                return t;
            }
            return null;
        }
        public abstract IResponse Action(IRequest entity);
        public abstract T Action<T>(IRequest entity) where T : class,IResponse, new();
        public static ApbwSMSBase GetInstance(CommandKind cmd)
        {
            switch (cmd)
            {
                case CommandKind.立即發送簡訊:
                    return new ImmediateSendSMS();
                case CommandKind.預約發送簡訊:
                    return new ReserveSendSMS();
                default:
                    return null;
            }
        }
        public void Dispose()
        {
            if (this != null)
            {
                GC.SuppressFinalize(this);
            }
            if (request != null)
            {
                request = null;
            }
            if (response != null)
            {
                response = null;
            }
        }
    }
}
