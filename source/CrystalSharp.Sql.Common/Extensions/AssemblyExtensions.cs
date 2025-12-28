using CrystalSharp.Sql.Common.Migrator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CrystalSharp.Sql.Common.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<SqlScriptMeta> GetEmbeddedSqlScripts(this Assembly assembly, string startsWith, params string[] scripts)
        {
            if (string.IsNullOrEmpty(startsWith)) throw new ArgumentNullException(nameof(startsWith));

            string stripAssemblyName = $"{assembly.GetName().Name}.";
            IOrderedEnumerable<string> manifestResourceNames = assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(startsWith))
                .OrderBy(n => n);

            if (scripts is not null && scripts.Length != 0)
            {
                manifestResourceNames = manifestResourceNames.Where(r => FilterScripts(scripts, r)).OrderBy(n => n);
            }

            foreach (string manifestResourceName in manifestResourceNames)
            {
                using Stream manifestResourceStream = assembly.GetManifestResourceStream(manifestResourceName);
                using StreamReader streamReader = new(manifestResourceStream);
                string name = manifestResourceName.Replace(stripAssemblyName, string.Empty);
                string content = streamReader.ReadToEnd();

                yield return new SqlScriptMeta(name, content);
            }
        }

        private static bool FilterScripts(string[] scripts, string script)
        {
            bool containsItem = false;

            foreach (string scriptItem in scripts)
            {
                containsItem = script.Contains(scriptItem, StringComparison.OrdinalIgnoreCase);

                if (containsItem)
                {
                    break;
                }
            }

            return containsItem;
        }
    }
}
