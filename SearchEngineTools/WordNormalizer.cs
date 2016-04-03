using Iveonik.Stemmers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineTools
{
    public interface IWordNormalizer
    {
        string NormalizeWord(string word);
    }

    public class StemmingNormalizer : IWordNormalizer
    {
        IStemmer stemmer; 
        public StemmingNormalizer()
        {
            stemmer = new RussianStemmer();
        }
        public string NormalizeWord(string word)
        {
            return stemmer.Stem(word);
        }
    }

    public class WordCaseNormalizer : IWordNormalizer
    {
        public string NormalizeWord(string word)
        {
            return word.ToUpper();
        }
    }
}
