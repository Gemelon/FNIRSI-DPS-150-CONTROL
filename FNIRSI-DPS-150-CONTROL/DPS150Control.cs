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
// ACKNOWLEDGMENTS:
// ================
// This implementation is based on the protocol documentation from:
// https://github.com/cho45/fnirsi-dps-150
//
// Special thanks to cho45 for reverse-engineering and documenting the
// FNIRSI-DPS-150 communication protocol. The protocol specification can be found at:
// https://github.com/cho45/fnirsi-dps-150/blob/main/docs/FNIRSI_DPS-150_Protocol.md
//
// Original project: https://github.com/cho45/fnirsi-dps-150
// Author: cho45 (https://github.com/cho45)
//

using System;
using System.Collections.Generic;
using System.Text;

namespace FNIRSI_DPS_150_CONTROL
{
    /// <summary>
    /// High-level control class for the FNIRSI-DPS-150 laboratory power supply.
    /// Provides comprehensive device control, configuration, and monitoring capabilities.
    /// </summary>
    /// <remarks>
    /// This class encapsulates all device operations including:
    /// - Connection management and session control
    /// - Output voltage and current control
    /// - Protection settings (OVP, OCP, OPP, OTP, LVP)
    /// - Preset memory management (M1-M6)
    /// - UI settings (brightness, volume)
    /// - Telemetry and status monitoring
    /// - Packet construction and validation
    /// </remarks>
    public class DPS150Control
    {
        #region Private Fields

        /// <summary>
        /// Low-level serial communication handler for the DPS-150 device.
        /// Manages the physical serial port connection and data transmission.
        /// </summary>
        private DPS150Communication _communication = new DPS150Communication();

        /// <summary>
        /// Register map and device configuration storage.
        /// Contains the protocol register definitions and device state information.
        /// </summary>
        private DPS150Registers _registers = new DPS150Registers();

        /// <summary>
        /// Tracks whether a communication session has been established with the device.
        /// The DPS-150 requires an active session before most register operations.
        /// Session is started with StartSession() and stopped with StopSession().
        /// </summary>
        private bool _sessionStarted = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets an array of available serial port names on the system.
        /// </summary>
        /// <remarks>
        /// This property provides a convenient way to enumerate COM ports
        /// without creating a DPS150Communication instance.
        /// Returns an empty array if no ports are available.
        /// </remarks>
        public string[] AvailablePorts { get => DPS150Communication.GetAvailablePorts() ?? new string[0]; }

        /// <summary>
        /// Gets a value indicating whether the serial port connection is currently open.
        /// </summary>
        /// <remarks>
        /// This property reflects the underlying serial port state.
        /// A connection can be established using ConnectToDevice() methods.
        /// Use this property to verify connection status before sending commands.
        /// </remarks>
        public bool IsConnected { get => _communication.IsConnected; }

