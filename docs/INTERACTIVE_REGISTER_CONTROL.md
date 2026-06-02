# DPS150RegistersTestProgram - Interaktive Register-Steuerung

## Übersicht

Das `DPS150RegistersTestProgram` wurde erweitert mit **7 interaktiven Menüoptionen** zum direkten Setzen von Register-Werten am verbundenen DPS-150-Gerät.

## Neue Menüoptionen

### **[S1] Set Voltage Setpoint**
Setzt die Ausgangsspannung interaktiv.

```
┌─────────────────────────────────────────────────────────┐
│ SET VOLTAGE SETPOINT                                    │
└─────────────────────────────────────────────────────────┘

Current voltage: 0.000V
Enter new voltage (0.0 - 150.0 V): 12.5
Setting voltage to 12.500V...
✓ Voltage set to: 12.500V
```

**Wertebereich:** 0.0 - 150.0 V  
**Validierung:** Automatische Bereichsprüfung  
**Bestätigung:** Zeigt bestätigten Wert nach Write-Read-Back

---

### **[S2] Set Current Limit**
Setzt die Strombegrenzung interaktiv.

```
┌─────────────────────────────────────────────────────────┐
│ SET CURRENT LIMIT                                       │
└─────────────────────────────────────────────────────────┘

Current limit: 0.000A
Enter new current limit (0.0 - 15.0 A): 2.5
Setting current limit to 2.500A...
✓ Current limit set to: 2.500A
```

**Wertebereich:** 0.0 - 15.0 A  
**Validierung:** Automatische Bereichsprüfung  
**Bestätigung:** Zeigt bestätigten Wert nach Write-Read-Back

---

### **[S3] Set Output Relay (ON/OFF)**
Schaltet den Ausgangsrelais (RUN/STOP).

```
┌─────────────────────────────────────────────────────────┐
│ SET OUTPUT RELAY (RUN/STOP)                             │
└─────────────────────────────────────────────────────────┘

Current state: STOP (OFF)

[1] Turn ON (RUN)
[2] Turn OFF (STOP)

Select: 1
Turning output ON...
✓ Output is now: ON
```

**Optionen:**  
- `[1]` = RUN (Ausgang aktiv)
- `[2]` = STOP (Ausgang inaktiv)

**Bestätigung:** Zeigt aktuellen Zustand nach Änderung

---

### **[S4] Set Preset Memory (M1-M6)**
Konfiguriert einen der 6 Preset-Speicherplätze mit Spannung und Strom.

```
┌─────────────────────────────────────────────────────────┐
│ SET PRESET MEMORY (M1-M6)                               │
└─────────────────────────────────────────────────────────┘

Select preset slot:
[1] M1   [2] M2   [3] M3
[4] M4   [5] M5   [6] M6

Select preset (1-6): 1

Enter voltage for M1 (0.0 - 150.0 V): 5.0
Enter current for M1 (0.0 - 15.0 A): 1.0

Setting M1 to 5.000V @ 1.000A...
✓ M1: 5.000V @ 1.000A
```

**Preset-Slots:** M1 - M6  
**Wertebereich Spannung:** 0.0 - 150.0 V  
**Wertebereich Strom:** 0.0 - 15.0 A  
**Bestätigung:** Zeigt konfigurierte Werte für den gewählten Preset

---

### **[S5] Set Protection Settings**
Konfiguriert Schutzfunktionen (OVP, OCP, OPP, OTP, LVP).

```
┌─────────────────────────────────────────────────────────┐
│ SET PROTECTION SETTINGS                                 │
└─────────────────────────────────────────────────────────┘

Current protection settings:
  OVP (Over-Voltage):       0.000V
  OCP (Over-Current):       0.000A
  OPP (Over-Power):         0.000W
  OTP (Over-Temperature):   0.0°C
  LVP (Low-Voltage):        0.000V

Select protection to configure:
[1] OVP   [2] OCP   [3] OPP   [4] OTP   [5] LVP   [6] All

Select: 6

OVP (0.0 - 160.0 V): 14.0
OCP (0.0 - 20.0 A): 3.0
OPP (0.0 - 3000.0 W): 45.0
OTP (0.0 - 100.0 °C): 80.0
LVP (0.0 - 30.0 V): 10.0

✓ All protection settings updated!
```

**Schutzfunktionen:**

| Funktion | Beschreibung | Wertebereich |
|----------|--------------|--------------|
| **OVP** | Over-Voltage Protection | 0.0 - 160.0 V |
| **OCP** | Over-Current Protection | 0.0 - 20.0 A |
| **OPP** | Over-Power Protection | 0.0 - 3000.0 W |
| **OTP** | Over-Temperature Protection | 0.0 - 100.0 °C |
| **LVP** | Low-Voltage Protection | 0.0 - 30.0 V |

