using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.NewBackend;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.NewFrontend.UI.Interfaces;
using BackSpeakerMod.S1Wrapper;

namespace BackSpeakerMod.NewFrontend.UI.Screens
{
    /// <summary>
    /// Advanced playlist editing screen with add/remove/reorder functionality
    /// </summary>
    public class PlaylistEditScreen : MonoBehaviour
    {
        #region Private Fields
        
        private BackSpeakerMainManager? mainManager;
        private NavigationManager? navigationManager;
        private IMusicBackend? musicBackend;
        
        // UI Components
        private GameObject? mainContainer;
        private GameObject? headerSection;
        private GameObject? editSection;
        private GameObject? songListSection;
        private GameObject? songScrollView;
        private GameObject? songListContent;
        
        // Header UI
        private Button? backButton;
        private Text? titleText;
        private InputField? playlistNameInput;
        private Button? saveButton;
        
        // Edit controls
        private Button? addSongsButton;
        private Button? selectAllButton;
        private Button? deleteSelectedButton;
        private Button? shuffleOrderButton;
        private Text? songCountText;
        
        // Song management
        private List<GameObject>? songItems;
        private List<NewSongDetails>? playlistSongs;
        private List<NewSongDetails>? selectedSongs;
        private NewYouTubePlaylistInfo? currentPlaylist;
        
        // State
        private bool hasChanges = false;
        
        public bool IsInitialized { get; private set; }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the playlist edit screen
        /// </summary>
        public void Initialize(BackSpeakerMainManager manager, NavigationManager navManager, IMusicBackend backend)
        {
            try
            {
                mainManager = manager;
                navigationManager = navManager;
                musicBackend = backend;
                
                songItems = new List<GameObject>();
                playlistSongs = new List<NewSongDetails>();
                selectedSongs = new List<NewSongDetails>();
                
                CreateUI();
                
                IsInitialized = true;
                NewLoggingSystem.Info("PlaylistEditScreen initialized successfully", "PlaylistEditScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to initialize PlaylistEditScreen: {ex}", "PlaylistEditScreen");
            }
        }
        
        /// <summary>
        /// Set the playlist to edit
        /// </summary>
        public void SetPlaylist(NewYouTubePlaylistInfo playlist)
        {
            currentPlaylist = playlist;
            if (playlist != null)
            {
                if (titleText != null)
                    titleText.text = $"Edit: {playlist.name}";
                if (playlistNameInput != null)
                    playlistNameInput.text = playlist.name;
                LoadPlaylistSongs();
            }
        }
        
        /// <summary>
        /// Show the screen
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            if (currentPlaylist != null)
            {
                LoadPlaylistSongs();
            }
        }
        
        /// <summary>
        /// Hide the screen
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateUI()
        {
            // Main container - following JukeboxScreen/LocalMusicScreen pattern
            CreateMainLayout();
            
            // Header section
            CreateHeaderSection();
            
            // Edit controls section
            CreateEditSection();
            
            // Song list section
            CreateSongListSection();
            
            // Hide initially
            gameObject.SetActive(false);
        }
        
        private void CreateMainLayout()
        {
            // Main container - following JukeboxScreen/LocalMusicScreen pattern
            mainContainer = new GameObject("PlaylistEditMainContainer");
            mainContainer.transform.SetParent(this.transform, false);
            
            var mainRect = mainContainer.AddComponent<RectTransform>();
            mainRect.anchorMin = Vector2.zero;
            mainRect.anchorMax = Vector2.one;
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;
            
            var mainImage = mainContainer.AddComponent<Image>();
            mainImage.color = ModernUIFactory.Colors.Background;
            
            // Main vertical layout - following the established pattern
            var mainLayout = mainContainer.AddComponent<VerticalLayoutGroup>();
            mainLayout.spacing = 0;
            mainLayout.padding = new RectOffset(0, 0, 0, 0);
            mainLayout.childControlHeight = true;  // Enable height control
            mainLayout.childControlWidth = true;
            mainLayout.childForceExpandHeight = false;  // DON'T force expand - let components use preferred heights
            mainLayout.childForceExpandWidth = true;
        }
        
