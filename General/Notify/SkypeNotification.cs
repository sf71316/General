using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace General.Notify
{
    public class SkypeNotification
    {
        private static readonly HttpClient _http = CreateHttpClient();
        private string _appName = "Default App";
        public SkypeNotification(string appName)
        {
            _appName = appName;
        }
        public string[] SendList { get; set; }

        public async Task NotifyAsync(string message, CancellationToken ct = default(CancellationToken))
        {
            if (SendList == null || SendList.Length == 0)
            {
                OnError("SendList 為空，未送出任何訊息。");
                return;
            }

            foreach (var agent in SendList.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                var payload = new NotifyModel
                {
                    conversationID = agent,
                    text = message
                };

                var json = JsonConvert.SerializeObject(payload);
                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    try
                    {
                        using (var req = new HttpRequestMessage(HttpMethod.Post, "/skype"))
                        {
                            req.Content = content;

                            var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);

                            if (resp.StatusCode == HttpStatusCode.OK ||
                                resp.StatusCode == HttpStatusCode.Created ||
                                resp.StatusCode == HttpStatusCode.Accepted)
                            {
                                OnSuccess("done");
                            }
                            else
                            {
                                string body;
                                try
                                {
                                    body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                                }
                                catch
                                {
                                    body = string.Empty;
                                }

                                OnError($"Skype failure Http code:{(int)resp.StatusCode} content:{body}");
                            }
                        }
                    }
                    catch (OperationCanceledException oce)
                    {
                        if (ct.IsCancellationRequested)
                            OnError("已取消送出：" + oce.Message);
                        else
                            OnError("逾時或被取消：" + oce.Message);
                    }
                    catch (Exception ex)
                    {
                        OnError("Exception: " + ex.Message + " " + ex.StackTrace);
                    }
                }
            }
        }

        // 與既有介面相容的同步包裝
        public void Notify(string message)
        {
            NotifyAsync(message).GetAwaiter().GetResult();
        }

        public event EventHandler<NotifyArg> Error;
        public event EventHandler<NotifyArg> Success;

        public void OnError(string message)
        {
            var handler = Error;
            if (handler != null)
                handler(this, new NotifyArg { Message = message });
        }

        public void OnSuccess(string message)
        {
            var handler = Success;
            if (handler != null)
                handler(this, new NotifyArg { Message = message });
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var http = new HttpClient(handler);
            http.BaseAddress = new Uri(ConfigHelper.SkypeNotificationAPIUrl, UriKind.Absolute);
            http.Timeout = TimeSpan.FromSeconds(15);
            http.DefaultRequestHeaders.Accept.Clear();
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return http;
        }

        private string GetIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return string.Empty;
        }

        public string DefaultMessage
        {
            get { return $"[{_appName} IP:({GetIP()})] {{0}} "; }
        }
    }

    public class NotifyModel
    {
        public string conversationID { get; set; }
        public string text { get; set; }
    }

    public class NotifyArg : EventArgs
    {
        public string Message { get; set; }
    }
}
