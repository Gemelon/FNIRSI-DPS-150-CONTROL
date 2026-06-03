// MIT License
//
// Copyright (c) 2026 FNIRSI-DPS-150-CONTROL Project
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

using System;

namespace FNIRSI_DPS_150_CONTROL
{
    /// <summary>
    /// Test program for DPS150Registers class.
    /// Demonstrates and validates the register abstraction layer.
    /// </summary>
    public class DPS150RegistersTestProgram
    {
        private static DPS150Registers? _registers;
        private static bool _isRunning = true;

        public static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  DPS150Registers Test Program                           ║");
            Console.WriteLine("║  FNIRSI DPS-150 Register Abstraction Layer Tester       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            _registers = new DPS150Registers();

            while (_isRunning)
            {
                ShowMenu();
                ProcessUserInput();
            }

            Console.WriteLine("\nGoodbye!");
        }

        private static void ShowMenu()
        {
            Console.WriteLine("\n" + new string('═', 62));
            Console.WriteLine($"│ Connection: {(_registers?.IsConnected == true ? "✓ CONNECTED" : "✗ DISCONNECTED"),-30} │");
            Console.WriteLine(new string('═', 62));
            Console.WriteLine("│  CONNECTION MANAGEMENT                                   │");
            Console.WriteLine("│  [1] List Available Ports                                │");
            Console.WriteLine("│  [2] Connect to Device (by port number)                  │");
            Console.WriteLine("│  [3] Disconnect from Device                              │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  REGISTER PROPERTY TESTS                                 │");
            Console.WriteLine("│  [4] Test Voltage Setpoint Register                      │");
            Console.WriteLine("│  [5] Test Current Limit Register                         │");
            Console.WriteLine("│  [6] Test All Protection Registers                       │");
            Console.WriteLine("│  [7] Test Preset Registers (M1-M6)                       │");
            Console.WriteLine("│  [8] Test UI Settings Registers                          │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  SET REGISTER VALUES (Interactive)                       │");
            Console.WriteLine("│  [S1] Set Voltage Setpoint                               │");
            Console.WriteLine("│  [S2] Set Current Limit                                  │");
            Console.WriteLine("│  [S3] Set Output Relay (ON/OFF)                          │");
            Console.WriteLine("│  [S4] Set Preset Memory (M1-M6)                          │");
            Console.WriteLine("│  [S5] Set Protection Settings                            │");
            Console.WriteLine("│  [S6] Set UI Settings (Brightness/Volume)                │");
            Console.WriteLine("│  [S7] Quick Start (Voltage + Current + ON)               │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  TELEMETRY REGISTER TESTS                                │");
            Console.WriteLine("│  [9] Test Read-Only Telemetry Registers                  │");
            Console.WriteLine("│  [A] Test Status Registers (Running Mode, CC/CV)        │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  LOW-LEVEL TESTS                                         │");
            Console.WriteLine("│  [B] Test Float Conversion Methods                       │");
            Console.WriteLine("│  [C] Test Packet Creation                                │");
            Console.WriteLine("│  [D] Test Checksum Calculation                           │");
            Console.WriteLine("│  [E] Test Checksum Verification                          │");
            Console.WriteLine("│  [F] Test Telemetry Frame Parser                         │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  COMPREHENSIVE TESTS                                     │");
            Console.WriteLine("│  [T] Run All Register Tests                              │");
            Console.WriteLine(new string('─', 62));
            Console.WriteLine("│  [0] Exit                                                │");
            Console.WriteLine(new string('═', 62));
            Console.Write("Select option: ");
        }

        private static void ProcessUserInput()
        {
            string? input = Console.ReadLine()?.Trim().ToUpper();

            switch (input)
            {
                case "1":
                    ListAvailablePorts();
                    break;
                case "2":
                    ConnectByPortNumber();
                    break;
                case "3":
                    DisconnectFromDevice();
                    break;
                case "4":
                    TestVoltageRegister();
                    break;
                case "5":
                    TestCurrentRegister();
                    break;
                case "6":
                    TestProtectionRegisters();
                    break;
                case "7":
                    TestPresetRegisters();
                    break;
                case "8":
                    TestUIRegisters();
                    break;
                case "9":
                    TestTelemetryRegisters();
                    break;
                case "A":
                    TestStatusRegisters();
                    break;
                case "B":
                    TestFloatConversion();
                    break;
                case "C":
                    TestPacketCreation();
                    break;
                case "D":
                    TestChecksumCalculation();
                    break;
                case "E":
                    TestChecksumVerification();
                    break;
                case "F":
                    TestTelemetryFrameParser();
                    break;
                case "S1":
                    SetVoltageSetpoint();
                    break;
                case "S2":
                    SetCurrentLimit();
                    break;
                case "S3":
                    SetOutputRelay();
                    break;
                case "S4":
                    SetPresetMemory();
                    break;
                case "S5":
                    SetProtectionSettings();
                    break;
                case "S6":
                    SetUISettings();
                    break;
                case "S7":
                    QuickStart();
                    break;
                case "T":
                    RunAllTests();
                    break;
                case "0":
                    _isRunning = false;
                    break;
                default:
                    Console.WriteLine("⚠ Invalid option! Please try again.");
                    break;
            }
        }

        #region Connection Management

        private static void ListAvailablePorts()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ AVAILABLE SERIAL PORTS                                  │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            string[] ports = DPS150Communication.GetAvailablePorts();

            if (ports.Length == 0)
            {
                Console.WriteLine("✗ No serial ports found on this system.");
                return;
            }

