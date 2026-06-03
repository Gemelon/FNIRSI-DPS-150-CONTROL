# Charging Profile Execution Guide

## Overview

The DPS150Control class supports automated battery charging profiles defined in YAML files. This feature allows you to execute complex multi-step charging cycles (CC/CV/Wait) with automatic monitoring and cut-off conditions.

## YAML Profile Format

A charging profile consists of:

- **Title**: Profile name
- **Capacity**: Battery capacity (e.g., "5Ah")
- **CorrectionVoltage**: Voltage correction factor to compensate for voltage drop (e.g., "0.7V")
- **CycleSteps**: List of charging steps

### Example Profile

```yaml
Title: EVE LiFePO4 Charging Profile
Capacity: 5Ah
CorrectionVoltage: 0.7V
CycleSteps:
  - Name: Bulk
	Mode: CC
	Current: 1A
	CutOffVoltage: 3.55V
	MaxTime: 5:15h
  - Name: Rest
	Mode: WAIT
	Time: 0:15h
  - Name: Absorption
	Mode: CV
	Voltage: 3.55V
	CutOffCurrent: 0.1A
	MaxTime: 2h
  - Name: Float
	Mode: CV
	Voltage: 3.4V
	Current: 0.05A
	Time: Indefinite
```

## Step Modes

### Constant Current (CC)
- **Required**: `Current`, `CutOffVoltage`
- **Optional**: `MaxTime`
- **Behavior**: Maintains constant current, monitors voltage until CutOffVoltage + CorrectionVoltage is reached

### Constant Voltage (CV)
- **Required**: `Voltage`
- **Optional**: `Current`, `CutOffCurrent`, `Time`, `MaxTime`
- **Behavior**: Maintains constant voltage, monitors current until CutOffCurrent is reached or Time elapses
- **Special**: Use `Time: Indefinite` to run until manually cancelled

### Wait/Rest (WAIT)
- **Required**: `Time`
- **Behavior**: Turns output OFF and waits for specified duration

## Time Format

Time values support the following formats:
- `2h` - 2 hours
- `5:15h` - 5 hours and 15 minutes
- `0:15h` - 15 minutes
- `Indefinite` - Run until cancelled (CV mode only)

## Usage Example

### C# Code

```csharp
using FNIRSI_DPS_150_CONTROL;
using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main(string[] args)
	{
		var control = new DPS150Control();

		// Connect to device
		if (!control.ConnectToDevice("COM3"))
		{
			Console.WriteLine("Failed to connect to device");
			return;
		}

		// Subscribe to events
		control.ChargingStepStarted += OnStepStarted;
		control.ChargingStepCompleted += OnStepCompleted;

		// Create cancellation token (optional)
		var cts = new CancellationTokenSource();
		Console.CancelKeyPress += (s, e) =>
		{
			e.Cancel = true;
			cts.Cancel();
			Console.WriteLine("\nCharging cancelled by user");
		};

		try
		{
			// Execute charging profile
			await control.ExecuteChargingProfileAsync(
				@"D:\Projekte\EVE LiFePO4 Charging Profile.yaml",
				cts.Token
			);

			Console.WriteLine("Charging profile completed!");
		}
		catch (OperationCanceledException)
		{
			Console.WriteLine("Charging was cancelled");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
		finally
		{
			control.DisconnectFromDevice();
		}
	}

	static void OnStepStarted(object? sender, DPS150Control.ChargingStepStartedEventArgs e)
	{
		Console.WriteLine($"\n[{e.StartTime:HH:mm:ss}] Starting Step {e.StepIndex + 1}/{e.TotalSteps}: {e.Step.Name}");
		Console.WriteLine($"  Mode: {e.Step.Mode}");

		switch (e.Step.Mode)
		{
			case ChargingMode.ConstantCurrent:
				Console.WriteLine($"  Current: {e.Step.Current}A");
				Console.WriteLine($"  CutOff Voltage: {e.Step.CutOffVoltage}V");
				break;
			case ChargingMode.ConstantVoltage:
				Console.WriteLine($"  Voltage: {e.Step.Voltage}V");
				if (e.Step.CutOffCurrent.HasValue)
					Console.WriteLine($"  CutOff Current: {e.Step.CutOffCurrent}A");
				break;
			case ChargingMode.Wait:
				Console.WriteLine($"  Duration: {e.Step.Time}");
				break;
		}
	}

	static void OnStepCompleted(object? sender, DPS150Control.ChargingStepCompletedEventArgs e)
	{
		Console.WriteLine($"[{e.EndTime:HH:mm:ss}] Completed Step {e.StepIndex + 1}/{e.TotalSteps}: {e.Step.Name}");
		Console.WriteLine($"  Reason: {e.CompletionReason}");
		Console.WriteLine($"  Duration: {e.Duration:hh\\:mm\\:ss}");
		Console.WriteLine($"  Final: {e.FinalVoltage:F3}V / {e.FinalCurrent:F3}A");
	}
}
```

## Events

### ChargingStepStarted
Raised when a charging step begins. Provides:
- `StepIndex`: Current step index (0-based)
- `TotalSteps`: Total number of steps
- `Step`: The CycleStep object
- `StartTime`: When the step started

### ChargingStepCompleted
Raised when a charging step finishes. Provides:
- `StepIndex`: Completed step index
- `TotalSteps`: Total number of steps
- `Step`: The CycleStep object
- `StartTime`: When the step started
- `EndTime`: When the step ended
- `Duration`: How long the step took
- `CompletionReason`: Why the step ended (e.g., "CutOff voltage reached", "Timeout", "Cancelled")
- `FinalVoltage`: Measured voltage at completion
- `FinalCurrent`: Measured current at completion

## Validation

The profile is validated before execution:
- Voltage values must be 0-30V
- Current values must be 0-5A
- CC mode requires Current and CutOffVoltage
- CV mode requires Voltage and at least one of: CutOffCurrent, Time, or "Indefinite"
- WAIT mode requires Time

## Safety Features

- **MaxTime**: Each step can have a maximum time limit as a safety timeout
- **Cancellation**: All steps respond to CancellationToken for emergency stop
- **Automatic output off**: Output is turned off when the profile completes or is cancelled
- **Correction voltage**: Applied to CC cut-off voltages to compensate for circuit voltage drop

## Notes

- The device must be connected before calling `ExecuteChargingProfileAsync`
- Telemetry is polled every 250ms during charging
- Steps execute sequentially in the order defined
- Each step turns the output ON/OFF as appropriate for the mode
- The method is fully asynchronous and does not block the calling thread
