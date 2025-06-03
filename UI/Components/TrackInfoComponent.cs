using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;

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
        private Image? albumArt;
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
            artRect.anchorMin = new Vector2(0.05f, 0.1f);  // 5% from left, 10% from bottom
            artRect.anchorMax = new Vector2(0.35f, 0.9f);  // 35% from left, 90% from bottom
            artRect.offsetMin = Vector2.zero;
            artRect.offsetMax = Vector2.zero;
            artRect.anchoredPosition = Vector2.zero;
            
            // Album art background
            var background = albumArtContainer.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // Album art image
            albumArt = albumArtContainer.AddComponent<Image>();
            
            // Default album art with musical notes
            CreateDefaultAlbumArt(albumArtContainer);
        }
        
        private void CreateDefaultAlbumArt(GameObject container)
        {
            // Create musical note text overlay
            var noteText = new GameObject("MusicalNotes");
            noteText.transform.SetParent(container.transform, false);
            
            var noteRect = noteText.AddComponent<RectTransform>();
            noteRect.anchorMin = Vector2.zero;
            noteRect.anchorMax = Vector2.one;
            noteRect.offsetMin = Vector2.zero;
            noteRect.offsetMax = Vector2.zero;
            
            var textComponent = noteText.AddComponent<Text>();
            textComponent.text = "🎵 🎶 🎵";
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 24;
            textComponent.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.fontStyle = FontStyle.Bold;
        }
        
        private void CreateTrackDetails()
        {
            var detailsContainer = new GameObject("TrackDetails");
            detailsContainer.transform.SetParent(this.transform, false);
            
            var detailsRect = detailsContainer.AddComponent<RectTransform>();
            // Position track details to the right of album art
            detailsRect.anchorMin = new Vector2(0.4f, 0.1f);   // Start after album art
            detailsRect.anchorMax = new Vector2(0.95f, 0.9f);  // Almost full width, proper height
            detailsRect.offsetMin = Vector2.zero;
            detailsRect.offsetMax = Vector2.zero;
            detailsRect.anchoredPosition = Vector2.zero;
            
            // Create text lines with relative positioning
            float lineSpacing = 0.2f; // 20% of container height per line
            float startY = 0.8f;      // Start near top
            
            // 🎵 Now Playing: "Track Name"
            nowPlayingText = CreateTextLine(detailsContainer, "NowPlaying", 
                "🎵 No Track Selected", startY, 14, FontStyle.Bold);
            
            // 🎤 Artist: "Artist Name"
            artistText = CreateTextLine(detailsContainer, "Artist", 
                "🎤 Artist: Unknown", startY - lineSpacing, 12, FontStyle.Normal);
            
            // 💿 Album: "Album Name"
            albumText = CreateTextLine(detailsContainer, "Album", 
                "💿 Album: Unknown", startY - (lineSpacing * 2), 12, FontStyle.Normal);
            
            // 📊 Source: "Music Source"
            sourceText = CreateTextLine(detailsContainer, "Source", 
                "📊 Source: None", startY - (lineSpacing * 3), 12, FontStyle.Normal);
        }
        
        private Text CreateTextLine(GameObject parent, string name, string text, float yPercent, int fontSize, FontStyle fontStyle)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            // Use relative positioning based on percentage
            textRect.anchorMin = new Vector2(0f, yPercent - 0.15f);  // Text line height
            textRect.anchorMax = new Vector2(1f, yPercent);           // Full width
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleLeft;
            textComponent.fontStyle = fontStyle;
            
            return textComponent;
        }
        
        public void UpdateDisplay()
        {
            if (manager == null) return;
            
            try
            {
                var currentTrackInfo = manager.GetCurrentTrackInfo();
                var isPlaying = manager.IsPlaying;
                
                if (!string.IsNullOrEmpty(currentTrackInfo) && currentTrackInfo != "No Track")
                {
                    // Parse track info or use as-is
                    nowPlayingText!.text = $"🎵 Now Playing: \"{currentTrackInfo}\"";
                    artistText!.text = "🎤 Artist: \"Game Audio\"";
                    albumText!.text = "💿 Album: \"Official Soundtrack\"";
                    sourceText!.text = "📊 Source: In-Game Jukebox";
                }
                else
                {
                    nowPlayingText!.text = "🎵 No Track Selected";
                    artistText!.text = "🎤 Artist: Unknown";
                    albumText!.text = "💿 Album: Unknown";
                    sourceText!.text = "📊 Source: Load music to get started";
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Track info update failed: {ex.Message}", "UI");
                nowPlayingText!.text = "🎵 Error loading track info";
            }
        }
    }
} 