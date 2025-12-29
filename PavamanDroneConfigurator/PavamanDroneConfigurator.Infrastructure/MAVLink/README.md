# MAVLink

This folder contains MAVLink protocol implementation for drone communication.

## Purpose

Implement MAVLink (Micro Air Vehicle Link) protocol for communicating with ArduPilot-based flight controllers via multiple connection types.

## Connection Types

The configurator supports two connection types:

1. **Serial/USB Connection**: Direct connection via COM port (implemented in `../Serial/`)
2. **TCP Connection**: Network-based connection for SITL or remote drones (implemented in `../Tcp/`)

## Contents

- **Protocol Handlers**: MAVLink message encoding/decoding
- **Message Definitions**: MAVLink message types and structures
- **Connection Management**: MAVLink connection lifecycle
- **Telemetry Processing**: Parse and process telemetry data

## Dependencies

This module uses the **Asv.Mavlink** library (MIT License) for MAVLink protocol support, which includes:
- **Asv.IO**: Low-level port abstraction for Serial, TCP, and UDP
- **MAVLink V2**: Protocol implementation

## Guidelines

- Use Asv.Mavlink library for core protocol operations
- Use Asv.IO for connection management (Serial, TCP, UDP)
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
        private readonly IConnectionService _connection;
        
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
