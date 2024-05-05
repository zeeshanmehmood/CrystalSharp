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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using FluentAssertions.Execution;
using CrystalSharp.Domain;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate;
using CrystalSharp.Tests.Common.PostgreSql.Infrastructure;

namespace CrystalSharp.PostgreSql.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.PostgreSqlIntegration)]
    public class PostgreSqlPersistenceTests : IClassFixture<PostgreSqlTestFixture>
    {
        private readonly PostgreSqlTestFixture _testFixture;

        public PostgreSqlPersistenceTests(PostgreSqlTestFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async Task Entity_persisted()
        {
            // Arrange
            IPostgreSqlDataContext sut = _testFixture.DataContext;
            Department department = Department.Create("Information Technology", new DepartmentInfo("IT", "it.department@test.com"));
            await sut.Department.AddAsync(department, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Department result = await sut.Department.SingleOrDefaultAsync(x => x.GlobalUId == department.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Name_code_and_email_are_equal()
        {
            // Arrange
            IPostgreSqlDataContext sut = _testFixture.DataContext;
            Department department = Department.Create("Software Development", new DepartmentInfo("SD", "software.development.department@test.com"));
            await sut.Department.AddAsync(department, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Department result = await sut.Department.SingleOrDefaultAsync(x => x.GlobalUId == department.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Software Development");
                result.DepartmentInfo.Code.Should().Be("SD");
                result.DepartmentInfo.Email.Should().Be("software.development.department@test.com");
            }
        }

        [Fact]
        public async Task New_name_code_and_email_are_equal()
        {
            // Arrange
            IPostgreSqlDataContext sut = _testFixture.DataContext;
            Department department = Department.Create("Quality Control", new DepartmentInfo("QC", "quality.control.department@test.com"));
            await sut.Department.AddAsync(department, CancellationToken.None).ConfigureAwait(false);
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            department.ChangeName("Quality Assurance");
            department.ChangeInfo(new DepartmentInfo("QA", "qa.department@test.com"));
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Department result = await sut.Department.SingleOrDefaultAsync(x => x.GlobalUId == department.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Quality Assurance");
                result.DepartmentInfo.Code.Should().Be("QA");
                result.DepartmentInfo.Email.Should().Be("qa.department@test.com");
            }
        }

        [Fact]
        public async Task Entity_is_deleted()
        {
            // Arrange
            IPostgreSqlDataContext sut = _testFixture.DataContext;
            Department department = Department.Create("Hardware", new DepartmentInfo("HW", "hardware.department@test.com"));
            await sut.Department.AddAsync(department, CancellationToken.None).ConfigureAwait(false);
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            department.Delete();
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Department result = await sut.Department.SingleOrDefaultAsync(x => x.GlobalUId == department.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.EntityStatus.Should().Be(EntityStatus.Deleted);
        }

        [Fact]
        public async Task Find_by_query_expression()
        {
            // Arrange
            IPostgreSqlDataContext sut = _testFixture.DataContext;
            Department department = Department.Create("Finance", new DepartmentInfo("FN", "finance.department@test.com"));
            await sut.Department.AddAsync(department, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Department result = await sut.Department.SingleOrDefaultAsync(x => x.GlobalUId == department.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }
    }
}
