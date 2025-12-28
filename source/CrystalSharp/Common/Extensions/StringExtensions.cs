using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CrystalSharp.Common.Extensions
{
    public static class StringExtensions
    {
        extension(string source)
        {
            public bool IsValidString()
            {
                bool isValid = true;

                if (source == null || string.IsNullOrEmpty(source) || string.IsNullOrWhiteSpace(source))
                {
                    isValid = false;
                }

                return isValid;
            }

            public bool IsEqual(string value, bool toUpperInvariant = false)
            {
                bool isEqual = string.Equals(toUpperInvariant ? source.ToUpperInvariant() : source, toUpperInvariant ? value.ToUpperInvariant() : value, StringComparison.OrdinalIgnoreCase);

                return isEqual;
            }

            public string ToDoubleQuotes()
            {
                return $"\"{source}\"";
            }

            public string ToCamelCase()
            {
                string[] words = source.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
                string leadWord = Regex.Replace(words[0], @"([A-Z])([A-Z]+|[a-z0-9]+)($|[A-Z]\w*)", m =>
                {
                    return m.Groups[1].Value.ToLower() + m.Groups[2].Value.ToLower() + m.Groups[3].Value;
                });

                string[] tailWords = words.Skip(1)
                    .Select(word => char.ToUpper(word[0]) + word[1..])
                    .ToArray();

                return $"{leadWord}{string.Join(string.Empty, tailWords)}";
            }
        }
    }
}
