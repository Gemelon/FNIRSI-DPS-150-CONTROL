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

using FNIRSI_DPS_150_CONTROL;
using System;

namespace FNIRSI_DPS_150_CONTROL
{
    /// <summary>
    /// Comprehensive test application for DPS150Control class.
    /// Tests all implemented methods and features.
    /// </summary>
    class TestProgram
    {
        private static DPS150Control? _control;
        private static bool _isRunning = true;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     FNIRSI-DPS-150 Control Test Application                 ║");
            Console.WriteLine("║     Comprehensive Testing for DPS150Control Class            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            _control = new DPS150Control();

            while (_isRunning)
            {
                ShowMenu();
                ProcessUserInput();
            }

            // Clean up
            if (_control.IsConnected)
            {
                _control.DisconnectFromDevice();
            }

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     Application terminated. Press any key to exit...         ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ReadKey();
        }

        /// <summary>
        /// Displays the main menu with all available test options.
        /// </summary>
        private static void ShowMenu()
        {
            Console.WriteLine("\n" + new string('═', 62));
            Console.WriteLine($"│ Connection: {(_control?.IsConnected == true ? "✓ CONNECTED" : "✗ DISCONNECTED"),-30} │");
            Console.WriteLine($"│ Session:    {(_control?.IsSessionStarted == true ? "✓ ACTIVE" : "✗ INACTIVE"),-30} │");
            Console.WriteLine(new string('═', 62));
            Console.WriteLine("│  CONNECTION & SESSION MANAGEMENT                            │");
            Console.WriteLine("│  [1] List Available Ports                                   │");
            Console.WriteLine("│  [2] Connect to Device (by port name)                       │");
            Console.WriteLine("│  [3] Connect to Device (by port number)                     │");
            Console.WriteLine("│  [4] Disconnect from Device                                 │");
            Console.WriteLine("│  [5] Start Session                                          │");
            Console.WriteLine("│  [6] Stop Session                                           │");
            Console.WriteLine("│  [L] Flush Communication Buffers                            │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  DATA COMMUNICATION                                          │");
            Console.WriteLine("│  [7] Send Raw Data (custom packet)                          │");
            Console.WriteLine("│  [8] Read Response                                          │");
            Console.WriteLine("│  [9] Send Command and Get Response                          │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  DEVICE CONTROL                                              │");
            Console.WriteLine("│  [A] Set Output Relay ON                                    │");
            Console.WriteLine("│  [B] Set Output Relay OFF                                   │");
            Console.WriteLine("│  [V] Set Voltage                                            │");
            Console.WriteLine("│  [I] Set Current Limit                                      │");
            Console.WriteLine("│  [G] Get Voltage Setpoint                                   │");
            Console.WriteLine("│  [J] Get Current Limit Setpoint                             │");
            Console.WriteLine("│  [P] Set Preset (M1-M6)                                     │");
            Console.WriteLine("│  [R] Get Preset (M1-M6)                                     │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  PROTECTION SETTINGS                                         │");
            Console.WriteLine("│  [O] Configure Protection Settings (OVP/OCP/OPP/OTP/LVP)   │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  UI SETTINGS                                                 │");
            Console.WriteLine("│  [U] Configure UI Settings (Brightness/Volume)              │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  TELEMETRY & STATUS                                          │");
            Console.WriteLine("│  [M] Monitor Device Telemetry                               │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  CHECKSUM UTILITIES                                          │");
            Console.WriteLine("│  [C] Calculate Checksum (from packet)                       │");
            Console.WriteLine("│  [D] Verify Checksum (validate packet)                      │");
            Console.WriteLine("│  [E] Test All Checksum Examples                             │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  COMPREHENSIVE TESTS                                         │");
            Console.WriteLine("│  [T] Run Complete Test Suite                                │");
            Console.WriteLine("│  [F] Run Full Device Control Test                           │");
            Console.WriteLine("│  [X] Test Float Endianness (IEEE-754)                       │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  [0] Exit Application                                       │");
            Console.WriteLine(new string('═', 62));
            Console.Write("Select option: ");
        }

        /// <summary>
        /// Processes user input and executes the selected menu option.
        /// </summary>
        private static void ProcessUserInput()
        {
            string? input = Console.ReadLine()?.ToUpper();

            Console.WriteLine(); // Blank line for readability

            switch (input)
            {
                case "1":
                    ListAvailablePorts();
                    break;
                case "2":
                    ConnectByPortName();
                    break;
                case "3":
                    ConnectByPortNumber();
                    break;
                case "4":
                    DisconnectFromDevice();
                    break;
                case "5":
                    StartSession();
                    break;
                case "6":
                    StopSession();
                    break;
                case "L":
                    FlushBuffers();
                    break;
                case "7":
                    SendRawData();
                    break;
                case "8":
                    ReadResponse();
                    break;
                case "9":
                    SendCommandAndGetResponse();
                    break;
                case "A":
                    SetOutputRelayOn();
                    break;
                case "B":
                    SetOutputRelayOff();
                    break;
                case "V":
                    SetVoltage();
                    break;
                case "I":
                    SetCurrent();
                    break;
                case "G":
                    GetVoltage();
                    break;
                case "J":
                    GetCurrent();
                    break;
                case "P":
                    SetPreset();
                    break;
                case "R":
                    GetPreset();
                    break;
                case "O":
                    ConfigureProtectionSettings();
                    break;
                case "U":
                    ConfigureUISettings();
                    break;
                case "M":
                    MonitorTelemetry();
                    break;
                case "C":
                    CalculateChecksum();
                    break;
                case "D":
                    VerifyChecksum();
                    break;
                case "E":
                    TestAllChecksums();
                    break;
                case "T":
                    RunCompleteTestSuite();
                    break;
                case "F":
                    RunFullDeviceControlTest();
                    break;
                case "X":
                    FloatEndiannessTest.RunTest();
                    break;
                case "0":
                    _isRunning = false;
                    break;
                default:
                    Console.WriteLine("⚠ Invalid option! Please try again.");
                    break;
            }

            if (_isRunning)
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        #region Connection & Session Management

        private static void ListAvailablePorts()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ LIST AVAILABLE SERIAL PORTS                             │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            string[] ports = _control!.AvailablePorts;

            if (ports.Length == 0)
            {
                Console.WriteLine("✗ No serial ports found!");
            }
            else
            {
                Console.WriteLine($"✓ Found {ports.Length} serial port(s):");
                for (int i = 0; i < ports.Length; i++)
                {
                    Console.WriteLine($"  [{i + 1}] {ports[i]}");
                }
            }
        }

        private static void ConnectByPortName()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ CONNECT TO DEVICE (by port name)                        │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            ListAvailablePorts();
            Console.WriteLine();
            Console.Write("Enter port name (e.g., COM3) or number: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("✗ Invalid input!");
                return;
            }

            Console.WriteLine($"\nAttempting to connect to: {input}");
            if (_control!.ConnectToDevice(input))
            {
                Console.WriteLine($"✓ Successfully connected to {input}");
                Console.WriteLine($"  Connection Status: {_control.IsConnected}");
            }
            else
            {
                Console.WriteLine($"✗ Failed to connect to {input}");
            }
        }

