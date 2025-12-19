using CrystalSharp.Envoy;
using CrystalSharp.Tests.Common.Envoy.Requests;
using CrystalSharp.Tests.Common.Envoy.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Envoy.RequestHandlers
{
    public class CreateProductRequestHandler : IRequestHandler<CreateProductRequest, CreateProductResponse>
    {
        public async Task<CreateProductResponse> Handle(CreateProductRequest request, CancellationToken cancellationToken = default)
        {
            CreateProductResponse response = new() { ProductName = request.Name, ProductPrice = request.Price };

            return await Task.FromResult(response);
        }
    }
}
