using System.Threading.Tasks;
using YeelightAPI.Interfaces;
using YeelightAPI.Models;
using YeelightAPI.Models.Adjust;
using YeelightAPI.Models.ColorFlow;
using YeelightAPI.Models.Cron;
using YeelightAPI.Models.Scene;

namespace YeelightAPI
{
    /// <summary>
    /// Group of Yeelight Devices : IDeviceController implementation
    /// </summary>
    public partial class DeviceGroup : IDeviceController
    {
        /// <summary>
        /// Adjusts the brightness
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> AdjustBright(int percent, int? smooth = null)
            => await Process(device => device.AdjustBright(percent, smooth));

        /// <summary>
        /// Adjusts the color
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> AdjustColor(int percent, int? smooth = null)
            => await Process(device => device.AdjustColor(percent, smooth));

        /// <summary>
        /// Adjusts the color temperature
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> AdjustColorTemperature(int percent, int? smooth = null)
            => await Process(device => device.AdjustColorTemperature(percent, smooth));

        /// <summary>
        /// Connect all the devices
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Connect()
            => await Process(device => device.Connect());

        /// <summary>
        /// Add a cron task for all devices
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<bool> CronAdd(int value, CronType type = CronType.PowerOff)
            => await Process(device => device.CronAdd(value, type));

        /// <summary>
        /// Delete a cron task for all devices
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<bool> CronDelete(CronType type = CronType.PowerOff)
            => await Process(device => device.CronDelete(type));

        /// <summary>
        /// Disconnect all the devices
        /// </summary>
        public void Disconnect()
        {
            foreach (var device in this)
            {
                device.Disconnect();
            }
        }

        /// <summary>
        /// Initiate a new Color Flow
        /// </summary>
        /// <returns></returns>
        public FluentFlow Flow()
        {
            return new FluentFlow(StartColorFlow, StopColorFlow);
        }

        /// <summary>
        /// Adjusts the state of all the devices
        /// </summary>
        /// <param name="action"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public async Task<bool> SetAdjust(AdjustAction action, AdjustProperty property)
            => await Process(device => device.SetAdjust(action, property));

        /// <summary>
        /// Set the brightness for all the devices
        /// </summary>
        /// <param name="value"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> SetBrightness(int value, int? smooth = null)
            => await Process(device => device.SetBrightness(value, smooth));

        /// <summary>
        /// Set the color temperature for all the devices
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> SetColorTemperature(int temperature, int? smooth = null)
            => await Process(device => device.SetColorTemperature(temperature, smooth));

        /// <summary>
        /// Set the current state as the default one
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetDefault()
            => await Process(device => device.SetDefault());

        /// <summary>
        /// Change HSV color asynchronously for all devices
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="sat"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> SetHsvColor(int hue, int sat, int? smooth = null)
            => await Process(device => device.SetHsvColor(hue, sat, smooth));

        /// <summary>
        /// Set the power for all the devices
        /// </summary>
        /// <param name="state"></param>
        /// <param name="smooth"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<bool> SetPower(bool state = true, int? smooth = null, PowerOnMode mode = PowerOnMode.Normal)
            => await Process(device => device.SetPower(state, smooth, mode));

        /// <summary>
        /// Set the RGB Color for all the devices
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> SetRgbColor(int r, int g, int b, int? smooth = null)
            => await Process(device => device.SetRgbColor(r, g, b, smooth));

        /// <summary>
        ///
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public async Task<bool> SetScene(Scene scene)
            => await Process(device => device.SetScene(scene));

        /// <summary>
        /// Starts a color flow for all devices
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        public async Task<bool> StartColorFlow(ColorFlow flow)
            => await Process(device => device.StartColorFlow(flow));

        /// <summary>
        /// starts the music mode for all devices
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task<bool> StartMusicMode(string hostName, int port)
            => await Process(device => device.StartMusicMode(hostName, port));

        /// <summary>
        /// stops the color flow of all devices
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StopColorFlow()
            => await Process(device => device.StopColorFlow());

        /// <summary>
        /// stops the music mode for all devices
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StopMusicMode()
            => await Process(device => device.StopMusicMode());

        /// <summary>
        /// Toggle the power for all the devices
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Toggle()
            => await Process(device => device.Toggle());

        /// <summary>
        /// Turn-Off the device
        /// </summary>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> TurnOff(int? smooth = null)
            => await Process(device => device.TurnOff(smooth));

        /// <summary>
        /// Turn-On the device
        /// </summary>
        /// <param name="smooth"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<bool> TurnOn(int? smooth = null, PowerOnMode mode = PowerOnMode.Normal)
            => await Process(device => device.TurnOn(smooth, mode));
    }
}