using CrystalSharp.Tests.Common.MsSql.Aggregates.InvoiceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrystalSharp.Tests.Common.MsSql.Infrastructure.Configuration
{
    public class InvoiceEntityConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder
                .Property(x => x.Code)
                .IsRequired(true);

            builder
                .HasMany(x => x.LineItems)
                .WithOne(y => y.Invoice)
                .HasForeignKey(z => z.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
