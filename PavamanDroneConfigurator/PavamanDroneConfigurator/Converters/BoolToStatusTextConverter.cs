using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PavamanDroneConfigurator.Converters
{
    /// <summary>
    /// Converts a boolean value to a connection status text.
    /// </summary>
    public class BoolToStatusTextConverter : IValueConverter
    {
        /// <summary>
        /// Gets the singleton instance of the converter.
        /// </summary>
        public static readonly BoolToStatusTextConverter Instance = new();

        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isConnected)
            {
                return isConnected ? "Connected" : "Disconnected";
            }
            return "Unknown";
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("BoolToStatusTextConverter only supports one-way conversion.");
        }
    }
}
