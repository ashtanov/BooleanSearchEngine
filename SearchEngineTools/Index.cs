using Iveonik.Stemmers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Set = System.Collections.Generic.SortedSet<int>;

namespace SearchEngineTools
{
    public class Statistic
    {
        public int TokenCount { get; set; }
        public int TermCount { get; set; }
        public int TokenSummaryLength { get; set; }
        public int TermSummaryLength { get; set; }
        public TimeSpan CreatingTime { get; set; }
    }
    public class Index
    {
        class BooleanTokenExtractor : ITokenExtractor<Set>
        {
            Index ind;
            public BooleanTokenExtractor(Index index)
            {
                ind = index;
            }
            static Regex ext = new Regex(@"[^\s«_,\.\(\)\[\]\{\}\?\!'&\|""]+|&|\||\!|\(|\)");
            public List<Token<Set>> SplitInput(string input, bool quote)
            {
                List<Token<Set>> res = new List<Token<Set>>();
                var ttt = ext.Matches(input);
                foreach (Match m in ttt)
                {
                    Token<Set> t;
                    switch (m.Value)
                    {
                        case "(":
                            t = new PToken<Set> { priority = -1, lexemm = "(" };
                            break;
                        case ")":
                            t = new PToken<Set> { priority = 10, lexemm = ")" };
                            break;
                        case "&":
                            t = new BOpToken<Set> { priority = 4, function = StubSmartIntersect, lexemm = "&" };
                            break;
                        case "|":
                            t = new BOpToken<Set> { priority = 2, function = (x, y) => new Set(x.Union(y)), lexemm = "|" };
                            break;
                        case "!":
                            t = new UOpToken<Set> { priority = 5, function = (x) => new Set(Enumerable.Range(0, ind.paragraph.Count).Except(x)), lexemm = "!" };
                            break;
                        default:
                            t = new ValueToken<Set>(ind[quote ? m.Value : ind.normalizer.NormalizeWord(m.Value)]) { lexemm = string.Concat("'", m.Value, "'") };
                            break;

                    }
                    res.Add(t);
                }
                return res;
            }

            Set Intersect(Set a, Set b, out int compares)
            {
                Console.WriteLine("Обычный {0}", new Set(a.Intersect(b)).Count);
                List<int> first, second;
                Set result = new Set();
                first = b.ToList();
                second = a.ToList();
                compares = 0;
                int i = 0, k = 0;
                while (i < first.Count && k < second.Count)
                {
                    compares++;
                    if (first[i] == second[k])
                    {
                        result.Add(first[i]);
                        k++;
                        i++;
                    }
                    else if (first[i] < second[k])
                        i++;
                    else
                        k++;

                }
                Console.WriteLine("intercount: {0}\tcomp:{1}", result.Count, compares);
                return result;
            }

            Set SmartIntersect(Set a, Set b, int jumpa, int jumpb, out int compares)
            {
                List<int> first, second;
                Set result = new Set();
                first = b.ToList();
                second = a.ToList();
                compares = 0;
                int i = 0, k = 0;
                while (i < first.Count && k < second.Count)
                {
                    if (first[i] == second[k])
                    {
                        compares++;
                        result.Add(first[i]);
                        k++;
                        i++;
                    }
                    else if (first[i] < second[k])
                    {
                        compares++;
                        if (i % jumpb == 0)
                        {
                            compares++;
                            while (i + jumpb < first.Count && first[i + jumpb] < second[k])
                            {
                                compares++;
                                i += jumpb;
                            }
                            i++;
                        }
                        else
                            i++;
                    }
                    else
                    {
                        compares++;
                        if (k % jumpa == 0)
                        {
                            compares++;
                            while (k + jumpa < second.Count && second[k + jumpa] < first[i])
                            {
                                compares++;
                                k += jumpa;
                            }
                            k++;
                        }
                        else
                            k++;
                    }
                }
                return result;
            }

            Set CompareIntersect(Set a, Set b)
            {
                int s_k;
                Intersect(a, b, out s_k);
                int[] c_ks = new int[48];
                for (int i = 2; i < 50; ++i)
                {
                    SmartIntersect(a, b, i, i, out c_ks[i - 2]);
                }
                int sm_k;
                var res = SmartIntersect(a, b, (int)Math.Sqrt(a.Count), (int)Math.Sqrt(b.Count), out sm_k);
                string format = string.Format("{0}\n{1}\n{2}\n{3}",
                    string.Join(";", c_ks),
                    string.Join(";", c_ks.Select(x => s_k)),
                    string.Join(";", c_ks.Select(x => sm_k)),
                    string.Join(";", c_ks.Select((x, i) => i + 2)));
                File.WriteAllText("stat.csv", format);
                return res;
            }

            Set StubSmartIntersect(Set a, Set b)
            {
                int c1;
                return SmartIntersect(a, b, (int)Math.Sqrt(a.Count), (int)Math.Sqrt(b.Count), out c1);
            }
        }

        Dictionary<string, Set> index;
        BooleanTokenExtractor ext;
        public List<string> paragraph { get; private set; }
        IWordNormalizer normalizer;

        Index()
        {
            index = new Dictionary<string, Set>();
            paragraph = new List<string>();
            normalizer = new WordCaseNormalizer();
        }

        public Set this[string s]
        {
            get
            {
                if (index.ContainsKey(s))
                    return index[s];
                else
                    return new Set();
            }
        }

        public IEnumerable<string> QuoteSearch(string query)
        {
            query = query.Trim(new[] { '\"' });
            var words = ParseHelper.FindAllWords(query);
            string prevWord = null;
            List<string> tmp = new List<string>();
            foreach (var w in words)
            {
                if (prevWord != null)
                    tmp.Add(prevWord + "$" + w);
                prevWord = w;
            }
            return Search(string.Join("&", tmp)).Where(x => ContainsSubSeq(ParseHelper.FindAllWords(x), words));
        }

