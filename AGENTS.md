# AGENTS.md - BackSpeaker Mod Codebase Guide

## Project Overview

**BackSpeaker Mod** is a Unity mod for the game "Schedule I" that provides immersive background music functionality through virtual headphones. The mod uses the MelonLoader framework and implements a sophisticated music management system with multiple music sources, YouTube integration, and a modular architecture.

## Architecture Overview

### Core Design Principles

1. **Provider Pattern** - Extensible music source providers (`IMusicSourceProvider`)
2. **Modular Architecture** - Separate systems for audio, UI, download management
3. **Consolidated Management** - `SystemManager` orchestrates all subsystems
4. **IL2CPP Compatibility** - Full Unity IL2CPP backend support
5. **Clean API Surface** - `BackSpeakerManager` provides simplified public interface

### Music Source Types

The mod supports three primary music sources:
- **🎮 Jukebox** - In-game jukebox objects for testing/development
- **📁 LocalFolder** - Local audio files (MP3, WAV, OGG, M4A, AAC)
- **📺 YouTube** - YouTube URLs with download and streaming capabilities

## File Structure & Navigation

### Entry Point (`/`)
- `BackSpeakerMod.cs` - **MAIN ENTRY POINT** MelonLoader mod class
  - IL2CPP type registration
  - Scene initialization
  - Component lifecycle management

### Core Systems (`/Core/`)
- `BackSpeakerManager.cs` - **PUBLIC API FACADE** (160 lines)
- `BackSpeakerApp.cs` - Main UI application coordinator
- `System/SystemManager.cs` - **CORE ORCHESTRATOR** (598 lines)
- `System/LoggingSystem.cs` - Centralized logging

### Modules (`/Core/Modules/`)
- `AudioController.cs` - Audio playback control
- `AudioSession.cs` - Track session management  
- `AudioSessionManager.cs` - **LARGEST MODULE** (1154 lines) - Session coordination
- `TrackLoader.cs` - Music source loading (646 lines)
- `YouTubeMusicProvider.cs` - YouTube integration (458 lines)
- `LocalFolderMusicProvider.cs` - Local file provider (351 lines)
- `YouTubeDownloadManager.cs` - Download orchestration
- `YouTubeDownloadCache.cs` - Download caching system

### Audio Features (`/Core/Features/Audio/`)
- `AudioManager.cs` - Unity audio integration
- `GameAudioManager.cs` - Game-specific audio handling

### UI System (`/UI/`)
- `BackSpeakerScreen.cs` - Main UI screen component
- `Components/` - **MODULAR UI COMPONENTS**:
  - `PlaylistToggleComponent.cs` - **LARGEST COMPONENT** (3858 lines)
  - `YouTubePopupComponent.cs` - YouTube interface (1195 lines)  
  - `ActionButtonsComponent.cs` - Control buttons (421 lines)
  - `TrackInfoComponent.cs` - Track display (330 lines)
  - `TabBarComponent.cs` - Source selection tabs
  - `ControlsComponent.cs` - Playback controls
  - `ContentAreaComponent.cs` - Main content area
  - `ProgressBarComponent.cs` - Progress indication
  - `PopupManager.cs` - Popup lifecycle management

### Utilities (`/Utils/`)
- `YouTubePlaylistManager.cs` - **PLAYLIST SYSTEM** (560 lines)
- `YoutubeHelper.cs` - **LARGEST UTILITY** (843 lines) - YouTube integration
- `YouTubeMetadataManager.cs` - Metadata handling (341 lines)
- `URPMaterialHelper.cs` - Unity rendering support (501 lines)
- `SongDetails.cs` - Track metadata structure
- `AudioHelper.cs` - Audio processing utilities
- `EmbeddedYtDlpLoader.cs` - YouTube-dl integration

### Configuration (`/Configuration/`)
- `FeatureFlags.cs` - Feature toggle system
- `HeadphoneConfig.cs` - Headphone system configuration
- `HeadphoneState.cs` - Headphone state management
- `LoggingConfig.cs` - Logging configuration

## Key Classes Deep Dive

### BackSpeakerManager.cs (Public API)
**Purpose**: Clean, singleton API facade for all mod functionality
**Key Methods**:
- Audio Control: `Play()`, `Pause()`, `NextTrack()`, `SetVolume()`
- Source Management: `SetMusicSource()`, `GetAvailableMusicSources()`
- YouTube: `AddYouTubeSong()`, `LoadYouTubePlaylist()`
- Headphones: `AttachHeadphones()`, `ToggleHeadphones()`

### SystemManager.cs (Core Orchestrator)
**Purpose**: Coordinates all subsystems, manages initialization
**Key Responsibilities**:
- Module initialization and lifecycle
- Music source provider management
- Audio session coordination
- Event propagation

### PlaylistToggleComponent.cs (Main UI)
**Purpose**: Primary playlist management interface
**Key Features**:
- Tab switching between music sources
- Playlist popup creation and management
- YouTube playlist integration
- Real-time UI updates
- Download management interface

