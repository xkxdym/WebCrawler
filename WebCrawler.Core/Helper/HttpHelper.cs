using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using WebCrawler.Model.Http;

namespace WebCrawler.Core
{
    /// <summary>
    /// http请求帮助类
    /// </summary>
    public class HttpHelper
    {
        private HttpWebRequest request;

        private HttpWebResponse response;

        /// <summary>
        /// 获取实例
        /// </summary>
        public static HttpHelper Instance
        {
           get
            {
                return new HttpHelper();
            }
        }
        /// <summary>
        /// 获取请求结果
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public HttpResult GetResult(HttpItem item)
        {

            HttpResult result= new HttpResult(); ;

            try
            {
                InitRequest(item);
            }
            catch (Exception ex)
            {
                result = new HttpResult()
                {
                    Header = null,
                    Cookie = string.Empty,
                    Html = ex.Message,
                    StatusDescription = "配置参数时出错：" + ex.Message
                };
                return result;
            }

            try
            {
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    result = new HttpResult()
                    {
                        StatusCode = response.StatusCode,
                        StatusDescription = response.StatusDescription,
                        Header = response.Headers,
                        CookieCollection = response.Cookies,
                        Cookie = response.Headers["set-cookie"],

                    };

                    MemoryStream _stream = new MemoryStream();
                    //GZIIP处理
                    if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _stream = GetMemoryStream(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress));

                    }
                    else
                    {
                        _stream = GetMemoryStream(response.GetResponseStream());
                    }

                    //获取Byte
                    byte[] ResponseByte = _stream.ToArray();
                    _stream.Close();
                    if (ResponseByte != null & ResponseByte.Length > 0)
                    {
                        //是否返回Byte类型数据
                        if (item.ResultType == HttpResultType.StringAndByte)
                        {
                            result.ResultByte = ResponseByte;
                        }

                        #region 当编码是空的时候从mete中获取编码类型
                        if (item.Encoding == null)
                        {
                            try
                            {
                                Match meta = Regex.Match(Encoding.Default.GetString(ResponseByte), "<meta([^<]*)charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
                                string c = (meta.Groups.Count > 1) ? meta.Groups[2].Value.ToLower().Trim() : string.Empty;
                                if (c.Length > 2)
                                {
                                    try
                                    {
                                        if (c.IndexOf(" ") > 0) c = c.Substring(0, c.IndexOf(" "));
                                        {
                                            item.Encoding = Encoding.GetEncoding(c.Replace("\"", "").Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk").Trim());
                                        }
                                    }
                                    catch
                                    {
                                        if (string.IsNullOrEmpty(response.CharacterSet))
                                        {
                                            item.Encoding = Encoding.UTF8;
                                        }
                                        else
                                        {
                                            item.Encoding = Encoding.GetEncoding(response.CharacterSet);
                                        }
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(response.CharacterSet))
                                    {
                                        item.Encoding = Encoding.UTF8;
                                    }
                                    else
                                    {
                                        item.Encoding = Encoding.GetEncoding(response.CharacterSet);
                                    }
                                }
                            }
                            catch
                            {
                                item.Encoding = Encoding.Default;
                            }
                        } 
                        #endregion
                        
                        //得到返回的HTML
                        result.Html = item.Encoding.GetString(ResponseByte);
                    }
                    else
                    {
                        result.Html =string.Empty;
                    }
                }
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
                result.Html = ex.Message;
                if (response != null)
                {
                    result.StatusCode = response.StatusCode;
                    result.StatusDescription = response.StatusDescription;
                }
            }
            catch (Exception ex)
            {
                result.Html = ex.Message;
            }
            finally
            {
                if (item.IsToLower)
                {
                    result.Html = result.Html.ToLower();
                }
                try
                {
                    item.finaly?.Invoke(result);
                }
                catch
                {}
            }
            return result;
        }

