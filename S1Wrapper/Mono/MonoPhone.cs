#if !IL2CPP
using System;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoPhone : IPhone
    {
        private readonly ScheduleOne.Phone _phone;

        public MonoPhone(ScheduleOne.Phone phone)
        {
            _phone = phone ?? throw new ArgumentNullException(nameof(phone));
        }

        public bool IsOpen => _phone.IsOpen;

        public void Open()
        {
            _phone.Open();
        }

        public void Close()
        {
            _phone.Close();
        }

        public void OpenApp(string appName)
        {
            _phone.OpenApp(appName);
        }

        public void CloseApp(string appName)
        {
            _phone.CloseApp(appName);
        }
    }
}
#endif
