# Pavaman Aviation - Drone Configurator

A professional Windows desktop application for configuring, calibrating, and validating ArduPilot-based flight controllers.

## 🚀 Features

- **MAVLink Communication** - Real-time telemetry and command interface
- **Calibration Suite** - Accelerometer, compass, and RC calibration
- **Flight Mode Configuration** - Easy setup of flight modes and parameters
- **Motor Testing** - Safe motor testing with built-in safety protocols
- **Firmware Management** - Upload and manage ArduPilot firmware
- **Spray System Control** - Agricultural drone spray system configuration
- **Safety First** - Multiple safety checks and user confirmations

## 🏗️ Technology Stack

- **Framework:** .NET 9.0
- **UI Framework:** AvaloniaUI (Cross-platform XAML)
- **Architecture:** MVVM with ReactiveUI
- **MAVLink Library:** Asv.Mavlink 4.x (MIT License)
- **Serial Communication:** System.IO.Ports
- **Authentication:** IdentityModel.OidcClient
- **Dependency Injection:** Microsoft.Extensions.DependencyInjection

## 📁 Project Structure

```
PavamanDroneConfigurator/
├── PavamanDroneConfigurator/              # UI Layer (Avalonia Views & ViewModels)
│   ├── Views/                             # XAML views
│   ├── Assets/                            # Images, icons, resources
│   ├── Styles/                            # UI styling
│   └── App.axaml                          # Application entry point
│
├── PavamanDroneConfigurator.Core/         # Business Logic Layer
│   ├── Interfaces/                        # Service interfaces
│   ├── Models/                            # Domain models
│   ├── Services/                          # Business logic services
│   ├── ViewModels/                        # MVVM ViewModels
│   ├── Enums/                             # Enumerations
│   └── Constants/                         # Application constants
│
└── PavamanDroneConfigurator.Infrastructure/  # Hardware/External Layer
    ├── MAVLink/                           # MAVLink protocol implementation
    ├── Serial/                            # Serial port communication
    ├── Calibration/                       # Calibration engines
    ├── Firmware/                          # Firmware upload logic
    └── Safety/                            # Safety gatekeeper services
```

## 🔧 Prerequisites for Developers

Before you can work on this project, you need to install:

### Required Software

1. **Visual Studio 2022 (v17.10 or later)**
   - Download: https://visualstudio.microsoft.com/downloads/
   - Edition: Community (free), Professional, or Enterprise
   - Workloads to install during VS setup:
     - ✅ .NET Desktop Development
     - ✅ Desktop development with C++

2. **.NET 9.0 SDK**
   - Download: https://dotnet.microsoft.com/download/dotnet/9.0
   - Choose: .NET SDK 9.0.x (x64)
   - **Important:** Install SDK, not just Runtime

3. **Git for Windows**
   - Download: https://git-scm.com/download/win
   - Use default installation options

### Optional but Recommended

- **GitHub Desktop** - Easier Git management: https://desktop.github.com/
- **Windows Terminal** - Better command line experience
- **USB Drivers** (for testing with hardware):
  - CP210x USB to UART Bridge: https://www.silabs.com/developers/usb-to-uart-bridge-vcp-drivers
  - FTDI Drivers: https://ftdichip.com/drivers/vcp-drivers/

---

## 🚀 Getting Started (First Time Setup)

### Step 1: Verify Prerequisites

Open **Command Prompt** and verify installations:

```bash
# Check .NET SDK
dotnet --version
# Should show: 9.0.xxx

# Check Git
git --version
# Should show: git version 2.x.x
```

If any command fails, install the missing prerequisite.

### Step 2: Clone the Repository

**Option A: Using Git Command Line**

```bash
# Navigate to your projects folder
cd C:\Projects

# Clone the repository
git clone https://github.com/YOUR_USERNAME/pavaman-drone-configurator.git

# Navigate into project
cd pavaman-drone-configurator
```

**Option B: Using Visual Studio**

1. Open Visual Studio 2022
2. Click **"Clone a repository"**
3. Enter repository URL: `https://github.com/YOUR_USERNAME/pavaman-drone-configurator.git`
4. Choose local path: `C:\Projects\pavaman-drone-configurator`
5. Click **Clone**

**Option C: Using GitHub Desktop**

1. Open GitHub Desktop
2. File → Clone Repository
3. Select the repository from your GitHub account
4. Choose local path
5. Click **Clone**

### Step 3: Install Avalonia Templates

Open **Command Prompt** or **PowerShell** and run:

