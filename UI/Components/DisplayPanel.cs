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
            
            // Create song title display - centered above controls
            songTitleText = UIFactory.CreateText(
                parent.transform, 
                "SongTitle", 
                "No Song Playing", 
                new Vector2(0f, -50f), // Above the controls
                new Vector2(300f, 30f), // Fixed width for consistency
                22 // Large font size
            );
            
            // Create artist display - positioned below title
            artistText = UIFactory.CreateText(
                parent.transform, 
                "Artist", 
                "Unknown Artist", 
                new Vector2(0f, -20f), // Below the title
                new Vector2(300f, 25f), // Fixed width for consistency
                16 // Smaller font
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