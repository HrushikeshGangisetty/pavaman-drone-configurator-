using System;
using System.Threading.Tasks;
using Asv.IO;
using Microsoft.Extensions.Logging;
using PavamanDroneConfigurator.Core.Enums;
using PavamanDroneConfigurator.Core.Interfaces;

namespace PavamanDroneConfigurator.Infrastructure.Tcp
{
    /// <summary>
    /// Implementation of TCP connection service using Asv.IO with proper TCP client configuration.
    /// </summary>
    public class TcpConnectionService : ITcpConnectionService, IDisposable
    {
        private readonly ILogger<TcpConnectionService> _logger;
        private IPort? _port;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpConnectionService"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics.</param>
        public TcpConnectionService(ILogger<TcpConnectionService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool IsConnected => _port?.IsEnabled.CurrentValue ?? false;

        /// <inheritdoc/>
        public string? Host { get; set; }

        /// <inheritdoc/>
        public int Port { get; set; }

        /// <inheritdoc/>
        public IPort? TcpPort => _port;

        /// <inheritdoc/>
        public event EventHandler<bool>? ConnectionStateChanged;

        /// <inheritdoc/>
        public async Task ConnectAsync(string host, int port)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("Host cannot be null or empty", nameof(host));
            }

            if (port <= 0 || port > 65535)
            {
                throw new ArgumentException("Port must be between 1 and 65535", nameof(port));
            }

            if (IsConnected)
            {
                throw new InvalidOperationException("Already connected to a TCP endpoint");
            }

            try
            {
                Host = host;
                Port = port;

                _logger.LogInformation("Attempting to connect to {Host}:{Port} in TCP Client mode", host, port);

                // Build the connection string for TCP client
                // Format: tcp://host:port
                string connectionString = $"tcp://{host}:{port}";

                _logger.LogDebug("Connection string: {ConnectionString}", connectionString);

                // Create the TCP port using PortFactory
                _port = PortFactory.Create(connectionString);

                if (_port == null)
                {
                    throw new InvalidOperationException("Failed to create TCP port");
                }

                // Enable the port (starts the connection)
                await Task.Run(() => _port.Enable());

                // Wait a moment to ensure connection is established
                await Task.Delay(100);

                if (!_port.IsEnabled.CurrentValue)
                {
                    throw new InvalidOperationException("Failed to enable TCP port");
                }

                _logger.LogInformation("Successfully connected to {Host}:{Port}", host, port);
                OnConnectionStateChanged(true);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid connection parameters: {Host}:{Port}", host, port);
                CleanupConnection();
                throw new ArgumentException($"Invalid connection parameters: {ex.Message}", ex);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Socket error connecting to {Host}:{Port}", host, port);
                CleanupConnection();
                throw new InvalidOperationException($"Network error: {ex.Message}. Is SITL running on {host}:{port}?", ex);
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Timeout connecting to {Host}:{Port}", host, port);
                CleanupConnection();
                throw new TimeoutException($"Connection timeout: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to {Host}:{Port} - {ExceptionType}", host, port, ex.GetType().Name);
                CleanupConnection();
                throw new InvalidOperationException($"Failed to connect to {host}:{port}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task ConnectAsync()
        {
            if (string.IsNullOrWhiteSpace(Host))
            {
                throw new InvalidOperationException("Host is not set");
            }

            if (Port <= 0)
            {
                throw new InvalidOperationException("Port is not set");
            }

            await ConnectAsync(Host, Port);
        }

        /// <inheritdoc/>
        public async Task DisconnectAsync()
        {
            if (!IsConnected)
            {
                _logger.LogWarning("Disconnect called but not connected");
                return;
            }

            try
            {
                _logger.LogInformation("Disconnecting from TCP endpoint");

                await Task.Run(() =>
                {
                    _port?.Disable();
                });

                CleanupConnection();
                _logger.LogInformation("Successfully disconnected from TCP endpoint");
                OnConnectionStateChanged(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from TCP endpoint");
                CleanupConnection();
                throw;
            }
        }

        /// <summary>
        /// Raises the ConnectionStateChanged event.
        /// </summary>
        /// <param name="isConnected">The new connection state.</param>
        protected virtual void OnConnectionStateChanged(bool isConnected)
        {
            ConnectionStateChanged?.Invoke(this, isConnected);
        }

        /// <summary>
        /// Cleans up the connection resources.
        /// </summary>
        private void CleanupConnection()
        {
            if (_port != null)
            {
                try
                {
                    if (_port.IsEnabled.CurrentValue)
                    {
                        _port.Disable();
                    }
                    _port.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing port during cleanup");
                }
                _port = null;
            }
        }

        /// <summary>
        /// Disposes the service and cleans up resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                CleanupConnection();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