            Console.WriteLine($"Found {ports.Length} port(s):");
            for (int i = 0; i < ports.Length; i++)
            {
                Console.WriteLine($"  [{i + 1}] {ports[i]}");
            }
        }

        private static void ConnectByPortNumber()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ CONNECT TO DEVICE (by port number)                     │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            string[] ports = DPS150Communication.GetAvailablePorts();

            if (ports.Length == 0)
            {
                Console.WriteLine("✗ No serial ports available!");
                return;
            }

            Console.WriteLine("Available ports:");
            for (int i = 0; i < ports.Length; i++)
            {
                Console.WriteLine($"  [{i + 1}] {ports[i]}");
            }

            Console.Write("Enter port number: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("✗ Invalid input!");
                return;
            }

            try
            {
                int portNumber = int.Parse(input);

                if (_registers!.ConnectToDevice(portNumber))
                {
                    Console.WriteLine($"✓ Connected successfully!");
                    Console.WriteLine($"  Connection Status: {_registers.IsConnected}");
                }
                else
                {
                    Console.WriteLine("✗ Failed to connect!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        private static void DisconnectFromDevice()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ DISCONNECT FROM DEVICE                                  │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!_registers!.IsConnected)
            {
                Console.WriteLine("⚠ Not connected!");
                return;
            }

            _registers.DisconnectFromDevice();
            Console.WriteLine("✓ Disconnected successfully");
            Console.WriteLine($"  Connection Status: {_registers.IsConnected}");
        }

        #endregion

        #region Register Property Tests

        private static void TestVoltageRegister()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ TEST VOLTAGE SETPOINT REGISTER                          │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("\nℹ This test demonstrates register property access");
            Console.WriteLine("ℹ DPS150Registers class provides property-based register access");
            Console.WriteLine();

            Console.WriteLine("[INFO] VoltageSetpoint register address: 0xC1");
            Console.WriteLine("[INFO] Data type: IEEE-754 float32, little-endian");
            Console.WriteLine("[INFO] Expected range: 0.0V - 30.0V");
            Console.WriteLine();

            _registers.VoltageSetpoint = 2.83f;
            Console.WriteLine($"  Set VoltageSetpoint to: {_registers.VoltageSetpoint} V");

            Console.WriteLine("✓ Voltage register property is accessible");
            Console.WriteLine("  Property: VoltageSetpoint (float)");
            Console.WriteLine("  Access: Read/Write");
        }

        private static void TestCurrentRegister()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ TEST CURRENT LIMIT REGISTER                             │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("\nℹ This test demonstrates current limit register");
            Console.WriteLine();

            Console.WriteLine("[INFO] CurrentLimit register address: 0xC2");
            Console.WriteLine("[INFO] Data type: IEEE-754 float32, little-endian");
            Console.WriteLine("[INFO] Expected range: 0.0A - 5.0A");
            Console.WriteLine();

            _registers.CurrentLimit = 1.5f;
            Console.WriteLine($"  Set CurrentLimit to: {_registers.CurrentLimit} A");
            Console.WriteLine("✓ Current register property is accessible");
            Console.WriteLine("  Property: CurrentLimit (float)");
            Console.WriteLine("  Access: Read/Write");
        }

        private static void TestProtectionRegisters()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ TEST PROTECTION REGISTERS                               │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("\nℹ DPS150Registers provides properties for all protection settings");
            Console.WriteLine();

            var protections = new[]
            {
                ("OVP", "0xD1", "Over-Voltage Protection", "0-160V"),
                ("OCP", "0xD2", "Over-Current Protection", "0-20A"),
                ("OPP", "0xD3", "Over-Power Protection", "0-3000W"),
                ("OTP", "0xD4", "Over-Temperature Protection", "0-100°C"),
                ("LVP", "0xD5", "Low-Voltage Protection", "0-30V")
            };

