using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IRLab1
{

    public class Token<T>
    {
        public string lexemm { get; set; }
        public override string ToString()
        {
            return lexemm;
        }
    }
    public class ValueToken<T> : Token<T>
    {
        public ValueToken(T value)
        {
            this.value = value;
        }
        public readonly T value;
    }
    public class PToken<T> : Token<T>
    {
        public int priority { get; set; }
    }

    public class BOpToken<T> : PToken<T>
    {
        public Func<T, T, T> function;
    }
    public class UOpToken<T> : PToken<T>
    {
        public Func<T, T> function;
    }
    public interface ITokenExtractor<T>
    {
        List<Token<T>> SplitInput(string input);
    }

    
}
