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

namespace FNIRSI_DPS_150_CONTROL
{
    /// <summary>
    /// High-level control class for the FNIRSI-DPS-150 laboratory power supply.
    /// This class is a facade/wrapper around DPS150Registers, providing a backward-compatible
    /// API while delegating all operations to the property-based register abstraction.
    /// </summary>
    /// <remarks>
    /// This class provides:
    /// - Connection management and session control
    /// - Output voltage and current control
    /// - Protection settings (OVP, OCP, OPP, OTP, LVP)
    /// - Preset memory management (M1-M6)
    /// - UI settings (brightness, volume)
    /// - Telemetry and status monitoring
    /// 
    /// All operations are delegated to the underlying DPS150Registers instance,
    /// which provides a modern property-based API with automatic read-back verification.
    /// </remarks>
    public class DPS150Control
    {
        #region Private Fields

        /// <summary>
        /// High-level register abstraction layer for the DPS-150 device.
        /// Manages device state, register operations, and communication.
        /// </summary>
        private readonly DPS150Registers _registers = new DPS150Registers();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets an array of available serial port names on the system.
        /// </summary>
        public string[] AvailablePorts => DPS150Registers.GetAvailablePorts();

        /// <summary>
        /// Gets a value indicating whether the serial port connection is currently open.
        /// </summary>
        public bool IsConnected => _registers.IsConnected;

        /// <summary>
        /// Gets a value indicating whether a communication session is currently active.
        /// </summary>
        public bool IsSessionStarted => _registers.IsSessionStarted;

        #endregion

        #region Connection & Session Management

        /// <summary>
        /// Connects to the DPS-150 device on the specified serial port.
        /// </summary>
        /// <param name="portName">The name of the COM port (e.g., "COM3").</param>
        /// <returns>True if connection was successful.</returns>
        public bool ConnectToDevice(string portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
                return false;

            // Try as port name first
            if (_registers._communication.Connect(portName))
                return true;

            // If that fails, try as port number
            if (int.TryParse(portName, out int portNumber))
            {
                return _registers.ConnectToDevice(portNumber);
            }

            return false;
        }

        /// <summary>
        /// Connects to the DPS-150 device using a port number (1-based index).
        /// </summary>
        /// <param name="portNumber">Port number (1 = first available port).</param>
        /// <returns>True if connection was successful.</returns>
        public bool ConnectToDevice(int portNumber)
        {
            return _registers.ConnectToDevice(portNumber);
        }

        /// <summary>
        /// Disconnects from the DPS-150 device and closes the serial port.
        /// </summary>
        public void DisconnectFromDevice()
        {
            _registers.DisconnectFromDevice();
        }

        /// <summary>
        /// Starts a communication session with the device.
        /// </summary>
        /// <returns>True if session started successfully.</returns>
        public bool StartSession()
        {
            return _registers.StartSession();
        }

        /// <summary>
        /// Stops the communication session with the device.
        /// </summary>
        /// <returns>True if session stopped successfully.</returns>
        public bool StopSession()
        {
            return _registers.StopSession();
        }

        /// <summary>
        /// Flushes the serial port buffers.
        /// </summary>
        /// <returns>True if successful (always returns true for compatibility).</returns>
        public bool FlushBuffers()
        {
            _registers.FlushBuffers();
            return true;
        }

