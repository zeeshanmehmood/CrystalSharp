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

using System;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;
using CrystalSharp.Tests.Common.EventStore.Aggregates.CourseAggregate.Events;
using CrystalSharp.Tests.Common.EventStore.Aggregates.CourseAggregate.Snapshots;

namespace CrystalSharp.Tests.Common.EventStore.Aggregates.CourseAggregate
{
    public class Course : SnapshotAggregateRoot<string, CourseSnapshot>
    {
        public override string Id { get; protected set; } = Guid.NewGuid().ToString("N");
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
            Course course = new Course { Name = name, CourseInfo = courseInfo };

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