        private static void ConnectByPortNumber()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ CONNECT TO DEVICE (by port number)                      │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            ListAvailablePorts();
            Console.WriteLine();
            Console.Write("Enter port number (1-N): ");
            string? input = Console.ReadLine();

            if (!int.TryParse(input, out int portNumber))
            {
                Console.WriteLine("✗ Invalid port number!");
                return;
            }

            Console.WriteLine($"\nAttempting to connect to port #{portNumber}");
            if (_control!.ConnectToDevice(portNumber))
            {
                Console.WriteLine($"✓ Successfully connected to port #{portNumber}");
                Console.WriteLine($"  Connection Status: {_control.IsConnected}");
            }
            else
            {
                Console.WriteLine($"✗ Failed to connect to port #{portNumber}");
            }
        }

        private static void DisconnectFromDevice()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ DISCONNECT FROM DEVICE                                  │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!_control!.IsConnected)
            {
                Console.WriteLine("⚠ Not connected to any device!");
                return;
            }

            Console.WriteLine("Disconnecting from device...");
            _control.DisconnectFromDevice();
            Console.WriteLine("✓ Disconnected successfully");
            Console.WriteLine($"  Connection Status: {_control.IsConnected}");
            Console.WriteLine($"  Session Status: {_control.IsSessionStarted}");
        }

        private static void StartSession()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ START SESSION                                           │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!_control!.IsConnected)
            {
                Console.WriteLine("✗ Not connected! Please connect to a device first.");
                return;
            }

            if (_control.IsSessionStarted)
            {
                Console.WriteLine("⚠ Session is already active!");
                return;
            }

            Console.WriteLine("Starting session...");
            if (_control.StartSession())
            {
                Console.WriteLine("✓ Session started successfully");
                Console.WriteLine($"  Session Status: {_control.IsSessionStarted}");
            }
            else
            {
                Console.WriteLine("✗ Failed to start session");
            }
        }

        private static void StopSession()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ STOP SESSION                                            │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!_control!.IsConnected)
            {
                Console.WriteLine("✗ Not connected! Please connect to a device first.");
                return;
            }

            if (!_control.IsSessionStarted)
            {
                Console.WriteLine("⚠ No active session!");
                return;
            }

            Console.WriteLine("Stopping session...");
            if (_control.StopSession())
            {
                Console.WriteLine("✓ Session stopped successfully");
                Console.WriteLine($"  Session Status: {_control.IsSessionStarted}");
            }
            else
            {
                        Console.WriteLine("✗ Failed to stop session");
                    }
                }

                /// <summary>
                /// Tests the FlushBuffers method by clearing all communication buffers.
                /// </summary>
                private static void FlushBuffers()
                {
                    Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
                    Console.WriteLine("│ FLUSH COMMUNICATION BUFFERS                             │");
                    Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                    if (!_control!.IsConnected)
                    {
                        Console.WriteLine("✗ Not connected! Please connect to a device first.");
                        return;
                    }

                    Console.WriteLine("Flushing input and output buffers...");
                    Console.WriteLine("This will clear:");
                    Console.WriteLine("  • Input buffer: Discards any unread received data");
                    Console.WriteLine("  • Output buffer: Discards any unsent transmission data");

                    if (_control.FlushBuffers())
                    {
                        Console.WriteLine("✓ Buffers flushed successfully");
                        Console.WriteLine("  ℹ All pending data has been discarded");
                        Console.WriteLine("  ℹ Communication state is now clean");
                    }
                    else
                    {
                        Console.WriteLine("✗ Failed to flush buffers");
                        Console.WriteLine("  Possible reasons:");
                        Console.WriteLine("  - Not connected to device");
                        Console.WriteLine("  - Serial port error");
                    }
                }

                #endregion

        #region Data Communication

        private static void SendRawData()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ SEND RAW DATA                                           │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!_control!.IsConnected)
            {
                Console.WriteLine("✗ Not connected! Please connect to a device first.");
                return;
            }

            Console.WriteLine("Enter hex bytes (space-separated, e.g., F1 B1 DB 01 01 DD):");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("✗ Invalid input!");
                return;
            }

            try
            {
                string[] hexBytes = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                byte[] data = hexBytes.Select(h => Convert.ToByte(h, 16)).ToArray();

                Console.WriteLine($"\nSending {data.Length} bytes: {BitConverter.ToString(data)}");
                if (_control.SendData(data))
                {
                    Console.WriteLine("✓ Data sent successfully");
                }
                else
                {
                    Console.WriteLine("✗ Failed to send data");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        private static void ReadResponse()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ READ RESPONSE                                           │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!_control!.IsConnected)
            {
                Console.WriteLine("✗ Not connected! Please connect to a device first.");
                return;
            }

            Console.Write("Enter timeout in milliseconds (default: 1000): ");
            string? input = Console.ReadLine();
            int timeout = string.IsNullOrWhiteSpace(input) ? 1000 : int.Parse(input);

            Console.WriteLine($"\nReading response (timeout: {timeout}ms)...");
            byte[]? response = _control.ReadResponse(timeout);

            if (response != null && response.Length > 0)
            {
                Console.WriteLine($"✓ Received {response.Length} bytes:");
                Console.WriteLine($"  Hex: {BitConverter.ToString(response)}");

                if (DPS150Control.VerifyChecksum(response))
                {
                    Console.WriteLine("  Checksum: ✓ VALID");
                }
                else
                {
                    Console.WriteLine("  Checksum: ✗ INVALID");
                }
            }
            else
            {
                Console.WriteLine("✗ No response received (timeout)");
            }
        }

        private static void SendCommandAndGetResponse()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ SEND COMMAND AND GET RESPONSE                           │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!_control!.IsConnected)
            {
                Console.WriteLine("✗ Not connected! Please connect to a device first.");
                return;
            }

            Console.WriteLine("Enter command hex bytes (space-separated):");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("✗ Invalid input!");
                return;
            }

            try
            {
                string[] hexBytes = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                byte[] command = hexBytes.Select(h => Convert.ToByte(h, 16)).ToArray();

                Console.Write("Enter timeout in milliseconds (default: 1000): ");
                string? timeoutInput = Console.ReadLine();
                int timeout = string.IsNullOrWhiteSpace(timeoutInput) ? 1000 : int.Parse(timeoutInput);

                Console.WriteLine($"\nSending {command.Length} bytes and waiting for response...");
                Console.WriteLine($"Command: {BitConverter.ToString(command)}");

                byte[]? response = _control.SendCommandAndGetResponse(command, timeout);

                if (response != null && response.Length > 0)
                {
                    Console.WriteLine($"✓ Received {response.Length} bytes:");
                    Console.WriteLine($"  Response: {BitConverter.ToString(response)}");

                    if (DPS150Control.VerifyChecksum(response))
                    {
                        Console.WriteLine("  Checksum: ✓ VALID");
                    }
                    else
                    {
                        Console.WriteLine("  Checksum: ✗ INVALID");
                    }
                }
                else
                {
                    Console.WriteLine("✗ No response received");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        #endregion

        #region Device Control

        private static void SetOutputRelayOn()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ SET OUTPUT RELAY ON                                     │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!_control!.IsConnected)
            {
                Console.WriteLine("✗ Not connected! Please connect to a device first.");
                return;
            }

            Console.WriteLine("Turning output relay ON...");
            Console.WriteLine("Command packet: F1 B1 DB 01 01 DD");

            if (_control.SetOutputRelay(OutputRelayState.ON))
            {
                Console.WriteLine("✓ Output relay turned ON successfully");
                Console.WriteLine("  Session Status: " + (_control.IsSessionStarted ? "✓ ACTIVE" : "✗ INACTIVE"));
                Console.WriteLine("  ⚠ Output is now ENABLED - power supply is running!");
            }
            else
            {
                Console.WriteLine("✗ Failed to turn output relay ON");
            }
        }

        private static void SetOutputRelayOff()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ SET OUTPUT RELAY OFF                                    │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!_control!.IsConnected)
            {
                Console.WriteLine("✗ Not connected! Please connect to a device first.");
                return;
            }

            Console.WriteLine("Turning output relay OFF...");
            Console.WriteLine("Command packet: F1 B1 DB 01 00 DC");

            if (_control.SetOutputRelay(OutputRelayState.OFF))
            {
                Console.WriteLine("✓ Output relay turned OFF successfully");
                Console.WriteLine("  Session Status: " + (_control.IsSessionStarted ? "✓ ACTIVE" : "✗ INACTIVE"));
                Console.WriteLine("  ✓ Output is now DISABLED - power supply is stopped");
            }
            else
            {
                Console.WriteLine("✗ Failed to turn output relay OFF");
            }
        }

        /// <summary>
        /// Tests the SetVoltage method by allowing the user to set a voltage value.
        /// </summary>
        private static void SetVoltage()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ SET VOLTAGE                                             │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!_control!.IsConnected)
            {
                Console.WriteLine("✗ Not connected! Please connect to a device first.");
                return;
            }

            Console.Write("Enter voltage value (e.g., 12.5): ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("✗ Invalid input!");
                return;
            }

            try
            {
                float voltage = float.Parse(input, System.Globalization.CultureInfo.InvariantCulture);

                // Validate voltage range (typical DPS-150 range: 0-150V)
                if (voltage < 0 || voltage > 150)
                {
                    Console.WriteLine("⚠ Warning: Voltage out of typical range (0-150V)");
                    Console.Write("Continue anyway? (Y/N): ");
                    string? confirm = Console.ReadLine()?.ToUpper();
                    if (confirm != "Y")
                    {
                        Console.WriteLine("Operation cancelled.");
                        return;
                    }
                }

                Console.WriteLine($"\nSetting voltage to {voltage:F2}V...");
                Console.WriteLine("Command packet: F1 B1 C1 04 [float bytes] [checksum]");

                if (_control.SetVoltage(voltage))
                {
                    Console.WriteLine($"✓ Voltage set to {voltage:F2}V successfully");
                    Console.WriteLine("  Session Status: " + (_control.IsSessionStarted ? "✓ ACTIVE" : "✗ INACTIVE"));
                    Console.WriteLine("  ⚠ Note: Voltage setpoint updated. Enable output to apply.");
                }
                else
                {
                                Console.WriteLine($"✗ Failed to set voltage to {voltage:F2}V");
                            }
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("✗ Invalid voltage format! Please enter a valid number (e.g., 12.5)");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"✗ Error: {ex.Message}");
                        }
                    }

                    /// <summary>
                    /// Tests the SetCurrent method by allowing the user to set a current limit value.
                    /// </summary>
                    private static void SetCurrent()
                    {
                        Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
                        Console.WriteLine("│ SET CURRENT LIMIT                                       │");
                        Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                        if (!_control!.IsConnected)
                        {
                            Console.WriteLine("✗ Not connected! Please connect to a device first.");
                            return;
                        }

                        Console.Write("Enter current limit value in amperes (e.g., 2.5): ");
                        string? input = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(input))
                        {
                            Console.WriteLine("✗ Invalid input!");
                            return;
                        }

                        try
                        {
                            float current = float.Parse(input, System.Globalization.CultureInfo.InvariantCulture);

                            // Validate current range (typical DPS-150 range: 0-15A)
                            if (current < 0 || current > 15)
                            {
                                Console.WriteLine("⚠ Warning: Current out of typical range (0-15A)");
                                Console.Write("Continue anyway? (Y/N): ");
                                string? confirm = Console.ReadLine()?.ToUpper();
                                if (confirm != "Y")
                                {
                                    Console.WriteLine("Operation cancelled.");
                                    return;
                                }
                            }

                            Console.WriteLine($"\nSetting current limit to {current:F2}A...");
                            Console.WriteLine("Command packet: F1 B1 C2 04 [float bytes] [checksum]");

                            if (_control.SetCurrent(current))
                            {
                                Console.WriteLine($"✓ Current limit set to {current:F2}A successfully");
                                Console.WriteLine("  Session Status: " + (_control.IsSessionStarted ? "✓ ACTIVE" : "✗ INACTIVE"));
                                Console.WriteLine("  ⚠ Note: Current limit updated. Enable output to apply.");
                                Console.WriteLine("  ℹ In CC mode: This is the regulated current");
                                Console.WriteLine("  ℹ In CV mode: This is the maximum allowed current");
                            }
                            else
                            {
                                Console.WriteLine($"✗ Failed to set current limit to {current:F2}A");
                            }
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("✗ Invalid current format! Please enter a valid number (e.g., 2.5)");
                        }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"✗ Error: {ex.Message}");
                            }
                        }

                        /// <summary>
                        /// Tests the GetVoltage method by reading the voltage setpoint from the device.
                        /// </summary>
                        private static void GetVoltage()
                        {
                            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
                            Console.WriteLine("│ GET VOLTAGE SETPOINT                                    │");
                            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                            if (!_control!.IsConnected)
                            {
                                Console.WriteLine("✗ Not connected! Please connect to a device first.");
                                return;
                            }

                            Console.WriteLine("Reading voltage setpoint from device...");
                            Console.WriteLine("Command packet: F1 A1 C1 00 [checksum]");

                            float voltage = _control.GetVoltage();

                            if (voltage >= 0)
                            {
                                Console.WriteLine($"✓ Voltage setpoint read successfully: {voltage:F2}V");
                                Console.WriteLine("  Session Status: " + (_control.IsSessionStarted ? "✓ ACTIVE" : "✗ INACTIVE"));
                                Console.WriteLine("  ℹ This is the SETPOINT, not the measured output voltage");
                            }
                            else
                            {
                                Console.WriteLine("✗ Failed to read voltage setpoint");
                                Console.WriteLine("  Possible reasons:");
                                Console.WriteLine("  - Device not responding");
                                Console.WriteLine("  - Communication timeout");
                                Console.WriteLine("  - Invalid response packet");
                                        Console.WriteLine("  - Checksum mismatch");
                                    }
                                }

                                /// <summary>
                                /// Tests the GetCurrent method by reading the current limit setpoint from the device.
                                /// </summary>
                                private static void GetCurrent()
                                {
                                    Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
                                    Console.WriteLine("│ GET CURRENT LIMIT SETPOINT                              │");
                                    Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                                    if (!_control!.IsConnected)
                                    {
                                        Console.WriteLine("✗ Not connected! Please connect to a device first.");
                                        return;
                                    }

                                    Console.WriteLine("Reading current limit setpoint from device...");
                                    Console.WriteLine("Command packet: F1 A1 C2 00 [checksum]");

                                    float current = _control.GetCurrent();

                                    if (current >= 0)
                                    {
                                        Console.WriteLine($"✓ Current limit setpoint read successfully: {current:F2}A");
                                        Console.WriteLine("  Session Status: " + (_control.IsSessionStarted ? "✓ ACTIVE" : "✗ INACTIVE"));
                                        Console.WriteLine("  ℹ This is the SETPOINT, not the measured output current");
                                        Console.WriteLine("  ℹ In CC mode: This is the regulated current");
                                        Console.WriteLine("  ℹ In CV mode: This is the maximum allowed current");
                                    }
                                    else
                                    {
                                        Console.WriteLine("✗ Failed to read current limit setpoint");
                                        Console.WriteLine("  Possible reasons:");
                                        Console.WriteLine("  - Device not responding");
                                        Console.WriteLine("  - Communication timeout");
                                        Console.WriteLine("  - Invalid response packet");
                                                Console.WriteLine("  - Checksum mismatch");
                                            }
                                        }

                                        /// <summary>
                                        /// Tests the SetPreset method by allowing the user to configure a preset memory slot.
                                        /// </summary>
                                        private static void SetPreset()
                                        {
                                            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
                                            Console.WriteLine("│ SET PRESET (M1-M6)                                      │");
                                            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                                            if (!_control!.IsConnected)
                                            {
                                                Console.WriteLine("✗ Not connected! Please connect to a device first.");
                                                return;
                                            }

                                            Console.WriteLine("Available presets: M1, M2, M3, M4, M5, M6");
                                            Console.Write("Enter preset number (1-6): ");
                                            string? presetInput = Console.ReadLine();

                                            if (string.IsNullOrWhiteSpace(presetInput))
                                            {
                                                Console.WriteLine("✗ Invalid input!");
                                                return;
                                            }

                                            try
                                            {
                                                int presetNumber = int.Parse(presetInput);

                                                if (presetNumber < 1 || presetNumber > 6)
                                                {
                                                    Console.WriteLine("✗ Preset number must be between 1 and 6!");
                                                    return;
                                                }

                                                Console.Write($"Enter voltage for M{presetNumber} (e.g., 12.5): ");
                                                string? voltageInput = Console.ReadLine();

                                                if (string.IsNullOrWhiteSpace(voltageInput))
                                                {
                                                    Console.WriteLine("✗ Invalid voltage input!");
                                                    return;
                                                }

                                                float voltage = float.Parse(voltageInput, System.Globalization.CultureInfo.InvariantCulture);

                                                Console.Write($"Enter current limit for M{presetNumber} (e.g., 2.5): ");
                                                string? currentInput = Console.ReadLine();

                                                if (string.IsNullOrWhiteSpace(currentInput))
                                                {
                                                    Console.WriteLine("✗ Invalid current input!");
                                                    return;
                                                }

                                                float current = float.Parse(currentInput, System.Globalization.CultureInfo.InvariantCulture);

                                                // Validate ranges
                                                if (voltage < 0 || voltage > 150)
                                                {
                                                    Console.WriteLine("⚠ Warning: Voltage out of typical range (0-150V)");
                                                }

                                                if (current < 0 || current > 15)
                                                {
                                                    Console.WriteLine("⚠ Warning: Current out of typical range (0-15A)");
                                                }

                                                Console.WriteLine($"\nSetting preset M{presetNumber} to {voltage:F2}V / {current:F2}A...");
                                                Console.WriteLine($"Command packets:");
                                                Console.WriteLine($"  Voltage: F1 B1 [register] 04 [float bytes] [checksum]");
                                                Console.WriteLine($"  Current: F1 B1 [register] 04 [float bytes] [checksum]");

                                                if (_control.SetPreset(presetNumber, voltage, current))
                                                {
                                                    Console.WriteLine($"✓ Preset M{presetNumber} configured successfully");
                                                    Console.WriteLine($"  Voltage: {voltage:F2}V");
                                                    Console.WriteLine($"  Current: {current:F2}A");
                                                    Console.WriteLine("  Session Status: " + (_control.IsSessionStarted ? "✓ ACTIVE" : "✗ INACTIVE"));
                                                    Console.WriteLine($"  ℹ Preset M{presetNumber} can now be recalled from the device");
                                                }
                                                else
                                                {
                                                    Console.WriteLine($"✗ Failed to set preset M{presetNumber}");
                                                    Console.WriteLine("  Possible reasons:");
                                                    Console.WriteLine("  - Device not responding");
                                                    Console.WriteLine("  - Communication error");
                                                    Console.WriteLine("  - Invalid preset number");
                                                }
                                            }
                                            catch (FormatException)
                                            {
                                                Console.WriteLine("✗ Invalid number format! Please enter valid numbers.");
                                            }
                                            catch (Exception ex)
                                            {
                                                        Console.WriteLine($"✗ Error: {ex.Message}");
                                                    }
                                                }

                                                /// <summary>
                                                /// Tests the GetPreset method by reading a preset memory slot.
                                                /// </summary>
                                                private static void GetPreset()
                                                {
                                                    Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
                                                    Console.WriteLine("│ GET PRESET (M1-M6)                                      │");
                                                    Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                                                    if (!_control!.IsConnected)
                                                    {
                                                        Console.WriteLine("✗ Not connected! Please connect to a device first.");
                                                        return;
                                                    }

                                                    Console.WriteLine("Available presets: M1, M2, M3, M4, M5, M6");
                                                    Console.Write("Enter preset number to read (1-6): ");
                                                    string? presetInput = Console.ReadLine();

                                                    if (string.IsNullOrWhiteSpace(presetInput))
                                                    {
                                                        Console.WriteLine("✗ Invalid input!");
                                                        return;
                                                    }

                                                    try
                                                    {
                                                        int presetNumber = int.Parse(presetInput);

                                                        if (presetNumber < 1 || presetNumber > 6)
                                                        {
                                                            Console.WriteLine("✗ Preset number must be between 1 and 6!");
                                                            return;
                                                        }

                                                        Console.WriteLine($"\nReading preset M{presetNumber}...");
                                                        Console.WriteLine($"Command packets:");
                                                        Console.WriteLine($"  Voltage: F1 A1 [voltage_register] 00 [checksum]");
                                                        Console.WriteLine($"  Current: F1 A1 [current_register] 00 [checksum]");

                                                        if (_control.GetPreset(presetNumber, out float voltage, out float current))
                                                        {
                                                            Console.WriteLine($"✓ Preset M{presetNumber} read successfully");
                                                            Console.WriteLine($"  Voltage: {voltage:F3}V");
                                                            Console.WriteLine($"  Current: {current:F3}A");
                                                            Console.WriteLine("  Session Status: " + (_control.IsSessionStarted ? "✓ ACTIVE" : "✗ INACTIVE"));
                                                            Console.WriteLine($"  ℹ These are the stored preset values in memory slot M{presetNumber}");
                                                            Console.WriteLine($"  ℹ To activate this preset, use the device's front panel");
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine($"✗ Failed to read preset M{presetNumber}");
                                                            Console.WriteLine("  Possible reasons:");
                                                            Console.WriteLine("  - Device not responding");
                                                            Console.WriteLine("  - Communication error");
                                                            Console.WriteLine("  - Invalid preset number");
                                                            Console.WriteLine("  - Timeout during read operation");
                                                        }
                                                    }
                                                                catch (FormatException)
                                                                {
                                                                    Console.WriteLine("✗ Invalid number format! Please enter a valid preset number.");
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    Console.WriteLine($"✗ Error: {ex.Message}");
                                                                }
                                                            }

                                                    /// <summary>
                                                    /// Interactive menu for configuring all protection settings.
                                                    /// </summary>
                                                    private static void ConfigureProtectionSettings()
                                                    {
                                                        Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
                                                        Console.WriteLine("│ PROTECTION SETTINGS                                     │");
                                                        Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                                                        if (!_control!.IsConnected)
                                                        {
                                                            Console.WriteLine("✗ Not connected! Please connect to a device first.");
                                                            return;
                                                        }

                                                        bool keepRunning = true;
                                                        while (keepRunning)
                                                        {
                                                            Console.WriteLine("\n" + new string('─', 62));
                                                            Console.WriteLine("Protection Settings Menu:");
                                                            Console.WriteLine("  [1] Set/Get OVP (Over-Voltage Protection)");
                                                            Console.WriteLine("  [2] Set/Get OCP (Over-Current Protection)");
                                                            Console.WriteLine("  [3] Set/Get OPP (Over-Power Protection)");
                                                            Console.WriteLine("  [4] Set/Get OTP (Over-Temperature Protection)");
                                                            Console.WriteLine("  [5] Set/Get LVP (Low-Voltage Protection)");
                                                            Console.WriteLine("  [0] Back to Main Menu");
                                                            Console.WriteLine(new string('─', 62));
                                                            Console.Write("Select option: ");

                                                            string? choice = Console.ReadLine()?.Trim().ToUpper();

                                                            switch (choice)
                                                            {
                                                                case "1":
                                                                    SetGetProtectionValue("OVP", "Over-Voltage Protection", "V", 0, 160,
                                                                        (val) => _control.SetOVP(val), () => _control.GetOVP());
                                                                    break;
                                                                case "2":
                                                                    SetGetProtectionValue("OCP", "Over-Current Protection", "A", 0, 20,
                                                                        (val) => _control.SetOCP(val), () => _control.GetOCP());
                                                                    break;
                                                                case "3":
                                                                    SetGetProtectionValue("OPP", "Over-Power Protection", "W", 0, 3000,
                                                                        (val) => _control.SetOPP(val), () => _control.GetOPP());
                                                                    break;
                                                                case "4":
                                                                    SetGetProtectionValue("OTP", "Over-Temperature Protection", "°C", 0, 100,
                                                                        (val) => _control.SetOTP(val), () => _control.GetOTP());
                                                                    break;
                                                                case "5":
                                                                    SetGetProtectionValue("LVP", "Low-Voltage Protection", "V", 0, 30,
                                                                        (val) => _control.SetLVP(val), () => _control.GetLVP());
                                                                    break;
                                                                case "0":
                                                                    keepRunning = false;
                                                                    break;
                                                                default:
                                                                    Console.WriteLine("⚠ Invalid option!");
                                                                    break;
                                                            }
                                                        }
                                                    }

                                                    /// <summary>
                                                    /// Helper method for setting and getting protection values.
                                                    /// </summary>
                                                    private static void SetGetProtectionValue(string shortName, string fullName, string unit,
                                                        float minValue, float maxValue, Func<float, bool> setter, Func<float> getter)
                                                    {
                                                        Console.WriteLine($"\n┌─────────────────────────────────────────────────────────┐");
                                                        Console.WriteLine($"│ {fullName,-55} │");
                                                        Console.WriteLine($"└─────────────────────────────────────────────────────────┘");

                                                        Console.WriteLine($"Range: {minValue}{unit} - {maxValue}{unit}");
                                                        Console.WriteLine("\nOptions:");
                                                        Console.WriteLine("  [S] Set value");
                                                        Console.WriteLine("  [G] Get current value");
                                                        Console.Write("Select: ");

                                                        string? action = Console.ReadLine()?.Trim().ToUpper();

                                                        if (action == "S")
                                                        {
                                                            Console.Write($"Enter {shortName} value ({unit}): ");
                                                            string? input = Console.ReadLine();

                                                            if (string.IsNullOrWhiteSpace(input))
                                                            {
                                                                Console.WriteLine("✗ Invalid input!");
                                                                return;
                                                            }

                                                            try
                                                            {
                                                                float value = float.Parse(input, System.Globalization.CultureInfo.InvariantCulture);

                                                                if (value < minValue || value > maxValue)
                                                                {
                                                                    Console.WriteLine($"⚠ Warning: Value outside typical range ({minValue}-{maxValue}{unit})");
                                                                }

                                                                Console.WriteLine($"Setting {shortName} to {value:F2}{unit}...");

                                                                if (setter(value))
                                                                {
                                                                    Console.WriteLine($"✓ {shortName} set successfully to {value:F2}{unit}");
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine($"✗ Failed to set {shortName}");
                                                                }
                                                            }
                                                            catch (FormatException)
                                                            {
                                                                Console.WriteLine("✗ Invalid number format!");
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                Console.WriteLine($"✗ Error: {ex.Message}");
                                                            }
                                                        }
                                                        else if (action == "G")
                                                        {
                                                            Console.WriteLine($"Reading {shortName}...");

                                                            float value = getter();

                                                            if (value >= 0)
                                                            {
                                                                Console.WriteLine($"✓ {shortName}: {value:F2}{unit}");
                                                            }
                                                            else
                                                            {
                                                                Console.WriteLine($"✗ Failed to read {shortName}");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("✗ Invalid action!");
                                                        }
                                                    }

                                                    /// <summary>
                                                    /// Interactive menu for configuring UI settings.
                                                    /// </summary>
                                                    private static void ConfigureUISettings()
                                                    {
                                                        Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
                                                        Console.WriteLine("│ UI SETTINGS                                             │");
                                                        Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                                                        if (!_control!.IsConnected)
                                                        {
                                                            Console.WriteLine("✗ Not connected! Please connect to a device first.");
                                                            return;
                                                        }

                                                        bool keepRunning = true;
                                                        while (keepRunning)
                                                        {
                                                            Console.WriteLine("\n" + new string('─', 62));
                                                            Console.WriteLine("UI Settings Menu:");
                                                            Console.WriteLine("  [1] Set/Get Brightness");
                                                            Console.WriteLine("  [2] Set/Get Volume");
                                                            Console.WriteLine("  [0] Back to Main Menu");
                                                            Console.WriteLine(new string('─', 62));
                                                            Console.Write("Select option: ");

                                                            string? choice = Console.ReadLine()?.Trim().ToUpper();

                                                            switch (choice)
                                                            {
                                                                case "1":
                                                                    SetGetIntValue("Brightness", "Display Brightness", "%", 0, 100,
                                                                        (val) => _control.SetBrightness(val), () => _control.GetBrightness());
                                                                    break;
                                                                case "2":
                                                                    SetGetIntValue("Volume", "Device Volume", "%", 0, 100,
                                                                        (val) => _control.SetVolume(val), () => _control.GetVolume());
                                                                    break;
                                                                case "0":
                                                                    keepRunning = false;
                                                                    break;
                                                                default:
                                                                    Console.WriteLine("⚠ Invalid option!");
                                                                    break;
                                                            }
                                                        }
                                                    }

                                                    /// <summary>
                                                    /// Helper method for setting and getting integer values.
                                                    /// </summary>
                                                    private static void SetGetIntValue(string shortName, string fullName, string unit,
                                                        int minValue, int maxValue, Func<int, bool> setter, Func<int> getter)
                                                    {
                                                        Console.WriteLine($"\n┌─────────────────────────────────────────────────────────┐");
                                                        Console.WriteLine($"│ {fullName,-55} │");
                                                        Console.WriteLine($"└─────────────────────────────────────────────────────────┘");

                                                        Console.WriteLine($"Range: {minValue}{unit} - {maxValue}{unit}");
                                                        Console.WriteLine("\nOptions:");
                                                        Console.WriteLine("  [S] Set value");
                                                        Console.WriteLine("  [G] Get current value");
                                                        Console.Write("Select: ");

                                                        string? action = Console.ReadLine()?.Trim().ToUpper();

                                                        if (action == "S")
                                                        {
                                                            Console.Write($"Enter {shortName} value ({unit}): ");
                                                            string? input = Console.ReadLine();

                                                            if (string.IsNullOrWhiteSpace(input))
                                                            {
                                                                Console.WriteLine("✗ Invalid input!");
                                                                return;
                                                            }

                                                            try
                                                            {
                                                                int value = int.Parse(input);

                                                                if (value < minValue || value > maxValue)
                                                                {
                                                                    Console.WriteLine($"⚠ Warning: Value outside range ({minValue}-{maxValue}{unit})");
                                                                }

                                                                Console.WriteLine($"Setting {shortName} to {value}{unit}...");

                                                                if (setter(value))
                                                                {
                                                                    Console.WriteLine($"✓ {shortName} set successfully to {value}{unit}");
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine($"✗ Failed to set {shortName}");
                                                                }
                                                            }
                                                            catch (FormatException)
                                                            {
                                                                Console.WriteLine("✗ Invalid number format!");
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                Console.WriteLine($"✗ Error: {ex.Message}");
                                                            }
                                                        }
                                                        else if (action == "G")
                                                        {
                                                            Console.WriteLine($"Reading {shortName}...");

                                                            int value = getter();

                                                            if (value >= 0)
                                                            {
                                                                Console.WriteLine($"✓ {shortName}: {value}{unit}");
                                                            }
                                                            else
                                                            {
                                                                Console.WriteLine($"✗ Failed to read {shortName}");
                                                            }
                                                        }
                                                        else
                                                        {
                                                                    Console.WriteLine("✗ Invalid action!");
                                                                }
                                                            }

                                                            /// <summary>
                                                            /// Comprehensive telemetry monitoring display.
                                                            /// </summary>
                                                            private static void MonitorTelemetry()
                                                            {
                                                                Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
                                                                Console.WriteLine("│ DEVICE TELEMETRY MONITOR                                │");
                                                                Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                                                                if (!_control!.IsConnected)
                                                                {
                                                                    Console.WriteLine("✗ Not connected! Please connect to a device first.");
                                                                    return;
                                                                }

                                                                Console.WriteLine("\nReading all telemetry values...\n");

                                                                // Device Capabilities
                                                                Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
                                                                Console.WriteLine("│ DEVICE CAPABILITIES                                     │");
                                                                Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                                                                float maxVoltage = _control.GetMaximumVoltage();
                                                                if (maxVoltage >= 0)
                                                                {
                                                                    Console.WriteLine($"  Maximum Voltage:     {maxVoltage:F2} V");
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("  Maximum Voltage:     ✗ READ FAILED");
                                                                }

                                                                float maxCurrent = _control.GetMaximumCurrent();
                                                                if (maxCurrent >= 0)
                                                                {
                                                                    Console.WriteLine($"  Maximum Current:     {maxCurrent:F2} A");
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("  Maximum Current:     ✗ READ FAILED");
                                                                }

                                                                if (maxVoltage >= 0 && maxCurrent >= 0)
                                                                {
                                                                    float maxPower = maxVoltage * maxCurrent;
                                                                    Console.WriteLine($"  Maximum Power:       {maxPower:F2} W (calculated)");
                                                                }

                                                                // Power Status
                                                                Console.WriteLine("\n┌─────────────────────────────────────────────────────────┐");
                                                                Console.WriteLine("│ POWER STATUS                                            │");
                                                                Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                                                                float inputVoltage = _control.GetInputVoltage();
                                                                if (inputVoltage >= 0)
                                                                {
                                                                    Console.WriteLine($"  Input Voltage:       {inputVoltage:F2} V");
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("  Input Voltage:       ✗ READ FAILED");
                                                                }

                                                                float temperature = _control.GetInternalTemperature();
                                                                if (temperature >= 0)
                                                                {
                                                                    Console.WriteLine($"  Internal Temp:       {temperature:F2} °C");
                                                                    if (temperature > 80)
                                                                    {
                                                                        Console.WriteLine("                       ⚠ WARNING: High temperature!");
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("  Internal Temp:       ✗ READ FAILED");
                                                                }

                                                                // Output Status
                                                                Console.WriteLine("\n┌─────────────────────────────────────────────────────────┐");
                                                                Console.WriteLine("│ OUTPUT STATUS                                           │");
                                                                Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                                                                bool? runningMode = _control.GetRunningMode();
                                                                if (runningMode.HasValue)
                                                                {
                                                                    Console.WriteLine($"  Running Mode:        {(runningMode.Value ? "✓ RUN (ON)" : "✗ STOP (OFF)")}");
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("  Running Mode:        ✗ READ FAILED");
                                                                }

                                                                bool? cccv = _control.GetCCCV();
                                                                if (cccv.HasValue)
                                                                {
                                                                    Console.WriteLine($"  Regulation Mode:     {(cccv.Value ? "CV (Constant Voltage)" : "CC (Constant Current)")}");
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("  Regulation Mode:     ✗ READ FAILED");
                                                                }

                                                                // Measurement Data
                                                                Console.WriteLine("\n┌─────────────────────────────────────────────────────────┐");
                                                                Console.WriteLine("│ MEASUREMENT DATA                                        │");
                                                                Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                                                                float[]? measurement = _control.GetMeasurement();
                                                                if (measurement != null && measurement.Length == 2)
                                                                {
                                                                    Console.WriteLine($"  Output Voltage:      {measurement[0]:F3} V");
                                                                    Console.WriteLine($"  Output Current:      {measurement[1]:F3} A");
                                                                    float outputPower = measurement[0] * measurement[1];
                                                                    Console.WriteLine($"  Output Power:        {outputPower:F3} W (calculated)");
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("  Measurement Data:    ✗ READ FAILED");
                                                                }

                                                                // Setpoints (for comparison)
                                                                Console.WriteLine("\n┌─────────────────────────────────────────────────────────┐");
                                                                Console.WriteLine("│ SETPOINTS (for comparison)                              │");
                                                                Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                                                                float voltageSetpoint = _control.GetVoltage();
                                                                if (voltageSetpoint >= 0)
                                                                {
                                                                    Console.WriteLine($"  Voltage Setpoint:    {voltageSetpoint:F3} V");
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("  Voltage Setpoint:    ✗ READ FAILED");
                                                                }

                                                                float currentSetpoint = _control.GetCurrent();
                                                                if (currentSetpoint >= 0)
                                                                {
                                                                    Console.WriteLine($"  Current Limit:       {currentSetpoint:F3} A");
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("  Current Limit:       ✗ READ FAILED");
                                                                }

                                                                // Energy/Capacity
                                                                Console.WriteLine("\n┌─────────────────────────────────────────────────────────┐");
                                                                Console.WriteLine("│ ENERGY & CAPACITY COUNTERS                              │");
                                                                Console.WriteLine("└─────────────────────────────────────────────────────────┘");

                                                                float capacity = _control.GetMeasuredCapacity();
                                                                if (capacity >= 0)
                                                                {
                                                                    Console.WriteLine($"  Capacity:            {capacity:F3} Ah");
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("  Capacity:            ✗ READ FAILED");
                                                                }

                                                                float energy = _control.GetMeasuredEnergy();
                                                                if (energy >= 0)
                                                                {
                                                                    Console.WriteLine($"  Energy:              {energy:F3} Wh");
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("  Energy:              ✗ READ FAILED");
                                                                }

                                                                Console.WriteLine("\n" + new string('─', 62));
                                                                Console.WriteLine("ℹ Telemetry snapshot complete");
                                                                Console.WriteLine("ℹ The device also transmits telemetry automatically (~500ms)");
                                                            }

                                                            #endregion

                                                            #region Checksum Utilities

                                                            private static void CalculateChecksum()
                                                            {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ CALCULATE CHECKSUM                                      │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("Enter packet hex bytes (space-separated, e.g., F1 B1 C1 04 00 00 B0 40):");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("✗ Invalid input!");
                return;
            }

            try
            {
                string[] hexBytes = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                byte[] data = hexBytes.Select(h => Convert.ToByte(h, 16)).ToArray();

                if (data.Length < 4)
                {
                    Console.WriteLine("✗ Packet too short! Minimum 4 bytes required.");
                    return;
                }

                byte checksum = DPS150Control.CalculateChecksum(data);

                Console.WriteLine($"\n✓ Checksum calculated successfully:");
                Console.WriteLine($"  Input packet: {BitConverter.ToString(data)}");
                Console.WriteLine($"  Checksum: 0x{checksum:X2}");
                Console.WriteLine($"\nComplete packet with checksum:");
                Console.WriteLine($"  {BitConverter.ToString(data)} {checksum:X2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        private static void VerifyChecksum()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ VERIFY CHECKSUM                                         │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("Enter complete packet with checksum (space-separated):");
            Console.WriteLine("Example: F1 B1 C1 04 00 00 B0 40 B5");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("✗ Invalid input!");
                return;
            }

            try
            {
                string[] hexBytes = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                byte[] packet = hexBytes.Select(h => Convert.ToByte(h, 16)).ToArray();

                if (packet.Length < 5)
                {
                    Console.WriteLine("✗ Packet too short! Minimum 5 bytes required.");
                    return;
                }

                bool isValid = DPS150Control.VerifyChecksum(packet);

                Console.WriteLine($"\nPacket: {BitConverter.ToString(packet)}");
                Console.WriteLine($"Checksum byte: 0x{packet[packet.Length - 1]:X2}");

                if (isValid)
                {
                    Console.WriteLine("✓ Checksum is VALID");
                }
                else
                {
                    Console.WriteLine("✗ Checksum is INVALID");

                    // Show what the correct checksum should be
                    byte[] dataWithoutChecksum = new byte[packet.Length - 1];
                    Array.Copy(packet, dataWithoutChecksum, packet.Length - 1);
                    byte correctChecksum = DPS150Control.CalculateChecksum(dataWithoutChecksum);
                    Console.WriteLine($"  Expected checksum: 0x{correctChecksum:X2}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        private static void TestAllChecksums()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ TEST ALL CHECKSUM EXAMPLES                              │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            var testCases = new[]
            {
                new { Name = "Voltage Set", Packet = new byte[] { 0xF1, 0xB1, 0xC1, 0x04, 0x00, 0x00, 0xB0, 0x40 }, Expected = (byte)0xB5 },
                new { Name = "Current Limit", Packet = new byte[] { 0xF1, 0xB1, 0xC2, 0x04, 0xFD, 0xFF, 0xFF, 0x3E }, Expected = (byte)0xFF },
                new { Name = "Output Relay ON", Packet = new byte[] { 0xF1, 0xB1, 0xDB, 0x01, 0x01 }, Expected = (byte)0xDD },
                new { Name = "Output Relay OFF", Packet = new byte[] { 0xF1, 0xB1, 0xDB, 0x01, 0x00 }, Expected = (byte)0xDC },
                new { Name = "Session Start", Packet = new byte[] { 0xF1, 0xC1, 0x00, 0x01, 0x01 }, Expected = (byte)0x02 },
                new { Name = "Session Stop", Packet = new byte[] { 0xF1, 0xC1, 0x00, 0x01, 0x00 }, Expected = (byte)0x02 }
            };

            int passed = 0;
            int failed = 0;

            foreach (var test in testCases)
            {
                byte calculated = DPS150Control.CalculateChecksum(test.Packet);
                bool success = calculated == test.Expected;

                if (success)
                {
                    passed++;
                    Console.WriteLine($"✓ {test.Name,-20} Expected: 0x{test.Expected:X2}, Got: 0x{calculated:X2}");
                }
                else
                {
                    failed++;
                    Console.WriteLine($"✗ {test.Name,-20} Expected: 0x{test.Expected:X2}, Got: 0x{calculated:X2}");
                }
            }

            Console.WriteLine(new string('─', 60));
            Console.WriteLine($"Test Results: {passed} passed, {failed} failed out of {testCases.Length} tests");
        }

        #endregion

        #region Comprehensive Tests

        private static void RunCompleteTestSuite()
        {
            Console.WriteLine("╔═════════════════════════════════════════════════════════╗");
            Console.WriteLine("║ COMPLETE TEST SUITE                                     ║");
            Console.WriteLine("╚═════════════════════════════════════════════════════════╝");

            Console.WriteLine("\n[TEST 1] Checksum Calculations");
            Console.WriteLine(new string('─', 60));
            TestAllChecksums();

            Console.WriteLine("\n[TEST 2] Port Enumeration");
            Console.WriteLine(new string('─', 60));
            ListAvailablePorts();

            Console.WriteLine("\n[TEST 3] Connection Properties");
            Console.WriteLine(new string('─', 60));
            Console.WriteLine($"IsConnected: {_control!.IsConnected}");
            Console.WriteLine($"IsSessionStarted: {_control.IsSessionStarted}");

            Console.WriteLine("\n✓ Complete test suite finished!");
        }

        private static void RunFullDeviceControlTest()
        {
            Console.WriteLine("╔═════════════════════════════════════════════════════════╗");
            Console.WriteLine("║ FULL DEVICE CONTROL TEST                                ║");
            Console.WriteLine("╚═════════════════════════════════════════════════════════╝");

            if (!_control!.IsConnected)
            {
                Console.WriteLine("✗ Not connected! Please connect to a device first.");
                Console.WriteLine("  This test requires an active device connection.");
                return;
            }

            Console.WriteLine("\n⚠ WARNING: This test will control the device output!");
            Console.WriteLine("  Make sure no load is connected or use appropriate safety measures.");
            Console.Write("\nContinue? (Y/N): ");

            if (Console.ReadLine()?.ToUpper() != "Y")
            {
                Console.WriteLine("Test cancelled.");
                return;
            }

            Console.WriteLine("\n[STEP 1] Starting session...");
            if (_control.StartSession())
            {
                Console.WriteLine("✓ Session started");
            }
            else
            {
                Console.WriteLine("✗ Failed to start session");
                return;
            }
            System.Threading.Thread.Sleep(100);

            Console.WriteLine("\n[STEP 2] Setting voltage to 12.5V...");
            if (_control.SetVoltage(12.5f))
            {
                Console.WriteLine("✓ Voltage set to 12.5V");
            }
            else
            {
                Console.WriteLine("✗ Failed to set voltage");
            }
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("\n[STEP 3] Reading voltage setpoint...");
            float readVoltage = _control.GetVoltage();
            if (readVoltage >= 0)
            {
                Console.WriteLine($"✓ Voltage setpoint read: {readVoltage:F2}V (Expected: 12.5V)");
                if (Math.Abs(readVoltage - 12.5f) < 0.1f)
                {
                    Console.WriteLine("  ✓ Setpoint matches written value");
                }
                else
                {
                    Console.WriteLine("  ⚠ Setpoint differs from written value");
                }
            }
            else
            {
                Console.WriteLine("✗ Failed to read voltage setpoint");
            }
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("\n[STEP 4] Setting current limit to 2.5A...");
            if (_control.SetCurrent(2.5f))
            {
                Console.WriteLine("✓ Current limit set to 2.5A");
            }
            else
            {
                Console.WriteLine("✗ Failed to set current limit");
            }
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("\n[STEP 5] Reading current limit setpoint...");
            float readCurrent = _control.GetCurrent();
            if (readCurrent >= 0)
            {
                Console.WriteLine($"✓ Current limit setpoint read: {readCurrent:F2}A (Expected: 2.5A)");
                if (Math.Abs(readCurrent - 2.5f) < 0.1f)
                {
                    Console.WriteLine("  ✓ Setpoint matches written value");
                }
                else
                {
                    Console.WriteLine("  ⚠ Setpoint differs from written value");
                }
            }
            else
            {
                Console.WriteLine("✗ Failed to read current limit setpoint");
            }
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("\n[STEP 6] Setting voltage to 5.0V...");
            if (_control.SetVoltage(5.0f))
            {
                Console.WriteLine("✓ Voltage set to 5.0V");
            }
            else
            {
                Console.WriteLine("✗ Failed to set voltage");
            }
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("\n[STEP 6] Reading voltage setpoint again...");
            readVoltage = _control.GetVoltage();
            if (readVoltage >= 0)
            {
                Console.WriteLine($"✓ Voltage setpoint read: {readVoltage:F2}V (Expected: 5.0V)");
                if (Math.Abs(readVoltage - 5.0f) < 0.1f)
                {
                    Console.WriteLine("  ✓ Setpoint matches written value");
                }
                else
                {
                    Console.WriteLine("  ⚠ Setpoint differs from written value");
                }
            }
            else
            {
                Console.WriteLine("✗ Failed to read voltage setpoint");
            }
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("\n[STEP 7] Setting current limit to 1.0A...");
            if (_control.SetCurrent(1.0f))
            {
                Console.WriteLine("✓ Current limit set to 1.0A");
            }
            else
            {
                Console.WriteLine("✗ Failed to set current limit");
            }
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("\n[STEP 8] Reading current limit setpoint again...");
            readCurrent = _control.GetCurrent();
            if (readCurrent >= 0)
            {
                Console.WriteLine($"✓ Current limit setpoint read: {readCurrent:F2}A (Expected: 1.0A)");
                if (Math.Abs(readCurrent - 1.0f) < 0.1f)
                {
                    Console.WriteLine("  ✓ Setpoint matches written value");
                }
                else
                {
                    Console.WriteLine("  ⚠ Setpoint differs from written value");
                }
            }
            else
            {
                Console.WriteLine("✗ Failed to read current limit setpoint");
            }
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("\n[STEP 9] Turning output ON...");
            if (_control.SetOutputRelay(OutputRelayState.ON))
            {
                Console.WriteLine("✓ Output enabled");
            }
            else
            {
                Console.WriteLine("✗ Failed to enable output");
            }
            System.Threading.Thread.Sleep(2000);

            Console.WriteLine("\n[STEP 10] Turning output OFF...");
            if (_control.SetOutputRelay(OutputRelayState.OFF))
            {
                Console.WriteLine("✓ Output disabled");
            }
            else
            {
                Console.WriteLine("✗ Failed to disable output");
            }
            System.Threading.Thread.Sleep(100);

            Console.WriteLine("\n[STEP 11] Setting preset M1 to 12.0V / 2.0A...");
            if (_control.SetPreset(1, 12.0f, 2.0f))
            {
                Console.WriteLine("✓ Preset M1 configured: 12.0V / 2.0A");
            }
            else
            {
                Console.WriteLine("✗ Failed to set preset M1");
            }
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("\n[STEP 12] Setting preset M2 to 5.0V / 1.0A...");
            if (_control.SetPreset(2, 5.0f, 1.0f))
            {
                Console.WriteLine("✓ Preset M2 configured: 5.0V / 1.0A");
            }
            else
            {
                Console.WriteLine("✗ Failed to set preset M2");
            }
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("\n[STEP 13] Reading preset M1...");
            if (_control.GetPreset(1, out float m1Voltage, out float m1Current))
            {
                Console.WriteLine($"✓ Preset M1 read: {m1Voltage:F2}V / {m1Current:F2}A (Expected: 12.0V / 2.0A)");
                if (Math.Abs(m1Voltage - 12.0f) < 0.1f && Math.Abs(m1Current - 2.0f) < 0.1f)
                {
                    Console.WriteLine("  ✓ Preset values match written data");
                }
                else
                {
                    Console.WriteLine("  ⚠ Preset values differ from written data");
                }
            }
            else
            {
                Console.WriteLine("✗ Failed to read preset M1");
            }
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("\n[STEP 14] Reading preset M2...");
            if (_control.GetPreset(2, out float m2Voltage, out float m2Current))
            {
                Console.WriteLine($"✓ Preset M2 read: {m2Voltage:F2}V / {m2Current:F2}A (Expected: 5.0V / 1.0A)");
                if (Math.Abs(m2Voltage - 5.0f) < 0.1f && Math.Abs(m2Current - 1.0f) < 0.1f)
                {
                    Console.WriteLine("  ✓ Preset values match written data");
                }
                else
                {
                    Console.WriteLine("  ⚠ Preset values differ from written data");
                }
            }
            else
            {
                Console.WriteLine("✗ Failed to read preset M2");
            }
            System.Threading.Thread.Sleep(500);

            Console.WriteLine("\n[STEP 15] Stopping session...");
            if (_control.StopSession())
            {
                Console.WriteLine("✓ Session stopped");
            }
            else
            {
                Console.WriteLine("✗ Failed to stop session");
            }

            Console.WriteLine("\n✓ Full device control test completed!");
            Console.WriteLine($"  Final connection status: {_control.IsConnected}");
            Console.WriteLine($"  Final session status: {_control.IsSessionStarted}");
        }

        #endregion
    }
}
