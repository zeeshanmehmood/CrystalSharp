using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CrystalSharp.Infrastructure
{
    public interface IDbManager
    {
        Task<IDbTransaction> BeginTransaction();
        Task Commit(IDbTransaction transaction);
        Task Rollback(IDbTransaction transaction);
        Task<IEnumerable<T>> ExecuteQuery<T>(string query, IList<IDataParameter> parameters = null, IDbTransaction transaction = null);
        Task<IEnumerable<T>> ExecuteStoredProcedure<T>(string storedProcedure, IList<IDataParameter> parameters = null, IDbTransaction transaction = null);
        Task<int> ExecuteNonQuery(string query, IList<IDataParameter> parameters = null, IDbTransaction transaction = null);
        Task<object> ExecuteScalar(string query, IList<IDataParameter> parameters = null, IDbTransaction transaction = null);
    }
}
