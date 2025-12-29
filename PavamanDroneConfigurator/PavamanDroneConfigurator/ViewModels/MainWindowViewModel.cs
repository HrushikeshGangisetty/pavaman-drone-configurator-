using PavamanDroneConfigurator.Core.ViewModels;

namespace PavamanDroneConfigurator.ViewModels
{
    /// <summary>
    /// ViewModel for the main application window.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <param name="connectionViewModel">The connection view model.</param>
        public MainWindowViewModel(ConnectionViewModel connectionViewModel)
        {
            ConnectionViewModel = connectionViewModel;
        }

        /// <summary>
        /// Gets the connection view model.
        /// </summary>
        public ConnectionViewModel ConnectionViewModel { get; }

        public string Greeting { get; } = "Welcome to Pavaman Drone Configurator";
    }
}
