using System;
using Asv.IO;
using Microsoft.Extensions.Logging;
using PavamanDroneConfigurator.Core.Interfaces;

namespace PavamanDroneConfigurator.Infrastructure.MAVLink
{
    /// <summary>
    /// Implementation of MAVLink communication service using Asv.Mavlink.
    /// </summary>
    public class MavlinkService : IMavlinkService
    {
        private readonly ILogger<MavlinkService> _logger;
        private IPort? _port;
        private bool _disposed;
        private bool _isHeartbeatReceived;
        private byte _systemId;
        private byte _componentId;
        private string _vehicleType = "Unknown";

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

                // For now, simulate heartbeat detection
                // In a full implementation, this would use Asv.Mavlink to parse MAVLink messages
                // and detect heartbeat packets from the port data stream
                
                // Simulate receiving a heartbeat after initialization
                System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
                {
                    if (_port != null && _port.IsEnabled.CurrentValue)
                    {
                        SystemId = 1;
                        ComponentId = 1;
                        VehicleType = "ArduCopter";
                        IsHeartbeatReceived = true;
                        _logger.LogInformation("MAVLink heartbeat detected (simulated)");
                    }
                });

                _logger.LogInformation("MAVLink service initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize MAVLink service");
                throw;
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            try
            {
                _logger.LogInformation("Stopping MAVLink service");

                // Reset state
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