        /// <summary>
        /// Sends raw byte data to the device.
        /// </summary>
        /// <param name="data">Data to send.</param>
        /// <returns>True if successful.</returns>
        public bool SendData(byte[]? data)
        {
            if (data == null || data.Length == 0 || !IsConnected)
                return false;

            try
            {
                return _registers._communication.SendData(data);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reads response data from the device.
        /// </summary>
        /// <param name="timeoutMs">Timeout in milliseconds.</param>
        /// <returns>Received data or null.</returns>
        public byte[]? ReadResponse(int timeoutMs = 1000)
        {
            if (!IsConnected)
                return null;

            try
            {
                return _registers._communication.ReadResponse(timeoutMs);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Sends a command and gets the response.
        /// </summary>
        /// <param name="data">Command data.</param>
        /// <param name="timeoutMs">Timeout in milliseconds.</param>
        /// <returns>Response data or null.</returns>
        public byte[]? SendCommandAndGetResponse(byte[]? data, int timeoutMs = 1000)
        {
            if (data == null || data.Length == 0 || !IsConnected)
                return null;

            try
            {
                return _registers._communication.SendDataAndGetResponse(data, timeoutMs);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Output Control

        /// <summary>
        /// Sets the output relay state (RUN/STOP).
        /// </summary>
        /// <param name="state">Desired relay state.</param>
        /// <returns>True if successful.</returns>
        public bool SetOutputRelay(OutputRelayState state)
        {
            try
            {
                _registers.OutputRelayState = (state == OutputRelayState.ON);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the voltage setpoint.
        /// </summary>
        /// <param name="voltage">Voltage in volts (0.0 - 150.0V).</param>
        /// <returns>True if successful.</returns>
        public bool SetVoltage(float voltage)
        {
            try
            {
                _registers.VoltageSetpoint = voltage;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the current voltage setpoint.
        /// </summary>
        /// <returns>Voltage in volts.</returns>
        public float GetVoltage()
        {
            return _registers.VoltageSetpoint;
        }

        /// <summary>
        /// Sets the current limit.
        /// </summary>
        /// <param name="current">Current in amperes (0.0 - 15.0A).</param>
        /// <returns>True if successful.</returns>
        public bool SetCurrent(float current)
        {
            try
            {
                _registers.CurrentLimit = current;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the current limit.
        /// </summary>
        /// <returns>Current in amperes.</returns>
        public float GetCurrent()
        {
            return _registers.CurrentLimit;
        }

        #endregion

        #region Preset Management

        /// <summary>
        /// Sets a preset memory slot (M1-M6).
        /// </summary>
        /// <param name="presetNumber">Preset number (1-6).</param>
        /// <param name="voltage">Voltage in volts.</param>
        /// <param name="current">Current in amperes.</param>
        /// <returns>True if successful.</returns>
        public bool SetPreset(int presetNumber, float voltage, float current)
        {
            try
            {
                switch (presetNumber)
                {
                    case 1:
                        _registers.PresetM1Voltage = voltage;
                        _registers.PresetM1Current = current;
                        break;
                    case 2:
                        _registers.PresetM2Voltage = voltage;
                        _registers.PresetM2Current = current;
                        break;
                    case 3:
                        _registers.PresetM3Voltage = voltage;
                        _registers.PresetM3Current = current;
                        break;
                    case 4:
                        _registers.PresetM4Voltage = voltage;
                        _registers.PresetM4Current = current;
                        break;
                    case 5:
                        _registers.PresetM5Voltage = voltage;
                        _registers.PresetM5Current = current;
                        break;
                    case 6:
                        _registers.PresetM6Voltage = voltage;
                        _registers.PresetM6Current = current;
                        break;
                    default:
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a preset memory slot (M1-M6).
        /// </summary>
        /// <param name="presetNumber">Preset number (1-6).</param>
        /// <param name="voltage">Output: voltage in volts.</param>
        /// <param name="current">Output: current in amperes.</param>
        /// <returns>True if successful.</returns>
        public bool GetPreset(int presetNumber, out float voltage, out float current)
        {
            voltage = 0;
            current = 0;

            try
            {
                switch (presetNumber)
                {
                    case 1:
                        voltage = _registers.PresetM1Voltage;
                        current = _registers.PresetM1Current;
                        break;
                    case 2:
                        voltage = _registers.PresetM2Voltage;
                        current = _registers.PresetM2Current;
                        break;
                    case 3:
                        voltage = _registers.PresetM3Voltage;
                        current = _registers.PresetM3Current;
                        break;
                    case 4:
                        voltage = _registers.PresetM4Voltage;
                        current = _registers.PresetM4Current;
                        break;
                    case 5:
                        voltage = _registers.PresetM5Voltage;
                        current = _registers.PresetM5Current;
                        break;
                    case 6:
                        voltage = _registers.PresetM6Voltage;
                        current = _registers.PresetM6Current;
                        break;
                    default:
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Protection Settings

        /// <summary>
        /// Sets Over-Voltage Protection (OVP) threshold.
        /// </summary>
        public bool SetOVP(float ovp)
        {
            try { _registers.OVP = ovp; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets Over-Voltage Protection (OVP) threshold.
        /// </summary>
        public float GetOVP() => _registers.OVP;

        /// <summary>
        /// Sets Over-Current Protection (OCP) threshold.
        /// </summary>
        public bool SetOCP(float ocp)
        {
            try { _registers.OCP = ocp; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets Over-Current Protection (OCP) threshold.
        /// </summary>
        public float GetOCP() => _registers.OCP;

        /// <summary>
        /// Sets Over-Power Protection (OPP) threshold.
        /// </summary>
        public bool SetOPP(float opp)
        {
            try { _registers.OPP = opp; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets Over-Power Protection (OPP) threshold.
        /// </summary>
        public float GetOPP() => _registers.OPP;

        /// <summary>
        /// Sets Over-Temperature Protection (OTP) threshold.
        /// </summary>
        public bool SetOTP(float otp)
        {
            try { _registers.OTP = otp; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets Over-Temperature Protection (OTP) threshold.
        /// </summary>
        public float GetOTP() => _registers.OTP;

        /// <summary>
        /// Sets Low-Voltage Protection (LVP) threshold.
        /// </summary>
        public bool SetLVP(float lvp)
        {
            try { _registers.LVP = lvp; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets Low-Voltage Protection (LVP) threshold.
        /// </summary>
        public float GetLVP() => _registers.LVP;

        #endregion

        #region UI Settings

        /// <summary>
        /// Sets the display brightness.
        /// </summary>
        /// <param name="brightness">Brightness level (0-100).</param>
        public bool SetBrightness(int brightness)
        {
            try { _registers.Brightness = (byte)brightness; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets the display brightness.
        /// </summary>
        public int GetBrightness() => _registers.Brightness;

        /// <summary>
        /// Sets the audio volume.
        /// </summary>
        /// <param name="volume">Volume level (0-100).</param>
        public bool SetVolume(int volume)
        {
            try { _registers.Volume = (byte)volume; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets the audio volume.
        /// </summary>
        public int GetVolume() => _registers.Volume;

        #endregion

        #region Telemetry & Monitoring

        /// <summary>
        /// Gets the input voltage.
        /// </summary>
        public float GetInputVoltage() => _registers.InputVoltage;

        /// <summary>
        /// Gets the maximum voltage setting.
        /// </summary>
        public float GetMaximumVoltage() => _registers.MaximumVoltage;

        /// <summary>
        /// Gets the maximum current setting.
        /// </summary>
        public float GetMaximumCurrent() => _registers.MaximumCurrent;

        /// <summary>
        /// Gets the internal temperature.
        /// </summary>
        public float GetInternalTemperature() => _registers.InternalTemperature;

        /// <summary>
        /// Gets measurement values (voltage, current, power).
        /// </summary>
        /// <returns>Array: [voltage, current, power]</returns>
        public float[]? GetMeasurement()
        {
            return new float[] 
            { 
                _registers.MeasuredVoltage, 
                _registers.MeasuredCurrent, 
                _registers.MeasuredPower 
            };
        }

        /// <summary>
        /// Gets the measured capacity.
        /// </summary>
        public float GetMeasuredCapacity() => _registers.MeasuredCapacity;

        /// <summary>
        /// Gets the measured energy.
        /// </summary>
        public float GetMeasuredEnergy() => _registers.MeasuredEnergy;

        /// <summary>
        /// Gets the running mode (STOP/RUN).
        /// </summary>
        public bool? GetRunningMode() => _registers.RunningMode;

        /// <summary>
        /// Gets the CC/CV mode.
        /// </summary>
        public bool? GetCCCV() => _registers.CCCV;

        #endregion

        #region Static Utility Methods (Protocol Level)

        /// <summary>
        /// Calculates the checksum for a DPS-150 protocol packet.
        /// </summary>
        /// <param name="data">Packet data (minimum 4 bytes).</param>
        /// <returns>Checksum byte (CHK = sum(DATA[2..n]) & 0xFF).</returns>
        public static byte CalculateChecksum(byte[] data)
        {
            if (data == null || data.Length < 4)
            {
                throw new ArgumentException("Data must contain at least 4 bytes.");
            }

            int sum = 0;
            for (int i = 2; i < data.Length; i++)
            {
                sum += data[i];
            }
            return (byte)(sum & 0xFF);
        }

        /// <summary>
        /// Verifies if a packet has a valid checksum.
        /// </summary>
        /// <param name="packet">Complete packet including checksum as last byte.</param>
        /// <returns>True if checksum is valid.</returns>
        public static bool VerifyChecksum(byte[] packet)
        {
            if (packet == null || packet.Length < 5)
            {
                return false;
            }

            byte receivedChecksum = packet[packet.Length - 1];
            int dataLength = packet.Length - 3;

            int sum = 0;
            for (int i = 2; i < 2 + dataLength; i++)
            {
                sum += packet[i];
            }

            byte calculatedChecksum = (byte)(sum & 0xFF);
            return receivedChecksum == calculatedChecksum;
        }

        #endregion
    }
}
