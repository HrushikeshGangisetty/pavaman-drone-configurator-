# TCP & USB Connection Guide

This guide explains how to use the TCP and USB (Serial) connection features in the Pavaman Drone Configurator.

## Overview

The configurator supports two types of connections to ArduPilot-based drones:

1. **Serial/USB Connection**: Direct physical connection via USB cable
2. **TCP Connection**: Network-based connection for SITL simulators or remote drones

## Connection Architecture

The application follows the MSSV (Model-Service-View-ViewModel) architecture:

```
┌─────────────────┐
│   View (XAML)   │  ← User Interface
└────────┬────────┘
         │
┌────────▼────────┐
│   ViewModel     │  ← Presentation Logic
└────────┬────────┘
         │
┌────────▼────────┐
│   Service       │  ← Business Logic
└────────┬────────┘
         │
┌────────▼────────┐
│   Asv.IO Port   │  ← Transport Layer
└─────────────────┘
```

## Serial/USB Connection

### When to Use
- Connecting to a physical drone via USB cable
- Initial setup and configuration
- Direct telemetry access with lowest latency

### How to Connect

1. **Physical Connection**:
   - Connect your flight controller to the computer via USB cable
   - Wait for the driver to install (if first-time connection)

2. **In the Application**:
   - Select "Serial" from the Connection Type dropdown
   - Click "Refresh" to scan for available COM ports
   - Select your flight controller's COM port (e.g., COM3, COM4)
   - Choose the baud rate (usually 57600 or 115200 for ArduPilot)
   - Click "Connect"

### Typical Settings
- **Baud Rate**: 57600 or 115200
- **Default Port**: Varies by system (COM3, COM4 on Windows; /dev/ttyUSB0, /dev/ttyACM0 on Linux)

## TCP Connection

### When to Use
- Testing with SITL (Software In The Loop) simulator
- Connecting to a drone over WiFi or Ethernet
- Remote telemetry access via network
- Connecting to companion computers (Raspberry Pi, etc.)

### Connection Modes

#### Client Mode (Most Common)
Used to **connect to** a TCP server (e.g., SITL simulator):

```
Configurator (Client) ──connects to──> SITL (Server)
```

**Example**: Connecting to ArduPilot SITL running on localhost:
- Host: `127.0.0.1`
- Port: `5760`
- Mode: `Client`

#### Server Mode
Used to **wait for** incoming connections from telemetry modules:

```
Telemetry Module (Client) ──connects to──> Configurator (Server)
```

**Example**: Waiting for a WiFi telemetry module to connect:
- Host: `0.0.0.0` (listen on all interfaces)
- Port: `14550`
- Mode: `Server`

### How to Connect

1. **In the Application**:
   - Select "TCP" from the Connection Type dropdown
   - Choose TCP Mode (Client or Server)
   - Enter the Host address:
     - For SITL: `127.0.0.1` or `localhost`
     - For remote drone: IP address of the drone (e.g., `192.168.1.100`)
     - For server mode: `0.0.0.0` (all interfaces) or specific interface IP
   - Enter the Port number (default: `5760`)
   - Click "Connect"

### Common TCP Ports

| Port  | Purpose                                    |
|-------|--------------------------------------------|
| 5760  | Default MAVLink TCP port for SITL         |
| 14550 | MAVLink telemetry (Ground Control Station)|
| 14551 | Additional MAVLink port                   |

### TCP Connection Examples

#### Example 1: SITL on Local Machine
```
Connection Type: TCP
TCP Mode: Client
Host: 127.0.0.1
Port: 5760
```

#### Example 2: Remote Drone via WiFi
```
Connection Type: TCP
TCP Mode: Client
Host: 192.168.1.50
Port: 5760
```

#### Example 3: Waiting for Telemetry Module
```
Connection Type: TCP
TCP Mode: Server
Host: 0.0.0.0
Port: 14550
```

## Setting Up ArduPilot SITL for TCP Testing

To test the TCP connection feature with ArduPilot SITL:

