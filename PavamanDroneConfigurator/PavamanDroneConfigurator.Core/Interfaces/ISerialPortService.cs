using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PavamanDroneConfigurator.Core.Interfaces
{
    /// <summary>
    /// Interface for serial port communication service.
    /// Provides methods for discovering, connecting, and disconnecting from serial ports.
    /// </summary>
    public interface ISerialPortService : IConnectionService
    {
        /// <summary>
        /// Gets the list of available COM ports on the system.
        /// </summary>
        /// <returns>A list of available COM port names.</returns>
        List<string> GetAvailablePorts();

        /// <summary>
        /// Asynchronously connects to the specified serial port with the given baud rate.
        /// </summary>
        /// <param name="portName">The name of the COM port (e.g., "COM3").</param>
        /// <param name="baudRate">The baud rate for communication.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when already connected.</exception>
        /// <exception cref="ArgumentException">Thrown when port name is invalid.</exception>
        /// <exception cref="TimeoutException">Thrown when connection times out.</exception>
        Task ConnectAsync(string portName, int baudRate);
    }
}