```bash
dotnet new install Avalonia.Templates
```

Wait for completion (shows "Success: Avalonia.Templates installed...")

### Step 4: Open Solution in Visual Studio

1. Navigate to the cloned folder
2. Double-click `PavamanDroneConfigurator.slnx` (or `.sln`)
3. Visual Studio opens the solution

### Step 5: Restore NuGet Packages

Visual Studio should automatically restore packages. If not:

1. Right-click on **Solution 'PavamanDroneConfigurator'** in Solution Explorer
2. Click **Restore NuGet Packages**
3. Wait for completion (check Output window)

### Step 6: Build the Solution

1. Press **Ctrl + Shift + B** (or Build → Build Solution)
2. Check **Output** window at bottom
3. Should see: **"Build: 3 succeeded, 0 failed"**

### Step 7: Verify Setup

Run this checklist:

- ✅ Solution Explorer shows 3 projects
- ✅ No red squiggly lines in code files
- ✅ Build succeeds without errors
- ✅ Dependencies folder in each project shows all NuGet packages

**If you see any errors**, see Troubleshooting section below.

---

## 💻 Development Workflow

### Daily Workflow

```bash
# 1. Pull latest changes before starting work
git pull

# 2. Create a new branch for your feature
git checkout -b feature/your-feature-name

# 3. Make your changes in Visual Studio

# 4. Build and test locally
# Press Ctrl + Shift + B in Visual Studio

# 5. Commit your changes
git add .
git commit -m "Add description of your changes"

# 6. Push your branch
git push origin feature/your-feature-name

# 7. Create Pull Request on GitHub for review
```

### Branch Naming Convention

- `feature/description` - New features (e.g., `feature/compass-calibration`)
- `bugfix/description` - Bug fixes (e.g., `bugfix/serial-timeout`)
- `refactor/description` - Code refactoring
- `docs/description` - Documentation updates

### Commit Message Guidelines

Write clear, descriptive commit messages:

```bash
# Good commits
git commit -m "Add MAVLink connection service with timeout handling"
git commit -m "Fix compass calibration state machine transitions"
git commit -m "Refactor parameter manager for better error handling"

# Bad commits (avoid these)
git commit -m "updates"
git commit -m "fix"
git commit -m "changes"
```

---

## 🏃 Running the Application

### Debug Mode

1. In Visual Studio, press **F5** (or click green "Play" button)
2. Application launches in debug mode
3. You can set breakpoints and step through code

### Release Mode

1. In Visual Studio, change dropdown from "Debug" to "Release"
2. Press **Ctrl + F5** to run without debugging
3. Application runs at full speed

### With Hardware

To test with actual drone hardware:

1. Connect flight controller via USB
2. Open Device Manager (Windows + X → Device Manager)
3. Find COM port under "Ports (COM & LPT)"
4. Note the COM port number (e.g., COM3)
5. Run application and select that COM port

---

## 📦 NuGet Packages Used

### Main UI Project
- Avalonia (UI Framework)
- Avalonia.ReactiveUI (MVVM support)
- ReactiveUI (Reactive programming)
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging
- Microsoft.Extensions.Logging.Console

### Core Project
- ReactiveUI
- System.Reactive
- Microsoft.Extensions.Logging.Abstractions

### Infrastructure Project
- **Asv.Mavlink** (MAVLink protocol - MIT License)
- System.IO.Ports (Serial communication)
- IdentityModel.OidcClient (Authentication)
- Microsoft.Extensions.Logging.Abstractions

---

## 🐛 Troubleshooting

### Problem: "Package Asv.Mavlink is not compatible"

**Solution:** You need .NET 9 SDK

```bash
# Check your .NET version
dotnet --version

# If it shows 8.x or lower:
# 1. Download .NET 9 SDK from https://dotnet.microsoft.com/download/dotnet/9.0
# 2. Install it
# 3. Restart Visual Studio
# 4. Rebuild solution
```

### Problem: "Avalonia templates not found"

**Solution:** Install Avalonia templates

```bash
dotnet new install Avalonia.Templates
```

Then restart Visual Studio.

### Problem: Build fails with "Could not find file"

**Solution:** Clean and rebuild

1. Build → Clean Solution
2. Close Visual Studio
3. Delete these folders in each project:
   - `bin/`
   - `obj/`
