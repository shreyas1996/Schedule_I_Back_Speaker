using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI.Components
{
    public class DisplayPanel : MonoBehaviour
    {
        private BackSpeakerManager manager;
        private Text songTitleText;
        private Text artistText;
        private Text trackInfoText;

        public void Setup(BackSpeakerManager manager, RectTransform parent)
        {
            this.manager = manager;
            LoggerUtil.Info("DisplayPanel: Setting up modern music app display");
            
            // Modern music app style - large song info area at top
            
            // Create song title display - large, prominent, Spotify-style
            songTitleText = UIFactory.CreateText(
                parent.transform, 
                "SongTitle", 
                "No Song Playing", 
                new Vector2(0f, 120f), // Much higher position for large display
                new Vector2(350f, 40f), // Larger display area
                26 // Large font like modern music apps
            );
            songTitleText.alignment = TextAnchor.MiddleCenter;
            songTitleText.fontStyle = FontStyle.Bold;
            
            // Create artist display - medium size, properly spaced below
            artistText = UIFactory.CreateText(
                parent.transform, 
                "Artist", 
                "Unknown Artist", 
                new Vector2(0f, 85f), // Good spacing below title
                new Vector2(300f, 25f), 
                18 // Medium font for artist
            );
            artistText.alignment = TextAnchor.MiddleCenter;
            artistText.color = new Color(0.7f, 0.7f, 0.7f, 1f); // Dimmed like Spotify
            
            // Create track info display - smaller, below artist
            trackInfoText = UIFactory.CreateText(
                parent.transform, 
                "TrackInfo", 
                "Track 1 of 0", 
                new Vector2(0f, 60f), // Below artist
                new Vector2(200f, 20f), 
                14 // Small font for track info
            );
            trackInfoText.alignment = TextAnchor.MiddleCenter;
            trackInfoText.color = new Color(0.5f, 0.5f, 0.5f, 1f); // More dimmed
            
            LoggerUtil.Info("DisplayPanel: Modern setup completed");
        }

        public void UpdateDisplay()
        {
            if (manager == null) return;
            
            if (songTitleText != null)
                songTitleText.text = manager.GetCurrentTrackInfo();
                
            if (artistText != null)
                artistText.text = manager.GetCurrentArtistInfo();
                
            if (trackInfoText != null)
            {
                int currentTrack = manager.CurrentTrackIndex + 1; // 1-based for display
                int totalTracks = manager.GetTrackCount();
                trackInfoText.text = $"Track {currentTrack} of {totalTracks}";
            }
        }
    }
} 