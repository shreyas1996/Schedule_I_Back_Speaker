using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI.Components
{
    public class PlaylistPanel : MonoBehaviour
    {
        // IL2CPP compatibility - explicit field initialization
        private BackSpeakerManager manager = null;
        private Button toggleButton = null;
        private GameObject playlistContainer = null;
        private ScrollRect scrollRect = null;
        private Transform contentParent = null;
        private bool isVisible = false;
        private List<Button> trackButtons = new List<Button>();
        
        // Search functionality
        private InputField searchInput = null;
        private string currentSearchQuery = "";
        private Button clearSearchButton = null;

        // IL2CPP compatibility - explicit parameterless constructor
        public PlaylistPanel() : base() { }

        public void Setup(BackSpeakerManager manager, RectTransform parent)
        {
            try
            {
                this.manager = manager;
                LoggerUtil.Info("PlaylistPanel: Setting up modern playlist interface with search");
                
                // Playlist toggle button - bottom corner, doesn't interfere with main controls
                toggleButton = UIFactory.CreateButton(
                    parent.transform,
                    "♫ Playlist",
                    new Vector2(120f, -170f), // Far bottom right corner
                    new Vector2(80f, 25f) // Compact button
                );
                toggleButton.onClick.AddListener((UnityEngine.Events.UnityAction)TogglePlaylist);
                LoggerUtil.Info("PlaylistPanel: Toggle button created");
                
                // Create playlist container (hidden by default)
                playlistContainer = new GameObject("PlaylistContainer");
                playlistContainer.transform.SetParent(parent.transform, false);
                
                var containerRect = playlistContainer.AddComponent<RectTransform>();
                containerRect.anchorMin = new Vector2(0f, 0f);
                containerRect.anchorMax = new Vector2(1f, 1f);
                containerRect.offsetMin = new Vector2(20f, 20f); // 20px margin
                containerRect.offsetMax = new Vector2(-20f, -200f); // Leave space for controls
                
                // Add background to playlist container with Spotify-style dark theme
                var background = playlistContainer.AddComponent<Image>();
                background.color = new Color(0.08f, 0.08f, 0.08f, 0.96f); // Very dark background
                
                // Create title for playlist
                var titleText = UIFactory.CreateText(
                    playlistContainer.transform,
                    "PlaylistTitle",
                    "♫ Music Playlist ♫",
                    new Vector2(0f, -25f), // Top of container
                    new Vector2(200f, 30f),
                    18
                );
                titleText.color = new Color(1f, 1f, 1f, 1f); // White text
                
                // Create search functionality
                CreateSearchInterface();
                
                // Create scroll view for track list
                var scrollObj = new GameObject("ScrollView");
                scrollObj.transform.SetParent(playlistContainer.transform, false);
                
                var scrollRectTransform = scrollObj.AddComponent<RectTransform>();
                scrollRectTransform.anchorMin = new Vector2(0f, 0f);
                scrollRectTransform.anchorMax = new Vector2(1f, 1f);
                scrollRectTransform.offsetMin = new Vector2(10f, 10f);
                scrollRectTransform.offsetMax = new Vector2(-10f, -90f); // Leave space for title and search
                
                scrollRect = scrollObj.AddComponent<ScrollRect>();
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                
                // Create content area for track buttons
                var contentObj = new GameObject("Content");
                contentObj.transform.SetParent(scrollObj.transform, false);
                
                var contentRect = contentObj.AddComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0f, 1f);
                contentRect.anchorMax = new Vector2(1f, 1f);
                contentRect.pivot = new Vector2(0.5f, 1f);
                contentRect.sizeDelta = new Vector2(0f, 100f); // Will resize based on content
                contentRect.anchoredPosition = Vector2.zero;
                
                contentParent = contentObj.transform;
                scrollRect.content = contentRect;
                
                // Start hidden
                playlistContainer.SetActive(false);
                
                LoggerUtil.Info("PlaylistPanel: Modern setup with search completed");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"PlaylistPanel: Setup failed: {ex}");
                throw;
            }
        }

        private void CreateSearchInterface()
        {
            // Create search container
            var searchContainer = new GameObject("SearchContainer");
            searchContainer.transform.SetParent(playlistContainer.transform, false);
            
            var searchRect = searchContainer.AddComponent<RectTransform>();
            searchRect.anchorMin = new Vector2(0f, 1f);
            searchRect.anchorMax = new Vector2(1f, 1f);
            searchRect.offsetMin = new Vector2(15f, -70f);
            searchRect.offsetMax = new Vector2(-15f, -50f);
            
            // Create search input field background
            var searchBg = searchContainer.AddComponent<Image>();
            searchBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f); // Dark input background
            
            // Create search input field
            var inputObj = new GameObject("SearchInput");
            inputObj.transform.SetParent(searchContainer.transform, false);
            
            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = Vector2.zero;
            inputRect.anchorMax = new Vector2(0.8f, 1f);
            inputRect.offsetMin = new Vector2(5f, 2f);
            inputRect.offsetMax = new Vector2(-5f, -2f);
            
            searchInput = inputObj.AddComponent<InputField>();
            
            // Create placeholder text
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputObj.transform, false);
            
            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            
            var placeholderText = placeholderObj.AddComponent<Text>();
            placeholderText.text = "Search tracks...";
            placeholderText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            placeholderText.fontSize = 12;
            placeholderText.color = new Color(0.6f, 0.6f, 0.6f, 1f); // Gray placeholder
            placeholderText.alignment = TextAnchor.MiddleLeft;
            
            // Create input text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var inputText = textObj.AddComponent<Text>();
            inputText.text = "";
            inputText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            inputText.fontSize = 12;
            inputText.color = new Color(1f, 1f, 1f, 1f); // White input text
            inputText.alignment = TextAnchor.MiddleLeft;
            
            // Setup InputField component
            searchInput.textComponent = inputText;
            searchInput.placeholder = placeholderText;
            searchInput.onValueChanged.AddListener((UnityEngine.Events.UnityAction<string>)OnSearchChanged);
            
            // Create clear search button
            clearSearchButton = UIFactory.CreateButton(
                searchContainer.transform,
                "✕",
                new Vector2(0f, 0f),
                new Vector2(30f, 16f)
            );
            
            var clearRect = clearSearchButton.GetComponent<RectTransform>();
            clearRect.anchorMin = new Vector2(0.8f, 0f);
            clearRect.anchorMax = new Vector2(1f, 1f);
            clearRect.offsetMin = new Vector2(5f, 2f);
            clearRect.offsetMax = new Vector2(-5f, -2f);
            
            clearSearchButton.onClick.AddListener((UnityEngine.Events.UnityAction)ClearSearch);
            
            // Style the clear button
            var clearImage = clearSearchButton.GetComponent<Image>();
            if (clearImage != null)
            {
                clearImage.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
            }
            
            LoggerUtil.Info("PlaylistPanel: Search interface created");
        }

        private void OnSearchChanged(string query)
        {
            currentSearchQuery = query.ToLower();
            LoggerUtil.Info($"PlaylistPanel: Search query changed to: '{query}'");
            FilterAndUpdatePlaylist();
        }

        private void ClearSearch()
        {
            if (searchInput != null)
            {
                searchInput.text = "";
                currentSearchQuery = "";
                FilterAndUpdatePlaylist();
                LoggerUtil.Info("PlaylistPanel: Search cleared");
            }
        }

        private void TogglePlaylist()
        {
            isVisible = !isVisible;
            if (playlistContainer != null)
            {
                playlistContainer.SetActive(isVisible);
                
                if (isVisible)
                {
                    FilterAndUpdatePlaylist(); // Refresh when showing
                    LoggerUtil.Info("PlaylistPanel: Playlist shown");
                }
                else
                {
                    LoggerUtil.Info("PlaylistPanel: Playlist hidden");
                }
            }
            
            // Update button text
            if (toggleButton != null)
            {
                var textComponent = toggleButton.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    textComponent.text = isVisible ? "✕ Close" : "♫ Playlist";
                }
            }
        }

        public void UpdatePlaylist()
        {
            FilterAndUpdatePlaylist();
        }

        private void FilterAndUpdatePlaylist()
        {
            if (manager == null || contentParent == null) return;
            
            try
            {
                // Clear existing buttons
                foreach (var button in trackButtons)
                {
                    if (button != null)
                        GameObject.Destroy(button.gameObject);
                }
                trackButtons.Clear();
                
                var allTracks = manager.GetAllTracks();
                int currentTrackIndex = manager.CurrentTrackIndex;
                
                if (allTracks.Count == 0)
                {
                    // Create "no tracks" message
                    var noTracksText = UIFactory.CreateText(
                        contentParent,
                        "NoTracks",
                        "No music loaded yet.\nUse 'RELOAD' to find music.",
                        new Vector2(0f, -30f),
                        new Vector2(300f, 60f),
                        14
                    );
                    noTracksText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    return;
                }
                
                // Filter tracks based on search query
                var filteredTracks = new List<(int originalIndex, (string title, string artist) track)>();
                
                for (int i = 0; i < allTracks.Count; i++)
                {
                    var track = allTracks[i];
                    bool matchesSearch = string.IsNullOrEmpty(currentSearchQuery) ||
                                       track.title.ToLower().Contains(currentSearchQuery);
                    
                    if (matchesSearch)
                    {
                        filteredTracks.Add((i, track));
                    }
                }
                
                if (filteredTracks.Count == 0 && !string.IsNullOrEmpty(currentSearchQuery))
                {
                    // Create "no matches" message
                    var noMatchText = UIFactory.CreateText(
                        contentParent,
                        "NoMatches",
                        $"No tracks found for '{currentSearchQuery}'",
                        new Vector2(0f, -30f),
                        new Vector2(300f, 40f),
                        14
                    );
                    noMatchText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                    return;
                }
                
                // Create button for each filtered track
                float yPosition = -20f;
                for (int i = 0; i < filteredTracks.Count; i++)
                {
                    var (originalIndex, track) = filteredTracks[i];
                    
                    // Highlight current track
                    string buttonText = $"{originalIndex + 1}. {track.title}";
                    if (originalIndex == currentTrackIndex)
                    {
                        buttonText = $"♪ {buttonText} ♪"; // Current track indicator
                    }
                    
                    var trackButton = UIFactory.CreateButton(
                        contentParent,
                        buttonText,
                        new Vector2(0f, yPosition),
                        new Vector2(280f, 35f) // Wide buttons for better touch targets
                    );
                    
                    // Apply Spotify-style button colors
                    var buttonImage = trackButton.GetComponent<Image>();
                    var buttonText_comp = trackButton.GetComponentInChildren<Text>();
                    
                    if (originalIndex == currentTrackIndex)
                    {
                        // Current track styling - Spotify green
                        if (buttonImage != null)
                            buttonImage.color = new Color(0.11f, 0.73f, 0.33f, 0.8f); // Spotify green
                        if (buttonText_comp != null)
                            buttonText_comp.color = new Color(0f, 0f, 0f, 1f); // Black text on green
                    }
                    else
                    {
                        // Regular track styling
                        if (buttonImage != null)
                            buttonImage.color = new Color(0.25f, 0.25f, 0.25f, 0.6f); // Dark gray
                        if (buttonText_comp != null)
                            buttonText_comp.color = new Color(1f, 1f, 1f, 0.9f); // White text
                    }
                    
                    // Add click handler
                    int trackIndex = originalIndex; // Capture for closure
                    trackButton.onClick.AddListener((UnityEngine.Events.UnityAction)(() => OnTrackSelected(trackIndex)));
                    
                    trackButtons.Add(trackButton);
                    yPosition -= 40f; // Space between buttons
                }
                
                // Resize content area to fit all buttons
                if (scrollRect?.content != null)
                {
                    float contentHeight = Mathf.Max(100f, filteredTracks.Count * 40f + 40f);
                    scrollRect.content.sizeDelta = new Vector2(0f, contentHeight);
                }
                
                string searchInfo = string.IsNullOrEmpty(currentSearchQuery) ? 
                    $"Updated with {allTracks.Count} tracks" : 
                    $"Filtered {filteredTracks.Count} of {allTracks.Count} tracks for '{currentSearchQuery}'";
                LoggerUtil.Info($"PlaylistPanel: {searchInfo}");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"PlaylistPanel: FilterAndUpdatePlaylist failed: {ex}");
            }
        }

        private void OnTrackSelected(int trackIndex)
        {
            if (manager != null)
            {
                LoggerUtil.Info($"PlaylistPanel: Track {trackIndex + 1} selected");
                manager.PlayTrack(trackIndex);
                
                // Auto-close playlist after selection (mobile-friendly)
                TogglePlaylist();
            }
        }
    }
} 