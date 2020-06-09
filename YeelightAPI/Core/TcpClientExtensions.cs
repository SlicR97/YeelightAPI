using System.Net.Sockets;

namespace YeelightAPI.Core
{
    /// <summary>
    /// Extensions for TcpClient
    /// </summary>
    public static class TcpClientExtensions
    {
        /// <summary>
        /// Returns a value to indicate if the Client is connected or not
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        public static bool IsConnected(this TcpClient tcpClient)
        {
            try
            {
                if (tcpClient?.Client?.Connected != true) return false;
                /* pear to the documentation on Poll:
                     * When passing SelectMode.SelectRead as a parameter to the Poll method it will return
                     * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                     * -or- true if data is available for reading;
                     * -or- true if the connection has been closed, reset, or terminated;
                     * otherwise, returns false
                     */

                // Detect if client disconnected
                if (!tcpClient.Client.Poll(0, SelectMode.SelectRead)) return true;
                var buff = new byte[1];
                return tcpClient.Client.Receive(buff, SocketFlags.Peek) != 0;

            }
            catch
            {
                return false;
            }
        }
    }
}