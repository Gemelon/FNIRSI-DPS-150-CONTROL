# Acknowledgments

## Protocol Documentation

This project would not have been possible without the excellent work of **cho45** who reverse-engineered and documented the FNIRSI-DPS-150 communication protocol.

### Original Project
- **Repository**: [cho45/fnirsi-dps-150](https://github.com/cho45/fnirsi-dps-150)
- **Author**: [cho45](https://github.com/cho45)
- **Protocol Documentation**: [FNIRSI_DPS-150_Protocol.md](https://github.com/cho45/fnirsi-dps-150/blob/main/docs/FNIRSI_DPS-150_Protocol.md)

### What We Used

The following components of this project are based on cho45's documentation:

1. **Protocol Specification**
   - Serial communication parameters (115200 baud, 8N1)
   - Packet structure and format
   - Command set and response formats
   - Status message structure (sent every 100-200ms)

2. **Device Parameters**
   - Protection modes (OVP, OCP, OPP, OTP, LVP, REP)
   - Voltage, current, and power ranges
   - Device status flags
   - Memory preset structure

3. **Implementation Details**
   - Byte ordering and data encoding
   - Checksum calculations
   - Communication timing requirements

## Credits

**Special Thanks to:**
- **cho45** - For the comprehensive reverse-engineering work and detailed protocol documentation that made this .NET implementation possible.

## Related Projects

If you're interested in FNIRSI-DPS-150 control, check out cho45's original project which includes:
- Complete protocol documentation
- TypeScript/JavaScript implementation
- Web-based control interface
- Additional technical details and insights

## References

- [FNIRSI-DPS-150 Protocol Documentation](https://github.com/cho45/fnirsi-dps-150/blob/main/docs/FNIRSI_DPS-150_Protocol.md)
- [cho45's FNIRSI-DPS-150 Repository](https://github.com/cho45/fnirsi-dps-150)

---

We are grateful for the open-source community and the sharing of knowledge that makes projects like this possible.
