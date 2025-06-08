using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Utils;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Helpers;
using Newtonsoft.Json;
using System;
using BackSpeakerMod.Core.Features.Audio;
using System.Collections.Generic;
using System.Linq;
using BackSpeakerMod.Core.Modules;

namespace BackSpeakerMod.UI.Components
{
public class YouTubePopupComponent : MonoBehaviour
{
    private BackSpeakerManager manager;

        // UI Elements - stored as class members to avoid scope issues
        private GameObject popupContainer;
        private InputField searchBarInputField;
        private Button searchButton;
        private Button cancelButton;
        private Text statusText;
        
        // Song table elements
        private GameObject songTableContainer;
        private GameObject songTableContent;
        private ScrollRect songTableScrollRect;
        private List<GameObject> songRows = new List<GameObject>();
        
        // Current song details
        private List<SongDetails> currentSongDetails;
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
                // Song Info Display (Table)
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
            LoggingSystem.Debug("Creating song info table display", "UI");
            
            // Main container for the song table
            songTableContainer = new GameObject("SongTableContainer");
            songTableContainer.transform.SetParent(parent.transform, false);
            
            var tableRect = songTableContainer.AddComponent<RectTransform>();
            tableRect.anchorMin = new Vector2(0.05f, 0.25f);
            tableRect.anchorMax = new Vector2(0.95f, 0.55f);
            tableRect.offsetMin = Vector2.zero;
            tableRect.offsetMax = Vector2.zero;

            // Background for table
            var tableBg = songTableContainer.AddComponent<Image>();
            tableBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            // Create header row
            CreateTableHeader(songTableContainer);

            // Create scrollable content area
            CreateScrollableContent(songTableContainer);

            // Show placeholder initially
            ShowPlaceholder();
        }

        private void CreateTableHeader(GameObject parent)
        {
            var headerContainer = new GameObject("TableHeader");
            headerContainer.transform.SetParent(parent.transform, false);
            
            var headerRect = headerContainer.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 0.8f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            // Header background
            var headerBg = headerContainer.AddComponent<Image>();
            headerBg.color = new Color(0.25f, 0.25f, 0.25f, 1f);

            // Column headers
            CreateHeaderColumn(headerContainer, "🎵 Title", 0f, 0.4f);
            CreateHeaderColumn(headerContainer, "🎤 Artist", 0.4f, 0.65f);
            CreateHeaderColumn(headerContainer, "⏱️ Duration", 0.65f, 0.8f);
            CreateHeaderColumn(headerContainer, "➕/➖ Playlist", 0.8f, 1f);
        }

        private void CreateHeaderColumn(GameObject parent, string text, float xMin, float xMax)
        {
            var columnObj = new GameObject($"Header_{text}");
            columnObj.transform.SetParent(parent.transform, false);
            
            var columnRect = columnObj.AddComponent<RectTransform>();
            columnRect.anchorMin = new Vector2(xMin, 0f);
            columnRect.anchorMax = new Vector2(xMax, 1f);
            columnRect.offsetMin = new Vector2(5f, 0f);
            columnRect.offsetMax = new Vector2(-5f, 0f);

            var columnText = columnObj.AddComponent<Text>();
            columnText.text = text;
            FontHelper.SetSafeFont(columnText);
            columnText.fontSize = 11;
            columnText.color = Color.white;
            columnText.alignment = TextAnchor.MiddleLeft;
            columnText.fontStyle = FontStyle.Bold;
        }

