using System;
using System.Linq;

namespace CrystalSharp.Common.Extensions
{
    public static class TypeExtensions
    {
        extension(Type type)
        {
            public string ToStreamName(Guid id)
            {
                // Ensure first character of type name is lower case to follow camelCase naming conventions.
                return $"{char.ToLower(type.Name[0])}{type.Name[1..]}-{id:N}";
            }

            public string PropertiesToColumns()
            {
                return type.PropertiesToColumns(string.Empty, string.Empty);
            }

            public string PropertiesToColumns(string prefix, string suffix)
            {
                return string.Join(",", type.GetProperties().Select(p => $"{prefix}{p.Name}{suffix}"));
            }
        }
    }
}
