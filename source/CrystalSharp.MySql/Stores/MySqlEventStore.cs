using CrystalSharp.Common.Extensions;
using CrystalSharp.Dapper.Common;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CrystalSharp.MySql.Stores
{
    public class MySqlEventStore(
        string eventStoreConnectionString,
        bool useSchema,
        string schema) : SqlEventStorePersistence(new DbManager(new MySqlConnection(eventStoreConnectionString)), new("`", "`", useSchema, schema)), IEventStorePersistence
    {
        protected override IList<IDataParameter> GenerateParameters(IDictionary<string, object> dataParameters)
        {
            IList<IDataParameter> parameters = null;

            if (dataParameters.HasAny())
            {
                parameters = [.. dataParameters.Select(x => { return new MySqlParameter(x.Key, dataParameters[x.Key]); })];
            }

            return parameters;
        }
    }
}
