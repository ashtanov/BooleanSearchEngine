﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IRLab1
{
    class Program
    {
        static void Main(string[] args)
        {
            try {
                string name = "index.idx";
                string source = "";
                for (int i = 0; i < args.Length; ++i)
                {
                    switch (args[i])
                    {
                        case "-n":
                            i++;
                            name = args[i] + ".idx";
                            break;
                        default:
                            source = args[i];
                            break;
                    }
                }
                List<string> docs;
                Stopwatch time = new Stopwatch();
                if (source.StartsWith("http://az.lib.ru")) // http://az.lib.ru/t/tolstoj_lew_nikolaewich/text_1860_dekabristy.shtml
                {
                    var req = (HttpWebRequest)WebRequest.Create(source);
                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(req.GetResponse().GetResponseStream());
                    var yy = doc.DocumentNode.InnerText.Replace("&nbsp;", " ").Replace("\r", "");
                    time.Start();
                    docs = yy.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                else if (source.StartsWith("http"))
                {
                    Console.WriteLine("Не поддерживается!");
                    return;
                }
                else
                {
                    docs = File.ReadAllLines(source).ToList();
                }
                Statistic stat;
                var index = Index.CreateIndex(docs, out stat);
                time.Stop();
                Console.WriteLine("term count\t{0}\ntoken count\t{1}\navg. term length\t{2}\navg. token length\t{3}\nelapsed time\t{4}",
                    stat.TermCount, stat.TokenCount, stat.TermSummaryLength / (float)stat.TermCount, stat.TokenSummaryLength / (float)stat.TokenCount, time.Elapsed);
                index.Serialize(name);
                index = Index.Deserialize(name);
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
