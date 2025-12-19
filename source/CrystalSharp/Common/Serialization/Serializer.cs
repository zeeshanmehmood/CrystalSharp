using CrystalSharp.Common.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;

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
