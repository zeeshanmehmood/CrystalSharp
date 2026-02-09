using CrystalSharp.Common.Exceptions;
using CrystalSharp.Common.Settings;
using CrystalSharp.EntityFrameworkCore.Common.Stores;
using CrystalSharp.Infrastructure.ReadModelStoresPersistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.MySql.Stores
{
    public class MySqlReadModelStore<TDbContext, TKey>(TDbContext dbContext) : ReadModelStore<TDbContext, TKey>(dbContext), IReadModelStore<TKey>
        where TDbContext : DbContext
    {
        public override Task<int> BulkDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
        {
            int errorCode = ReservedErrorCode.SystemError;
            string message = "Read Model Store: \"Bulk delete with Guid\" - This functionality is not available for MySQL.";

            throw new FunctionalityNotAvailableException(errorCode, message);
        }

        public override Task<int> BulkSoftDelete<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
        {
            int errorCode = ReservedErrorCode.SystemError;
            string message = "Read Model Store: \"Bulk soft delete with Guid\" - This functionality is not available for MySQL.";

            throw new FunctionalityNotAvailableException(errorCode, message);
        }

        public override Task<int> BulkRestore<T>(IEnumerable<Guid> globalUIds, CancellationToken cancellationToken = default)
        {
            int errorCode = ReservedErrorCode.SystemError;
            string message = "Read Model Store: \"Bulk restore with Guid\" - This functionality is not available for MySQL.";

            throw new FunctionalityNotAvailableException(errorCode, message);
        }
    }
}
