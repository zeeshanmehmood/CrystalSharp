using CrystalSharp.Tests.Common.Oracle.Aggregates.SaleOrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrystalSharp.Tests.Common.Oracle.Infrastructure.Configuration
{
    public class SaleOrderEntityConfiguration : IEntityTypeConfiguration<SaleOrder>
    {
        public void Configure(EntityTypeBuilder<SaleOrder> builder)
        {
            builder.Property(x => x.Id).HasColumnName(nameof(SaleOrder.Id).ToUpper());
            builder.Property(x => x.GlobalUId).HasColumnName(nameof(SaleOrder.GlobalUId).ToUpper());
            builder.Property(x => x.EntityStatus).HasColumnName(nameof(SaleOrder.EntityStatus).ToUpper());
            builder.Property(x => x.CreatedAt).HasColumnName(nameof(SaleOrder.CreatedAt).ToUpper());
            builder.Property(x => x.ModifiedOn).HasColumnName(nameof(SaleOrder.ModifiedOn).ToUpper());
            builder.Property(x => x.Version).HasColumnName(nameof(SaleOrder.Version).ToUpper());
            builder.Property(x => x.Code).HasColumnName(nameof(SaleOrder.Code).ToUpper());
            builder.Property(x => x.TotalAmount).HasColumnName(nameof(SaleOrder.TotalAmount).ToUpper());

            builder
                .Property(x => x.Code)
                .IsRequired(true);

            builder
                .HasMany(x => x.Orders)
                .WithOne(y => y.SaleOrder)
                .HasForeignKey(z => z.SaleOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
