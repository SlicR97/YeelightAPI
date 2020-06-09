namespace YeelightAPI.Models.Cron
{
    /// <summary>
    /// Cron result
    /// </summary>
    public class CronResult
    {
        /// <summary>
        /// ???
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        /// ???
        /// </summary>
        public int Mix { get; set; }

        /// <summary>
        /// Type of the cron task
        /// </summary>
        public int Type { get; set; }
    }
}