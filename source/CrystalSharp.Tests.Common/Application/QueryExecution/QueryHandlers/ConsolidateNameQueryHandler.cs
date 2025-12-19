using CrystalSharp.Application;
using CrystalSharp.Application.Handlers;
using CrystalSharp.Tests.Common.Application.QueryExecution.Queries;
using CrystalSharp.Tests.Common.Application.QueryExecution.ReadModels;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.Tests.Common.Application.QueryExecution.QueryHandlers
{
    public class ConsolidateNameQueryHandler : QueryHandler<ConsolidateNameQuery, NameReadModel>
    {
        public override async Task<QueryExecutionResult<NameReadModel>> Handle(ConsolidateNameQuery request, CancellationToken cancellationToken = default)
        {
            NameReadModel readModel = new() { Name = $"{request.FirstName} {request.LastName}" };

            return await Ok(readModel);
        }
    }
}
