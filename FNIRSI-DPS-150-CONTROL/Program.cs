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

using FNIRSI_DPS_150_CONTROL;
using System.Text;

namespace FNIRSI_DPS_150_CONTROL
{
    /// <summary>
    /// Test console application for the DPS150Communication class.
    /// Provides an interactive menu to test all communication features.
    /// </summary>
    class Program
    {
        private static DPS150Communication? _communication;
        private static bool _isRunning = true;
        private static int _receivedMessageCount = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  FNIRSI-DPS-150 Communication Test Application          ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            _communication = new DPS150Communication();
            _communication.DataReceived += OnDataReceived;

            while (_isRunning)
            {
                ShowMenu();
                ProcessUserInput();
            }

            // Clean up
            if (_communication.IsConnected)
            {
                _communication.StopReceiving();
                _communication.Disconnect();
            }

            Console.WriteLine("\nApplication terminated. Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Displays the main menu with all available test options.
        /// </summary>
        private static void ShowMenu()
        {
            Console.WriteLine("\n" + new string('─', 60));
            Console.WriteLine($"Connection Status: {(_communication?.IsConnected == true ? "CONNECTED ✓" : "DISCONNECTED ✗")}");
            Console.WriteLine($"Received Messages: {_receivedMessageCount}");
            Console.WriteLine(new string('─', 60));
            Console.WriteLine("1. List Available Ports");
            Console.WriteLine("2. Connect to Device");
            Console.WriteLine("3. Disconnect from Device");
            Console.WriteLine("4. Send Test Data");
            Console.WriteLine("5. Read Response");
            Console.WriteLine("6. Send Command and Get Response");
            Console.WriteLine("7. Start Continuous Receiving");
            Console.WriteLine("8. Stop Continuous Receiving");
            Console.WriteLine("9. Test Complete Communication Cycle");
            Console.WriteLine("T. Test Checksum Calculation");
            Console.WriteLine("0. Exit Application");
            Console.WriteLine(new string('─', 60));
            Console.Write("Select option: ");
        }

        /// <summary>
        /// Processes user input and executes the selected menu option.
        /// </summary>
        private static void ProcessUserInput()
        {
            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    ListAvailablePorts();
                    break;
                case "2":
                    ConnectToDevice();
                    break;
                case "3":
                    DisconnectFromDevice();
                    break;
                case "4":
                    SendTestData();
                    break;
                case "5":
                    ReadResponse();
                    break;
                case "6":
                    SendCommandAndGetResponse();
                    break;
                case "7":
                    StartContinuousReceiving();
                    break;
                case "8":
                    StopContinuousReceiving();
                    break;
                case "9":
                    TestCompleteCycle();
                    break;
                case "T":
                case "t":
                    TestChecksumCalculation();
                    break;
                case "0":
                    _isRunning = false;
                    break;
                default:
                    Console.WriteLine("Invalid option! Please try again.");
                    break;
            }
        }

        /// <summary>
        /// Lists all available serial ports on the system.
        /// </summary>
        private static void ListAvailablePorts()
        {
            Console.WriteLine("\n[TEST] Getting available ports...");
            string[] ports = DPS150Communication.GetAvailablePorts();

            if (ports.Length == 0)
            {
                Console.WriteLine("No serial ports found!");
            }
            else
            {
                Console.WriteLine($"Found {ports.Length} serial port(s):");
                for (int i = 0; i < ports.Length; i++)
                {
                    Console.WriteLine($"  [{i + 1}] {ports[i]}");
                }
            }
        }

        /// <summary>
        /// Connects to a selected serial port.
        /// </summary>
        private static void ConnectToDevice()
        {
            if (_communication?.IsConnected == true)
            {
                Console.WriteLine("\n[ERROR] Already connected! Disconnect first.");
                return;
            }

            string[] ports = DPS150Communication.GetAvailablePorts();
            if (ports.Length == 0)
            {
                Console.WriteLine("\n[ERROR] No serial ports available!");
                return;
            }

            Console.WriteLine("\nAvailable ports:");
            for (int i = 0; i < ports.Length; i++)
            {
                Console.WriteLine($"  [{i + 1}] {ports[i]}");
            }

            Console.Write("Select port number (or enter custom port name): ");
            string? input = Console.ReadLine();

            string portName;
            if (int.TryParse(input, out int portIndex) && portIndex > 0 && portIndex <= ports.Length)
            {
                portName = ports[portIndex - 1];
            }
            else if (!string.IsNullOrWhiteSpace(input))
            {
                portName = input;
            }
            else
            {
                Console.WriteLine("[ERROR] Invalid input!");
                return;
            }

            Console.WriteLine($"\n[TEST] Connecting to {portName}...");
            bool success = _communication!.Connect(portName);

            if (success)
            {
                Console.WriteLine($"[SUCCESS] Connected to {portName}");
                Console.WriteLine("Connection settings: 115200 baud, 8N1, No handshake");
            }
            else
            {
                Console.WriteLine($"[ERROR] Failed to connect to {portName}");
            }
        }

