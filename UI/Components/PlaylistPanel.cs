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
        
        // Layout management
        private BackSpeakerScreen mainScreen = null;
        private RectTransform canvasRect = null;
        
        // Track list change detection to prevent constant recreation
        private int lastTrackCount = -1;
        private int lastCurrentTrackIndex = -1;
        private string lastSearchQuery = "";
        private bool needsPlaylistRefresh = false;

        // IL2CPP compatibility - explicit parameterless constructor
        public PlaylistPanel() : base() { }

        public void Setup(BackSpeakerManager manager, RectTransform canvasRect, BackSpeakerScreen mainScreen)
        {
            try
            {
                this.manager = manager;
                this.canvasRect = canvasRect;
                this.mainScreen = mainScreen;
                LoggerUtil.Info("PlaylistPanel: Setting up responsive playlist interface");
                
                // Create playlist container first (but keep it hidden)
                CreatePlaylistContainer();
                
                // Note: Playlist toggle button will be created by the main screen 
                // in the controls container so it shifts with other controls
                
                LoggerUtil.Info("PlaylistPanel: Responsive playlist setup completed");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"PlaylistPanel: Setup failed: {ex}");
                throw;
            }
        }

        private void CreatePlaylistContainer()
        {
            // PROTECTION: Don't create multiple containers
            if (playlistContainer != null)
            {
                return;
            }
            
            playlistContainer = new GameObject("PlaylistContainer");
            playlistContainer.transform.SetParent(canvasRect.transform, false);
            
            var containerRect = playlistContainer.AddComponent<RectTransform>();
            // Position on the right side with proper 50/50 split
            containerRect.anchorMin = new Vector2(0.5f, 0f);
            containerRect.anchorMax = new Vector2(1f, 1f);
            containerRect.offsetMin = new Vector2(10f, 10f);
            containerRect.offsetMax = new Vector2(-10f, -10f);
            
            // Ensure NO background image when container is created
            var existingImages = playlistContainer.GetComponents<Image>();
            for (int i = existingImages.Length - 1; i >= 0; i--)
            {
                if (existingImages[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(existingImages[i]);
                }
            }
            
            LoggerUtil.Info("PlaylistPanel: PlaylistContainer created with NO background components");
            
            // Create title at the TOP of the playlist container - fix positioning
            var titleText = UIFactory.CreateText(
                playlistContainer.transform,
                "PlaylistTitle",
                "♫ Music Playlist ♫",
                new Vector2(0f, 0f),
                new Vector2(220f, 30f),
                18
            );
            titleText.color = new Color(1f, 1f, 1f, 1f);
            
            // Position title at the TOP of the container
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(0f, -35f);
            titleRect.offsetMax = new Vector2(0f, -5f);
            
            // Create search functionality
            CreateSearchInterface();
            
            // Create scroll view for track list
            CreateScrollView();
            
            // Start completely hidden
            playlistContainer.SetActive(false);
            isVisible = false;
            
            LoggerUtil.Info("PlaylistPanel: Playlist setup completed");
        }

        private void CreateSearchInterface()
        {
            // Create search container
            var searchContainer = new GameObject("SearchContainer");
            searchContainer.transform.SetParent(playlistContainer.transform, false);
            
            var searchRect = searchContainer.AddComponent<RectTransform>();
            searchRect.anchorMin = new Vector2(0f, 1f);
            searchRect.anchorMax = new Vector2(1f, 1f);
            searchRect.offsetMin = new Vector2(15f, -75f); // Below title (was -70f)
            searchRect.offsetMax = new Vector2(-15f, -45f); // 30px height for search
            
            // Create search input field background
            var searchBg = searchContainer.AddComponent<Image>();
            searchBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f); // Dark input background
            
            // Create search input field
            var inputObj = new GameObject("SearchInput");
            inputObj.transform.SetParent(searchContainer.transform, false);
            
            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = Vector2.zero;
            inputRect.anchorMax = new Vector2(0.8f, 1f);
            inputRect.offsetMin = new Vector2(8f, 3f); // Better padding
            inputRect.offsetMax = new Vector2(-8f, -3f);
            
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
            placeholderText.fontSize = 11;
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
            inputText.fontSize = 11;
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
                new Vector2(25f, 16f)
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
            
            // Make sure the clear button text is visible
            var clearTextComponent = clearSearchButton.GetComponentInChildren<Text>();
            if (clearTextComponent != null)
            {
                clearTextComponent.color = new Color(1f, 1f, 1f, 1f);
                clearTextComponent.fontSize = 12;
                clearTextComponent.fontStyle = FontStyle.Bold;
            }
            
            LoggerUtil.Info("PlaylistPanel: Search interface created");
        }

        private void CreateScrollView()
        {
            var scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(playlistContainer.transform, false);
            
            var scrollRectTransform = scrollObj.AddComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0f, 0f);
            scrollRectTransform.anchorMax = new Vector2(1f, 1f);
            scrollRectTransform.offsetMin = new Vector2(15f, 15f); // Bottom margin
            scrollRectTransform.offsetMax = new Vector2(-15f, -85f); // Leave space for title (35px) + search (30px) + margin (20px)
            
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
        }

        private void ApplyToggleButtonStyling(Button button)
        {
            if (button == null) return;
            
            // Purple accent for playlist button
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.4f, 0.2f, 0.8f, 0.8f); // Purple accent
            }
            
            var textComponent = button.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.color = new Color(1f, 1f, 1f, 1f); // White text
                textComponent.fontSize = 10;
                textComponent.fontStyle = FontStyle.Normal;
            }
        }

        private void OnSearchChanged(string query)
        {
            currentSearchQuery = query.ToLower();
            needsPlaylistRefresh = true;
        }

        private void ClearSearch()
        {
            if (searchInput != null)
            {
                searchInput.text = "";
                currentSearchQuery = "";
                needsPlaylistRefresh = true;
            }
        }

        private void TogglePlaylist()
        {
            isVisible = !isVisible;
            LoggerUtil.Info($"PlaylistPanel: Playlist {(isVisible ? "opened" : "closed")}");
            
            if (playlistContainer != null)
            {
                if (isVisible)
                {
                    playlistContainer.SetActive(true);
                    
                    // Add background and styling when opening
                    var background = playlistContainer.GetComponent<Image>();
                    if (background == null)
                    {
                        background = playlistContainer.AddComponent<Image>();
                        background.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
                        UIFactory.ApplyModernBorder(playlistContainer, new Color(0.3f, 0.3f, 0.3f, 0.8f), 2f);
                    }
                    
                    needsPlaylistRefresh = true;
                }
                else
                {
                    // Clean up all visual components
                    var allImages = playlistContainer.GetComponents<Image>();
                    for (int i = allImages.Length - 1; i >= 0; i--)
                    {
                        if (allImages[i] != null)
                        {
                            allImages[i].enabled = false;
                            allImages[i].color = new Color(0f, 0f, 0f, 0f);
                            UnityEngine.Object.Destroy(allImages[i]);
                        }
                    }
                    
                    // Clean up border objects created by ApplyModernBorder
                    if (playlistContainer.transform.parent != null)
                    {
                        var parentTransform = playlistContainer.transform.parent;
                        for (int i = 0; i < parentTransform.childCount; i++)
                        {
                            var child = parentTransform.GetChild(i);
                            if (child.name.Contains("PlaylistContainer_Border"))
                            {
                                var borderImages = child.GetComponents<Image>();
                                foreach (var img in borderImages)
                                {
                                    if (img != null)
                                    {
                                        img.enabled = false;
                                        img.color = new Color(0f, 0f, 0f, 0f);
                                        UnityEngine.Object.Destroy(img);
                                    }
                                }
                                UnityEngine.Object.Destroy(child.gameObject);
                            }
                        }
                    }
                    
                    // Clean up other components
                    var outlines = playlistContainer.GetComponents<UnityEngine.UI.Outline>();
                    for (int i = outlines.Length - 1; i >= 0; i--)
                    {
                        if (outlines[i] != null)
                        {
                            outlines[i].enabled = false;
                            UnityEngine.Object.Destroy(outlines[i]);
                        }
                    }
                    
                    var shadows = playlistContainer.GetComponents<UnityEngine.UI.Shadow>();
                    for (int i = shadows.Length - 1; i >= 0; i--)
                    {
                        if (shadows[i] != null)
                        {
                            shadows[i].enabled = false;
                            UnityEngine.Object.Destroy(shadows[i]);
                        }
                    }
                    
                    playlistContainer.SetActive(false);
                }
            }
            
            // Notify main screen about layout change
            if (mainScreen != null)
            {
                mainScreen.OnPlaylistToggle(isVisible);
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
            // Only update if playlist is visible AND something actually changed
            if (!isVisible) return;
            
            if (manager == null) return;
            
            // Check if anything actually changed
            var allTracks = manager.GetAllTracks();
            int currentTrackIndex = manager.CurrentTrackIndex;
            
            bool hasChanges = needsPlaylistRefresh ||
                             allTracks.Count != lastTrackCount ||
                             currentTrackIndex != lastCurrentTrackIndex ||
                             currentSearchQuery != lastSearchQuery;
            
            if (!hasChanges) return; // No changes, don't recreate buttons
            
            // Update tracking variables
            lastTrackCount = allTracks.Count;
            lastCurrentTrackIndex = currentTrackIndex;
            lastSearchQuery = currentSearchQuery;
            needsPlaylistRefresh = false;
            
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
                        new Vector2(250f, 60f),
                        12
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
                                       track.title.ToLower().Contains(currentSearchQuery) ||
                                       track.artist.ToLower().Contains(currentSearchQuery);
                    
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
                        new Vector2(250f, 40f),
                        12
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
                        buttonText = $"♪ {buttonText} ♪";
                    }
                    
                    var trackButton = UIFactory.CreateButton(
                        contentParent,
                        buttonText,
                        new Vector2(0f, yPosition),
                        new Vector2(260f, 35f)
                    );
                    
                    // Apply styling
                    var buttonImage = trackButton.GetComponent<Image>();
                    var buttonText_comp = trackButton.GetComponentInChildren<Text>();
                    
                    if (originalIndex == currentTrackIndex)
                    {
                        // Current track styling - Spotify green
                        if (buttonImage != null)
                            buttonImage.color = new Color(0.11f, 0.73f, 0.33f, 0.8f);
                        if (buttonText_comp != null)
                        {
                            buttonText_comp.color = new Color(0f, 0f, 0f, 1f);
                            buttonText_comp.fontSize = 11;
                            buttonText_comp.alignment = TextAnchor.MiddleLeft;
                        }
                    }
                    else
                    {
                        // Regular track styling
                        if (buttonImage != null)
                            buttonImage.color = new Color(0.25f, 0.25f, 0.25f, 0.6f);
                        if (buttonText_comp != null)
                        {
                            buttonText_comp.color = new Color(1f, 1f, 1f, 0.9f);
                            buttonText_comp.fontSize = 11;
                            buttonText_comp.alignment = TextAnchor.MiddleLeft;
                        }
                    }
                    
                    // Add click handler
                    int capturedIndex = originalIndex;
                    trackButton.onClick.AddListener((UnityEngine.Events.UnityAction)(() => {
                        OnTrackSelected(capturedIndex);
                    }));
                    
                    trackButtons.Add(trackButton);
                    yPosition -= 38f;
                }
                
                // Resize content area to fit all buttons
                if (scrollRect?.content != null)
                {
                    float contentHeight = Mathf.Max(100f, filteredTracks.Count * 38f + 40f);
                    scrollRect.content.sizeDelta = new Vector2(0f, contentHeight);
                }
                
                // Log only important information
                if (!string.IsNullOrEmpty(currentSearchQuery))
                {
                    LoggerUtil.Info($"PlaylistPanel: Filtered {filteredTracks.Count} of {allTracks.Count} tracks for '{currentSearchQuery}'");
                }
                else
                {
                    LoggerUtil.Info($"PlaylistPanel: Updated with {allTracks.Count} tracks");
                }
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
                needsPlaylistRefresh = true;
            }
        }

        public void CreateToggleButton(Transform parentTransform)
        {
            // Create playlist toggle button positioned at the bottom with other controls
            toggleButton = UIFactory.CreateButton(
                parentTransform,
                "♫ Playlist",
                new Vector2(0f, -250f),
                new Vector2(80f, 30f)
            );
            toggleButton.onClick.AddListener((UnityEngine.Events.UnityAction)TogglePlaylist);
            ApplyToggleButtonStyling(toggleButton);
        }
    }
} 