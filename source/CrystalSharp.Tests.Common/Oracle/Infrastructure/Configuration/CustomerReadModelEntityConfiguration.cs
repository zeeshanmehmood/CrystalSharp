using CrystalSharp.Tests.Common.Oracle.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrystalSharp.Tests.Common.Oracle.Infrastructure.Configuration
{
    public class CustomerReadModelEntityConfiguration : IEntityTypeConfiguration<CustomerReadModel>
    {
        public void Configure(EntityTypeBuilder<CustomerReadModel> builder)
        {
            builder.Property(x => x.Id).HasColumnName(nameof(CustomerReadModel.Id).ToUpper());
            builder.Property(x => x.GlobalUId).HasColumnName(nameof(CustomerReadModel.GlobalUId).ToUpper());
            builder.Property(x => x.EntityStatus).HasColumnName(nameof(CustomerReadModel.EntityStatus).ToUpper());
            builder.Property(x => x.CreatedAt).HasColumnName(nameof(CustomerReadModel.CreatedAt).ToUpper());
            builder.Property(x => x.ModifiedOn).HasColumnName(nameof(CustomerReadModel.ModifiedOn).ToUpper());
            builder.Property(x => x.Name).HasColumnName(nameof(CustomerReadModel.Name).ToUpper());
            builder.Property(x => x.Code).HasColumnName(nameof(CustomerReadModel.Code).ToUpper());
        }
    }
}
