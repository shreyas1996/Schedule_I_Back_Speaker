using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;
using System.Collections.Generic;
using System;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
#if IL2CPP
    public class Il2CppJukebox : IJukebox
    {
        private readonly Il2CppScheduleOne.ObjectScripts.Jukebox jukebox;

        public Il2CppJukebox(Il2CppScheduleOne.ObjectScripts.Jukebox jukebox)
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
                // Try to access the TrackList property if it exists
                if (jukebox.TrackList != null && jukebox.TrackList.Count > 0)
                {
                    foreach (var track in jukebox.TrackList)
                    {
                        if (track != null)
                        {
                            // Try different ways to get AudioClip from Track
                            AudioClip clip = null;
                            
                            // Method 1: Try direct clip property
                            try
                            {
                                var clipField = track.GetType().GetField("clip");
                                if (clipField != null)
                                {
                                    clip = clipField.GetValue(track) as AudioClip;
                                }
                            }
                            catch { }
                            
                            // Method 2: Try audioClip property
                            if (clip == null)
                            {
                                try
                                {
                                    var clipProperty = track.GetType().GetProperty("audioClip");
                                    if (clipProperty != null)
                                    {
                                        clip = clipProperty.GetValue(track) as AudioClip;
                                    }
                                }
                                catch { }
                            }
                            
                            // Method 3: Skip GetComponent as Track may not be a MonoBehaviour
                            // This would require the Track to be a MonoBehaviour which it may not be
                            
                            if (clip != null)
                            {
                                tracks.Add(clip);
                            }
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
                // This handles cases where the jukebox structure might be different
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
#endif
} 