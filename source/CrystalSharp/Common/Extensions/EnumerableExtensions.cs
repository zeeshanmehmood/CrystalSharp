using System.Collections.Generic;
using System.Linq;

namespace CrystalSharp.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool HasAny<T>(this IEnumerable<T> source)
        {
            return source is not null && source.Any();
        }
    }
}
