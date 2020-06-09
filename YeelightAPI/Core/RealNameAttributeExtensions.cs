using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace YeelightAPI.Core
{
    /// <summary>
    /// Extensions for RealNameAttribute
    /// </summary>
    internal static class RealNameAttributeExtension
    {
        private static readonly ConcurrentDictionary<Enum, string> RealNames = new ConcurrentDictionary<Enum, string>();

        /// <summary>
        /// Retrieve the RealNameAttribute of an enum value
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetRealName(this Enum enumValue)
        {
            if (RealNames.ContainsKey(enumValue))
            {
                // get from the cache
                return RealNames[enumValue];
            }

            //read the attribute
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();

            if (memberInfo == null) return null;
            var attribute = (RealNameAttribute)memberInfo.GetCustomAttributes(typeof(RealNameAttribute), false).FirstOrDefault();

            //adding to cache
            if (attribute == null) return null;
            RealNames.TryAdd(enumValue, attribute.PropertyName);

            return attribute.PropertyName;

        }

        /// <summary>
        /// Gets the enum value with the given <see cref="RealNameAttribute"/>.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="realName">The name of the <see cref="RealNameAttribute"/>.</param>
        /// <param name="result">The enum value.</param>
        /// <returns><see langword="true"/> if a matching enum value was found; otherwise <see langword="false"/>.</returns>
        public static bool TryParseByRealName<TEnum>(string realName, out TEnum result) where TEnum : struct
        {
            // we don't cache anything here, because
            // a) it would require for each enum type a dictionary of realName to value
            // b) the method is only used by device locator and therefore seldom called.

            foreach (var fieldInfo in typeof(TEnum).GetFields())
            {
                var attribute = fieldInfo.GetCustomAttribute<RealNameAttribute>();
                if (attribute?.PropertyName != realName) continue;
                result = (TEnum)fieldInfo.GetValue(null);
                return true;
            }

            result = default;
            return false;
        }
    }
}