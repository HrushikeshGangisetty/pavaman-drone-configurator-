# Calibration

This folder contains calibration engines for drone sensors.

## Purpose

Implement calibration procedures for accelerometers, gyroscopes, compasses, and RC transmitters.

## Contents

- **Accelerometer Calibration**: 6-point level calibration
- **Compass Calibration**: Magnetic field calibration with interference detection
- **Gyro Calibration**: Gyroscope bias calibration
- **RC Calibration**: Radio control stick calibration
- **ESC Calibration**: Electronic Speed Controller calibration

## Calibration Types

### Accelerometer (6-Point)
Collects data in 6 orientations (level, nose down, nose up, left, right, inverted) to calculate offsets and scale factors.

### Compass (Magnetic)
Rotates the drone to collect magnetometer data and calculate hard/soft iron corrections.

### Gyroscope
Requires drone to be stationary while collecting samples to determine gyro bias.

### RC Radio
Captures min/max/center values for each channel while user moves sticks to extremes.

## Guidelines

- Always verify drone is stationary before gyro calibration
- Require user confirmation at each step
- Implement timeout mechanisms for safety
- Store calibration results in flight controller EEPROM
- Validate calibration quality before accepting

## Example

```csharp
namespace PavamanDroneConfigurator.Infrastructure.Calibration
{
    public class AccelerometerCalibrationEngine : ICalibrationEngine
    {
        public async Task<CalibrationResult> RunCalibration()
        {
            // 1. Level position
            await CollectSamples(CalibrationOrientation.Level);
            
            // 2. Nose down
            await CollectSamples(CalibrationOrientation.NoseDown);
            
            // ... continue for all 6 positions
            
            // Calculate offsets and scale factors
            var result = CalculateCalibrationParameters();
            
            // Store to flight controller
            await WriteParametersToFC(result);
            
            return result;
        }
    }
}
```

## Safety Considerations

- ⚠️ ALWAYS ensure props are removed during calibration
- ⚠️ Verify drone is disarmed before starting
- ⚠️ Timeout all operations to prevent infinite loops
- ⚠️ Validate sensor readings are within expected ranges
