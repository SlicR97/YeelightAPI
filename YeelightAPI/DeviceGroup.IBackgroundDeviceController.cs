using System.Threading.Tasks;
using YeelightAPI.Interfaces;
using YeelightAPI.Models;
using YeelightAPI.Models.Adjust;
using YeelightAPI.Models.ColorFlow;
using YeelightAPI.Models.Scene;

namespace YeelightAPI
{
    /// <summary>
    /// Group of Yeelight Devices : IBackgroundDeviceController implementation
    /// </summary>
    public partial class DeviceGroup : IBackgroundDeviceController
    {
        /// <summary>
        /// Adjusts the brightness
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundAdjustBright(int percent, int? smooth = null)
            => await Process(device => device.BackgroundAdjustBright(percent, smooth));

        /// <summary>
        /// Adjusts the color
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundAdjustColor(int percent, int? smooth = null)
            => await Process(device => device.BackgroundAdjustColor(percent, smooth));

        /// <summary>
        /// Adjusts the color temperature
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundAdjustColorTemperature(int percent, int? smooth = null)
            => await Process(device => device.BackgroundAdjustColorTemperature(percent, smooth));

        /// <summary>
        /// Initiate a new background Color Flow
        /// </summary>
        /// <returns></returns>
        public FluentFlow BackgroundFlow()
            => new FluentFlow(BackgroundStartColorFlow, BackgroundStopColorFlow);

        /// <summary>
        /// Adjusts background light
        /// </summary>
        /// <param name="action"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetAdjust(AdjustAction action, AdjustProperty property)
            => await Process(device => device.BackgroundSetAdjust(action, property));

        /// <summary>
        /// Set background light color
        /// </summary>
        /// <param name="value"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetBrightness(int value, int? smooth = null)
            => await Process(device => device.BackgroundSetBrightness(value, smooth));

        /// <summary>
        /// Set background light temperature
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetColorTemperature(int temperature, int? smooth)
            => await Process(device => device.BackgroundSetColorTemperature(temperature, smooth));

        /// <summary>
        /// Set the background current state as the default one
        /// </summary>
        /// <returns></returns>
        public async Task<bool> BackgroundSetDefault()
            => await Process(device => device.BackgroundSetDefault());

        /// <summary>
        /// Set background light HSV color
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="sat"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetHsvColor(int hue, int sat, int? smooth = null)
            => await Process(device => device.BackgroundSetHsvColor(hue, sat, smooth));

        /// <summary>
        /// Set background light power
        /// </summary>
        /// <param name="state"></param>
        /// <param name="smooth"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetPower(bool state = true, int? smooth = null, PowerOnMode mode = PowerOnMode.Normal)
            => await Process(device => device.BackgroundSetPower(state, smooth, mode));

        /// <summary>
        /// Set background light RGB color
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetRgbColor(int r, int g, int b, int? smooth)
            => await Process(device => device.BackgroundSetRgbColor(r, g, b, smooth));

        /// <summary>
        /// Set background scene
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetScene(Scene scene)
            => await Process(device => device.BackgroundSetScene(scene));

        /// <summary>
        /// Starts a color flow
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundStartColorFlow(ColorFlow flow)
            => await Process(device => device.BackgroundStartColorFlow(flow));

        /// <summary>
        /// Stops the color flow
        /// </summary>
        /// <returns></returns>
        public async Task<bool> BackgroundStopColorFlow()
            => await Process(device => device.BackgroundStopColorFlow());

        /// <summary>
        /// Toggle background light
        /// </summary>
        /// <returns></returns>
        public async Task<bool> BackgroundToggle()
            => await Process(device => device.BackgroundToggle());

        /// <summary>
        /// Turn-Off the devices background light
        /// </summary>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundTurnOff(int? smooth = null)
            => await Process(device => device.BackgroundTurnOff(smooth));

        /// <summary>
        /// Turn-On the devices background light
        /// </summary>
        /// <param name="smooth"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundTurnOn(int? smooth = null, PowerOnMode mode = PowerOnMode.Normal)
            => await Process(device => device.BackgroundTurnOn(smooth, mode));

        /// <summary>
        /// Toggle Both Background and normal light
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DevToggle() 
            => await Process(device => device.DevToggle());
    }
}