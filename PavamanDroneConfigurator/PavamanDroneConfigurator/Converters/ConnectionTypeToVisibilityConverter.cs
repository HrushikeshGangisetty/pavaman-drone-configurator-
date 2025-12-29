using System;
using System.Globalization;
using Avalonia.Data.Converters;
using PavamanDroneConfigurator.Core.Enums;

namespace PavamanDroneConfigurator.Converters
{
    /// <summary>
    /// Converter that converts ConnectionType enum to visibility boolean.
    /// </summary>
    public class ConnectionTypeToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets the singleton instance for Serial visibility.
        /// </summary>
        public static readonly ConnectionTypeToVisibilityConverter SerialInstance = new(ConnectionType.Serial);

        /// <summary>
        /// Gets the singleton instance for TCP visibility.
        /// </summary>
        public static readonly ConnectionTypeToVisibilityConverter TcpInstance = new(ConnectionType.Tcp);

        private readonly ConnectionType _targetType;

        private ConnectionTypeToVisibilityConverter(ConnectionType targetType)
        {
            _targetType = targetType;
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ConnectionType connectionType)
            {
                return connectionType == _targetType;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
