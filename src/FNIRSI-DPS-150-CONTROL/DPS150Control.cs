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
    /// Inherits from DPS150Registers and provides additional convenience methods 
    /// with simplified return types (bool for success/failure) for easier integration.
    /// </summary>
    /// <remarks>
    /// This class extends DPS150Registers with:
    /// - Simplified connection management (string-based port names or numbers)
    /// - Convenience methods with bool return values for error handling
    /// - Backward-compatible API for existing applications
    /// - Additional helper methods for common operations
    /// 
    /// All DPS150Registers properties and methods are directly accessible:
    /// - Connection management and session control
    /// - Output voltage and current control via properties
    /// - Protection settings (OVP, OCP, OPP, OTP, LVP)
    /// - Preset memory management (M1-M6)
    /// - UI settings (brightness, volume)
    /// - Telemetry and status monitoring
    /// - TelemetryUpdated event for real-time data updates
    /// </remarks>
    public class DPS150Control : DPS150Registers
    {
        #region Public Properties

        /// <summary>
        /// Gets an array of available serial port names on the system.
        /// </summary>
        public string[] AvailablePorts => DPS150Registers.GetAvailablePorts();

        #endregion

        #region Connection & Session Management

        /// <summary>
        /// Connects to the DPS-150 device on the specified serial port.
        /// </summary>
        /// <param name="portName">The name of the COM port (e.g., "COM3") or a port number as string.</param>
        /// <returns>True if connection was successful.</returns>
        /// <remarks>
        /// This method provides flexible port selection:
        /// - If portName is a COM port name (e.g., "COM3"), connects directly to that port
        /// - If portName is a numeric string (e.g., "1", "2"), connects using port index
        /// </remarks>
        public bool ConnectToDevice(string portName)
        {
            if (string.IsNullOrWhiteSpace(portName))
                return false;

            // Try as port name first
            if (_communication.Connect(portName))
                return true;

            // If that fails, try as port number
            if (int.TryParse(portName, out int portNumber))
            {
                return base.ConnectToDevice(portNumber);
            }

            return false;
        }

        /// <summary>
        /// Flushes the serial port buffers.
        /// </summary>
        /// <returns>True if successful (always returns true for compatibility).</returns>
        public new bool FlushBuffers()
        {
            base.FlushBuffers();
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
                return _communication.SendData(data);
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
                return _communication.ReadResponse(timeoutMs);
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
                return _communication.SendDataAndGetResponse(data, timeoutMs);
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
                base.OutputRelayState = (state == FNIRSI_DPS_150_CONTROL.OutputRelayState.ON);
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
        /// <param name="voltage">Voltage in volts (0.0 - 30.0V).</param>
        /// <returns>True if successful.</returns>
        public bool SetVoltage(float voltage)
        {
            try
            {
                VoltageSetpoint = voltage;
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
            return VoltageSetpoint;
        }

        /// <summary>
        /// Sets the current limit.
        /// </summary>
        /// <param name="current">Current in amperes (0.0 - 5.0A).</param>
        /// <returns>True if successful.</returns>
        public bool SetCurrent(float current)
        {
            try
            {
                CurrentLimit = current;
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
            return CurrentLimit;
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
                        PresetM1Voltage = voltage;
                        PresetM1Current = current;
                        break;
                    case 2:
                        PresetM2Voltage = voltage;
                        PresetM2Current = current;
                        break;
                    case 3:
                        PresetM3Voltage = voltage;
                        PresetM3Current = current;
                        break;
                    case 4:
                        PresetM4Voltage = voltage;
                        PresetM4Current = current;
                        break;
                    case 5:
                        PresetM5Voltage = voltage;
                        PresetM5Current = current;
                        break;
                    case 6:
                        PresetM6Voltage = voltage;
                        PresetM6Current = current;
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
                        voltage = PresetM1Voltage;
                        current = PresetM1Current;
                        break;
                    case 2:
                        voltage = PresetM2Voltage;
                        current = PresetM2Current;
                        break;
                    case 3:
                        voltage = PresetM3Voltage;
                        current = PresetM3Current;
                        break;
                    case 4:
                        voltage = PresetM4Voltage;
                        current = PresetM4Current;
                        break;
                    case 5:
                        voltage = PresetM5Voltage;
                        current = PresetM5Current;
                        break;
                    case 6:
                        voltage = PresetM6Voltage;
                        current = PresetM6Current;
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
            try { OVP = ovp; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets Over-Voltage Protection (OVP) threshold.
        /// </summary>
        public float GetOVP() => OVP;

        /// <summary>
        /// Sets Over-Current Protection (OCP) threshold.
        /// </summary>
        public bool SetOCP(float ocp)
        {
            try { OCP = ocp; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets Over-Current Protection (OCP) threshold.
        /// </summary>
        public float GetOCP() => OCP;

        /// <summary>
        /// Sets Over-Power Protection (OPP) threshold.
        /// </summary>
        public bool SetOPP(float opp)
        {
            try { OPP = opp; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets Over-Power Protection (OPP) threshold.
        /// </summary>
        public float GetOPP() => OPP;

        /// <summary>
        /// Sets Over-Temperature Protection (OTP) threshold.
        /// </summary>
        public bool SetOTP(float otp)
        {
            try { OTP = otp; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets Over-Temperature Protection (OTP) threshold.
        /// </summary>
        public float GetOTP() => OTP;

        /// <summary>
        /// Sets Low-Voltage Protection (LVP) threshold.
        /// </summary>
        public bool SetLVP(float lvp)
        {
            try { LVP = lvp; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets Low-Voltage Protection (LVP) threshold.
        /// </summary>
        public float GetLVP() => LVP;

        #endregion

        #region UI Settings

        /// <summary>
        /// Sets the display brightness.
        /// </summary>
        /// <param name="brightness">Brightness level (0-100).</param>
        public bool SetBrightness(int brightness)
        {
            try { Brightness = (byte)brightness; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets the display brightness.
        /// </summary>
        public int GetBrightness() => Brightness;

        /// <summary>
        /// Sets the audio volume.
        /// </summary>
        /// <param name="volume">Volume level (0-100).</param>
        public bool SetVolume(int volume)
        {
            try { Volume = (byte)volume; return true; } catch { return false; }
        }

        /// <summary>
        /// Gets the audio volume.
        /// </summary>
        public int GetVolume() => Volume;

        #endregion

        #region Telemetry & Monitoring

        /// <summary>
        /// Gets the input voltage.
        /// </summary>
        public float GetInputVoltage() => InputVoltage;

        /// <summary>
        /// Gets the maximum voltage setting.
        /// </summary>
        public float GetMaximumVoltage() => MaximumVoltage;

        /// <summary>
        /// Gets the maximum current setting.
        /// </summary>
        public float GetMaximumCurrent() => MaximumCurrent;

        /// <summary>
        /// Gets the internal temperature.
        /// </summary>
        public float GetInternalTemperature() => InternalTemperature;

        /// <summary>
        /// Gets measurement values (voltage, current, power).
        /// </summary>
        /// <returns>Array: [voltage, current, power]</returns>
        public float[]? GetMeasurement()
        {
            return new float[] 
            { 
                MeasuredVoltage, 
                MeasuredCurrent, 
                MeasuredPower 
            };
        }

        /// <summary>
        /// Gets the measured capacity.
        /// </summary>
        public float GetMeasuredCapacity() => MeasuredCapacity;

        /// <summary>
        /// Gets the measured energy.
        /// </summary>
        public float GetMeasuredEnergy() => MeasuredEnergy;

        /// <summary>
        /// Gets the running mode (STOP/RUN).
        /// </summary>
        public bool? GetRunningMode() => RunningMode;

        /// <summary>
        /// Gets the CC/CV mode.
        /// </summary>
        public bool? GetCCCV() => CCCV;

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
