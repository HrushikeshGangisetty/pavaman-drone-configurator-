using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
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
        private readonly IMavlinkService _mavlinkService;
        private readonly ILogger<ConnectionViewModel> _logger;
        private IConnectionService? _activeConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionViewModel"/> class.
        /// </summary>
        /// <param name="serialPortService">The serial port service.</param>
        /// <param name="tcpConnectionService">The TCP connection service.</param>
        /// <param name="mavlinkService">The MAVLink service.</param>
        /// <param name="logger">Logger for diagnostics.</param>
        public ConnectionViewModel(
            ISerialPortService serialPortService,
            ITcpConnectionService tcpConnectionService,
            IMavlinkService mavlinkService,
            ILogger<ConnectionViewModel> logger)
        {
            _serialPortService = serialPortService ?? throw new ArgumentNullException(nameof(serialPortService));
            _tcpConnectionService = tcpConnectionService ?? throw new ArgumentNullException(nameof(tcpConnectionService));
            _mavlinkService = mavlinkService ?? throw new ArgumentNullException(nameof(mavlinkService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Subscribe to heartbeat state changes (which will set IsConnected)
            _mavlinkService.HeartbeatStateChanged += OnHeartbeatStateChanged;
            _mavlinkService.HeartbeatLost += OnHeartbeatLost;

            // Initialize connection types
            AvailableConnectionTypes = new List<ConnectionType> { ConnectionType.Serial, ConnectionType.Tcp };
            SelectedConnectionType = ConnectionType.Serial;

            // Initialize available baud rates
            AvailableBaudRates = new List<int> { 9600, 19200, 38400, 57600, 115200 };
            SelectedBaudRate = 57600; // Default for ArduPilot

            // Initialize serial configuration options
            AvailableDataBits = new List<int> { 6, 7, 8 };
            SelectedDataBits = 8; // Default

            AvailableParityOptions = new List<Parity> { Parity.None, Parity.Even, Parity.Odd };
            SelectedParity = Parity.None; // Default

            AvailableStopBits = new List<StopBits> { StopBits.One, StopBits.OnePointFive, StopBits.Two };
            SelectedStopBits = StopBits.One; // Default

            // Initialize TCP settings
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

        partial void OnSelectedConnectionTypeChanged(ConnectionType value)
        {
            ConnectCommand.NotifyCanExecuteChanged();
        }

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

        partial void OnSelectedPortChanged(string? value)
        {
            ConnectCommand.NotifyCanExecuteChanged();
        }

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
        /// Gets the list of available data bits options.
        /// </summary>
        [ObservableProperty]
        private List<int> _availableDataBits;

        /// <summary>
        /// Gets or sets the currently selected data bits.
        /// </summary>
        [ObservableProperty]
        private int _selectedDataBits;

        /// <summary>
        /// Gets the list of available parity options.
        /// </summary>
        [ObservableProperty]
        private List<Parity> _availableParityOptions;

        /// <summary>
        /// Gets or sets the currently selected parity.
        /// </summary>
        [ObservableProperty]
        private Parity _selectedParity;

        /// <summary>
        /// Gets the list of available stop bits options.
        /// </summary>
        [ObservableProperty]
        private List<StopBits> _availableStopBits;

        /// <summary>
        /// Gets or sets the currently selected stop bits.
        /// </summary>
        [ObservableProperty]
        private StopBits _selectedStopBits;

        /// <summary>
        /// Gets or sets the TCP host address.
        /// </summary>
        [ObservableProperty]
        private string _tcpHost = string.Empty;

        partial void OnTcpHostChanged(string value)
        {
            ConnectCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Gets or sets the TCP port number.
        /// </summary>
        [ObservableProperty]
        private int _tcpPort;

        partial void OnTcpPortChanged(int value)
        {
            ConnectCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Gets or sets a value indicating whether a connection is active.
        /// </summary>
        [ObservableProperty]
        private bool _isConnected;

        partial void OnIsConnectedChanged(bool value)
        {
            ConnectCommand.NotifyCanExecuteChanged();
            DisconnectCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Gets or sets a value indicating whether a connection attempt is in progress.
        /// </summary>
        [ObservableProperty]
        private bool _isConnecting;

        partial void OnIsConnectingChanged(bool value)
        {
            ConnectCommand.NotifyCanExecuteChanged();
            DisconnectCommand.NotifyCanExecuteChanged();
        }

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
                    _logger.LogInformation("Starting serial connection to {Port}", SelectedPort);
                    
                    await _serialPortService.ConnectAsync(SelectedPort, SelectedBaudRate, SelectedDataBits, SelectedParity, SelectedStopBits);
                    _activeConnection = _serialPortService;
                    
                    // Initialize MAVLink with the serial port
                    if (_serialPortService.Port != null)
                    {
                        _logger.LogInformation("Initializing MAVLink on serial port");
                        _mavlinkService.Initialize(_serialPortService.Port);
                        StatusMessage = $"Port opened. Waiting for heartbeat...";
                    }
                    else
                    {
                        _logger.LogWarning("Serial port is null after connection");
                        StatusMessage = "Failed to open serial port";
                        _activeConnection = null;
                    }
                    
                    _logger.LogInformation("Serial port opened: {Port} at {BaudRate}", SelectedPort, SelectedBaudRate);
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

                    StatusMessage = $"Connecting to {TcpHost}:{TcpPort}...";
                    _logger.LogInformation("Starting TCP connection to {Host}:{Port}", TcpHost, TcpPort);
                    
                    await _tcpConnectionService.ConnectAsync(TcpHost, TcpPort);
                    _activeConnection = _tcpConnectionService;
                    
                    _logger.LogInformation("TCP connection established. IsConnected: {IsConnected}", _tcpConnectionService.IsConnected);
                    
                    // Initialize MAVLink with the TCP port
                    if (_tcpConnectionService.TcpPort != null)
                    {
                        _logger.LogInformation("Initializing MAVLink on TCP port");
                        _mavlinkService.Initialize(_tcpConnectionService.TcpPort);
                        StatusMessage = $"TCP connected. Waiting for heartbeat...";
                    }
                    else
                    {
                        _logger.LogWarning("TCP port is null after connection");
                        StatusMessage = $"TCP connection failed - port unavailable";
                        _activeConnection = null;
                    }
                    
                    _logger.LogInformation("TCP port opened: {Host}:{Port}", TcpHost, TcpPort);
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError(ex, "Connection failed - SocketException: {Message}", ex.Message);
                StatusMessage = $"Network error: {ex.Message}. Is SITL running on {TcpHost}:{TcpPort}?";
                _activeConnection = null;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Connection failed - TimeoutException: {Message}", ex.Message);
                StatusMessage = $"Connection timeout: No response from {TcpHost}:{TcpPort}";
                _activeConnection = null;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Connection failed - InvalidOperationException: {Message}", ex.Message);
                StatusMessage = $"Connection failed: {ex.Message}";
                _activeConnection = null;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Connection failed - ArgumentException: {Message}", ex.Message);
                StatusMessage = $"Invalid parameters: {ex.Message}";
                _activeConnection = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection failed - Unexpected exception: {ExceptionType} - {Message}", ex.GetType().Name, ex.Message);
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
                // Stop MAVLink service first
                _mavlinkService.Stop();
                
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
        /// Handles heartbeat state changes from the MAVLink service.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="isHeartbeatReceived">New heartbeat state.</param>
        private void OnHeartbeatStateChanged(object? sender, bool isHeartbeatReceived)
        {
            if (isHeartbeatReceived)
            {
                // Only now do we consider the connection fully established
                IsConnected = true;
                StatusMessage = $"Connected - Heartbeat received from System {_mavlinkService.SystemId} (Type: {_mavlinkService.VehicleType})";
                _logger.LogInformation("Heartbeat received from System {SystemId} - Connection fully established", _mavlinkService.SystemId);
            }
            else
            {
                if (!IsConnected)
                {
                    StatusMessage = "Waiting for heartbeat...";
                }
            }
        }

        /// <summary>
        /// Handles heartbeat loss from the MAVLink service.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void OnHeartbeatLost(object? sender, EventArgs e)
        {
            _logger.LogWarning("Heartbeat lost - Connection lost");
            IsConnected = false;
            StatusMessage = "Disconnected - Heartbeat lost";
        }
    }
}
