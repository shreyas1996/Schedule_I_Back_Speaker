using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.NewBackend;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.NewFrontend.UI.Interfaces;
using BackSpeakerMod.S1Wrapper;

namespace BackSpeakerMod.NewFrontend.UI.Screens
{
    /// <summary>
    /// Screen for managing download progress and download queue
    /// </summary>
    public class DownloadsScreen : MonoBehaviour
    {
        #region Private Fields
        
        private BackSpeakerMainManager? mainManager;
        private NavigationManager? navigationManager;
        private IMusicBackend? musicBackend;
        
        // UI Components
        private GameObject? mainContainer;
        private GameObject? headerSection;
        private GameObject? downloadListSection;
        private ScrollRect? downloadScrollView;
        private GameObject? downloadListContent;
        
        // Header components
        private Button? backButton;
        private Text? titleText;
        private Button? clearCompletedButton;
        private Text? downloadStatsText;
        
        // Download management
        private List<GameObject>? downloadItems;
        private List<DownloadInfo>? currentDownloads;
        
        // State
        private bool isInitialized = false;
        private bool isRefreshing = false;
        
        #endregion
        
        #region Properties
        
        public bool IsInitialized => isInitialized;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the downloads screen
        /// </summary>
        public void Initialize(BackSpeakerMainManager manager, NavigationManager navManager)
        {
            try
            {
                mainManager = manager;
                navigationManager = navManager;
                musicBackend = manager.GetMusicBackend(); // Use the same backend as the rest of the app
                
                downloadItems = new List<GameObject>();
                currentDownloads = new List<DownloadInfo>();
                
                CreateMainLayout();
                CreateHeader();
                CreateDownloadList();
                
                // Load initial downloads
                RefreshDownloads();
                
                isInitialized = true;
                NewLoggingSystem.Info("DownloadsScreen initialized successfully", "DownloadsScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to initialize DownloadsScreen: {ex}", "DownloadsScreen");
            }
        }
        
        /// <summary>
        /// Show the downloads screen
        /// </summary>
        public void Show()
        {
            if (!isInitialized || mainContainer == null) return;
            
            mainContainer.SetActive(true);
            RefreshDownloads();
            NewLoggingSystem.Info("DownloadsScreen shown", "DownloadsScreen");
        }
        
