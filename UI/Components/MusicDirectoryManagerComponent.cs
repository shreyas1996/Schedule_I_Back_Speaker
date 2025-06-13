using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Common.Managers;
using BackSpeakerMod.UI.Helpers;

namespace BackSpeakerMod.UI.Components
{
    /// <summary>
    /// Simple music directory manager - list view with edit popup
    /// </summary>
    public class MusicDirectoryManagerComponent : MonoBehaviour
    {
        // Main popup
        private GameObject? mainPopup;
        private bool isMainPopupOpen = false;
        
        // Edit popup (secondary popup)
        private GameObject? editPopup;
        private bool isEditPopupOpen = false;
        
        // UI elements
        private GameObject? directoryListContent;
        private InputField? pathInputField;
        private Text? errorText;
        private Text? editTitleText;
        
        // Edit state
        private string? editingHash;
        private bool isEditing = false;
        
        // Callback for when directories change
        private Action? onDirectoriesChanged;

        public MusicDirectoryManagerComponent() : base() { }
        
        public void Initialize(Action? onChanged = null)
        {
            try
            {
                LoggingSystem.Info("Initializing MusicDirectoryManagerComponent...", "DirectoryManager");
                onDirectoriesChanged = onChanged;
                
                LoggingSystem.Info("Creating main popup...", "DirectoryManager");
                CreateMainPopup();
                
                LoggingSystem.Info("Creating edit popup...", "DirectoryManager");
                CreateEditPopup();
                
                LoggingSystem.Info("MusicDirectoryManagerComponent initialized successfully", "DirectoryManager");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to initialize MusicDirectoryManagerComponent: {ex.Message}", "DirectoryManager");
                LoggingSystem.Error($"Stack trace: {ex.StackTrace}", "DirectoryManager");
                throw;
            }
        }
        
        public void ShowPopup()
        {
            if (mainPopup != null)
            {
                RefreshDirectoryList();
                mainPopup.SetActive(true);
                isMainPopupOpen = true;
            }
        }
        
        public void HidePopup()
        {
            if (mainPopup != null)
            {
                mainPopup.SetActive(false);
                isMainPopupOpen = false;
            }
            CloseEditPopup();
        }
        
