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
    /// YouTube screen for searching and playing YouTube music
    /// Features: Search functionality, results list, play/add functionality, playlist integration
    /// </summary>
    public class YouTubeScreen : MonoBehaviour
    {
        #region Private Fields
        
        private BackSpeakerMainManager mainManager;
        private NavigationManager navigationManager;
        private IMusicBackend musicBackend;
        
        // UI Elements
        private GameObject mainContainer;
        private GameObject headerSection;
        private GameObject searchSection;
        private GameObject resultsSection;
        private ScrollRect resultsScrollView;
        private GameObject resultsListContent;
        
        // Header components
        private Button backButton;
        private Text titleText;
        private Button playlistButton;
        
        // Search components
        private InputField searchInput;
        private Button searchButton;
        private Text searchStatus;
        
        // Results management
        private List<GameObject> resultItems;
        private List<NewSongDetails> searchResults;
        
        // State
        private bool isInitialized = false;
        private bool isSearching = false;
        private string lastSearchQuery = "";
        
        #endregion
        
        #region Public Properties
        
        public bool IsInitialized => isInitialized;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the YouTube screen
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
                
                NewLoggingSystem.Info("Initializing YouTubeScreen", "YouTubeScreen");
                
                resultItems = new List<GameObject>();
                searchResults = new List<NewSongDetails>();
                
                CreateMainLayout();
                CreateHeader();
                CreateSearchSection();
                CreateResultsList();
                
                ShowWelcomeMessage();
                
                isInitialized = true;
                NewLoggingSystem.Info("YouTubeScreen initialized successfully", "YouTubeScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to initialize YouTubeScreen: {ex}", "YouTubeScreen");
            }
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateMainLayout()
        {
            // Main container
            mainContainer = new GameObject("YouTubeMainContainer");
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
            // Use same clean header design as jukebox
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
            
            titleText = ModernUIFactory.CreateModernText(titleContainer.transform, "Y YouTube Music", 18, TextStyle.Primary);
            titleText.fontStyle = FontStyle.Bold;
            var titleTextLayout = titleText.gameObject.AddComponent<LayoutElement>();
            titleTextLayout.preferredHeight = 25;
            
            searchStatus = ModernUIFactory.CreateModernText(titleContainer.transform, "Enter search terms to find YouTube music", 12, TextStyle.Secondary);
            var statusLayout = searchStatus.gameObject.AddComponent<LayoutElement>();
            statusLayout.preferredHeight = 20;
            
            // Playlist button
            playlistButton = ModernUIFactory.CreateIconButton(headerSection.transform, "P", S1Factory.ConvertToUnityAction(OnPlaylistClick), new Vector2(40, 40), false);
            var playlistLayout = playlistButton.gameObject.AddComponent<LayoutElement>();
            playlistLayout.preferredWidth = 40;
            playlistLayout.preferredHeight = 40;
            
            // Header layout element
            var headerLayoutElement = headerSection.AddComponent<LayoutElement>();
            headerLayoutElement.preferredHeight = 80;
            headerLayoutElement.flexibleHeight = 0;  // Don't expand
        }
        
        private void CreateSearchSection()
        {
            // Simple search bar only
            searchSection = ModernUIFactory.CreateCard(mainContainer.transform, new Vector2(0, 60), false);
            
            var searchLayout = searchSection.AddComponent<HorizontalLayoutGroup>();
            searchLayout.spacing = 10;
            searchLayout.padding = new RectOffset(20, 20, 10, 10);
            searchLayout.childControlHeight = true;
            searchLayout.childControlWidth = false;
            
            // Search input field
            var inputFieldObj = ModernUIFactory.CreateCard(searchSection.transform, new Vector2(0, 40), true);
            inputFieldObj.GetComponent<Image>().color = ModernUIFactory.Colors.Background;
            
            searchInput = inputFieldObj.AddComponent<InputField>();
            searchInput.textComponent = ModernUIFactory.CreateModernText(inputFieldObj.transform, "", 16, TextStyle.Primary, TextAnchor.MiddleLeft);
            searchInput.placeholder = ModernUIFactory.CreateModernText(inputFieldObj.transform, "Search YouTube...", 16, TextStyle.Muted, TextAnchor.MiddleLeft);
            
            // Adjust text positioning
            var textRect = searchInput.textComponent.GetComponent<RectTransform>();
            textRect.offsetMin = new Vector2(15, 5);
            textRect.offsetMax = new Vector2(-15, -5);
            
            var placeholderRect = ((Text)searchInput.placeholder).GetComponent<RectTransform>();
            placeholderRect.offsetMin = new Vector2(15, 5);
            placeholderRect.offsetMax = new Vector2(-15, -5);
            
            var inputLayout = inputFieldObj.AddComponent<LayoutElement>();
            inputLayout.flexibleWidth = 1;
            inputLayout.preferredHeight = 40;
            
            // Search button
            searchButton = ModernUIFactory.CreateIconButton(searchSection.transform, "?", S1Factory.ConvertToUnityAction(OnSearchClick), new Vector2(40, 40));
            var searchBtnLayout = searchButton.gameObject.AddComponent<LayoutElement>();
            searchBtnLayout.preferredWidth = 40;
            searchBtnLayout.preferredHeight = 40;
            
            // Search section layout element
            var searchLayoutElement = searchSection.AddComponent<LayoutElement>();
            searchLayoutElement.preferredHeight = 60;
            searchLayoutElement.flexibleHeight = 0;  // Don't expand
        }
        
        private void CreateResultsList()
        {
            resultsSection = new GameObject("ResultsList");
            resultsSection.transform.SetParent(mainContainer.transform, false);
            
            // Simple background
            var scrollImage = resultsSection.AddComponent<Image>();
            scrollImage.color = ModernUIFactory.Colors.Background;
            
            // SIMPLIFIED: Direct content container, no complex viewport masking
            resultsListContent = new GameObject("Content");
            resultsListContent.transform.SetParent(resultsSection.transform, false);
            
            var contentRect = resultsListContent.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Simple content layout
            var contentLayout = resultsListContent.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 8;
            contentLayout.padding = new RectOffset(15, 15, 15, 15);
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            
            // Content size fitter for proper scrolling
            var sizeFitter = resultsListContent.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            // Simple scroll setup
            resultsScrollView = resultsSection.AddComponent<ScrollRect>();
            resultsScrollView.content = contentRect;
            resultsScrollView.vertical = true;
            resultsScrollView.horizontal = false;
            resultsScrollView.scrollSensitivity = 15;
            
            // Results list layout element - take remaining space
            var listLayoutElement = resultsSection.AddComponent<LayoutElement>();
            listLayoutElement.flexibleHeight = 1;
        }
        
        #endregion
        
        #region Search Management
        
        private void ShowWelcomeMessage()
        {
            UpdateSearchStatus("Enter search terms to find YouTube music");
            
            // Show welcome card
            var welcomeCard = ModernUIFactory.CreateCard(resultsListContent.transform, new Vector2(0, 120), true);
            
            var welcomeLayout = welcomeCard.AddComponent<VerticalLayoutGroup>();
            welcomeLayout.spacing = 10;
            welcomeLayout.padding = new RectOffset(20, 20, 20, 20);
            welcomeLayout.childControlHeight = false;
            welcomeLayout.childControlWidth = true;
            welcomeLayout.childAlignment = TextAnchor.MiddleCenter;
            
            var icon = ModernUIFactory.CreateModernText(welcomeCard.transform, "♪", 48, TextStyle.Primary, TextAnchor.MiddleCenter);
            var iconLayout = icon.gameObject.AddComponent<LayoutElement>();
            iconLayout.preferredHeight = 60;
            
            var title = ModernUIFactory.CreateModernText(welcomeCard.transform, "YouTube Music Search", 20, TextStyle.Primary, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            var titleLayout = title.gameObject.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 30;
            
            var description = ModernUIFactory.CreateModernText(welcomeCard.transform, "Search for your favorite songs and add them to your playlists", 14, TextStyle.Secondary, TextAnchor.MiddleCenter);
            var descLayout = description.gameObject.AddComponent<LayoutElement>();
            descLayout.preferredHeight = 20;
            
            var cardLayout = welcomeCard.AddComponent<LayoutElement>();
            cardLayout.preferredHeight = 120;
            
            resultItems.Add(welcomeCard);
        }
        
        private void PerformSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                UpdateSearchStatus("Please enter search terms");
                return;
            }
            
            if (isSearching)
            {
                UpdateSearchStatus("Search in progress...");
                return;
            }
            
            try
            {
                NewLoggingSystem.Info($"Searching YouTube for: {query}", "YouTubeScreen");
                isSearching = true;
                lastSearchQuery = query;
                
                UpdateSearchStatus($"Searching for '{query}'...");
                
                // Clear existing results
                ClearResultsList();
                
                // Get search results from backend
                searchResults = musicBackend.SearchYouTube(query);
                
                PopulateResults();
                isSearching = false;
                
                UpdateSearchStatus($"Found {searchResults.Count} results for '{query}'");
                NewLoggingSystem.Info($"YouTube search completed: {searchResults.Count} results", "YouTubeScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to search YouTube: {ex}", "YouTubeScreen");
                isSearching = false;
                UpdateSearchStatus("Search failed. Please try again.");
            }
        }
        
        private void PopulateResults()
        {
            foreach (var result in searchResults)
            {
                CreateResultItem(result);
            }
        }
        
        private void CreateResultItem(NewSongDetails song)
        {
            var resultItem = ModernUIFactory.CreateCard(resultsListContent.transform, new Vector2(0, 80), true);
            
            var itemLayout = resultItem.AddComponent<HorizontalLayoutGroup>();
            itemLayout.spacing = 15;
            itemLayout.padding = new RectOffset(15, 15, 10, 10);
            itemLayout.childControlHeight = true;
            itemLayout.childControlWidth = false;
            itemLayout.childForceExpandHeight = false;
            itemLayout.childForceExpandWidth = false;
            
            // Music icon
            var iconText = ModernUIFactory.CreateModernText(resultItem.transform, "♫", 24, TextStyle.Primary, TextAnchor.MiddleCenter);
            var iconLayout = iconText.gameObject.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = 40;
            iconLayout.preferredHeight = 40;
            
            // Song info section
            var infoContainer = ModernUIFactory.CreateVerticalLayout(resultItem.transform, 2, new RectOffset(0, 0, 0, 0));
            var infoLayout = infoContainer.AddComponent<LayoutElement>();
            infoLayout.flexibleWidth = 1;
            
            var titleText = ModernUIFactory.CreateModernText(infoContainer.transform, song.title, 16, TextStyle.Primary);
            titleText.fontStyle = FontStyle.Bold;
            var titleLayout = titleText.gameObject.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 25;
            
            var artistText = ModernUIFactory.CreateModernText(infoContainer.transform, song.artist, 14, TextStyle.Secondary);
            var artistLayout = artistText.gameObject.AddComponent<LayoutElement>();
            artistLayout.preferredHeight = 20;
            
            var durationText = ModernUIFactory.CreateModernText(infoContainer.transform, $"{(int)(song.duration / 60)}:{(int)(song.duration % 60):D2}", 12, TextStyle.Muted);
            var durationLayout = durationText.gameObject.AddComponent<LayoutElement>();
            durationLayout.preferredHeight = 15;
            
            // Action buttons
            // Play button
            var playButton = ModernUIFactory.CreateIconButton(resultItem.transform, ">", S1Factory.ConvertToUnityAction(() => OnResultPlay(song)), new Vector2(50, 50));
            var playLayout = playButton.gameObject.AddComponent<LayoutElement>();
            playLayout.preferredWidth = 50;
            playLayout.preferredHeight = 50;
            
            // Add to queue button
            var addButton = ModernUIFactory.CreateIconButton(resultItem.transform, "+", S1Factory.ConvertToUnityAction(() => OnResultAdd(song)), new Vector2(40, 40));
            var addLayout = addButton.gameObject.AddComponent<LayoutElement>();
            addLayout.preferredWidth = 40;
            addLayout.preferredHeight = 40;
            
            // Save to playlist button
            var saveButton = ModernUIFactory.CreateIconButton(resultItem.transform, "♬", S1Factory.ConvertToUnityAction(() => OnResultSave(song)), new Vector2(40, 40));
            var saveLayout = saveButton.gameObject.AddComponent<LayoutElement>();
            saveLayout.preferredWidth = 40;
            saveLayout.preferredHeight = 40;
            
            // Result item layout element
            var resultLayout = resultItem.AddComponent<LayoutElement>();
            resultLayout.preferredHeight = 80;
            
            resultItems.Add(resultItem);
        }
        
        private void ClearResultsList()
        {
            foreach (var item in resultItems)
            {
                if (item != null)
                    DestroyImmediate(item);
            }
            resultItems.Clear();
        }
        
        private void UpdateSearchStatus(string status)
        {
            if (searchStatus != null)
            {
                searchStatus.text = status;
            }
        }
        
        private void ShowPlaylistSaveDialog(NewSongDetails song)
        {
            // Create modal overlay
            var overlay = new GameObject("PlaylistSaveOverlay");
            overlay.transform.SetParent(this.transform, false);
            
            var overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            
            var overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.7f);
            
            // Create dialog card
            var dialog = ModernUIFactory.CreateCard(overlay.transform, new Vector2(350, 300), true);
            var dialogRect = dialog.GetComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.pivot = new Vector2(0.5f, 0.5f);
            
            var dialogLayout = dialog.AddComponent<VerticalLayoutGroup>();
            dialogLayout.spacing = 15;
            dialogLayout.padding = new RectOffset(20, 20, 20, 20);
            dialogLayout.childControlHeight = false;
            dialogLayout.childControlWidth = true;
            
            // Title
            var title = ModernUIFactory.CreateModernText(dialog.transform, "Save to Playlist", 18, TextStyle.Primary);
            title.fontStyle = FontStyle.Bold;
            var titleLayout = title.gameObject.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 25;
            
            // Song info
            var songInfo = ModernUIFactory.CreateModernText(dialog.transform, $"'{song.title}' by {song.artist}", 14, TextStyle.Secondary, TextAnchor.MiddleCenter);
            var songLayout = songInfo.gameObject.AddComponent<LayoutElement>();
            songLayout.preferredHeight = 20;
            
            // Playlist selection section
            var playlistContainer = ModernUIFactory.CreateCard(dialog.transform, new Vector2(0, 120), false);
            var containerLayout = playlistContainer.AddComponent<LayoutElement>();
            containerLayout.preferredHeight = 120;
            
            var playlistScrollContent = new GameObject("PlaylistScrollContent");
            playlistScrollContent.transform.SetParent(playlistContainer.transform, false);
            
            var contentRect = playlistScrollContent.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            var contentVerticalLayout = playlistScrollContent.AddComponent<VerticalLayoutGroup>();
            contentVerticalLayout.spacing = 5;
            contentVerticalLayout.padding = new RectOffset(10, 10, 10, 10);
            contentVerticalLayout.childControlHeight = false;
            contentVerticalLayout.childControlWidth = true;
            
            var contentSizeFitter = playlistScrollContent.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Add scroll functionality
            var scrollRect = playlistContainer.AddComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            
            // Get playlists and populate
            try
            {
                var playlists = musicBackend.GetAllPlaylists();
                
                if (playlists.Count == 0)
                {
                    var noPlaylistsText = ModernUIFactory.CreateModernText(playlistScrollContent.transform, "No playlists found", 14, TextStyle.Muted, TextAnchor.MiddleCenter);
                    var noPlaylistsLayout = noPlaylistsText.gameObject.AddComponent<LayoutElement>();
                    noPlaylistsLayout.preferredHeight = 30;
                }
                else
                {
                    foreach (var playlist in playlists)
                    {
                        var playlistButton = ModernUIFactory.CreateModernButton(playlistScrollContent.transform, playlist.name, 
                            S1Factory.ConvertToUnityAction(() => {
                                SaveToPlaylist(song, playlist);
                                DestroyImmediate(overlay);
                            }), ButtonStyle.Secondary, new Vector2(0, 30));
                        
                        var playlistButtonLayout = playlistButton.gameObject.AddComponent<LayoutElement>();
                        playlistButtonLayout.preferredHeight = 30;
                    }
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to load playlists for save dialog: {ex}", "YouTubeScreen");
                var errorText = ModernUIFactory.CreateModernText(playlistScrollContent.transform, "Error loading playlists", 14, TextStyle.Muted, TextAnchor.MiddleCenter);
                var errorLayout = errorText.gameObject.AddComponent<LayoutElement>();
                errorLayout.preferredHeight = 30;
            }
            
            // Buttons
            var buttonContainer = ModernUIFactory.CreateHorizontalLayout(dialog.transform, 10, new RectOffset(0, 0, 0, 0));
            var buttonLayout = buttonContainer.AddComponent<LayoutElement>();
            buttonLayout.preferredHeight = 50;
            
            var newPlaylistButton = ModernUIFactory.CreateModernButton(buttonContainer.transform, "New Playlist", 
                S1Factory.ConvertToUnityAction(() => {
                    CreateNewPlaylistWithSong(song);
                    DestroyImmediate(overlay);
                }), ButtonStyle.Primary, new Vector2(120, 40));
            
            var cancelButton = ModernUIFactory.CreateModernButton(buttonContainer.transform, "Cancel", 
                S1Factory.ConvertToUnityAction(() => DestroyImmediate(overlay)), ButtonStyle.Secondary, new Vector2(80, 40));
        }
        
        private void SaveToPlaylist(NewSongDetails song, NewYouTubePlaylistInfo playlistInfo)
        {
            try
            {
                // Load the full playlist
                var fullPlaylist = musicBackend.LoadPlaylist(playlistInfo.id);
                if (fullPlaylist != null)
                {
                    // Add song to playlist
                    bool added = fullPlaylist.AddSong(song);
                    if (added)
                    {
                        // Save the updated playlist
                        bool saved = musicBackend.SavePlaylist(fullPlaylist);
                        if (saved)
                        {
                            UpdateSearchStatus($"Saved '{song.title}' to '{playlistInfo.name}'");
                        }
                        else
                        {
                            UpdateSearchStatus("Failed to save playlist");
                        }
                    }
                    else
                    {
                        UpdateSearchStatus($"'{song.title}' already in '{playlistInfo.name}'");
                    }
                }
                else
                {
                    UpdateSearchStatus("Failed to load playlist");
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to save song to playlist: {ex}", "YouTubeScreen");
                UpdateSearchStatus("Failed to save to playlist");
            }
        }
        
        private void CreateNewPlaylistWithSong(NewSongDetails song)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("MM/dd HH:mm");
                var playlistName = $"YouTube Playlist {timestamp}";
                
                var newPlaylist = musicBackend.CreatePlaylist(playlistName, "Created from YouTube search");
                if (newPlaylist != null)
                {
                    // Add the song to the new playlist
                    bool added = newPlaylist.AddSong(song);
                    if (added)
                    {
                        bool saved = musicBackend.SavePlaylist(newPlaylist);
                        if (saved)
                        {
                            UpdateSearchStatus($"Created '{playlistName}' with '{song.title}'");
                        }
                        else
                        {
                            UpdateSearchStatus("Failed to save new playlist");
                        }
                    }
                    else
                    {
                        UpdateSearchStatus("Failed to add song to new playlist");
                    }
                }
                else
                {
                    UpdateSearchStatus("Failed to create new playlist");
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to create new playlist with song: {ex}", "YouTubeScreen");
                UpdateSearchStatus("Failed to create playlist");
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnBackClick()
        {
            NewLoggingSystem.Info("Back button clicked", "YouTubeScreen");
            navigationManager?.NavigateBack();
        }
        
        private void OnSearchClick()
        {
            var query = searchInput?.text?.Trim();
            NewLoggingSystem.Info($"Search button clicked: {query}", "YouTubeScreen");
            PerformSearch(query);
        }
        
        private void OnPlaylistClick()
        {
            NewLoggingSystem.Info("Playlist button clicked", "YouTubeScreen");
            // Navigate back first, then the navigation menu can be used to go to playlists
            navigationManager?.NavigateBack();
        }
        
        private void OnResultPlay(NewSongDetails song)
        {
            NewLoggingSystem.Info($"Playing YouTube song: {song.title}", "YouTubeScreen");
            try
            {
                bool success = musicBackend.PlayTrack(song);
                if (success)
                {
                    UpdateSearchStatus($"Now playing: {song.title}");
                    // Navigate to main player to show playback
                    navigationManager?.ShowMainScreen();
                }
                else
                {
                    UpdateSearchStatus("Failed to play track");
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to play YouTube track: {ex}", "YouTubeScreen");
                UpdateSearchStatus("Failed to play track");
            }
        }
        
        private void OnResultAdd(NewSongDetails song)
        {
            NewLoggingSystem.Info($"Adding YouTube song to queue: {song.title}", "YouTubeScreen");
            try
            {
                bool success = musicBackend.AddToQueue(song);
                if (success)
                {
                    UpdateSearchStatus($"Added '{song.title}' to queue");
                }
                else
                {
                    UpdateSearchStatus("Failed to add to queue");
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to add YouTube track to queue: {ex}", "YouTubeScreen");
                UpdateSearchStatus("Failed to add to queue");
            }
        }
        
        private void OnResultSave(NewSongDetails song)
        {
            NewLoggingSystem.Info($"Saving YouTube song to playlist: {song.title}", "YouTubeScreen");
            ShowPlaylistSaveDialog(song);
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        public void Update()
        {
            // Handle Enter key in search input
            if (searchInput != null && searchInput.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                OnSearchClick();
            }
        }
        
        public void OnDestroy()
        {
            ClearResultsList();
        }
        
        #endregion
    }
} 