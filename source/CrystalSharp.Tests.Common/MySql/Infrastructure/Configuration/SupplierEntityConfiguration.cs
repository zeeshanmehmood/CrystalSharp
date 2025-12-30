using CrystalSharp.Tests.Common.MySql.Aggregates.SupplierAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrystalSharp.Tests.Common.MySql.Infrastructure.Configuration
{
    public class SupplierEntityConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.OwnsOne(x => x.SupplierDetails);
        }
    }
}
