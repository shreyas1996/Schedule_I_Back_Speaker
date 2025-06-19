#if !IL2CPP
using System;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoMusicPlayer : IMusicPlayer
    {
        private readonly ScheduleOne.MusicPlayer _musicPlayer;

        public MonoMusicPlayer(ScheduleOne.MusicPlayer musicPlayer)
        {
            _musicPlayer = musicPlayer ?? throw new ArgumentNullException(nameof(musicPlayer));
        }

        public bool IsPlaying => _musicPlayer.IsPlaying;

        public void Play()
        {
            _musicPlayer.Play();
        }

        public void Pause()
        {
            _musicPlayer.Pause();
        }

        public void Stop()
        {
            _musicPlayer.Stop();
        }

        public void SetVolume(float volume)
        {
            _musicPlayer.SetVolume(volume);
        }

        public float GetVolume()
        {
            return _musicPlayer.GetVolume();
        }
    }
    }
#endif