        private void CreateHeaderSection()
        {
            // Following JukeboxScreen/LocalMusicScreen pattern for header
            if (mainContainer != null)
            {
                headerSection = ModernUIFactory.CreateCard(mainContainer.transform, new Vector2(0, 120), false);
            }
            
            var headerLayout = headerSection?.AddComponent<VerticalLayoutGroup>();
            if (headerLayout != null)
            {
                headerLayout.spacing = 10;
                headerLayout.padding = new RectOffset(20, 20, 15, 15);
                headerLayout.childControlHeight = false;
                headerLayout.childControlWidth = true;
                headerLayout.childForceExpandWidth = true;
            }
            
            // Top bar
            GameObject? topBar = null;
            if (headerSection != null)
            {
                topBar = ModernUIFactory.CreateHorizontalLayout(headerSection.transform, 15, new RectOffset(0, 0, 0, 0));
            }
            var topBarLayout = topBar?.AddComponent<LayoutElement>();
            if (topBarLayout != null)
            {
                topBarLayout.preferredHeight = 50;
            }
            
            // Back button
            if (topBar != null)
            {
                backButton = ModernUIFactory.CreateIconButton(topBar.transform, "←", 
                    S1Factory.ConvertToUnityAction(() => OnBackButtonPressed()), new Vector2(40, 40), false);
            }
            var backLayout = backButton?.gameObject?.AddComponent<LayoutElement>();
            if (backLayout != null)
            {
                backLayout.preferredWidth = 40;
                backLayout.preferredHeight = 40;
            }
            
            // Title
            if (topBar != null)
            {
                titleText = ModernUIFactory.CreateModernText(topBar.transform, "✏ Edit Playlist", 18, TextStyle.Primary, TextAnchor.MiddleLeft);
            }
            if (titleText != null)
            {
                titleText.fontStyle = FontStyle.Bold;
                var titleLayout = titleText.gameObject?.AddComponent<LayoutElement>();
                if (titleLayout != null)
                {
                    titleLayout.flexibleWidth = 1;
                }
            }
            
            // Save button
            if (topBar != null)
            {
                saveButton = ModernUIFactory.CreateIconButton(topBar.transform, "💾", 
                    S1Factory.ConvertToUnityAction(() => SavePlaylist()), new Vector2(40, 40), false);
            }
            var saveLayout = saveButton?.gameObject?.AddComponent<LayoutElement>();
            if (saveLayout != null)
            {
                saveLayout.preferredWidth = 40;
                saveLayout.preferredHeight = 40;
            }
            
            // Playlist name input
            GameObject? nameContainer = null;
            if (headerSection != null)
            {
                nameContainer = ModernUIFactory.CreateHorizontalLayout(headerSection.transform, 10, new RectOffset(0, 0, 0, 0));
            }
            var nameContainerLayout = nameContainer?.AddComponent<LayoutElement>();
            if (nameContainerLayout != null)
            {
                nameContainerLayout.preferredHeight = 50;
            }
            
            Text? nameLabel = null;
            if (nameContainer != null)
            {
                nameLabel = ModernUIFactory.CreateModernText(nameContainer.transform, "Name:", 14, TextStyle.Primary, TextAnchor.MiddleLeft);
            }
            var nameLabelLayout = nameLabel?.gameObject?.AddComponent<LayoutElement>();
            if (nameLabelLayout != null)
            {
                nameLabelLayout.preferredWidth = 60;
                nameLabelLayout.preferredHeight = 40;
            }
            
            if (nameContainer != null)
            {
                playlistNameInput = ModernUIFactory.CreateModernInputField(nameContainer.transform, "Enter playlist name...", new Vector2(0, 40));
            }
            if (playlistNameInput != null)
            {
                playlistNameInput.onValueChanged.AddListener((value) => 
                {
                    hasChanges = true;
                    if (saveButton != null && saveButton.image != null)
                        saveButton.image.color = ModernUIFactory.Colors.Warning;
                });
                
                var inputLayout = playlistNameInput.gameObject?.AddComponent<LayoutElement>();
                if (inputLayout != null)
                {
                    inputLayout.flexibleWidth = 1;
                    inputLayout.preferredHeight = 40;
                }
            }
            
            // Header section layout
            var headerSectionLayout = headerSection?.AddComponent<LayoutElement>();
            if (headerSectionLayout != null)
            {
                headerSectionLayout.preferredHeight = 120;
                headerSectionLayout.flexibleHeight = 0;
            }
        }
        
