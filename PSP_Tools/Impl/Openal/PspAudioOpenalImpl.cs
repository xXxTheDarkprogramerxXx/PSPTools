using CSharpPlatform.AL;
using System;

namespace PSP_Tools.Impl.Openal
{
    public unsafe class PspAudioOpenalImpl : PspAudioImpl
    {
        protected static IntPtr* Device;

        protected static IntPtr* Context;

        //protected static XRamExtension XRam;
        internal static AudioStream AudioStream;

        public PspAudioOpenalImpl()
        {
        }

        private void InitOnce()
        {
            if (AudioStream != null) return;
            //AudioContext = new AudioContext(AudioContext.DefaultDevice, 44100, 4410);
            //AudioContext = new AudioContext();
            //XRam = new XRamExtension();

            Device = AL.alcOpenDevice(AL.alcGetString(null, AL.ALC_DEFAULT_DEVICE_SPECIFIER));
            Context = AL.alcCreateContext(Device, null);
            AL.alcMakeContextCurrent(Context);

            AL.alListener3f(AL.AL_POSITION, 0f, 0f, 0f);
            AL.alListener3f(AL.AL_VELOCITY, 0f, 0f, 0f);

            AudioStream = new AudioStream();
        }

        public override void Update(Action<short[]> readStream)
        {
            InitOnce();
            AL.alcProcessContext(Context);
            AudioStream.Update(readStream);
        }

        public override void StopSynchronized()
        {
            AudioStream.StopSynchronized();
        }
    }
}