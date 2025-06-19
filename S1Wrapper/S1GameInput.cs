namespace BackSpeakerMod.S1Wrapper
{
    public static class S1GameInput
    {
        public static bool IsTyping
        {
            get
            {
                #if IL2CPP
                    return Il2CppScheduleOne.GameInput.IsTyping;
                #else
                    return ScheduleOne.GameInput.IsTyping;
                #endif
            }
            set
            {
                #if IL2CPP
                    Il2CppScheduleOne.GameInput.IsTyping = value;
                #else
                    ScheduleOne.GameInput.IsTyping = value;
                #endif
            }
        }
    }
}
