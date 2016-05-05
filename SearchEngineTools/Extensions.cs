using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineTools
{
    public static class Extensions
    {
        public static bool ContainsSubSeq(this IEnumerable<string> text, IList<string> quote)
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
    }
}
