# DPS-150 Protocol Checksum Calculation

## Overview

The FNIRSI-DPS-150 uses a simple additive checksum algorithm to verify packet integrity. This document describes the checksum calculation method and provides implementation details.

## Checksum Formula

```
CHK = sum(DATA[2..n]) & 0xFF
```

Where:
- **DATA[0]**: Header byte (0xF0 or 0xF1) - **NOT included in checksum**
- **DATA[1]**: Access type (0xA1, 0xB0, 0xB1, 0xC1) - **NOT included in checksum**
- **DATA[2]**: REG (register address) - **Included in checksum**
- **DATA[3]**: LEN (payload length) - **Included in checksum**
- **DATA[4..n]**: Payload bytes - **Included in checksum**
- **CHK**: Calculated checksum (1 byte)

The `& 0xFF` operation ensures the result is a single byte (0-255).

**Important:** The first two bytes (DATA[0] and DATA[1]) are header bytes and are **NOT** included in the checksum calculation. The checksum starts from DATA[2] onwards.

**Minimum data length:** 4 bytes (Header + Access + REG + LEN minimum)

## Packet Structure

A complete DPS-150 protocol packet has the following structure:

```
[HEADER] [ACCESS] [REG] [LEN] [PAYLOAD...] [CHK]
 DATA[0]  DATA[1]  DATA[2] DATA[3] DATA[4..n]
   ↓        ↓         ↓       ↓        ↓
NOT CHK  NOT CHK    IN CHK  IN CHK   IN CHK
```

- **HEADER (DATA[0])**: 0xF0 (device to PC) or 0xF1 (PC to device) - NOT in checksum
- **ACCESS (DATA[1])**: Access type (0xA1=Read, 0xB0=Baud, 0xB1=Write, 0xC1=Control) - NOT in checksum
- **REG (DATA[2])**: Register address - Included in checksum
- **LEN (DATA[3])**: Number of payload bytes - Included in checksum
- **PAYLOAD (DATA[4..n])**: Payload data - Included in checksum
- **CHK**: Checksum byte = sum(DATA[2..n]) & 0xFF

## Implementation

### C# Implementation (DPS150Control.cs)

The `DPS150Control` class provides three public static methods for checksum operations:

#### 1. Calculate Checksum (From Complete Packet Array)

```csharp
public static byte CalculateChecksum(byte[] data)
```

Calculates the checksum from a complete packet array including header bytes.

**Parameters:**
- `data`: Complete packet array [HEADER] [ACCESS] [REG] [LEN] [PAYLOAD...]

**Returns:** Calculated checksum byte

**Important:** The method automatically skips the first two bytes (header) and calculates the checksum starting from index 2.

**Example:**
```csharp
byte[] packet = new byte[] { 0xF1, 0xB1, 0xC1, 0x04, 0x00, 0x00, 0xB0, 0x40 };
byte checksum = DPS150Control.CalculateChecksum(packet);
// Calculates: (C1 + 04 + 00 + 00 + B0 + 40) & FF = B5
// Result: 0xB5
```

#### 2. Calculate Checksum (From Components)

```csharp
public static byte CalculateChecksum(byte reg, byte len, byte[] payloadData)
```

Calculates the checksum from individual components (convenience method).

**Parameters:**
- `reg`: Register address
- `len`: Payload data length
- `payloadData`: Payload bytes (not including REG and LEN)

**Returns:** Calculated checksum byte

**Example:**
```csharp
byte reg = 0xC1;
byte len = 0x04;
byte[] payload = new byte[] { 0x00, 0x00, 0xB0, 0x40 };
byte checksum = DPS150Control.CalculateChecksum(reg, len, payload);
// Result: 0xB5
```

#### 3. Verify Checksum

```csharp
public static bool VerifyChecksum(byte[] packet)
```

Verifies if a complete packet has a valid checksum.

**Parameters:**
- `packet`: Complete packet including header and checksum

**Returns:** `true` if checksum is valid; `false` otherwise

**Example:**
```csharp
byte[] packet = new byte[] { 0xF1, 0xB1, 0xDB, 0x01, 0x01, 0xDD };
bool isValid = DPS150Control.VerifyChecksum(packet);
// Verifies: (DB + 01 + 01) & FF = DD
// Result: true
```

## Protocol Examples

### Example 1: Set Voltage to 5.5V

**Command Packet:**
```
F1 B1 C1 04 00 00 B0 40 B5
```

**Breakdown:**
- DATA[0] = `F1` (Header - PC to device) - **NOT in checksum**
- DATA[1] = `B1` (Write access) - **NOT in checksum**
- DATA[2] = `C1` (Voltage Setpoint register) - **In checksum**
- DATA[3] = `04` (4 bytes of payload) - **In checksum**
- DATA[4..7] = `00 00 B0 40` (5.5 as IEEE 754 float, little-endian) - **In checksum**
- CHK = `B5`

