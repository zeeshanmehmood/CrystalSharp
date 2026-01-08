using CrystalSharp.Common.Extensions;
using CrystalSharp.Infrastructure;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Sagas
{
    public abstract class SagaStoreDb(IDbManager dbManager, SagaStoreQuery sagaStoreQuery)
    {
        private readonly IDbManager _dbManager = dbManager;
        private readonly SagaStoreQuery _sagaStoreQuery = sagaStoreQuery;

        public abstract IList<IDataParameter> GenerateParameters(IDictionary<string, object> dataParameters);

        public async Task<SagaTransactionMeta> Get(string correlationId, CancellationToken cancellationToken = default)
        {
            (string query, IDictionary<string, object> dataParameters) = _sagaStoreQuery.GetSagaTransactionQuery(correlationId);
            IList<IDataParameter> parameters = GenerateParameters(dataParameters);
            IEnumerable<SagaTransactionMeta> sagaTransactions = await _dbManager.ExecuteQuery<SagaTransactionMeta>(query, parameters).ConfigureAwait(false);
            SagaTransactionMeta sagaTransactionMeta = null;

            if (sagaTransactions.HasAny())
            {
                sagaTransactionMeta = sagaTransactions.FirstOrDefault();
            }

            return sagaTransactionMeta;
        }

        public async Task Upsert(SagaTransactionMeta sagaTransactionMeta, CancellationToken cancellationToken = default)
        {
            SagaTransactionMeta existingSagaTransactionMeta = await Get(sagaTransactionMeta.CorrelationId, cancellationToken).ConfigureAwait(false);
            bool existing = existingSagaTransactionMeta is not null;
            string dataQuery = string.Empty;
            IList<IDataParameter> parameters = null;

            if (!existing)
            {
                sagaTransactionMeta.CreatedAt = SystemDate.UtcNow;
                (string query, IDictionary<string, object> dataParameters) = _sagaStoreQuery.StoreTransactionQuery(
                    sagaTransactionMeta.Id,
                    sagaTransactionMeta.CorrelationId,
                    sagaTransactionMeta.StartedBy,
                    sagaTransactionMeta.Step,
                    (int)sagaTransactionMeta.State,
                    sagaTransactionMeta.ErrorTrail,
                    sagaTransactionMeta.CreatedAt);
                dataQuery = query;
                parameters = GenerateParameters(dataParameters);
            }
            else
            {
                sagaTransactionMeta.ModifiedOn = SystemDate.UtcNow;
                (string query, IDictionary<string, object> dataParameters) = _sagaStoreQuery.ChangeTransactionStateQuery(
                    sagaTransactionMeta.CorrelationId,
                    sagaTransactionMeta.Step,
                    (int)sagaTransactionMeta.State,
                    sagaTransactionMeta.ErrorTrail,
                    sagaTransactionMeta.ModifiedOn.Value);
                dataQuery = query;
                parameters = GenerateParameters(dataParameters);
            }

            await _dbManager.ExecuteNonQuery(dataQuery, parameters).ConfigureAwait(false);
        }
    }
}
