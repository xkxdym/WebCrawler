using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebCrawler.Model;
using WebCrawler.Model.Http;

namespace WebCrawler.Core
{
    public class DownLoadProvider
    {
        //所有找到的链接
        private static ConcurrentDictionary<string, int> allHrefDic = new ConcurrentDictionary<string, int>();
        //已经访问的链接
        private static ConcurrentDictionary<string, int> requiedHrefDic = new ConcurrentDictionary<string, int>();
        //所有找到的图片
        private static ConcurrentDictionary<string, string> allImgDic = new ConcurrentDictionary<string, string>();
        //下载队列
        private static ConcurrentQueue<HtmlTags.Img> imgQueue = new ConcurrentQueue<HtmlTags.Img>();

        private static DownLoadProvider _Load = null;
        public static DownLoadProvider Instance
        {
            get
            {
                if (_Load == null)
                {
                    _Load = new DownLoadProvider();
                }
                return _Load;
            }
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="depth">深度 最大50</param>
        /// <param name="msgAction">输出的消息</param>
        public void DownLoad(string url, int depth = 2, Action<string> msgAction = null, Action<HttpRequestModel> finaly = null)
        {
            HttpRequestModel model = new HttpRequestModel()
            {
                Url = url,
                Depth = 1
            };
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    msgAction?.Invoke("url is can not null");
                    return;
                }
                if (depth < 0)
                {
                    depth = 1;
                }
                if (depth > 50)
                {
                    msgAction?.Invoke("depth is max 50 ");
                    return;
                }
                GetModel(model, depth, msgAction);
            }
            catch (Exception ex)
            {
                msgAction?.Invoke("Error:" + ex.Message);
            }
            finally
            {
                finaly?.Invoke(model);
            }
        }

