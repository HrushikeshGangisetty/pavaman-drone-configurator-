using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PavamanDroneConfigurator.Core.Interfaces;

namespace PavamanDroneConfigurator.Infrastructure.Serial
{
    /// <summary>
    /// Implementation of serial port communication service using System.IO.Ports.SerialPort.
    /// </summary>
    public class SerialPortService : ISerialPortService, IDisposable
    {
        private readonly ILogger<SerialPortService> _logger;
        private SerialPort? _serialPort;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialPortService"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics.</param>
        public SerialPortService(ILogger<SerialPortService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool IsConnected => _serialPort?.IsOpen ?? false;

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
        public async Task ConnectAsync(string portName, int baudRate)
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
                _logger.LogInformation("Attempting to connect to {Port} at {BaudRate} baud", portName, baudRate);

                _serialPort = new SerialPort(portName, baudRate)
                {
                    ReadTimeout = 1000,
                    WriteTimeout = 1000,
                    DtrEnable = true,
                    RtsEnable = true
                };

                await Task.Run(() => _serialPort.Open());

                _logger.LogInformation("Successfully connected to {Port}", portName);
                OnConnectionStateChanged(true);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied to port {Port}", portName);
                CleanupSerialPort();
                throw new InvalidOperationException($"Access denied to port {portName}. Port may be in use.", ex);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid port name {Port}", portName);
                CleanupSerialPort();
                throw new ArgumentException($"Invalid port name: {portName}", nameof(portName), ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to {Port}", portName);
                CleanupSerialPort();
                throw new InvalidOperationException($"Failed to connect to {portName}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public Task ConnectAsync()
        {
            throw new NotSupportedException("Serial port service requires port name and baud rate. Use ConnectAsync(string portName, int baudRate) instead.");
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
                    if (_serialPort?.IsOpen == true)
                    {
                        _serialPort.Close();
                    }
                });

                CleanupSerialPort();
                _logger.LogInformation("Successfully disconnected from serial port");
                OnConnectionStateChanged(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from serial port");
                CleanupSerialPort();
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
        /// Cleans up the serial port instance.
        /// </summary>
        private void CleanupSerialPort()
        {
            if (_serialPort != null)
            {
                try
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error closing serial port during cleanup");
                }

                _serialPort.Dispose();
                _serialPort = null;
            }
        }

        /// <summary>
        /// Disposes the service and cleans up resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                CleanupSerialPort();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
