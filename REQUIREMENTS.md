# BackSpeaker Mod - Requirements Document

## Core Concept
A Unity game mod that provides an in-game music player with headphone attachment requirement. Users can listen to music from multiple sources through a modern UI interface.

## Key Requirements

### 1. Player Detection & Lifecycle
- **Player Detection**: Detect when player enters main scene
- **System Loading**: Load backend and frontend systems when player is detected
- **System Unloading**: Shutdown and cleanup everything when:
  - Player leaves main scene
  - Player is lost/disconnected
  - Mod is disabled

### 2. Headphone System
- **Attachment Requirement**: Headphones must be attached to play any audio
- **Visual Attachment**: Headphones appear on player character when attached
- **Detachment Handling**: 
  - Stop all audio immediately when headphones are detached
  - Prevent audio playback until headphones are reattached
  - Provide in-app button for manual detachment
- **Audio Isolation**: All mod audio only plays through headphones

### 3. Music Sources
Three primary music sources:
- **Jukebox**: In-game audio clips from the game's jukebox system
- **LocalFolder**: Local audio files from user's computer
- **YouTube**: Downloaded YouTube videos/music

### 4. Audio Processing Pipeline
- **YouTube**: yt-dlp + ffmpeg + ffprobe for downloading and processing
- **LocalFolder**: audioimportlib for loading and playing local files
- **Jukebox**: Direct access to game's audio clips
- **Playback**: All sources use audioimportlib for consistent playback

### 5. User Interface
- **Tab System**: Separate tabs for each music source (Jukebox, LocalFolder, YouTube)
- **Music Controls**: Standard controls (play, pause, stop, next, previous, shuffle, repeat)
- **Playlist Management**:
  - Create new playlists
  - Edit existing playlists
  - Delete playlists
  - Add/remove tracks from playlists
- **Track Management**:
  - Display track details (title, artist, duration, status)
  - Show if track exists and is playable
  - Queue management and upcoming tracks display
- **Performance**: Smooth, responsive UI with proper loading states and refresh cycles

### 6. Playlist System
- **Per-Source Playlists**: Each music source can have multiple playlists
- **Playlist Popups**: Modal dialogs for playlist operations
- **Track Validation**: Check if tracks exist before attempting playback
- **Persistence**: Save playlists and settings between sessions

### 7. Audio Playback Rules
- **Headphone Requirement**: No audio without headphones attached
- **Scene Exit**: Stop all audio when leaving main scene
- **Headphone Detachment**: Immediate audio stop
- **Source Switching**: Seamless transition between different music sources
- **Error Handling**: Graceful handling of missing files, network issues, etc.

### 8. Technical Architecture
- **Wrapper System**: Use existing S1Wrapper for game integration
- **Modular Backend**: Clean separation of concerns
- **Event-Driven**: Reactive system with proper event handling
- **Resource Management**: Efficient memory and resource usage
- **Error Recovery**: Robust error handling and recovery mechanisms

## Success Criteria
1. ✅ Headphones attach/detach visually and functionally
2. ✅ Music plays only when headphones are attached
3. ✅ All three music sources work reliably
4. ✅ Playlists can be created, edited, and managed
5. ✅ UI is responsive and performs well
6. ✅ System loads/unloads properly with player detection
7. ✅ Audio stops immediately on scene exit or headphone detachment
8. ✅ Settings and playlists persist between sessions

## Non-Requirements
- External music streaming services (Spotify, Apple Music, etc.)
- Complex audio effects or equalizers
- Social features or sharing
- Multiple simultaneous audio streams
- Audio recording or editing capabilities 