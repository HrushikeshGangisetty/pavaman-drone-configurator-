using CommunityToolkit.Mvvm.ComponentModel;
using PavamanDroneConfigurator.Core.ViewModels;
using PavamanDroneConfigurator.Core.Interfaces;

namespace PavamanDroneConfigurator.ViewModels
{
    /// <summary>
    /// ViewModel for the main application window.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IMavlinkService _mavlinkService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <param name="connectionViewModel">The connection view model.</param>
        /// <param name="mavlinkService">The MAVLink service.</param>
        public MainWindowViewModel(ConnectionViewModel connectionViewModel, IMavlinkService mavlinkService)
        {
            ConnectionViewModel = connectionViewModel;
            _mavlinkService = mavlinkService;
            
            // Subscribe to heartbeat state changes
            _mavlinkService.HeartbeatStateChanged += OnHeartbeatStateChanged;
            
            UpdateConnectionStatus();
        }

        /// <summary>
        /// Gets the connection view model.
        /// </summary>
        public ConnectionViewModel ConnectionViewModel { get; }

        /// <summary>
        /// Gets or sets the connection status color.
        /// </summary>
        [ObservableProperty]
        private string _connectionStatusColor = "#808080"; // Gray by default

        /// <summary>
        /// Gets or sets the connection status text.
        /// </summary>
        [ObservableProperty]
        private string _connectionStatusText = "DISCONNECTED";

        /// <summary>
        /// Handles heartbeat state changes and updates the connection status.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="isHeartbeatReceived">Whether heartbeat is received.</param>
        private void OnHeartbeatStateChanged(object? sender, bool isHeartbeatReceived)
        {
            UpdateConnectionStatus();
        }

        /// <summary>
        /// Updates the connection status color and text based on the heartbeat state.
        /// </summary>
        private void UpdateConnectionStatus()
        {
            if (_mavlinkService.IsHeartbeatReceived)
            {
                // Green when connected and heartbeat received
                ConnectionStatusColor = "#4CAF50";
                ConnectionStatusText = $"CONNECTED - System {_mavlinkService.SystemId} ({_mavlinkService.VehicleType})";
            }
            else
            {
                // Gray/Red when disconnected or no heartbeat
                ConnectionStatusColor = "#808080";
                ConnectionStatusText = "DISCONNECTED";
            }
        }
    }
}
