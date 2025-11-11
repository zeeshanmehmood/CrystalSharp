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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Common.Settings;
using CrystalSharp.Domain;
using CrystalSharp.Infrastructure;
using CrystalSharp.Infrastructure.Paging;
using CrystalSharp.Infrastructure.ReadModels;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using CrystalSharp.ReadModelStores.Elasticsearch.Extensions;

namespace CrystalSharp.ReadModelStores.Elasticsearch
{
    public class ElasticsearchReadModelStore<TKey> : IReadModelStore<TKey>
    {
        private readonly ElasticsearchSettings _settings;
        private readonly ElasticClient _elasticClient;

        public ElasticsearchReadModelStore(ElasticsearchSettings settings)
        {
            _settings = settings;
            Uri node = new(_settings.ConnectionString);
            ConnectionSettings connectionSettings = new(node);
            _elasticClient = new ElasticClient(connectionSettings);
        }

        public async Task<bool> Store<T>(T record, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            string index = GenerateIndexName<T>();

            await CreateIndexIfNotExists<T>(index, cancellationToken).ConfigureAwait(false);

            record.CreatedOn = SystemDate.UtcNow;
            IndexResponse response = await _elasticClient.IndexAsync<T>(record, i => i.Index(index), cancellationToken);

            return response.IsValid;
        }

        public async Task<bool> BulkStore<T>(IEnumerable<T> records, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            string index = GenerateIndexName<T>();

            await CreateIndexIfNotExists<T>(index, cancellationToken).ConfigureAwait(false);

            IEnumerable<T> recordsToStore = records.Select((x) => { x.CreatedOn = SystemDate.UtcNow; return x; }).ToList();
            BulkResponse response = await _elasticClient.IndexManyAsync<T>(records, index, cancellationToken).ConfigureAwait(false);

            return response.IsValid;
        }

        public async Task<bool> Update<T>(T record, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            string index = GenerateIndexName<T>();

            await CreateIndexIfNotExists<T>(index, cancellationToken).ConfigureAwait(false);

            record.ModifiedOn = SystemDate.UtcNow;

            DocumentPath<T> recordPath = new(record.Id.ToString());
            IUpdateResponse<T> response = await _elasticClient.UpdateAsync<T>(recordPath,
                i => i.Index(index)
                    .Doc(record),
                cancellationToken)
                .ConfigureAwait(false);

            return response.IsValid;
        }

        public async Task<bool> Delete<T>(TKey id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            string index = GenerateIndexName<T>();
            IDeleteRequest deleteRequest = new DeleteRequest(index, new Id(id.ToString()));
            DeleteResponse response = await _elasticClient.DeleteAsync(deleteRequest, cancellationToken).ConfigureAwait(false);

            return response.IsValid;
        }

        public async Task<bool> Delete<T>(Guid globalUId, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            T record = await Find<T>(globalUId, false, cancellationToken).ConfigureAwait(false);

            if (record is null) return false;

            bool response = await Delete<T>(record.Id, cancellationToken).ConfigureAwait(false);

            return response;
        }

