using CrystalSharp.Tests.Common.Oracle.Aggregates.SaleOrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrystalSharp.Tests.Common.Oracle.Infrastructure.Configuration
{
    public class OrderDetailEntityConfiguration : IEntityTypeConfiguration<OrderDetail>
    {
        public void Configure(EntityTypeBuilder<OrderDetail> builder)
        {
            builder.Property(x => x.Id).HasColumnName(nameof(OrderDetail.Id).ToUpper());
            builder.Property(x => x.GlobalUId).HasColumnName(nameof(OrderDetail.GlobalUId).ToUpper());
            builder.Property(x => x.EntityStatus).HasColumnName(nameof(OrderDetail.EntityStatus).ToUpper());
            builder.Property(x => x.CreatedAt).HasColumnName(nameof(OrderDetail.CreatedAt).ToUpper());
            builder.Property(x => x.ModifiedOn).HasColumnName(nameof(OrderDetail.ModifiedOn).ToUpper());
            builder.Property(x => x.SaleOrderId).HasColumnName(nameof(OrderDetail.SaleOrderId).ToUpper());
            builder.Property(x => x.Name).HasColumnName(nameof(OrderDetail.Name).ToUpper());
            builder.Property(x => x.Quantity).HasColumnName(nameof(OrderDetail.Quantity).ToUpper());
            builder.Property(x => x.Price).HasColumnName(nameof(OrderDetail.Price).ToUpper());
        }
    }
}
