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

        public static void AddSorted<T>(this List<T> @this, T item) where T : IComparable<T>
        {
            if (@this.Count == 0)
            {
                @this.Add(item);
                return;
            }
            if (@this[@this.Count - 1].CompareTo(item) <= 0)
            {
                @this.Add(item);
                return;
            }
            if (@this[0].CompareTo(item) >= 0)
            {
                @this.Insert(0, item);
                return;
            }
            int index = @this.BinarySearch(item);
            if (index < 0)
                index = ~index;
            @this.Insert(index, item);
        }

        public static bool ContainsAny(this string @this, params string[] ss)
        {
            return ss.Any(@this.Contains);
        }

        public static string ReplaceAll(this string @this, params char[] ss)
        {
            return ss.Aggregate(@this, (current, s) => current.Replace(s.ToString(), ""));
        }
    }
}
