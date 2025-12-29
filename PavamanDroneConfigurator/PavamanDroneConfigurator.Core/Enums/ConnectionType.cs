namespace PavamanDroneConfigurator.Core.Enums
{
    /// <summary>
    /// Represents the type of connection to the drone.
    /// </summary>
    public enum ConnectionType
    {
        /// <summary>
        /// Serial/USB connection through a COM port.
        /// </summary>
        Serial = 0,

        /// <summary>
        /// TCP/IP network connection.
        /// </summary>
        Tcp = 1
    }
}
