# BackSpeaker Mod for Schedule I

A headphone-based audio system mod for Schedule I that allows players to listen to in-game music through virtual headphones.

## ✅ Recent Improvements

### System Architecture Consolidation
- **Simplified System Files**: Consolidated 6 redundant system files into a single `SystemManager.cs`
- **Cleaner API**: All functionality now accessible through `BackSpeakerManager` with a clean, simple interface
- **Reduced Complexity**: Removed overlapping responsibilities between SystemCoordinator, SystemComponents, APIManager, etc.
- **Better Performance**: Smaller build size and reduced memory footprint

### Build Configuration System
- **Automatic Log Levels**: Log levels automatically adjust based on build configuration
- **Multiple Build Types**: Debug, Release, Verbose, Minimal, and IL2CPP configurations
- **Smart Optimization**: Conditional compilation removes unused logging code in production builds
- **Easy Build Script**: Simple `./build_configurations.sh [type]` command for building

### UI & User Experience Fixes
- **Fixed UI Bleeding**: Apps no longer show in other phone apps
- **Clean Button Layout**: Headphone controls don't overlap with music app buttons
- **Headphone-First Audio**: Music only plays when headphones are attached
- **Visual Feedback**: Button colors and states clearly indicate headphone status

## 📁 Architecture Overview

### Core System Structure (Simplified)
```
Core/
├── BackSpeakerManager.cs          # Main API - your entry point
├── BackSpeakerApp.cs              # Phone app integration
└── System/
    ├── SystemManager.cs           # Consolidated system management (NEW)
    └── LoggingSystem.cs           # Logging with build-aware levels
```

### Key Features
- **Consolidated SystemManager**: Handles initialization, API, configuration, and components in one place
- **Build-Aware Logging**: Automatically adjusts verbosity based on Debug/Release/Verbose/Minimal builds
- **Feature Toggles**: Easy runtime control of headphones, audio, and debugging features
- **Event-Driven Architecture**: Clean event handling for headphone attachment/detachment
- **Configuration Management**: Built-in settings system for runtime configuration

### Removed Redundancy
Previously had 6 separate system files with overlapping responsibilities:
- ❌ `SystemCoordinator.cs` (replaced by SystemManager)
- ❌ `SystemComponents.cs` (functionality moved to SystemManager)
- ❌ `APIManager.cs` (API methods moved to SystemManager)
- ❌ `SystemInitializer.cs` (initialization moved to SystemManager)
- ❌ `FeatureToggleSystem.cs` (feature flags moved to SystemManager.Features)
- ❌ `ConfigurationManager.cs` (configuration moved to SystemManager)

## 🚀 Quick Start

### Building the Mod
```bash
# Development (full logging)
./build_configurations.sh debug

# Production (minimal logging) 
./build_configurations.sh release

# Troubleshooting (verbose logging, optimized)
./build_configurations.sh verbose

# High performance (errors only)
./build_configurations.sh minimal

# View all options
./build_configurations.sh help
```

### Basic Usage
```csharp
// Get the manager instance
var manager = BackSpeakerManager.Instance;

// Control headphones
bool attached = manager.AttachHeadphones();
manager.RemoveHeadphones();
bool toggled = manager.ToggleHeadphones();

// Control audio (only works with headphones attached)
manager.Play();
manager.Pause();
manager.NextTrack();
manager.SetVolume(0.8f);

// Get status
string status = manager.GetSystemStatus();
bool isPlaying = manager.IsPlaying;
```

### Configuration
```csharp
// Feature toggles
SystemManager.Features.HeadphonesEnabled = true;
SystemManager.Features.AudioEnabled = true;
SystemManager.Features.ShowDebugInfo = false;
SystemManager.Features.AutoLoadTracks = true;

// Runtime settings
manager.SetSetting("Audio.Volume", 0.7f);
float volume = manager.GetSetting<float>("Audio.Volume", 0.5f);
```

## 🔧 Development

### Logging System
The logging system automatically adjusts based on your build configuration:

- **Debug builds**: Show all logs (Debug, Info, Warning, Error)
- **Release builds**: Show warnings and errors only
- **Verbose builds**: Show all logs but with optimization
- **Minimal builds**: Show errors only

```csharp
// Logging examples
LoggingSystem.Debug("Detailed debug info", "Category");
LoggingSystem.Info("Important information", "Category");
LoggingSystem.Warning("Something concerning", "Category");
LoggingSystem.Error("An error occurred", "Category");

// Conditional logging (only in debug/verbose builds)
LoggingSystem.Verbose("Very detailed info", "Category");
LoggingSystem.Performance("Operation took 50ms", "Performance");
```

### System Manager API
```csharp
// Direct access to system manager
var systemManager = BackSpeakerManager.Instance.systemManager;

// Lifecycle
bool initialized = systemManager.Initialize();
systemManager.Update();
systemManager.Shutdown();

// Feature control
SystemManager.Features.HeadphonesEnabled = false; // Disable headphones
SystemManager.Features.AudioEnabled = false;      // Disable audio

// Event handling
systemManager.OnTracksReloaded += () => LoggingSystem.Info("Tracks reloaded");
systemManager.OnSpeakerAttached += (audioSource) => LoggingSystem.Info("Speaker attached");
```

## 📊 Build Results

| Configuration | Size | Logging Level | Use Case |
|---------------|------|---------------|----------|
| Debug | 484K | All logs | Development |
| Release | 476K | Warnings+ | Production |
| Verbose | 484K | All logs (optimized) | Troubleshooting |
| Minimal | 476K | Errors only | High performance |

## 🎯 Key Benefits of Consolidation

1. **Simplified Development**: One file to understand instead of six
2. **Reduced Complexity**: Clear separation of concerns within regions
3. **Better Performance**: Smaller builds, less memory overhead
4. **Easier Debugging**: All system logic in one place
5. **Cleaner API**: Consistent interface through BackSpeakerManager
6. **Build-Aware Features**: Automatic optimization based on build type

## 📖 Documentation

- **[LOGGING.md](LOGGING.md)**: Complete logging system documentation
- **[refactor.md](refactor.md)**: Notes on recent refactoring (simplified architecture)

## 🎵 Features

- 🎧 **Virtual Headphones**: Attach/detach headphones to control audio access
- 🎵 **Music Playback**: Play in-game jukebox tracks through headphones
- 📱 **Phone Integration**: Clean phone app with music controls
- 🎛️ **Audio Controls**: Play, pause, skip, volume, progress seeking
- 🔁 **Repeat Modes**: None, Single, All
- 📃 **Playlist Management**: View and select tracks
- ⚙️ **Runtime Configuration**: Adjustable settings and feature toggles
- 🔧 **Developer Tools**: Comprehensive logging and debugging features

The mod now features a much cleaner, simpler architecture while maintaining all original functionality and adding new capabilities!