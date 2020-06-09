using System.Collections.Generic;

namespace YeelightAPI.Models
{
    /// <summary>
    /// Command used to send to the bulb
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Request Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Method to call
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Parameters
        /// </summary>
        public IList<object> Params { get; set; }
    }
}