        /// <summary>
        /// Disconnects from the current device.
        /// </summary>
        private static void DisconnectFromDevice()
        {
            if (_communication?.IsConnected != true)
            {
                Console.WriteLine("\n[ERROR] Not connected!");
                return;
            }

            Console.WriteLine("\n[TEST] Disconnecting...");
            _communication.Disconnect();
            Console.WriteLine("[SUCCESS] Disconnected");
            _receivedMessageCount = 0;
        }

        /// <summary>
        /// Sends test data to the device.
        /// </summary>
        private static void SendTestData()
        {
            if (_communication?.IsConnected != true)
            {
                Console.WriteLine("\n[ERROR] Not connected!");
                return;
            }

            Console.Write("\nEnter data to send (hex bytes, e.g., '01 02 03' or text): ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("[ERROR] No data entered!");
                return;
            }

            byte[] data;
            if (input.Contains(' '))
            {
                // Parse as hex bytes
                try
                {
                    string[] hexValues = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    data = hexValues.Select(h => Convert.ToByte(h, 16)).ToArray();
                }
                catch
                {
                    Console.WriteLine("[ERROR] Invalid hex format! Use format like: 01 02 03");
                    return;
                }
            }
            else
            {
                // Convert text to bytes
                data = Encoding.ASCII.GetBytes(input);
            }

            Console.WriteLine($"\n[TEST] Sending {data.Length} bytes...");
            Console.WriteLine($"Data: {BitConverter.ToString(data).Replace("-", " ")}");

            bool success = _communication.SendData(data);

            if (success)
            {
                Console.WriteLine("[SUCCESS] Data sent successfully");
            }
            else
            {
                Console.WriteLine("[ERROR] Failed to send data");
            }
        }

        /// <summary>
        /// Reads response data from the device.
        /// </summary>
        private static void ReadResponse()
        {
            if (_communication?.IsConnected != true)
            {
                Console.WriteLine("\n[ERROR] Not connected!");
                return;
            }

            Console.Write("Enter timeout in milliseconds (default 1000): ");
            string? input = Console.ReadLine();
            int timeout = 1000;

            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int parsedTimeout))
            {
                timeout = parsedTimeout;
            }

            Console.WriteLine($"\n[TEST] Reading response with {timeout}ms timeout...");
            byte[]? response = _communication.ReadResponse(timeout);