        public async Task<bool> SoftDelete<T>(TKey id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            return await SoftDeleteRecord<T, TKey>(id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> SoftDelete<T>(Guid globalUId, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            return await SoftDeleteRecord<T, Guid>(globalUId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> BulkDelete<T>(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            return await BulkDeleteRecords<T, TKey>(ids, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> BulkDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            return await BulkDeleteRecords<T, Guid>(globalUIds, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> BulkSoftDelete<T>(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            return await BulkSoftDeleteRecords<T, TKey>(ids, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> BulkSoftDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            return await BulkSoftDeleteRecords<T, Guid>(globalUIds, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> Restore<T>(TKey id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            return await RestoreSoftDeletedRecord<T, TKey>(id, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> Restore<T>(Guid globalUId, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            return await RestoreSoftDeletedRecord<T, Guid>(globalUId, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> BulkRestore<T>(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            return await BulkRestoreSoftDeleteRecords<T, TKey>(ids, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> BulkRestore<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            return await BulkRestoreSoftDeleteRecords<T, Guid>(globalUIds, cancellationToken).ConfigureAwait(false);
        }

        public async Task<long> Count<T>(RecordMode recordMode = RecordMode.Active, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            string index = GenerateIndexName<T>();
            CountResponse result = await CountRecords<T>(index, recordMode, cancellationToken).ConfigureAwait(false);
            long response = result.IsValid ? result.Count : -1;

            return response;
        }

        public virtual async Task<long> Count<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            await Task.CompletedTask;

            throw new NotImplementedException("Not implemented.");
        }

        public async Task<T> Find<T>(TKey id, bool tracking = false, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            return await GetRecord<T, TKey>(id, EntityStatus.Active, cancellationToken).ConfigureAwait(false);
        }

        public async Task<T> Find<T>(Guid globalUId, bool tracking = false, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            return await GetRecord<T, Guid>(globalUId, EntityStatus.Active, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<IQueryable<T>> Filter<T>(Expression<Func<T, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            await Task.CompletedTask;

            throw new NotImplementedException("This method is not implemented in the Elasticsearch read model store.");
        }

        public async Task<PagedResult<T>> Get<T>(int skip = 0,
            int take = 10,
            Expression<Func<T, bool>> predicate = null,
            bool tracking = false,
            RecordMode recordMode = RecordMode.Active,
            string sortColumn = "",
            DataSortMode sortMode = DataSortMode.None,
            CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            string index = GenerateIndexName<T>();
            PagedResult<T> result = null;
            CountResponse countResponse = await CountRecords<T>(index, recordMode, cancellationToken).ConfigureAwait(false);
            bool isValidCountResponse = IsValidCountResponse(countResponse);

            if (isValidCountResponse)
            {
                ISearchResponse<T> searchResponse = await _elasticClient.SearchAsync<T>(s =>
                    s.Index(new string[] { index })
                    .MatchAll()
                    .Query(q => GenerateBoolQueryContainer<T>(q, recordMode))
                    .From(skip)
                    .Size(take),
                    cancellationToken)
                    .ConfigureAwait(false);

                result = GenerateResult<T>(skip, take, countResponse.Count, searchResponse);
            }

            return result;
        }

        public async Task<PagedResult<T>> Search<T>(string term,
            bool useWildcard,
            int skip = 0,
            int take = 10,
            bool tracking = false,
            RecordMode recordMode = RecordMode.Active,
            string sortColumn = "",
            DataSortMode sortMode = DataSortMode.None,
            CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            string index = GenerateIndexName<T>();
            PagedResult<T> result = null;
            CountResponse countResponse = await CountRecords<T>(index, recordMode, cancellationToken).ConfigureAwait(false);
            bool isValidCountResponse = IsValidCountResponse(countResponse);

            if (useWildcard)
            {
                term = $"{term}*";
            }

            if (isValidCountResponse)
            {
                ISearchResponse<T> searchResponse = await _elasticClient.SearchAsync<T>(s =>
                    s.Index(new string[] { index })
                    .Query(q => GenerateBoolQueryContainer<T>(q, recordMode, term))
                    .From(skip)
                    .Size(take),
                    cancellationToken)
                    .ConfigureAwait(false);

                long totalCount = (long)searchResponse?.Hits.Count;
                result = GenerateResult<T>(skip, take, totalCount, searchResponse);
            }

            return result;
        }

        private string GenerateIndexName<T>()
        {
            return $"{typeof(T).Name.ToLower()}_{ReservedName.Index.ToLower()}";
        }

        private async Task<bool> IndexExists(string index, CancellationToken cancellationToken = default)
        {
            ExistsResponse existsReponse = await _elasticClient.Indices.ExistsAsync(index, null, cancellationToken).ConfigureAwait(false);

            return existsReponse.Exists;
        }

        private async Task CreateIndexIfNotExists<T>(string index, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            bool existingIndex = await IndexExists(index, cancellationToken).ConfigureAwait(false);

            if (!existingIndex)
            {
                IndexSettings indexSettings = new() { NumberOfReplicas = _settings.NumberOfReplicas, NumberOfShards = _settings.NumberOfShards };
                IndexState indexState = new() { Settings = indexSettings };

                CreateIndexResponse createIndexResponse = await _elasticClient.Indices.CreateAsync(index, descriptor => 
                    descriptor.InitializeUsing(indexState)
                    .Map(map => map.AutoMap()),
                    cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private QueryContainer GenerateBoolQueryContainer<T>(QueryContainerDescriptor<T> queryContainerDescriptor,
            RecordMode recordMode = RecordMode.Active,
            string searchTerm = "") 
            where T : class
        {
            QueryContainer queryContainer;

            if (recordMode == RecordMode.All)
            {
                queryContainer = (string.IsNullOrEmpty(searchTerm)) 
                    ? 
                    new QueryContainer() 
                    : 
                    queryContainerDescriptor.Bool(b => b.Must(m => m.QueryString(qs => qs.Query(searchTerm))));
            }
            else
            {
                int entityStatus = (recordMode == RecordMode.Active) ? 1 : 0;

                queryContainer = (string.IsNullOrEmpty(searchTerm))
                    ?
                    queryContainer = queryContainerDescriptor.Bool(b => b
                    .Filter(f => f
                        .Term(new Field("entityStatus"), entityStatus)))
                    :
                    queryContainer = queryContainerDescriptor.Bool(b => b
                    .Filter(f => f
                        .Term(new Field("entityStatus"), entityStatus))
                    .Must(m => m
                        .QueryString(qs => qs
                            .Query(searchTerm))));
            }

            return queryContainer;
        }

        private async Task<T> GetRecord<T, TId>(TId id, EntityStatus entityStatus, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            string index = GenerateIndexName<T>();
            GetRequest<T> request = new(index, new Id(id.ToString()));
            T record = default;

            IGetResponse<T> response = await _elasticClient.GetAsync<T>(request, cancellationToken).ConfigureAwait(false);

            if (response != null 
                && response.Source != null 
                && response.Source.EntityStatus == entityStatus)
            {
                record = response.Source;
            }

            return record;
        }

        private async Task<bool> SoftDeleteRecord<T, TId>(TId id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            T record = await GetRecord<T, TId>(id, EntityStatus.Active, cancellationToken).ConfigureAwait(false);

            if (record is null) return false;

            record.EntityStatus = EntityStatus.Deleted;
            bool response = await Update<T>(record, cancellationToken).ConfigureAwait(false);

            return response;
        }

        private async Task<bool> BulkDeleteRecords<T, TId>(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            if (!ids.HasAny()) return false;

            string index = GenerateIndexName<T>();
            IList<T> records = new List<T>();
            bool response = false;

            foreach (TId id in ids)
            {
                T record = await GetRecord<T, TId>(id, EntityStatus.Active, cancellationToken).ConfigureAwait(false);

                if (record != null)
                {
                    records.Add(record);
                }
            }

            if (records.Any())
            {
                BulkResponse result = await _elasticClient.DeleteManyAsync<T>(records.AsEnumerable(), index, cancellationToken).ConfigureAwait(false);
                response = result.IsValid;
            }

            return response;
        }

        private async Task<bool> BulkUpdateEntityStatus<T, TId>(IEnumerable<TId> ids,
            EntityStatus fromEntityStatus,
            EntityStatus toEntityStatus,
            CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            if (!ids.HasAny()) return false;

            int recordsCount = 0;
            int updateCount = 0;

            foreach (TId id in ids)
            {
                T record = await GetRecord<T, TId>(id, fromEntityStatus, cancellationToken).ConfigureAwait(false);

                if (record != null)
                {
                    ++recordsCount;

                    record.EntityStatus = toEntityStatus;
                    bool updated = await Update<T>(record, cancellationToken).ConfigureAwait(false);

                    if (updated)
                    {
                        ++updateCount;
                    }
                }
            }

            bool response = updateCount == recordsCount;

            return response;
        }

        private async Task<bool> BulkSoftDeleteRecords<T, TId>(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            if (!ids.HasAny()) return false;

            bool response = await BulkUpdateEntityStatus<T, TId>(ids,
                EntityStatus.Active,
                EntityStatus.Deleted,
                cancellationToken)
                .ConfigureAwait(false);

            return response;
        }

        private async Task<bool> RestoreSoftDeletedRecord<T, TId>(TId id, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            T existingRecord = await GetRecord<T, TId>(id, EntityStatus.Deleted, cancellationToken).ConfigureAwait(false);

            if (existingRecord == null) return false;

            existingRecord.EntityStatus = EntityStatus.Active;
            bool restored = await Update<T>(existingRecord, cancellationToken).ConfigureAwait(false); ;

            return restored;
        }

        private async Task<bool> BulkRestoreSoftDeleteRecords<T, TId>(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
            where T : class, IReadModel<TKey>
        {
            if (!ids.HasAny()) return false;

            return await BulkUpdateEntityStatus<T, TId>(ids,
                EntityStatus.Deleted,
                EntityStatus.Active,
                cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<CountResponse> CountRecords<T>(string index,
            RecordMode recordMode = RecordMode.Active,
            CancellationToken cancellationToken = default)
            where T : class
        {
            CountResponse response = await _elasticClient.CountAsync<T>(i => i.Index(new string[] { index })
                .Query(q => GenerateBoolQueryContainer<T>(q, recordMode)),
                cancellationToken)
                .ConfigureAwait(false);

            return response;
        }

        private bool IsValidCountResponse(CountResponse countResponse)
        {
            return countResponse != null && countResponse.IsValid == true && countResponse.Count > 0;
        }

        private bool IsValidSearchResponse<T>(ISearchResponse<T> searchResponse)
            where T : class, IReadModel<TKey>
        {
            return searchResponse != null && searchResponse.Hits != null && searchResponse.Hits.Count > 0;
        }

        private PagedResult<T> GenerateResult<T>(int skip, int take, long totalCount, ISearchResponse<T> searchResponse) 
            where T : class, IReadModel<TKey>
        {
            if (!IsValidSearchResponse(searchResponse)) return null;

            PagedResult<T> result = new(skip, take, totalCount, searchResponse.Documents);

            return result;
        }
    }
}
