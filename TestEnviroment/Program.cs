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
using InfoExtractor;

namespace TestEnviroment
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestAverageTime();

            CreateIndex(
                fromDirectiory: @"E:\SourceFiles",
                saveToFile: @"E:\indexShort.idx"
            );
            //ClassifyFeature(@"E:\SourceFiles");

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

        //static void ClassifyFeature(string fromDirectiory)
        //{

        //    CounterSet<string> qual = new CounterSet<string>();
        //    CounterSet<string> lang = new CounterSet<string>();
        //    CounterSet<string> keygen = new CounterSet<string>();
        //    Dictionary<string, double> qual1 = new Dictionary<string, double>();
        //    Dictionary<string, double> lang1 = new Dictionary<string, double>();
        //    Dictionary<string, double> keygen1 = new Dictionary<string, double>();
        //    foreach (var file in Directory.GetFiles(fromDirectiory).Reverse())
        //    {
        //        var t = JArray.Load(new JsonTextReader(new StreamReader(file)));
        //        List<Document> docs = new List<Document>();
        //        foreach (JObject j in t.Cast<JObject>())
        //            if (j.HasValues)
        //                docs.Add(j);
        //        foreach (var d in docs)
        //        {
        //            if (!string.IsNullOrWhiteSpace(d.qual))
        //            {
        //                var k = d.qual.ToUpper().ReplaceAll(toTrim);
        //                if (!qual1.ContainsKey(k))
        //                {
        //                    qual1.Add(k, QualMap(k));
        //                }
        //            }
        //            if (!string.IsNullOrWhiteSpace(d.keygen))
        //            {
        //                var k = d.keygen.ToUpper().ReplaceAll(toTrim);
        //                if (!keygen1.ContainsKey(k))
        //                {
        //                    keygen1.Add(k, KeygenMap(k));
        //                }
        //            }
        //            if (!string.IsNullOrWhiteSpace(d.lang))
        //            {
        //                var k = d.lang.ToUpper().ReplaceAll(toTrim);
        //                if (!lang1.ContainsKey(k))
        //                {
        //                    lang1.Add(k, LangMap(k));
        //                }
        //            }
        //        }
        //    }
        //    double maxLang = lang1.Max(x => x.Value);
        //    double minLang = lang1.Min(x => x.Value);
        //    //var sortQual = qual.OrderByDescending(x => x.Value);
        //    //var sortLang = lang.OrderByDescending(x => x.Value);
        //    //var sortKeygen = keygen.OrderByDescending(x => x.Value);

        //    int a = 0;

        //}


        //static string LangNorm(string s)
        //{
        //    s = s.ToUpper().ReplaceAll(toTrim);
        //    if (s.ContainsAny("ДУБЛ", "ПРОФ", "МНОГ", "РУССКИ", "УБТИТ", "НЕТРЕБ", "RUS", "ENG", "НГЛ", "ЛЮБИТ", "ОДНОГОЛОС", "ОТСУТСТВУ", "НЕВАЖНО", "ДВУХГОЛ", "ОРИГИН", "ПОНСК", "АВТОРСК", "JAP", "НЕТ"))
        //        s = "OK";
        //    return s;
        //}

        //static string KeygenNorm(string s)
        //{
        //    s = s.ToUpper().ReplaceAll(toTrim);
        //    if (s.ContainsAny("ЕТРЕБ", "РИСУТ", "ОТСУТ", "ШИТА", "ЭМУЛ", "CODEX", "RELOAD", "ЕСТЬ", "НЕТ", "НЕНУЖ", "ЭТРЭБУЕ", "DRMFREE", "ЛЕЧ", "ОБРАЗ", "СЕРИ", "STEAM"))
        //        s = "OK";
        //    return s;
        //}

        //static string QualNorm(string s)
        //{
        //    s = s.ToUpper().ReplaceAll(toTrim);
        //    if (s.ContainsAny("DVDRIP", "DVD5", "DVD9", "BDRIP", "TVRIP", "HDTV", "SATRIP", "HDRIP", "VHS", "DVB", "WEBDV", "WEBDL", "BLURAY", "CAM", "WEBRIP", "BDREMUX", "TS"))
        //        s = "OK";
        //    return s;
        //}

        //static double LangMap(string s)
        //{
        //    s = s.ToUpper().ReplaceAll(toTrim);
        //    double accum = -1;
        //    if (s.ContainsAny("ДУБЛ"))
        //        accum += 5;
        //    if (s.ContainsAny("ПРОФ"))
        //        accum += 4;
        //    if (s.ContainsAny("МНОГ"))
        //        accum += 3;
        //    if (s.ContainsAny("РУССКИ", "RUS", "НЕТРЕБ", "НЕВАЖН"))
        //        accum += 2;
        //    if (s.ContainsAny("ДВУХГОЛ"))
        //        accum += 2;
        //    if (s.ContainsAny("УБТИТ", "САБ", "SUB"))
        //        accum += 1;
        //    if (s.ContainsAny("ПОНСК", "JAP"))
        //        accum += 1;
        //    if (s.ContainsAny("ОДНОГОЛОС", "АВТОРСК"))
        //        accum += 1;
        //    if (s.ContainsAny("ENG", "НГЛ", "ОТСУТСТВУ", "ОРИГИН", "НЕТ"))
        //        accum += 0;
        //    if (s.ContainsAny("ЛЮБИТ"))
        //        accum += -1;
        //    return (accum + 2) / 18;
        //}

        //static double QualMap(string s)
        //{
        //    s = s.ToUpper().ReplaceAll(toTrim);
        //    int start = -1;
        //    if (s.Contains("CAM"))
        //        start += -5;
        //    if (s.Contains("TS"))
        //        start += -3;
        //    if (s.Contains("VHS"))
        //        start += -1;
        //    if (s.ContainsAny("TVRIP", "SATRIP", "DVB", "WEBDV", "WEBRIP"))
        //        start += 0;
        //    if (s.Contains("DVDRIP"))
        //        start += 1;
        //    if (s.ContainsAny("HDRIP", "HDTVRIP", "BDRIP"))
        //        start += 2;
        //    if (s.ContainsAny("DVD5", "DVD9", "WEBDL"))
        //        start += 3;
        //    if (s.ContainsAny("BLURAY", "REMUX"))
        //        start += 4;
        //    return (start + 10.0) / 19;
        //}

        //static double KeygenMap(string s)
        //{
        //    s = s.ToUpper().ReplaceAll(toTrim);
        //    if (s.ContainsAny("ЕТРЕБ", "DRMFREE", "НЕНУЖ", "ЭТРЭБУЕ"))
        //        return 1;
        //    if (s.ContainsAny("ЛЕЧ", "ШИТА"))
        //        return 0.75;
        //    if (s.ContainsAny("РИСУТ", "CODEX", "RELOAD", "ЕСТЬ", "СЕРИ", "STEAM"))
        //        return 0.5;
        //    if (s.ContainsAny("ЭМУЛ", "ОБРАЗ"))
        //        return 0.25;
        //    if (s.ContainsAny("BLURAY", "REMUX"))
        //        return 0;
        //    return 0.1;
        //}

    }
}
