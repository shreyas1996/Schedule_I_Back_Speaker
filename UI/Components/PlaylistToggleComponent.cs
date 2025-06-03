using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Modules;

namespace BackSpeakerMod.UI.Components
{
    public class PlaylistToggleComponent : MonoBehaviour
    {
        private BackSpeakerManager? manager;
        private MusicSourceType currentTab = MusicSourceType.Jukebox;
        private Button? playlistButton;
        private Text? buttonText;
        
        public PlaylistToggleComponent() : base() { }
        
        public void Setup(BackSpeakerManager manager)
        {
            this.manager = manager;
            CreatePlaylistButton();
        }
        
        private void CreatePlaylistButton()
        {
            var buttonObj = new GameObject("PlaylistButton");
            buttonObj.transform.SetParent(this.transform, false);
            
            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.3f, 0.1f);
            buttonRect.anchorMax = new Vector2(0.7f, 0.9f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            playlistButton = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.7f, 0.2f, 0.8f); // Green for jukebox default
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 0f);
            textRect.offsetMax = new Vector2(-10f, 0f);
            
            buttonText = textObj.AddComponent<Text>();
            buttonText.text = "♫ Jukebox Playlist (0 tracks)";
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 14;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontStyle = FontStyle.Bold;
            
            playlistButton.targetGraphic = buttonImage;
            playlistButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnPlaylistToggle);
        }
        
        private void OnPlaylistToggle()
        {
            LoggingSystem.Info($"Playlist toggle requested for {currentTab}", "UI");
            // TODO: Implement playlist popup
        }
        
        public void UpdateForTab(MusicSourceType newTab)
        {
            currentTab = newTab;
            UpdatePlaylistButton();
        }
        
        private void UpdatePlaylistButton()
        {
            if (buttonText == null || playlistButton == null) return;
            
            var trackCount = manager?.GetAllTracks().Count ?? 0;
            
            var (text, color) = currentTab switch
            {
                MusicSourceType.Jukebox => ($"♫ Jukebox Playlist ({trackCount} tracks)", new Color(0.2f, 0.7f, 0.2f, 0.8f)),
                MusicSourceType.LocalFolder => ($"♫ Local Playlist ({trackCount} tracks)", new Color(0.2f, 0.4f, 0.8f, 0.8f)),
                MusicSourceType.YouTube => ($"♫ YouTube Playlist ({trackCount} tracks)", new Color(0.8f, 0.2f, 0.2f, 0.8f)),
                _ => ($"♫ Playlist ({trackCount} tracks)", new Color(0.5f, 0.5f, 0.5f, 0.8f))
            };
            
            buttonText.text = text;
            playlistButton.GetComponent<Image>().color = color;
        }
        
        public void UpdatePlaylist()
        {
            UpdatePlaylistButton();
        }
    }
} 