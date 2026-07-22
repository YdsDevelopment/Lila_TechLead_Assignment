using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace TicTacToe
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings _settings;

        static JsonHelper()
        {
            _settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                },
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            };
        }

        public static string Serialize<T>(T payload)
        {
            return JsonConvert.SerializeObject(payload, _settings);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        public static T DeserializeFromToken<T>(JToken token)
        {
            return token.ToObject<T>(JsonSerializer.Create(_settings));
        }

        public static bool TryDeserialize<T>(string json, out T result)
        {
            try
            {
                result = Deserialize<T>(json);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public static string ToJsonString(object payload, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(payload, formatting, _settings);
        }

        public static string PrettyPrint(string json)
        {
            try
            {
                var obj = JToken.Parse(json);
                return obj.ToString(Formatting.Indented);
            }
            catch
            {
                return json;
            }
        }

        public static Dictionary<string, object> ToDictionary(object payload)
        {
            var json = Serialize(payload);
            return Deserialize<Dictionary<string, object>>(json);
        }
    }
}
