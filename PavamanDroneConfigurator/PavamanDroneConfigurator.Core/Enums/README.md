# Enums

This folder contains enumeration types used throughout the application.

## Purpose

Define strongly-typed enumeration values for consistent domain concepts across the application.

## Contents

- **Status Enumerations**: States and statuses (e.g., ConnectionStatus, CalibrationStatus)
- **Type Enumerations**: Categories and types (e.g., FlightMode, SensorType)
- **Action Enumerations**: Operation types (e.g., CalibrationAction, FirmwareAction)

## Guidelines

- Use enums for fixed sets of related constants
- Use `[Flags]` attribute for bit-field enums
- Consider using descriptive names that are self-documenting
- Add XML documentation comments for clarity

## Example

```csharp
namespace PavamanDroneConfigurator.Core.Enums
{
    /// <summary>
    /// Represents the current status of the drone connection
    /// </summary>
    public enum ConnectionStatus
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Error = 3
    }
    
    /// <summary>
    /// Available flight modes for ArduPilot
    /// </summary>
    public enum FlightMode
    {
        Stabilize = 0,
        Acro = 1,
        AltHold = 2,
        Auto = 3,
        Guided = 4,
        Loiter = 5,
        RTL = 6,
        Land = 9
    }
}
```
