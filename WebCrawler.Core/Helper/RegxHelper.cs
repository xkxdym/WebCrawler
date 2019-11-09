using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WebCrawler.Model;

namespace WebCrawler.Core
{
    public static class RegxHelper
    {
        private const string regex_Meta = @"<meta[^<]*{0}=""(?<value>[^<=]+)""[^<>]*/?>";
        private const string regex_Title = @"<title>(?<title>[^<]*)</title>";
        private const string regex_A= @"<a[^<]*{0}\s*=\s*""(?<value>[^<=]+)""[^<]*>(?<text>[^<]*)</a>";
        private const string regex_A_R = @"<a[^<]*{0}\s*=\s*""(?<value>[^<=]+)""[^<]*>";
        private const string regex_Img = @"<img[^<]*{0}\s*=\s*""(?<value>[^<=]+)""[^<>]*/?>";
        
        /// <summary>
        /// 获取head 的title
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string GetTitle(this string html)
        {
            string title = string.Empty;
            if (string.IsNullOrEmpty(html))
            {
                return title;
            }
            try
            {
                var mc = Regex.Matches(html, regex_Title, RegexOptions.IgnoreCase);
                if (mc != null && mc.Count > 0)
                {
                    foreach (Match m in mc)
                    {
                        title = m.Groups["title"].Value;
                        if (!string.IsNullOrEmpty(title))
                        {
                            break;
                        }
                    }
                }
            }
            catch
            { }

            return title;
        }
        /// <summary>
        /// 获取head 的meta 信息
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<HtmlTags.META> GetMetas(this string html)
        {
            List<HtmlTags.META> list = new List<HtmlTags.META>();
            if (string.IsNullOrEmpty(html))
            {
                return list;
            }
            try
            {
                var mc = Regex.Matches(html, string.Format(regex_Meta, "name"), RegexOptions.IgnoreCase);
                if (mc != null && mc.Count > 0)
                {
                    foreach (Match m in mc)
                    {
                        var htm = m.Groups[0].Value;

                        if (string.IsNullOrEmpty(htm))
                        {
                            continue;
                        }

                        list.Add(new HtmlTags.META()
                        {
                            Name = m.Groups["value"].Value,
                            Content = htm.GetAttribute("content", regex_Meta),
                        });
                    }
                }
            }
            catch
            { }

            return list;
        }