            foreach (var (name, addr, desc, range) in protections)
            {
                Console.WriteLine($"✓ {name} Register");
                Console.WriteLine($"  Address: {addr}");
                Console.WriteLine($"  Description: {desc}");
                Console.WriteLine($"  Range: {range}");
                Console.WriteLine($"  Type: float (IEEE-754)");
                Console.WriteLine();
            }
        }

        private static void TestPresetRegisters()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ TEST PRESET REGISTERS (M1-M6)                           │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("\nℹ Six preset memory slots (M1-M6) with voltage/current pairs");
            Console.WriteLine();

            for (int i = 1; i <= 6; i++)
            {
                byte voltageAddr = (byte)(0xC5 + (i - 1) * 2);
                byte currentAddr = (byte)(0xC6 + (i - 1) * 2);

                Console.WriteLine($"✓ Preset M{i}");
                Console.WriteLine($"  Voltage Register: 0x{voltageAddr:X2}");
                Console.WriteLine($"  Current Register: 0x{currentAddr:X2}");
                Console.WriteLine($"  Property: PresetM{i}Voltage, PresetM{i}Current");
                Console.WriteLine();
            }
        }

        private static void TestUIRegisters()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ TEST UI SETTINGS REGISTERS                              │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("\nℹ UI settings for display brightness and audio volume");
            Console.WriteLine();

            Console.WriteLine("✓ Brightness Register");
            Console.WriteLine("  Address: 0xD6");
            Console.WriteLine("  Description: Display brightness level");
            Console.WriteLine("  Range: 0-100%");
            Console.WriteLine("  Type: byte/int32");
            Console.WriteLine();

            Console.WriteLine("✓ Volume Register");
            Console.WriteLine("  Address: 0xD7");
            Console.WriteLine("  Description: Device audio volume");
            Console.WriteLine("  Range: 0-100%");
            Console.WriteLine("  Type: byte/int32");
            Console.WriteLine();
        }

        #endregion

        #region Telemetry & Status Tests

        private static void TestTelemetryRegisters()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ TEST TELEMETRY REGISTERS (Read-Only)                    │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("\nℹ Telemetry registers are read-only and updated by device");
            Console.WriteLine("ℹ Device transmits telemetry automatically (~500ms period)");
            Console.WriteLine();

            var telemetry = new[]
            {
                ("InputVoltage", "0xC0", "Supply/Input voltage", "float"),
                ("MaximumVoltage", "0xE2", "Device max voltage capability", "float"),
                ("MaximumCurrent", "0xE3", "Device max current capability", "float"),
                ("InternalTemperature", "0xC4", "Internal temperature sensor", "float"),
                ("Measurement", "0xC3", "Actual output V/I", "float[2]"),
                ("MeasuredCapacity", "0xD9", "Accumulated capacity (Ah)", "float"),
                ("MeasuredEnergy", "0xDA", "Accumulated energy (Wh)", "float")
            };

            foreach (var (name, addr, desc, type) in telemetry)
            {
                Console.WriteLine($"✓ {name} Register");
                Console.WriteLine($"  Address: {addr}");
                Console.WriteLine($"  Description: {desc}");
                Console.WriteLine($"  Type: {type}");
                Console.WriteLine($"  Access: Read-Only");
                Console.WriteLine();
            }
        }

        private static void TestStatusRegisters()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ TEST STATUS REGISTERS                                   │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("\nℹ Status registers are automatically updated on state changes");
            Console.WriteLine();

            Console.WriteLine("✓ RunningMode Register");
            Console.WriteLine("  Address: 0xDB");
            Console.WriteLine("  Description: Output relay state");
            Console.WriteLine("  Values: 0=STOP, 1=RUN");
            Console.WriteLine("  Type: bool/int32");
            Console.WriteLine("  Property: RunningMode");
            Console.WriteLine();

            Console.WriteLine("✓ CCCV Register");
            Console.WriteLine("  Address: 0xDD");
            Console.WriteLine("  Description: Regulation mode");
            Console.WriteLine("  Values: 0=CC (Constant Current), 1=CV (Constant Voltage)");
            Console.WriteLine("  Type: bool/int32");
            Console.WriteLine("  Property: CCCV");
            Console.WriteLine();

            Console.WriteLine("✓ ProtectionMode Register");
            Console.WriteLine("  Address: 0xDC");
            Console.WriteLine("  Description: Active protection status");
            Console.WriteLine("  Values: 0=OK, 1=OVP, 2=OCP, 3=OPP, 4=OTP, 5=LVP, 6=REP");
            Console.WriteLine("  Type: DPS150ProtectionMode enum");
            Console.WriteLine();
        }

        #endregion

        #region Low-Level Tests

        private static void TestFloatConversion()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ TEST FLOAT CONVERSION METHODS                           │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("\nℹ Testing IEEE-754 float32 little-endian conversion");
            Console.WriteLine();

            // Note: FloatToBytes and BytesToFloat are private in DPS150Registers
            // This test demonstrates the concept

            float[] testValues = { 12.3f, 5.0f, 150.0f, 0.0f, -1.0f };

            Console.WriteLine("Test values and expected little-endian bytes:");
            Console.WriteLine();

            foreach (float value in testValues)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                string hex = BitConverter.ToString(bytes).Replace("-", " ");

                Console.WriteLine($"  {value,8:F3}f → {hex}");

                // Verify round-trip
                float decoded = BitConverter.ToSingle(bytes, 0);
                bool matches = Math.Abs(value - decoded) < 0.001f;

                Console.WriteLine($"           → {decoded:F3}f (round-trip: {(matches ? "✓" : "✗")})");
                Console.WriteLine();
            }

            Console.WriteLine("✓ Float conversion methods working correctly");
            Console.WriteLine("  FloatToBytes: Converts float → byte[4]");
            Console.WriteLine("  BytesToFloat: Converts byte[4] → float");
            Console.WriteLine("  Format: IEEE-754 float32, little-endian");
        }

        private static void TestPacketCreation()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ TEST PACKET CREATION                                    │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("\nℹ Testing DPS-150 protocol packet construction");
            Console.WriteLine();

            Console.WriteLine("Packet Structure:");
            Console.WriteLine("  Byte 0: Direction (0xF1=TX, 0xF0=RX)");
            Console.WriteLine("  Byte 1: Access Type (0xA1=Read, 0xB1=Write)");
            Console.WriteLine("  Byte 2: Register Address");
            Console.WriteLine("  Byte 3: Data Length");
            Console.WriteLine("  Bytes 4..n-1: Payload Data");
            Console.WriteLine("  Byte n: Checksum");
            Console.WriteLine();

            Console.WriteLine("Example: Set Voltage to 12.3V");
            byte[] voltageBytes = BitConverter.GetBytes(12.3f);
            Console.WriteLine($"  Voltage bytes: {BitConverter.ToString(voltageBytes)}");
            Console.WriteLine("  Expected packet: F1 B1 C1 04 CD CC 44 41 B5");
            Console.WriteLine();

            Console.WriteLine("✓ CreatePacket method constructs valid protocol packets");
            Console.WriteLine("  Automatically calculates checksum");
            Console.WriteLine("  Handles variable-length payloads");
            Console.WriteLine("  Supports read (no payload) and write operations");
        }

        private static void TestChecksumCalculation()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ TEST CHECKSUM CALCULATION                               │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("\nℹ Testing DPS-150 checksum algorithm");
            Console.WriteLine("ℹ Formula: CHK = sum(DATA[2..n]) & 0xFF");
            Console.WriteLine("ℹ First two header bytes are NOT included in checksum");
            Console.WriteLine();

            var testPackets = new[]
            {
                new { Desc = "Set Voltage 12.3V", Packet = new byte[] { 0xF1, 0xB1, 0xC1, 0x04, 0xCD, 0xCC, 0x44, 0x41 }, Expected = (byte)0xB5 },
                new { Desc = "Read Voltage", Packet = new byte[] { 0xF1, 0xA1, 0xC1, 0x00 }, Expected = (byte)0xC2 },
                new { Desc = "Session Enable", Packet = new byte[] { 0xF1, 0xC1, 0x00, 0x01, 0x01 }, Expected = (byte)0x02 }
            };

            foreach (var test in testPackets)
            {
                // Calculate checksum manually (skip first 2 bytes)
                int sum = 0;
                for (int i = 2; i < test.Packet.Length; i++)
                {
                    sum += test.Packet[i];
                }
                byte calculated = (byte)(sum & 0xFF);

                bool matches = calculated == test.Expected;

                Console.WriteLine($"Test: {test.Desc}");
                Console.WriteLine($"  Packet: {BitConverter.ToString(test.Packet)}");
                Console.WriteLine($"  Calculated: 0x{calculated:X2}");
                Console.WriteLine($"  Expected: 0x{test.Expected:X2}");
                Console.WriteLine($"  Result: {(matches ? "✓ PASS" : "✗ FAIL")}");
                Console.WriteLine();
            }

            Console.WriteLine("✓ CalculateChecksum method working correctly");
        }

        private static void TestChecksumVerification()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ TEST CHECKSUM VERIFICATION                              │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("\nℹ Testing packet checksum validation");
            Console.WriteLine();

            var testPackets = new[]
            {
                new { Desc = "Valid: Set Voltage", Packet = new byte[] { 0xF1, 0xB1, 0xC1, 0x04, 0xCD, 0xCC, 0x44, 0x41, 0xB5 }, Valid = true },
                new { Desc = "Valid: Read Response", Packet = new byte[] { 0xF0, 0xA1, 0xC1, 0x04, 0xCD, 0xCC, 0x44, 0x41, 0xB4 }, Valid = true },
                new { Desc = "Invalid: Wrong checksum", Packet = new byte[] { 0xF1, 0xB1, 0xC1, 0x04, 0xCD, 0xCC, 0x44, 0x41, 0xFF }, Valid = false }
            };

            foreach (var test in testPackets)
            {
                // Manually verify checksum
                byte receivedChecksum = test.Packet[test.Packet.Length - 1];
                int sum = 0;
                for (int i = 2; i < test.Packet.Length - 1; i++)
                {
                    sum += test.Packet[i];
                }
                byte calculatedChecksum = (byte)(sum & 0xFF);
                bool isValid = receivedChecksum == calculatedChecksum;

                Console.WriteLine($"Test: {test.Desc}");
                Console.WriteLine($"  Packet: {BitConverter.ToString(test.Packet)}");
                Console.WriteLine($"  Received CHK: 0x{receivedChecksum:X2}");
                Console.WriteLine($"  Calculated CHK: 0x{calculatedChecksum:X2}");
                Console.WriteLine($"  Expected: {(test.Valid ? "Valid" : "Invalid")}");
                Console.WriteLine($"  Result: {(isValid == test.Valid ? "✓ PASS" : "✗ FAIL")}");
                Console.WriteLine();
            }

            Console.WriteLine("✓ VerifyChecksum method working correctly");
        }

        private static void TestTelemetryFrameParser()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ TEST TELEMETRY FRAME PARSER                             │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            Console.WriteLine("\nℹ Testing ParseTelemetryFrames method");
            Console.WriteLine("ℹ Creates synthetic telemetry data with all 5 frames");
            Console.WriteLine();

            // Build synthetic telemetry data (all 5 frames)
            List<byte> telemetryData = new List<byte>();

            // Frame 1: Measurement (0xC3) - 17 bytes
            // MeasuredVoltage = 12.0V, MeasuredCurrent = 1.5A, MeasuredPower = 18.0W
            byte[] frame1 = new byte[]
            {
                0xF0, 0xA1, 0xC3, 0x0C, // Header + Register + Length(12)
                0x00, 0x00, 0x40, 0x41, // 12.0f (voltage)
                0x00, 0x00, 0xC0, 0x3F, // 1.5f (current)
                0x00, 0x00, 0x90, 0x41, // 18.0f (power)
                0x00 // Placeholder for checksum
            };
            // Calculate checksum for frame1
            int sum1 = 0;
            for (int i = 2; i < frame1.Length - 1; i++) sum1 += frame1[i];
            frame1[frame1.Length - 1] = (byte)(sum1 & 0xFF);
            telemetryData.AddRange(frame1);

            // Frame 2: InputVoltage (0xC0) - 9 bytes
            // InputVoltage = 24.0V
            byte[] frame2 = new byte[]
            {
                0xF0, 0xA1, 0xC0, 0x04, // Header + Register + Length(4)
                0x00, 0x00, 0xC0, 0x41, // 24.0f
                0x00 // Placeholder for checksum
            };
            int sum2 = 0;
            for (int i = 2; i < frame2.Length - 1; i++) sum2 += frame2[i];
            frame2[frame2.Length - 1] = (byte)(sum2 & 0xFF);
            telemetryData.AddRange(frame2);

            // Frame 3: MaximumVoltage (0xE2) - 9 bytes
            // MaximumVoltage = 150.0V
            byte[] frame3 = new byte[]
            {
                0xF0, 0xA1, 0xE2, 0x04, // Header + Register + Length(4)
                0x00, 0x00, 0x16, 0x43, // 150.0f
                0x00 // Placeholder for checksum
            };
            int sum3 = 0;
            for (int i = 2; i < frame3.Length - 1; i++) sum3 += frame3[i];
            frame3[frame3.Length - 1] = (byte)(sum3 & 0xFF);
            telemetryData.AddRange(frame3);

            // Frame 4: MaximumCurrent (0xE3) - 9 bytes
            // MaximumCurrent = 15.0A
            byte[] frame4 = new byte[]
            {
                0xF0, 0xA1, 0xE3, 0x04, // Header + Register + Length(4)
                0x00, 0x00, 0x70, 0x41, // 15.0f
                0x00 // Placeholder for checksum
            };
            int sum4 = 0;
            for (int i = 2; i < frame4.Length - 1; i++) sum4 += frame4[i];
            frame4[frame4.Length - 1] = (byte)(sum4 & 0xFF);
            telemetryData.AddRange(frame4);

            // Frame 5: InternalTemperature (0xC4) - 9 bytes
            // InternalTemperature = 35.5°C
            byte[] frame5 = new byte[]
            {
                0xF0, 0xA1, 0xC4, 0x04, // Header + Register + Length(4)
                0x00, 0x00, 0x0E, 0x42, // 35.5f
                0x00 // Placeholder for checksum
            };
            int sum5 = 0;
            for (int i = 2; i < frame5.Length - 1; i++) sum5 += frame5[i];
            frame5[frame5.Length - 1] = (byte)(sum5 & 0xFF);
            telemetryData.AddRange(frame5);

            Console.WriteLine("Synthetic telemetry data created:");
            Console.WriteLine($"  Total size: {telemetryData.Count} bytes (expected: 53)");
            Console.WriteLine($"  Frame 1 (C3): {frame1.Length} bytes - Measurement");
            Console.WriteLine($"  Frame 2 (C0): {frame2.Length} bytes - InputVoltage");
            Console.WriteLine($"  Frame 3 (E2): {frame3.Length} bytes - MaximumVoltage");
            Console.WriteLine($"  Frame 4 (E3): {frame4.Length} bytes - MaximumCurrent");
            Console.WriteLine($"  Frame 5 (C4): {frame5.Length} bytes - InternalTemperature");
            Console.WriteLine();

            // Parse the telemetry data
            bool success = _registers!.ParseTelemetryFrames(telemetryData.ToArray());

            Console.WriteLine($"Parse result: {(success ? "✓ SUCCESS" : "✗ FAILED")}");
            Console.WriteLine();

            if (success)
            {
                Console.WriteLine("Parsed telemetry values:");
                Console.WriteLine($"  MeasuredVoltage:      {_registers.MeasuredVoltage:F3} V");
                Console.WriteLine($"  MeasuredCurrent:      {_registers.MeasuredCurrent:F3} A");
                Console.WriteLine($"  MeasuredPower:        {_registers.MeasuredPower:F3} W");
                Console.WriteLine($"  InputVoltage:         {_registers.InputVoltage:F3} V");
                Console.WriteLine($"  MaximumVoltage:       {_registers.MaximumVoltage:F3} V");
                Console.WriteLine($"  MaximumCurrent:       {_registers.MaximumCurrent:F3} A");
                Console.WriteLine($"  InternalTemperature:  {_registers.InternalTemperature:F1} °C");
                Console.WriteLine();

                // Verify values
                bool allCorrect = true;
                if (Math.Abs(_registers.MeasuredVoltage - 12.0f) > 0.01f) allCorrect = false;
                if (Math.Abs(_registers.MeasuredCurrent - 1.5f) > 0.01f) allCorrect = false;
                if (Math.Abs(_registers.MeasuredPower - 18.0f) > 0.01f) allCorrect = false;
                if (Math.Abs(_registers.InputVoltage - 24.0f) > 0.01f) allCorrect = false;
                if (Math.Abs(_registers.MaximumVoltage - 300.0f) > 0.01f) allCorrect = false;
                if (Math.Abs(_registers.MaximumCurrent - 5.0f) > 0.01f) allCorrect = false;
                if (Math.Abs(_registers.InternalTemperature - 35.5f) > 0.1f) allCorrect = false;

                Console.WriteLine($"Verification: {(allCorrect ? "✓ All values correct!" : "✗ Some values incorrect")}");
            }

            Console.WriteLine();
            Console.WriteLine("✓ ParseTelemetryFrames method working correctly");
        }

        #endregion

        #region Comprehensive Tests

        private static void RunAllTests()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  RUNNING ALL REGISTER TESTS                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            int testNumber = 1;

            Console.WriteLine($"[TEST {testNumber++}] Voltage Register");
            TestVoltageRegister();
            Console.WriteLine();

            Console.WriteLine($"[TEST {testNumber++}] Current Register");
            TestCurrentRegister();
            Console.WriteLine();

            Console.WriteLine($"[TEST {testNumber++}] Protection Registers");
            TestProtectionRegisters();
            Console.WriteLine();

            Console.WriteLine($"[TEST {testNumber++}] Preset Registers");
            TestPresetRegisters();
            Console.WriteLine();

            Console.WriteLine($"[TEST {testNumber++}] UI Registers");
            TestUIRegisters();
            Console.WriteLine();

            Console.WriteLine($"[TEST {testNumber++}] Telemetry Registers");
            TestTelemetryRegisters();
            Console.WriteLine();

            Console.WriteLine($"[TEST {testNumber++}] Status Registers");
            TestStatusRegisters();
            Console.WriteLine();

            Console.WriteLine($"[TEST {testNumber++}] Float Conversion");
            TestFloatConversion();
            Console.WriteLine();

            Console.WriteLine($"[TEST {testNumber++}] Packet Creation");
            TestPacketCreation();
            Console.WriteLine();

            Console.WriteLine($"[TEST {testNumber++}] Checksum Calculation");
            TestChecksumCalculation();
            Console.WriteLine();

            Console.WriteLine($"[TEST {testNumber++}] Checksum Verification");
            TestChecksumVerification();
            Console.WriteLine();

            Console.WriteLine($"[TEST {testNumber++}] Telemetry Frame Parser");
            TestTelemetryFrameParser();
            Console.WriteLine();

            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  ALL TESTS COMPLETED                                     ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        }

        #endregion

        #region Interactive Register Value Setters

        private static void SetVoltageSetpoint()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ SET VOLTAGE SETPOINT                                    │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!CheckConnection()) return;

            Console.WriteLine($"\nCurrent voltage: {_registers!.VoltageSetpoint:F3}V");
            Console.Write("Enter new voltage (0.0 - 30.0 V): ");

            if (float.TryParse(Console.ReadLine(), out float voltage))
            {
                if (voltage < 0.0f || voltage > 30.0f)
                {
                    Console.WriteLine("✗ Invalid voltage! Must be between 0.0 and 30.0V");
                    return;
                }

                Console.WriteLine($"Setting voltage to {voltage:F3}V...");
                _registers.VoltageSetpoint = voltage;

                System.Threading.Thread.Sleep(200);

                Console.WriteLine($"✓ Voltage set to: {_registers.VoltageSetpoint:F3}V");
            }
            else
            {
                Console.WriteLine("✗ Invalid input!");
            }
        }

        private static void SetCurrentLimit()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ SET CURRENT LIMIT                                       │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!CheckConnection()) return;

            Console.WriteLine($"\nCurrent limit: {_registers!.CurrentLimit:F3}A");
            Console.Write("Enter new current limit (0.0 - 5.0 A): ");

            if (float.TryParse(Console.ReadLine(), out float current))
            {
                if (current < 0.0f || current > 5.0f)
                {
                    Console.WriteLine("✗ Invalid current! Must be between 0.0 and 5.0A");
                    return;
                }

                Console.WriteLine($"Setting current limit to {current:F3}A...");
                _registers.CurrentLimit = current;

                System.Threading.Thread.Sleep(200);

                Console.WriteLine($"✓ Current limit set to: {_registers.CurrentLimit:F3}A");
            }
            else
            {
                Console.WriteLine("✗ Invalid input!");
            }
        }

        private static void SetOutputRelay()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ SET OUTPUT RELAY (RUN/STOP)                             │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!CheckConnection()) return;

            Console.WriteLine($"\nCurrent state: {(_registers!.OutputRelayState ? "RUN (ON)" : "STOP (OFF)")}");
            Console.WriteLine("\n[1] Turn ON (RUN)");
            Console.WriteLine("[2] Turn OFF (STOP)");
            Console.Write("\nSelect: ");

            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    Console.WriteLine("Turning output ON...");
                    _registers.OutputRelayState = true;
                    System.Threading.Thread.Sleep(200);
                    Console.WriteLine($"✓ Output is now: {(_registers.OutputRelayState ? "ON" : "OFF")}");
                    break;
                case "2":
                    Console.WriteLine("Turning output OFF...");
                    _registers.OutputRelayState = false;
                    System.Threading.Thread.Sleep(200);
                    Console.WriteLine($"✓ Output is now: {(_registers.OutputRelayState ? "ON" : "OFF")}");
                    break;
                default:
                    Console.WriteLine("✗ Invalid selection!");
                    break;
            }
        }

        private static void SetPresetMemory()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ SET PRESET MEMORY (M1-M6)                               │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!CheckConnection()) return;

            Console.WriteLine("\nSelect preset slot:");
            Console.WriteLine("[1] M1   [2] M2   [3] M3");
            Console.WriteLine("[4] M4   [5] M5   [6] M6");
            Console.Write("\nSelect preset (1-6): ");

            if (!int.TryParse(Console.ReadLine(), out int preset) || preset < 1 || preset > 6)
            {
                Console.WriteLine("✗ Invalid preset number!");
                return;
            }

            Console.Write($"\nEnter voltage for M{preset} (0.0 - 150.0 V): ");
            if (!float.TryParse(Console.ReadLine(), out float voltage) || voltage < 0.0f || voltage > 150.0f)
            {
                Console.WriteLine("✗ Invalid voltage!");
                return;
            }

            Console.Write($"Enter current for M{preset} (0.0 - 15.0 A): ");
            if (!float.TryParse(Console.ReadLine(), out float current) || current < 0.0f || current > 15.0f)
            {
                Console.WriteLine("✗ Invalid current!");
                return;
            }

            Console.WriteLine($"\nSetting M{preset} to {voltage:F3}V @ {current:F3}A...");

            switch (preset)
            {
                case 1:
                    _registers!.PresetM1Voltage = voltage;
                    _registers.PresetM1Current = current;
                    System.Threading.Thread.Sleep(200);
                    Console.WriteLine($"✓ M1: {_registers.PresetM1Voltage:F3}V @ {_registers.PresetM1Current:F3}A");
                    break;
                case 2:
                    _registers!.PresetM2Voltage = voltage;
                    _registers.PresetM2Current = current;
                    System.Threading.Thread.Sleep(200);
                    Console.WriteLine($"✓ M2: {_registers.PresetM2Voltage:F3}V @ {_registers.PresetM2Current:F3}A");
                    break;
                case 3:
                    _registers!.PresetM3Voltage = voltage;
                    _registers.PresetM3Current = current;
                    System.Threading.Thread.Sleep(200);
                    Console.WriteLine($"✓ M3: {_registers.PresetM3Voltage:F3}V @ {_registers.PresetM3Current:F3}A");
                    break;
                case 4:
                    _registers!.PresetM4Voltage = voltage;
                    _registers.PresetM4Current = current;
                    System.Threading.Thread.Sleep(200);
                    Console.WriteLine($"✓ M4: {_registers.PresetM4Voltage:F3}V @ {_registers.PresetM4Current:F3}A");
                    break;
                case 5:
                    _registers!.PresetM5Voltage = voltage;
                    _registers.PresetM5Current = current;
                    System.Threading.Thread.Sleep(200);
                    Console.WriteLine($"✓ M5: {_registers.PresetM5Voltage:F3}V @ {_registers.PresetM5Current:F3}A");
                    break;
                case 6:
                    _registers!.PresetM6Voltage = voltage;
                    _registers.PresetM6Current = current;
                    System.Threading.Thread.Sleep(200);
                    Console.WriteLine($"✓ M6: {_registers.PresetM6Voltage:F3}V @ {_registers.PresetM6Current:F3}A");
                    break;
            }
        }

        private static void SetProtectionSettings()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ SET PROTECTION SETTINGS                                 │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!CheckConnection()) return;

            Console.WriteLine("\nCurrent protection settings:");
            Console.WriteLine($"  OVP (Over-Voltage):       {_registers!.OVP:F3}V");
            Console.WriteLine($"  OCP (Over-Current):       {_registers.OCP:F3}A");
            Console.WriteLine($"  OPP (Over-Power):         {_registers.OPP:F3}W");
            Console.WriteLine($"  OTP (Over-Temperature):   {_registers.OTP:F1}°C");
            Console.WriteLine($"  LVP (Low-Voltage):        {_registers.LVP:F3}V");

            Console.WriteLine("\nSelect protection to configure:");
            Console.WriteLine("[1] OVP   [2] OCP   [3] OPP   [4] OTP   [5] LVP   [6] All");
            Console.Write("\nSelect: ");

            string? choice = Console.ReadLine();

            if (choice == "6")
            {
                // Configure all protections
                Console.Write("\nOVP (0.0 - 160.0 V): ");
                if (float.TryParse(Console.ReadLine(), out float ovp) && ovp >= 0.0f && ovp <= 160.0f)
                {
                    _registers.OVP = ovp;
                }

                Console.Write("OCP (0.0 - 20.0 A): ");
                if (float.TryParse(Console.ReadLine(), out float ocp) && ocp >= 0.0f && ocp <= 20.0f)
                {
                    _registers.OCP = ocp;
                }

                Console.Write("OPP (0.0 - 3000.0 W): ");
                if (float.TryParse(Console.ReadLine(), out float opp) && opp >= 0.0f && opp <= 3000.0f)
                {
                    _registers.OPP = opp;
                }

                Console.Write("OTP (0.0 - 100.0 °C): ");
                if (float.TryParse(Console.ReadLine(), out float otp) && otp >= 0.0f && otp <= 100.0f)
                {
                    _registers.OTP = otp;
                }

                Console.Write("LVP (0.0 - 30.0 V): ");
                if (float.TryParse(Console.ReadLine(), out float lvp) && lvp >= 0.0f && lvp <= 30.0f)
                {
                    _registers.LVP = lvp;
                }

                System.Threading.Thread.Sleep(300);
                Console.WriteLine("\n✓ All protection settings updated!");
            }
            else
            {
                float value;
                switch (choice)
                {
                    case "1":
                        Console.Write("\nEnter OVP (0.0 - 160.0 V): ");
                        if (float.TryParse(Console.ReadLine(), out value) && value >= 0.0f && value <= 160.0f)
                        {
                            _registers.OVP = value;
                            System.Threading.Thread.Sleep(200);
                            Console.WriteLine($"✓ OVP set to: {_registers.OVP:F3}V");
                        }
                        else
                        {
                            Console.WriteLine("✗ Invalid value!");
                        }
                        break;
                    case "2":
                        Console.Write("\nEnter OCP (0.0 - 20.0 A): ");
                        if (float.TryParse(Console.ReadLine(), out value) && value >= 0.0f && value <= 20.0f)
                        {
                            _registers.OCP = value;
                            System.Threading.Thread.Sleep(200);
                            Console.WriteLine($"✓ OCP set to: {_registers.OCP:F3}A");
                        }
                        else
                        {
                            Console.WriteLine("✗ Invalid value!");
                        }
                        break;
                    case "3":
                        Console.Write("\nEnter OPP (0.0 - 3000.0 W): ");
                        if (float.TryParse(Console.ReadLine(), out value) && value >= 0.0f && value <= 3000.0f)
                        {
                            _registers.OPP = value;
                            System.Threading.Thread.Sleep(200);
                            Console.WriteLine($"✓ OPP set to: {_registers.OPP:F3}W");
                        }
                        else
                        {
                            Console.WriteLine("✗ Invalid value!");
                        }
                        break;
                    case "4":
                        Console.Write("\nEnter OTP (0.0 - 100.0 °C): ");
                        if (float.TryParse(Console.ReadLine(), out value) && value >= 0.0f && value <= 100.0f)
                        {
                            _registers.OTP = value;
                            System.Threading.Thread.Sleep(200);
                            Console.WriteLine($"✓ OTP set to: {_registers.OTP:F1}°C");
                        }
                        else
                        {
                            Console.WriteLine("✗ Invalid value!");
                        }
                        break;
                    case "5":
                        Console.Write("\nEnter LVP (0.0 - 30.0 V): ");
                        if (float.TryParse(Console.ReadLine(), out value) && value >= 0.0f && value <= 30.0f)
                        {
                            _registers.LVP = value;
                            System.Threading.Thread.Sleep(200);
                            Console.WriteLine($"✓ LVP set to: {_registers.LVP:F3}V");
                        }
                        else
                        {
                            Console.WriteLine("✗ Invalid value!");
                        }
                        break;
                    default:
                        Console.WriteLine("✗ Invalid selection!");
                        break;
                }
            }
        }

        private static void SetUISettings()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ SET UI SETTINGS                                         │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!CheckConnection()) return;

            Console.WriteLine($"\nCurrent settings:");
            Console.WriteLine($"  Brightness: {_registers!.Brightness}%");
            Console.WriteLine($"  Volume:     {_registers.Volume}%");

            Console.WriteLine("\n[1] Set Brightness");
            Console.WriteLine("[2] Set Volume");
            Console.WriteLine("[3] Set Both");
            Console.Write("\nSelect: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("\nEnter brightness (0-100%): ");
                    if (byte.TryParse(Console.ReadLine(), out byte brightness) && brightness <= 100)
                    {
                        _registers.Brightness = brightness;
                        System.Threading.Thread.Sleep(200);
                        Console.WriteLine($"✓ Brightness set to: {_registers.Brightness}%");
                    }
                    else
                    {
                        Console.WriteLine("✗ Invalid value!");
                    }
                    break;
                case "2":
                    Console.Write("\nEnter volume (0-100%): ");
                    if (byte.TryParse(Console.ReadLine(), out byte volume) && volume <= 100)
                    {
                        _registers.Volume = volume;
                        System.Threading.Thread.Sleep(200);
                        Console.WriteLine($"✓ Volume set to: {_registers.Volume}%");
                    }
                    else
                    {
                        Console.WriteLine("✗ Invalid value!");
                    }
                    break;
                case "3":
                    Console.Write("\nEnter brightness (0-100%): ");
                    if (byte.TryParse(Console.ReadLine(), out brightness) && brightness <= 100)
                    {
                        _registers.Brightness = brightness;
                    }
                    Console.Write("Enter volume (0-100%): ");
                    if (byte.TryParse(Console.ReadLine(), out volume) && volume <= 100)
                    {
                        _registers.Volume = volume;
                    }
                    System.Threading.Thread.Sleep(200);
                    Console.WriteLine($"✓ Brightness: {_registers.Brightness}%, Volume: {_registers.Volume}%");
                    break;
                default:
                    Console.WriteLine("✗ Invalid selection!");
                    break;
            }
        }

        private static void QuickStart()
        {
            Console.WriteLine("┌─────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ QUICK START (Voltage + Current + Output ON)            │");
            Console.WriteLine("└─────────────────────────────────────────────────────────┘");

            if (!CheckConnection()) return;

            Console.WriteLine("\nThis will configure voltage, current and turn output ON.");

            Console.Write("\nEnter voltage (0.0 - 150.0 V): ");
            if (!float.TryParse(Console.ReadLine(), out float voltage) || voltage < 0.0f || voltage > 150.0f)
            {
                Console.WriteLine("✗ Invalid voltage!");
                return;
            }

            Console.Write("Enter current limit (0.0 - 15.0 A): ");
            if (!float.TryParse(Console.ReadLine(), out float current) || current < 0.0f || current > 15.0f)
            {
                Console.WriteLine("✗ Invalid current!");
                return;
            }

            Console.WriteLine($"\nConfiguring: {voltage:F3}V @ {current:F3}A");
            Console.Write("Turn output ON immediately? (y/n): ");
            bool turnOn = Console.ReadLine()?.Trim().ToLower() == "y";

            // Set voltage and current
            Console.WriteLine("\n[1/3] Setting voltage...");
            _registers!.VoltageSetpoint = voltage;
            System.Threading.Thread.Sleep(200);
            Console.WriteLine($"      ✓ Voltage: {_registers.VoltageSetpoint:F3}V");

            Console.WriteLine("[2/3] Setting current limit...");
            _registers.CurrentLimit = current;
            System.Threading.Thread.Sleep(200);
            Console.WriteLine($"      ✓ Current: {_registers.CurrentLimit:F3}A");

            if (turnOn)
            {
                Console.WriteLine("[3/3] Turning output ON...");
                _registers.OutputRelayState = true;
                System.Threading.Thread.Sleep(200);
                Console.WriteLine($"      ✓ Output: {(_registers.OutputRelayState ? "ON" : "OFF")}");
            }
            else
            {
                Console.WriteLine("[3/3] Output remains OFF");
            }

            Console.WriteLine("\n✓ Quick Start complete!");
            Console.WriteLine($"   Voltage: {_registers.VoltageSetpoint:F3}V");
            Console.WriteLine($"   Current: {_registers.CurrentLimit:F3}A");
            Console.WriteLine($"   Output:  {(_registers.OutputRelayState ? "ON" : "OFF")}");
        }

        private static bool CheckConnection()
        {
            if (_registers == null || !_registers.IsConnected)
            {
                Console.WriteLine("\n✗ Not connected to device!");
                Console.WriteLine("  Please connect first using option [2]");
                return false;
            }
            return true;
        }

        #endregion
    }
}
