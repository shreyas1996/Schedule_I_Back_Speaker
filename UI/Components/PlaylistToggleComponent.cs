using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Modules;
using System.Collections.Generic;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;
using System;
using System.Linq;
using Il2CppCollections = Il2CppSystem.Collections.Generic;

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
        
        // YouTube playlist management
        private YouTubePlaylist? currentYouTubePlaylist;
        private List<YouTubePlaylistInfo> availableYouTubePlaylists = new List<YouTubePlaylistInfo>();
        private string selectedPlaylistId = ""; // Track which playlist is currently selected
        private Dropdown? youTubePlaylistDropdown;
        private Button? createPlaylistButton;
        private Button? deletePlaylistButton;
        
        // External UI elements (outside popup)
        private Dropdown? externalPlaylistDropdown;
        private Button? managePlaylistsButton;
        private GameObject? managePlaylistsPopup;
        
        // Editable playlist name UI
        private InputField? playlistNameInput;
        private Button? savePlaylistNameButton;
        private Button? cancelPlaylistButton;
        private string originalPlaylistName = "";
        
        // Save/Cancel workflow
        private bool isPlaylistBeingEdited = false;
        private YouTubePlaylist? originalPlaylistState;
        
        // Add to Playlist secondary popup
        private GameObject? addToPlaylistPopup;
        private bool isAddToPlaylistPopupOpen = false;
        private int pendingSongIndex = -1;
        private SongDetails? pendingSongDetails;
        
        public PlaylistToggleComponent() : base() { }
        
        public void Setup(BackSpeakerManager manager)
        {
            this.manager = manager;
            CreatePlaylistButton();
            
            // Initialize YouTube playlist functionality
            InitializeYouTubePlaylists();
            
            // Subscribe to YouTube playlist events
            YouTubePlaylistManager.OnPlaylistCreated += OnYouTubePlaylistCreated;
            YouTubePlaylistManager.OnPlaylistUpdated += OnYouTubePlaylistUpdated;
            YouTubePlaylistManager.OnPlaylistDeleted += OnYouTubePlaylistDeleted;
            YouTubePlaylistManager.OnPlaylistIndexChanged += OnYouTubePlaylistIndexChanged;
        }
        
        private void CreatePlaylistButton()
        {
            if (playlistButton != null) return;
            
            var buttonObj = new GameObject("PlaylistButton");
            buttonObj.transform.SetParent(this.transform, false);
            
            var buttonRect = buttonObj.AddComponent<RectTransform>();
            
            // Use most of the panel height with some padding
            buttonRect.anchorMin = new Vector2(0.6f, 0.1f);
            buttonRect.anchorMax = new Vector2(0.9f, 0.7f);
            
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            playlistButton = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.8f, 0.8f);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            buttonText = textObj.AddComponent<Text>();
            buttonText.text = "Show Playlist";
            FontHelper.SetSafeFont(buttonText);
            buttonText.fontSize = 10;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontStyle = FontStyle.Bold;
            
            playlistButton.targetGraphic = buttonImage;
            playlistButton.onClick.AddListener((UnityEngine.Events.UnityAction)TogglePlaylist);
            
            // Create external controls for YouTube tab
            if (currentTab == MusicSourceType.YouTube)
            {
                CreateExternalYouTubeControls();
            }
            
            UpdatePlaylistButton();
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
            // Header with editable playlist name
            var header = new GameObject("Header");
            header.transform.SetParent(panel.transform, false);
            
            var headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 0.9f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.offsetMin = new Vector2(10f, 0f);
            headerRect.offsetMax = new Vector2(-10f, 0f);
            
            // Create editable playlist name for YouTube tab, regular text for others
            if (currentTab == MusicSourceType.YouTube)
            {
                CreateEditablePlaylistHeader(header);
            }
            else
            {
                CreateStaticPlaylistHeader(header);
            }
            
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
            closeTextComponent.text = "Close";
            FontHelper.SetSafeFont(closeTextComponent);
            closeTextComponent.fontSize = 10;
            closeTextComponent.color = Color.white;
            closeTextComponent.alignment = TextAnchor.MiddleCenter;
            closeTextComponent.fontStyle = FontStyle.Bold;
            
            closeBtn.targetGraphic = closeBtnImage;
            closeBtn.onClick.AddListener((UnityEngine.Events.UnityAction)delegate() { ClosePlaylist(); });
            
            // Track list area (no more YouTube management in main popup)
            CreateTrackList(panel);
        }
        
        private void CreateTrackList(GameObject panel)
        {
            var trackListContainer = new GameObject("TrackList");
            trackListContainer.transform.SetParent(panel.transform, false);
            
            var listRect = trackListContainer.AddComponent<RectTransform>();
            
            // Simplified positioning since YouTube management moved to manage popup
            listRect.anchorMin = new Vector2(0f, 0.1f);
            listRect.anchorMax = new Vector2(1f, 0.85f);  // Full height available
            
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

            // Viewport with Mask
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0.01f); // Transparent but needed for Mask
            var viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            
            // Content area
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            
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
            scrollRect.viewport = viewportRect;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            
            // Add tracks to the list
            PopulateTrackList(content);

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            scrollRect.movementType = ScrollRect.MovementType.Clamped;
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
                noTracksText.text = "No tracks available";
                FontHelper.SetSafeFont(noTracksText);
                noTracksText.fontSize = 14;
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
            
            // Track name text - adjust width based on whether remove button is needed
            var trackText = new GameObject("TrackText");
            trackText.transform.SetParent(trackItem.transform, false);
            
            var textRect = trackText.AddComponent<RectTransform>();
            if (currentTab == MusicSourceType.YouTube)
            {
                textRect.anchorMin = new Vector2(0.05f, 0f);
                textRect.anchorMax = new Vector2(0.45f, 1f); // Leave more space for 3 buttons
            }
            else
            {
                textRect.anchorMin = new Vector2(0.05f, 0f);
                textRect.anchorMax = new Vector2(0.8f, 1f); // Original width
            }
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = trackText.AddComponent<Text>();
            text.text = trackName;
            FontHelper.SetSafeFont(text);
            text.fontSize = 12;
            text.color = isCurrentTrack ? Color.white : Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.fontStyle = isCurrentTrack ? FontStyle.Bold : FontStyle.Normal;
            
            // Play button for track
            var playTrackBtn = new GameObject("PlayButton");
            playTrackBtn.transform.SetParent(trackItem.transform, false);
            
            var playBtnRect = playTrackBtn.AddComponent<RectTransform>();
            if (currentTab == MusicSourceType.YouTube)
            {
                playBtnRect.anchorMin = new Vector2(0.5f, 0.2f);
                playBtnRect.anchorMax = new Vector2(0.62f, 0.8f);
            }
            else
            {
                playBtnRect.anchorMin = new Vector2(0.85f, 0.2f);
                playBtnRect.anchorMax = new Vector2(0.95f, 0.8f);
            }
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
            playTextComponent.text = "Play";
            FontHelper.SetSafeFont(playTextComponent);
            playTextComponent.fontSize = 8;
            playTextComponent.color = Color.white;
            playTextComponent.alignment = TextAnchor.MiddleCenter;
            
            playBtn.targetGraphic = playBtnImage;
            playBtn.onClick.AddListener((UnityEngine.Events.UnityAction)delegate() { PlayTrack(index); });
            
            // Add remove button for YouTube playlists
            if (currentTab == MusicSourceType.YouTube)
            {
                // Add to Other Playlist button
                var addToPlaylistBtn = new GameObject("AddToPlaylistButton");
                addToPlaylistBtn.transform.SetParent(trackItem.transform, false);
                
                var addToPlaylistBtnRect = addToPlaylistBtn.AddComponent<RectTransform>();
                addToPlaylistBtnRect.anchorMin = new Vector2(0.64f, 0.2f);
                addToPlaylistBtnRect.anchorMax = new Vector2(0.8f, 0.8f);
                addToPlaylistBtnRect.offsetMin = Vector2.zero;
                addToPlaylistBtnRect.offsetMax = Vector2.zero;
                
                var addToPlaylistBtnComponent = addToPlaylistBtn.AddComponent<Button>();
                var addToPlaylistBtnImage = addToPlaylistBtn.AddComponent<Image>();
                addToPlaylistBtnImage.color = new Color(0.3f, 0.5f, 0.8f, 0.8f); // Blue color
                
                var addToPlaylistBtnTextObj = new GameObject("Text");
                addToPlaylistBtnTextObj.transform.SetParent(addToPlaylistBtn.transform, false);
                var addToPlaylistBtnTextRect = addToPlaylistBtnTextObj.AddComponent<RectTransform>();
                addToPlaylistBtnTextRect.anchorMin = Vector2.zero;
                addToPlaylistBtnTextRect.anchorMax = Vector2.one;
                addToPlaylistBtnTextRect.offsetMin = Vector2.zero;
                addToPlaylistBtnTextRect.offsetMax = Vector2.zero;
                
                var addToPlaylistBtnTextComponent = addToPlaylistBtnTextObj.AddComponent<Text>();
                addToPlaylistBtnTextComponent.text = "Add to Playlist";
                FontHelper.SetSafeFont(addToPlaylistBtnTextComponent);
                addToPlaylistBtnTextComponent.fontSize = 6;
                addToPlaylistBtnTextComponent.color = Color.white;
                addToPlaylistBtnTextComponent.alignment = TextAnchor.MiddleCenter;
                
                addToPlaylistBtnComponent.targetGraphic = addToPlaylistBtnImage;
                addToPlaylistBtnComponent.onClick.AddListener((UnityEngine.Events.UnityAction)delegate() { ShowAddToPlaylistPopup(index); });

                // Remove button
                var removeBtn = new GameObject("RemoveButton");
                removeBtn.transform.SetParent(trackItem.transform, false);
                
                var removeBtnRect = removeBtn.AddComponent<RectTransform>();
                removeBtnRect.anchorMin = new Vector2(0.82f, 0.2f);
                removeBtnRect.anchorMax = new Vector2(0.97f, 0.8f);
                removeBtnRect.offsetMin = Vector2.zero;
                removeBtnRect.offsetMax = Vector2.zero;
                
                var removeBtnComponent = removeBtn.AddComponent<Button>();
                var removeBtnImage = removeBtn.AddComponent<Image>();
                removeBtnImage.color = new Color(0.8f, 0.3f, 0.3f, 0.8f); // Red color
                
                var removeBtnTextObj = new GameObject("Text");
                removeBtnTextObj.transform.SetParent(removeBtn.transform, false);
                var removeBtnTextRect = removeBtnTextObj.AddComponent<RectTransform>();
                removeBtnTextRect.anchorMin = Vector2.zero;
                removeBtnTextRect.anchorMax = Vector2.one;
                removeBtnTextRect.offsetMin = Vector2.zero;
                removeBtnTextRect.offsetMax = Vector2.zero;
                
                var removeBtnTextComponent = removeBtnTextObj.AddComponent<Text>();
                removeBtnTextComponent.text = "Remove";
                FontHelper.SetSafeFont(removeBtnTextComponent);
                removeBtnTextComponent.fontSize = 7;
                removeBtnTextComponent.color = Color.white;
                removeBtnTextComponent.alignment = TextAnchor.MiddleCenter;
                
                removeBtnComponent.targetGraphic = removeBtnImage;
                removeBtnComponent.onClick.AddListener((UnityEngine.Events.UnityAction)delegate() { RemoveTrackFromPlaylist(index); });
            }
        }
        
        private List<string> GetTracksForCurrentSource()
        {
            // Get actual tracks from manager
            var tracks = new List<string>();
            
            try
            {
                LoggingSystem.Debug($"Getting tracks for current source: {currentTab}", "UI");
                
                var allTracks = manager?.GetAllTracks();
                if (allTracks != null && allTracks.Count > 0)
                {
                    LoggingSystem.Debug($"Found {allTracks.Count} tracks from manager", "UI");
                    foreach (var track in allTracks)
                    {
                        // Format as "Title - Artist" or just title if artist is empty
                        string trackDisplay = !string.IsNullOrEmpty(track.artist) 
                            ? $"{track.title} - {track.artist}"
                            : track.title;
                        tracks.Add(trackDisplay);
                    }
                }
                else
                {
                    LoggingSystem.Debug("No tracks found from manager", "UI");
                    
                    // For YouTube tab, check if we have a current playlist but manager has no tracks
                    if (currentTab == MusicSourceType.YouTube && currentYouTubePlaylist != null)
                    {
                        LoggingSystem.Debug($"YouTube playlist '{currentYouTubePlaylist.name}' has {currentYouTubePlaylist.songs.Count} songs but manager has no tracks", "UI");
                        
                        // If playlist has songs but manager doesn't, show playlist songs directly
                        if (currentYouTubePlaylist.songs.Count > 0)
                        {
                            foreach (var song in currentYouTubePlaylist.songs)
                            {
                                string trackDisplay = !string.IsNullOrEmpty(song.artist) 
                                    ? $"{song.title} - {song.artist}"
                                    : song.title ?? "Unknown Title";
                                tracks.Add(trackDisplay);
                            }
                            LoggingSystem.Debug($"Added {tracks.Count} tracks from YouTube playlist directly", "UI");
                        }
                    }
                }
                
                LoggingSystem.Debug($"Returning {tracks.Count} tracks for display", "UI");
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
        
        private void RemoveTrackFromPlaylist(int index)
        {
            LoggingSystem.Info($"Removing track {index} from YouTube playlist", "UI");
            
            try
            {
                if (currentYouTubePlaylist == null)
                {
                    LoggingSystem.Warning("No current YouTube playlist to remove from", "UI");
                    return;
                }
                
                if (index < 0 || index >= currentYouTubePlaylist.songs.Count)
                {
                    LoggingSystem.Warning($"Invalid track index {index} for playlist with {currentYouTubePlaylist.songs.Count} songs", "UI");
                    return;
                }
                
                var songToRemove = currentYouTubePlaylist.songs[index];
                var videoId = songToRemove.GetVideoId();
                var songTitle = songToRemove.title;
                
                if (RemoveSongFromCurrentYouTubePlaylist(videoId))
                {
                    LoggingSystem.Info($"✅ Successfully removed '{songTitle}' from playlist and auto-saved", "UI");
                    
                    // Refresh the playlist display
                    if (isPlaylistOpen)
                    {
                        ClosePlaylist();
                        OpenPlaylist();
                    }
                }
                else
                {
                    LoggingSystem.Warning($"❌ Failed to remove '{songTitle}' from playlist", "UI");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error removing track from playlist: {ex.Message}", "UI");
            }
        }
        
        private void ShowAddToPlaylistPopup(int songIndex)
        {
            LoggingSystem.Info($"Showing Add to Playlist popup for song index {songIndex}", "UI");
            
            try
            {
                // Get song details for the selected track
                var tracks = GetTracksForCurrentSource();
                if (songIndex < 0 || songIndex >= tracks.Count)
                {
                    LoggingSystem.Warning($"Invalid song index {songIndex}", "UI");
                    return;
                }
                
                // Get the song details from the current YouTube playlist
                if (currentYouTubePlaylist == null || songIndex >= currentYouTubePlaylist.songs.Count)
                {
                    LoggingSystem.Warning("No current YouTube playlist or invalid index", "UI");
                    return;
                }
                
                pendingSongIndex = songIndex;
                pendingSongDetails = currentYouTubePlaylist.songs[songIndex];
                
                if (isAddToPlaylistPopupOpen)
                {
                    CloseAddToPlaylistPopup();
                }
                
                CreateAddToPlaylistPopup();
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error showing add to playlist popup: {ex.Message}", "UI");
            }
        }
        
        private void CreateAddToPlaylistPopup()
        {
            try
            {
                // Find our app's container like the playlist popup
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
                    LoggingSystem.Error("No app Container found for add to playlist popup! This will cause UI bleeding.", "UI");
                    return;
                }
                
                LoggingSystem.Info($"Found app Container for add to playlist popup: {appContainer.name}", "UI");
                
                // Create popup background
                addToPlaylistPopup = new GameObject("AddToPlaylistPopup");
                addToPlaylistPopup.transform.SetParent(appContainer, false);
                
                // Full screen background with transparency
                var popupRect = addToPlaylistPopup.AddComponent<RectTransform>();
                popupRect.anchorMin = Vector2.zero;
                popupRect.anchorMax = Vector2.one;
                popupRect.offsetMin = Vector2.zero;
                popupRect.offsetMax = Vector2.zero;
                popupRect.anchoredPosition = Vector2.zero;
                popupRect.sizeDelta = Vector2.zero;
                
                // Semi-transparent background
                var backgroundImage = addToPlaylistPopup.AddComponent<Image>();
                
                // Make sure popup appears on top within our container
                addToPlaylistPopup.transform.SetAsLastSibling();
                backgroundImage.color = new Color(0f, 0f, 0f, 0.5f);
                
                // // Click background to close
                // var backgroundButton = addToPlaylistPopup.AddComponent<Button>();
                // backgroundButton.targetGraphic = backgroundImage;
                // backgroundButton.onClick.AddListener((UnityEngine.Events.UnityAction)(CloseAddToPlaylistPopup));
                
                // Create the main popup panel
                var popup = new GameObject("Popup");
                popup.transform.SetParent(addToPlaylistPopup.transform, false);
                
                var panelRect = popup.AddComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.15f, 0.15f);
                panelRect.anchorMax = new Vector2(0.95f, 0.85f);
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                
                var panelImage = popup.AddComponent<Image>();
                panelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
                
                // Title bar
                var titleBar = new GameObject("TitleBar");
                titleBar.transform.SetParent(popup.transform, false);
                
                var titleBarRect = titleBar.AddComponent<RectTransform>();
                titleBarRect.anchorMin = new Vector2(0f, 0.9f);
                titleBarRect.anchorMax = new Vector2(1f, 1f);
                titleBarRect.offsetMin = Vector2.zero;
                titleBarRect.offsetMax = Vector2.zero;
                
                var titleBarImage = titleBar.AddComponent<Image>();
                titleBarImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                
                // Title text
                var titleText = new GameObject("TitleText");
                titleText.transform.SetParent(titleBar.transform, false);
                
                var titleTextRect = titleText.AddComponent<RectTransform>();
                titleTextRect.anchorMin = new Vector2(0.05f, 0f);
                titleTextRect.anchorMax = new Vector2(0.85f, 1f);
                titleTextRect.offsetMin = Vector2.zero;
                titleTextRect.offsetMax = Vector2.zero;
                
                var titleTextComponent = titleText.AddComponent<Text>();
                titleTextComponent.text = $"Add '{pendingSongDetails?.title ?? "Song"}' to Playlist";
                FontHelper.SetSafeFont(titleTextComponent);
                titleTextComponent.fontSize = 14;
                titleTextComponent.color = Color.white;
                titleTextComponent.alignment = TextAnchor.MiddleLeft;
                titleTextComponent.fontStyle = FontStyle.Bold;
                
                // Close button
                var closeButton = new GameObject("CloseButton");
                closeButton.transform.SetParent(titleBar.transform, false);
                
                var closeBtnRect = closeButton.AddComponent<RectTransform>();
                closeBtnRect.anchorMin = new Vector2(0.9f, 0.15f);
                closeBtnRect.anchorMax = new Vector2(0.98f, 0.85f);
                closeBtnRect.offsetMin = Vector2.zero;
                closeBtnRect.offsetMax = Vector2.zero;
                
                var closeBtnComponent = closeButton.AddComponent<Button>();
                var closeBtnImage = closeButton.AddComponent<Image>();
                closeBtnImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
                
                var closeBtnText = new GameObject("Text");
                closeBtnText.transform.SetParent(closeButton.transform, false);
                var closeBtnTextRect = closeBtnText.AddComponent<RectTransform>();
                closeBtnTextRect.anchorMin = Vector2.zero;
                closeBtnTextRect.anchorMax = Vector2.one;
                closeBtnTextRect.offsetMin = Vector2.zero;
                closeBtnTextRect.offsetMax = Vector2.zero;
                
                var closeBtnTextComponent = closeBtnText.AddComponent<Text>();
                closeBtnTextComponent.text = "✕";
                FontHelper.SetSafeFont(closeBtnTextComponent);
                closeBtnTextComponent.fontSize = 12;
                closeBtnTextComponent.color = Color.white;
                closeBtnTextComponent.alignment = TextAnchor.MiddleCenter;
                closeBtnTextComponent.fontStyle = FontStyle.Bold;
                
                closeBtnComponent.targetGraphic = closeBtnImage;
                closeBtnComponent.onClick.AddListener((UnityEngine.Events.UnityAction)CloseAddToPlaylistPopup);
                
                // Content area with scroll view
                var contentArea = new GameObject("ContentArea");
                contentArea.transform.SetParent(popup.transform, false);
                
                var contentAreaRect = contentArea.AddComponent<RectTransform>();
                contentAreaRect.anchorMin = new Vector2(0.05f, 0.05f);
                contentAreaRect.anchorMax = new Vector2(0.95f, 0.85f);
                contentAreaRect.offsetMin = Vector2.zero;
                contentAreaRect.offsetMax = Vector2.zero;
                
                // Create scroll view for playlists
                CreatePlaylistScrollView(contentArea);
                
                // Disable main popup interaction
                if (playlistPopup != null)
                {
                    var mainPopupButtons = playlistPopup.GetComponentsInChildren<Button>();
                    foreach (var btn in mainPopupButtons)
                    {
                        btn.interactable = false;
                    }
                }
                
                isAddToPlaylistPopupOpen = true;
                LoggingSystem.Info("Add to Playlist popup created successfully", "UI");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating add to playlist popup: {ex.Message}", "UI");
            }
        }
        
        private void CreatePlaylistScrollView(GameObject parent)
        {
            try
            {
                // Create scroll view
                var scrollView = new GameObject("ScrollView");
                scrollView.transform.SetParent(parent.transform, false);
                
                var scrollRect = scrollView.AddComponent<ScrollRect>();
                var scrollRectTransform = scrollView.GetComponent<RectTransform>();
                scrollRectTransform.anchorMin = Vector2.zero;
                scrollRectTransform.anchorMax = Vector2.one;
                scrollRectTransform.offsetMin = Vector2.zero;
                scrollRectTransform.offsetMax = Vector2.zero;
                
                // Viewport
                var viewport = new GameObject("Viewport");
                viewport.transform.SetParent(scrollView.transform, false);
                
                var viewportRect = viewport.AddComponent<RectTransform>();
                viewportRect.anchorMin = Vector2.zero;
                viewportRect.anchorMax = Vector2.one;
                viewportRect.offsetMin = Vector2.zero;
                viewportRect.offsetMax = Vector2.zero;
                
                var viewportImage = viewport.AddComponent<Image>();
                viewportImage.color = new Color(0, 0, 0, 0.01f); // Transparent but needed for Mask

                var viewportMask = viewport.AddComponent<Mask>();
                viewportMask.showMaskGraphic = false;

                // // Add scrollbar
                // var scrollbar = new GameObject("Scrollbar");
                // scrollbar.transform.SetParent(scrollView.transform, false);
                // var scrollbarRect = scrollbar.AddComponent<RectTransform>();
                // scrollbarRect.anchorMin = new Vector2(1f, 0f);
                // scrollbarRect.anchorMax = new Vector2(1f, 1f);
                // scrollbarRect.pivot = new Vector2(1f, 1f);
                // scrollbarRect.sizeDelta = new Vector2(20f, 0f);
                
                // var scrollbarImage = scrollbar.AddComponent<Image>();
                // scrollbarImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                
                // var scrollbarComponent = scrollbar.AddComponent<Scrollbar>();
                // scrollbarComponent.direction = Scrollbar.Direction.BottomToTop;
                
                // // Add sliding area
                // var slidingArea = new GameObject("SlidingArea");
                // slidingArea.transform.SetParent(scrollbar.transform, false);
                // var slidingAreaRect = slidingArea.AddComponent<RectTransform>();
                // slidingAreaRect.anchorMin = Vector2.zero;
                // slidingAreaRect.anchorMax = Vector2.one;
                // slidingAreaRect.sizeDelta = Vector2.zero;
                
                // // Add handle
                // var handle = new GameObject("Handle");
                // handle.transform.SetParent(slidingArea.transform, false);
                // var handleRect = handle.AddComponent<RectTransform>();
                // handleRect.sizeDelta = Vector2.zero;
                
                // var handleImage = handle.AddComponent<Image>();
                // handleImage.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
                
                // scrollbarComponent.handleRect = handleRect;
                // scrollbarComponent.targetGraphic = handleImage;
                
                // // Set up the scroll rect to use our scrollbar
                // scrollRect.verticalScrollbar = scrollbarComponent;
                
                // Content
                var content = new GameObject("Content");
                content.transform.SetParent(viewport.transform, false);
                
                var contentRect = content.AddComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0f, 1f);
                contentRect.anchorMax = new Vector2(1f, 1f);
                contentRect.pivot = new Vector2(0.5f, 1f);
                
                var contentLayout = content.AddComponent<VerticalLayoutGroup>();
                contentLayout.spacing = 5f;  // Smaller spacing
                contentLayout.padding = new RectOffset(10, 10, 10, 10);  // Smaller padding

                var contentFitter = content.AddComponent<ContentSizeFitter>();
                contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                scrollRect.content = contentRect;
                scrollRect.viewport = viewportRect;
                scrollRect.vertical = true;
                scrollRect.horizontal = false;
                
                // Populate with playlists
                PopulatePlaylistSelection(content);

                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

                scrollRect.movementType = ScrollRect.MovementType.Clamped;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating playlist scroll view: {ex.Message}", "UI");
            }
        }
        
        private void PopulatePlaylistSelection(GameObject content)
        {
            try
            {
                // Get all available playlists
                var allPlaylists = YouTubePlaylistManager.GetAllPlaylists();
                LoggingSystem.Debug($"Found {allPlaylists.Count} total playlists", "UI");
                
                if (allPlaylists == null || allPlaylists.Count == 0)
                {
                    // Show "no playlists" message
                    LoggingSystem.Debug("No playlists available, showing message", "UI");
                    LoggingSystem.Debug("Creating 'No Playlists' message", "UI");
                    var noPlaylistsMessage = new GameObject("NoPlaylistsMessage");
                    noPlaylistsMessage.transform.SetParent(content.transform, false);
                    
                    var messageLayout = noPlaylistsMessage.AddComponent<LayoutElement>();
                    messageLayout.minHeight = 60f;
                    messageLayout.preferredHeight = 60f;
                    
                    var messageText = noPlaylistsMessage.AddComponent<Text>();
                    messageText.text = "No other playlists available.\nCreate a new playlist first.";
                    FontHelper.SetSafeFont(messageText);
                    messageText.fontSize = 12;
                    messageText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    messageText.alignment = TextAnchor.MiddleCenter;
                    messageText.fontStyle = FontStyle.Italic;
                    
                    return;
                }
                LoggingSystem.Debug($"Populating playlist selection with {allPlaylists.Count} playlists", "UI");
                
                // Filter out current playlist
                var currentPlaylistId = currentYouTubePlaylist?.id ?? "";
                LoggingSystem.Debug($"Current playlist ID: {currentPlaylistId}", "UI");
                LoggingSystem.Debug("Filtering out current playlist from selection", "UI");
                // Get all playlists except the current one
                var otherPlaylists = allPlaylists.Where(p => p.id != currentPlaylistId).ToList();
                LoggingSystem.Debug($"Found {otherPlaylists.Count} other playlists", "UI");
                
                if (otherPlaylists == null || otherPlaylists.Count == 0)
                {
                    // Show "no other playlists" message
                    LoggingSystem.Debug("No other playlists available, showing message", "UI");
                    var noOtherPlaylistsMessage = new GameObject("NoOtherPlaylistsMessage");
                    noOtherPlaylistsMessage.transform.SetParent(content.transform, false);
                    
                    var messageLayout = noOtherPlaylistsMessage.AddComponent<LayoutElement>();
                    messageLayout.minHeight = 60f;
                    messageLayout.preferredHeight = 60f;
                    
                    var messageText = noOtherPlaylistsMessage.AddComponent<Text>();
                    messageText.text = "No other playlists available.\nThis song is only in the current playlist.";
                    FontHelper.SetSafeFont(messageText);
                    messageText.fontSize = 12;
                    messageText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    messageText.alignment = TextAnchor.MiddleCenter;
                    messageText.fontStyle = FontStyle.Italic;
                    
                    return;
                }
                
                // Create playlist items
                foreach (var playlistInfo in otherPlaylists)
                {
                    LoggingSystem.Debug($"Creating playlist selection item for {playlistInfo.name} ({playlistInfo.id})", "UI");
                    // Create a selection item for each playlist
                    CreatePlaylistSelectionItem(content, playlistInfo);
                }
                
                LoggingSystem.Info($"Created {otherPlaylists.Count} playlist selection items", "UI");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error populating playlist selection: {ex.Message}", "UI");
            }
        }
        
        private void CreatePlaylistSelectionItem(GameObject parent, YouTubePlaylistInfo playlistInfo)
        {
            try
            {
                var playlistItem = new GameObject($"PlaylistItem_{playlistInfo.id}");
                playlistItem.transform.SetParent(parent.transform, false);
                
                var itemRect = playlistItem.AddComponent<RectTransform>();
                var itemLayout = playlistItem.AddComponent<LayoutElement>();
                itemLayout.minHeight = 40f;
                itemLayout.preferredHeight = 40f;
                
                
                var itemImage = playlistItem.AddComponent<Image>();
                itemImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                
                // Playlist info text
                var infoText = new GameObject("InfoText");
                infoText.transform.SetParent(playlistItem.transform, false);
                
                var infoTextRect = infoText.AddComponent<RectTransform>();
                infoTextRect.anchorMin = new Vector2(0.05f, 0f);
                infoTextRect.anchorMax = new Vector2(0.8f, 1f);
                infoTextRect.offsetMin = Vector2.zero;
                infoTextRect.offsetMax = Vector2.zero;
                
                var infoTextComponent = infoText.AddComponent<Text>();
                infoTextComponent.text = $"{playlistInfo.name}\n{playlistInfo.downloadedCount}/{playlistInfo.songCount} songs";
                FontHelper.SetSafeFont(infoTextComponent);
                infoTextComponent.fontSize = 11;
                infoTextComponent.color = Color.white;
                infoTextComponent.alignment = TextAnchor.MiddleLeft;
                
                // Check if song already exists in this playlist
                var targetPlaylist = YouTubePlaylistManager.LoadPlaylist(playlistInfo.id);
                bool songAlreadyExists = targetPlaylist?.ContainsSong(pendingSongDetails?.GetVideoId() ?? "") ?? false;
                
                // Action button
                var actionButton = new GameObject("ActionButton");
                actionButton.transform.SetParent(playlistItem.transform, false);
                
                var actionBtnRect = actionButton.AddComponent<RectTransform>();
                actionBtnRect.anchorMin = new Vector2(0.85f, 0.2f);
                actionBtnRect.anchorMax = new Vector2(0.95f, 0.8f);
                actionBtnRect.offsetMin = Vector2.zero;
                actionBtnRect.offsetMax = Vector2.zero;
                
                var actionBtnComponent = actionButton.AddComponent<Button>();
                var actionBtnImage = actionButton.AddComponent<Image>();
                
                var actionBtnText = new GameObject("Text");
                actionBtnText.transform.SetParent(actionButton.transform, false);
                var actionBtnTextRect = actionBtnText.AddComponent<RectTransform>();
                actionBtnTextRect.anchorMin = Vector2.zero;
                actionBtnTextRect.anchorMax = Vector2.one;
                actionBtnTextRect.offsetMin = Vector2.zero;
                actionBtnTextRect.offsetMax = Vector2.zero;
                
                var actionBtnTextComponent = actionBtnText.AddComponent<Text>();
                FontHelper.SetSafeFont(actionBtnTextComponent);
                actionBtnTextComponent.fontSize = 10;
                actionBtnTextComponent.color = Color.white;
                actionBtnTextComponent.alignment = TextAnchor.MiddleCenter;
                actionBtnTextComponent.fontStyle = FontStyle.Bold;
                
                if (songAlreadyExists)
                {
                    // Song already exists - show "Already Added" (disabled)
                    actionBtnTextComponent.text = "Already Added";
                    actionBtnImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                    actionBtnComponent.interactable = false;
                }
                else
                {
                    // Song can be added - show "Add" button
                    actionBtnTextComponent.text = "Add";
                    actionBtnImage.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
                    actionBtnComponent.targetGraphic = actionBtnImage;
                    actionBtnComponent.onClick.AddListener((UnityEngine.Events.UnityAction)delegate(){ AddSongToPlaylist(playlistInfo.id, playlistInfo.name) ; });
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating playlist selection item for {playlistInfo.name}: {ex.Message}", "UI");
            }
        }
        
        private void AddSongToPlaylist(string playlistId, string playlistName)
        {
            try
            {
                if (pendingSongDetails == null)
                {
                    LoggingSystem.Warning("No pending song details to add", "UI");
                    return;
                }
                
                LoggingSystem.Info($"Adding song '{pendingSongDetails.title}' to playlist '{playlistName}'", "UI");
                
                // Load the target playlist
                var targetPlaylist = YouTubePlaylistManager.LoadPlaylist(playlistId);
                if (targetPlaylist == null)
                {
                    LoggingSystem.Error($"Failed to load playlist {playlistId}", "UI");
                    return;
                }
                
                // Add the song
                bool added = targetPlaylist.AddSong(pendingSongDetails);
                if (added)
                {
                    // Save the playlist
                    bool saved = YouTubePlaylistManager.SavePlaylist(targetPlaylist);
                    if (saved)
                    {
                        LoggingSystem.Info($"✅ Successfully added '{pendingSongDetails.title}' to playlist '{playlistName}'", "UI");

                        // Update the add to playlist popup text to show "Already Added"
                        UpdateAddToPlaylistPopupText(playlistId, playlistName);
                        
                        // Keep popup open so user can add to other playlists if desired
                    }
                    else
                    {
                        LoggingSystem.Error($"Failed to save playlist '{playlistName}' after adding song", "UI");
                    }
                }
                else
                {
                    LoggingSystem.Warning($"Song '{pendingSongDetails.title}' already exists in playlist '{playlistName}'", "UI");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error adding song to playlist: {ex.Message}", "UI");
            }
        }
        
        private void UpdateAddToPlaylistPopupText(string playlistId, string playlistName)
        {
            if (addToPlaylistPopup == null)
            {
                LoggingSystem.Error("Add to playlist popup not found", "UI");
                return;
            }
            
            // Find the content through proper hierarchy: Popup -> ContentArea -> ScrollView -> Viewport -> Content
            var popup = addToPlaylistPopup.transform.Find("Popup");
            var contentArea = popup?.Find("ContentArea");
            var scrollView = contentArea?.Find("ScrollView");
            var viewport = scrollView?.Find("Viewport");
            var content = viewport?.Find("Content");
            
            if (content != null)
            {   
                var popupItem = content.Find($"PlaylistItem_{playlistId}");
                if (popupItem != null)
                {   
                    LoggingSystem.Debug($"Found add to playlist popup item with id {playlistId}", "UI");
                    // first find the action button gameobject
                    var actionBtn = popupItem.transform.Find("ActionButton");
                    if (actionBtn != null)
                    {
                        LoggingSystem.Debug("Found action button in playlist item", "UI");
                        // then find the text gameobject
                        var actionBtnText = actionBtn.transform.Find("Text");
                        if (actionBtnText != null)
                        {
                            LoggingSystem.Debug("Found text component in action button", "UI");
                            var actionBtnTextComponent = actionBtnText.GetComponent<Text>();
                            actionBtnTextComponent.text = "Already Added";
                            actionBtnTextComponent.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                        }
                        else
                        {
                            LoggingSystem.Error("Text component not found in action button", "UI");
                        }
                        
                        // get the image component
                        var actionBtnImage = actionBtn.GetComponent<Image>();
                        if (actionBtnImage != null)
                        {
                            LoggingSystem.Debug("Found image component in action button", "UI");
                            actionBtnImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                        }
                        else
                        {
                            LoggingSystem.Error("Image component not found in action button", "UI");
                        }
                        
                        // get the button component
                        var actionBtnButton = actionBtn.GetComponent<Button>();
                        if (actionBtnButton != null)
                        {
                            LoggingSystem.Debug("Found button component in action button", "UI");
                            actionBtnButton.interactable = false;
                        }
                        else
                        {
                            LoggingSystem.Error("Button component not found in action button", "UI");
                        }
                    }
                    else
                    {
                        LoggingSystem.Error("Action button not found in playlist item", "UI");
                    }
                }
                else
                {
                    LoggingSystem.Error($"Add to playlist popup item with id {playlistId} not found", "UI");
                }
            }
            else
            {
                LoggingSystem.Error("Add to playlist popup content not found", "UI");
            }
        }
        
        private void CloseAddToPlaylistPopup()
        {
            try
            {
                if (addToPlaylistPopup != null)
                {
                    GameObject.Destroy(addToPlaylistPopup);
                    addToPlaylistPopup = null;
                }
                
                // Re-enable main popup interaction
                if (playlistPopup != null)
                {
                    var mainPopupButtons = playlistPopup.GetComponentsInChildren<Button>();
                    foreach (var btn in mainPopupButtons)
                    {
                        btn.interactable = true;
                    }
                }
                
                isAddToPlaylistPopupOpen = false;
                pendingSongIndex = -1;
                pendingSongDetails = null;
                
                LoggingSystem.Info("Add to Playlist popup closed", "UI");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error closing add to playlist popup: {ex.Message}", "UI");
            }
        }
        
        public void UpdateForTab(MusicSourceType newTab)
        {
            var previousTab = currentTab;
            LoggingSystem.Info($"🔄 Updating playlist component for tab: {previousTab} -> {newTab}", "UI");
            
            currentTab = newTab;
            
            // Handle external controls based on tab change
            if (previousTab != MusicSourceType.YouTube && newTab == MusicSourceType.YouTube)
            {
                // Switching TO YouTube tab - create external controls
                LoggingSystem.Info("➡️ Switching to YouTube tab - creating external controls", "UI");
                
                // Recreate playlist button with smaller size and external controls
                if (playlistButton != null)
                {
                    GameObject.Destroy(playlistButton.gameObject);
                    playlistButton = null;
                    buttonText = null;
                }
                CreatePlaylistButton();
                
                // Initialize YouTube playlists and load into backend
                InitializeYouTubePlaylists();
                
                LoggingSystem.Info("✅ YouTube tab initialization completed", "UI");
            }
            else if (previousTab == MusicSourceType.YouTube && newTab != MusicSourceType.YouTube)
            {
                // Switching FROM YouTube tab - destroy external controls
                LoggingSystem.Info("⬅️ Switching from YouTube tab - destroying external controls", "UI");
                
                // Destroy external controls
                if (externalPlaylistDropdown != null)
                {
                    GameObject.Destroy(externalPlaylistDropdown.gameObject);
                    externalPlaylistDropdown = null;
                }
                if (managePlaylistsButton != null)
                {
                    GameObject.Destroy(managePlaylistsButton.gameObject);
                    managePlaylistsButton = null;
                }
                if (managePlaylistsPopup != null)
                {
                    GameObject.Destroy(managePlaylistsPopup);
                    managePlaylistsPopup = null;
                }
                
                // Recreate playlist button with full size
                if (playlistButton != null)
                {
                    GameObject.Destroy(playlistButton.gameObject);
                    playlistButton = null;
                    buttonText = null;
                }
                CreatePlaylistButton();
                
                LoggingSystem.Info("✅ Switched away from YouTube tab", "UI");
            }
            else if (newTab == MusicSourceType.YouTube)
            {
                // Already on YouTube tab - just refresh playlists and ensure backend is synced
                LoggingSystem.Info("🔄 Already on YouTube tab - refreshing playlists and syncing backend", "UI");
                RefreshYouTubePlaylists();
                
                // Ensure the current playlist is loaded in the backend
                if (currentYouTubePlaylist != null)
                {
                    LoggingSystem.Debug($"Re-syncing current playlist '{currentYouTubePlaylist.name}' with backend", "UI");
                    UpdateYouTubeManagerPlaylist();
                }
            }
            
            // Update playlist button text and appearance
            UpdatePlaylistButton();
            
            // Close any open playlist popup when switching tabs
            if (isPlaylistOpen)
            {
                ClosePlaylist();
            }
            
            LoggingSystem.Info($"🏁 Tab update completed: {previousTab} -> {newTab}", "UI");
        }
        
        private void UpdatePlaylistButton()
        {
            if (buttonText == null || playlistButton == null) return;
            
            try
            {
                var trackCount = manager?.GetTrackCount() ?? 0;
                
                var (text, color) = currentTab switch
                {
                    MusicSourceType.Jukebox => ($"📋 Jukebox Playlist ({trackCount} tracks)", new Color(0.2f, 0.7f, 0.2f, 0.8f)),
                    MusicSourceType.LocalFolder => ($"📋 Local Playlist ({trackCount} tracks)", new Color(0.2f, 0.4f, 0.8f, 0.8f)),
                    MusicSourceType.YouTube => ($"📋 YouTube Playlist ({trackCount} tracks)", new Color(0.8f, 0.2f, 0.2f, 0.8f)),
                    _ => ($"📋 Playlist ({trackCount} tracks)", new Color(0.5f, 0.5f, 0.5f, 0.8f))
                };
                
                buttonText.text = text;
                playlistButton.GetComponent<Image>().color = color;
                
                // LoggingSystem.Debug($"Playlist button updated for {currentTab}: {trackCount} tracks", "UI");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to update playlist button: {ex.Message}", "UI");
                buttonText.text = $"📋 Playlist Error";
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
        
        private void InitializeYouTubePlaylists()
        {
            try
            {
                LoggingSystem.Info("🚀 Initializing YouTube playlists", "UI");
                
                availableYouTubePlaylists = YouTubePlaylistManager.GetAllPlaylists();
                LoggingSystem.Debug($"Found {availableYouTubePlaylists.Count} existing YouTube playlists", "UI");
                
                // If no playlists exist, try to create a default one from cache
                if (availableYouTubePlaylists.Count == 0)
                {
                    LoggingSystem.Info("No YouTube playlists found, attempting to create default playlist from cache", "UI");
                    var defaultPlaylist = YouTubePlaylistManager.CreateDefaultPlaylistFromCache();
                    
                    if (defaultPlaylist != null)
                    {
                        availableYouTubePlaylists = YouTubePlaylistManager.GetAllPlaylists();
                        LoggingSystem.Info($"✅ Created default YouTube playlist '{defaultPlaylist.name}' with {defaultPlaylist.songs.Count} songs", "UI");
                    }
                    else
                    {
                        LoggingSystem.Warning("❌ Failed to create default playlist from cache", "UI");
                    }
                }
                
                // Auto-select first playlist and load it into backend
                if (availableYouTubePlaylists.Count > 0)
                {
                    var firstPlaylistId = availableYouTubePlaylists[0].id;
                    LoggingSystem.Info($"🎵 Auto-selecting first playlist: {availableYouTubePlaylists[0].name}", "UI");
                    
                    SelectYouTubePlaylist(firstPlaylistId);
                    
                    if (currentYouTubePlaylist != null)
                    {
                        LoggingSystem.Info($"✅ Successfully initialized with playlist '{currentYouTubePlaylist.name}' containing {currentYouTubePlaylist.songs.Count} songs", "UI");
                    }
                    else
                    {
                        LoggingSystem.Error($"❌ Failed to load first playlist '{firstPlaylistId}'", "UI");
                    }
                }
                else
                {
                    LoggingSystem.Warning("⚠️ No YouTube playlists available after initialization", "UI");
                }
                
                LoggingSystem.Info($"🏁 YouTube playlist initialization completed with {availableYouTubePlaylists.Count} playlists", "UI");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"❌ Error initializing YouTube playlists: {ex.Message}", "UI");
                LoggingSystem.Error($"Stack trace: {ex.StackTrace}", "UI");
            }
        }
        
        private void SelectYouTubePlaylist(string playlistId)
        {
            try
            {
                currentYouTubePlaylist = YouTubePlaylistManager.LoadPlaylist(playlistId);
                if (currentYouTubePlaylist != null)
                {
                    LoggingSystem.Info($"Selected YouTube playlist: {currentYouTubePlaylist.name} with {currentYouTubePlaylist.songs.Count} songs", "UI");
                    
                    // Update the manager with the playlist songs if this is the YouTube tab
                    if (currentTab == MusicSourceType.YouTube)
                    {
                        UpdateYouTubeManagerPlaylist();
                    }
                }
                else
                {
                    LoggingSystem.Error($"Failed to load YouTube playlist: {playlistId}", "UI");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error selecting YouTube playlist {playlistId}: {ex.Message}", "UI");
            }
        }
        
        private void UpdateYouTubeManagerPlaylist()
        {
            try
            {
                if (currentYouTubePlaylist == null || manager == null) 
                {
                    LoggingSystem.Debug("No current playlist or manager available for update", "UI");
                    return;
                }
                
                LoggingSystem.Info($"🔄 Updating manager with playlist '{currentYouTubePlaylist.name}' containing {currentYouTubePlaylist.songs.Count} songs", "UI");
                
                // Clear existing YouTube playlist completely
                manager.ClearYouTubePlaylist();
                LoggingSystem.Debug("✅ Cleared existing YouTube playlist from manager", "UI");
                
                // Load the new playlist if it has songs
                if (currentYouTubePlaylist.songs.Count > 0)
                {
                    manager.LoadYouTubePlaylist(currentYouTubePlaylist.songs);
                    LoggingSystem.Info($"✅ Loaded {currentYouTubePlaylist.songs.Count} songs from playlist '{currentYouTubePlaylist.name}' into manager", "UI");
                }
                else
                {
                    LoggingSystem.Info($"ℹ️ Playlist '{currentYouTubePlaylist.name}' is empty - no songs to load", "UI");
                }
                
                // Update the playlist button to reflect the new track count
                UpdatePlaylistButton();
                
                LoggingSystem.Info($"🎵 YouTube playlist update completed: {currentYouTubePlaylist.songs.Count} tracks loaded", "UI");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"❌ Error updating manager with YouTube playlist: {ex.Message}", "UI");
                LoggingSystem.Error($"Stack trace: {ex.StackTrace}", "UI");
            }
        }
        
        private void OnYouTubePlaylistCreated(YouTubePlaylist playlist)
        {
            LoggingSystem.Debug($"YouTube playlist created event: {playlist.name}", "UI");
            RefreshYouTubePlaylists();
        }
        
        private void OnYouTubePlaylistUpdated(YouTubePlaylist playlist)
        {
            LoggingSystem.Debug($"YouTube playlist updated event: {playlist.name}", "UI");
            RefreshYouTubePlaylists();
            
            // If this is the current playlist, update the manager
            if (currentYouTubePlaylist?.id == playlist.id)
            {
                currentYouTubePlaylist = playlist;
                if (currentTab == MusicSourceType.YouTube)
                {
                    UpdateYouTubeManagerPlaylist();
                }
            }
        }
        
        private void OnYouTubePlaylistDeleted(string playlistId)
        {
            LoggingSystem.Debug($"YouTube playlist deleted event: {playlistId}", "UI");
            RefreshYouTubePlaylists();
            
            // If the current playlist was deleted, select another one
            if (currentYouTubePlaylist?.id == playlistId)
            {
                currentYouTubePlaylist = null;
                if (availableYouTubePlaylists.Count > 0)
                {
                    SelectYouTubePlaylist(availableYouTubePlaylists[0].id);
                }
            }
        }
        
        private void OnYouTubePlaylistIndexChanged()
        {
            LoggingSystem.Debug("YouTube playlist index changed event", "UI");
            RefreshYouTubePlaylists();
        }
        
        private void RefreshYouTubePlaylists()
        {
            try
            {
                availableYouTubePlaylists = YouTubePlaylistManager.GetAllPlaylists();
                
                // Update internal dropdown if it exists and we're on YouTube tab
                if (currentTab == MusicSourceType.YouTube && youTubePlaylistDropdown != null)
                {
                    UpdateYouTubePlaylistDropdown();
                }
                
                // Update playlist button
                UpdatePlaylistButton();
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error refreshing YouTube playlists: {ex.Message}", "UI");
            }
        }
        

        
        private void CreateYouTubePlaylistManagement(GameObject panel)
        {
            LoggingSystem.Info("Creating YouTube playlist management UI", "UI");
            
            var managementContainer = new GameObject("YouTubePlaylistManagement");
            managementContainer.transform.SetParent(panel.transform, false);
            
            var managementRect = managementContainer.AddComponent<RectTransform>();
            managementRect.anchorMin = new Vector2(0f, 0.76f);  // Move up slightly 
            managementRect.anchorMax = new Vector2(1f, 0.88f);  // Make taller
            managementRect.offsetMin = new Vector2(10f, 0f);
            managementRect.offsetMax = new Vector2(-10f, 0f);
            
            // Add background to make it visible
            var managementBg = managementContainer.AddComponent<Image>();
            managementBg.color = new Color(0.2f, 0.2f, 0.2f, 0.3f); // Semi-transparent background
            
            LoggingSystem.Info("Created management container", "UI");
            
            // Playlist dropdown (70% width)
            CreateYouTubePlaylistDropdown(managementContainer);
            
            // Action buttons (30% width)
            CreateYouTubePlaylistButtons(managementContainer);
            
            LoggingSystem.Info("YouTube playlist management UI created successfully", "UI");
        }
        
        private void CreateYouTubePlaylistDropdown(GameObject parent)
        {
            LoggingSystem.Info("Creating YouTube playlist dropdown", "UI");
            
            var dropdownObj = new GameObject("PlaylistDropdown");
            dropdownObj.transform.SetParent(parent.transform, false);
            
            var dropdownRect = dropdownObj.AddComponent<RectTransform>();
            dropdownRect.anchorMin = new Vector2(0f, 0f);
            dropdownRect.anchorMax = new Vector2(0.68f, 1f);  // Slightly smaller to make room for buttons
            dropdownRect.offsetMin = new Vector2(0f, 0f);
            dropdownRect.offsetMax = new Vector2(-5f, 0f);
            
            // Background - more visible
            var dropdownBg = dropdownObj.AddComponent<Image>();
            dropdownBg.color = new Color(0.15f, 0.15f, 0.15f, 1f); // Fully opaque background
            
            youTubePlaylistDropdown = dropdownObj.AddComponent<Dropdown>();
            
            LoggingSystem.Info("Created dropdown component", "UI");
            
            // Create dropdown label with proper size
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(dropdownObj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.05f, 0f);
            labelRect.anchorMax = new Vector2(0.85f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            var labelText = labelObj.AddComponent<Text>();
            labelText.text = "Select Playlist...";
            FontHelper.SetSafeFont(labelText);
            labelText.fontSize = 11;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            // Create dropdown arrow (simple)
            var arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(dropdownObj.transform, false);
            var arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.85f, 0.3f);
            arrowRect.anchorMax = new Vector2(0.95f, 0.7f);
            arrowRect.offsetMin = Vector2.zero;
            arrowRect.offsetMax = Vector2.zero;
            
            var arrowText = arrowObj.AddComponent<Text>();
            arrowText.text = "▼";
            FontHelper.SetSafeFont(arrowText);
            arrowText.fontSize = 10;
            arrowText.color = Color.white;
            arrowText.alignment = TextAnchor.MiddleCenter;
            
            // Create proper dropdown template for options display
            CreateDropdownTemplate(dropdownObj);
            
            youTubePlaylistDropdown.captionText = labelText;
            youTubePlaylistDropdown.targetGraphic = dropdownBg;
            youTubePlaylistDropdown.onValueChanged.AddListener((UnityEngine.Events.UnityAction<int>)((int index) => OnYouTubePlaylistSelected(index)));
            
            LoggingSystem.Info("Dropdown setup complete, updating with playlists", "UI");
            
            // Update dropdown with current playlists - this should happen immediately
            UpdateYouTubePlaylistDropdown();
            
            LoggingSystem.Info($"Dropdown populated. Available playlists: {availableYouTubePlaylists.Count}", "UI");
        }
        
        private void CreateDropdownTemplate(GameObject dropdown)
        {
            // Create template for dropdown items
            var templateObj = new GameObject("Template");
            templateObj.transform.SetParent(dropdown.transform, false);
            templateObj.SetActive(false); // Template should be inactive
            
            var templateRect = templateObj.AddComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.sizeDelta = new Vector2(0f, 150f); // Height for dropdown list
            
            // Scrollable area for template
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(templateObj.transform, false);
            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            var viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            
            // Content area
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            
            // Item template
            var item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            var itemRect = item.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 0.5f);
            itemRect.anchorMax = new Vector2(1f, 0.5f);
            itemRect.sizeDelta = new Vector2(0f, 20f);
            
            var itemToggle = item.AddComponent<Toggle>();
            var itemBg = item.AddComponent<Image>();
            itemBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            itemToggle.targetGraphic = itemBg;
            
            // Item label
            var itemLabel = new GameObject("Item Label");
            itemLabel.transform.SetParent(item.transform, false);
            var itemLabelRect = itemLabel.AddComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(10f, 1f);
            itemLabelRect.offsetMax = new Vector2(-10f, -1f);
            
            var itemText = itemLabel.AddComponent<Text>();
            itemText.text = "Option";
            FontHelper.SetSafeFont(itemText);
            itemText.fontSize = 10;
            itemText.color = Color.white;
            itemText.alignment = TextAnchor.MiddleLeft;
            
            // ScrollRect for template
            var scrollRect = templateObj.AddComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            
            // Assign template to dropdown
            youTubePlaylistDropdown.template = templateRect;
            youTubePlaylistDropdown.itemText = itemText;
            
            LoggingSystem.Info("Dropdown template created", "UI");
        }
        
        private void CreateYouTubePlaylistButtons(GameObject parent)
        {
            var buttonsContainer = new GameObject("PlaylistButtons");
            buttonsContainer.transform.SetParent(parent.transform, false);
            
            var buttonsRect = buttonsContainer.AddComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0.72f, 0f);
            buttonsRect.anchorMax = new Vector2(1f, 1f);
            buttonsRect.offsetMin = Vector2.zero;
            buttonsRect.offsetMax = Vector2.zero;
            
            // Create New button (50% width)
            createPlaylistButton = CreatePlaylistButton(buttonsContainer, "Create", new Vector2(0f, 0f), new Vector2(0.5f, 1f), 
                         "New", Color.green, OnCreateYouTubePlaylistClicked);
            
            // Delete button (50% width)
            deletePlaylistButton = CreatePlaylistButton(buttonsContainer, "Delete", new Vector2(0.5f, 0f), new Vector2(1f, 1f), 
                         "Delete", Color.red, OnDeleteYouTubePlaylistClicked);
        }
        
        private Button CreatePlaylistButton(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, 
                                           string text, Color color, System.Action onClick)
        {
            var buttonObj = new GameObject($"{name}Button");
            buttonObj.transform.SetParent(parent.transform, false);
            
            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.offsetMin = new Vector2(2f, 1f);
            buttonRect.offsetMax = new Vector2(-2f, -1f);
            
            var button = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(color.r, color.g, color.b, 0.7f);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            FontHelper.SetSafeFont(buttonText);
            buttonText.fontSize = 9; // Smaller font for button text
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontStyle = FontStyle.Bold;
            
            button.targetGraphic = buttonImage;
            button.onClick.AddListener((UnityEngine.Events.UnityAction)onClick);
            
            return button;
        }
        
        private void UpdateYouTubePlaylistDropdown()
        {
            if (youTubePlaylistDropdown == null) 
            {
                LoggingSystem.Warning("youTubePlaylistDropdown is null, cannot update", "UI");
                return;
            }
            
            LoggingSystem.Info($"Updating YouTube playlist dropdown with {availableYouTubePlaylists.Count} playlists", "UI");
            
            youTubePlaylistDropdown.ClearOptions();
            
            var options = new Il2CppCollections.List<Dropdown.OptionData>();
            
            if (availableYouTubePlaylists == null || availableYouTubePlaylists.Count == 0)
            {
                LoggingSystem.Warning("No available YouTube playlists found", "UI");
                options.Add(new Dropdown.OptionData("No playlists available"));
                youTubePlaylistDropdown.AddOptions(options);
                youTubePlaylistDropdown.interactable = false;
                return;
            }
            
            foreach (var playlist in availableYouTubePlaylists)
            {
                var optionText = $"{playlist.name} ({playlist.downloadedCount}/{playlist.songCount})";
                options.Add(new Dropdown.OptionData(optionText));
                LoggingSystem.Debug($"Added playlist option: {optionText}", "UI");
            }
            
            youTubePlaylistDropdown.AddOptions(options);
            youTubePlaylistDropdown.interactable = true;
            
            LoggingSystem.Info($"Added {options.Count} options to dropdown", "UI");
            
            // Select the current playlist in the dropdown
            if (currentYouTubePlaylist != null)
            {
                LoggingSystem.Info($"Looking for current playlist '{currentYouTubePlaylist.name}' in dropdown", "UI");
                for (int i = 0; i < availableYouTubePlaylists.Count; i++)
                {
                    if (availableYouTubePlaylists[i].id == currentYouTubePlaylist.id)
                    {
                        youTubePlaylistDropdown.value = i;
                        youTubePlaylistDropdown.RefreshShownValue(); // Force UI update
                        LoggingSystem.Info($"Selected playlist at index {i}: {availableYouTubePlaylists[i].name}", "UI");
                        break;
                    }
                }
            }
            else
            {
                LoggingSystem.Warning("No current YouTube playlist selected", "UI");
            }
            
            // Update button states
            bool hasPlaylists = availableYouTubePlaylists.Count > 0;
            if (deletePlaylistButton != null) 
            {
                deletePlaylistButton.interactable = hasPlaylists;
                LoggingSystem.Debug($"Delete button interactable: {hasPlaylists}", "UI");
            }
        }
        
        private void OnYouTubePlaylistSelected(int index)
        {
            if (index >= 0 && index < availableYouTubePlaylists.Count)
            {
                var selectedPlaylistInfo = availableYouTubePlaylists[index];
                LoggingSystem.Info($"User selected playlist: {selectedPlaylistInfo.name}", "UI");
                
                // Load the selected playlist
                SelectYouTubePlaylist(selectedPlaylistInfo.id);
                
                // Update the editable name field if it exists
                if (playlistNameInput != null && currentYouTubePlaylist != null)
                {
                    playlistNameInput.text = currentYouTubePlaylist.name;
                    originalPlaylistName = currentYouTubePlaylist.name;
                    OnPlaylistNameChanged(currentYouTubePlaylist.name); // Reset save button state
                }
                
                // Refresh the playlist view if it's open
                if (isPlaylistOpen)
                {
                    // Close and reopen to refresh the track list
                    ClosePlaylist();
                    OpenPlaylist();
                }
            }
        }
        
        private void OnCreateYouTubePlaylistClicked()
        {
            // Create a simple playlist with a timestamp name
            var playlistName = $"New Playlist {DateTime.Now:HH:mm:ss}";
            
            try
            {
                var newPlaylist = YouTubePlaylistManager.CreatePlaylist(playlistName, "User created playlist from manage popup");
                if (newPlaylist != null)
                {
                    LoggingSystem.Info($"✅ Created new playlist from manage popup: {newPlaylist.name}", "UI");
                    
                    // Auto-select the newly created playlist
                    selectedPlaylistId = newPlaylist.id;
                    SelectYouTubePlaylist(newPlaylist.id);
                    
                    // Update playlist button
                    UpdatePlaylistButton();
                    
                    // Refresh the manage popup to show the new playlist
                    if (managePlaylistsPopup != null)
                    {
                        GameObject.Destroy(managePlaylistsPopup);
                        CreateManagePlaylistsPopup();
                    }
                    
                    LoggingSystem.Info($"🎉 New playlist '{newPlaylist.name}' created and auto-selected", "UI");
                }
                else
                {
                    LoggingSystem.Warning("❌ Failed to create new playlist from manage popup", "UI");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating playlist from manage popup: {ex.Message}", "UI");
            }
        }
        
        private void OnDeleteYouTubePlaylistClicked()
        {
            if (currentYouTubePlaylist == null)
            {
                return;
            }
            
            try
            {
                var playlistName = currentYouTubePlaylist.name;
                var playlistId = currentYouTubePlaylist.id;
                
                if (YouTubePlaylistManager.DeletePlaylist(playlistId))
                {
                    LoggingSystem.Info($"Deleted YouTube playlist: {playlistName}", "UI");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error deleting YouTube playlist: {ex.Message}", "UI");
            }
        }
        
        /// <summary>
        /// Add a song to the current YouTube playlist (for use by YouTube popup)
        /// </summary>
        public bool AddSongToCurrentYouTubePlaylist(SongDetails song)
        {
            if (currentYouTubePlaylist == null || song == null) return false;
            
            try
            {
                if (currentYouTubePlaylist.AddSong(song))
                {
                    YouTubePlaylistManager.SavePlaylist(currentYouTubePlaylist);
                    LoggingSystem.Info($"Added '{song.title}' to YouTube playlist '{currentYouTubePlaylist.name}'", "UI");
                    
                    // Update the manager if this is the current tab
                    if (currentTab == MusicSourceType.YouTube)
                    {
                        manager?.AddYouTubeSong(song);
                    }
                    
                    return true;
                }
                else
                {
                    LoggingSystem.Debug($"Song '{song.title}' already in playlist", "UI");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error adding song to YouTube playlist: {ex.Message}", "UI");
                return false;
            }
        }
        
        /// <summary>
        /// Remove a song from the current YouTube playlist (for use by YouTube popup)
        /// </summary>
        public bool RemoveSongFromCurrentYouTubePlaylist(string videoId)
        {
            if (currentYouTubePlaylist == null || string.IsNullOrEmpty(videoId)) return false;
            
            try
            {
                var song = currentYouTubePlaylist.GetSong(videoId);
                if (song != null && currentYouTubePlaylist.RemoveSong(videoId))
                {
                    YouTubePlaylistManager.SavePlaylist(currentYouTubePlaylist);
                    LoggingSystem.Info($"Removed '{song.title}' from YouTube playlist '{currentYouTubePlaylist.name}'", "UI");
                    
                    // Update the manager if this is the current tab
                    if (currentTab == MusicSourceType.YouTube && song.url != null)
                    {
                        manager?.RemoveYouTubeSong(song.url);
                    }
                    
                    return true;
                }
                else
                {
                    LoggingSystem.Debug($"Song with video ID '{videoId}' not found in playlist", "UI");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error removing song from YouTube playlist: {ex.Message}", "UI");
                return false;
            }
        }
        
        /// <summary>
        /// Check if a song is in the current YouTube playlist (for use by YouTube popup)
        /// </summary>
        public bool IsInCurrentYouTubePlaylist(string videoId)
        {
            return currentYouTubePlaylist?.ContainsSong(videoId) ?? false;
        }
        
        /// <summary>
        /// Get the current YouTube playlist (for use by YouTube popup)
        /// </summary>
        public YouTubePlaylist? GetCurrentYouTubePlaylist()
        {
            return currentYouTubePlaylist;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from YouTube playlist events
            YouTubePlaylistManager.OnPlaylistCreated -= OnYouTubePlaylistCreated;
            YouTubePlaylistManager.OnPlaylistUpdated -= OnYouTubePlaylistUpdated;
            YouTubePlaylistManager.OnPlaylistDeleted -= OnYouTubePlaylistDeleted;
            YouTubePlaylistManager.OnPlaylistIndexChanged -= OnYouTubePlaylistIndexChanged;
        }
        
        private void CreateEditablePlaylistHeader(GameObject parent)
        {
            // Current playlist name input (70% width)
            var nameInputObj = new GameObject("PlaylistNameInput");
            nameInputObj.transform.SetParent(parent.transform, false);
            
            var nameInputRect = nameInputObj.AddComponent<RectTransform>();
            nameInputRect.anchorMin = new Vector2(0f, 0f);
            nameInputRect.anchorMax = new Vector2(0.7f, 1f);
            nameInputRect.offsetMin = Vector2.zero;
            nameInputRect.offsetMax = new Vector2(-5f, 0f);
            
            // Input background
            var inputBg = nameInputObj.AddComponent<Image>();
            inputBg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            
            // Create input text component
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(nameInputObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 0f);
            textRect.offsetMax = new Vector2(-8f, 0f);
            
            var textComponent = textObj.AddComponent<Text>();
            FontHelper.SetSafeFont(textComponent);
            textComponent.fontSize = 14;
            textComponent.color = Color.white;
            textComponent.fontStyle = FontStyle.Bold;
            textComponent.supportRichText = false;
            
            // Create placeholder
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(nameInputObj.transform, false);
            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(8f, 0f);
            placeholderRect.offsetMax = new Vector2(-8f, 0f);
            
            var placeholderText = placeholderObj.AddComponent<Text>();
            placeholderText.text = "Enter playlist name...";
            FontHelper.SetSafeFont(placeholderText);
            placeholderText.fontSize = 14;
            placeholderText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            placeholderText.fontStyle = FontStyle.Italic;
            
            // Create InputField
            playlistNameInput = nameInputObj.AddComponent<InputField>();
            playlistNameInput.targetGraphic = inputBg;
            playlistNameInput.textComponent = textComponent;
            playlistNameInput.placeholder = placeholderText;
            playlistNameInput.characterLimit = 50;
            
            // Set current playlist name
            if (currentYouTubePlaylist != null)
            {
                playlistNameInput.text = currentYouTubePlaylist.name;
                originalPlaylistName = currentYouTubePlaylist.name;
            }
            
            // Listen for text changes
            playlistNameInput.onValueChanged.AddListener((UnityEngine.Events.UnityAction<string>)OnPlaylistNameChanged);

            // Set up input field for keybind management
            UI.Helpers.InputFieldManager.SetupInputField(playlistNameInput);
            
            // Save button (20% width)
            var saveButtonObj = new GameObject("SaveNameButton");
            saveButtonObj.transform.SetParent(parent.transform, false);
            
            var saveButtonRect = saveButtonObj.AddComponent<RectTransform>();
            saveButtonRect.anchorMin = new Vector2(0.72f, 0f);
            saveButtonRect.anchorMax = new Vector2(0.88f, 1f);
            saveButtonRect.offsetMin = Vector2.zero;
            saveButtonRect.offsetMax = Vector2.zero;
            
            savePlaylistNameButton = saveButtonObj.AddComponent<Button>();
            var saveButtonImage = saveButtonObj.AddComponent<Image>();
            saveButtonImage.color = new Color(0.2f, 0.8f, 0.2f, 0.5f); // Start disabled
            
            var saveButtonTextObj = new GameObject("Text");
            saveButtonTextObj.transform.SetParent(saveButtonObj.transform, false);
            var saveButtonTextRect = saveButtonTextObj.AddComponent<RectTransform>();
            saveButtonTextRect.anchorMin = Vector2.zero;
            saveButtonTextRect.anchorMax = Vector2.one;
            saveButtonTextRect.offsetMin = Vector2.zero;
            saveButtonTextRect.offsetMax = Vector2.zero;
            
            var saveButtonText = saveButtonTextObj.AddComponent<Text>();
            saveButtonText.text = "Save";
            FontHelper.SetSafeFont(saveButtonText);
            saveButtonText.fontSize = 10;
            saveButtonText.color = Color.white;
            saveButtonText.alignment = TextAnchor.MiddleCenter;
            saveButtonText.fontStyle = FontStyle.Bold;
            
            savePlaylistNameButton.targetGraphic = saveButtonImage;
            savePlaylistNameButton.interactable = false; // Start disabled
            savePlaylistNameButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnSavePlaylistNameClicked);
        }
        
        private void CreateStaticPlaylistHeader(GameObject parent)
        {
            var headerText = parent.AddComponent<Text>();
            headerText.text = "Playlist";
            FontHelper.SetSafeFont(headerText);
            headerText.fontSize = 16;
            headerText.color = Color.white;
            headerText.alignment = TextAnchor.MiddleLeft;
            headerText.fontStyle = FontStyle.Bold;
        }
        
        private void OnPlaylistNameChanged(string newName)
        {
            if (savePlaylistNameButton == null) return;
            
            // Enable save button if name is different and has >3 characters
            bool canSave = !string.IsNullOrEmpty(newName) && 
                          newName.Trim().Length > 3 && 
                          newName.Trim() != originalPlaylistName;
            
            savePlaylistNameButton.interactable = canSave;
            
            var buttonImage = savePlaylistNameButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = canSave ? 
                    new Color(0.2f, 0.8f, 0.2f, 0.8f) : // Enabled - bright green
                    new Color(0.2f, 0.8f, 0.2f, 0.5f);  // Disabled - faded green
            }
        }
        
        private void OnSavePlaylistNameClicked()
        {
            if (playlistNameInput == null || currentYouTubePlaylist == null) return;
            
            var newName = playlistNameInput.text?.Trim();
            if (string.IsNullOrEmpty(newName) || newName.Length <= 3)
            {
                LoggingSystem.Warning("Playlist name must be more than 3 characters", "UI");
                return;
            }
            
            try
            {
                var oldName = currentYouTubePlaylist.name;
                currentYouTubePlaylist.name = newName;
                currentYouTubePlaylist.lastModified = DateTime.Now;
                
                if (YouTubePlaylistManager.SavePlaylist(currentYouTubePlaylist))
                {
                    originalPlaylistName = newName;
                    LoggingSystem.Info($"Renamed playlist from '{oldName}' to '{newName}'", "UI");
                    
                    // Update save button state
                    OnPlaylistNameChanged(newName);
                    
                    // Update playlist button
                    UpdatePlaylistButton();
                    
                    // Update dropdown if it exists
                    RefreshYouTubePlaylists();
                }
                else
                {
                    LoggingSystem.Error("Failed to save playlist with new name", "UI");
                    // Revert the name
                    currentYouTubePlaylist.name = oldName;
                    playlistNameInput.text = oldName;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error renaming playlist: {ex.Message}", "UI");
            }
        }
        
        private void CreateExternalYouTubeControls()
        {
            LoggingSystem.Info("Creating external YouTube controls", "UI");
            
            // Only create manage playlists button (dropdown removed)
            CreateManagePlaylistsButton();
        }
        
        private void CreateManagePlaylistsButton()
        {
            var buttonObj = new GameObject("ManagePlaylistsButton");
            buttonObj.transform.SetParent(this.transform, false);
            
            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.1f, 0.1f); // Centered and wider since dropdown removed
            buttonRect.anchorMax = new Vector2(0.55f, 0.7f); // Centered and wider since dropdown removed
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            managePlaylistsButton = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.6f, 0.2f, 0.8f, 0.8f); // Purple for manage
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var buttonText = textObj.AddComponent<Text>();
            buttonText.text = "Manage";
            FontHelper.SetSafeFont(buttonText);
            buttonText.fontSize = 10;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontStyle = FontStyle.Bold;
            
            managePlaylistsButton.targetGraphic = buttonImage;
            managePlaylistsButton.onClick.AddListener((UnityEngine.Events.UnityAction)OpenManagePlaylistsPopup);
            
            LoggingSystem.Info("Manage playlists button created", "UI");
        }
        
        private void OnExternalPlaylistSelected(int index)
        {
            if (index >= 0 && index < availableYouTubePlaylists.Count)
            {
                var selectedPlaylistInfo = availableYouTubePlaylists[index];
                LoggingSystem.Info($"External dropdown: User selected playlist: {selectedPlaylistInfo.name}", "UI");
                
                // Load the selected playlist
                SelectYouTubePlaylist(selectedPlaylistInfo.id);
                
                // Update internal dropdown too if it exists
                if (youTubePlaylistDropdown != null)
                {
                    youTubePlaylistDropdown.value = index;
                    youTubePlaylistDropdown.RefreshShownValue();
                }
                
                // Update the editable name field if playlist popup is open
                if (playlistNameInput != null && currentYouTubePlaylist != null)
                {
                    playlistNameInput.text = currentYouTubePlaylist.name;
                    originalPlaylistName = currentYouTubePlaylist.name;
                    OnPlaylistNameChanged(currentYouTubePlaylist.name);
                }
                
                // Refresh the playlist view if it's open
                if (isPlaylistOpen)
                {
                    ClosePlaylist();
                    OpenPlaylist();
                }
            }
        }
        
        private void OpenManagePlaylistsPopup()
        {
            LoggingSystem.Info("Opening manage playlists popup", "UI");
            
            if (managePlaylistsPopup != null)
            {
                GameObject.Destroy(managePlaylistsPopup);
            }
            
            CreateManagePlaylistsPopup();
        }
        
        private void CreateManagePlaylistsPopup()
        {
            LoggingSystem.Info("Creating manage playlists popup...", "UI");
            
            // Find our app's container instead of any canvas - SAME AS MAIN PLAYLIST POPUP
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
                LoggingSystem.Error("No app Container found for manage playlist popup! This will cause UI bleeding.", "UI");
                return;
            }
            
            LoggingSystem.Info($"Found app Container: {appContainer.name}", "UI");
            
            // Create popup that covers the entire app container (not the whole screen) - SAME AS MAIN PLAYLIST POPUP
            managePlaylistsPopup = new GameObject("ManagePlaylistsPopup");
            managePlaylistsPopup.transform.SetParent(appContainer, false);
            
            var popupRect = managePlaylistsPopup.AddComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;
            popupRect.anchoredPosition = Vector2.zero;
            popupRect.sizeDelta = Vector2.zero;
            
            // Semi-transparent background that blocks clicks - SAME AS MAIN PLAYLIST POPUP
            var popupBg = managePlaylistsPopup.AddComponent<Image>();
            popupBg.color = new Color(0f, 0f, 0f, 0.8f);
            popupBg.raycastTarget = true; // Block clicks behind popup
            
            // Make sure popup appears on top within our container
            managePlaylistsPopup.transform.SetAsLastSibling();
            
            LoggingSystem.Info("Manage popup background created within app container", "UI");
            
            // Create the actual manage panel inside the popup - SAME PATTERN AS MAIN PLAYLIST POPUP
            var managePanel = new GameObject("ManagePanel");
            managePanel.transform.SetParent(managePlaylistsPopup.transform, false);
            
            var panelRect = managePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            var panelBg = managePanel.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            
            LoggingSystem.Info("Manage panel created", "UI");
            
            // Create manage content inside the panel
            CreateManagePlaylistContent(managePanel);
            
            LoggingSystem.Info("Manage playlist popup creation completed", "UI");
        }
        
        private void CreateManagePlaylistContent(GameObject panel)
        {
            // Header with title and close button
            var header = new GameObject("Header");
            header.transform.SetParent(panel.transform, false);
            
            var headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 0.9f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.offsetMin = new Vector2(15f, 0f);
            headerRect.offsetMax = new Vector2(-15f, 0f);
            
            // Header background
            var headerBg = header.AddComponent<Image>();
            headerBg.color = new Color(0.8f, 0.2f, 0.2f, 0.3f); // Subtle red tint for header
            
            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(header.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0f);
            titleRect.anchorMax = new Vector2(0.8f, 1f);
            titleRect.offsetMin = new Vector2(10f, 0f);
            titleRect.offsetMax = Vector2.zero;
            
            var titleText = titleObj.AddComponent<Text>();
            titleText.text = "🎵 Manage YouTube Playlists";
            FontHelper.SetSafeFont(titleText);
            titleText.fontSize = 18;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.fontStyle = FontStyle.Bold;
            
            // Close button (top right)
            var closeButtonObj = new GameObject("CloseButton");
            closeButtonObj.transform.SetParent(header.transform, false);
            var closeButtonRect = closeButtonObj.AddComponent<RectTransform>();
            closeButtonRect.anchorMin = new Vector2(0.85f, 0.1f);
            closeButtonRect.anchorMax = new Vector2(0.98f, 0.9f);
            closeButtonRect.offsetMin = Vector2.zero;
            closeButtonRect.offsetMax = Vector2.zero;
            
            var closeButton = closeButtonObj.AddComponent<Button>();
            var closeButtonImage = closeButtonObj.AddComponent<Image>();
            closeButtonImage.color = new Color(0.9f, 0.2f, 0.2f, 0.9f); // Bright red for visibility
            
            var closeButtonTextObj = new GameObject("Text");
            closeButtonTextObj.transform.SetParent(closeButtonObj.transform, false);
            var closeButtonTextRect = closeButtonTextObj.AddComponent<RectTransform>();
            closeButtonTextRect.anchorMin = Vector2.zero;
            closeButtonTextRect.anchorMax = Vector2.one;
            closeButtonTextRect.offsetMin = Vector2.zero;
            closeButtonTextRect.offsetMax = Vector2.zero;
            
            var closeButtonText = closeButtonTextObj.AddComponent<Text>();
            closeButtonText.text = "✕";
            FontHelper.SetSafeFont(closeButtonText);
            closeButtonText.fontSize = 16;
            closeButtonText.color = Color.white;
            closeButtonText.alignment = TextAnchor.MiddleCenter;
            closeButtonText.fontStyle = FontStyle.Bold;
            
            closeButton.targetGraphic = closeButtonImage;
            closeButton.onClick.AddListener((UnityEngine.Events.UnityAction)CloseManagePlaylistsPopup);
            
            // Create New Playlist button
            var newPlaylistButtonObj = new GameObject("NewPlaylistButton");
            newPlaylistButtonObj.transform.SetParent(panel.transform, false);
            var newButtonRect = newPlaylistButtonObj.AddComponent<RectTransform>();
            newButtonRect.anchorMin = new Vector2(0.1f, 0.82f);
            newButtonRect.anchorMax = new Vector2(0.9f, 0.88f);
            newButtonRect.offsetMin = Vector2.zero;
            newButtonRect.offsetMax = Vector2.zero;
            
            var newButton = newPlaylistButtonObj.AddComponent<Button>();
            var newButtonImage = newPlaylistButtonObj.AddComponent<Image>();
            newButtonImage.color = new Color(0.2f, 0.8f, 0.3f, 0.9f); // Brighter green
            
            var newButtonTextObj = new GameObject("Text");
            newButtonTextObj.transform.SetParent(newPlaylistButtonObj.transform, false);
            var newButtonTextRect = newButtonTextObj.AddComponent<RectTransform>();
            newButtonTextRect.anchorMin = Vector2.zero;
            newButtonTextRect.anchorMax = Vector2.one;
            newButtonTextRect.offsetMin = Vector2.zero;
            newButtonTextRect.offsetMax = Vector2.zero;
            
            var newButtonText = newButtonTextObj.AddComponent<Text>();
            newButtonText.text = "➕ Create New Playlist";
            FontHelper.SetSafeFont(newButtonText);
            newButtonText.fontSize = 14;
            newButtonText.color = Color.white;
            newButtonText.alignment = TextAnchor.MiddleCenter;
            newButtonText.fontStyle = FontStyle.Bold;
            
            newButton.targetGraphic = newButtonImage;
            newButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnManageCreatePlaylistClicked);
            
            // Scrollable playlist list
            CreateManagePlaylistsList(panel);
        }
        
        private void CreateManagePlaylistsList(GameObject panel)
        {
            // Scrollable list area
            var listContainer = new GameObject("PlaylistList");
            listContainer.transform.SetParent(panel.transform, false);
            
            var listRect = listContainer.AddComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.05f, 0.1f);
            listRect.anchorMax = new Vector2(0.95f, 0.8f);  // Adjusted to make room for Create button
            listRect.offsetMin = Vector2.zero;
            listRect.offsetMax = Vector2.zero;
            
            // Scrollable content
            var scrollContent = new GameObject("Content");
            scrollContent.transform.SetParent(listContainer.transform, false);
            
            var contentRect = scrollContent.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            
            // Set first playlist as selected by default if none selected
            if (string.IsNullOrEmpty(selectedPlaylistId) && availableYouTubePlaylists.Count > 0)
            {
                selectedPlaylistId = availableYouTubePlaylists[0].id;
                SelectYouTubePlaylist(selectedPlaylistId);
            }
            
            // Populate with playlists
            float yPos = 0f;
            foreach (var playlist in availableYouTubePlaylists)
            {
                CreateManagePlaylistItem(scrollContent, playlist, yPos);
                yPos -= 55f; // Slightly more space for Select/Delete buttons
            }
            
            // Set content size
            contentRect.sizeDelta = new Vector2(0f, Math.Max(200f, availableYouTubePlaylists.Count * 55f));
            
            // Add ScrollRect
            var scrollRect = listContainer.AddComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.verticalScrollbar = null; // No scrollbar for simplicity
        }
        
        private void CreateManagePlaylistItem(GameObject parent, YouTubePlaylistInfo playlist, float yPos)
        {
            var itemObj = new GameObject($"PlaylistItem_{playlist.id}");
            itemObj.transform.SetParent(parent.transform, false);
            
            var itemRect = itemObj.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 1f);
            itemRect.anchorMax = new Vector2(1f, 1f);
            itemRect.pivot = new Vector2(0.5f, 1f);
            itemRect.anchoredPosition = new Vector2(0f, yPos);
            itemRect.sizeDelta = new Vector2(0f, 50f);
            
            // Background
            var itemBg = itemObj.AddComponent<Image>();
            itemBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Playlist name and info (60% width)
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(itemObj.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0f);
            nameRect.anchorMax = new Vector2(0.65f, 1f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            
            var nameText = nameObj.AddComponent<Text>();
            nameText.text = $"{playlist.name}\n{playlist.downloadedCount}/{playlist.songCount} songs downloaded";
            FontHelper.SetSafeFont(nameText);
            nameText.fontSize = 11;
            nameText.color = Color.white;
            nameText.alignment = TextAnchor.MiddleLeft;
            
            // Select/Selected button (25% width)
            var selectButtonObj = new GameObject("SelectButton");
            selectButtonObj.transform.SetParent(itemObj.transform, false);
            var selectButtonRect = selectButtonObj.AddComponent<RectTransform>();
            selectButtonRect.anchorMin = new Vector2(0.67f, 0.2f);
            selectButtonRect.anchorMax = new Vector2(0.82f, 0.8f);
            selectButtonRect.offsetMin = Vector2.zero;
            selectButtonRect.offsetMax = Vector2.zero;
            
            var selectButton = selectButtonObj.AddComponent<Button>();
            var selectBg = selectButtonObj.AddComponent<Image>();
            
            var selectTextObj = new GameObject("Text");
            selectTextObj.transform.SetParent(selectButtonObj.transform, false);
            var selectTextRect = selectTextObj.AddComponent<RectTransform>();
            selectTextRect.anchorMin = Vector2.zero;
            selectTextRect.anchorMax = Vector2.one;
            selectTextRect.offsetMin = Vector2.zero;
            selectTextRect.offsetMax = Vector2.zero;
            
            var selectText = selectTextObj.AddComponent<Text>();
            FontHelper.SetSafeFont(selectText);
            selectText.fontSize = 9;
            selectText.color = Color.white;
            selectText.alignment = TextAnchor.MiddleCenter;
            selectText.fontStyle = FontStyle.Bold;
            
            // Set button state based on selection
            bool isSelected = playlist.id == selectedPlaylistId;
            if (isSelected)
            {
                selectText.text = "Selected";
                selectBg.color = new Color(0.2f, 0.6f, 0.2f, 0.8f); // Green for selected
                selectButton.interactable = false;
            }
            else
            {
                selectText.text = "Select";
                selectBg.color = new Color(0.2f, 0.4f, 0.8f, 0.8f); // Blue for selectable
                selectButton.interactable = true;
            }
            
            selectButton.targetGraphic = selectBg;
            selectButton.onClick.AddListener((UnityEngine.Events.UnityAction)delegate() { OnSelectPlaylistClicked(playlist.id); });
            
            // Delete button (13% width)
            var deleteButtonObj = new GameObject("DeleteButton");
            deleteButtonObj.transform.SetParent(itemObj.transform, false);
            var deleteButtonRect = deleteButtonObj.AddComponent<RectTransform>();
            deleteButtonRect.anchorMin = new Vector2(0.84f, 0.2f);
            deleteButtonRect.anchorMax = new Vector2(0.95f, 0.8f);
            deleteButtonRect.offsetMin = Vector2.zero;
            deleteButtonRect.offsetMax = Vector2.zero;
            
            var deleteButton = deleteButtonObj.AddComponent<Button>();
            var deleteBg = deleteButtonObj.AddComponent<Image>();
            deleteBg.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            
            var deleteTextObj = new GameObject("Text");
            deleteTextObj.transform.SetParent(deleteButtonObj.transform, false);
            var deleteTextRect = deleteTextObj.AddComponent<RectTransform>();
            deleteTextRect.anchorMin = Vector2.zero;
            deleteTextRect.anchorMax = Vector2.one;
            deleteTextRect.offsetMin = Vector2.zero;
            deleteTextRect.offsetMax = Vector2.zero;
            
            var deleteText = deleteTextObj.AddComponent<Text>();
            deleteText.text = "Delete";
            FontHelper.SetSafeFont(deleteText);
            deleteText.fontSize = 8;
            deleteText.color = Color.white;
            deleteText.alignment = TextAnchor.MiddleCenter;
            deleteText.fontStyle = FontStyle.Bold;
            
            deleteButton.targetGraphic = deleteBg;
            deleteButton.onClick.AddListener((UnityEngine.Events.UnityAction)delegate() { OnManageDeletePlaylistClicked(playlist.id); });
        }
        
        private void OnSelectPlaylistClicked(string playlistId)
        {
            LoggingSystem.Info($"🎯 Selecting playlist: {playlistId}", "UI");
            
            try
            {
                // Update selected playlist
                selectedPlaylistId = playlistId;
                
                // Load the playlist and update backend
                SelectYouTubePlaylist(playlistId);
                
                // Verify the playlist was loaded
                if (currentYouTubePlaylist != null)
                {
                    LoggingSystem.Info($"✅ Successfully selected playlist '{currentYouTubePlaylist.name}' with {currentYouTubePlaylist.songs.Count} songs", "UI");
                    
                    // Close manage popup first
                    CloseManagePlaylistsPopup();
                    
                    // Update playlist button to reflect new selection and track count
                    UpdatePlaylistButton();
                    
                    // If the main playlist popup is open, refresh it to show new tracks
                    if (isPlaylistOpen)
                    {
                        LoggingSystem.Debug("Refreshing open playlist popup with new tracks", "UI");
                        ClosePlaylist();
                        OpenPlaylist();
                    }
                    
                    LoggingSystem.Info($"🎉 Playlist selection and UI refresh completed for '{currentYouTubePlaylist.name}'", "UI");
                }
                else
                {
                    LoggingSystem.Error($"❌ Failed to load playlist '{playlistId}' - currentYouTubePlaylist is null", "UI");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"❌ Error selecting playlist '{playlistId}': {ex.Message}", "UI");
                LoggingSystem.Error($"Stack trace: {ex.StackTrace}", "UI");
            }
        }
        
        private void OnManageCreatePlaylistClicked()
        {
            // Create a simple playlist with a timestamp name
            var playlistName = $"New Playlist {DateTime.Now:HH:mm:ss}";
            
            try
            {
                var newPlaylist = YouTubePlaylistManager.CreatePlaylist(playlistName, "User created playlist from manage popup");
                if (newPlaylist != null)
                {
                    LoggingSystem.Info($"✅ Created new playlist from manage popup: {newPlaylist.name}", "UI");
                    
                    // Auto-select the newly created playlist
                    selectedPlaylistId = newPlaylist.id;
                    SelectYouTubePlaylist(newPlaylist.id);
                    
                    // Update playlist button
                    UpdatePlaylistButton();
                    
                    // Refresh the manage popup to show the new playlist
                    if (managePlaylistsPopup != null)
                    {
                        GameObject.Destroy(managePlaylistsPopup);
                        CreateManagePlaylistsPopup();
                    }
                    
                    LoggingSystem.Info($"🎉 New playlist '{newPlaylist.name}' created and auto-selected", "UI");
                }
                else
                {
                    LoggingSystem.Warning("❌ Failed to create new playlist from manage popup", "UI");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating playlist from manage popup: {ex.Message}", "UI");
            }
        }
        
        private void OnManageDeletePlaylistClicked(string playlistId)
        {
            try
            {
                var playlist = YouTubePlaylistManager.LoadPlaylist(playlistId);
                if (playlist != null)
                {
                    LoggingSystem.Info($"🗑️ Deleting playlist from manage popup: {playlist.name}", "UI");
                    
                    if (YouTubePlaylistManager.DeletePlaylist(playlistId))
                    {
                        LoggingSystem.Info($"✅ Successfully deleted playlist: {playlist.name}", "UI");
                        
                        // If the deleted playlist was the current one, select another
                        if (selectedPlaylistId == playlistId || currentYouTubePlaylist?.id == playlistId)
                        {
                            currentYouTubePlaylist = null;
                            selectedPlaylistId = "";
                            
                            // Try to select the first available playlist
                            RefreshYouTubePlaylists();
                            if (availableYouTubePlaylists.Count > 0)
                            {
                                selectedPlaylistId = availableYouTubePlaylists[0].id;
                                SelectYouTubePlaylist(selectedPlaylistId);
                                LoggingSystem.Info($"🔄 Auto-selected new playlist: {availableYouTubePlaylists[0].name}", "UI");
                            }
                        }
                        
                        // Update playlist button
                        UpdatePlaylistButton();
                        
                        // Refresh the manage popup
                        if (managePlaylistsPopup != null)
                        {
                            GameObject.Destroy(managePlaylistsPopup);
                            CreateManagePlaylistsPopup();
                        }
                    }
                    else
                    {
                        LoggingSystem.Warning($"❌ Failed to delete playlist: {playlist.name}", "UI");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error deleting playlist from manage popup: {ex.Message}", "UI");
            }
        }
        
        private void CloseManagePlaylistsPopup()
        {
            if (managePlaylistsPopup != null)
            {
                GameObject.Destroy(managePlaylistsPopup);
                managePlaylistsPopup = null;
                LoggingSystem.Info("Manage playlists popup closed", "UI");
            }
        }
    }
} 