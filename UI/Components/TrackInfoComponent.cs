using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Modules;
using BackSpeakerMod.UI.Helpers;

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
        private Text? nowPlayingText;
        private Text? artistText;
        private Text? albumText;
        private Text? sourceText;
        
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
            // Position album art in left portion, properly constrained
            artRect.anchorMin = new Vector2(0.05f, 0.05f);  // 5% from left, 5% from bottom
            artRect.anchorMax = new Vector2(0.15f, 0.85f);  // 15% from left, 85% from bottom
            artRect.offsetMin = Vector2.zero;
            artRect.offsetMax = Vector2.zero;
            artRect.anchoredPosition = Vector2.zero;
            // artRect.sizeDelta = new Vector2(100, 100); // 100x100px
            
            // Album art background
            var background = albumArtContainer.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Album art image
            // albumArt = albumArtContainer.AddComponent<Image>();
            
            // Default album art with musical notes
            // CreateDefaultAlbumArt(albumArtContainer);
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
            if (_themedArt == null) return;
            try{
                var currentSource = manager?.GetCurrentMusicSource();
                if (currentSource == null) return;

                var themedArtImage = _themedArt.GetComponent<Image>();
                themedArtImage.sprite = Utils.ResourceLoader.LoadEmbeddedSprite($"BackSpeakerMod.EmbeddedResources.AlbumArt.{currentSource?.ToString().ToLower()}.png");
                themedArtImage.color = new Color(1f, 1f, 1f, 1f);
            } catch (System.Exception ex) {
                LoggingSystem.Error($"Error updating themed album art: {ex.Message}", "UI");
                // fallback to default album art
                // already showing default album art, so do nothing
            }
        }
        
        private void CreateTrackDetails()
        {
            var detailsContainer = new GameObject("TrackDetails");
            detailsContainer.transform.SetParent(this.transform, false);
            
            var detailsRect = detailsContainer.AddComponent<RectTransform>();
            // Position track details to the right of album art
            detailsRect.anchorMin = new Vector2(0.2f, 0.05f);   // Start after album art
            detailsRect.anchorMax = new Vector2(0.95f, 0.85f);  // Almost full width, proper height
            detailsRect.offsetMin = Vector2.zero;
            detailsRect.offsetMax = Vector2.zero;
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