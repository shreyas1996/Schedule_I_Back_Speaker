using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Utils;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Helpers;
using Newtonsoft.Json;
using System;
using BackSpeakerMod.Core.Features.Audio;

namespace BackSpeakerMod.UI.Components
{
    public class YouTubePopupComponent : MonoBehaviour
    {
        private BackSpeakerManager manager;

        // UI Elements - stored as class members to avoid scope issues
        private GameObject popupContainer;
        private InputField searchBarInputField;
        private Button searchButton;
        private Button downloadButton;
        private Button cancelButton;
        private Text songInfoText;
        private Text statusText;
        
        // Current song details
        private SongDetails currentSongDetails;
        private bool isSearching = false;
        private bool isDownloading = false;
        
        // Loading animation
        private float loadingDots = 0f;
        private const float LoadingSpeed = 2f;

        public YouTubePopupComponent() : base() { }

        public void Setup(BackSpeakerManager manager)
        {
            LoggingSystem.Info("YouTube popup component setup", "UI");
            this.manager = manager;
        }

        void Update()
        {
            // Animate loading indicators
            if (isSearching || isDownloading)
            {
                loadingDots += Time.deltaTime * LoadingSpeed;
                if (loadingDots > 3f) loadingDots = 0f;
                
                int dotCount = (int)loadingDots + 1;
                string dots = new string('.', dotCount);
                
                if (isSearching && statusText != null)
                {
                    statusText.text = $"🔍 Getting song information{dots}";
                    statusText.color = Color.yellow;
                }
                else if (isDownloading && statusText != null)
                {
                    statusText.text = $"⬇️ Downloading song{dots} This may take a while";
                    statusText.color = Color.yellow;
                }
            }
        }

        public void OpenYouTubeSearchPopup()
        {
            LoggingSystem.Info("YouTube popup component shown", "UI");
            try
            {
                CreatePopupUI();
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating popup UI: {ex.Message}", "UI");
                throw;
            }
        }

        private void CreatePopupUI()
        {
            // Create the popup container with proper background - make it fill the parent container, not this component
            popupContainer = new GameObject("YouTubeSearchPopupContainer");
            popupContainer.transform.SetParent(this.transform.parent, false);  // Use parent instead of this.transform
            
            var popupRect = popupContainer.AddComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;

            // Make sure it appears on top
            popupContainer.transform.SetAsLastSibling();

            // Add background
            var bgImage = popupContainer.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // Create main content area
            var contentArea = new GameObject("YouTubeContentArea");
            contentArea.transform.SetParent(popupContainer.transform, false);
            
            var contentRect = contentArea.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.1f, 0.1f);
            contentRect.anchorMax = new Vector2(0.9f, 0.9f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            try {
                // Title
                CreateTitleText(contentArea);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating title text: {ex.Message}", "UI");
                throw;
            }
            
            try {
                // URL Input
                CreateUrlInput(contentArea);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating URL input: {ex.Message}", "UI");
                throw;
            }
            
            try {
                // Search Button
                CreateSearchButton(contentArea);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating search button: {ex.Message}", "UI");
                throw;
            }
            
            try {
                // Song Info Display
                CreateSongInfoDisplay(contentArea);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating song info display: {ex.Message}", "UI");
                throw;
            }
            
            try {
                // Action Buttons
                CreateActionButtons(contentArea);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating action buttons: {ex.Message}", "UI");
                throw;
            }
            
            try {
                // Status Text
                CreateStatusText(contentArea);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating status text: {ex.Message}", "UI");
                throw;
            }
            
            try {
                // Update button states
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error updating button states: {ex.Message}", "UI");
                throw;
            }
        }

        private void CreateTitleText(GameObject parent)
        {
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent.transform, false);
            
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0.85f);
            titleRect.anchorMax = new Vector2(1f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            var titleText = titleObj.AddComponent<Text>();
            titleText.text = "📺 YouTube Music Downloader";
            FontHelper.SetSafeFont(titleText);
            titleText.fontSize = 18;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontStyle = FontStyle.Bold;
        }

