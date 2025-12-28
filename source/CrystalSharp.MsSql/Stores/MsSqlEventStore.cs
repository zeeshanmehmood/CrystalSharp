using CrystalSharp.Common.Extensions;
using CrystalSharp.Dapper.Common;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CrystalSharp.MsSql.Stores
{
    public class MsSqlEventStore(
        string eventStoreConnectionString,
        bool useSchema,
        string schema) : SqlEventStorePersistence(new DbManager(new SqlConnection(eventStoreConnectionString)), new("[", "]", useSchema, schema)), IEventStorePersistence
    {
        protected override IList<IDataParameter> GenerateParameters(IDictionary<string, object> dataParameters)
        {
            IList<IDataParameter> parameters = null;

            if (dataParameters.HasAny())
            {
                parameters = [.. dataParameters.Select(x => { return new SqlParameter(x.Key, dataParameters[x.Key]); })];
            }

            return parameters;
        }
    }
}
