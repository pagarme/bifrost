using System.Collections.Generic;
using System.Linq;

namespace PagarMe.Generic
{
    public static class MakeArrayExtension
    {
        public static T[] ArrayWith<T>(this T obj, params T[] others)
        {
            return new List<T> { obj }.join(others);
        }

        public static T[] ArrayWith<T>(this IEnumerable<T> array, params T[] others)
        {
            return array.ToList().join(others);
        }

        private static T[] join<T>(this List<T> list, params T[] others)
        {
            list.AddRange(others);
            return list.ToArray();
        }
        }
}
