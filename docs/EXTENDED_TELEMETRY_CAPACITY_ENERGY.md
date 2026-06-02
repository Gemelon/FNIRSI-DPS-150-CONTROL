# Extended Telemetry Frames - Capacity & Energy

## Übersicht

Die `ParseTelemetryFrames` Methode wurde erweitert, um **zwei zusätzliche Telemetrie-Frames** zu verarbeiten:

- **Frame 6: MeasuredCapacity (0xD9)** - Gemessene Kapazität in Ah (Amperestunden)
- **Frame 7: MeasuredEnergy (0xDA)** - Gemessene Energie in Wh (Wattstunden)

## Telemetrie-Frame-Struktur

### Standard-Telemetrie (5 Frames - 53 Bytes)

Die Standard-Telemetrie wird vom DPS-150 periodisch (alle 500ms) gesendet:

| Frame | Register | Beschreibung | Größe | Datentyp |
|-------|----------|--------------|-------|----------|
| 1 | `0xC3` | Measurement (V, A, W) | 17 Bytes | 3× float32 |
| 2 | `0xC0` | InputVoltage | 9 Bytes | float32 |
| 3 | `0xE2` | MaximumVoltage | 9 Bytes | float32 |
| 4 | `0xE3` | MaximumCurrent | 9 Bytes | float32 |
| 5 | `0xC4` | InternalTemperature | 9 Bytes | float32 |

**Gesamt:** 53 Bytes

---

### Erweiterte Telemetrie (7 Frames - 71 Bytes)

Zusätzlich zu den Standard-Frames können **zwei weitere Frames** vorkommen:

| Frame | Register | Beschreibung | Größe | Datentyp |
|-------|----------|--------------|-------|----------|
| 6 | `0xD9` | MeasuredCapacity (Ah) | 9 Bytes | float32 |
| 7 | `0xDA` | MeasuredEnergy (Wh) | 9 Bytes | float32 |

**Gesamt:** 71 Bytes (wenn alle 7 Frames vorhanden sind)

---

## Frame-Format

Alle Telemetrie-Frames haben die gleiche Struktur:

```
┌────────┬────────┬──────────┬────────┬──────────────┬──────────┐
│ Header │ Header │ Register │ Length │     Data     │ Checksum │
│  0xF0  │  0xA1  │   REG    │  LEN   │  (LEN bytes) │   CHK    │
└────────┴────────┴──────────┴────────┴──────────────┴──────────┘
```

### Beispiel: MeasuredCapacity Frame

```
F0 A1 D9 04 9B D6 34 00 81
│  │  │  │  └─────────┘ │
│  │  │  │   4-byte     │
│  │  │  │   float32    │
│  │  │  └─ Length = 4  │
│  │  └─ Register = 0xD9 (MeasuredCapacity)
│  └─ Access Type = 0xA1 (Read)
└─ Direction = 0xF0 (RX)
```

- **Offset 0-1:** `F0 A1` (Header)
- **Offset 2:** `D9` (Register: MeasuredCapacity)
- **Offset 3:** `04` (Length: 4 bytes)
- **Offset 4-7:** `9B D6 34 00` (float32: Capacity in Ah)
- **Offset 8:** `81` (Checksum)

### Beispiel: MeasuredEnergy Frame

```
F0 A1 DA 04 CF AE 28 35 B8
│  │  │  │  └─────────┘ │
│  │  │  │   4-byte     │
│  │  │  │   float32    │
│  │  │  └─ Length = 4  │
│  │  └─ Register = 0xDA (MeasuredEnergy)
│  └─ Access Type = 0xA1 (Read)
└─ Direction = 0xF0 (RX)
```

- **Offset 0-1:** `F0 A1` (Header)
- **Offset 2:** `DA` (Register: MeasuredEnergy)
- **Offset 3:** `04` (Length: 4 bytes)
- **Offset 4-7:** `CF AE 28 35` (float32: Energy in Wh)
- **Offset 8:** `B8` (Checksum)

---

## ParseTelemetryFrames Implementation

Die `ParseTelemetryFrames` Methode wurde erweitert um folgende Cases im Switch-Statement:

```csharp
case 0xD9: // MeasuredCapacity (in Ah)
	if (dataLength == 4)
	{
		_measuredCapacity = BytesToFloat(frame, 4);
		parsedAtLeastOne = true;
	}
	break;

case 0xDA: // MeasuredEnergy (in Wh)
	if (dataLength == 4)
	{
		_measuredEnergy = BytesToFloat(frame, 4);
		parsedAtLeastOne = true;
	}
	break;
```

---

## Public Properties

Die gemessenen Werte sind über folgende Properties verfügbar:

