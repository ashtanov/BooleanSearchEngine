using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SearchEngineTools
{
    public static class ParseHelper
    {
        static Regex wordFinder = new Regex(@"[^\s«_,\.\(\)\[\]\{\}\?\!'&\|""<>#\*\\=/;:-]+", RegexOptions.Compiled);
        public static List<string> DivideIntoParagraphs(string text)
        {
            return text.Replace("&nbsp;", " ").Replace("&NBSP;", " ").Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        public static List<string> FindAllWords(string text)
        {
            List<string> s = new List<string>();
            foreach(Match m in wordFinder.Matches(text))
            {
                s.Add(m.Value);
            }
            return s;
        }
    }
}