        private void CreateScrollableContent(GameObject parent)
        {
            // Scrollable area
            var scrollContainer = new GameObject("ScrollContainer");
            scrollContainer.transform.SetParent(parent.transform, false);
            
            var scrollRect = scrollContainer.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 0.8f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;

            // Add ScrollRect component
            songTableScrollRect = scrollContainer.AddComponent<ScrollRect>();
            songTableScrollRect.horizontal = false;
            songTableScrollRect.vertical = true;

            // Create content container
            songTableContent = new GameObject("TableContent");
            songTableContent.transform.SetParent(scrollContainer.transform, false);
            
            var contentRect = songTableContent.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            // Add VerticalLayoutGroup for automatic row spacing
            var layoutGroup = songTableContent.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.spacing = 2f;

            // Add ContentSizeFitter for automatic sizing
            var sizeFitter = songTableContent.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            songTableScrollRect.content = contentRect;
        }

        private void ShowPlaceholder()
        {
            ClearSongRows();
            
            var placeholderRow = CreateSongRow();
            var placeholderText = placeholderRow.transform.Find("Title").GetComponent<Text>();
            placeholderText.text = "Enter a YouTube URL and click 'Get Song Info' to see songs here...";
            placeholderText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            placeholderText.fontStyle = FontStyle.Italic;
            
            // Hide other columns for placeholder
            placeholderRow.transform.Find("Artist").gameObject.SetActive(false);
            placeholderRow.transform.Find("Duration").gameObject.SetActive(false);
            placeholderRow.transform.Find("Actions").gameObject.SetActive(false);
        }

        private GameObject CreateSongRow()
        {
            var rowObj = new GameObject("SongRow");
            rowObj.transform.SetParent(songTableContent.transform, false);
            
            var rowRect = rowObj.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 30f); // Fixed height for rows

