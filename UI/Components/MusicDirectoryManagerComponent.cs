using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.UI.Helpers;

namespace BackSpeakerMod.UI.Components
{
    /// <summary>
    /// Simple music directory manager - list view with edit popup
    /// </summary>
    public class MusicDirectoryManagerComponent : MonoBehaviour
    {
        // Main popup
        private GameObject mainPopup;
        private GameObject directoryListContent;
        private Text titleText;
        private Button addNewButton;
        private Button closeButton;
        
        // Edit popup
        private GameObject editPopup;
        private InputField pathInputField;
        private Text errorText;
        private Button saveButton;
        private Button cancelButton;
        private Text editTitleText;
        
        // Current editing
        private string? editingHash;
        private bool isEditing = false;
        
        // Callbacks
        private Action? onDirectoriesChanged;

        public MusicDirectoryManagerComponent() : base() { }
        
        public void Initialize(Action? onChanged = null)
        {
            onDirectoriesChanged = onChanged;
            CreateMainPopup();
            CreateEditPopup();
            RefreshDirectoryList();
            HidePopup();
        }
        
        public void ShowPopup()
        {
            if (mainPopup != null)
            {
                mainPopup.SetActive(true);
                RefreshDirectoryList();
            }
        }
        
        public void HidePopup()
        {
            if (mainPopup != null) mainPopup.SetActive(false);
            if (editPopup != null) editPopup.SetActive(false);
        }
        
        private void CreateMainPopup()
        {
            // Create the popup container exactly like YouTube popup - make it fill the parent container
            mainPopup = new GameObject("DirectoryManagerPopup");
            mainPopup.transform.SetParent(this.transform.parent, false);  // Use parent exactly like YouTube popup
            
            var popupRect = mainPopup.AddComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;

            // Make sure it appears on top
            mainPopup.transform.SetAsLastSibling();

            // Add background exactly like YouTube popup
            var bgImage = mainPopup.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // Create main content area exactly like YouTube popup
            var mainPanel = new GameObject("DirectoryContentArea");
            mainPanel.transform.SetParent(mainPopup.transform, false);
            
            var mainPanelRect = mainPanel.AddComponent<RectTransform>();
            mainPanelRect.anchorMin = new Vector2(0.1f, 0.1f);
            mainPanelRect.anchorMax = new Vector2(0.9f, 0.9f);
            mainPanelRect.offsetMin = Vector2.zero;
            mainPanelRect.offsetMax = Vector2.zero;
            
            // Add background to main panel so it's visible
            var mainPanelImage = mainPanel.AddComponent<Image>();
            mainPanelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
            
            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(mainPanel.transform, false);
            titleText = titleObj.AddComponent<Text>();
            titleText.text = "Music Directories";
            FontHelper.SetSafeFont(titleText);
            titleText.fontSize = 24;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            var titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.9f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(20, -10);
            titleRect.offsetMax = new Vector2(-60, -10);
            
            // Close button (X)
            closeButton = CreateButton("X", mainPanel.transform, OnCloseClicked);
            var closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 0.9f);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.offsetMin = new Vector2(-50, -10);
            closeRect.offsetMax = new Vector2(-10, -10);
            closeButton.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f);
            
