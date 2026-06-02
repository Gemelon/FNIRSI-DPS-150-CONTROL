# DPS-150 Telemetrie Frame Struktur

## Übersicht

Das FNIRSI DPS-150 Labornetzteil sendet periodisch Telemetriedaten in Form von mehreren aufeinanderfolgenden Frames. Diese Dokumentation beschreibt die Struktur und das Parsing dieser Frames.

## Gesamtstruktur

Die Telemetrie besteht aus **5 einzelnen Frames** mit insgesamt **53 Bytes**:

1. **Frame 1 (Measurement)**: 17 Bytes - Gemessene Spannung, Strom und Leistung
2. **Frame 2 (InputVoltage)**: 9 Bytes - Eingangsspannung
3. **Frame 3 (MaximumVoltage)**: 9 Bytes - Maximale Ausgangsspannung
4. **Frame 4 (MaximumCurrent)**: 9 Bytes - Maximaler Ausgangsstrom
5. **Frame 5 (InternalTemperature)**: 9 Bytes - Interne Temperatur

## Frame-Aufbau

Alle Frames folgen derselben Grundstruktur:

```
┌──────┬──────┬──────┬──────┬───────────────┬──────┐
│  F0  │  A1  │ REG  │ LEN  │     DATA      │ CHK  │
└──────┴──────┴──────┴──────┴───────────────┴──────┘
  Byte 0 Byte 1 Byte 2 Byte 3  Bytes 4..n-1  Byte n
```

- **Byte 0**: Header `0xF0` (RX - Device Response)
- **Byte 1**: Access Type `0xA1` (Read)
- **Byte 2**: Register-Adresse
- **Byte 3**: Datenlänge (Anzahl der Payload-Bytes)
- **Bytes 4..n-1**: Payload-Daten (IEEE-754 float32, little-endian)
- **Byte n**: Checksumme (sum(DATA[2..n-1]) & 0xFF)

## Frame 1: Measurement (0xC3)

**Länge**: 17 Bytes  
**Register**: `0xC3`  
**Payload**: 12 Bytes (3 × float32)

| Byte  | Wert | Beschreibung                          |
|-------|------|---------------------------------------|
| 0     | F0   | Header: RX                            |
| 1     | A1   | Access: Read                          |
| 2     | C3   | Register: Measurement                 |
| 3     | 0C   | Länge: 12 bytes                       |
| 4-7   | ...  | MeasuredVoltage (float32, LE)         |
| 8-11  | ...  | MeasuredCurrent (float32, LE)         |
| 12-15 | ...  | MeasuredPower (float32, LE)           |
| 16    | CHK  | Checksumme                            |

**Werte**:
- **MeasuredVoltage**: Gemessene Ausgangsspannung (0.0 - 150.0 V)
- **MeasuredCurrent**: Gemessener Ausgangsstrom (0.0 - 15.0 A)
- **MeasuredPower**: Gemessene Ausgangsleistung (0.0 - 300.0 W)

## Frame 2: InputVoltage (0xC0)

**Länge**: 9 Bytes  
**Register**: `0xC0`  
**Payload**: 4 Bytes (1 × float32)

| Byte  | Wert | Beschreibung                        |
|-------|------|-------------------------------------|
| 0     | F0   | Header: RX                          |
| 1     | A1   | Access: Read                        |
| 2     | C0   | Register: InputVoltage              |
| 3     | 04   | Länge: 4 bytes                      |
| 4-7   | ...  | InputVoltage (float32, LE)          |
| 8     | CHK  | Checksumme                          |

**Wert**:
- **InputVoltage**: Eingangsspannung der Stromversorgung (typisch 12-60 V)

## Frame 3: MaximumVoltage (0xE2)

**Länge**: 9 Bytes  
**Register**: `0xE2`  
**Payload**: 4 Bytes (1 × float32)

| Byte  | Wert | Beschreibung                         |
|-------|------|--------------------------------------|
| 0     | F0   | Header: RX                           |
| 1     | A1   | Access: Read                         |
| 2     | E2   | Register: MaximumVoltage             |
| 3     | 04   | Länge: 4 bytes                       |
| 4-7   | ...  | MaximumVoltage (float32, LE)         |
| 8     | CHK  | Checksumme                           |

**Wert**:
- **MaximumVoltage**: Maximale Ausgangsspannung des Geräts (typisch 150.0 V)

## Frame 4: MaximumCurrent (0xE3)

**Länge**: 9 Bytes  
**Register**: `0xE3`  
**Payload**: 4 Bytes (1 × float32)

| Byte  | Wert | Beschreibung                         |
|-------|------|--------------------------------------|
| 0     | F0   | Header: RX                           |
| 1     | A1   | Access: Read                         |
| 2     | E3   | Register: MaximumCurrent             |
| 3     | 04   | Länge: 4 bytes                       |
| 4-7   | ...  | MaximumCurrent (float32, LE)         |
| 8     | CHK  | Checksumme                           |

**Wert**:
- **MaximumCurrent**: Maximaler Ausgangsstrom des Geräts (typisch 15.0 A)

## Frame 5: InternalTemperature (0xC4)

**Länge**: 9 Bytes  
**Register**: `0xC4`  
**Payload**: 4 Bytes (1 × float32)

