using System;
using System.Threading;
using Asv.IO;
using Asv.Mavlink.Minimal;
using Microsoft.Extensions.Logging;
using PavamanDroneConfigurator.Core.Interfaces;
using R3;

namespace PavamanDroneConfigurator.Infrastructure.MAVLink
{
    /// <summary>
    /// Implementation of MAVLink communication service using Asv.Mavlink.
    /// Parses MAVLink V2 protocol packets from the data stream.
    /// </summary>
    public class MavlinkService : IMavlinkService
    {
        private readonly ILogger<MavlinkService> _logger;
        private IPort? _port;
        private IDisposable? _portSubscription;
        private bool _disposed;
        private bool _isHeartbeatReceived;
        private byte _systemId;
        private byte _componentId;
        private string _vehicleType = "Unknown";
        private Timer? _heartbeatTimer;
        private DateTime _lastHeartbeatTime;
        private readonly TimeSpan _heartbeatTimeout = TimeSpan.FromSeconds(5); // 5 seconds timeout

        // MAVLink V2 protocol constants
        private const int MAVLINK_V2_MAX_PACKET_SIZE = 280; // Max MAVLink V2 packet size
        private const int MAVLINK_V2_HEADER_SIZE = 10; // MAVLink V2 header size (STX + 9 bytes)
        private const byte MAVLINK_V2_STX = 0xFD; // MAVLink V2 start marker
        private const int MAVLINK_V2_CRC_SIZE = 2; // CRC size
        private const int MAVLINK_V2_SIGNATURE_SIZE = 13; // Signature size

