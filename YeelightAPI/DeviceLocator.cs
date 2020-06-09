using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YeelightAPI.Core;
using YeelightAPI.Events;
using YeelightAPI.Models;

namespace YeelightAPI
{
    /// <summary>
    /// Finds devices through LAN
    /// </summary>
    public static class DeviceLocator
    {
        private const string SsdpMessage = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1982\r\nMAN: \"ssdp:discover\"\r\nST: wifi_bulb";
        private static readonly List<object> AllPropertyRealNames = Properties.All.GetRealNames();
        private static readonly char[] Colon = new char[] { ':' };
        private static readonly IPEndPoint MulticastEndPoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1982);
        private static readonly byte[] SsdpDiagram = Encoding.ASCII.GetBytes(SsdpMessage);
        private const string YeelightLocationMatch = "Location: yeelight://";

        /// <summary>
        /// Notification Received event
        /// </summary>
        public static event DeviceFoundEventHandler OnDeviceFound;

        /// <summary>
        /// Notification Received event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void DeviceFoundEventHandler(object sender, DeviceFoundEventArgs e);

        /// <summary>
        /// Discover devices in a specific Network Interface
        /// </summary>
        /// <param name="preferredInterface"></param>
        /// <returns></returns>
        public static async Task<List<Device>> Discover(NetworkInterface preferredInterface)
        {
            var tasks = CreateDiscoverTasks(preferredInterface);
            var devices = new List<Device>();

            if (tasks.Count == 0) return devices;
            await Task.WhenAll(tasks);

            devices.AddRange(tasks.SelectMany(t => t.Result).GroupBy(d => d.Hostname).Select(g => g.First()));

            return devices;
        }

        /// <summary>
        /// Discover devices in LAN
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Device>> Discover()
        {
            var tasks = new List<Task<List<Device>>>();
            var devices = new List<Device>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces().Where(n => n.OperationalStatus == OperationalStatus.Up))
            {
                tasks.AddRange(CreateDiscoverTasks(ni));
            }

            if (tasks.Count == 0) return devices;
            await Task.WhenAll(tasks);

            devices.AddRange(tasks.SelectMany(t => t.Result).GroupBy(d => d.Hostname).Select(g => g.First()));

            return devices;
        }

        /// <summary>
        /// Create Discovery tasks for a specific Network Interface
        /// </summary>
        /// <param name="netInterface"></param>
        /// <returns></returns>
        private static List<Task<List<Device>>> CreateDiscoverTasks(NetworkInterface netInterface)
        {
            var devices = new ConcurrentDictionary<string, Device>();
            var tasks = new List<Task<List<Device>>>();

            try
            {
                var addr = netInterface.GetIPProperties().GatewayAddresses.FirstOrDefault();

                if (addr != null && !addr.Address.ToString().Equals("0.0.0.0"))
                {
                    if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        foreach (var ip in netInterface.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                            for (var cpt = 0; cpt < 3; cpt++)
                            {
                                var t = Task.Factory.StartNew(() =>
                                {
                                    var ssdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                                    {
                                        Blocking = false,
                                        Ttl = 1,
                                        UseOnlyOverlappedIO = true,
                                        MulticastLoopback = false,
                                    };
                                    ssdpSocket.Bind(new IPEndPoint(ip.Address, 0));
                                    ssdpSocket.SetSocketOption(
                                        SocketOptionLevel.IP,
                                        SocketOptionName.AddMembership,
                                        new MulticastOption(MulticastEndPoint.Address));

                                    ssdpSocket.SendTo(SsdpDiagram, SocketFlags.None, MulticastEndPoint);

                                    var start = DateTime.Now;
                                    while (DateTime.Now - start < TimeSpan.FromSeconds(1))
                                    {
                                        var available = ssdpSocket.Available;

                                        if (available > 0)
                                        {
                                            var buffer = new byte[available];
                                            var i = ssdpSocket.Receive(buffer, SocketFlags.None);

                                            if (i > 0)
                                            {
                                                var response = Encoding.UTF8.GetString(buffer.Take(i).ToArray());
                                                var device = GetDeviceInformationFromSsdpMessage(response);

                                                //add only if no device already matching
                                                if(devices.TryAdd(device.Hostname, device))
                                                {
                                                    OnDeviceFound?.Invoke(null, new DeviceFoundEventArgs(device));
                                                }
                                            }
                                        }
                                        Thread.Sleep(10);
                                    }

                                    return devices.Values.ToList();
                                });
                                tasks.Add(t);
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return tasks;
        }

        /// <summary>
        /// Gets the information from a raw SSDP message (host, port)
        /// </summary>
        /// <param name="ssdpMessage"></param>
        /// <returns></returns>
        private static Device GetDeviceInformationFromSsdpMessage(string ssdpMessage)
        {
            if (ssdpMessage == null) return null;
            var split = ssdpMessage.Split(new[] { Constants.LineSeparator }, StringSplitOptions.RemoveEmptyEntries);
            string host = null;
            var port = Constants.DefaultPort;
            var properties = new Dictionary<string, object>();
            var supportedMethods = new List<Methods>();
            string id = null;
            var model = default(Model);

            foreach (var part in split)
            {
                if (part.StartsWith(YeelightLocationMatch))
                {
                    var url = part.Substring(YeelightLocationMatch.Length);
                    var hostnameParts = url.Split(Colon, StringSplitOptions.RemoveEmptyEntries);
                    if (hostnameParts.Length >= 1)
                    {
                        host = hostnameParts[0];
                    }
                    if (hostnameParts.Length == 2)
                    {
                        int.TryParse(hostnameParts[1], out port);
                    }
                }
                else
                {
                    var property = part.Split(Colon);
                    if (property.Length != 2) continue;
                    var propertyName = property[0].Trim();
                    var propertyValue = property[1].Trim();

                    if (AllPropertyRealNames.Contains(propertyName))
                    {
                        properties.Add(propertyName, propertyValue);
                    }
                    else switch (propertyName)
                    {
                        case "id":
                            id = propertyValue;
                            break;
                        case "model":
                        {
                            if (!RealNameAttributeExtension.TryParseByRealName(propertyValue, out model))
                            {
                                model = default;
                            }

                            break;
                        }
                        case "support":
                        {
                            var supportedOperations = propertyValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var operation in supportedOperations)
                            {
                                if (RealNameAttributeExtension.TryParseByRealName(operation, out Methods method))
                                {
                                    supportedMethods.Add(method);
                                }
                            }

                            break;
                        }
                        case "fw_ver":
                            break;
                    }
                }
            }
            return new Device(host, port, id, model, properties, supportedMethods);

        }
    }
}