### 1. Install ArduPilot SITL

```bash
# Ubuntu/Debian
sudo apt-get install git
git clone https://github.com/ArduPilot/ardupilot.git
cd ardupilot
git submodule update --init --recursive
```

### 2. Run SITL with TCP

```bash
cd ardupilot/ArduCopter
sim_vehicle.py --console --map --out=tcpin:0.0.0.0:5760
```

This starts SITL and opens a TCP server on port 5760.

### 3. Connect from Configurator

- Connection Type: TCP
- TCP Mode: Client
- Host: 127.0.0.1
- Port: 5760
- Click "Connect"

## Troubleshooting

### Serial Connection Issues

**Problem**: COM port not appearing
- **Solution**: 
  - Check USB cable connection
  - Verify driver installation (Windows Device Manager)
  - Click "Refresh" in the application

**Problem**: "Access denied" error
- **Solution**: 
  - Close other applications using the port (Mission Planner, QGroundControl)
  - On Linux, add user to dialout group: `sudo usermod -a -G dialout $USER`

### TCP Connection Issues

**Problem**: Connection timeout (Client mode)
- **Solution**: 
  - Verify SITL or server is running
  - Check firewall settings
  - Verify correct IP address and port

**Problem**: Connection refused (Client mode)
- **Solution**: 
  - Ensure the target server is listening on the specified port
  - Check if port is not already in use

**Problem**: No incoming connections (Server mode)
- **Solution**: 
  - Check firewall allows incoming connections on the port
  - Verify client is connecting to correct IP and port
  - Use `0.0.0.0` as host to listen on all network interfaces

## Code Examples

### Using Serial Connection in Code

```csharp
// Inject the serial port service
var serialService = serviceProvider.GetRequiredService<ISerialPortService>();

// Connect
await serialService.ConnectAsync("COM3", 57600);

// Check connection status
if (serialService.IsConnected)
{
    Console.WriteLine("Connected!");
}

// Disconnect
await serialService.DisconnectAsync();
```

### Using TCP Connection in Code

```csharp
// Inject the TCP connection service
var tcpService = serviceProvider.GetRequiredService<ITcpConnectionService>();

// Connect in client mode
await tcpService.ConnectAsync("127.0.0.1", 5760, TcpMode.Client);

// Check connection status
if (tcpService.IsConnected)
{
    Console.WriteLine("Connected!");
}

// Disconnect
await tcpService.DisconnectAsync();
```

### Using Generic Connection Service

```csharp
// Works with both Serial and TCP
IConnectionService connection = GetCurrentConnection(); // Returns either ISerialPortService or ITcpConnectionService

// Both implement the same interface
if (connection.IsConnected)
{
    await connection.DisconnectAsync();
}
```

## Architecture Benefits

The unified `IConnectionService` interface provides:

1. **Abstraction**: Switch between Serial and TCP without changing business logic
2. **Testability**: Easy to mock for unit testing
3. **Extensibility**: Can add UDP or other connection types in the future
4. **MSSV Compliance**: Clean separation of concerns

## Advanced Topics

### Connection String Format (Internal)

The TCP service uses Asv.IO connection strings internally:

```
tcp:host:port?mode
```

Where:
- `host`: IP address or hostname
- `port`: TCP port number (1-65535)
- `mode`: `clnt` (client) or `srv` (server)

Examples:
- `tcp:127.0.0.1:5760?clnt` - Connect to localhost:5760 as client
- `tcp:0.0.0.0:14550?srv` - Listen on all interfaces:14550 as server

### MAVLink Integration

While the current implementation establishes TCP connectivity, future versions will integrate MAVLink protocol handling for:
- Heartbeat monitoring
- Parameter management
- Telemetry data processing
- Command execution

## References

- [Asv.Mavlink Documentation](https://github.com/asv-soft/asv-mavlink)
- [ArduPilot SITL Documentation](https://ardupilot.org/dev/docs/sitl-simulator-software-in-the-loop.html)
- [MAVLink Protocol](https://mavlink.io/en/)
