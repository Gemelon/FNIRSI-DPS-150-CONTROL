// MIT License
//
// Copyright (c) 2026 Gemelon
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FNIRSI_DPS_150_CONTROL
{
    /// <summary>
    /// Represents a battery charging profile loaded from a YAML file.
    /// </summary>
    public class ChargingProfile
    {
        /// <summary>
        /// Title/name of the charging profile.
        /// </summary>
        [YamlMember(Alias = "Title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Battery capacity in Ah (Ampere-hours).
        /// </summary>
        [YamlMember(Alias = "Capacity")]
        public string CapacityString { get; set; } = "0Ah";

        /// <summary>
        /// Gets the battery capacity as a float value in Ampere-hours.
        /// </summary>
        [YamlIgnore]
        public float Capacity => ParseValue(CapacityString, "Ah");

        /// <summary>
        /// Voltage correction factor to account for voltage drop in the charging circuit.
        /// This value is added to cut-off voltages.
        /// </summary>
        [YamlMember(Alias = "CorrectionVoltage")]
        public string CorrectionVoltageString { get; set; } = "0V";

        /// <summary>
        /// Gets the correction voltage as a float value in Volts.
        /// </summary>
        [YamlIgnore]
        public float CorrectionVoltage => ParseValue(CorrectionVoltageString, "V");

        /// <summary>
        /// List of charging cycle steps.
        /// </summary>
        [YamlMember(Alias = "CycleSteps")]
        public List<CycleStep> CycleSteps { get; set; } = new List<CycleStep>();

        /// <summary>
        /// Loads a charging profile from a YAML file.
        /// </summary>
        /// <param name="filePath">Path to the YAML file.</param>
        /// <returns>The loaded charging profile.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the YAML is invalid.</exception>
        public static ChargingProfile LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Charging profile file not found: {filePath}");

            var yaml = File.ReadAllText(filePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var profile = deserializer.Deserialize<ChargingProfile>(yaml);

            if (profile == null)
                throw new InvalidOperationException("Failed to parse charging profile YAML.");

            return profile;
        }

        /// <summary>
        /// Validates the charging profile for valid ranges and parameters.
        /// </summary>
        /// <returns>True if the profile is valid.</returns>
        /// <exception cref="InvalidOperationException">Thrown if validation fails.</exception>
        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Title))
                throw new InvalidOperationException("Charging profile must have a title.");

            if (CycleSteps == null || CycleSteps.Count == 0)
                throw new InvalidOperationException("Charging profile must contain at least one cycle step.");

            foreach (var step in CycleSteps)
            {
                step.Validate(CorrectionVoltage);
            }

            return true;
        }

        /// <summary>
        /// Parses a value string with a unit suffix (e.g., "5Ah", "3.6V").
        /// </summary>
        private static float ParseValue(string value, string unit)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0f;

            var cleaned = value.Replace(unit, "").Trim();
            if (float.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                return result;

            return 0f;
        }
    }

    /// <summary>
    /// Represents a single step in a charging cycle.
    /// </summary>
    public class CycleStep
    {
        /// <summary>
        /// Name/description of this step.
        /// </summary>
        [YamlMember(Alias = "Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Charging mode for this step.
        /// </summary>
        [YamlMember(Alias = "Mode")]
        public string ModeString { get; set; } = "CC";

        /// <summary>
        /// Gets the charging mode as an enum value.
        /// </summary>
        [YamlIgnore]
        public ChargingMode Mode
        {
            get
            {
                return ModeString?.ToUpperInvariant() switch
                {
                    "CC" => ChargingMode.ConstantCurrent,
                    "CV" => ChargingMode.ConstantVoltage,
                    "WAIT" => ChargingMode.Wait,
                    _ => ChargingMode.ConstantCurrent
                };
            }
        }

        /// <summary>
        /// Current setpoint for CC mode (e.g., "1A").
        /// </summary>
        [YamlMember(Alias = "Current")]
        public string? CurrentString { get; set; }

        /// <summary>
        /// Gets the current as a float value in Amperes.
        /// </summary>
        [YamlIgnore]
        public float? Current => ParseValue(CurrentString, "A");

        /// <summary>
        /// Voltage setpoint for CV mode (e.g., "3.55V").
        /// </summary>
        [YamlMember(Alias = "Voltage")]
        public string? VoltageString { get; set; }

        /// <summary>
        /// Gets the voltage as a float value in Volts.
        /// </summary>
        [YamlIgnore]
        public float? Voltage => ParseValue(VoltageString, "V");

        /// <summary>
        /// Cut-off voltage for CC mode - when measured voltage reaches this, move to next step.
        /// </summary>
        [YamlMember(Alias = "CutOffVoltage")]
        public string? CutOffVoltageString { get; set; }

        /// <summary>
        /// Gets the cut-off voltage as a float value in Volts.
        /// </summary>
        [YamlIgnore]
        public float? CutOffVoltage => ParseValue(CutOffVoltageString, "V");

        /// <summary>
        /// Cut-off current for CV mode - when measured current drops to this, move to next step.
        /// </summary>
        [YamlMember(Alias = "CutOffCurrent")]
        public string? CutOffCurrentString { get; set; }

        /// <summary>
        /// Gets the cut-off current as a float value in Amperes.
        /// </summary>
        [YamlIgnore]
        public float? CutOffCurrent => ParseValue(CutOffCurrentString, "A");

        /// <summary>
        /// Time duration for WAIT mode (e.g., "0:15h" for 15 minutes, "2h" for 2 hours).
        /// </summary>
        [YamlMember(Alias = "Time")]
        public string? TimeString { get; set; }

        /// <summary>
        /// Gets the time duration as a TimeSpan, or null if "Indefinite".
        /// </summary>
        [YamlIgnore]
        public TimeSpan? Time => ParseTime(TimeString);

        /// <summary>
        /// Maximum time for this step as a safety timeout.
        /// </summary>
        [YamlMember(Alias = "MaxTime")]
        public string? MaxTimeString { get; set; }

        /// <summary>
        /// Gets the maximum time as a TimeSpan, or null if not specified.
        /// </summary>
        [YamlIgnore]
        public TimeSpan? MaxTime => ParseTime(MaxTimeString);

        /// <summary>
        /// Validates this cycle step for correct parameters and valid ranges.
        /// </summary>
        /// <param name="correctionVoltage">Voltage correction factor from the profile.</param>
        /// <exception cref="InvalidOperationException">Thrown if validation fails.</exception>
        public void Validate(float correctionVoltage)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Cycle step must have a name.");

            switch (Mode)
            {
                case ChargingMode.ConstantCurrent:
                    if (!Current.HasValue || Current.Value <= 0)
                        throw new InvalidOperationException($"Step '{Name}': CC mode requires a positive Current value.");
                    if (Current.Value > 5.0f)
                        throw new InvalidOperationException($"Step '{Name}': Current {Current.Value}A exceeds device maximum of 5A.");
                    if (!CutOffVoltage.HasValue)
                        throw new InvalidOperationException($"Step '{Name}': CC mode requires a CutOffVoltage value.");

                    var effectiveCutOff = CutOffVoltage.Value + correctionVoltage;
                    if (effectiveCutOff > 30.0f)
                        throw new InvalidOperationException($"Step '{Name}': Effective cut-off voltage {effectiveCutOff}V (including correction) exceeds device maximum of 30V.");
                    break;

                case ChargingMode.ConstantVoltage:
                    if (!Voltage.HasValue || Voltage.Value <= 0)
                        throw new InvalidOperationException($"Step '{Name}': CV mode requires a positive Voltage value.");
                    if (Voltage.Value > 30.0f)
                        throw new InvalidOperationException($"Step '{Name}': Voltage {Voltage.Value}V exceeds device maximum of 30V.");
                    if (!CutOffCurrent.HasValue && !Time.HasValue && TimeString?.ToLowerInvariant() != "indefinite")
                        throw new InvalidOperationException($"Step '{Name}': CV mode requires either CutOffCurrent, Time, or 'Indefinite'.");
                    break;

                case ChargingMode.Wait:
                    if (!Time.HasValue)
                        throw new InvalidOperationException($"Step '{Name}': WAIT mode requires a Time value.");
                    break;
            }
        }

        /// <summary>
        /// Parses a value string with a unit suffix (e.g., "5A", "3.6V").
        /// </summary>
        private static float? ParseValue(string? value, string unit)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var cleaned = value.Replace(unit, "").Trim();
            if (float.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                return result;

            return null;
        }

        /// <summary>
        /// Parses a time string in format "h:mm" or "h" (e.g., "5:15h", "2h", "0:15h").
        /// Returns null for "Indefinite" or invalid formats.
        /// </summary>
        private static TimeSpan? ParseTime(string? timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                return null;

            if (timeString.Equals("Indefinite", StringComparison.OrdinalIgnoreCase))
                return null;

            // Match patterns like "5:15h", "2h", "0:15h"
            var match = Regex.Match(timeString, @"^(\d+):(\d+)h$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                int hours = int.Parse(match.Groups[1].Value);
                int minutes = int.Parse(match.Groups[2].Value);
                return new TimeSpan(hours, minutes, 0);
            }

            // Match simple hours like "2h"
            match = Regex.Match(timeString, @"^(\d+)h$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                int hours = int.Parse(match.Groups[1].Value);
                return TimeSpan.FromHours(hours);
            }

            return null;
        }
    }

    /// <summary>
    /// Charging mode for a cycle step.
    /// </summary>
    public enum ChargingMode
    {
        /// <summary>
        /// Constant Current mode - maintain fixed current, monitor voltage.
        /// </summary>
        ConstantCurrent,

        /// <summary>
        /// Constant Voltage mode - maintain fixed voltage, monitor current.
        /// </summary>
        ConstantVoltage,

        /// <summary>
        /// Wait/Rest mode - output off, wait for specified time.
        /// </summary>
        Wait
    }
}