**Calculation:**
```
Sum = C1 + 04 + 00 + 00 + B0 + 40 = 1B5
CHK = 1B5 & FF = B5 ✓
```

### Example 2: Enable Output (RUN)

**Command Packet:**
```
F1 B1 DB 01 01 DD
```

**Breakdown:**
- DATA[0] = `F1` (Header) - **NOT in checksum**
- DATA[1] = `B1` (Write access) - **NOT in checksum**
- DATA[2] = `DB` (Output Relay State register) - **In checksum**
- DATA[3] = `01` (1 byte payload) - **In checksum**
- DATA[4] = `01` (ON) - **In checksum**
- CHK = `DD`

**Calculation:**
```
Sum = DB + 01 + 01 = DD
CHK = DD & FF = DD ✓
```

### Example 3: Disable Output (STOP)

**Command Packet:**
```
F1 B1 DB 01 00 DC
```

**Calculation:**
```
Sum = DB + 01 + 00 = DC
CHK = DC & FF = DC ✓
```

### Example 4: Set Brightness to 12

**Command Packet:**
```
F1 B1 D6 01 0C E3
```

**Calculation:**
```
Sum = D6 + 01 + 0C = E3
CHK = E3 & FF = E3 ✓
```

### Example 5: Set Volume to 9

**Command Packet:**
```
F1 B1 D7 01 09 E1
```

**Calculation:**
```
Sum = D7 + 01 + 09 = E1
CHK = E1 & FF = E1 ✓
```

### Example 6: Set OTP to 64°C

**Command Packet:**
```
F1 B1 D4 04 00 00 80 42 9A
```

**Calculation:**
```
Sum = D4 + 04 + 00 + 00 + 80 + 42 = 19A
CHK = 19A & FF = 9A ✓
```

## Testing the Implementation

The test console application includes a comprehensive checksum test function. To run it:

1. Start the application: `dotnet run`
2. Select option `T` (Test Checksum Calculation)

The test includes:
- 5 pre-defined test cases with known checksums
- Tests both calculation methods (component-based and array-based)
- Complete packet verification test
- Interactive manual calculation mode

### Sample Output

```
[TEST 1] Voltage Set Command
Packet: F1 B1 C1 04 00 00 B0 40 | CHK
Expected: 0xB5
Calculated: 0xB5
Result: ✓ PASS
Using full packet array: 0xB5
Array method result: ✓ PASS

[TEST 2] Current Limit Command
Packet: F1 B1 C2 04 FD FF FF 3E | CHK
Expected: 0xFF
Calculated: 0xFF
Result: ✓ PASS
```

## Important Notes

1. **Header Exclusion**: The first two bytes (header and access type) are **NEVER** included in the checksum calculation. This is critical for correct implementation.

2. **Byte Order**: The DPS-150 uses little-endian byte order for multi-byte values (like floats).

3. **Overflow Handling**: The `& 0xFF` operation handles overflow automatically. For example:
   - Sum = 0x1B5 (437 decimal)
   - Result = 0x1B5 & 0xFF = 0xB5 (181 decimal)

4. **Minimum Packet Size**: A valid packet must have at least 5 bytes: Header (1) + Access (1) + REG (1) + LEN (1) + CHK (1)

5. **Verification**: Always verify received packets using `VerifyChecksum()` before processing.

6. **Error Detection**: This simple checksum can detect:
   - Single bit errors
   - Most multi-bit errors
   - Byte order errors

   But it cannot detect:
   - Errors that cancel out (e.g., +1 in one byte, -1 in another)
   - Some systematic errors

## Common Mistakes to Avoid

❌ **WRONG:** Including header bytes in checksum
```csharp
// INCORRECT - includes all bytes
sum = F1 + B1 + C1 + 04 + 00 + 00 + B0 + 40 = 266 & FF = 66 (WRONG!)
```

✅ **CORRECT:** Starting checksum from byte index 2
```csharp
// CORRECT - skips first two bytes
sum = C1 + 04 + 00 + 00 + B0 + 40 = 1B5 & FF = B5 (CORRECT!)
```

## Reference

For complete protocol documentation, see:
- [FNIRSI_DPS-150_Protocol.md](https://github.com/cho45/fnirsi-dps-150/blob/main/docs/FNIRSI_DPS-150_Protocol.md)
- Original work by [cho45](https://github.com/cho45)

## See Also

- `DPS150Control.cs` - Implementation of checksum methods
- `DPS150Device.cs` - Register addresses and enums
- `Program.cs` - Test application with checksum testing
