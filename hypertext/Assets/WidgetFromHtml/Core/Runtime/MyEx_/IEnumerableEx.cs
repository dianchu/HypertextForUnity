using System;
using System.Collections.Generic;
using System.Linq;

namespace WidgetFromHtml.Core
{
    public static class IEnumerableEx
    {
        public static bool isEmpty<T>(this IEnumerable<T> ieable)
        {
            // return ieable.GetEnumerator().MoveNext();
            return !ieable.Any();
        }

        public static bool isNotEmpty<T>(this IEnumerable<T> ieable)
        {
            return !ieable.isEmpty();
        }

        public static int length<T>(this IEnumerable<T> ieable)
        {
            int count = 0;

            var ietor = ieable.GetEnumerator();
            while (ietor.MoveNext())
            {
                count++;
            }

            return count;
        }

        public static IEnumerable<TOut> map<TIn, TOut>(this IEnumerable<TIn> ieable, Func<TIn, TOut> func)
        {
            return ieable.Select(func);
        }
    }
}