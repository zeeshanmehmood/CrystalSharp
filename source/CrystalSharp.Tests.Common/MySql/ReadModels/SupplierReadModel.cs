using CrystalSharp.Common.Extensions;
using CrystalSharp.Infrastructure.ReadModels;
using System;

namespace CrystalSharp.Tests.Common.MySql.ReadModels
{
    public class SupplierReadModel : ReadModel<int>
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public static (string, string) GetSampleSupplier()
        {
            string name = "Sample Supplier";
            string code = "Sample Code";

            return (name, code);
        }

        public static (string, string) GetTestSupplier()
        {
            string name = "TEST Supplier";
            string code = "TEST Code";

            return (name, code);
        }

        public static SupplierReadModel Create(string name, string code)
        {
            Guid globalUId = Guid.Create();

            return new SupplierReadModel
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