        /// <summary>
        /// Hide the downloads screen
        /// </summary>
        public void Hide()
        {
            if (!isInitialized || mainContainer == null) return;
            
            mainContainer.SetActive(false);
            NewLoggingSystem.Info("DownloadsScreen hidden", "DownloadsScreen");
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateMainLayout()
        {
            // Main container - following JukeboxScreen/LocalMusicScreen pattern
            mainContainer = new GameObject("DownloadsMainContainer");
            mainContainer.transform.SetParent(this.transform, false);
            
            var mainRect = mainContainer.AddComponent<RectTransform>();
            mainRect.anchorMin = Vector2.zero;
            mainRect.anchorMax = Vector2.one;
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;
            
            var mainImage = mainContainer.AddComponent<Image>();
            mainImage.color = ModernUIFactory.Colors.Background;
            
            // Main vertical layout - following the established pattern
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
            if (mainContainer == null) return;
            
            // Following JukeboxScreen/LocalMusicScreen pattern
            headerSection = ModernUIFactory.CreateCard(mainContainer.transform, new Vector2(0, 80), false);
            
            var headerLayout = headerSection.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 15;
            headerLayout.padding = new RectOffset(20, 20, 15, 15);
            headerLayout.childControlHeight = true;
            headerLayout.childControlWidth = false;
            headerLayout.childForceExpandHeight = false;
            headerLayout.childForceExpandWidth = false;
            
            // Back button
            backButton = ModernUIFactory.CreateIconButton(headerSection.transform, "←", 
                S1Factory.ConvertToUnityAction(OnBackClick), new Vector2(40, 40), false);
            var backLayout = backButton.gameObject.AddComponent<LayoutElement>();
            backLayout.preferredWidth = 40;
            backLayout.preferredHeight = 40;
            
            // Title and stats section
            var titleContainer = ModernUIFactory.CreateVerticalLayout(headerSection.transform, 2, new RectOffset(0, 0, 0, 0));
            var titleContainerLayout = titleContainer.AddComponent<LayoutElement>();
            titleContainerLayout.flexibleWidth = 1;
            
            titleText = ModernUIFactory.CreateModernText(titleContainer.transform, "📥 Downloads", 18, TextStyle.Primary);
            titleText.fontStyle = FontStyle.Bold;
            var titleTextLayout = titleText.gameObject.AddComponent<LayoutElement>();
            titleTextLayout.preferredHeight = 25;
            
            downloadStatsText = ModernUIFactory.CreateModernText(titleContainer.transform, "Loading downloads...", 12, TextStyle.Secondary);
            var statsLayout = downloadStatsText.gameObject.AddComponent<LayoutElement>();
            statsLayout.preferredHeight = 20;
            
            // Clear completed button
            clearCompletedButton = ModernUIFactory.CreateIconButton(headerSection.transform, "🗑", 
                S1Factory.ConvertToUnityAction(OnClearCompletedClick), new Vector2(40, 40), false);
            var clearLayout = clearCompletedButton.gameObject.AddComponent<LayoutElement>();
            clearLayout.preferredWidth = 40;
            clearLayout.preferredHeight = 40;
            
            // Header layout element - following the pattern
            var headerLayoutElement = headerSection.AddComponent<LayoutElement>();
            headerLayoutElement.preferredHeight = 80;
            headerLayoutElement.flexibleHeight = 0;  // Don't expand
        }
        
        private void CreateDownloadList()
        {
            if (mainContainer == null) return;
            
            // Following JukeboxScreen/LocalMusicScreen pattern
            downloadListSection = new GameObject("DownloadList");
            downloadListSection.transform.SetParent(mainContainer.transform, false);
            
            // Simple background
            var scrollImage = downloadListSection.AddComponent<Image>();
            scrollImage.color = ModernUIFactory.Colors.Background;
            
            // SIMPLIFIED: Direct content container, no complex viewport masking
            downloadListContent = new GameObject("Content");
            downloadListContent.transform.SetParent(downloadListSection.transform, false);
            
            var contentRect = downloadListContent.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Simple content layout
            var contentLayout = downloadListContent.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 8;
            contentLayout.padding = new RectOffset(15, 15, 15, 15);
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            
            // Content size fitter for proper scrolling
            var sizeFitter = downloadListContent.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            // Simple scroll setup
            downloadScrollView = downloadListSection.AddComponent<ScrollRect>();
            downloadScrollView.content = contentRect;
            downloadScrollView.vertical = true;
            downloadScrollView.horizontal = false;
            downloadScrollView.scrollSensitivity = 15;
            
            // Download list layout element - take remaining space
            var listLayoutElement = downloadListSection.AddComponent<LayoutElement>();
            listLayoutElement.flexibleHeight = 1;
        }
        
        #endregion
        
        #region Download Management
        
        private void RefreshDownloads()
        {
            try
            {
                if (isRefreshing || musicBackend == null) return;
                
                isRefreshing = true;
                NewLoggingSystem.Info("Refreshing downloads", "DownloadsScreen");
                
                // Clear existing items
                ClearDownloadList();
                
                // Get all downloads (active and completed)
                var activeDownloads = musicBackend.GetActiveDownloads();
                var allDownloads = new List<DownloadInfo>(activeDownloads);
                
                currentDownloads = allDownloads;
                PopulateDownloadList();
                UpdateDownloadStats();
                
                isRefreshing = false;
                NewLoggingSystem.Info($"Refreshed {allDownloads.Count} downloads", "DownloadsScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to refresh downloads: {ex}", "DownloadsScreen");
                isRefreshing = false;
                UpdateDownloadStats("Error loading downloads");
            }
        }
        
        private void PopulateDownloadList()
        {
            if (currentDownloads.Count == 0)
            {
                CreateEmptyState();
                return;
            }
            
            foreach (var download in currentDownloads)
            {
                CreateDownloadItem(download);
            }
        }
        
        private void CreateEmptyState()
        {
            if (downloadListContent == null) return;
            
            var emptyState = ModernUIFactory.CreateCard(downloadListContent.transform, new Vector2(0, 150), false);
            var emptyLayout = emptyState?.AddComponent<LayoutElement>();
            if (emptyLayout != null)
            {
                emptyLayout.preferredHeight = 150;
            }
            
            GameObject? emptyContent = null;
            if (emptyState != null)
            {
                emptyContent = ModernUIFactory.CreateVerticalLayout(emptyState.transform, 10, new RectOffset(20, 20, 20, 20));
            }
            
            Text? emptyIcon = null;
            if (emptyContent != null)
            {
                emptyIcon = ModernUIFactory.CreateModernText(emptyContent.transform, "📥", 48, TextStyle.Secondary, TextAnchor.MiddleCenter);
            }
            var iconLayout = emptyIcon?.gameObject?.AddComponent<LayoutElement>();
            if (iconLayout != null)
            {
                iconLayout.preferredHeight = 60;
            }
            
            Text? emptyText = null;
            if (emptyContent != null)
            {
                emptyText = ModernUIFactory.CreateModernText(emptyContent.transform, "No downloads yet", 16, TextStyle.Primary, TextAnchor.MiddleCenter);
            }
            var textLayout = emptyText?.gameObject?.AddComponent<LayoutElement>();
            if (textLayout != null)
            {
                textLayout.preferredHeight = 25;
            }
            
            Text? emptySubtext = null;
            if (emptyContent != null)
            {
                emptySubtext = ModernUIFactory.CreateModernText(emptyContent.transform, "Download songs from YouTube to enjoy them offline", 12, TextStyle.Secondary, TextAnchor.MiddleCenter);
            }
            var subtextLayout = emptySubtext?.gameObject?.AddComponent<LayoutElement>();
            if (subtextLayout != null)
            {
                subtextLayout.preferredHeight = 20;
            }
            
            if (emptyState != null && downloadItems != null)
            {
                downloadItems.Add(emptyState);
            }
        }
        
        private void CreateDownloadItem(DownloadInfo download)
        {
            if (downloadListContent == null || downloadItems == null) return;
            
            var downloadItem = ModernUIFactory.CreateCard(downloadListContent.transform, new Vector2(0, 100), false);
            
            var itemLayout = downloadItem?.AddComponent<VerticalLayoutGroup>();
            if (itemLayout != null)
            {
                itemLayout.spacing = 8;
                itemLayout.padding = new RectOffset(15, 15, 10, 10);
                itemLayout.childControlHeight = false;
                itemLayout.childControlWidth = true;
                itemLayout.childForceExpandHeight = false;
                itemLayout.childForceExpandWidth = true;
            }
            
            // Top row - song info and status
            GameObject? topRow = null;
            if (downloadItem != null)
            {
                topRow = ModernUIFactory.CreateHorizontalLayout(downloadItem.transform, 10);
            }
            var topLayout = topRow?.AddComponent<LayoutElement>();
            if (topLayout != null)
            {
                topLayout.preferredHeight = 35;
            }
            
            // Download icon
            var statusIcon = GetStatusIcon(download.status);
            Text? iconText = null;
            if (topRow != null)
            {
                iconText = ModernUIFactory.CreateModernText(topRow.transform, statusIcon, 20, TextStyle.Primary, TextAnchor.MiddleCenter);
            }
            var iconLayout = iconText?.gameObject?.AddComponent<LayoutElement>();
            if (iconLayout != null)
            {
                iconLayout.preferredWidth = 30;
                iconLayout.preferredHeight = 30;
            }
            
            // Song info
            GameObject? infoContainer = null;
            if (topRow != null)
            {
                infoContainer = ModernUIFactory.CreateVerticalLayout(topRow.transform, 2);
            }
            var infoLayout = infoContainer?.AddComponent<LayoutElement>();
            if (infoLayout != null)
            {
                infoLayout.flexibleWidth = 1;
            }
            
            Text? titleText = null;
            if (infoContainer != null)
            {
                titleText = ModernUIFactory.CreateModernText(infoContainer.transform, download.songTitle, 14, TextStyle.Primary);
            }
            if (titleText != null)
            {
                titleText.fontStyle = FontStyle.Bold;
            }
            var titleLayout = titleText?.gameObject?.AddComponent<LayoutElement>();
            if (titleLayout != null)
            {
                titleLayout.preferredHeight = 18;
            }
            
            Text? statusText = null;
            if (infoContainer != null)
            {
                statusText = ModernUIFactory.CreateModernText(infoContainer.transform, download.GetStatusText(), 11, TextStyle.Secondary);
            }
            var statusLayout = statusText?.gameObject?.AddComponent<LayoutElement>();
            if (statusLayout != null)
            {
                statusLayout.preferredHeight = 15;
            }
            
            // Action button
            if (download.status == DownloadStatus.Downloading || download.status == DownloadStatus.Pending)
            {
                if (topRow != null)
                {
                    var cancelButton = ModernUIFactory.CreateIconButton(topRow.transform, "✕", 
                        S1Factory.ConvertToUnityAction(() => OnCancelDownload(download)), new Vector2(30, 30));
                    var cancelLayout = cancelButton?.gameObject?.AddComponent<LayoutElement>();
                    if (cancelLayout != null)
                    {
                        cancelLayout.preferredWidth = 30;
                        cancelLayout.preferredHeight = 30;
                    }
                }
            }
            else if (download.status == DownloadStatus.Failed)
            {
                if (topRow != null)
                {
                    var retryButton = ModernUIFactory.CreateIconButton(topRow.transform, "↻", 
                        S1Factory.ConvertToUnityAction(() => OnRetryDownload(download)), new Vector2(30, 30));
                    var retryLayout = retryButton?.gameObject?.AddComponent<LayoutElement>();
                    if (retryLayout != null)
                    {
                        retryLayout.preferredWidth = 30;
                        retryLayout.preferredHeight = 30;
                    }
                }
            }
            
            // Progress bar (only for active downloads)
            if (download.status == DownloadStatus.Downloading || download.status == DownloadStatus.Pending)
            {
                if (downloadItem != null)
                {
                    var progressBar = ModernUIFactory.CreateModernSlider(downloadItem.transform, 0f, 1f, download.progress);
                    if (progressBar != null)
                    {
                        progressBar.interactable = false;
                        var progressLayout = progressBar.gameObject?.AddComponent<LayoutElement>();
                        if (progressLayout != null)
                        {
                            progressLayout.preferredHeight = 20;
                        }
                    }
                }
            }
            
            // Download item layout
            var downloadLayout = downloadItem?.AddComponent<LayoutElement>();
            if (downloadLayout != null)
            {
                downloadLayout.preferredHeight = 90;
            }
            
            if (downloadItem != null)
            {
                downloadItems?.Add(downloadItem);
            }
        }
        
        private string GetStatusIcon(DownloadStatus status)
        {
            switch (status)
            {
                case DownloadStatus.Pending: return "⏳";
                case DownloadStatus.Downloading: return "📥";
                case DownloadStatus.Completed: return "✅";
                case DownloadStatus.Failed: return "❌";
                case DownloadStatus.Cancelled: return "🚫";
                default: return "📁";
            }
        }
        
        private void ClearDownloadList()
        {
            if (downloadItems != null)
            {
                foreach (var item in downloadItems)
                {
                    if (item != null)
                    {
                        UnityEngine.Object.Destroy(item);
                    }
                }
                downloadItems.Clear();
            }
        }
        
        private void UpdateDownloadStats(string? customMessage = null)
        {
            if (downloadStatsText == null) return;
            
            if (!string.IsNullOrEmpty(customMessage))
            {
                downloadStatsText.text = customMessage;
                return;
            }
            
            var active = currentDownloads?.FindAll(d => d.status == DownloadStatus.Downloading || d.status == DownloadStatus.Pending)?.Count ?? 0;
            var completed = currentDownloads?.FindAll(d => d.status == DownloadStatus.Completed)?.Count ?? 0;
            var failed = currentDownloads?.FindAll(d => d.status == DownloadStatus.Failed)?.Count ?? 0;
            
            downloadStatsText.text = $"{active} active • {completed} completed • {failed} failed";
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnBackClick()
        {
            NewLoggingSystem.Info("Back button clicked", "DownloadsScreen");
            navigationManager?.NavigateBack();
        }
        
        private void OnClearCompletedClick()
        {
            NewLoggingSystem.Info("Clear completed button clicked", "DownloadsScreen");
            musicBackend?.ClearCompletedDownloads();
            RefreshDownloads();
        }
        
        private void OnCancelDownload(DownloadInfo download)
        {
            NewLoggingSystem.Info($"Cancel download: {download.songTitle}", "DownloadsScreen");
            musicBackend?.CancelDownload(download.downloadId);
            RefreshDownloads();
        }
        
        private void OnRetryDownload(DownloadInfo download)
        {
            NewLoggingSystem.Info($"Retry download: {download.songTitle}", "DownloadsScreen");
            // Create new song details and restart download
            var song = new NewSongDetails 
            { 
                title = download.songTitle, 
                url = download.songUrl,
                source = "youtube" 
            };
            musicBackend?.DownloadSong(song);
            RefreshDownloads();
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Update()
        {
            // Auto-refresh downloads every 2 seconds when screen is visible
            if (isInitialized && mainContainer?.activeInHierarchy == true && !isRefreshing)
            {
                if (Time.time % 2f < Time.deltaTime)
                {
                    RefreshDownloads();
                }
            }
        }
        
        public void OnDestroy()
        {
            ClearDownloadList();
        }
        
        #endregion
    }
} 