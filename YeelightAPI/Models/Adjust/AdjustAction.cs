namespace YeelightAPI.Models.Adjust
{
    /// <summary>
    /// Actions for a adjust command
    /// </summary>
    public enum AdjustAction
    {
        /// <summary>
        /// Increase the value
        /// </summary>
        Increase = 0,

        /// <summary>
        /// Decrease the value
        /// </summary>
        Decrease = 1,

        /// <summary>
        /// Increase the value and go back to 1 if the maximum value is reached
        /// </summary>
        Circle = 2
    }
}