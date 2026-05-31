// MIT License
//
// Copyright (c) 2026 FNIRSI-DPS-150-CONTROL Project
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

//
// PROTOCOL REFERENCE:
// ===================
// This implementation is based on the FNIRSI-DPS-150 protocol documentation by cho45.
// Protocol specification:
// https://github.com/cho45/fnirsi-dps-150/blob/main/docs/FNIRSI_DPS-150_Protocol.md
//
// Original project repository:
// https://github.com/cho45/fnirsi-dps-150
//
// The protocol documentation includes detailed information about:
// - Communication packet structure
// - Command set and responses
// - Device status messages
// - Protection modes and parameters
//
// Many thanks to cho45 for the excellent reverse-engineering work!
//

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FNIRSI_DPS_150_CONTROL
{
    /// <summary>
    /// Represents the output relay state of the DPS-150 power supply.
    /// </summary>
    public enum OutputRelayState : byte
    {
        /// <summary>
        /// Output relay is OFF (output disabled).
        /// </summary>
        OFF = 0,

        /// <summary>
        /// Output relay is ON (output enabled).
        /// </summary>
        ON = 1
    }

    // 0 = OK, 1 = OVP, 2 = OCP, 3 = OPP, 4 = OTP, 5 = LVP, 6 = REP
    enum DPS150ProtectionMode : byte
    {
        OK = 0,
        OVP = 1,
        OCP = 2,
        OPP = 3,
        OTP = 4,
        LVP = 5,
        REP = 6
    }

    enum DPS150ComDirection: byte
    {
        RX = 0xF0,
        TX = 0xF1,
    }

    enum DPS150AccessType : byte
    {
        Read = 0xA1,
        Baud = 0xB0,
        Write = 0xB1,
        Control = 0xC1,
    }

    enum DPS150RegisterAddress : byte
    {
        // Active Output Control
        VoltageSetpoint = 0xC1,
        CurrentLimit = 0xC2,

        // Output relay state
        OutputRelayState = 0xDB,

        // Preset Memory (M1–M6)
        PresetM1Voltage = 0xC5,
        PresetM1Current = 0xC6,
        PresetM2Voltage = 0xC7,
        PresetM2Current = 0xC8,
        PresetM3Voltage = 0xC9,
        PresetM3Current = 0xCA,
        PresetM4Voltage = 0xCB,
        PresetM4Current = 0xCC,
        PresetM5Voltage = 0xCD,
        PresetM5Current = 0xCE,
        PresetM6Voltage = 0xCF,
        PresetM6Current = 0xD0,

        // Protection Settings
        OVP = 0xD1,
        OCP = 0xD2,
        OPP = 0xD3,
        OTP = 0xD4,
        LVP = 0xD5,

        // UI / System Settings
        Brightness = 0xD6,
        Volume = 0xD7,

        // Telemetry (RX only)
        // DPS‑150 periodically transmits telemetry (period - 500ms).
        InputVoltage = 0xC0,
        MaximumVoltage = 0xE2,
        MaximumCurrent = 0xE3,
        InternalTemperature = 0xC4,
        Measurement = 0xC3,

        // Additional telemetry (RX - Energy + Capacity)
        // You can request additional telemetry for energy and capacity:
        EnergyAndCapacity = 0xD8,
        MeasuredCapacity = 0xD9,
        MeasuredEnergy = 0xDA,

        //Telemetry on change
        // Some frames are automatically sent by device on register change (no need to request it)
        RunningMode = 0xDB,         // 0 = STOP, 1 = RUN
        ProtectionMode = 0xDC,      // 0 = OK, 1 = OVP, 2 = OCP, 3 = OPP, 4 = OTP, 5 = LVP, 6 = REP
        CCCV = 0xDD                 // 0 = CC, 1 = CV
    }

    public class DPS150Registers
    {
        // Session Control (mandatory) Group C1
        // Before accessing any registers, communication must be enabled.
        // Enable session	F1 C1 00 01 01 02	Must be sent once
        // Disable session	F1 C1 00 01 00 01	Must be sent once
        bool SessionEnabled { get; set; }

        // Baud rate Group B0
        // Baud rate must be send to device after session activation
        // Send F1 B0 00 01 XX 06 where XX is
        int BaudRate { get; set; } = 0;

        // Group B1 - TX
        // Group A1 - RX

        // Active Output Control

        // Voltage Setpoint (C1)
        // TX: F1 B1 C1 04 CD CC 44 41 E3
        // RX: F0 A1 C1 04 CD CC 44 41 E3
        float VoltageSetpoint { get; set; }

        //Current Limit (C2)
        // TX: F1 B1 C2 04 FD FF FF 3E FF
        float CurrentLimit { get; set; }

        // Output Enable (RUN / STOP)
        // 0	STOP
        // 1	RUN
        //
        // RUN
        // TX: F1 B1 DB 01 01 DD
        // RX: F0 A1 DB 01 01 DD
        // STOP
        // TX: F1 B1 DB 01 00 DC
        // RX: F0 A1 DB 01 00 DC
        uint OutputRelayState { get; set; }

        // Preset Memory (M1–M6)
        // TX: F1 B1 C7 04 00 00 B0 40 BB
        // TX: F1 B1 C8 04 FD FF FF 3E 05
        float PresetM1Voltage { get; set; }
        float PresetM1Current { get; set; }
        float PresetM2Voltage { get; set; }
        float PresetM2Current { get; set; }
        float PresetM3Voltage { get; set; }
        float PresetM3Current { get; set; }
        float PresetM4Voltage { get; set; }
        float PresetM4Current { get; set; }
        float PresetM5Voltage { get; set; }
        float PresetM5Current { get; set; }
        float PresetM6Voltage { get; set; }
        float PresetM6Current { get; set; }

        // Protection Settings
        // TX: F1 B1 D4 04 00 00 80 42 9A
        // All protection writes are followed by:
        // TX: F1 A1 FF 01 00 00
        float OVP { get; set; }
        float OCP { get; set; }
        float OPP { get; set; }
        float OTP { get; set; }
        float LVP { get; set; }

        // UI / System Settings
        //Brightness
        // TX: F1 B1 D6 01 0C E3
        uint Brightness { get; set; }

        // Volume
        // TX: F1 B1 D7 01 09 E1
        uint Volume { get; set; }

        // Telemetry(RX only)
        // Telemetry frames are unsolicited and must not be acknowledged.
        float InputVoltage { get; } = 0.0f;
        float MaximumVoltage { get; } = 0.0f;
        float MaximumCurrent { get; } = 0.0f;
        float InternalTemperature { get; } = 0.0f;
        float[] Measurement { get; } = new float[3];

        // Additional telemetry (RX - Energy + Capacity)
        // ...additionally with main telemetry data there will be 2 additional frames...
        // RX: F0 A1 D9 04 9B D6 34 00 81
        // RX: F0 A1 DA 04 CF AE 28 35 B8
        bool EnergyAndCapacity { get; } = false;
        float MeasuredCapacity { get; } = 0.0f;
        float MeasuredEnergy { get; } = 0.0f;

        // Telemetry on change
        // Some frames are automatically sent by device on register change (no need to request it)
        bool RunningMode { get; } = false;         // 0 = STOP, 1 = RUN
        DPS150ProtectionMode ProtectionMode { get; set; } = DPS150ProtectionMode.OK;
        bool CCCV { get; } = false;                 // 0 = CC, 1 = CV
    }
}