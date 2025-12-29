# Models

This folder contains domain models and data transfer objects (DTOs) for the application.

## Purpose

Define the data structures that represent the core business entities and data contracts used throughout the application.

## Contents

- **Domain Models**: Core business entities (e.g., DroneConfiguration, CalibrationData)
- **DTOs**: Data transfer objects for moving data between layers
- **Value Objects**: Immutable objects representing domain concepts

## Guidelines

- Keep models free of business logic (use Services for that)
- Models should be plain C# classes with properties
- Use data annotations for validation where appropriate
- Implement `INotifyPropertyChanged` only if needed for UI binding

## Example

```csharp
namespace PavamanDroneConfigurator.Core.Models
{
    public class DroneConfiguration
    {
        public string FirmwareVersion { get; set; }
        public List<FlightMode> FlightModes { get; set; }
        public CalibrationStatus Status { get; set; }
    }
}
```
