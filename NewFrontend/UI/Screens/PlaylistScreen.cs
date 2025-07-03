using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.NewBackend;
using BackSpeakerMod.S1Wrapper;
using BackSpeakerMod.NewFrontend.UI.Interfaces;

namespace BackSpeakerMod.NewFrontend.UI.Screens
{
    /// <summary>
    /// Playlist screen for managing and viewing music playlists
    /// Features: Create playlists, view existing playlists, edit playlists, play playlists
    /// </summary>
    public class PlaylistScreen : MonoBehaviour
    {
        #region Private Fields
        
        private BackSpeakerMainManager? mainManager;
        private NavigationManager? navigationManager;
        private IMusicBackend? musicBackend;
        
        // UI Elements
        private GameObject? mainContainer;
        private GameObject? headerSection;
        private GameObject? playlistsSection;
        private ScrollRect? playlistsScrollView;
        private GameObject? playlistsListContent;
        
        // Header components
        private Button? backButton;
        private Text? titleText;
        private Text? playlistCountText;
        private Button? createButton;
        
        // Playlist management
        private List<GameObject>? playlistItems;
        private List<NewYouTubePlaylistInfo>? availablePlaylists;
        
        // State
        private bool isInitialized = false;
        
        #endregion
        
        #region Public Properties
        
        public bool IsInitialized => isInitialized;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the playlist screen
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
                
                NewLoggingSystem.Info("Initializing PlaylistScreen", "PlaylistScreen");
                
                playlistItems = new List<GameObject>();
                availablePlaylists = new List<NewYouTubePlaylistInfo>();
                
                CreateMainLayout();
                CreateHeader();
                CreatePlaylistsList();
                
                LoadPlaylists();
                
                isInitialized = true;
                NewLoggingSystem.Info("PlaylistScreen initialized successfully", "PlaylistScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to initialize PlaylistScreen: {ex}", "PlaylistScreen");
            }
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateMainLayout()
        {
            // Main container
            mainContainer = new GameObject("PlaylistMainContainer");
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
            if (mainContainer != null)
            {
                headerSection = ModernUIFactory.CreateCard(mainContainer.transform, new Vector2(0, 80), false);
            }
            
            var headerLayout = headerSection?.AddComponent<HorizontalLayoutGroup>();
            if (headerLayout != null)
            {
                headerLayout.spacing = 15;
                headerLayout.padding = new RectOffset(20, 20, 15, 15);
                headerLayout.childControlHeight = true;
                headerLayout.childControlWidth = false;
                headerLayout.childForceExpandHeight = false;
                headerLayout.childForceExpandWidth = false;
            }
            
            // Back button
            if (headerSection != null)
            {
                backButton = ModernUIFactory.CreateIconButton(headerSection.transform, "←", S1Factory.ConvertToUnityAction(OnBackClick), new Vector2(40, 40), false);
            }
            var backLayout = backButton?.gameObject?.AddComponent<LayoutElement>();
            if (backLayout != null)
            {
                backLayout.preferredWidth = 40;
                backLayout.preferredHeight = 40;
            }
            
            // Title and info section
            GameObject? titleContainer = null;
            if (headerSection != null)
            {
                titleContainer = ModernUIFactory.CreateVerticalLayout(headerSection.transform, 2, new RectOffset(0, 0, 0, 0));
            }
            var titleContainerLayout = titleContainer?.AddComponent<LayoutElement>();
            if (titleContainerLayout != null)
            {
                titleContainerLayout.flexibleWidth = 1;
            }
            
            if (titleContainer != null)
            {
                titleText = ModernUIFactory.CreateModernText(titleContainer.transform, "P My Playlists", 18, TextStyle.Primary);
                if (titleText != null)
                {
                    titleText.fontStyle = FontStyle.Bold;
                    var titleTextLayout = titleText.gameObject?.AddComponent<LayoutElement>();
                    if (titleTextLayout != null)
                    {
                        titleTextLayout.preferredHeight = 25;
                    }
                }
                
                playlistCountText = ModernUIFactory.CreateModernText(titleContainer.transform, "Loading playlists...", 12, TextStyle.Secondary);
            }
            var countLayout = playlistCountText?.gameObject?.AddComponent<LayoutElement>();
            if (countLayout != null)
            {
                countLayout.preferredHeight = 20;
            }
            
            // Create playlist button
            if (headerSection != null)
            {
                createButton = ModernUIFactory.CreateIconButton(headerSection.transform, "+", S1Factory.ConvertToUnityAction(OnCreateClick), new Vector2(40, 40), false);
            }
            var createLayout = createButton?.gameObject?.AddComponent<LayoutElement>();
            if (createLayout != null)
            {
                createLayout.preferredWidth = 40;
                createLayout.preferredHeight = 40;
            }
            
            // Header layout element
            var headerLayoutElement = headerSection?.AddComponent<LayoutElement>();
            if (headerLayoutElement != null)
            {
                headerLayoutElement.preferredHeight = 80;
                headerLayoutElement.flexibleHeight = 0;  // Don't expand
            }
        }
        
