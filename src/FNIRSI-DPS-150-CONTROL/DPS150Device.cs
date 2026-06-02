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
// PROTOCOL REFERENCE:
// ===================
// This implementation is based on the FNIRSI-DPS-150 protocol documentation by cho45.
// Protocol specification:
// https://github.com/cho45/fnirsi-dps-150/blob/main/docs/FNIRSI_DPS-150_Protocol.md
//
// Original project repository:
// https://github.com/cho45/fnirsi-dps-150
//
// The protocol documentation includes detailed information about:
// - Communication packet structure
// - Command set and responses
// - Device status messages
// - Protection modes and parameters
//
// Many thanks to cho45 for the excellent reverse-engineering work!
//

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FNIRSI_DPS_150_CONTROL
{
    /// <summary>
    /// Represents the output relay state of the DPS-150 power supply.
    /// </summary>
    public enum OutputRelayState : byte
    {
        /// <summary>
        /// Output relay is OFF (output disabled).
        /// </summary>
        OFF = 0,

        /// <summary>
        /// Output relay is ON (output enabled).
        /// </summary>
        ON = 1
    }

    // 0 = OK, 1 = OVP, 2 = OCP, 3 = OPP, 4 = OTP, 5 = LVP, 6 = REP
    /// <summary>
    /// Represents the protection mode status of the DPS-150 power supply.
    /// </summary>
    public enum DPS150ProtectionMode : byte
    {
        /// <summary>Protection is OK, no protection triggered.</summary>
        OK = 0,
        /// <summary>Over-Voltage Protection triggered.</summary>
        OVP = 1,
        /// <summary>Over-Current Protection triggered.</summary>
        OCP = 2,
        /// <summary>Over-Power Protection triggered.</summary>
        OPP = 3,
        /// <summary>Over-Temperature Protection triggered.</summary>
        OTP = 4,
        /// <summary>Low-Voltage Protection triggered.</summary>
        LVP = 5,
        /// <summary>Reverse polarity protection triggered.</summary>
        REP = 6
    }

    enum DPS150ComDirection : byte
    {
        RX = 0xF0,
        TX = 0xF1,
    }

    enum DPS150AccessType : byte
    {
        Read = 0xA1,
        Baud = 0xB0,
        Write = 0xB1,
        Control = 0xC1,
    }

    enum DPS150RegisterAddress : byte
    {
        // Active Output Control
        VoltageSetpoint = 0xC1,
        CurrentLimit = 0xC2,

        // Output relay state
        OutputRelayState = 0xDB,

        // Preset Memory (M1–M6)
        PresetM1Voltage = 0xC5,
        PresetM1Current = 0xC6,
        PresetM2Voltage = 0xC7,
        PresetM2Current = 0xC8,
        PresetM3Voltage = 0xC9,
        PresetM3Current = 0xCA,
        PresetM4Voltage = 0xCB,
        PresetM4Current = 0xCC,
        PresetM5Voltage = 0xCD,
        PresetM5Current = 0xCE,
        PresetM6Voltage = 0xCF,
        PresetM6Current = 0xD0,

        // Protection Settings
        OVP = 0xD1,
        OCP = 0xD2,
        OPP = 0xD3,
        OTP = 0xD4,
        LVP = 0xD5,

        // UI / System Settings
        Brightness = 0xD6,
        Volume = 0xD7,

        // Telemetry (RX only)
        // DPS‑150 periodically transmits telemetry (period - 500ms).
        InputVoltage = 0xC0,
        MaximumVoltage = 0xE2,
        MaximumCurrent = 0xE3,
        InternalTemperature = 0xC4,
        Measurement = 0xC3,

        // Additional telemetry (RX - Energy + Capacity)
        // You can request additional telemetry for energy and capacity:
        EnergyAndCapacity = 0xD8,
        MeasuredCapacity = 0xD9,
        MeasuredEnergy = 0xDA,

        //Telemetry on change
        // Some frames are automatically sent by device on register change (no need to request it)
        RunningMode = 0xDB,         // 0 = STOP, 1 = RUN
        ProtectionMode = 0xDC,      // 0 = OK, 1 = OVP, 2 = OCP, 3 = OPP, 4 = OTP, 5 = LVP, 6 = REP
        CCCV = 0xDD                 // 0 = CC, 1 = CV
    }

    public class DPS150Registers
    {
        /// <summary>
        /// Low-level serial communication handler for the DPS-150 device.
        /// Manages the physical serial port connection and data transmission.
        /// Internal access allows DPS150Control to use low-level methods when needed.
        /// </summary>
        internal DPS150Communication _communication = new DPS150Communication();

        /// <summary>
        /// Tracks whether a communication session has been established with the device.
        /// The DPS-150 requires an active session before most register operations.
        /// Session is started with StartSession() and stopped with StopSession().
        /// </summary>
        private bool _sessionStarted = false;

        // Private fields for register values
        private float voltageSetpoint;
        private float currentLimit;
        private bool outputRelayState;

        // Preset memory fields
        private float presetM1Voltage;
        private float presetM1Current;
        private float presetM2Voltage;
        private float presetM2Current;
        private float presetM3Voltage;
        private float presetM3Current;
        private float presetM4Voltage;
        private float presetM4Current;
        private float presetM5Voltage;
        private float presetM5Current;
        private float presetM6Voltage;
        private float presetM6Current;

        // Protection settings fields
        private float ovp;
        private float ocp;
        private float opp;
        private float otp;
        private float lvp;

        // UI settings fields
        private byte brightness;
        private byte volume;

        // Telemetry fields
        private float _measuredVoltage;
        private float _measuredCurrent;
        private float _measuredPower;
        private float _inputVoltage;
        private float _maximumVoltage;
        private float _maximumCurrent;
        private float _internalTemperature;
        private float _measuredCapacity;
        private float _measuredEnergy;
        private bool _runningMode;
        private DPS150ProtectionMode _protectionMode;
        private bool _cccv;

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
        /// Most register operations automatically start a session if one is not already active.
        /// </remarks>
        public bool IsSessionStarted { get => _sessionStarted; }

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
        private byte CalculateChecksum(byte[] data)
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
        private bool VerifyChecksum(byte[] packet)
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
        /// Parses telemetry frames received from the DPS-150 device and updates corresponding properties.
        /// </summary>
        /// <param name="data">The complete byte array containing one or more telemetry frames.</param>
        /// <returns>True if at least one frame was successfully parsed; otherwise, false.</returns>
        /// <remarks>
        /// The DPS-150 device periodically sends telemetry data in multiple frames:
        /// 
        /// Frame 1 (17 bytes): Measurement (0xC3)
        ///   - Header: F0 A1
        ///   - Register: C3 (Measurement)
        ///   - Length: 0C (12 bytes)
        ///   - Data: MeasuredVoltage (float32), MeasuredCurrent (float32), MeasuredPower (float32)
        ///   - Checksum: 1 byte
        /// 
        /// Frame 2 (9 bytes): InputVoltage (0xC0)
        ///   - Header: F0 A1
        ///   - Register: C0
        ///   - Length: 04 (4 bytes)
        ///   - Data: InputVoltage (float32)
        ///   - Checksum: 1 byte
        /// 
        /// Frame 3 (9 bytes): MaximumVoltage (0xE2)
        ///   - Header: F0 A1
        ///   - Register: E2
        ///   - Length: 04 (4 bytes)
        ///   - Data: MaximumVoltage (float32)
        ///   - Checksum: 1 byte
        /// 
        /// Frame 4 (9 bytes): MaximumCurrent (0xE3)
        ///   - Header: F0 A1
        ///   - Register: E3
        ///   - Length: 04 (4 bytes)
        ///   - Data: MaximumCurrent (float32)
        ///   - Checksum: 1 byte
        /// 
        /// Frame 5 (9 bytes): InternalTemperature (0xC4)
        ///   - Header: F0 A1
        ///   - Register: C4
        ///   - Length: 04 (4 bytes)
        ///   - Data: InternalTemperature (float32)
        ///   - Checksum: 1 byte
        /// 
        /// Frame 6 (9 bytes): MeasuredCapacity (0xD9) - OPTIONAL
        ///   - Header: F0 A1
        ///   - Register: D9
        ///   - Length: 04 (4 bytes)
        ///   - Data: MeasuredCapacity (float32) in Ah
        ///   - Checksum: 1 byte
        /// 
        /// Frame 7 (9 bytes): MeasuredEnergy (0xDA) - OPTIONAL
        ///   - Header: F0 A1
        ///   - Register: DA
        ///   - Length: 04 (4 bytes)
        ///   - Data: MeasuredEnergy (float32) in Wh
        ///   - Checksum: 1 byte
        /// 
        /// Standard telemetry size: 53 bytes (frames 1-5)
        /// Extended telemetry size: 71 bytes (frames 1-7, when capacity and energy frames are present)
        /// 
        /// Each frame begins with F0 A1 (RX Read header) and ends with a checksum byte.
        /// The method automatically identifies individual frames and extracts the corresponding values.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Parse telemetry data received from device
        /// byte[] telemetryData = _communication.ReadResponse(5000);
        /// if (ParseTelemetryFrames(telemetryData))
        /// {
        ///     Console.WriteLine($"Voltage: {MeasuredVoltage:F3}V");
        ///     Console.WriteLine($"Current: {MeasuredCurrent:F3}A");
        ///     Console.WriteLine($"Power: {MeasuredPower:F3}W");
        ///     Console.WriteLine($"Input Voltage: {InputVoltage:F3}V");
        ///     Console.WriteLine($"Temperature: {InternalTemperature:F1}°C");
        ///     Console.WriteLine($"Capacity: {MeasuredCapacity:F3}Ah");
        ///     Console.WriteLine($"Energy: {MeasuredEnergy:F3}Wh");
        /// }
        /// </code>
        /// </example>
        public bool ParseTelemetryFrames(byte[] data)
        {
            if (data == null || data.Length < 9)
            {
                return false; // Minimum frame size is 9 bytes (header + register + length + 4-byte float + checksum)
            }

            bool parsedAtLeastOne = false;
            int offset = 0;

            // Parse all frames in the data buffer
            while (offset < data.Length - 4) // Need at least 5 bytes for minimal frame
            {
                // Check for frame start: F0 A1 (RX Read header)
                if (data[offset] != 0xF0 || data[offset + 1] != 0xA1)
                {
                    offset++;
                    continue; // Skip to next byte
                }

                // Extract register address and data length
                byte registerAddress = data[offset + 2];
                byte dataLength = data[offset + 3];

                // Calculate frame length: 2 (header) + 1 (register) + 1 (length) + dataLength + 1 (checksum)
                int frameLength = 2 + 1 + 1 + dataLength + 1;

                // Check if we have enough data for this frame
                if (offset + frameLength > data.Length)
                {
                    break; // Incomplete frame
                }

                // Extract the complete frame
                byte[] frame = new byte[frameLength];
                Array.Copy(data, offset, frame, 0, frameLength);

                // Verify checksum
                if (!VerifyChecksum(frame))
                {
                    offset++; // Skip to next byte if checksum is invalid
                    continue;
                }

                // Parse frame based on register address
                try
                {
                    switch (registerAddress)
                    {
                        case 0xC3: // Measurement (Voltage, Current, Power)
                            if (dataLength == 12)
                            {
                                _measuredVoltage = BytesToFloat(frame, 4);
                                _measuredCurrent = BytesToFloat(frame, 8);
                                _measuredPower = BytesToFloat(frame, 12);
                                parsedAtLeastOne = true;
                            }
                            break;

                        case 0xC0: // InputVoltage
                            if (dataLength == 4)
                            {
                                _inputVoltage = BytesToFloat(frame, 4);
                                parsedAtLeastOne = true;
                            }
                            break;

                        case 0xE2: // MaximumVoltage
                            if (dataLength == 4)
                            {
                                _maximumVoltage = BytesToFloat(frame, 4);
                                parsedAtLeastOne = true;
                            }
                            break;

                        case 0xE3: // MaximumCurrent
                            if (dataLength == 4)
                            {
                                _maximumCurrent = BytesToFloat(frame, 4);
                                parsedAtLeastOne = true;
                            }
                            break;

                        case 0xC4: // InternalTemperature
                            if (dataLength == 4)
                            {
                                _internalTemperature = BytesToFloat(frame, 4);
                                parsedAtLeastOne = true;
                            }
                            break;

                        case 0xD9: // MeasuredCapacity (in Ah)
                            if (dataLength == 4)
                            {
                                _measuredCapacity = BytesToFloat(frame, 4);
                                parsedAtLeastOne = true;
                            }
                            break;

                        case 0xDA: // MeasuredEnergy (in Wh)
                            if (dataLength == 4)
                            {
                                _measuredEnergy = BytesToFloat(frame, 4);
                                parsedAtLeastOne = true;
                            }
                            break;

                        default:
                            // Unknown register, skip frame
                            break;
                    }
                }
                catch
                {
                    // Error parsing frame, continue with next
                }

                // Move to the next frame
                offset += frameLength;
            }

            return parsedAtLeastOne;
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
            bool success = _communication.SendData(data);

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
            bool success = _communication.SendData(data);

            if (success)
            {
                _sessionStarted = false;
            }

            return success;
        }

        /// <summary>
        /// Flushes the serial port buffers (both input and output).
        /// </summary>
        /// <remarks>
        /// This method clears any pending data in the serial port's receive and transmit buffers.
        /// Use this to discard old or unexpected data before sending new commands.
        /// </remarks>
        public void FlushBuffers()
        {
            if (IsConnected)
            {
                _communication.Flush();
            }
        }

        /// <summary>
        /// Gets an array of available serial port names on the system.
        /// </summary>
        /// <returns>An array of COM port names, or an empty array if none are available.</returns>
        /// <remarks>
        /// This is a static method that enumerates all serial ports without requiring a connection.
        /// Useful for port selection UI or discovery.
        /// </remarks>
        public static string[] GetAvailablePorts()
        {
            return DPS150Communication.GetAvailablePorts() ?? new string[0];
        }

        /// <summary>
        /// Helper method to write a float value to a register and read it back for verification.
        /// </summary>
        /// <param name="registerAddress">The register address to write to.</param>
        /// <param name="value">The float value to write.</param>
        /// <param name="resultValue">The verified value read back from the device.</param>
        /// <returns>True if the write and read-back were successful; otherwise, false.</returns>
        /// <remarks>
        /// This method:
        /// 1. Ensures the device is connected
        /// 2. Automatically starts a session if not already active
        /// 3. Sends a write command with the float value
        /// 4. Parses any telemetry frames from the response
        /// 5. Sends a read command to verify the written value
        /// 6. Validates the response structure and checksum
        /// 7. Returns the confirmed value via the out parameter
        /// 
        /// This reduces code duplication for properties like VoltageSetpoint and CurrentLimit.
        /// </remarks>
        private bool WriteAndReadBackFloat(DPS150RegisterAddress registerAddress, float value, out float resultValue)
        {
            resultValue = 0.0f;

            if (!IsConnected)
            {
                return false;
            }

            // Ensure a session is started before sending command
            if (!_sessionStarted)
            {
                if (!StartSession())
                {
                    return false;
                }

                // Give device time to process session start command
                System.Threading.Thread.Sleep(50);
            }

            byte[] data = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, registerAddress, FloatToBytes(value));

            try
            {
                byte[]? response = _communication.SendDataAndGetResponse(data, 1000);

                ParseTelemetryFrames(response); // Update telemetry properties from response frames

                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, registerAddress, null);

                // Send read command and wait for response
                response = _communication.SendDataAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 9)
                {
                    // Invalid response (minimum: F0 A1 REG 04 [4 float bytes] [checksum] = 9 bytes)
                    return false;
                }

                // Verify response packet structure
                if (response[0] != (byte)DPS150ComDirection.RX || // Should be 0xF0 (device response)
                    response[1] != (byte)DPS150AccessType.Read || // Should be 0xA1 (Read)
                    response[2] != (byte)registerAddress) // Should match the register address
                {
                    return false; // Invalid response header
                }

                // Verify checksum
                if (!VerifyChecksum(response))
                {
                    return false; // Checksum mismatch
                }

                // Extract float value (bytes 4-7)
                resultValue = BytesToFloat(response, 4);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Helper method to write a byte value to a register and read it back for verification.
        /// </summary>
        /// <param name="registerAddress">The register address to write to.</param>
        /// <param name="value">The byte value to write.</param>
        /// <param name="resultValue">The verified value read back from the device.</param>
        /// <returns>True if the write and read-back were successful; otherwise, false.</returns>
        private bool WriteAndReadBackByte(DPS150RegisterAddress registerAddress, byte value, out byte resultValue)
        {
            resultValue = 0;

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

            byte[] data = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, registerAddress, new byte[] { value });

            try
            {
                byte[]? response = _communication.SendDataAndGetResponse(data, 1000);
                ParseTelemetryFrames(response);

                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, registerAddress, null);
                response = _communication.SendDataAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 6)
                {
                    // Minimum: F0 A1 REG 01 [1 byte] [checksum] = 6 bytes
                    return false;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)registerAddress)
                {
                    return false;
                }

                if (!VerifyChecksum(response))
                {
                    return false;
                }

                resultValue = response[4];
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Helper method to write a boolean value (as byte) to a register and read it back.
        /// </summary>
        /// <param name="registerAddress">The register address to write to.</param>
        /// <param name="value">The boolean value to write (true=1, false=0).</param>
        /// <param name="resultValue">The verified boolean value read back from the device.</param>
        /// <returns>True if the write and read-back were successful; otherwise, false.</returns>
        private bool WriteAndReadBackBool(DPS150RegisterAddress registerAddress, bool value, out bool resultValue)
        {
            resultValue = false;

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

            byte byteValue = (byte)(value ? 1 : 0);
            byte[] data = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Write, registerAddress, new byte[] { byteValue });

            try
            {
                byte[]? response = _communication.SendDataAndGetResponse(data, 1000);
                ParseTelemetryFrames(response);

                byte[] readCommand = CreatePacket(DPS150ComDirection.TX, DPS150AccessType.Read, registerAddress, null);
                response = _communication.SendDataAndGetResponse(readCommand, 1000);

                if (response == null || response.Length < 6)
                {
                    return false;
                }

                if (response[0] != (byte)DPS150ComDirection.RX ||
                    response[1] != (byte)DPS150AccessType.Read ||
                    response[2] != (byte)registerAddress)
                {
                    return false;
                }

                if (!VerifyChecksum(response))
                {
                    return false;
                }

                resultValue = response[4] != 0;
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Session Control (mandatory) Group C1
        // Before accessing any registers, communication must be enabled.
        // Enable session	F1 C1 00 01 01 02	Must be sent once
        // Disable session	F1 C1 00 01 00 01	Must be sent once
        bool SessionEnabled { get; set; }

        // Baud rate Group B0
        // Baud rate must be send to device after session activation
        // Send F1 B0 00 01 XX 06 where XX is
        int BaudRate { get; set; } = 0;

        // Group B1 - TX
        // Group A1 - RX

        // Active Output Control

        // Voltage Setpoint (C1)
        // TX: F1 B1 C1 04 CD CC 44 41 E3
        // RX: F0 A1 C1 04 CD CC 44 41 E3
        /// <summary>
        /// Gets or sets the voltage setpoint (target output voltage) for the DPS-150 device.
        /// </summary>
        /// <value>
        /// The voltage setpoint in volts (V). Valid range is typically 0.0V to 150.0V.
        /// </value>
        /// <remarks>
        /// Setting this property:
        /// 1. Automatically starts a session if not already active
        /// 2. Sends a write command to register 0xC1
        /// 3. Waits for device response with telemetry data
        /// 4. Sends a read command to verify the written value
        /// 5. Updates the property with the confirmed value from the device
        /// 
        /// The setter will return silently (without throwing exceptions) if:
        /// - The device is not connected (check IsConnected first)
        /// - Session start fails
        /// - Communication timeout occurs
        /// - Response validation fails (header, checksum)
        /// 
        /// Protocol details:
        /// - Register Address: 0xC1
        /// - Data Type: IEEE-754 float32 (little-endian)
        /// - Write Command: F1 B1 C1 04 [float bytes] [checksum]
        /// - Read Command: F1 A1 C1 00 [checksum]
        /// - Response: F0 A1 C1 04 [float bytes] [checksum]
        /// </remarks>
        /// <example>
        /// <code>
        /// var registers = new DPS150Registers();
        /// registers.ConnectToDevice(1);
        /// 
        /// // Set voltage to 12.3V
        /// registers.VoltageSetpoint = 12.3f;
        /// 
        /// // Read back the voltage
        /// float voltage = registers.VoltageSetpoint;
        /// Console.WriteLine($"Voltage: {voltage:F3}V");
        /// </code>
        /// </example>
        public float VoltageSetpoint
        {
            get => voltageSetpoint;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.VoltageSetpoint, value, out float confirmedValue))
                {
                    voltageSetpoint = confirmedValue;
                }
            }
        }

        //Current Limit (C2)
        // TX: F1 B1 C2 04 FD FF FF 3E FF
        // RX: F0 A1 C2 04 FD FF FF 3E FF
        /// <summary>
        /// Gets or sets the current limit (maximum output current) for the DPS-150 device.
        /// </summary>
        /// <value>
        /// The current limit in amperes (A). Valid range is typically 0.0A to 15.0A.
        /// </value>
        /// <remarks>
        /// Setting this property:
        /// 1. Automatically starts a session if not already active
        /// 2. Sends a write command to register 0xC2
        /// 3. Waits for device response with telemetry data
        /// 4. Sends a read command to verify the written value
        /// 5. Updates the property with the confirmed value from the device
        /// 
        /// The setter will return silently (without throwing exceptions) if:
        /// - The device is not connected (check IsConnected first)
        /// - Session start fails
        /// - Communication timeout occurs
        /// - Response validation fails (header, checksum)
        /// 
        /// Protocol details:
        /// - Register Address: 0xC2
        /// - Data Type: IEEE-754 float32 (little-endian)
        /// - Write Command: F1 B1 C2 04 [float bytes] [checksum]
        /// - Read Command: F1 A1 C2 00 [checksum]
        /// - Response: F0 A1 C2 04 [float bytes] [checksum]
        /// </remarks>
        /// <example>
        /// <code>
        /// var registers = new DPS150Registers();
        /// registers.ConnectToDevice(1);
        /// 
        /// // Set current limit to 2.5A
        /// registers.CurrentLimit = 2.5f;
        /// 
        /// // Read back the current limit
        /// float current = registers.CurrentLimit;
        /// Console.WriteLine($"Current Limit: {current:F3}A");
        /// </code>
        /// </example>
        public float CurrentLimit
        {
            get => currentLimit;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.CurrentLimit, value, out float confirmedValue))
                {
                    currentLimit = confirmedValue;
                }
            }
        }

        // Output Enable (RUN / STOP)
        // 0	STOP
        // 1	RUN
        //
        // RUN
        // TX: F1 B1 DB 01 01 DD
        // RX: F0 A1 DB 01 01 DD
        // STOP
        // TX: F1 B1 DB 01 00 DC
        // RX: F0 A1 DB 01 00 DC
        /// <summary>
        /// Gets or sets the output relay state (RUN/STOP) for the DPS-150 device.
        /// </summary>
        /// <value>
        /// True for RUN (output enabled), false for STOP (output disabled).
        /// </value>
        /// <remarks>
        /// This property controls the main output relay of the power supply.
        /// When set to true (RUN), the device will output voltage/current according to the setpoints.
        /// When set to false (STOP), the output is disabled.
        /// 
        /// Protocol details:
        /// - Register Address: 0xDB (RunningMode)
        /// - Data Type: byte (0=STOP, 1=RUN)
        /// - Write RUN: F1 B1 DB 01 01 DD
        /// - Write STOP: F1 B1 DB 01 00 DC
        /// - Read Response: F0 A1 DB 01 [00|01] [checksum]
        /// </remarks>
        /// <example>
        /// <code>
        /// var registers = new DPS150Registers();
        /// registers.ConnectToDevice(1);
        /// 
        /// // Enable output
        /// registers.OutputRelayState = true;
        /// 
        /// // Disable output
        /// registers.OutputRelayState = false;
        /// </code>
        /// </example>
        public bool OutputRelayState
        {
            get => outputRelayState;
            set
            {
                if (WriteAndReadBackBool(DPS150RegisterAddress.RunningMode, value, out bool confirmedValue))
                {
                    outputRelayState = confirmedValue;
                }
            }
        }

        // Preset Memory (M1–M6)
        // TX: F1 B1 C7 04 00 00 B0 40 BB
        // TX: F1 B1 C8 04 FD FF FF 3E 05
        /// <summary>
        /// Gets or sets the voltage for Preset Memory slot M1.
        /// </summary>
        /// <remarks>
        /// Preset M1 Voltage register: 0xC5
        /// Use in combination with PresetM1Current to configure a complete preset.
        /// </remarks>
        public float PresetM1Voltage
        {
            get => presetM1Voltage;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.PresetM1Voltage, value, out float confirmedValue))
                {
                    presetM1Voltage = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current for Preset Memory slot M1.
        /// </summary>
        /// <remarks>
        /// Preset M1 Current register: 0xC6
        /// Use in combination with PresetM1Voltage to configure a complete preset.
        /// </remarks>
        public float PresetM1Current
        {
            get => presetM1Current;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.PresetM1Current, value, out float confirmedValue))
                {
                    presetM1Current = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the voltage for Preset Memory slot M2.
        /// </summary>
        public float PresetM2Voltage
        {
            get => presetM2Voltage;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.PresetM2Voltage, value, out float confirmedValue))
                {
                    presetM2Voltage = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current for Preset Memory slot M2.
        /// </summary>
        public float PresetM2Current
        {
            get => presetM2Current;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.PresetM2Current, value, out float confirmedValue))
                {
                    presetM2Current = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the voltage for Preset Memory slot M3.
        /// </summary>
        public float PresetM3Voltage
        {
            get => presetM3Voltage;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.PresetM3Voltage, value, out float confirmedValue))
                {
                    presetM3Voltage = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current for Preset Memory slot M3.
        /// </summary>
        public float PresetM3Current
        {
            get => presetM3Current;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.PresetM3Current, value, out float confirmedValue))
                {
                    presetM3Current = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the voltage for Preset Memory slot M4.
        /// </summary>
        public float PresetM4Voltage
        {
            get => presetM4Voltage;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.PresetM4Voltage, value, out float confirmedValue))
                {
                    presetM4Voltage = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current for Preset Memory slot M4.
        /// </summary>
        public float PresetM4Current
        {
            get => presetM4Current;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.PresetM4Current, value, out float confirmedValue))
                {
                    presetM4Current = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the voltage for Preset Memory slot M5.
        /// </summary>
        public float PresetM5Voltage
        {
            get => presetM5Voltage;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.PresetM5Voltage, value, out float confirmedValue))
                {
                    presetM5Voltage = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current for Preset Memory slot M5.
        /// </summary>
        public float PresetM5Current
        {
            get => presetM5Current;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.PresetM5Current, value, out float confirmedValue))
                {
                    presetM5Current = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the voltage for Preset Memory slot M6.
        /// </summary>
        public float PresetM6Voltage
        {
            get => presetM6Voltage;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.PresetM6Voltage, value, out float confirmedValue))
                {
                    presetM6Voltage = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current for Preset Memory slot M6.
        /// </summary>
        public float PresetM6Current
        {
            get => presetM6Current;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.PresetM6Current, value, out float confirmedValue))
                {
                    presetM6Current = confirmedValue;
                }
            }
        }

        // Protection Settings
        // TX: F1 B1 D4 04 00 00 80 42 9A
        // All protection writes are followed by:
        // TX: F1 A1 FF 01 00 00
        /// <summary>
        /// Gets or sets the Over-Voltage Protection (OVP) threshold.
        /// </summary>
        /// <value>
        /// The OVP threshold in volts (V). Valid range is typically 0.0V to 160.0V.
        /// </value>
        /// <remarks>
        /// When the output voltage exceeds this threshold, the device will trigger OVP protection.
        /// Register Address: 0xD1
        /// </remarks>
        public float OVP
        {
            get => ovp;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.OVP, value, out float confirmedValue))
                {
                    ovp = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Over-Current Protection (OCP) threshold.
        /// </summary>
        /// <value>
        /// The OCP threshold in amperes (A). Valid range is typically 0.0A to 20.0A.
        /// </value>
        /// <remarks>
        /// When the output current exceeds this threshold, the device will trigger OCP protection.
        /// Register Address: 0xD2
        /// </remarks>
        public float OCP
        {
            get => ocp;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.OCP, value, out float confirmedValue))
                {
                    ocp = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Over-Power Protection (OPP) threshold.
        /// </summary>
        /// <value>
        /// The OPP threshold in watts (W). Valid range is typically 0.0W to 3000.0W.
        /// </value>
        /// <remarks>
        /// When the output power exceeds this threshold, the device will trigger OPP protection.
        /// Register Address: 0xD3
        /// </remarks>
        public float OPP
        {
            get => opp;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.OPP, value, out float confirmedValue))
                {
                    opp = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Over-Temperature Protection (OTP) threshold.
        /// </summary>
        /// <value>
        /// The OTP threshold in degrees Celsius (°C). Valid range is typically 0°C to 100°C.
        /// </value>
        /// <remarks>
        /// When the internal temperature exceeds this threshold, the device will trigger OTP protection.
        /// Register Address: 0xD4
        /// </remarks>
        public float OTP
        {
            get => otp;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.OTP, value, out float confirmedValue))
                {
                    otp = confirmedValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Low-Voltage Protection (LVP) threshold.
        /// </summary>
        /// <value>
        /// The LVP threshold in volts (V). Valid range is typically 0.0V to 30.0V.
        /// </value>
        /// <remarks>
        /// When the input voltage drops below this threshold, the device will trigger LVP protection.
        /// Register Address: 0xD5
        /// </remarks>
        public float LVP
        {
            get => lvp;
            set
            {
                if (WriteAndReadBackFloat(DPS150RegisterAddress.LVP, value, out float confirmedValue))
                {
                    lvp = confirmedValue;
                }
            }
        }

        // UI / System Settings
        //Brightness
        // TX: F1 B1 D6 01 0C E3
        /// <summary>
        /// Gets or sets the display brightness level.
        /// </summary>
        /// <value>
        /// The brightness level (0-100). 0 = minimum brightness, 100 = maximum brightness.
        /// </value>
        /// <remarks>
        /// Register Address: 0xD6
        /// Data Type: byte (0-100)
        /// </remarks>
        /// <example>
        /// <code>
        /// registers.Brightness = 75; // Set to 75% brightness
        /// </code>
        /// </example>
        public byte Brightness
        {
            get => brightness;
            set
            {
                if (WriteAndReadBackByte(DPS150RegisterAddress.Brightness, value, out byte confirmedValue))
                {
                    brightness = confirmedValue;
                }
            }
        }

        // Volume
        // TX: F1 B1 D7 01 09 E1
        /// <summary>
        /// Gets or sets the device audio volume level.
        /// </summary>
        /// <value>
        /// The volume level (0-100). 0 = muted, 100 = maximum volume.
        /// </value>
        /// <remarks>
        /// Register Address: 0xD7
        /// Data Type: byte (0-100)
        /// </remarks>
        /// <example>
        /// <code>
        /// registers.Volume = 50; // Set to 50% volume
        /// </code>
        /// </example>
        public byte Volume
        {
            get => volume;
            set
            {
                if (WriteAndReadBackByte(DPS150RegisterAddress.Volume, value, out byte confirmedValue))
                {
                    volume = confirmedValue;
                }
            }
        }

        // Telemetry(RX only)
        // Telemetry frames are unsolicited and must not be acknowledged.
        public float InputVoltage => _inputVoltage;
        public float MaximumVoltage => _maximumVoltage;
        public float MaximumCurrent => _maximumCurrent;
        public float InternalTemperature => _internalTemperature;
        public float MeasuredVoltage => _measuredVoltage;
        public float MeasuredCurrent => _measuredCurrent;
        public float MeasuredPower => _measuredPower;
        public float MeasuredCapacity => _measuredCapacity;
        public float MeasuredEnergy => _measuredEnergy;
        float[] Measurement { get; } = new float[3];

        // Additional telemetry (RX - Energy + Capacity)
        // ...additionally with main telemetry data there will be 2 additional frames...
        // RX: F0 A1 D9 04 9B D6 34 00 81 (MeasuredCapacity in Ah)
        // RX: F0 A1 DA 04 CF AE 28 35 B8 (MeasuredEnergy in Wh)
        // These frames are now parsed by ParseTelemetryFrames() and accessible via public properties

        // Telemetry on change
        // Some frames are automatically sent by device on register change (no need to request it)

        /// <summary>
        /// Gets the current running mode of the device.
        /// </summary>
        /// <remarks>
        /// false = STOP (output disabled), true = RUN (output enabled)
        /// This value is updated automatically when telemetry frames are parsed.
        /// </remarks>
        public bool RunningMode => _runningMode;

        /// <summary>
        /// Gets the current protection mode status.
        /// </summary>
        /// <remarks>
        /// Indicates which protection (if any) has been triggered:
        /// OK=0, OVP=1, OCP=2, OPP=3, OTP=4, LVP=5, REP=6
        /// This value is updated automatically when telemetry frames are parsed.
        /// </remarks>
        public DPS150ProtectionMode ProtectionMode => _protectionMode;

        /// <summary>
        /// Gets the current CC/CV mode.
        /// </summary>
        /// <remarks>
        /// false = CC (Constant Current mode), true = CV (Constant Voltage mode)
        /// This value is updated automatically when telemetry frames are parsed.
        /// </remarks>
        public bool CCCV => _cccv;
    }
}