using CrystalSharp.Tests.Common.PostgreSql.Aggregates.ReceiptAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrystalSharp.Tests.Common.PostgreSql.Infrastructure.Configuration
{
    public class ReceiptEntityConfiguration : IEntityTypeConfiguration<Receipt>
    {
        public void Configure(EntityTypeBuilder<Receipt> builder)
        {
            builder
                .Property(x => x.Code)
                .IsRequired(true);

            builder
                .HasMany(x => x.InventoryItems)
                .WithOne(y => y.Receipt)
                .HasForeignKey(z => z.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
