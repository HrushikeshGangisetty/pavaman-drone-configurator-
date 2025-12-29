using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PavamanDroneConfigurator.Core.Interfaces;
using PavamanDroneConfigurator.Core.Enums;

namespace PavamanDroneConfigurator.Core.ViewModels
{
    /// <summary>
    /// ViewModel for managing connections to ArduPilot flight controllers (Serial and TCP).
    /// </summary>
    public partial class ConnectionViewModel : ObservableObject
    {
        private readonly ISerialPortService _serialPortService;
        private readonly ITcpConnectionService _tcpConnectionService;
        private readonly ILogger<ConnectionViewModel> _logger;
        private IConnectionService? _activeConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionViewModel"/> class.
        /// </summary>
        /// <param name="serialPortService">The serial port service.</param>
        /// <param name="tcpConnectionService">The TCP connection service.</param>
        /// <param name="logger">Logger for diagnostics.</param>
        public ConnectionViewModel(
            ISerialPortService serialPortService,
            ITcpConnectionService tcpConnectionService,
            ILogger<ConnectionViewModel> logger)
        {
            _serialPortService = serialPortService ?? throw new ArgumentNullException(nameof(serialPortService));
            _tcpConnectionService = tcpConnectionService ?? throw new ArgumentNullException(nameof(tcpConnectionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Subscribe to connection state changes
            _serialPortService.ConnectionStateChanged += OnConnectionStateChanged;
            _tcpConnectionService.ConnectionStateChanged += OnConnectionStateChanged;

            // Initialize connection types
            AvailableConnectionTypes = new List<ConnectionType> { ConnectionType.Serial, ConnectionType.Tcp };
            SelectedConnectionType = ConnectionType.Serial;

            // Initialize available baud rates
            AvailableBaudRates = new List<int> { 9600, 19200, 38400, 57600, 115200 };
            SelectedBaudRate = 57600; // Default for ArduPilot

            // Initialize TCP settings
            AvailableTcpModes = new List<TcpMode> { TcpMode.Client, TcpMode.Server };
            SelectedTcpMode = TcpMode.Client;
            TcpHost = "127.0.0.1"; // Default for SITL
            TcpPort = 5760; // Default MAVLink port

            // Initialize and load available ports
            AvailablePorts = new ObservableCollection<string>();
            RefreshPorts();
        }

        /// <summary>
        /// Gets the list of available connection types.
        /// </summary>
        [ObservableProperty]
        private List<ConnectionType> _availableConnectionTypes;

        /// <summary>
        /// Gets or sets the currently selected connection type.
        /// </summary>
        [ObservableProperty]
        private ConnectionType _selectedConnectionType;

        /// <summary>
        /// Gets the collection of available COM ports.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> _availablePorts;

        /// <summary>
        /// Gets or sets the currently selected COM port.
        /// </summary>
        [ObservableProperty]
        private string? _selectedPort;

        /// <summary>
        /// Gets the list of available baud rates.
        /// </summary>
        [ObservableProperty]
        private List<int> _availableBaudRates;

        /// <summary>
        /// Gets or sets the currently selected baud rate.
        /// </summary>
        [ObservableProperty]
        private int _selectedBaudRate;

        /// <summary>
        /// Gets the list of available TCP modes.
        /// </summary>
        [ObservableProperty]
        private List<TcpMode> _availableTcpModes;

        /// <summary>
        /// Gets or sets the currently selected TCP mode.
        /// </summary>
        [ObservableProperty]
        private TcpMode _selectedTcpMode;

        /// <summary>
        /// Gets or sets the TCP host address.
        /// </summary>
        [ObservableProperty]
        private string _tcpHost = string.Empty;

        /// <summary>
        /// Gets or sets the TCP port number.
        /// </summary>
        [ObservableProperty]
        private int _tcpPort;

        /// <summary>
        /// Gets or sets a value indicating whether a connection is active.
        /// </summary>
        [ObservableProperty]
        private bool _isConnected;

        /// <summary>
        /// Gets or sets a value indicating whether a connection attempt is in progress.
        /// </summary>
        [ObservableProperty]
        private bool _isConnecting;

        /// <summary>
        /// Gets or sets the status message to display to the user.
        /// </summary>
        [ObservableProperty]
        private string _statusMessage = "Ready to connect";

        /// <summary>
        /// Command to refresh the list of available COM ports.
        /// </summary>
        [RelayCommand]
        private void RefreshPorts()
        {
            try
            {
                _logger.LogInformation("Refreshing available ports");
                
                var ports = _serialPortService.GetAvailablePorts();
                
                AvailablePorts.Clear();
                foreach (var port in ports)
                {
                    AvailablePorts.Add(port);
                }

                if (AvailablePorts.Count > 0)
                {
                    StatusMessage = $"Found {AvailablePorts.Count} port(s)";
                    if (SelectedPort == null)
                    {
                        SelectedPort = AvailablePorts.First();
                    }
                }
                else
                {
                    StatusMessage = "No COM ports found";
                }

                _logger.LogInformation("Found {Count} ports", AvailablePorts.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing ports");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Command to initiate a connection to the selected port or TCP endpoint.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanConnect))]
        private async Task ConnectAsync()
        {
            IsConnecting = true;

            try
            {
                if (SelectedConnectionType == ConnectionType.Serial)
                {
                    if (string.IsNullOrWhiteSpace(SelectedPort))
                    {
                        StatusMessage = "Please select a COM port";
                        return;
                    }

                    StatusMessage = $"Connecting to {SelectedPort}...";
                    await _serialPortService.ConnectAsync(SelectedPort, SelectedBaudRate);
                    _activeConnection = _serialPortService;
                    StatusMessage = $"Connected to {SelectedPort} at {SelectedBaudRate} baud";
                    _logger.LogInformation("Connected to {Port} at {BaudRate}", SelectedPort, SelectedBaudRate);
                }
                else // TCP
                {
                    if (string.IsNullOrWhiteSpace(TcpHost))
                    {
                        StatusMessage = "Please enter a host address";
                        return;
                    }

                    if (TcpPort <= 0 || TcpPort > 65535)
                    {
                        StatusMessage = "Please enter a valid port number (1-65535)";
                        return;
                    }

                    StatusMessage = $"Connecting to {TcpHost}:{TcpPort} ({SelectedTcpMode})...";
                    await _tcpConnectionService.ConnectAsync(TcpHost, TcpPort, SelectedTcpMode);
                    _activeConnection = _tcpConnectionService;
                    StatusMessage = $"Connected to {TcpHost}:{TcpPort} ({SelectedTcpMode} mode)";
                    _logger.LogInformation("Connected to {Host}:{Port} in {Mode} mode", TcpHost, TcpPort, SelectedTcpMode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection failed");
                StatusMessage = $"Connection failed: {ex.Message}";
                _activeConnection = null;
            }
            finally
            {
                IsConnecting = false;
            }
        }

        /// <summary>
        /// Determines whether the Connect command can execute.
        /// </summary>
        /// <returns>True if connection is possible, false otherwise.</returns>
        private bool CanConnect()
        {
            if (IsConnected || IsConnecting)
                return false;

            if (SelectedConnectionType == ConnectionType.Serial)
            {
                return !string.IsNullOrWhiteSpace(SelectedPort);
            }
            else // TCP
            {
                return !string.IsNullOrWhiteSpace(TcpHost) && TcpPort > 0 && TcpPort <= 65535;
            }
        }

        /// <summary>
        /// Command to disconnect from the current port.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanDisconnect))]
        private async Task DisconnectAsync()
        {
            StatusMessage = "Disconnecting...";

            try
            {
                if (_activeConnection != null)
                {
                    await _activeConnection.DisconnectAsync();
                    _activeConnection = null;
                }
                StatusMessage = "Disconnected";
                _logger.LogInformation("Disconnected from connection");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnect");
                StatusMessage = $"Disconnect error: {ex.Message}";
            }
        }

        /// <summary>
        /// Determines whether the Disconnect command can execute.
        /// </summary>
        /// <returns>True if disconnection is possible, false otherwise.</returns>
        private bool CanDisconnect()
        {
            return IsConnected && !IsConnecting;
        }

        /// <summary>
        /// Handles connection state changes from the serial port service.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="isConnected">New connection state.</param>
        private void OnConnectionStateChanged(object? sender, bool isConnected)
        {
            IsConnected = isConnected;
            ConnectCommand.NotifyCanExecuteChanged();
            DisconnectCommand.NotifyCanExecuteChanged();
        }
    }
}