### YouTubePlaylistManager.cs (Playlist System)
**Purpose**: Manages YouTube playlist persistence and operations
**Key Features**:
- Playlist CRUD operations
- JSON persistence
- Event-driven updates
- Cache management

## Development Patterns

### Provider Pattern Implementation
```csharp
public interface IMusicSourceProvider
{
    MusicSourceType SourceType { get; }
    Task<List<AudioClip>> LoadTracksAsync();
    bool IsAvailable();
}
```

### Event-Driven Architecture
```csharp
// System-wide events
public Action? OnTracksReloaded { get; set; }

// YouTube playlist events  
YouTubePlaylistManager.OnPlaylistCreated += OnPlaylistCreated;
YouTubePlaylistManager.OnPlaylistUpdated += OnPlaylistUpdated;
```

### IL2CPP Registration Pattern
```csharp
// In BackSpeakerMod.OnLateInitializeMelon()
ClassInjector.RegisterTypeInIl2Cpp<UI.Components.TabBarComponent>();
ClassInjector.RegisterTypeInIl2Cpp<Core.Modules.YouTubeMusicProvider>();
```

### Async/Await for Non-Blocking Operations
```csharp
// Music loading doesn't block UI
await provider.LoadTracksAsync();
```

## Integration Points

### Game Integration
- **Unity UI System**: Full Unity UGUI integration
- **Audio Pipeline**: Integrates with Unity AudioSource/AudioClip
- **Scene Management**: Hooks into Unity scene loading
- **IL2CPP Backend**: Full compatibility with IL2CPP compilation

### External Dependencies
- **MelonLoader**: Mod framework and runtime
- **YoutubeExplode**: YouTube API integration (embedded)
- **AudioImportLib**: Audio format support (embedded)
- **Unity Engine**: Core functionality

## Navigation Helpers

### Understanding System Flow
1. **Initialization**: `BackSpeakerMod.OnInitializeMelon()`
2. **System Startup**: `SystemManager.Initialize()`
3. **UI Creation**: `BackSpeakerApp.CreateUI()`
4. **Provider Loading**: `TrackLoader.LoadFromProvider()`
5. **Audio Playback**: `AudioController.PlayTrack()`

### Key State Variables
- `SystemManager.currentMusicSource` - Active music source
- `AudioSessionManager.currentSession` - Active audio session
- `YouTubePlaylistManager.playlists` - All playlists
- `BackSpeakerManager.IsInitialized` - System ready state

### Debug Information
```csharp
// Consistent logging across all modules
LoggingSystem.Info($"Message", "ModuleName");
LoggingSystem.Debug($"Details", "ModuleName");
LoggingSystem.Error($"Error: {ex}", "ModuleName");
```

## Development Workflow

### Adding New Music Sources
1. Implement `IMusicSourceProvider` interface
2. Add to `MusicSourceType` enum
3. Register in `SystemManager.InitializeProviders()`
4. Add UI tab in `TabBarComponent`
5. Update `TrackLoader` routing

### Adding New UI Components
1. Create component class inheriting `MonoBehaviour`
2. Register in `BackSpeakerMod.OnLateInitializeMelon()`
3. Add to `BackSpeakerScreen.CreateUI()`
4. Wire up events and data binding

### Performance Considerations
- **Async Loading**: All track loading is non-blocking
- **Embedded Libraries**: No external dependencies to install
- **Smart Caching**: Downloaded content cached locally
- **Memory Management**: Proper cleanup in `Shutdown()`

## Common Development Scenarios

### Debugging Audio Issues
1. Check `AudioManager.IsInitialized()`
2. Verify `AudioSource` component attachment
3. Review `AudioSessionManager` track loading
4. Examine `GameAudioManager` integration

### Debugging YouTube Issues
1. Verify internet connectivity
2. Check `YoutubeHelper.IsValidUrl()`
3. Review `YouTubeDownloadManager` queue
4. Examine download cache in `YouTubeDownloadCache`

### UI Issues
1. Verify IL2CPP registration in `BackSpeakerMod`
2. Check component initialization in `BackSpeakerScreen`
3. Review event subscription in UI components
4. Examine `PopupManager` lifecycle

## Quick Reference

### Most Important Files (Prioritized)
1. `BackSpeakerMod.cs` - Entry point and registration
2. `BackSpeakerManager.cs` - Public API surface
3. `Core/System/SystemManager.cs` - Core orchestration
4. `UI/Components/PlaylistToggleComponent.cs` - Main UI logic
5. `Core/Modules/AudioSessionManager.cs` - Audio coordination
6. `Utils/YouTubePlaylistManager.cs` - Playlist management

### Key Extension Points
- **New Music Sources**: Implement `IMusicSourceProvider`
- **UI Components**: Add to `UI/Components/` and register
- **Audio Processing**: Extend `AudioHelper` utilities
- **Configuration**: Add to `Configuration/` system

### Common Debugging Entry Points
- `SystemManager.Initialize()` - System startup
- `TrackLoader.LoadFromProvider()` - Music loading
- `AudioController.PlayTrack()` - Playback start
- Component `Setup()` methods - UI initialization

This guide provides comprehensive navigation for the BackSpeaker mod's mature, production-ready codebase. 