        private void CreateMainPopup()
        {
            try
            {
                LoggingSystem.Info("Creating directory manager popup...", "DirectoryManager");
                
                // Find our app's container exactly like PlaylistToggleComponent
                Transform? appContainer = null;
                Transform current = this.transform;
                
                LoggingSystem.Info($"Starting container search from: {current?.name ?? "null"}", "DirectoryManager");
                
                // Walk up the hierarchy to find "Container" (our app's container)
                while (current != null && appContainer == null)
                {
                    LoggingSystem.Debug($"Checking transform: {current.name}", "DirectoryManager");
                    if (current.name == "Container")
                    {
                        appContainer = current;
                        break;
                    }
                    current = current.parent;
                }
                
                // If no container found, try to find BackSpeakerApp canvas
                if (appContainer == null)
                {
                    LoggingSystem.Info("Container not found in hierarchy, trying BackSpeakerApp approach", "DirectoryManager");
                    current = this.transform;
                    while (current != null)
                    {
                        LoggingSystem.Debug($"Checking for BackSpeakerApp: {current.name}", "DirectoryManager");
                        if (current.name == "BackSpeakerApp")
                        {
                            // Look for Container child
                            var containerChild = current.FindChild("Container");
                            if (containerChild != null)
                            {
                                appContainer = containerChild;
                                break;
                            }
                        }
                        current = current.parent;
                    }
                }
                
                if (appContainer == null)
                {
                    LoggingSystem.Error("No app Container found for directory manager popup! This will cause UI bleeding.", "DirectoryManager");
                    return;
                }
                
                LoggingSystem.Info($"Found app Container: {appContainer.name}", "DirectoryManager");
            
                // Create popup that covers the entire app container (not the whole screen)
                LoggingSystem.Info("Creating main popup GameObject", "DirectoryManager");
                mainPopup = new GameObject("DirectoryManagerPopup");
                mainPopup.transform.SetParent(appContainer, false);
                
                LoggingSystem.Info("Setting up popup RectTransform", "DirectoryManager");
                var popupRect = mainPopup.AddComponent<RectTransform>();
                popupRect.anchorMin = Vector2.zero;
                popupRect.anchorMax = Vector2.one;
                popupRect.offsetMin = Vector2.zero;
                popupRect.offsetMax = Vector2.zero;
                popupRect.anchoredPosition = Vector2.zero;
                popupRect.sizeDelta = Vector2.zero;
                
                LoggingSystem.Info("Adding popup background", "DirectoryManager");
                // Semi-transparent background that blocks clicks
                var popupBg = mainPopup.AddComponent<Image>();
                popupBg.color = new Color(0f, 0f, 0f, 0.8f);
                popupBg.raycastTarget = true; // Block clicks behind popup
                
                // Make sure popup appears on top within our container
                mainPopup.transform.SetAsLastSibling();
                
                LoggingSystem.Info("Directory manager popup background created within app container", "DirectoryManager");
                
                LoggingSystem.Info("Creating main panel", "DirectoryManager");
                // Main panel - using exact same coordinates as PlaylistToggleComponent
                var mainPanel = new GameObject("DirectoryPanel");
                mainPanel.transform.SetParent(mainPopup.transform, false);
                
                var panelRect = mainPanel.AddComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.1f, 0.1f);
                panelRect.anchorMax = new Vector2(0.9f, 0.9f);
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                
                var panelBg = mainPanel.AddComponent<Image>();
                panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
                
                LoggingSystem.Info("Directory panel created", "DirectoryManager");
                
                LoggingSystem.Info("Creating directory content", "DirectoryManager");
                // Create directory content
                CreateDirectoryContent(mainPanel);
                
                LoggingSystem.Info("Setting popup to hidden", "DirectoryManager");
                // Start hidden
                mainPopup.SetActive(false);
                
                LoggingSystem.Info("Directory manager popup creation completed", "DirectoryManager");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error in CreateMainPopup: {ex.Message}", "DirectoryManager");
                LoggingSystem.Error($"Stack trace: {ex.StackTrace}", "DirectoryManager");
                throw;
            }
        }
        
        private void CreateDirectoryContent(GameObject panel)
        {
            // Header with title
            var header = new GameObject("Header");
            header.transform.SetParent(panel.transform, false);
            
            var headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 0.9f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.offsetMin = new Vector2(10f, 0f);
            headerRect.offsetMax = new Vector2(-10f, 0f);
            
            // Title text
            var titleText = new GameObject("TitleText");
            titleText.transform.SetParent(header.transform, false);
            
            var titleTextRect = titleText.AddComponent<RectTransform>();
            titleTextRect.anchorMin = new Vector2(0f, 0f);
            titleTextRect.anchorMax = new Vector2(0.85f, 1f);
            titleTextRect.offsetMin = Vector2.zero;
            titleTextRect.offsetMax = Vector2.zero;
            
            var titleTextComponent = titleText.AddComponent<Text>();
            titleTextComponent.text = "Music Directories";
            FontHelper.SetSafeFont(titleTextComponent);
            titleTextComponent.fontSize = 18;
            titleTextComponent.color = Color.white;
            titleTextComponent.alignment = TextAnchor.MiddleLeft;
            titleTextComponent.fontStyle = FontStyle.Bold;
            
            // Close button - exact same as PlaylistToggleComponent
            var closeButton = new GameObject("CloseButton");
            closeButton.transform.SetParent(header.transform, false);
            
            var closeRect = closeButton.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.9f, 0f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            
            var closeBtn = closeButton.AddComponent<Button>();
            var closeBtnImage = closeButton.AddComponent<Image>();
            closeBtnImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            
            var closeText = new GameObject("Text");
            closeText.transform.SetParent(closeButton.transform, false);
            var closeTextRect = closeText.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;
            
            var closeTextComponent = closeText.AddComponent<Text>();
            closeTextComponent.text = "Close";
            FontHelper.SetSafeFont(closeTextComponent);
            closeTextComponent.fontSize = 10;
            closeTextComponent.color = Color.white;
            closeTextComponent.alignment = TextAnchor.MiddleCenter;
            closeTextComponent.fontStyle = FontStyle.Bold;
            
            closeBtn.targetGraphic = closeBtnImage;
            closeBtn.onClick.AddListener((UnityEngine.Events.UnityAction)delegate() { HidePopup(); });
            
            // Add New Directory button area
            CreateAddNewButton(panel);
            
            // Directory list area
            CreateDirectoryList(panel);
        }
        
