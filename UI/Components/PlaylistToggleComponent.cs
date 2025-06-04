using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Modules;
using System.Collections.Generic;

namespace BackSpeakerMod.UI.Components
{
    public class PlaylistToggleComponent : MonoBehaviour
    {
        private BackSpeakerManager? manager;
        private MusicSourceType currentTab = MusicSourceType.Jukebox;
        private Button? playlistButton;
        private Text? buttonText;
        private GameObject? playlistPopup;
        private bool isPlaylistOpen = false;
        
        public PlaylistToggleComponent() : base() { }
        
        public void Setup(BackSpeakerManager manager)
        {
            this.manager = manager;
            CreatePlaylistButton();
        }
        
        private void CreatePlaylistButton()
        {
            LoggingSystem.Info("Creating playlist button...", "UI");
            
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
            playlistButton.onClick.AddListener((UnityEngine.Events.UnityAction)delegate() { TogglePlaylist(); });
            
            LoggingSystem.Info("Playlist button created successfully", "UI");
        }
        
        private void TogglePlaylist()
        {
            LoggingSystem.Info($"Playlist toggle clicked. Current state: {(isPlaylistOpen ? "Open" : "Closed")}", "UI");
            
            if (isPlaylistOpen)
            {
                ClosePlaylist();
            }
            else
            {
                OpenPlaylist();
            }
        }
        
        private void OpenPlaylist()
        {
            CreatePlaylistPopup();
            isPlaylistOpen = true;
            LoggingSystem.Info($"Opened playlist for {currentTab}", "UI");
        }
        
        private void ClosePlaylist()
        {
            if (playlistPopup != null)
            {
                UnityEngine.Object.Destroy(playlistPopup);
                playlistPopup = null;
            }
            isPlaylistOpen = false;
            LoggingSystem.Info("Closed playlist", "UI");
        }
        
        private void CreatePlaylistPopup()
        {
            LoggingSystem.Info("Creating playlist popup...", "UI");
            
            // Find our app's container instead of any canvas
            Transform? appContainer = null;
            Transform current = this.transform;
            
            // Walk up the hierarchy to find "Container" (our app's container)
            while (current != null && appContainer == null)
            {
                if (current.name == "Container")
                {
                    appContainer = current;
                    break;
                }
                current = current.parent;
            }
            
            // If no container found, try to find BackSpeakerApp canvas
            if (appContainer == null)
            {
                current = this.transform;
                while (current != null)
                {
                    if (current.name == "BackSpeakerApp")
                    {
                        // Look for Container child
                        var containerChild = current.FindChild("Container");
                        if (containerChild != null)
                        {
                            appContainer = containerChild;
                            break;
                        }
                    }
                    current = current.parent;
                }
            }
            
            if (appContainer == null)
            {
                LoggingSystem.Error("No app Container found for playlist popup! This will cause UI bleeding.", "UI");
                return;
            }
            
            LoggingSystem.Info($"Found app Container: {appContainer.name}", "UI");
            
            // Create popup that covers the entire app container (not the whole screen)
            playlistPopup = new GameObject("PlaylistPopup");
            playlistPopup.transform.SetParent(appContainer, false);
            
            var popupRect = playlistPopup.AddComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;
            popupRect.anchoredPosition = Vector2.zero;
            popupRect.sizeDelta = Vector2.zero;
            
            // Semi-transparent background that blocks clicks
            var popupBg = playlistPopup.AddComponent<Image>();
            popupBg.color = new Color(0f, 0f, 0f, 0.8f);
            popupBg.raycastTarget = true; // Block clicks behind popup
            
            // Make sure popup appears on top within our container
            playlistPopup.transform.SetAsLastSibling();
            
            LoggingSystem.Info("Popup background created within app container", "UI");
            
            // Playlist panel
            var playlistPanel = new GameObject("PlaylistPanel");
            playlistPanel.transform.SetParent(playlistPopup.transform, false);
            
            var panelRect = playlistPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            var panelBg = playlistPanel.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            
            LoggingSystem.Info("Playlist panel created", "UI");
            
            // Create playlist content
            CreatePlaylistContent(playlistPanel);
            
            LoggingSystem.Info("Playlist popup creation completed", "UI");
        }
        
