using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PavamanDroneConfigurator.Core.Interfaces;

namespace PavamanDroneConfigurator.Core.ViewModels
{
    /// <summary>
    /// ViewModel for managing serial port connections to ArduPilot flight controllers.
    /// </summary>
    public partial class ConnectionViewModel : ObservableObject
    {
        private readonly ISerialPortService _serialPortService;
        private readonly ILogger<ConnectionViewModel> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionViewModel"/> class.
        /// </summary>
        /// <param name="serialPortService">The serial port service.</param>
        /// <param name="logger">Logger for diagnostics.</param>
        public ConnectionViewModel(ISerialPortService serialPortService, ILogger<ConnectionViewModel> logger)
        {
            _serialPortService = serialPortService ?? throw new ArgumentNullException(nameof(serialPortService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Subscribe to connection state changes
            _serialPortService.ConnectionStateChanged += OnConnectionStateChanged;

            // Initialize available baud rates
            AvailableBaudRates = new List<int> { 9600, 19200, 38400, 57600, 115200 };
            SelectedBaudRate = 57600; // Default for ArduPilot

            // Initialize and load available ports
            AvailablePorts = new ObservableCollection<string>();
            RefreshPorts();
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
        /// Command to initiate a connection to the selected port.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanConnect))]
        private async Task ConnectAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedPort))
            {
                StatusMessage = "Please select a COM port";
                return;
            }

            IsConnecting = true;
            StatusMessage = $"Connecting to {SelectedPort}...";

            try
            {
                await _serialPortService.ConnectAsync(SelectedPort, SelectedBaudRate);
                StatusMessage = $"Connected to {SelectedPort} at {SelectedBaudRate} baud";
                _logger.LogInformation("Connected to {Port} at {BaudRate}", SelectedPort, SelectedBaudRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection failed");
                StatusMessage = $"Connection failed: {ex.Message}";
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
            return !IsConnected && !IsConnecting && !string.IsNullOrWhiteSpace(SelectedPort);
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
                await _serialPortService.DisconnectAsync();
                StatusMessage = "Disconnected";
                _logger.LogInformation("Disconnected from serial port");
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