        private void CreateEditSection()
        {
            if (mainContainer != null)
            {
                editSection = ModernUIFactory.CreateCard(mainContainer.transform, new Vector2(0, 80), false);
            }
            var editLayout = editSection?.AddComponent<HorizontalLayoutGroup>();
            if (editLayout != null)
            {
                editLayout.spacing = 10;
                editLayout.padding = new RectOffset(20, 20, 15, 15);
                editLayout.childControlHeight = false;
                editLayout.childControlWidth = false;
                editLayout.childForceExpandWidth = false;
            }
            
            // Add songs button
            if (editSection != null)
            {
                addSongsButton = ModernUIFactory.CreateIconButton(editSection.transform, "+", 
                    S1Factory.ConvertToUnityAction(ShowAddSongsDialog), new Vector2(40, 40), false);
            }
            var addLayout = addSongsButton?.gameObject?.AddComponent<LayoutElement>();
            if (addLayout != null)
            {
                addLayout.preferredWidth = 40;
                addLayout.preferredHeight = 40;
            }
            
            // Select all button
            if (editSection != null)
            {
                selectAllButton = ModernUIFactory.CreateIconButton(editSection.transform, "✓", 
                    S1Factory.ConvertToUnityAction(ToggleSelectAll), new Vector2(40, 40), false);
            }
            var selectLayout = selectAllButton?.gameObject?.AddComponent<LayoutElement>();
            if (selectLayout != null)
            {
                selectLayout.preferredWidth = 40;
                selectLayout.preferredHeight = 40;
            }
            
            // Delete selected button
            if (editSection != null)
            {
                deleteSelectedButton = ModernUIFactory.CreateIconButton(editSection.transform, "✕", 
                    S1Factory.ConvertToUnityAction(DeleteSelectedSongs), new Vector2(40, 40), false);
            }
            var deleteLayout = deleteSelectedButton?.gameObject?.AddComponent<LayoutElement>();
            if (deleteLayout != null)
            {
                deleteLayout.preferredWidth = 40;
                deleteLayout.preferredHeight = 40;
            }
            
            // Shuffle order button
            if (editSection != null)
            {
                shuffleOrderButton = ModernUIFactory.CreateIconButton(editSection.transform, "🔀", 
                    S1Factory.ConvertToUnityAction(ShufflePlaylistOrder), new Vector2(40, 40), false);
            }
            var shuffleLayout = shuffleOrderButton?.gameObject?.AddComponent<LayoutElement>();
            if (shuffleLayout != null)
            {
                shuffleLayout.preferredWidth = 40;
                shuffleLayout.preferredHeight = 40;
            }
            
            // Spacer
            var spacer = new GameObject("Spacer");
            if (editSection != null)
            {
                spacer.transform.SetParent(editSection.transform, false);
            }
            var spacerLayout = spacer?.AddComponent<LayoutElement>();
            if (spacerLayout != null)
            {
                spacerLayout.flexibleWidth = 1;
            }
            
            // Song count
            if (editSection != null)
            {
                songCountText = ModernUIFactory.CreateModernText(editSection.transform, "0 songs", 14, TextStyle.Muted, TextAnchor.MiddleRight);
            }
            var countLayout = songCountText?.gameObject?.AddComponent<LayoutElement>();
            if (countLayout != null)
            {
                countLayout.preferredHeight = 25;
            }
            
            // Edit section layout element - following the pattern
            var editLayoutElement = editSection?.AddComponent<LayoutElement>();
            if (editLayoutElement != null)
            {
                editLayoutElement.preferredHeight = 80;
                editLayoutElement.flexibleHeight = 0;  // Don't expand
            }
        }
        