        private void CreatePlaylistsList()
        {
            if (mainContainer == null) return;
            
            playlistsSection = new GameObject("PlaylistsList");
            playlistsSection.transform.SetParent(mainContainer.transform, false);
            
            // Simple background
            var scrollImage = playlistsSection.AddComponent<Image>();
            scrollImage.color = ModernUIFactory.Colors.Background;
            
            // SIMPLIFIED: Direct content container, no complex viewport masking
            playlistsListContent = new GameObject("Content");
            playlistsListContent.transform.SetParent(playlistsSection.transform, false);
            
            var contentRect = playlistsListContent.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Simple content layout
            var contentLayout = playlistsListContent.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 8;
            contentLayout.padding = new RectOffset(15, 15, 15, 15);
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            
            // Content size fitter for proper scrolling
            var sizeFitter = playlistsListContent.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            // Simple scroll setup
            playlistsScrollView = playlistsSection.AddComponent<ScrollRect>();
            playlistsScrollView.content = contentRect;
            playlistsScrollView.vertical = true;
            playlistsScrollView.horizontal = false;
            playlistsScrollView.scrollSensitivity = 15;
            
            // Playlists list layout element - take remaining space
            var listLayoutElement = playlistsSection.AddComponent<LayoutElement>();
            listLayoutElement.flexibleHeight = 1;
        }
        
        #endregion
        
        #region Playlist Management
        
        private void LoadPlaylists()
        {
            try
            {
                NewLoggingSystem.Info("Loading playlists", "PlaylistScreen");
                // Loading playlists
                
                // Clear existing playlists
                ClearPlaylistsList();
                
                // Get playlists from backend
                availablePlaylists = musicBackend?.GetAllPlaylists() ?? new List<NewYouTubePlaylistInfo>();
                
                if (availablePlaylists.Count == 0)
                {
                    UpdatePlaylistCount("No playlists");
                    ShowEmptyState();
                }
                else
                {
                    UpdatePlaylistCount($"{availablePlaylists.Count} playlists");
                    PopulatePlaylistsList();
                }
                
                // Finished loading
                NewLoggingSystem.Info($"Loaded {availablePlaylists.Count} playlists", "PlaylistScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to load playlists: {ex}", "PlaylistScreen");
                // Failed to load
            }
        }
        
        private void ShowEmptyState()
        {
            if (playlistsListContent == null) return;
            
            var emptyCard = ModernUIFactory.CreateCard(playlistsListContent.transform, new Vector2(0, 150), true);
            
            var emptyLayout = emptyCard.AddComponent<VerticalLayoutGroup>();
            emptyLayout.spacing = 15;
            emptyLayout.padding = new RectOffset(30, 30, 30, 30);
            emptyLayout.childControlHeight = false;
            emptyLayout.childControlWidth = true;
            emptyLayout.childAlignment = TextAnchor.MiddleCenter;
            
            var icon = ModernUIFactory.CreateModernText(emptyCard.transform, "♬", 48, TextStyle.Muted, TextAnchor.MiddleCenter);
            var iconLayout = icon.gameObject.AddComponent<LayoutElement>();
            iconLayout.preferredHeight = 60;
            
            var title = ModernUIFactory.CreateModernText(emptyCard.transform, "No Playlists Yet", 20, TextStyle.Primary, TextAnchor.MiddleCenter);
            if (title != null)
            {
                title.fontStyle = FontStyle.Bold;
                var titleLayout = title.gameObject.AddComponent<LayoutElement>();
                titleLayout.preferredHeight = 30;
            }
            
            var description = ModernUIFactory.CreateModernText(emptyCard.transform, "Create your first playlist by tapping the + button", 14, TextStyle.Secondary, TextAnchor.MiddleCenter);
            var descLayout = description.gameObject.AddComponent<LayoutElement>();
            descLayout.preferredHeight = 20;
            
            var cardLayout = emptyCard.AddComponent<LayoutElement>();
            cardLayout.preferredHeight = 150;
            
            playlistItems?.Add(emptyCard);
        }
        
