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
using System.Linq;
using CrystalSharp.Common.Extensions;

namespace CrystalSharp.Infrastructure.EventStoresPersistence
{
    public abstract class EventStorePersistence
    {
        public abstract long GetExpectedVersion(long originalVersion);

        public string IdToStreamName(Type type, Guid id)
        {
            return type.ToStreamName(id);
        }

        public IEnumerable<EventDataItem<TEvent>> PrepareEventData<TEvent>(IEnumerable<TEvent> list,
            IDictionary<string, object> headers)
            where TEvent : class
        {
            IEnumerable<EventDataItem<TEvent>> eventsToSave = null;

            if (list.HasAny())
            {
                eventsToSave = list.Select(e => new EventDataItem<TEvent> { Headers = headers, Event = e });
            }

            return eventsToSave;
        }
    }
}
