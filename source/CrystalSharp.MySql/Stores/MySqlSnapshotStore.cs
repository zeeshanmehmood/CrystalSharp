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
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using CrystalSharp.Common.Extensions;
using CrystalSharp.Dapper.Common;
using CrystalSharp.Infrastructure.EventStoresPersistence;
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;

namespace CrystalSharp.MySql.Stores
{
    public class MySqlSnapshotStore : SqlSnapshotStorePersistence, ISnapshotStore
    {
        public MySqlSnapshotStore(string eventStoreConnectionString, bool useSchema, string schema)
            : base(new DbManager(new MySqlConnection(eventStoreConnectionString)), new EventStoreQuery("`", "`", useSchema, schema))
        {
            //
        }

        protected override IList<IDataParameter> GenerateParameters(IDictionary<string, object> dataParameters)
        {
            IList<IDataParameter> parameters = null;

            if (dataParameters.HasAny())
            {
                parameters = dataParameters.Select(x => { return new MySqlParameter(x.Key, dataParameters[x.Key]); }).ToList<IDataParameter>();
            }

            return parameters;
        }
    }
}
