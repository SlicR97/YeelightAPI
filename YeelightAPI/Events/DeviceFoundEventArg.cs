using System;

namespace YeelightAPI.Events
{
    /// <summary>
    /// Device found event argument
    /// </summary>
    public class DeviceFoundEventArgs : EventArgs
    {
        /// <summary>
        /// Notification Result
        /// </summary>
        public Device Device { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DeviceFoundEventArgs() { }

        /// <summary>
        /// Constructor with notification result
        /// </summary>
        /// <param name="device"></param>
        public DeviceFoundEventArgs(Device device)
        {
            Device = device;
        }
    }
}
