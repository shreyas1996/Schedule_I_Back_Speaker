using System;

namespace BackSpeakerMod.NewBackend.Utils
{
    [Serializable]
    public class NewSongDetails
    {
        public string title = "";
        public string artist = "";
        public string album = "";
        public string url = "";
        public string cachedFilePath = "";
        public int duration = 0;
        public bool isDownloaded = false;
        public DateTime downloadDate = DateTime.MinValue;
        public long fileSize = 0;
        public string thumbnailUrl = "";
        public string description = "";
        public string genre = "";
        public int year = 0;
        public int trackNumber = 0;
        public string albumArtist = "";
        public string composer = "";
        public string publisher = "";
        public string copyright = "";
        public int bitrate = 0;
        public string format = "";
        public string codec = "";
        public int sampleRate = 0;
        public int channels = 0;
        public string language = "";
        public string country = "";
        public string videoId = "";
        public string playlistId = "";
        public int viewCount = 0;
        public int likeCount = 0;
        public string uploaderName = "";
        public DateTime uploadDate = DateTime.MinValue;
        public string tags = "";
        public string category = "";
        public bool isLive = false;
        public bool isPrivate = false;
        public bool isAgeRestricted = false;
        public string quality = "";
        public string source = "";
        public string checksum = "";
        public DateTime lastPlayed = DateTime.MinValue;
        public int playCount = 0;
        public bool isFavorite = false;
        public float rating = 0.0f;
        public string notes = "";
        public string customField1 = "";
        public string customField2 = "";
        public string customField3 = "";

        public NewSongDetails()
        {
        }

        public NewSongDetails(string title, string artist, string url)
        {
            this.title = title;
            this.artist = artist;
            this.url = url;
        }

        public override string ToString()
        {
            return $"{artist} - {title}";
        }

        public string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(title))
            {
                return $"{artist} - {title}";
            }
            else if (!string.IsNullOrEmpty(title))
            {
                return title;
            }
            else if (!string.IsNullOrEmpty(url))
            {
                return url;
            }
            else
            {
                return "Unknown Track";
            }
        }

        public string GetFormattedDuration()
        {
            if (duration <= 0) return "0:00";
            
            var timeSpan = TimeSpan.FromSeconds(duration);
            if (timeSpan.Hours > 0)
            {
                return $"{timeSpan.Hours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else
            {
                return $"{timeSpan.Minutes}:{timeSpan.Seconds:D2}";
            }
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(title);
        }
    }
} 