        /// <summary>
        /// 获取页面所有的a标签
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<HtmlTags.A> GetAllTagForA(this string html,string url)
        {

            List<HtmlTags.A> list = new List<HtmlTags.A>();
            if (string.IsNullOrEmpty(html))
            {
                return list;
            }
            try
            {
                var mc= Regex.Matches(html, string.Format(regex_A, "href"), RegexOptions.IgnoreCase);
                if (mc != null && mc.Count > 0)
                {
                    foreach (Match m in mc)
                    {
                        try
                        {
                            var a = m.Groups[0].Value;

                            if (string.IsNullOrEmpty(a))
                            {
                                continue;
                            }

                            var href = m.Groups["value"].Value;
                            if (string.IsNullOrEmpty(href) || href == "/" || href.Contains("javascript") || href.Contains("void(0)"))
                            {
                                continue;
                            }

                            var base_Uri = new Uri(url);

                            if (!href.StartsWith("http"))
                            {
                                href = new Uri(base_Uri, href).AbsoluteUri;
                            }
                            //剔除不是相同域名的标签
                            var host_url = base_Uri.Host;
                            var host_href = new Uri(href).Host;
                            
                            if (!(host_url.Contains(host_href)||host_href.Contains(host_url)))
                            {
                                continue;
                            }
                            if (list.Exists(e => e.Href == href))
                            {
                                continue;
                            }
                            var text = m.Groups["text"].Value;
                            if (!string.IsNullOrEmpty(text))
                            {
                                if (text != text.HtmlToText())
                                {
                                    text = text.Length > 15 ? text.Substring(0, 15) : text;
                                }
                            }
                            var title = a.GetAttribute("title", regex_A);

                            if (string.IsNullOrEmpty(text + title))
                            {
                                continue;
                            }

                            list.Add(new HtmlTags.A()
                            {
                                Href = href,
                                Title = title,
                                Text = text
                            });
                        }
                        catch
                        {
                            continue;
                        }
                    }

                }
            }
            catch
            {}
            try
            {
                var mc = Regex.Matches(html, string.Format(regex_A_R, "href"), RegexOptions.IgnoreCase);
                if (mc != null && mc.Count > 0)
                {
                    foreach (Match m in mc)
                    {
                        try
                        {
                            var a = m.Groups[0].Value;

                            if (string.IsNullOrEmpty(a))
                            {
                                continue;
                            }

                            var href = m.Groups["value"].Value;
                            if (string.IsNullOrEmpty(href) || href == "/" || href.Contains("javascript") || href.Contains("void(0)"))
                            {
                                continue;
                            }

                            var base_Uri = new Uri(url);

                            if (!href.StartsWith("http"))
                            {
                                href = new Uri(base_Uri, href).AbsoluteUri;
                            }
                            //剔除不是相同域名的标签
                            var host_url = base_Uri.Host;
                            var host_href = new Uri(href).Host;

                            if (!(host_url.Contains(host_href) || host_href.Contains(host_url)))
                            {
                                continue;
                            }
                            if (list.Exists(e => e.Href == href))
                            {
                                continue;
                            }
                           
                            var title = a.GetAttribute("title", regex_A);

                            if (string.IsNullOrEmpty(title))
                            {
                                title = "[未知]";
                            }

                            list.Add(new HtmlTags.A()
                            {
                                Href = href,
                                Title = title
                            });
                        }
                        catch
                        {
                            continue;
                        }
                    }

                }
            }
            catch
            { }

            return list;

        }
        /// <summary>
        /// 获取页面所有的img标签
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<HtmlTags.Img> GetAllTagForImg(this string html,string url)
        {

            List<HtmlTags.Img> list = new List<HtmlTags.Img>();
            if (string.IsNullOrEmpty(html))
            {
                return list;
            }
            try
            {
                var mc = Regex.Matches(html, string.Format(regex_Img, "src"), RegexOptions.IgnoreCase);
                if (mc != null && mc.Count > 0)
                {
                    foreach (Match m in mc)
                    {
                        try
                        {
                            var htm = m.Groups[0].Value;

                            if (string.IsNullOrEmpty(htm))
                            {
                                continue;
                            }
                            var src = m.Groups["value"].Value;


                            if (!Regex.IsMatch(src, @"\w+\.(gif|jpg|bmp|png)$"))
                            {
                                continue;
                            }

                            if (!src.StartsWith("http"))
                            {
                                var baseUri = new Uri(url);
                                src = src.Trim('/').Replace(baseUri.Host, string.Empty);
                                var imgSrc = new Uri(baseUri, src);

                                src = imgSrc.AbsoluteUri;
                            }

                            if (list.Exists(e => e.Src == src))
                            {
                                continue;
                            }
                            list.Add(new HtmlTags.Img()
                            {
                                Src = src,
                                Alt = htm.GetAttribute("alt", regex_Img)
                            });
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch
            { }
            return list;
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="html"></param>
        /// <param name="attribute"></param>
        /// <param name="pattren"></param>
        /// <returns></returns>
        public static string GetAttribute(this string html,string attribute,string pattren)
        {
            var value =string.Empty;
            if (string.IsNullOrEmpty(html))
            {
                return value;
            }
            try
            {
                var mc = Regex.Matches(html, string.Format(pattren, attribute), RegexOptions.IgnoreCase);
                if (mc != null && mc.Count > 0)
                {
                    foreach (Match m in mc)
                    {
                        value= m.Groups["value"].Value;
                        if (!string.IsNullOrEmpty(value))
                        {
                            break;
                        }
                    }
                }
            }
            catch {}

            return value;
        }
    }
}
