using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Exceptions;
using CrystalSharp.Tests.Common;
using CrystalSharp.Tests.Common.EventStore.Aggregates.CourseAggregate;
using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.MongoDb.Tests.IntegrationTests
{
    [Trait(TestSettings.Category, TestType.MongoDbEventStoreDbIntegration)]
    public class MongoDbEventStoreDbPersistenceTests(MongoDbTestFixture fixture) : IClassFixture<MongoDbTestFixture>
    {
        private readonly MongoDbTestFixture _testFixture = fixture;

        [Fact]
        public async Task Event_persisted()
        {
            // Arrange
            IAggregateEventStore<string> sut = _testFixture.EventStorePersistence;
            Course course = Course.Create("Modern History", new CourseInfo(10, 2000));

            // Act
            await sut.Store(course, CancellationToken.None).ConfigureAwait(false);
            Course result = await sut.Get<Course>(course.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(result, options => options.IncludingNestedObjects());
        }

        [Fact]
        public async Task Event_version_increased()
        {
            // Arrange
            IAggregateEventStore<string> sut = _testFixture.EventStorePersistence;
            Course course = Course.Create("Programming", new CourseInfo(20, 4000));
            await sut.Store(course, CancellationToken.None).ConfigureAwait(false);
            Course existingCourse = await sut.Get<Course>(course.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Act
            existingCourse.ChangeName("Computer Programming");
            await sut.Store(existingCourse, CancellationToken.None).ConfigureAwait(false);
            Course result = await sut.Get<Course>(existingCourse.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.Version.Should().Be(1);
        }

        [Fact]
        public async Task Get_particular_event_by_version()
        {
            // Arrange
            IAggregateEventStore<string> sut = _testFixture.EventStorePersistence;
            Course course = Course.Create("Advertising", new CourseInfo(7, 1000));
            await sut.Store(course, CancellationToken.None).ConfigureAwait(false);
            Course existingCourse = await sut.Get<Course>(course.GlobalUId, CancellationToken.None).ConfigureAwait(false);
            existingCourse.ChangeName("Online Advertising");
            existingCourse.ChangeCourseInfo(new CourseInfo(8, 1500));
            await sut.Store(existingCourse, CancellationToken.None).ConfigureAwait(false);
            existingCourse.ChangeName("Art of Online Advertising");
            existingCourse.ChangeCourseInfo(new CourseInfo(15, 3000));
            await sut.Store(existingCourse, CancellationToken.None).ConfigureAwait(false);

            // Act
            Course result = await sut.GetByVersion<Course>(existingCourse.GlobalUId, 2, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Version.Should().Be(2);
                result.Name.Should().BeNull();
                result.CourseInfo.Lectures.Should().Be(8);
                result.CourseInfo.Fees.Should().Be(1500);
            }
        }

        [Fact]
        public async Task Read_from_snapshot()
        {
            // Arrange
            IAggregateEventStore<string> sut = _testFixture.EventStorePersistence;
            Course course = Course.Create("Critical Thinking", new CourseInfo(5, 1000));
            await sut.Store(course, CancellationToken.None).ConfigureAwait(false);
            Course existingCourse = await sut.Get<Course>(course.GlobalUId, CancellationToken.None).ConfigureAwait(false);
            existingCourse.ChangeName("Foster Strategic Thinking");
            await sut.Store(existingCourse, CancellationToken.None).ConfigureAwait(false);
            existingCourse.ChangeCourseInfo(new CourseInfo(15, 1500));
            await sut.Store(existingCourse, CancellationToken.None).ConfigureAwait(false);
            existingCourse.ChangeName("Advanced Thinking");
            existingCourse.ChangeCourseInfo(new CourseInfo(30, 8000));
            await sut.Store(existingCourse, CancellationToken.None).ConfigureAwait(false);
            existingCourse.ChangeName("Creative Thinking");
            await sut.Store(existingCourse, CancellationToken.None).ConfigureAwait(false);
            existingCourse.ChangeCourseInfo(new CourseInfo(5, 2000));
            await sut.Store(existingCourse, CancellationToken.None).ConfigureAwait(false);
            existingCourse.ChangeName("Adaptive Thinking");
            await sut.Store(existingCourse, CancellationToken.None).ConfigureAwait(false);
            existingCourse.ChangeCourseInfo(new CourseInfo(10, 300));
            await sut.Store(existingCourse, CancellationToken.None).ConfigureAwait(false);
            existingCourse.ChangeCourseInfo(new CourseInfo(25, 7000));
            await sut.Store(existingCourse, CancellationToken.None).ConfigureAwait(false);

            // Act
            Course result = await sut.Get<Course>(existingCourse.GlobalUId, CancellationToken.None).ConfigureAwait(false);

            // Assert
            using (new AssertionScope())
            {
                result.Version.Should().Be(9);
                result.Name.Should().Be("Adaptive Thinking");
                result.CourseInfo.Lectures.Should().Be(25);
                result.CourseInfo.Fees.Should().Be(7000);
                result.SnapshotVersion.Should().Be(2);
            }
        }

        [Fact]
        public async Task Raise_negative_version_exception()
        {
            // Arrange
            IAggregateEventStore<string> sut = _testFixture.EventStorePersistence;
            Course course = Course.Create("Embedded Systems Programming", new CourseInfo(25, 5000));
            await sut.Store(course, CancellationToken.None).ConfigureAwait(false);

            // Act
            Func<Task<Course>> result = async () => await sut.GetByVersion<Course>(course.GlobalUId, -1, CancellationToken.None).ConfigureAwait(false);

            // Assert
            await result.Should().ThrowAsync<EventStoreNegativeVersionException>();
        }

        [Fact]
        public async Task Raise_stream_not_found_exception()
        {
            // Arrange
            IAggregateEventStore<string> sut = _testFixture.EventStorePersistence;

            // Act
            Func<Task<Course>> result = async () => await sut.Get<Course>(Guid.NewGuid(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            await result.Should().ThrowAsync<EventStoreStreamNotFoundException>();
        }
    }
}
