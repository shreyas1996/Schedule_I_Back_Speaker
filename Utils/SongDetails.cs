using System;
using Newtonsoft.Json;

namespace BackSpeakerMod.Utils
{
    [Serializable]
    public class SongDetails
    {
        [JsonProperty("title")]
        public string? title { get; set; }
        
        [JsonProperty("uploader")]
        public string? artist { get; set; }
        
        [JsonProperty("thumbnail")]
        public string? thumbnail { get; set; }
        
        [JsonProperty("duration")]
        public int duration { get; set; } // in seconds
        
        [JsonProperty("id")]
        public string? id { get; set; }
        
        [JsonProperty("webpage_url")]
        public string? url { get; set; }
        
        [JsonProperty("description")]
        public string? description { get; set; }
        
        // Alternative fields that yt-dlp might use
        [JsonProperty("channel")]
        public string? channel { get; set; }
        
        [JsonProperty("uploader_id")]
        public string? uploader_id { get; set; }
        
        // Download metadata and caching
        public bool isDownloaded { get; set; } = false;
        public bool isDownloading { get; set; } = false;
        public bool downloadFailed { get; set; } = false;
        public string? cachedFilePath { get; set; }
        public string? videoId { get; set; }
        public DateTime? downloadTimestamp { get; set; }
        public long? fileSizeBytes { get; set; }
        
        /// <summary>
        /// Extract video ID from YouTube URL for unique identification
        /// </summary>
        public string GetVideoId()
        {
            if (string.IsNullOrEmpty(videoId) && !string.IsNullOrEmpty(url))
            {
                videoId = ExtractVideoIdFromUrl(url);
            }
            return videoId ?? "unknown";
        }
        
        /// <summary>
        /// Get download status as human-readable string
        /// </summary>
        public string GetDownloadStatus()
        {
            if (downloadFailed) return "Failed";
            if (isDownloaded) return "Downloaded";
            if (isDownloading) return "Downloading...";
            return "Pending";
        }
        
        /// <summary>
        /// Check if song is ready to play (downloaded and file exists)
        /// </summary>
        public bool IsReadyToPlay()
        {
            return isDownloaded && 
                   !string.IsNullOrEmpty(cachedFilePath) && 
                   System.IO.File.Exists(cachedFilePath);
        }
        
        /// <summary>
        /// Generate simple filename for caching using video ID only
        /// </summary>
        public string GenerateCacheFileName()
        {
            var videoId = GetVideoId();
            return $"{videoId}.mp3";
        }
        
        /// <summary>
        /// Mark download as started
        /// </summary>
        public void MarkDownloadStarted(string filePath)
        {
            isDownloading = true;
            downloadFailed = false;
            cachedFilePath = filePath;
            downloadTimestamp = DateTime.Now;
        }
        
        /// <summary>
        /// Mark download as completed
        /// </summary>
        public void MarkDownloadCompleted(long? fileSize = null)
        {
            isDownloaded = true;
            isDownloading = false;
            downloadFailed = false;
            fileSizeBytes = fileSize;
        }
        
        /// <summary>
        /// Mark download as failed
        /// </summary>
        public void MarkDownloadFailed()
        {
            isDownloaded = false;
            isDownloading = false;
            downloadFailed = true;
        }

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
        
        private static string ExtractVideoIdFromUrl(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return "unknown";
                
                // Handle various YouTube URL formats
                if (url.Contains("?v="))
                {
                    var start = url.IndexOf("?v=") + 3;
                    var end = url.IndexOf('&', start);
                    if (end == -1) end = url.Length;
                    return url.Substring(start, end - start);
                }
                
                if (url.Contains("/v/"))
                {
                    var start = url.IndexOf("/v/") + 3;
                    var end = url.IndexOf('?', start);
                    if (end == -1) end = url.Length;
                    return url.Substring(start, end - start);
                }
                
                if (url.Contains("youtu.be/"))
                {
                    var start = url.IndexOf("youtu.be/") + 9;
                    var end = url.IndexOf('?', start);
                    if (end == -1) end = url.Length;
                    return url.Substring(start, end - start);
                }
                
                return "unknown";
            }
            catch
            {
                return "unknown";
            }
        }
        
        private static string GetSafeFileName(string fileName)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }
            
            // Limit length and remove extra spaces
            fileName = fileName.Trim().Replace(" ", "_");
            if (fileName.Length > 50)
                fileName = fileName.Substring(0, 50);
                
            return fileName;
        }
    }
} 