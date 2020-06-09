using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    /// Yeelight Device
    /// </summary>
    public partial class Device : IDisposable
    {
        /// <summary>
        /// Dictionary of results
        /// </summary>
        private readonly Dictionary<int, ICommandResultHandler> _currentCommandResults = new Dictionary<int, ICommandResultHandler>();

        /// <summary>
        /// lock
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// The unique id to send when executing a command.
        /// </summary>
        private int _uniqueId;

        /// <summary>
        /// TCP client used to communicate with the device
        /// </summary>
        private TcpClient _tcpClient;
        
        /// <summary>
        /// Notification Received event
        /// </summary>
        public event ErrorEventHandler OnError;

        /// <summary>
        /// Notification Received event
        /// </summary>
        public event NotificationReceivedEventHandler OnNotificationReceived;

        /// <summary>
        /// Notification Received event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ErrorEventHandler(object sender, UnhandledExceptionEventArgs e);

        /// <summary>
        /// Notification Received event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void NotificationReceivedEventHandler(object sender, NotificationReceivedEventArgs e);

        /// <summary>
        /// HostName
        /// </summary>
        public string Hostname { get; }

        /// <summary>
        /// The ID.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets a value indicating if the connection to Device is established
        /// </summary>
        // ReSharper disable once InconsistentlySynchronizedField
        public bool IsConnected => _tcpClient != null && _tcpClient.IsConnected();

        /// <summary>
        /// The model.
        /// </summary>
        public Model Model { get; }

        /// <summary>
        /// Port number
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Constructor with a hostname and (optionally) a port number
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="autoConnect"></param>
        public Device(string hostname, int port = Constants.DefaultPort, bool autoConnect = false)
        {
            Hostname = hostname;
            Port = port;

            //auto connect device if specified
            if (autoConnect)
            {
                Connect().Wait();
            }
        }

        internal Device(string hostname, int port, string id, Model model, Dictionary<string, object> properties, List<Methods> supportedOperations)
        {
            Hostname = hostname;
            Port = port;
            Id = id;
            Model = model;
            Properties = properties;
            SupportedOperations = supportedOperations;
        }

        /// <summary>
        /// List of device properties
        /// </summary>
        public readonly Dictionary<string, object> Properties = new Dictionary<string, object>();

        /// <summary>
        /// List of supported operations
        /// </summary>
        public readonly List<Methods> SupportedOperations = new List<Methods>();

        /// <summary>
        /// Name of the device
        /// </summary>
        public string Name
        {
            get => this[Models.Properties.Name] as string;
            set => this[Models.Properties.Name] = value;
        }

        /// <summary>
        /// Access property from its enum value
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public object this[Properties property]
        {
            get => this[property.ToString()];
            set => this[property.ToString()] = value;
        }

        /// <summary>
        /// Access property from its name
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object this[string propertyName]
        {
            get => Properties.ContainsKey(propertyName) ? Properties[propertyName] : null;
            set
            {
                if (Properties.ContainsKey(propertyName))
                {
                    Properties[propertyName] = value;
                }
                else if (!string.IsNullOrWhiteSpace(propertyName))
                {
                    Properties.Add(propertyName, value);
                }
            }
        }
        
        /// <summary>
        /// Dispose the device
        /// </summary>
        public void Dispose()
        {
            lock (_syncLock)
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Execute a command
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        public void ExecuteCommand(Methods method, List<object> parameters = null)
        {
            ExecuteCommand(method, GetUniqueIdForCommand(), parameters);
        }


        /// <summary>
        /// Execute a command and waits for a response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<CommandResult<T>> ExecuteCommandWithResponse<T>(Methods method, List<object> parameters = null)
        {
            return await ExecuteCommandWithResponse<T>(method, GetUniqueIdForCommand(), parameters);
        }


        /// <summary>
        /// Readable value for the device
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Model.ToString()} ({Hostname}:{Port})";
        }

        /// <summary>
        /// Execute a command
        /// </summary>
        /// <param name="method"></param>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
        internal void ExecuteCommand(Methods method, int id, List<object> parameters = null)
        {
            if (!IsMethodSupported(method))
            {
                throw new InvalidOperationException($"The operation {method.GetRealName()} is not allowed by the device");
            }

            var command = new Command()
            {
                Id = id,
                Method = method.GetRealName(),
                Params = parameters ?? new List<object>()
            };

            var data = JsonConvert.SerializeObject(command, Constants.DeviceSerializerSettings);
            var sentData = Encoding.ASCII.GetBytes(data + Constants.LineSeparator); // \r\n is the end of the message, it needs to be sent for the message to be read by the device

            lock (_syncLock)
            {
                _tcpClient.Client.Send(sentData);
            }
        }

        /// <summary>
        /// Execute a command and waits for a response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        internal async Task<CommandResult<T>> ExecuteCommandWithResponse<T>(Methods method, int id, List<object> parameters = null)
        {
            try
            {
                return await UnsafeExecuteCommandWithResponse<T>(method, id, parameters);
            }
            catch (TaskCanceledException) { }

            return null;
        }
        
        /// <summary>
        /// Generate valid parameters for percent values
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="percent"></param>
        private static void HandlePercentValue(ref List<object> parameters, int percent)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            parameters.Add(percent < 0 ? Math.Max(percent, -100) : Math.Min(percent, 100));
        }

        /// <summary>
        /// Generate valid parameters for smooth values
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="smooth"></param>
        private static void HandleSmoothValue(ref List<object> parameters, int? smooth)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (smooth.HasValue)
            {
                parameters.Add("smooth");
                parameters.Add(Math.Max(smooth.Value, Constants.MinimumSmoothDuration));
            }
            else
            {
                parameters.Add("sudden");
                parameters.Add(0); // two parameters needed
            }
        }

        /// <summary>
        /// Check if the method is supported by the device
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private bool IsMethodSupported(Methods method)
        {
            if (SupportedOperations?.Count != 0)
            {
                return SupportedOperations != null && SupportedOperations.Contains(method);
            }

            return true;
            //no supported operations, so we can't check if the operation is permitted
        }

        /// <summary>
        /// Execute a command and waits for a response (Unsafe because of Task Cancellation)
        /// </summary>
        /// <param name="method"></param>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
        /// <exception cref="TaskCanceledException"></exception>
        /// <returns></returns>
        private async Task<CommandResult<T>> UnsafeExecuteCommandWithResponse<T>(Methods method, int id = 0, List<object> parameters = null)
        {
            CommandResultHandler<T> commandResultHandler;
            lock (_currentCommandResults)
            {
                if (_currentCommandResults.TryGetValue(id, out var oldHandler))
                {
                    oldHandler.TrySetCanceled();
                    _currentCommandResults.Remove(id);
                }

                commandResultHandler = new CommandResultHandler<T>();
                _currentCommandResults.Add(id, commandResultHandler);
            }

            try
            {
                ExecuteCommand(method, id, parameters);
                return await commandResultHandler.Task;
            }
            finally
            {
                lock (_currentCommandResults)
                {
                    // remove the command if its the current handler in the dictionary
                    if (_currentCommandResults.TryGetValue(id, out var currentHandler))
                    {
                        if (commandResultHandler == currentHandler)
                            _currentCommandResults.Remove(id);
                    }
                }
            }
        }

        /// <summary>
        /// Watch for device responses and notifications
        /// </summary>
        /// <returns></returns>
        private async Task Watch()
        {
            await Task.Factory.StartNew(async () =>
            {
                //while device is connected
                while (_tcpClient != null)
                {
                    lock (_syncLock)
                    {
                        if (_tcpClient != null)
                        {
                            //automatic re-connection
                            if (!_tcpClient.IsConnected())
                            {
                                _tcpClient.ConnectAsync(Hostname, Port).Wait();
                            }

                            if (_tcpClient.IsConnected())
                            {
                                //there is data available in the pipe
                                if (_tcpClient.Client.Available > 0)
                                {
                                    var bytes = new byte[_tcpClient.Client.Available];

                                    //read data
                                    _tcpClient.Client.Receive(bytes);

                                    try
                                    {
                                        var data = Encoding.UTF8.GetString(bytes);
                                        if (!string.IsNullOrEmpty(data))
                                        {
                                            //get every messages in the pipe
                                            foreach (var entry in data.Split(new[] { Constants.LineSeparator },
                                                StringSplitOptions.RemoveEmptyEntries))
                                            {
                                                var commandResult =
                                                    JsonConvert.DeserializeObject<CommandResult>(entry, Constants.DeviceSerializerSettings);
                                                if (commandResult != null && commandResult.Id != 0)
                                                {
                                                    ICommandResultHandler commandResultHandler;
                                                    lock (_currentCommandResults)
                                                    {
                                                        if (!_currentCommandResults.TryGetValue(commandResult.Id, out commandResultHandler))
                                                            continue; // ignore if the result can't be found
                                                    }

                                                    if (commandResult.Error == null)
                                                    {
                                                        commandResult = (CommandResult)JsonConvert.DeserializeObject(entry, commandResultHandler.ResultType, Constants.DeviceSerializerSettings);
                                                        commandResultHandler.SetResult(commandResult);
                                                    }
                                                    else
                                                    {
                                                        commandResultHandler.SetError(commandResult.Error);
                                                    }
                                                }
                                                else
                                                {
                                                    var notificationResult =
                                                        JsonConvert.DeserializeObject<NotificationResult>(entry,
                                                            Constants.DeviceSerializerSettings);

                                                    if (notificationResult?.Method == null) continue;
                                                    if (notificationResult.Params != null)
                                                    {
                                                        //save properties
                                                        foreach (var (key, value) in
                                                            notificationResult.Params)
                                                        {
                                                            this[key] = value;
                                                        }
                                                    }

                                                    //notification result
                                                    OnNotificationReceived?.Invoke(this,
                                                        new NotificationReceivedEventArgs(notificationResult));
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        OnError?.Invoke(this, new UnhandledExceptionEventArgs(ex, false));
                                    }
                                }
                            }
                        }
                    }

                    await Task.Delay(100);
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Get a thread-safe unique Id to pass to the API
        /// </summary>
        /// <returns></returns>
        private int GetUniqueIdForCommand()
        {
            return Interlocked.Increment(ref _uniqueId);
        }
    }
}