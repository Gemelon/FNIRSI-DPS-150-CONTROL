// MIT License
//
// Copyright (c) 2026 Gemelon
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

//
// Protocol Reference: https://github.com/cho45/fnirsi-dps-150/blob/main/docs/FNIRSI_DPS-150_Protocol.md
//

using System.IO.Ports;

namespace FNIRSI_DPS_150_CONTROL
{
    /// <summary>
    /// Communication class for serial communication with a FNIRSI-DPS-150 laboratory power supply.
    /// Handles connection management, data transmission, and continuous reception of status messages.
    /// </summary>
    public class DPS150Communication
    {
        private SerialPort? _serialPort;
        private bool _isReceiving;
        private readonly object _lockObject = new();
        private CancellationTokenSource? _receiveCts;
        private Task? _receiveTask;

        /// <summary>
        /// Event raised when data is received from the device.
        /// The device sends status messages approximately every 100-200ms.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs>? DataReceived;

        /// <summary>
        /// Gets a value indicating whether the serial port is currently connected and open.
        /// </summary>
        /// <summary>
        /// Gets a value indicating whether the serial port is currently connected and open.
        /// </summary>
        public bool IsConnected => _serialPort?.IsOpen ?? false;

        /// <summary>
        /// Establishes a serial connection to the FNIRSI-DPS-150 device.
        /// </summary>
        /// <param name="portInput">Either a port index number (1-based, e.g., "1", "2") or a port name (e.g., "COM3").</param>
        /// <returns>True if the connection was successfully established; otherwise, false.</returns>
        /// <remarks>
        /// This method provides flexible port selection:
        /// - If portInput is a number (e.g., "1", "2"), it automatically retrieves available ports and selects the corresponding port
        /// - If portInput is a port name (e.g., "COM3"), it connects directly to that port
        /// - If portInput is null/empty, returns false
        /// 
        /// Connection settings:
        /// - Baud Rate: 115200
        /// - Data Bits: 8
        /// - Parity: None
        /// - Stop Bits: One
        /// - Handshake: None
        /// - Read/Write Timeout: 1000ms
        /// 
        /// If a connection is already open, it will be closed before establishing a new connection.
        /// 
        /// Example usage:
        /// <code>
        /// var communication = new DPS150Communication();
        /// 
        /// // Connect using port index (first available port)
        /// communication.Connect("1");
        /// 
        /// // Or connect using explicit port name
        /// communication.Connect("COM5");
        /// </code>
        /// </remarks>
        public bool Connect(string portInput)
        {
            if (string.IsNullOrWhiteSpace(portInput))
            {
                return false;
            }

            try
            {
                if (_serialPort?.IsOpen == true)
                {
                    Disconnect();
                }

                string? portName = null;

                // Try to parse as port index (1-based)
                if (int.TryParse(portInput, out int portIndex) && portIndex > 0)
                {
                    string[] ports = GetAvailablePorts();
                    if (portIndex <= ports.Length)
                    {
                        portName = ports[portIndex - 1];
                    }
                    else
                    {
                        return false; // Port index out of range
                    }
                }
                // Otherwise treat as direct port name
                else
                {
                    portName = portInput;
                }

                if (string.IsNullOrWhiteSpace(portName))
                {
                    return false;
                }

                _serialPort = new SerialPort(portName)
                {
                    BaudRate = 115200,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                _serialPort.Open();
                return _serialPort.IsOpen;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Closes the serial connection and releases all resources.
        /// </summary>
        /// <remarks>
        /// This method first stops any ongoing receive operations, then closes and disposes
        /// the serial port. It is thread-safe and can be called multiple times.
        /// </remarks>
        public void Disconnect()
        {
            StopReceiving();

            lock (_lockObject)
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                }
                        _serialPort?.Dispose();
                        _serialPort = null;
                    }
                }

                /// <summary>
                /// Flushes the input and output buffers of the serial port.
                /// </summary>
                /// <returns>True if the buffers were successfully flushed; otherwise, false.</returns>
                /// <remarks>
                /// This method clears:
                /// - Input buffer: Discards any unread data received from the device
                /// - Output buffer: Discards any unsent data in the transmission queue
                /// 
                /// Useful for:
                /// - Clearing old/stale data before starting a new communication sequence
                /// - Recovering from communication errors
                /// - Ensuring a clean state before sending critical commands
                /// 
                /// This method is thread-safe and will only flush if the port is currently open.
                /// 
                /// Example usage:
                /// <code>
                /// // Clear any pending data before starting a session
                /// if (communication.Flush())
                /// {
                ///     Console.WriteLine("Buffers cleared successfully");
                /// }
                /// </code>
                /// </remarks>
                public bool Flush()
                {
                    lock (_lockObject)
                    {
                        if (_serialPort?.IsOpen != true)
                        {
                            return false;
                        }

                        try
                        {
                            _serialPort.DiscardInBuffer();
                            _serialPort.DiscardOutBuffer();
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }

                /// <summary>
                /// Retrieves a list of all available serial port names on the system.
                /// </summary>
                /// <returns>An array of strings containing the names of all available COM ports.</returns>
                /// <remarks>
                /// This is a static method and can be called without creating an instance of the class.
                /// Useful for populating a dropdown list or selection menu for port selection.
                /// </remarks>
                public static string[] GetAvailablePorts()
                {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// Sends raw byte data to the connected device.
        /// </summary>
        /// <param name="data">The byte array to send. If null or empty, the method returns false.</param>
        /// <returns>True if the data was successfully sent; otherwise, false.</returns>
        /// <remarks>
        /// This method is thread-safe and uses a lock to ensure data integrity.
        /// The serial port must be open before calling this method.
        /// </remarks>
        public bool SendData(byte[]? data = null)
        {
            if (data == null || data.Length == 0)
                return false;

            lock (_lockObject)
            {
                if (_serialPort?.IsOpen != true)
                    return false;

                try
                {
                    _serialPort.Write(data, 0, data.Length);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Reads response data from the device with a specified timeout.
        /// </summary>
        /// <param name="timeoutMs">The maximum time in milliseconds to wait for data. Default is 1000ms.</param>
        /// <returns>A byte array containing the received data, or null if no data was received or an error occurred.</returns>
        /// <remarks>
        /// This method continuously reads available bytes from the serial port buffer until either:
        /// - No more data is available for 10ms (indicating the end of a message)
        /// - The timeout period expires
        /// The method waits in 10ms intervals to accumulate a complete data packet.
        /// </remarks>
        public byte[]? ReadResponse(int timeoutMs = 1000)
        {
            if (_serialPort?.IsOpen != true)
                return null;

            try
            {
                var startTime = DateTime.Now;
                var buffer = new List<byte>();

                while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
                {
                    if (_serialPort.BytesToRead > 0)
                    {
                        var tempBuffer = new byte[_serialPort.BytesToRead];
                        int bytesRead = _serialPort.Read(tempBuffer, 0, tempBuffer.Length);
                        buffer.AddRange(tempBuffer.Take(bytesRead));

                        // Wait briefly to receive additional data that may be part of the same packet
                        Thread.Sleep(10);
                    }
                    else if (buffer.Count > 0)
                    {
                        // Data received and no more available - packet is complete
                        break;
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }

                return buffer.Count > 0 ? buffer.ToArray() : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Sends a command to the device and waits for a response.
        /// </summary>
        /// <param name="data">The byte array containing the command to send. If null or empty, null is returned.</param>
        /// <param name="timeoutMs">The maximum time in milliseconds to wait for a response. Default is 1000ms.</param>
        /// <returns>A byte array containing the device's response, or null if sending failed or no response was received.</returns>
        /// <remarks>
        /// This is a convenience method that combines SendData() and ReadResponse() in a thread-safe manner.
        /// The entire operation (send and receive) is locked to prevent interference from other threads.
        /// </remarks>
        public byte[]? SendDataAndGetResponse(byte[]? data = null, int timeoutMs = 1000)
        {
            lock (_lockObject)
            {
                if (_serialPort?.IsOpen != true)
                    return null;
                    
                _serialPort.DiscardOutBuffer(); // Clear any unsent data before sending a new command
                _serialPort.DiscardInBuffer(); // Clear any existing data before sending a new command

                if (!SendData(data))
                    return null;

                return ReadResponse(timeoutMs);
            }
        }

        /// <summary>
        /// Starts continuous reception of data from the device in a background task.
        /// </summary>
        /// <returns>True if the receive task was successfully started; false if the port is not open or already receiving.</returns>
        /// <remarks>
        /// This method creates an asynchronous background task that continuously monitors the serial port
        /// for incoming data. When data is received, it raises the DataReceived event.
        /// The FNIRSI-DPS-150 device sends status messages approximately every 100-200ms.
        /// Call StopReceiving() to stop the background task before disconnecting.
        /// </remarks>
        public bool StartReceiving()
        {
            if (_serialPort?.IsOpen != true)
                return false;

            if (_isReceiving)
                return true;

            _isReceiving = true;
            _receiveCts = new CancellationTokenSource();
            _receiveTask = Task.Run(() => ReceiveLoop(_receiveCts.Token));
            return true;
        }

        /// <summary>
        /// Stops the continuous reception of data and terminates the background receive task.
        /// </summary>
        /// <remarks>
        /// This method signals the background task to stop and waits up to 500ms for it to terminate.
        /// It safely handles cancellation exceptions and cleans up resources.
        /// This method should be called before calling Disconnect() to ensure a clean shutdown.
        /// If no receive task is running, this method has no effect.
        /// </remarks>
        public void StopReceiving()
        {
            if (!_isReceiving)
                return;

            _isReceiving = false;
            _receiveCts?.Cancel();

            try
            {
                _receiveTask?.Wait(500);
            }
            catch (AggregateException)
            {
                // Ignore cancellation exceptions during task termination
            }

            _receiveCts?.Dispose();
            _receiveCts = null;
            _receiveTask = null;
        }

        /// <summary>
        /// Background loop that continuously receives data from the serial port.
        /// </summary>
        /// <param name="cancellationToken">Token used to signal when the receive loop should terminate.</param>
        /// <returns>A task representing the asynchronous receive operation.</returns>
        /// <remarks>
        /// This method runs in a separate task and continuously monitors the serial port for incoming data.
        /// When data is detected:
        /// 1. Reads all available bytes from the buffer
        /// 2. Waits 50ms to ensure the complete packet has arrived
        /// 3. If no more data arrives, considers the packet complete and raises the DataReceived event
        /// The loop continues until the cancellation token is signaled or the serial port is closed.
        /// Any exceptions during reading are silently caught to maintain the receive loop.
        /// </remarks>
        private async Task ReceiveLoop(CancellationToken cancellationToken)
        {
            var buffer = new List<byte>();

            while (!cancellationToken.IsCancellationRequested && _serialPort?.IsOpen == true)
            {
                try
                {
                    var port = _serialPort;
                    if (port == null || !port.IsOpen)
                        break;
                        
                    if (port.BytesToRead > 0)
                    {
                        var tempBuffer = new byte[port.BytesToRead];
                        int bytesRead = port.Read(tempBuffer, 0, tempBuffer.Length);
                        buffer.AddRange(tempBuffer.Take(bytesRead));

                        // Wait briefly to collect a complete packet
                        await Task.Delay(50, cancellationToken);

                        // If no more data is coming, consider the packet complete
                        if (port.BytesToRead == 0 && buffer.Count > 0)
                        {
                            OnDataReceived(buffer.ToArray());
                            buffer.Clear();
                        }
                    }
                    else
                    {
                        // Wait for new data (50ms intervals, matching the device's 100-200ms transmission rate)
                        await Task.Delay(50, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Cancellation requested - exit the loop gracefully
                    break;
                }
                catch
                {
                    // Ignore read errors and continue the receive loop
                }
            }
        }

        /// <summary>
        /// Raises the DataReceived event when data is received from the device.
        /// </summary>
        /// <param name="data">The byte array containing the received data.</param>
        /// <remarks>
        /// This method is called by the receive loop when a complete data packet has been received.
        /// It safely invokes all registered event handlers on the thread pool.
        /// </remarks>
        protected virtual void OnDataReceived(byte[]? data)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs(data));
        }
    }

    /// <summary>
    /// Event arguments for data received from the FNIRSI-DPS-150 device.
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the received data as a byte array.
        /// </summary>
        /// <remarks>
        /// This property contains the raw bytes received from the device.
        /// The data typically represents status information sent by the device every 100-200ms.
        /// May be null if no data was received.
        /// </remarks>
        public byte[]? Data { get; }

        /// <summary>
        /// Initializes a new instance of the DataReceivedEventArgs class.
        /// </summary>
        /// <param name="data">The byte array containing the received data.</param>
        public DataReceivedEventArgs(byte[]? data)
        {
            Data = data;
        }
    }
}
