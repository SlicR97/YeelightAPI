﻿namespace YeelightAPI.Core
{
    /// <summary>
    /// Helper for colors
    /// </summary>
    internal static class ColorHelper
    {
        /// <summary>
        /// Compute a RGB color
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static int ComputeRgbColor(int r, int g, int b)
        {
            return ((r) << 16) | ((g) << 8) | (b);
        }
    }
}