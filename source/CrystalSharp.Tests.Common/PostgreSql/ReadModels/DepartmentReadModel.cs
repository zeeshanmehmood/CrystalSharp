using CrystalSharp.Common.Extensions;
using CrystalSharp.Infrastructure.ReadModels;
using System;

namespace CrystalSharp.Tests.Common.PostgreSql.ReadModels
{
    public class DepartmentReadModel : ReadModel<int>
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public static (string, string) GetSampleDepartment()
        {
            string name = "Sample Department";
            string code = "Sample Code";

            return (name, code);
        }

        public static (string, string) GetTestDepartment()
        {
            string name = "TEST Department";
            string code = "TEST Code";

            return (name, code);
        }

        public static DepartmentReadModel Create(string name, string code)
        {
            Guid globalUId = Guid.Create();

            return new DepartmentReadModel
            {
                GlobalUId = globalUId,
                Name = name,
                Code = code
            };
        }

        public void Change(string name, string code)
        {
            Name = name;
            Code = code;
        }
    }
}
