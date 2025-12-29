# Safety

This folder contains safety gatekeeper services that prevent dangerous operations.

## Purpose

Implement safety checks, validation, and user confirmation mechanisms to prevent dangerous operations that could damage equipment or cause injury.

## Contents

- **Safety Gatekeeper**: Central safety validation service
- **Pre-flight Checks**: Verify drone is safe to operate
- **Arming Checks**: Validate conditions before arming
- **User Confirmations**: Require explicit user acknowledgment
- **Safety Interlocks**: Prevent simultaneous dangerous operations

## Safety Categories

### Hardware Safety
- Verify propellers are removed for motor tests
- Check battery voltage is within safe range
- Validate sensor health before flight
- Ensure RC radio is bound and responsive

### Software Safety
- Require explicit user confirmation for dangerous operations
- Implement operation timeouts
- Prevent arming if calibration is invalid
- Block motor tests if armed
- Require parameter backup before firmware update

### Operational Safety
- Multi-step confirmation for critical operations
- Visual and auditory warnings
- Emergency stop capabilities
- Operation logging for audit trail

## Guidelines

- **Never bypass safety checks** in production code
- Implement multiple layers of safety validation
- Require explicit user confirmation with clear warnings
- Log all safety-critical operations
- Use timeouts for all potentially dangerous operations
- Fail safe - default to safest state on error

## Example

```csharp
namespace PavamanDroneConfigurator.Infrastructure.Safety
{
    public class SafetyGatekeeperService : ISafetyGatekeeperService
    {
        public async Task<SafetyCheckResult> ValidateMotorTest()
        {
            var checks = new List<SafetyCheck>
            {
                await CheckDroneIsDisarmed(),
                await CheckPropellersRemoved(),
                await CheckBatteryVoltage(),
                await CheckUserConfirmation("Are you sure you want to test motors?"),
                await CheckUserConfirmation("Have you removed all propellers?"),
                await CheckUserConfirmation("Is the drone secured to prevent movement?")
            };
            
            return new SafetyCheckResult 
            { 
                Passed = checks.All(c => c.Passed),
                FailedChecks = checks.Where(c => !c.Passed).ToList()
            };
        }
        
        public async Task<bool> RequireMultiStepConfirmation(string operation)
        {
            // Step 1: Show warning
            var warning = await ShowWarning(operation);
            if (!warning) return false;
            
            // Step 2: Type confirmation
            var typed = await RequireTypedConfirmation("I UNDERSTAND THE RISKS");
            if (!typed) return false;
            
            // Step 3: Final OK
            var final = await ShowFinalConfirmation();
            return final;
        }
    }
}
```

## Critical Safety Rules

### ⚠️ ABSOLUTE RULES - NEVER VIOLATE

1. **NEVER** test motors with propellers attached
2. **NEVER** bypass safety confirmation dialogs in production
3. **ALWAYS** verify drone is disarmed before motor tests
4. **ALWAYS** require user confirmation for dangerous operations
5. **ALWAYS** implement operation timeouts
6. **ALWAYS** log safety-critical operations
7. **NEVER** allow multiple dangerous operations simultaneously
8. **ALWAYS** default to safest state on error

### Confirmation Requirements

Operations requiring multi-step confirmation:
- Motor testing
- Firmware upload
- Parameter reset to defaults
- ESC calibration
- Full throttle operations

## Audit and Logging

All safety-critical operations must be logged with:
- Timestamp
- User identity
- Operation type
- Safety check results
- User confirmations
- Operation outcome

## Testing

Safety systems must be:
- Unit tested for all code paths
- Integration tested with real hardware
- Reviewed by multiple developers
- Never mocked or stubbed in production
