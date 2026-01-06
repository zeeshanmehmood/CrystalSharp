using CrystalSharp.Tests.Common.Sagas.Aggregates.OrderAggregate;
using CrystalSharp.Tests.Common.Sagas.Aggregates.TripAggregate;
using Microsoft.EntityFrameworkCore;

namespace CrystalSharp.Tests.Common.Sagas.Infrastructure
{
    public class InMemoryDbContext : DbContext, IInMemoryDataContext
    {
        public DbSet<Order> Order { get; set; }
        public DbSet<Trip> Trip { get; set; }

        public InMemoryDbContext()
        {
            //
        }

        public InMemoryDbContext(DbContextOptions<InMemoryDbContext> options)
            : base(options)
        {
            //
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
