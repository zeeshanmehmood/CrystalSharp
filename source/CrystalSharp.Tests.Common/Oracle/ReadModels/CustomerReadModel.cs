using CrystalSharp.Common.Extensions;
using CrystalSharp.Infrastructure.ReadModels;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrystalSharp.Tests.Common.Oracle.ReadModels
{
    [Table("CUSTOMER", Schema = "SYSTEM")]
    public class CustomerReadModel : ReadModel<int>
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public static (string, string) GetSampleCustomer()
        {
            string name = "Sample Customer";
            string code = "Sample Code";

            return (name, code);
        }

        public static (string, string) GetTestCustomer()
        {
            string name = "TEST Customer";
            string code = "TEST Code";

            return (name, code);
        }

        public static CustomerReadModel Create(Guid globalUId, string name, string code)
        {
            return new CustomerReadModel
            {
                GlobalUId = globalUId,
                Name = name,
                Code = code
            };
        }

        public static CustomerReadModel Create(string name, string code)
        {
            return Create(Guid.Create(), name, code);
        }

        public void Change(string name, string code)
        {
            Name = name;
            Code = code;
        }
    }
}