        private void CreateSongListSection()
        {
            // Following JukeboxScreen/LocalMusicScreen pattern
            songListSection = new GameObject("SongList");
            songListSection.transform.SetParent(mainContainer.transform, false);
            
            // Simple background
            var scrollImage = songListSection.AddComponent<Image>();
            scrollImage.color = ModernUIFactory.Colors.Background;
            
            // SIMPLIFIED: Direct content container, no complex viewport masking
            songListContent = new GameObject("Content");
            songListContent.transform.SetParent(songListSection.transform, false);
            
            var contentRect = songListContent.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Simple content layout
            var contentLayout = songListContent.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 8;
            contentLayout.padding = new RectOffset(15, 15, 15, 15);
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;
            
            // Content size fitter for proper scrolling
            var sizeFitter = songListContent.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            // Simple scroll setup
            songScrollView = songListSection;
            var scrollRect = songListSection.AddComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 15;
            
            // Song list layout element - take remaining space
            var listLayoutElement = songListSection.AddComponent<LayoutElement>();
            listLayoutElement.flexibleHeight = 1;
        }
        
        #endregion
        
        #region Song Management
        
        private void LoadPlaylistSongs()
        {
            if (currentPlaylist == null || musicBackend == null) return;
            
            // Loading playlist
            ClearSongList();
            
            try
            {
                playlistSongs = musicBackend.GetPlaylistSongs(currentPlaylist.id);
                
                foreach (var song in playlistSongs)
                {
                    CreateSongItem(song);
                }
                
                UpdateSongCount();
                // Finished loading
                
                NewLoggingSystem.Info($"Loaded {playlistSongs.Count} songs for playlist {currentPlaylist.name}", "PlaylistEditScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to load playlist songs: {ex}", "PlaylistEditScreen");
                // Failed to load
            }
        }
        
        private void CreateSongItem(NewSongDetails song)
        {
            if (songListContent == null || songItems == null) return;
            
            var songItem = ModernUIFactory.CreateCard(songListContent.transform, new Vector2(0, 80), false);
            if (songItem != null)
            {
                songItem.gameObject.name = $"SongItem_{song.url}";
                
                // Add to tracking list
                songItems.Add(songItem);
            }
            
            var itemLayout = songItem?.AddComponent<HorizontalLayoutGroup>();
            if (itemLayout != null)
            {
                itemLayout.spacing = 15;
                itemLayout.padding = new RectOffset(20, 20, 15, 15);
                itemLayout.childControlHeight = false;
                itemLayout.childControlWidth = false;
                itemLayout.childForceExpandWidth = false;
            }
            
            // Selection checkbox
            Toggle? checkbox = null;
            if (songItem != null)
            {
                checkbox = ModernUIFactory.CreateModernToggle(songItem.transform, "", (isSelected) => OnSongSelectionChanged(song, isSelected), new Vector2(25, 25));
            }
            var checkboxLayout = checkbox?.gameObject?.AddComponent<LayoutElement>();
            if (checkboxLayout != null)
            {
                checkboxLayout.preferredWidth = 25;
            }
            
            // Drag handle
            Button? dragHandle = null;
            if (songItem != null)
            {
                dragHandle = ModernUIFactory.CreateIconButton(songItem.transform, "≡", null!, new Vector2(30, 30));
            }
            var dragHandleImage = dragHandle?.GetComponent<Image>();
            if (dragHandleImage != null)
            {
                dragHandleImage.color = ModernUIFactory.Colors.TextMuted;
            }
            var dragLayout = dragHandle?.gameObject?.AddComponent<LayoutElement>();
            if (dragLayout != null)
            {
                dragLayout.preferredWidth = 30;
            }
            
            // Song info
            GameObject? infoContainer = null;
            if (songItem != null)
            {
                infoContainer = ModernUIFactory.CreateVerticalLayout(songItem.transform, 5, new RectOffset(0, 0, 0, 0));
            }
            var infoLayout = infoContainer?.AddComponent<LayoutElement>();
            if (infoLayout != null)
            {
                infoLayout.flexibleWidth = 1;
            }
            
            Text? titleText = null;
            if (infoContainer != null)
            {
                titleText = ModernUIFactory.CreateModernText(infoContainer.transform, song.title, 14, TextStyle.Primary, TextAnchor.MiddleLeft);
            }
            if (titleText != null)
            {
                titleText.fontStyle = FontStyle.Bold;
            }
            
            Text? artistText = null;
            if (infoContainer != null)
            {
                artistText = ModernUIFactory.CreateModernText(infoContainer.transform, song.artist, 12, TextStyle.Secondary, TextAnchor.MiddleLeft);
            }
            var artistLayout = artistText?.gameObject?.AddComponent<LayoutElement>();
            if (artistLayout != null)
            {
                artistLayout.preferredHeight = 15;
            }
            
            if (infoContainer != null)
            {
                var durationText = ModernUIFactory.CreateModernText(infoContainer.transform, FormatDuration(song.duration), 11, TextStyle.Muted, TextAnchor.MiddleLeft);
            }
            
            // Action buttons
            var playButton = ModernUIFactory.CreateIconButton(songItem.transform, "▶", 
                S1Factory.ConvertToUnityAction(() => PlaySong(song)), new Vector2(35, 35));
            var playLayout = playButton?.gameObject?.AddComponent<LayoutElement>();
            if (playLayout != null)
            {
                playLayout.preferredWidth = 35;
            }
            
            var removeButton = ModernUIFactory.CreateIconButton(songItem.transform, "✕", 
                S1Factory.ConvertToUnityAction(() => RemoveSong(song)), new Vector2(35, 35));
            var removeButtonImage = removeButton.GetComponent<Image>();
            if (removeButtonImage != null)
            {
                removeButtonImage.color = ModernUIFactory.Colors.Accent;
            }
            var removeLayout = removeButton.gameObject.AddComponent<LayoutElement>();
            removeLayout.preferredWidth = 35;
            
            // Context menu button
            var contextButton = ModernUIFactory.CreateContextMenuButton(songItem.transform, 
                () => ShowSongContextMenu(song, songItem.transform));
            var contextLayout = contextButton.gameObject.AddComponent<LayoutElement>();
            contextLayout.preferredWidth = 35;
        }
        
