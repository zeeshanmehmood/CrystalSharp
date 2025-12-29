using CrystalSharp.Domain;
using CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate.Events;

namespace CrystalSharp.Tests.Common.PostgreSql.Aggregates.DepartmentAggregate
{
    public class Department : AggregateRoot<int>
    {
        public string Name { get; private set; }
        public DepartmentDetails DepartmentDetails { get; private set; }

        private static void ValidateDepartment(Department department)
        {
            if (string.IsNullOrEmpty(department.Name))
            {
                department.ThrowDomainException("Department name is required.");
            }

            if (department.DepartmentDetails == null)
            {
                department.ThrowDomainException("Department details are required.");
            }

            if (string.IsNullOrEmpty(department.DepartmentDetails.Code))
            {
                department.ThrowDomainException("Department code is required.");
            }

            if (department.DepartmentDetails.Code.Length > 3)
            {
                department.ThrowDomainException("Only three characters allowed for department code.");
            }

            if (string.IsNullOrEmpty(department.DepartmentDetails.Email))
            {
                department.ThrowDomainException("Department email is required.");
            }
        }

        public static string GetSampleDepartmentName()
        {
            string name = "Sample";

            return name;
        }

        public static string GetTestDepartmentName()
        {
            string name = "TEST";

            return name;
        }

        public static Department Create(string name, DepartmentDetails departmentDetails)
        {
            Department department = new() { Name = name, DepartmentDetails = departmentDetails };

            ValidateDepartment(department);

            department.Raise(new DepartmentCreatedDomainEvent(department.GlobalUId, department.Name, department.DepartmentDetails));

            return department;
        }

        public void ChangeName(string name)
        {
            Name = name;

            ValidateDepartment(this);

            Raise(new DepartmentNameChangedDomainEvent(GlobalUId, Name));
        }

        public void ChangeDetails(DepartmentDetails departmentDetails)
        {
            DepartmentDetails = departmentDetails;

            ValidateDepartment(this);

            Raise(new DepartmentDetailsChangedDomainEvent(GlobalUId, DepartmentDetails));
        }

        public override void Delete()
        {
            base.Delete();
            Raise(new DepartmentDeletedDomainEvent(GlobalUId, Name, DepartmentDetails));
        }

        private void Apply(DepartmentCreatedDomainEvent @event)
        {
            Name = @event.Name;
            DepartmentDetails = @event.DepartmentDetails;
        }

        private void Apply(DepartmentNameChangedDomainEvent @event)
        {
            Name = @event.Name;
        }

        private void Apply(DepartmentDetailsChangedDomainEvent @event)
        {
            DepartmentDetails = @event.DepartmentDetails;
        }

        private void Apply(DepartmentDeletedDomainEvent @event)
        {
            //
        }
    }
}
