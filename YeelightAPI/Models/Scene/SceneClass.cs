namespace YeelightAPI.Models.Scene
{
    /// <summary>
    /// Available classes for a Scene
    /// </summary>
    public enum SceneClass
    {
        /// <summary>
        /// RGB Color
        /// </summary>
        Color,

        /// <summary>
        /// HSV Color
        /// </summary>
        Hsv,

        /// <summary>
        /// Color temperature
        /// </summary>
        Ct,

        /// <summary>
        /// Color Flow
        /// </summary>
        Cf,

        /// <summary>
        /// automatic delay off
        /// </summary>
        AutoDelayOff
    }
}