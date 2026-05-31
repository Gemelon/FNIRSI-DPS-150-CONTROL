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

using System;

namespace FNIRSI_DPS_150_CONTROL
{
    /// <summary>
    /// Test class to verify the endianness of float values in the DPS-150 protocol.
    /// According to the protocol documentation:
    /// Voltage Setpoint (C1): F1 B1 C1 04 CD CC 44 41 E3
    /// Where CD CC 44 41 are the float bytes.
    /// </summary>
    public static class FloatEndiannessTest
    {
        public static void RunTest()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║ DPS-150 FLOAT ENDIANNESS TEST                                ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // From protocol documentation:
            // TX: F1 B1 C1 04 CD CC 44 41 E3
            // The float bytes are: CD CC 44 41
            byte[] protocolBytes = new byte[] { 0xCD, 0xCC, 0x44, 0x41 };

            Console.WriteLine("Protocol bytes (from documentation):");
            Console.WriteLine($"  Hex: {BitConverter.ToString(protocolBytes)}");
            Console.WriteLine();

            // Test 1: Little-endian (direct interpretation)
            float littleEndianValue = BitConverter.ToSingle(protocolBytes, 0);
            Console.WriteLine("Test 1: Little-Endian Interpretation");
            Console.WriteLine($"  Value: {littleEndianValue:F2}V");
            Console.WriteLine($"  Expected: 12.30V (common test value)");
            Console.WriteLine($"  Match: {(Math.Abs(littleEndianValue - 12.30f) < 0.01f ? "✓ YES" : "✗ NO")}");
            Console.WriteLine();

            // Test 2: Big-endian (reversed interpretation)
            byte[] reversedBytes = new byte[4];
            Array.Copy(protocolBytes, reversedBytes, 4);
            Array.Reverse(reversedBytes);
            float bigEndianValue = BitConverter.ToSingle(reversedBytes, 0);
            Console.WriteLine("Test 2: Big-Endian Interpretation");
            Console.WriteLine($"  Value: {bigEndianValue:F2}V");
            Console.WriteLine($"  Expected: 12.30V (common test value)");
            Console.WriteLine($"  Match: {(Math.Abs(bigEndianValue - 12.30f) < 0.01f ? "✓ YES" : "✗ NO")}");
            Console.WriteLine();

            // Test 3: Round-trip test with current FloatToBytes implementation
            Console.WriteLine("Test 3: Current FloatToBytes Implementation (Big-Endian)");
            float testValue = 12.3f;
            byte[] currentImplBytes = FloatToBytesBigEndian(testValue);
            Console.WriteLine($"  Input: {testValue:F2}V");
            Console.WriteLine($"  Output bytes: {BitConverter.ToString(currentImplBytes)}");
            Console.WriteLine($"  Expected: {BitConverter.ToString(protocolBytes)}");
            Console.WriteLine($"  Match: {(ByteArraysEqual(currentImplBytes, protocolBytes) ? "✓ YES" : "✗ NO")}");
            Console.WriteLine();

            // Test 4: Round-trip test with little-endian (direct BitConverter)
            Console.WriteLine("Test 4: Corrected Implementation (Little-Endian)");
            byte[] littleEndianBytes = BitConverter.GetBytes(testValue);
            Console.WriteLine($"  Input: {testValue:F2}V");
            Console.WriteLine($"  Output bytes: {BitConverter.ToString(littleEndianBytes)}");
            Console.WriteLine($"  Expected: {BitConverter.ToString(protocolBytes)}");
            Console.WriteLine($"  Match: {(ByteArraysEqual(littleEndianBytes, protocolBytes) ? "✓ YES" : "✗ NO")}");
            Console.WriteLine();

            // Test 5: Round-trip verification
            Console.WriteLine("Test 5: Round-Trip Verification");
            float roundTripValue = BitConverter.ToSingle(protocolBytes, 0);
            byte[] roundTripBytes = BitConverter.GetBytes(roundTripValue);
            Console.WriteLine($"  Protocol bytes → float → bytes");
            Console.WriteLine($"  {BitConverter.ToString(protocolBytes)} → {roundTripValue:F2}V → {BitConverter.ToString(roundTripBytes)}");
            Console.WriteLine($"  Round-trip successful: {(ByteArraysEqual(protocolBytes, roundTripBytes) ? "✓ YES" : "✗ NO")}");
            Console.WriteLine();

            // Conclusion
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
            Console.WriteLine("CONCLUSION:");
            if (ByteArraysEqual(littleEndianBytes, protocolBytes))
            {
                Console.WriteLine("✓ The DPS-150 protocol uses LITTLE-ENDIAN byte order!");
                Console.WriteLine("  FloatToBytes should use: BitConverter.GetBytes(value)");
                Console.WriteLine("  (no byte reversal needed on little-endian systems)");
            }
            else if (ByteArraysEqual(currentImplBytes, protocolBytes))
            {
                Console.WriteLine("✓ The DPS-150 protocol uses BIG-ENDIAN byte order!");
                Console.WriteLine("  Current FloatToBytes implementation is CORRECT.");
            }
            else
            {
                Console.WriteLine("✗ Unable to determine byte order from test data.");
            }
            Console.WriteLine("═══════════════════════════════════════════════════════════════");
        }

        private static byte[] FloatToBytesBigEndian(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }
}
