using UnityEngine;
using BackSpeakerMod.S1Wrapper;
using UnityEngine.Events;

namespace BackSpeakerMod.UIWrapper
{
    /// <summary>
    /// Usage examples for the complete UIWrapper system
    /// Shows how to use the layered approach: S1Factory + S1PhoneAppBuilder + BackSpeakerUIBuilder
    /// </summary>
    public static class UIWrapperUsageExample
    {
        #region Complete App Creation Examples

        /// <summary>
        /// Example 1: Create complete BackSpeaker app with default settings
        /// </summary>
        public static void CreateDefaultBackSpeakerApp()
        {
            // Initialize S1 systems
            S1Factory.Initialize();

            // Create complete app using the UIWrapper system
            var app = BackSpeakerUIBuilder.CreateCompleteBackSpeakerApp("BackSpeakerApp", "Back Speaker");

            if (app != null)
            {
                UnityEngine.Debug.Log("Complete BackSpeaker app created with default settings!");
            }
        }

        /// <summary>
        /// Example 2: Create customized BackSpeaker app
        /// </summary>
        public static void CreateCustomBackSpeakerApp()
        {
            // Initialize S1 systems
            S1Factory.Initialize();

            // Create custom configuration
            var config = new MainScreenConfig
            {
                ShowTrackInfo = true,
                ShowControls = true,
                ShowProgressBar = true,
                ShowTabBar = true,
                
                // Customize track info
                TrackInfoConfig = new TrackInfoComponentConfig
                {
                    LayoutConfig = new TrackInfoConfig { Width = 350, Height = 100, AlbumArtSize = 80 },
                    TitleFontSize = 20,
                    SubtextFontSize = 14,
                    TitleColor = new Color(1f, 1f, 1f, 1f),
                    SubtextColor = new Color(0.9f, 0.9f, 0.9f, 1f)
                },
                
                // Customize controls
                ControlsConfig = new ControlsComponentConfig
                {
                    LayoutConfig = new ControlsConfig { Width = 320, ButtonCount = 5, ButtonSize = 60 },
                    ButtonStyle = ButtonStyle.Primary,
                    OnPlayPause = () => UnityEngine.Debug.Log("Play/Pause clicked"),
                    OnNext = () => UnityEngine.Debug.Log("Next clicked"),
                    OnPrevious = () => UnityEngine.Debug.Log("Previous clicked"),
                    OnStop = () => UnityEngine.Debug.Log("Stop clicked"),
                    OnShuffle = () => UnityEngine.Debug.Log("Shuffle clicked")
                },
                
                // Customize tab bar
                TabBarConfig = new TabBarComponentConfig
                {
                    TabNames = new[] { "Now Playing", "Playlists", "YouTube", "Settings" },
                    OnTabClick = (index) => UnityEngine.Debug.Log($"Tab {index} clicked")
                }
            };

            var app = BackSpeakerUIBuilder.CreateCompleteBackSpeakerApp("CustomBackSpeakerApp", "Custom Back Speaker", config);

            if (app != null)
            {
                UnityEngine.Debug.Log("Custom BackSpeaker app created!");
            }
        }

        /// <summary>
        /// Example 3: Create app with manual screen building
        /// </summary>
        public static void CreateManualBackSpeakerApp(BackSpeakerMod.NewBackend.BackSpeakerMainManager manager)
        {
            // Initialize S1 systems
            S1Factory.Initialize();

            // Create app using S1PhoneAppBuilder
            var app = S1PhoneAppBuilder.CreateBackSpeakerApp("ManualBackSpeakerApp", "Manual Back Speaker", container =>
            {
                // Create main screen using BackSpeakerUIBuilder
                var mainScreen = BackSpeakerUIBuilder.CreateMainScreen(container);

                // Setup real event handlers
                if (mainScreen.Controls != null)
                {
                    // Connect to actual BackSpeaker manager
                    SetupRealControlHandlers(mainScreen.Controls, manager);
                }

                if (mainScreen.TabBar != null)
                {
                    SetupTabBarHandlers(mainScreen.TabBar, container);
                }

                // Store reference for updates
                StoreMainScreenReference(mainScreen);
            });

            if (app != null)
            {
                UnityEngine.Debug.Log("Manual BackSpeaker app created with real handlers!");
            }
        }

        #endregion

        #region Popup Examples