            // Row background (alternating colors can be added here)
            var rowBg = rowObj.AddComponent<Image>();
            rowBg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);

            // Create columns
            CreateRowColumn(rowObj, "Title", 0f, 0.4f);
            CreateRowColumn(rowObj, "Artist", 0.4f, 0.65f);
            CreateRowColumn(rowObj, "Duration", 0.65f, 0.8f);
            CreateActionsColumn(rowObj, 0.8f, 1f);

            songRows.Add(rowObj);
            return rowObj;
        }

        private void CreateRowColumn(GameObject parent, string columnName, float xMin, float xMax)
        {
            var columnObj = new GameObject(columnName);
            columnObj.transform.SetParent(parent.transform, false);
            
            var columnRect = columnObj.AddComponent<RectTransform>();
            columnRect.anchorMin = new Vector2(xMin, 0f);
            columnRect.anchorMax = new Vector2(xMax, 1f);
            columnRect.offsetMin = new Vector2(5f, 2f);
            columnRect.offsetMax = new Vector2(-5f, -2f);

            var columnText = columnObj.AddComponent<Text>();
            columnText.text = "";
            FontHelper.SetSafeFont(columnText);
            columnText.fontSize = 10;
            columnText.color = Color.white;
            columnText.alignment = TextAnchor.MiddleLeft;
            columnText.verticalOverflow = VerticalWrapMode.Truncate;
            columnText.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private void CreateActionsColumn(GameObject parent, float xMin, float xMax)
        {
            var columnObj = new GameObject("Actions");
            columnObj.transform.SetParent(parent.transform, false);
            
            var columnRect = columnObj.AddComponent<RectTransform>();
            columnRect.anchorMin = new Vector2(xMin, 0f);
            columnRect.anchorMax = new Vector2(xMax, 1f);
            columnRect.offsetMin = new Vector2(2f, 2f);
            columnRect.offsetMax = new Vector2(-2f, -2f);

            // Single Add/Remove button that fills the entire column
            var toggleBtn = columnObj.AddComponent<Button>();
            var btnImage = columnObj.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.8f, 0.2f, 0.8f); // Default to green (Add)
            toggleBtn.targetGraphic = btnImage;

            var btnText = new GameObject("Text");
            btnText.transform.SetParent(columnObj.transform, false);
            var btnTextRect = btnText.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            var btnTextComponent = btnText.AddComponent<Text>();
            btnTextComponent.text = "➕";
            FontHelper.SetSafeFont(btnTextComponent);
            btnTextComponent.fontSize = 12;
            btnTextComponent.color = Color.white;
            btnTextComponent.alignment = TextAnchor.MiddleCenter;
        }

        private void ClearSongRows()
        {
            foreach (var row in songRows)
            {
                if (row != null)
                    Destroy(row);
            }
            songRows.Clear();
        }

        private void PopulateSongTable(List<SongDetails> songs)
        {
            ClearSongRows();
            
            if (songs == null || songs.Count == 0)
            {
                ShowPlaceholder();
                return;
            }

            foreach (var song in songs)
            {
                var row = CreateSongRow();
                
                // Populate columns
                var titleText = row.transform.Find("Title").GetComponent<Text>();
                titleText.text = song.title ?? "Unknown Title";
                
                var artistText = row.transform.Find("Artist").GetComponent<Text>();
                artistText.text = song.GetArtist();
                
                var durationText = row.transform.Find("Duration").GetComponent<Text>();
                durationText.text = song.GetFormattedDuration();
                
                // Setup toggle button for this specific song
                var actionsColumn = row.transform.Find("Actions");
                var toggleBtn = actionsColumn.GetComponent<Button>();
                var btnImage = actionsColumn.GetComponent<Image>();
                var btnText = actionsColumn.transform.Find("Text").GetComponent<Text>();
                
                var songUrl = song.url; // Capture for closure
                var songTitle = song.title ?? "Unknown Title";
                var songArtist = song.GetArtist();
                
                // Check if song is already in playlist and update button appearance
                bool inPlaylist = manager?.ContainsYouTubeSong(songUrl) ?? false;
                if (inPlaylist)
                {
                    btnText.text = "➖";
                    btnImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f); // Red for remove
                }
                else
                {
                    btnText.text = "➕";
                    btnImage.color = new Color(0.2f, 0.8f, 0.2f, 0.8f); // Green for add
                }
                
                toggleBtn.onClick.AddListener((UnityEngine.Events.UnityAction)(() => OnSongTogglePlaylistClicked(songUrl, songTitle, songArtist)));
            }
        }

        private void OnSongTogglePlaylistClicked(string songUrl, string songTitle, string songArtist)
        {
            LoggingSystem.Info($"Toggle playlist button clicked for song: {songTitle} by {songArtist}", "UI");
            
            try
            {
                bool inPlaylist = manager?.ContainsYouTubeSong(songUrl) ?? false;
                
                if (inPlaylist)
                {
                    // Remove from playlist
                    UpdateStatus($"🎵 Removing '{songTitle}' from playlist...", Color.yellow);
                    
                    bool removed = manager?.RemoveYouTubeSong(songUrl) ?? false;
                    if (removed)
                    {
                        UpdateStatus($"✅ Removed '{songTitle}' from YouTube playlist!", Color.green);
                        LoggingSystem.Info($"Successfully removed '{songTitle}' from YouTube playlist", "UI");
                        
                        // Switch to YouTube tab to show the updated playlist
                        manager?.SetMusicSource(MusicSourceType.YouTube);
                        
                        // Update the button in the table
                        RefreshSongButtonStates();
                    }
                    else
                    {
                        UpdateStatus($"❌ Failed to remove '{songTitle}' from playlist", Color.red);
                    }
                }
                else
                {
                    // Add to playlist
                    UpdateStatus($"🎵 Adding '{songTitle}' to playlist...", Color.yellow);
                    
                    // Create SongDetails from the current data
                    var songDetails = currentSongDetails?.FirstOrDefault(s => s.url == songUrl);
                    if (songDetails != null)
                    {
                        bool added = manager?.AddYouTubeSong(songDetails) ?? false;
                        if (added)
                        {
                            UpdateStatus($"✅ Added '{songTitle}' to YouTube playlist!", Color.green);
                            LoggingSystem.Info($"Successfully added '{songTitle}' to YouTube playlist", "UI");
                            
                            // Switch to YouTube tab to show the added song
                            manager?.SetMusicSource(MusicSourceType.YouTube);
                            
                            // Update the button in the table
                            RefreshSongButtonStates();
                        }
                        else
                        {
                            UpdateStatus($"❌ Failed to add '{songTitle}' to playlist (may already exist)", Color.red);
                        }
                    }
                    else
                    {
                        UpdateStatus($"❌ Song details not found for '{songTitle}'", Color.red);
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Error toggling '{songTitle}' in playlist", Color.red);
                LoggingSystem.Error($"Error toggling song in playlist: {ex.Message}", "UI");
            }
        }

        private void RefreshSongButtonStates()
        {
            // Update all song buttons to reflect current playlist state
            foreach (var row in songRows)
            {
                if (row == null) continue;
                
                var actionsColumn = row.transform.Find("Actions");
                if (actionsColumn == null) continue;
                
                var btnImage = actionsColumn.GetComponent<Image>();
                var btnText = actionsColumn.transform.Find("Text")?.GetComponent<Text>();
                
                if (btnImage == null || btnText == null) continue;
                
                // Find the corresponding song in currentSongDetails
                var titleText = row.transform.Find("Title")?.GetComponent<Text>();
                if (titleText == null) continue;
                
                var matchingSong = currentSongDetails?.FirstOrDefault(s => (s.title ?? "Unknown Title") == titleText.text);
                if (matchingSong != null)
                {
                    bool inPlaylist = manager?.ContainsYouTubeSong(matchingSong.url) ?? false;
                    if (inPlaylist)
                    {
                        btnText.text = "➖";
                        btnImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f); // Red for remove
                    }
                    else
                    {
                        btnText.text = "➕";
                        btnImage.color = new Color(0.2f, 0.8f, 0.2f, 0.8f); // Green for add
                    }
                }
            }
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
            
            // Show loading in table
            ShowPlaceholder();
            var placeholderText = songRows[0].transform.Find("Title").GetComponent<Text>();
            placeholderText.text = "Fetching song details...";
            placeholderText.color = Color.yellow;
            
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
                    ShowPlaceholder();
                    return;
                }

                LoggingSystem.Info($"Song details: {songDetails.Count} songs found", "UI");
                currentSongDetails = songDetails;
                
                // Populate the table with songs
                PopulateSongTable(songDetails);
                
                var statusMessage = songDetails.Count == 1 ? 
                    "✅ Song details loaded! Click toggle buttons to add/remove songs." :
                    $"✅ Found {songDetails.Count} songs! Click toggle buttons to add/remove songs from playlist.";
                    
                UpdateStatus(statusMessage, Color.green);
                
                LoggingSystem.Info($"Populated song table with {songDetails.Count} songs", "UI");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error processing song details: {ex.Message}", "UI");
                UpdateStatus("❌ Error processing song details", Color.red);
                ShowPlaceholder();
                currentSongDetails = null;
            }
        }

        private void OnDownloadAllButtonClicked()
        {
            // This method is no longer used since we're using playlist streaming instead of downloads
            UpdateStatus("❌ Download functionality has been replaced with playlist streaming", Color.red);
        }

        private void OnDownloadCompleted(string output)
        {
            // This method is no longer used since we're using playlist streaming instead of downloads
            isDownloading = false;
            UpdateButtonStates();
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

        private void CreateActionButtons(GameObject parent)
        {
            // Add All to Playlist Button
            var addAllObj = new GameObject("AddAllButton");
            addAllObj.transform.SetParent(parent.transform, false);
            
            var addAllRect = addAllObj.AddComponent<RectTransform>();
            addAllRect.anchorMin = new Vector2(0.1f, 0.15f);
            addAllRect.anchorMax = new Vector2(0.55f, 0.22f);
            addAllRect.offsetMin = Vector2.zero;
            addAllRect.offsetMax = Vector2.zero;

            var addAllImage = addAllObj.AddComponent<Image>();
            addAllImage.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);

            var addAllButton = addAllObj.AddComponent<Button>();
            addAllButton.targetGraphic = addAllImage;

            var addAllTextObj = new GameObject("Text");
            addAllTextObj.transform.SetParent(addAllObj.transform, false);
            var addAllTextRect = addAllTextObj.AddComponent<RectTransform>();
            addAllTextRect.anchorMin = Vector2.zero;
            addAllTextRect.anchorMax = Vector2.one;
            addAllTextRect.offsetMin = Vector2.zero;
            addAllTextRect.offsetMax = Vector2.zero;

            var addAllText = addAllTextObj.AddComponent<Text>();
            addAllText.text = "➕ Add All to Playlist";
            FontHelper.SetSafeFont(addAllText);
            addAllText.fontSize = 12;
            addAllText.color = Color.white;
            addAllText.alignment = TextAnchor.MiddleCenter;
            addAllText.fontStyle = FontStyle.Bold;

            addAllButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnAddAllToPlaylistButtonClicked);

            // Cancel Button
            var cancelObj = new GameObject("CancelButton");
            cancelObj.transform.SetParent(parent.transform, false);
            
            var cancelRect = cancelObj.AddComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0.6f, 0.15f);
            cancelRect.anchorMax = new Vector2(0.9f, 0.22f);
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
            cancelText.fontSize = 12;
            cancelText.color = Color.white;
            cancelText.alignment = TextAnchor.MiddleCenter;
            cancelText.fontStyle = FontStyle.Bold;

            cancelButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnCancelButtonClicked);
        }

        private void OnAddAllToPlaylistButtonClicked()
        {
            if (currentSongDetails == null || currentSongDetails.Count == 0)
            {
                UpdateStatus("❌ No songs to add to playlist", Color.red);
                return;
            }

            LoggingSystem.Info($"Add All to Playlist button clicked - {currentSongDetails.Count} songs", "UI");
            
            try
            {
                UpdateStatus($"🎵 Adding {currentSongDetails.Count} songs to playlist...", Color.yellow);
                
                int successCount = 0;
                int skipCount = 0;
                
                foreach (var song in currentSongDetails)
                {
                    bool added = manager?.AddYouTubeSong(song) ?? false;
                    if (added)
                    {
                        successCount++;
                        LoggingSystem.Info($"Added '{song.title}' to YouTube playlist ({successCount}/{currentSongDetails.Count})", "UI");
                    }
                    else
                    {
                        skipCount++;
                        LoggingSystem.Info($"Skipped '{song.title}' (already in playlist)", "UI");
                    }
                }
                
                if (successCount > 0)
                {
                    // Switch to YouTube tab to show the added songs
                    manager?.SetMusicSource(MusicSourceType.YouTube);
                    
                    var statusMessage = skipCount > 0 ? 
                        $"✅ Added {successCount} songs to playlist ({skipCount} already existed)" :
                        $"✅ Added all {successCount} songs to YouTube playlist!";
                    UpdateStatus(statusMessage, Color.green);
                    
                    // Update button states in the table
                    RefreshSongButtonStates();
                }
                else
                {
                    UpdateStatus("ℹ️ All songs are already in the playlist", Color.yellow);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("❌ Error adding songs to playlist", Color.red);
                LoggingSystem.Error($"Error in AddAllToPlaylist: {ex.Message}", "UI");
            }
        }

        private void CreateStatusText(GameObject parent)
        {
            var statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(parent.transform, false);
            
            var statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 0.05f);
            statusRect.anchorMax = new Vector2(1f, 0.12f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;

            statusText = statusObj.AddComponent<Text>();
            statusText.text = "💡 Tip: Enter a YouTube URL or playlist link to add songs to your YouTube playlist.";
            FontHelper.SetSafeFont(statusText);
            statusText.fontSize = 11;
            statusText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            statusText.alignment = TextAnchor.UpperCenter;
        }
    }
}