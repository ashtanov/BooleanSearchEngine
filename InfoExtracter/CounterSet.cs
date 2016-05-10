using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoExtractor
{
    public class CounterSet<T> :IEnumerable<KeyValuePair<T,int>>
    {
        ConcurrentDictionary<T, int> _dictionary;
        public CounterSet()
        {
            _dictionary = new ConcurrentDictionary<T, int>();
        }

        public IEnumerator<KeyValuePair<T, int>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
        
        public void Add(T item)
        {
            int k;
            if (_dictionary.TryGetValue(item, out k))
                _dictionary[item] = k + 1;
            else
                _dictionary.TryAdd(item, 1);
        }
    }
}
