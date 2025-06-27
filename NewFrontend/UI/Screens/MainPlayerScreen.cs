using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.NewBackend;
using BackSpeakerMod.S1Wrapper;
using BackSpeakerMod.NewFrontend.UI.Interfaces;

namespace BackSpeakerMod.NewFrontend.UI.Screens
{
    /// <summary>
    /// Main player screen - the primary interface for the BackSpeaker music app
    /// Features: Now Playing, Player Controls, Queue, Navigation, Headphone Control
    /// </summary>
    public class MainPlayerScreen : MonoBehaviour
    {
        #region Private Fields
        
        private BackSpeakerMainManager mainManager;
        private NavigationManager navigationManager;
        private IMusicBackend musicBackend;
        
        // UI Elements
        private GameObject mainContainer;
        private GameObject topBar;
        private GameObject nowPlayingCard;
        private GameObject playerControls;
        private GameObject bottomBar;
        private CollapsibleSection queueSection;
        
        // Now Playing Components
        private Image albumArtImage;
        private Text songTitleText;
        private Text artistText;
        private Slider progressSlider;
        private Text currentTimeText;
        private Text totalTimeText;
        
        // Player Control Buttons
        private Button previousButton;
        private Button playPauseButton;
        private Button nextButton;
        private Button shuffleButton;
        private Button repeatButton;
        
        // Navigation & System
        private Button hamburgerButton;
        private Button headphoneToggleButton;
        private Slider volumeSlider;
        private Text headphoneStatusText;
        
        // State
        private bool isShuffleOn = false;
        private RepeatMode repeatMode = RepeatMode.None;
        
        #endregion
        
        #region Public Properties
        