        /// <summary>
        /// 初始化请求
        /// </summary>
        private void InitRequest(HttpItem item)
        {
            //证书
            SetCer(item);

            //设置Header
            if (item.Header != null && item.Header.Count > 0)
            {
                foreach (string key in item.Header.AllKeys)
                {
                    request.Headers.Add(key, item.Header[key]);
                }
            }
            request.Proxy = item.WebProxy;
            request.Method = item.Method.ToString();
            request.Timeout = item.TimeOut;
            request.KeepAlive = item.KeepAlive;
            request.ReadWriteTimeout = item.ReadWriteTimeOut;
            request.Accept = item.Accept;
            request.ContentType = item.ContentType;
            request.UserAgent = item.UserAgent;
            request.Referer = item.Referer;
            request.AllowAutoRedirect = item.AllowAutoRedirect;
            request.Credentials = item.ICredentials;
            if (item.ConnectionLimit > 0)
            {
                request.ServicePoint.ConnectionLimit = item.ConnectionLimit;
            };
            //设置Cookie
            if (!string.IsNullOrEmpty(item.Cookie))
            {
                request.Headers[HttpRequestHeader.Cookie] = item.Cookie;
            }
            //设置CookieCollection
            if (item.CookieCollection != null && item.CookieCollection.Count > 0)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(item.CookieCollection);
            }
               
            //设置Post数据
            SetPostData(item);
            
        }

        /// <summary>
        /// 设置Post数据
        /// </summary>
        /// <param name="item">Http参数</param>
        private void SetPostData(HttpItem item)
        {
            //验证在得到结果时是否有传入数据
            if (item.Method==HttpMethod.Post)
            {
                byte[] buffer = null;
                switch (item.PostDataType)
                {
                    case HttpPostDataType.String:
                        buffer = item.PostEncoding.GetBytes(item.PostData);
                        break;
                    case HttpPostDataType.Byte:
                        if (item.PostDataByte != null && item.PostDataByte.Length > 0)
                        {
                            buffer = item.PostDataByte;
                        }
                        break;
                    case HttpPostDataType.FilePath:
                        using (StreamReader r = new StreamReader(item.PostData, item.PostEncoding))
                        {
                            buffer = item.PostEncoding.GetBytes(r.ReadToEnd());
                        }
                        break;
                }
                if (buffer != null)
                {
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }
            }
        }
        
        /// <summary>
        /// 设置证书
        /// </summary>
        /// <param name="item"></param>
        private void SetCer(HttpItem item)
        {
            if (!string.IsNullOrEmpty(item.CerPath))
            {
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                //初始化对像，并设置请求的URL地址
                request = (HttpWebRequest)WebRequest.Create(item.URL);
                SetCerList(item);
                //将证书添加到请求里
                request.ClientCertificates.Add(new X509Certificate(item.CerPath));
            }
            else
            {
                //初始化对像，并设置请求的URL地址
                request = (HttpWebRequest)WebRequest.Create(item.URL);
                SetCerList(item);
            }
        }

        /// <summary>
        /// 设置多个证书
        /// </summary>
        /// <param name="item"></param>
        private void SetCerList(HttpItem item)
        {
            if (item.ClentCertificates != null && item.ClentCertificates.Count > 0)
            {
                foreach (X509Certificate c in item.ClentCertificates)
                {
                    request.ClientCertificates.Add(c);
                }
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="streamResponse">流</param>
        private MemoryStream GetMemoryStream(Stream streamResponse)
        {
            MemoryStream _stream = new MemoryStream();
            int Length = 256;
            Byte[] buffer = new Byte[Length];
            int bytesRead = streamResponse.Read(buffer, 0, Length);
            while (bytesRead > 0)
            {
                _stream.Write(buffer, 0, bytesRead);
                bytesRead = streamResponse.Read(buffer, 0, Length);
            }
            return _stream;
        }

        /// <summary>
        /// 回调验证证书问题
        /// </summary>
        /// <param name="sender">流对象</param>
        /// <param name="certificate">证书</param>
        /// <param name="chain">X509Chain</param>
        /// <param name="errors">SslPolicyErrors</param>
        /// <returns>bool</returns>
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; }
    }
}