        private void PopulatePlaylistsList()
        {
            if (availablePlaylists != null)
            {
                foreach (var playlist in availablePlaylists)
                {
                    CreatePlaylistItem(playlist);
                }
            }
        }
        
        private void CreatePlaylistItem(NewYouTubePlaylistInfo playlist)
        {
            if (playlistsListContent == null) return;
            
            var playlistItem = ModernUIFactory.CreateCard(playlistsListContent.transform, new Vector2(0, 100), true);
            
            var itemLayout = playlistItem.AddComponent<HorizontalLayoutGroup>();
            itemLayout.spacing = 15;
            itemLayout.padding = new RectOffset(20, 15, 15, 15);
            itemLayout.childControlHeight = true;
            itemLayout.childControlWidth = false;
            itemLayout.childForceExpandHeight = false;
            itemLayout.childForceExpandWidth = false;
            
            // Playlist icon
            Text? iconText = null;
            if (playlistItem != null)
            {
                iconText = ModernUIFactory.CreateModernText(playlistItem.transform, "📁", 24, TextStyle.Primary, TextAnchor.MiddleCenter);
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
            
            // Playlist info section
            var infoContainer = ModernUIFactory.CreateVerticalLayout(playlistItem.transform, 5, new RectOffset(0, 0, 0, 0));
            var infoLayout = infoContainer.AddComponent<LayoutElement>();
            infoLayout.flexibleWidth = 1;
            
            var nameText = ModernUIFactory.CreateModernText(infoContainer.transform, playlist.name, 18, TextStyle.Primary);
            if (nameText != null)
            {
                nameText.fontStyle = FontStyle.Bold;
                var nameLayout = nameText.gameObject.AddComponent<LayoutElement>();
                nameLayout.preferredHeight = 25;
            }
            
            var descText = ModernUIFactory.CreateModernText(infoContainer.transform, playlist.description, 14, TextStyle.Secondary);
            var descLayout = descText?.gameObject?.AddComponent<LayoutElement>();
            if (descLayout != null)
            {
                descLayout.preferredHeight = 20;
            }
            
            var detailText = ModernUIFactory.CreateModernText(infoContainer.transform, $"{playlist.songCount} songs • Updated {GetRelativeTime(playlist.lastModified)}", 12, TextStyle.Muted);
            var detailLayout = detailText.gameObject.AddComponent<LayoutElement>();
            detailLayout.preferredHeight = 15;
            
            // Action buttons
            // Play button
            var playButton = ModernUIFactory.CreateIconButton(playlistItem.transform, ">", S1Factory.ConvertToUnityAction(() => OnPlaylistPlay(playlist)), new Vector2(55, 55));
            var playLayout = playButton.gameObject.AddComponent<LayoutElement>();
            playLayout.preferredWidth = 55;
            playLayout.preferredHeight = 55;
            
            // Edit button
            var editButton = ModernUIFactory.CreateIconButton(playlistItem.transform, "E", S1Factory.ConvertToUnityAction(() => OnPlaylistEdit(playlist)), new Vector2(45, 45));
            var editLayout = editButton.gameObject.AddComponent<LayoutElement>();
            editLayout.preferredWidth = 45;
            editLayout.preferredHeight = 45;
            
            // Delete button
            var deleteButton = ModernUIFactory.CreateIconButton(playlistItem.transform, "X", S1Factory.ConvertToUnityAction(() => OnPlaylistDelete(playlist)), new Vector2(45, 45));
            var deleteImage = deleteButton.GetComponent<Image>();
            if (deleteImage != null)
            {
                deleteImage.color = ModernUIFactory.Colors.Accent;
            }
            var deleteLayout = deleteButton.gameObject.AddComponent<LayoutElement>();
            deleteLayout.preferredWidth = 45;
            deleteLayout.preferredHeight = 45;
            
            // Playlist item layout element
            var playlistLayout = playlistItem.AddComponent<LayoutElement>();
            playlistLayout.preferredHeight = 100;
            
            playlistItems?.Add(playlistItem);
        }
        
        private void ClearPlaylistsList()
        {
            if (playlistItems != null)
            {
                foreach (var item in playlistItems)
                {
                    if (item != null)
                        DestroyImmediate(item);
                }
                playlistItems.Clear();
            }
        }
        
        private void UpdatePlaylistCount(string text)
        {
            if (playlistCountText != null)
            {
                playlistCountText.text = text;
            }
        }
        
        private string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            
            if (timeSpan.Days > 0)
                return $"{timeSpan.Days} days ago";
            else if (timeSpan.Hours > 0)
                return $"{timeSpan.Hours} hours ago";
            else if (timeSpan.Minutes > 0)
                return $"{timeSpan.Minutes} minutes ago";
            else
                return "Just now";
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnBackClick()
        {
            NewLoggingSystem.Info("Back button clicked", "PlaylistScreen");
            navigationManager?.NavigateBack();
        }
        
        private void OnCreateClick()
        {
            NewLoggingSystem.Info("Create playlist button clicked", "PlaylistScreen");
            CreateNewPlaylist();
        }
        
        private void OnPlaylistPlay(NewYouTubePlaylistInfo playlist)
        {
            NewLoggingSystem.Info($"Playing playlist: {playlist.name}", "PlaylistScreen");
            
            try
            {
                // Load the full playlist from backend
                var fullPlaylist = musicBackend?.LoadPlaylist(playlist.id);
                if (fullPlaylist != null && fullPlaylist.songs.Count > 0)
                {
                    // Set playlist as current queue and start playing
                    musicBackend?.SetQueue(fullPlaylist.songs);
                    musicBackend?.PlayTrack(fullPlaylist.songs[0]);
                    
                    ShowTemporaryMessage($"Playing '{playlist.name}' ({fullPlaylist.songs.Count} songs)");
                    
                    // Navigate back to main player to show playback
                    navigationManager?.ShowMainScreen();
                }
                else
                {
                    ShowTemporaryMessage($"Playlist '{playlist.name}' is empty or couldn't be loaded");
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to play playlist {playlist.name}: {ex}", "PlaylistScreen");
                ShowTemporaryMessage("Failed to play playlist");
            }
        }
        
        private void OnPlaylistEdit(NewYouTubePlaylistInfo playlist)
        {
            NewLoggingSystem.Info($"Editing playlist: {playlist.name}", "PlaylistScreen");
            ShowPlaylistEditDialog(playlist);
        }
        
        private void OnPlaylistDelete(NewYouTubePlaylistInfo playlist)
        {
            NewLoggingSystem.Info($"Deleting playlist: {playlist.name}", "PlaylistScreen");
            ShowDeleteConfirmationDialog(playlist);
        }
        
        private void CreateNewPlaylist()
        {
            try
            {
                // Create playlist with default name
                var timestamp = DateTime.Now.ToString("MM/dd HH:mm");
                var playlistName = $"Playlist {timestamp}";
                
                var newPlaylist = musicBackend?.CreatePlaylist(playlistName, "Created from BackSpeaker");
                
                if (newPlaylist != null)
                {
                    ShowTemporaryMessage($"Created '{playlistName}'!");
                    
                    // Refresh the playlists list
                    LoadPlaylists();
                }
                else
                {
                    ShowTemporaryMessage("Failed to create playlist");
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to create new playlist: {ex}", "PlaylistScreen");
                ShowTemporaryMessage("Failed to create playlist");
            }
        }
        
        private void ShowPlaylistEditDialog(NewYouTubePlaylistInfo playlist)
        {
            // Create modal overlay
            var overlay = new GameObject("EditPlaylistOverlay");
            overlay.transform.SetParent(this.transform, false);
            
            var overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            
            var overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.7f); // Semi-transparent black
            
            // Create dialog card
            var dialog = ModernUIFactory.CreateCard(overlay.transform, new Vector2(350, 200), true);
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
            var title = ModernUIFactory.CreateModernText(dialog.transform, "Edit Playlist", 18, TextStyle.Primary);
            title.fontStyle = FontStyle.Bold;
            var titleLayout = title.gameObject.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 25;
            
            // Name input placeholder (simplified)
            var nameContainer = ModernUIFactory.CreateCard(dialog.transform, new Vector2(0, 40), false);
            var nameText = ModernUIFactory.CreateModernText(nameContainer.transform, playlist.name, 16, TextStyle.Primary);
            var nameLayout = nameContainer.AddComponent<LayoutElement>();
            nameLayout.preferredHeight = 40;
            
            // Description input placeholder (simplified)
            var descContainer = ModernUIFactory.CreateCard(dialog.transform, new Vector2(0, 40), false);
            var descText = ModernUIFactory.CreateModernText(descContainer.transform, playlist.description, 14, TextStyle.Secondary);
            var descLayout = descContainer.AddComponent<LayoutElement>();
            descLayout.preferredHeight = 40;
            
            // Buttons
            var buttonContainer = ModernUIFactory.CreateHorizontalLayout(dialog.transform, 10, new RectOffset(0, 0, 0, 0));
            var buttonLayout = buttonContainer.AddComponent<LayoutElement>();
            buttonLayout.preferredHeight = 50;
            
            var cancelButton = ModernUIFactory.CreateModernButton(buttonContainer.transform, "Cancel", S1Factory.ConvertToUnityAction(() => DestroyImmediate(overlay)), ButtonStyle.Secondary, new Vector2(100, 40));
            var saveButton = ModernUIFactory.CreateModernButton(buttonContainer.transform, "Save", S1Factory.ConvertToUnityAction(() => {
                // For now, just close dialog - in full implementation would update playlist
                ShowTemporaryMessage($"Playlist editing - Coming in future update!");
                DestroyImmediate(overlay);
            }), ButtonStyle.Primary, new Vector2(100, 40));
        }
        
        private void ShowDeleteConfirmationDialog(NewYouTubePlaylistInfo playlist)
        {
            // Create modal overlay
            var overlay = new GameObject("DeleteConfirmationOverlay");
            overlay.transform.SetParent(this.transform, false);
            
            var overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            
            var overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.7f);
            
            // Create dialog card
            var dialog = ModernUIFactory.CreateCard(overlay.transform, new Vector2(320, 160), true);
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
            var title = ModernUIFactory.CreateModernText(dialog.transform, "Delete Playlist", 18, TextStyle.Primary);
            title.fontStyle = FontStyle.Bold;
            var titleLayout = title.gameObject.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 25;
            
            // Confirmation message
            var message = ModernUIFactory.CreateModernText(dialog.transform, $"Are you sure you want to delete '{playlist.name}'?", 14, TextStyle.Secondary, TextAnchor.MiddleCenter);
            var messageLayout = message.gameObject.AddComponent<LayoutElement>();
            messageLayout.preferredHeight = 40;
            
            // Buttons
            var buttonContainer = ModernUIFactory.CreateHorizontalLayout(dialog.transform, 10, new RectOffset(0, 0, 0, 0));
            var buttonLayout = buttonContainer.AddComponent<LayoutElement>();
            buttonLayout.preferredHeight = 50;
            
            var cancelButton = ModernUIFactory.CreateModernButton(buttonContainer.transform, "Cancel", S1Factory.ConvertToUnityAction(() => DestroyImmediate(overlay)), ButtonStyle.Secondary, new Vector2(100, 40));
            var deleteButton = ModernUIFactory.CreateModernButton(buttonContainer.transform, "Delete", S1Factory.ConvertToUnityAction(() => {
                ConfirmDeletePlaylist(playlist);
                DestroyImmediate(overlay);
            }), ButtonStyle.Danger, new Vector2(100, 40));
        }
        
        private void ConfirmDeletePlaylist(NewYouTubePlaylistInfo playlist)
        {
            try
            {
                bool success = musicBackend?.DeletePlaylist(playlist.id) ?? false;
                
                if (success)
                {
                    ShowTemporaryMessage($"Deleted '{playlist.name}'");
                    
                    // Refresh the playlists list
                    LoadPlaylists();
                }
                else
                {
                    ShowTemporaryMessage("Failed to delete playlist");
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to delete playlist {playlist.name}: {ex}", "PlaylistScreen");
                ShowTemporaryMessage("Failed to delete playlist");
            }
        }
        
        private void ShowTemporaryMessage(string message)
        {
            // Create a temporary message that fades out
            var messageObj = ModernUIFactory.CreateCard(this.transform, new Vector2(300, 60), true);
            messageObj.GetComponent<Image>().color = ModernUIFactory.Colors.Primary;
            
            var messageRect = messageObj.GetComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0.5f, 0.8f);
            messageRect.anchorMax = new Vector2(0.5f, 0.8f);
            messageRect.pivot = new Vector2(0.5f, 0.5f);
            
            var messageText = ModernUIFactory.CreateModernText(messageObj.transform, message, 16, TextStyle.Primary, TextAnchor.MiddleCenter);
            var textRect = messageText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Auto-destroy after 3 seconds
            MelonCoroutines.Start(DestroyMessageAfterDelay(messageObj, 3f));
        }

        private IEnumerator DestroyMessageAfterDelay(GameObject messageObj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (messageObj != null)
                DestroyImmediate(messageObj);
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        public void OnDestroy()
        {
            ClearPlaylistsList();
        }
        
        #endregion
    }
} 