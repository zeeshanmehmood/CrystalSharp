using System;

namespace CrystalSharp.Sql.Common.Migrator
{
    public class SqlScriptMeta
    {
        public string Name { get; }
        public string Content { get; }

        public SqlScriptMeta(string name, string content)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(content)) throw new ArgumentNullException(nameof(content));

            Name = name;
            Content = content;
        }
    }
}
