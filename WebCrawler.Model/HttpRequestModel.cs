using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Model
{
    /// <summary>
    /// http请求模型
    /// </summary>
    public class HttpRequestModel
    {
        /// <summary>
        /// 请求的Url地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 请求的深度
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// 返回的页面模型
        /// </summary>
        public HtmlModel HtmlModel { get; set; }

        /// <summary>
        /// 子请求
        /// </summary>
        public List<HttpRequestModel> Childrens { get; set; } = new List<HttpRequestModel>();

    }
}
