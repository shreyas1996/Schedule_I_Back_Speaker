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

        public void Setup(BackSpeakerManager manager, RectTransform parent)
        {
            this.manager = manager;
            LoggerUtil.Info("DisplayPanel: Setting up display components");
            
            // Get the actual canvas dimensions for proper positioning
            Rect canvasRect = parent.rect;
            float canvasWidth = canvasRect.width;
            
            // Create song title display - positioned like Drones status label
            songTitleText = UIFactory.CreateText(
                parent.transform, 
                "SongTitle", 
                "No Song Playing", 
                new Vector2(0f, -50f), // Position from top-center
                new Vector2(canvasWidth * 0.8f, 40f), // 80% width, 40px height
                24 // Large font size
            );
            
            // Create artist display - positioned below title
            artistText = UIFactory.CreateText(
                parent.transform, 
                "Artist", 
                "Unknown Artist", 
                new Vector2(0f, -100f), // Below the title
                new Vector2(canvasWidth * 0.8f, 30f), // 80% width, 30px height
                18 // Smaller font
            );
            artistText.color = Color.gray;
            
            LoggerUtil.Info("DisplayPanel: Setup completed");
        }

        public void UpdateDisplay()
        {
            if (manager == null) return;
            
            if (songTitleText != null)
                songTitleText.text = manager.GetCurrentTrackInfo();
                
            if (artistText != null)
                artistText.text = manager.GetCurrentArtistInfo();
        }
    }
} 