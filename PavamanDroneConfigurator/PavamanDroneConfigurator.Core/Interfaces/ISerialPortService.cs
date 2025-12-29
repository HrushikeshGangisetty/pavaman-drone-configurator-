using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using Asv.IO;

namespace PavamanDroneConfigurator.Core.Interfaces
{
    /// <summary>
    /// Interface for serial port communication service.
    /// Provides methods for discovering, connecting, and disconnecting from serial ports.
    /// </summary>
    public interface ISerialPortService : IConnectionService
    {
        /// <summary>
        /// Gets or sets the data bits for serial communication.
        /// </summary>
        int DataBits { get; set; }

        /// <summary>
        /// Gets or sets the parity for serial communication.
        /// </summary>
        Parity Parity { get; set; }

        /// <summary>
        /// Gets or sets the stop bits for serial communication.
        /// </summary>
        StopBits StopBits { get; set; }

        /// <summary>
        /// Gets the underlying IPort for MAVLink integration.
        /// </summary>
        IPort? Port { get; }

        /// <summary>
        /// Gets the list of available COM ports on the system.
        /// </summary>
        /// <returns>A list of available COM port names.</returns>
        List<string> GetAvailablePorts();

        /// <summary>
        /// Asynchronously connects to the specified serial port with the given configuration.
        /// </summary>
        /// <param name="portName">The name of the COM port (e.g., "COM3").</param>
        /// <param name="baudRate">The baud rate for communication.</param>
        /// <param name="dataBits">The data bits (default: 8).</param>
        /// <param name="parity">The parity (default: None).</param>
        /// <param name="stopBits">The stop bits (default: One).</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when already connected.</exception>
        /// <exception cref="ArgumentException">Thrown when port name is invalid.</exception>
        /// <exception cref="TimeoutException">Thrown when connection times out.</exception>
        Task ConnectAsync(string portName, int baudRate, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One);
    }
}
