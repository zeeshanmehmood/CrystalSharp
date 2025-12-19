using CrystalSharp.Application;
using CrystalSharp.Tests.Common.Application.QueryExecution.ReadModels;

namespace CrystalSharp.Tests.Common.Application.QueryExecution.Queries
{
    public class ConsolidateNameQuery : IQuery<QueryExecutionResult<NameReadModel>>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
