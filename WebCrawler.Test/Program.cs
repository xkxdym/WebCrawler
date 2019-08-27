using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WebCrawler.Core;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "ImgLoad";
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(@"Type Description:
    Type 1: Get All Link And Images By Depth!
    Type 2: Get All Images By Url Range!");
            Console.ForegroundColor = ConsoleColor.Green;
            string type = string.Empty;
            while (string.IsNullOrEmpty(type)||!new List<string>() { "1","2"}.Contains(type))
            {
                Console.WriteLine("Please Choose The Type:");
                type = Console.ReadLine();
            }

            Console.WriteLine(@"Is Group By Size（1-Y,2-N）：");
            Console.ForegroundColor = ConsoleColor.Green;
            string isGroupBySize = string.Empty;
            bool groupBySize = false;
            while (string.IsNullOrEmpty(isGroupBySize) || !new List<string>() { "1", "2" }.Contains(isGroupBySize))
            {
                isGroupBySize = Console.ReadLine();
                if (isGroupBySize == "1")
                {
                    groupBySize = true;
                }
            }

            Console.WriteLine(@"Is Group Pictures（1-Y,2-N）：");
            Console.ForegroundColor = ConsoleColor.Green;
            string isGroupPic = string.Empty;
            bool groupPic = false;
            while (string.IsNullOrEmpty(isGroupPic) || !new List<string>() { "1", "2" }.Contains(isGroupPic))
            {
                isGroupPic = Console.ReadLine();
                if (isGroupPic == "1")
                {
                    groupPic = true;
                }
            }

            string url = string.Empty;
            while (string.IsNullOrEmpty(url)||!url.StartsWith("http"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("URL Description:");
                Console.WriteLine("    1:URL Must Start With Http!】");
                if (type != "1")
                {
                    Console.WriteLine("    2:URL Must Contains The Tags '{data}'!】");
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Please Enter URL:");
                url = Console.ReadLine();
                if (type == "2"&&!url.Contains("{data}"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("UnLegal URL!");
                    url = string.Empty;
                }
            }

            string depthStr = string.Empty;
            int depth = 1;

            string startStr = string.Empty;
            string endStr = string.Empty;
            int start = 0,end=0;
            Console.ForegroundColor = ConsoleColor.Green;
            if (type == "1")
            {
                while (string.IsNullOrEmpty(depthStr))
                {
                    Console.WriteLine("Please Enter Depth:");
                    depthStr = Console.ReadLine();
                    try
                    {
                        depth = Convert.ToInt32(depthStr);
                    }
                    catch
                    {
                        depthStr = string.Empty;
                    }
                }
            }
            else
            {
                while (string.IsNullOrEmpty(startStr))
                {
                    Console.WriteLine("Please Enter StartIndex:");
                    startStr = Console.ReadLine();
                    try
                    {
                        start = Convert.ToInt32(startStr);
                    }
                    catch
                    {
                        startStr = string.Empty;
                    }
                }
                while (string.IsNullOrEmpty(endStr))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Please Enter EndIndex:");
                    endStr = Console.ReadLine();
                    try
                    {
                        end = Convert.ToInt32(endStr);
                    }
                    catch
                    {
                        endStr = string.Empty;
                    }
                    if (end < start)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("【EndIndex Can Not Less StartIndex!】");
                        endStr = string.Empty;
                    }
                }
            }
            


            string path = AppDomain.CurrentDomain.BaseDirectory + "/Logs/"+DateTime.Now.ToString("yyyyMMdd")+"/";
            DirectoryInfo info = new DirectoryInfo(path);
            if (!info.Exists)
            {
                info.Create();
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("《---------------START----------------》");
            Console.ForegroundColor = ConsoleColor.Green;

            if (type == "1")
            {
                DownLoadProvider.Instance.DownLoad(url, depth,groupBySize,groupPic, (msg) =>
                {
                    File.AppendAllLines($"{path}{DateTime.Now.ToString("yyMMdd HHmm")}.log", new List<string>() { msg, Environment.NewLine }, Encoding.Default);
                    Console.WriteLine(msg);
                }, (model) =>
                {
                    var json = JsonConvert.SerializeObject(model);
                    File.AppendAllText($"{path}{DateTime.Now.ToString("yyMMdd HHmmss")}.json", json, Encoding.Default);
                });

                Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("《---------------END----------------》");
                Console.WriteLine("Enter Any Key To Exit!");
            }
            else
            {
                DownLoadProvider.Instance.DownLoadByUrlPattern(url, start,end,groupBySize,groupPic, (msg) =>
                {
                    File.AppendAllLines($"{path}{DateTime.Now.ToString("yyMMdd HHmm")}.log", new List<string>() { msg, Environment.NewLine }, Encoding.Default);
                    Console.WriteLine(msg);
                }, () =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("《---------------END----------------》");
                    Console.WriteLine("Enter Any Key To Exit!");
                });
            }
            Console.ReadLine();
        }
    }
}
