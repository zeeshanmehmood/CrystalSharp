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

using System.ComponentModel.DataAnnotations.Schema;
using CrystalSharp.Domain;
using CrystalSharp.Tests.Common.Oracle.Aggregates.EmployeeAggregate.Events;

namespace CrystalSharp.Tests.Common.Oracle.Aggregates.EmployeeAggregate
{
    [Table("EMPLOYEE", Schema = "SYSTEM")]
    public class Employee : AggregateRoot<int>
    {
        public string Name { get; private set; }
        public string Code { get; private set; }

        private static void ValidateEmployee(Employee employee)
        {
            if (string.IsNullOrEmpty(employee.Name))
            {
                employee.ThrowDomainException("Employee name is required.");
            }

            if (string.IsNullOrEmpty(employee.Code))
            {
                employee.ThrowDomainException("Employee code is required.");
            }

            if (employee.Code.Length > 3)
            {
                employee.ThrowDomainException("Only three characters allowed for employee code.");
            }
        }

        public static Employee Create(string name, string code)
        {
            Employee employee = new() { Name = name, Code = code };

            ValidateEmployee(employee);

            employee.Raise(new EmployeeCreatedDomainEvent(employee.GlobalUId, employee.Name, employee.Code));

            return employee;
        }

        public void Change(string name, string code)
        {
            Name = name;
            Code = code;

            ValidateEmployee(this);

            Raise(new EmployeeChangedDomainEvent(GlobalUId, Name, Code));
        }

        public override void Delete()
        {
            base.Delete();
            Raise(new EmployeeDeletedDomainEvent(GlobalUId, Name, Code));
        }

        private void Apply(EmployeeCreatedDomainEvent @event)
        {
            Name = @event.Name;
            Code = @event.Code;
        }

        private void Apply(EmployeeChangedDomainEvent @event)
        {
            Name = @event.Name;
            Code = @event.Code;
        }

        private void Apply(EmployeeDeletedDomainEvent @event)
        {
            //
        }
    }
}
