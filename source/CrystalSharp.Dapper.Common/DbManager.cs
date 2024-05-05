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
using System.Threading.Tasks;
using Dapper;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Infrastructure;

namespace CrystalSharp.Dapper.Common
{
    public class DbManager : IDbManager
    {
        private readonly IDbConnection _dbConnection;

        public DbManager(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IDbTransaction> BeginTransaction()
        {
            await Task.CompletedTask;

            return _dbConnection.BeginTransaction();
        }

        public async Task Commit(IDbTransaction transaction)
        {
            await Task.CompletedTask;

            transaction.Commit();
        }

        public async Task Rollback(IDbTransaction transaction)
        {
            await Task.CompletedTask;

            transaction.Rollback();
        }

        public async Task<IEnumerable<T>> ExecuteQuery<T>(string query,
            IList<IDataParameter> parameters = null,
            IDbTransaction transaction = null)
        {
            return await ExecuteDbQuery<T>(query, false, parameters, transaction);
        }

        public async Task<IEnumerable<T>> ExecuteStoredProcedure<T>(string storedProcedure,
            IList<IDataParameter> parameters = null,
            IDbTransaction transaction = null)
        {
            return await ExecuteDbQuery<T>(storedProcedure, true, parameters, transaction);
        }

        public async Task<int> ExecuteNonQuery(string query,
            IList<IDataParameter> parameters = null,
            IDbTransaction transaction = null)
        {
            return await _dbConnection.ExecuteAsync(query, BuildParameters(parameters), transaction).ConfigureAwait(false);
        }

        public async Task<object> ExecuteScalar(string query,
            IList<IDataParameter> parameters = null,
            IDbTransaction transaction = null)
        {
            object result = await _dbConnection.ExecuteScalarAsync(query, BuildParameters(parameters), transaction).ConfigureAwait(false);

            return result;
        }

        private static DynamicParameters BuildParameters(IList<IDataParameter> parameters)
        {
            DynamicParameters dataParameters = null;

            if (parameters.HasAny())
            {
                dataParameters = new();

                foreach (IDataParameter parameter in parameters)
                {
                    dataParameters.Add(parameter.ParameterName, parameter.Value);
                }
            }

            return dataParameters;
        }

        private async Task<IEnumerable<T>> ExecuteDbQuery<T>(string query,
            bool storedProcedure,
            IList<IDataParameter> parameters,
            IDbTransaction transaction = null)
        {
            CommandType commandType = storedProcedure ? CommandType.StoredProcedure : CommandType.Text;
            IEnumerable<T> result = await _dbConnection.QueryAsync<T>(query,
                BuildParameters(parameters),
                transaction,
                commandType: commandType)
                .ConfigureAwait(false);

            return result;
        }
    }
}
