using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.NewBackend;
using BackSpeakerMod.S1Wrapper;

namespace BackSpeakerMod.NewFrontend.UI.Screens
{
    /// <summary>
    /// Navigation Manager for handling screen transitions and hamburger menu
    /// </summary>
    public class NavigationManager : MonoBehaviour
    {
        private BackSpeakerMainManager? mainManager;
        
        // Navigation UI
        private GameObject? navigationMenu;
        private GameObject? menuOverlay;
        private CanvasGroup? menuCanvasGroup;
        private CanvasGroup? overlayCanvasGroup;
        private bool isMenuOpen = false;
        private bool isAnimating = false;
        
        // Animation settings
        private const float ANIMATION_DURATION = 0.3f;
        private const float MENU_WIDTH = 300f;
        
        // Screens
        private Dictionary<string, GameObject>? screens;
        private string currentScreen = "main";
        private string previousScreen = "main";
        
        // Screen objects
        private MainPlayerScreen? mainPlayerScreen;
        private JukeboxScreen? jukeboxScreen;
        private PlaylistEditScreen? playlistEditScreen;
        private DownloadsScreen? downloadsScreen;
        
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// Initialize the navigation manager
        /// </summary>
        public void Initialize(BackSpeakerMainManager manager)
        {
            try
            {
                mainManager = manager;
                NewLoggingSystem.Info("Initializing NavigationManager", "NavigationManager");
                
                screens = new Dictionary<string, GameObject>();
                CreateNavigationMenu();
                
                // Get reference to main player screen
                mainPlayerScreen = GetComponentInParent<MainPlayerScreen>();
                
                IsInitialized = true;
                NewLoggingSystem.Info("NavigationManager initialized successfully", "NavigationManager");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to initialize NavigationManager: {ex}", "NavigationManager");
            }
        }
        
        private void CreateNavigationMenu()
        {
            // Menu overlay (dark background with blur effect) - FULL SCREEN
            menuOverlay = new GameObject("MenuOverlay");
            menuOverlay.transform.SetParent(this.transform, false);
            
            var overlayRect = menuOverlay.AddComponent<RectTransform>();
            // Ensure overlay covers ENTIRE screen
            overlayRect.anchorMin = Vector2.zero;    // Bottom-left corner
            overlayRect.anchorMax = Vector2.one;     // Top-right corner
            overlayRect.offsetMin = Vector2.zero;    // No offset from anchors
            overlayRect.offsetMax = Vector2.zero;    // No offset from anchors
            overlayRect.anchoredPosition = Vector2.zero; // Center position
            
            overlayCanvasGroup = menuOverlay.AddComponent<CanvasGroup>();
            overlayCanvasGroup.alpha = 0f;
            overlayCanvasGroup.interactable = false;
            overlayCanvasGroup.blocksRaycasts = false;
            
            var overlayImage = menuOverlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0.7f); // Darker background for better contrast
            
            var overlayButton = menuOverlay.AddComponent<Button>();
            overlayButton.targetGraphic = overlayImage;
            overlayButton.onClick.AddListener(S1Factory.ConvertToUnityAction(HideNavigationMenu));
            
            // Navigation menu panel - positioned off-screen initially (LEFT SIDE)
            navigationMenu = ModernUIFactory.CreateCard(menuOverlay.transform, new Vector2(MENU_WIDTH, 0), true);
            
            var menuRect = navigationMenu.GetComponent<RectTransform>();
            // Anchor to the LEFT side of screen, full height
            menuRect.anchorMin = new Vector2(0, 0);  // Bottom-left anchor
            menuRect.anchorMax = new Vector2(0, 1);  // Top-left anchor  
            menuRect.pivot = new Vector2(0, 0.5f);   // Pivot at left center
            menuRect.offsetMin = Vector2.zero;       // Clear any offset
            menuRect.offsetMax = Vector2.zero;       // Clear any offset
            menuRect.anchoredPosition = new Vector2(-MENU_WIDTH, 0); // Start off-screen to the LEFT
            menuRect.sizeDelta = new Vector2(MENU_WIDTH, 0); // Width only, height auto from anchors
            
