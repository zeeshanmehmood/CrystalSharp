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
using CrystalSharp.Infrastructure.EventStoresPersistence.Snapshots;

namespace CrystalSharp.Infrastructure.EventStoresPersistence
{
    public class EventStoreQuery
    {
        private readonly string _prefix = string.Empty;
        private readonly string _suffix = string.Empty;
        private readonly bool _useSchema = false;
        private readonly string _schema = string.Empty;
        private readonly string _table = string.Empty;
        private readonly string _columns = string.Empty;
        private readonly string _snapshotTable = string.Empty;
        private readonly string _snapshotColumns = string.Empty;

        public EventStoreQuery(string prefix = "", string suffix = "", bool useSchema = true, string schema = "")
        {
            _prefix = prefix;
            _suffix = suffix;
            _useSchema = useSchema;
            _schema = FormatDbObject(_prefix, _suffix, schema.ToLower());
            _table = EventStoreSettings.EventStoreTable;
            _columns = EventStoreSettings.FormatEventStoreTableColumns(_prefix, _suffix);
            _snapshotTable = SnapshotStoreSettings.SnapshotTable;
            _snapshotColumns = SnapshotStoreSettings.FormatSnapshotTableColumns(_prefix, _suffix);
        }

        public (string, IDictionary<string, object>) GetEventQuery(string streamName)
        {
            StringBuilder query = new();

            query.Append($"SELECT {_columns} FROM {FormatTable(_table)} ");
            query.Append($" WHERE {FormatColumn("EntityStatus")} = @EntityStatus");
            query.Append($" AND {FormatColumn("StreamName")} = @StreamName");
            query.Append($" ORDER BY {FormatColumn("Version")} ASC");

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@EntityStatus", 1 },
                { "@StreamName", streamName }
            };

            return (query.ToString(), parameters);
        }

        public (string, IDictionary<string, object>) GetByVersionQuery(string streamName, long version)
        {
            StringBuilder query = new();

            query.Append($"SELECT {_columns} FROM {FormatTable(_table)} ");
            query.Append($" WHERE {FormatColumn("EntityStatus")} = @EntityStatus");
            query.Append($" AND {FormatColumn("StreamName")} = @StreamName");
            query.Append($" AND {FormatColumn("Version")} = @Version");
            query.Append($" ORDER BY {FormatColumn("Version")} ASC");

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@EntityStatus", 1 },
                { "@StreamName", streamName },
                { "@Version", version }
            };

