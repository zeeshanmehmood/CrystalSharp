using CrystalSharp.Envoy.Contracts;
using CrystalSharp.Tests.Common.Envoy.Responses;

namespace CrystalSharp.Tests.Common.Envoy.Requests
{
    public class CreateProductRequest : IRequest<CreateProductResponse>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
