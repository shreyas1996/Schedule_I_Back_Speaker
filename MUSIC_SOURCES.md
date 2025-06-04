# BackSpeaker Music Sources Guide

## Overview

BackSpeaker now supports three different music sources, each with its own capabilities and use cases:

1. **🎮 In-Game Jukebox** - Original functionality for testing
2. **📁 Local Music Folder** - Play your own music files  
3. **📺 YouTube Music** - Stream and download music from YouTube

## Music Source Types

### 1. In-Game Jukebox Music 🎮

**Purpose**: Testing and integration with game audio
- Loads music from in-game jukebox objects
- Primarily used for mod testing and development
- Always available (fallback option)

**How to use**:
1. Click the "🎮 Jukebox" button
2. Click "Reload Jukebox Music" to refresh
3. Music will load from any jukebox objects in the current scene

### 2. Local Music Folder 📁

**Purpose**: Play your personal music collection
- Loads music files from a dedicated folder
- Supports multiple audio formats
- Files are loaded directly into memory

**Supported Formats**:
- `.mp3` (recommended)
- `.wav`
- `.ogg`
- `.m4a`
- `.aac`

**Setup**:
1. Click the "📁 Local Files" button
2. Click "Open Folder" to create/open the music directory
3. Copy your music files to: `GameDirectory/Mods/BackSpeaker/Music/`
4. Click "Refresh Tracks" to load the files

**Folder Location**: `[Game Directory]/Mods/BackSpeaker/Music/`

### 3. YouTube Music 📺

**Purpose**: Download and play music from YouTube
- Downloads audio from YouTube videos using YoutubeExplode library
- **No external dependencies required** - everything built-in
- Caches downloaded files for future use
- Requires internet connection

**Requirements**:
- Internet connection for downloads
- Valid YouTube URLs
- **No installation required** - YoutubeExplode is included

**How to use**:
1. Click the "📺 YouTube" button
2. Paste a YouTube URL in the input field
3. Click "Download" to extract audio
4. The track will be added to your playlist automatically

**Supported URL formats**:
- `https://youtube.com/watch?v=VIDEO_ID`
- `https://youtu.be/VIDEO_ID`
- `https://m.youtube.com/watch?v=VIDEO_ID`
- `https://youtube.com/embed/VIDEO_ID`

**Cache Location**: `[Game Directory]/Mods/BackSpeaker/Cache/YouTube/`

## UI Navigation

### Music Source Selector
- Located at the top of the BackSpeaker interface
- Three color-coded buttons for each source:
  - 🎮 Green: Jukebox (always available)
  - 📁 Blue: Local Files (available when folder exists)
  - 📺 Red: YouTube (always available - no dependencies)

### Source-Specific Panels
Each music source has its own control panel that appears when selected:

#### Jukebox Panel
- Simple reload button
- Status information

#### Local Folder Panel
- Folder path display
- "Open Folder" button (creates folder if needed)
- "Refresh Tracks" button
- File count and status information

#### YouTube Panel
- URL input field
- Download button with progress indication
- Help button with detailed instructions
- Status messages and error reporting

## Tips and Best Practices

### Local Music Files
1. **Organize your files**: Use clear, descriptive filenames
2. **Use MP3 format**: Best compatibility and performance
3. **Avoid very large files**: They may cause memory issues
4. **Check file permissions**: Ensure the mod can read your files

### YouTube Downloads
1. **Check video availability**: Some videos may be region-locked
2. **Respect copyright**: Only download content you have rights to use
3. **Monitor cache size**: Downloads are cached (max 50 files by default)
4. **Network requirements**: Downloads require stable internet connection

### General Usage
1. **Switch sources anytime**: You can change between sources without restarting
2. **Playlist integration**: All sources work with the existing playlist system
3. **Audio controls**: All standard controls (play, pause, volume, etc.) work with any source
4. **Headphone compatibility**: All sources work with the headphone attachment system

## Troubleshooting

### Common Issues

#### Local Files Not Loading
- Check folder permissions
- Verify file formats are supported
- Try refreshing tracks
- Check log files for specific errors

#### YouTube Downloads Failing
- Check internet connection
- Verify URL format is correct
- Some videos may not be downloadable due to restrictions
- Try a different video URL

#### General Issues
- Check the mod logs for detailed error messages
- Try switching to a different music source
- Restart the BackSpeaker app if needed

### File Locations
- **Music Folder**: `[Game Directory]/Mods/BackSpeaker/Music/`
- **YouTube Cache**: `[Game Directory]/Mods/BackSpeaker/Cache/YouTube/`
- **Logs**: Check Unity console or mod logging system

## Developer Information

### Architecture
The music source system uses a provider pattern:
- `IMusicSourceProvider` interface for different sources
- `TrackLoader` manages providers and source switching
- `MusicSourceSelector` handles UI and user interaction

### YouTube Integration
- Uses **YoutubeExplode** library (pure C#, no external dependencies)
- Automatically extracts the best audio-only stream
- Supports MP4 and WebM containers
- Built-in caching system with automatic cleanup

### Adding New Sources
To add a new music source:
1. Implement `IMusicSourceProvider`
2. Register in `TrackLoader.InitializeMusicProviders()`
3. Add UI panel component
4. Update `MusicSourceSelector` button handling

### Configuration
Each provider supports configuration through the `GetConfiguration()` and `ApplyConfiguration()` methods. This allows for future customization options.

## Changelog

### Version 1.1.0
- **BREAKING CHANGE**: Replaced yt-dlp dependency with YoutubeExplode library
- YouTube downloads now work without any external installation
- Improved download reliability and error handling
- Better integration with Unity's async/await system
- Updated help documentation

### Version 1.0.0
- Initial implementation of multi-source music system
- Added local folder music provider
- Added YouTube music provider with yt-dlp integration
- Created unified UI for source selection
- Maintained backward compatibility with existing jukebox system

# Music Sources Documentation

This document explains how the Back Speaker Mod handles different music sources and audio loading.

## 🎵 Audio Sources

### 1. Local Folder Music Provider

**Real Audio Playback with Embedded AudioImportLib**

The mod now includes AudioImportLib embedded as a resource, providing professional-grade audio decoding without requiring separate installations.

**How it works:**
- AudioImportLib.dll is embedded in the mod as a resource
- Loaded at runtime using reflection and Assembly.Load()
- Uses BASS audio library internally for robust format support
- Supports: MP3, WAV, OGG, FLAC, AIFF, WMA, M4A

**Technical Implementation:**
```csharp
// Embedded loading approach
EmbeddedAssemblyLoader.LoadAudioImportLib() // Load embedded DLL
AudioImportLib.API.LoadAudioClip(filePath, true) // via reflection
```

**Benefits:**
- ✅ Real audio decoding and playback
- ✅ No dependency installation required  
- ✅ Works in IL2CPP/MelonLoader environment
- ✅ Wide format support via BASS library
- ✅ Single-file distribution

// ... existing code ... 