using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Set = System.Collections.Generic.SortedSet<int>;

namespace IRLab1
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
            public List<Token<Set>> SplitInput(string input)
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
                            t = new BOpToken<Set> { priority = 4, function = (x, y) => new Set(x.Intersect(y)), lexemm = "&" };
                            break;
                        case "|":
                            t = new BOpToken<Set> { priority = 2, function = (x, y) => new Set(x.Union(y)), lexemm = "|" };
                            break;
                        case "!":
                            t = new UOpToken<Set> { priority = 5, function = (x) => new Set(Enumerable.Range(0, ind.paragraph.Count).Except(x)), lexemm = "!" };
                            break;
                        default:
                            t = new ValueToken<Set>(ind[m.Value.ToUpper()]) { lexemm = string.Concat("'", m.Value, "'") };
                            break;

                    }
                    res.Add(t);
                }
                return res;
            }
        }

        Dictionary<string, Set> index;
        static Regex wordFinder = new Regex(@"[^\s«_,\.\(\)\[\]\{\}\?\!'&\|""<>#\*\\=/;:-]+", RegexOptions.Compiled);
        BooleanTokenExtractor ext;
        public List<string> paragraph { get; private set; }
        
        Index()
        {
            index = new Dictionary<string, Set>();
            paragraph = new List<string>();
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

        public IEnumerable<string> Search(string query)
        {
            var tokens = ext.SplitInput(query);
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
            while(stack.Count != 0)
            {
                output.Add(stack.Pop());
            }
            Stack<Set> solveStack = new Stack<Set>();
            foreach(var t in output)
            {
                if(t is ValueToken<Set>)
                    solveStack.Push((t as ValueToken<Set>).value);
                else if(t is UOpToken<Set>)
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
                res.paragraph.Add(par);
                foreach (Match word in wordFinder.Matches(par))
                {
                    stat.TokenCount++;
                    stat.TokenSummaryLength += word.Value.Length;
                    Set ss;
                    if (res.index.TryGetValue(word.Value.ToUpper(), out ss))
                        ss.Add(i);
                    else
                    {
                        res.index.Add(word.Value.ToUpper(), new Set { i });
                        stat.TermCount++;
                        stat.TermSummaryLength += word.Value.Length;
                    }
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
                    for(int i = 0; i< t.Value.Count; ++i)
                    {
                        bw.Write(t.Value.ElementAt(i));
                    }
                }
                bw.Write(paragraph.Count);
                foreach(var p in paragraph)
                {
                    bw.Write(p);
                }
            }
        }

        public static Index Deserialize(string filePath)
        {
            Index ind = new Index();
            using(var br = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
            {
                int count = br.ReadInt32();
                for(int i = 0; i < count; ++i)
                {
                    string name = br.ReadString();
                    int scount = br.ReadInt32();
                    Set s = new Set();
                    for(int k = 0;  k < scount; ++k)
                    {
                        s.Add(br.ReadInt32());
                    }
                    ind.index.Add(name, s);
                }
                count = br.ReadInt32();
                for(int i =0; i < count; ++i)
                {
                    ind.paragraph.Add(br.ReadString());
                }
            }
            return ind;
        }
    }

}
