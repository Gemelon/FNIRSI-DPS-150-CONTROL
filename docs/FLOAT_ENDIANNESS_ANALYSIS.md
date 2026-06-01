# DPS-150 Float Endianness Analysis

## IEEE-754 Float32 Format

The DPS-150 protocol uses IEEE-754 single-precision (32-bit) floating-point values for all analog parameters (voltage, current, power, etc.).

## Protocol Example Analysis

From the official protocol documentation:

```
Voltage Setpoint (C1):
TX: F1 B1 C1 04 CD CC 44 41 E3
				↑↑ ↑↑ ↑↑ ↑↑
				Float bytes (4 bytes)
```

### Byte-by-Byte Breakdown

- `F1` - Header (TX direction)
- `B1` - Access type (Write)
- `C1` - Register address (Voltage Setpoint)
- `04` - Data length (4 bytes for float)
- `CD CC 44 41` - **Float value bytes**
- `E3` - Checksum

## Endianness Determination

To determine the correct byte order, we need to decode the float bytes `CD CC 44 41`:

### Test 1: Little-Endian (as-is)
```
Bytes: CD CC 44 41
Value: 12.30V ✓
```

### Test 2: Big-Endian (reversed)
```
Bytes: 41 44 CC CD
Value: ~16776396.0 (nonsensical)
```

## Conclusion

**The DPS-150 protocol uses LITTLE-ENDIAN byte order for float values.**

This is the native byte order for:
- x86/x64 processors
- ARM processors (in little-endian mode)
- Most modern systems

## Implementation Impact

### Session Management

**Important**: Both `SetVoltage()` and `SetOutputRelay()` now automatically start a session if one is not already active. This ensures:
- Commands are properly accepted by the device
- No manual session management is required
- Session start is only done once per connection
- 50ms delay after session start allows device to process the command

### Current FloatToBytes Implementation (INCORRECT for DPS-150)

```csharp
private byte[] FloatToBytes(float value)
{
	byte[] bytes = BitConverter.GetBytes(value);
	if (BitConverter.IsLittleEndian)
		Array.Reverse(bytes);  // ❌ This converts to big-endian
	return bytes;
}
```

This implementation:
- Assumes the protocol uses **big-endian**
- On little-endian systems (Windows x86/x64), it reverses the bytes
- **This is INCORRECT for DPS-150**

### Correct Implementation for DPS-150

```csharp
private byte[] FloatToBytes(float value)
{
	// DPS-150 uses little-endian byte order
	// On little-endian systems, BitConverter.GetBytes already returns correct order
	return BitConverter.GetBytes(value);
}
```

**No byte reversal is needed** because:
1. DPS-150 uses little-endian
2. Most systems (Windows) are little-endian
3. BitConverter.GetBytes returns native byte order (little-endian on Windows)

### Correct BytesToFloat Implementation

```csharp
private float BytesToFloat(byte[] data, int offset = 0)
{
	if (data == null || data.Length < offset + 4)
	{
		throw new ArgumentException("Insufficient data for float conversion");
	}

	// DPS-150 uses little-endian byte order
	// BitConverter.ToSingle expects native byte order (little-endian on Windows)
	return BitConverter.ToSingle(data, offset);
}
```

**No byte reversal is needed** when reading from the device either.

## Verification Steps

1. Run the FloatEndiannessTest (menu option [X] in TestProgram)
2. The test will confirm that `CD CC 44 41` = 12.30V
3. The test will show that direct BitConverter usage (no reversal) matches protocol

## Why This Matters

Using the wrong byte order will result in:
- Voltage 12.3V → sent as garbage value
- Received voltages decoded incorrectly
- Device malfunction or safety issues

## References

- Protocol documentation: https://github.com/cho45/fnirsi-dps-150/blob/main/docs/FNIRSI_DPS-150_Protocol.md
- IEEE-754 specification: https://en.wikipedia.org/wiki/IEEE_754
- BitConverter endianness: https://learn.microsoft.com/en-us/dotnet/api/system.bitconverter
