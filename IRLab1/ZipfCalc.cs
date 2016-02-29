using SearchEngineTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRLab1
{
    public class ZipfCalc
    {
        public static void WriteStat(string filePath, string text)
        {
            Dictionary<string, int> resStat = new Dictionary<string, int>();
            foreach (var p in ParseHelper.DivideIntoParagraphs(text.ToUpper()))
            {
                foreach (var w in ParseHelper.FindAllWords(p))
                {
                    int cur;
                    if (resStat.TryGetValue(w, out cur))
                        resStat[w] = cur + 1;
                    else
                        resStat.Add(w, 1);
                }
            }
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath, false, Encoding.UTF8))
            {
                int i = 1;
                foreach (var kvp in resStat.OrderByDescending(x => x.Value))
                {
                    sw.WriteLine("{0};{1};{2};{3}", i, kvp.Value, kvp.Key,i * kvp.Value);
                    i++;
                }
            }
        }
    }
}
