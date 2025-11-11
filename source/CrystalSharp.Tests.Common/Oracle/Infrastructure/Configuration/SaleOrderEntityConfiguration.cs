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
using CrystalSharp.Tests.Common.Oracle.Aggregates.SaleOrderAggregate;

namespace CrystalSharp.Tests.Common.Oracle.Infrastructure.Configuration
{
    public class SaleOrderEntityConfiguration : IEntityTypeConfiguration<SaleOrder>
    {
        public void Configure(EntityTypeBuilder<SaleOrder> builder)
        {
            builder.Property(x => x.Id).HasColumnName(nameof(SaleOrder.Id).ToUpper());
            builder.Property(x => x.GlobalUId).HasColumnName(nameof(SaleOrder.GlobalUId).ToUpper());
            builder.Property(x => x.EntityStatus).HasColumnName(nameof(SaleOrder.EntityStatus).ToUpper());
            builder.Property(x => x.CreatedOn).HasColumnName(nameof(SaleOrder.CreatedOn).ToUpper());
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
