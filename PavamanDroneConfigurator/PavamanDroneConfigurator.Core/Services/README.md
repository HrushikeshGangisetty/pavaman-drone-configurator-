# Services

This folder contains business logic services that orchestrate application workflows.

## Purpose

Implement core business logic and coordinate between different parts of the application without depending on infrastructure details.

## Contents

- **Business Logic Services**: Core application logic (e.g., CalibrationService, FlightModeService)
- **Orchestration Services**: Coordinate complex workflows
- **Validation Services**: Business rule validation

## Guidelines

- Services should depend on interfaces, not concrete implementations
- Keep services focused on business logic, not infrastructure concerns
- Use dependency injection for service dependencies
- Services should be stateless when possible

## Example

```csharp
namespace PavamanDroneConfigurator.Core.Services
{
    public class CalibrationService : ICalibrationService
    {
        private readonly ISerialPortService _serialPort;
        
        public CalibrationService(ISerialPortService serialPort)
        {
            _serialPort = serialPort;
        }
        
        public async Task<CalibrationResult> CalibrateAccelerometer()
        {
            // Business logic for calibration workflow
        }
    }
}
```
