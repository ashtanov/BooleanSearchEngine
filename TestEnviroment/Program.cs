using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SearchEngineTools;

namespace TestEnviroment
{
    class Program
    {
        static void Main(string[] args)
        {
            //MongoStorage ms = new MongoStorage();
            //for (int i = 0; i < 100000; ++i)
            //{
            //    Document doc = new Document
            //    {
            //        extId = i,
            //        description = "asldk",
            //        link = "asd",
            //        meta = "asdkl",
            //        magnet = " kjd",
            //        rank = 1,
            //        title = "d;l"
            //    };
            //    ms.AddAsync(doc);
            //}
            //Console.WriteLine("Wait");
            //IndexCore ic = CreateIndex();
            //ic.Serialize("indexFilms.idx");
            IndexCore ic = IndexCore.Deserialize("indexFilms.idx");
            var yy1 = ic.SearchQuery("джеймс бонд");
            //var yy = yy1.Select(x => x.id).Except(yy2.Select(c => c.id));
            //ic.Serialize("abc.idx");
            //IndexCore ic2 = IndexCore.Deserialize("abc.idx");

            Console.ReadKey();
        }

        static IndexCore CreateIndex()
        {
            IndexCore ic = new IndexCore();
            for (int i = 1; i <= 6; ++i)
            {
                var file = File.ReadAllText(@"E:\films" + i + ".json");
                var t = JArray.Parse(file);
                List<Document> docs = new List<Document>();
                foreach (JObject j in t.Cast<JObject>())
                {
                    if (docs.Count % 1000 == 0)
                        Console.WriteLine(docs.Count);
                    if (j.HasValues)
                        docs.Add(j);
                }
                ic.AddRange(docs);
                Console.WriteLine("Stage" + i);
            }
            return ic;
        }

    }
}
