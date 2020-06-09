namespace YeelightAPI.Models
{
    /// <summary>
    /// Power-ON mode
    /// </summary>
    public enum PowerOnMode
    {
        /// <summary>
        /// Normal
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Color Temperature
        /// </summary>
        Ct = 1,

        /// <summary>
        /// RGB color
        /// </summary>
        Rgb = 2,

        /// <summary>
        /// HSV color
        /// </summary>
        Hsv = 3,

        /// <summary>
        /// Color Flow
        /// </summary>
        ColorFlow = 4,

        /// <summary>
        /// Night (ceiling light only)
        /// </summary>
        Night = 5
    }
}