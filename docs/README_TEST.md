# FNIRSI-DPS-150 Communication Test Application

## Overview
This console application provides comprehensive testing capabilities for the `DPS150Communication` class, which handles serial communication with the FNIRSI-DPS-150 laboratory power supply.

## Features

### Interactive Menu System
The application provides a user-friendly menu with the following test options:

1. **List Available Ports** - Displays all available serial (COM) ports on the system
2. **Connect to Device** - Establishes connection to the selected serial port (115200 baud, 8N1)
3. **Disconnect from Device** - Closes the serial connection
4. **Send Test Data** - Sends custom data (hex bytes or ASCII text) to the device
5. **Read Response** - Reads incoming data with configurable timeout
6. **Send Command and Get Response** - Combined send/receive operation
7. **Start Continuous Receiving** - Begins continuous monitoring for device status messages (sent every 100-200ms)
8. **Stop Continuous Receiving** - Stops the continuous receive loop
9. **Test Complete Communication Cycle** - Automated test of the full communication workflow
0. **Exit Application** - Clean shutdown with resource cleanup

## Usage

### Starting the Application
```bash
dotnet run
```

### Testing Basic Connection
1. Select option `1` to list available ports
2. Select option `2` to connect to a port
3. Select option `7` to start receiving status messages
4. Select option `8` to stop receiving
5. Select option `3` to disconnect

### Sending Data
Data can be sent in two formats:
- **Hex bytes**: Enter space-separated hex values (e.g., `01 02 03 FF`)
- **ASCII text**: Enter plain text (automatically converted to bytes)

### Reading Responses
- Responses are displayed in both hex and ASCII format
- ASCII display filters out non-printable characters (< 32 or >= 127)
- Configurable timeout (default: 1000ms)

### Continuous Receiving
When continuous receiving is active:
- The application displays each received message with timestamp
- Shows message count, length, hex data, and ASCII representation
- Messages are typically received every 100-200ms from the device

## Connection Settings
The application uses the following serial port configuration:
- **Baud Rate**: 115200
- **Data Bits**: 8
- **Parity**: None
- **Stop Bits**: 1
- **Handshake**: None
- **Timeout**: 1000ms (read/write)

## Example Test Sequence

```
1. List Available Ports
   → Displays: COM1, COM3, COM5

2. Connect to Device
   → Select: COM3
   → Result: Connected successfully

3. Start Continuous Receiving
   → Device sends status messages every 100-200ms
   → Messages displayed in real-time

4. Send Test Data
   → Enter: 01 02 03
   → Data sent to device

5. Stop Continuous Receiving
   → Continuous monitoring stopped

6. Disconnect from Device
   → Connection closed cleanly
```

## Complete Cycle Test (Option 9)
This automated test performs:
1. Lists all available ports
2. Connects to specified port
3. Starts continuous receiving
4. Waits 5 seconds to collect messages
5. Sends a test command (01 02 03)
6. Stops receiving
7. Disconnects cleanly

This is useful for quick validation of all communication features.

## Event Handling
The application demonstrates proper event handling:
- Subscribes to `DataReceived` event
- Displays real-time status messages
- Shows timestamp, byte count, and data content
- Maintains message counter

## Error Handling
The application includes comprehensive error checking:
- Validates connection state before operations
- Checks for available ports before connection attempts
- Validates user input (hex format, numeric values)
- Provides clear error messages
- Ensures clean resource cleanup on exit

## Requirements
- .NET 10.0
- System.IO.Ports NuGet package (automatically included)
- FNIRSI-DPS-150 device (or compatible serial device for testing)
- Available COM port

## Notes
- The application automatically cleans up resources on exit
- Connection state is displayed in the menu header
- Received message count is tracked and displayed
- All timestamps use high-precision format (HH:mm:ss.fff)

## Development
This test application serves as:
- Validation tool for the DPS150Communication class
- Example implementation for serial communication
- Interactive debugging interface
- Reference for proper resource management and event handling
