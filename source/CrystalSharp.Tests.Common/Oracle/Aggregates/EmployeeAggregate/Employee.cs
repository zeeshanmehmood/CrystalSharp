using CrystalSharp.Domain;
using CrystalSharp.Tests.Common.Oracle.Aggregates.EmployeeAggregate.Events;
using System.ComponentModel.DataAnnotations.Schema;

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

        public static string GetSampleEmployeeName()
        {
            string name = "Sample";

            return name;
        }

        public static string GetTestEmployeeName()
        {
            string name = "TEST";

            return name;
        }

        public static Employee Create(string name, string code)
        {
            Employee employee = new() { Name = name, Code = code };

            ValidateEmployee(employee);

            employee.Raise(new EmployeeCreatedDomainEvent(employee.GlobalUId, employee.Name, employee.Code));

            return employee;
        }

        public void ChangeName(string name)
        {
            Name = name;

            ValidateEmployee(this);
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