            return (query.ToString(), parameters);
        }

        public (string, IDictionary<string, object>) GetLastEventQuery(string streamName)
        {
            StringBuilder query = new();

            query.Append($"SELECT {_columns} FROM {FormatTable(_table)} ");
            query.Append($" WHERE {FormatColumn("EntityStatus")} = @EntityStatus");
            query.Append($" AND {FormatColumn("StreamName")} = @StreamName");
            query.Append($" ORDER BY {FormatColumn("Version")} ASC");

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@EntityStatus", 1 },
                { "@StreamName", streamName }
            };

            return (query.ToString(), parameters);
        }

        public (string, IDictionary<string, object>) GetLastVersionQuery(string streamName)
        {
            StringBuilder query = new();

            query.Append($"SELECT MAX({FormatColumn("Version")}) FROM {FormatTable(_table)} ");
            query.Append($" WHERE {FormatColumn("EntityStatus")} = @EntityStatus");
            query.Append($" AND {FormatColumn("StreamName")} = @StreamName");

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@EntityStatus", 1 },
                { "@StreamName", streamName }
            };

            return (query.ToString(), parameters);
        }

        public (string, IDictionary<string, object>) GetLastEventSequence(string streamName)
        {
            StringBuilder query = new();

            query.Append($"SELECT MAX({FormatColumn("Sequence")}) FROM {FormatTable(_table)} ");
            query.Append($" WHERE {FormatColumn("EntityStatus")} = @EntityStatus");
            query.Append($" AND {FormatColumn("StreamName")} = @StreamName");

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@EntityStatus", 1 },
                { "@StreamName", streamName }
            };

            return (query.ToString(), parameters);
        }

        public (string, IDictionary<string, object>) StoreEventQuery(Guid id,
            Guid streamId,
            string streamName,
            long sequence,
            Guid eventId,
            string eventType,
            string eventAssembly,
            int entityStatus,
            DateTime createdOn,
            DateTime occuredOn,
            long version,
            string data)
        {
            StringBuilder query = new();

            query.Append($"INSERT INTO {FormatTable(_table)} ");
            query.Append($" ({FormatColumn("Id")}, {FormatColumn("StreamId")}, {FormatColumn("StreamName")}, {FormatColumn("Sequence")},");
            query.Append($" {FormatColumn("EventId")}, {FormatColumn("EventType")}, {FormatColumn("EventAssembly")},");
            query.Append($" {FormatColumn("EntityStatus")}, {FormatColumn("CreatedOn")}, {FormatColumn("OccuredOn")},");
            query.Append($" {FormatColumn("Version")}, {FormatColumn("Data")})");
            query.Append(" VALUES ");
            query.Append(" (@Id, @StreamId, @StreamName, @Sequence, @EventId, @EventType, @EventAssembly, @EntityStatus, @CreatedOn, @OccuredOn, @Version, @Data)");

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@Id", id },
                { "@StreamId", streamId },
                { "@StreamName", streamName },
                { "@Sequence", sequence },
                { "@EventId", eventId },
                { "@EventType", eventType },
                { "@EventAssembly", eventAssembly },
                { "@EntityStatus", entityStatus },
                { "@CreatedOn", createdOn },
                { "@OccuredOn", occuredOn },
                { "@Version", version },
                { "@Data", data }
            };

            return (query.ToString(), parameters);
        }

        public (string, IDictionary<string, object>) DeleteEventQuery(string streamName)
        {
            StringBuilder query = new();

            query.Append($"UPDATE {FormatTable(_table)} SET {FormatColumn("EntityStatus")} = @EntityStatus");
            query.Append($" WHERE {FormatColumn("EntityStatus")} = 1");
            query.Append($" AND {FormatColumn("StreamName")} = @StreamName");

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@EntityStatus", 0 },
                { "@StreamName", streamName }
            };

            return (query.ToString(), parameters);
        }

        public (string, IDictionary<string, object>) SetSnapshotQuery(Guid snapshotId,
            string snapshotAssembly,
            long snapshotVersion,
            string streamName,
            int entityStatus,
            DateTime createdOn,
            string data)
        {
            StringBuilder query = new();

            query.Append($"INSERT INTO {FormatTable(_snapshotTable)} ");
            query.Append($" ({FormatColumn("SnapshotId")}, {FormatColumn("SnapshotAssembly")}, {FormatColumn("SnapshotVersion")},");
            query.Append($" {FormatColumn("StreamName")}, {FormatColumn("EntityStatus")}, {FormatColumn("CreatedOn")}, {FormatColumn("Data")})");
            query.Append(" VALUES ");
            query.Append(" (@SnapshotId, @SnapshotAssembly, @SnapshotVersion, @StreamName, @EntityStatus, @CreatedOn, @Data)");

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@SnapshotId", snapshotId },
                { "@SnapshotAssembly", snapshotAssembly },
                { "@SnapshotVersion", snapshotVersion },
                { "@StreamName", streamName },
                { "@EntityStatus", entityStatus },
                { "@CreatedOn", createdOn },
                { "@Data", data }
            };

            return (query.ToString(), parameters);
        }

        public (string, IDictionary<string, object>) LoadSnapshotQuery(string streamName)
        {
            StringBuilder query = new();

            query.Append($"SELECT {_snapshotColumns} FROM {FormatTable(_snapshotTable)} ");
            query.Append($" WHERE {FormatColumn("EntityStatus")} = @EntityStatus");
            query.Append($" AND {FormatColumn("StreamName")} = @StreamName");
            query.Append($" ORDER BY {FormatColumn("SnapshotVersion")} DESC");

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@EntityStatus", 1 },
                { "@StreamName", streamName }
            };

            return (query.ToString(), parameters);
        }

        public (string, IDictionary<string, object>) GetSnapshotLastVersionQuery(string streamName)
        {
            StringBuilder query = new();

            query.Append($"SELECT MAX({FormatColumn("SnapshotVersion")}) FROM {FormatTable(_snapshotTable)} WHERE {FormatColumn("EntityStatus")} = @EntityStatus");
            query.Append($" AND {FormatColumn("StreamName")} = @StreamName");

            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@EntityStatus", 1 },
                { "@StreamName", streamName }
            };

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
