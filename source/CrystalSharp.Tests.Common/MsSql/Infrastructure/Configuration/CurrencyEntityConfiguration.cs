using CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrystalSharp.Tests.Common.MsSql.Infrastructure.Configuration
{
    public class CurrencyEntityConfiguration : IEntityTypeConfiguration<Currency>
    {
        public void Configure(EntityTypeBuilder<Currency> builder)
        {
            builder.OwnsOne(x => x.CurrencyDetails);
        }
    }
}
