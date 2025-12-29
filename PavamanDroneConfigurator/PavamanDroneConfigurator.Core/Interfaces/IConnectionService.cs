using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PavamanDroneConfigurator.Core.Interfaces
{
    /// <summary>
    /// Interface for connection communication service.
    /// Provides unified methods for connecting to drones via different transport types (Serial, TCP).
    /// </summary>
    public interface IConnectionService
    {
        /// <summary>
        /// Gets a value indicating whether the connection is currently active.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Occurs when the connection state changes.
        /// </summary>
        event EventHandler<bool>? ConnectionStateChanged;

        /// <summary>
        /// Asynchronously connects using the service-specific configuration.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ConnectAsync();

        /// <summary>
        /// Asynchronously disconnects from the current connection.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DisconnectAsync();
    }
}
