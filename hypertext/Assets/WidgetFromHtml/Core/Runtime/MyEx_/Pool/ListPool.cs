using System.Collections.Generic;

namespace WidgetFromHtml.Core
{
    public static class ListPool2<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(OnNew, Clear, Clear);

        private static List<T> OnNew()
        {
            return new List<T>(32);
        }

        static void Clear(List<T> l)
        {
            l.Clear();
        }

        public static List<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }
}