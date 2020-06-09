using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using YeelightAPI.Core;
using YeelightAPI.Interfaces;
using YeelightAPI.Models;
using YeelightAPI.Models.Adjust;
using YeelightAPI.Models.ColorFlow;
using YeelightAPI.Models.Cron;
using YeelightAPI.Models.Music;
using YeelightAPI.Models.Scene;

namespace YeelightAPI
{
    /// <summary>
    /// Yeelight Device : IDeviceController implementation
    /// </summary>
    public partial class Device : IDeviceController
    {
        /// <summary>
        /// Adjusts the brightness
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public async Task<bool> AdjustBright(int percent, int? duration = null)
        {
            var parameters = new List<object>();
            HandlePercentValue(ref parameters, percent);
            parameters.Add(Math.Max(duration ?? 0, Constants.MinimumSmoothDuration));

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.AdjustBright,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Adjusts the color
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public async Task<bool> AdjustColor(int percent, int? duration = null)
        {
            var parameters = new List<object>();
            HandlePercentValue(ref parameters, percent);
            parameters.Add(Math.Max(duration ?? 0, Constants.MinimumSmoothDuration));

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.AdjustColor,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Adjusts the color temperature
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public async Task<bool> AdjustColorTemperature(int percent, int? duration = null)
        {
            var parameters = new List<object>();
            HandlePercentValue(ref parameters, percent);
            parameters.Add(Math.Max(duration ?? 0, Constants.MinimumSmoothDuration));

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.AdjustColorTemperature,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Connects to a device asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Connect()
        {
            Disconnect();

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(Hostname, Port);

            if (!_tcpClient.IsConnected())
            {
                return false;
            }

            //continuous receiving
#pragma warning disable 4014
            Watch();
#pragma warning restore 4014

            //Initializing all properties
            var properties = await GetAllProps();

            if (properties == null) return false;
            foreach (var property in properties)
            {
                this[property.Key] = property.Value;
            }

            return true;
        }

        /// <summary>
        /// Add a cron job
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<bool> CronAdd(int value, CronType type = CronType.PowerOff)
        {
            var parameters = new List<object>() { (int)type, value };

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.AddCron,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Delete a cron job
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<bool> CronDelete(CronType type = CronType.PowerOff)
        {
            var parameters = new List<object>() { (int)type };

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.DeleteCron,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Disconnect the current device
        /// </summary>
        /// <returns></returns>
        public void Disconnect()
        {
            if (_tcpClient == null) return;
            _tcpClient.Close();
            _tcpClient = null;
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
        /// Adjusts the state of the device
        /// </summary>
        /// <param name="action"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public async Task<bool> SetAdjust(AdjustAction action, AdjustProperty property)
        {
            var parameters = new List<object>() { action.ToString(), property.ToString() };

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetAdjust,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Change the device brightness asynchronously
        /// </summary>
        /// <param name="value"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> SetBrightness(int value, int? smooth = null)
        {
            var parameters = new List<object>() { value };

            HandleSmoothValue(ref parameters, smooth);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetBrightness,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Change Color Temperature asynchronously
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> SetColorTemperature(int temperature, int? smooth = null)
        {
            var parameters = new List<object>() { temperature };

            HandleSmoothValue(ref parameters, smooth);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetColorTemperature,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Set the current state as the default one
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetDefault()
        {
            var result = await ExecuteCommandWithResponse<List<string>>(Methods.SetDefault);

            return result.IsOk();
        }

        /// <summary>
        /// Change HSV color asynchronously
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="sat"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> SetHsvColor(int hue, int sat, int? smooth = null)
        {
            var parameters = new List<object>() { hue, sat };

            HandleSmoothValue(ref parameters, smooth);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetHsvColor,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Set the device power state asynchronously
        /// </summary>
        /// <param name="state"></param>
        /// <param name="smooth"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<bool> SetPower(bool state = true, int? smooth = null, PowerOnMode mode = PowerOnMode.Normal)
        {
            var parameters = new List<object>() { state ? Constants.On : Constants.Off };
            HandleSmoothValue(ref parameters, smooth);
            parameters.Add((int)mode);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetPower,
                parameters
            );

            return result.IsOk();
        }

        /// <summary>
        /// Change the device RGB color asynchronously
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> SetRgbColor(int r, int g, int b, int? smooth = null)
        {
            //Convert RGB into integer 0x00RRGGBB
            var value = ColorHelper.ComputeRgbColor(r, g, b);
            var parameters = new List<object>() { value };

            HandleSmoothValue(ref parameters, smooth);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetRgbColor,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Set a Scene
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public async Task<bool> SetScene(Scene scene)
        {
            List<object> parameters = scene;

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetScene,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Starts a color flow asynchronously
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        public async Task<bool> StartColorFlow(ColorFlow flow)
        {
            var parameters = new List<object>() { flow.RepetitionCount * flow.Count, (int)flow.EndAction, flow.GetColorFlowExpression() };

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.StartColorFlow,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Starts the music mode
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task<bool> StartMusicMode(string hostName, int port)
        {
            var parameters = new List<object>() { (int)MusicAction.On, hostName, port };

            var result = await ExecuteCommandWithResponse<List<string>>(
                            Methods.SetMusicMode,
                            parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Stops the color flow
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StopColorFlow()
        {
            var result = await ExecuteCommandWithResponse<List<string>>(
                            Methods.StopColorFlow);

            return result.IsOk();
        }

        /// <summary>
        /// Stops the music mode
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StopMusicMode()
        {
            var parameters = new List<object>() { (int)MusicAction.Off };

            var result = await ExecuteCommandWithResponse<List<string>>(
                            Methods.SetMusicMode,
                            parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Toggle the device power asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Toggle()
        {
            var result = await ExecuteCommandWithResponse<List<string>>(Methods.Toggle);

            return result.IsOk();
        }

        /// <summary>
        /// Turn-Off the device
        /// </summary>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> TurnOff(int? smooth = null)
        {
            var parameters = new List<object>() { Constants.Off };
            HandleSmoothValue(ref parameters, smooth);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetPower,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Turn-On the device
        /// </summary>
        /// <param name="smooth"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<bool> TurnOn(int? smooth = null, PowerOnMode mode = PowerOnMode.Normal)
        {
            var parameters = new List<object>() { Constants.On };
            HandleSmoothValue(ref parameters, smooth);
            parameters.Add((int)mode);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetPower,
                parameters);

            return result.IsOk();
        }
    }
}