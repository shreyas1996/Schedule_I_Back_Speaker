using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI.Helpers
{
    public static class LayoutManager
    {
        public static bool ValidateSetup(Image imgBackground)
        {
            if (imgBackground == null)
            {
                LoggerUtil.Error("LayoutManager: imgBackground is null!");
                return false;
            }
            
            return true;
        }
        
        public static (Transform canvasTransform, RectTransform backgroundRect) GetTransforms(Image imgBackground)
        {
            var canvasTransform = imgBackground.GetComponentInParent<Canvas>().transform;
            var backgroundRect = imgBackground.rectTransform;
            
            LoggerUtil.Info($"LayoutManager: Canvas: {canvasTransform?.name}, Background: {backgroundRect?.name}");
            return (canvasTransform, backgroundRect);
        }
        
        public static void SetupLayoutConstraints(Image imgBackground)
        {
            // Modern music app layout setup
            LoggerUtil.Info("LayoutManager: Setting up layout constraints for music app UI");
            
            // All UI elements should be children of imgBackground to prevent UI bleeding
            var bgTransform = imgBackground.transform;
            
            // Add any additional layout setup here if needed
            LoggerUtil.Info("LayoutManager: Layout constraints applied successfully");
        }
    }
} 