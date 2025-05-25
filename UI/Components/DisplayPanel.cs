using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI.Components
{
    public class DisplayPanel : MonoBehaviour
    {
        // IL2CPP compatibility - explicit field initialization
        private BackSpeakerManager manager = null;
        private Text trackNameText = null;
        private Text artistText = null;
        private Image albumArt = null;
        private GameObject albumArtContainer = null;

        // IL2CPP compatibility - explicit parameterless constructor
        public DisplayPanel() : base() { }

        public void Setup(BackSpeakerManager manager, RectTransform parent)
        {
            try
            {
                this.manager = manager;
                LoggerUtil.Info("DisplayPanel: Setting up modern track display with album art");
                
                // Create album art container with modern styling
                CreateAlbumArtDisplay(parent);
                
                // Modern track info display - positioned to work with album art
                trackNameText = UIFactory.CreateText(
                    parent.transform,
                    "TrackName",
                    "♪ No Track Selected ♪",
                    new Vector2(0f, 25f), // Slightly lower to accommodate album art
                    new Vector2(320f, 40f), // Wider for better text display
                    24 // Large, readable text
                );
                
                // Apply Spotify-style text color
                trackNameText.color = new Color(1f, 1f, 1f, 1f); // Pure white for primary text
                LoggerUtil.Info("DisplayPanel: Track name text created");
                
                // Artist/source info - smaller, below track name with Spotify styling
                artistText = UIFactory.CreateText(
                    parent.transform,
                    "ArtistName",
                    "Load music to get started",
                    new Vector2(0f, -5f), // Below track name, good spacing
                    new Vector2(300f, 25f), // Appropriate width
                    16 // Subtitle text size
                );
                
                // Apply Spotify-style secondary text color
                artistText.color = new Color(0.7f, 0.7f, 0.7f, 1f); // Light gray for secondary text
                LoggerUtil.Info("DisplayPanel: Artist text created");
                
                LoggerUtil.Info("DisplayPanel: Modern setup with album art completed");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"DisplayPanel: Setup failed: {ex}");
                throw;
            }
        }

        private void CreateAlbumArtDisplay(RectTransform parent)
        {
            // Create album art container
            albumArtContainer = new GameObject("AlbumArtContainer");
            albumArtContainer.transform.SetParent(parent.transform, false);
            
            var containerRect = albumArtContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = new Vector2(0f, 70f); // Above track info
            containerRect.sizeDelta = new Vector2(80f, 80f); // Square album art
            
            // Add album art background with rounded appearance and modern depth
            var background = albumArtContainer.AddComponent<Image>();
            background.color = new Color(0.15f, 0.15f, 0.15f, 0.9f); // Dark background
            
            // Apply modern visual depth effects
            UIFactory.ApplyModernShadow(albumArtContainer, new Color(0f, 0f, 0f, 0.3f), new Vector2(2f, -3f));
            UIFactory.ApplyModernBorder(albumArtContainer, new Color(0.3f, 0.3f, 0.3f, 0.6f), 1f);
            
            // Create the actual album art image
            var albumArtObj = new GameObject("AlbumArt");
            albumArtObj.transform.SetParent(albumArtContainer.transform, false);
            
            var artRect = albumArtObj.AddComponent<RectTransform>();
            artRect.anchorMin = Vector2.zero;
            artRect.anchorMax = Vector2.one;
            artRect.offsetMin = new Vector2(4f, 4f); // Small border
            artRect.offsetMax = new Vector2(-4f, -4f);
            
            albumArt = albumArtObj.AddComponent<Image>();
            
            // Create default "no album art" display
            CreateDefaultAlbumArt();
            
            LoggerUtil.Info("DisplayPanel: Album art container created with modern visual depth");
        }

        private void CreateDefaultAlbumArt()
        {
            if (albumArt == null) return;
            
            // Create a simple texture for the default album art
            var texture = new Texture2D(64, 64);
            var pixels = new Color32[64 * 64];
            
            // Create a gradient effect for the default album art
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float gradient = (float)(x + y) / 128f;
                    byte value = (byte)(30 + gradient * 60); // Dark to medium gray gradient
                    pixels[y * 64 + x] = new Color32(value, value, value, 255);
                }
            }
            
            // Add a musical note symbol in the center
            AddMusicalNoteToTexture(pixels, 64, 64);
            
            texture.SetPixels32(pixels);
            texture.Apply();
            
            albumArt.sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
            albumArt.color = new Color(1f, 1f, 1f, 0.8f); // Slightly transparent
        }

        private void AddMusicalNoteToTexture(Color32[] pixels, int width, int height)
        {
            // Simple musical note pattern - just a basic representation
            int centerX = width / 2;
            int centerY = height / 2;
            
            // Note stem (vertical line)
            for (int y = centerY - 10; y < centerY + 8; y++)
            {
                if (y >= 0 && y < height)
                {
                    int x = centerX + 4;
                    if (x >= 0 && x < width)
                        pixels[y * width + x] = new Color32(100, 200, 100, 255); // Green musical note
                }
            }
            
            // Note head (filled circle)
            for (int y = centerY + 5; y < centerY + 12; y++)
            {
                for (int x = centerX - 3; x < centerX + 5; x++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        int dx = x - centerX;
                        int dy = y - (centerY + 8);
                        if (dx * dx + dy * dy <= 12) // Simple circle
                        {
                            pixels[y * width + x] = new Color32(100, 200, 100, 255); // Green
                        }
                    }
                }
            }
        }

        public void UpdateDisplay()
        {
            if (manager == null) return;
            
            try
            {
                if (trackNameText != null)
                {
                    string trackInfo = manager.GetCurrentTrackInfo();
                    if (string.IsNullOrEmpty(trackInfo) || trackInfo == "No Track")
                    {
                        trackNameText.text = "♪ No Track Selected ♪";
                        UpdateAlbumArtForEmptyState();
                    }
                    else
                    {
                        trackNameText.text = $"♪ {trackInfo} ♪";
                        UpdateAlbumArtForTrack();
                    }
                }
                
                if (artistText != null)
                {
                    string artistInfo = manager.GetCurrentArtistInfo();
                    int trackCount = manager.GetTrackCount();
                    int currentIndex = manager.CurrentTrackIndex;
                    
                    if (trackCount == 0)
                    {
                        artistText.text = "Load music to get started";
                    }
                    else
                    {
                        artistText.text = $"{artistInfo} • Track {currentIndex + 1} of {trackCount}";
                    }
                }
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"DisplayPanel: UpdateDisplay failed: {ex}");
            }
        }

        private void UpdateAlbumArtForEmptyState()
        {
            if (albumArt != null)
            {
                albumArt.color = new Color(1f, 1f, 1f, 0.4f); // More transparent when no track
            }
        }

        private void UpdateAlbumArtForTrack()
        {
            if (albumArt != null)
            {
                albumArt.color = new Color(1f, 1f, 1f, 0.9f); // More opaque when playing
                
                // In a real implementation, you would load actual album art here
                // For now, we'll just ensure the default art is visible
            }
        }
    }
} 