        private void ClearSongList()
        {
            if (songItems != null)
            {
                foreach (var item in songItems)
                {
                    if (item != null)
                    {
                        UnityEngine.Object.Destroy(item);
                    }
                }
                songItems.Clear();
            }
            
            if (selectedSongs != null)
            {
                selectedSongs.Clear();
            }
        }
        
        private void UpdateSongCount()
        {
            if (songCountText != null && playlistSongs != null)
            {
                songCountText.text = $"{playlistSongs.Count} songs";
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnBackButtonPressed()
        {
            if (hasChanges)
            {
                ShowUnsavedChangesDialog();
            }
            else
            {
                if (navigationManager != null)
                {
                    navigationManager.NavigateBack();
                }
            }
        }
        
        private void OnSongSelectionChanged(NewSongDetails song, bool isSelected)
        {
            if (selectedSongs == null) return;
            
            if (isSelected && !selectedSongs.Contains(song))
            {
                selectedSongs.Add(song);
            }
            else if (!isSelected && selectedSongs.Contains(song))
            {
                selectedSongs.Remove(song);
            }
            
            // Update button states
            if (deleteSelectedButton != null)
            {
                deleteSelectedButton.interactable = selectedSongs.Count > 0;
            }
        }
        
        private void SavePlaylist()
        {
            if (currentPlaylist == null || musicBackend == null || playlistNameInput == null) return;
            
            try
            {
                // Update playlist name if changed
                if (playlistNameInput.text != currentPlaylist.name)
                {
                    musicBackend.UpdatePlaylistInfo(currentPlaylist.id, playlistNameInput.text, currentPlaylist.description);
                    currentPlaylist.name = playlistNameInput.text;
                }
                
                hasChanges = false;
                NewLoggingSystem.Info($"Saved playlist {currentPlaylist.name}", "PlaylistEditScreen");
                
                // Show success feedback
                ShowFeedback("Playlist saved successfully!", ModernUIFactory.Colors.Primary);
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to save playlist: {ex}", "PlaylistEditScreen");
                ShowFeedback("Failed to save playlist", ModernUIFactory.Colors.Accent);
            }
        }
        
        private void ToggleSelectAll()
        {
            if (songItems == null || selectedSongs == null) return;
            
            bool selectAll = selectedSongs.Count == 0;
            
            selectedSongs.Clear();
            
            if (selectAll)
            {
                selectedSongs.AddRange(playlistSongs ?? new List<NewSongDetails>());
            }
            
            // Update checkbox states
            foreach (var songItem in songItems)
            {
                var checkbox = songItem?.GetComponentInChildren<Toggle>();
                if (checkbox != null)
                {
                    checkbox.isOn = selectAll;
                }
            }
            
            if (selectAllButton != null)
            {
                var selectAllText = selectAllButton.GetComponentInChildren<Text>();
                if (selectAllText != null)
                {
                    selectAllText.text = selectAll ? "Deselect All" : "Select All";
                }
            }
            
            if (deleteSelectedButton != null && selectedSongs != null)
            {
                deleteSelectedButton.interactable = selectedSongs.Count > 0;
            }
        }
        
        private void DeleteSelectedSongs()
        {
            if (selectedSongs == null || selectedSongs.Count == 0) return;
            
            ShowConfirmationDialog($"Delete {selectedSongs.Count} songs from playlist?", () =>
            {
                if (selectedSongs != null)
                {
                    foreach (var song in selectedSongs.ToList())
                    {
                        RemoveSong(song);
                    }
                    selectedSongs.Clear();
                    ToggleSelectAll(); // Reset selection
                }
            });
        }
        
        private void ShufflePlaylistOrder()
        {
            ShowConfirmationDialog("Shuffle the order of all songs in this playlist?", () =>
            {
                if (playlistSongs != null)
                {
                    // Shuffle the list
                    var random = new System.Random();
                    playlistSongs = playlistSongs.OrderBy(x => random.Next()).ToList();
                    
                    // Recreate the UI
                    ClearSongList();
                    foreach (var song in playlistSongs)
                    {
                        CreateSongItem(song);
                    }
                    
                    hasChanges = true;
                    ShowFeedback("Playlist order shuffled!", ModernUIFactory.Colors.Primary);
                }
            });
        }
        
        private void ShowAddSongsDialog()
        {
            // Create a dialog to add songs from different sources
            var dialog = ModernUIFactory.CreateDialog(this.transform, "Add Songs", new Vector2(600, 500));
            
            // Add tabs for different sources
            var tabContainer = ModernUIFactory.CreateHorizontalLayout(dialog.transform, 10, new RectOffset(20, 20, 20, 10));
            var tabLayout = tabContainer.AddComponent<LayoutElement>();
            tabLayout.preferredHeight = 50;
            
            var jukeboxTab = ModernUIFactory.CreateModernButton(tabContainer.transform, "Jukebox", null!, ButtonStyle.Secondary);
            var localTab = ModernUIFactory.CreateModernButton(tabContainer.transform, "Local Music", null!, ButtonStyle.Secondary);
            var youtubeTab = ModernUIFactory.CreateModernButton(tabContainer.transform, "YouTube", null!, ButtonStyle.Secondary);
            
            // Add content area for song selection
            var contentArea = ModernUIFactory.CreateCard(dialog.transform, new Vector2(0, 0), false);
            var contentLayout = contentArea.AddComponent<LayoutElement>();
            contentLayout.flexibleHeight = 1;
            
            // For now, show a coming soon message
            var comingSoonText = ModernUIFactory.CreateModernText(contentArea.transform, 
                "Song selection from different sources\nwill be available in the next update!", 
                16, TextStyle.Secondary, TextAnchor.MiddleCenter);
            
            ShowFeedback("Add Songs feature coming soon!", ModernUIFactory.Colors.Primary);
        }
        
        private void PlaySong(NewSongDetails song)
        {
            musicBackend.PlayTrack(song);
            ShowFeedback($"Playing: {song.title}", ModernUIFactory.Colors.Primary);
        }
        
        private void RemoveSong(NewSongDetails song)
        {
            if (currentPlaylist == null || musicBackend == null) return;
            
            try
            {
                musicBackend.RemoveSongFromPlaylist(currentPlaylist.id, song.url);
                playlistSongs?.Remove(song);
                selectedSongs?.Remove(song);
                
                // Remove from UI
                var songItem = songItems?.FirstOrDefault(item => item?.name == $"SongItem_{song.url}");
                if (songItem != null && songItems != null)
                {
                    songItems.Remove(songItem);
                    UnityEngine.Object.Destroy(songItem);
                }
                
                UpdateSongCount();
                hasChanges = true;
                
                ShowFeedback($"Removed: {song.title}", ModernUIFactory.Colors.Primary);
                NewLoggingSystem.Info($"Removed song {song.title} from playlist", "PlaylistEditScreen");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to remove song: {ex}", "PlaylistEditScreen");
                ShowFeedback("Failed to remove song", ModernUIFactory.Colors.Accent);
            }
        }
        
        private void ShowSongContextMenu(NewSongDetails song, Transform relativeTo)
        {
            NewLoggingSystem.Info($"Show context menu for: {song.title}", "PlaylistEditScreen");
            
            // Create the context menu relative to the song item
            var menu = ModernUIFactory.CreateSongContextMenu(this.transform, song, 
                (action, songDetails) => HandleSongContextAction(action, songDetails));
        }
        
        private void HandleSongContextAction(ModernUIFactory.SongContextAction action, NewSongDetails song)
        {
            switch (action)
            {
                case ModernUIFactory.SongContextAction.AddToQueue:
                    musicBackend.AddToQueue(song);
                    ShowFeedback($"Added to queue: {song.title}", ModernUIFactory.Colors.Primary);
                    break;
                    
                case ModernUIFactory.SongContextAction.AddToPlaylist:
                    ShowAddToPlaylistDialog(song);
                    break;
                    
                case ModernUIFactory.SongContextAction.Download:
                    musicBackend.DownloadSong(song);
                    ShowFeedback($"Downloading: {song.title}", ModernUIFactory.Colors.Primary);
                    break;
                    
                case ModernUIFactory.SongContextAction.RemoveFromPlaylist:
                    RemoveSong(song);
                    break;
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void ShowAddToPlaylistDialog(NewSongDetails song)
        {
            if (this.transform == null) return;
            if (musicBackend == null) return;
            // Create a dialog to select which playlist to add the song to
            var dialog = ModernUIFactory.CreateDialog(this.transform, "Add to Playlist", new Vector2(400, 300));
            if (dialog == null) return;
            
            var playlists = musicBackend?.GetAllPlaylists();
            if (playlists == null) return;
            var scrollRect = ModernUIFactory.CreateScrollableContainer(dialog.transform, new Vector2(0, 200));
            var scrollView = scrollRect;
            var content = scrollView?.GetComponentInChildren<VerticalLayoutGroup>()?.gameObject;
            
            foreach (var playlist in playlists)
            {
                if (playlist.id != currentPlaylist?.id) // Don't show current playlist
                {
                    if (content == null) continue;
                    var playlistButton = ModernUIFactory.CreateModernButton(content.transform, 
                        $"{playlist.name} ({playlist.songCount} songs)", 
                        S1Factory.ConvertToUnityAction(() =>
                        {
                            musicBackend.AddSongToPlaylist(playlist.id, song);
                            ShowFeedback($"Added to {playlist.name}", ModernUIFactory.Colors.Primary);
                            UnityEngine.Object.Destroy(dialog);
                        }), ButtonStyle.Secondary);
                    if (playlistButton == null) continue;
                    playlistButton.gameObject.AddComponent<LayoutElement>();
                }
            }
            
            // Add "Create New Playlist" option
            if (content == null) return;
            var createButton = ModernUIFactory.CreateModernButton(content.transform, 
                "+ Create New Playlist", 
                S1Factory.ConvertToUnityAction(() =>
                {
                    // Show create playlist dialog
                    ShowCreatePlaylistDialog(song);
                    UnityEngine.Object.Destroy(dialog);
                }), ButtonStyle.Primary);
        }
        
        private void ShowCreatePlaylistDialog(NewSongDetails song)
        {
            if (this.transform == null) return;
            if (musicBackend == null) return;
            var dialog = ModernUIFactory.CreateDialog(this.transform, "Create Playlist", new Vector2(400, 200));
            if (dialog == null) return;
            
            var nameInput = ModernUIFactory.CreateModernInputField(dialog.transform, "Playlist Name", new Vector2(0, 40));
            if (nameInput == null) return;

            var buttonContainer = ModernUIFactory.CreateHorizontalLayout(dialog.transform, 15, new RectOffset(0, 0, 20, 0));
            if (buttonContainer == null) return;

            var cancelButton = ModernUIFactory.CreateModernButton(buttonContainer.transform, "Cancel", 
                S1Factory.ConvertToUnityAction(() => UnityEngine.Object.Destroy(dialog)), ButtonStyle.Secondary);
            if (cancelButton == null) return;
            
            var createButton = ModernUIFactory.CreateModernButton(buttonContainer.transform, "Create", 
                S1Factory.ConvertToUnityAction(() =>
                {
                    if (!string.IsNullOrEmpty(nameInput.text))
                    {
                        var newPlaylist = musicBackend.CreateNewPlaylist(nameInput.text);
                        if (newPlaylist != null)
                        {
                            musicBackend.AddSongToPlaylist(newPlaylist.id, song);
                            ShowFeedback($"Created playlist '{nameInput.text}' with song", ModernUIFactory.Colors.Primary);
                        }
                    }
                    UnityEngine.Object.Destroy(dialog);
                }), ButtonStyle.Primary);
            if (createButton == null) return;
        }
        
        private void ShowUnsavedChangesDialog()
        {
            ShowConfirmationDialog("You have unsaved changes. Discard them?", () =>
            {
                hasChanges = false;
                navigationManager?.NavigateBack();
            });
        }
        
        private void ShowConfirmationDialog(string message, System.Action onConfirm)
        {
            var dialog = ModernUIFactory.CreateDialog(this.transform, "Confirm", new Vector2(350, 150));
            if (dialog == null) return;
            
            var messageText = ModernUIFactory.CreateModernText(dialog.transform, message, 14, TextStyle.Primary, TextAnchor.MiddleCenter);
            
            var buttonContainer = ModernUIFactory.CreateHorizontalLayout(dialog.transform, 15, new RectOffset(0, 0, 20, 0));
            
            var cancelButton = ModernUIFactory.CreateModernButton(buttonContainer.transform, "Cancel", 
                S1Factory.ConvertToUnityAction(() => UnityEngine.Object.Destroy(dialog)), ButtonStyle.Secondary);
            
            var confirmButton = ModernUIFactory.CreateModernButton(buttonContainer.transform, "Confirm", 
                S1Factory.ConvertToUnityAction(() =>
                {
                    onConfirm?.Invoke();
                    UnityEngine.Object.Destroy(dialog);
                }), ButtonStyle.Danger);
        }
        
        private void ShowFeedback(string message, Color color)
        {
            // Create a temporary feedback message that fades out
            var feedback = ModernUIFactory.CreateModernText(this.transform, message, 14, TextStyle.Primary, TextAnchor.MiddleCenter);
            feedback.color = color;
            
            var feedbackRect = feedback.GetComponent<RectTransform>();
            feedbackRect.anchoredPosition = new Vector2(0, -100);
            
            // Fade out after 2 seconds
            StartCoroutine(FadeOutFeedback(feedback.gameObject));
        }
        
        private System.Collections.IEnumerator FadeOutFeedback(GameObject feedback)
        {
            yield return new WaitForSeconds(2f);
            
            var text = feedback.GetComponent<Text>();
            var startAlpha = text.color.a;
            var timer = 0f;
            var duration = 1f;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                var alpha = Mathf.Lerp(startAlpha, 0f, timer / duration);
                var color = text.color;
                color.a = alpha;
                text.color = color;
                yield return null;
            }
            
            UnityEngine.Object.Destroy(feedback);
        }
        
        private string FormatDuration(int seconds)
        {
            var minutes = seconds / 60;
            var remainingSeconds = seconds % 60;
            return $"{minutes}:{remainingSeconds:D2}";
        }
        
        #endregion
    }
} 