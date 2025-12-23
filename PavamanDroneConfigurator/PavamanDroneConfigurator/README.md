# Pavaman Aviation - Drone Configurator

A professional Windows desktop application for configuring ArduPilot-based flight controllers.

## Features

- MAVLink communication with flight controllers
- Accelerometer and compass calibration
- Flight mode configuration
- Motor testing with safety protocols
- RC calibration
- Firmware upload capability
- Spray system configuration

## Technology Stack

- **Framework:** .NET 9
- **UI:** AvaloniaUI with MVVM
- **MAVLink:** Asv.Mavlink
- **Architecture:** Clean Architecture (UI → Core → Infrastructure)

## Project Structure
```
PavamanDroneConfigurator/
├── PavamanDroneConfigurator/          # UI Layer (Avalonia)
├── PavamanDroneConfigurator.Core/     # Business Logic & Interfaces
└── PavamanDroneConfigurator.Infrastructure/  # MAVLink, Hardware
```

## Development Setup

### Prerequisites
- Visual Studio 2022 (v17.10+)
- .NET 9 SDK
- Git

### Build Instructions
1. Clone the repository
2. Open `PavamanDroneConfigurator.slnx` in Visual Studio
3. Restore NuGet packages
4. Build solution (Ctrl + Shift + B)

## Status

🚧 **In Development** - Initial architecture setup complete

## License

[Add your license here]

## Contact

Pavaman Aviation