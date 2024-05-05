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
using CrystalSharp.Tests.Common.Oracle.Aggregates.EmployeeAggregate;
using CrystalSharp.Tests.Common.Oracle.Infrastructure;

namespace CrystalSharp.Oracle.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.OracleIntegration)]
    public class OraclePersistenceTests : IClassFixture<OracleTestFixture>
    {
        private readonly OracleTestFixture _testFixture;

        public OraclePersistenceTests(OracleTestFixture fixture)
        {
            _testFixture = fixture;
        }

        [Fact]
        public async Task Entity_persisted()
        {
            // Arrange
            IOracleDataContext sut = _testFixture.DataContext;
            Employee employee = Employee.Create("Sylvester Webb", "SLW");
            await sut.Employee.AddAsync(employee, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Employee result = await sut.Employee.SingleOrDefaultAsync(x => x.GlobalUId == employee.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Name_and_code_are_equal()
        {
            // Arrange
            IOracleDataContext sut = _testFixture.DataContext;
            Employee employee = Employee.Create("Mark Anthony", "MRA");
            await sut.Employee.AddAsync(employee, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Employee result = await sut.Employee.SingleOrDefaultAsync(x => x.GlobalUId == employee.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Mark Anthony");
                result.Code.Should().Be("MRA");
            }
        }

        [Fact]
        public async Task New_name_and_code_are_equal()
        {
            // Arrange
            IOracleDataContext sut = _testFixture.DataContext;
            Employee employee = Employee.Create("Jack Wayne", "JWY");
            await sut.Employee.AddAsync(employee, CancellationToken.None).ConfigureAwait(false);
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            employee.Change("Dan Thomas", "DTM");
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Employee result = await sut.Employee.SingleOrDefaultAsync(x => x.GlobalUId == employee.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Name.Should().Be("Dan Thomas");
                result.Code.Should().Be("DTM");
            }
        }

        [Fact]
        public async Task Entity_is_deleted()
        {
            // Arrange
            IOracleDataContext sut = _testFixture.DataContext;
            Employee employee = Employee.Create("Trevor Anderson", "TAD");
            await sut.Employee.AddAsync(employee, CancellationToken.None).ConfigureAwait(false);
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            // Act
            employee.Delete();
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Employee result = await sut.Employee.SingleOrDefaultAsync(x => x.GlobalUId == employee.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.EntityStatus.Should().Be(EntityStatus.Deleted);
        }

        [Fact]
        public async Task Find_by_query_expression()
        {
            // Arrange
            IOracleDataContext sut = _testFixture.DataContext;
            Employee employee = Employee.Create("Ron Christopher", "RCS");
            await sut.Employee.AddAsync(employee, CancellationToken.None).ConfigureAwait(false);

            // Act
            await sut.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            Employee result = await sut.Employee.SingleOrDefaultAsync(x => x.GlobalUId == employee.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
        }
    }
}
