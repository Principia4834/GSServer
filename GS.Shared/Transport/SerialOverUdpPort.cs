/* Copyright(C) 2019-2026 Rob Morgan (robert.morgan.e@gmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace GS.Shared.Transport
{
    /// <summary>
    /// Provides serial-port-style communication over UDP.
    /// </summary>
    /// <remarks>Connects to a configured remote endpoint and uses ASCII encoding with
    /// carriage-return-terminated lines. Read operations wait up to the configured timeout.</remarks>
    public sealed class SerialOverUdpPort : UdpClient, ISerialPort
    {
        private readonly IPEndPoint _remoteEndpoint;
        private readonly object _sync = new object();
        private string _input = string.Empty;
        private int _receiveStarted;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialOverUdpPort"/> class.
        /// </summary>
        /// <param name="remoteEndpoint">The remote endpoint to communicate with.</param>
        /// <param name="readTimeout">The maximum time to wait for a read operation.</param>
        public SerialOverUdpPort(IPEndPoint remoteEndpoint, TimeSpan readTimeout)
        {
            Encoding = System.Text.Encoding.ASCII;
            _remoteEndpoint = remoteEndpoint;
            ReadTimeout = (int)readTimeout.TotalMilliseconds;
            NewLine = "\r";
        }
        public Encoding Encoding { get; }
        public int ReadTimeout { get; }
        public string NewLine { get; }
        public bool IsOpen => Client?.Connected == true;

        /// <summary>
        /// Clears the input buffer.
        /// </summary>
        public void DiscardInBuffer()
        {
            lock (_sync)
            {
                _input = string.Empty;
            }
        }
        /// <summary>
        /// Discards data from the output buffer.
        /// </summary>
        public void DiscardOutBuffer(){}
        public string ReadLine()
        {
            if (!IsOpen)
            {
                throw new InvalidOperationException("The UDP port is not open.");
            }

            var stopwatch = Stopwatch.StartNew();
            lock (_sync)
            {
                while (true)
                {
                    var terminatorIndex = _input.IndexOf(NewLine, StringComparison.Ordinal);
                    if (terminatorIndex >= 0)
                    {
                        var line = _input.Substring(0, terminatorIndex);
                        _input = _input.Substring(terminatorIndex + NewLine.Length);
                        return line;
                    }

                    var remainingTimeout = ReadTimeout == Timeout.Infinite
                        ? Timeout.Infinite
                        : ReadTimeout - (int)stopwatch.ElapsedMilliseconds;
                    if ((remainingTimeout <= 0 && ReadTimeout != Timeout.Infinite)
                        || !Monitor.Wait(_sync, remainingTimeout))
                    {
                        throw new TimeoutException("The UDP read operation timed out.");
                    }
                }
            }
        }
        /// <summary>
        /// Opens the connection to the remote endpoint and starts receiving data if reception has not already started.
        /// </summary>
        /// <remarks>Exceptions are logged and rethrown.</remarks>
        public void Open()
        {
            try
            {
                Connect(_remoteEndpoint);
                if (Interlocked.Exchange(ref _receiveStarted, 1) == 0)
                {
                    _ = BeginReceive(EndReceiveCb, null);
                }
            }
            catch (Exception ex)
            {
                var monitorItem = new MonitorEntry
                {
                    Type = MonitorType.Error,
                    Category = MonitorCategory.Other,
                    Datetime = DateTime.UtcNow,
                    Method = MethodBase.GetCurrentMethod()?.Name,
                    Thread = Thread.CurrentThread.ManagedThreadId,
                    Device = MonitorDevice.Server,
                    Message = $"{_remoteEndpoint}|{ex.Message}"
                };
                MonitorLog.LogToMonitor(monitorItem);
                throw;
            }
        }
        public string ReadExisting()
        {
            lock (_sync)
            {
                var received = _input;
                _input = string.Empty;
                return received;
            }
        }

        /// <summary>
        /// Sends the specified data to the remote endpoint.
        /// </summary>
        /// <remarks>Errors are logged and rethrown.</remarks>
        /// <param name="data">The data to send.</param>
        public void Write(string data)
        {
            try
            {
                var bytes = Encoding.GetBytes(data);
                Send(bytes, bytes.Length);
                // Debug.WriteLine("Cmd: " + Encoding.Default.GetString(bytes));
            }
            catch (Exception ex)
            {
                var monitorItem = new MonitorEntry
                {
                    Type = MonitorType.Error,
                    Category = MonitorCategory.Other,
                    Datetime = DateTime.UtcNow,
                    Method = MethodBase.GetCurrentMethod()?.Name,
                    Thread = Thread.CurrentThread.ManagedThreadId,
                    Device = MonitorDevice.Server,
                    Message = $"|{_remoteEndpoint}|Error|{ex.Message}"
                };
                MonitorLog.LogToMonitor(monitorItem);
                throw;
            }
        }

        /// <summary>
        /// Completes an asynchronous receive operation and processes the received data.
        /// </summary>
        /// <remarks>Received data is decoded and appended to the input buffer, then another receive is
        /// started. Unexpected errors are logged and close the client.</remarks>
        /// <param name="result">The result of the asynchronous receive operation.</param>
        private void EndReceiveCb(IAsyncResult result)
        {
            try
            {
                if (result == null){return;}
                if (!result.IsCompleted){return;}
                IPEndPoint ep = null;
                var bytes = EndReceive(result, ref ep);
                if (bytes?.Length > 0)
                {
                    lock (_sync)
                    {
                        _input += Encoding.GetString(bytes);
                        Monitor.PulseAll(_sync);
                    }
                }

                _ = BeginReceive(EndReceiveCb, null);
            }
            catch (ObjectDisposedException)
            {
                // the port was closed
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
                // the port was closed
            }
            catch (Exception ex)
            {
                var monitorItem = new MonitorEntry
                {
                    Type = MonitorType.Error,
                    Category = MonitorCategory.Other,
                    Datetime = DateTime.UtcNow,
                    Method = MethodBase.GetCurrentMethod()?.Name,
                    Thread = Thread.CurrentThread.ManagedThreadId,
                    Device = MonitorDevice.Server,
                    Message = $"SerialOverUdpPort|{_remoteEndpoint}|Receive|{ex.Message}"
                };
                MonitorLog.LogToMonitor(monitorItem);
                Client?.Close();
            }
        }
    }
}