        private void CreatePlaylistContent(GameObject panel)
        {
            // Header
            var header = new GameObject("Header");
            header.transform.SetParent(panel.transform, false);
            
            var headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 0.9f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.offsetMin = new Vector2(10f, 0f);
            headerRect.offsetMax = new Vector2(-10f, 0f);
            
            var headerText = header.AddComponent<Text>();
            headerText.text = $"{currentTab} Playlist";
            headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            headerText.fontSize = 18;
            headerText.color = Color.white;
            headerText.alignment = TextAnchor.MiddleLeft;
            headerText.fontStyle = FontStyle.Bold;
            
            // Close button
            var closeButton = new GameObject("CloseButton");
            closeButton.transform.SetParent(header.transform, false);
            
            var closeRect = closeButton.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.9f, 0f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            
            var closeBtn = closeButton.AddComponent<Button>();
            var closeBtnImage = closeButton.AddComponent<Image>();
            closeBtnImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            
            var closeText = new GameObject("Text");
            closeText.transform.SetParent(closeButton.transform, false);
            var closeTextRect = closeText.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;
            
            var closeTextComponent = closeText.AddComponent<Text>();
            closeTextComponent.text = "X";
            closeTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            closeTextComponent.fontSize = 16;
            closeTextComponent.color = Color.white;
            closeTextComponent.alignment = TextAnchor.MiddleCenter;
            closeTextComponent.fontStyle = FontStyle.Bold;
            
            closeBtn.targetGraphic = closeBtnImage;
            closeBtn.onClick.AddListener((UnityEngine.Events.UnityAction)delegate() { ClosePlaylist(); });
            
            // Track list area
            CreateTrackList(panel);
        }
        
        private void CreateTrackList(GameObject panel)
        {
            var trackListContainer = new GameObject("TrackList");
            trackListContainer.transform.SetParent(panel.transform, false);
            
            var listRect = trackListContainer.AddComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0f, 0.1f);
            listRect.anchorMax = new Vector2(1f, 0.85f);
            listRect.offsetMin = new Vector2(10f, 0f);
            listRect.offsetMax = new Vector2(-10f, 0f);
            
            // Scroll view for track list
            var scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(trackListContainer.transform, false);
            
            var scrollRect = scrollView.AddComponent<ScrollRect>();
            var scrollRectTransform = scrollView.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.offsetMin = Vector2.zero;
            scrollRectTransform.offsetMax = Vector2.zero;
            
            // Content area
            var content = new GameObject("Content");
            content.transform.SetParent(scrollView.transform, false);
            
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            
            var contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 5f;
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            
            var contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scrollRect.content = contentRect;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            
            // Add tracks to the list
            PopulateTrackList(content);
        }
        
        private void PopulateTrackList(GameObject content)
        {
            // Get tracks for current source
            var tracks = GetTracksForCurrentSource();
            
            if (tracks == null || tracks.Count == 0)
            {
                // Show "No tracks" message with proper styling
                var noTracksObj = new GameObject("NoTracksMessage");
                noTracksObj.transform.SetParent(content.transform, false);
                
                var noTracksText = noTracksObj.AddComponent<Text>();
                noTracksText.text = $"No tracks loaded for {currentTab}\n\n" +
                                  "• Attach headphones first\n" +
                                  "• Use the reload button to load tracks\n" +
                                  "• For local music: place files in game folder\n" +
                                  "• For YouTube: search and cache videos";
                noTracksText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                noTracksText.fontSize = 12;
                noTracksText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                noTracksText.alignment = TextAnchor.MiddleCenter;
                noTracksText.fontStyle = FontStyle.Italic;
                
                var noTracksLayout = noTracksObj.AddComponent<LayoutElement>();
                noTracksLayout.minHeight = 120f;
                noTracksLayout.preferredHeight = 120f;
                
                return;
            }
            
            // Create track items for real tracks
            for (int i = 0; i < tracks.Count; i++)
            {
                CreateTrackItem(content, tracks[i], i);
            }
        }
        
