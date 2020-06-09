using System;

namespace YeelightAPI.Models
{
    /// <summary>
    /// Available Properties
    /// </summary>
    [Flags]
    public enum Properties
    {
        /// <summary>
        /// None, only exists to prevent Casts exceptions if a value does not exists in the enum
        /// </summary>
        None = 0,

        /// <summary>
        /// on: smart LED is turned on / off: smart LED is turned off
        /// </summary>
        Power = 1,

        /// <summary>
        /// Brightness percentage. Range 1 ~ 100
        /// </summary>
        Bright = 1 << 1,

        /// <summary>
        /// Color temperature. Range 1700 ~ 6500(k)
        /// </summary>
        Ct = 1 << 2,

        /// <summary>
        /// Color. Range 1 ~ 16777215
        /// </summary>
        Rgb = 1 << 3,

        /// <summary>
        /// Hue. Range 0 ~ 359
        /// </summary>
        Hue = 1 << 4,

        /// <summary>
        /// Saturation. Range 0 ~ 100
        /// </summary>
        Sat = 1 << 5,

        /// <summary>
        /// 1: rgb mode / 2: color temperature mode / 3: hsv mode
        /// </summary>
        ColorMode = 1 << 6,

        /// <summary>
        /// 0: no flow is running / 1:color flow is running
        /// </summary>
        Flowing = 1 << 7,

        /// <summary>
        /// The remaining time of a sleep timer. Range 1 ~ 60 (minutes)
        /// </summary>
        DelayOff = 1 << 8,

        /// <summary>
        /// Current flow parameters (only meaningful when 'flowing' is 1)
        /// </summary>
        FlowParams = 1 << 9,

        /// <summary>
        /// 1: Music mode is on / 0: Music mode is off
        /// </summary>
        MusicOn = 1 << 10,

        /// <summary>
        /// The name of the device set by “set_name” command
        /// </summary>
        Name = 1 << 11,

        /// <summary>
        /// Background light power status
        /// </summary>
        BgPower = 1 << 12,

        /// <summary>
        /// Background light is flowing
        /// </summary>
        BgFlowing = 1 << 13,

        /// <summary>
        /// Current flow parameters of background light
        /// </summary>
        BgFlowParams = 1 << 14,

        /// <summary>
        /// Color temperature of background light
        /// </summary>
        BgCt = 1 << 15,

        /// <summary>
        /// 1: rgb mode / 2: color temperature mode / 3: hsv mode
        /// </summary>
        BgLMode = 1 << 16,

        /// <summary>
        /// Brightness percentage of background light
        /// </summary>
        BgBright = 1 << 17,

        /// <summary>
        /// Color of background light
        /// </summary>
        BgRgb = 1 << 18,

        /// <summary>
        /// Hue of background light
        /// </summary>
        BgHue = 1 << 19,

        /// <summary>
        /// Saturation of background light
        /// </summary>
        BgSat = 1 << 20,

        /// <summary>
        /// Brightness of night mode light
        /// </summary>
        NlBr = 1 << 21,

        /// <summary>
        /// 0: daylight mode / 1: moonlight mode (ceiling light only)
        /// </summary>
        ActiveMode = 1 << 22,

        /// <summary>
        /// All Properties
        /// </summary>
        All = ~0
    }
}