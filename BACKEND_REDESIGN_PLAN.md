# BackSpeaker Backend Redesign Plan

## 🎯 **Goal**
Create a clean, modular backend architecture from scratch that properly integrates with the redesigned UI and follows the core requirements.

## 📋 **Analysis Summary**

### What We Have (Existing)
- ✅ **S1Wrapper**: Game integration wrappers (keep)
- ✅ **UIWrapper**: UI component system (keep)
- ✅ **Utils**: Helper utilities (keep)
- ✅ **Documentation**: Comprehensive docs (keep)
- ❌ **Core**: Complex, tightly coupled backend (replace)

### What We Need (New Backend)
- 🎯 **Player Detection & Lifecycle Management**
- 🎧 **Headphone Attachment System**
- 🎵 **Music Source Management** (Jukebox, LocalFolder, YouTube)
- 🎛️ **Audio Playback Control**
- 📋 **Playlist Management**
- 🔄 **Event-Driven Architecture**
- 💾 **Data Persistence**

## 🏗️ **New Backend Architecture**

### Directory Structure
```
NewBackend/
├── BackSpeakerCore.cs              # Main entry point & API
├── Lifecycle/
│   ├── PlayerDetector.cs           # Player detection
│   ├── SceneManager.cs             # Scene lifecycle
│   └── SystemLifecycle.cs          # System startup/shutdown
├── Audio/
│   ├── AudioEngine.cs              # Core audio playback
│   ├── AudioController.cs          # Playback controls
│   └── AudioValidator.cs           # Audio validation
├── Sources/
│   ├── ISource.cs                  # Source interface
│   ├── JukeboxSource.cs           # Jukebox integration
│   ├── LocalFolderSource.cs       # Local files
│   ├── YouTubeSource.cs           # YouTube integration
│   └── SourceManager.cs           # Source coordination
├── Headphones/
│   ├── HeadphoneController.cs     # Headphone logic
│   ├── HeadphoneRenderer.cs       # Visual rendering
│   └── HeadphoneValidator.cs      # Attachment validation
├── Playlists/
│   ├── PlaylistEngine.cs          # Playlist operations
│   ├── PlaylistStorage.cs         # Persistence
│   └── PlaylistValidator.cs       # Validation
├── Events/
│   ├── EventBus.cs                # Central event system
│   └── EventTypes.cs              # Event definitions
└── Data/
    ├── Models/                    # Data models
    ├── Storage/                   # Persistence layer
    └── Cache/                     # Caching system
```

## 🔄 **Core Systems Design**

### 1. BackSpeakerCore (Main API)
```csharp
public class BackSpeakerCore
{
    // Lifecycle
    public bool Initialize()
    public void Shutdown()
    public bool IsReady { get; }
    
    // Audio Control
    public void Play()
    public void Pause()
    public void Stop()
    public void Next()
    public void Previous()
    
    // Source Management
    public void SetSource(SourceType type)
    public SourceType CurrentSource { get; }
    
    // Headphone Control
    public bool AttachHeadphones()
    public void DetachHeadphones()
    public bool AreHeadphonesAttached { get; }
    
    // Events
    public event Action<TrackInfo> OnTrackChanged
    public event Action<bool> OnPlayStateChanged
    public event Action<bool> OnHeadphonesChanged
}
```

### 2. Event-Driven Architecture
```csharp
public class EventBus
{
    public void Publish<T>(T eventData)
    public void Subscribe<T>(Action<T> handler)
    public void Unsubscribe<T>(Action<T> handler)
}

// Event Types
public class TrackChangedEvent { TrackInfo Track }
public class PlayStateChangedEvent { bool IsPlaying }
public class HeadphonesChangedEvent { bool Attached }
public class SourceChangedEvent { SourceType Source }
```

### 3. Headphone System
```csharp
public class HeadphoneController
{
    public bool Attach()
    public void Detach()
    public bool IsAttached { get; }
    public event Action<bool> OnAttachmentChanged
}

public class HeadphoneValidator
{
    public bool CanPlayAudio() // Must have headphones
    public void ValidatePlayback()
}
```

### 4. Audio Engine
```csharp
public class AudioEngine
{
    public void Play(AudioClip clip)
    public void Pause()
    public void Stop()
    public void SetVolume(float volume)
    public float Progress { get; }
    public bool IsPlaying { get; }
}

public class AudioController
{
    public void PlayTrack(TrackInfo track)
    public void Next()
    public void Previous()
    public void Shuffle()
    public void Repeat()
}
```

### 5. Source Management
```csharp
public interface ISource
{
    SourceType Type { get; }
    Task<List<TrackInfo>> LoadTracksAsync()
    bool IsAvailable()
    Task<AudioClip> GetAudioClipAsync(TrackInfo track)
}

public class SourceManager
{
    public void RegisterSource(ISource source)
    public void SetActiveSource(SourceType type)
    public ISource GetActiveSource()
    public List<SourceType> GetAvailableSources()
}
```

