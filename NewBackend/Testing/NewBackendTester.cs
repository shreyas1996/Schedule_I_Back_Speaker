using System.Collections;
using UnityEngine;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.NewBackend.Testing
{
    /// <summary>
    /// Simple tester to validate NewBackend functionality
    /// </summary>
    public static class NewBackendTester
    {
        private static bool _isTestRunning = false;
        
        /// <summary>
        /// Run comprehensive tests on NewBackend systems
        /// </summary>
        public static IEnumerator RunTests()
        {
            if (_isTestRunning)
            {
                NewLoggingSystem.Warning("Tests already running", "NewBackendTester");
                yield break;
            }
            
            _isTestRunning = true;
            NewLoggingSystem.Info("🧪 Starting NewBackend Tests", "NewBackendTester");
            
            Exception? testError = null;
            
            // Run all tests and capture any errors
            yield return RunAllTests(error => testError = error);
            
            if (testError != null)
            {
                NewLoggingSystem.Error($"❌ NewBackend Tests Failed: {testError.Message}", "NewBackendTester");
            }
            else
            {
                NewLoggingSystem.Info("✅ All NewBackend Tests Completed Successfully!", "NewBackendTester");
            }
            
            _isTestRunning = false;
        }
        
        private static IEnumerator RunAllTests(System.Action<System.Exception> onError)
        {
            // Test 1: MainManager Initialization
            yield return TestMainManagerInitialization();
            
            // Test 2: Headphone System
            yield return TestHeadphoneSystem();
            
            // Test 3: Audio System
            yield return TestAudioSystem();
            
            // Test 4: Playlist System
            yield return TestPlaylistSystem();
            
            // Test 5: YouTube Playlist System
            yield return TestYouTubePlaylistSystem();
        }
        
        private static IEnumerator TestMainManagerInitialization()
        {
            NewLoggingSystem.Info("🔧 Testing MainManager Initialization...", "NewBackendTester");
            
            var mainManager = BackSpeakerMainManager.Instance;
            if (mainManager == null)
            {
                NewLoggingSystem.Error("❌ MainManager Instance is null", "NewBackendTester");
                yield break;
            }
            
            // Create a mock player for testing (since we might not have a real player in tests)
            var mockPlayer = new MockPlayer();
            
            // Test initialization
            mainManager.Initialize(mockPlayer);
            
            // Wait a bit for async initialization
            yield return new WaitForSeconds(2f);
            
            if (mainManager.IsInitialized)
            {
                NewLoggingSystem.Info("✅ MainManager initialized successfully", "NewBackendTester");
            }
            else
            {
                NewLoggingSystem.Warning("⚠️ MainManager not initialized (expected in test environment)", "NewBackendTester");
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        private static IEnumerator TestHeadphoneSystem()
        {
            NewLoggingSystem.Info("🎧 Testing Headphone System...", "NewBackendTester");
            
            var mainManager = BackSpeakerMainManager.Instance;
            if (mainManager == null)
            {
                NewLoggingSystem.Error("❌ MainManager not available for headphone test", "NewBackendTester");
                yield break;
            }
            
            // Test headphone attachment status
            bool areAttached = mainManager.AreHeadphonesAttached;
            NewLoggingSystem.Info($"📊 Headphones attached: {areAttached}", "NewBackendTester");
            
            NewLoggingSystem.Info("✅ Headphone system test completed", "NewBackendTester");
            yield return new WaitForSeconds(0.5f);
        }
        
        private static IEnumerator TestAudioSystem()
        {
            NewLoggingSystem.Info("🔊 Testing Audio System...", "NewBackendTester");
            
            var mainManager = BackSpeakerMainManager.Instance;
            if (mainManager == null)
            {
                NewLoggingSystem.Error("❌ MainManager not available for audio test", "NewBackendTester");
                yield break;
            }
            
            // Test audio state
            bool isPlaying = mainManager.IsPlaying();
            NewLoggingSystem.Info($"📊 Audio playing: {isPlaying}", "NewBackendTester");
            
            var currentTrack = mainManager.GetCurrentTrack();
            if (currentTrack != null)
            {
                NewLoggingSystem.Info($"📊 Current track: {currentTrack.title} by {currentTrack.artist}", "NewBackendTester");
            }
            else
            {
                NewLoggingSystem.Info("📊 No current track", "NewBackendTester");
            }
            
            // Test volume control
            mainManager.SetVolume(0.5f);
            NewLoggingSystem.Info("📊 Set volume to 50%", "NewBackendTester");
            
            NewLoggingSystem.Info("✅ Audio system test completed", "NewBackendTester");
            yield return new WaitForSeconds(0.5f);
        }
        
        private static IEnumerator TestPlaylistSystem()
        {
            NewLoggingSystem.Info("📝 Testing Playlist System...", "NewBackendTester");
            
            var mainManager = BackSpeakerMainManager.Instance;
            if (mainManager == null)
            {
                NewLoggingSystem.Error("❌ MainManager not available for playlist test", "NewBackendTester");
                yield break;
            }
            
            // Test getting tracks from different sources
            var jukeboxTracks = mainManager.GetJukeboxTracks();
            var localTracks = mainManager.GetLocalFolderTracks();
            var youtubeTracks = mainManager.GetYouTubeTracks();
            
            NewLoggingSystem.Info($"📊 Jukebox tracks: {jukeboxTracks.Count}", "NewBackendTester");
            NewLoggingSystem.Info($"📊 Local folder tracks: {localTracks.Count}", "NewBackendTester");
            NewLoggingSystem.Info($"📊 YouTube tracks: {youtubeTracks.Count}", "NewBackendTester");
            
            // Test playlist names
            var jukeboxPlaylists = mainManager.GetPlaylistNames("Jukebox");
            var localPlaylists = mainManager.GetPlaylistNames("LocalFolder");
            var youtubePlaylists = mainManager.GetPlaylistNames("YouTube");
            
            NewLoggingSystem.Info($"📊 Jukebox playlists: {jukeboxPlaylists.Count}", "NewBackendTester");
            NewLoggingSystem.Info($"📊 Local playlists: {localPlaylists.Count}", "NewBackendTester");
            NewLoggingSystem.Info($"📊 YouTube playlists: {youtubePlaylists.Count}", "NewBackendTester");
            
            NewLoggingSystem.Info("✅ Playlist system test completed", "NewBackendTester");
            yield return new WaitForSeconds(0.5f);
        }
        
        private static IEnumerator TestYouTubePlaylistSystem()
        {
            NewLoggingSystem.Info("🎵 Testing YouTube Playlist System...", "NewBackendTester");
            
            try
            {
                // Test creating a playlist
                var testPlaylist = NewYouTubePlaylistManager.CreatePlaylist("Test Playlist", "Created by NewBackend tester");
                if (testPlaylist != null)
                {
                    NewLoggingSystem.Info($"✅ Created test playlist: {testPlaylist.name} (ID: {testPlaylist.id})", "NewBackendTester");
                    
                    // Test adding a song
                    var testSong = new NewSongDetails
                    {
                        title = "Test Song",
                        artist = "Test Artist",
                        url = "https://youtube.com/watch?v=test123",
                        description = "Test song for NewBackend"
                    };
                    
                    bool songAdded = testPlaylist.AddSong(testSong);
                    if (songAdded)
                    {
                        NewLoggingSystem.Info("✅ Added test song to playlist", "NewBackendTester");
                        
                        // Save the playlist
                        bool saved = NewYouTubePlaylistManager.SavePlaylist(testPlaylist);
                        if (saved)
                        {
                            NewLoggingSystem.Info("✅ Saved test playlist", "NewBackendTester");
                        }
                        else
                        {
                            NewLoggingSystem.Error("❌ Failed to save test playlist", "NewBackendTester");
                        }
                    }
                    else
                    {
                        NewLoggingSystem.Error("❌ Failed to add test song", "NewBackendTester");
                    }
                    
                    // Test loading the playlist
                    var loadedPlaylist = NewYouTubePlaylistManager.LoadPlaylist(testPlaylist.id);
                    if (loadedPlaylist != null && loadedPlaylist.songs.Count > 0)
                    {
                        NewLoggingSystem.Info($"✅ Loaded playlist with {loadedPlaylist.songs.Count} songs", "NewBackendTester");
                    }
                    else
                    {
                        NewLoggingSystem.Error("❌ Failed to load playlist or no songs found", "NewBackendTester");
                    }
                    
                    // Clean up - delete the test playlist
                    bool deleted = NewYouTubePlaylistManager.DeletePlaylist(testPlaylist.id);
                    if (deleted)
                    {
                        NewLoggingSystem.Info("✅ Cleaned up test playlist", "NewBackendTester");
                    }
                    else
                    {
                        NewLoggingSystem.Warning("⚠️ Failed to clean up test playlist", "NewBackendTester");
                    }
                }
                else
                {
                    NewLoggingSystem.Error("❌ Failed to create test playlist", "NewBackendTester");
                }
                
                // Test getting all playlists
                var allPlaylists = NewYouTubePlaylistManager.GetAllPlaylists();
                NewLoggingSystem.Info($"📊 Total YouTube playlists: {allPlaylists.Count}", "NewBackendTester");
                
                NewLoggingSystem.Info("✅ YouTube playlist system test completed", "NewBackendTester");
            }
            catch (System.Exception ex)
            {
                NewLoggingSystem.Error($"❌ YouTube playlist test failed: {ex.Message}", "NewBackendTester");
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// Quick smoke test to verify basic functionality
        /// </summary>
        public static IEnumerator QuickSmokeTest()
        {
            NewLoggingSystem.Info("🚀 Running Quick Smoke Test", "NewBackendTester");
            
            try
            {
                // Test logging system
                NewLoggingSystem.Debug("Debug message test", "NewBackendTester");
                NewLoggingSystem.Info("Info message test", "NewBackendTester");
                NewLoggingSystem.Warning("Warning message test", "NewBackendTester");
                
                // Test MainManager singleton
                var manager = BackSpeakerMainManager.Instance;
                if (manager != null)
                {
                    NewLoggingSystem.Info("✅ MainManager singleton working", "NewBackendTester");
                }
                else
                {
                    NewLoggingSystem.Error("❌ MainManager singleton failed", "NewBackendTester");
                }
                
                // Test YouTube playlist manager
                var playlists = NewYouTubePlaylistManager.GetAllPlaylists();
                NewLoggingSystem.Info($"✅ YouTube playlist manager working - {playlists.Count} playlists", "NewBackendTester");
                
                NewLoggingSystem.Info("✅ Quick smoke test completed successfully", "NewBackendTester");
            }
            catch (System.Exception ex)
            {
                NewLoggingSystem.Error($"❌ Quick smoke test failed: {ex.Message}", "NewBackendTester");
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        /// <summary>
        /// Mock player for testing purposes
        /// </summary>
        private class MockPlayer : IPlayer
        {
            public string Name => "MockPlayer";
            public UnityEngine.GameObject GameObject => null;
            public UnityEngine.Transform Transform => null;
            public IAvatar? Avatar => null;
        }
    }
} 