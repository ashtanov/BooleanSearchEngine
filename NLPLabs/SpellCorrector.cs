using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StrEnum = System.Collections.Generic.IEnumerable<string>;


namespace NLPLabs
{
    static class Minified
    {
        const string EnglishAlphabet = "abcdefghijklmnopqrstuvwxyz";
        const string RussianAlphabet = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";

        static bool IsEnglishWord(string w)
        {
            return w.IndexOfAny(EnglishAlphabet.ToArray()) != -1;
        }

        static public StrEnum Edits1(string w)
        {
            string alphabet = IsEnglishWord(w) ? EnglishAlphabet : RussianAlphabet;
            // Deletion
            return (from i in Enumerable.Range(0, w.Length)
                    select w.Substring(0, i) + w.Substring(i + 1))
             // Transposition
             .Union(from i in Enumerable.Range(0, w.Length - 1)
                    select w.Substring(0, i) + w.Substring(i + 1, 1) +
                           w.Substring(i, 1) + w.Substring(i + 2))
             // Alteration
             .Union(from i in Enumerable.Range(0, w.Length)
                    from c in alphabet
                    select w.Substring(0, i) + c + w.Substring(i + 1))
             // Insertion
             .Union(from i in Enumerable.Range(0, w.Length + 1)
                    from c in alphabet
                    select w.Substring(0, i) + c + w.Substring(i));
        }

        static public StrEnum Edits2(string word)
        {

            var l = new HashSet<string>();
            foreach (var e1 in Edits1(word))
                foreach (var e2 in Edits1(e1))
                        l.Add(e2);
            return l;
        }
        //static void Main(string[] args)
        //{
        //    var nWords = (from Match m in Regex.Matches(File.ReadAllText("big.txt").ToLower(), "[a-z]+")
        //                  group m.Value by m.Value)
        //                 .ToDictionary(gr => gr.Key, gr => gr.Count());

        //    Func<StrEnum, StrEnum> nullIfEmpty = c => c.Any() ? c : null;

        //    var candidates =
        //        nullIfEmpty(new[] { args[0] }.Where(nWords.ContainsKey))
        //        ?? nullIfEmpty(Edits1(args[0]).Where(nWords.ContainsKey))
        //        ?? nullIfEmpty((from e1 in Edits1(args[0])
        //                        from e2 in Edits1(e1)
        //                        where nWords.ContainsKey(e2)
        //                        select e2).Distinct());

        //    Console.WriteLine(
        //        candidates == null
        //            ? args[0]
        //            : (from cand in candidates
        //               orderby (nWords.ContainsKey(cand) ? nWords[cand] : 1) descending
        //               select cand).First());
        //}
    }
}
