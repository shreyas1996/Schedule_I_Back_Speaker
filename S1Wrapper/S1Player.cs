using BackSpeakerMod.S1Wrapper.Interfaces;
using System;
using MelonLoader;
using System.Collections;
using UnityEngine;

namespace BackSpeakerMod.S1Wrapper
{
    public static class S1Player
    {
        private static IPlayer? _player;

        private static void SetPlayer(IPlayer player)
        {
            _player = player;
        }

        public static void DetectPlayer()
        {
            MelonCoroutines.Start(PlayerDetectionCoroutine());
        }

        public static IPlayer? GetPlayer()
        {
            return _player;
        }

        public static void ClearPlayer()
        {
            _player = null;
        }

        private static IPlayer? FindPlayer()
        {
            #if IL2CPP
                if (S1Environment.IsIl2Cpp)
                {
                    var player = Il2CppScheduleOne.PlayerScripts.Player.Local;
                    if (player != null)
                    {
                        return new Il2Cpp.Il2CppPlayer(player);
                    }
                }
            #else
                var player = ScheduleOne.PlayerScripts.Player.Local;
                if (player != null)
                {
                    return new Mono.MonoPlayer(player);
                }
            #endif
            return null;
        }

        private static IEnumerator PlayerDetectionCoroutine()
        {
            if (_player != null)
            {
                yield break;
            }
            else
            {
                var player = FindPlayer();
                if (player != null)
                {
                    SetPlayer(player);
                    yield break;
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }
}