            menuCanvasGroup = navigationMenu.AddComponent<CanvasGroup>();
            menuCanvasGroup.alpha = 0f;
            
            // Menu layout with better spacing
            var menuLayout = navigationMenu.AddComponent<VerticalLayoutGroup>();
            menuLayout.spacing = 8;
            menuLayout.padding = new RectOffset(25, 25, 40, 30);
            menuLayout.childControlHeight = false;
            menuLayout.childControlWidth = true;
            menuLayout.childForceExpandWidth = true;
            
            CreateMenuItems();
            
            // Initially hide the menu
            menuOverlay.SetActive(false);
        }
        
        private void CreateMenuItems()
        {
            // Close button and title bar
            if (navigationMenu == null) return;
            var headerContainer = ModernUIFactory.CreateHorizontalLayout(navigationMenu.transform, 15, new RectOffset(0, 0, 0, 0));
            if (headerContainer == null) return;
            var headerLayout = headerContainer.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 50;
            
            // Close button (X)
            var closeButton = ModernUIFactory.CreateIconButton(headerContainer.transform, "×", S1Factory.ConvertToUnityAction(HideNavigationMenu), new Vector2(40, 40), true);
            var closeButtonImage = closeButton?.GetComponent<Image>();
            if (closeButtonImage != null)
            {
                closeButtonImage.color = ModernUIFactory.Colors.Surface;
            }
            var closeButtonText = closeButton?.GetComponentInChildren<Text>();
            if (closeButtonText != null)
            {
                closeButtonText.color = ModernUIFactory.Colors.TextMuted;
                closeButtonText.fontSize = 18;
            }
            var closeLayout = closeButton?.gameObject?.AddComponent<LayoutElement>();
            if (closeLayout != null)
            {
                closeLayout.preferredWidth = 40;
                closeLayout.preferredHeight = 40;
            }
            
            // Title
            Text? titleText = null;
            if (headerContainer != null)
            {
                titleText = ModernUIFactory.CreateModernText(headerContainer.transform, "BackSpeaker", 22, TextStyle.Primary, TextAnchor.MiddleLeft);
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
            
            // Separator
            CreateMenuSeparator();
            
            // Music Sources Section
            Text? sourcesLabel = null;
            if (navigationMenu != null)
            {
                sourcesLabel = ModernUIFactory.CreateModernText(navigationMenu.transform, "MUSIC SOURCES", 13, TextStyle.Muted, TextAnchor.MiddleLeft);
            }
            if (sourcesLabel != null)
            {
                sourcesLabel.fontStyle = FontStyle.Bold;
                var sourcesLayout = sourcesLabel.gameObject?.AddComponent<LayoutElement>();
                if (sourcesLayout != null)
                {
                    sourcesLayout.preferredHeight = 30;
                }
            }
            
            CreateMenuButton("♫ Jukebox", () => NavigateToScreen("jukebox"));
            CreateMenuButton("F Local Music", () => NavigateToScreen("local"));
            CreateMenuButton("Y YouTube", () => NavigateToScreen("youtube"));
            
            CreateMenuSeparator();
            
            // Playlists Section
            Text? playlistsLabel = null;
            if (navigationMenu != null)
            {
                playlistsLabel = ModernUIFactory.CreateModernText(navigationMenu.transform, "PLAYLISTS", 13, TextStyle.Muted, TextAnchor.MiddleLeft);
            }
            if (playlistsLabel != null)
            {
                playlistsLabel.fontStyle = FontStyle.Bold;
                var playlistsLayout = playlistsLabel.gameObject?.AddComponent<LayoutElement>();
                if (playlistsLayout != null)
                {
                    playlistsLayout.preferredHeight = 30;
                }
            }
            
            CreateMenuButton("P My Playlists", () => NavigateToScreen("playlists"));
            CreateMenuButton("G Global Mix", () => NavigateToScreen("global"));
            
            CreateMenuSeparator();
            
            // Downloads Section
            Text? downloadsLabel = null;
            if (navigationMenu != null)
            {
                downloadsLabel = ModernUIFactory.CreateModernText(navigationMenu.transform, "DOWNLOADS", 13, TextStyle.Muted, TextAnchor.MiddleLeft);
            }
            if (downloadsLabel != null)
            {
                downloadsLabel.fontStyle = FontStyle.Bold;
                var downloadsLayout = downloadsLabel.gameObject?.AddComponent<LayoutElement>();
                if (downloadsLayout != null)
                {
                    downloadsLayout.preferredHeight = 30;
                }
            }
            
            CreateMenuButton("↓ Downloads", () => NavigateToScreen("downloads"));
            
            CreateMenuSeparator();
            
            // Settings Section
            CreateMenuButton("S Settings", () => NavigateToScreen("settings"));
            CreateMenuButton("? About", () => NavigateToScreen("about"));
        }
        
        private void CreateMenuButton(string text, System.Action onClick)
        {
            if (navigationMenu == null) return;
            var button = ModernUIFactory.CreateModernButton(navigationMenu.transform, text, S1Factory.ConvertToUnityAction(() => {
                onClick?.Invoke();
                HideNavigationMenu();
            }), ButtonStyle.Secondary, new Vector2(0, 50));
            if (button == null) return;
            
            // Style the button for menu
            var buttonImage = button?.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.2f, 0.2f, 0.25f, 0.3f); // Subtle background
            }
            