```csharp
/// <summary>
/// Gets the measured capacity in ampere-hours (Ah).
/// </summary>
/// <remarks>
/// This value represents the total charge that has flowed through the output
/// since the last reset. Updated automatically when telemetry frames are parsed.
/// </remarks>
public float MeasuredCapacity => _measuredCapacity;

/// <summary>
/// Gets the measured energy in watt-hours (Wh).
/// </summary>
/// <remarks>
/// This value represents the total energy delivered by the output
/// since the last reset. Updated automatically when telemetry frames are parsed.
/// </remarks>
public float MeasuredEnergy => _measuredEnergy;
```

---

## Verwendung

### Beispiel 1: Telemetrie mit Capacity & Energy lesen

```csharp
var registers = new DPS150Registers();
registers.ConnectToDevice(1);

// Parse telemetry data received from device
byte[] telemetryData = _communication.ReadResponse(5000);

if (registers.ParseTelemetryFrames(telemetryData))
{
	// Standard-Telemetrie
	Console.WriteLine($"Voltage:     {registers.MeasuredVoltage:F3}V");
	Console.WriteLine($"Current:     {registers.MeasuredCurrent:F3}A");
	Console.WriteLine($"Power:       {registers.MeasuredPower:F3}W");
	Console.WriteLine($"Input:       {registers.InputVoltage:F3}V");
	Console.WriteLine($"Temp:        {registers.InternalTemperature:F1}°C");

	// Erweiterte Telemetrie (wenn vorhanden)
	Console.WriteLine($"Capacity:    {registers.MeasuredCapacity:F3}Ah");
	Console.WriteLine($"Energy:      {registers.MeasuredEnergy:F3}Wh");
}
```

### Beispiel 2: Energie-Überwachung

```csharp
// Monitor energy consumption
while (true)
{
	byte[] data = _communication.ReadResponse(1000);
	if (registers.ParseTelemetryFrames(data))
	{
		float capacity = registers.MeasuredCapacity;
		float energy = registers.MeasuredEnergy;

		Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Capacity: {capacity:F3}Ah | Energy: {energy:F3}Wh");

		// Check if energy limit exceeded
		if (energy > 10.0f)
		{
			Console.WriteLine("⚠ Energy limit exceeded! Stopping output...");
			registers.OutputRelayState = false;
			break;
		}
	}

	Thread.Sleep(500);
}
```

### Beispiel 3: Kapazitäts-Test

```csharp
// Battery capacity test
Console.WriteLine("Starting battery capacity test...");
registers.VoltageSetpoint = 4.2f;    // 4.2V
registers.CurrentLimit = 1.0f;       // 1A discharge
registers.OutputRelayState = true;   // Start

// Reset counters (if supported by device)
// ... device-specific reset command ...

while (true)
{
	byte[] data = _communication.ReadResponse(1000);
	if (registers.ParseTelemetryFrames(data))
	{
		float voltage = registers.MeasuredVoltage;
		float current = registers.MeasuredCurrent;
		float capacity = registers.MeasuredCapacity;

		Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {voltage:F3}V @ {current:F3}A | {capacity:F3}Ah");

		// Stop when cutoff voltage reached
		if (voltage < 3.0f)
		{
			registers.OutputRelayState = false;
			Console.WriteLine($"✓ Test complete! Battery capacity: {capacity:F3}Ah");
			break;
		}
	}

	Thread.Sleep(1000);
}
```

---

## Wichtige Hinweise

### ⚠ **Optional Frames**

Die Frames für `MeasuredCapacity` (0xD9) und `MeasuredEnergy` (0xDA) sind **OPTIONAL** und werden nicht immer gesendet.

- Die Standard-Telemetrie (Frames 1-5) wird **immer** periodisch gesendet (alle 500ms)
- Die erweiterten Frames (Frames 6-7) werden nur gesendet wenn:
  - Das Gerät konfiguriert ist, diese Daten zu senden
  - Ein aktiver Lade-/Entladezyklus läuft
  - Die Werte sich geändert haben

### ✅ **Robustes Parsing**

Die `ParseTelemetryFrames` Methode ist robust implementiert:

```csharp
// Methode durchläuft den gesamten Buffer
// und sucht nach Frame-Start-Markern (F0 A1)
while (offset < data.Length - 4)
{
	// Check for frame start
	if (data[offset] != 0xF0 || data[offset + 1] != 0xA1)
	{
		offset++;
		continue; // Skip to next byte
	}

	// Parse frame...
}
```

**Vorteile:**

- Kann mit **partiellen Frames** umgehen (incomplete frames am Ende)
- Kann mit **variablen Frame-Sequenzen** umgehen (nicht alle Frames vorhanden)
- Kann mit **unbekannten Registern** umgehen (überspringt sie)
- Kann mit **ungültigen Checksummen** umgehen (überspringt fehlerhafte Frames)

