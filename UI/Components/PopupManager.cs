using UnityEngine;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.UI.Components
{
    /// <summary>
    /// Centralized popup management system for efficient create-once, show/hide pattern
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        // Dependencies
        private BackSpeakerManager? manager;
        private Transform? appContainer;
        
        // Popup Components (created once, reused)
        private YouTubePopupComponent? youtubePopup;
        private GameObject? playlistPopup;
        private GameObject? managePlaylistsPopup;
        
        // State tracking
        private bool isInitialized = false;
        
        public PopupManager() : base() { }
        
        /// <summary>
        /// Initialize the popup manager with all popup components
        /// </summary>
        public void Initialize(BackSpeakerManager manager, Transform appContainer)
        {
            if (isInitialized) return;
            
            try
            {
                LoggingSystem.Info("Initializing PopupManager with create-once pattern", "UI");
                
                this.manager = manager;
                this.appContainer = appContainer;
                
                // Create all popups once (but keep them inactive)
                CreateYouTubePopup();
                CreatePlaylistPopup();  
                CreateManagePlaylistsPopup();
                
                isInitialized = true;
                LoggingSystem.Info("✓ PopupManager initialized - all popups created and ready", "UI");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to initialize PopupManager: {ex}", "UI");
                throw;
            }
        }
        
        /// <summary>
        /// Show YouTube search popup
        /// </summary>
        public void ShowYouTubePopup()
        {
            if (!isInitialized || youtubePopup == null) return;
            
            try
            {
                LoggingSystem.Info("Showing YouTube popup", "UI");
                youtubePopup.gameObject.SetActive(true);
                youtubePopup.transform.SetAsLastSibling(); // Ensure it's on top
                youtubePopup.RefreshContent(); // Refresh data without recreating UI
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to show YouTube popup: {ex}", "UI");
            }
        }
        
        /// <summary>
        /// Hide YouTube search popup
        /// </summary>
        public void HideYouTubePopup()
        {
            if (youtubePopup != null && youtubePopup.gameObject.activeInHierarchy)
            {
                LoggingSystem.Info("Hiding YouTube popup", "UI");
                youtubePopup.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Show playlist popup
        /// </summary>
        public void ShowPlaylistPopup()
        {
            if (!isInitialized || playlistPopup == null) return;
            
            try
            {
                LoggingSystem.Info("Showing playlist popup", "UI");
                playlistPopup.SetActive(true);
                playlistPopup.transform.SetAsLastSibling();
                // Playlist content will be refreshed by PlaylistToggleComponent
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to show playlist popup: {ex}", "UI");
            }
        }
        
        /// <summary>
        /// Hide playlist popup  
        /// </summary>
        public void HidePlaylistPopup()
        {
            if (playlistPopup != null && playlistPopup.activeInHierarchy)
            {
                LoggingSystem.Info("Hiding playlist popup", "UI");
                playlistPopup.SetActive(false);
            }
        }
        
        /// <summary>
        /// Show manage playlists popup
        /// </summary>
        public void ShowManagePlaylistsPopup()
        {
            if (!isInitialized || managePlaylistsPopup == null) return;
            
            try
            {
                LoggingSystem.Info("Showing manage playlists popup", "UI");
                managePlaylistsPopup.SetActive(true);
                managePlaylistsPopup.transform.SetAsLastSibling();
                // Content refresh will be handled by the component that requests this popup
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to show manage playlists popup: {ex}", "UI");
            }
        }
        
        /// <summary>
        /// Hide manage playlists popup
        /// </summary>
        public void HideManagePlaylistsPopup()
        {
            if (managePlaylistsPopup != null && managePlaylistsPopup.activeInHierarchy)
            {
                LoggingSystem.Info("Hiding manage playlists popup", "UI");
                managePlaylistsPopup.SetActive(false);
            }
        }
        
        /// <summary>
        /// Hide all popups
        /// </summary>
        public void HideAllPopups()
        {
            LoggingSystem.Info("Hiding all popups", "UI");
            HideYouTubePopup();
            HidePlaylistPopup();
            HideManagePlaylistsPopup();
        }
        
        /// <summary>
        /// Get reference to playlist popup for external content management
        /// </summary>
        public GameObject? GetPlaylistPopup() => playlistPopup;
        
        /// <summary>
        /// Get reference to manage playlists popup for external content management
        /// </summary>
        public GameObject? GetManagePlaylistsPopup() => managePlaylistsPopup;
        
        /// <summary>
        /// Create YouTube popup component once
        /// </summary>
        private void CreateYouTubePopup()
        {
            if (appContainer == null || manager == null) return;
            
            var popupContainer = new GameObject("YouTubeSearchPopupComponent");
            popupContainer.transform.SetParent(appContainer, false);
            popupContainer.SetActive(false); // Start inactive
            
            youtubePopup = popupContainer.AddComponent<YouTubePopupComponent>();
            youtubePopup.Setup(manager);
            youtubePopup.CreatePersistentUI(); // Create UI once
            
            LoggingSystem.Debug("Created YouTube popup component", "UI");
        }
        
        /// <summary>
        /// Create playlist popup GameObject once
        /// </summary>
        private void CreatePlaylistPopup()
        {
            if (appContainer == null) return;
            
            playlistPopup = new GameObject("PlaylistPopup");
            playlistPopup.transform.SetParent(appContainer, false);
            playlistPopup.SetActive(false); // Start inactive
            
            var popupRect = playlistPopup.AddComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;
            
            var popupBg = playlistPopup.AddComponent<UnityEngine.UI.Image>();
            popupBg.color = new Color(0f, 0f, 0f, 0.8f);
            popupBg.raycastTarget = true;
            
            LoggingSystem.Debug("Created playlist popup container", "UI");
        }
        
        /// <summary>
        /// Create manage playlists popup GameObject once
        /// </summary>
        private void CreateManagePlaylistsPopup()
        {
            if (appContainer == null) return;
            
            managePlaylistsPopup = new GameObject("ManagePlaylistsPopup");
            managePlaylistsPopup.transform.SetParent(appContainer, false);
            managePlaylistsPopup.SetActive(false); // Start inactive
            
            var popupRect = managePlaylistsPopup.AddComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;
            
            var popupBg = managePlaylistsPopup.AddComponent<UnityEngine.UI.Image>();
            popupBg.color = new Color(0f, 0f, 0f, 0.8f);
            popupBg.raycastTarget = true;
            
            LoggingSystem.Debug("Created manage playlists popup container", "UI");
        }
        
        /// <summary>
        /// Cleanup all popups when leaving main scene
        /// </summary>
        private void OnDestroy()
        {
            LoggingSystem.Info("PopupManager destroyed - cleaning up all popups", "UI");
            
            // Clear references
            manager = null;
            appContainer = null;
            youtubePopup = null;
            playlistPopup = null;
            managePlaylistsPopup = null;
            
            isInitialized = false;
        }
    }
} 