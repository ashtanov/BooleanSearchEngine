using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SearchEngineTools;


namespace IRLab1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string name = "index";
                string source = "";
                bool search = false;
                for (int i = 0; i < args.Length; ++i)
                {
                    switch (args[i])
                    {
                        case "-n":
                            i++;
                            name = args[i];
                            break;
                        case "-s":
                            search = true;
                            break;
                        default:
                            source = args[i];
                            break;
                    }
                }
                Index index;
                if (!search)
                {
                    List<string> docs;
                    if (source.StartsWith("http://az.lib.ru")) // http://az.lib.ru/t/tolstoj_lew_nikolaewich/text_1860_dekabristy.shtml
                    {
                        var text = WebHelper.GetLibCompositionText(new Uri(source));
                        docs = ParseHelper.DivideIntoParagraphs(text);
                        ZipfCalc.WriteStat(name + ".csv", text);
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
                    index = Index.CreateIndex(docs, out stat);
                    Console.WriteLine("term count\t{0}\ntoken count\t{1}\navg. term length\t{2}\navg. token length\t{3}\nelapsed time\t{4}",
                        stat.TermCount, stat.TokenCount, stat.TermSummaryLength / (float)stat.TermCount, stat.TokenSummaryLength / (float)stat.TokenCount, stat.CreatingTime);
                    index.Serialize(name + ".idx");
                    Console.ReadKey();
                }
                else
                {
                    index = Index.Deserialize(name + ".idx");
                    while (true)
                    {
                        string req = Console.ReadLine().Trim();
                        if (req.Equals(@"\quit"))
                            break;
                        else
                        {
                            IEnumerable<string> results;
                            if (req.StartsWith("\"") && req.EndsWith("\""))
                                results = index.QuoteSearch(req);
                            else if (req.IsDistanceQuery())
                            {
                                results = index.DistanceSearch(req);
                            }
                            else
                                results = index.Search(req);
                            if (results.Count() > 0)
                            {
                                Console.WriteLine(string.Join("\n", results.Select((x, i) => string.Format("{0}. {1}", i + 1, x))));
                            }
                            else
                                Console.WriteLine("Не найдено");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
