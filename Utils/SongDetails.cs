using System;
using Newtonsoft.Json;

namespace BackSpeakerMod.Utils
{
    [Serializable]
    public class SongDetails
    {
        [JsonProperty("title")]
        public string title { get; set; } = "";
        
        [JsonProperty("uploader")]
        public string artist { get; set; } = "";
        
        [JsonProperty("thumbnail")]
        public string thumbnail { get; set; } = "";
        
        [JsonProperty("duration")]
        public float duration { get; set; } = 0f;
        
        [JsonProperty("id")]
        public string id { get; set; } = "";
        
        [JsonProperty("webpage_url")]
        public string url { get; set; } = "";
        
        [JsonProperty("description")]
        public string description { get; set; } = "";
        
        // Alternative fields that yt-dlp might use
        [JsonProperty("channel")]
        public string channel { get; set; } = "";
        
        [JsonProperty("uploader_id")]
        public string uploader_id { get; set; } = "";
        
        // Fallback artist property
        public string GetArtist()
        {
            if (!string.IsNullOrEmpty(artist)) return artist;
            if (!string.IsNullOrEmpty(channel)) return channel;
            if (!string.IsNullOrEmpty(uploader_id)) return uploader_id;
            return "Unknown Artist";
        }
        
        // Formatted duration
        public string GetFormattedDuration()
        {
            if (duration <= 0) return "Unknown";
            
            var timeSpan = TimeSpan.FromSeconds(duration);
            if (timeSpan.TotalHours >= 1)
                return timeSpan.ToString(@"h\:mm\:ss");
            else
                return timeSpan.ToString(@"m\:ss");
        }
    }
} 