        private void CreateUrlInput(GameObject parent)
        {
            var inputContainer = new GameObject("URLInputContainer");
            inputContainer.transform.SetParent(parent.transform, false);
            
            var inputRect = inputContainer.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0f, 0.7f);
            inputRect.anchorMax = new Vector2(1f, 0.8f);
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;

            // Background for input
            var inputBg = inputContainer.AddComponent<Image>();
            inputBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Create the text component for InputField first
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(inputContainer.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 0f);
            textRect.offsetMax = new Vector2(-10f, 0f);
            
            var textComponent = textObj.AddComponent<Text>();
            FontHelper.SetSafeFont(textComponent);
            textComponent.fontSize = 14;
            textComponent.color = Color.white;
            textComponent.supportRichText = false;

            // Create placeholder
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputContainer.transform, false);
            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(10f, 0f);
            placeholderRect.offsetMax = new Vector2(-10f, 0f);
            
            var placeholderText = placeholderObj.AddComponent<Text>();
            placeholderText.text = "Enter YouTube URL here...";
            FontHelper.SetSafeFont(placeholderText);
            placeholderText.fontSize = 14;
            placeholderText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            placeholderText.fontStyle = FontStyle.Italic;

            // Now create InputField and assign the components
            searchBarInputField = inputContainer.AddComponent<InputField>();
            searchBarInputField.targetGraphic = inputBg;
            searchBarInputField.textComponent = textComponent;
            searchBarInputField.placeholder = placeholderText;
            searchBarInputField.characterLimit = 500;
        }

        private void CreateSearchButton(GameObject parent)
        {
            var buttonObj = new GameObject("SearchButton");
            buttonObj.transform.SetParent(parent.transform, false);
            
            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.35f, 0.6f);
            buttonRect.anchorMax = new Vector2(0.65f, 0.68f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;

            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.7f, 0.2f, 0.8f);

            searchButton = buttonObj.AddComponent<Button>();
            searchButton.targetGraphic = buttonImage;

            var buttonText = new GameObject("Text");
            buttonText.transform.SetParent(buttonObj.transform, false);
            var textRect = buttonText.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = buttonText.AddComponent<Text>();
            text.text = "🔍 Get Song Info";
            FontHelper.SetSafeFont(text);
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;

            searchButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnSearchButtonClicked);
        }

        private void CreateSongInfoDisplay(GameObject parent)
        {
            LoggingSystem.Debug("CreateSongInfoDisplay: Starting", "UI");
            
            if (parent == null)
            {
                LoggingSystem.Error("CreateSongInfoDisplay: parent is null!", "UI");
                throw new ArgumentNullException(nameof(parent));
            }
            LoggingSystem.Debug("CreateSongInfoDisplay: parent is valid", "UI");
            
            var infoContainer = new GameObject("SongInfoContainer");
            LoggingSystem.Debug("CreateSongInfoDisplay: Created infoContainer GameObject", "UI");
            
            if (parent.transform == null)
            {
                LoggingSystem.Error("CreateSongInfoDisplay: parent.transform is null!", "UI");
                throw new ArgumentNullException("parent.transform");
            }
            
            infoContainer.transform.SetParent(parent.transform, false);
            LoggingSystem.Debug("CreateSongInfoDisplay: Set parent transform", "UI");
            
            var infoRect = infoContainer.AddComponent<RectTransform>();
            LoggingSystem.Debug("CreateSongInfoDisplay: Added RectTransform component", "UI");
            
            infoRect.anchorMin = new Vector2(0.05f, 0.35f);
            infoRect.anchorMax = new Vector2(0.95f, 0.55f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;
            LoggingSystem.Debug("CreateSongInfoDisplay: Set RectTransform properties", "UI");

            // Background
            var infoBg = infoContainer.AddComponent<Image>();
            LoggingSystem.Debug("CreateSongInfoDisplay: Added Image component", "UI");
            
            infoBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            LoggingSystem.Debug("CreateSongInfoDisplay: Set Image color", "UI");

            // Debug the GameObject state before adding Text component
            LoggingSystem.Debug($"CreateSongInfoDisplay: About to add Text component to infoContainer", "UI");
            LoggingSystem.Debug($"CreateSongInfoDisplay: infoContainer name: {(infoContainer != null ? infoContainer.name : "NULL")}", "UI");
            LoggingSystem.Debug($"CreateSongInfoDisplay: infoContainer active: {(infoContainer != null ? infoContainer.activeInHierarchy.ToString() : "NULL")}", "UI");
            LoggingSystem.Debug($"CreateSongInfoDisplay: infoContainer activeSelf: {(infoContainer != null ? infoContainer.activeSelf.ToString() : "NULL")}", "UI");
            LoggingSystem.Debug($"CreateSongInfoDisplay: infoContainer transform: {(infoContainer != null && infoContainer.transform != null ? "Valid" : "NULL")}", "UI");
            LoggingSystem.Debug($"CreateSongInfoDisplay: infoContainer has RectTransform: {(infoContainer != null && infoContainer.GetComponent<RectTransform>() != null ? "Yes" : "No")}", "UI");
            LoggingSystem.Debug($"CreateSongInfoDisplay: infoContainer has Image: {(infoContainer != null && infoContainer.GetComponent<Image>() != null ? "Yes" : "No")}", "UI");

            // Create a separate GameObject for the Text component to avoid potential conflicts
            LoggingSystem.Debug("CreateSongInfoDisplay: Creating separate GameObject for Text component", "UI");
            var textContainer = new GameObject("SongInfoText");
            LoggingSystem.Debug("CreateSongInfoDisplay: Created textContainer GameObject", "UI");
            
            textContainer.transform.SetParent(infoContainer.transform, false);
            LoggingSystem.Debug("CreateSongInfoDisplay: Set textContainer parent", "UI");
            
            // Setup RectTransform for the text container to fill the info container
            var textRect = textContainer.AddComponent<RectTransform>();
            LoggingSystem.Debug("CreateSongInfoDisplay: Added RectTransform to textContainer", "UI");
            
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 10f);  // Built-in padding
            textRect.offsetMax = new Vector2(-10f, -10f);
            LoggingSystem.Debug("CreateSongInfoDisplay: Set textContainer RectTransform properties", "UI");

            try 
            {
                songInfoText = textContainer.AddComponent<Text>();
                LoggingSystem.Debug($"CreateSongInfoDisplay: AddComponent<Text>() returned: {(songInfoText != null ? "Valid Text Component" : "NULL")}", "UI");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"CreateSongInfoDisplay: Exception during AddComponent<Text>(): {ex.Message}", "UI");
                LoggingSystem.Error($"CreateSongInfoDisplay: Exception stack trace: {ex.StackTrace}", "UI");
                throw;
            }
            
            if (songInfoText == null)
            {
                LoggingSystem.Error("CreateSongInfoDisplay: songInfoText is null after AddComponent!", "UI");
                LoggingSystem.Error($"CreateSongInfoDisplay: textContainer state when Text is null - Name: {textContainer?.name}, Active: {textContainer?.activeInHierarchy}", "UI");
                throw new System.Exception("Failed to add Text component");
            }
            
            songInfoText.text = "Enter a YouTube URL and click 'Get Song Info' to see details here.";
            LoggingSystem.Debug("CreateSongInfoDisplay: Set text content", "UI");
            
            try
            {
                FontHelper.SetSafeFont(songInfoText);
                LoggingSystem.Debug("CreateSongInfoDisplay: FontHelper.SetSafeFont completed", "UI");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"CreateSongInfoDisplay: FontHelper.SetSafeFont failed: {ex.Message}", "UI");
                // Continue without font, Unity will use default
            }
            
            songInfoText.fontSize = 12;
            LoggingSystem.Debug("CreateSongInfoDisplay: Set fontSize", "UI");
            
            songInfoText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            LoggingSystem.Debug("CreateSongInfoDisplay: Set text color", "UI");
            
            songInfoText.alignment = TextAnchor.UpperLeft;
            LoggingSystem.Debug("CreateSongInfoDisplay: Set text alignment", "UI");
            
            LoggingSystem.Debug("CreateSongInfoDisplay: Completed successfully", "UI");
        }

        private void CreateActionButtons(GameObject parent)
        {
            // Download Button
            var downloadObj = new GameObject("DownloadButton");
            downloadObj.transform.SetParent(parent.transform, false);
            
            var downloadRect = downloadObj.AddComponent<RectTransform>();
            downloadRect.anchorMin = new Vector2(0.1f, 0.22f);
            downloadRect.anchorMax = new Vector2(0.45f, 0.3f);
            downloadRect.offsetMin = Vector2.zero;
            downloadRect.offsetMax = Vector2.zero;

            var downloadImage = downloadObj.AddComponent<Image>();
            downloadImage.color = new Color(0.2f, 0.5f, 0.8f, 0.8f);

            downloadButton = downloadObj.AddComponent<Button>();
            downloadButton.targetGraphic = downloadImage;

            var downloadTextObj = new GameObject("Text");
            downloadTextObj.transform.SetParent(downloadObj.transform, false);
            var downloadTextRect = downloadTextObj.AddComponent<RectTransform>();
            downloadTextRect.anchorMin = Vector2.zero;
            downloadTextRect.anchorMax = Vector2.one;
            downloadTextRect.offsetMin = Vector2.zero;
            downloadTextRect.offsetMax = Vector2.zero;

            var downloadText = downloadTextObj.AddComponent<Text>();
            downloadText.text = "⬇️ Download";
            FontHelper.SetSafeFont(downloadText);
            downloadText.fontSize = 14;
            downloadText.color = Color.white;
            downloadText.alignment = TextAnchor.MiddleCenter;
            downloadText.fontStyle = FontStyle.Bold;

            downloadButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnDownloadButtonClicked);

            // Cancel Button
            var cancelObj = new GameObject("CancelButton");
            cancelObj.transform.SetParent(parent.transform, false);
            
            var cancelRect = cancelObj.AddComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0.55f, 0.22f);
            cancelRect.anchorMax = new Vector2(0.9f, 0.3f);
            cancelRect.offsetMin = Vector2.zero;
            cancelRect.offsetMax = Vector2.zero;

            var cancelImage = cancelObj.AddComponent<Image>();
            cancelImage.color = new Color(0.7f, 0.2f, 0.2f, 0.8f);

            cancelButton = cancelObj.AddComponent<Button>();
            cancelButton.targetGraphic = cancelImage;

            var cancelTextObj = new GameObject("Text");
            cancelTextObj.transform.SetParent(cancelObj.transform, false);
            var cancelTextRect = cancelTextObj.AddComponent<RectTransform>();
            cancelTextRect.anchorMin = Vector2.zero;
            cancelTextRect.anchorMax = Vector2.one;
            cancelTextRect.offsetMin = Vector2.zero;
            cancelTextRect.offsetMax = Vector2.zero;

            var cancelText = cancelTextObj.AddComponent<Text>();
            cancelText.text = "❌ Cancel";
            FontHelper.SetSafeFont(cancelText);
            cancelText.fontSize = 14;
            cancelText.color = Color.white;
            cancelText.alignment = TextAnchor.MiddleCenter;
            cancelText.fontStyle = FontStyle.Bold;

            cancelButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnCancelButtonClicked);
        }

        private void CreateStatusText(GameObject parent)
        {
            var statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(parent.transform, false);
            
            var statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 0.05f);
            statusRect.anchorMax = new Vector2(1f, 0.18f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;

            statusText = statusObj.AddComponent<Text>();
            statusText.text = "💡 Tip: Make sure the YouTube URL is valid and the video is accessible.";
            FontHelper.SetSafeFont(statusText);
            statusText.fontSize = 11;
            statusText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            statusText.alignment = TextAnchor.UpperCenter;
        }

        private void OnSearchButtonClicked()
        {
            if (isSearching) return;
            
            var url = searchBarInputField.text?.Trim();
            if (string.IsNullOrEmpty(url))
            {
                UpdateStatus("❌ Please enter a YouTube URL", Color.red);
                return;
            }

            if (!url.Contains("youtube.com") && !url.Contains("youtu.be"))
            {
                UpdateStatus("❌ Please enter a valid YouTube URL", Color.red);
                return;
            }

            LoggingSystem.Info("Search button clicked", "UI");
            LoggingSystem.Info("URL: " + url, "UI");
            
            isSearching = true;
            loadingDots = 0f; // Reset animation
            UpdateButtonStates();
            
            // Clear previous song info immediately
            if (songInfoText != null)
            {
                songInfoText.text = "Fetching song details...";
            }
            
            YoutubeHelper.GetSongDetails(url, OnSongDetailsReceived);
        }

        private void OnSongDetailsReceived(List<SongDetails> songDetails)
        {
            isSearching = false;
            UpdateButtonStates();
            
            try
            {
                if (songDetails == null || songDetails.Count == 0)
                {
                    UpdateStatus("❌ Failed to get song details. Check the URL and try again.", Color.red);
                    return;
                }

                LoggingSystem.Info("Song details: " + songDetails.Count + " songs found", "UI");
                currentSongDetails = songDetails[0];
                
                if (currentSongDetails == null)
                {
                    UpdateStatus("❌ Failed to parse song details", Color.red);
                    return;
                }

                var artist = currentSongDetails.GetArtist();
                var duration = currentSongDetails.GetFormattedDuration();
                
                var infoText = $"🎵 Title: {currentSongDetails.title}\n" +
                              $"🎤 Artist: {artist}\n" +
                              $"⏱️ Duration: {duration}\n" +
                              $"🆔 Video ID: {currentSongDetails.id}";
                
                songInfoText.text = infoText;
                UpdateStatus("✅ Song details loaded! Ready to download.", Color.green);
                
                LoggingSystem.Info($"Song details loaded: {currentSongDetails.title} by {artist}", "UI");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error parsing song details: {ex.Message}", "UI");
                UpdateStatus("❌ Error parsing song details", Color.red);
                currentSongDetails = null;
            }
        }

        private void OnDownloadButtonClicked()
        {
            if (isDownloading || currentSongDetails == null) return;
            
            var url = searchBarInputField.text?.Trim();
            if (string.IsNullOrEmpty(url))
            {
                UpdateStatus("❌ No URL to download", Color.red);
                return;
            }

            LoggingSystem.Info("Download button clicked", "UI");
            LoggingSystem.Info("Downloading URL: " + url, "UI");
            
            isDownloading = true;
            loadingDots = 0f; // Reset animation
            UpdateButtonStates();
            
            YoutubeHelper.DownloadSong(url, OnDownloadCompleted);
        }

        private void OnDownloadCompleted(string output)
        {
            isDownloading = false;
            UpdateButtonStates();
            
            LoggingSystem.Info("Download completed: " + output, "UI");
            
            if (!string.IsNullOrEmpty(output))
            {
                UpdateStatus("✅ Download completed! Reloading YouTube tracks...", Color.green);
                
                // Reload YouTube tracks to include the new download
                manager?.LoadTracksFromCurrentSource();
                
                // Close popup after successful download
                ClosePopup();
            }
            else
            {
                UpdateStatus("❌ Download failed. Check the logs for details.", Color.red);
            }
        }

        private void OnCancelButtonClicked()
        {
            LoggingSystem.Info("Cancel button clicked", "UI");
            ClosePopup();
        }

        private void ClosePopup()
        {
            if (popupContainer != null)
            {
                Destroy(popupContainer);
            }
            
            // Destroy this component
            Destroy(this);
        }

        private void UpdateButtonStates()
        {
            if (searchButton != null)
            {
                searchButton.interactable = !isSearching && !isDownloading;
            }
            
            if (downloadButton != null)
            {
                downloadButton.interactable = !isSearching && !isDownloading && currentSongDetails != null;
                var downloadImage = downloadButton.GetComponent<Image>();
                if (downloadImage != null)
                {
                    downloadImage.color = downloadButton.interactable ? 
                        new Color(0.2f, 0.5f, 0.8f, 0.8f) : 
                        new Color(0.3f, 0.3f, 0.3f, 0.5f);
                }
            }
            
            if (cancelButton != null)
            {
                cancelButton.interactable = !isDownloading;
            }
        }

        private void UpdateStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }
        }
    }
}