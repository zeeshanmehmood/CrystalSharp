using CrystalSharp.Application;
using CrystalSharp.Application.Handlers;
using CrystalSharp.Tests.Common.Application.QueryExecution.Queries;
using CrystalSharp.Tests.Common.Application.QueryExecution.ReadModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Application.QueryExecution.QueryHandlers
{
    public class ConsolidateCustomerTypeQueryHandler : QueryHandler<ConsolidateCustomerTypeQuery, IEnumerable<CustomerTypeReadModel>>
    {
        public override async Task<QueryExecutionResult<IEnumerable<CustomerTypeReadModel>>> Handle(ConsolidateCustomerTypeQuery request, CancellationToken cancellationToken = default)
        {
            List<CustomerTypeReadModel> readModel =
            [
                new() { Type = request.FirstCustomerType},
                new() { Type = request.SecondCustomerType }
            ];

            return await Ok(readModel);
        }
    }
}
