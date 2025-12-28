using CrystalSharp.Common.Extensions;
using CrystalSharp.Infrastructure.ReadModels;
using System;

namespace CrystalSharp.Tests.Common.MsSql.ReadModels
{
    public class VirtualShopReadModel : ReadModel<int>
    {
        public string Product { get; private set; }
        public decimal Price { get; private set; }

        public static (string, decimal) GetSampleProduct()
        {
            string name = "Sample";
            decimal price = 0;

            return (name, price);
        }

        public static (string, decimal) GetTestProduct()
        {
            string name = "TEST";
            decimal price = 1;

            return (name, price);
        }

        public static VirtualShopReadModel Create(string product, decimal price)
        {
            Guid globalUId = Guid.Create();

            return new VirtualShopReadModel
            {
                GlobalUId = globalUId,
                Product = product,
                Price = price
            };
        }

        public void Change(string product, decimal price)
        {
            Product = product;
            Price = price;
        }
    }
}
