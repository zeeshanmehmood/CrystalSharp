using CrystalSharp.Common.Extensions;
using CrystalSharp.Dapper.Common;
using CrystalSharp.Sagas;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CrystalSharp.MsSql.Stores
{
    public class MsSqlSagaStore(string sagaStoreConnectionString, bool useSchema, string schema)
        : SagaStoreDb(new DbManager(new SqlConnection(sagaStoreConnectionString)), new("[", "]", useSchema, schema)), ISagaStore
    {
        public override IList<IDataParameter> GenerateParameters(IDictionary<string, object> dataParameters)
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
