using CrystalSharp.Common.Extensions;
using CrystalSharp.Dapper.Common;
using CrystalSharp.Sagas;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CrystalSharp.PostgreSql.Stores
{
    public class PostgreSqlSagaStore(
        string sagaStoreConnectionString,
        bool useSchema,
        string schema) : SagaStoreDb(new DbManager(new NpgsqlConnection(sagaStoreConnectionString)), new("\"", "\"", useSchema, schema)), ISagaStore
    {
        public override IList<IDataParameter> GenerateParameters(IDictionary<string, object> dataParameters)
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