        public bool ContainsSubSeq(IEnumerable<string> text, IList<string> quote)
        {
            int i = 0;
            foreach (var word in text)
            {
                if (i == quote.Count)
                    return true;
                if (word.Equals(quote[i]))
                    i++;
                else
                    i = 0;
            }
            return false;
        }

        public IEnumerable<string> Search(string query)
        {
            var tokens = ext.SplitInput(query, query.Contains("$"));
            Stack<PToken<Set>> stack = new Stack<PToken<Set>>();
            List<Token<Set>> output = new List<Token<Set>>();
            foreach (var t in tokens)
            {
                if (t is PToken<Set>)
                {
                    PToken<Set> pt = t as PToken<Set>;
                    if (stack.Count == 0)
                        stack.Push(pt);
                    else if (pt.priority == 10)
                    {
                        while (stack.Peek().priority != -1)
                        {
                            output.Add(stack.Pop());
                        }
                        stack.Pop();
                    }
                    else if (pt.priority == -1)
                        stack.Push(pt);
                    else
                    {
                        while (stack.Count != 0 && pt.priority < stack.Peek().priority)
                        {
                            output.Add(stack.Pop());
                        }
                        stack.Push(pt);
                    }
                }
                else
                    output.Add(t);
            }
            while (stack.Count != 0)
            {
                output.Add(stack.Pop());
            }
            Stack<Set> solveStack = new Stack<Set>();
            foreach (var t in output)
            {
                if (t is ValueToken<Set>)
                    solveStack.Push((t as ValueToken<Set>).value);
                else if (t is UOpToken<Set>)
                    solveStack.Push((t as UOpToken<Set>).function(solveStack.Pop()));
                else
                    solveStack.Push((t as BOpToken<Set>).function(solveStack.Pop(), solveStack.Pop()));
            }
            return solveStack.Pop().Select(x => paragraph[x]);
        }

        public static Index CreateIndex(IEnumerable<string> docs, out Statistic stat)
        {
            Index res = new Index();
            stat = new Statistic();
            int i = 0;
            Stopwatch time = new Stopwatch();
            time.Start();
            foreach (var par in docs)
            {
                string prevWord = null;
                res.paragraph.Add(par);
                foreach (string word in ParseHelper.FindAllWords(par))
                {
                    if (prevWord != null)
                    {
                        string pair = prevWord + "$" + word;
                        Set ss1;
                        if (res.index.TryGetValue(pair, out ss1))
                            ss1.Add(i);
                        else
                        {
                            res.index.Add(pair, new Set { i });
                            stat.TermCount++;
                            stat.TermSummaryLength += word.Length;
                        }
                    }
                    stat.TokenCount++;
                    stat.TokenSummaryLength += word.Length;
                    Set ss;
                    var nword = res.normalizer.NormalizeWord(word);
                    if (res.index.TryGetValue(nword, out ss))
                        ss.Add(i);
                    else
                    {
                        res.index.Add(nword, new Set { i });
                        stat.TermCount++;
                        stat.TermSummaryLength += nword.Length;
                    }
                    prevWord = word;
                }
                i++;
            }
            stat.CreatingTime = time.Elapsed;
            time.Stop();
            res.ext = new BooleanTokenExtractor(res);
            return res;
        }

        public void Serialize(string filePath)
        {
            using (var bw = new BinaryWriter(new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write)))
            {
                bw.Write(index.Count);
                foreach (var t in index)
                {
                    bw.Write(t.Key);
                    bw.Write(t.Value.Count);
                    var list = t.Value.ToList();
                    bw.Write(list[0]);
                    for (int i = 1; i < t.Value.Count; ++i)
                        bw.Write(list[i] - list[i - 1]);
                }
                bw.Write(paragraph.Count);
                foreach (var p in paragraph)
                {
                    bw.Write(p);
                }
            }
        }

        private static byte[] WriteCompressedInt(int val)
        {
            int max = (int)Math.Ceiling(LastBit(val) / 7.0);
            byte[] result = new byte[max];
            BitArray ba = new BitArray(new[] { val });
            for (int i = 0, k = -1; k < max; ++i)
            {
                if (i % 7 == 0)
                {
                    k++;
                    i = 0;
                }
                if (ba[7 * k + i])
                    result[k] += (byte)(1 << i);
            }
            result[max-1] += 128;
            return result;
        }

        static int LastBit(int val)
        {
            for (int i = 0; i < 32; ++i)
            {
                if (val >> i == 0)
                    return i;
            }
            return 32;
        }

        private static int ReadCompressedInt(BinaryReader sr)
        {
            byte cur;
            int val = 0;
            while ((cur = sr.ReadByte()) < 128)
            {

            }
            cur -= 128;
            return 0;
        }
        public static Index Deserialize(string filePath)
        {
            while (true)
            {
                int y = 127;
                WriteCompressedInt(y);
            }
            Index ind = new Index();
            using (var br = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
            {
                int count = br.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    string name = br.ReadString();
                    int scount = br.ReadInt32();
                    List<int> list = new List<int>();
                    list.Add(br.ReadInt32());
                    for (int k = 1; k < scount; ++k)
                        list.Add(list[k - 1] + br.ReadInt32());
                    ind.index.Add(name, new Set(list));
                }
                count = br.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    ind.paragraph.Add(br.ReadString());
                }
            }
            ind.ext = new BooleanTokenExtractor(ind);
            return ind;
        }
    }
}
