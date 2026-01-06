using CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate;
using CrystalSharp.Tests.Common.Sagas.Aggregates.TripAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Sagas.Infrastructure
{
    public interface IInMemoryDataContext
    {
        public DbSet<Order> Order { get; set; }
        public DbSet<Trip> Trip { get; set; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
