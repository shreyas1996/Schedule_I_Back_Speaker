using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core.System;
using System;

namespace BackSpeakerMod.UI.Helpers
{
    public static class FontHelper
    {
        private static Font _cachedFont = null;
        private static bool _fontCacheAttempted = false;

        /// <summary>
        /// Gets a safe font that won't cause null reference exceptions
        /// </summary>
        public static Font GetSafeFont()
        {
            // Return cached font if we have one
            if (_fontCacheAttempted && _cachedFont != null)
            {
                return _cachedFont;
            }

            // Only attempt to cache once to avoid repeated resource searches
            if (!_fontCacheAttempted)
            {
                _fontCacheAttempted = true;
                _cachedFont = TryLoadFont();
            }

            return _cachedFont; // May be null, which is fine - Unity will use default
        }

        private static Font TryLoadFont()
        {
            try
            {
                // Try common built-in fonts in order of preference
                var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font != null)
                {
                    // LoggingSystem.Debug("Using LegacyRuntime.ttf font", "UI");
                    return font;
                }
                
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (font != null)
                {
                    // LoggingSystem.Debug("Using Arial.ttf font", "UI");
                    return font;
                }
                
                font = Resources.GetBuiltinResource<Font>("Arial");
                if (font != null)
                {
                    // LoggingSystem.Debug("Using Arial font", "UI");
                    return font;
                }
                
                // Try to find any available font
                var fonts = Resources.FindObjectsOfTypeAll<Font>();
                if (fonts != null && fonts.Length > 0)
                {
                    // LoggingSystem.Debug($"Using first available font: {fonts[0].name}", "UI");
                    return fonts[0];
                }
                
                LoggingSystem.Warning("No built-in fonts found, Unity will use default", "UI");
                return null; // Unity will use default font
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error loading font: {ex.Message}", "UI");
                return null; // Unity will use default font
            }
        }

        /// <summary>
        /// Safely sets the font on a Text component
        /// </summary>
        public static void SetSafeFont(Text textComponent)
        {
            // LoggingSystem.Debug("FontHelper.SetSafeFont: Starting", "UI");
            
            if (textComponent == null) 
            {
                // LoggingSystem.Debug("FontHelper.SetSafeFont: textComponent is null, returning", "UI");
                return;
            }
            
            // LoggingSystem.Debug("FontHelper.SetSafeFont: textComponent is valid, getting font", "UI");
            
            var font = GetSafeFont();
            // LoggingSystem.Debug($"FontHelper.SetSafeFont: Got font: {(font != null ? font.name : "null")}", "UI");
            
            if (font != null)
            {
                try
                {
                    textComponent.font = font;
                    // LoggingSystem.Debug("FontHelper.SetSafeFont: Successfully set font", "UI");
                }
                catch (System.Exception ex)
                {
                    LoggingSystem.Error($"FontHelper.SetSafeFont: Error setting font: {ex.Message}", "UI");
                    throw;
                }
            }
            else
            {
                // LoggingSystem.Debug("FontHelper.SetSafeFont: Font is null, Unity will use default", "UI");
            }
            
            // LoggingSystem.Debug("FontHelper.SetSafeFont: Completed", "UI");
        }
    }
} 