        /// <summary>
        /// Example 4: Show different types of popups
        /// </summary>
        public static void ShowPopupExamples(Transform parent)
        {
            // YouTube add popup
            var youtubePopup = BackSpeakerUIBuilder.ShowPopup(parent, PopupType.YouTubeAdd, new PopupScreenConfig
            {
                PopupConfig = ComponentPresets.YouTubeAddPopup,
                OnPlaylistSelect = () => UnityEngine.Debug.Log("Playlist selection requested")
            });

            // Settings popup
            var settingsPopup = BackSpeakerUIBuilder.ShowPopup(parent, PopupType.Settings, new PopupScreenConfig
            {
                PopupConfig = ComponentPresets.ConfirmationPopup,
                OnAutoPlayToggle = () => UnityEngine.Debug.Log("Auto-play toggled")
            });

            // Error popup
            var errorPopup = BackSpeakerUIBuilder.ShowPopup(parent, PopupType.Error, new PopupScreenConfig
            {
                PopupConfig = ComponentPresets.ErrorPopup
            });

            UnityEngine.Debug.Log("Popup examples created!");
        }

        /// <summary>
        /// Example 5: Custom popup creation
        /// </summary>
        public static void CreateCustomPopup(Transform parent)
        {
            var customPopupConfig = new PopupComponentConfig
            {
                Title = "Custom Action",
                PopupConfig = new PopupConfig
                {
                    Width = 400,
                    Height = 250,
                    BackgroundColor = new Color(0.1f, 0.2f, 0.3f, 0.95f),
                    HasTitleBar = true,
                    HasButtonArea = true
                },
                ButtonConfigs = new[]
                {
                    new PopupButtonConfig { Text = "Option 1", Style = ButtonStyle.Primary, OnClick = () => UnityEngine.Debug.Log("Option 1") },
                    new PopupButtonConfig { Text = "Option 2", Style = ButtonStyle.Success, OnClick = () => UnityEngine.Debug.Log("Option 2") },
                    new PopupButtonConfig { Text = "Cancel", Style = ButtonStyle.Default, OnClick = () => UnityEngine.Debug.Log("Cancelled") }
                },
                OnClose = () => UnityEngine.Debug.Log("Popup closed")
            };

            var popup = ComponentBuilder.CreatePopup(parent, customPopupConfig.Title, customPopupConfig.OnClose);
            popup.Show();

            UnityEngine.Debug.Log("Custom popup created!");
        }

        #endregion

        #region Individual Component Examples

        /// <summary>
        /// Example 6: Create individual components
        /// </summary>
        public static void CreateIndividualComponents(Transform parent)
        {
            // Create track info component
            var trackInfo = ComponentBuilder.CreateTrackInfoComponent(parent, ComponentPresets.DefaultTrackInfo);
            trackInfo.UpdateTrackInfo("Sample Track", "Sample Artist", "Sample Album", "3:45");

            // Create controls component
            var controls = ComponentBuilder.CreateControlsComponent(parent, ComponentPresets.MusicControls);
            controls.SetPlayState(true);

            // Create progress bar
            var progressBar = ComponentBuilder.CreateProgressBarComponent(parent, ComponentPresets.DefaultProgressBar);
            progressBar.UpdateProgress(0.6f, "2:15", "3:45");

            // Create tab bar
            var tabBar = ComponentBuilder.CreateTabBarComponent(parent, ComponentPresets.DefaultTabBar);
            tabBar.SetActiveTab(0);

            UnityEngine.Debug.Log("Individual components created!");
        }

        /// <summary>
        /// Example 7: Layout-only creation (for custom content)
        /// </summary>
        public static void CreateLayoutsOnly(Transform parent)
        {
            // Create main screen layout
            var mainLayout = LayoutFactory.CreateMainScreenLayout(parent, LayoutPresets.BackSpeakerMain);

            // Create popup layout
            var popupLayout = LayoutFactory.CreatePopupLayout(parent, LayoutPresets.StandardPopup);

            // Create list layout
            var listLayout = LayoutFactory.CreateListLayout(parent, LayoutPresets.PlaylistList);

            // Create button group layout
            var buttonLayout = LayoutFactory.CreateHorizontalButtonGroup(parent, LayoutPresets.PlaylistActions);

            UnityEngine.Debug.Log("Layout structures created - ready for custom content!");
        }

        #endregion

        #region Integration with Existing System

