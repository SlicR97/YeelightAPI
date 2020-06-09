using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using YeelightAPI.Models;

namespace YeelightAPI.Core
{
    /// <summary>
    /// custom converter for Dictionaries with PROPERTIES as key
    /// </summary>
    public class PropertiesDictionaryConverter : JsonConverter
    {
        /// <summary>
        /// Can convert
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        /// <summary>
        /// Read from JSON
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var dict = new Dictionary<Properties, object>();

            foreach (var jToken in jsonObject.Children())
            {
                var child = (JProperty) jToken;
                //skip if the property is not in PROPERTIES enum
                if (Enum.TryParse(child.Name, out Properties prop))
                {
                    //only integers and string
                    dict.Add(prop,
                        child.Value.Type == JTokenType.Integer
                            ? child.ToObject(typeof(decimal))
                            : child.Value.ToString());
                }
            }

            return dict;
        }

        /// <summary>
        /// Write to JSON
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var token = JToken.FromObject(value);
            token.WriteTo(writer);
        }
    }
}