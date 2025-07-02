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
        
        private BackSpeakerMainManager mainManager;
        private NavigationManager navigationManager;
        private IMusicBackend musicBackend;
        
        // UI Elements
        private GameObject mainContainer;
        private GameObject headerSection;
        private GameObject trackListSection;
        private ScrollRect trackScrollView;
        private GameObject trackListContent;
        
        // Header Controls
        private Button backButton;
        private Text titleText;
        private Button refreshButton;
        private Text trackCountText;
        
        // Track List
        private List<GameObject> trackItems;
        private List<NewSongDetails> availableTracks;
        
        // State
        private bool isInitialized = false;
        private bool isLoading = false;
        
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
            headerSection = ModernUIFactory.CreateCard(mainContainer.transform, new Vector2(0, 80), false);
            
            var headerLayout = headerSection.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 15;
            headerLayout.padding = new RectOffset(20, 20, 15, 15);
            headerLayout.childControlHeight = true;
            headerLayout.childControlWidth = false;
            headerLayout.childForceExpandHeight = false;
            headerLayout.childForceExpandWidth = false;
            
            // Back button
            backButton = ModernUIFactory.CreateIconButton(headerSection.transform, "←", S1Factory.ConvertToUnityAction(OnBackClick), new Vector2(40, 40), false);
            var backLayout = backButton.gameObject.AddComponent<LayoutElement>();
            backLayout.preferredWidth = 40;
            backLayout.preferredHeight = 40;
            
            // Title and info section
            var titleContainer = ModernUIFactory.CreateVerticalLayout(headerSection.transform, 2, new RectOffset(0, 0, 0, 0));
            var titleContainerLayout = titleContainer.AddComponent<LayoutElement>();
            titleContainerLayout.flexibleWidth = 1;
            
            titleText = ModernUIFactory.CreateModernText(titleContainer.transform, "🎵 Jukebox", 18, TextStyle.Primary);
            titleText.fontStyle = FontStyle.Bold;
            var titleTextLayout = titleText.gameObject.AddComponent<LayoutElement>();
            titleTextLayout.preferredHeight = 25;
            
            trackCountText = ModernUIFactory.CreateModernText(titleContainer.transform, "Loading tracks...", 12, TextStyle.Secondary);
            var countLayout = trackCountText.gameObject.AddComponent<LayoutElement>();
            countLayout.preferredHeight = 20;
            
            // Refresh button
            refreshButton = ModernUIFactory.CreateIconButton(headerSection.transform, "⟳", S1Factory.ConvertToUnityAction(OnRefreshClick), new Vector2(40, 40), false);
            var refreshLayout = refreshButton.gameObject.AddComponent<LayoutElement>();
            refreshLayout.preferredWidth = 40;
            refreshLayout.preferredHeight = 40;
            
            // Header layout element
            var headerLayoutElement = headerSection.AddComponent<LayoutElement>();
            headerLayoutElement.preferredHeight = 80;
            headerLayoutElement.flexibleHeight = 0;  // Don't expand
        }
        
        private void CreateTrackList()
        {
            trackListSection = new GameObject("TrackListSection");
            trackListSection.transform.SetParent(mainContainer.transform, false);
            
            // Background
            var scrollImage = trackListSection.AddComponent<Image>();
            scrollImage.color = ModernUIFactory.Colors.Background;
            
            // Scroll view
            trackScrollView = trackListSection.AddComponent<ScrollRect>();
            var listRect = trackListSection.AddComponent<RectTransform>();
            
            // SIMPLIFIED: Direct content container, no complex viewport masking
            trackListContent = new GameObject("Content");
            trackListContent.transform.SetParent(trackListSection.transform, false);
            
            var contentRect = trackListContent.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Simple content layout
            var contentLayout = trackListContent.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 8;
            contentLayout.padding = new RectOffset(15, 15, 15, 15);
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            
            // Content size fitter for proper scrolling
            var sizeFitter = trackListContent.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            // Simple scroll setup
            trackScrollView.content = contentRect;
            trackScrollView.vertical = true;
            trackScrollView.horizontal = false;
            trackScrollView.scrollSensitivity = 15;
            
            // Track list layout element - take remaining space
            var listLayoutElement = trackListSection.AddComponent<LayoutElement>();
            listLayoutElement.flexibleHeight = 1;
        }
        
        #endregion
        
        #region Track Management
        
        private void LoadJukeboxTracks()
        {
            try
            {
                NewLoggingSystem.Info("Loading jukebox tracks", "JukeboxScreen");
                isLoading = true;
                UpdateTrackCount("Loading...");
                
                // Clear existing tracks
                ClearTrackList();
                
                // Get tracks from backend
                availableTracks = musicBackend.GetJukeboxTracks();
                
                PopulateTrackList();
                isLoading = false;
                
                NewLoggingSystem.Info($"Loaded {availableTracks.Count} jukebox tracks", "JukeboxScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to load jukebox tracks: {ex}", "JukeboxScreen");
                isLoading = false;
                UpdateTrackCount("Error loading tracks");
            }
        }
        

        
        private void PopulateTrackList()
        {
            UpdateTrackCount($"{availableTracks.Count} tracks available");
            
            foreach (var track in availableTracks)
            {
                CreateTrackItem(track);
            }
        }
        
        private void CreateTrackItem(NewSongDetails track)
        {
            var trackItem = ModernUIFactory.CreateCard(trackListContent.transform, new Vector2(0, 70), true);
            
            var itemLayout = trackItem.AddComponent<HorizontalLayoutGroup>();
            itemLayout.spacing = 15;
            itemLayout.padding = new RectOffset(15, 15, 10, 10);
            itemLayout.childControlHeight = true;
            itemLayout.childControlWidth = false;
            itemLayout.childForceExpandHeight = false;
            itemLayout.childForceExpandWidth = false;
            
            // Track icon
            var iconText = ModernUIFactory.CreateModernText(trackItem.transform, "♫", 24, TextStyle.Primary, TextAnchor.MiddleCenter);
            var iconLayout = iconText.gameObject.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = 40;
            iconLayout.preferredHeight = 40;
            
            // Track info section
            var infoContainer = ModernUIFactory.CreateVerticalLayout(trackItem.transform, 2, new RectOffset(0, 0, 0, 0));
            var infoLayout = infoContainer.AddComponent<LayoutElement>();
            infoLayout.flexibleWidth = 1;
            
            var titleText = ModernUIFactory.CreateModernText(infoContainer.transform, track.title, 14, TextStyle.Primary);
            titleText.fontStyle = FontStyle.Bold;
            var titleLayout = titleText.gameObject.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 20;
            
            var artistText = ModernUIFactory.CreateModernText(infoContainer.transform, $"{track.artist} • {FormatDuration(track.duration)}", 12, TextStyle.Secondary);
            var artistLayout = artistText.gameObject.AddComponent<LayoutElement>();
            artistLayout.preferredHeight = 18;
            
            // Play button
            var playButton = ModernUIFactory.CreateIconButton(trackItem.transform, ">", S1Factory.ConvertToUnityAction(() => OnTrackPlay(track)), new Vector2(45, 45));
            var playLayout = playButton.gameObject.AddComponent<LayoutElement>();
            playLayout.preferredWidth = 45;
            playLayout.preferredHeight = 45;
            
            // Add to playlist button
            var addButton = ModernUIFactory.CreateIconButton(trackItem.transform, "+", S1Factory.ConvertToUnityAction(() => OnTrackAdd(track)), new Vector2(35, 35));
            var addLayout = addButton.gameObject.AddComponent<LayoutElement>();
            addLayout.preferredWidth = 35;
            addLayout.preferredHeight = 35;
            
            // Track item layout element
            var trackLayout = trackItem.AddComponent<LayoutElement>();
            trackLayout.preferredHeight = 70;
            
            trackItems.Add(trackItem);
        }
        
        private void ClearTrackList()
        {
            foreach (var item in trackItems)
            {
                if (item != null)
                    DestroyImmediate(item);
            }
            trackItems.Clear();
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
            musicBackend.PlayTrack(track);
        }
        
        private void OnTrackAdd(NewSongDetails track)
        {
            NewLoggingSystem.Info($"Add track to playlist: {track.title}", "JukeboxScreen");
            musicBackend.AddToQueue(track);
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