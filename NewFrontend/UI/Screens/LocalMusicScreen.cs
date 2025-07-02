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
    /// Local Music screen for browsing and playing local music files
    /// Features: File browser, folder navigation, track loading, play/add functionality
    /// </summary>
    public class LocalMusicScreen : MonoBehaviour
    {
        #region Private Fields
        
        private BackSpeakerMainManager mainManager;
        private NavigationManager navigationManager;
        private IMusicBackend musicBackend;
        
        // UI Elements
        private GameObject mainContainer;
        private GameObject headerSection;
        private GameObject fileListSection;
        private ScrollRect fileScrollView;
        private GameObject fileListContent;
        
        // Header components
        private Button backButton;
        private Text titleText;
        private Button refreshButton;
        private Text pathText;
        
        // File management
        private List<GameObject> fileItems;
        private List<LocalFileInfo> availableFiles;
        private string currentPath = "";
        
        // State
        private bool isInitialized = false;
        private bool isLoading = false;
        
        #endregion
        
        #region Public Properties
        
        public bool IsInitialized => isInitialized;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the local music screen
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
                
                NewLoggingSystem.Info("Initializing LocalMusicScreen", "LocalMusicScreen");
                
                fileItems = new List<GameObject>();
                availableFiles = new List<LocalFileInfo>();
                
                CreateMainLayout();
                CreateHeader();
                CreateFileList();
                
                LoadLocalFiles();
                
                isInitialized = true;
                NewLoggingSystem.Info("LocalMusicScreen initialized successfully", "LocalMusicScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to initialize LocalMusicScreen: {ex}", "LocalMusicScreen");
            }
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateMainLayout()
        {
            // Main container
            mainContainer = new GameObject("LocalMusicMainContainer");
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
            
            titleText = ModernUIFactory.CreateModernText(titleContainer.transform, "F Local Music", 18, TextStyle.Primary);
            titleText.fontStyle = FontStyle.Bold;
            var titleTextLayout = titleText.gameObject.AddComponent<LayoutElement>();
            titleTextLayout.preferredHeight = 25;
            
            pathText = ModernUIFactory.CreateModernText(titleContainer.transform, "Loading...", 12, TextStyle.Secondary);
            var pathLayout = pathText.gameObject.AddComponent<LayoutElement>();
            pathLayout.preferredHeight = 20;
            
            // Refresh button
            refreshButton = ModernUIFactory.CreateIconButton(headerSection.transform, "R", S1Factory.ConvertToUnityAction(OnRefreshClick), new Vector2(40, 40), false);
            var refreshLayout = refreshButton.gameObject.AddComponent<LayoutElement>();
            refreshLayout.preferredWidth = 40;
            refreshLayout.preferredHeight = 40;
            
            // Header layout element
            var headerLayoutElement = headerSection.AddComponent<LayoutElement>();
            headerLayoutElement.preferredHeight = 80;
            headerLayoutElement.flexibleHeight = 0;  // Don't expand
        }
        
        private void CreateFileList()
        {
            fileListSection = new GameObject("FileList");
            fileListSection.transform.SetParent(mainContainer.transform, false);
            
            // Simple background
            var scrollImage = fileListSection.AddComponent<Image>();
            scrollImage.color = ModernUIFactory.Colors.Background;
            
            // SIMPLIFIED: Direct content container, no complex viewport masking
            fileListContent = new GameObject("Content");
            fileListContent.transform.SetParent(fileListSection.transform, false);
            
            var contentRect = fileListContent.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Simple content layout
            var contentLayout = fileListContent.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 8;
            contentLayout.padding = new RectOffset(15, 15, 15, 15);
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            
            // Content size fitter for proper scrolling
            var sizeFitter = fileListContent.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            // Simple scroll setup
            fileScrollView = fileListSection.AddComponent<ScrollRect>();
            fileScrollView.content = contentRect;
            fileScrollView.vertical = true;
            fileScrollView.horizontal = false;
            fileScrollView.scrollSensitivity = 15;
            
            // File list layout element - take remaining space
            var listLayoutElement = fileListSection.AddComponent<LayoutElement>();
            listLayoutElement.flexibleHeight = 1;
        }
        
        #endregion
        
        #region File Management
        
        private void LoadLocalFiles()
        {
            try
            {
                NewLoggingSystem.Info("Loading local music files", "LocalMusicScreen");
                isLoading = true;
                UpdatePath("Loading...");
                
                // Clear existing files
                ClearFileList();
                
                // Get files from backend  
                var localTracks = musicBackend.GetLocalTracks();
                availableFiles = ConvertTracksToFileInfo(localTracks);
                currentPath = "/Music/";
                
                PopulateFileList();
                isLoading = false;
                
                NewLoggingSystem.Info($"Loaded {availableFiles.Count} local files", "LocalMusicScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to load local files: {ex}", "LocalMusicScreen");
                isLoading = false;
                UpdatePath("Error loading files");
            }
        }
        
        private List<LocalFileInfo> ConvertTracksToFileInfo(List<NewSongDetails> tracks)
        {
            var fileInfos = new List<LocalFileInfo>();
            
            foreach (var track in tracks)
            {
                bool isFolder = track.url.StartsWith("folder://");
                var fileInfo = new LocalFileInfo
                {
                    Name = track.title,
                    IsFolder = isFolder,
                    Path = track.url,
                    Size = isFolder ? track.artist : FormatFileSize(track.duration)
                };
                fileInfos.Add(fileInfo);
            }
            
            return fileInfos;
        }
        
        private string FormatFileSize(float durationSeconds)
        {
            // Estimate file size based on duration (rough approximation)
            var estimatedMB = (durationSeconds / 60.0f) * 4.0f; // ~4MB per minute
            return $"{estimatedMB:F1} MB";
        }
        
        private void PopulateFileList()
        {
            UpdatePath(currentPath);
            
            foreach (var file in availableFiles)
            {
                CreateFileItem(file);
            }
        }
        
        private void CreateFileItem(LocalFileInfo fileInfo)
        {
            var fileItem = ModernUIFactory.CreateCard(fileListContent.transform, new Vector2(0, 70), true);
            
            var itemLayout = fileItem.AddComponent<HorizontalLayoutGroup>();
            itemLayout.spacing = 15;
            itemLayout.padding = new RectOffset(15, 15, 10, 10);
            itemLayout.childControlHeight = true;
            itemLayout.childControlWidth = false;
            itemLayout.childForceExpandHeight = false;
            itemLayout.childForceExpandWidth = false;
            
            // File icon
            var iconText = ModernUIFactory.CreateModernText(fileItem.transform, fileInfo.IsFolder ? "D" : "♫", 24, TextStyle.Primary, TextAnchor.MiddleCenter);
            var iconLayout = iconText.gameObject.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = 40;
            iconLayout.preferredHeight = 40;
            
            // File info section
            var infoContainer = ModernUIFactory.CreateVerticalLayout(fileItem.transform, 2, new RectOffset(0, 0, 0, 0));
            var infoLayout = infoContainer.AddComponent<LayoutElement>();
            infoLayout.flexibleWidth = 1;
            
            var nameText = ModernUIFactory.CreateModernText(infoContainer.transform, fileInfo.Name, 16, TextStyle.Primary);
            nameText.fontStyle = FontStyle.Bold;
            var nameLayout = nameText.gameObject.AddComponent<LayoutElement>();
            nameLayout.preferredHeight = 25;
            
            var detailText = ModernUIFactory.CreateModernText(infoContainer.transform, $"{(fileInfo.IsFolder ? "Folder" : "Audio File")} • {fileInfo.Size}", 12, TextStyle.Secondary);
            var detailLayout = detailText.gameObject.AddComponent<LayoutElement>();
            detailLayout.preferredHeight = 20;
            
            // Action buttons
            if (fileInfo.IsFolder)
            {
                // Open folder button
                var openButton = ModernUIFactory.CreateIconButton(fileItem.transform, "→", S1Factory.ConvertToUnityAction(() => OnFolderOpen(fileInfo)), new Vector2(45, 45));
                var openLayout = openButton.gameObject.AddComponent<LayoutElement>();
                openLayout.preferredWidth = 45;
                openLayout.preferredHeight = 45;
            }
            else
            {
                // Play button
                var playButton = ModernUIFactory.CreateIconButton(fileItem.transform, ">", S1Factory.ConvertToUnityAction(() => OnFilePlay(fileInfo)), new Vector2(45, 45));
                var playLayout = playButton.gameObject.AddComponent<LayoutElement>();
                playLayout.preferredWidth = 45;
                playLayout.preferredHeight = 45;
                
                // Add to queue button
                var addButton = ModernUIFactory.CreateIconButton(fileItem.transform, "+", S1Factory.ConvertToUnityAction(() => OnFileAdd(fileInfo)), new Vector2(35, 35));
                var addLayout = addButton.gameObject.AddComponent<LayoutElement>();
                addLayout.preferredWidth = 35;
                addLayout.preferredHeight = 35;
            }
            
            // File item layout element
            var fileLayout = fileItem.AddComponent<LayoutElement>();
            fileLayout.preferredHeight = 70;
            
            fileItems.Add(fileItem);
        }
        
        private void ClearFileList()
        {
            foreach (var item in fileItems)
            {
                if (item != null)
                    DestroyImmediate(item);
            }
            fileItems.Clear();
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnBackClick()
        {
            NewLoggingSystem.Info("Back button clicked", "LocalMusicScreen");
            navigationManager?.NavigateBack();
        }
        
        private void OnRefreshClick()
        {
            NewLoggingSystem.Info("Refresh button clicked", "LocalMusicScreen");
            LoadLocalFiles();
        }
        
        private void OnFolderOpen(LocalFileInfo folderInfo)
        {
            NewLoggingSystem.Info($"Opening folder: {folderInfo.Name}", "LocalMusicScreen");
            // TODO: Implement folder navigation
            currentPath = folderInfo.Path;
            UpdatePath(currentPath + " (Coming Soon)");
        }
        
        private void OnFilePlay(LocalFileInfo fileInfo)
        {
            NewLoggingSystem.Info($"Playing file: {fileInfo.Name}", "LocalMusicScreen");
            // Convert to NewSongDetails for backend
            var songDetails = new NewSongDetails
            {
                title = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name),
                artist = "Local File",
                url = fileInfo.Path,
                duration = 180 // Mock duration
            };
            musicBackend.PlayTrack(songDetails);
        }
        
        private void OnFileAdd(LocalFileInfo fileInfo)
        {
            NewLoggingSystem.Info($"Adding file to queue: {fileInfo.Name}", "LocalMusicScreen");
            // Convert to NewSongDetails for backend
            var songDetails = new NewSongDetails
            {
                title = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name),
                artist = "Local File",
                url = fileInfo.Path,
                duration = 180 // Mock duration
            };
            musicBackend.AddToQueue(songDetails);
        }
        
        #endregion
        
        #region Helper Methods
        
        private void UpdatePath(string path)
        {
            if (pathText != null)
            {
                pathText.text = path;
            }
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        public void OnDestroy()
        {
            ClearFileList();
        }
        
        #endregion
    }
    
    #region Helper Classes
    
    /// <summary>
    /// Information about a local file or folder
    /// </summary>
    [Serializable]
    public class LocalFileInfo
    {
        public string Name;
        public string Path;
        public bool IsFolder;
        public string Size;
    }
    
    #endregion
} 