        /// <summary>
        /// Example 8: Replace existing BackSpeakerApp creation
        /// </summary>
        public static bool ReplaceExistingBackSpeakerApp(BackSpeakerMod.NewBackend.BackSpeakerMainManager manager)
        {
            try
            {
                // Initialize wrapper systems
                S1Factory.Initialize();

                // Create app using new system
                var app = BackSpeakerUIBuilder.CreateCompleteBackSpeakerApp("BackSpeakerApp", "Back Speaker", new MainScreenConfig
                {
                    ControlsConfig = new ControlsComponentConfig
                    {
                        OnPlayPause = () => TogglePlayback(manager),
                        OnNext = () => { /* Next track functionality not implemented yet */ },
                        OnPrevious = () => { /* Previous track functionality not implemented yet */ },
                        OnStop = () => manager?.Pause(),
                        OnShuffle = () => { /* manager.ToggleShuffle() - method not available */ }
                    },
                    TabBarConfig = new TabBarComponentConfig
                    {
                        OnTabClick = (index) => HandleTabSwitch(index, manager)
                    }
                });

                return app != null;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to create BackSpeaker app: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        private static void SetupRealControlHandlers(ControlsComponent controls, BackSpeakerMod.NewBackend.BackSpeakerMainManager manager)
        {
            // Wire up real music control logic here
            if (controls.PlayPauseButton != null)
            {
                controls.PlayPauseButton.onClick.RemoveAllListeners();
                controls.PlayPauseButton.onClick.AddListener((UnityAction)(() => TogglePlayback(manager)));
            }

            if (controls.NextButton != null)
            {
                controls.NextButton.onClick.RemoveAllListeners();
                controls.NextButton.onClick.AddListener((UnityAction)(() => { /* Next track functionality not implemented yet */ }));
            }

            if (controls.PreviousButton != null)
            {
                controls.PreviousButton.onClick.RemoveAllListeners();
                controls.PreviousButton.onClick.AddListener((UnityAction)(() => { /* Previous track functionality not implemented yet */ }));
            }

            if (controls.StopButton != null)
            {
                controls.StopButton.onClick.RemoveAllListeners();
                controls.StopButton.onClick.AddListener((UnityAction)(() => manager?.Pause()));
            }
        }

        private static void SetupTabBarHandlers(TabBarComponent tabBar, Transform container)
        {
            // Tab switching logic
            for (int i = 0; i < tabBar.TabButtons.Count; i++)
            {
                var tabIndex = i;
                var button = tabBar.TabButtons[i];
                
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener((UnityAction)(() =>
                {
                    tabBar.SetActiveTab(tabIndex);
                    SwitchToTab(tabIndex, container);
                }));
            }
        }

        private static void TogglePlayback(BackSpeakerMod.NewBackend.BackSpeakerMainManager manager)
        {
            if (manager != null && manager.IsPlaying())
            {
                manager.Pause();
            }
            else
            {
                manager?.Play();
            }
        }

        private static void HandleTabSwitch(int tabIndex, BackSpeakerMod.NewBackend.BackSpeakerMainManager manager)
        {
            switch (tabIndex)
            {
                case 0: // Now Playing
                    UnityEngine.Debug.Log("Switched to Now Playing tab");
                    break;
                case 1: // Playlists
                    UnityEngine.Debug.Log("Switched to Playlists tab");
                    break;
                case 2: // YouTube/Sources
                    UnityEngine.Debug.Log("Switched to Sources tab");
                    break;
                case 3: // Settings
                    UnityEngine.Debug.Log("Switched to Settings tab");
                    break;
            }
        }

        private static void SwitchToTab(int tabIndex, Transform container)
        {
            // Hide/show different content areas based on tab
            // This would be implemented based on your specific needs
        }

        private static void StoreMainScreenReference(BackSpeakerMainScreen mainScreen)
        {
            // Store reference for later updates (progress bar, track info, etc.)
            // This would integrate with your existing update system
        }

        /// <summary>
        /// Complete initialization for new UIWrapper system
        /// </summary>
        public static void InitializeUIWrapperSystem()
        {
            UnityEngine.Debug.Log("Initializing UIWrapper system...");

            // Initialize S1 game wrapper
            S1Factory.Initialize();

            // Register any additional types needed for UI
            S1Factory.RegisterType<UnityEngine.UI.ScrollRect>();
            S1Factory.RegisterType<UnityEngine.UI.Mask>();
            S1Factory.RegisterType<UnityEngine.UI.InputField>();

            UnityEngine.Debug.Log("UIWrapper system initialized!");
        }

        #endregion
    }
} 