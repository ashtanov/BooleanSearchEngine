using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRLab1
{
    class Program
    {
        static void Main(string[] args)
        {
            var index = Index.CreateIndex(File.ReadAllText(@"C:\book1.txt"));
            var tt = index.Search("!пьер&!кобыла");
            //
            int a = 0;
        }
    }
}
