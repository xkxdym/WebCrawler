using System;
using System.Collections.Generic;

namespace WebCrawler.Model
{
    /// <summary>
    /// html 页面模型
    /// </summary>
    [Serializable]
    public class HtmlModel
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///元信息
        /// </summary>
        public List<HtmlTags.META> Meta { get; set; }
        
        /// <summary>
        ///链接 
        /// </summary>
        public List<HtmlTags.A> LinkList { get; set; } 

        /// <summary>
        /// 图片
        /// </summary>
        public List<HtmlTags.Img> ImageList { get; set; }
    }


    /// <summary>
    /// html 标签
    /// </summary>
    [Serializable]
    public class HtmlTags
    {
        public class META : TagBase
        {
            public string Name { get; set; }
            public string Content { get; set; }
        }

        public class A : TagBase
        {
            public string Href { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
        }

        public class Img : TagBase
        {
            public string Src { get; set; }
            public string Alt { get; set; }
        }

    }

    /// <summary>
    /// 标签基类
    /// </summary>
    public class TagBase
    {
        //public string Id { get; set; }
    }
}
