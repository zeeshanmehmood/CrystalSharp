using System.Collections.Generic;
using System.Linq;

namespace CrystalSharp.Common.Extensions
{
    public static class EnumerableExtensions
    {
        extension<T>(IEnumerable<T> source)
        {
            public bool HasAny()
            {
                return source is not null && source.Any();
            }
        }
    }
}
