# Constants

This folder contains application-wide constant values.

## Purpose

Define immutable constant values used throughout the application for configuration, limits, and default values.

## Contents

- **Application Constants**: App-wide settings and limits
- **Communication Constants**: Timeouts, buffer sizes, baudrates
- **Calibration Constants**: Thresholds, tolerances, sample counts
- **UI Constants**: Default values for UI elements

## Guidelines

- Use `const` for compile-time constants
- Use `static readonly` for runtime constants
- Group related constants in static classes
- Add XML documentation for each constant

## Example

```csharp
namespace PavamanDroneConfigurator.Core.Constants
{
    /// <summary>
    /// Constants related to serial communication
    /// </summary>
    public static class SerialConstants
    {
        /// <summary>
        /// Default baud rate for MAVLink communication (115200)
        /// </summary>
        public const int DefaultBaudRate = 115200;
        
        /// <summary>
        /// Timeout for serial port operations in milliseconds
        /// </summary>
        public const int ReadTimeoutMs = 1000;
        
        /// <summary>
        /// Maximum packet size for MAVLink messages
        /// </summary>
        public const int MaxPacketSize = 263;
    }
    
    /// <summary>
    /// Constants related to calibration procedures
    /// </summary>
    public static class CalibrationConstants
    {
        /// <summary>
        /// Number of samples required for accelerometer calibration
        /// </summary>
        public const int AccelSampleCount = 400;
        
        /// <summary>
        /// Tolerance for gyro calibration in degrees/second
        /// </summary>
        public const double GyroToleranceDegPerSec = 0.5;
    }
}
```
