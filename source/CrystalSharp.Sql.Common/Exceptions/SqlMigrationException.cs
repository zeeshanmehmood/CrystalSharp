using System;
using System.Collections.Generic;

namespace CrystalSharp.Sql.Common.Exceptions
{
    public class SqlMigrationException(
        IEnumerable<string> scripts,
        string message,
        Exception innerException) : Exception(message, innerException)
    {
        public IReadOnlyCollection<string> Scripts { get; } = [.. scripts];
    }
}
