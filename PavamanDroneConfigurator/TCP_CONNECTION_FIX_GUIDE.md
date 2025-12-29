# TCP Connection Fix - 127.0.0.1:5760 Issue

## Problem
TCP connections to 127.0.0.1:5760 (SITL) were not working properly.

## Root Cause
The TCP connection was using an incorrect connection string format. The code was using:
```csharp
string connectionString = $"tcp:{host}:{port}?clnt";
```

But Asv.IO expects:
```csharp
string connectionString = $"tcp://{host}:{port}";
```

## Changes Made

### 1. TcpConnectionService.cs
- **Fixed connection string format**: Changed from `tcp:host:port?clnt` to `tcp://host:port`
- **Added better exception handling**: Now catches and handles:
  - `SocketException` - Network/connection errors
  - `TimeoutException` - Connection timeouts
  - `ArgumentException` - Invalid parameters
  - `InvalidOperationException` - State errors
- **Improved logging**: Added detailed logging at each step
- **Better error messages**: User-friendly error messages that help diagnose issues

### 2. ConnectionViewModel.cs
- **Enhanced exception handling**: Specific catch blocks for different error types
- **Better status messages**: More informative messages for users
  - "Network error: {message}. Is SITL running on {host}:{port}?"
  - "Connection timeout: No response from {host}:{port}"
- **Additional logging**: Track connection progress and errors

### 3. Property Change Notifications
Added automatic command updates when properties change:
- `OnSelectedConnectionTypeChanged` ? Updates Connect button
- `OnSelectedPortChanged` ? Updates Connect button  
- `OnTcpHostChanged` ? Updates Connect button
- `OnTcpPortChanged` ? Updates Connect button
- `OnIsConnectedChanged` ? Updates both buttons
- `OnIsConnectingChanged` ? Updates both buttons

## How to Test

### Prerequisites
You need ArduPilot SITL running on 127.0.0.1:5760

#### Option 1: Using sim_vehicle.py (Linux/WSL/Mac)
```bash
cd ardupilot/ArduCopter
sim_vehicle.py --console --map --out=tcpin:0.0.0.0:5760
```

#### Option 2: Using Mission Planner SITL (Windows)
1. Open Mission Planner
2. Go to Simulation ? Multirotor
3. Start the simulator
4. Note the TCP port (usually 5760)

#### Option 3: Manual TCP Server Test (Any Platform)
Create a simple TCP echo server for testing:
```python
# test_tcp_server.py
import socket

server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind(('127.0.0.1', 5760))
server.listen(1)
print("TCP Server listening on 127.0.0.1:5760")

while True:
    client, addr = server.accept()
    print(f"Connection from {addr}")
    data = client.recv(1024)
    if data:
        print(f"Received: {data}")
        client.send(data)
```

Run: `python test_tcp_server.py`

### Testing Steps

1. **Stop the running application** (if running)
2. **Rebuild the solution** in Visual Studio
3. **Start the application**
4. **Make sure SITL or test server is running**
5. **Verify SITL is listening**:
   ```bash
   # Windows PowerShell
   netstat -ano | findstr :5760
   
   # Linux/Mac
   netstat -an | grep 5760
   ```
   You should see a line showing something is listening on port 5760

6. **In the application**:
   - Select "TCP" connection type
   - Host: `127.0.0.1`
   - Port: `5760`
   - Click "Connect"

### Expected Results

#### Success Case
```
Status: Connecting to 127.0.0.1:5760...
Status: Connected to 127.0.0.1:5760. Waiting for heartbeat...
Status: Connected - Heartbeat received from System 1 (Type: ArduCopter)
```

#### Failure Cases with Better Error Messages

**SITL Not Running:**
```
Status: Network error: Connection refused. Is SITL running on 127.0.0.1:5760?
```

**Wrong Port:**
```
Status: Network error: No connection could be made. Is SITL running on 127.0.0.1:5761?
```

**Network Issue:**
```
Status: Connection timeout: No response from 127.0.0.1:5760
```

## Verification Commands

### Check if port 5760 is open (Windows)
```powershell
Test-NetConnection -ComputerName 127.0.0.1 -Port 5760
```

### Check if port 5760 is open (Linux/Mac)
```bash
nc -zv 127.0.0.1 5760
# or
telnet 127.0.0.1 5760
```

### Start a quick SITL test (Linux/WSL)
```bash
# Install ArduPilot if not already installed
git clone https://github.com/ArduPilot/ardupilot.git
cd ardupilot
git submodule update --init --recursive

# Run SITL
cd ArduCopter
../Tools/autotest/sim_vehicle.py --console --map --out=tcpin:0.0.0.0:5760
```

## Debugging Tips

### Enable Verbose Logging
Check Visual Studio Output window ? Debug pane for detailed logs:
```
[INFO] Starting TCP connection to 127.0.0.1:5760
[DEBUG] Connection string: tcp://127.0.0.1:5760
[INFO] Successfully connected to 127.0.0.1:5760
[INFO] Initializing MAVLink on TCP port
```

### Common Issues

**Issue**: "Network error: Connection refused"
- **Cause**: SITL is not running
- **Fix**: Start ArduPilot SITL or test server

**Issue**: "Connection timeout"
- **Cause**: Firewall blocking connection or wrong IP
- **Fix**: Check firewall, verify IP address

**Issue**: "TCP port is null after connection"
- **Cause**: Port created but Enable() failed
- **Fix**: Check Asv.IO port creation logs

**Issue**: Button disabled/not clickable
- **Cause**: Missing fields or already connected
- **Fix**: Ensure Host and Port are filled, not already connected

## Technical Details

### Connection String Format
Asv.IO PortFactory supports these formats:
- Serial: `serial:COM3:115200:8:N:1`
- TCP Client: `tcp://host:port`
- TCP Server: `tcp://:port?srv=true`
- UDP: `udp://host:port`

### Why tcp:// format works
The `tcp://` format is the standard URI scheme that Asv.IO v3.3.0 expects. The old format `tcp:host:port?clnt` was from an older version or different library.

## Next Steps

1. **Test with actual SITL** - Verify connection with real ArduPilot SITL
2. **Test heartbeat detection** - Ensure MAVLink messages are parsed correctly
3. **Test parameter reading** - Verify full MAVLink communication works
4. **Test disconnection** - Ensure clean disconnect and reconnect works

## Files Modified
- `PavamanDroneConfigurator.Infrastructure\Tcp\TcpConnectionService.cs`
- `PavamanDroneConfigurator.Core\ViewModels\ConnectionViewModel.cs`

## Validation Checklist
- [ ] Code compiles without errors
- [ ] SITL is running on 127.0.0.1:5760
- [ ] Application starts successfully
- [ ] TCP connection type is selectable
- [ ] Connect button is enabled when host/port are filled
- [ ] Connection succeeds to SITL
- [ ] Status messages are clear and helpful
- [ ] Error messages help diagnose issues
- [ ] Disconnect works properly
- [ ] Can reconnect after disconnect
