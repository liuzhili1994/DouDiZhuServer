using System;
using System.Collections.Generic;

namespace Protocol.Dto.Constant
{
    public static class DictionaryConstant
    {
        
        public static List<K> GetKeyList<K,V>(this Dictionary<K,V> dic)
        {
            K[] array = new K[dic.Count];
            dic.Keys.CopyTo(array,0);
            return new List<K>(array);
        }

        public static List<V> GetValueList<K, V>(this Dictionary<K, V> dic)
        {
            V[] array = new V[dic.Count];
            dic.Values.CopyTo(array, 0);
            return new List<V>(array);
        }
    }
}
