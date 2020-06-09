using System;
using System.Collections.Generic;
using YeelightAPI.Core;

namespace YeelightAPI.Models.Scene
{
    /// <summary>
    /// Scene
    /// </summary>
    public class Scene : List<object>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parameters"></param>
        public Scene(IEnumerable<object> parameters)
        {
            AddRange(parameters);
        }
        
        /// <summary>
        /// Get a Scene from an auto delay off timing
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="brightness"></param>
        /// <returns></returns>
        public static Scene FromAutoDelayOff(int delay, int brightness)
        {
            var parameters = new List<object>()
            {
                SceneClass.AutoDelayOff.ToString(),
                brightness,
                delay
            };

            return new Scene(parameters);
        }

        /// <summary>
        /// Get a Scene from a color flow
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        public static Scene FromColorFlow(ColorFlow.ColorFlow flow)
        {
            if (flow == null)
            {
                throw new ArgumentNullException(nameof(flow));
            }

            var parameters = new List<object>()
            {
                SceneClass.Cf.ToString(),
                flow.RepetitionCount,
                (int)flow.EndAction,
                flow.GetColorFlowExpression()
            };

            return new Scene(parameters);
        }

        /// <summary>
        /// Get a Scene from a color temperature
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="brightness"></param>
        /// <returns></returns>
        public static Scene FromColorTemperature(int temperature, int brightness)
        {
            var parameters = new List<object>()
            {
                SceneClass.Ct.ToString(),
                temperature,
                brightness
            };

            return new Scene(parameters);
        }

        /// <summary>
        /// Get a Scene from a HSV color
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="sat"></param>
        /// <param name="brightness"></param>
        /// <returns></returns>
        public static Scene FromHsvColor(int hue, int sat, int brightness)
        {
            var parameters = new List<object>()
            {
                SceneClass.Hsv.ToString(),
                hue,
                sat,
                brightness
            };

            return new Scene(parameters);
        }

        /// <summary>
        /// Get a Scene from a RGB color
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="brightness"></param>
        /// <returns></returns>
        public static Scene FromRgbColor(int r, int g, int b, int brightness)
        {
            var parameters = new List<object>()
            {
                SceneClass.Color.ToString(),
                ColorHelper.ComputeRgbColor(r, g, b),
                brightness
            };

            return new Scene(parameters);
        }
    }
}