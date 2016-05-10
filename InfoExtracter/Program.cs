using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding enc = Encoding.GetEncoding(1251);
            RutrackerGamesParser parser = new RutrackerGamesParser();
            int i = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Parallel.ForEach(Directory.GetFiles(@"E:\RutrackerGames", "*", SearchOption.AllDirectories),
                new ParallelOptions { MaxDegreeOfParallelism = 10 },
                file =>
             {
                 if (i % 1000 == 0)
                     Console.WriteLine(i);
                 var res = parser.ParseDocument(File.ReadAllText(file, enc));
                 i++;
             });
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            var topkek = RutrackerGamesParser.cnt.OrderByDescending(x => x.Value).Take(1000);
            int a = 0;
        }
    }
}
