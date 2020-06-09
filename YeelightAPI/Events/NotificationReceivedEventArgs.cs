using System;
using YeelightAPI.Models;

namespace YeelightAPI.Events
{
    /// <summary>
    /// Notification event Argument
    /// </summary>
    public class NotificationReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Notification Result
        /// </summary>
        public NotificationResult Result { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public NotificationReceivedEventArgs() { }

        /// <summary>
        /// Constructor with notification result
        /// </summary>
        /// <param name="result"></param>
        public NotificationReceivedEventArgs(NotificationResult result)
        {
            Result = result;
        }
    }
}