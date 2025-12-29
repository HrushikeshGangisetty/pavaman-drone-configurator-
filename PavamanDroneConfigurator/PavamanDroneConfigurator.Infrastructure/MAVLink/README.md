# MAVLink

This folder contains MAVLink protocol implementation for drone communication.

## Purpose

Implement MAVLink (Micro Air Vehicle Link) protocol for communicating with ArduPilot-based flight controllers.

## Contents

- **Protocol Handlers**: MAVLink message encoding/decoding
- **Message Definitions**: MAVLink message types and structures
- **Connection Management**: MAVLink connection lifecycle
- **Telemetry Processing**: Parse and process telemetry data

## Dependencies

This module uses the **Asv.Mavlink** library (MIT License) for MAVLink protocol support.

## Guidelines

- Use Asv.Mavlink library for core protocol operations
- Implement connection retry logic with exponential backoff
- Handle message versioning (MAVLink 1.0 and 2.0)
- Validate message checksums before processing
- Log all communication for debugging

## Example

```csharp
namespace PavamanDroneConfigurator.Infrastructure.MAVLink
{
    public class MavLinkService : IMavLinkService
    {
        private readonly ISerialPortService _serialPort;
        
        public async Task<HeartbeatMessage> WaitForHeartbeat(int timeoutMs)
        {
            // Wait for and parse heartbeat message from flight controller
        }
        
        public async Task SendCommand(MavLinkCommand command)
        {
            // Encode and send MAVLink command
        }
    }
}
```

## Resources

- [MAVLink Official Documentation](https://mavlink.io/en/)
- [ArduPilot MAVLink Guide](https://ardupilot.org/dev/docs/mavlink-basics.html)
- [Asv.Mavlink Library](https://github.com/asv-soft/asv-mavlink)
