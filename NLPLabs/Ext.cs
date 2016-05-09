using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPLabs
{
    static public class Ext
    {
        public static void TryAdd<K, V>(this Dictionary<K, V> d, K key, V value, Func<V, V, V> updateFunc)
        {
            V o;
            if (d.TryGetValue(key, out o))
                d[key] = updateFunc(o, value);
            else
                d.Add(key, value);
        }
    }
}