**Optionen:**  
- `[1-5]` = Einzelne Schutzfunktion konfigurieren
- `[6]` = Alle Schutzfunktionen nacheinander konfigurieren

---

### **[S6] Set UI Settings (Brightness/Volume)**
Konfiguriert Display-Helligkeit und Lautstärke.

```
┌─────────────────────────────────────────────────────────┐
│ SET UI SETTINGS                                         │
└─────────────────────────────────────────────────────────┘

Current settings:
  Brightness: 0%
  Volume:     0%

[1] Set Brightness
[2] Set Volume
[3] Set Both

Select: 3

Enter brightness (0-100%): 75
Enter volume (0-100%): 50
✓ Brightness: 75%, Volume: 50%
```

**UI-Einstellungen:**

| Setting | Beschreibung | Wertebereich |
|---------|--------------|--------------|
| **Brightness** | Display-Helligkeit | 0 - 100 % |
| **Volume** | Audio-Lautstärke | 0 - 100 % |

**Optionen:**  
- `[1]` = Nur Helligkeit ändern
- `[2]` = Nur Lautstärke ändern
- `[3]` = Beide Einstellungen ändern

---

### **[S7] Quick Start**
Schnellkonfiguration: Spannung + Strom + Optional Ausgang aktivieren.

```
┌─────────────────────────────────────────────────────────┐
│ QUICK START (Voltage + Current + Output ON)            │
└─────────────────────────────────────────────────────────┘

This will configure voltage, current and turn output ON.

Enter voltage (0.0 - 150.0 V): 12.0
Enter current limit (0.0 - 15.0 A): 2.0

Configuring: 12.000V @ 2.000A
Turn output ON immediately? (y/n): y

[1/3] Setting voltage...
	  ✓ Voltage: 12.000V
[2/3] Setting current limit...
	  ✓ Current: 2.000A
[3/3] Turning output ON...
	  ✓ Output: ON

✓ Quick Start complete!
   Voltage: 12.000V
   Current: 2.000A
   Output:  ON
```

**Schritte:**  
1. Fragt Zielspannung ab (0.0 - 150.0 V)
2. Fragt Stromgrenze ab (0.0 - 15.0 A)
3. Fragt ob Ausgang sofort aktiviert werden soll
4. Konfiguriert alle Parameter in einem Durchlauf

**Ideal für:** Schnelles Starten des Geräts mit bekannten Werten

---

## Gemeinsame Funktionen

### ✅ **Verbindungsprüfung**
Alle interaktiven Funktionen prüfen automatisch, ob eine Verbindung zum Gerät besteht:

```
✗ Not connected to device!
  Please connect first using option [2]
```

### ✅ **Eingabe-Validierung**
Alle Werte werden auf gültige Bereiche geprüft:

```
✗ Invalid voltage! Must be between 0.0 and 150.0V
```

```
✗ Invalid current! Must be between 0.0 and 15.0A
```

### ✅ **Write-and-Read-Back**
Alle Setzer verwenden die `WriteAndReadBack*`-Helper-Methoden:
- Senden Write-Command
- Warten auf Response
- Senden Read-Command
- Zeigen bestätigten Wert

### ✅ **Telemetrie-Update**
Nach jedem Write-Command werden automatisch Telemetrie-Frames geparst und aktualisiert.

---

## Verwendungs-Workflow

### **Typischer Workflow:**

```
1. Start DPS150RegistersTestProgram
2. [1] List Available Ports
3. [2] Connect to Device (by port number)
4. [S7] Quick Start
   - Enter voltage: 12.0
   - Enter current: 2.0
   - Turn on: y
5. Monitor output with telemetry
6. [S3] Set Output Relay -> [2] Turn OFF
7. [3] Disconnect from Device
8. [0] Exit
```

### **Sicherheits-Workflow:**

```
1. Connect to device [2]
2. [S5] Set Protection Settings -> [6] All
   - OVP: 14.0
   - OCP: 3.0
   - OPP: 45.0
   - OTP: 80.0
   - LVP: 10.0
3. [S1] Set Voltage Setpoint -> 12.0
4. [S2] Set Current Limit -> 2.0
5. [S3] Set Output Relay -> [1] Turn ON
```

### **Preset-Konfigurations-Workflow:**

