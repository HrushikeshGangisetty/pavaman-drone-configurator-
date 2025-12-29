using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PavamanDroneConfigurator.Converters
{
    /// <summary>
    /// Converts a boolean value to a color for connection status indication.
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        /// <summary>
        /// Gets the singleton instance of the converter.
        /// </summary>
        public static readonly BoolToColorConverter Instance = new();

        /// <inheritdoc/>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isConnected)
            {
                return isConnected 
                    ? new SolidColorBrush(Color.FromRgb(76, 175, 80))  // Green
                    : new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Gray
            }
            return new SolidColorBrush(Color.FromRgb(158, 158, 158));
        }

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("BoolToColorConverter only supports one-way conversion.");
        }
    }
}
