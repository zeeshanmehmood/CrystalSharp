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

using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalSharp.Sagas
{
    public class SagaStoreQuery
    {
        private readonly string _prefix = string.Empty;
        private readonly string _suffix = string.Empty;
        private readonly bool _useSchema = false;
        private readonly string _schema = string.Empty;
        private readonly string _table = string.Empty;
        private readonly string _columns = string.Empty;

        public SagaStoreQuery(string prefix = "", string suffix = "", bool useSchema = true, string schema = "")
        {
            _prefix = prefix;
            _suffix = suffix;
            _useSchema = useSchema;
            _schema = FormatDbObject(_prefix, _suffix, schema.ToLower());
            _table = SagaStoreSettings.SagaStoreTable;
            _columns = SagaStoreSettings.FormatSagaStoreTableColumns(_prefix, _suffix);
        }

        public (string, IDictionary<string, object>) GetSagaTransactionQuery(string correlationId)
        {
            StringBuilder query = new();

            query.Append($"SELECT {_columns} FROM {FormatTable(_table)} ");
            query.Append($" WHERE {FormatColumn("CorrelationId")} = @CorrelationId");

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@CorrelationId", correlationId }
            };

            return (query.ToString(), parameters);
        }

        public (string, IDictionary<string, object>) StoreTransactionQuery(string id,
            string correlationId,
            string startedBy,
            string step,
            int state,
            string errorTrail,
            DateTime createdOn)
        {
            StringBuilder query = new();

            query.Append($"INSERT INTO {FormatTable(_table)} ");
            query.Append($" ({FormatColumn("Id")}, {FormatColumn("CorrelationId")}, {FormatColumn("StartedBy")}, ");
            query.Append($" {FormatColumn("Step")}, {FormatColumn("State")}, {FormatColumn("ErrorTrail")}, {FormatColumn("CreatedOn")})");
            query.Append(" VALUES ");
            query.Append(" (@Id, @CorrelationId, @StartedBy, @Step, @State, @ErrorTrail, @CreatedOn)");

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@Id", id },
                { "@CorrelationId", correlationId },
                { "@StartedBy", startedBy },
                { "@Step", step },
                { "@State", state },
                { "@ErrorTrail", errorTrail },
                { "@CreatedOn", createdOn }
            };

            return (query.ToString(), parameters);
        }

        public (string, IDictionary<string, object>) ChangeTransactionStateQuery(string correlationId,
            string step,
            int state,
            string errorTrail,
            DateTime modifiedOn)
        {
            StringBuilder query = new();

            query.Append($"UPDATE {FormatTable(_table)} SET {FormatColumn("Step")} = @Step, ");
            query.Append($" {FormatColumn("State")} = @State, ");

            if (!string.IsNullOrEmpty(errorTrail))
            {
                query.Append($" {FormatColumn("ErrorTrail")} = @ErrorTrail,");
            }

            query.Append($" {FormatColumn("ModifiedOn")} = @ModifiedOn ");
            query.Append($" WHERE {FormatColumn("CorrelationId")} = @CorrelationId");

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@Step", step },
                { "@State", state }
            };

            if (!string.IsNullOrEmpty(errorTrail))
            {
                parameters.Add("@ErrorTrail", errorTrail);
            }

            parameters.Add("@ModifiedOn", modifiedOn);
            parameters.Add("@CorrelationId", correlationId);

            return (query.ToString(), parameters);
        }

        private string FormatColumn(string column)
        {
            return FormatDbObject(_prefix, _suffix, column);
        }

        private string FormatTable(string table)
        {
            return (_useSchema == true) ? $"{_schema}.{FormatDbObject(_prefix, _suffix, table)}" : FormatDbObject(_prefix, _suffix, table);
        }

        private string FormatDbObject(string prefix, string suffix, string dbObject)
        {
            return $"{prefix}{dbObject}{suffix}";
        }
    }
}