            // Scroll view for directory list
            var scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(mainPanel.transform, false);
            var scrollRect = scrollView.AddComponent<ScrollRect>();
            var scrollImage = scrollView.AddComponent<Image>();
            scrollImage.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);
            var scrollViewRect = scrollView.GetComponent<RectTransform>();
            scrollViewRect.anchorMin = new Vector2(0, 0.15f); // Adjusted for larger button panel
            scrollViewRect.anchorMax = new Vector2(1, 0.88f);
            scrollViewRect.offsetMin = new Vector2(20, 10);
            scrollViewRect.offsetMax = new Vector2(-20, -10);
            
            // Content for scroll view
            directoryListContent = new GameObject("Content");
            directoryListContent.transform.SetParent(scrollView.transform, false);
            var contentRect = directoryListContent.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            
            var contentLayout = directoryListContent.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 10;
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandWidth = true;
            
            var contentSizeFitter = directoryListContent.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            
            // Bottom buttons
            var buttonPanel = new GameObject("ButtonPanel");
            buttonPanel.transform.SetParent(mainPanel.transform, false);
            var buttonPanelRect = buttonPanel.GetComponent<RectTransform>();
            buttonPanelRect.anchorMin = new Vector2(0, 0);
            buttonPanelRect.anchorMax = new Vector2(1, 0.15f); // Made slightly larger
            buttonPanelRect.offsetMin = new Vector2(20, 10);
            buttonPanelRect.offsetMax = new Vector2(-20, -10);
            
            // Add background to button panel so it's visible
            var buttonPanelImage = buttonPanel.AddComponent<Image>();
            buttonPanelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            var buttonLayout = buttonPanel.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 20;
            buttonLayout.childControlWidth = false;
            buttonLayout.childControlHeight = true;
            buttonLayout.childForceExpandHeight = true;
            buttonLayout.childAlignment = TextAnchor.MiddleCenter;
            
            // Add New button
            addNewButton = CreateButton("Add New Directory", buttonPanel.transform, OnAddNewClicked);
            var addNewRect = addNewButton.GetComponent<RectTransform>();
            addNewRect.sizeDelta = new Vector2(200, 40);
            addNewButton.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f);
            
            mainPopup.SetActive(false);
        }
        
        private void CreateEditPopup()
        {
            if (mainPopup == null) return;
            
            // Edit popup (child of main popup)
            editPopup = new GameObject("EditDirectoryPopup");
            editPopup.transform.SetParent(mainPopup.transform, false);
            
            var editPopupRect = editPopup.AddComponent<RectTransform>();
            editPopupRect.anchorMin = Vector2.zero;
            editPopupRect.anchorMax = Vector2.one;
            editPopupRect.offsetMin = Vector2.zero;
            editPopupRect.offsetMax = Vector2.zero;
            
            var editBg = editPopup.AddComponent<Image>();
            editBg.color = new Color(0f, 0f, 0f, 0.5f);
            editBg.raycastTarget = true;
            
            // Edit panel
            var editPanel = new GameObject("EditPanel");
            editPanel.transform.SetParent(editPopup.transform, false);
            var editPanelImage = editPanel.AddComponent<Image>();
            editPanelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
            var editPanelRect = editPanel.GetComponent<RectTransform>();
            editPanelRect.anchorMin = new Vector2(0.3f, 0.4f);
            editPanelRect.anchorMax = new Vector2(0.7f, 0.6f);
            editPanelRect.offsetMin = Vector2.zero;
            editPanelRect.offsetMax = Vector2.zero;
            
            // Edit title
            var editTitleObj = new GameObject("EditTitle");
            editTitleObj.transform.SetParent(editPanel.transform, false);
            editTitleText = editTitleObj.AddComponent<Text>();
            editTitleText.text = "Edit Directory Path";
            FontHelper.SetSafeFont(editTitleText);
            editTitleText.fontSize = 18;
            editTitleText.color = Color.white;
            editTitleText.alignment = TextAnchor.MiddleCenter;
            var editTitleRect = editTitleObj.GetComponent<RectTransform>();
            editTitleRect.anchorMin = new Vector2(0, 0.7f);
            editTitleRect.anchorMax = new Vector2(1, 0.9f);
            editTitleRect.offsetMin = new Vector2(20, 0);
            editTitleRect.offsetMax = new Vector2(-20, 0);
            
            // Close button for edit popup
            var editCloseButton = CreateButton("✕", editPanel.transform, OnCancelClicked);
            var editCloseRect = editCloseButton.GetComponent<RectTransform>();
            editCloseRect.anchorMin = new Vector2(1, 0.7f);
            editCloseRect.anchorMax = new Vector2(1, 0.9f);
            editCloseRect.offsetMin = new Vector2(-50, 0);
            editCloseRect.offsetMax = new Vector2(-20, 0);
            editCloseButton.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f);
            
            // Path input field
            var inputObj = new GameObject("PathInput");
            inputObj.transform.SetParent(editPanel.transform, false);
            pathInputField = inputObj.AddComponent<InputField>();
            var inputImage = inputObj.AddComponent<Image>();
            inputImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            var inputRect = inputObj.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0, 0.45f);
            inputRect.anchorMax = new Vector2(1, 0.65f);
            inputRect.offsetMin = new Vector2(20, 0);
            inputRect.offsetMax = new Vector2(-20, 0);
            
            // Input text
            var inputTextObj = new GameObject("Text");
            inputTextObj.transform.SetParent(inputObj.transform, false);
            var inputText = inputTextObj.AddComponent<Text>();
            FontHelper.SetSafeFont(inputText);
            inputText.fontSize = 14;
            inputText.color = Color.white;
            inputText.supportRichText = false;
            var inputTextRect = inputTextObj.GetComponent<RectTransform>();
            inputTextRect.anchorMin = Vector2.zero;
            inputTextRect.anchorMax = Vector2.one;
            inputTextRect.offsetMin = new Vector2(10, 0);
            inputTextRect.offsetMax = new Vector2(-10, 0);
            
            pathInputField.textComponent = inputText;
            pathInputField.text = "";
            
            // Add input field manager for keybind handling
            pathInputField.onValueChanged.AddListener((UnityEngine.Events.UnityAction<string>)OnPathInputChanged);
            
            // Error text
            var errorObj = new GameObject("ErrorText");
            errorObj.transform.SetParent(editPanel.transform, false);
            errorText = errorObj.AddComponent<Text>();
            FontHelper.SetSafeFont(errorText);
            errorText.fontSize = 12;
            errorText.color = Color.red;
            errorText.alignment = TextAnchor.MiddleCenter;
            errorText.text = "";
            var errorRect = errorObj.GetComponent<RectTransform>();
            errorRect.anchorMin = new Vector2(0, 0.25f);
            errorRect.anchorMax = new Vector2(1, 0.4f);
            errorRect.offsetMin = new Vector2(20, 0);
            errorRect.offsetMax = new Vector2(-20, 0);
            
            // Edit buttons
            var editButtonPanel = new GameObject("EditButtonPanel");
            editButtonPanel.transform.SetParent(editPanel.transform, false);
            var editButtonPanelRect = editButtonPanel.GetComponent<RectTransform>();
            editButtonPanelRect.anchorMin = new Vector2(0, 0);
            editButtonPanelRect.anchorMax = new Vector2(1, 0.25f);
            editButtonPanelRect.offsetMin = new Vector2(20, 10);
            editButtonPanelRect.offsetMax = new Vector2(-20, -10);
            
            var editButtonLayout = editButtonPanel.AddComponent<HorizontalLayoutGroup>();
            editButtonLayout.spacing = 20;
            editButtonLayout.childControlWidth = true;
            editButtonLayout.childControlHeight = true;
            editButtonLayout.childForceExpandWidth = true;
            editButtonLayout.childForceExpandHeight = true;
            
            // Save and Cancel buttons
            saveButton = CreateButton("Save", editButtonPanel.transform, OnSaveClicked);
            saveButton.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f);
            
            cancelButton = CreateButton("Cancel", editButtonPanel.transform, OnCancelClicked);
            cancelButton.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f);
            
            editPopup.SetActive(false);
        }
        
        private Button CreateButton(string text, Transform parent, System.Action onClick)
        {
            var buttonObj = new GameObject($"Button_{text}");
            buttonObj.transform.SetParent(parent, false);
            
            var button = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.3f, 0.3f);
            
            var buttonText = new GameObject("Text");
            buttonText.transform.SetParent(buttonObj.transform, false);
            var textComponent = buttonText.AddComponent<Text>();
            textComponent.text = text;
            FontHelper.SetSafeFont(textComponent);
            textComponent.fontSize = 14;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            var textRect = buttonText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            button.onClick.AddListener((UnityEngine.Events.UnityAction)onClick);
            return button;
        }
        
        private void RefreshDirectoryList()
        {
            if (directoryListContent == null) return;
            
            // Clear existing entries
            for (int i = directoryListContent.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(directoryListContent.transform.GetChild(i).gameObject);
            }
            
            // Get directories
            var config = MusicDirectoryConfig.Instance;
            config.ValidateDirectories();
            var directories = config.GetAllDirectoriesWithHashes();
            
            // Create entries
            foreach (var kvp in directories.OrderBy(d => d.Value.IsDefault ? 0 : 1).ThenBy(d => d.Value.Path))
            {
                CreateDirectoryEntry(kvp.Key, kvp.Value);
            }
        }
        
        private void CreateDirectoryEntry(string pathHash, MusicDirectory directory)
        {
            var entryObj = new GameObject("DirectoryEntry");
            entryObj.transform.SetParent(directoryListContent.transform, false);
            
            var entryRect = entryObj.AddComponent<RectTransform>();
            entryRect.sizeDelta = new Vector2(0, 70); // Proper height for clean layout
            
            var entryImage = entryObj.AddComponent<Image>();
            entryImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            var entryLayout = entryObj.AddComponent<HorizontalLayoutGroup>();
            entryLayout.spacing = 15;
            entryLayout.padding = new RectOffset(15, 15, 10, 10);
            entryLayout.childControlWidth = false;
            entryLayout.childControlHeight = true;
            entryLayout.childForceExpandHeight = true;
            entryLayout.childAlignment = TextAnchor.MiddleLeft;
            
            // Directory path info - takes most of the space
            var infoPanel = new GameObject("InfoPanel");
            infoPanel.transform.SetParent(entryObj.transform, false);
            var infoPanelLayout = infoPanel.AddComponent<LayoutElement>();
            infoPanelLayout.flexibleWidth = 1;
            infoPanelLayout.minWidth = 250;
            
            var infoVerticalLayout = infoPanel.AddComponent<VerticalLayoutGroup>();
            infoVerticalLayout.spacing = 3;
            infoVerticalLayout.childControlHeight = false;
            infoVerticalLayout.childControlWidth = true;
            infoVerticalLayout.childForceExpandWidth = true;
            
            // Path text
            var pathObj = new GameObject("Path");
            pathObj.transform.SetParent(infoPanel.transform, false);
            var pathText = pathObj.AddComponent<Text>();
            FontHelper.SetSafeFont(pathText);
            pathText.fontSize = 14;
            pathText.color = Color.white;
            pathText.text = directory.Path + (directory.IsDefault ? " (Default)" : "");
            pathText.fontStyle = directory.IsDefault ? FontStyle.Bold : FontStyle.Normal;
            pathText.alignment = TextAnchor.MiddleLeft;
            
            // Status text
            var statusObj = new GameObject("Status");
            statusObj.transform.SetParent(infoPanel.transform, false);
            var statusText = statusObj.AddComponent<Text>();
            FontHelper.SetSafeFont(statusText);
            statusText.fontSize = 11;
            statusText.alignment = TextAnchor.MiddleLeft;
            
            string statusDisplay;
            if (!Directory.Exists(directory.Path))
            {
                statusDisplay = "❌ Directory Not Found";
                statusText.color = Color.red;
            }
            else if (directory.IsEnabled)
            {
                statusDisplay = $"✅ Enabled • {directory.FileCount} files";
                statusText.color = Color.green;
            }
            else
            {
                statusDisplay = $"⚠️ Disabled • {directory.FileCount} files";
                statusText.color = Color.yellow;
            }
            statusText.text = statusDisplay;
            
            // Button section - fixed width
            var buttonPanel = new GameObject("ButtonPanel");
            buttonPanel.transform.SetParent(entryObj.transform, false);
            var buttonPanelLayout = buttonPanel.AddComponent<LayoutElement>();
            buttonPanelLayout.minWidth = directory.IsDefault ? 100 : 200;
            
            var buttonHorizontalLayout = buttonPanel.AddComponent<HorizontalLayoutGroup>();
            buttonHorizontalLayout.spacing = 8;
            buttonHorizontalLayout.childControlWidth = false;
            buttonHorizontalLayout.childControlHeight = true;
            buttonHorizontalLayout.childForceExpandHeight = true;
            buttonHorizontalLayout.childAlignment = TextAnchor.MiddleCenter;
            
            if (directory.IsDefault)
            {
                // Show "Always On" for default directory
                var alwaysEnabledButton = CreateButton("Always On", buttonPanel.transform, null);
                alwaysEnabledButton.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 35);
                alwaysEnabledButton.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f);
                alwaysEnabledButton.interactable = false;
                var alwaysText = alwaysEnabledButton.GetComponentInChildren<Text>();
                if (alwaysText != null) alwaysText.fontSize = 11;
            }
            else
            {
                // Edit button
                var editButton = CreateButton("Edit", buttonPanel.transform, () => OnEditClicked(pathHash, directory));
                editButton.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 35);
                editButton.GetComponent<Image>().color = new Color(0.3f, 0.5f, 0.7f);
                var editText = editButton.GetComponentInChildren<Text>();
                if (editText != null) editText.fontSize = 12;
                
                // Enable/Disable toggle
                string toggleText = directory.IsEnabled ? "Disable" : "Enable";
                var toggleButton = CreateButton(toggleText, buttonPanel.transform, () => OnToggleClicked(pathHash, directory));
                toggleButton.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 35);
                toggleButton.GetComponent<Image>().color = directory.IsEnabled ? 
                    new Color(0.7f, 0.5f, 0.2f) : new Color(0.2f, 0.6f, 0.2f);
                var toggleText_comp = toggleButton.GetComponentInChildren<Text>();
                if (toggleText_comp != null) toggleText_comp.fontSize = 12;
                
                // Remove button
                var removeButton = CreateButton("Remove", buttonPanel.transform, () => OnRemoveClicked(pathHash, directory));
                removeButton.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 35);
                removeButton.GetComponent<Image>().color = new Color(0.7f, 0.2f, 0.2f);
                var removeText = removeButton.GetComponentInChildren<Text>();
                if (removeText != null) removeText.fontSize = 12;
            }
        }
        
        // Event handlers
        private void OnAddNewClicked()
        {
            // Ensure edit popup is created
            if (editPopup == null)
            {
                LoggingSystem.Error("Edit popup not created! Cannot add new directory.", "DirectoryManager");
                return;
            }
            
            editingHash = null;
            isEditing = false;
            
            if (editTitleText != null)
                editTitleText.text = "Add New Directory";
            
            if (pathInputField != null)
                pathInputField.text = "";
            
            if (errorText != null)
                errorText.text = "";
            
            editPopup.SetActive(true);
            
            // Focus input field and enable input field manager
            if (pathInputField != null)
            {
                pathInputField.ActivateInputField();
                InputFieldManager.SetupInputField(pathInputField);
            }
        }
        
        private void OnEditClicked(string pathHash, MusicDirectory directory)
        {
            // Ensure edit popup is created
            if (editPopup == null)
            {
                LoggingSystem.Error("Edit popup not created! Cannot edit directory.", "DirectoryManager");
                return;
            }
            
            editingHash = pathHash;
            isEditing = true;
            
            if (editTitleText != null)
                editTitleText.text = "Edit Directory Path";
            
            if (pathInputField != null)
                pathInputField.text = directory.Path;
            
            if (errorText != null)
                errorText.text = "";
            
            editPopup.SetActive(true);
            
            // Focus input field and enable input field manager
            if (pathInputField != null)
            {
                pathInputField.ActivateInputField();
                InputFieldManager.SetupInputField(pathInputField);
            }
        }
        
        private void OnToggleClicked(string pathHash, MusicDirectory directory)
        {
            // Don't allow toggling default directory - it should always be enabled
            if (directory.IsDefault)
            {
                LoggingSystem.Warning("Cannot disable default directory", "DirectoryManager");
                return;
            }
            
            var config = MusicDirectoryConfig.Instance;
            config.SetDirectoryEnabledByHash(pathHash, !directory.IsEnabled);
            
            LoggingSystem.Info($"Directory {directory.Path} {(!directory.IsEnabled ? "enabled" : "disabled")}", "DirectoryManager");
            
            RefreshDirectoryList();
            
            // Reload tracks immediately
            onDirectoriesChanged?.Invoke();
        }
        
        private void OnRemoveClicked(string pathHash, MusicDirectory directory)
        {
            // Don't allow removing default directory
            if (directory.IsDefault)
            {
                LoggingSystem.Warning("Cannot remove default directory", "DirectoryManager");
                return;
            }
            
            var config = MusicDirectoryConfig.Instance;
            if (config.RemoveDirectoryByHash(pathHash, out string error))
            {
                LoggingSystem.Info($"Removed directory: {directory.Path}", "DirectoryManager");
                RefreshDirectoryList();
                onDirectoriesChanged?.Invoke();
            }
            else
            {
                LoggingSystem.Warning($"Failed to remove directory: {error}", "DirectoryManager");
            }
        }
        
        private void OnSaveClicked()
        {
            var path = pathInputField.text.Trim();
            
            if (string.IsNullOrEmpty(path))
            {
                errorText.text = "Path cannot be empty";
                return;
            }
            
            // Validate path exists
            if (!Directory.Exists(path))
            {
                errorText.text = "Directory does not exist";
                return;
            }
            
            var config = MusicDirectoryConfig.Instance;
            
            if (isEditing && editingHash != null)
            {
                // Remove old entry and add new one
                if (config.RemoveDirectoryByHash(editingHash, out string removeError))
                {
                    if (config.AddDirectory(path, out string addError))
                    {
                        editPopup.SetActive(false);
                        RefreshDirectoryList();
                        onDirectoriesChanged?.Invoke();
                        // Input field cleanup handled by InputFieldManager.Reset()
                    }
                    else
                    {
                        errorText.text = addError;
                        // Restore the old directory if add failed
                        var oldDir = config.GetAllDirectoriesWithHashes().Values.FirstOrDefault();
                        if (oldDir != null)
                        {
                            config.AddDirectory(oldDir.Path, out _, oldDir.Name, oldDir.Description);
                        }
                    }
                }
                else
                {
                    errorText.text = removeError;
                }
            }
            else
            {
                // Add new directory
                if (config.AddDirectory(path, out string error))
                {
                    editPopup.SetActive(false);
                    RefreshDirectoryList();
                    onDirectoriesChanged?.Invoke();
                    // Input field cleanup handled by InputFieldManager.Reset()
                }
                else
                {
                    errorText.text = error;
                }
            }
        }
        
        private void OnCancelClicked()
        {
            editPopup.SetActive(false);
            // Input field cleanup handled by InputFieldManager.Reset()
        }
        
        private void OnCloseClicked()
        {
            HidePopup();
        }
        
        private void OnPathInputChanged(string value)
        {
            // Clear error when user starts typing
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(errorText.text))
            {
                errorText.text = "";
            }
        }
        
        private void OnDestroy()
        {
            if (pathInputField != null)
            {
                // Input field cleanup handled by InputFieldManager.Reset()
            }
            
            if (mainPopup != null)
            {
                Destroy(mainPopup);
            }
        }
    }
} 