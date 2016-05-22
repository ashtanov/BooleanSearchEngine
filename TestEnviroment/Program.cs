using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SearchEngineTools;
using System.Text.RegularExpressions;

namespace TestEnviroment
{
    class Program
    {
        static void Main(string[] args)
        {


            //Regex r = new Regex(@"\w+[\'-]?\w+");
            //var uu = r.Matches("Регулярные выражения - это шаблоны используемые для сопоставления последовательностей символов в строках. В JavaScript, регулярные выражения тоже объекты. Эти шаблоны используются в методах exec и test объекта RegExp, а также match, replace, search, и split объекта String. Данная глава описывает регулярные выражения в JavaScript. lskaldka'dkd пиздык-призык, 'ldldl'");

            //TestAverageTime();

            CreateIndex(
                fromDirectiory: @"E:\SourceFiles",
                saveToFile: @"E:\indexShort.idx"
            );

            Console.WriteLine("Нажмите любую клавишу для выхода");
            Console.ReadKey();
        }

        private static void TestAverageTime()
        {
            TimeSpan total = new TimeSpan();
            for (int i = 0; i < 1000; ++i)
            {
                HttpWebRequest wr =
                    (HttpWebRequest)
                        HttpWebRequest.Create(
                            @"http://iwa.local/SearchService.svc/find?query=Люди%20Икс%20первый%20класс&debug=true");
                dynamic yy = JObject.Load(new JsonTextReader(new StreamReader(wr.GetResponse().GetResponseStream())));
                string elapsed = (string)yy.elapsedTime;
                TimeSpan ts = TimeSpan.FromSeconds(double.Parse(elapsed.Substring(2, elapsed.Length - 3).Replace('.', ',')));
                total += ts;
                Console.WriteLine(ts);
            }
            total = TimeSpan.FromSeconds(total.TotalSeconds / 1000);
            Console.WriteLine(total);
        }

        static void CreateIndex(string fromDirectiory, string saveToFile)
        {
            Console.WriteLine("Создать индекс?");
            var line = Console.ReadLine();
            if (line == "yes" || line == "y")
            {
                IndexCore ic = new IndexCore();
                Stopwatch sw = new Stopwatch();
                TimeSpan total = new TimeSpan();
                foreach (var file in Directory.GetFiles(fromDirectiory).Reverse())
                {
                    sw.Restart();
                    Console.WriteLine("Load: {0}", Path.GetFileName(file));
                    var t = JArray.Load(new JsonTextReader(new StreamReader(file)));
                    List<Document> docs = new List<Document>();
                    foreach (JObject j in t.Cast<JObject>())
                        if (j.HasValues)
                            docs.Add(j);
                    ic.AddRange(docs);
                    total += sw.Elapsed;
                    Console.WriteLine("Complete: {0}, elapsed: {1}", Path.GetFileName(file), sw.Elapsed);
                }
                Console.WriteLine("Total: {0}", total);
                ic.Serialize(saveToFile);
                Console.WriteLine("Serializatin complete: {0}", saveToFile);
            }
        }

    }
}
