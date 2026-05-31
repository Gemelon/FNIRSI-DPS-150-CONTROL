# FNIRSI-DPS-150-CONTROL

A .NET 10 implementation for serial communication and control of the FNIRSI-DPS-150 laboratory power supply.

## Overview

This project provides a comprehensive C# library for interfacing with the FNIRSI-DPS-150 power supply via serial communication. It includes classes for communication management, device control, and a test console application.

## Features

- **Serial Communication Management** (`DPS150Communication`)
  - Connect/Disconnect to serial ports
  - Send commands and receive responses
  - Continuous reception of device status messages (100-200ms intervals)
  - Event-driven data reception
  - Thread-safe operations

- **Device Control** (`DPS150Device`, `DPS150Control`)
  - Device status monitoring
  - Protection mode handling (OVP, OCP, OPP, OTP, LVP, REP)
  - Parameter configuration
  - Memory preset management

- **Test Console Application** (`Program.cs`)
  - Interactive menu for testing all features
  - Port selection and connection management
  - Data transmission testing
  - Real-time status monitoring
  - Complete communication cycle testing

## Requirements

- .NET 10.0 SDK
- System.IO.Ports NuGet package (automatically included)
- FNIRSI-DPS-150 laboratory power supply
- Available serial (COM) port

## Installation

1. Clone this repository
2. Build the solution:
   ```bash
   dotnet build
   ```

## Usage

### Running the Test Application

```bash
dotnet run
```

The interactive console will guide you through:
- Listing available ports
- Connecting to the device
- Sending/receiving data
- Monitoring device status

### Using the Library in Your Project

```csharp
using FNIRSI_DPS_150_CONTROL;

// Create communication instance
var comm = new DPS150Communication();

// Subscribe to data received event
comm.DataReceived += (sender, e) => {
	if (e.Data != null) {
		Console.WriteLine($"Received: {BitConverter.ToString(e.Data)}");
	}
};

// Connect to device
if (comm.Connect("COM3")) {
	// Start receiving status messages
	comm.StartReceiving();

	// Send command
	byte[] command = new byte[] { 0x01, 0x02, 0x03 };
	comm.SendData(command);

	// Or send and wait for response
	byte[]? response = comm.SendCommandAndGetResponse(command);

	// Clean up
	comm.StopReceiving();
	comm.Disconnect();
}
```

## Connection Settings

- **Baud Rate**: 115200
- **Data Bits**: 8
- **Parity**: None
- **Stop Bits**: 1
- **Handshake**: None

## Protocol Documentation

This implementation is based on the FNIRSI-DPS-150 protocol documentation by **cho45**.

**Protocol Reference**: [FNIRSI_DPS-150_Protocol.md](https://github.com/cho45/fnirsi-dps-150/blob/main/docs/FNIRSI_DPS-150_Protocol.md)

For detailed protocol information, please refer to the original documentation.

## Project Structure

```
FNIRSI-DPS-150-CONTROL/
├── DPS150Communication.cs    # Serial communication class
├── DPS150Device.cs           # Device definitions and enums
├── DPS150Control.cs          # High-level control class
├── Program.cs                # Test console application
├── LICENSE                   # MIT License
├── ACKNOWLEDGMENTS.md        # Credits and references
├── README_TEST.md            # Test application documentation
└── README.md                 # This file
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

**Special thanks to [cho45](https://github.com/cho45)** for reverse-engineering and documenting the FNIRSI-DPS-150 communication protocol. This project would not have been possible without that excellent work.

See [ACKNOWLEDGMENTS.md](ACKNOWLEDGMENTS.md) for detailed credits.

## Related Projects

- [cho45/fnirsi-dps-150](https://github.com/cho45/fnirsi-dps-150) - Original protocol documentation and TypeScript implementation

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## Disclaimer

This is an unofficial implementation and is not affiliated with or endorsed by FNIRSI. Use at your own risk.

## Support

For protocol-related questions, please refer to the [original protocol documentation](https://github.com/cho45/fnirsi-dps-150/blob/main/docs/FNIRSI_DPS-150_Protocol.md).

For implementation-specific issues, please open an issue in this repository.
