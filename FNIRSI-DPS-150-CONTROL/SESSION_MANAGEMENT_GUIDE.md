# DPS-150 Session Management Guide

## Overview

The DPS-150 power supply requires an active **session** before most commands can be executed. This document explains how session management works in the `DPS150Control` class.

## What is a Session?

A session is a communication state that must be established before the device accepts control commands. Think of it as "unlocking" the device for remote control.

### Session Control Commands

```
Start Session:  F1 C1 00 01 01 02
Stop Session:   F1 C1 00 01 00 01
```

## Automatic Session Management

The `DPS150Control` class provides **automatic session management** for convenience:

### Methods with Auto-Session Start

The following methods automatically start a session if one is not already active:

1. **`SetOutputRelay(OutputRelayState state)`**
   - Turns the output ON or OFF
   - Auto-starts session if needed

2. **`SetVoltage(float voltage)`**
   - Sets the voltage setpoint
   - Auto-starts session if needed

### Implementation Pattern

```csharp
public bool SetVoltage(float voltage)
{
	if (!IsConnected)
	{
		return false;
	}

	// Ensure a session is started before sending voltage command
	if (!_sessionStarted)
	{
		if (!StartSession())
		{
			return false; // Failed to start session
		}

		// Give device time to process session start command
		System.Threading.Thread.Sleep(50);
	}

	// ... send voltage command ...
}
```

## Manual Session Management

You can also manually manage sessions:

```csharp
var control = new DPS150Control();
control.ConnectToDevice("COM3");

// Manually start session
if (control.StartSession())
{
	Console.WriteLine("Session started");

	// Now you can send multiple commands
	control.SetVoltage(12.5f);
	control.SetOutputRelay(OutputRelayState.ON);

	// ... more operations ...

	// Manually stop session when done
	control.StopSession();
}

control.DisconnectFromDevice();
```

## Session State Tracking

The `DPS150Control` class tracks the session state internally:

```csharp
// Check if session is active
bool isActive = control.IsSessionStarted;
```

## Best Practices

### 1. Let Auto-Session Handle It (Recommended)

For simple use cases, just call the control methods directly:

```csharp
var control = new DPS150Control();
control.ConnectToDevice("COM3");

// Session auto-starts on first command
control.SetVoltage(12.5f);
control.SetOutputRelay(OutputRelayState.ON);

// Session auto-stops on disconnect
control.DisconnectFromDevice();
```

### 2. Manual Session for Batch Operations

For multiple commands in sequence, manually starting a session once is more efficient:

```csharp
var control = new DPS150Control();
control.ConnectToDevice("COM3");

// Start session once
control.StartSession();

// Send multiple commands (no session overhead)
control.SetVoltage(12.0f);
System.Threading.Thread.Sleep(100);
control.SetVoltage(15.0f);
System.Threading.Thread.Sleep(100);
control.SetVoltage(18.0f);
System.Threading.Thread.Sleep(100);
control.SetOutputRelay(OutputRelayState.ON);

// Stop session when done
control.StopSession();
control.DisconnectFromDevice();
```

### 3. Disconnect Cleanup

**Important**: `DisconnectFromDevice()` automatically stops the session before disconnecting:

```csharp
public bool DisconnectFromDevice()
{
	if (!IsConnected)
	{
		return false;
	}

	// If a session is active, stop it before disconnecting
	if (_sessionStarted)
	{
		StopSession();
		System.Threading.Thread.Sleep(100);
	}

	_communication.Disconnect();
	return true;
}
```

This ensures clean disconnection and prevents leaving the device in an active session state.

## Session Lifecycle

```
┌─────────────────────────────────────────────────────────────┐
│ Device Lifecycle with Session Management                    │
└─────────────────────────────────────────────────────────────┘

1. ConnectToDevice()
   ├─ Serial port opened
   └─ Session: INACTIVE

2. SetVoltage() / SetOutputRelay() [First call]
   ├─ Auto-starts session
   ├─ Wait 50ms for device processing
   └─ Session: ACTIVE

3. Subsequent commands
   ├─ Session already active
   └─ No session overhead

4. DisconnectFromDevice()
   ├─ Auto-stops session if active
   ├─ Wait 100ms for device processing
   └─ Serial port closed
```

## Error Handling

### Connection Required

All session-related methods check for an active connection first:

```csharp
if (!IsConnected)
{
	return false; // Cannot start session without connection
}
```

### Session Start Failure

If session start fails, subsequent commands will not be sent:

```csharp
if (!_sessionStarted)
{
	if (!StartSession())
	{
		return false; // Failed to start session
	}
}
```

## Protocol Reference

### Session Start Packet

```
F1 C1 00 01 01 02
│  │  │  │  │  └─ Checksum
│  │  │  │  └──── Data: 0x01 (START)
│  │  │  └─────── Length: 1 byte
│  │  └────────── Register: 0x00 (Session Control)
│  └───────────── Access: 0xC1 (Control)
└──────────────── Header: 0xF1 (TX)
```

### Session Stop Packet

```
F1 C1 00 01 00 01
│  │  │  │  │  └─ Checksum
│  │  │  │  └──── Data: 0x00 (STOP)
│  │  │  └─────── Length: 1 byte
│  │  └────────── Register: 0x00 (Session Control)
│  └───────────── Access: 0xC1 (Control)
└──────────────── Header: 0xF1 (TX)
```

## Testing

The `TestProgram` provides menu options to test session management:

- **[5]** Start Session (manual)
- **[6]** Stop Session (manual)
- **[V]** Set Voltage (auto-session)
- **[A]** Set Output Relay ON (auto-session)
- **[B]** Set Output Relay OFF (auto-session)
- **[F]** Run Full Device Control Test (comprehensive test)

## Summary

✓ **Auto-session** is enabled for `SetVoltage()` and `SetOutputRelay()`  
✓ **Manual session** control is available via `StartSession()` / `StopSession()`  
✓ **Session tracking** prevents redundant session starts  
✓ **Clean disconnect** automatically stops active sessions  
✓ **50ms delay** after session start ensures device readiness  
✓ **Error handling** prevents commands without active session
