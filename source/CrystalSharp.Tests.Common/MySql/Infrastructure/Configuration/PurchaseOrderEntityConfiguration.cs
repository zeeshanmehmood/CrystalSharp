using CrystalSharp.Tests.Common.MySql.Aggregates.PurchaseOrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrystalSharp.Tests.Common.MySql.Infrastructure.Configuration
{
    public class PurchaseOrderEntityConfiguration : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
        {
            builder
                .Property(x => x.Code)
                .IsRequired(true);

            builder
                .HasMany(x => x.OrderItems)
                .WithOne(y => y.PurchaseOrder)
                .HasForeignKey(z => z.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
