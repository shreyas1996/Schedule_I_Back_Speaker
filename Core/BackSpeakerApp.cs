using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using System.Reflection;
using UnityEngine.Events;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Utils;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.UI.Phone;
using Il2CppScheduleOne.UI.Phone.ProductManagerApp;
using Il2CppScheduleOne.PlayerScripts;

namespace BackSpeakerMod.Core
{
    public class BackSpeakerApp
    {
        public static BackSpeakerApp? Instance;
        private BackSpeakerManager? manager;
        private GameObject? homeScreen;
        private GameObject? appsCanvas;
        private GameObject? canvas;
        private GameObject? appIcon;
        private Button? appButton;
        // UI elements are now handled by BackSpeakerScreen
        public Sprite? AppLogo { get; set; }
        public string AppLabel => "Back Speaker";
        
        // Add App framework support like Drones
        public App<ProductManagerApp>? app;
        
        // App state tracking like Drones
        public static bool appActive = false;
        public static bool appBecameActive = false;

        public BackSpeakerApp(BackSpeakerManager manager)
        {
            this.manager = manager;
        }

        public bool Create()
        {
            Instance = this;
            return InitAppUI();
        }

        private bool InitAppUI()
        {
            // Check if our app canvas already exists
            if (GameObject.Find("BackSpeakerApp") != null)
            {
                return false;
            }
            // Check if our app icon already exists
            var appIcons = GameObject.Find("AppIcons");
            if (appIcons != null)
            {
                for (int i = 0; i < appIcons.transform.childCount; i++)
                {
                    var icon = appIcons.transform.GetChild(i);
                    var label = icon.FindChild("Label")?.GetComponent<Text>();
                    if (label != null && label.text == AppLabel)
                    {
                        return false;
                    }
                }
            }
            // Find canvases
            appsCanvas = GameObject.Find("AppsCanvas");
            homeScreen = GameObject.Find("HomeScreen");
            if (appsCanvas == null || homeScreen == null)
            {
                return false;
            }
            
            // ColorBlock setup - EXACTLY like Drones
            ColorBlock colorBlock = default(ColorBlock);
            colorBlock.normalColor = new Color(0.25f, 0.25f, 0.25f, 0.1f);
            colorBlock.highlightedColor = new Color(0.25f, 0.275f, 0.35f, 0.3f);
            colorBlock.pressedColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);
            colorBlock.selectedColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            colorBlock.fadeDuration = 0.1f;
            colorBlock.colorMultiplier = 1f;
            
            var baseAppObj = appsCanvas.transform.FindChild("ProductManagerApp");
            if (baseAppObj == null)
            {
                return false;
            }
            
            // Clone the original ProductManagerApp as our app canvas - EXACTLY like Drones does
            canvas = UnityEngine.Object.Instantiate<GameObject>(baseAppObj.gameObject, appsCanvas.transform);
            
            // Get the App component like Drones does
            app = canvas.GetComponent<App<ProductManagerApp>>();
            if (app != null)
                this.app.AppName = AppLabel;
            
            canvas.name = "BackSpeakerApp";
            canvas.transform.localPosition = Vector3.zero;
            canvas.transform.localScale = Vector3.one;
            canvas.transform.localRotation = Quaternion.identity;
            canvas.active = false;

            // CRITICAL: Set proper canvas sorting to prevent bleeding
            var canvasComponent = canvas.GetComponent<Canvas>();
            if (canvasComponent != null)
            {
                canvasComponent.sortingOrder = 0; // Keep same level as other apps
                canvasComponent.overrideSorting = false; // Don't override phone's sorting
            }

            // Get the last icon and modify it directly - EXACTLY like Drones does
            if (appIcons == null)
            {
                UnityEngine.Object.DestroyImmediate(canvas);
                return false;
            }
            
            // Add safety check for icon count
            int iconCount = appIcons.transform.childCount;
            
            if (iconCount == 0)
            {
                UnityEngine.Object.DestroyImmediate(canvas);
                return false;
            }
            
            appIcon = appIcons.transform.GetChild(iconCount - 1).gameObject;
            appIcon.transform.FindChild("Label").gameObject.GetComponent<Text>().text = AppLabel;
            // Set icon sprite (must be PNG)
            AppLogo = Utils.ResourceLoader.LoadEmbeddedSprite("BackSpeakerMod.EmbeddedResources.back_speaker_logo.png");
            var mask = appIcon.transform.FindChild("Mask").GetChild(0).GetComponent<Image>();
            if (AppLogo != null && mask != null)
                mask.sprite = AppLogo;
            appButton = appIcon.GetComponent<Button>();
            appButton.onClick.AddListener((UnityEngine.Events.UnityAction)delegate()
            {
                this.OnHomeScreenBtnClick();
            });

            var container = canvas.transform.FindChild("Container");
            if (container == null)
            {
                UnityEngine.Object.DestroyImmediate(canvas);
                return false;
            }
            
            // --- Drones-style cleanup - EXACTLY like Drones ---
            // Update topbar title - EXACTLY like Drones
            Transform transform = container.FindChild("Topbar").FindChild("Title");
            var txtHeading = transform.GetComponent<Text>();
            txtHeading.text = AppLabel;
            
            // Hide instruction - EXACTLY like Drones  
            Transform transform2 = container.FindChild("Details").FindChild("Instruction");
            transform2.GetComponent<Text>().text = "Click a viewport to zoom in";
            transform2.gameObject.SetActive(false);
            
            // Remove Scroll View and Details - EXACTLY like Drones
            container.FindChild("Scroll View").DetachChildren();
            UnityEngine.Object.Destroy(container.FindChild("Scroll View"));
            UnityEngine.Object.Destroy(container.FindChild("Details").gameObject);
                
            // Set up background - EXACTLY like Drones
            GameObject gameObject3 = container.FindChild("Background").gameObject;
            gameObject3.transform.SetAsFirstSibling();
            var imgBackground = gameObject3.GetComponent<Image>();
            imgBackground.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark background
            // --- End Drones-style cleanup ---

            var backSpeakerScreenObj = new GameObject("BackSpeakerScreen");
            backSpeakerScreenObj.transform.SetParent(container, false); // Attach to Container, not canvas directly
            var backSpeakerScreen = backSpeakerScreenObj.AddComponent<BackSpeakerMod.UI.BackSpeakerScreen>();
            backSpeakerScreen.Setup(manager);
            
            // Activate the canvas like Drones does
            canvas.active = true;
            return true;
        }

        private void OnHomeScreenBtnClick()
        {
            // Use Drones pattern for proper navigation - EXACTLY like Drones
            if (homeScreen != null) homeScreen.GetComponent<Canvas>().enabled = false;
            if (appsCanvas != null) appsCanvas.GetComponent<Canvas>().enabled = true;
            if (canvas != null) canvas.active = true; // EXACTLY like Drones does
        }

        // Add Update method like Drones for proper app state tracking
        public void Update()
        {
            if (app != null && canvas != null)
            {
                Phone phone = PlayerSingleton<Phone>.instance;
                bool isActive = this.app.isOpen && phone != null && phone.IsOpen;
                
                appBecameActive = (!appActive && isActive);
                appActive = isActive;
            }
        }

        // UI interactions are now handled by BackSpeakerScreen

        private void LogHierarchy(Transform t, int depth)
        {
            // LoggerUtil.Info(new string(' ', depth * 2) + t.name);
            for (int i = 0; i < t.childCount; i++)
                LogHierarchy(t.GetChild(i), depth + 1);
        }
    }
} 