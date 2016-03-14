using System;
using System.Collections.Generic;

namespace Utils
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> AddTo<T>(this IEnumerable<T> e, T value)
        {
            foreach (var cur in e)
            {
                yield return cur;
            }
            yield return value;
        }

        public static IEnumerable<T> AddTo<T>(this IEnumerable<T> e, IEnumerable<T> l2)
        {
            foreach (var cur in e)
            {
                yield return cur;
            }
            foreach (var cur in l2)
            {
                yield return cur;
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }
    }


    public static class TupleListExtensions
    {
        public static void Add<T1, T2>(this IList<Tuple<T1, T2>> list,
                T1 item1, T2 item2)
        {
            list.Add(Tuple.Create(item1, item2));
        }

        public static void Add<T1, T2, T3>(this IList<Tuple<T1, T2, T3>> list,
                T1 item1, T2 item2, T3 item3)
        {
            list.Add(Tuple.Create(item1, item2, item3));
        }

        // and so on...
    }
}