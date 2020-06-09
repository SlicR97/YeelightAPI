using System;

namespace YeelightAPI.Core
{
    /// <summary>
    /// Attribute to set the real name of a Yeelight Enum
    /// </summary>
    internal class RealNameAttribute : Attribute
    {
        public string PropertyName { get; set; }

        public RealNameAttribute(string name)
        {
            PropertyName = name;
        }
    }
}