        /// <summary>
        /// Gets a value indicating whether a communication session is currently active.
        /// </summary>
        /// <remarks>
        /// The DPS-150 requires an active session before accessing most registers.
        /// A session is started with StartSession() and stopped with StopSession().
        /// Many control methods (e.g., SetVoltage, SetCurrent) automatically start
        /// a session if one is not already active.
        /// 
        /// Session lifecycle:
        /// 1. Connect to device (IsConnected = true)
        /// 2. Start session (IsSessionStarted = true)
        /// 3. Perform device operations
        /// 4. Stop session (IsSessionStarted = false)
        /// 5. Disconnect (IsConnected = false)
        /// </remarks>
        public bool IsSessionStarted { get => _sessionStarted; }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Converts a float to four bytes (little-endian, IEEE-754 float32).
        /// </summary>
        /// <param name="value">The float value to convert.</param>
        /// <returns>A 4-byte array in little-endian byte order.</returns>
        /// <remarks>
        /// The DPS-150 protocol uses IEEE-754 single-precision (32-bit) floating-point format
        /// in LITTLE-ENDIAN byte order. BitConverter.GetBytes() on Windows (and most platforms)
        /// already returns little-endian bytes, so no byte reversal is needed.
        /// 
        /// Example: 12.3f → CD CC 44 41 (little-endian)
        /// </remarks>
        private byte[] FloatToBytes(float value)
        {
            // DPS-150 uses little-endian byte order for IEEE-754 float32
            // BitConverter.GetBytes returns native byte order (little-endian on Windows)
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Converts four bytes to a float (little-endian, IEEE-754 float32).
        /// </summary>
        /// <param name="data">The byte array containing the float bytes.</param>
        /// <param name="offset">The offset in the array where the float bytes start.</param>
        /// <returns>The decoded float value.</returns>
        /// <remarks>
        /// The DPS-150 protocol uses IEEE-754 single-precision (32-bit) floating-point format
        /// in LITTLE-ENDIAN byte order. BitConverter.ToSingle() on Windows (and most platforms)
        /// expects little-endian bytes, so no byte reversal is needed.
        /// 
        /// Example: CD CC 44 41 (little-endian) → 12.3f
        /// </remarks>
        private float BytesToFloat(byte[] data, int offset = 0)
        {
            if (data == null || data.Length < offset + 4)
            {
                throw new ArgumentException("Insufficient data for float conversion");
            }

            // DPS-150 uses little-endian byte order for IEEE-754 float32
            // BitConverter.ToSingle expects native byte order (little-endian on Windows)
            return BitConverter.ToSingle(data, offset);
        }

        /// <summary>
        /// Creates a complete DPS-150 protocol packet with header, access type, register, data, and checksum.
        /// </summary>
        /// <param name="direction">Communication direction (TX=0xF1 or RX=0xF0).</param>
        /// <param name="accessType">Access type (Read=0xA1, Write=0xB1, Baud=0xB0, Control=0xC1).</param>
        /// <param name="register">Register address to access.</param>
        /// <param name="data">Payload data bytes (can be null for read operations).</param>
        /// <returns>A complete packet with checksum, ready to send.</returns>
        /// <remarks>
        /// This method automatically calculates the correct checksum for the packet.
        /// 
        /// Packet structure:
        /// - Byte 0: Direction header (0xF1 for TX, 0xF0 for RX)
        /// - Byte 1: Access type (0xA1, 0xB1, 0xB0, or 0xC1)
        /// - Byte 2: Register address
        /// - Byte 3: Data length (LEN)
        /// - Bytes 4..n-1: Payload data (if any)
        /// - Byte n: Checksum (calculated from bytes 2 to n-1)
        /// 
        /// The checksum is calculated as: CHK = sum(DATA[2..n]) & 0xFF
        /// </remarks>
        /// <example>
        /// <code>
        /// // Create a voltage set command (12.3V)
        /// byte[] voltageData = FloatToBytes(12.3f);
        /// byte[] packet = CreatePacket(
        ///     DPS150ComDirection.TX, 
        ///     DPS150AccessType.Write, 
        ///     DPS150RegisterAddress.VoltageSetpoint, 
        ///     voltageData
        /// );
        /// // Result: F1 B1 C1 04 [float bytes] [checksum]
        /// </code>
        /// </example>
        private byte[] CreatePacket(DPS150ComDirection direction, DPS150AccessType accessType, DPS150RegisterAddress register, byte[] data)
        {
            // Calculate data length
            byte dataLength = (byte)(data?.Length ?? 0);

            // Build packet without checksum
            List<byte> packet = new List<byte>();

            // Add header bytes
            packet.Add((byte)direction);     // Byte 0: Direction (0xF1 or 0xF0)
            packet.Add((byte)accessType);    // Byte 1: Access type (0xA1, 0xB1, 0xB0, 0xC1)
            packet.Add((byte)register);      // Byte 2: Register address
            packet.Add(dataLength);          // Byte 3: Data length

            // Add payload data if present
            if (data != null && data.Length > 0)
            {
                packet.AddRange(data);       // Bytes 4..n-1: Payload
            }

            // Calculate checksum (sum of bytes from index 2 onwards)
            byte checksum = CalculateChecksum(packet.ToArray());

            // Add checksum as last byte
            packet.Add(checksum);            // Byte n: Checksum

            return packet.ToArray();
        }

        /// <summary>
        /// Calculates the checksum for a DPS-150 protocol packet.
        /// </summary>
        /// <param name="data">The complete packet data starting from header bytes.</param>
        /// <returns>The calculated checksum byte (CHK = sum(DATA[2..n]) & 0xFF).</returns>
        /// <remarks>
        /// The checksum is calculated according to the FNIRSI-DPS-150 protocol:
        /// CHK = sum(DATA[2..n]) & 0xFF
        /// 
        /// The first two bytes (DATA[0] and DATA[1]) are header bytes and are NOT included in the checksum.
        /// The checksum calculation starts from DATA[2] onwards.
        /// 
        /// Minimum data length: 4 bytes (Header + Header + REG + LEN minimum)
        /// 
        /// Example:
        /// For packet F1 B1 C1 04 00 00 B0 40 B5:
        /// - DATA[0] = 0xF1 (Header - NOT in checksum)
        /// - DATA[1] = 0xB1 (Access Type - NOT in checksum)
        /// - DATA[2] = 0xC1 (REG - included in checksum)
        /// - DATA[3] = 0x04 (LEN - included in checksum)
        /// - DATA[4..7] = [0x00, 0x00, 0xB0, 0x40] (Payload - included in checksum)
        /// - CHK = (0xC1 + 0x04 + 0x00 + 0x00 + 0xB0 + 0x40) & 0xFF = 0xB5
        /// </remarks>
        public static byte CalculateChecksum(byte[] data)
        {
            if (data == null || data.Length < 4)
            {
                throw new ArgumentException("Data must contain at least 4 bytes (Header + Header + REG + LEN).", nameof(data));
            }

            int sum = 0;

            // Sum all bytes starting from index 2 (skip DATA[0] and DATA[1] which are header bytes)
            for (int i = 2; i < data.Length; i++)
            {
                sum += data[i];
            }

            return (byte)(sum & 0xFF);
        }

        /// <summary>
        /// Verifies if a packet has a valid checksum.
        /// </summary>
        /// <param name="packet">The complete packet including header and checksum as the last byte.</param>
        /// <returns>True if the checksum is valid; otherwise, false.</returns>
        /// <remarks>
        /// The packet should include:
        /// - DATA[0]: Header (0xF0 or 0xF1)
        /// - DATA[1]: Access type (0xA1, 0xB0, 0xB1, or 0xC1)
        /// - DATA[2]: REG (register address)
        /// - DATA[3]: LEN (payload length)
        /// - DATA[4..n-1]: Payload (LEN bytes)
        /// - DATA[n]: CHK (checksum byte, last byte)
        /// 
        /// The checksum is calculated from DATA[2] to DATA[n-1] (excluding header bytes and checksum).
        /// CHK = sum(DATA[2..n-1]) & 0xFF
        /// </remarks>
        public static bool VerifyChecksum(byte[] packet)
        {
            if (packet == null || packet.Length < 5)
            {
                return false;
            }

            // Last byte is the checksum
            byte receivedChecksum = packet[packet.Length - 1];

            // Extract data portion (from index 2 to second-to-last byte)
            // This includes REG, LEN, and payload, but excludes the two header bytes and the checksum
            int dataLength = packet.Length - 3; // Exclude 2 header bytes and 1 checksum byte
            byte[] data = new byte[dataLength];
            Array.Copy(packet, 2, data, 0, dataLength);

            // Calculate checksum by summing all extracted bytes
            int sum = 0;
            foreach (byte b in data)
            {
                sum += b;
            }
            byte calculatedChecksum = (byte)(sum & 0xFF);

            return receivedChecksum == calculatedChecksum;
        }

        /// <summary>
        /// Connects to the DPS-150 device on the specified serial port.
        /// </summary>
        /// <param name="portName">The name of the COM port to connect to (e.g., "COM3").</param>
        /// <returns>True if the connection was successfully established; otherwise, false.</returns>
        /// <remarks>
        /// This method establishes a serial connection with the following parameters:
        /// - Baud Rate: 115200
        /// - Data Bits: 8
        /// - Parity: None
        /// - Stop Bits: One
        /// - Handshake: None
        /// 
        /// If the connection is successful, the device is ready to receive commands.
        /// Use IsConnected property to check the connection status.
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// if (control.ConnectToDevice("COM3"))
        /// {
        ///     Console.WriteLine("Connected successfully!");
        /// }
        /// </code>
        /// </example>
        public bool ConnectToDevice(string portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
            {
                return false;
            }

            try
            {
                return _communication.Connect(portName);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Connects to the DPS-150 device using a port index or custom port name.
        /// </summary>
        /// <param name="portInput">Either a port index number (1-based) or a custom port name (e.g., "COM3").</param>
        /// <returns>True if the connection was successfully established; otherwise, false.</returns>
        /// <remarks>
        /// This method provides flexible port selection:
        /// - If portInput is a number (e.g., "1", "2"), it selects the corresponding port from available ports
        /// - If portInput is a port name (e.g., "COM3"), it connects directly to that port
        /// 
        /// The method automatically retrieves the list of available ports when a numeric index is provided.
        /// 
        /// Connection parameters:
        /// - Baud Rate: 115200
        /// - Data Bits: 8
        /// - Parity: None
        /// - Stop Bits: One
        /// - Handshake: None
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// 
        /// // Connect using port index (selects first available port)
        /// if (control.ConnectToDevice("1"))
        /// {
        ///     Console.WriteLine("Connected to first available port!");
        /// }
        /// 
        /// // Or connect using explicit port name
        /// if (control.ConnectToDevice("COM5"))
        /// {
        ///     Console.WriteLine("Connected to COM5!");
        /// }
        /// </code>
        /// </example>
        public bool ConnectToDevice(int portNumber)
        {
            if (portNumber <= 0)
            {
                return false;
            }

            try
            {
                return _communication.Connect(portNumber.ToString());
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the DPS-150 device and closes the serial port connection.
        /// </summary>
        /// <remarks>
        /// This method performs a clean shutdown:
        /// 1. Stops any active session (calls StopSession if needed)
        /// 2. Stops any ongoing data reception
        /// 3. Closes the serial port connection
        /// 4. Releases all communication resources
        /// 
        /// It is safe to call this method even if no connection is active.
        /// After disconnection, you can reconnect using ConnectToDevice().
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// // ... perform operations ...
        /// control.DisconnectFromDevice();
        /// </code>
        /// </example>
        public void DisconnectFromDevice()
        {
            try
            {
                // Stop session if active
                if (_sessionStarted)
                {
                    StopSession();
                }

                _communication.Disconnect();
                _sessionStarted = false;
            }
            catch
            {
                // Ignore exceptions during disconnect
                _sessionStarted = false;
            }
        }

        /// <summary>
        /// Flushes the input and output buffers of the serial communication.
        /// </summary>
        /// <returns>True if the buffers were successfully flushed; otherwise, false.</returns>
        /// <remarks>
        /// This method clears all pending data in the serial communication buffers:
        /// - Input buffer: Discards any unread data received from the device
        /// - Output buffer: Discards any unsent data waiting to be transmitted
        /// 
        /// Use cases:
        /// - Clear stale data before starting a new communication sequence
        /// - Recover from communication errors or timeouts
        /// - Ensure a clean state before sending critical commands (e.g., before StartSession)
        /// - Reset communication state after protocol violations
        /// 
        /// This method requires an active connection (IsConnected must be true).
        /// It is thread-safe and uses the underlying communication layer's locking mechanism.
        /// 
        /// Best practices:
        /// - Call Flush() before StartSession() to ensure clean session startup
        /// - Call Flush() after recovering from communication errors
        /// - Call Flush() before sending commands that require precise timing
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// 
        /// // Clear any stale data before starting a session
        /// if (control.FlushBuffers())
        /// {
        ///     control.StartSession();
        /// }
        /// </code>
        /// </example>
        public bool FlushBuffers()
        {
            if (!IsConnected)
            {
                return false;
            }

            return _communication.Flush();
        }

        /// <summary>
        /// Sends raw byte data to the connected DPS-150 device.
        /// </summary>
        /// <param name="data">The byte array to send. If null or empty, the method returns false.</param>
        /// <returns>True if the data was successfully sent; otherwise, false.</returns>
        /// <remarks>
        /// This method is a wrapper around the underlying communication layer's SendData method.
        /// The device must be connected before calling this method (check IsConnected property).
        /// 
        /// For protocol-compliant packets, ensure the data includes:
        /// - Header byte (0xF0 or 0xF1)
        /// - Access type byte (0xA1, 0xB0, 0xB1, or 0xC1)
        /// - Register address (REG)
        /// - Data length (LEN)
        /// - Payload data
        /// - Checksum byte (calculated using CalculateChecksum method)
        /// 
        /// Thread-safe: This method uses internal locking in the communication layer.
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// 
        /// // Example: Send a voltage set command
        /// byte[] packet = new byte[] { 0xF1, 0xB1, 0xC1, 0x04, 0x00, 0x00, 0xB0, 0x40 };
        /// byte checksum = DPS150Control.CalculateChecksum(packet);
        /// byte[] completePacket = packet.Append(checksum).ToArray();
        /// 
        /// if (control.SendData(completePacket))
        /// {
        ///     Console.WriteLine("Data sent successfully!");
        /// }
        /// </code>
        /// </example>
        public bool SendData(byte[]? data = null)
        {
            if (data == null || data.Length == 0)
            {
                return false;
            }

            if (!IsConnected)
            {
                return false;
            }

            try
            {
                return _communication.SendData(data);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reads response data from the connected DPS-150 device.
        /// </summary>
        /// <param name="timeoutMs">The maximum time in milliseconds to wait for data. Default is 1000ms.</param>
        /// <returns>A byte array containing the received data, or null if no data was received or an error occurred.</returns>
        /// <remarks>
        /// This method is a wrapper around the underlying communication layer's ReadResponse method.
        /// The device must be connected before calling this method (check IsConnected property).
        /// 
        /// The method continuously reads available bytes from the serial port buffer until either:
        /// - No more data is available for 10ms (indicating the end of a message)
        /// - The timeout period expires
        /// 
        /// For DPS-150 protocol packets, the response typically includes:
        /// - Header byte (0xF0 or 0xF1)
        /// - Access type byte (0xA1, 0xB0, 0xB1, or 0xC1)
        /// - Register address (REG)
        /// - Data length (LEN)
        /// - Payload data
        /// - Checksum byte
        /// 
        /// Use VerifyChecksum() to validate the received packet.
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// 
        /// // Send a command and read the response
        /// byte[] command = new byte[] { 0xF1, 0xA1, 0xC1, 0x00, 0xC1 };
        /// control.SendData(command);
        /// 
        /// byte[]? response = control.ReadResponse(2000); // 2 second timeout
        /// if (response != null)
        /// {
        ///     Console.WriteLine($"Received {response.Length} bytes");
        ///     if (DPS150Control.VerifyChecksum(response))
        ///     {
        ///         Console.WriteLine("Checksum valid!");
        ///     }
        /// }
        /// </code>
        /// </example>
        public byte[]? ReadResponse(int timeoutMs = 1000)
        {
            if (!IsConnected)
            {
                return null;
            }

            try
            {
                return _communication.ReadResponse(timeoutMs);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Sends a command to the DPS-150 device and waits for a response.
        /// </summary>
        /// <param name="data">The byte array containing the command to send. If null or empty, null is returned.</param>
        /// <param name="timeoutMs">The maximum time in milliseconds to wait for a response. Default is 1000ms.</param>
        /// <returns>A byte array containing the device's response, or null if sending failed or no response was received.</returns>
        /// <remarks>
        /// This is a convenience method that combines SendData() and ReadResponse() in a single operation.
        /// The device must be connected before calling this method (check IsConnected property).
        /// 
        /// The method performs the following steps:
        /// 1. Validates the connection status
        /// 2. Sends the command data to the device
        /// 3. Waits for and reads the response with the specified timeout
        /// 
        /// The operation is thread-safe due to locking in the underlying communication layer.
        /// 
        /// For DPS-150 protocol packets, ensure the command includes:
        /// - Header byte (0xF0 or 0xF1)
        /// - Access type byte (0xA1 for read, 0xB0/0xB1 for write)
        /// - Register address (REG)
        /// - Data length (LEN)
        /// - Payload data (if applicable)
        /// - Checksum byte (calculated using CalculateChecksum method)
        /// 
        /// Use VerifyChecksum() to validate the received response packet.
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// 
        /// // Example: Read voltage setting (register 0xC1)
        /// byte[] readCommand = new byte[] { 0xF1, 0xA1, 0xC1, 0x00 };
        /// byte checksum = DPS150Control.CalculateChecksum(readCommand);
        /// 
        /// List&lt;byte&gt; completeCommand = new List&lt;byte&gt;(readCommand);
        /// completeCommand.Add(checksum);
        /// 
        /// byte[]? response = control.SendCommandAndGetResponse(completeCommand.ToArray(), 2000);
        /// 
        /// if (response != null &amp;&amp; DPS150Control.VerifyChecksum(response))
        /// {
        ///     Console.WriteLine($"Received valid response: {BitConverter.ToString(response)}");
        ///     // Parse response data...
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Failed to receive valid response");
        /// }
        /// </code>
        /// </example>
        public byte[]? SendCommandAndGetResponse(byte[]? data = null, int timeoutMs = 1000)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }

            if (!IsConnected)
            {
                return null;
            }

            try
            {
                return _communication.SendCommandAndGetResponse(data, timeoutMs);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Starts a communication session with the DPS-150 device.
        /// </summary>
        /// <returns>True if the session start command was successfully sent; otherwise, false.</returns>
        /// <remarks>
        /// This method sends a session enable command to the device. Starting a session is typically
        /// required before certain operations or to enable specific communication modes.
        /// 
        /// The command packet sent is: F1 C1 00 01 01 02
        /// - 0xF1: Header byte
        /// - 0xC1: Access type (Write)
        /// - 0x00: Register address
        /// - 0x01: Data length
        /// - 0x01: Enable session (payload)
        /// - 0x02: Checksum
        /// 
        /// The device must be connected before calling this method (check IsConnected property).
        /// This method does not wait for a response from the device.
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// 
        /// if (control.StartSession())
        /// {
        ///     Console.WriteLine("Session started successfully!");
        ///     // Perform device operations...
        ///     control.StopSession();
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Failed to start session");
        /// }
        /// </code>
        /// </example>
        public bool StartSession()
        {
            if (!IsConnected)
            {
                return false;
            }

            if (_sessionStarted)
            {
                return true; // Session already started
            }

            byte[] data = new byte[] { 0xF1, 0xC1, 0x00, 0x01, 0x01, 0x02 }; // Enable session command
            bool success = SendData(data);

            if (success)
            {
                _sessionStarted = true;
            }

            return success;
        }

        /// <summary>
        /// Stops the communication session with the DPS-150 device.
        /// </summary>
        /// <returns>True if the session stop command was successfully sent; otherwise, false.</returns>
        /// <remarks>
        /// This method sends a session disable command to the device. Stopping a session is typically
        /// performed after completing device operations to properly close the communication mode.
        /// 
        /// The command packet sent is: F1 C1 00 01 00 02
        /// - 0xF1: Header byte
        /// - 0xC1: Access type (Write)
        /// - 0x00: Register address
        /// - 0x01: Data length
        /// - 0x00: Disable session (payload)
        /// - 0x02: Checksum
        /// 
        /// The device must be connected before calling this method (check IsConnected property).
        /// This method does not wait for a response from the device.
        /// 
        /// It is good practice to call StopSession() before disconnecting from the device.
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// 
        /// if (control.StartSession())
        /// {
        ///     // Perform device operations...
        ///     Console.WriteLine("Performing operations...");
        ///     
        ///     // Always stop the session when done
        ///     if (control.StopSession())
        ///     {
        ///         Console.WriteLine("Session stopped successfully!");
        ///     }
        /// }
        /// 
        /// control.DisconnectFromDevice();
        /// </code>
        /// </example>
        public bool StopSession()
        {
            if (!IsConnected)
            {
                return false;
            }

            byte[] data = new byte[] { 0xF1, 0xC1, 0x00, 0x01, 0x00, 0x02 }; // Disable session command
            bool success = SendData(data);

            if (success)
            {
                _sessionStarted = false;
            }

            return success;
        }

        /// <summary>
        /// Sets the output relay state of the DPS-150 device (ON/OFF).
        /// </summary>
        /// <param name="state">The desired relay state (ON or OFF).</param>
        /// <returns>True if the command was successfully sent; otherwise, false.</returns>
        /// <remarks>
        /// This method controls the main output relay of the power supply, effectively
        /// turning the output ON or OFF. This is equivalent to pressing the RUN/STOP button
        /// on the device front panel.
        /// 
        /// IMPORTANT: This method automatically starts a session if one is not already active.
        /// The session is required for the relay control command to work properly.
        /// 
        /// Command packets:
        /// - ON:  F1 B1 DB 01 01 DD
        /// - OFF: F1 B1 DB 01 00 DC
        /// 
        /// Packet structure:
        /// - 0xF1: Header byte
        /// - 0xB1: Access type (Write)
        /// - 0xDB: Register address (OutputRelayState)
        /// - 0x01: Data length (1 byte)
        /// - 0x01/0x00: Payload (1 = ON, 0 = OFF)
        /// - 0xDD/0xDC: Checksum
        /// 
        /// The device must be connected before calling this method (check IsConnected property).
        /// This method does not wait for a response from the device.
        /// 
        /// When turning ON:
        /// - The output voltage and current will be set according to the current setpoints
        /// - Protection features (OVP, OCP, OPP, etc.) remain active
        /// 
        /// When turning OFF:
        /// - The output is immediately disabled
        /// - Settings and setpoints are preserved
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// 
        /// // Session is automatically started if needed
        /// // Turn output ON
        /// if (control.SetOutputRelay(OutputRelayState.ON))
        /// {
        ///     Console.WriteLine("Output enabled - Power supply is now ON");
        /// }
        /// 
        /// // Wait for some operations...
        /// System.Threading.Thread.Sleep(5000);
        /// 
        /// // Turn output OFF
        /// if (control.SetOutputRelay(OutputRelayState.OFF))
        /// {
        ///     Console.WriteLine("Output disabled - Power supply is now OFF");
        /// }
        /// 
        /// control.DisconnectFromDevice(); // Automatically stops session
        /// </code>
        /// </example>
        public bool SetOutputRelay(OutputRelayState state)
        {
            if (!IsConnected)
            {
                return false;
            }

            // Ensure a session is started before sending relay command
            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return false; // Failed to start session
                }

                // Give device time to process session start command
                System.Threading.Thread.Sleep(50);
            }

            byte[] data;

            // Build command packet based on desired state
            if (state == OutputRelayState.ON)
            {
                // Command: F1 B1 DB 01 01 DD (Turn output ON)
                data = new byte[] { 0xF1, 0xB1, 0xDB, 0x01, 0x01, 0xDD };
            }
            else // OutputRelayState.OFF
            {
                // Command: F1 B1 DB 01 00 DC (Turn output OFF)
                data = new byte[] { 0xF1, 0xB1, 0xDB, 0x01, 0x00, 0xDC };
            }

            try
            {
                return SendData(data);
            }
            catch
            {
                return false;
            }
        }



        //public bool SetBaudRate(int baudRate) { }

        /// <summary>
        /// Sets the voltage setpoint of the power supply.
        /// </summary>
        /// <param name="voltage">The target voltage in volts (typically 0-150V for DPS-150).</param>
        /// <returns>True if the command was sent successfully; otherwise, false.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// The voltage setpoint is stored in the device but will only be applied to the output
        /// when the output relay is enabled (using SetOutputRelay(OutputRelayState.ON)).
        /// 
        /// Protocol details:
        /// - Command: F1 B1 C1 04 [float bytes] [checksum]
        /// - Register: 0xC1 (VoltageSetpoint)
        /// - Float format: IEEE-754 float32, little-endian
        /// - Checksum: Automatically calculated
        /// 
        /// Important notes:
        /// - The device must be connected before calling this method
        /// - A session is automatically started if needed
        /// - Setting voltage does not automatically enable the output
        /// - Use SetOutputRelay(ON) to apply the voltage to the output terminals
        /// - Consider device safety limits and connected load ratings
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// 
        /// // Set voltage to 12.5V (session auto-starts if needed)
        /// if (control.SetVoltage(12.5f))
        /// {
        ///     Console.WriteLine("Voltage setpoint set to 12.5V");
        ///     
        ///     // Enable output to apply the voltage
        ///     control.SetOutputRelay(OutputRelayState.ON);
        /// }
        /// 
        /// control.DisconnectFromDevice();
        /// </code>
        /// </example>
        public bool SetVoltage(float voltage)
        {
            if (!IsConnected)
            {
                return false;
            }

            // Ensure a session is started before sending voltage command
            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return false; // Failed to start session
                }

                // Give device time to process session start command
                System.Threading.Thread.Sleep(50);
            }

            byte[] data = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, DPS150RegisterAddress.VoltageSetpoint, FloatToBytes(voltage));

            try
            {
                return SendData(data);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the current limit of the power supply.
        /// </summary>
        /// <param name="current">The target current limit in amperes (typically 0-15A for DPS-150).</param>
        /// <returns>True if the command was sent successfully; otherwise, false.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// The current limit is stored in the device but will only be applied to the output
        /// when the output relay is enabled (using SetOutputRelay(OutputRelayState.ON)).
        /// 
        /// Protocol details:
        /// - Command: F1 B1 C2 04 [float bytes] [checksum]
        /// - Register: 0xC2 (CurrentLimit)
        /// - Float format: IEEE-754 float32, little-endian
        /// - Checksum: Automatically calculated
        /// 
        /// Important notes:
        /// - The device must be connected before calling this method
        /// - A session is automatically started if needed
        /// - Setting current limit does not automatically enable the output
        /// - Use SetOutputRelay(ON) to apply the current limit to the output
        /// - The current limit acts as a protection and operational limit
        /// - In CC (Constant Current) mode, this is the regulated current
        /// - In CV (Constant Voltage) mode, this is the maximum allowed current
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// 
        /// // Set current limit to 2.5A (session auto-starts if needed)
        /// if (control.SetCurrent(2.5f))
        /// {
        ///     Console.WriteLine("Current limit set to 2.5A");
        ///     
        ///     // Set voltage and enable output
        ///     control.SetVoltage(12.0f);
        ///     control.SetOutputRelay(OutputRelayState.ON);
        /// }
        /// 
        /// control.DisconnectFromDevice();
        /// </code>
        /// </example>
        public bool SetCurrent(float current)
        {
            if (!IsConnected)
            {
                return false;
            }

            // Ensure a session is started before sending current limit command
            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return false; // Failed to start session
                }

                // Give device time to process session start command
                System.Threading.Thread.Sleep(50);
            }

            byte[] data = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, DPS150RegisterAddress.CurrentLimit, FloatToBytes(current));

            try
            {
                return SendData(data);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the current voltage setpoint from the device.
        /// </summary>
        /// <returns>The voltage setpoint in volts, or -1.0f if the read operation failed.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// It sends a read command to the device and waits for the response.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 C1 00 [checksum] (Read request)
        /// - Response: F0 A1 C1 04 [float bytes] [checksum]
        /// - Register: 0xC1 (VoltageSetpoint)
        /// - Float format: IEEE-754 float32, little-endian
        /// - Checksum: Automatically calculated and verified
        /// 
        /// Important notes:
        /// - The device must be connected before calling this method
        /// - A session is automatically started if needed
        /// - This reads the voltage SETPOINT, not the actual measured output voltage
        /// - A timeout of 1000ms is used for waiting for the device response
        /// - Returns -1.0f on communication errors or invalid responses
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// 
        /// // Read the current voltage setpoint
        /// float voltage = control.GetVoltage();
        /// if (voltage >= 0)
        /// {
        ///     Console.WriteLine($"Current voltage setpoint: {voltage:F2}V");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Failed to read voltage");
        /// }
        /// 
        /// control.DisconnectFromDevice();
        /// </code>
        /// </example>
        public float GetVoltage()
        {
            if (!IsConnected)
            {
                return -1.0f;
            }

            // Ensure a session is started before sending read command
            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1.0f; // Failed to start session
                }

                // Give device time to process session start command
                System.Threading.Thread.Sleep(50);
            }

            // Create read command packet (no data payload for read)
            byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.VoltageSetpoint, null);

            try
            {
                // Send read command and wait for response
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    // Invalid response (minimum: F0 A1 C1 04 [4 float bytes] [checksum] = 9 bytes)
                    return -1.0f;
                }

                // Verify response packet structure
                if (response[0] != (byte)DPS150ComDirection.RX || // Should be 0xF0 (device response)
                    response[1] != (byte)DPS150AccessType.Read || // Should be 0xA1 (Read)
                    response[2] != (byte)DPS150RegisterAddress.VoltageSetpoint) // Should be 0xC1
                {
                    return -1.0f; // Invalid response header
                }

                // Verify checksum
                if (!VerifyChecksum(response))
                {
                    return -1.0f; // Checksum mismatch
                }

                // Extract float value (bytes 4-7)
                float voltage = BytesToFloat(response, 4);
                return voltage;
            }
            catch
            {
                return -1.0f;
            }
        }

        /// <summary>
        /// Gets the current limit setpoint from the device.
        /// </summary>
        /// <returns>The current limit in amperes, or -1.0f if the read operation failed.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// It sends a read command to the device and waits for the response.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 C2 00 [checksum] (Read request)
        /// - Response: F0 A1 C2 04 [float bytes] [checksum]
        /// - Register: 0xC2 (CurrentLimit)
        /// - Float format: IEEE-754 float32, little-endian
        /// - Checksum: Automatically calculated and verified
        /// 
        /// Important notes:
        /// - The device must be connected before calling this method
        /// - A session is automatically started if needed
        /// - This reads the current limit SETPOINT, not the actual measured output current
        /// - A timeout of 1000ms is used for waiting for the device response
        /// - Returns -1.0f on communication errors or invalid responses
        /// - In CC mode, this is the regulated current value
        /// - In CV mode, this is the maximum allowed current (protection limit)
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// 
        /// // Read the current limit setpoint
        /// float current = control.GetCurrent();
        /// if (current >= 0)
        /// {
        ///     Console.WriteLine($"Current limit setpoint: {current:F2}A");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Failed to read current limit");
        /// }
        /// 
        /// control.DisconnectFromDevice();
        /// </code>
        /// </example>
        public float GetCurrent()
        {
            if (!IsConnected)
            {
                return -1.0f;
            }

            // Ensure a session is started before sending read command
            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1.0f; // Failed to start session
                }

                // Give device time to process session start command
                System.Threading.Thread.Sleep(50);
            }

