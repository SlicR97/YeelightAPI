using Newtonsoft.Json;
using System.Collections.Generic;
using YeelightAPI.Core;

namespace YeelightAPI.Models
{
    /// <summary>
    /// Notification, resulting of a command which has changed the state of the bulb
    /// </summary>
    public class NotificationResult
    {
        /// <summary>
        /// Method
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Parameters
        /// </summary>
        [JsonConverter(typeof(PropertiesDictionaryConverter))]
        public Dictionary<Properties, object> Params { get; set; }
    }
}