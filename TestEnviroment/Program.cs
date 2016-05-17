using System;
using System.Collections.Generic;
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
            //        body = "asldk",
            //        link = "asd",
            //        meta = "asdkl",
            //        magnet = " kjd",
            //        rank = 1,
            //        title = "d;l"
            //    };
            //    ms.AddAsync(doc);
            //}
            //Console.WriteLine("Wait");
            var currentId = 10;
            int t;
            t = Interlocked.Increment(ref currentId);
            Console.ReadKey();
        }
    }
}