        private void CreateAddNewButton(GameObject panel)
        {
            var addNewContainer = new GameObject("AddNewContainer");
            addNewContainer.transform.SetParent(panel.transform, false);
            
            var containerRect = addNewContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0f, 0.82f);
            containerRect.anchorMax = new Vector2(1f, 0.88f);
            containerRect.offsetMin = new Vector2(10f, 0f);
            containerRect.offsetMax = new Vector2(-10f, 0f);
            
            // Add New Directory button
            var addNewButton = new GameObject("AddNewButton");
            addNewButton.transform.SetParent(addNewContainer.transform, false);
            
            var addNewRect = addNewButton.AddComponent<RectTransform>();
            addNewRect.anchorMin = new Vector2(0f, 0f);
            addNewRect.anchorMax = new Vector2(1f, 1f);
            addNewRect.offsetMin = Vector2.zero;
            addNewRect.offsetMax = Vector2.zero;
            
            var addNewBtn = addNewButton.AddComponent<Button>();
            var addNewBtnImage = addNewButton.AddComponent<Image>();
            addNewBtnImage.color = new Color(0.2f, 0.6f, 0.2f, 0.8f);
            
            var addNewText = new GameObject("Text");
            addNewText.transform.SetParent(addNewButton.transform, false);
            var addNewTextRect = addNewText.AddComponent<RectTransform>();
            addNewTextRect.anchorMin = Vector2.zero;
            addNewTextRect.anchorMax = Vector2.one;
            addNewTextRect.offsetMin = Vector2.zero;
            addNewTextRect.offsetMax = Vector2.zero;
            
            var addNewTextComponent = addNewText.AddComponent<Text>();
            addNewTextComponent.text = "Add New Directory";
            FontHelper.SetSafeFont(addNewTextComponent);
            addNewTextComponent.fontSize = 12;
            addNewTextComponent.color = Color.white;
            addNewTextComponent.alignment = TextAnchor.MiddleCenter;
            addNewTextComponent.fontStyle = FontStyle.Bold;
            
