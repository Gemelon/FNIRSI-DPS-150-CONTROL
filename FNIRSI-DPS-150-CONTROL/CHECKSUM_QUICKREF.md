# Checksummen-Berechnung - Schnellreferenz

## ✅ Korrekte Regel

```
CHK = sum(DATA[2..n]) & 0xFF
```

## 📦 Paketstruktur

```
Byte Index:  [0]    [1]    [2]   [3]   [4..n]    [last]
			┌──────┬──────┬─────┬─────┬────────┬────────┐
Packet:     │ F1   │ B1   │ C1  │ 04  │ Payload│  CHK   │
			└──────┴──────┴─────┴─────┴────────┴────────┘
Name:       │Header│Access│ REG │ LEN │  DATA  │Checksum│
			└──────┴──────┴─────┴─────┴────────┴────────┘
In CHK?     │  NO  │  NO  │ YES │ YES │  YES   │   -    │
			└──────┴──────┴─────┴─────┴────────┴────────┘
```

### Wichtig!
- **Byte 0 (Header)**: NICHT in Checksumme
- **Byte 1 (Access)**: NICHT in Checksumme
- **Byte 2+ (REG, LEN, Payload)**: IN Checksumme

## 🧮 Berechnungsbeispiel

**Paket:** `F1 B1 C1 04 00 00 B0 40 B5`

```
Index:  0    1    2    3    4    5    6    7    8
	   ┌────┬────┬────┬────┬────┬────┬────┬────┬────┐
Byte:  │ F1 │ B1 │ C1 │ 04 │ 00 │ 00 │ B0 │ 40 │ B5 │
	   └────┴────┴────┴────┴────┴────┴────┴────┴────┘
				↑    ↑    ↑    ↑    ↑    ↑    ↑
				└────┴────┴────┴────┴────┴────┘
				  Diese Bytes summieren!
```

**Berechnung:**
```
Sum = C1 + 04 + 00 + 00 + B0 + 40
	= 193 + 4 + 0 + 0 + 176 + 64
	= 437 (decimal)
	= 0x1B5 (hex)

CHK = 0x1B5 & 0xFF
	= 0xB5 ✓
```

## 💻 Code-Beispiele

### Methode 1: Komponenten-basiert

```csharp
byte reg = 0xC1;
byte len = 0x04;
byte[] payload = { 0x00, 0x00, 0xB0, 0x40 };

byte chk = DPS150Control.CalculateChecksum(reg, len, payload);
// Ergebnis: 0xB5
```

### Methode 2: Array-basiert (mit Header)

```csharp
byte[] packet = { 0xF1, 0xB1, 0xC1, 0x04, 0x00, 0x00, 0xB0, 0x40 };

byte chk = DPS150Control.CalculateChecksum(packet);
// Überspringt automatisch die ersten 2 Bytes!
// Ergebnis: 0xB5
```

### Methode 3: Verifizierung

```csharp
byte[] fullPacket = { 0xF1, 0xB1, 0xDB, 0x01, 0x01, 0xDD };

bool valid = DPS150Control.VerifyChecksum(fullPacket);
// Ergebnis: true
```

## ❌ Häufige Fehler

### FALSCH ❌
```csharp
// Header-Bytes mitgezählt
sum = 0xF1 + 0xB1 + 0xC1 + 0x04 + ...  // FALSCH!
```

### RICHTIG ✅
```csharp
// Header-Bytes übersprungen
sum = 0xC1 + 0x04 + 0x00 + ...  // RICHTIG!
```

## 📊 Test-Beispiele

| Paket | REG | LEN | Payload | CHK | Prüfung |
|-------|-----|-----|---------|-----|---------|
| `F1 B1 C1 04 00 00 B0 40` | C1 | 04 | 00 00 B0 40 | B5 | ✓ |
| `F1 B1 DB 01 01` | DB | 01 | 01 | DD | ✓ |
| `F1 B1 DB 01 00` | DB | 01 | 00 | DC | ✓ |
| `F1 B1 D6 01 0C` | D6 | 01 | 0C | E3 | ✓ |
| `F1 B1 D7 01 09` | D7 | 01 | 09 | E1 | ✓ |

## 🔧 Implementierungs-Details

### Mindestanforderungen
- **Minimale Paketlänge**: 4 Bytes (Header + Access + REG + LEN)
- **Minimale Checksummen-Länge**: 2 Bytes (REG + LEN, wenn LEN=0)

### Datentypen
- Alle Bytes sind `byte` (0-255)
- Summe wird als `int` berechnet (Overflow-sicher)
- Ergebnis wird mit `& 0xFF` auf 1 Byte maskiert

### Overflow-Behandlung
```
Beispiel: Sum = 437 (0x1B5)
Result = 0x1B5 & 0xFF = 0xB5 (181 decimal)
```

## 📚 Referenzen

- **Implementierung**: `DPS150Control.cs`
- **Tests**: `Program.cs` (Option "T")
- **Vollständige Doku**: `CHECKSUM_DOCUMENTATION.md`
- **Protokoll**: [cho45/fnirsi-dps-150](https://github.com/cho45/fnirsi-dps-150/blob/main/docs/FNIRSI_DPS-150_Protocol.md)
