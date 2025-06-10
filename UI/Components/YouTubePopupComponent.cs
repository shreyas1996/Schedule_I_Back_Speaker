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
using Il2CppCollections = Il2CppSystem.Collections.Generic;

namespace BackSpeakerMod.UI.Components
{
public class YouTubePopupComponent : MonoBehaviour
{
    private BackSpeakerManager? manager;

        // UI Elements - stored as class members to avoid scope issues
        private GameObject? popupContainer;
        private InputField? searchBarInputField;
        private Button? searchButton;
        private Button? cancelButton;
        private Text? statusText;
        
        // Song table elements
        private GameObject? songTableContainer;
        private GameObject? songTableContent;
        private ScrollRect? songTableScrollRect;
        private List<GameObject> songRows = new List<GameObject>();
        
        // Playlist management reference
        private PlaylistToggleComponent? playlistToggleComponent;
        
        // Playlist selection UI
        private Dropdown? targetPlaylistDropdown;
        private InputField? newPlaylistNameInput;
        private Button? createNewPlaylistButton;
        
        // Current song details
        private List<SongDetails> currentSongDetails = new List<SongDetails>();
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
        
        // Find the PlaylistToggleComponent in the parent hierarchy
        playlistToggleComponent = this.transform.GetComponentInParent<PlaylistToggleComponent>();
        if (playlistToggleComponent == null)
        {
            // Try to find it in the same parent
            playlistToggleComponent = this.transform.parent?.GetComponentInChildren<PlaylistToggleComponent>();
        }
        
        if (playlistToggleComponent != null)
        {
            LoggingSystem.Info("Found PlaylistToggleComponent for YouTube popup integration", "UI");
        }
        else
        {
            LoggingSystem.Warning("PlaylistToggleComponent not found - playlist functionality may not work", "UI");
        }
    }

    /// <summary>
    /// Refresh the target playlist dropdown (called from PlaylistToggleComponent when playlists change)
    /// </summary>
    public void RefreshTargetPlaylistDropdown()
    {
        if (targetPlaylistDropdown != null)
        {
            UpdateTargetPlaylistDropdown();
        }
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
                // Playlist Selection
                CreatePlaylistSelection(contentArea);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating playlist selection: {ex.Message}", "UI");
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

        private void CreatePlaylistSelection(GameObject parent)
        {
            var playlistContainer = new GameObject("PlaylistSelection");
            playlistContainer.transform.SetParent(parent.transform, false);
            
            var playlistRect = playlistContainer.AddComponent<RectTransform>();
            playlistRect.anchorMin = new Vector2(0f, 0.78f);
            playlistRect.anchorMax = new Vector2(1f, 0.85f);
            playlistRect.offsetMin = new Vector2(10f, 0f);
            playlistRect.offsetMax = new Vector2(-10f, 0f);
            
            // Section label
            var labelObj = new GameObject("PlaylistLabel");
            labelObj.transform.SetParent(playlistContainer.transform, false);
            
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.6f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            var labelText = labelObj.AddComponent<Text>();
            labelText.text = "📋 Add to Playlist:";
            FontHelper.SetSafeFont(labelText);
            labelText.fontSize = 12;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.fontStyle = FontStyle.Bold;
            
            // Playlist dropdown (60% width)
            CreateTargetPlaylistDropdown(playlistContainer);
            
            // New playlist creation (40% width)
            CreateNewPlaylistSection(playlistContainer);
            
            // Update the dropdown with current playlists
            UpdateTargetPlaylistDropdown();
        }
        
        private void CreateTargetPlaylistDropdown(GameObject parent)
        {
            var dropdownObj = new GameObject("TargetPlaylistDropdown");
            dropdownObj.transform.SetParent(parent.transform, false);
            
            var dropdownRect = dropdownObj.AddComponent<RectTransform>();
            dropdownRect.anchorMin = new Vector2(0f, 0f);
            dropdownRect.anchorMax = new Vector2(0.6f, 0.6f);
            dropdownRect.offsetMin = new Vector2(0f, 0f);
            dropdownRect.offsetMax = new Vector2(-5f, 0f);
            
            // Background
            var dropdownBg = dropdownObj.AddComponent<Image>();
            dropdownBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            targetPlaylistDropdown = dropdownObj.AddComponent<Dropdown>();
            
            // Create dropdown label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(dropdownObj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.05f, 0f);
            labelRect.anchorMax = new Vector2(0.85f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            var labelText = labelObj.AddComponent<Text>();
            labelText.text = "Select Playlist...";
            FontHelper.SetSafeFont(labelText);
            labelText.fontSize = 11;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
            
            targetPlaylistDropdown.captionText = labelText;
            targetPlaylistDropdown.targetGraphic = dropdownBg;
        }
        
        private void CreateNewPlaylistSection(GameObject parent)
        {
            var newPlaylistContainer = new GameObject("NewPlaylistSection");
            newPlaylistContainer.transform.SetParent(parent.transform, false);
            
            var containerRect = newPlaylistContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.62f, 0f);
            containerRect.anchorMax = new Vector2(1f, 0.6f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            // New playlist name input (70% width)
            var inputObj = new GameObject("NewPlaylistInput");
            inputObj.transform.SetParent(newPlaylistContainer.transform, false);
            
            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0f, 0f);
            inputRect.anchorMax = new Vector2(0.7f, 1f);
            inputRect.offsetMin = new Vector2(0f, 0f);
            inputRect.offsetMax = new Vector2(-2f, 0f);
            
            // Input background
            var inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            
            // Create input text component
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5f, 0f);
            textRect.offsetMax = new Vector2(-5f, 0f);
            
            var textComponent = textObj.AddComponent<Text>();
            FontHelper.SetSafeFont(textComponent);
            textComponent.fontSize = 10;
            textComponent.color = Color.white;
            textComponent.supportRichText = false;
            
            // Create placeholder
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputObj.transform, false);
            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(5f, 0f);
            placeholderRect.offsetMax = new Vector2(-5f, 0f);
            
