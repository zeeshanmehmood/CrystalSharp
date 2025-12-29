using CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrystalSharp.Tests.Common.PostgreSql.Infrastructure.Configuration
{
    public class DepartmentEntityConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.OwnsOne(x => x.DepartmentDetails);
        }
    }
}
