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

using CrystalSharp.Domain;
using CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate.Events;

namespace CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate
{
    public class Department : AggregateRoot<int>
    {
        public string Name { get; private set; }
        public DepartmentInfo DepartmentInfo { get; private set; }

        private static void ValidateDepartment(Department department)
        {
            if (string.IsNullOrEmpty(department.Name))
            {
                department.ThrowDomainException("Department name is required.");
            }

            if (department.DepartmentInfo == null)
            {
                department.ThrowDomainException("Department info is required.");
            }

            if (string.IsNullOrEmpty(department.DepartmentInfo.Code))
            {
                department.ThrowDomainException("Department code is required.");
            }

            if (department.DepartmentInfo.Code.Length > 3)
            {
                department.ThrowDomainException("Only three characters allowed for department code.");
            }

            if (string.IsNullOrEmpty(department.DepartmentInfo.Email))
            {
                department.ThrowDomainException("Department email is required.");
            }
        }

        public static Department Create(string name, DepartmentInfo departmentInfo)
        {
            Department department = new Department { Name = name, DepartmentInfo = departmentInfo };

            ValidateDepartment(department);

            department.Raise(new DepartmentCreatedDomainEvent(department.GlobalUId, department.Name, department.DepartmentInfo));

            return department;
        }

        public void ChangeName(string name)
        {
            Name = name;

            ValidateDepartment(this);

            Raise(new DepartmentNameChangedDomainEvent(GlobalUId, Name));
        }

        public void ChangeInfo(DepartmentInfo departmentInfo)
        {
            DepartmentInfo = departmentInfo;

            ValidateDepartment(this);

            Raise(new DepartmentInfoChangedDomainEvent(GlobalUId, DepartmentInfo));
        }

        public override void Delete()
        {
            base.Delete();
            Raise(new DepartmentDeletedDomainEvent(GlobalUId, Name, DepartmentInfo));
        }

        private void Apply(DepartmentCreatedDomainEvent @event)
        {
            Name = @event.Name;
            DepartmentInfo = @event.DepartmentInfo;
        }

        private void Apply(DepartmentNameChangedDomainEvent @event)
        {
            Name = @event.Name;
        }

        private void Apply(DepartmentInfoChangedDomainEvent @event)
        {
            DepartmentInfo = @event.DepartmentInfo;
        }

        private void Apply(DepartmentDeletedDomainEvent @event)
        {
            //
        }
    }
}