### 📊 **Einheiten**

| Property | Einheit | Beschreibung |
|----------|---------|--------------|
| `MeasuredCapacity` | **Ah** (Amperestunden) | Gesamte Ladung seit Reset |
| `MeasuredEnergy` | **Wh** (Wattstunden) | Gesamte Energie seit Reset |

### 🔄 **Reset Counter**

Die Capacity- und Energy-Counter können möglicherweise zurückgesetzt werden über:

- Geräte-Reset
- Manuellen Counter-Reset am Gerät
- Spezifisches Kommando (siehe DPS-150 Protokoll-Dokumentation)

**Hinweis:** Das Reset-Verfahren ist gerätespezifisch und muss in der DPS-150 Dokumentation nachgeschlagen werden.

---

## Integration in DPS150RegistersTestProgram

Das Test-Programm kann jetzt auch die erweiterten Telemetrie-Werte anzeigen:

```csharp
private static void TestTelemetryFrameParser()
{
	Console.WriteLine("\n┌─────────────────────────────────────────────────────────┐");
	Console.WriteLine("│ TEST: TELEMETRY FRAME PARSER (EXTENDED)                │");
	Console.WriteLine("└─────────────────────────────────────────────────────────┘\n");

	if (!CheckConnection())
		return;

	Console.WriteLine("Reading telemetry frames from device...");

	// Read larger buffer to accommodate extended frames
	byte[]? data = _registers._communication.ReadResponse(2000);

	if (data != null)
	{
		Console.WriteLine($"Received {data.Length} bytes");

		if (_registers.ParseTelemetryFrames(data))
		{
			Console.WriteLine("\n✓ Parsed telemetry data:");
			Console.WriteLine($"  • Measured Voltage:       {_registers.MeasuredVoltage:F3} V");
			Console.WriteLine($"  • Measured Current:       {_registers.MeasuredCurrent:F3} A");
			Console.WriteLine($"  • Measured Power:         {_registers.MeasuredPower:F3} W");
			Console.WriteLine($"  • Input Voltage:          {_registers.InputVoltage:F3} V");
			Console.WriteLine($"  • Maximum Voltage:        {_registers.MaximumVoltage:F3} V");
			Console.WriteLine($"  • Maximum Current:        {_registers.MaximumCurrent:F3} A");
			Console.WriteLine($"  • Internal Temperature:   {_registers.InternalTemperature:F1} °C");

			// Extended telemetry (if present)
			if (_registers.MeasuredCapacity > 0.0f || _registers.MeasuredEnergy > 0.0f)
			{
				Console.WriteLine("\n✓ Extended telemetry:");
				Console.WriteLine($"  • Measured Capacity:      {_registers.MeasuredCapacity:F3} Ah");
				Console.WriteLine($"  • Measured Energy:        {_registers.MeasuredEnergy:F3} Wh");
			}
			else
			{
				Console.WriteLine("\nℹ Extended telemetry (Capacity/Energy) not present in this frame");
			}
		}
		else
		{
			Console.WriteLine("✗ Failed to parse telemetry frames!");
		}
	}
	else
	{
		Console.WriteLine("✗ No data received from device!");
	}
}
```

---

## Zusammenfassung

### ✅ Was wurde erweitert?

1. **Private Felder hinzugefügt:**
   - `_measuredCapacity` (float)
   - `_measuredEnergy` (float)

2. **ParseTelemetryFrames erweitert:**
   - Case `0xD9`: MeasuredCapacity
   - Case `0xDA`: MeasuredEnergy

3. **Public Properties hinzugefügt:**
   - `MeasuredCapacity` (read-only)
   - `MeasuredEnergy` (read-only)

4. **Dokumentation aktualisiert:**
   - XML-Kommentare in `ParseTelemetryFrames`
   - Frame-Struktur-Dokumentation
   - Beispiel-Code erweitert

### 📦 Frame-Größen

| Konfiguration | Frames | Größe | Verwendung |
|---------------|--------|-------|------------|
| **Standard** | 1-5 | 53 Bytes | Immer vorhanden |
| **Erweitert** | 1-7 | 71 Bytes | Optional |

### 🎯 Anwendungsfälle

- **Batterie-Kapazitäts-Tests**
- **Energie-Monitoring**
- **Lade-/Entlade-Zyklen überwachen**
- **Power-Budget-Management**
- **Langzeit-Verbrauchsanalyse**

Die erweiterte Telemetrie ermöglicht präzises Monitoring von Lade- und Entladevorgängen! 🔋⚡
