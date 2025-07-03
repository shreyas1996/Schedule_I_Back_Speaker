using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.NewBackend;
using BackSpeakerMod.S1Wrapper;
using BackSpeakerMod.NewFrontend.UI.Interfaces;

namespace BackSpeakerMod.NewFrontend.UI.Screens
{
    /// <summary>
    /// Jukebox Screen - Browse and select in-game jukebox music
    /// </summary>
    public class JukeboxScreen : MonoBehaviour
    {
        #region Private Fields
        
        private BackSpeakerMainManager? mainManager;
        private NavigationManager? navigationManager;
        private IMusicBackend? musicBackend;
        
        // UI Elements
        private GameObject? mainContainer;
        private GameObject? headerSection;
        private GameObject? trackListSection;
        private ScrollRect? trackScrollView;
        private GameObject? trackListContent;
        
        // Header Controls
        private Button? backButton;
        private Text? titleText;
        private Button? refreshButton;
        private Text? trackCountText;
        
        // Track List
        private List<GameObject>? trackItems;
        private List<NewSongDetails>? availableTracks;
        
        // State
        private bool isInitialized = false;
        
        #endregion
        
        #region Public Properties
        
        public bool IsInitialized => isInitialized;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the jukebox screen
        /// </summary>
        public void Initialize(BackSpeakerMainManager manager, NavigationManager navManager)
        {
            try
            {
                mainManager = manager;
                navigationManager = navManager;
                
                // Get centralized backend from main manager
                musicBackend = mainManager?.GetMusicBackend();
                if (musicBackend == null)
                {
                    throw new InvalidOperationException("Music backend not available from main manager");
                }
                
                trackItems = new List<GameObject>();
                availableTracks = new List<NewSongDetails>();
                
                NewLoggingSystem.Info("Initializing JukeboxScreen", "JukeboxScreen");
                
                CreateMainLayout();
                CreateHeader();
                CreateTrackList();
                
                LoadJukeboxTracks();
                
                isInitialized = true;
                NewLoggingSystem.Info("JukeboxScreen initialized successfully", "JukeboxScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to initialize JukeboxScreen: {ex}", "JukeboxScreen");
            }
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateMainLayout()
        {
            // Main container
            mainContainer = new GameObject("JukeboxScreen");
            mainContainer.transform.SetParent(this.transform, false);
            
            var mainRect = mainContainer.AddComponent<RectTransform>();
            mainRect.anchorMin = Vector2.zero;
            mainRect.anchorMax = Vector2.one;
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;
            
            var mainImage = mainContainer.AddComponent<Image>();
            mainImage.color = ModernUIFactory.Colors.Background;
            
            // Main vertical layout
            var mainLayout = mainContainer.AddComponent<VerticalLayoutGroup>();
            mainLayout.spacing = 0;
            mainLayout.padding = new RectOffset(0, 0, 0, 0);
            mainLayout.childControlHeight = true;  // Enable height control
            mainLayout.childControlWidth = true;
            mainLayout.childForceExpandHeight = false;  // DON'T force expand - let components use preferred heights
            mainLayout.childForceExpandWidth = true;
        }
        
        private void CreateHeader()
        {
            headerSection = new GameObject("Header");
            headerSection.transform.SetParent(mainContainer?.transform, false);
            
            var headerLayout = headerSection?.AddComponent<HorizontalLayoutGroup>();
            if (headerLayout != null)
            {
                headerLayout.spacing = 15;
                headerLayout.padding = new RectOffset(20, 20, 20, 20);
                headerLayout.childControlHeight = false;
                headerLayout.childControlWidth = false;
                headerLayout.childForceExpandWidth = false;
            }
            
            // Back button
            if (headerSection != null)
            {
                backButton = ModernUIFactory.CreateIconButton(headerSection.transform, "←", 
                    S1Factory.ConvertToUnityAction(OnBackClick), new Vector2(40, 40), true);
            }
            var backLayout = backButton?.gameObject?.AddComponent<LayoutElement>();
            if (backLayout != null)
            {
                backLayout.preferredWidth = 40;
                backLayout.preferredHeight = 40;
            }
            
            // Title
            if (headerSection != null)
            {
                titleText = ModernUIFactory.CreateModernText(headerSection.transform, "Jukebox", 24, TextStyle.Primary, TextAnchor.MiddleLeft);
            }
            if (titleText != null)
            {
                titleText.fontStyle = FontStyle.Bold;
            }
            var titleLayout = titleText?.gameObject?.AddComponent<LayoutElement>();
            if (titleLayout != null)
            {
                titleLayout.flexibleWidth = 1;
            }
            
            // Track count text
            if (headerSection != null)
            {
                trackCountText = ModernUIFactory.CreateModernText(headerSection.transform, "", 14, TextStyle.Secondary, TextAnchor.MiddleRight);
            }
            var trackCountLayout = trackCountText?.gameObject?.AddComponent<LayoutElement>();
            if (trackCountLayout != null)
            {
                trackCountLayout.preferredWidth = 150;
            }
            
            // Search button
            if (headerSection != null)
            {
                refreshButton = ModernUIFactory.CreateIconButton(headerSection.transform, "⟳", 
                    S1Factory.ConvertToUnityAction(OnRefreshClick), new Vector2(40, 40), false);
            }
            var refreshLayout = refreshButton?.gameObject?.AddComponent<LayoutElement>();
            if (refreshLayout != null)
            {
                refreshLayout.preferredWidth = 40;
                refreshLayout.preferredHeight = 40;
            }
            
            // Header layout element
            var headerLayoutElement = headerSection?.AddComponent<LayoutElement>();
            if (headerLayoutElement != null)
            {
                headerLayoutElement.preferredHeight = 80;
                headerLayoutElement.flexibleHeight = 0;
            }
        }
        
        private void CreateTrackList()
        {
            trackListSection = new GameObject("TrackList");
            if (mainContainer != null)
            {
                trackListSection.transform.SetParent(mainContainer.transform, false);
            }
            
            var scrollImage = trackListSection?.AddComponent<Image>();
            if (scrollImage != null)
            {
                scrollImage.color = ModernUIFactory.Colors.Background;
            }
            
            trackListContent = new GameObject("Content");
            if (trackListSection != null)
            {
                trackListContent.transform.SetParent(trackListSection.transform, false);
            }
            
            var contentRect = trackListContent?.AddComponent<RectTransform>();
            if (contentRect != null)
            {
                contentRect.anchorMin = Vector2.zero;
                contentRect.anchorMax = Vector2.one;
                contentRect.offsetMin = Vector2.zero;
                contentRect.offsetMax = Vector2.zero;
            }
            
            var contentLayout = trackListContent?.AddComponent<VerticalLayoutGroup>();
            if (contentLayout != null)
            {
                contentLayout.spacing = 8;
                contentLayout.padding = new RectOffset(15, 15, 15, 15);
                contentLayout.childControlHeight = false;
                contentLayout.childControlWidth = true;
                contentLayout.childForceExpandHeight = false;
                contentLayout.childForceExpandWidth = true;
            }
            
            var sizeFitter = trackListContent?.AddComponent<ContentSizeFitter>();
            if (sizeFitter != null)
            {
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            
            var scrollRect = trackListSection?.AddComponent<ScrollRect>();
            trackScrollView = scrollRect;
            if (scrollRect != null && contentRect != null)
            {
                scrollRect.content = contentRect;
                scrollRect.vertical = true;
                scrollRect.horizontal = false;
                scrollRect.scrollSensitivity = 15;
                trackScrollView = scrollRect;
            }
            
            var listLayoutElement = trackListSection?.AddComponent<LayoutElement>();
            if (listLayoutElement != null)
            {
                listLayoutElement.flexibleHeight = 1;
            }
        }
        
        #endregion
        
        #region Track Management
        
        private void LoadJukeboxTracks()
        {
            try
            {
                NewLoggingSystem.Info("Loading jukebox tracks", "JukeboxScreen");
                // Loading tracks
                UpdateTrackCount("Loading...");
                
                // Clear existing tracks
                ClearTrackList();
                
                // Get tracks from backend
                availableTracks = musicBackend?.GetJukeboxTracks() ?? new List<NewSongDetails>();
                
                PopulateTrackList();
                // Finished loading
                
                NewLoggingSystem.Info($"Loaded {availableTracks.Count} jukebox tracks", "JukeboxScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to load jukebox tracks: {ex}", "JukeboxScreen");
                // Failed to load
                UpdateTrackCount("Error loading tracks");
            }
        }
        

        
        private void PopulateTrackList()
        {
            if (availableTracks != null)
            {
                UpdateTrackCount($"{availableTracks.Count} tracks available");
                
                foreach (var track in availableTracks)
                {
                    CreateTrackItem(track);
                }
            }
            else
            {
                UpdateTrackCount("No tracks available");
            }
        }
        
        private void CreateTrackItem(NewSongDetails track)
        {
            if (trackListContent == null) return;
            
            var trackItem = ModernUIFactory.CreateCard(trackListContent.transform, new Vector2(0, 70), true);
            
            var itemLayout = trackItem?.AddComponent<HorizontalLayoutGroup>();
            if (itemLayout != null)
            {
                itemLayout.spacing = 15;
                itemLayout.padding = new RectOffset(15, 15, 10, 10);
                itemLayout.childControlHeight = true;
                itemLayout.childControlWidth = false;
                itemLayout.childForceExpandHeight = false;
                itemLayout.childForceExpandWidth = false;
            }
            
            // Track icon
            Text? iconText = null;
            if (trackItem != null)
            {
                iconText = ModernUIFactory.CreateModernText(trackItem.transform, "♫", 24, TextStyle.Primary, TextAnchor.MiddleCenter);
            }
            if (iconText != null)
            {
                var iconLayout = iconText.gameObject?.AddComponent<LayoutElement>();
                if (iconLayout != null)
                {
                    iconLayout.preferredWidth = 40;
                    iconLayout.preferredHeight = 40;
                }
            }
            
            // Track info section
            GameObject? infoContainer = null;
            if (trackItem != null)
            {
                infoContainer = ModernUIFactory.CreateVerticalLayout(trackItem.transform, 2, new RectOffset(0, 0, 0, 0));
            }
            var infoLayout = infoContainer?.AddComponent<LayoutElement>();
            if (infoLayout != null)
            {
                infoLayout.flexibleWidth = 1;
            }
            
            Text? titleText = null;
            if (infoContainer != null)
            {
                titleText = ModernUIFactory.CreateModernText(infoContainer.transform, track.title, 14, TextStyle.Primary);
            }
            if (titleText != null)
            {
                titleText.fontStyle = FontStyle.Bold;
                var titleLayout = titleText.gameObject?.AddComponent<LayoutElement>();
                if (titleLayout != null)
                {
                    titleLayout.preferredHeight = 20;
                }
            }
            
            Text? artistText = null;
            if (infoContainer != null)
            {
                artistText = ModernUIFactory.CreateModernText(infoContainer.transform, $"{track.artist} • {FormatDuration(track.duration)}", 12, TextStyle.Secondary);
            }
            var artistLayout = artistText?.gameObject?.AddComponent<LayoutElement>();
            if (artistLayout != null)
            {
                artistLayout.preferredHeight = 18;
            }
            
            // Play button
            Button? playButton = null;
            if (trackItem != null)
            {
                playButton = ModernUIFactory.CreateIconButton(trackItem.transform, ">", S1Factory.ConvertToUnityAction(() => OnTrackPlay(track)), new Vector2(45, 45));
            }
            var playLayout = playButton?.gameObject?.AddComponent<LayoutElement>();
            if (playLayout != null)
            {
                playLayout.preferredWidth = 45;
                playLayout.preferredHeight = 45;
            }
            
            // Add to playlist button
            Button? addButton = null;
            if (trackItem != null)
            {
                addButton = ModernUIFactory.CreateIconButton(trackItem.transform, "+", S1Factory.ConvertToUnityAction(() => OnTrackAdd(track)), new Vector2(35, 35));
            }
            var addLayout = addButton?.gameObject?.AddComponent<LayoutElement>();
            if (addLayout != null)
            {
                addLayout.preferredWidth = 35;
                addLayout.preferredHeight = 35;
            }
            
            // Context menu button (3-dot menu)
            if (trackItem != null)
            {
                var contextButton = ModernUIFactory.CreateContextMenuButton(trackItem.transform, 
                    () => ShowTrackContextMenu(track, trackItem.transform), new Vector2(35, 35));
                var contextLayout = contextButton?.gameObject?.AddComponent<LayoutElement>();
                if (contextLayout != null)
                {
                    contextLayout.preferredWidth = 35;
                    contextLayout.preferredHeight = 35;
                }
            }
            
            // Track item layout element
            var trackLayout = trackItem?.AddComponent<LayoutElement>();
            if (trackLayout != null)
            {
                trackLayout.preferredHeight = 70;
            }
            
            if (trackItem != null)
            {
                trackItems?.Add(trackItem);
            }
        }
        
        private void ClearTrackList()
        {
            if (trackItems != null)
            {
                foreach (var item in trackItems)
                {
                    if (item != null)
                    {
                        UnityEngine.Object.Destroy(item);
                    }
                }
                trackItems.Clear();
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnBackClick()
        {
            NewLoggingSystem.Info("Back button clicked", "JukeboxScreen");
            navigationManager?.NavigateBack();
        }
        
        private void OnRefreshClick()
        {
            NewLoggingSystem.Info("Refresh button clicked", "JukeboxScreen");
            LoadJukeboxTracks();
        }
        
        private void OnTrackPlay(NewSongDetails track)
        {
            NewLoggingSystem.Info($"Play track: {track.title}", "JukeboxScreen");
            musicBackend?.PlayTrack(track);
        }
        
        private void OnTrackAdd(NewSongDetails track)
        {
            NewLoggingSystem.Info($"Add track to playlist: {track.title}", "JukeboxScreen");
            musicBackend?.AddToQueue(track);
        }
        
        private void ShowTrackContextMenu(NewSongDetails track, Transform relativeTo)
        {
            NewLoggingSystem.Info($"Show context menu for: {track.title}", "JukeboxScreen");
            
            // Calculate position relative to the track item
            var screenPos = RectTransformUtility.WorldToScreenPoint(null, relativeTo.position);
            var canvasPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                this.transform as RectTransform, screenPos, null, out canvasPos);
            
            ModernUIFactory.CreateSongContextMenu(this.transform, track, 
                OnContextMenuAction, canvasPos + new Vector2(100, 0));
        }
        
        private void OnContextMenuAction(ModernUIFactory.SongContextAction action, NewSongDetails song)
        {
            switch (action)
            {
                case ModernUIFactory.SongContextAction.AddToQueue:
                    musicBackend?.AddToQueue(song);
                    NewLoggingSystem.Info($"Added '{song.title}' to queue", "JukeboxScreen");
                    break;
                    
                case ModernUIFactory.SongContextAction.AddToPlaylist:
                    ShowPlaylistSelectionDialog(song);
                    break;
                    
                case ModernUIFactory.SongContextAction.Download:
                    musicBackend?.DownloadSong(song);
                    NewLoggingSystem.Info($"Started download: '{song.title}'", "JukeboxScreen");
                    break;
            }
        }
        
        private void ShowPlaylistSelectionDialog(NewSongDetails song)
        {
            // Create overlay
            var overlay = new GameObject("PlaylistSelectionOverlay");
            overlay.transform.SetParent(this.transform, false);
            
            var overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            
            var overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0.5f);
            
            // Create dialog
            var dialog = ModernUIFactory.CreateCard(overlay.transform, new Vector2(300, 400), true);
            if (dialog == null) return;
            var dialogRect = dialog.GetComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.pivot = new Vector2(0.5f, 0.5f);
            
            var dialogLayout = dialog.AddComponent<VerticalLayoutGroup>();
            dialogLayout.padding = new RectOffset(15, 15, 15, 15);
            dialogLayout.spacing = 10;
            
            // Title
            Text? titleText = null;
            titleText = ModernUIFactory.CreateModernText(dialog.transform, $"Add '{song.title}' to Playlist", 16, TextStyle.Primary, TextAnchor.MiddleCenter);
            var titleLayout = titleText?.gameObject?.AddComponent<LayoutElement>();
            if (titleLayout != null)
            {
                titleLayout.preferredHeight = 30;
            }
            
            // Playlist list
            var scrollView = ModernUIFactory.CreateCard(dialog.transform, new Vector2(0, 250), false);
            var scrollLayout = scrollView?.AddComponent<LayoutElement>();
            if (scrollLayout != null)
            {
                scrollLayout.flexibleHeight = 1;
            }
            
            var scrollContent = new GameObject("Content");
            scrollContent.transform.SetParent(scrollView?.transform, false);
            var contentRect = scrollContent.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            var contentLayout = scrollContent.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 5;
            contentLayout.padding = new RectOffset(5, 5, 5, 5);
            
            // Add playlists
            if (musicBackend != null)
            {
                var playlists = musicBackend.GetAllPlaylists();
                foreach (var playlist in playlists)
                {
                    var playlistButton = ModernUIFactory.CreateModernButton(scrollContent.transform, playlist.name,
                        S1Factory.ConvertToUnityAction(() => {
                            musicBackend.AddSongToPlaylist(playlist.id, song);
                            DestroyImmediate(overlay);
                            NewLoggingSystem.Info($"Added '{song.title}' to playlist '{playlist.name}'", "JukeboxScreen");
                        }), ButtonStyle.Secondary, new Vector2(0, 35));
                }
            }
            
            // Buttons
            var buttonContainer = ModernUIFactory.CreateHorizontalLayout(dialog.transform, 10);
            var buttonLayout = buttonContainer.AddComponent<LayoutElement>();
            buttonLayout.preferredHeight = 40;
            
            var cancelButton = ModernUIFactory.CreateModernButton(buttonContainer.transform, "Cancel",
                S1Factory.ConvertToUnityAction(() => DestroyImmediate(overlay)), ButtonStyle.Secondary, new Vector2(100, 35));
                
            var newPlaylistButton = ModernUIFactory.CreateModernButton(buttonContainer.transform, "New Playlist",
                S1Factory.ConvertToUnityAction(() => {
                    if (musicBackend != null)
                    {
                        var newPlaylist = musicBackend.CreateNewPlaylist($"Playlist {System.DateTime.Now:HH:mm}");
                        if (newPlaylist != null)
                        {
                            musicBackend.AddSongToPlaylist(newPlaylist.id, song);
                            DestroyImmediate(overlay);
                            NewLoggingSystem.Info($"Created new playlist and added '{song.title}'", "JukeboxScreen");
                        }
                    }
                }), ButtonStyle.Primary, new Vector2(120, 35));
        }
        
        #endregion
        
        #region Helper Methods
        
        private void UpdateTrackCount(string text)
        {
            if (trackCountText != null)
            {
                trackCountText.text = text;
            }
        }
        
        private string FormatDuration(float seconds)
        {
            var minutes = Mathf.FloorToInt(seconds / 60);
            var remainingSeconds = Mathf.FloorToInt(seconds % 60);
            return $"{minutes}:{remainingSeconds:00}";
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        public void OnDestroy()
        {
            ClearTrackList();
        }
        
        #endregion
    }
} 