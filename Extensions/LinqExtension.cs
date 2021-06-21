using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EbingUs
{
    /// <summary>
    /// LINQをもっと強くする拡張機能
    /// </summary>
    public static class LinqExtension
    {
        public static T? Random<T>(this IEnumerable<T> e)
        {
            var count = e.Count();
            if (count == 0) return default;
            return e.Skip(rnd.Next(count)).First();
        }

        private static readonly Random rnd = new();
    }
}
