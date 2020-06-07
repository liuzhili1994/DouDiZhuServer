using System;
using System.Collections.Generic;
using Protocol.Dto.Card;

namespace Protocol.Dto.Constant
{
    public static class EXtensionConstant
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

        /// <summary>
        /// 降序
        /// </summary>
        /// <param name="cardList"></param>
        /// <returns></returns>
        public static void SortEx(this List<CardDto> cardList)
        {
            cardList.Sort((CardDto a,CardDto b)=> {
                return b.weight.CompareTo(a.weight);
            });
        }
    }
}