| Byte  | Wert | Beschreibung                              |
|-------|------|-------------------------------------------|
| 0     | F0   | Header: RX                                |
| 1     | A1   | Access: Read                              |
| 2     | C4   | Register: InternalTemperature             |
| 3     | 04   | Länge: 4 bytes                            |
| 4-7   | ...  | InternalTemperature (float32, LE)         |
| 8     | CHK  | Checksumme                                |

**Wert**:
- **InternalTemperature**: Interne Temperatur des Geräts in °C (typisch 20-80 °C)

## Parsing-Algorithmus

Die `ParseTelemetryFrames(byte[] data)`-Methode in der `DPS150Registers`-Klasse implementiert den folgenden Algorithmus:

1. **Frame-Start erkennen**: Suche nach Sequenz `F0 A1`
2. **Register-Adresse extrahieren**: Byte 2
3. **Datenlänge extrahieren**: Byte 3
4. **Frame-Länge berechnen**: `2 + 1 + 1 + dataLength + 1`
5. **Checksumme verifizieren**: `sum(DATA[2..n-1]) & 0xFF`
6. **Payload dekodieren**: Je nach Register-Adresse:
   - `0xC3`: 3 float32-Werte (Voltage, Current, Power)
   - `0xC0`: 1 float32-Wert (InputVoltage)
   - `0xE2`: 1 float32-Wert (MaximumVoltage)
   - `0xE3`: 1 float32-Wert (MaximumCurrent)
   - `0xC4`: 1 float32-Wert (InternalTemperature)
7. **Nächstes Frame**: Offset um Frame-Länge erhöhen und zu Schritt 1 zurückkehren

## Beispiel: Vollständige Telemetrie-Sequenz

```
Frame 1 (Measurement): 
F0 A1 C3 0C 00 00 40 41 00 00 C0 3F 00 00 90 41 XX
└─────────┘ └─────────────────────────────────┘ └─┘
  Header         12.0V, 1.5A, 18.0W              CHK

Frame 2 (InputVoltage):
F0 A1 C0 04 00 00 C0 41 XX
└─────────┘ └─────────┘ └─┘
  Header      24.0V      CHK

Frame 3 (MaximumVoltage):
F0 A1 E2 04 00 00 16 43 XX
└─────────┘ └─────────┘ └─┘
  Header      150.0V     CHK

Frame 4 (MaximumCurrent):
F0 A1 E3 04 00 00 70 41 XX
└─────────┘ └─────────┘ └─┘
  Header       15.0A     CHK

Frame 5 (InternalTemperature):
F0 A1 C4 04 00 00 0E 42 XX
└─────────┘ └─────────┘ └─┘
  Header      35.5°C     CHK

Gesamt: 53 Bytes
```

## Verwendung in der Software

```csharp
// Telemetriedaten vom Gerät empfangen
byte[] telemetryData = _communication.ReadResponse(5000);

// Frames parsen und Properties aktualisieren
if (_registers.ParseTelemetryFrames(telemetryData))
{
	// Zugriff auf die Werte über Properties
	float voltage = _registers.MeasuredVoltage;
	float current = _registers.MeasuredCurrent;
	float power = _registers.MeasuredPower;
	float inputVoltage = _registers.InputVoltage;
	float maxVoltage = _registers.MaximumVoltage;
	float maxCurrent = _registers.MaximumCurrent;
	float temperature = _registers.InternalTemperature;

	Console.WriteLine($"Output: {voltage:F3}V, {current:F3}A, {power:F3}W");
	Console.WriteLine($"Input: {inputVoltage:F3}V");
	Console.WriteLine($"Device Max: {maxVoltage:F1}V, {maxCurrent:F1}A");
	Console.WriteLine($"Temperature: {temperature:F1}°C");
}
```

## Hinweise

1. **Periodisches Senden**: Das Gerät sendet diese Telemetrie automatisch in regelmäßigen Intervallen (ca. 500ms)
2. **Keine Bestätigung erforderlich**: Telemetrie-Frames sind unaufgefordert und müssen nicht bestätigt werden
3. **Frame-Reihenfolge**: Die Frames werden immer in der oben beschriebenen Reihenfolge gesendet
4. **Checksummen-Validierung**: Jedes Frame muss individuell auf Checksummen-Korrektheit geprüft werden
5. **Float-Format**: Alle Fließkommawerte sind IEEE-754 float32 im Little-Endian-Format
6. **Fehlerbehandlung**: Frames mit ungültiger Checksumme werden übersprungen, ohne das Parsing abzubrechen

## Referenz

Diese Implementierung basiert auf:
- **Protokoll-Dokumentation**: [FNIRSI DPS-150 Protocol](https://github.com/cho45/fnirsi-dps-150/blob/main/docs/FNIRSI_DPS-150_Protocol.md)
- **Original-Projekt**: [cho45/fnirsi-dps-150](https://github.com/cho45/fnirsi-dps-150)

## Siehe auch

- `DPS150Registers.cs` - Implementierung der `ParseTelemetryFrames`-Methode
- `FLOAT_ENDIANNESS_ANALYSIS.md` - Details zum IEEE-754 float32 Little-Endian-Format
- `CHECKSUM_DOCUMENTATION.md` - Details zur Checksummen-Berechnung
- `DPS150RegistersTestProgram.cs` - Test-Suite mit synthetischen Telemetrie-Frames
