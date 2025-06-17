namespace BackSpeakerMod.S1Wrapper
{
    public static class S1GameInput
    {
        public static bool IsTyping
        {
            get
            {
#if IL2CPP
                if (S1Environment.IsIl2Cpp)
                    return Il2CppScheduleOne.GameInput.IsTyping;
                return ScheduleOne.GameInput.IsTyping;
#else
                return ScheduleOne.GameInput.IsTyping;
#endif
            }
            set
            {
#if IL2CPP
                if (S1Environment.IsIl2Cpp)
                    Il2CppScheduleOne.GameInput.IsTyping = value;
                else
                    ScheduleOne.GameInput.IsTyping = value;
#else
                ScheduleOne.GameInput.IsTyping = value;
#endif
            }
        }
    }
}
