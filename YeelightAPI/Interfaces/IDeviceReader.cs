using System.Collections.Generic;
using System.Threading.Tasks;
using YeelightAPI.Models;
using YeelightAPI.Models.Cron;

namespace YeelightAPI.Interfaces
{
    /// <summary>
    /// Descriptor for Device Reading operations
    /// </summary>
    public interface IDeviceReader
    {
        /// <summary>
        /// Get a cron task
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<CronResult> CronGet(CronType type = CronType.PowerOff);

        /// <summary>
        /// Get all properties values
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<Properties, object>> GetAllProps();

        /// <summary>
        /// Get a single property value
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        Task<object> GetProp(Properties prop);

        /// <summary>
        /// Get multiple properties values
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        Task<Dictionary<Properties, object>> GetProps(Properties props);

        /// <summary>
        /// Set the name of the device
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<bool> SetName(string name);
    }
}