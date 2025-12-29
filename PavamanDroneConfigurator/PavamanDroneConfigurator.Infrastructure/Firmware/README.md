# Firmware

This folder contains firmware upload and management logic.

## Purpose

Handle ArduPilot firmware upload, verification, and version management for flight controllers.

## Contents

- **Firmware Upload**: Flash firmware to flight controller
- **Bootloader Management**: Enter/exit bootloader mode
- **Verification**: Verify firmware integrity after upload
- **Version Detection**: Detect installed firmware version
- **Firmware Selection**: Help user select appropriate firmware

## Supported Firmware

- **ArduCopter**: Multi-rotor firmware
- **ArduPlane**: Fixed-wing firmware
- **ArduRover**: Ground vehicle firmware
- **Custom Builds**: Support for custom ArduPilot builds

## Upload Process

1. **Detect Flight Controller**: Identify board type and bootloader
2. **Enter Bootloader**: Reset FC into bootloader mode
3. **Erase Flash**: Clear existing firmware
4. **Upload Firmware**: Write new firmware in chunks
5. **Verify**: Read back and verify checksums
6. **Reboot**: Exit bootloader and start new firmware
7. **Validate**: Confirm firmware boots and responds

## Guidelines

- Always backup parameters before firmware update
- Verify firmware file integrity before upload
- Implement progress reporting for long uploads
- Handle upload failures gracefully with retry logic
- Never interrupt power during upload
- Validate firmware compatibility with hardware

## Example

```csharp
namespace PavamanDroneConfigurator.Infrastructure.Firmware
{
    public class FirmwareUploadService : IFirmwareUploadService
    {
        public async Task<FirmwareUploadResult> UploadFirmware(
            string firmwarePath, 
            IProgress<int> progress)
        {
            // 1. Verify firmware file
            ValidateFirmwareFile(firmwarePath);
            
            // 2. Enter bootloader
            await EnterBootloaderMode();
            
            // 3. Upload with progress reporting
            await UploadFirmwareChunks(firmwarePath, progress);
            
            // 4. Verify upload
            var verified = await VerifyFirmware();
            
            // 5. Reboot
            await RebootFlightController();
            
            return new FirmwareUploadResult { Success = verified };
        }
    }
}
```

## Safety Warnings

- ⚠️ **NEVER** disconnect power during upload
- ⚠️ **ALWAYS** backup parameters before update
- ⚠️ Verify firmware is for correct hardware
- ⚠️ Failed upload may brick the flight controller
- ⚠️ Keep backup of working firmware version

## Resources

- [ArduPilot Firmware Downloads](https://firmware.ardupilot.org/)
- [Bootloader Documentation](https://ardupilot.org/dev/docs/using-the-command-line-interpreter-to-configure-apm.html)
