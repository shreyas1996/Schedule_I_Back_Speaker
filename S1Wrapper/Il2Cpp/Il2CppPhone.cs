#if IL2CPP
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
    public class Il2CppPhone : IPhone
    {
        private readonly Il2CppScheduleOne.UI.Phone.Phone phone;
        public Il2CppPhone(Il2CppScheduleOne.UI.Phone.Phone phone)
        {
            this.phone = phone;
        }
        public bool IsOpen => phone.IsOpen;
        public bool Enabled
        {
            get => phone.enabled;
            set => phone.enabled = value;
        }
    }
}
#endif
