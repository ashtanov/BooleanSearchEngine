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
        class Bigramm
        {
            public string word1 { get; set; }
            public string word2 { get; set; }

            public Bigramm(string w1, string w2)
            {
                string[] orderedWords = { w1, w2 };
                Array.Sort(orderedWords);
                word1 = orderedWords[0];
                word2 = orderedWords[1];
            }
            public override string ToString()
            {
                return $"{word1} {word2}";
            }
            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }
            public override bool Equals(object obj)
            {
                return obj.GetHashCode() == GetHashCode();
            }
        }
        static Regex r = new Regex(@"[а-яА-ЯЁёA-Za-z]+", RegexOptions.Compiled);
        private static Dictionary<string, Dictionary<string, int>> biDictionary;
        private static Dictionary<string, int> wordsFreq;



        private static Dictionary<string, Dictionary<Bigramm, int>> trigramm;
        private static int trigrammcount = 0;
        private static Dictionary<Bigramm, int> bigramm;
        private static Dictionary<string, int> unigramm;
        static string[] corpus;


        static double awc = 0;
        static double bic = 0;

        private static HashSet<string> stopWords = new HashSet<string>(new[] { "и", "в", "во", "не", "что", "он", "на", "я", "с", "со", "как", "а", "то", "все", "она", "так", "его", "но", "да", "ты", "к", "у", "же", "вы", "за", "бы", "по", "только", "ее", "мне", "было", "вот", "от", "меня", "еще", "нет", "о", "из", "ему", "теперь", "когда", "даже", "ну", "вдруг", "ли", "если", "уже", "или", "ни", "быть", "был", "него", "до", "вас", "нибудь", "опять", "уж", "вам", "ведь", "там", "потом", "себя", "ничего", "ей", "может", "они", "тут", "где", "есть", "надо", "ней", "для", "мы", "тебя", "их", "чем", "была", "сам", "чтоб", "без", "будто", "чего", "раз", "тоже", "себе", "под", "будет", "ж", "тогда", "кто", "этот", "того", "потому", "этого", "какой", "совсем", "ним", "здесь", "этом", "один", "почти", "мой", "тем", "чтобы", "нее", "сейчас", "были", "куда", "зачем", "всех", "никогда", "можно", "при", "наконец", "два", "об", "другой", "хоть", "после", "над", "больше", "тот", "через", "эти", "нас", "про", "всего", "них", "какая", "много", "разве", "три", "эту", "моя", "впрочем", "хорошо", "свою", "этой", "перед", "иногда", "лучше", "чуть", "том", "нельзя", "такой", "им", "более", "всегда", "конечно", "всю", "между" });
        static void Main(string[] args)
        {
            //Collocations();

            corpus = File.ReadAllLines(@"E:\rus_news_2010_300K-sentences.txt").ToArray();
            string[] test;
            string[] train;
            for (int k = 0; k < 100; ++k)
            {
                trigramm = new Dictionary<string, Dictionary<Bigramm, int>>();
                bigramm = new Dictionary<Bigramm, int>();
                unigramm = new Dictionary<string, int>();
                SplitCorpus(corpus, 0.2, out train, out test);
                CreateModel(train);
                double counter = 0;
                Func<string, string, string, double> f = (x, y, z) => Lindstone3(x, y, z, 0.01);
                for (int i = 0; i < test.Length; ++i)
                    counter += ProbEstimate(ParseSentence(test[i]), f);
                var testEstimate = counter / test.Length;
                counter = 0;
                for (int i = 0; i < train.Length; ++i)
                    counter += ProbEstimate(ParseSentence(train[i]), f);
                var trainEstimate = counter / train.Length;
                Console.WriteLine($"train: {trainEstimate}\ntest: {testEstimate}");
            }
            int a = 0;
        }

        public static void SplitCorpus(string[] corpus, double percent, out string[] train, out string[] test)
        {
            var testpart = (int)(corpus.Length * percent);
            Random rand = new Random();
            for (int i = 0; i < corpus.Length; ++i)
            {
                int rind = rand.Next(0, corpus.Length);
                var tmp = corpus[rind];
                corpus[rind] = corpus[i];
                corpus[i] = tmp;
            }
            test = corpus.Skip(corpus.Length - testpart).Take(testpart).ToArray();
            train = corpus.Take(corpus.Length - testpart).ToArray();
        }

        public static double MLE(string curr, string prev1, string prev2)
        {
            var b = new Bigramm(prev1, prev2);
            return (trigramm[curr][b] + 0.0) / bigramm[b];
        }

        public static double Lindstone3(string curr, string prev1, string prev2, double alpha)
        {
            var b = new Bigramm(prev1, prev2);
            int bscore;
            int tscore;
            if (!bigramm.TryGetValue(b, out bscore))
                bscore = 0;
            Dictionary<Bigramm, int> tmp;
            if (!trigramm.TryGetValue(curr, out tmp))
                tscore = 0;
            else if (!tmp.TryGetValue(b, out tscore))
                tscore = 0;
            return (tscore + alpha)  / (bscore + alpha * unigramm.Count);
        }

        public static string[] ParseSentence(string s)
        {
            var line = s.ToLower().Replace("­", "");
            return r.Matches(line).Cast<Match>().Select(x => x.Value).ToArray();
        }

        public static double ProbEstimate(string[] sentence, Func<string, string, string, double> probF)
        {
            double prob = 0;
            var prev = "@";
            var prev2 = "@";
            for (int i = 0; i < sentence.Length; ++i)
            {
                prob += Math.Log(probF(sentence[i], prev, prev2));
                prev2 = prev;
                prev = sentence[i];
            }
            return Math.Exp(-prob / sentence.Length);
            //double prob = 1;
            //var prev = "@";
            //var prev2 = "@";
            //for (int i = 0; i < sentence.Length; ++i)
            //{
            //    prob /= probF(sentence[i], prev, prev2);
            //    prev2 = prev;
            //    prev = sentence[i];
            //}
            //return Math.Pow(prob, 1.0 / sentence.Length);
        }

        private static void CreateModel(string[] corp)
        {
            foreach (var dline in corp)
            {
                string curr_word;
                string prev_word = "@";
                string prev2_word = "@";
                bigramm.TryAdd(new Bigramm("@", "@"), 1, (o, n) => o + n);
                foreach (string word in ParseSentence(dline))
                {
                    curr_word = word;
                    Bigramm b = new Bigramm(prev_word, prev2_word);
                    //3
                    Dictionary<Bigramm, int> tritmp;
                    if (trigramm.TryGetValue(curr_word, out tritmp))
                        tritmp.TryAdd(b, 1, (o, n) => o + n);
                    else
                    {
                        trigrammcount++;
                        trigramm.Add(curr_word, new Dictionary<Bigramm, int> { { b, 1 } });
                    }
                    //2
                    Bigramm b2 = new Bigramm(curr_word, prev_word);
                    bigramm.TryAdd(b2, 1, (o, n) => o + n);
                    //1
                    unigramm.TryAdd(curr_word, 1, (o, n) => o + n);

                    prev2_word = prev_word;
                    prev_word = curr_word;
                }
            }
        }

        private static void Collocations()
        {
            biDictionary = new Dictionary<string, Dictionary<string, int>>();
            wordsFreq = new Dictionary<string, int>();

            using (var sr = new StreamReader(@"E:\rus_news_2010_300K-sentences.txt"))
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
                biDictionary.SelectMany(x => x.Value.Select(y => new { bi = new[] { x.Key, y.Key }, freq = y.Value }));
            var student = topSimple.Select(x => foo(x)).OrderByDescending(x => x.res).Take(100).ToList();
            int a = 0;
        }

        static dynamic foo(dynamic x)
        {
            var p = (wordsFreq[x.bi[0]] / bic) * (wordsFreq[x.bi[1]] / bic);
            var s = p * (1 - p);
            var z = x.freq / bic;
            var res1 = (z - p) / Math.Sqrt(s / bic);
            if (res1 > 1000)
                Console.WriteLine(11);
            return new { bi = $"{x.bi[0]} {x.bi[1]}", res = res1 };
        }
    }
}

