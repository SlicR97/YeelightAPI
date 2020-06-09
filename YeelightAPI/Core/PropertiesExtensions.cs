using System;
using System.Collections.Generic;
using System.Linq;
using YeelightAPI.Models;

namespace YeelightAPI.Core
{
    /// <summary>
    /// Extensions for PROPERTIES enum
    /// </summary>
    internal static class PropertiesExtensions
    {
        /// <summary>
        /// Get the real name of the properties
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static List<object> GetRealNames(this Properties properties)
        {
            var values = Enum.GetValues(typeof(Properties));
            return values
                .Cast<Properties>()
                .Where(m => properties.HasFlag(m) && m != Properties.All && m != Properties.None)
                .Select(x => x.ToString())
                .ToList<object>();
        }
    }
}