        public void DownLoadByUrlPattern(string url,int startIndex,int endIndex,Action<string> msgAction = null, Action finaly = null)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    msgAction?.Invoke("url can not null");
                    return;
                }

                if (!url.Contains("{data}"))
                {
                    msgAction?.Invoke("url can not without '{data}' tags");
                    return;
                }

                if (startIndex > endIndex)
                {
                    msgAction?.Invoke("endIndex can not < startIndex");
                    return;
                }
                
                while (startIndex <= endIndex)
                {
                    Task task = new Task(() =>
                    {
                        GetModel(new HttpRequestModel()
                        {
                            Url = url.Replace("{data}", (startIndex++).ToString()),
                            Depth = 1
                        }, 1, msgAction);
                    });
                    task.Start();
                    task.Wait();
                }
            }
            catch (Exception ex)
            {
                msgAction?.Invoke("Error:" + ex.Message);
            }
            finally
            {
                finaly?.Invoke();
            }
        }


        /// <summary>
        /// 获取模型
        /// </summary>
        /// <param name="url"></param>
        /// <param name="maxDepth"></param>
        /// <returns></returns>
        private bool GetModel(HttpRequestModel model, int maxDepth, Action<string> msgAction = null)
        {
            string msg = string.Empty;

            try
            {
                if (maxDepth < 1)
                {
                    msg = $"{model.Url}:depth error!";
                    return false;
                }
                if (model.Depth > maxDepth)
                {
                    return false;
                }
                int v = 0;
                if (requiedHrefDic.TryGetValue(model.Url, out v))
                {
                    return false;
                }

                //当前请求的描述（A 的 title）
                var description = string.Empty;
                if (model.HtmlModel != null)
                {
                    description = model.HtmlModel.Title;
                }

                HttpHelper.Instance.GetResult(new HttpItem()
                {
                    URL = model.Url,
                    ResultType = HttpResultType.String,
                    finaly = (httpResult) =>
                    {
                        if (httpResult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            requiedHrefDic.TryAdd(model.Url, model.Depth);

                            var htmlModel = httpResult.Html.ParseHtml(model.Url, description);

                            if (htmlModel != null && htmlModel.LinkList != null)
                            {
                                var list = new List<HtmlTags.A>();
                                list.AddRange(htmlModel.LinkList);

                                list.ForEach(f =>
                                {
                                    try
                                    {
                                        if (allHrefDic.TryGetValue(f.Href, out v))
                                        {
                                            htmlModel.LinkList.Remove(f);
                                        }
                                        else
                                        {
                                            allHrefDic.TryAdd(f.Href, model.Depth);
                                        }
                                    }
                                    catch
                                    { }
                                });
                            }
                            model.HtmlModel = htmlModel;

                            msgAction?.Invoke($"【Thread {Thread.CurrentThread.ManagedThreadId}】{DateTime.Now.ToString()} URl({model.Url}),Depth({model.Depth}) 获取数据完成,结果成功(链接数量{htmlModel.LinkList?.Count ?? 0},图片数量{htmlModel.ImageList?.Count ?? 0})!");

                            Task downLoadTask = new Task(() =>
                            {
                                ImageFilter.Invoke(htmlModel, msgAction);
                                DownLoadImage.Invoke(msgAction);
                            });
                            downLoadTask.Start();

                            Task getChildTask = new Task(()=> 
                            {
                                GetChildModels(model, maxDepth, msgAction);
                            });
                            getChildTask.Start();
                        }
                        else
                        {
                            msgAction?.Invoke($"【Thread {Thread.CurrentThread.ManagedThreadId}】{DateTime.Now.ToString()} URl({model.Url}),Depth({model.Depth}) 获取数据完成,结果失败({httpResult.StatusDescription})！");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                msgAction?.Invoke($"【Thread {Thread.CurrentThread.ManagedThreadId}】{DateTime.Now.ToString()} URl({model.Url}),Depth({model.Depth}) Error-->{ex.Message}！");
            }

            return true;
        }

        /// <summary>
        /// 获取子集模型
        /// </summary>
        /// <param name="model"></param>
        /// <param name="maxDepth"></param>
        private void GetChildModels(HttpRequestModel model, int maxDepth, Action<string> msgAction = null)
        {
            try
            {
                if (model == null || model.HtmlModel.LinkList == null || model.HtmlModel.LinkList.Count == 0)
                {
                    return;
                }
                int depth = model.Depth + 1;

                model.HtmlModel.LinkList.ForEach(item =>
                {
                    try
                    {
                        var url = item.Href;
                        if (url.Contains("http"))
                        {
                            if (!string.IsNullOrEmpty(url))
                            {
                                HttpRequestModel chilModel = new HttpRequestModel()
                                {
                                    Url = url,
                                    HtmlModel = new HtmlModel()
                                    {
                                        Title = string.IsNullOrEmpty(item.Title) ? item.Text : item.Title
                                    },
                                    Depth = depth
                                };
                                if (GetModel(chilModel, maxDepth, msgAction))
                                {
                                    model.Childrens.Add(chilModel);
                                    Thread.Sleep(100);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    { }
                });
            }
            catch
            { }
        }

        /// <summary>
        /// 筛选
        /// </summary>
        private Action<HtmlModel, Action<string>> ImageFilter = (model, msgAction) =>
        {
            if (model != null && model.ImageList != null)
            {
                try
                {
                    int index = 0;

                    foreach (var item in model.ImageList)
                    {

                        var title = string.Empty;
                        if (allImgDic.TryGetValue(item.Src, out title))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(item.Alt))
                        {
                            item.Alt = (++index).ToString();
                        }

                        item.Alt = Regex.Replace(item.Alt, @"[*/\<>""?:|]", string.Empty, RegexOptions.IgnoreCase);
                        model.Title = Regex.Replace(model.Title, @"[*/\<>""?:|]", string.Empty, RegexOptions.IgnoreCase);

                        allImgDic.TryAdd(item.Src, model.Title);
                        imgQueue.Enqueue(item);
                    }
                }
                catch
                {
                }
            }
        };

        /// <summary>
        /// 下载
        /// </summary>
        private static Action<Action<string>> DownLoadImage = (msgAction) =>
        {

            try
            {
                HtmlTags.Img img = null;
                while (imgQueue.TryDequeue(out img))
                {
                    try
                    {
                        if (!allImgDic.ContainsKey(img.Src))
                        {
                            continue;
                        }

                        var title = allImgDic[img.Src];

                        HttpHelper.Instance.GetResult(new HttpItem()
                        {
                            URL = img.Src,
                            ResultType = HttpResultType.StringAndByte,
                            finaly = httpResult =>
                            {
                                if (httpResult.StatusCode == HttpStatusCode.OK)
                                {
                                    if (httpResult.ResultByte != null)
                                    {
                                        var sizeStr = string.Empty;
                                        var size = httpResult.ResultByte.Length;
                                        if (size < 100 * 1024)
                                        {
                                            sizeStr = "100KB以内";
                                        }
                                        else if (size < 200 * 1024)
                                        {
                                            sizeStr = "100-200KB";
                                        }
                                        else if (size < 500 * 1024)
                                        {
                                            sizeStr = "200-500KB";
                                        }
                                        else if (size < 1 * 1024 * 1024)
                                        {
                                            sizeStr = "500KB-1M";
                                        }
                                        else if (size < 1.5 * 1024 * 1024)
                                        {
                                            sizeStr = "1-1.5M";
                                        }
                                        else
                                        {
                                            sizeStr = "1.5M以上";
                                        }
                                        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("yyyyMMdd"), sizeStr, title);
                                        DirectoryInfo info = new DirectoryInfo(path);
                                        if (!info.Exists)
                                        {
                                            info.Create();
                                        }

                                        var extsion = Path.GetExtension(img.Src);
                                        var imgName = img.Alt + extsion;

                                        if (info.GetFiles().Length > 0)
                                        {
                                            if (info.GetFiles().Any(a => a.Name == imgName))
                                            {
                                                imgName = Guid.NewGuid().ToString().Replace('-', ' ').ToLower() + extsion;
                                            }
                                        }

                                        var imgSavePath = Path.Combine(path, imgName);

                                        using (FileStream stream = new FileStream(imgSavePath, FileMode.Create))
                                        {
                                            stream.Write(httpResult.ResultByte, 0, httpResult.ResultByte.Length);
                                        }

                                        msgAction?.Invoke($"【Thread {Thread.CurrentThread.ManagedThreadId}】[{title}]--[{img.Alt}] 下载完成");

                                        try
                                        {
                                            allImgDic[title] = "SUCCESS";
                                        }
                                        catch
                                        {}
                                        if (info.GetFiles().Length == 0)
                                        {
                                            info.Delete();
                                        }
                                    }
                                    else
                                    {
                                        msgAction?.Invoke($"【Thread {Thread.CurrentThread.ManagedThreadId}】[{title}]--[{img.Alt}] Error:没有返回数据！");
                                    }
                                }
                                else
                                {
                                    msgAction?.Invoke($"【Thread {Thread.CurrentThread.ManagedThreadId}】[{title}]--[{img.Alt}] Error:{httpResult.StatusDescription}");
                                }
                            }
                        });
                        Thread.Sleep(100);
                    }
                    catch
                    { }
                }
            }
            catch
            {
            }
        };
    }
}
