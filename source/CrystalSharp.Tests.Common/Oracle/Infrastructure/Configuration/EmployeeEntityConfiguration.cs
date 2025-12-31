using CrystalSharp.Tests.Common.Oracle.Aggregates.EmployeeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrystalSharp.Tests.Common.Oracle.Infrastructure.Configuration
{
    public class EmployeeEntityConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.Property(x => x.Id).HasColumnName(nameof(Employee.Id).ToUpper());
            builder.Property(x => x.GlobalUId).HasColumnName(nameof(Employee.GlobalUId).ToUpper());
            builder.Property(x => x.EntityStatus).HasColumnName(nameof(Employee.EntityStatus).ToUpper());
            builder.Property(x => x.CreatedAt).HasColumnName(nameof(Employee.CreatedAt).ToUpper());
            builder.Property(x => x.ModifiedOn).HasColumnName(nameof(Employee.ModifiedOn).ToUpper());
            builder.Property(x => x.Version).HasColumnName(nameof(Employee.Version).ToUpper());
            builder.Property(x => x.Name).HasColumnName(nameof(Employee.Name).ToUpper());
            builder.Property(x => x.Code).HasColumnName(nameof(Employee.Code).ToUpper());
        }
    }
}
