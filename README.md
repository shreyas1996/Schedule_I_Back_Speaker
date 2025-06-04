# BackSpeaker Mod for Schedule I

A Unity C# mod that provides background music functionality through virtual headphones in the Schedule I game.

## Features

🎵 **Multiple Music Sources**:
- **🎮 In-Game Jukebox**: Load music from game's jukebox objects for testing
- **📁 Local Music Folder**: Play your own music files (MP3, WAV, OGG, M4A, AAC)
- **📺 YouTube Music**: Download and stream music directly from YouTube URLs

🎧 **Virtual Headphones**: Immersive audio experience that integrates with the game world

🎮 **Game Integration**: 
- Seamless integration with Schedule I's audio system
- Compatible with existing game audio and effects
- Works with both IL2CPP and Mono backends

✨ **Modern UI**: Clean, intuitive interface with source switching and playlist management

## Quick Start

1. **Download** the latest release from the releases page
2. **Extract** to your game's mod directory
3. **Launch** Schedule I and enjoy your music!

## Music Sources

### YouTube Music 📺
- **No installation required** - Built-in YoutubeExplode library
- Paste any YouTube URL and download audio instantly
- Automatic caching for offline playback
- Supports all standard YouTube URL formats

### Local Music Files 📁
- Supports MP3, WAV, OGG, M4A, AAC formats
- Auto-creates music folder: `[Game]/Mods/BackSpeaker/Music/`
- Simple drag-and-drop workflow
- Instant refresh and reload

### In-Game Jukebox 🎮
- Testing integration with game audio
- Loads from existing jukebox objects
- Perfect for mod development and debugging

## 🎵 Audio Support

### Local Audio Files
- **Location**: Create a `MusicPlaylist` folder in your game's root directory
- **Formats**: MP3, WAV, OGG, FLAC, AIFF, WMA, M4A (powered by embedded AudioImportLib)
- **Detection**: Automatically scans for supported audio files
- **Playback**: Real audio decoding and playback (no placeholders!)

**Note**: AudioImportLib is embedded in the mod - no separate installation required! 🎉

## Installation

### For Users

1. Download the latest release ZIP file
2. Extract to your Schedule I game directory
3. The mod will auto-create necessary folders on first run

### For Developers

**Prerequisites**:
- Unity 2021.3 LTS or compatible version
- .NET Standard 2.1 support
- IL2CPP backend support

**Dependencies**:
```xml
<!-- Add to your .csproj file -->
<PackageReference Include="YoutubeExplode" Version="6.3.16" />
```

**Build Instructions**:
1. Clone this repository
2. Install YoutubeExplode NuGet package
3. Build with your preferred Unity IL2CPP setup
4. Copy output to game mod directory

## Technical Architecture

### Provider Pattern
- `IMusicSourceProvider` interface for extensible music sources
- `TrackLoader` manages provider lifecycle and switching
- `MusicSourceSelector` handles UI and user interactions

### Unity Integration
- **IL2CPP Compatible**: Works with Unity's IL2CPP backend
- **Async/Await**: Modern async patterns for smooth operation
- **Memory Efficient**: Smart caching and cleanup systems
- **Cross-Platform**: Windows, macOS, Linux support

### Audio Pipeline
```
YouTube URL → YoutubeExplode → Audio Stream → Unity AudioClip → Game Audio System
Local Files → Unity AudioLoader → AudioClip → Game Audio System  
Jukebox → Game Objects → AudioClip → Game Audio System
```

## Configuration

### User Settings
- Music source selection (persistent across sessions)
- Volume controls and audio mixing
- Cache management (automatic cleanup)
- Playlist organization

### Developer Settings
```csharp
// Example configuration
var config = new Dictionary<string, object>
{
    ["MaxCacheSize"] = 50,
    ["AudioQuality"] = "High",
    ["AutoCleanup"] = true
};
provider.ApplyConfiguration(config);
```

## Troubleshooting

### Common Issues

**YouTube downloads not working:**
- Check internet connection
- Verify URL format (youtube.com/watch?v=... or youtu.be/...)
- Some videos may be region-restricted

**Local files not loading:**
- Ensure files are in supported formats (MP3, WAV, OGG, M4A, AAC)
- Check file permissions
- Try the "Refresh Tracks" button

**Audio not playing:**
- Verify game audio settings
- Check mod is properly loaded
- Try switching between music sources

### Debug Information
Enable detailed logging in `Configuration/BackSpeakerConfig.json`:
```json
{
  "LogLevel": "Debug",
  "EnableAudioLogging": true
}
```

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

### Development Setup
1. Fork the repository
2. Create a feature branch
3. Install dependencies: `dotnet restore`
4. Make your changes
5. Test with Unity IL2CPP build
6. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Changelog

### Version 1.1.0 (Latest)
- ✨ **NEW**: YoutubeExplode integration - no more external dependencies!
- 🔧 **IMPROVED**: YouTube downloads now work out-of-the-box
- 🐛 **FIXED**: IL2CPP compatibility issues
- 📚 **UPDATED**: Comprehensive documentation and help system

### Version 1.0.0
- 🎉 Initial release with multi-source music system
- 🎧 Virtual headphone functionality
- 📁 Local folder music support
- 🎮 In-game jukebox integration

## Support

- **Documentation**: See [MUSIC_SOURCES.md](MUSIC_SOURCES.md) for detailed usage guide
- **Issues**: Report bugs on the GitHub Issues page
- **Community**: Join discussions in the Schedule I modding community

---

**Made with ❤️ for the Schedule I community**