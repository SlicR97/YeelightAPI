using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YeelightAPI
{
    /// <summary>
    /// Group of Yeelight Devices
    /// </summary>
    public partial class DeviceGroup : List<Device>, IDisposable
    {
        /// <summary>
        /// Name of the group
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Constructor with one device
        /// </summary>
        /// <param name="name"></param>
        public DeviceGroup(string name = null)
        {
            Name = name;
        }

        /// <summary>
        /// Constructor with one device
        /// </summary>
        /// <param name="device"></param>
        /// <param name="name"></param>
        public DeviceGroup(Device device, string name = null)
        {
            Add(device);
            Name = name;
        }

        /// <summary>
        /// Constructor with devices as params
        /// </summary>
        /// <param name="devices"></param>
        public DeviceGroup(params Device[] devices)
        {
            AddRange(devices);
        }

        /// <summary>
        /// Constructor with a list (IEnumerable) of devices
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="name"></param>
        public DeviceGroup(IEnumerable<Device> devices, string name = null)
        {
            AddRange(devices);
            Name = name;
        }
        
        /// <summary>
        /// Dispose the devices
        /// </summary>
        public void Dispose()
        {
            foreach (var device in this)
            {
                device.Dispose();
            }
        }
        
        /// <summary>
        /// Execute code for all the devices
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        protected async Task<bool> Process(Func<Device, Task<bool>> function)
        {
            var tasks = this.Select(function).ToList();

            await Task.WhenAll(tasks);
            return tasks.All(x => x.Result);
        }
    }
}