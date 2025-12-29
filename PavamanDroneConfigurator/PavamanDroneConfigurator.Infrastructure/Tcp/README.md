# TCP Connection Service

This directory contains the TCP connection implementation for connecting to drones via TCP/IP network.

## Overview

The `TcpConnectionService` provides TCP connection capabilities using the Asv.IO library, which is part of the Asv.Mavlink ecosystem. This enables connections to:

- **SITL Simulators**: Connect to ArduPilot SITL running locally or remotely
- **Network Telemetry Modules**: Connect to WiFi or Ethernet-based telemetry devices
- **Companion Computers**: Connect to Raspberry Pi or other companion computers running MAVLink services

## Usage

### Client Mode (Default)

Used to connect to a TCP server (e.g., SITL simulator):

```csharp
var tcpService = new TcpConnectionService(logger);
await tcpService.ConnectAsync("127.0.0.1", 5760, TcpMode.Client);
```

### Server Mode

Used to wait for incoming connections from telemetry modules:

```csharp
var tcpService = new TcpConnectionService(logger);
await tcpService.ConnectAsync("0.0.0.0", 5760, TcpMode.Server);
```

## Connection String Format

The service internally uses Asv.IO's connection string format:

- **Client**: `tcp:host:port?clnt` (e.g., `tcp:127.0.0.1:5760?clnt`)
- **Server**: `tcp:host:port?srv` (e.g., `tcp:0.0.0.0:5760?srv`)

## Common Ports

- **5760**: Default MAVLink TCP port for SITL
- **14550**: Alternative MAVLink port
- **14551**: Additional MAVLink port for multiple connections

## Examples

### Connecting to ArduPilot SITL

```csharp
var tcpService = serviceProvider.GetRequiredService<ITcpConnectionService>();
await tcpService.ConnectAsync("127.0.0.1", 5760, TcpMode.Client);
```

### Setting up a Server for Telemetry

```csharp
var tcpService = serviceProvider.GetRequiredService<ITcpConnectionService>();
await tcpService.ConnectAsync("0.0.0.0", 14550, TcpMode.Server);
```

## Architecture

The TCP connection service follows the MSSV (Model-Service-View-ViewModel) architecture:

- **Model**: `TcpMode` enum defines connection modes
- **Service**: `TcpConnectionService` implements connection logic
- **ViewModel**: `ConnectionViewModel` manages UI state
- **View**: `ConnectionView` provides user interface

## Dependencies

- **Asv.IO**: Provides the underlying port abstraction and TCP implementation
- **Asv.Mavlink**: MAVLink protocol support (dependency of Asv.IO)

## Error Handling

The service handles common connection errors:

- Invalid host or port
- Connection timeout
- Network unreachable
- Already connected state

All errors are logged and propagated as descriptive exceptions.