            var buttonLayout = button?.gameObject?.AddComponent<LayoutElement>();
            if (buttonLayout != null)
            {
                buttonLayout.preferredHeight = 50;
            }
            
            // Left-align text with padding
            var textComponent = button?.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.alignment = TextAnchor.MiddleLeft;
                textComponent.fontSize = 16;
                var textRect = textComponent.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    textRect.offsetMin = new Vector2(20, 0);
                    textRect.offsetMax = new Vector2(-20, 0);
                }
            }
            
            // Add hover effect
            if (button != null)
            {
                var buttonTransition = button.transition;
                button.transition = Selectable.Transition.ColorTint;
                var colors = button.colors;
                colors.normalColor = new Color(0.2f, 0.2f, 0.25f, 0.3f);
                colors.highlightedColor = new Color(0.1f, 0.8f, 0.4f, 0.2f);
                colors.pressedColor = new Color(0.1f, 0.8f, 0.4f, 0.4f);
                button.colors = colors;
            }
        }
        
        private void CreateMenuSeparator()
        {
            var separator = new GameObject("Separator");
            separator.transform.SetParent(navigationMenu!.transform, false);
            
            var separatorRect = separator.AddComponent<RectTransform>();
            separatorRect.sizeDelta = new Vector2(0, 1);
            
            var separatorImage = separator.AddComponent<Image>();
            separatorImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            
            var separatorLayout = separator.AddComponent<LayoutElement>();
            separatorLayout.preferredHeight = 15;
        }
        
        public void ShowNavigationMenu()
        {
            if (isAnimating) return;
            
            if (menuOverlay != null)
            {
                menuOverlay.SetActive(true);
                isMenuOpen = true;
                
                // Start slide-in animation
                MelonCoroutines.Start(AnimateMenuIn());
                
                NewLoggingSystem.Info("Navigation menu opened", "NavigationManager");
            }
        }
        
        public void HideNavigationMenu()
        {
            if (isAnimating) return;
            
            if (menuOverlay != null && isMenuOpen)
            {
                isMenuOpen = false;
                
                // Start slide-out animation
                MelonCoroutines.Start(AnimateMenuOut());
                
                NewLoggingSystem.Info("Navigation menu closed", "NavigationManager");
            }
        }
        
        private IEnumerator AnimateMenuIn()
        {
            isAnimating = true;
            
            if (navigationMenu == null) yield break;
            
            var menuRect = navigationMenu.GetComponent<RectTransform>();
            if (menuRect == null) yield break;
            
            var startPos = new Vector2(-MENU_WIDTH, 0); // Off-screen LEFT
            var endPos = Vector2.zero;                  // On-screen LEFT edge
            
            float elapsed = 0f;
            
            while (elapsed < ANIMATION_DURATION)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / ANIMATION_DURATION;
                
                // Smooth ease-out curve
                t = 1f - Mathf.Pow(1f - t, 3f);
                
                // Animate position
                menuRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                
                // Animate alpha
                if (overlayCanvasGroup != null)
                    overlayCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                if (menuCanvasGroup != null)
                    menuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                
                yield return null;
            }
            
            // Ensure final position
            menuRect.anchoredPosition = endPos;
            if (overlayCanvasGroup != null)
            {
                overlayCanvasGroup.alpha = 1f;
                overlayCanvasGroup.interactable = true;
                overlayCanvasGroup.blocksRaycasts = true;
            }
            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = 1f;
            }
            
            isAnimating = false;
        }
        
        private IEnumerator AnimateMenuOut()
        {
            isAnimating = true;
            
            if (navigationMenu == null) yield break;
            
            var menuRect = navigationMenu.GetComponent<RectTransform>();
            if (menuRect == null) yield break;
            
            var startPos = Vector2.zero;                  // On-screen LEFT edge
            var endPos = new Vector2(-MENU_WIDTH, 0);     // Off-screen LEFT
            
            float elapsed = 0f;
            
            // Disable interactions
            if (overlayCanvasGroup != null)
            {
                overlayCanvasGroup.interactable = false;
                overlayCanvasGroup.blocksRaycasts = false;
            }
            
            while (elapsed < ANIMATION_DURATION)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / ANIMATION_DURATION;
                
                // Smooth ease-in curve
                t = Mathf.Pow(t, 3f);
                
                // Animate position
                menuRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                
                // Animate alpha
                if (overlayCanvasGroup != null)
                    overlayCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                if (menuCanvasGroup != null)
                    menuCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                
                yield return null;
            }
            
            // Ensure final position
            menuRect.anchoredPosition = endPos;
            if (overlayCanvasGroup != null)
            {
                overlayCanvasGroup.alpha = 0f;
            }
            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = 0f;
            }
            
            // Hide the menu
            if (menuOverlay != null)
            {
                menuOverlay.SetActive(false);
            }
            
            isAnimating = false;
        }
        
        private void NavigateToScreen(string screenName)
        {
            NewLoggingSystem.Info($"Navigating to screen: {screenName}", "NavigationManager");
            
            // Store previous screen for back navigation
            previousScreen = currentScreen;
            currentScreen = screenName;
            
            // Hide main player screen when navigating to sub-screens
            if (mainPlayerScreen != null)
            {
                mainPlayerScreen.gameObject.SetActive(false);
            }
            
            switch (screenName)
            {
                case "jukebox":
                    ShowJukeboxScreen();
                    break;
                case "local":
                    ShowLocalMusicScreen();
                    break;
                case "youtube":
                    ShowYouTubeScreen();
                    break;
                case "playlists":
                    ShowPlaylistScreen();
                    break;
                case "global":
                    ShowGlobalPlaylistScreen();
                    break;
                case "downloads":
                    ShowDownloadsScreen();
                    break;
                case "playlist-edit":
                    ShowPlaylistEditScreen();
                    break;
                case "settings":
                    ShowSettingsScreen();
                    break;
                case "about":
                    ShowAboutScreen();
                    break;
                default:
                    NewLoggingSystem.Warning($"Unknown screen: {screenName}", "NavigationManager");
                    break;
            }
        }
        
        /// <summary>
        /// Navigate back to the previous screen or main screen
        /// </summary>
        public void NavigateBack()
        {
            NewLoggingSystem.Info($"Navigating back from {currentScreen} to {previousScreen}", "NavigationManager");
            
            if (previousScreen == "main" || currentScreen == "main")
            {
                ShowMainScreen();
            }
            else
            {
                NavigateToScreen(previousScreen);
            }
        }
        
        /// <summary>
        /// Show the main player screen and hide all others
        /// </summary>
        public void ShowMainScreen()
        {
            NewLoggingSystem.Info("Showing main player screen", "NavigationManager");
            
            previousScreen = currentScreen;
            currentScreen = "main";
            
            // Hide all sub-screens
            HideAllScreens();
            
            // Show main player screen
            if (mainPlayerScreen != null)
            {
                mainPlayerScreen.gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// Hide all sub-screens
        /// </summary>
        private void HideAllScreens()
        {
            if (screens != null)
            {
                foreach (var screen in screens.Values)
                {
                    if (screen != null)
                    {
                        screen.SetActive(false);
                    }
                }
            }
        }
        
        private void ShowJukeboxScreen()
        {
            // Create and show the jukebox screen
            if (!screens!.ContainsKey("jukebox"))
            {
                var jukeboxScreenObj = new GameObject("JukeboxScreen");
                jukeboxScreenObj.transform.SetParent(this.transform.parent, false);
                
                var jukeboxRect = jukeboxScreenObj.AddComponent<RectTransform>();
                jukeboxRect.anchorMin = Vector2.zero;
                jukeboxRect.anchorMax = Vector2.one;
                jukeboxRect.offsetMin = Vector2.zero;
                jukeboxRect.offsetMax = Vector2.zero;
                
                jukeboxScreen = S1Factory.RegisterAndAddComponent<JukeboxScreen>(jukeboxScreenObj);
                if (jukeboxScreen != null && mainManager != null)
                {
                    jukeboxScreen.Initialize(mainManager, this);
                }
                
                screens["jukebox"] = jukeboxScreenObj;
                NewLoggingSystem.Info("Created Jukebox screen", "NavigationManager");
            }
            else
            {
                screens["jukebox"]!.SetActive(true);
                NewLoggingSystem.Info("Showing existing Jukebox screen", "NavigationManager");
            }
        }
        
        private void ShowLocalMusicScreen()
        {
            // Create and show the local music screen
            if (!screens!.ContainsKey("localmusic"))
            {
                var localMusicScreenObj = new GameObject("LocalMusicScreen");
                localMusicScreenObj.transform.SetParent(this.transform.parent, false);
                
                var localMusicRect = localMusicScreenObj.AddComponent<RectTransform>();
                localMusicRect.anchorMin = Vector2.zero;
                localMusicRect.anchorMax = Vector2.one;
                localMusicRect.offsetMin = Vector2.zero;
                localMusicRect.offsetMax = Vector2.zero;
                
                var localMusicScreen = S1Factory.RegisterAndAddComponent<LocalMusicScreen>(localMusicScreenObj);
                if (localMusicScreen != null && mainManager != null)
                {
                    localMusicScreen.Initialize(mainManager, this);
                }
                
                screens["localmusic"] = localMusicScreenObj;
                NewLoggingSystem.Info("Created Local Music screen", "NavigationManager");
            }
            else
            {
                screens["localmusic"]!.SetActive(true);
                NewLoggingSystem.Info("Showing existing Local Music screen", "NavigationManager");
            }
        }
        
        private void ShowYouTubeScreen()
        {
            // Create and show the YouTube screen
            if (!screens!.ContainsKey("youtube"))
            {
                var youTubeScreenObj = new GameObject("YouTubeScreen");
                youTubeScreenObj.transform.SetParent(this.transform.parent, false);
                
                var youTubeRect = youTubeScreenObj.AddComponent<RectTransform>();
                youTubeRect.anchorMin = Vector2.zero;
                youTubeRect.anchorMax = Vector2.one;
                youTubeRect.offsetMin = Vector2.zero;
                youTubeRect.offsetMax = Vector2.zero;
                
                var youTubeScreen = S1Factory.RegisterAndAddComponent<YouTubeScreen>(youTubeScreenObj);
                if (youTubeScreen != null && mainManager != null)
                {
                    youTubeScreen.Initialize(mainManager, this);
                }
                
                screens["youtube"] = youTubeScreenObj;
                NewLoggingSystem.Info("Created YouTube screen", "NavigationManager");
            }
            else
            {
                screens["youtube"]!.SetActive(true);
                NewLoggingSystem.Info("Showing existing YouTube screen", "NavigationManager");
            }
        }
        
        private void ShowPlaylistScreen()
        {
            // Create and show the playlist screen
            if (!screens!.ContainsKey("playlists"))
            {
                var playlistScreenObj = new GameObject("PlaylistScreen");
                playlistScreenObj.transform.SetParent(this.transform.parent, false);
                
                var playlistRect = playlistScreenObj.AddComponent<RectTransform>();
                playlistRect.anchorMin = Vector2.zero;
                playlistRect.anchorMax = Vector2.one;
                playlistRect.offsetMin = Vector2.zero;
                playlistRect.offsetMax = Vector2.zero;
                
                var playlistScreen = S1Factory.RegisterAndAddComponent<PlaylistScreen>(playlistScreenObj);
                if (playlistScreen != null && mainManager != null)
                {
                    playlistScreen.Initialize(mainManager, this);
                }
                
                screens["playlists"] = playlistScreenObj;
                NewLoggingSystem.Info("Created Playlist screen", "NavigationManager");
            }
            else
            {
                screens["playlists"]!.SetActive(true);
                NewLoggingSystem.Info("Showing existing Playlist screen", "NavigationManager");
            }
        }
        
        private void ShowGlobalPlaylistScreen()
        {
            ShowComingSoonScreen("Global Mix", "global");
        }
        
        private void ShowDownloadsScreen()
        {
            // Create and show the downloads screen
            if (!screens!.ContainsKey("downloads"))
            {
                var downloadsScreenObj = new GameObject("DownloadsScreen");
                downloadsScreenObj.transform.SetParent(this.transform.parent, false);
                
                var downloadsRect = downloadsScreenObj.AddComponent<RectTransform>();
                downloadsRect.anchorMin = Vector2.zero;
                downloadsRect.anchorMax = Vector2.one;
                downloadsRect.offsetMin = Vector2.zero;
                downloadsRect.offsetMax = Vector2.zero;
                
                downloadsScreen = S1Factory.RegisterAndAddComponent<DownloadsScreen>(downloadsScreenObj);
                if (downloadsScreen != null && mainManager != null)
                {
                    downloadsScreen.Initialize(mainManager, this);
                }
                downloadsScreen?.Show(); // Call Show() to make content visible
                
                screens["downloads"] = downloadsScreenObj;
                NewLoggingSystem.Info("Created Downloads screen", "NavigationManager");
            }
            else
            {
                screens["downloads"]!.SetActive(true);
                if (downloadsScreen != null)
                {
                    downloadsScreen.Show(); // Call Show() on existing screen too
                }
                NewLoggingSystem.Info("Showing existing Downloads screen", "NavigationManager");
            }
        }
        
        private void ShowPlaylistEditScreen()
        {
            // Create and show the playlist edit screen
            if (!screens!.ContainsKey("playlist-edit"))
            {
                var playlistEditScreenObj = new GameObject("PlaylistEditScreen");
                playlistEditScreenObj.transform.SetParent(this.transform.parent, false);
                
                var playlistEditRect = playlistEditScreenObj.AddComponent<RectTransform>();
                playlistEditRect.anchorMin = Vector2.zero;
                playlistEditRect.anchorMax = Vector2.one;
                playlistEditRect.offsetMin = Vector2.zero;
                playlistEditRect.offsetMax = Vector2.zero;
                
                playlistEditScreen = S1Factory.RegisterAndAddComponent<PlaylistEditScreen>(playlistEditScreenObj);
                if (playlistEditScreen != null && mainManager?.GetMusicBackend() != null)
                {
                    playlistEditScreen.Initialize(mainManager, this, mainManager.GetMusicBackend()!);
                }
                
                screens["playlist-edit"] = playlistEditScreenObj;
                NewLoggingSystem.Info("Created Playlist Edit screen", "NavigationManager");
            }
            else
            {
                screens["playlist-edit"]!.SetActive(true);
                NewLoggingSystem.Info("Showing existing Playlist Edit screen", "NavigationManager");
            }
        }
        
        /// <summary>
        /// Show playlist edit screen for specific playlist
        /// </summary>
        public void ShowPlaylistEditScreen(NewYouTubePlaylistInfo playlist)
        {
            ShowPlaylistEditScreen();
            if (playlistEditScreen != null && playlist != null)
            {
                playlistEditScreen.SetPlaylist(playlist);
            }
        }
        
        private void ShowSettingsScreen()
        {
            ShowComingSoonScreen("Settings", "settings");
        }
        
        private void ShowAboutScreen()
        {
            ShowComingSoonScreen("About", "about");
        }
        
        /// <summary>
        /// Show a temporary "Coming Soon" screen with back button
        /// </summary>
        private void ShowComingSoonScreen(string screenTitle, string screenKey)
        {
            NewLoggingSystem.Info($"{screenTitle} screen - Coming Soon!", "NavigationManager");
            
            if (screens != null && !screens.ContainsKey(screenKey))
            {
                var screenObj = new GameObject($"{screenTitle}Screen");
                screenObj.transform.SetParent(this.transform.parent, false);
                
                var screenRect = screenObj.AddComponent<RectTransform>();
                screenRect.anchorMin = Vector2.zero;
                screenRect.anchorMax = Vector2.one;
                screenRect.offsetMin = Vector2.zero;
                screenRect.offsetMax = Vector2.zero;
                
                var screenImage = screenObj.AddComponent<Image>();
                screenImage.color = ModernUIFactory.Colors.Background;
                
                // Vertical layout
                var layout = screenObj.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 20;
                layout.padding = new RectOffset(30, 30, 50, 50);
                layout.childControlHeight = false;
                layout.childControlWidth = true;
                layout.childAlignment = TextAnchor.UpperCenter;
                
                // Header with back button
                var header = ModernUIFactory.CreateHorizontalLayout(screenObj.transform, 15, new RectOffset(0, 0, 0, 0));
                var headerLayout = header.AddComponent<LayoutElement>();
                headerLayout.preferredHeight = 60;
                
                var backBtn = ModernUIFactory.CreateIconButton(header.transform, "←", S1Factory.ConvertToUnityAction(NavigateBack), new Vector2(50, 50), false);
                var backLayout = backBtn.gameObject.AddComponent<LayoutElement>();
                backLayout.preferredWidth = 50;
                backLayout.preferredHeight = 50;
                
                var title = ModernUIFactory.CreateModernText(header.transform, screenTitle, 24, TextStyle.Primary, TextAnchor.MiddleLeft);
                if (title != null)
                {
                    title.fontStyle = FontStyle.Bold;
                    var titleLayout = title.gameObject.AddComponent<LayoutElement>();
                    titleLayout.flexibleWidth = 1;
                }
                
                // Coming soon message
                var comingSoon = ModernUIFactory.CreateModernText(screenObj.transform, "Coming Soon!", 32, TextStyle.Primary, TextAnchor.MiddleCenter);
                comingSoon.fontStyle = FontStyle.Bold;
                var messageLayout = comingSoon.gameObject.AddComponent<LayoutElement>();
                messageLayout.preferredHeight = 100;
                messageLayout.flexibleHeight = 1;
                
                var description = ModernUIFactory.CreateModernText(screenObj.transform, $"The {screenTitle} feature is currently under development.", 16, TextStyle.Secondary, TextAnchor.MiddleCenter);
                var descLayout = description.gameObject.AddComponent<LayoutElement>();
                descLayout.preferredHeight = 40;
                
                screens[screenKey] = screenObj;
                NewLoggingSystem.Info($"Created {screenTitle} coming soon screen", "NavigationManager");
            }
            else if (screens != null)
            {
                screens[screenKey].SetActive(true);
                NewLoggingSystem.Info($"Showing existing {screenTitle} screen", "NavigationManager");
            }
        }
        
        public void Update()
        {
            // Handle back button or escape key
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            {
                if (isMenuOpen)
                {
                    HideNavigationMenu();
                }
                else if (currentScreen != "main")
                {
                    NavigateBack();
                }
            }
        }
    }
} 