// The MIT License (MIT)
//
// Copyright (c) 2024 Zeeshan Mehmood
// https://github.com/zeeshanmehmood/CrystalSharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CrystalSharp.Sql.Common.Migrator;

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

            if (scripts != null && scripts.Any())
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
