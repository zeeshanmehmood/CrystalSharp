using CrystalSharp.Common.Extensions;
using CrystalSharp.Dapper.Common;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CrystalSharp.PostgreSql.Stores
{
    public class PostgreSqlEventStore(
        string eventStoreConnectionString,
        bool useSchema,
        string schema) : SqlEventStorePersistence(new DbManager(new NpgsqlConnection(eventStoreConnectionString)), new("\"", "\"", useSchema, schema)), IEventStorePersistence
    {
        protected override IList<IDataParameter> GenerateParameters(IDictionary<string, object> dataParameters)
        {
            IList<IDataParameter> parameters = null;

            if (dataParameters.HasAny())
            {
                parameters = [.. dataParameters.Select(x => { return new NpgsqlParameter(x.Key, dataParameters[x.Key]); })];
            }

            return parameters;
        }
    }
}
