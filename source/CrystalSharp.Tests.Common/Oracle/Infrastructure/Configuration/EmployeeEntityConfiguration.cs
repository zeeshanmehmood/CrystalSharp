// The MIT License (MIT)
//
// Copyright (c) 2024 Zeeshan Mehmood
// https://github.com/zeeshanmehmood/CrystalSharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CrystalSharp.Tests.Common.Oracle.Aggregates.EmployeeAggregate;

namespace CrystalSharp.Tests.Common.Oracle.Infrastructure.Configuration
{
    public class EmployeeEntityConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.Property(x => x.Id).HasColumnName(nameof(Employee.Id).ToUpper());
            builder.Property(x => x.GlobalUId).HasColumnName(nameof(Employee.GlobalUId).ToUpper());
            builder.Property(x => x.EntityStatus).HasColumnName(nameof(Employee.EntityStatus).ToUpper());
            builder.Property(x => x.CreatedOn).HasColumnName(nameof(Employee.CreatedOn).ToUpper());
            builder.Property(x => x.ModifiedOn).HasColumnName(nameof(Employee.ModifiedOn).ToUpper());
            builder.Property(x => x.Version).HasColumnName(nameof(Employee.Version).ToUpper());
            builder.Property(x => x.Name).HasColumnName(nameof(Employee.Name).ToUpper());
            builder.Property(x => x.Code).HasColumnName(nameof(Employee.Code).ToUpper());
        }
    }
}
