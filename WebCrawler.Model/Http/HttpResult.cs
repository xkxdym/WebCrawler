using System.Net;

namespace WebCrawler.Model.Http
{
    /// <summary>
    /// Http请求返回参数类
    /// </summary>
    public class HttpResult
    {
        /// <summary>
        /// 返回的String类型数据
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// 返回的Byte数组
        /// </summary>
        public byte[] ResultByte { get; set; }

        /// <summary>
        /// header对象
        /// </summary>
        public WebHeaderCollection Header { get; set; }

        /// <summary>
        /// Http请求返回的Cookie
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        /// Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection { set; get; }

        /// <summary>
        /// 返回状态说明
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// 返回状态码,默认为OK
        /// </summary>
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    }
}
