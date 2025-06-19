using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace BackSpeakerMod.UIWrapper
{
    #region Component Structures

    /// <summary>
    /// Track info component structure
    /// </summary>
    public class TrackInfoComponent
    {
        public TrackInfoLayout? Layout { get; set; }
        public Image? AlbumArtImage { get; set; }
        public Text? TrackTitleText { get; set; }
        public Text? ArtistText { get; set; }
        public Text? AlbumText { get; set; }
        public Text? DurationText { get; set; }

        public void UpdateTrackInfo(string title, string artist, string album, string duration, Sprite albumArt = null)
        {
            if (TrackTitleText != null) TrackTitleText.text = title;
            if (ArtistText != null) ArtistText.text = artist;
            if (AlbumText != null) AlbumText.text = album;
            if (DurationText != null) DurationText.text = duration;
            if (AlbumArtImage != null && albumArt != null) AlbumArtImage.sprite = albumArt;
        }
    }

    /// <summary>
    /// Controls component structure
    /// </summary>
    public class ControlsComponent
    {
        public ControlsLayout? Layout { get; set; }
        public Button? PreviousButton { get; set; }
        public Button? PlayPauseButton { get; set; }
        public Button? StopButton { get; set; }
        public Button? NextButton { get; set; }
        public Button? ShuffleButton { get; set; }

        public void SetPlayState(bool isPlaying)
        {
            if (PlayPauseButton?.GetComponentInChildren<Text>() != null)
            {
                PlayPauseButton.GetComponentInChildren<Text>().text = isPlaying ? "⏸" : "▶";
            }
        }
    }

    /// <summary>
    /// Progress bar component structure
    /// </summary>
    public class ProgressBarComponent
    {
        public GameObject? Container { get; set; }
        public Image? Background { get; set; }
        public Image? Fill { get; set; }
        public Text? CurrentTimeText { get; set; }
        public Text? TotalTimeText { get; set; }

        public void UpdateProgress(float progress, string currentTime, string totalTime)
        {
            if (Fill != null)
            {
                var fillRect = Fill.GetComponent<RectTransform>();
                fillRect.anchorMax = new Vector2(Mathf.Clamp01(progress), 1);
            }
            if (CurrentTimeText != null) CurrentTimeText.text = currentTime;
            if (TotalTimeText != null) TotalTimeText.text = totalTime;
        }
    }

    /// <summary>
    /// Tab bar component structure
    /// </summary>
    public class TabBarComponent
    {
        public ButtonGroupLayout? Layout { get; set; }
        public List<Button> TabButtons { get; set; } = new List<Button>();

        public void SetActiveTab(int tabIndex)
        {
            for (int i = 0; i < TabButtons.Count; i++)
            {
                var button = TabButtons[i];
                var colors = button.colors;
                if (i == tabIndex)
                {
                    colors.normalColor = new Color(0.2f, 0.6f, 1f, 0.8f);
                }
                else
                {
                    colors.normalColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);
                }
                button.colors = colors;
            }
        }
    }

    /// <summary>
    /// Popup component structure
    /// </summary>
    public class PopupComponent
    {
        public PopupLayout? Layout { get; set; }
        public Text? TitleText { get; set; }
        public Button? CloseButton { get; set; }
        public List<Button> ActionButtons { get; set; } = new List<Button>();

        public void Show()
        {
            Layout?.BackgroundOverlay?.SetActive(true);
        }

        public void Hide()
        {
            Layout?.BackgroundOverlay?.SetActive(false);
        }
    }

    #endregion

    #region Component Configuration Classes

    /// <summary>
    /// Track info component configuration
    /// </summary>
    public class TrackInfoComponentConfig
    {
        public TrackInfoConfig LayoutConfig { get; set; } = new TrackInfoConfig();
        public AnchorPresets AnchorPreset { get; set; } = AnchorPresets.TopCenter;
        public Vector2 Position { get; set; } = new Vector2(0, -50);
        public int TitleFontSize { get; set; } = 16;
        public int SubtextFontSize { get; set; } = 12;
        public Color TitleColor { get; set; } = Color.white;
        public Color SubtextColor { get; set; } = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    /// <summary>
    /// Controls component configuration
    /// </summary>
    public class ControlsComponentConfig
    {
        public ControlsConfig LayoutConfig { get; set; } = new ControlsConfig();
        public AnchorPresets AnchorPreset { get; set; } = AnchorPresets.MiddleCenter;
        public Vector2 Position { get; set; } = Vector2.zero;
        public ButtonStyle ButtonStyle { get; set; } = ButtonStyle.Primary;
        public System.Action? OnPrevious { get; set; }
        public System.Action? OnPlayPause { get; set; }
        public System.Action? OnStop { get; set; }
        public System.Action? OnNext { get; set; }
        public System.Action? OnShuffle { get; set; }
    }

    /// <summary>
    /// Progress bar component configuration
    /// </summary>
    public class ProgressBarComponentConfig
    {
        public AnchorPresets AnchorPreset { get; set; } = AnchorPresets.TopCenter;
        public Vector2 Position { get; set; } = new Vector2(0, -200);
        public float Width { get; set; } = 300f;
        public float Height { get; set; } = 20f;
        public Color BackgroundColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        public Color FillColor { get; set; } = new Color(0.2f, 0.8f, 0.2f, 1f);
    }

    /// <summary>
    /// Tab bar component configuration
    /// </summary>
    public class TabBarComponentConfig
    {
        public AnchorPresets AnchorPreset { get; set; } = AnchorPresets.TopCenter;
        public Vector2 Position { get; set; } = new Vector2(0, -250);
        public float Width { get; set; } = 320f;
        public float Height { get; set; } = 40f;
        public float TabSpacing { get; set; } = 5f;
        public string[] TabNames { get; set; } = { "Music", "Playlists", "Settings" };
        public System.Action<int>? OnTabClick { get; set; }
    }

    /// <summary>
    /// Popup component configuration
    /// </summary>
    public class PopupComponentConfig
    {
        public PopupConfig PopupConfig { get; set; } = new PopupConfig();
        public string Title { get; set; } = "Popup";
        public System.Action? OnClose { get; set; }
        public PopupButtonConfig[]? ButtonConfigs { get; set; }
    }

    /// <summary>
    /// Popup button configuration
    /// </summary>
    public class PopupButtonConfig
    {
        public string Text { get; set; } = "Button";
        public ButtonStyle Style { get; set; } = ButtonStyle.Default;
        public System.Action? OnClick { get; set; }
    }

    #endregion

    #region Component Presets

    /// <summary>
    /// Predefined component configurations
    /// </summary>
    public static class ComponentPresets
    {
        public static TrackInfoComponentConfig DefaultTrackInfo => new TrackInfoComponentConfig
        {
            LayoutConfig = LayoutPresets.DefaultTrackInfo,
            AnchorPreset = AnchorPresets.TopCenter,
            Position = new Vector2(0, -60),
            TitleFontSize = 18,
            SubtextFontSize = 13,
            TitleColor = new Color(1f, 1f, 1f, 1f),
            SubtextColor = new Color(0.85f, 0.85f, 0.85f, 1f)
        };

        public static ControlsComponentConfig MusicControls => new ControlsComponentConfig
        {
            LayoutConfig = LayoutPresets.MusicControls,
            AnchorPreset = AnchorPresets.MiddleCenter,
            Position = new Vector2(0, -50),
            ButtonStyle = ButtonStyle.Primary
        };

        public static ProgressBarComponentConfig DefaultProgressBar => new ProgressBarComponentConfig
        {
            AnchorPreset = AnchorPresets.TopCenter,
            Position = new Vector2(0, -180),
            Width = 320f,
            Height = 25f,
            BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.9f),
            FillColor = new Color(0.3f, 0.7f, 1f, 1f)
        };

        public static TabBarComponentConfig DefaultTabBar => new TabBarComponentConfig
        {
            AnchorPreset = AnchorPresets.TopCenter,
            Position = new Vector2(0, -220),
            Width = 350f,
            Height = 45f,
            TabSpacing = 8f,
            TabNames = new[] { "Now Playing", "Playlists", "Sources" }
        };

        public static PopupComponentConfig ConfirmationPopup => new PopupComponentConfig
        {
            PopupConfig = LayoutPresets.StandardPopup,
            Title = "Confirmation",
            ButtonConfigs = new[]
            {
                new PopupButtonConfig { Text = "Cancel", Style = ButtonStyle.Default },
                new PopupButtonConfig { Text = "Confirm", Style = ButtonStyle.Primary }
            }
        };

        public static PopupComponentConfig YouTubeAddPopup => new PopupComponentConfig
        {
            PopupConfig = new PopupConfig 
            { 
                Width = 500f, 
                Height = 400f, 
                BackgroundColor = new Color(0.12f, 0.12f, 0.15f, 0.98f),
                HasTitleBar = true,
                HasButtonArea = true,
                TitleBarHeight = 50f,
                ButtonAreaHeight = 60f
            },
            Title = "Add YouTube Content",
            ButtonConfigs = new[]
            {
                new PopupButtonConfig { Text = "Cancel", Style = ButtonStyle.Default },
                new PopupButtonConfig { Text = "Add", Style = ButtonStyle.Success }
            }
        };

        public static PopupComponentConfig ErrorPopup => new PopupComponentConfig
        {
            PopupConfig = new PopupConfig 
            { 
                Width = 350f, 
                Height = 250f, 
                BackgroundColor = new Color(0.15f, 0.1f, 0.1f, 0.98f),
                HasTitleBar = true,
                HasButtonArea = true,
                TitleBarHeight = 40f,
                ButtonAreaHeight = 50f
            },
            Title = "Error",
            ButtonConfigs = new[]
            {
                new PopupButtonConfig { Text = "OK", Style = ButtonStyle.Danger }
            }
        };

        public static PopupComponentConfig PlaylistSelectionPopup => new PopupComponentConfig
        {
            PopupConfig = new PopupConfig 
            { 
                Width = 450f, 
                Height = 350f, 
                BackgroundColor = new Color(0.1f, 0.1f, 0.12f, 0.98f),
                HasTitleBar = true,
                HasButtonArea = true,
                TitleBarHeight = 45f,
                ButtonAreaHeight = 55f
            },
            Title = "Select Playlist",
            ButtonConfigs = new[]
            {
                new PopupButtonConfig { Text = "Cancel", Style = ButtonStyle.Default }
            }
        };

        public static PopupComponentConfig SettingsPopup => new PopupComponentConfig
        {
            PopupConfig = new PopupConfig 
            { 
                Width = 480f, 
                Height = 420f, 
                BackgroundColor = new Color(0.1f, 0.12f, 0.1f, 0.98f),
                HasTitleBar = true,
                HasButtonArea = true,
                TitleBarHeight = 50f,
                ButtonAreaHeight = 60f
            },
            Title = "Settings",
            ButtonConfigs = new[]
            {
                new PopupButtonConfig { Text = "Cancel", Style = ButtonStyle.Default },
                new PopupButtonConfig { Text = "Save", Style = ButtonStyle.Primary }
            }
        };
    }

    #endregion
} 