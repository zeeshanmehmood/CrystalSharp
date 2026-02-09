using CrystalSharp.Common.Extensions;
using CrystalSharp.Infrastructure;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CrystalSharp.Dapper.Common
{
    public class DbManager(IDbConnection dbConnection) : IDbManager
    {
        private readonly IDbConnection _dbConnection = dbConnection;

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

        public async Task<IEnumerable<T>> ExecuteQuery<T>(
            string query,
            IList<IDataParameter> parameters = null,
            IDbTransaction transaction = null)
        {
            return await ExecuteDbQuery<T>(query, false, parameters, transaction);
        }

        public async Task<IEnumerable<T>> ExecuteStoredProcedure<T>(
            string storedProcedure,
            IList<IDataParameter> parameters = null,
            IDbTransaction transaction = null)
        {
            return await ExecuteDbQuery<T>(storedProcedure, true, parameters, transaction);
        }

        public async Task<int> ExecuteNonQuery(
            string query,
            IList<IDataParameter> parameters = null,
            IDbTransaction transaction = null)
        {
            return await _dbConnection.ExecuteAsync(query, BuildParameters(parameters), transaction).ConfigureAwait(false);
        }

        public async Task<object> ExecuteScalar(
            string query,
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

        private async Task<IEnumerable<T>> ExecuteDbQuery<T>(
            string query,
            bool storedProcedure,
            IList<IDataParameter> parameters,
            IDbTransaction transaction = null)
        {
            CommandType commandType = storedProcedure ? CommandType.StoredProcedure : CommandType.Text;
            IEnumerable<T> result = await _dbConnection.QueryAsync<T>(
                query,
                BuildParameters(parameters),
                transaction,
                commandType: commandType)
                .ConfigureAwait(false);

            return result;
        }
    }
}
