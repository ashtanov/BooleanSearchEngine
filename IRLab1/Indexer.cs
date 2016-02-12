using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Set = System.Collections.Generic.SortedSet<int>;

namespace IRLab1
{
    public class Index
    {
        public class BooleanTokenExtractor : ITokenExtractor<Set>
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
        private Dictionary<string, Set> index;
        BooleanTokenExtractor ext;
        public List<string> paragraph { get; private set; }
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


        Index()
        {
            index = new Dictionary<string, Set>();
            paragraph = new List<string>();
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
        static Regex wordFinder = new Regex(@"[^\s«_,\.\(\)\[\]\{\}\?\!'&\|""]+", RegexOptions.Compiled);
        public static Index CreateIndex(string text)
        {
            Index res = new Index();
            int i = 0;
            foreach (var par in text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                res.paragraph.Add(par);
                foreach (Match word in wordFinder.Matches(par.ToUpper()))
                {
                    Set ss;
                    if (res.index.TryGetValue(word.Value, out ss))
                        ss.Add(i);
                    else
                        res.index.Add(word.Value, new Set { i });
                }
                i++;
            }
            res.ext = new BooleanTokenExtractor(res);
            return res;
        }
    }
}
