using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using Asv.IO;
using Microsoft.Extensions.Logging;
using PavamanDroneConfigurator.Core.Interfaces;

namespace PavamanDroneConfigurator.Infrastructure.Serial
{
    /// <summary>
    /// Implementation of serial port communication service using Asv.IO.
    /// </summary>
    public class SerialPortService : ISerialPortService, IDisposable
    {
        private readonly ILogger<SerialPortService> _logger;
        private IPort? _port;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialPortService"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics.</param>
        public SerialPortService(ILogger<SerialPortService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;
        }

        /// <inheritdoc/>
        public bool IsConnected => _port?.IsEnabled.CurrentValue ?? false;

        /// <inheritdoc/>
        public int DataBits { get; set; }

        /// <inheritdoc/>
        public Parity Parity { get; set; }

        /// <inheritdoc/>
        public StopBits StopBits { get; set; }

        /// <inheritdoc/>
        public IPort? Port => _port;

        /// <inheritdoc/>
        public event EventHandler<bool>? ConnectionStateChanged;

        /// <inheritdoc/>
        public List<string> GetAvailablePorts()
        {
            try
            {
                var ports = SerialPort.GetPortNames().ToList();
                _logger.LogInformation("Found {Count} serial ports", ports.Count);
                return ports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available serial ports");
                return new List<string>();
            }
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(string portName, int baudRate, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
        {
            if (string.IsNullOrWhiteSpace(portName))
            {
                throw new ArgumentException("Port name cannot be null or empty", nameof(portName));
            }

            if (IsConnected)
            {
                throw new InvalidOperationException("Already connected to a serial port");
            }

            try
            {
                DataBits = dataBits;
                Parity = parity;
                StopBits = stopBits;

                _logger.LogInformation("Attempting to connect to {Port} at {BaudRate} baud, {DataBits} data bits, {Parity} parity, {StopBits} stop bits", 
                    portName, baudRate, dataBits, parity, stopBits);

                // Build the connection string for Asv.IO
                // Format: serial:port:baudrate:databits:parity:stopbits
                // Parity: N (None), E (Even), O (Odd), M (Mark), S (Space)
                char parityChar = parity switch
                {
                    Parity.None => 'N',
                    Parity.Even => 'E',
                    Parity.Odd => 'O',
                    Parity.Mark => 'M',
                    Parity.Space => 'S',
                    _ => 'N'
                };

                // StopBits: 1, 1.5, 2
                string stopBitsStr = stopBits switch
                {
                    StopBits.One => "1",
                    StopBits.OnePointFive => "1.5",
                    StopBits.Two => "2",
                    _ => "1"
                };

                string connectionString = $"serial:{portName}:{baudRate}:{dataBits}:{parityChar}:{stopBitsStr}";
                
                _logger.LogDebug("Connection string: {ConnectionString}", connectionString);

                // Create the port using Asv.IO PortFactory
                _port = PortFactory.Create(connectionString);

                // Enable the port (starts the connection)
                if (_port != null)
                {
                    await Task.Run(() => _port.Enable());
                }

                _logger.LogInformation("Successfully connected to {Port}", portName);
                OnConnectionStateChanged(true);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied to port {Port}", portName);
                CleanupPort();
                throw new InvalidOperationException($"Access denied to port {portName}. Port may be in use.", ex);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid port name {Port}", portName);
                CleanupPort();
                throw new ArgumentException($"Invalid port name: {portName}", nameof(portName), ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to {Port}", portName);
                CleanupPort();
                throw new InvalidOperationException($"Failed to connect to {portName}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public Task ConnectAsync()
        {
            throw new NotSupportedException("Serial port service requires port name and baud rate. Use ConnectAsync(string portName, int baudRate, ...) instead.");
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
                _logger.LogInformation("Disconnecting from serial port");
                
                await Task.Run(() =>
                {
                    if (_port?.IsEnabled.CurrentValue == true)
                    {
                        _port.Disable();
                    }
                });

                CleanupPort();
                _logger.LogInformation("Successfully disconnected from serial port");
                OnConnectionStateChanged(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from serial port");
                CleanupPort();
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
        /// Cleans up the port instance.
        /// </summary>
        private void CleanupPort()
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
                CleanupPort();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
