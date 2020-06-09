using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YeelightAPI.Core;
using YeelightAPI.Interfaces;
using YeelightAPI.Models;
using YeelightAPI.Models.Cron;

namespace YeelightAPI
{
    /// <summary>
    /// Yeelight Device : IDeviceReader implementation
    /// </summary>
    public partial class Device : IDeviceReader
    {
        /// <summary>
        /// Get a cron JOB
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<CronResult> CronGet(CronType type = CronType.PowerOff)
        {
            var parameters = new List<object>() { (int)type };

            var result = await ExecuteCommandWithResponse<CronResult[]>(
                            Methods.GetCron,
                            parameters);

            return result?.Result?.FirstOrDefault();
        }

        /// <summary>
        /// Get all the properties asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<Properties, object>> GetAllProps()
            => await GetProps(Models.Properties.All);

        /// <summary>
        /// Get a single property value asynchronously
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public async Task<object> GetProp(Properties prop)
        {
            var result = await ExecuteCommandWithResponse<List<string>>(
                Methods.GetProp,
                new List<object>() { prop.ToString() } );

            return result?.Result?.Count == 1 ? result.Result[0] : null;
        }

        /// <summary>
        /// Get multiple properties asynchronously
        /// </summary>
        /// <param name="props"></param>
        /// <returns></returns>
        public async Task<Dictionary<Properties, object>> GetProps(Properties props)
        {
            var names = props.GetRealNames();
            var results = new List<string>();
            if (names.Count <= 20)
            {
                var commandResult = await ExecuteCommandWithResponse<List<string>>(
                    Methods.GetProp,
                    names
                    );
                
                if (commandResult == null) throw new Exception();
                results.AddRange(commandResult.Result);
            }
            else
            {
                var commandResult1 = await ExecuteCommandWithResponse<List<string>>(
                    Methods.GetProp,
                    names.Take(20).ToList() );
                var commandResult2 = await ExecuteCommandWithResponse<List<string>>(
                    Methods.GetProp,
                    names.Skip(20).ToList());
                
                if (commandResult1 == null || commandResult2 == null) throw new Exception();
                results.AddRange(commandResult1.Result);
                results.AddRange(commandResult2.Result);
            }

            if (results.Count <= 0) return null;
            var result = new Dictionary<Properties, object>();

            for (var n = 0; n < names.Count; n++)
            {
                var name = names[n].ToString();

                if (Enum.TryParse(name, out Properties p))
                {
                    result.Add(p, results[n]);
                }
            }

            return result;
        }

        /// <summary>
        /// Set the name of the device
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<bool> SetName(string name)
        {
            var parameters = new List<object>() { name };

            var result = await ExecuteCommandWithResponse<List<string>>(
                            Methods.SetName,
                            parameters);

            if (!result.IsOk()) return false;
            
            Name = name;
            return true;
        }
    }
}