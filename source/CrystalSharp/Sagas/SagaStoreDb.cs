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

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Infrastructure;

namespace CrystalSharp.Sagas
{
    public abstract class SagaStoreDb
    {
        private readonly IDbManager _dbManager;
        private readonly SagaStoreQuery _sagaStoreQuery;

        protected SagaStoreDb(IDbManager dbManager, SagaStoreQuery sagaStoreQuery)
        {
            _dbManager = dbManager;
            _sagaStoreQuery = sagaStoreQuery;
        }

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
            bool existing = existingSagaTransactionMeta != null;
            string dataQuery = string.Empty;
            IList<IDataParameter> parameters = null;

            if (!existing)
            {
                sagaTransactionMeta.CreatedOn = SystemDate.UtcNow;
                (string query, IDictionary<string, object> dataParameters) = _sagaStoreQuery.StoreTransactionQuery(sagaTransactionMeta.Id,
                    sagaTransactionMeta.CorrelationId,
                    sagaTransactionMeta.StartedBy,
                    sagaTransactionMeta.Step,
                    (int)sagaTransactionMeta.State,
                    sagaTransactionMeta.ErrorTrail,
                    sagaTransactionMeta.CreatedOn);
                dataQuery = query;
                parameters = GenerateParameters(dataParameters);
            }
            else
            {
                sagaTransactionMeta.ModifiedOn = SystemDate.UtcNow;
                (string query, IDictionary<string, object> dataParameters) = _sagaStoreQuery.ChangeTransactionStateQuery(sagaTransactionMeta.CorrelationId,
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
