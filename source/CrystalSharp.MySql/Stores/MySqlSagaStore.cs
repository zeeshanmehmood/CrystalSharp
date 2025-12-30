using CrystalSharp.Common.Extensions;
using CrystalSharp.Dapper.Common;
using CrystalSharp.Sagas;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CrystalSharp.MySql.Stores
{
    public class MySqlSagaStore(
        string sagaStoreConnectionString,
        bool useSchema,
        string schema) : SagaStoreDb(new DbManager(new MySqlConnection(sagaStoreConnectionString)), new("`", "`", useSchema, schema)), ISagaStore
    {
        public override IList<IDataParameter> GenerateParameters(IDictionary<string, object> dataParameters)
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