        public bool IsInitialized { get; private set; }
        public NavigationManager Navigation => navigationManager;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the main player screen
        /// </summary>
        public void Initialize(BackSpeakerMainManager manager)
        {
            try
            {
                mainManager = manager;
                
                // For now, use mock backend - TODO: Replace with actual backend integration
                musicBackend = new MockMusicBackend();
                
                NewLoggingSystem.Info("Initializing MainPlayerScreen", "MainPlayerScreen");
                
                CreateMainLayout();
                CreateTopBar();
                CreateNowPlayingCard();
                CreatePlayerControls();
                CreateBottomBar();
                CreateQueue();
                
                InitializeNavigation();
                SetupEventHandlers();
                UpdateUI();
                
                IsInitialized = true;
                NewLoggingSystem.Info("MainPlayerScreen initialized successfully", "MainPlayerScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to initialize MainPlayerScreen: {ex}", "MainPlayerScreen");
            }
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateMainLayout()
        {
            // Main container with dark background - FULL SCREEN
            mainContainer = new GameObject("MainPlayerScreen");
            mainContainer.transform.SetParent(this.transform, false);
            
            var mainRect = mainContainer.AddComponent<RectTransform>();
            mainRect.anchorMin = Vector2.zero;
            mainRect.anchorMax = Vector2.one;
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;
            
            var mainImage = mainContainer.AddComponent<Image>();
            mainImage.color = ModernUIFactory.Colors.Background;
            
            // Main vertical layout - fills entire screen
            var mainLayout = mainContainer.AddComponent<VerticalLayoutGroup>();
            mainLayout.spacing = 0;
            mainLayout.padding = new RectOffset(0, 0, 0, 0);
            mainLayout.childControlHeight = false;
            mainLayout.childControlWidth = true;
            mainLayout.childForceExpandHeight = true;
            mainLayout.childForceExpandWidth = true;
        }
        
        private void CreateTopBar()
        {
            topBar = new GameObject("TopBar");
            topBar.transform.SetParent(mainContainer.transform, false);
            
            var topBarRect = topBar.AddComponent<RectTransform>();
            topBarRect.sizeDelta = new Vector2(0, 80); // Taller top bar
            
            var topBarImage = topBar.AddComponent<Image>();
            topBarImage.color = ModernUIFactory.Colors.Surface;
            
            // Horizontal layout for top bar
            var topBarLayout = topBar.AddComponent<HorizontalLayoutGroup>();
            topBarLayout.spacing = 15;
            topBarLayout.padding = new RectOffset(20, 20, 15, 15);
            topBarLayout.childControlHeight = true;
            topBarLayout.childControlWidth = false;
            topBarLayout.childForceExpandHeight = false;
            topBarLayout.childForceExpandWidth = false;
            
            // Hamburger menu button
            hamburgerButton = ModernUIFactory.CreateHamburgerButton(topBar.transform, S1Factory.ConvertToUnityAction(OnHamburgerClick));
            var hamburgerLayout = hamburgerButton.gameObject.AddComponent<LayoutElement>();
            hamburgerLayout.preferredWidth = 50;
            hamburgerLayout.preferredHeight = 50;
            
            // Title
            var titleText = ModernUIFactory.CreateModernText(topBar.transform, "BackSpeaker", 22, TextStyle.Primary, TextAnchor.MiddleLeft);
            titleText.fontStyle = FontStyle.Bold;
            var titleLayout = titleText.gameObject.AddComponent<LayoutElement>();
            titleLayout.flexibleWidth = 1;
            
            // Headphone status indicator
            headphoneStatusText = ModernUIFactory.CreateModernText(topBar.transform, "♫ OFF", 16, TextStyle.Muted, TextAnchor.MiddleRight);
            var statusLayout = headphoneStatusText.gameObject.AddComponent<LayoutElement>();
            statusLayout.preferredWidth = 100;
            
            // Layout element for top bar
            var topBarLayoutElement = topBar.AddComponent<LayoutElement>();
            topBarLayoutElement.preferredHeight = 80;
            topBarLayoutElement.flexibleHeight = 0;
        }
        
        private void CreateNowPlayingCard()
        {
            nowPlayingCard = ModernUIFactory.CreateCard(mainContainer.transform, new Vector2(0, 0), true);
            
            var cardLayout = nowPlayingCard.AddComponent<VerticalLayoutGroup>();
            cardLayout.spacing = 15;
            cardLayout.padding = new RectOffset(25, 25, 25, 25);
            cardLayout.childControlHeight = false;
            cardLayout.childControlWidth = true;
            
            // Album art and song info horizontal layout
            var infoContainer = ModernUIFactory.CreateHorizontalLayout(nowPlayingCard.transform, 20, new RectOffset(0, 0, 0, 0));
            var infoLayout = infoContainer.AddComponent<LayoutElement>();
            infoLayout.preferredHeight = 120;
            
            // Album art
            albumArtImage = ModernUIFactory.CreateAlbumArt(infoContainer.transform, new Vector2(100, 100));
            var albumLayout = albumArtImage.gameObject.AddComponent<LayoutElement>();
            albumLayout.preferredWidth = 100;
            albumLayout.preferredHeight = 100;
            
            // Song info vertical layout
            var songInfoContainer = ModernUIFactory.CreateVerticalLayout(infoContainer.transform, 8, new RectOffset(0, 0, 0, 0));
            
            // Song title
            songTitleText = ModernUIFactory.CreateModernText(songInfoContainer.transform, "No song playing", 18, TextStyle.Primary);
            songTitleText.fontStyle = FontStyle.Bold;
            var titleLayout = songTitleText.gameObject.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 25;
            
            // Artist name
            artistText = ModernUIFactory.CreateModernText(songInfoContainer.transform, "Select a song to play", 14, TextStyle.Secondary);
            var artistLayout = artistText.gameObject.AddComponent<LayoutElement>();
            artistLayout.preferredHeight = 20;
            
            // Progress section
            CreateProgressSection();
            
            // Layout element for now playing card - take remaining space
            var cardLayoutElement = nowPlayingCard.AddComponent<LayoutElement>();
            cardLayoutElement.flexibleHeight = 1; // Take remaining space
            cardLayoutElement.minHeight = 280;
        }
        
        private void CreateProgressSection()
        {
            var progressContainer = ModernUIFactory.CreateVerticalLayout(nowPlayingCard.transform, 5, new RectOffset(0, 0, 0, 0));
            var progressLayout = progressContainer.AddComponent<LayoutElement>();
            progressLayout.preferredHeight = 60;
            
            // Progress slider
            progressSlider = ModernUIFactory.CreateModernSlider(progressContainer.transform, 0f, 100f, 0f, S1Factory.ConvertToUnityAction<float>(OnProgressSliderChanged));
            var sliderLayout = progressSlider.gameObject.AddComponent<LayoutElement>();
            sliderLayout.preferredHeight = 25;
            
            // Time labels
            var timeContainer = ModernUIFactory.CreateHorizontalLayout(progressContainer.transform, 0, new RectOffset(0, 0, 0, 0));
            var timeLayout = timeContainer.AddComponent<LayoutElement>();
            timeLayout.preferredHeight = 20;
            
            currentTimeText = ModernUIFactory.CreateModernText(timeContainer.transform, "0:00", 12, TextStyle.Muted, TextAnchor.MiddleLeft);
            var currentLayout = currentTimeText.gameObject.AddComponent<LayoutElement>();
            currentLayout.flexibleWidth = 1;
            
            totalTimeText = ModernUIFactory.CreateModernText(timeContainer.transform, "0:00", 12, TextStyle.Muted, TextAnchor.MiddleRight);
            var totalLayout = totalTimeText.gameObject.AddComponent<LayoutElement>();
            totalLayout.flexibleWidth = 1;
        }
        
        private void CreatePlayerControls()
        {
            playerControls = ModernUIFactory.CreateCard(mainContainer.transform, new Vector2(0, 120), false);
            
            var controlsLayout = playerControls.AddComponent<HorizontalLayoutGroup>();
            controlsLayout.spacing = 25;
            controlsLayout.padding = new RectOffset(35, 35, 25, 25);
            controlsLayout.childControlHeight = true;
            controlsLayout.childControlWidth = false;
            controlsLayout.childForceExpandHeight = false;
            controlsLayout.childForceExpandWidth = false;
            controlsLayout.childAlignment = TextAnchor.MiddleCenter;
            
            // Shuffle button
            shuffleButton = ModernUIFactory.CreateIconButton(playerControls.transform, "⧢", S1Factory.ConvertToUnityAction(OnShuffleClick), new Vector2(55, 55));
            var shuffleLayout = shuffleButton.gameObject.AddComponent<LayoutElement>();
            shuffleLayout.preferredWidth = 55;
            shuffleLayout.preferredHeight = 55;
            
            // Previous button
            previousButton = ModernUIFactory.CreateIconButton(playerControls.transform, "⏪", S1Factory.ConvertToUnityAction(OnPreviousClick), new Vector2(60, 60));
            var prevLayout = previousButton.gameObject.AddComponent<LayoutElement>();
            prevLayout.preferredWidth = 60;
            prevLayout.preferredHeight = 60;
            
            // Play/Pause button (larger)
            playPauseButton = ModernUIFactory.CreateIconButton(playerControls.transform, "▶", S1Factory.ConvertToUnityAction(OnPlayPauseClick), new Vector2(75, 75));
            var playLayout = playPauseButton.gameObject.AddComponent<LayoutElement>();
            playLayout.preferredWidth = 75;
            playLayout.preferredHeight = 75;
            
            // Next button
            nextButton = ModernUIFactory.CreateIconButton(playerControls.transform, "⏩", S1Factory.ConvertToUnityAction(OnNextClick), new Vector2(60, 60));
            var nextLayout = nextButton.gameObject.AddComponent<LayoutElement>();
            nextLayout.preferredWidth = 60;
            nextLayout.preferredHeight = 60;
            
            // Repeat button
            repeatButton = ModernUIFactory.CreateIconButton(playerControls.transform, "↻", S1Factory.ConvertToUnityAction(OnRepeatClick), new Vector2(55, 55));
            var repeatLayout = repeatButton.gameObject.AddComponent<LayoutElement>();
            repeatLayout.preferredWidth = 55;
            repeatLayout.preferredHeight = 55;
            
            // Layout element for player controls
            var controlsLayoutElement = playerControls.AddComponent<LayoutElement>();
            controlsLayoutElement.preferredHeight = 120;
            controlsLayoutElement.flexibleHeight = 0;
        }
        
        private void CreateBottomBar()
        {
            bottomBar = new GameObject("BottomBar");
            bottomBar.transform.SetParent(mainContainer.transform, false);
            
            var bottomRect = bottomBar.AddComponent<RectTransform>();
            bottomRect.sizeDelta = new Vector2(0, 100);
            
            var bottomImage = bottomBar.AddComponent<Image>();
            bottomImage.color = ModernUIFactory.Colors.Surface;
            
            // Horizontal layout for bottom bar
            var bottomLayout = bottomBar.AddComponent<HorizontalLayoutGroup>();
            bottomLayout.spacing = 20;
            bottomLayout.padding = new RectOffset(25, 25, 20, 20);
            bottomLayout.childControlHeight = true;
            bottomLayout.childControlWidth = false;
            bottomLayout.childForceExpandHeight = false;
            bottomLayout.childForceExpandWidth = false;
            
            // Headphone toggle button
            headphoneToggleButton = ModernUIFactory.CreateModernButton(bottomBar.transform, "♫ Put On", S1Factory.ConvertToUnityAction(OnHeadphoneToggle), ButtonStyle.Primary, new Vector2(120, 60));
            var headphoneLayout = headphoneToggleButton.gameObject.AddComponent<LayoutElement>();
            headphoneLayout.preferredWidth = 120;
            headphoneLayout.preferredHeight = 60;
            
            // Volume control
            var volumeContainer = ModernUIFactory.CreateVerticalLayout(bottomBar.transform, 5, new RectOffset(0, 0, 0, 0));
            var volContainerLayout = volumeContainer.AddComponent<LayoutElement>();
            volContainerLayout.flexibleWidth = 1;
            volContainerLayout.preferredHeight = 50;
            
            var volumeLabel = ModernUIFactory.CreateModernText(volumeContainer.transform, "Volume", 12, TextStyle.Secondary, TextAnchor.MiddleCenter);
            var volumeLabelLayout = volumeLabel.gameObject.AddComponent<LayoutElement>();
            volumeLabelLayout.preferredHeight = 20;
            
            volumeSlider = ModernUIFactory.CreateModernSlider(volumeContainer.transform, 0f, 1f, 0.7f, S1Factory.ConvertToUnityAction<float>(OnVolumeChanged));
            var volumeSliderLayout = volumeSlider.gameObject.AddComponent<LayoutElement>();
            volumeSliderLayout.preferredHeight = 25;
            
            // Layout element for bottom bar
            var bottomLayoutElement = bottomBar.AddComponent<LayoutElement>();
            bottomLayoutElement.preferredHeight = 100;
            bottomLayoutElement.flexibleHeight = 0;
        }
        
        private void CreateQueue()
        {
            queueSection = ModernUIFactory.CreateCollapsibleSection(mainContainer.transform, "Queue", false);
            var queueLayoutElement = queueSection.gameObject.AddComponent<LayoutElement>();
            queueLayoutElement.flexibleHeight = 1;
            
            // Queue will be populated dynamically
            // For now, add placeholder
            var queuePlaceholder = ModernUIFactory.CreateModernText(queueSection.ContentContainer, "Queue is empty", 14, TextStyle.Muted, TextAnchor.MiddleCenter);
            var placeholderLayout = queuePlaceholder.gameObject.AddComponent<LayoutElement>();
            placeholderLayout.preferredHeight = 40;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnHamburgerClick()
        {
            NewLoggingSystem.Info("Hamburger menu clicked", "MainPlayerScreen");
            navigationManager?.ShowNavigationMenu();
        }
        
        private void OnPlayPauseClick()
        {
            if (!musicBackend.HeadphonesEnabled)
            {
                NewLoggingSystem.Warning("Cannot play music - headphones not enabled", "MainPlayerScreen");
                return;
            }
            
            if (musicBackend.IsPlaying)
            {
                musicBackend.PausePlayback();
            }
            else
            {
                if (musicBackend.CurrentTrack != null)
                {
                    musicBackend.ResumePlayback();
                }
                else
                {
                    // No current track, need to load something
                    NewLoggingSystem.Info("No track loaded - please select a song first", "MainPlayerScreen");
                }
            }
            
            UpdatePlayPauseButton();
        }
        

        
        private void OnShuffleClick()
        {
            ToggleShuffle();
        }
        
        private void OnRepeatClick()
        {
            CycleRepeatMode();
        }
        
        private void OnPreviousClick()
        {
            musicBackend.PreviousTrack();
        }
        
        private void OnNextClick()
        {
            musicBackend.NextTrack();
        }
        
        private void OnHeadphoneToggle()
        {
            if (musicBackend.HeadphonesEnabled)
            {
                musicBackend.DisableHeadphones();
            }
            else
            {
                musicBackend.EnableHeadphones();
            }
            
            UpdateHeadphoneButton();
            UpdatePlayPauseButton();
        }
        
        private void OnVolumeChanged(float value)
        {
            musicBackend.SetVolume(value);
        }
        
        private void OnProgressSliderChanged(float value)
        {
            musicBackend.SeekTo(value / 100f); // Convert from 0-100 to 0-1
        }
        
        #endregion
        
        #region Control Logic
        
        private void ToggleShuffle()
        {
            isShuffleOn = !isShuffleOn;
            if (isShuffleOn)
            {
                musicBackend.ShuffleQueue();
            }
            UpdateShuffleButton();
            NewLoggingSystem.Info($"Shuffle turned {(isShuffleOn ? "ON" : "OFF")}", "MainPlayerScreen");
        }
        
        private void CycleRepeatMode()
        {
            repeatMode = (RepeatMode)(((int)repeatMode + 1) % 3);
            UpdateRepeatButton();
            NewLoggingSystem.Info($"Repeat mode: {repeatMode}", "MainPlayerScreen");
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateUI()
        {
            UpdatePlayPauseButton();
            UpdateShuffleButton();
            UpdateRepeatButton();
            UpdateHeadphoneButton();
        }
        
        private void UpdatePlayPauseButton()
        {
            if (playPauseButton != null)
            {
                var textComponent = playPauseButton.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    textComponent.text = musicBackend.IsPlaying ? "⏸" : "▶";
                }
            }
        }
        
        private void UpdateShuffleButton()
        {
            if (shuffleButton != null)
            {
                var image = shuffleButton.GetComponent<Image>();
                if (image != null)
                {
                    image.color = isShuffleOn ? ModernUIFactory.Colors.Primary : ModernUIFactory.Colors.Secondary;
                }
            }
        }
        
        private void UpdateRepeatButton()
        {
            if (repeatButton != null)
            {
                var textComponent = repeatButton.GetComponentInChildren<Text>();
                var image = repeatButton.GetComponent<Image>();
                
                if (textComponent != null && image != null)
                {
                    switch (repeatMode)
                    {
                        case RepeatMode.None:
                            textComponent.text = "↻";
                            image.color = ModernUIFactory.Colors.Secondary;
                            break;
                        case RepeatMode.All:
                            textComponent.text = "↻";
                            image.color = ModernUIFactory.Colors.Primary;
                            break;
                        case RepeatMode.One:
                            textComponent.text = "1↻";
                            image.color = ModernUIFactory.Colors.Primary;
                            break;
                    }
                }
            }
        }
        
        private void UpdateHeadphoneButton()
        {
            if (headphoneToggleButton != null)
            {
                var textComponent = headphoneToggleButton.GetComponentInChildren<Text>();
                var image = headphoneToggleButton.GetComponent<Image>();
                
                if (textComponent != null && image != null)
                {
                    if (musicBackend.HeadphonesEnabled)
                    {
                        textComponent.text = "♫ Take Off";
                        image.color = ModernUIFactory.Colors.Success;
                    }
                    else
                    {
                        textComponent.text = "♫ Put On";
                        image.color = ModernUIFactory.Colors.Primary;
                    }
                }
            }
            
            if (headphoneStatusText != null)
            {
                headphoneStatusText.text = musicBackend.HeadphonesEnabled ? "♫ ON" : "♫ OFF";
                headphoneStatusText.color = musicBackend.HeadphonesEnabled ? ModernUIFactory.Colors.Success : ModernUIFactory.Colors.TextMuted;
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void InitializeNavigation()
        {
            var navObj = new GameObject("NavigationManager");
            navObj.transform.SetParent(this.transform, false);
            
            // Ensure NavigationManager covers the full screen area
            var navRect = navObj.AddComponent<RectTransform>();
            navRect.anchorMin = Vector2.zero;    // Bottom-left
            navRect.anchorMax = Vector2.one;     // Top-right  
            navRect.offsetMin = Vector2.zero;    // No offset
            navRect.offsetMax = Vector2.zero;    // No offset
            navRect.anchoredPosition = Vector2.zero;
            
            navigationManager = S1Factory.RegisterAndAddComponent<NavigationManager>(navObj);
            navigationManager.Initialize(mainManager);
        }
        
        private void SetupEventHandlers()
        {
            // Subscribe to backend events for music playback updates
            if (musicBackend != null)
            {
                musicBackend.OnTrackStarted += OnTrackStarted;
                musicBackend.OnPlaybackPaused += OnPlaybackPaused;
                musicBackend.OnPlaybackResumed += OnPlaybackResumed;
                musicBackend.OnTrackEnded += OnTrackEnded;
                musicBackend.OnPlaybackProgress += OnPlaybackProgress;
                musicBackend.OnVolumeChanged += OnVolumeChangedEvent;
                musicBackend.OnHeadphoneStateChanged += OnHeadphoneStateChanged;
                musicBackend.OnQueueChanged += OnQueueChanged;
            }
        }
        #endregion
        
        #region Backend Event Handlers
        
        private void OnTrackStarted(NewSongDetails track)
        {
            if (songTitleText != null)
                songTitleText.text = track.title;
            if (artistText != null)
                artistText.text = track.artist;
            UpdatePlayPauseButton();
        }
        
        private void OnPlaybackPaused()
        {
            UpdatePlayPauseButton();
        }
        
        private void OnPlaybackResumed()
        {
            UpdatePlayPauseButton();
        }
        
        private void OnTrackEnded(NewSongDetails track)
        {
            UpdatePlayPauseButton();
        }
        
        private void OnPlaybackProgress(float currentTime, float totalTime)
        {
            if (progressSlider != null && totalTime > 0)
            {
                progressSlider.value = (currentTime / totalTime) * 100f;
            }
            
            if (currentTimeText != null)
                currentTimeText.text = FormatTime(currentTime);
            if (totalTimeText != null)
                totalTimeText.text = FormatTime(totalTime);
        }
        
        private void OnVolumeChangedEvent(float volume)
        {
            if (volumeSlider != null)
                volumeSlider.value = volume;
        }
        
        private void OnHeadphoneStateChanged(bool enabled)
        {
            UpdateHeadphoneButton();
            if (!enabled)
            {
                UpdatePlayPauseButton();
            }
        }
        
        private void OnQueueChanged(System.Collections.Generic.List<NewSongDetails> queue)
        {
            // TODO: Update queue UI
            NewLoggingSystem.Info($"Queue updated with {queue.Count} tracks", "MainPlayerScreen");
        }
        
        #endregion
        
        #region Helper Methods
        
        private string FormatTime(float seconds)
        {
            var minutes = Mathf.FloorToInt(seconds / 60);
            var remainingSeconds = Mathf.FloorToInt(seconds % 60);
            return $"{minutes}:{remainingSeconds:00}";
        }
        
        #endregion
    }
    
    #region Enums
    
    public enum RepeatMode
    {
        None = 0,
        All = 1,
        One = 2
    }
    
    #endregion
} 