```
1. Connect to device [2]
2. [S4] Set Preset Memory
   - M1: 5V @ 1A
3. [S4] Set Preset Memory
   - M2: 12V @ 2.5A
4. [S4] Set Preset Memory
   - M3: 24V @ 1.5A
```

---

## Best Practices

### ✅ **Immer zuerst Schutzfunktionen konfigurieren**
Bevor Sie Spannung/Strom setzen:
```
[S5] Set Protection Settings
[S1] Set Voltage Setpoint
[S2] Set Current Limit
[S3] Set Output Relay
```

### ✅ **Nutzen Sie Quick Start für bekannte Konfigurationen**
Wenn Sie häufig die gleichen Werte verwenden:
```
[S7] Quick Start -> 12V @ 2A
```

### ✅ **Presets für häufige Konfigurationen**
Speichern Sie häufig verwendete Einstellungen in Presets M1-M6:
```
M1: 5V @ 1A (Arduino)
M2: 12V @ 2A (Standard)
M3: 24V @ 1.5A (Industrial)
```

### ✅ **UI-Settings nach Präferenz anpassen**
```
[S6] Set UI Settings
Brightness: 80%
Volume: 50%
```

---

## Fehlerbehandlung

### **Keine Verbindung**
```
✗ Not connected to device!
  Please connect first using option [2]
```
**Lösung:** Erst mit `[2] Connect to Device` verbinden

### **Ungültige Eingabe**
```
✗ Invalid input!
```
**Lösung:** Nur numerische Werte eingeben (Dezimalpunkt mit `.`)

### **Wert außerhalb des Bereichs**
```
✗ Invalid voltage! Must be between 0.0 and 150.0V
```
**Lösung:** Wert im gültigen Bereich eingeben

### **Kommunikationsfehler**
Wenn die Anzeige nach dem Setzen nicht aktualisiert wird:
- Gerät könnte nicht antworten
- Session nicht gestartet (wird automatisch versucht)
- Timeout bei der Kommunikation

**Lösung:**
1. Prüfen Sie die Verbindung
2. Versuchen Sie es erneut
3. Trennen und neu verbinden `[3]` -> `[2]`

---

## Technische Details

### **Verwendete Helper-Methoden**

| Register-Typ | Helper-Methode | Beispiel-Properties |
|--------------|----------------|---------------------|
| `float` | `WriteAndReadBackFloat` | VoltageSetpoint, CurrentLimit, OVP, OCP, Presets |
| `byte` | `WriteAndReadBackByte` | Brightness, Volume |
| `bool` | `WriteAndReadBackBool` | OutputRelayState |

### **Automatische Features**

1. **Session-Management**
   - Automatischer Session-Start bei Bedarf
   - 50ms Wartezeit nach Session-Start

2. **Telemetrie-Parsing**
   - Nach jedem Write-Command
   - Aktualisiert MeasuredVoltage, MeasuredCurrent, etc.

3. **Write-Read-Back-Verifikation**
   - Jeder Setter liest den Wert zurück
   - Zeigt bestätigten Wert an
   - Garantiert Synchronisation mit Gerät

4. **Checksummen-Validierung**
   - Automatisch für alle Pakete
   - Fehlerhafte Pakete werden abgewiesen

### **Timeouts**
- Write-Command: 1000ms
- Read-Command: 1000ms
- Anzeigeverzögerung: 200ms (für Benutzerfeedback)

---

## Zusammenfassung

Die erweiterte Version von `DPS150RegistersTestProgram` bietet:

✅ **7 interaktive Menüoptionen** für direktes Register-Setzen  
✅ **Automatische Eingabe-Validierung** mit Bereichsprüfung  
✅ **Write-and-Read-Back** für alle Werte  
✅ **Verbindungsprüfung** vor jeder Operation  
✅ **Quick Start** für schnelle Konfiguration  
✅ **Preset-Management** für häufige Konfigurationen  
✅ **Schutzfunktionen-Konfiguration** für sicheren Betrieb  
✅ **UI-Anpassung** für Helligkeit und Lautstärke  

Diese Erweiterungen machen das Test-Programm zu einem **vollwertigen interaktiven Konfigurations-Tool** für das DPS-150 Labornetzteil.

## Siehe auch

- **DPS150Device.cs** - Register-Property-Implementierung
- **COMPLETE_PROPERTY_IMPLEMENTATION.md** - Vollständige Property-Dokumentation
- **CODE_IMPROVEMENTS_REGISTER_PROPERTIES.md** - Refactoring-Details
- **TELEMETRY_FRAME_DOCUMENTATION.md** - Telemetrie-Parsing-Dokumentation
