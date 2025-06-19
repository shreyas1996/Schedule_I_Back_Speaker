using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BackSpeakerMod.UIWrapper
{
    #region Layout Structures
    
    /// <summary>
    /// Layout structures for the UIWrapper system
    /// </summary>
    
    /// <summary>
    /// Main screen layout structure
    /// </summary>
    public class BackSpeakerLayout
    {
        public GameObject? MainContainer { get; set; }
        public GameObject? HeaderArea { get; set; }
        public GameObject? ContentArea { get; set; }
        public GameObject? FooterArea { get; set; }
    }

    /// <summary>
    /// Track info layout structure
    /// </summary>
    public class TrackInfoLayout
    {
        public GameObject? Container { get; set; }
        public GameObject? AlbumArtArea { get; set; }
        public GameObject? TextInfoArea { get; set; }
    }

    /// <summary>
    /// Controls layout structure
    /// </summary>
    public class ControlsLayout
    {
        public GameObject? Container { get; set; }
        public List<GameObject> ButtonContainers { get; set; } = new List<GameObject>();
    }

    /// <summary>
    /// Popup layout structure
    /// </summary>
    public class PopupLayout
    {
        public GameObject? BackgroundOverlay { get; set; }
        public GameObject? PopupContainer { get; set; }
        public GameObject? TitleBar { get; set; }
        public GameObject? ContentArea { get; set; }
        public GameObject? ButtonArea { get; set; }
    }

    /// <summary>
    /// Button group layout structure
    /// </summary>
    public class ButtonGroupLayout
    {
        public GameObject? Container { get; set; }
        public List<GameObject> ButtonSlots { get; set; } = new List<GameObject>();
        public List<Button> Buttons { get; set; } = new List<Button>();
    }

    /// <summary>
    /// List layout structure
    /// </summary>
    public class ListLayout
    {
        public GameObject? Container { get; set; }
        public ScrollRect? ScrollView { get; set; }
        public GameObject? ContentArea { get; set; }
    }

    #endregion

    #region Configuration Classes

    /// <summary>
    /// Main layout configuration
    /// </summary>
    public class LayoutConfig
    {
        public float Padding { get; set; } = 10f;
        public float HeaderHeight { get; set; } = 60f;
        public float FooterHeight { get; set; } = 50f;
    }

    /// <summary>
    /// Track info layout configuration
    /// </summary>
    public class TrackInfoConfig
    {
        public float Width { get; set; } = 300f;
        public float Height { get; set; } = 80f;
        public float AlbumArtSize { get; set; } = 60f;
    }

    /// <summary>
    /// Controls layout configuration
    /// </summary>
    public class ControlsConfig
    {
        public float Width { get; set; } = 280f;
        public float Height { get; set; } = 60f;
        public int ButtonCount { get; set; } = 4;
        public float ButtonSize { get; set; } = 50f;
    }

    /// <summary>
    /// Popup configuration
    /// </summary>
    public class PopupConfig
    {
        public float Width { get; set; } = 400f;
        public float Height { get; set; } = 300f;
        public Color BackgroundColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        public bool HasTitleBar { get; set; } = true;
        public bool HasButtonArea { get; set; } = true;
        public float TitleBarHeight { get; set; } = 40f;
        public float ButtonAreaHeight { get; set; } = 50f;
    }

    /// <summary>
    /// Button group configuration
    /// </summary>
    public class ButtonGroupConfig
    {
        public int ButtonCount { get; set; } = 3;
        public float TotalWidth { get; set; } = 300f;
        public float ButtonHeight { get; set; } = 40f;
        public float ButtonSpacing { get; set; } = 10f;
    }

    /// <summary>
    /// List configuration
    /// </summary>
    public class ListConfig
    {
        public float Width { get; set; } = 300f;
        public float Height { get; set; } = 200f;
    }

    #endregion

    #region Predefined Configurations

    /// <summary>
    /// Predefined layout configurations for common use cases
    /// </summary>
    public static class LayoutPresets
    {
        public static LayoutConfig BackSpeakerMain => new LayoutConfig
        {
            Padding = 15f,
            HeaderHeight = 70f,
            FooterHeight = 60f
        };

        public static TrackInfoConfig DefaultTrackInfo => new TrackInfoConfig
        {
            Width = 320f,
            Height = 90f,
            AlbumArtSize = 70f
        };

        public static ControlsConfig MusicControls => new ControlsConfig
        {
            Width = 300f,
            Height = 70f,
            ButtonCount = 5,
            ButtonSize = 55f
        };

        public static PopupConfig StandardPopup => new PopupConfig
        {
            Width = 450f,
            Height = 350f,
            BackgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.98f),
            HasTitleBar = true,
            HasButtonArea = true,
            TitleBarHeight = 45f,
            ButtonAreaHeight = 55f
        };

        public static PopupConfig SimplePopup => new PopupConfig
        {
            Width = 300f,
            Height = 200f,
            BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f),
            HasTitleBar = false,
            HasButtonArea = false
        };

        public static PopupConfig YouTubePopup => new PopupConfig
        {
            Width = 500f,
            Height = 400f,
            BackgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.98f),
            HasTitleBar = true,
            HasButtonArea = true,
            TitleBarHeight = 50f,
            ButtonAreaHeight = 60f
        };

        public static ButtonGroupConfig PlaylistActions => new ButtonGroupConfig
        {
            ButtonCount = 4,
            TotalWidth = 350f,
            ButtonHeight = 45f,
            ButtonSpacing = 15f
        };

        public static ListConfig PlaylistList => new ListConfig
        {
            Width = 350f,
            Height = 400f
        };

        public static ListConfig SourcesList => new ListConfig
        {
            Width = 350f,
            Height = 350f
        };
    }

    /// <summary>
    /// Predefined button group configurations
    /// </summary>
    public static class ButtonGroupPresets
    {
        public static ButtonGroupConfig PlaylistActions => new ButtonGroupConfig
        {
            ButtonCount = 3,
            TotalWidth = 300f,
            ButtonHeight = 40f,
            ButtonSpacing = 10f
        };

        public static ButtonGroupConfig SourceActions => new ButtonGroupConfig
        {
            ButtonCount = 3,
            TotalWidth = 320f,
            ButtonHeight = 45f,
            ButtonSpacing = 12f
        };

        public static ButtonGroupConfig MediaControls => new ButtonGroupConfig
        {
            ButtonCount = 5,
            TotalWidth = 280f,
            ButtonHeight = 50f,
            ButtonSpacing = 8f
        };
    }

    /// <summary>
    /// Predefined list configurations
    /// </summary>
    public static class ListLayoutConfig
    {
        public static ListConfig PlaylistList => new ListConfig
        {
            Width = 350f,
            Height = 400f
        };

        public static ListConfig SourcesList => new ListConfig
        {
            Width = 350f,
            Height = 350f
        };

        public static ListConfig TrackQueue => new ListConfig
        {
            Width = 300f,
            Height = 200f
        };
    }



    #endregion
} 