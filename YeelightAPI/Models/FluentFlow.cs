using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YeelightAPI.Core;
using YeelightAPI.Models.ColorFlow;

namespace YeelightAPI.Models
{
    /// <summary>
    /// Fluent-syntax Color FLow
    /// </summary>
    public class FluentFlow
    {
        private readonly List<ColorFlowExpression> _expressions = new List<ColorFlowExpression>();

        private readonly Func<ColorFlow.ColorFlow, Task<bool>> _startColorFlowMethod;

        private readonly Func<Task<bool>> _stopColorFlowMethod;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        internal FluentFlow(Func<ColorFlow.ColorFlow, Task<bool>> start, Func<Task<bool>> stop)
        {
            _startColorFlowMethod = start;
            _stopColorFlowMethod = stop;
        }
        
        /// <summary>
        /// Set the duration of the previous action
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public FluentFlow During(int duration)
        {
            CheckExpressions();

            _expressions.Last().Duration = Math.Max(duration, Constants.MinimumFlowExpressionDuration);

            return this;
        }

        /// <summary>
        /// Play the color flow
        /// </summary>
        /// <param name="endAction"></param>
        /// <param name="repetition"></param>
        /// <returns></returns>
        public async Task<FluentFlow> Play(ColorFlowEndAction endAction = ColorFlowEndAction.Restore, int repetition = 0)
        {
            CheckExpressions();

            var flow = new ColorFlow.ColorFlow(repetition, endAction, _expressions);

            await _startColorFlowMethod(flow);

            return this;
        }

        /// <summary>
        /// Play the color flow after the specified delay
        /// </summary>
        /// <param name="millisecondsDelay"></param>
        /// <param name="endAction"></param>
        /// <param name="repetition"></param>
        /// <returns></returns>
        public async Task<FluentFlow> PlayAfter(int millisecondsDelay, ColorFlowEndAction endAction, int repetition = 0)
        {
            CheckExpressions();

            await Task.Delay(millisecondsDelay);

            return await Play(endAction, repetition);
        }

        /// <summary>
        /// Reset the flow expressions
        /// </summary>
        /// <returns></returns>
        public FluentFlow Reset()
        {
            _expressions.Clear();

            return this;
        }

        /// <summary>
        /// Set a RGB Color
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="brightness"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public FluentFlow RgbColor(int r, int g, int b, int brightness, int duration = Constants.MinimumFlowExpressionDuration)
        {
            _expressions.Add(new ColorFlowRgbExpression(r, g, b, brightness, duration));

            return this;
        }

        /// <summary>
        /// Sleep
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public FluentFlow Sleep(int duration = Constants.MinimumFlowExpressionDuration)
        {
            _expressions.Add(new ColorFlowSleepExpression(duration));

            return this;
        }

        /// <summary>
        /// Stop the color flow
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            await _stopColorFlowMethod();
        }

        /// <summary>
        /// Stop the color flow
        /// </summary>
        /// <returns></returns>
        public async Task StopAfter(int milliSecondsDelay)
        {
            await Task.Delay(milliSecondsDelay);

            await Stop();
        }

        /// <summary>
        /// Set the color temperature
        /// </summary>
        /// <param name="temperature"></param>
        /// <param name="brightness"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public FluentFlow Temperature(int temperature, int brightness, int duration = Constants.MinimumFlowExpressionDuration)
        {
            _expressions.Add(new ColorFlowTemperatureExpression(temperature, brightness, duration));

            return this;
        }

        /// <summary>
        /// Throw an exception if the expressions list is empty
        /// </summary>
        private void CheckExpressions()
        {
            if (_expressions.Count == 0)
            {
                throw new InvalidOperationException("The flow must contains at least one expression");
            }
        }
    }
}