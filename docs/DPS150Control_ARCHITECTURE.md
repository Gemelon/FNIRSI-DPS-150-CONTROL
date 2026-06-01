# DPS150Control Class Architecture

## Overview

`DPS150Control` is the high-level control class for the FNIRSI-DPS-150 laboratory power supply. It provides a comprehensive API for device control, configuration, and monitoring.

## Class Structure

### Private Fields

| Field | Type | Purpose |
|-------|------|---------|
| `_communication` | `DPS150Communication` | Low-level serial communication handler. Manages physical serial port connection and data transmission. |
| `_registers` | `DPS150Registers` | Register map and device configuration storage. Contains protocol register definitions and device state information. |
| `_sessionStarted` | `bool` | Session state tracker. The DPS-150 requires an active session before most register operations. |

### Public Properties

| Property | Type | Description |
|----------|------|-------------|
| `AvailablePorts` | `string[]` | Gets an array of available serial port names on the system. Returns empty array if no ports are available. |
| `IsConnected` | `bool` | Indicates whether the serial port connection is currently open. |
| `IsSessionStarted` | `bool` | Indicates whether a communication session is currently active. |

## Session Lifecycle

The DPS-150 requires a specific session management flow:

```
1. Connect to device        → IsConnected = true
2. Start session           → IsSessionStarted = true
3. Perform operations      → Device control methods
4. Stop session            → IsSessionStarted = false
5. Disconnect              → IsConnected = false
```

### Auto-Session Management

Many control methods automatically start a session if one is not already active:
- `SetVoltage()`, `SetCurrent()`
- `SetOutputRelay()`
- `SetPreset()`, `GetPreset()`
- All protection setters/getters (OVP, OCP, OPP, OTP, LVP)
- All UI settings (Brightness, Volume)
- All telemetry getters

## API Categories

### 1. Connection Management
- `ConnectToDevice(string portInput)` - Connect using port name or number
- `DisconnectFromDevice()` - Disconnect and cleanup
- `StartSession()` - Start communication session
- `StopSession()` - Stop communication session
- `FlushBuffers()` - Clear serial buffers

### 2. Output Control
- `SetVoltage(float voltage)` - Set output voltage setpoint
- `GetVoltage()` - Read voltage setpoint
- `SetCurrent(float current)` - Set current limit
- `GetCurrent()` - Read current limit
- `SetOutputRelay(OutputRelayState state)` - Turn output ON/OFF

### 3. Preset Management
- `SetPreset(int presetNumber, float voltage, float current)` - Configure preset M1-M6
- `GetPreset(int presetNumber, out float voltage, out float current)` - Read preset values

### 4. Protection Settings
- **OVP** (Over-Voltage Protection):
  - `SetOVP(float ovp)` / `GetOVP()` - Range: 0-160V
- **OCP** (Over-Current Protection):
  - `SetOCP(float ocp)` / `GetOCP()` - Range: 0-20A
- **OPP** (Over-Power Protection):
  - `SetOPP(float opp)` / `GetOPP()` - Range: 0-3000W
- **OTP** (Over-Temperature Protection):
  - `SetOTP(float otp)` / `GetOTP()` - Range: 0-100°C
- **LVP** (Low-Voltage Protection):
  - `SetLVP(float lvp)` / `GetLVP()` - Range: 0-30V

### 5. UI Settings
- `SetBrightness(int brightness)` / `GetBrightness()` - Display brightness (0-100%)
- `SetVolume(int volume)` / `GetVolume()` - Device volume (0-100%)

### 6. Telemetry & Status
- **Device Capabilities:**
  - `GetInputVoltage()` - Input/supply voltage
  - `GetMaximumVoltage()` - Maximum voltage capability
  - `GetMaximumCurrent()` - Maximum current capability
  - `GetInternalTemperature()` - Internal device temperature

- **Measurement Data:**
  - `GetMeasurement()` - Actual output [voltage, current]

- **Energy & Capacity:**
  - `GetMeasuredCapacity()` - Accumulated capacity (Ah)
  - `GetMeasuredEnergy()` - Accumulated energy (Wh)

- **Status:**
  - `GetRunningMode()` - Output relay state (RUN/STOP)
  - `GetCCCV()` - Regulation mode (CC/CV)

### 7. Low-Level Operations
- `SendData(byte[] data)` - Send raw data packet
- `ReadResponse(int timeoutMs)` - Read response packet
- `SendCommandAndGetResponse(byte[] data, int timeoutMs)` - Combined send+receive

### 8. Packet & Checksum Utilities
- `CreatePacket(...)` - Build protocol-compliant packets
- `CalculateChecksum(byte[] data)` - Calculate packet checksum
- `VerifyChecksum(byte[] packet)` - Validate packet checksum
- `FloatToBytes(float value)` - Convert float to IEEE-754 bytes
- `BytesToFloat(byte[] data, int offset)` - Parse float from bytes

## Protocol Details

### Data Format
- **Float values**: IEEE-754 float32, little-endian (4 bytes)
- **Integer values**: 32-bit signed integer, little-endian (4 bytes)
- **Checksum**: 8-bit sum of bytes [2..n-1], masked with 0xFF

### Packet Structure
```
[Header][AccessType][Register][Length][...Payload...][Checksum]
  0xF1     0xB1       0xC1      0x04    4 bytes       0xXX
```

### Register Access Types
- `0xA1` - Read
- `0xB0` - Write (alternative)
- `0xB1` - Write
- `0xC1` - Session control

### Communication Direction
- `0xF1` - TX (Host → Device)
- `0xF0` - RX (Device → Host)

## Error Handling

All methods implement robust error handling:
- Return `false` or `-1.0f` / `null` on failure
- Verify connection state before operations
- Auto-start session when needed
- Validate packet structure and checksums
- Catch and suppress exceptions (fail-safe)

## Thread Safety

The underlying `DPS150Communication` class uses locking for thread-safe serial port access. All `DPS150Control` methods are safe to call from multiple threads.

## Usage Example

```csharp
var control = new DPS150Control();

// Connect to device
if (control.ConnectToDevice("COM3"))
{
	Console.WriteLine("Connected!");

	// Session is auto-started by control methods
	control.SetVoltage(12.0f);
	control.SetCurrent(2.0f);
	control.SetOutputRelay(OutputRelayState.ON);

	// Read actual values
	float[] measurement = control.GetMeasurement();
	Console.WriteLine($"Output: {measurement[0]:F2}V / {measurement[1]:F2}A");

	// Monitor status
	bool? isCV = control.GetCCCV();
	Console.WriteLine($"Mode: {(isCV == true ? "CV" : "CC")}");

	// Cleanup
	control.DisconnectFromDevice();
}
```

## Related Classes

- **DPS150Communication**: Low-level serial communication handler
- **DPS150Device**: Protocol enums and register definitions
- **DPS150Registers**: Register map (currently unused placeholder)

## Documentation References

- Protocol specification: [FNIRSI-DPS-150 Protocol](https://github.com/cho45/fnirsi-dps-150/blob/main/docs/FNIRSI_DPS-150_Protocol.md)
- Checksum documentation: `CHECKSUM_DOCUMENTATION.md`
- Float endianness: `FLOAT_ENDIANNESS_ANALYSIS.md`
- Session management: `SESSION_MANAGEMENT_GUIDE.md`