4. Delete `.vs/` folder in solution root (it's hidden)
5. Reopen Visual Studio
6. Build → Rebuild Solution

### Problem: NuGet packages not restoring

**Solution:** Clear NuGet cache

```bash
# In Command Prompt or PowerShell
dotnet nuget locals all --clear

# Then in Visual Studio:
# Right-click Solution → Restore NuGet Packages
```

### Problem: Git authentication issues

**Solution:** Use Personal Access Token

1. Go to GitHub → Settings → Developer Settings → Personal Access Tokens
2. Generate new token (classic)
3. Give it `repo` permissions
4. Copy the token
5. When Git asks for password, paste the token

### Problem: "Cannot connect to COM port"

**Solution:** Install USB drivers

- For Silicon Labs chips: https://www.silabs.com/developers/usb-to-uart-bridge-vcp-drivers
- For FTDI chips: https://ftdichip.com/drivers/vcp-drivers/

---

## 📚 Learning Resources

### Avalonia UI
- Official Docs: https://docs.avaloniaui.net/
- Samples: https://github.com/AvaloniaUI/Avalonia.Samples

### MAVLink Protocol
- Official: https://mavlink.io/en/
- ArduPilot MAVLink: https://ardupilot.org/dev/docs/mavlink-basics.html

### ReactiveUI
- Documentation: https://www.reactiveui.net/docs/
- Handbook: https://www.reactiveui.net/docs/handbook/

### MVVM Pattern
- Microsoft Guide: https://learn.microsoft.com/en-us/dotnet/architecture/maui/mvvm

---

## 🤝 Contributing

### Before Creating a Pull Request

1. ✅ Code builds without errors
2. ✅ All existing tests pass
3. ✅ New features have been tested manually
4. ✅ Code follows project style guidelines
5. ✅ Commit messages are clear and descriptive
6. ✅ No sensitive data (passwords, API keys) in commits

### Code Style

- Use **meaningful variable names** (no single letters except loop counters)
- Add **XML comments** to public methods and classes
- Keep methods **small and focused** (under 50 lines when possible)
- Follow **C# naming conventions**:
  - Classes: `PascalCase`
  - Methods: `PascalCase`
  - Private fields: `_camelCase`
  - Local variables: `camelCase`

---

## 🔒 Safety & Legal

### Hardware Safety

⚠️ **CRITICAL SAFETY RULES:**

1. **NEVER** test motors with propellers attached
2. **ALWAYS** verify drone is disarmed before motor tests
3. **ALWAYS** use bench power supply, not flight battery, during testing
4. **NEVER** override safety checks in production code
5. **ALWAYS** require explicit user confirmation for dangerous operations

### License Information

- **Asv.Mavlink:** MIT License (Commercial use allowed)
- **Avalonia:** MIT License (Commercial use allowed)
- **Project License:** [To be determined by Pavaman Aviation]

---

## 📞 Support & Contact

### For Development Questions

1. Check this README first
2. Search existing GitHub Issues
3. Create a new Issue with detailed description

### For Hardware Issues

Contact Pavaman Aviation technical support

---

## 📊 Project Status

🚧 **Current Status:** Initial Development Phase

### Completed
- ✅ Project architecture setup
- ✅ NuGet packages configuration
- ✅ Git repository initialization
- ✅ Development environment documentation

### In Progress
- 🔄 MAVLink communication layer
- 🔄 Basic UI layout
- 🔄 Connection management

### Planned
- 📋 Calibration state machines
- 📋 Parameter management
- 📋 Firmware upload
- 📋 Motor testing
- 📋 Flight mode configuration
- 📋 Spray system integration

---

## 🎯 Quick Reference

### Essential Commands

```bash
# Update your local repository
git pull

# Check what files changed
git status

# Create and switch to new branch
git checkout -b feature/my-feature

# Stage all changes
git add .

# Commit changes
git commit -m "Description"

# Push to GitHub
git push origin feature/my-feature

# Switch back to main branch
git checkout main

# Delete local branch
git branch -d feature/my-feature
```

### Visual Studio Shortcuts

- **Build:** Ctrl + Shift + B
- **Run (Debug):** F5
- **Run (No Debug):** Ctrl + F5
- **Find in Files:** Ctrl + Shift + F
- **Go to Definition:** F12
- **Format Document:** Ctrl + K, Ctrl + D
- **Comment/Uncomment:** Ctrl + K, Ctrl + C / Ctrl + K, Ctrl + U

---

## 📝 Version History

### v0.1.0 (Current)
- Initial project setup
- Architecture design
- Development environment configuration

---

**Last Updated:** December 2024  
**Maintained by:** Pavaman Aviation Development Team