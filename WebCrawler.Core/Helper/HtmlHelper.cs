using System;
using System.Linq;
using System.Text.RegularExpressions;
using WebCrawler.Model;

namespace WebCrawler.Core
{
    public static class HtmlHelper
    {
        public static HtmlModel ParseHtml(this string html,string url=null,string description=null)
        {
            HtmlModel model = null;
            if (string.IsNullOrEmpty(html))
            {
                return model;
            }
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    var uri = new Uri(url);
                    url = $"{uri.Scheme}://{uri.Host}";
                }
                
                model = new HtmlModel()
                {
                    Title=html.GetTitle(),
                    Meta=html.GetMetas(),
                    LinkList=html.GetAllTagForA(url),
                    ImageList=html.GetAllTagForImg(url)
                };
            }
            catch
            {}
            finally
            {
                #region Title 处理
                try
                {
                    if (model != null)
                    {
                        if (string.IsNullOrEmpty(model.Title))
                        {
                            if (model.Meta.Exists(f => f.Name.ToLower() == "description"))
                            {
                                model.Title = model.Meta.FirstOrDefault(f => f.Name.ToLower() == "description").Content;
                            }
                            else
                            {
                                model.Title = description;
                            }
                        }
                        if (!string.IsNullOrEmpty(model.Title))
                        {
                            if (model.Title.Contains("("))
                            {
                                model.Title = model.Title.Substring(0, model.Title.IndexOf('('));
                            }
                            if (model.Title.Contains("（"))
                            {
                                model.Title = model.Title.Substring(0, model.Title.IndexOf('（'));
                            }
                        }
                    }
                }
                catch
                { } 
                #endregion
            }

            return model;
        }

        /// <summary>
        /// HTML转纯文本
        /// </summary>
        public static string HtmlToText(this string html)
        {
            string regexstr = @"(&(#)?.+;)|(<[^>]*>)";
            return Regex.Replace(html, regexstr, "", RegexOptions.IgnoreCase);
        }
    }
}
