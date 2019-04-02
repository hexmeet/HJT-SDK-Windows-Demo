using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using log4net;
using System.Net.Http.Headers;

public enum HttpVerb
{
    GET,
    POST,
    PUT,
    DELETE
}

namespace EasyVideoWin.HttpUtils
{
    public class RestResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
    }
    
    public class RestClient
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly string METHOD_GET = "GET";
        public static readonly string METHOD_PUT = "PUT";
        public static readonly string METHOD_POST = "POST";
        public static readonly string METHOD_DELETE = "DELETE";

        public static string getVerbStr(HttpVerb verb)
        {
            string httpMethod = METHOD_GET;
            switch(verb)
            {
                case HttpVerb.GET:
                    {
                        httpMethod = METHOD_GET;
                    }
                    break;
                case HttpVerb.PUT:
                    {
                        httpMethod = METHOD_PUT;
                    }
                    break;
                case HttpVerb.POST:
                    {
                        httpMethod = METHOD_POST;
                    }
                    break;
                case HttpVerb.DELETE:
                    {
                        httpMethod = METHOD_DELETE;
                    }
                    break;
                default:
                    {
                        httpMethod = METHOD_GET;
                    }
                    break;
            }
            return httpMethod;
        }

        public static string HttpConnectToServer(HttpVerb method, string ServerUrl, string strData)
        {
            byte[] dataArray = Encoding.UTF8.GetBytes(strData);
            
            //创建请求
            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ServerUrl);

            if (ServerUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            }

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ServerUrl);

            request.Method = getVerbStr(method);
            request.ContentLength = dataArray.Length;
            request.ContentType = "application/json;charset=utf8";
            request.Timeout = 20000;
            request.ReadWriteTimeout = 20000;

            if (method == HttpVerb.POST || method == HttpVerb.PUT)
            {
                //创建输入流
                Stream dataStream = null;
                try
                {
                    dataStream = request.GetRequestStream();
                }
                catch (Exception e)
                {
                    log.InfoFormat("Failed to get request stream, casue:{0}", e.Message);
                    return null;//连接服务器失败
                }
                //发送请求
                dataStream.Write(dataArray, 0, dataArray.Length);
                dataStream.Close();
            }
            //读取返回消息
            string res = string.Empty;
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    res = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                log.InfoFormat("Failed to read response stream, casue:{0}", ex.Message);
                return null;//连接服务器失败
            }
            return res;
        }
        
        #region -- Members --

        private HttpClient _httpClient = new HttpClient();

        #endregion

        #region -- Properties --

        #endregion

        #region -- Constructor --

        public RestClient()
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        #endregion

        #region -- Public Methods --

        public RestResponse GetObject(string url)
        {
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            }

            var response = _httpClient.GetAsync(url).Result;
            string resContent = response.Content.ReadAsStringAsync().Result;

            RestResponse restResponse = new RestResponse { StatusCode = response.StatusCode, Content = resContent };

            return restResponse;
        }

        public RestResponse PostObject(string url, string data)
        {
            using (HttpContent content = new StringContent(data))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                content.Headers.ContentType.CharSet = "UTF-8";

                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                var result = _httpClient.PostAsync(url, content).Result;
                string res = result.Content.ReadAsStringAsync().Result;

                RestResponse restResponse = new RestResponse { StatusCode = result.StatusCode, Content = res };

                return restResponse;
            }                
        }

        public RestResponse PutObject(string url, string data)
        {
            using (HttpContent content = new StringContent(data))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                content.Headers.ContentType.CharSet = "UTF-8";

                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                }

                var result = _httpClient.PutAsync(url, content).Result;
                string res = result.Content.ReadAsStringAsync().Result;

                RestResponse restResponse = new RestResponse { StatusCode = result.StatusCode, Content = res };

                return restResponse;
            }
        }

        public RestResponse DeleteObject(string url)
        {
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            }

            var response = _httpClient.DeleteAsync(url).Result;
            string resContent = response.Content.ReadAsStringAsync().Result;

            RestResponse restResponse = new RestResponse { StatusCode = response.StatusCode, Content = resContent };

            return restResponse;
        }

        public bool DownloadFile(string url, string localPath)
        {
            log.InfoFormat("Download file, url: {0}, local path: {1}", url, localPath);
            try
            {
                using (MyWebClient webClient = new MyWebClient())
                {
                    webClient.DownloadFile(url, localPath);
                }
            }
            catch (Exception e)
            {
                log.Error("Failed to download file, exception:", e);
                return false;
            }

            return true;
        }

        #endregion

        #region -- Private Methods --

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //accept always  
        }

        #endregion

        class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest w = base.GetWebRequest(address);
                w.Timeout = 30 * 1000;
                return w;
            }
        }
    }

}
