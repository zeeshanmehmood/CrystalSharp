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

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CrystalSharp.PostgreSql.Database;
using CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate;
using CrystalSharp.Tests.Common.PostgreSql.Infrastructure.Configuration;

namespace CrystalSharp.Tests.Common.PostgreSql.Infrastructure
{
    public class PostgreSqlAppDbContext : DbContext, IPostgreSqlDataContext
    {
        private readonly IPostgreSqlEntityFrameworkCoreContext _entityFrameworkCoreContext;
        public DbSet<Department> Department { get; set; }

        public PostgreSqlAppDbContext()
        {
            //
        }

        public PostgreSqlAppDbContext(DbContextOptions<PostgreSqlAppDbContext> options, IPostgreSqlEntityFrameworkCoreContext entityFrameworkCoreContext)
            : base(options)
        {
            _entityFrameworkCoreContext = entityFrameworkCoreContext;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _entityFrameworkCoreContext.SaveChanges(this, cancellationToken).ConfigureAwait(false);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new DepartmentEntityConfiguration());
        }
    }
}