            var placeholderText = placeholderObj.AddComponent<Text>();
            placeholderText.text = "New playlist name...";
            FontHelper.SetSafeFont(placeholderText);
            placeholderText.fontSize = 10;
            placeholderText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            placeholderText.fontStyle = FontStyle.Italic;
            
            // Create InputField
            newPlaylistNameInput = inputObj.AddComponent<InputField>();
            newPlaylistNameInput.targetGraphic = inputBg;
            newPlaylistNameInput.textComponent = textComponent;
            newPlaylistNameInput.placeholder = placeholderText;
            newPlaylistNameInput.characterLimit = 50;
            
            // Create button (30% width)
            var buttonObj = new GameObject("CreateButton");
            buttonObj.transform.SetParent(newPlaylistContainer.transform, false);
            
            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.72f, 0f);
            buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            createNewPlaylistButton = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
            
            var buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            var buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            var buttonText = buttonTextObj.AddComponent<Text>();
            buttonText.text = "Create";
            FontHelper.SetSafeFont(buttonText);
            buttonText.fontSize = 9;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.fontStyle = FontStyle.Bold;
            
            createNewPlaylistButton.targetGraphic = buttonImage;
            createNewPlaylistButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnCreateNewPlaylistClicked);
        }
        
        private void UpdateTargetPlaylistDropdown()
        {
            if (targetPlaylistDropdown == null || playlistToggleComponent == null) return;
            
            targetPlaylistDropdown.ClearOptions();
            
            var availablePlaylists = YouTubePlaylistManager.GetAllPlaylists();
            var options = new Il2CppCollections.List<Dropdown.OptionData>();
            
            foreach (var playlist in availablePlaylists)
            {
                options.Add(new Dropdown.OptionData($"{playlist.name} ({playlist.downloadedCount}/{playlist.songCount})"));
            }
            
            targetPlaylistDropdown.AddOptions(options);
            
            // Select the current playlist if available
            var currentPlaylist = playlistToggleComponent.GetCurrentYouTubePlaylist();
            if (currentPlaylist != null)
            {
                for (int i = 0; i < availablePlaylists.Count; i++)
                {
                    if (availablePlaylists[i].id == currentPlaylist.id)
                    {
                        targetPlaylistDropdown.value = i;
                        break;
                    }
                }
            }
        }
        
        private void OnCreateNewPlaylistClicked()
        {
            if (newPlaylistNameInput == null) return;
            
            var playlistName = newPlaylistNameInput.text?.Trim();
            if (string.IsNullOrEmpty(playlistName))
            {
                UpdateStatus("❌ Please enter a playlist name", Color.red);
                return;
            }
            
            try
            {
                var newPlaylist = YouTubePlaylistManager.CreatePlaylist(playlistName, "Created from YouTube search");
                if (newPlaylist != null)
                {
                    UpdateStatus($"✅ Created new playlist: {playlistName}", Color.green);
                    newPlaylistNameInput.text = "";
                    
                    // Update the dropdown and select the new playlist
                    UpdateTargetPlaylistDropdown();
                    
                    // Set the new playlist as selected
                    var playlists = YouTubePlaylistManager.GetAllPlaylists();
                    for (int i = 0; i < playlists.Count; i++)
                    {
                        if (playlists[i].id == newPlaylist.id)
                        {
                            targetPlaylistDropdown.value = i;
                            break;
                        }
                    }
                }
                else
                {
                    UpdateStatus("❌ Failed to create playlist", Color.red);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("❌ Error creating playlist", Color.red);
                LoggingSystem.Error($"Error creating new playlist: {ex.Message}", "UI");
            }
        }

        private void CreateUrlInput(GameObject parent)
        {
            var searchContainer = new GameObject("SearchContainer");
            searchContainer.transform.SetParent(parent.transform, false);
            
            var searchRect = searchContainer.AddComponent<RectTransform>();
            searchRect.anchorMin = new Vector2(0f, 0.70f);  // Moved down further to accommodate playlist selection
            searchRect.anchorMax = new Vector2(1f, 0.78f); // Moved down from 0.78f
            searchRect.offsetMin = new Vector2(10f, 0f);
            searchRect.offsetMax = new Vector2(-10f, 0f);

            // Background for input
            var inputBg = searchContainer.AddComponent<Image>();
            inputBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Create the text component for InputField first
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(searchContainer.transform, false);
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
            placeholderObj.transform.SetParent(searchContainer.transform, false);
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
            searchBarInputField = searchContainer.AddComponent<InputField>();
            searchBarInputField.targetGraphic = inputBg;
            searchBarInputField.textComponent = textComponent;
            searchBarInputField.placeholder = placeholderText;
            searchBarInputField.characterLimit = 500;
        }

        private void CreateSearchButton(GameObject parent)
        {
            var searchButtonObj = new GameObject("SearchButton");
            searchButtonObj.transform.SetParent(parent.transform, false);
            
            var buttonRect = searchButtonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0f, 0.62f);  // Moved down further
            buttonRect.anchorMax = new Vector2(1f, 0.70f);   // Moved down
            buttonRect.offsetMin = new Vector2(10f, 0f);
            buttonRect.offsetMax = new Vector2(-10f, 0f);

            var buttonImage = searchButtonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.7f, 0.2f, 0.8f);

            searchButton = searchButtonObj.AddComponent<Button>();
            searchButton.targetGraphic = buttonImage;

            var buttonText = new GameObject("Text");
            buttonText.transform.SetParent(searchButtonObj.transform, false);
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
            songTableContainer = new GameObject("SongTable");
            songTableContainer.transform.SetParent(parent.transform, false);
            
            var tableRect = songTableContainer.AddComponent<RectTransform>();
            tableRect.anchorMin = new Vector2(0f, 0.25f);  // Keep same
            tableRect.anchorMax = new Vector2(1f, 0.62f);  // Adjusted to new search button position
            tableRect.offsetMin = new Vector2(10f, 0f);
            tableRect.offsetMax = new Vector2(-10f, 0f);

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
            CreateHeaderColumn(headerContainer, "🎵 Title", 0f, 0.35f);
            CreateHeaderColumn(headerContainer, "🎤 Artist", 0.35f, 0.55f);
            CreateHeaderColumn(headerContainer, "⏱️ Duration", 0.55f, 0.7f);
            CreateHeaderColumn(headerContainer, "📥 Status", 0.7f, 0.85f);
            CreateHeaderColumn(headerContainer, "➕/➖", 0.85f, 1f);
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

            // Add Viewport with Mask
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollContainer.transform, false);
            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(0, 0, 0, 0.01f); // Transparent but needed for Mask
            var viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            // Create content container
            songTableContent = new GameObject("TableContent");
            songTableContent.transform.SetParent(viewport.transform, false);
            
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
            // layoutGroup.padding = new RectOffset(10, 10, 10, 10);

            // Add ContentSizeFitter for automatic sizing
            var sizeFitter = songTableContent.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            songTableScrollRect.content = contentRect;
            songTableScrollRect.viewport = viewportRect;
            songTableScrollRect.vertical = true;
            songTableScrollRect.horizontal = false;
            songTableScrollRect.movementType = ScrollRect.MovementType.Clamped;

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
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
            placeholderRow.transform.Find("Status").gameObject.SetActive(false);
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
            CreateRowColumn(rowObj, "Title", 0f, 0.35f);
            CreateRowColumn(rowObj, "Artist", 0.35f, 0.55f);
            CreateRowColumn(rowObj, "Duration", 0.55f, 0.7f);
            CreateRowColumn(rowObj, "Status", 0.7f, 0.85f);
            CreateActionsColumn(rowObj, 0.85f, 1f);

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
            btnTextComponent.text = "Add";
            FontHelper.SetSafeFont(btnTextComponent);
            btnTextComponent.fontSize = 12;
            btnTextComponent.color = Color.white;
            btnTextComponent.alignment = TextAnchor.MiddleCenter;
        }

        private void ClearSongRows()
        {
            try
            {
                foreach (var row in songRows)
                {
                    if (row != null)
                    {
                        // Clear any button listeners to prevent memory leaks
                        var buttons = row.GetComponentsInChildren<Button>();
                        foreach (var button in buttons)
                        {
                            button?.onClick?.RemoveAllListeners();
                        }
                        
                        // Destroy the GameObject
                        Destroy(row);
                    }
                }
                songRows.Clear();
                
                LoggingSystem.Debug($"Cleared {songRows.Count} song rows from UI", "UI");
            }
            catch (Exception ex)
            {
                LoggingSystem.Warning($"Error clearing song rows: {ex.Message}", "UI");
                // Try to clear the list anyway
                songRows.Clear();
            }
        }

        private void PopulateSongTable(List<SongDetails> songs)
        {
            try
            {
                // Clear existing rows to prevent memory leaks
                ClearSongRows();
                
                if (songs == null || songs.Count == 0)
                {
                    ShowPlaceholder();
                    return;
                }

                // Limit the number of songs to prevent UI overload (MacBook Air safety)
                const int maxSongsToDisplay = 50;
                if (songs.Count > maxSongsToDisplay)
                {
                    LoggingSystem.Warning($"Limiting display to {maxSongsToDisplay} songs (of {songs.Count} total) to prevent UI overload", "UI");
                    songs = songs.Take(maxSongsToDisplay).ToList();
                }

                foreach (var song in songs)
                {
                    try
                    {
                        var row = CreateSongRow();
                        if (row == null) continue;
                        
                        // Populate columns with null safety
                        var titleText = row.transform.Find("Title")?.GetComponent<Text>();
                        if (titleText != null)
                            titleText.text = song.title ?? "Unknown Title";
                        
                        var artistText = row.transform.Find("Artist")?.GetComponent<Text>();
                        if (artistText != null)
                            artistText.text = song.GetArtist();
                        
                        var durationText = row.transform.Find("Duration")?.GetComponent<Text>();
                        if (durationText != null)
                            durationText.text = song.GetFormattedDuration();
                        
                        // Populate status column
                        var statusText = row.transform.Find("Status")?.GetComponent<Text>();
                        if (statusText != null)
                        {
                            statusText.text = song.GetDownloadStatus();
                            
                            // Set status color based on download state
                            if (song.downloadFailed)
                            {
                                statusText.color = new Color(1f, 0.3f, 0.3f, 1f); // Red for failed
                            }
                            else if (song.isDownloaded)
                            {
                                statusText.color = new Color(0.3f, 1f, 0.3f, 1f); // Green for downloaded
                            }
                            else if (song.isDownloading)
                            {
                                statusText.color = new Color(1f, 1f, 0.3f, 1f); // Yellow for downloading
                            }
                            else
                            {
                                statusText.color = new Color(0.7f, 0.7f, 0.7f, 1f); // Gray for pending
                            }
                        }
                        
                        // Setup toggle button for this specific song
                        var actionsColumn = row.transform.Find("Actions");
                        if (actionsColumn != null)
                        {
                            var toggleBtn = actionsColumn.GetComponent<Button>();
                            var btnImage = actionsColumn.GetComponent<Image>();
                            var btnText = actionsColumn.transform.Find("Text")?.GetComponent<Text>();
                            
                            if (toggleBtn != null && btnImage != null && btnText != null)
                            {
                                var songUrl = song.url; // Capture for closure
                                var songTitle = song.title ?? "Unknown Title";
                                var songArtist = song.GetArtist();
                                
                                // Check if song is already in playlist and update button appearance
                                YouTubePlaylist? targetPlaylist = null;
                                
                                if (targetPlaylistDropdown != null && targetPlaylistDropdown.value >= 0)
                                {
                                    var availablePlaylists = YouTubePlaylistManager.GetAllPlaylists();
                                    if (targetPlaylistDropdown.value < availablePlaylists.Count)
                                    {
                                        var selectedPlaylistInfo = availablePlaylists[targetPlaylistDropdown.value];
                                        targetPlaylist = YouTubePlaylistManager.LoadPlaylist(selectedPlaylistInfo.id);
                                    }
                                }
                                
                                // Fall back to current playlist if no target selected
                                if (targetPlaylist == null)
                                {
                                    targetPlaylist = playlistToggleComponent?.GetCurrentYouTubePlaylist();
                                }
                                
                                bool inPlaylist = targetPlaylist?.ContainsSong(song.GetVideoId()) ?? false;
                                
                                if (inPlaylist)
                                {
                                    btnText.text = "Remove";
                                    btnImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f); // Red for remove
                                }
                                else
                                {
                                    btnText.text = "Add";
                                    btnImage.color = new Color(0.2f, 0.8f, 0.2f, 0.8f); // Green for add
                                }
                                
                                // Clear any existing listeners to prevent memory leaks
                                toggleBtn.onClick.RemoveAllListeners();
                                toggleBtn.onClick.AddListener((UnityEngine.Events.UnityAction)(() => OnSongTogglePlaylistClicked(songUrl, songTitle, songArtist)));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingSystem.Warning($"Error creating UI row for song '{song.title}': {ex.Message}", "UI");
                        // Continue with other songs
                    }
                }
                
                LoggingSystem.Debug($"Successfully populated song table with {songs.Count} songs", "UI");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Critical error in PopulateSongTable: {ex.Message}", "UI");
                LoggingSystem.Error($"Stack trace: {ex.StackTrace}", "UI");
                
                // Show placeholder on error
                try
                {
                    ClearSongRows();
                    ShowPlaceholder();
                }
                catch
                {
                    // Silent fallback
                }
            }
        }

        private void OnSongTogglePlaylistClicked(string songUrl, string songTitle, string songArtist)
        {
            LoggingSystem.Info($"Toggle playlist button clicked for song: {songTitle} by {songArtist}", "UI");
            
            try
            {
                if (playlistToggleComponent == null)
                {
                    UpdateStatus("❌ Playlist component not available", Color.red);
                    return;
                }
                
                // Get the target playlist from the dropdown selection
                YouTubePlaylist? targetPlaylist = null;
                
                if (targetPlaylistDropdown != null && targetPlaylistDropdown.value >= 0)
                {
                    var availablePlaylists = YouTubePlaylistManager.GetAllPlaylists();
                    if (targetPlaylistDropdown.value < availablePlaylists.Count)
                    {
                        var selectedPlaylistInfo = availablePlaylists[targetPlaylistDropdown.value];
                        targetPlaylist = YouTubePlaylistManager.LoadPlaylist(selectedPlaylistInfo.id);
                    }
                }
                
                // Fall back to current playlist if no target selected
                if (targetPlaylist == null)
                {
                    targetPlaylist = playlistToggleComponent.GetCurrentYouTubePlaylist();
                }
                
                if (targetPlaylist == null)
                {
                    UpdateStatus("❌ No playlist selected. Please select a playlist or create a new one.", Color.red);
                    return;
                }
                
                // Find the song details
                var songDetails = currentSongDetails?.FirstOrDefault(s => s.url == songUrl);
                if (songDetails == null)
                {
                    UpdateStatus($"❌ Song details not found for '{songTitle}'", Color.red);
                    return;
                }
                
                var videoId = songDetails.GetVideoId();
                bool inPlaylist = targetPlaylist.ContainsSong(videoId);
                
                if (inPlaylist)
                {
                    // Remove from playlist
                    UpdateStatus($"🎵 Removing '{songTitle}' from playlist...", Color.yellow);
                    
                    if (targetPlaylist.RemoveSong(videoId))
                    {
                        YouTubePlaylistManager.SavePlaylist(targetPlaylist);
                        UpdateStatus($"✅ Removed '{songTitle}' from playlist '{targetPlaylist.name}'!", Color.green);
                        LoggingSystem.Info($"Successfully removed '{songTitle}' from playlist", "UI");
                        
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
                    
                    if (targetPlaylist.AddSong(songDetails))
                    {
                        YouTubePlaylistManager.SavePlaylist(targetPlaylist);
                        UpdateStatus($"✅ Added '{songTitle}' to playlist '{targetPlaylist.name}'!", Color.green);
                        LoggingSystem.Info($"Successfully added '{songTitle}' to playlist", "UI");
                        
                        // Update the button in the table
                        RefreshSongButtonStates();
                    }
                    else
                    {
                        UpdateStatus($"❌ Failed to add '{songTitle}' to playlist (may already exist)", Color.red);
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
            // Get the target playlist from dropdown selection
            YouTubePlaylist? targetPlaylist = null;
            
            if (targetPlaylistDropdown != null && targetPlaylistDropdown.value >= 0)
            {
                var availablePlaylists = YouTubePlaylistManager.GetAllPlaylists();
                if (targetPlaylistDropdown.value < availablePlaylists.Count)
                {
                    var selectedPlaylistInfo = availablePlaylists[targetPlaylistDropdown.value];
                    targetPlaylist = YouTubePlaylistManager.LoadPlaylist(selectedPlaylistInfo.id);
                }
            }
            
            // Fall back to current playlist if no target selected
            if (targetPlaylist == null)
            {
                targetPlaylist = playlistToggleComponent?.GetCurrentYouTubePlaylist();
            }
            
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
                    bool inPlaylist = targetPlaylist?.ContainsSong(matchingSong.GetVideoId()) ?? false;
                    if (inPlaylist)
                    {
                        btnText.text = "Remove";
                        btnImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f); // Red for remove
                    }
                    else
                    {
                        btnText.text = "Add";
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
            // With MelonCoroutines, callbacks are already on the main thread
            if (!UnityEngine.Application.isPlaying)
            {
                LoggingSystem.Warning("OnSongDetailsReceived called when application not playing", "UI");
                return;
            }
            
            // Direct call since we're now using Unity-safe coroutines throughout
            ProcessSongDetailsOnMainThread(songDetails);
        }
        
        private void ProcessSongDetailsOnMainThread(List<SongDetails> songDetails)
        {
            // This method runs on the main thread and is safe for UI operations
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
                
                // Populate the table with songs (safe on main thread)
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
                LoggingSystem.Error($"Stack trace: {ex.StackTrace}", "UI");
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
            try
            {
                LoggingSystem.Debug("Closing YouTube popup and cleaning up resources", "UI");
                
                // Clear search state
                isSearching = false;
                isDownloading = false;
                
                // Clear song data
                currentSongDetails = null;
                
                // Clear UI rows to prevent memory leaks
                ClearSongRows();
                
                // Clean up UI elements
                if (popupContainer != null)
                {
                    // Remove all button listeners from the entire popup
                    var allButtons = popupContainer.GetComponentsInChildren<Button>();
                    foreach (var button in allButtons)
                    {
                        button?.onClick?.RemoveAllListeners();
                    }
                    
                    Destroy(popupContainer);
                    popupContainer = null;
                }
                
                // Clear references
                searchBarInputField = null;
                searchButton = null;
                cancelButton = null;
                statusText = null;
                songTableContainer = null;
                songTableContent = null;
                songTableScrollRect = null;
                
                LoggingSystem.Debug("YouTube popup cleanup completed", "UI");
            }
            catch (Exception ex)
            {
                LoggingSystem.Warning($"Error during popup cleanup: {ex.Message}", "UI");
            }
            finally
            {
                // Always destroy this component
                try
                {
                    Destroy(this);
                }
                catch
                {
                    // Silent fallback
                }
            }
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
                // Get the target playlist from the dropdown selection
                YouTubePlaylist? targetPlaylist = null;
                
                if (targetPlaylistDropdown != null && targetPlaylistDropdown.value >= 0)
                {
                    var availablePlaylists = YouTubePlaylistManager.GetAllPlaylists();
                    if (targetPlaylistDropdown.value < availablePlaylists.Count)
                    {
                        var selectedPlaylistInfo = availablePlaylists[targetPlaylistDropdown.value];
                        targetPlaylist = YouTubePlaylistManager.LoadPlaylist(selectedPlaylistInfo.id);
                    }
                }
                
                // Fall back to current playlist if no target selected
                if (targetPlaylist == null)
                {
                    targetPlaylist = playlistToggleComponent?.GetCurrentYouTubePlaylist();
                }
                
                if (targetPlaylist == null)
                {
                    UpdateStatus("❌ No playlist selected. Please select a playlist or create a new one.", Color.red);
                    return;
                }
                
                UpdateStatus($"🎵 Adding {currentSongDetails.Count} songs to playlist...", Color.yellow);
                
                int successCount = 0;
                int skipCount = 0;
                
                foreach (var song in currentSongDetails)
                {
                    if (targetPlaylist.AddSong(song))
                    {
                        successCount++;
                        LoggingSystem.Info($"Added '{song.title}' to playlist", "UI");
                    }
                    else
                    {
                        skipCount++;
                        LoggingSystem.Info($"Skipped '{song.title}' (already in playlist)", "UI");
                    }
                }
                
                // Save the playlist after all additions
                if (successCount > 0)
                {
                    YouTubePlaylistManager.SavePlaylist(targetPlaylist);
                    
                    var statusMessage = skipCount > 0 ? 
                        $"✅ Added {successCount} songs to playlist ({skipCount} already existed)" :
                        $"✅ Added all {successCount} songs to playlist '{targetPlaylist.name}'!";
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