using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Core.System;
using System;

namespace BackSpeakerMod.UI.Components.Playlist
{
    /// <summary>
    /// Handles rendering and filtering of playlist tracks
    /// </summary>
    public class PlaylistRenderer
    {
        private Transform? contentParent;
        private ScrollRect? scrollRect;
        private List<Button> trackButtons = new List<Button>();

        /// <summary>
        /// Event fired when a track is selected
        /// </summary>
        public event Action<int> OnTrackSelected = null!;

        /// <summary>
        /// Initialize with content parent and scroll rect
        /// </summary>
        public void Initialize(Transform contentParent, ScrollRect scrollRect)
        {
            this.contentParent = contentParent;
            this.scrollRect = scrollRect;
        }

        /// <summary>
        /// Render filtered tracks
        /// </summary>
        public void RenderTracks(List<(string title, string artist)> allTracks, int currentTrackIndex, PlaylistSearch search)
        {
            if (contentParent == null) return;

            try
            {
                // Clear existing buttons
                ClearTrackButtons();

                if (allTracks.Count == 0)
                {
                    CreateNoTracksMessage();
                    return;
                }

                // Filter tracks based on search
                var filteredTracks = FilterTracks(allTracks, search);

                if (filteredTracks.Count == 0 && !string.IsNullOrEmpty(search.CurrentQuery))
                {
                    CreateNoMatchesMessage(search.CurrentQuery);
                    return;
                }

                // Create buttons for filtered tracks
                CreateTrackButtons(filteredTracks, currentTrackIndex);

                // Resize content area
                ResizeContentArea(filteredTracks.Count);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"PlaylistRenderer: RenderTracks failed: {ex.Message}", "UI");
            }
        }

        /// <summary>
        /// Clear all track buttons
        /// </summary>
        private void ClearTrackButtons()
        {
            foreach (var button in trackButtons)
            {
                if (button != null)
                    GameObject.Destroy(button.gameObject);
            }
            trackButtons.Clear();
        }

        /// <summary>
        /// Create "no tracks" message
        /// </summary>
        private void CreateNoTracksMessage()
        {
            var noTracksText = UIFactory.CreateText(
                contentParent!,
                "NoTracks",
                "No music loaded yet.\nUse 'RELOAD' to find music.",
                new Vector2(0f, -30f),
                new Vector2(250f, 60f),
                12
            );
            noTracksText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        }

        /// <summary>
        /// Create "no matches" message
        /// </summary>
        private void CreateNoMatchesMessage(string searchQuery)
        {
            var noMatchText = UIFactory.CreateText(
                contentParent!,
                "NoMatches",
                $"No tracks found for '{searchQuery}'",
                new Vector2(0f, -30f),
                new Vector2(250f, 40f),
                12
            );
            noMatchText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        }

        /// <summary>
        /// Filter tracks based on search criteria
        /// </summary>
        private List<(int originalIndex, (string title, string artist) track)> FilterTracks(
            List<(string title, string artist)> allTracks, PlaylistSearch search)
        {
            var filteredTracks = new List<(int originalIndex, (string title, string artist) track)>();

            for (int i = 0; i < allTracks.Count; i++)
            {
                var track = allTracks[i];
                if (search.MatchesSearch(track.title, track.artist))
                {
                    filteredTracks.Add((i, track));
                }
            }

            return filteredTracks;
        }

        /// <summary>
        /// Create track buttons for filtered tracks
        /// </summary>
        private void CreateTrackButtons(List<(int originalIndex, (string title, string artist) track)> filteredTracks, int currentTrackIndex)
        {
            float yPosition = -20f;
            
            for (int i = 0; i < filteredTracks.Count; i++)
            {
                var (originalIndex, track) = filteredTracks[i];
                
                var trackButton = CreateSingleTrackButton(originalIndex, track, currentTrackIndex, yPosition);
                trackButtons.Add(trackButton);
                yPosition -= 38f;
            }
        }

        /// <summary>
        /// Create a single track button
        /// </summary>
        private Button CreateSingleTrackButton(int originalIndex, (string title, string artist) track, int currentTrackIndex, float yPosition)
        {
            // Create button text
            string buttonText = $"{originalIndex + 1}. {track.title}";
            if (originalIndex == currentTrackIndex)
            {
                buttonText = $"♪ {buttonText} ♪";
            }

            var trackButton = UIFactory.CreateButton(
                contentParent!,
                buttonText,
                new Vector2(0f, yPosition),
                new Vector2(260f, 35f)
            );

            // Apply styling
            ApplyTrackButtonStyling(trackButton, originalIndex == currentTrackIndex);

            // Add click handler
            int capturedIndex = originalIndex;
            trackButton.onClick.AddListener((UnityEngine.Events.UnityAction)(() => {
                OnTrackSelected?.Invoke(capturedIndex);
            }));

            return trackButton;
        }

        /// <summary>
        /// Apply styling to track button
        /// </summary>
        private void ApplyTrackButtonStyling(Button trackButton, bool isCurrentTrack)
        {
            var buttonImage = trackButton.GetComponent<Image>();
            var buttonText = trackButton.GetComponentInChildren<Text>();

            if (isCurrentTrack)
            {
                // Current track styling - Spotify green
                if (buttonImage != null)
                    buttonImage.color = new Color(0.11f, 0.73f, 0.33f, 0.8f);
                if (buttonText != null)
                {
                    buttonText.color = new Color(0f, 0f, 0f, 1f);
                    buttonText.fontSize = 11;
                    buttonText.alignment = TextAnchor.MiddleLeft;
                }
            }
            else
            {
                // Regular track styling
                if (buttonImage != null)
                    buttonImage.color = new Color(0.25f, 0.25f, 0.25f, 0.6f);
                if (buttonText != null)
                {
                    buttonText.color = new Color(1f, 1f, 1f, 0.9f);
                    buttonText.fontSize = 11;
                    buttonText.alignment = TextAnchor.MiddleLeft;
                }
            }
        }

        /// <summary>
        /// Resize content area to fit tracks
        /// </summary>
        private void ResizeContentArea(int trackCount)
        {
            if (scrollRect?.content != null)
            {
                float contentHeight = Mathf.Max(100f, trackCount * 38f + 40f);
                scrollRect.content.sizeDelta = new Vector2(0f, contentHeight);
            }
        }
    }
} 