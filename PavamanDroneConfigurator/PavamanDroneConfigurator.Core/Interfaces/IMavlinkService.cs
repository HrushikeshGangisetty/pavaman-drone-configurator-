using System;
using Asv.Mavlink;

namespace PavamanDroneConfigurator.Core.Interfaces
{
    /// <summary>
    /// Interface for MAVLink communication service.
    /// Provides methods for MAVLink protocol handling and heartbeat monitoring.
    /// </summary>
    public interface IMavlinkService : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether a heartbeat has been received from the drone.
        /// </summary>
        bool IsHeartbeatReceived { get; }

        /// <summary>
        /// Gets the system ID of the connected drone.
        /// </summary>
        byte SystemId { get; }

        /// <summary>
        /// Gets the component ID of the connected drone.
        /// </summary>
        byte ComponentId { get; }

        /// <summary>
        /// Gets the vehicle type of the connected drone.
        /// </summary>
        string VehicleType { get; }

        /// <summary>
        /// Occurs when the heartbeat state changes.
        /// </summary>
        event EventHandler<bool>? HeartbeatStateChanged;

        /// <summary>
        /// Occurs when heartbeat is lost (no heartbeat received for timeout period).
        /// </summary>
        event EventHandler? HeartbeatLost;

        /// <summary>
        /// Initializes the MAVLink client with the specified port.
        /// </summary>
        /// <param name="port">The communication port.</param>
        void Initialize(Asv.IO.IPort port);

        /// <summary>
        /// Stops the MAVLink client and cleans up resources.
        /// </summary>
        void Stop();
    }
}
