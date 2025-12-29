using System;
using System.Threading.Tasks;
using Asv.IO;
using PavamanDroneConfigurator.Core.Enums;

namespace PavamanDroneConfigurator.Core.Interfaces
{
    /// <summary>
    /// Interface for TCP connection communication service.
    /// Provides methods for connecting to drones via TCP/IP network.
    /// </summary>
    public interface ITcpConnectionService : IConnectionService
    {
        /// <summary>
        /// Gets or sets the TCP host address (IP address or hostname).
        /// </summary>
        string? Host { get; set; }

        /// <summary>
        /// Gets or sets the TCP port number.
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// Gets the underlying IPort for MAVLink integration.
        /// </summary>
        IPort? TcpPort { get; }

        /// <summary>
        /// Asynchronously connects to the specified TCP endpoint in client mode.
        /// </summary>
        /// <param name="host">The IP address or hostname to connect to.</param>
        /// <param name="port">The TCP port number.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when already connected.</exception>
        /// <exception cref="ArgumentException">Thrown when host or port is invalid.</exception>
        /// <exception cref="TimeoutException">Thrown when connection times out.</exception>
        Task ConnectAsync(string host, int port);
    }
}
