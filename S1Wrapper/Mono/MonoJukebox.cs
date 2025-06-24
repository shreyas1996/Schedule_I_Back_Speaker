#if !IL2CPP
using System;
using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;
using System.Collections.Generic;
using System.Reflection;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoJukebox : IJukebox
    {
        private readonly ScheduleOne.ObjectScripts.Jukebox jukebox;

        public MonoJukebox(ScheduleOne.ObjectScripts.Jukebox jukebox)
        {
            this.jukebox = jukebox ?? throw new ArgumentNullException(nameof(jukebox));
        }

        public GameObject GameObject => jukebox.gameObject;
        public Transform Transform => jukebox.transform;
        public string Name => jukebox.name;

        public List<AudioClip> GetTracks()
        {
            var tracks = new List<AudioClip>();
            
            try
            {
                // Access the private GetTrack method using reflection since it's private
                // We still need reflection for this one method because it's private in the game code
                var getTrackMethod = typeof(ScheduleOne.ObjectScripts.Jukebox).GetMethod("GetTrack", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (getTrackMethod != null)
                {
                    // Mono Jukebox has 27 tracks (TRACK_COUNT constant)
                    for (int i = 0; i < 27; i++)
                    {
                        try
                        {
                            var track = getTrackMethod.Invoke(jukebox, new object[] { i }) as ScheduleOne.ObjectScripts.Jukebox.Track;
                            if (track?.Clip != null)
                            {
                                tracks.Add(track.Clip);
                            }
                        }
                        catch
                        {
                            // Skip invalid tracks
                            continue;
                        }
                    }
                }
                
                // Fallback: Try to get tracks from AudioSource components
                if (tracks.Count == 0)
                {
                    var audioSources = jukebox.GetComponentsInChildren<AudioSource>();
                    foreach (var source in audioSources)
                    {
                        if (source.clip != null)
                        {
                            tracks.Add(source.clip);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If accessing properties fails, return empty list
            }

            return tracks;
        }

        public int TrackCount => GetTracks().Count;
        public bool HasTracks => TrackCount > 0;
        public bool IsActive => jukebox != null && jukebox.gameObject.activeInHierarchy;

        public AudioClip? GetTrack(int index)
        {
            var tracks = GetTracks();
            if (index >= 0 && index < tracks.Count)
            {
                return tracks[index];
            }
            return null;
        }
    }
}
#endif 