        private void CreateTrackItem(GameObject parent, string trackName, int index)
        {
            var trackItem = new GameObject($"Track_{index}");
            trackItem.transform.SetParent(parent.transform, false);
            
            var itemRect = trackItem.AddComponent<RectTransform>();
            var layoutElement = trackItem.AddComponent<LayoutElement>();
            layoutElement.minHeight = 40f;
            layoutElement.preferredHeight = 40f;
            
            // Check if this is the currently playing track
            var currentTrackIndex = manager?.CurrentTrackIndex ?? -1;
            var isCurrentTrack = (index == currentTrackIndex && manager?.IsPlaying == true);
            
            // Track item background - highlight current track with tab theme color
            var itemBg = trackItem.AddComponent<Image>();
            if (isCurrentTrack)
            {
                // Use theme color for currently playing track
                itemBg.color = currentTab switch
                {
                    MusicSourceType.Jukebox => new Color(0.2f, 0.7f, 0.2f, 0.6f),      // Green
                    MusicSourceType.LocalFolder => new Color(0.2f, 0.4f, 0.8f, 0.6f),  // Blue
                    MusicSourceType.YouTube => new Color(0.8f, 0.2f, 0.2f, 0.6f),      // Red
                    _ => new Color(0.5f, 0.5f, 0.5f, 0.6f)
                };
            }
            else
            {
                itemBg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            }
            
            // Track name text
            var trackText = new GameObject("TrackText");
            trackText.transform.SetParent(trackItem.transform, false);
            
            var textRect = trackText.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0f);
            textRect.anchorMax = new Vector2(0.8f, 1f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = trackText.AddComponent<Text>();
            text.text = isCurrentTrack ? $"🎵 {index + 1}. {trackName}" : $"{index + 1}. {trackName}";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 12;
            text.color = isCurrentTrack ? Color.white : Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.fontStyle = isCurrentTrack ? FontStyle.Bold : FontStyle.Normal;
            
            // Play button for track
            var playTrackBtn = new GameObject("PlayButton");
            playTrackBtn.transform.SetParent(trackItem.transform, false);
            
            var playBtnRect = playTrackBtn.AddComponent<RectTransform>();
            playBtnRect.anchorMin = new Vector2(0.85f, 0.2f);
            playBtnRect.anchorMax = new Vector2(0.95f, 0.8f);
            playBtnRect.offsetMin = Vector2.zero;
            playBtnRect.offsetMax = Vector2.zero;
            
            var playBtn = playTrackBtn.AddComponent<Button>();
            var playBtnImage = playTrackBtn.AddComponent<Image>();
            playBtnImage.color = new Color(0.3f, 0.7f, 0.3f, 0.8f);
            
            var playBtnText = new GameObject("Text");
            playBtnText.transform.SetParent(playTrackBtn.transform, false);
            var playTextRect = playBtnText.AddComponent<RectTransform>();
            playTextRect.anchorMin = Vector2.zero;
            playTextRect.anchorMax = Vector2.one;
            playTextRect.offsetMin = Vector2.zero;
            playTextRect.offsetMax = Vector2.zero;
            
            var playTextComponent = playBtnText.AddComponent<Text>();
            playTextComponent.text = isCurrentTrack ? "⏸" : "►";  // Show pause if currently playing
            playTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            playTextComponent.fontSize = 10;
            playTextComponent.color = Color.white;
            playTextComponent.alignment = TextAnchor.MiddleCenter;
            
            playBtn.targetGraphic = playBtnImage;
            playBtn.onClick.AddListener((UnityEngine.Events.UnityAction)delegate() { PlayTrack(index); });
        }
        
        private List<string> GetTracksForCurrentSource()
        {
            // Get actual tracks from manager
            var tracks = new List<string>();
            
            try
            {
                var allTracks = manager?.GetAllTracks();
                if (allTracks != null && allTracks.Count > 0)
                {
                    foreach (var track in allTracks)
                    {
                        // Format as "Title - Artist" or just title if artist is empty
                        string trackDisplay = !string.IsNullOrEmpty(track.artist) 
                            ? $"{track.title} - {track.artist}"
                            : track.title;
                        tracks.Add(trackDisplay);
                    }
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to get tracks: {ex.Message}", "UI");
            }
            
            return tracks; // Return empty list if no tracks, no placeholders
        }
        
        private void PlayTrack(int index)
        {
            LoggingSystem.Info($"Playing track {index} from {currentTab} playlist", "UI");
            try
            {
                manager?.PlayTrack(index);
                ClosePlaylist();
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to play track {index}: {ex.Message}", "UI");
            }
        }
        
        public void UpdateForTab(MusicSourceType newTab)
        {
            currentTab = newTab;
            UpdatePlaylistButton();
            
            // Close playlist if it's open when switching tabs
            if (isPlaylistOpen)
            {
                ClosePlaylist();
            }
        }
        
        private void UpdatePlaylistButton()
        {
            if (buttonText == null || playlistButton == null) return;
            
            try
            {
                var trackCount = manager?.GetTrackCount() ?? 0;
                
                var (text, color) = currentTab switch
                {
                    MusicSourceType.Jukebox => ($"♫ Jukebox Playlist ({trackCount} tracks)", new Color(0.2f, 0.7f, 0.2f, 0.8f)),
                    MusicSourceType.LocalFolder => ($"♫ Local Playlist ({trackCount} tracks)", new Color(0.2f, 0.4f, 0.8f, 0.8f)),
                    MusicSourceType.YouTube => ($"♫ YouTube Playlist ({trackCount} tracks)", new Color(0.8f, 0.2f, 0.2f, 0.8f)),
                    _ => ($"♫ Playlist ({trackCount} tracks)", new Color(0.5f, 0.5f, 0.5f, 0.8f))
                };
                
                buttonText.text = text;
                playlistButton.GetComponent<Image>().color = color;
                
                // LoggingSystem.Debug($"Playlist button updated for {currentTab}: {trackCount} tracks", "UI");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to update playlist button: {ex.Message}", "UI");
                buttonText.text = $"♫ Playlist Error";
                playlistButton.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            }
        }
        
        public void UpdatePlaylist()
        {
            UpdatePlaylistButton();
        }
        
        public bool IsPlaylistOpen() => isPlaylistOpen;
        
        public void ClosePlaylistIfOpen()
        {
            if (isPlaylistOpen)
            {
                ClosePlaylist();
            }
        }
    }
} 