            addNewBtn.targetGraphic = addNewBtnImage;
            addNewBtn.onClick.AddListener((UnityEngine.Events.UnityAction)delegate() { OnAddNewClicked(); });
        }
        
        private void CreateDirectoryList(GameObject panel)
        {
            // Directory list area - similar to track list in PlaylistToggleComponent
            var listContainer = new GameObject("DirectoryListContainer");
            listContainer.transform.SetParent(panel.transform, false);
            
            var listRect = listContainer.AddComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0f, 0.05f);
            listRect.anchorMax = new Vector2(1f, 0.8f);
            listRect.offsetMin = new Vector2(10f, 0f);
            listRect.offsetMax = new Vector2(-10f, 0f);
            
            var listBg = listContainer.AddComponent<Image>();
            listBg.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);
            
            // Create scroll view
            var scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(listContainer.transform, false);
            
            var scrollRect = scrollView.AddComponent<ScrollRect>();
            var scrollRectTransform = scrollView.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.offsetMin = new Vector2(5f, 5f);
            scrollRectTransform.offsetMax = new Vector2(-5f, -5f);
            
            // Viewport
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            
            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            
            var viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;
            
            // Content
            directoryListContent = new GameObject("Content");
            directoryListContent.transform.SetParent(viewport.transform, false);
            
            var contentRect = directoryListContent.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            var contentLayout = directoryListContent.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 5f;
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandWidth = true;
            
            var contentSizeFitter = directoryListContent.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Setup scroll rect
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 20f;
        }
        
        private void CreateEditPopup()
        {
            if (mainPopup == null) return;
            
            // Find our app's container exactly like PlaylistToggleComponent secondary popup
            Transform? appContainer = null;
            Transform current = this.transform;
            
            // Walk up the hierarchy to find "Container" (our app's container)
            while (current != null && appContainer == null)
            {
                if (current.name == "Container")
                {
                    appContainer = current;
                    break;
                }
                current = current.parent;
            }
            
            // If no container found, try to find BackSpeakerApp canvas
            if (appContainer == null)
            {
                current = this.transform;
                while (current != null)
                {
                    if (current.name == "BackSpeakerApp")
                    {
                        // Look for Container child
                        var containerChild = current.FindChild("Container");
                        if (containerChild != null)
                        {
                            appContainer = containerChild;
                            break;
                        }
                    }
                    current = current.parent;
                }
            }
            
            if (appContainer == null)
            {
                LoggingSystem.Error("No app Container found for edit popup! This will cause UI bleeding.", "UI");
                return;
            }
            
            LoggingSystem.Info($"Found app Container for edit popup: {appContainer.name}", "UI");
            
            // Create popup background - exact same as PlaylistToggleComponent secondary popup
            editPopup = new GameObject("EditDirectoryPopup");
            editPopup.transform.SetParent(appContainer, false);
            
            // Full screen background with transparency
            var popupRect = editPopup.AddComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;
            popupRect.anchoredPosition = Vector2.zero;
            popupRect.sizeDelta = Vector2.zero;
            
            // Semi-transparent background
            var backgroundImage = editPopup.AddComponent<Image>();
            backgroundImage.color = new Color(0f, 0f, 0f, 0.5f);
            
            // Make sure popup appears on top within our container
            editPopup.transform.SetAsLastSibling();
            
            // Create the main popup panel - exact same coordinates as PlaylistToggleComponent
            var popup = new GameObject("Popup");
            popup.transform.SetParent(editPopup.transform, false);
            
            var panelRect = popup.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.15f, 0.15f);
            panelRect.anchorMax = new Vector2(0.95f, 0.85f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            var panelImage = popup.AddComponent<Image>();
            panelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
            
            // Title bar - exact same as PlaylistToggleComponent
            var titleBar = new GameObject("TitleBar");
            titleBar.transform.SetParent(popup.transform, false);
            
            var titleBarRect = titleBar.AddComponent<RectTransform>();
            titleBarRect.anchorMin = new Vector2(0f, 0.9f);
            titleBarRect.anchorMax = new Vector2(1f, 1f);
            titleBarRect.offsetMin = Vector2.zero;
            titleBarRect.offsetMax = Vector2.zero;
            
            var titleBarImage = titleBar.AddComponent<Image>();
            titleBarImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            // Title text
            var titleText = new GameObject("TitleText");
            titleText.transform.SetParent(titleBar.transform, false);
            
            var titleTextRect = titleText.AddComponent<RectTransform>();
            titleTextRect.anchorMin = new Vector2(0.05f, 0f);
            titleTextRect.anchorMax = new Vector2(0.85f, 1f);
            titleTextRect.offsetMin = Vector2.zero;
            titleTextRect.offsetMax = Vector2.zero;
            
            editTitleText = titleText.AddComponent<Text>();
            editTitleText.text = "Edit Directory Path";
            FontHelper.SetSafeFont(editTitleText);
            editTitleText.fontSize = 14;
            editTitleText.color = Color.white;
            editTitleText.alignment = TextAnchor.MiddleLeft;
            editTitleText.fontStyle = FontStyle.Bold;
            
            // Close button - exact same as PlaylistToggleComponent
            var closeButton = new GameObject("CloseButton");
            closeButton.transform.SetParent(titleBar.transform, false);
            
            var closeBtnRect = closeButton.AddComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.9f, 0.15f);
            closeBtnRect.anchorMax = new Vector2(0.98f, 0.85f);
            closeBtnRect.offsetMin = Vector2.zero;
            closeBtnRect.offsetMax = Vector2.zero;
            
            var closeBtnComponent = closeButton.AddComponent<Button>();
            var closeBtnImage = closeButton.AddComponent<Image>();
            closeBtnImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            
            var closeBtnText = new GameObject("Text");
            closeBtnText.transform.SetParent(closeButton.transform, false);
            var closeBtnTextRect = closeBtnText.AddComponent<RectTransform>();
            closeBtnTextRect.anchorMin = Vector2.zero;
            closeBtnTextRect.anchorMax = Vector2.one;
            closeBtnTextRect.offsetMin = Vector2.zero;
            closeBtnTextRect.offsetMax = Vector2.zero;
            
            var closeBtnTextComponent = closeBtnText.AddComponent<Text>();
            closeBtnTextComponent.text = "✕";
            FontHelper.SetSafeFont(closeBtnTextComponent);
            closeBtnTextComponent.fontSize = 12;
            closeBtnTextComponent.color = Color.white;
            closeBtnTextComponent.alignment = TextAnchor.MiddleCenter;
            closeBtnTextComponent.fontStyle = FontStyle.Bold;
            
            closeBtnComponent.targetGraphic = closeBtnImage;
            closeBtnComponent.onClick.AddListener((UnityEngine.Events.UnityAction)CloseEditPopup);
            
            // Content area
            var contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(popup.transform, false);
            
            var contentAreaRect = contentArea.AddComponent<RectTransform>();
            contentAreaRect.anchorMin = new Vector2(0.05f, 0.05f);
            contentAreaRect.anchorMax = new Vector2(0.95f, 0.85f);
            contentAreaRect.offsetMin = Vector2.zero;
            contentAreaRect.offsetMax = Vector2.zero;
            
            CreateEditContent(contentArea);
            
            // Start hidden
            editPopup.SetActive(false);
        }
        
        private void CreateEditContent(GameObject parent)
        {
            // Path input field
            var inputContainer = new GameObject("InputContainer");
            inputContainer.transform.SetParent(parent.transform, false);
            
            var inputContainerRect = inputContainer.AddComponent<RectTransform>();
            inputContainerRect.anchorMin = new Vector2(0f, 0.6f);
            inputContainerRect.anchorMax = new Vector2(1f, 0.8f);
            inputContainerRect.offsetMin = Vector2.zero;
            inputContainerRect.offsetMax = Vector2.zero;
            
            var inputBg = inputContainer.AddComponent<Image>();
            inputBg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
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
            placeholderText.text = "Enter directory path...";
            FontHelper.SetSafeFont(placeholderText);
            placeholderText.fontSize = 14;
            placeholderText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            placeholderText.fontStyle = FontStyle.Italic;
            
            // Now create InputField and assign the components
            pathInputField = inputContainer.AddComponent<InputField>();
            pathInputField.targetGraphic = inputBg;
            pathInputField.textComponent = textComponent;
            pathInputField.placeholder = placeholderText;
            pathInputField.characterLimit = 500;
            
            // Set up input field for keybind management
            InputFieldManager.SetupInputField(pathInputField);
            pathInputField.onValueChanged.AddListener((UnityEngine.Events.UnityAction<string>)OnPathInputChanged);
            
            // Error text
            var errorContainer = new GameObject("ErrorContainer");
            errorContainer.transform.SetParent(parent.transform, false);
            
            var errorContainerRect = errorContainer.AddComponent<RectTransform>();
            errorContainerRect.anchorMin = new Vector2(0f, 0.4f);
            errorContainerRect.anchorMax = new Vector2(1f, 0.55f);
            errorContainerRect.offsetMin = Vector2.zero;
            errorContainerRect.offsetMax = Vector2.zero;
            
            errorText = errorContainer.AddComponent<Text>();
            FontHelper.SetSafeFont(errorText);
            errorText.fontSize = 12;
            errorText.color = Color.red;
            errorText.alignment = TextAnchor.MiddleCenter;
            errorText.text = "";
            
            // Buttons
            var buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(parent.transform, false);
            
            var buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
            buttonContainerRect.anchorMin = new Vector2(0f, 0.1f);
            buttonContainerRect.anchorMax = new Vector2(1f, 0.3f);
            buttonContainerRect.offsetMin = Vector2.zero;
            buttonContainerRect.offsetMax = Vector2.zero;
            
            // Save button
            var saveButton = new GameObject("SaveButton");
            saveButton.transform.SetParent(buttonContainer.transform, false);
            
            var saveButtonRect = saveButton.AddComponent<RectTransform>();
            saveButtonRect.anchorMin = new Vector2(0.1f, 0f);
            saveButtonRect.anchorMax = new Vector2(0.45f, 1f);
            saveButtonRect.offsetMin = Vector2.zero;
            saveButtonRect.offsetMax = Vector2.zero;
            
            var saveBtn = saveButton.AddComponent<Button>();
            var saveBtnImage = saveButton.AddComponent<Image>();
            saveBtnImage.color = new Color(0.2f, 0.6f, 0.2f, 0.8f);
            
            var saveText = new GameObject("Text");
            saveText.transform.SetParent(saveButton.transform, false);
            var saveTextRect = saveText.AddComponent<RectTransform>();
            saveTextRect.anchorMin = Vector2.zero;
            saveTextRect.anchorMax = Vector2.one;
            saveTextRect.offsetMin = Vector2.zero;
            saveTextRect.offsetMax = Vector2.zero;
            
            var saveTextComponent = saveText.AddComponent<Text>();
            saveTextComponent.text = "Save";
            FontHelper.SetSafeFont(saveTextComponent);
            saveTextComponent.fontSize = 14;
            saveTextComponent.color = Color.white;
            saveTextComponent.alignment = TextAnchor.MiddleCenter;
            saveTextComponent.fontStyle = FontStyle.Bold;
            
            saveBtn.targetGraphic = saveBtnImage;
            saveBtn.onClick.AddListener((UnityEngine.Events.UnityAction)OnSaveClicked);
            
            // Cancel button
            var cancelButton = new GameObject("CancelButton");
            cancelButton.transform.SetParent(buttonContainer.transform, false);
            
            var cancelButtonRect = cancelButton.AddComponent<RectTransform>();
            cancelButtonRect.anchorMin = new Vector2(0.55f, 0f);
            cancelButtonRect.anchorMax = new Vector2(0.9f, 1f);
            cancelButtonRect.offsetMin = Vector2.zero;
            cancelButtonRect.offsetMax = Vector2.zero;
            
            var cancelBtn = cancelButton.AddComponent<Button>();
            var cancelBtnImage = cancelButton.AddComponent<Image>();
            cancelBtnImage.color = new Color(0.6f, 0.2f, 0.2f, 0.8f);
            
            var cancelText = new GameObject("Text");
            cancelText.transform.SetParent(cancelButton.transform, false);
            var cancelTextRect = cancelText.AddComponent<RectTransform>();
            cancelTextRect.anchorMin = Vector2.zero;
            cancelTextRect.anchorMax = Vector2.one;
            cancelTextRect.offsetMin = Vector2.zero;
            cancelTextRect.offsetMax = Vector2.zero;
            
            var cancelTextComponent = cancelText.AddComponent<Text>();
            cancelTextComponent.text = "Cancel";
            FontHelper.SetSafeFont(cancelTextComponent);
            cancelTextComponent.fontSize = 14;
            cancelTextComponent.color = Color.white;
            cancelTextComponent.alignment = TextAnchor.MiddleCenter;
            cancelTextComponent.fontStyle = FontStyle.Bold;
            
            cancelBtn.targetGraphic = cancelBtnImage;
            cancelBtn.onClick.AddListener((UnityEngine.Events.UnityAction)CloseEditPopup);
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
            entryRect.sizeDelta = new Vector2(0, 60f);
            
            var entryImage = entryObj.AddComponent<Image>();
            entryImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            var entryLayout = entryObj.AddComponent<HorizontalLayoutGroup>();
            entryLayout.spacing = 10;
            entryLayout.padding = new RectOffset(10, 10, 10, 10);
            entryLayout.childControlWidth = false;
            entryLayout.childControlHeight = true;
            entryLayout.childForceExpandHeight = true;
            entryLayout.childAlignment = TextAnchor.MiddleLeft;
            
            // Directory info panel
            var infoPanel = new GameObject("InfoPanel");
            infoPanel.transform.SetParent(entryObj.transform, false);
            var infoPanelLayout = infoPanel.AddComponent<LayoutElement>();
            infoPanelLayout.flexibleWidth = 1;
            infoPanelLayout.minWidth = 300;
            
            var infoVerticalLayout = infoPanel.AddComponent<VerticalLayoutGroup>();
            infoVerticalLayout.spacing = 2;
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
            
            // Status text
            var statusObj = new GameObject("Status");
            statusObj.transform.SetParent(infoPanel.transform, false);
            var statusText = statusObj.AddComponent<Text>();
            FontHelper.SetSafeFont(statusText);
            statusText.fontSize = 12;
            statusText.text = directory.IsValid ? 
                (directory.IsEnabled ? $"Enabled • {directory.FileCount} files" : "Disabled") : 
                "Directory not found";
            statusText.color = directory.IsValid ? 
                (directory.IsEnabled ? Color.green : Color.yellow) : 
                Color.red;
            
            // Buttons panel
            var buttonPanel = new GameObject("ButtonPanel");
            buttonPanel.transform.SetParent(entryObj.transform, false);
            var buttonPanelLayout = buttonPanel.AddComponent<LayoutElement>();
            
            if (directory.IsDefault)
            {
                // Default directory: only show status (always enabled)
                buttonPanelLayout.preferredWidth = 80;
                
                var statusLabel = new GameObject("StatusLabel");
                statusLabel.transform.SetParent(buttonPanel.transform, false);
                var statusLabelText = statusLabel.AddComponent<Text>();
                FontHelper.SetSafeFont(statusLabelText);
                statusLabelText.fontSize = 12;
                statusLabelText.color = Color.green;
                statusLabelText.text = "Always On";
                statusLabelText.alignment = TextAnchor.MiddleCenter;
                statusLabelText.fontStyle = FontStyle.Bold;
                
                var statusLabelRect = statusLabel.GetComponent<RectTransform>();
                statusLabelRect.anchorMin = Vector2.zero;
                statusLabelRect.anchorMax = Vector2.one;
                statusLabelRect.offsetMin = Vector2.zero;
                statusLabelRect.offsetMax = Vector2.zero;
            }
            else
            {
                // Non-default directory: Edit, Toggle, Remove buttons
                buttonPanelLayout.preferredWidth = 200;
                
                var buttonHorizontalLayout = buttonPanel.AddComponent<HorizontalLayoutGroup>();
                buttonHorizontalLayout.spacing = 5;
                buttonHorizontalLayout.childControlWidth = true;
                buttonHorizontalLayout.childControlHeight = true;
                buttonHorizontalLayout.childForceExpandWidth = true;
                buttonHorizontalLayout.childForceExpandHeight = true;
                
                // Edit button
                var editButton = CreateButton("Edit", buttonPanel.transform, () => OnEditClicked(pathHash, directory));
                editButton.GetComponent<Image>().color = new Color(0.2f, 0.4f, 0.6f);
                
                // Toggle button
                var toggleText = directory.IsEnabled ? "Disable" : "Enable";
                var toggleColor = directory.IsEnabled ? new Color(0.6f, 0.4f, 0.2f) : new Color(0.2f, 0.6f, 0.2f);
                var toggleButton = CreateButton(toggleText, buttonPanel.transform, () => OnToggleClicked(pathHash, directory));
                toggleButton.GetComponent<Image>().color = toggleColor;
                
                // Remove button
                var removeButton = CreateButton("Remove", buttonPanel.transform, () => OnRemoveClicked(pathHash, directory));
                removeButton.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f);
            }
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
            textComponent.fontSize = 10;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            var textRect = buttonText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            button.targetGraphic = buttonImage;
            button.onClick.AddListener((UnityEngine.Events.UnityAction)onClick);
            return button;
        }
        
        private void OnAddNewClicked()
        {
            editingHash = null;
            isEditing = false;
            
            if (editTitleText != null)
                editTitleText.text = "Add New Directory";
            if (pathInputField != null)
                pathInputField.text = "";
            if (errorText != null)
                errorText.text = "";
            
            ShowEditPopup();
        }
        
        private void OnEditClicked(string pathHash, MusicDirectory directory)
        {
            editingHash = pathHash;
            isEditing = true;
            
            if (editTitleText != null)
                editTitleText.text = "Edit Directory Path";
            if (pathInputField != null)
                pathInputField.text = directory.Path;
            if (errorText != null)
                errorText.text = "";
            
            ShowEditPopup();
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
                RefreshDirectoryList();
                onDirectoriesChanged?.Invoke();
            }
            else
            {
                LoggingSystem.Warning($"Failed to remove directory: {error}", "DirectoryManager");
            }
        }
        
        private void ShowEditPopup()
        {
            if (editPopup != null)
            {
                editPopup.SetActive(true);
                isEditPopupOpen = true;
                
                // Focus input field
                if (pathInputField != null)
                    pathInputField.ActivateInputField();
                
                // Disable main popup interaction
                if (mainPopup != null)
                {
                    var mainPopupButtons = mainPopup.GetComponentsInChildren<Button>();
                    foreach (var btn in mainPopupButtons)
                    {
                        btn.interactable = false;
                    }
                }
            }
        }
        
        private void CloseEditPopup()
        {
            if (editPopup != null)
            {
                editPopup.SetActive(false);
                isEditPopupOpen = false;
                
                // Re-enable main popup interaction
                if (mainPopup != null)
                {
                    var mainPopupButtons = mainPopup.GetComponentsInChildren<Button>();
                    foreach (var btn in mainPopupButtons)
                    {
                        btn.interactable = true;
                    }
                }
            }
        }
        
        private void OnSaveClicked()
        {
            if (pathInputField == null || errorText == null) return;
            
            var path = pathInputField.text.Trim();
            if (string.IsNullOrEmpty(path))
            {
                errorText.text = "Please enter a directory path";
                return;
            }
            
            if (!System.IO.Directory.Exists(path))
            {
                errorText.text = "Directory does not exist";
                return;
            }
            
            var config = MusicDirectoryConfig.Instance;
            string error;
            
            if (isEditing && !string.IsNullOrEmpty(editingHash))
            {
                // Edit existing directory - remove old and add new
                var oldDirectory = config.GetDirectoryByHash(editingHash);
                if (oldDirectory != null)
                {
                    // Remove old directory
                    if (config.RemoveDirectoryByHash(editingHash, out error))
                    {
                        // Add new directory with same settings
                        if (config.AddDirectory(path, out error, oldDirectory.Name, oldDirectory.Description))
                        {
                            RefreshDirectoryList();
                            onDirectoriesChanged?.Invoke();
                            CloseEditPopup();
                        }
                        else
                        {
                            // If adding new failed, try to restore old directory
                            config.AddDirectory(oldDirectory.Path, out _, oldDirectory.Name, oldDirectory.Description);
                            errorText.text = error;
                        }
                    }
                    else
                    {
                        errorText.text = error;
                    }
                }
                else
                {
                    errorText.text = "Directory not found";
                }
            }
            else
            {
                // Add new directory
                if (config.AddDirectory(path, out error))
                {
                    RefreshDirectoryList();
                    onDirectoriesChanged?.Invoke();
                    CloseEditPopup();
                }
                else
                {
                    errorText.text = error;
                }
            }
        }
        
        private void OnPathInputChanged(string value)
        {
            if (errorText != null)
                errorText.text = "";
        }
        
        private void OnDestroy()
        {
            if (mainPopup != null)
                DestroyImmediate(mainPopup);
            if (editPopup != null)
                DestroyImmediate(editPopup);
        }
    }
} 