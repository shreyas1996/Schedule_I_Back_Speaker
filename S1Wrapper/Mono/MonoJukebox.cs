#if !IL2CPP
using System;
using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;
using System.Collections.Generic;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoJukebox : IJukebox
    {
        private readonly ScheduleOne.Jukebox _jukebox;

        public MonoJukebox(ScheduleOne.Jukebox jukebox)
        {
            _jukebox = jukebox ?? throw new ArgumentNullException(nameof(jukebox));
        }

        public GameObject GameObject => _jukebox.gameObject;
        public Transform Transform => _jukebox.transform;
        public string Name => _jukebox.name;

        public AudioClip? CurrentTrack => _jukebox.CurrentTrack;

        public bool IsPlaying => _jukebox.IsPlaying;

        public void Play()
        {
            _jukebox.Play();
        }

        public void Pause()
        {
            _jukebox.Pause();
        }

        public void Stop()
        {
            _jukebox.Stop();
        }

        public void NextTrack()
        {
            _jukebox.NextTrack();
        }

        public void PreviousTrack()
        {
            _jukebox.PreviousTrack();
        }

        public void SetVolume(float volume)
        {
            _jukebox.SetVolume(volume);
        }

        public float GetVolume()
        {
            return _jukebox.GetVolume();
        }

        public void LoadTrack(AudioClip clip)
        {
            _jukebox.LoadTrack(clip);
        }

        public float GetCurrentTime()
        {
            return _jukebox.GetCurrentTime();
        }

        public float GetTrackLength()
        {
            return _jukebox.GetTrackLength();
        }

        public void SetTime(float time)
        {
            _jukebox.SetTime(time);
        }

        public AudioClip GetCurrentTrack()
        {
            return _jukebox.GetCurrentTrack();
        }

        public List<AudioClip> GetTracks()
        {
            var tracks = new List<AudioClip>();
            
            try
            {
                // Try to access the TrackList property if it exists
                if (_jukebox.TrackList != null && _jukebox.TrackList.Count > 0)
                {
                    foreach (var track in _jukebox.TrackList)
                    {
                        if (track != null)
                        {
                            tracks.Add(track);
                        }
                    }
                }
                
                // Fallback: Try to get tracks from AudioSource components
                if (tracks.Count == 0)
                {
                    var audioSources = _jukebox.GetComponentsInChildren<AudioSource>();
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
        public bool IsActive => _jukebox != null && _jukebox.gameObject.activeInHierarchy;

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