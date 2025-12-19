using CrystalSharp.Application;
using CrystalSharp.Tests.Common.Application.QueryExecution.ReadModels;
using System.Collections.Generic;

namespace CrystalSharp.Tests.Common.Application.QueryExecution.Queries
{
    public class ConsolidateCustomerTypeQuery : IQuery<QueryExecutionResult<IEnumerable<CustomerTypeReadModel>>>
    {
        public string FirstCustomerType { get; set; }
        public string SecondCustomerType { get; set; }
    }
}
