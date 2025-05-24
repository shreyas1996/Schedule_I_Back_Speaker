using MelonLoader;

namespace BackSpeakerMod.Utils
{
    public static class LoggerUtil
    {
        public static void Info(string message) => MelonLogger.Msg("[BackSpeaker] " + message);
        public static void Warn(string message) => MelonLogger.Warning("[BackSpeaker] " + message);
        public static void Error(string message) => MelonLogger.Error("[BackSpeaker] " + message);
    }
} 