            if (response != null && response.Length > 0)
            {
                Console.WriteLine($"[SUCCESS] Received {response.Length} bytes:");
                Console.WriteLine($"Hex: {BitConverter.ToString(response).Replace("-", " ")}");
                Console.WriteLine($"ASCII: {Encoding.ASCII.GetString(response.Where(b => b >= 32 && b < 127).ToArray())}");
            }
            else
            {
                Console.WriteLine("[INFO] No data received (timeout or no data available)");
            }
        }

        /// <summary>
        /// Sends a command and waits for a response.
        /// </summary>
        private static void SendCommandAndGetResponse()
        {
            if (_communication?.IsConnected != true)
            {
                Console.WriteLine("\n[ERROR] Not connected!");
                return;
            }

            Console.Write("\nEnter command to send (hex bytes, e.g., '01 02 03'): ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("[ERROR] No command entered!");
                return;
            }

            byte[] command;
            try
            {
                string[] hexValues = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                command = hexValues.Select(h => Convert.ToByte(h, 16)).ToArray();
            }
            catch
            {
                Console.WriteLine("[ERROR] Invalid hex format! Use format like: 01 02 03");
                return;
            }

            Console.Write("Enter timeout in milliseconds (default 1000): ");
            string? timeoutInput = Console.ReadLine();
            int timeout = 1000;

            if (!string.IsNullOrWhiteSpace(timeoutInput) && int.TryParse(timeoutInput, out int parsedTimeout))
            {
                timeout = parsedTimeout;
            }

            Console.WriteLine($"\n[TEST] Sending command and waiting for response...");
            Console.WriteLine($"Command: {BitConverter.ToString(command).Replace("-", " ")}");

            byte[]? response = _communication.SendCommandAndGetResponse(command, timeout);

            if (response != null && response.Length > 0)
            {
                Console.WriteLine($"[SUCCESS] Received {response.Length} bytes:");
                Console.WriteLine($"Hex: {BitConverter.ToString(response).Replace("-", " ")}");
                Console.WriteLine($"ASCII: {Encoding.ASCII.GetString(response.Where(b => b >= 32 && b < 127).ToArray())}");
            }
            else
            {
                Console.WriteLine("[INFO] No response received");
            }
        }

        /// <summary>
        /// Starts continuous reception of data from the device.
        /// </summary>
        private static void StartContinuousReceiving()
        {
            if (_communication?.IsConnected != true)
            {
                Console.WriteLine("\n[ERROR] Not connected!");
                return;
            }

            Console.WriteLine("\n[TEST] Starting continuous receiving...");
            bool success = _communication.StartReceiving();

            if (success)
            {
                Console.WriteLine("[SUCCESS] Continuous receiving started");
                Console.WriteLine("Status messages will be displayed as they arrive (every 100-200ms)");
                Console.WriteLine("Press any key to return to menu...");
                Console.ReadKey(true);
            }
            else
            {
                Console.WriteLine("[ERROR] Failed to start continuous receiving");
            }
        }

        /// <summary>
        /// Stops continuous reception of data.
        /// </summary>
        private static void StopContinuousReceiving()
        {
            if (_communication?.IsConnected != true)
            {
                Console.WriteLine("\n[ERROR] Not connected!");
                return;
            }

            Console.WriteLine("\n[TEST] Stopping continuous receiving...");
            _communication.StopReceiving();
            Console.WriteLine("[SUCCESS] Continuous receiving stopped");
        }

        /// <summary>
        /// Tests a complete communication cycle: connect, send, receive, disconnect.
        /// </summary>
        private static void TestCompleteCycle()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           COMPLETE COMMUNICATION CYCLE TEST              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            // Step 1: List ports
            Console.WriteLine("\n[STEP 1] Listing available ports...");
            string[] ports = DPS150Communication.GetAvailablePorts();
            if (ports.Length == 0)
            {
                Console.WriteLine("[ERROR] No ports available. Test aborted.");
                return;
            }
            Console.WriteLine($"Found {ports.Length} port(s): {string.Join(", ", ports)}");

            // Step 2: Connect
            Console.Write("\nEnter port name to connect to: ");
            string? portName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(portName))
            {
                Console.WriteLine("[ERROR] Invalid port name. Test aborted.");
                return;
            }

            Console.WriteLine($"\n[STEP 2] Connecting to {portName}...");
            if (!_communication!.Connect(portName))
            {
                Console.WriteLine("[ERROR] Connection failed. Test aborted.");
                return;
            }
            Console.WriteLine("[SUCCESS] Connected");

            // Step 3: Start receiving
            Console.WriteLine("\n[STEP 3] Starting continuous receiving...");
            _communication.StartReceiving();
            Console.WriteLine("[SUCCESS] Receiving started");

            // Step 4: Wait for some messages
            Console.WriteLine("\n[STEP 4] Waiting for 5 seconds to receive messages...");
            int initialCount = _receivedMessageCount;
            Thread.Sleep(5000);
            Console.WriteLine($"[INFO] Received {_receivedMessageCount - initialCount} messages during test");

            // Step 5: Send test command
            Console.WriteLine("\n[STEP 5] Sending test command (01 02 03)...");
            byte[] testCommand = new byte[] { 0x01, 0x02, 0x03 };
            if (_communication.SendData(testCommand))
            {
                Console.WriteLine("[SUCCESS] Test command sent");
            }

            // Step 6: Stop receiving
            Console.WriteLine("\n[STEP 6] Stopping continuous receiving...");
            _communication.StopReceiving();
            Console.WriteLine("[SUCCESS] Receiving stopped");

            // Step 7: Disconnect
            Console.WriteLine("\n[STEP 7] Disconnecting...");
            _communication.Disconnect();
            Console.WriteLine("[SUCCESS] Disconnected");

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              COMPLETE CYCLE TEST FINISHED                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        }

        /// <summary>
        /// Tests the checksum calculation functionality with known protocol examples.
        /// </summary>
        private static void TestChecksumCalculation()
        {
            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║            CHECKSUM CALCULATION TEST                     ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

            Console.WriteLine("\nThe DPS-150 protocol uses the checksum formula:");
            Console.WriteLine("CHK = sum(DATA[2..n]) & 0xFF");
            Console.WriteLine("(First two bytes are header and NOT included in checksum)\n");

            // Test Case 1: Voltage Set Command
            Console.WriteLine("[TEST 1] Voltage Set Command");
            Console.WriteLine("Packet: F1 B1 C1 04 00 00 B0 40 | CHK");
            Console.WriteLine("Expected: 0xB5");
            byte[] fullPacket1 = new byte[] { 0xF1, 0xB1, 0xC1, 0x04, 0x00, 0x00, 0xB0, 0x40 };
            byte checksum1 = DPS150Control.CalculateChecksum(fullPacket1);
            Console.WriteLine($"Calculated: 0x{checksum1:X2}");
            Console.WriteLine($"Result: {(checksum1 == 0xB5 ? "✓ PASS" : "✗ FAIL")}\n");

            // Test Case 2: Current Limit Command
            Console.WriteLine("[TEST 2] Current Limit Command");
            Console.WriteLine("Packet: F1 B1 C2 04 FD FF FF 3E | CHK");
            Console.WriteLine("Expected: 0xFF");
            byte[] fullPacket2 = new byte[] { 0xF1, 0xB1, 0xC2, 0x04, 0xFD, 0xFF, 0xFF, 0x3E };
            byte checksum2 = DPS150Control.CalculateChecksum(fullPacket2);
            Console.WriteLine($"Calculated: 0x{checksum2:X2}");
            Console.WriteLine($"Result: {(checksum2 == 0xFF ? "✓ PASS" : "✗ FAIL")}\n");

            // Test Case 3: Output Relay ON
            Console.WriteLine("[TEST 3] Output Relay ON (RUN)");
            Console.WriteLine("Packet: F1 B1 DB 01 01 | CHK");
            Console.WriteLine("Expected: 0xDD");
            byte[] fullPacket3 = new byte[] { 0xF1, 0xB1, 0xDB, 0x01, 0x01 };
            byte checksum3 = DPS150Control.CalculateChecksum(fullPacket3);
            Console.WriteLine($"Calculated: 0x{checksum3:X2}");
            Console.WriteLine($"Result: {(checksum3 == 0xDD ? "✓ PASS" : "✗ FAIL")}\n");

            // Test Case 4: Output Relay OFF
            Console.WriteLine("[TEST 4] Output Relay OFF (STOP)");
            Console.WriteLine("Packet: F1 B1 DB 01 00 | CHK");
            Console.WriteLine("Expected: 0xDC");
            byte[] fullPacket4 = new byte[] { 0xF1, 0xB1, 0xDB, 0x01, 0x00 };
            byte checksum4 = DPS150Control.CalculateChecksum(fullPacket4);
            Console.WriteLine($"Calculated: 0x{checksum4:X2}");
            Console.WriteLine($"Result: {(checksum4 == 0xDC ? "✓ PASS" : "✗ FAIL")}\n");

            // Test Case 5: Brightness Setting
            Console.WriteLine("[TEST 5] Brightness Setting (12)");
            Console.WriteLine("Packet: F1 B1 D6 01 0C | CHK");
            Console.WriteLine("Expected: 0xE3");
            byte[] fullPacket5 = new byte[] { 0xF1, 0xB1, 0xD6, 0x01, 0x0C };
            byte checksum5 = DPS150Control.CalculateChecksum(fullPacket5);
            Console.WriteLine($"Calculated: 0x{checksum5:X2}");
            Console.WriteLine($"Result: {(checksum5 == 0xE3 ? "✓ PASS" : "✗ FAIL")}\n");

            // Test Case 6: Complete Packet Verification
            Console.WriteLine("[TEST 6] Complete Packet Verification");
            byte[] completePacket = new byte[] { 0xF1, 0xB1, 0xDB, 0x01, 0x01, 0xDD };
            Console.WriteLine($"Packet: {BitConverter.ToString(completePacket).Replace("-", " ")}");
            bool isValid = DPS150Control.VerifyChecksum(completePacket);
            Console.WriteLine($"Verification: {(isValid ? "✓ VALID" : "✗ INVALID")}\n");

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║            CHECKSUM TEST COMPLETED                       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        }

        /// <summary>
        /// Event handler for received data from the device.
        /// </summary>
        private static void OnDataReceived(object? sender, DataReceivedEventArgs e)
        {
            _receivedMessageCount++;

            if (e.Data != null && e.Data.Length > 0)
            {
                Console.WriteLine($"\n[RECEIVED #{_receivedMessageCount}] {DateTime.Now:HH:mm:ss.fff}");
                Console.WriteLine($"  Length: {e.Data.Length} bytes");
                Console.WriteLine($"  Hex: {BitConverter.ToString(e.Data).Replace("-", " ")}");

                // Try to show ASCII representation
                string ascii = Encoding.ASCII.GetString(e.Data.Where(b => b >= 32 && b < 127).ToArray());
                if (!string.IsNullOrWhiteSpace(ascii))
                {
                    Console.WriteLine($"  ASCII: {ascii}");
                }

                Console.Write("\nSelect option: "); // Reprint prompt
            }
        }
    }
}
