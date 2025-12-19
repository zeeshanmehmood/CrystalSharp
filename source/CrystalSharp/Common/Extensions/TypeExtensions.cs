using System;
using System.Linq;

namespace CrystalSharp.Common.Extensions
{
    public static class TypeExtensions
    {
        public static string ToStreamName(this Type type, Guid id)
        {
            // Ensure first character of type name is lower case to follow camelCase naming conventions.
            return $"{char.ToLower(type.Name[0])}{type.Name[1..]}-{id:N}";
        }

        public static string PropertiesToColumns(this Type type)
        {
            return type.PropertiesToColumns(string.Empty, string.Empty);
        }

        public static string PropertiesToColumns(this Type type, string prefix, string suffix)
        {
            return string.Join(",", type.GetProperties().Select(p => $"{prefix}{p.Name}{suffix}"));
        }
    }
}