### 6. Playlist System
```csharp
public class PlaylistEngine
{
    public void CreatePlaylist(string name, SourceType source)
    public void DeletePlaylist(string id)
    public void AddTrack(string playlistId, TrackInfo track)
    public void RemoveTrack(string playlistId, string trackId)
    public List<PlaylistInfo> GetPlaylists(SourceType source)
}

public class PlaylistStorage
{
    public void SavePlaylist(PlaylistInfo playlist)
    public PlaylistInfo LoadPlaylist(string id)
    public void DeletePlaylist(string id)
}
```

## 🔧 **Implementation Strategy**

### Phase 1: Core Infrastructure (Week 1)
1. ✅ Create NewBackend folder structure
2. ✅ Implement EventBus system
3. ✅ Create base interfaces (ISource, etc.)
4. ✅ Implement BackSpeakerCore entry point
5. ✅ Basic logging and error handling

### Phase 2: Player & Lifecycle (Week 1)
1. ✅ PlayerDetector implementation
2. ✅ SceneManager for scene transitions
3. ✅ SystemLifecycle for startup/shutdown
4. ✅ Integration with existing S1Wrapper

### Phase 3: Headphone System (Week 2)
1. ✅ HeadphoneController logic
2. ✅ HeadphoneRenderer visual system
3. ✅ HeadphoneValidator rules
4. ✅ Integration with existing headphone assets

### Phase 4: Audio Engine (Week 2)
1. ✅ AudioEngine core playback
2. ✅ AudioController user controls
3. ✅ Integration with Unity AudioSource
4. ✅ AudioValidator for headphone checks

### Phase 5: Music Sources (Week 3)
1. ✅ JukeboxSource (simplest)
2. ✅ LocalFolderSource with audioimportlib
3. ✅ YouTubeSource with yt-dlp integration
4. ✅ SourceManager coordination

### Phase 6: Playlist System (Week 3)
1. ✅ PlaylistEngine operations
2. ✅ PlaylistStorage persistence
3. ✅ Integration with UI components
4. ✅ Validation and error handling

### Phase 7: UI Integration (Week 4)
1. ✅ Update UI components to use new backend
2. ✅ Event binding and data flow
3. ✅ Performance optimization
4. ✅ Testing and debugging

### Phase 8: Migration & Cleanup (Week 4)
1. ✅ Migrate any needed data from old system
2. ✅ Remove old Core/ directory
3. ✅ Update documentation
4. ✅ Final testing and polish

## 🎯 **Key Design Principles**

### 1. **Single Responsibility**
- Each class has one clear purpose
- No god classes or massive files
- Clear separation of concerns

### 2. **Event-Driven**
- Loose coupling between components
- Reactive UI updates
- Clean async operations

### 3. **Testable**
- Dependency injection where needed
- Interface-based design
- Clear contracts

### 4. **Performance**
- Lazy loading of resources
- Efficient caching
- Minimal memory footprint

### 5. **Robust**
- Comprehensive error handling
- Graceful degradation
- Proper cleanup

## 🔗 **Integration Points**

### With Existing Code
- ✅ **S1Wrapper**: Use for game integration
- ✅ **UIWrapper**: Use for UI components
- ✅ **Utils**: Use existing utilities
- ✅ **BackSpeakerMod.cs**: Update to use new backend

### With Unity
- ✅ **AudioSource**: For audio playback
- ✅ **MonoBehaviour**: For Unity lifecycle
- ✅ **Coroutines**: For async operations
- ✅ **ScriptableObject**: For configuration

### With External Libraries
- ✅ **audioimportlib**: For local file loading
- ✅ **yt-dlp**: For YouTube downloads
- ✅ **MelonLoader**: For mod framework

## 📊 **Success Metrics**

### Technical
- ✅ < 500ms startup time
- ✅ < 100ms UI response time
- ✅ < 50MB memory usage
- ✅ Zero memory leaks
- ✅ Graceful error recovery

### Functional
- ✅ All three sources work reliably
- ✅ Headphone attachment works visually
- ✅ Audio stops immediately on detachment
- ✅ Playlists persist correctly
- ✅ UI updates smoothly

### User Experience
- ✅ Intuitive interface
- ✅ Fast loading times
- ✅ Clear error messages
- ✅ Responsive controls
- ✅ Stable performance

## 🚀 **Next Steps**

1. **Create NewBackend folder structure**
2. **Implement core interfaces and EventBus**
3. **Start with PlayerDetector and lifecycle**
4. **Build incrementally with testing**
5. **Integrate with UI as we go**

This plan ensures we build a solid, maintainable backend that meets all requirements while being properly integrated with the existing UI system. 