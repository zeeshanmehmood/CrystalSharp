using CrystalSharp.Common.Extensions;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.Tests.Common.EventStore.Aggregates.CourseAggregate.Events;
using CrystalSharp.Tests.Common.EventStore.Aggregates.CourseAggregate.Snapshots;
using System;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.CourseAggregate
{
    public class Course : SnapshotAggregateRoot<string, CourseSnapshot>
    {
        public override string Id { get; protected set; } = Guid.Create("N");
        public string Name { get; private set; }
        public CourseInfo CourseInfo { get; private set; }

        private static void ValidateCourse(Course course)
        {
            if (string.IsNullOrEmpty(course.Name))
            {
                course.ThrowDomainException("Course name is required.");
            }

            if (course.CourseInfo.Lectures <= 0)
            {
                course.ThrowDomainException("Course lectures is required.");
            }

            if (course.CourseInfo.Fees <= 0)
            {
                course.ThrowDomainException("Course fees is required.");
            }
        }

        public static Course Create(string name, CourseInfo courseInfo)
        {
            Course course = new() { Name = name, CourseInfo = courseInfo };

            ValidateCourse(course);

            course.Raise(new CourseCreatedDomainEvent(course.GlobalUId, course.Name, course.CourseInfo));

            return course;
        }

        public void ChangeName(string name)
        {
            Name = name;

            ValidateCourse(this);

            Raise(new CourseNameChangedDomainEvent(GlobalUId, Name));
        }

        public void ChangeCourseInfo(CourseInfo courseInfo)
        {
            CourseInfo = courseInfo;

            ValidateCourse(this);

            Raise(new CourseInfoChangedDomainEvent(GlobalUId, CourseInfo));
        }

        public override void Delete()
        {
            base.Delete();
            Raise(new CourseDeletedDomainEvent(GlobalUId));
        }

        private void Apply(CourseCreatedDomainEvent @event)
        {
            Name = @event.Name;
            CourseInfo = @event.CourseInfo;
        }

        private void Apply(CourseNameChangedDomainEvent @event)
        {
            Name = @event.Name;
        }

        private void Apply(CourseInfoChangedDomainEvent @event)
        {
            CourseInfo = @event.CourseInfo;
        }

        private void Apply(CourseDeletedDomainEvent @event)
        {
            //
        }
    }
}
