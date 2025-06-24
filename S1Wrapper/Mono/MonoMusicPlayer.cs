#if !IL2CPP
using System;
using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoMusicPlayer : IMusicPlayer
    {
        private readonly object _player;

        public MonoMusicPlayer(object player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public void Start()
        {
            try
            {
                var method = _player.GetType().GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(_player, null);
            }
            catch (Exception)
            {
                // Handle reflection failures silently
            }
        }

        public void Stop()
        {
            try
            {
                var method = _player.GetType().GetMethod("StopAndDisableTracks");
                method?.Invoke(_player, null);
            }
            catch (Exception)
            {
                // Handle reflection failures silently
            }
        }
    }
}
#endif
