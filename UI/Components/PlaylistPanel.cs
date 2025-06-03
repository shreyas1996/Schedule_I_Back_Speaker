using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;
using BackSpeakerMod.UI.Components.Playlist;

namespace BackSpeakerMod.UI.Components
{
    public class PlaylistPanel : MonoBehaviour
    {
        // Core dependencies
        private BackSpeakerManager? manager;
        private BackSpeakerScreen? mainScreen;
        private RectTransform? canvasRect;
        
        // UI Components
        private Button? toggleButton;
        private GameObject? playlistContainer;
        private ScrollRect? scrollRect;
        private Transform? contentParent;
        private bool isVisible = false;
        
        // Extracted functionality
        private PlaylistSearch? searchComponent;
        private PlaylistRenderer? renderComponent;
        
        // Change detection to prevent constant recreation
        private int lastTrackCount = -1;
        private int lastCurrentTrackIndex = -1;
        private string lastSearchQuery = "";
        private bool needsPlaylistRefresh = false;

        // // IL2CPP compatibility - explicit parameterless constructor
        // public PlaylistPanel() : base() { }

        public void Setup(BackSpeakerManager manager, RectTransform canvasRect, BackSpeakerScreen mainScreen)
        {
            try
            {
                this.manager = manager;
                this.canvasRect = canvasRect;
                this.mainScreen = mainScreen;
                
                // Initialize extracted components
                InitializeComponents();
                
                // Create playlist container first (but keep it hidden)
                CreatePlaylistContainer();
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"PlaylistPanel: Setup failed: {ex.Message}", "UI");
                throw;
            }
        }

        /// <summary>
        /// Initialize extracted components
        /// </summary>
        private void InitializeComponents()
        {
            searchComponent = new PlaylistSearch();
            renderComponent = new PlaylistRenderer();
            
            // Wire up events
            searchComponent.OnSearchChanged += OnSearchChanged;
            renderComponent.OnTrackSelected += OnTrackSelected;
        }

        private void CreatePlaylistContainer()
        {
            // PROTECTION: Don't create multiple containers
            if (playlistContainer != null)
            {
                return;
            }
            
            playlistContainer = new GameObject("PlaylistContainer");
            playlistContainer.transform.SetParent(canvasRect!.transform, false);
            
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
            
            // LoggerUtil.Info("PlaylistPanel: PlaylistContainer created with NO background components");
            
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
            
            // Create search functionality using extracted component
            searchComponent?.CreateSearchInterface(playlistContainer!.transform);
            
            // Create scroll view for track list
            CreateScrollView();
            
            // Initialize render component
            renderComponent?.Initialize(contentParent!, scrollRect!);
            
            // Start completely hidden
            playlistContainer.SetActive(false);
            isVisible = false;
            
        }

        private void CreateScrollView()
        {
            var scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(playlistContainer!.transform, false);
            
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

        /// <summary>
        /// Handle search changes from search component
        /// </summary>
        private void OnSearchChanged(string query)
        {
            needsPlaylistRefresh = true;
        }

        /// <summary>
        /// Handle track selection from render component
        /// </summary>
        private void OnTrackSelected(int trackIndex)
        {
            if (manager != null)
            {
                manager.PlayTrack(trackIndex);
                needsPlaylistRefresh = true;
            }
        }

        private void TogglePlaylist()
        {
            isVisible = !isVisible;
            // LoggerUtil.Info($"PlaylistPanel: Playlist {(isVisible ? "opened" : "closed")}");
            
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
            
            if (manager == null || renderComponent == null || searchComponent == null) return;
            
            // Check if anything actually changed
            var allTracks = manager.GetAllTracks();
            int currentTrackIndex = manager.CurrentTrackIndex;
            string currentSearchQuery = searchComponent.CurrentQuery ?? "";
            
            bool hasChanges = needsPlaylistRefresh ||
                             allTracks.Count != lastTrackCount ||
                             currentTrackIndex != lastCurrentTrackIndex ||
                             currentSearchQuery != lastSearchQuery;
            
            if (!hasChanges) return; // No changes, don't recreate buttons
            
            // Update tracking variables
            lastTrackCount = allTracks.Count;
            lastCurrentTrackIndex = currentTrackIndex;
            lastSearchQuery = currentSearchQuery ?? "";
            needsPlaylistRefresh = false;
            
            // Use render component to update the playlist
            renderComponent.RenderTracks(allTracks, currentTrackIndex, searchComponent);
        }

        public void CreateToggleButton(Transform parentTransform)
        {
            // Find the HeadphonePanel that was created by HeadphoneControlPanel
            var headphonePanel = parentTransform.Find("HeadphonePanel");
            if (headphonePanel == null)
            {
                LoggingSystem.Warning("Could not find HeadphonePanel - creating playlist button with fallback positioning", "UI");
                // Fallback to old positioning if HeadphonePanel not found
                toggleButton = UIFactory.CreateButton(
                    parentTransform,
                    "♫ Playlist",
                    new Vector2(0f, -250f),
                    new Vector2(80f, 30f)
                );
            }
            else
            {
                // Position playlist button in RIGHT SIDE of the HeadphonePanel (70% to 95% width, same height as headphone button)
                toggleButton = CreateButton(headphonePanel, "♫ Playlist", new Vector2(0.7f, 0.5f), new Vector2(0.95f, 1f), (UnityEngine.Events.UnityAction)TogglePlaylist);
                LoggingSystem.Info("Playlist button positioned alongside headphone button", "UI");
                ApplyToggleButtonStyling(toggleButton);
                return; // Return early since we already applied styling
            }
            
            toggleButton.onClick.AddListener((UnityEngine.Events.UnityAction)TogglePlaylist);
            ApplyToggleButtonStyling(toggleButton);
        }

        private Button CreateButton(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObj = new GameObject($"{text}Button").AddComponent<RectTransform>();
            buttonObj.SetParent(parent, false);
            buttonObj.anchorMin = anchorMin;
            buttonObj.anchorMax = anchorMax;
            buttonObj.offsetMin = new Vector2(2f, 2f); // Small margins
            buttonObj.offsetMax = new Vector2(-2f, -2f);
            
            var button = buttonObj.gameObject.AddComponent<Button>();
            var image = buttonObj.gameObject.AddComponent<Image>();
            image.color = new Color(0.4f, 0.2f, 0.8f, 0.8f); // Purple accent for playlist
            
            var textObj = new GameObject("Text").AddComponent<RectTransform>();
            textObj.SetParent(buttonObj, false);
            textObj.anchorMin = Vector2.zero;
            textObj.anchorMax = Vector2.one;
            textObj.offsetMin = Vector2.zero;
            textObj.offsetMax = Vector2.zero;
            
            var textComponent = textObj.gameObject.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 10;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            button.onClick.AddListener(onClick);
            return button;
        }
    }
} 