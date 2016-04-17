﻿using Iveonik.Stemmers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Set = System.Collections.Generic.SortedSet<int>;
using PositionDict = System.Collections.Generic.SortedDictionary<System.Guid, System.Collections.Generic.SortedSet<int>>;

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
        class PairCopmarer : IEqualityComparer<KeyValuePair<Guid, Set>>
        {
            public bool Equals(KeyValuePair<Guid, Set> x, KeyValuePair<Guid, Set> y)
            {
                return x.Key.Equals(y.Key);
            }

            public int GetHashCode(KeyValuePair<Guid, Set> obj)
            {
                return obj.Key.GetHashCode();
            }
        }
        class BooleanTokenExtractor : ITokenExtractor<PositionDict>
        {
            Index ind;
            public BooleanTokenExtractor(Index index)
            {
                ind = index;
            }
            static Regex ext = new Regex(@"[^\s«_,\.\(\)\[\]\{\}\?\!'&\|""]+|&|\||\!|\(|\)");
            public List<Token<PositionDict>> SplitInput(string input, bool quote)
            {
                List<Token<PositionDict>> res = new List<Token<PositionDict>>();
                var ttt = ext.Matches(input);
                foreach (Match m in ttt)
                {
                    Token<PositionDict> t;
                    switch (m.Value)
                    {
                        case "(":
                            t = new PToken<PositionDict> { priority = -1, lexemm = "(" };
                            break;
                        case ")":
                            t = new PToken<PositionDict> { priority = 10, lexemm = ")" };
                            break;
                        case "&":
                            t = new BOpToken<PositionDict> { priority = 4, function = StubSmartIntersect, lexemm = "&" };
                            break;
                        case "|":
                            t = new BOpToken<PositionDict>
                            {
                                priority = 2,
                                function =
                                (x, y) => new PositionDict(x.Union(y, new PairCopmarer()).ToDictionary(k => k.Key, k => k.Value)),
                                lexemm = "|"
                            };
                            break;
                        case "!":
                            t = new UOpToken<PositionDict> { priority = 5, function = (x) => { throw new NotImplementedException(); }, lexemm = "!" };
                            break;
                        default:
                            t = new ValueToken<PositionDict>(ind[quote ? m.Value : ind.normalizer.NormalizeWord(m.Value)]) { lexemm = string.Concat("'", m.Value, "'") };
                            break;

                    }
                    res.Add(t);
                }
                return res;
            }

            PositionDict SmartIntersect(PositionDict a, PositionDict b, int jumpa, int jumpb)
            {
                List<Guid> first, second;
                PositionDict result = new PositionDict();
                first = b.Keys.ToList();
                second = a.Keys.ToList();
                int i = 0, k = 0;
                while (i < first.Count && k < second.Count)
                {
                    if (first[i] == second[k])
                    {
                        result.Add(first[i], new Set());
                        k++;
                        i++;
                    }
                    else if (first[i].CompareTo(second[k]) < 0)
                    {
                        if (i % jumpb == 0)
                        {
                            while (i + jumpb < first.Count && first[i + jumpb].CompareTo(second[k]) < 0)
                            {
                                i += jumpb;
                            }
                            i++;
                        }
                        else
                            i++;
                    }
                    else
                    {
                        if (k % jumpa == 0)
                        {
                            while (k + jumpa < second.Count && second[k + jumpa].CompareTo(first[i]) < 0)
                                k += jumpa;
                            k++;
                        }
                        else
                            k++;
                    }
                }
                return result;
            }

            PositionDict StubSmartIntersect(PositionDict a, PositionDict b)
            {
                return SmartIntersect(a, b, (int)Math.Sqrt(a.Count), (int)Math.Sqrt(b.Count));
            }
        }

        Dictionary<string, PositionDict> index;
        BooleanTokenExtractor ext;
        IWordNormalizer normalizer;
        string path_docs;

        Index()
        {
            index = new Dictionary<string, PositionDict>();
            normalizer = new WordCaseNormalizer();
        }

        public PositionDict this[string s]
        {
            get
            {
                if (index.ContainsKey(s))
                    return index[s];
                else
                    return new PositionDict();
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

        public static void Merge(List<Index> indexes, string dir, string name)
        {
            if (!File.Exists(dir))
                Directory.CreateDirectory(dir);
            Index ind = new Index { path_docs = dir };
            foreach (var index in indexes)
            {
                foreach (var file in Directory.GetFiles(index.path_docs))
                {
                    File.Copy(file, string.Format("{0}/{1}", dir, Path.GetFileName(file)));
                }
                foreach (var kvp in index.index)
                {
                    PositionDict pd;
                    if(ind.index.TryGetValue(kvp.Key, out pd))
                        foreach(var elem in kvp.Value)
                            pd.Add(elem.Key, elem.Value);
                    else
                    {
                        pd = new SortedDictionary<Guid, SortedSet<int>>(kvp.Value);
                        ind.index.Add(kvp.Key, pd);
                    }

                }
            }
            ind.Serialize(name);
        }

        public IEnumerable<string> DistanceSearch(string query)
        {
            var words = ParseHelper.FindAllWords(query);
            List<Tuple<string, string, int>> tmp = new List<Tuple<string, string, int>>();
            for (int i = 2; i < words.Count; i += 2)
                tmp.Add(new Tuple<string, string, int>(words[i - 2], words[i], int.Parse(words[i - 1].Substring(1))));
            List<IList<Coord>> res = new List<IList<Coord>>();
            foreach (var ds in tmp)
            {
                res.Add(
                    DistanceSearch2Docs(
                        index[normalizer.NormalizeWord(ds.Item1)],
                        index[normalizer.NormalizeWord(ds.Item2)],
                        ds.Item3)
                    );
            }
            IList<Coord> current = res[0];
            for (int i = 1; i < res.Count; ++i)
            {
                List<Coord> intersect = new List<Coord>();
                int j = 0, k = 0;
                while (j < current.Count && k < res[i].Count)
                    if (current[j].docId == res[i][k].docId)
                    {
                        if (current[j].sPos == res[i][k].fPos)
                            intersect.Add(res[i][k]);
                        k++;
                        j++;
                    }
                    else if (current[j].docId.CompareTo(res[i][k].docId) < 0)
                        j++;
                    else
                        k++;
                current = intersect;
            }
            return current.Select(x => GetDocument(x.docId));
        }

        public class Coord
        {
            public Guid docId { get; set; }
            public int fPos { get; set; }
            public int sPos { get; set; }
        }

        private IList<Coord> DistanceSearch2Docs(PositionDict a, PositionDict b, int distance)
        {
            int i = 0, k = 0;
            IList<Guid> first = a.Keys.ToList();
            IList<Guid> second = b.Keys.ToList();
            List<Coord> answer = new List<Coord>();
            while (i < first.Count && k < second.Count)
            {
                if (first[i] == second[k])
                {
                    Guid docId = first[i];
                    List<int> l = new List<int>();
                    int fPos = 0;
                    int sPos = 0;
                    List<int> posA = a[docId].ToList();
                    while (fPos < posA.Count)
                    {
                        List<int> posB = b[docId].ToList();
                        while (sPos < posB.Count)
                        {
                            if (Math.Abs(posA[fPos] - posB[sPos]) <= distance)
                                l.Add(posB[sPos]);
                            else if (posB[sPos] > posA[fPos])
                                break;
                            sPos++;
                        }
                        while (l.Count != 0 && Math.Abs(l[0] - posA[fPos]) > distance)
                            l.RemoveAt(0);
                        foreach (var ps in l)
                            answer.Add(new Coord { docId = docId, fPos = posA[fPos], sPos = ps });
                        fPos++;
                    }
                    k++;
                    i++;
                }
                else if (first[i].CompareTo(second[k]) > 0)
                    k++;
                else
                    i++;

            }
            return answer;
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
            Stack<PToken<PositionDict>> stack = new Stack<PToken<PositionDict>>();
            List<Token<PositionDict>> output = new List<Token<PositionDict>>();
            foreach (var t in tokens)
            {
                if (t is PToken<PositionDict>)
                {
                    PToken<PositionDict> pt = t as PToken<PositionDict>;
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
            Stack<PositionDict> solveStack = new Stack<PositionDict>();
            foreach (var t in output)
            {
                if (t is ValueToken<PositionDict>)
                    solveStack.Push((t as ValueToken<PositionDict>).value);
                else if (t is UOpToken<PositionDict>)
                    solveStack.Push((t as UOpToken<PositionDict>).function(solveStack.Pop()));
                else
                    solveStack.Push((t as BOpToken<PositionDict>).function(solveStack.Pop(), solveStack.Pop()));
            }
            return solveStack.Pop().Select(x => GetDocument(x.Key));
        }
        private string GetDocument(Guid g)
        {
            return File.ReadAllText(string.Format("{0}/{1}", path_docs, g.ToString("N")));
        }

        public static Index CreateIndex(IEnumerable<Tuple<Guid, string>> docs, string path_to_docs, out Statistic stat)
        {
            Index res = new Index();
            res.path_docs = path_to_docs;
            stat = new Statistic();
            Stopwatch time = new Stopwatch();
            time.Start();
            foreach (var par in docs)
            {
                int wordPos = 0;
                foreach (string word in ParseHelper.FindAllWords(par.Item2))
                {
                    stat.TokenCount++;
                    stat.TokenSummaryLength += word.Length;
                    PositionDict tmp;
                    var nword = res.normalizer.NormalizeWord(word);
                    if (res.index.TryGetValue(nword, out tmp))
                        if (tmp.ContainsKey(par.Item1))
                            tmp[par.Item1].Add(wordPos);
                        else
                            tmp.Add(par.Item1, new Set { wordPos });
                    else
                    {
                        res.index.Add(nword,
                            new PositionDict
                            {
                                { par.Item1, new Set { wordPos } }
                            });
                        stat.TermCount++;
                        stat.TermSummaryLength += nword.Length;
                    }
                    wordPos++;
                }
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
                    foreach (var v in t.Value)
                    {
                        bw.Write(v.Key.ToByteArray());
                        bw.Write(v.Value.Count);
                        foreach (var p in v.Value)
                            bw.Write(p);
                    }
                }
                bw.Write(path_docs);
            }
        }

        public static Index Deserialize(string filePath)
        {
            Index ind = new Index();
            using (var br = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
            {
                int count = br.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    string name = br.ReadString();
                    int scount = br.ReadInt32();
                    PositionDict tmp = new PositionDict();
                    for (int k = 0; k < scount; ++k)
                    {
                        Guid docId = new Guid(br.ReadBytes(16));
                        int pCount = br.ReadInt32();
                        Set s = new Set();
                        for (int p = 0; p < pCount; ++p)
                        {
                            s.Add(br.ReadInt32());
                        }
                        tmp.Add(docId, s);
                    }
                    ind.index.Add(name, tmp);
                }
                ind.path_docs = br.ReadString();
            }
            ind.ext = new BooleanTokenExtractor(ind);
            return ind;
        }
    }
}
