using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLPLabs
{
    class Program
    {
        static Regex r = new Regex(@"[а-яА-ЯЁёA-Za-z]+", RegexOptions.Compiled);
        private static Dictionary<string, Dictionary<string, int>> biDictionary;
        private static Dictionary<string, int> wordsFreq;
        static double awc = 0;
        static double bic = 0;

        private static HashSet<string> stopWords = new HashSet<string>(new[] { "и", "в", "во", "не", "что", "он", "на", "я", "с", "со", "как", "а", "то", "все", "она", "так", "его", "но", "да", "ты", "к", "у", "же", "вы", "за", "бы", "по", "только", "ее", "мне", "было", "вот", "от", "меня", "еще", "нет", "о", "из", "ему", "теперь", "когда", "даже", "ну", "вдруг", "ли", "если", "уже", "или", "ни", "быть", "был", "него", "до", "вас", "нибудь", "опять", "уж", "вам", "ведь", "там", "потом", "себя", "ничего", "ей", "может", "они", "тут", "где", "есть", "надо", "ней", "для", "мы", "тебя", "их", "чем", "была", "сам", "чтоб", "без", "будто", "чего", "раз", "тоже", "себе", "под", "будет", "ж", "тогда", "кто", "этот", "того", "потому", "этого", "какой", "совсем", "ним", "здесь", "этом", "один", "почти", "мой", "тем", "чтобы", "нее", "сейчас", "были", "куда", "зачем", "всех", "никогда", "можно", "при", "наконец", "два", "об", "другой", "хоть", "после", "над", "больше", "тот", "через", "эти", "нас", "про", "всего", "них", "какая", "много", "разве", "три", "эту", "моя", "впрочем", "хорошо", "свою", "этой", "перед", "иногда", "лучше", "чуть", "том", "нельзя", "такой", "им", "более", "всегда", "конечно", "всю", "между" });
        static void Main(string[] args)
        {
            biDictionary = new Dictionary<string, Dictionary<string, int>>();
            wordsFreq = new Dictionary<string, int>();
            
            using (var sr = new StreamReader("rus_news_2010_300K-sentences.txt"))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().ToLower();
                    string prevWord = null;
                    foreach (var m in r.Matches(line).Cast<Match>())
                    {
                        if (!stopWords.Contains(m.Value))
                        {
                            if (prevWord != null)
                            {
                                Dictionary<string, int> tmp;
                                if (biDictionary.TryGetValue(prevWord, out tmp))
                                {
                                    int val;
                                    if (tmp.TryGetValue(m.Value, out val))
                                        tmp[m.Value] = val + 1;
                                    else
                                        tmp.Add(m.Value, 1);
                                }
                                else
                                {
                                    biDictionary.Add(
                                        prevWord,
                                        new Dictionary<string, int>
                                            {{m.Value, 1}}
                                        );
                                }
                                bic++;
                            }
                            prevWord = m.Value;
                            awc++;
                            int k;
                            if (wordsFreq.TryGetValue(m.Value, out k))
                                wordsFreq[m.Value] = k + 1;
                            else
                                wordsFreq.Add(m.Value, 1);
                        }
                        else
                            prevWord = null;
                    }
                }

            }
            var topSimple =
                biDictionary.SelectMany(x => x.Value.Select(y => new { bi = new []{x.Key, y.Key}, freq = y.Value }));
            var student = topSimple.Select(x => foo(x)).OrderByDescending(x => x.res).Take(100).ToList();
            int a = 0;


        }
        static dynamic foo(dynamic x)
        {
            var p = (wordsFreq[x.bi[0]] / bic) * (wordsFreq[x.bi[1]] / bic);
            var s = p*(1-p);
            var z = x.freq / bic;
            var res1 = (z - p)/Math.Sqrt(s / bic);
            if (res1 > 1000)
                Console.WriteLine(11);
            return new { bi = $"{x.bi[0]} {x.bi[1]}", res = res1 };
        }
    }

    
}
