using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.UI.Helpers;
using System;

namespace BackSpeakerMod.UI.Components.Playlist
{
    /// <summary>
    /// Handles search functionality for playlist
    /// </summary>
    public class PlaylistSearch
    {
        private InputField? searchInput;
        private Button? clearSearchButton;
        private string? currentSearchQuery = "";

        /// <summary>
        /// Event fired when search query changes
        /// </summary>
        public event Action<string>? OnSearchChanged;

        /// <summary>
        /// Current search query
        /// </summary>
        public string? CurrentQuery => currentSearchQuery;

        /// <summary>
        /// Create search interface in parent container
        /// </summary>
        public void CreateSearchInterface(Transform parent)
        {
            // Create search container
            var searchContainer = new GameObject("SearchContainer");
            searchContainer.transform.SetParent(parent, false);
            
            var searchRect = searchContainer.AddComponent<RectTransform>();
            searchRect.anchorMin = new Vector2(0f, 1f);
            searchRect.anchorMax = new Vector2(1f, 1f);
            searchRect.offsetMin = new Vector2(15f, -75f);
            searchRect.offsetMax = new Vector2(-15f, -45f);
            
            // Create search input field background
            var searchBg = searchContainer.AddComponent<Image>();
            searchBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            
            // Create search input field
            CreateInputField(searchContainer.transform);
            
            // Create clear search button
            CreateClearButton(searchContainer.transform);
        }

        /// <summary>
        /// Create the input field component
        /// </summary>
        private void CreateInputField(Transform parent)
        {
            var inputObj = new GameObject("SearchInput");
            inputObj.transform.SetParent(parent, false);
            
            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = Vector2.zero;
            inputRect.anchorMax = new Vector2(0.8f, 1f);
            inputRect.offsetMin = new Vector2(8f, 3f);
            inputRect.offsetMax = new Vector2(-8f, -3f);
            
            searchInput = inputObj.AddComponent<InputField>();
            
            CreatePlaceholderText(inputObj.transform);
            CreateInputText(inputObj.transform);
            
            searchInput.onValueChanged.AddListener((UnityEngine.Events.UnityAction<string>)OnSearchValueChanged);
        }

        /// <summary>
        /// Create placeholder text
        /// </summary>
        private void CreatePlaceholderText(Transform parent)
        {
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(parent, false);
            
            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            
            var placeholderText = placeholderObj.AddComponent<Text>();
            placeholderText.text = "Search tracks...";
            placeholderText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            placeholderText.fontSize = 11;
            placeholderText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            placeholderText.alignment = TextAnchor.MiddleLeft;
            
            searchInput!.placeholder = placeholderText;
        }

        /// <summary>
        /// Create input text component
        /// </summary>
        private void CreateInputText(Transform parent)
        {
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var inputText = textObj.AddComponent<Text>();
            inputText.text = "";
            inputText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            inputText.fontSize = 11;
            inputText.color = new Color(1f, 1f, 1f, 1f);
            inputText.alignment = TextAnchor.MiddleLeft;
            
            searchInput!.textComponent = inputText;
        }

        /// <summary>
        /// Create clear search button
        /// </summary>
        private void CreateClearButton(Transform parent)
        {
            clearSearchButton = UIFactory.CreateButton(
                parent,
                "✕",
                new Vector2(0f, 0f),
                new Vector2(25f, 16f)
            );
            
            var clearRect = clearSearchButton.GetComponent<RectTransform>();
            clearRect.anchorMin = new Vector2(0.8f, 0.2f);
            clearRect.anchorMax = new Vector2(0.95f, 0.8f);
            clearRect.offsetMin = Vector2.zero;
            clearRect.offsetMax = Vector2.zero;
            
            clearSearchButton.onClick.AddListener((UnityEngine.Events.UnityAction)ClearSearch);
            
            // Style clear button
            var clearBg = clearSearchButton.GetComponent<Image>();
            if (clearBg != null)
                clearBg.color = new Color(0.6f, 0.2f, 0.2f, 0.8f);
                
            var clearText = clearSearchButton.GetComponentInChildren<Text>();
            if (clearText != null)
            {
                clearText.color = Color.white;
                clearText.fontSize = 10;
            }
        }

        /// <summary>
        /// Handle search input changes
        /// </summary>
        private void OnSearchValueChanged(string query)
        {
            currentSearchQuery = query.ToLower();
            OnSearchChanged?.Invoke(currentSearchQuery);
        }

        /// <summary>
        /// Clear the search
        /// </summary>
        private void ClearSearch()
        {
            searchInput!.text = "";
            currentSearchQuery = "";
            OnSearchChanged?.Invoke(currentSearchQuery);
        }

        /// <summary>
        /// Check if a track matches current search
        /// </summary>
        public bool MatchesSearch(string title, string artist)
        {
            if (string.IsNullOrEmpty(currentSearchQuery))
                return true;
                
            return title.ToLower().Contains(currentSearchQuery) || 
                   artist.ToLower().Contains(currentSearchQuery);
        }
    }
} 