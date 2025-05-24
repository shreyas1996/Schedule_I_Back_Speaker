using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;
using System.Collections.Generic;

namespace BackSpeakerMod.UI.Components
{
    public class PlaylistPanel : MonoBehaviour
    {
        private BackSpeakerManager manager;
        private ScrollRect scrollRect;
        private RectTransform contentRect;
        private List<Button> trackButtons = new List<Button>();
        private List<Text> trackTexts = new List<Text>();

        public void Setup(BackSpeakerManager manager, RectTransform parent)
        {
            this.manager = manager;
            LoggerUtil.Info("PlaylistPanel: Setting up overlay playlist");
            
            // Create overlay playlist that covers the main UI when visible
            var playlistObj = new GameObject("PlaylistPanel");
            var playlistRect = playlistObj.AddComponent<RectTransform>();
            playlistObj.transform.SetParent(parent, false);
            
            // Cover the entire UI area when shown
            playlistRect.anchorMin = Vector2.zero;
            playlistRect.anchorMax = Vector2.one;
            playlistRect.pivot = new Vector2(0.5f, 0.5f);
            playlistRect.anchoredPosition = Vector2.zero;
            playlistRect.sizeDelta = Vector2.zero; // Fill parent
            
            // Add semi-transparent background
            var bgImage = playlistObj.AddComponent<Image>();
            bgImage.color = new Color(0.0f, 0.0f, 0.0f, 0.9f); // Dark overlay background
            
            // Start hidden
            playlistObj.SetActive(false);
            
            // Create title
            var titleText = UIFactory.CreateText(
                playlistRect,
                "PlaylistTitle",
                "PLAYLIST",
                new Vector2(0f, -15f),
                new Vector2(180f, 20f),
                16
            );
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.fontStyle = FontStyle.Bold;
            
            // Create scroll rect
            var scrollObj = new GameObject("ScrollArea");
            var scrollRectTransform = scrollObj.AddComponent<RectTransform>();
            scrollObj.transform.SetParent(playlistRect, false);
            
            scrollRectTransform.anchorMin = new Vector2(0f, 0f);
            scrollRectTransform.anchorMax = new Vector2(1f, 1f);
            scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
            scrollRectTransform.anchoredPosition = new Vector2(0f, -15f); // Below title
            scrollRectTransform.sizeDelta = new Vector2(-10f, -40f); // Margins
            
            scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            
            // Create content area
            var contentObj = new GameObject("Content");
            contentRect = contentObj.AddComponent<RectTransform>();
            contentObj.transform.SetParent(scrollRectTransform, false);
            
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f); // Will be resized based on content
            
            scrollRect.content = contentRect;
            
            // Add vertical layout group to content
            var layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 2f;
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            
            // Add content size fitter
            var contentSizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            LoggerUtil.Info("PlaylistPanel: Setup completed");
        }

        public void UpdatePlaylist()
        {
            try
            {
                LoggerUtil.Info("PlaylistPanel: Updating playlist");
                
                // Clear existing buttons
                foreach (var button in trackButtons)
                {
                    if (button != null && button.gameObject != null)
                        DestroyImmediate(button.gameObject);
                }
                trackButtons.Clear();
                trackTexts.Clear();
                
                // Get all tracks
                var tracks = manager.GetAllTracks();
                int currentIndex = manager.CurrentTrackIndex;
                
                LoggerUtil.Info($"PlaylistPanel: Creating {tracks.Count} track buttons");
                
                // Create button for each track
                for (int i = 0; i < tracks.Count; i++)
                {
                    var track = tracks[i];
                    var trackIndex = i; // Capture for closure
                    
                    // Create track button
                    var buttonObj = new GameObject($"Track_{i}");
                    var buttonRect = buttonObj.AddComponent<RectTransform>();
                    buttonObj.transform.SetParent(contentRect, false);
                    
                    buttonRect.sizeDelta = new Vector2(0f, 30f); // 30px tall
                    
                    var button = buttonObj.AddComponent<Button>();
                    var buttonImage = buttonObj.AddComponent<Image>();
                    
                    // Set button colors based on whether it's currently playing
                    bool isCurrentTrack = (i == currentIndex);
                    if (isCurrentTrack)
                    {
                        buttonImage.color = new Color(0.3f, 0.6f, 0.9f, 0.8f); // Blue for current track
                    }
                    else
                    {
                        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.6f); // Dark gray for others
                    }
                    
                    // Create text for track name
                    var textObj = new GameObject("Text");
                    var textRect = textObj.AddComponent<RectTransform>();
                    textObj.transform.SetParent(buttonRect, false);
                    
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.sizeDelta = Vector2.zero;
                    textRect.anchoredPosition = Vector2.zero;
                    
                    var text = textObj.AddComponent<Text>();
                    text.text = $"{i + 1}. {track.title}";
                    text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    text.fontSize = 12;
                    text.color = Color.white;
                    text.alignment = TextAnchor.MiddleLeft;
                    
                    // Make text overflow properly
                    text.horizontalOverflow = HorizontalWrapMode.Wrap;
                    text.verticalOverflow = VerticalWrapMode.Truncate;
                    
                    // Add padding
                    textRect.offsetMin = new Vector2(5f, 0f);
                    textRect.offsetMax = new Vector2(-5f, 0f);
                    
                    // Add click handler
                    button.onClick.AddListener((UnityEngine.Events.UnityAction)(() => OnTrackClicked(trackIndex)));
                    
                    trackButtons.Add(button);
                    trackTexts.Add(text);
                }
                
                LoggerUtil.Info($"PlaylistPanel: Created {trackButtons.Count} track buttons");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"PlaylistPanel: UpdatePlaylist failed: {ex}");
            }
        }

        private void OnTrackClicked(int trackIndex)
        {
            LoggerUtil.Info($"PlaylistPanel: Track {trackIndex} clicked");
            manager.PlayTrack(trackIndex);
            
            // Hide playlist after selection (like modern music apps)
            Hide();
        }
        
        public void Show()
        {
            if (this.gameObject != null)
            {
                this.gameObject.SetActive(true);
                UpdatePlaylist(); // Refresh when showing
                LoggerUtil.Info("PlaylistPanel: Shown");
            }
        }
        
        public void Hide()
        {
            if (this.gameObject != null)
            {
                this.gameObject.SetActive(false);
                LoggerUtil.Info("PlaylistPanel: Hidden");
            }
        }
        
        public void Toggle()
        {
            if (this.gameObject != null)
            {
                bool isActive = this.gameObject.activeSelf;
                if (isActive)
                    Hide();
                else
                    Show();
            }
        }
    }
} 