        // MAVLink V2 parsing state
        private byte[] _buffer = new byte[MAVLINK_V2_MAX_PACKET_SIZE];
        private int _bufferIndex = 0;
        private enum ParserState { WaitingForStart, GotStart, InPacket }
        private ParserState _state = ParserState.WaitingForStart;
        private int _expectedLength = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MavlinkService"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics.</param>
        public MavlinkService(ILogger<MavlinkService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool IsHeartbeatReceived
        {
            get => _isHeartbeatReceived;
            private set
            {
                if (_isHeartbeatReceived != value)
                {
                    _isHeartbeatReceived = value;
                    HeartbeatStateChanged?.Invoke(this, value);
                }
            }
        }

        /// <inheritdoc/>
        public byte SystemId
        {
            get => _systemId;
            private set => _systemId = value;
        }

        /// <inheritdoc/>
        public byte ComponentId
        {
            get => _componentId;
            private set => _componentId = value;
        }

        /// <inheritdoc/>
        public string VehicleType
        {
            get => _vehicleType;
            private set => _vehicleType = value;
        }

        /// <inheritdoc/>
        public event EventHandler<bool>? HeartbeatStateChanged;

        /// <inheritdoc/>
        public event EventHandler? HeartbeatLost;

        /// <inheritdoc/>
        public void Initialize(IPort port)
        {
            if (port == null)
            {
                throw new ArgumentNullException(nameof(port));
            }

            try
            {
                _logger.LogInformation("Initializing MAVLink service");

                // Stop any existing connection
                Stop();

                // Store the port reference
                _port = port;

                // Subscribe to port data and parse MAVLink packets  
                _portSubscription = _port.OnReceive.Subscribe(dataArray =>
                {
                    try
                    {
                        // Process each byte in the received data
                        foreach (var data in dataArray)
                        {
                            ProcessByte(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing MAVLink data");
                    }
                });

                // Start heartbeat monitoring timer (check every second)
                _heartbeatTimer = new Timer(CheckHeartbeatTimeout, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                _lastHeartbeatTime = DateTime.UtcNow;

                _logger.LogInformation("MAVLink service initialized successfully, listening for heartbeat");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize MAVLink service");
                throw;
            }
        }

        /// <summary>
        /// Processes a single byte from the data stream.
        /// </summary>
        /// <param name="data">The byte to process.</param>
        private void ProcessByte(byte data)
        {
            try
            {
                switch (_state)
                {
                    case ParserState.WaitingForStart:
                        if (data == MAVLINK_V2_STX)
                        {
                            _buffer[0] = data;
                            _bufferIndex = 1;
                            _state = ParserState.GotStart;
                        }
                        break;

                    case ParserState.GotStart:
                        if (_bufferIndex < _buffer.Length)
                        {
                            _buffer[_bufferIndex++] = data;
                        }
                        if (_bufferIndex == MAVLINK_V2_HEADER_SIZE)
                        {
                            // Calculate expected packet length
                            byte payloadLength = _buffer[1];
                            _expectedLength = MAVLINK_V2_HEADER_SIZE + payloadLength + MAVLINK_V2_CRC_SIZE;
                            
                            // Add signature length if present
                            byte incompatFlags = _buffer[2];
                            if ((incompatFlags & 0x01) != 0) // Signature flag
                            {
                                _expectedLength += MAVLINK_V2_SIGNATURE_SIZE;
                            }

                            _state = ParserState.InPacket;
                        }
                        break;

                    case ParserState.InPacket:
                        if (_bufferIndex < _buffer.Length)
                        {
                            _buffer[_bufferIndex++] = data;
                        }
                        if (_bufferIndex >= _expectedLength)
                        {
                            // Full packet received, process it
                            ProcessPacket();
                            
                            // Reset parser
                            _state = ParserState.WaitingForStart;
                            _bufferIndex = 0;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MAVLink byte");
                _state = ParserState.WaitingForStart;
                _bufferIndex = 0;
            }
        }

        /// <summary>
        /// Processes a complete MAVLink packet.
        /// </summary>
        private void ProcessPacket()
        {
            try
            {
                // Extract packet fields
                byte payloadLength = _buffer[1];
                byte systemId = _buffer[5];
                byte componentId = _buffer[6];
                uint messageId = (uint)(_buffer[7] | (_buffer[8] << 8) | (_buffer[9] << 16));

                // Check if this is a HEARTBEAT message (ID = 0)
                if (messageId == 0 && payloadLength >= 9)
                {
                    // Parse heartbeat payload
                    // Payload starts at index 10
                    byte mavType = _buffer[14]; // Type is at offset 4 in payload

                    // Update vehicle information
                    SystemId = systemId;
                    ComponentId = componentId;
                    VehicleType = GetVehicleTypeName((MavType)mavType);

                    // Update last heartbeat time
                    _lastHeartbeatTime = DateTime.UtcNow;

                    // Update heartbeat state if this is the first heartbeat
                    if (!IsHeartbeatReceived)
                    {
                        IsHeartbeatReceived = true;
                        _logger.LogInformation(
                            "MAVLink heartbeat received from System {SystemId}, Component {ComponentId}, Type: {VehicleType}",
                            SystemId, ComponentId, VehicleType);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MAVLink packet");
            }
        }

        /// <summary>
        /// Checks if heartbeat has timed out.
        /// </summary>
        private void CheckHeartbeatTimeout(object? state)
        {
            try
            {
                if (IsHeartbeatReceived)
                {
                    var timeSinceLastHeartbeat = DateTime.UtcNow - _lastHeartbeatTime;
                    if (timeSinceLastHeartbeat > _heartbeatTimeout)
                    {
                        _logger.LogWarning("Heartbeat timeout - no heartbeat received for {Timeout} seconds", _heartbeatTimeout.TotalSeconds);
                        IsHeartbeatReceived = false;
                        HeartbeatLost?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking heartbeat timeout");
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            try
            {
                _logger.LogInformation("Stopping MAVLink service");

                // Stop heartbeat timer
                _heartbeatTimer?.Dispose();
                _heartbeatTimer = null;

                // Unsubscribe from port data
                _portSubscription?.Dispose();
                _portSubscription = null;

                // Reset parser state
                _state = ParserState.WaitingForStart;
                _bufferIndex = 0;

                // Reset connection state
                IsHeartbeatReceived = false;
                SystemId = 0;
                ComponentId = 0;
                VehicleType = "Unknown";
                _port = null;

                _logger.LogInformation("MAVLink service stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping MAVLink service");
            }
        }

        /// <summary>
        /// Converts MAVLink vehicle type to human-readable string.
        /// </summary>
        /// <param name="vehicleType">MAVLink vehicle type.</param>
        /// <returns>Human-readable vehicle type name.</returns>
        private static string GetVehicleTypeName(MavType vehicleType)
        {
            return vehicleType switch
            {
                MavType.MavTypeQuadrotor => "QuadCopter",
                MavType.MavTypeHelicopter => "Helicopter",
                MavType.MavTypeHexarotor => "HexaCopter",
                MavType.MavTypeOctorotor => "OctoCopter",
                MavType.MavTypeTricopter => "TriCopter",
                MavType.MavTypeFixedWing => "Fixed Wing",
                MavType.MavTypeGroundRover => "Rover",
                MavType.MavTypeSubmarine => "Submarine",
                MavType.MavTypeSurfaceBoat => "Boat",
                MavType.MavTypeAirship => "Airship",
                MavType.MavTypeVtolTiltrotor => "VTOL Tiltrotor",
                MavType.MavTypeVtolTailsitter => "VTOL Tailsitter",
                MavType.MavTypeVtolTiltwing => "VTOL Tiltwing",
                MavType.MavTypeAntennaTracker => "Antenna Tracker",
                MavType.MavTypeGcs => "Ground Control Station",
                _ => $"Unknown ({vehicleType})"
            };
        }

        /// <summary>
        /// Disposes the service and cleans up resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Stop();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
