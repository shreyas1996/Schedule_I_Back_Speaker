using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core.Modules;

namespace BackSpeakerMod.UI.Components
{
    public class HelpTextComponent : MonoBehaviour
    {
        private Text? helpText;
        private MusicSourceType currentTab = MusicSourceType.Jukebox;
        
        public HelpTextComponent() : base() { }
        
        public void Setup()
        {
            CreateHelpText();
        }
        
        private void CreateHelpText()
        {
            var textObj = new GameObject("HelpText");
            textObj.transform.SetParent(this.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 5f);
            textRect.offsetMax = new Vector2(-10f, -5f);
            
            helpText = textObj.AddComponent<Text>();
            helpText.text = "💡 Tip: Jukebox music loads automatically from in-game audio sources";
            helpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            helpText.fontSize = 11;
            helpText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            helpText.alignment = TextAnchor.MiddleLeft;
            helpText.fontStyle = FontStyle.Italic;
        }
        
        public void UpdateForTab(MusicSourceType newTab)
        {
            currentTab = newTab;
            UpdateHelpText();
        }
        
        private void UpdateHelpText()
        {
            if (helpText == null) return;
            
            helpText.text = currentTab switch
            {
                MusicSourceType.Jukebox => "💡 Tip: Jukebox music loads automatically from in-game audio sources",
                MusicSourceType.LocalFolder => "💡 Tip: Add MP3, WAV, OGG files to Mods/BackSpeaker/Music/ folder",
                MusicSourceType.YouTube => "💡 Feature coming soon! For now, manually add MP3s to Cache/YouTube/ folder",
                _ => "💡 BackSpeaker: Multi-source music player for Schedule I"
            };
        }
    }
} 