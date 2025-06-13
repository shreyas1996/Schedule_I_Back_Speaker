using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Modules;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI.Components
{
    /// <summary>
    /// Track info component following design specifications
    /// Layout: Album Art (120x120px) + Track Details (Now Playing, Artist, Album, Source)
    /// </summary>
    public class TrackInfoComponent : MonoBehaviour
    {
        private BackSpeakerManager? manager;
        
        // UI Elements
        private GameObject? _themedArt;
        private GameObject? _defaultArt;
        private Image? _thumbnailImage;
        private Text? nowPlayingText;
        private Text? artistText;
        private Text? albumText;
        private Text? sourceText;
        
        // Current track info for thumbnail loading
        private string? currentTrackId;
        private MusicSourceType currentSourceType;
        
        public TrackInfoComponent() : base() { }
        
        public void Setup(BackSpeakerManager manager)
        {
            this.manager = manager;
            CreateTrackInfoLayout();
        }
        
        private void CreateTrackInfoLayout()
        {
            // Album Art Container (120x120px on left side)
            CreateAlbumArt();
            
            // Track Details Container (right side)
            CreateTrackDetails();
        }
        
        private void CreateAlbumArt()
        {
            var albumArtContainer = new GameObject("AlbumArtContainer");
            albumArtContainer.transform.SetParent(this.transform, false);
            
            var artRect = albumArtContainer.AddComponent<RectTransform>();
            // Position album art on the LEFT side as a fixed square
            artRect.anchorMin = new Vector2(0.05f, 0.05f);        // Left side anchor
            artRect.anchorMax = new Vector2(0.15f, 0.85f);        // Left side anchor, full height
            // artRect.offsetMin = new Vector2(10f, 10f);      // 10px padding from left and bottom
            // artRect.offsetMax = new Vector2(130f, -10f);    // 120px width + 10px padding, 10px from top
            artRect.offsetMin = Vector2.zero;
            artRect.offsetMax = Vector2.zero;
            artRect.anchoredPosition = Vector2.zero;
            
            // Album art background
            var background = albumArtContainer.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Create themed album art (will be updated with thumbnails for YouTube)
            CreateThemedAlbumArt(albumArtContainer);
        }

        private void CreateThemedAlbumArt(GameObject container)
        {
            // this function will create a themed album art image from EmbeddedResources based on the current music source
            var currentSource = manager?.GetCurrentMusicSource();
            if (currentSource == null) return;

            try {
                _themedArt = new GameObject("ThemedAlbumArt");
                _themedArt.transform.SetParent(container.transform, false);
                
                var themedArtRect = _themedArt.AddComponent<RectTransform>();
                themedArtRect.anchorMin = Vector2.zero;
                themedArtRect.anchorMax = Vector2.one;
                themedArtRect.offsetMin = Vector2.zero;
                themedArtRect.offsetMax = Vector2.zero;
            
                var themedArtImage = _themedArt.AddComponent<Image>();
                themedArtImage.sprite = Utils.ResourceLoader.LoadEmbeddedSprite($"BackSpeakerMod.EmbeddedResources.AlbumArt.{currentSource?.ToString().ToLower()}.png");
                themedArtImage.color = new Color(1f, 1f, 1f, 1f);
                
                // Store reference for thumbnail updates
                _thumbnailImage = themedArtImage;

                var themedArtText = _themedArt.AddComponent<Text>();
                themedArtText.text = "♫";
                FontHelper.SetSafeFont(themedArtText);
                themedArtText.fontSize = 24;
            } catch (System.Exception ex) {
                LoggingSystem.Error($"Error creating themed album art: {ex.Message}", "UI");
                // fallback to default album art
                CreateDefaultAlbumArt(container);
            }
        }
        
        private void CreateDefaultAlbumArt(GameObject container)
        {
            // Create musical note text overlay
            _defaultArt = new GameObject("MusicalNotes");
            _defaultArt.transform.SetParent(container.transform, false);
            
            var noteRect = _defaultArt.AddComponent<RectTransform>();
            noteRect.anchorMin = Vector2.zero;
            noteRect.anchorMax = Vector2.one;
            noteRect.offsetMin = Vector2.zero;
            noteRect.offsetMax = Vector2.zero;
            
            var textComponent = _defaultArt.AddComponent<Text>();
            textComponent.text = "🎵 🎶 ��";
            FontHelper.SetSafeFont(textComponent);
            textComponent.fontSize = 24;
            textComponent.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.fontStyle = FontStyle.Bold;
        }

        private void UpdateThemedAlbumArt()
        {
            if (_themedArt == null || _thumbnailImage == null) return;
            
            try{
                var currentSource = manager?.GetCurrentMusicSource();
                if (currentSource == null) return;

                // Check if we need to load a YouTube thumbnail
                if (currentSource == MusicSourceType.YouTube)
                {
                    LoadYouTubeThumbnail();
                }
                else
                {
                    // Use themed album art for non-YouTube sources
                    var themedSprite = Utils.ResourceLoader.LoadEmbeddedSprite($"BackSpeakerMod.EmbeddedResources.AlbumArt.{currentSource?.ToString().ToLower()}.png");
                    if (themedSprite != null)
                    {
                        _thumbnailImage.sprite = themedSprite;
                        _thumbnailImage.color = new Color(1f, 1f, 1f, 1f);
                    }
                }
            } catch (System.Exception ex) {
                LoggingSystem.Error($"Error updating themed album art: {ex.Message}", "UI");
                // fallback to default album art
                // already showing default album art, so do nothing
            }
        }
        
        private void LoadYouTubeThumbnail()
        {
            try
            {
                // Get current YouTube song details
                var session = manager?.GetSession(MusicSourceType.YouTube);
                var currentSong = session?.GetCurrentYouTubeSong();
                
                if (currentSong != null && !string.IsNullOrEmpty(currentSong.thumbnail))
                {
                    var trackId = currentSong.GetVideoId();
                    
                    // Only load if this is a different track
                    if (trackId != currentTrackId)
                    {
                        currentTrackId = trackId;
                        LoggingSystem.Debug($"Loading YouTube thumbnail for: {currentSong.title}", "TrackInfo");
                        
                        //TODO: Properly load the thumbnail from the cache or download it
                        
                        // Fallback to YouTube themed art
                        LoadFallbackYouTubeArt();
                    }
                }
                else
                {
                    // No thumbnail available, use YouTube themed art
                    LoadFallbackYouTubeArt();
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Error loading YouTube thumbnail: {ex.Message}", "TrackInfo");
                LoadFallbackYouTubeArt();
            }
        }
        
        private void LoadFallbackYouTubeArt()
        {
            if (_thumbnailImage == null) return;
            
            try
            {
                var youtubeSprite = Utils.ResourceLoader.LoadEmbeddedSprite("BackSpeakerMod.EmbeddedResources.AlbumArt.youtube.png");
                if (youtubeSprite != null)
                {
                    _thumbnailImage.sprite = youtubeSprite;
                    _thumbnailImage.color = new Color(1f, 1f, 1f, 1f);
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Warning($"Error loading fallback YouTube art: {ex.Message}", "TrackInfo");
            }
        }
        
        private void CreateTrackDetails()
        {
            var detailsContainer = new GameObject("TrackDetails");
            detailsContainer.transform.SetParent(this.transform, false);
            
            var detailsRect = detailsContainer.AddComponent<RectTransform>();
            // Position track details to the RIGHT of album art (150px total space for art + padding)
            detailsRect.anchorMin = new Vector2(0.2f, 0.05f);        // Start from left
            detailsRect.anchorMax = new Vector2(0.95f, 0.85f);        // Full width and height
            // detailsRect.offsetMin = new Vector2(150f, 10f);     // 130px album art + 20px padding
            // detailsRect.offsetMax = new Vector2(-10f, -10f);    // 10px right and top padding
            detailsRect.offsetMin = new Vector2(0, 0);
            detailsRect.offsetMax = new Vector2(0, 0);

            detailsRect.anchoredPosition = Vector2.zero;
            
            // Create text lines with relative positioning
            float lineSpacing = 0.18f; // 20% of container height per line
            float startY = 0.9f;      // Start near top
            
            // 🎵 Now Playing: "Track Name"
            nowPlayingText = CreateTextLine(detailsContainer, "NowPlaying", 
                "🎵 No Track Selected", startY, 15, FontStyle.Bold);
            
            // 🎤 Artist: "Artist Name"
            artistText = CreateTextLine(detailsContainer, "Artist", 
                "🎤 Artist: Unknown", startY - lineSpacing, 12, FontStyle.Normal);
            
            // 💿 Album: "Album Name"
            albumText = CreateTextLine(detailsContainer, "Album", 
                "💿 Album: Unknown", startY - (lineSpacing * 2), 11, FontStyle.Normal);
            
            // 📊 Source: "Music Source"
            sourceText = CreateTextLine(detailsContainer, "Source", 
                "📊 Source: None", startY - (lineSpacing * 3), 11, FontStyle.Normal);
        }
        
        private Text CreateTextLine(GameObject parent, string name, string text, float yPercent, int fontSize, FontStyle fontStyle)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            // Use relative positioning based on percentage
            textRect.anchorMin = new Vector2(0f, yPercent - 0.15f);  // Text line height
            textRect.anchorMax = new Vector2(1f, yPercent);           // Full width
            textRect.offsetMin = new Vector2(5f, 0f);
            textRect.offsetMax = new Vector2(-5f, 0f);
            textRect.anchoredPosition = Vector2.zero;
            
            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            FontHelper.SetSafeFont(textComponent);
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleLeft;
            textComponent.fontStyle = fontStyle;
            textComponent.horizontalOverflow = HorizontalWrapMode.Overflow; // Allow text to show even if long
            textComponent.verticalOverflow = VerticalWrapMode.Overflow;
            
            return textComponent;
        }
        
        public void UpdateDisplay()
        {
            if (manager == null) return;
            
            try
            {
                var currentTrackInfo = manager.GetCurrentTrackInfo();
                var currentArtist = manager.GetCurrentArtistInfo();
                var isPlaying = manager.IsPlaying;
                var isAudioReady = manager.IsAudioReady();
                var headphonesAttached = manager.AreHeadphonesAttached();
                var trackCount = manager.GetTrackCount();

                UpdateThemedAlbumArt(); // update the themed album art based on the current music source
                
                // Priority 1: Check if headphones are attached
                if (!headphonesAttached)
                {
                    nowPlayingText!.text = "🎧 Headphones Required";
                    artistText!.text = "🔌 Please attach headphones to start";
                    albumText!.text = "💡 Use the attach button below";
                    sourceText!.text = "📊 System: Waiting for headphones";
                    
                    // Orange color for attention
                    if (nowPlayingText != null)
                    {
                        nowPlayingText.color = new Color(1f, 0.6f, 0.2f, 1f);
                    }
                    return;
                }
                
                // Priority 2: Check if audio system is ready
                if (!isAudioReady)
                {
                    nowPlayingText!.text = "⚠️ Audio System Not Ready";
                    artistText!.text = "🔧 Please wait while initializing";
                    albumText!.text = "⏳ This may take a moment";
                    sourceText!.text = "📊 System: Initializing";
                    
                    // Yellow color for initialization
                    if (nowPlayingText != null)
                    {
                        nowPlayingText.color = new Color(1f, 1f, 0.4f, 1f);
                    }
                    return;
                }
                
                // Priority 3: Check if we have tracks
                if (trackCount == 0)
                {
                    nowPlayingText!.text = "📂 No Tracks Loaded";
                    artistText!.text = "🔄 Use the reload button to load music";
                    albumText!.text = "🎵 Switch tabs to try different sources";
                    sourceText!.text = "📊 System: Ready but no tracks";
                    
                    // Gray color for no content
                    if (nowPlayingText != null)
                    {
                        nowPlayingText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    }
                    return;
                }
                
                // Priority 4: Show actual track info
                if (!string.IsNullOrEmpty(currentTrackInfo) && currentTrackInfo != "No Track" && currentTrackInfo != "Unknown Track")
                {
                    // Display real track information
                    var playState = isPlaying ? "Now Playing" : "Paused";
                    nowPlayingText!.text = $"{playState}: \"{currentTrackInfo}\"";
                    
                    // Display artist info - show the actual artist from track data
                    if (!string.IsNullOrEmpty(currentArtist) && currentArtist != "Unknown Artist")
                    {
                        artistText!.text = $"🎤 Artist: {currentArtist}";
                    }
                    else
                    {
                        artistText!.text = "🎤 Artist: Unknown";
                    }
                    
                    // Show track position
                    var currentIndex = manager.CurrentTrackIndex;
                    albumText!.text = $"💿 Track {currentIndex + 1} of {trackCount}";
                    
                    // Show current music source
                    var currentSource = manager.GetCurrentMusicSource();
                    var sourceDisplay = currentSource switch
                    {
                        MusicSourceType.Jukebox => "In-Game Jukebox",
                        MusicSourceType.LocalFolder => "Local Music",
                        MusicSourceType.YouTube => "YouTube Music",
                        _ => "Unknown Source"
                    };
                    sourceText!.text = $"📊 Source: {sourceDisplay}";
                    
                    // Green when playing, white when paused
                    if (nowPlayingText != null)
                    {
                        nowPlayingText.color = isPlaying ? 
                            new Color(0.4f, 1f, 0.4f, 1f) :  // Green when playing
                            Color.white;                      // White when paused/stopped
                    }
                }
                else
                {
                    // Fallback case
                    nowPlayingText!.text = "🎵 Ready to Play";
                    artistText!.text = "🎤 Select a track from playlist";
                    albumText!.text = $"💿 {trackCount} tracks available";
                    sourceText!.text = "📊 System: Ready";
                    
                    if (nowPlayingText != null)
                    {
                        nowPlayingText.color = Color.white;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Track info update failed: {ex.Message}", "UI");
                nowPlayingText!.text = "🎵 Error loading track info";
                artistText!.text = "🎤 Error";
                albumText!.text = "💿 Error";
                sourceText!.text = "📊 Error";
                
                if (nowPlayingText != null)
                {
                    nowPlayingText.color = new Color(1f, 0.4f, 0.4f, 1f); // Red for error
                }
            }
        }
    }
} 