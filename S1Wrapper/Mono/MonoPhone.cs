#if !IL2CPP
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoPhone : IPhone
    {
        private readonly ScheduleOne.UI.Phone.Phone phone;
        public MonoPhone(ScheduleOne.UI.Phone.Phone phone)
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
