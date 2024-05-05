// The MIT License (MIT)
//
// Copyright (c) 2024 Zeeshan Mehmood
// https://github.com/zeeshanmehmood/CrystalSharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrystalSharp.Application;
using CrystalSharp.Application.Handlers;
using CrystalSharp.Tests.Common.Application.QueryExecution.Queries;
using CrystalSharp.Tests.Common.Application.QueryExecution.ReadModels;

namespace CrystalSharp.Tests.Common.Application.QueryExecution.QueryHandlers
{
    public class ConsolidateCustomerTypeQueryHandler : QueryHandler<ConsolidateCustomerTypeQuery, IEnumerable<CustomerTypeReadModel>>
    {
        public override async Task<QueryExecutionResult<IEnumerable<CustomerTypeReadModel>>> Handle(ConsolidateCustomerTypeQuery request, CancellationToken cancellationToken = default)
        {
            List<CustomerTypeReadModel> readModel = new List<CustomerTypeReadModel>()
            {
                new CustomerTypeReadModel() { Type = request.FirstCustomerType},
                new CustomerTypeReadModel() { Type = request.SecondCustomerType }
            };

            return await Ok(readModel);
        }
    }
}
