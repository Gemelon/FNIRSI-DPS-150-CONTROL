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

using Xunit;
using Xunit.Abstractions;

namespace FNIRSI_DPS_150_CONTROL.Tests
{
    /// <summary>
    /// xUnit tests to verify the endianness of float values in the DPS-150 protocol.
    /// According to the protocol documentation:
    /// Voltage Setpoint (C1): F1 B1 C1 04 CD CC 44 41 E3
    /// Where CD CC 44 41 are the float bytes representing 12.30V.
    /// </summary>
    public class FloatEndiannessTests
    {
        private readonly ITestOutputHelper _output;
        private readonly byte[] _protocolBytes = new byte[] { 0xCD, 0xCC, 0x44, 0x41 };
        private const float ExpectedVoltage = 12.30f;
        private const float Tolerance = 0.01f;

        public FloatEndiannessTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void LittleEndian_Interpretation_ShouldMatch_ProtocolValue()
        {
            // Arrange
            _output.WriteLine("Testing Little-Endian interpretation of protocol bytes");
            _output.WriteLine($"Protocol bytes: {BitConverter.ToString(_protocolBytes)}");

            // Act
            float value = BitConverter.ToSingle(_protocolBytes, 0);
            _output.WriteLine($"Interpreted value: {value:F2}V");
            _output.WriteLine($"Expected value: {ExpectedVoltage:F2}V");

            // Assert
            Assert.InRange(value, ExpectedVoltage - Tolerance, ExpectedVoltage + Tolerance);
        }

        [Fact]
        public void BigEndian_Interpretation_ShouldNotMatch_ProtocolValue()
        {
            // Arrange
            byte[] reversedBytes = new byte[4];
            Array.Copy(_protocolBytes, reversedBytes, 4);
            Array.Reverse(reversedBytes);
            _output.WriteLine("Testing Big-Endian interpretation (reversed bytes)");
            _output.WriteLine($"Reversed bytes: {BitConverter.ToString(reversedBytes)}");

            // Act
            float value = BitConverter.ToSingle(reversedBytes, 0);
            _output.WriteLine($"Interpreted value: {value}");

            // Assert - Big-endian should NOT match the expected voltage
            Assert.False(Math.Abs(value - ExpectedVoltage) < Tolerance,
                "Big-endian interpretation should not match expected voltage");
        }

        [Fact]
        public void BitConverter_GetBytes_ShouldProduce_ProtocolBytes()
        {
            // Arrange
            float testValue = 12.3f;
            _output.WriteLine($"Converting {testValue:F2}V to bytes using BitConverter.GetBytes()");

            // Act
            byte[] resultBytes = BitConverter.GetBytes(testValue);
            _output.WriteLine($"Result bytes: {BitConverter.ToString(resultBytes)}");
            _output.WriteLine($"Expected bytes: {BitConverter.ToString(_protocolBytes)}");

            // Assert
            Assert.Equal(_protocolBytes, resultBytes);
        }

        [Fact]
        public void RoundTrip_LittleEndian_ShouldPreserve_OriginalValue()
        {
            // Arrange
            _output.WriteLine("Testing round-trip: bytes → float → bytes");

            // Act
            float value = BitConverter.ToSingle(_protocolBytes, 0);
            byte[] roundTripBytes = BitConverter.GetBytes(value);

            _output.WriteLine($"Original bytes: {BitConverter.ToString(_protocolBytes)}");
            _output.WriteLine($"Float value: {value:F2}V");
            _output.WriteLine($"Round-trip bytes: {BitConverter.ToString(roundTripBytes)}");

            // Assert
            Assert.Equal(_protocolBytes, roundTripBytes);
        }

        [Theory]
        [InlineData(12.3f, new byte[] { 0xCD, 0xCC, 0x44, 0x41 })]
        [InlineData(5.0f, new byte[] { 0x00, 0x00, 0xA0, 0x40 })]
        [InlineData(0.0f, new byte[] { 0x00, 0x00, 0x00, 0x00 })]
        public void FloatToBytes_WithVariousValues_ShouldMatch_ExpectedBytes(float voltage, byte[] expectedBytes)
        {
            // Arrange
            _output.WriteLine($"Testing voltage: {voltage:F2}V");

            // Act
            byte[] resultBytes = BitConverter.GetBytes(voltage);
            _output.WriteLine($"Result bytes: {BitConverter.ToString(resultBytes)}");
            _output.WriteLine($"Expected bytes: {BitConverter.ToString(expectedBytes)}");

            // Assert
            Assert.Equal(expectedBytes, resultBytes);
        }

        [Fact]
        public void DPS150Protocol_Uses_LittleEndian_ByteOrder()
        {
            // This test documents the conclusion that DPS-150 uses little-endian
            _output.WriteLine("═══════════════════════════════════════════════════════════════");
            _output.WriteLine("VERIFIED: The DPS-150 protocol uses LITTLE-ENDIAN byte order!");
            _output.WriteLine("FloatToBytes should use: BitConverter.GetBytes(value)");
            _output.WriteLine("(no byte reversal needed on little-endian systems)");
            _output.WriteLine("═══════════════════════════════════════════════════════════════");

            // Verify with the known protocol example
            float voltage = BitConverter.ToSingle(_protocolBytes, 0);
            Assert.InRange(voltage, ExpectedVoltage - Tolerance, ExpectedVoltage + Tolerance);

            byte[] bytes = BitConverter.GetBytes(12.3f);
            Assert.Equal(_protocolBytes, bytes);
        }
    }
}
