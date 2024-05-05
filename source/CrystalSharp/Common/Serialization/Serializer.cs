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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using CrystalSharp.Common.Settings;

namespace CrystalSharp.Common.Serialization
{
    public static class Serializer
    {
        public static string Serialize(object value, bool typeNameHandling = false)
        {
            JsonSerializerSettings settings = new()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateFormatString = DateTimePattern.JsonDateTimePattern,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            if (typeNameHandling)
            {
                settings.TypeNameHandling = TypeNameHandling.Objects;
            }

            string json = JsonConvert.SerializeObject(value, settings);

            return json;
        }

        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(json, DefaultJsonDeserializerSettings());
        }

        public static object Deserialize(string json, Type type)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new object();
            }

            return JsonConvert.DeserializeObject(json, type, DefaultJsonDeserializerSettings());
        }

        public static T GetPropertyValue<T>(string json, string propertyPath)
        {
            JObject jObject = JObject.Parse(json);
            JToken jToken = jObject.SelectToken(propertyPath);

            return jToken.ToObject<T>();
        }

        public static T DeepCopy<T>(T @object, bool typeNameHandling = false)
        {
            string json = Serialize(@object, typeNameHandling);

            return Deserialize<T>(json);
        }

        private static JsonSerializerSettings DefaultJsonDeserializerSettings()
        {
            JsonSerializerSettings settings = new()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                DateFormatString = DateTimePattern.ShortDatePattern
            };

            settings.Converters.Add(new IsoDateTimeConverter { DateTimeFormat = DateTimePattern.LongDatePattern });

            return settings;
        }
    }
}
