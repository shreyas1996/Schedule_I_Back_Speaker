# Logging System Documentation

## Overview

The BackSpeaker mod includes a sophisticated logging system that automatically adjusts log levels based on build configuration. This allows for detailed debugging during development while keeping production builds clean and performant.

## Build Configurations

### Available Configurations

| Configuration | Log Level | Description | Use Case |
|---------------|-----------|-------------|----------|
| `Debug` | Debug | All logs enabled, unoptimized code | Development |
| `Release` | Warning | Warnings and errors only, optimized | Production release |
| `Verbose` | Debug | All logs enabled, optimized code | Production troubleshooting |
| `Minimal` | Error | Errors only, fully optimized | High-performance production |
| `IL2CPP` | Error | IL2CPP optimized with minimal logging | IL2CPP builds |

### Building with Different Configurations

Use the provided build script for easy configuration switching:

```bash
# Development build with full logging
./build_configurations.sh debug

# Production build with minimal logging
./build_configurations.sh release

# Troubleshooting build (optimized but verbose)
./build_configurations.sh verbose

# High-performance build (errors only)
./build_configurations.sh minimal

# Build all configurations
./build_configurations.sh all

# Clean build artifacts
./build_configurations.sh clean

# Show help
./build_configurations.sh help
```

### Manual Building

You can also build manually using dotnet:

```bash
# Debug build
dotnet build --configuration Debug

# Release build
dotnet build --configuration Release

# Verbose build
dotnet build --configuration Verbose

# Minimal build
dotnet build --configuration Minimal
```

## Logging API

### Log Levels

```csharp
public enum LogLevel
{
    Debug = 0,    // Detailed debug information
    Info = 1,     // Important information
    Warning = 2,  // Warnings and concerning events
    Error = 3     // Errors and critical issues
}
```

### Basic Logging Methods

```csharp
// Debug information (hidden in production builds)
LoggingSystem.Debug("Detailed debug info", "Category");

// Important information
LoggingSystem.Info("Important information", "Category");

// Warnings
LoggingSystem.Warning("Something concerning happened", "Category");

// Errors
LoggingSystem.Error("An error occurred", "Category");
```

### Advanced Logging Methods

```csharp
// Verbose debug logging (only in debug/verbose builds)
LoggingSystem.Verbose("Very detailed debug info", "Category");

// Performance logging (only in debug builds)
LoggingSystem.Performance("Operation took 50ms", "Performance");
```

### Runtime Configuration

```csharp
// Change log level at runtime
LoggingSystem.MinLevel = LogLevel.Warning;

// Enable/disable logging entirely
LoggingSystem.Enabled = false;

// Convenience methods
LoggingSystem.SetDebugMode();     // LogLevel.Debug
LoggingSystem.SetProductionMode(); // LogLevel.Info
LoggingSystem.SetQuietMode();     // LogLevel.Warning
```

## Conditional Compilation

The logging system uses preprocessor directives to optimize builds:

### Debug Builds (`DEBUG`)
- All logging methods are active
- Performance logging enabled
- Detailed system information logged
- No optimization

### Release Builds (`RELEASE`)
- Info and Debug messages may be suppressed
- Optimized code
- Minimal logging overhead

### Verbose Builds (`VERBOSE_LOGGING`)
- All logging methods active
- Optimized code
- Useful for production troubleshooting

### Minimal Builds (`MINIMAL_LOGGING`)
- Only warnings and errors
- Maximum optimization
- Minimal logging overhead

## Configuration Class

The `LoggingConfig` class provides build-time defaults and runtime configuration:

```csharp
// Get build-appropriate default log level
var defaultLevel = LoggingConfig.DefaultLogLevel;

// Check if performance logging is enabled
if (LoggingConfig.EnablePerformanceLogging)
{
    LoggingSystem.Performance("Operation completed", "Performance");
}

// Apply configuration to logging system
LoggingConfig.ApplyToLoggingSystem();

// Get configuration summary
var summary = LoggingConfig.GetConfigSummary();
```

## Best Practices

### During Development
- Use `Debug` configuration for full visibility
- Use appropriate categories for different subsystems
- Use `LoggingSystem.Verbose()` for very detailed debugging

### For Production
- Use `Release` configuration for normal production builds
- Use `Verbose` configuration when troubleshooting production issues
- Use `Minimal` configuration for maximum performance

### Code Guidelines
- Always provide meaningful categories
- Use appropriate log levels
- Avoid logging in tight loops (use `Performance` logging instead)
- Use conditional compilation for expensive logging operations

### Example Usage

```csharp
public class AudioManager
{
    public void Initialize()
    {
        LoggingSystem.Info("AudioManager initializing", "Audio");
        
        try
        {
            // Initialization code
            LoggingSystem.Debug("Audio system initialized successfully", "Audio");
        }
        catch (Exception ex)
        {
            LoggingSystem.Error($"Failed to initialize audio: {ex.Message}", "Audio");
            throw;
        }
    }
    
    public void Update()
    {
        // Only log performance in debug builds
        LoggingSystem.Performance("Audio update completed", "Audio");
        
        // Verbose logging for detailed debugging
        LoggingSystem.Verbose($"Current track: {currentTrack?.name}", "Audio");
    }
}
```

## Output Examples

### Debug Build Output
```
[INFO][System] LoggingSystem initialized in DEBUG mode - all logs enabled
[INFO][System] Build Configuration: DEBUG
[DEBUG][Audio] Loading audio tracks from jukebox
[VERBOSE][Audio] Found 15 tracks in playlist
[PERF][Audio] Track loading completed in 125ms
```

### Release Build Output
```
[INFO][System] LoggingSystem initialized in RELEASE mode - minimal logging
[WARN][Audio] Audio track not found: missing_track.mp3
[ERROR][Audio] Failed to initialize audio system
```

### Minimal Build Output
```
[ERROR][Audio] Critical audio system failure
``` 