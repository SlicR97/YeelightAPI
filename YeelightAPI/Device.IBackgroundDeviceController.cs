using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YeelightAPI.Core;
using YeelightAPI.Interfaces;
using YeelightAPI.Models;
using YeelightAPI.Models.Adjust;
using YeelightAPI.Models.ColorFlow;
using YeelightAPI.Models.Scene;

namespace YeelightAPI
{
    /// <summary>
    /// Yeelight Device : IBackgroundDeviceController implementation
    /// </summary>
    public partial class Device : IBackgroundDeviceController
    {
        /// <summary>
        /// Adjusts the brightness
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundAdjustBright(int percent, int? duration = null)
        {
            var parameters = new List<object>();
            HandlePercentValue(ref parameters, percent);
            parameters.Add(Math.Max(duration ?? 0, Constants.MinimumSmoothDuration));

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.BackgroundAdjustBright,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Adjusts the color
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundAdjustColor(int percent, int? duration = null)
        {
            var parameters = new List<object>();
            HandlePercentValue(ref parameters, percent);
            parameters.Add(Math.Max(duration ?? 0, Constants.MinimumSmoothDuration));

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.BackgroundAdjustColor,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Adjusts the color temperature
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundAdjustColorTemperature(int percent, int? duration = null)
        {
            var parameters = new List<object>();
            HandlePercentValue(ref parameters, percent);
            parameters.Add(Math.Max(duration ?? 0, Constants.MinimumSmoothDuration));

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.BackgroundAdjustColorTemperature,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Initiate a new Background Color Flow
        /// </summary>
        /// <returns></returns>
        public FluentFlow BackgroundFlow()
        {
            return new FluentFlow(BackgroundStartColorFlow, BackgroundStopColorFlow);
        }

        /// <summary>
        /// Adjusts the background light state
        /// </summary>
        /// <param name="action"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetAdjust(AdjustAction action, AdjustProperty property)
        {
            var parameters = new List<object>() { action.ToString(), property.ToString() };

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetBackgroundLightAdjust,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Set the brightness
        /// </summary>
        /// <param name="value"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetBrightness(int value, int? smooth = null)
        {
            var parameters = new List<object>() { value };

            HandleSmoothValue(ref parameters, smooth);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetBackgroundLightBrightness,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Set the background temperature
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetColorTemperature(int temperature, int? smooth = null)
        {
            var parameters = new List<object>() { temperature };

            HandleSmoothValue(ref parameters, smooth);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetBackgroundColorTemperature,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Save the background current state as the default one
        /// </summary>
        /// <returns></returns>
        public async Task<bool> BackgroundSetDefault()
        {
            var result = await ExecuteCommandWithResponse<List<string>>(Methods.SetBackgroundLightDefault);

            return result.IsOk();
        }

        /// <summary>
        /// Set the background light HSV color
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="sat"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetHsvColor(int hue, int sat, int? smooth = null)
        {
            var parameters = new List<object>() { hue, sat };

            HandleSmoothValue(ref parameters, smooth);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetBackgroundLightHsvColor,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Set the power of the device
        /// </summary>
        /// <param name="state"></param>
        /// <param name="smooth"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetPower(bool state = true, int? smooth = null, PowerOnMode mode = PowerOnMode.Normal)
        {
            var parameters = new List<object>() { state ? Constants.On : Constants.Off };
            HandleSmoothValue(ref parameters, smooth);
            parameters.Add((int)mode);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetBackgroundLightPower,
                parameters
            );

            return result.IsOk();
        }

        /// <summary>
        /// set the RGB color
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetRgbColor(int r, int g, int b, int? smooth = null)
        {
            //Convert RGB into integer 0x00RRGGBB
            var value = ColorHelper.ComputeRgbColor(r, g, b);
            var parameters = new List<object>() { value };

            HandleSmoothValue(ref parameters, smooth);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetBackgroundLightRgbColor,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Sets the background Scene
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundSetScene(Scene scene)
        {
            List<object> parameters = scene;

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetBackgroundLightScene,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Starts a background color flow
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundStartColorFlow(ColorFlow flow)
        {
            var parameters = new List<object>() { flow.RepetitionCount * flow.Count, (int)flow.EndAction, flow.GetColorFlowExpression() };

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.StartBackgroundLightColorFlow,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Stops the background color flow
        /// </summary>
        /// <returns></returns>
        public async Task<bool> BackgroundStopColorFlow()
        {
            var result = await ExecuteCommandWithResponse<List<string>>(
                            Methods.StopBackgroundLightColorFlow);

            return result.IsOk();
        }

        /// <summary>
        /// Toggle device
        /// </summary>
        /// <returns></returns>
        public async Task<bool> BackgroundToggle()
        {
            var result = await ExecuteCommandWithResponse<List<string>>(Methods.ToggleBackgroundLight);

            return result.IsOk();
        }

        /// <summary>
        /// Turn-Off the device background light
        /// </summary>
        /// <param name="smooth"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundTurnOff(int? smooth = null)
        {
            var parameters = new List<object>() { Constants.Off };
            HandleSmoothValue(ref parameters, smooth);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetBackgroundLightPower,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Turn-On the device background light
        /// </summary>
        /// <param name="smooth"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<bool> BackgroundTurnOn(int? smooth = null, PowerOnMode mode = PowerOnMode.Normal)
        {
            var parameters = new List<object>() { Constants.On };
            HandleSmoothValue(ref parameters, smooth);
            parameters.Add((int)mode);

            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.SetBackgroundLightPower,
                parameters);

            return result.IsOk();
        }

        /// <summary>
        /// Toggle Both Background and normal light
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DevToggle()
        {
            var result = await ExecuteCommandWithResponse<List<string>>(Methods.ToggleDev);

            return result.IsOk();
        }
    }
}