            // Create read command packet (no data payload for read)
            byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.CurrentLimit, null);

            try
            {
                // Send read command and wait for response
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    // Invalid response (minimum: F0 A1 C2 04 [4 float bytes] [checksum] = 9 bytes)
                    return -1.0f;
                }

                // Verify response packet structure
                if (response[0] != (byte)DPS150ComDirection.RX || // Should be 0xF0 (device response)
                    response[1] != (byte)DPS150AccessType.Read || // Should be 0xA1 (Read)
                    response[2] != (byte)DPS150RegisterAddress.CurrentLimit) // Should be 0xC2
                {
                    return -1.0f; // Invalid response header
                }

                // Verify checksum
                if (!VerifyChecksum(response))
                {
                    return -1.0f; // Checksum mismatch
                }

                // Extract float value (bytes 4-7)
                float current = BytesToFloat(response, 4);
                return current;
            }
            catch
            {
                return -1.0f;
            }
        }

        /// <summary>
        /// Sets a preset memory slot (M1-M6) with the specified voltage and current values.
        /// </summary>
        /// <param name="presetNumber">The preset number (1-6 for M1-M6).</param>
        /// <param name="voltage">The voltage value in volts to store in the preset.</param>
        /// <param name="current">The current limit value in amperes to store in the preset.</param>
        /// <returns>True if both voltage and current were successfully written; otherwise, false.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// It writes both the voltage and current values to the specified preset memory slot.
        /// 
        /// The DPS-150 has 6 preset memory slots (M1-M6) that can store voltage and current setpoints.
        /// These presets can be recalled later on the device to quickly switch between different settings.
        /// 
        /// Protocol details:
        /// - Voltage command: F1 B1 [voltage_register] 04 [float bytes] [checksum]
        /// - Current command: F1 B1 [current_register] 04 [float bytes] [checksum]
        /// - Float format: IEEE-754 float32, little-endian
        /// - Register pairs: M1(0xC5/0xC6), M2(0xC7/0xC8), M3(0xC9/0xCA), M4(0xCB/0xCC), M5(0xCD/0xCE), M6(0xCF/0xD0)
        /// 
        /// Important notes:
        /// - The device must be connected before calling this method
        /// - A session is automatically started if needed
        /// - Preset numbers must be in the range 1-6
        /// - Invalid preset numbers return false
        /// - Both values must be written successfully for the method to return true
        /// - A 100ms delay is applied between voltage and current writes
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// 
        /// // Set preset M1 to 12V / 2A
        /// if (control.SetPreset(1, 12.0f, 2.0f))
        /// {
        ///     Console.WriteLine("Preset M1 configured: 12V / 2A");
        /// }
        /// 
        /// // Set preset M2 to 5V / 1A
        /// if (control.SetPreset(2, 5.0f, 1.0f))
        /// {
        ///     Console.WriteLine("Preset M2 configured: 5V / 1A");
        /// }
        /// 
        /// control.DisconnectFromDevice();
        /// </code>
        /// </example>
        public bool SetPreset(int presetNumber, float voltage, float current)
        {
            if (!IsConnected)
            {
                return false;
            }

            // Validate preset number (1-6)
            if (presetNumber < 1 || presetNumber > 6)
            {
                return false; // Invalid preset number
            }

            // Ensure a session is started before sending preset commands
            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return false; // Failed to start session
                }

                // Give device time to process session start command
                System.Threading.Thread.Sleep(50);
            }

            // Map preset number to voltage and current register addresses
            DPS150RegisterAddress voltageRegister;
            DPS150RegisterAddress currentRegister;

            switch (presetNumber)
            {
                case 1:
                    voltageRegister = DPS150RegisterAddress.PresetM1Voltage;
                    currentRegister = DPS150RegisterAddress.PresetM1Current;
                    break;
                case 2:
                    voltageRegister = DPS150RegisterAddress.PresetM2Voltage;
                    currentRegister = DPS150RegisterAddress.PresetM2Current;
                    break;
                case 3:
                    voltageRegister = DPS150RegisterAddress.PresetM3Voltage;
                    currentRegister = DPS150RegisterAddress.PresetM3Current;
                    break;
                case 4:
                    voltageRegister = DPS150RegisterAddress.PresetM4Voltage;
                    currentRegister = DPS150RegisterAddress.PresetM4Current;
                    break;
                case 5:
                    voltageRegister = DPS150RegisterAddress.PresetM5Voltage;
                    currentRegister = DPS150RegisterAddress.PresetM5Current;
                    break;
                case 6:
                    voltageRegister = DPS150RegisterAddress.PresetM6Voltage;
                    currentRegister = DPS150RegisterAddress.PresetM6Current;
                    break;
                default:
                    return false; // Should never reach here due to earlier validation
            }

            try
            {
                // Write voltage to preset
                byte[] voltageData = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, voltageRegister, FloatToBytes(voltage));
                if (!SendData(voltageData))
                {
                    return false; // Failed to write voltage
                }

                // Small delay between commands
                System.Threading.Thread.Sleep(100);

                // Write current to preset
                byte[] currentData = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, currentRegister, FloatToBytes(current));
                if (!SendData(currentData))
                {
                    return false; // Failed to write current
                }

                return true; // Both values written successfully
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the voltage and current values of a specific preset memory slot.
        /// </summary>
        /// <param name="presetNumber">The preset number (1-6, corresponding to M1-M6).</param>
        /// <param name="voltage">Output parameter that receives the preset voltage in volts.</param>
        /// <param name="current">Output parameter that receives the preset current in amperes.</param>
        /// <returns>True if both values were successfully read; otherwise, false.</returns>
        /// <remarks>
        /// This method reads the stored voltage and current values from one of the device's six preset memory slots (M1-M6).
        /// It automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Voltage read command: F1 A1 [voltage_register] 00 [checksum]
        /// - Current read command: F1 A1 [current_register] 00 [checksum]
        /// - Voltage response: F0 A1 [voltage_register] 04 [float bytes] [checksum]
        /// - Current response: F0 A1 [current_register] 04 [float bytes] [checksum]
        /// - Float format: IEEE-754 float32, little-endian
        /// - Register pairs: M1(0xC5/0xC6), M2(0xC7/0xC8), M3(0xC9/0xCA), M4(0xCB/0xCC), M5(0xCD/0xCE), M6(0xCF/0xD0)
        /// 
        /// Important notes:
        /// - The device must be connected before calling this method
        /// - A session is automatically started if needed
        /// - Preset numbers must be in the range 1-6
        /// - Invalid preset numbers return false and set output parameters to 0.0f
        /// - Both values must be read successfully for the method to return true
        /// - A 50ms delay is applied between voltage and current reads
        /// - If either read fails, both output parameters are set to 0.0f
        /// </remarks>
        /// <example>
        /// <code>
        /// var control = new DPS150Control();
        /// control.ConnectToDevice("COM3");
        /// 
        /// // Read preset M1
        /// if (control.GetPreset(1, out float voltage, out float current))
        /// {
        ///     Console.WriteLine($"Preset M1: {voltage:F2}V / {current:F2}A");
        /// }
        /// 
        /// // Read preset M2
        /// if (control.GetPreset(2, out float v2, out float i2))
        /// {
        ///     Console.WriteLine($"Preset M2: {v2:F2}V / {i2:F2}A");
        /// }
        /// 
        /// control.DisconnectFromDevice();
        /// </code>
        /// </example>
        public bool GetPreset(int presetNumber, out float voltage, out float current)
        {
            // Initialize output parameters
            voltage = 0.0f;
            current = 0.0f;

            if (!IsConnected)
            {
                return false;
            }

            // Validate preset number (1-6)
            if (presetNumber < 1 || presetNumber > 6)
            {
                return false; // Invalid preset number
            }

            // Ensure a session is started before reading preset values
            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return false; // Failed to start session
                }

                // Give device time to process session start command
                System.Threading.Thread.Sleep(50);
            }

            // Map preset number to voltage and current register addresses
            DPS150RegisterAddress voltageRegister;
            DPS150RegisterAddress currentRegister;

            switch (presetNumber)
            {
                case 1:
                    voltageRegister = DPS150RegisterAddress.PresetM1Voltage;
                    currentRegister = DPS150RegisterAddress.PresetM1Current;
                    break;
                case 2:
                    voltageRegister = DPS150RegisterAddress.PresetM2Voltage;
                    currentRegister = DPS150RegisterAddress.PresetM2Current;
                    break;
                case 3:
                    voltageRegister = DPS150RegisterAddress.PresetM3Voltage;
                    currentRegister = DPS150RegisterAddress.PresetM3Current;
                    break;
                case 4:
                    voltageRegister = DPS150RegisterAddress.PresetM4Voltage;
                    currentRegister = DPS150RegisterAddress.PresetM4Current;
                    break;
                case 5:
                    voltageRegister = DPS150RegisterAddress.PresetM5Voltage;
                    currentRegister = DPS150RegisterAddress.PresetM5Current;
                    break;
                case 6:
                    voltageRegister = DPS150RegisterAddress.PresetM6Voltage;
                    currentRegister = DPS150RegisterAddress.PresetM6Current;
                    break;
                default:
                    return false; // Should never reach here due to earlier validation
            }

            try
            {
                // Read voltage from preset
                byte[] voltageReadCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, voltageRegister, null);
                byte[]? voltageResponse = SendCommandAndGetResponse(voltageReadCommand, 1000);

                if (voltageResponse == null || voltageResponse.Length < 9)
                {
                    return false; // Invalid voltage response
                }

                // Verify voltage response packet structure
                if (voltageResponse[0] != (byte)DPS150ComDirection.RX || // Should be 0xF0 (device response)
                    voltageResponse[1] != (byte)DPS150AccessType.Read || // Should be 0xA1 (Read)
                    voltageResponse[2] != (byte)voltageRegister) // Should match requested register
                {
                    return false; // Invalid voltage response header
                }

                // Verify voltage response checksum
                if (!VerifyChecksum(voltageResponse))
                {
                    return false; // Voltage checksum mismatch
                }

                // Extract voltage value (bytes 4-7)
                voltage = BytesToFloat(voltageResponse, 4);

                // Small delay between reads
                System.Threading.Thread.Sleep(50);

                // Read current from preset
                byte[] currentReadCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, currentRegister, null);
                byte[]? currentResponse = SendCommandAndGetResponse(currentReadCommand, 1000);

                if (currentResponse == null || currentResponse.Length < 9)
                {
                    voltage = 0.0f; // Reset voltage on failure
                    return false; // Invalid current response
                }

                // Verify current response packet structure
                if (currentResponse[0] != (byte)DPS150ComDirection.RX || // Should be 0xF0 (device response)
                    currentResponse[1] != (byte)DPS150AccessType.Read || // Should be 0xA1 (Read)
                    currentResponse[2] != (byte)currentRegister) // Should match requested register
                {
                    voltage = 0.0f; // Reset voltage on failure
                    return false; // Invalid current response header
                }

                // Verify current response checksum
                if (!VerifyChecksum(currentResponse))
                {
                    voltage = 0.0f; // Reset voltage on failure
                    return false; // Current checksum mismatch
                }

                // Extract current value (bytes 4-7)
                current = BytesToFloat(currentResponse, 4);

                return true; // Both values read successfully
            }
            catch
            {
                voltage = 0.0f;
                current = 0.0f;
                return false;
                }
            }



            #endregion

            #region Protection Settings (OVP, OCP, OPP, OTP, LVP)

            /// <summary>
            /// Sets the Over-Voltage Protection (OVP) threshold.
            /// </summary>
            /// <param name="ovp">The OVP threshold in volts (typically 0-160V).</param>
        /// <returns>True if the OVP was successfully set; otherwise, false.</returns>
        /// <remarks>
        /// Over-Voltage Protection automatically disables the output when the voltage exceeds this threshold.
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 B1 D1 04 [float bytes] [checksum]
        /// - Register: 0xD1 (OVP)
        /// - Float format: IEEE-754 float32, little-endian
        /// 
        /// Typical range: 0.0V to 160.0V (depends on device capability)
        /// </remarks>
        public bool SetOVP(float ovp)
        {
            if (!IsConnected)
            {
                return false;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return false;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] packet = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, DPS150RegisterAddress.OVP, FloatToBytes(ovp));
                return SendData(packet);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Over-Voltage Protection (OVP) threshold from the device.
        /// </summary>
        /// <returns>The OVP threshold in volts, or -1.0f if the read operation failed.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 D1 00 [checksum]
        /// - Response: F0 A1 D1 04 [float bytes] [checksum]
        /// - Register: 0xD1 (OVP)
        /// - Float format: IEEE-754 float32, little-endian
        /// </remarks>
        public float GetOVP()
        {
            if (!IsConnected)
            {
                return -1.0f;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1.0f;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.OVP, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return -1.0f;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.OVP)
                {
                    return -1.0f;
                }

                if (!VerifyChecksum(response))
                {
                    return -1.0f;
                }

                return BytesToFloat(response, 4);
            }
            catch
            {
                return -1.0f;
            }
        }

        /// <summary>
        /// Sets the Over-Current Protection (OCP) threshold.
        /// </summary>
        /// <param name="ocp">The OCP threshold in amperes (typically 0-20A).</param>
        /// <returns>True if the OCP was successfully set; otherwise, false.</returns>
        /// <remarks>
        /// Over-Current Protection automatically disables the output when the current exceeds this threshold.
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 B1 D2 04 [float bytes] [checksum]
        /// - Register: 0xD2 (OCP)
        /// - Float format: IEEE-754 float32, little-endian
        /// 
        /// Typical range: 0.0A to 20.0A (depends on device capability)
        /// </remarks>
        public bool SetOCP(float ocp)
        {
            if (!IsConnected)
            {
                return false;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return false;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] packet = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, DPS150RegisterAddress.OCP, FloatToBytes(ocp));
                return SendData(packet);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Over-Current Protection (OCP) threshold from the device.
        /// </summary>
        /// <returns>The OCP threshold in amperes, or -1.0f if the read operation failed.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 D2 00 [checksum]
        /// - Response: F0 A1 D2 04 [float bytes] [checksum]
        /// - Register: 0xD2 (OCP)
        /// - Float format: IEEE-754 float32, little-endian
        /// </remarks>
        public float GetOCP()
        {
            if (!IsConnected)
            {
                return -1.0f;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1.0f;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.OCP, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return -1.0f;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.OCP)
                {
                    return -1.0f;
                }

                if (!VerifyChecksum(response))
                {
                    return -1.0f;
                }

                return BytesToFloat(response, 4);
            }
            catch
            {
                return -1.0f;
            }
        }

        /// <summary>
        /// Sets the Over-Power Protection (OPP) threshold.
        /// </summary>
        /// <param name="opp">The OPP threshold in watts (typically 0-3000W).</param>
        /// <returns>True if the OPP was successfully set; otherwise, false.</returns>
        /// <remarks>
        /// Over-Power Protection automatically disables the output when the power (V × I) exceeds this threshold.
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 B1 D3 04 [float bytes] [checksum]
        /// - Register: 0xD3 (OPP)
        /// - Float format: IEEE-754 float32, little-endian
        /// 
        /// Typical range: 0.0W to 3000.0W (depends on device capability)
        /// </remarks>
        public bool SetOPP(float opp)
        {
            if (!IsConnected)
            {
                return false;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return false;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] packet = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, DPS150RegisterAddress.OPP, FloatToBytes(opp));
                return SendData(packet);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Over-Power Protection (OPP) threshold from the device.
        /// </summary>
        /// <returns>The OPP threshold in watts, or -1.0f if the read operation failed.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 D3 00 [checksum]
        /// - Response: F0 A1 D3 04 [float bytes] [checksum]
        /// - Register: 0xD3 (OPP)
        /// - Float format: IEEE-754 float32, little-endian
        /// </remarks>
        public float GetOPP()
        {
            if (!IsConnected)
            {
                return -1.0f;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1.0f;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.OPP, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return -1.0f;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.OPP)
                {
                    return -1.0f;
                }

                if (!VerifyChecksum(response))
                {
                    return -1.0f;
                }

                return BytesToFloat(response, 4);
            }
            catch
            {
                return -1.0f;
            }
        }

        /// <summary>
        /// Sets the Over-Temperature Protection (OTP) threshold.
        /// </summary>
        /// <param name="otp">The OTP threshold in degrees Celsius (typically 0-100°C).</param>
        /// <returns>True if the OTP was successfully set; otherwise, false.</returns>
        /// <remarks>
        /// Over-Temperature Protection automatically disables the output when the internal temperature exceeds this threshold.
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 B1 D4 04 [float bytes] [checksum]
        /// - Register: 0xD4 (OTP)
        /// - Float format: IEEE-754 float32, little-endian
        /// 
        /// Typical range: 0.0°C to 100.0°C (depends on device capability)
        /// </remarks>
        public bool SetOTP(float otp)
        {
            if (!IsConnected)
            {
                return false;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return false;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] packet = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, DPS150RegisterAddress.OTP, FloatToBytes(otp));
                return SendData(packet);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Over-Temperature Protection (OTP) threshold from the device.
        /// </summary>
        /// <returns>The OTP threshold in degrees Celsius, or -1.0f if the read operation failed.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 D4 00 [checksum]
        /// - Response: F0 A1 D4 04 [float bytes] [checksum]
        /// - Register: 0xD4 (OTP)
        /// - Float format: IEEE-754 float32, little-endian
        /// </remarks>
        public float GetOTP()
        {
            if (!IsConnected)
            {
                return -1.0f;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1.0f;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.OTP, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return -1.0f;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.OTP)
                {
                    return -1.0f;
                }

                if (!VerifyChecksum(response))
                {
                    return -1.0f;
                }

                return BytesToFloat(response, 4);
            }
            catch
            {
                return -1.0f;
            }
        }

        /// <summary>
        /// Sets the Low-Voltage Protection (LVP) threshold.
        /// </summary>
        /// <param name="lvp">The LVP threshold in volts (typically 0-30V).</param>
        /// <returns>True if the LVP was successfully set; otherwise, false.</returns>
        /// <remarks>
        /// Low-Voltage Protection automatically disables the output when the input voltage drops below this threshold.
        /// This protects against battery deep discharge or unstable power sources.
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 B1 D5 04 [float bytes] [checksum]
        /// - Register: 0xD5 (LVP)
        /// - Float format: IEEE-754 float32, little-endian
        /// 
        /// Typical range: 0.0V to 30.0V (depends on input voltage range)
        /// </remarks>
        public bool SetLVP(float lvp)
        {
            if (!IsConnected)
            {
                return false;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return false;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] packet = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, DPS150RegisterAddress.LVP, FloatToBytes(lvp));
                return SendData(packet);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Low-Voltage Protection (LVP) threshold from the device.
        /// </summary>
        /// <returns>The LVP threshold in volts, or -1.0f if the read operation failed.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 D5 00 [checksum]
        /// - Response: F0 A1 D5 04 [float bytes] [checksum]
        /// - Register: 0xD5 (LVP)
        /// - Float format: IEEE-754 float32, little-endian
        /// </remarks>
        public float GetLVP()
        {
            if (!IsConnected)
            {
                return -1.0f;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1.0f;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.LVP, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return -1.0f;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.LVP)
                {
                    return -1.0f;
                }

                if (!VerifyChecksum(response))
                {
                    return -1.0f;
                }

                return BytesToFloat(response, 4);
            }
            catch
            {
                return -1.0f;
            }
        }

        #endregion

        #region UI Settings (Brightness, Volume)

        /// <summary>
        /// Sets the display brightness level.
        /// </summary>
        /// <param name="brightness">The brightness level (typically 0-100, where 0 is off and 100 is maximum).</param>
        /// <returns>True if the brightness was successfully set; otherwise, false.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 B1 D6 04 [int32 bytes] [checksum]
        /// - Register: 0xD6 (Brightness)
        /// - Data format: 32-bit integer, little-endian
        /// 
        /// Typical range: 0 (off) to 100 (maximum brightness)
        /// </remarks>
        public bool SetBrightness(int brightness)
        {
            if (!IsConnected)
            {
                return false;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return false;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                // Convert int to 4 bytes (little-endian)
                byte[] intBytes = BitConverter.GetBytes(brightness);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(intBytes);
                }

                byte[] packet = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, DPS150RegisterAddress.Brightness, intBytes);
                return SendData(packet);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the display brightness level from the device.
        /// </summary>
        /// <returns>The brightness level (0-100), or -1 if the read operation failed.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 D6 00 [checksum]
        /// - Response: F0 A1 D6 04 [int32 bytes] [checksum]
        /// - Register: 0xD6 (Brightness)
        /// - Data format: 32-bit integer, little-endian
        /// </remarks>
        public int GetBrightness()
        {
            if (!IsConnected)
            {
                return -1;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.Brightness, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return -1;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.Brightness)
                {
                    return -1;
                }

                if (!VerifyChecksum(response))
                {
                    return -1;
                }

                // Extract int32 value (bytes 4-7)
                byte[] intBytes = new byte[4];
                Array.Copy(response, 4, intBytes, 0, 4);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(intBytes);
                }

                return BitConverter.ToInt32(intBytes, 0);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Sets the device volume level.
        /// </summary>
        /// <param name="volume">The volume level (typically 0-100, where 0 is mute and 100 is maximum).</param>
        /// <returns>True if the volume was successfully set; otherwise, false.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 B1 D7 04 [int32 bytes] [checksum]
        /// - Register: 0xD7 (Volume)
        /// - Data format: 32-bit integer, little-endian
        /// 
        /// Typical range: 0 (mute) to 100 (maximum volume)
        /// </remarks>
        public bool SetVolume(int volume)
        {
            if (!IsConnected)
            {
                return false;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return false;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                // Convert int to 4 bytes (little-endian)
                byte[] intBytes = BitConverter.GetBytes(volume);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(intBytes);
                }

                byte[] packet = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, DPS150RegisterAddress.Volume, intBytes);
                return SendData(packet);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the device volume level from the device.
        /// </summary>
        /// <returns>The volume level (0-100), or -1 if the read operation failed.</returns>
        /// <remarks>
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 D7 00 [checksum]
        /// - Response: F0 A1 D7 04 [int32 bytes] [checksum]
        /// - Register: 0xD7 (Volume)
        /// - Data format: 32-bit integer, little-endian
        /// </remarks>
        public int GetVolume()
        {
            if (!IsConnected)
            {
                return -1;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.Volume, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return -1;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.Volume)
                {
                    return -1;
                }

                if (!VerifyChecksum(response))
                {
                    return -1;
                }

                // Extract int32 value (bytes 4-7)
                byte[] intBytes = new byte[4];
                Array.Copy(response, 4, intBytes, 0, 4);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(intBytes);
                }

                return BitConverter.ToInt32(intBytes, 0);
            }
            catch
            {
                return -1;
            }
        }

        #endregion


        #region Telemetry (Read-Only Values)

        /// <summary>
        /// Gets the input voltage (supply voltage) from the device.
        /// </summary>
        /// <returns>The input voltage in volts, or -1.0f if the read operation failed.</returns>
        /// <remarks>
        /// This method reads the actual input/supply voltage feeding the power supply.
        /// The DPS-150 also periodically transmits telemetry data (every ~500ms).
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 C0 00 [checksum]
        /// - Response: F0 A1 C0 04 [float bytes] [checksum]
        /// - Register: 0xC0 (InputVoltage)
        /// - Float format: IEEE-754 float32, little-endian
        /// 
        /// Use case: Monitor input voltage to ensure stable operation and detect power issues.
        /// </remarks>
        public float GetInputVoltage()
        {
            if (!IsConnected)
            {
                return -1.0f;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1.0f;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.InputVoltage, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return -1.0f;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.InputVoltage)
                {
                    return -1.0f;
                }

                if (!VerifyChecksum(response))
                {
                    return -1.0f;
                }

                return BytesToFloat(response, 4);
            }
            catch
            {
                return -1.0f;
            }
        }

        /// <summary>
        /// Gets the maximum voltage capability of the device.
        /// </summary>
        /// <returns>The maximum voltage in volts, or -1.0f if the read operation failed.</returns>
        /// <remarks>
        /// This method reads the device's maximum voltage specification/capability.
        /// This is typically a fixed value based on the device model (e.g., 150V for DPS-150).
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 E2 00 [checksum]
        /// - Response: F0 A1 E2 04 [float bytes] [checksum]
        /// - Register: 0xE2 (MaximumVoltage)
        /// - Float format: IEEE-754 float32, little-endian
        /// 
        /// Use case: Determine device capabilities and validate setpoint ranges.
        /// </remarks>
        public float GetMaximumVoltage()
        {
            if (!IsConnected)
            {
                return -1.0f;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1.0f;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.MaximumVoltage, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return -1.0f;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.MaximumVoltage)
                {
                    return -1.0f;
                }

                if (!VerifyChecksum(response))
                {
                    return -1.0f;
                }

                return BytesToFloat(response, 4);
            }
            catch
            {
                return -1.0f;
            }
        }

        /// <summary>
        /// Gets the maximum current capability of the device.
        /// </summary>
        /// <returns>The maximum current in amperes, or -1.0f if the read operation failed.</returns>
        /// <remarks>
        /// This method reads the device's maximum current specification/capability.
        /// This is typically a fixed value based on the device model (e.g., 15A for DPS-150).
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 E3 00 [checksum]
        /// - Response: F0 A1 E3 04 [float bytes] [checksum]
        /// - Register: 0xE3 (MaximumCurrent)
        /// - Float format: IEEE-754 float32, little-endian
        /// 
        /// Use case: Determine device capabilities and validate setpoint ranges.
        /// </remarks>
        public float GetMaximumCurrent()
        {
            if (!IsConnected)
            {
                return -1.0f;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1.0f;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.MaximumCurrent, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return -1.0f;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.MaximumCurrent)
                {
                    return -1.0f;
                }

                if (!VerifyChecksum(response))
                {
                    return -1.0f;
                }

                return BytesToFloat(response, 4);
            }
            catch
            {
                return -1.0f;
            }
        }

        /// <summary>
        /// Gets the internal temperature of the device.
        /// </summary>
        /// <returns>The internal temperature in degrees Celsius, or -1.0f if the read operation failed.</returns>
        /// <remarks>
        /// This method reads the device's internal temperature sensor.
        /// Monitor this value to prevent overheating and ensure safe operation.
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 C4 00 [checksum]
        /// - Response: F0 A1 C4 04 [float bytes] [checksum]
        /// - Register: 0xC4 (InternalTemperature)
        /// - Float format: IEEE-754 float32, little-endian
        /// 
        /// Use case: Monitor device temperature for thermal management and OTP validation.
        /// </remarks>
        public float GetInternalTemperature()
        {
            if (!IsConnected)
            {
                return -1.0f;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1.0f;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.InternalTemperature, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return -1.0f;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.InternalTemperature)
                {
                    return -1.0f;
                }

                if (!VerifyChecksum(response))
                {
                    return -1.0f;
                }

                return BytesToFloat(response, 4);
            }
            catch
            {
                return -1.0f;
            }
        }

        /// <summary>
        /// Gets the current measurement data from the device (voltage and current).
        /// </summary>
        /// <returns>A float array containing [voltage, current], or null if the read operation failed.</returns>
        /// <remarks>
        /// This method reads the actual measured output values (not setpoints).
        /// The returned array contains:
        /// - [0]: Measured output voltage in volts
        /// - [1]: Measured output current in amperes
        /// 
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 C3 00 [checksum]
        /// - Response: F0 A1 C3 08 [voltage float] [current float] [checksum]
        /// - Register: 0xC3 (Measurement)
        /// - Data format: Two IEEE-754 float32 values, little-endian (8 bytes total)
        /// 
        /// Use case: Monitor actual output voltage and current for regulation verification.
        /// </remarks>
        public float[]? GetMeasurement()
        {
            if (!IsConnected)
            {
                return null;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return null;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.Measurement, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                // Expected response: F0 A1 C3 08 [4 voltage bytes] [4 current bytes] [checksum] = 13 bytes
                if (response == null || response.Length < 13)
                {
                    return null;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.Measurement)
                {
                    return null;
                }

                if (!VerifyChecksum(response))
                {
                    return null;
                }

                // Extract two float values
                float voltage = BytesToFloat(response, 4);
                float current = BytesToFloat(response, 8);

                return new float[] { voltage, current };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the measured capacity (Ah) from the device.
        /// </summary>
        /// <returns>The measured capacity in ampere-hours (Ah), or -1.0f if the read operation failed.</returns>
        /// <remarks>
        /// This method reads the accumulated charge capacity since the last reset.
        /// Useful for battery charging/discharging applications.
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 D9 00 [checksum]
        /// - Response: F0 A1 D9 04 [float bytes] [checksum]
        /// - Register: 0xD9 (MeasuredCapacity)
        /// - Float format: IEEE-754 float32, little-endian
        /// 
        /// Use case: Battery capacity testing, charge monitoring.
        /// </remarks>
        public float GetMeasuredCapacity()
        {
            if (!IsConnected)
            {
                return -1.0f;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1.0f;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.MeasuredCapacity, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return -1.0f;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.MeasuredCapacity)
                {
                    return -1.0f;
                }

                if (!VerifyChecksum(response))
                {
                    return -1.0f;
                }

                return BytesToFloat(response, 4);
            }
            catch
            {
                return -1.0f;
            }
        }

        /// <summary>
        /// Gets the measured energy (Wh) from the device.
        /// </summary>
        /// <returns>The measured energy in watt-hours (Wh), or -1.0f if the read operation failed.</returns>
        /// <remarks>
        /// This method reads the accumulated energy consumption since the last reset.
        /// Useful for power consumption monitoring and testing.
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 DA 00 [checksum]
        /// - Response: F0 A1 DA 04 [float bytes] [checksum]
        /// - Register: 0xDA (MeasuredEnergy)
        /// - Float format: IEEE-754 float32, little-endian
        /// 
        /// Use case: Energy consumption monitoring, efficiency testing.
        /// </remarks>
        public float GetMeasuredEnergy()
        {
            if (!IsConnected)
            {
                return -1.0f;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return -1.0f;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.MeasuredEnergy, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return -1.0f;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.MeasuredEnergy)
                {
                    return -1.0f;
                }

                if (!VerifyChecksum(response))
                {
                    return -1.0f;
                }

                return BytesToFloat(response, 4);
            }
            catch
            {
                return -1.0f;
            }
        }

        /// <summary>
        /// Gets the running mode (output state) of the device.
        /// </summary>
        /// <returns>True if the output is running (ON), false if stopped (OFF), or null if the read operation failed.</returns>
        /// <remarks>
        /// This method reads the current output relay state.
        /// The device automatically sends this frame when the running mode changes.
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 DB 00 [checksum]
        /// - Response: F0 A1 DB 04 [int32 bytes] [checksum]
        /// - Register: 0xDB (RunningMode)
        /// - Data format: 32-bit integer (0 = STOP, 1 = RUN)
        /// 
        /// Use case: Monitor output state, verify relay commands.
        /// </remarks>
        public bool? GetRunningMode()
        {
            if (!IsConnected)
            {
                return null;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return null;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.RunningMode, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return null;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.RunningMode)
                {
                    return null;
                }

                if (!VerifyChecksum(response))
                {
                    return null;
                }

                // Extract int32 value (bytes 4-7)
                byte[] intBytes = new byte[4];
                Array.Copy(response, 4, intBytes, 0, 4);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(intBytes);
                }

                int mode = BitConverter.ToInt32(intBytes, 0);
                return mode == 1; // 0 = STOP, 1 = RUN
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the CC/CV mode of the device.
        /// </summary>
        /// <returns>True if in CV (Constant Voltage) mode, false if in CC (Constant Current) mode, or null if the read operation failed.</returns>
        /// <remarks>
        /// This method reads the current regulation mode of the power supply:
        /// - CC (Constant Current): Current limit is reached, voltage is regulated
        /// - CV (Constant Voltage): Voltage setpoint is reached, current is below limit
        /// 
        /// The device automatically sends this frame when the mode changes.
        /// This method automatically starts a session if one is not already active.
        /// 
        /// Protocol details:
        /// - Command: F1 A1 DD 00 [checksum]
        /// - Response: F0 A1 DD 04 [int32 bytes] [checksum]
        /// - Register: 0xDD (CCCV)
        /// - Data format: 32-bit integer (0 = CC, 1 = CV)
        /// 
        /// Use case: Monitor regulation mode, verify load behavior.
        /// </remarks>
        public bool? GetCCCV()
        {
            if (!IsConnected)
            {
                return null;
            }

            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return null;
                }
                System.Threading.Thread.Sleep(50);
            }

            try
            {
                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, DPS150RegisterAddress.CCCV, null);
                byte[]? response = SendCommandAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    return null;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)DPS150RegisterAddress.CCCV)
                {
                    return null;
                }

                if (!VerifyChecksum(response))
                {
                    return null;
                }

                // Extract int32 value (bytes 4-7)
                byte[] intBytes = new byte[4];
                Array.Copy(response, 4, intBytes, 0, 4);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(intBytes);
                }

                int mode = BitConverter.ToInt32(intBytes, 0);
                return mode == 1; // 0 = CC (Constant Current), 1 = CV (Constant Voltage)
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
