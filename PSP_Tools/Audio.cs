using CSharpUtils;
using CSPspEmu.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSP_Tools
{
    public class Audio : IDisposable
    {
        /// <summary>
        /// Output formats for PSP audio.
        /// </summary>
        public enum FormatEnum
        {
            /// <summary>
            /// Channel is set to stereo output (2 channels).
            /// </summary>
            Stereo = 0x00,

            /// <summary>
            /// Channel is set to mono output (1 channel).
            /// </summary>
            Mono = 0x10,
        }

        /// <summary>
        /// The maximum output volume.
        /// </summary>
        public const int MaxVolume = 0x8000;

        /// <summary>
        /// 
        /// </summary>
        public const int SamplesMax = 0x10000 - 64;

        /// <summary>
        /// Used to request the next available hardware channel.
        /// </summary>
        //public const int FreeChannel = -1;
        /// <summary>
        /// Maximum number of allowed audio channels
        /// </summary>
        public const int MaxChannels = 8;
        //public const int MaxChannels = 32;

        /// <summary>
        /// Number of audio channels
        /// </summary>
        public AudioChannel[] Channels;

        /// <summary>
        /// 
        /// </summary>
        public AudioChannel SrcOutput2Channel;

        /// <summary>
        /// 
        /// </summary>
        public PspAudioImpl AudioImpl;

        /// <summary>
        /// 
        /// </summary>
        private Audio()
        {
            Channels = new AudioChannel[MaxChannels];
            for (int n = 0; n < MaxChannels; n++)
            {
                Channels[n] = new AudioChannel(this)
                {
                    Index = n,
                    Available = true,
                };
            }

            SrcOutput2Channel = new AudioChannel(this)
            {
                Index = MaxChannels,
                Available = true,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public AudioChannel GetFreeChannel()
        {
            if (!Channels.Any(Channel => Channel.Available)) throw (new NoChannelsAvailableException());
            return Channels.Reverse().First(Channel => Channel.Available);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ChannelId"></param>
        private void CheckChannelId(int ChannelId)
        {
            if (ChannelId < 0 || ChannelId >= Channels.Length)
            {
                throw (new InvalidChannelException());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ChannelId"></param>
        /// <param name="CanAlloc"></param>
        /// <returns></returns>
        public AudioChannel GetChannel(int ChannelId, bool CanAlloc = false)
        {
            AudioChannel Channel;
            if (CanAlloc && ChannelId < 0)
            {
                Channel = GetFreeChannel();
            }
            else
            {
                CheckChannelId(ChannelId);
                Channel = Channels[ChannelId];
            }
            return Channel;
        }

        /// <summary>
        /// 
        /// </summary>
        public unsafe void Update()
        {
            AudioImpl.Update((MixedSamples) =>
            {
                var RequiredSamples = MixedSamples.Length;
                fixed (short* MixedSamplesPtr = MixedSamples)
                {
                    var MixedSamplesDenormalized = stackalloc int[RequiredSamples];

                    foreach (var Channel in Channels)
                    {
                        var ChannelSamples = Channel.Read(RequiredSamples);

                        fixed (short* ChannelSamplesPtr = ChannelSamples)
                        {
                            for (int n = 0; n < ChannelSamples.Length; n++)
                            {
                                MixedSamplesDenormalized[n] += ChannelSamplesPtr[n];
                            }
                        }
                    }

                    for (int n = 0; n < RequiredSamples; n++)
                    {
                        MixedSamplesPtr[n] = StereoShortSoundSample.Clamp(MixedSamplesDenormalized[n]);
                    }
                }
            });
        }

        private bool Disposed = false;

        public void StopSynchronized()
        {
            if (!Disposed)
            {
                Disposed = true;
                AudioImpl.StopSynchronized();
            }
        }

        void IDisposable.Dispose()
        {
            StopSynchronized();
        }


    }
    public class InvalidChannelException : Exception
    {
    }

    public class NoChannelsAvailableException : Exception
    {
    }

    public unsafe class AudioChannel
    {
        /// <summary>
        /// 
        /// </summary>
        protected Audio Audio;

        /// <summary>
        /// 10 ms
        /// </summary>
        //protected const int FillSamples = 4410;
        //protected const int FillSamples = 44100;
        protected const int FillSamples = 4410 * 2;

        /// <summary>
        /// Channel's frequency. One of 48000, 44100, 32000, 24000, 22050, 16000, 12000, 11050, 8000.
        /// </summary>
        public int Frequency = 44100;

        /// <summary>
        /// 
        /// </summary>
        public int Index;

        /// <summary>
        /// 
        /// </summary>
        public bool Available;

        /// <summary>
        /// 
        /// </summary>
        public bool IsReserved
        {
            get { return !Available; }
            set { Available = !value; }
        }

        /// <summary>
        /// Total amount of samples in the channel.
        /// </summary>
        private int _SampleCount;

        public int SampleCount
        {
            get { return _SampleCount; }
            set { _SampleCount = Math.Max(0, value); }
        }

        /// <summary>
        /// Format of the audio in the channel.
        /// Can be mono or stereo.
        /// </summary>
        public Audio.FormatEnum Format;

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan BufferTimeLength => TimeSpan.FromSeconds((double)SampleCount / (double)Frequency);

        private unsafe ProduceConsumeBuffer<short> Buffer = new ProduceConsumeBuffer<short>();
        private List<Tuple<long, Action>> BufferEvents = new List<Tuple<long, Action>>();

        /// <summary>
        /// Returns the total number of audio channels present.
        /// </summary>
        public int NumberOfChannels
        {
            get
            {
                switch (Format)
                {
                    case Audio.FormatEnum.Mono: return 1;
                    case Audio.FormatEnum.Stereo: return 2;
                    default: throw (new InvalidAudioFormatException());
                }
            }
            set
            {
                switch (value)
                {
                    case 1:
                        Format = Audio.FormatEnum.Mono;
                        break;
                    case 2:
                        Format = Audio.FormatEnum.Stereo;
                        break;
                    default: throw (new InvalidAudioFormatException());
                }
            }
        }

        private short[] Samples = new short[0];
        private short[] StereoSamplesBuffer = new short[0];

        public int VolumeLeft = Audio.MaxVolume;
        public int VolumeRight = Audio.MaxVolume;

        public void Release()
        {
            this.IsReserved = false;
            this.VolumeLeft = Audio.MaxVolume;
            this.VolumeRight = Audio.MaxVolume;
        }

        public void Updated()
        {
            if (SampleCount < 1) throw (new InvalidOperationException("SampleCount < 1"));
            if (NumberOfChannels < 1) throw (new InvalidOperationException("NumberOfChannels < 1"));
            this.Samples = new short[SampleCount * NumberOfChannels];
            this.StereoSamplesBuffer = new short[SampleCount * 2];
            this.VolumeLeft = Audio.MaxVolume;
            this.VolumeRight = Audio.MaxVolume;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Audio"></param>
        public AudioChannel(Audio Audio)
        {
            this.Audio = Audio;
        }

        /// <summary>
        /// Read 'Count' number of samples
        /// </summary>
        /// <param name="Count">Number of samples to read</param>
        /// <returns></returns>
        public short[] Read(int Count)
        {
            lock (this)
            {
                try
                {
                    /*
                    short[] Readed = new short[Count];
                    int ReadCount = Math.Min(Buffer.ConsumeRemaining, Count);
                    Buffer.Consume(Readed, 0, ReadCount);
                    return Readed;
                    */
                    return Buffer.Consume(Math.Min(Buffer.ConsumeRemaining, Count));
                }
                finally
                {
                    foreach (var Event in BufferEvents
                        .Where(ExpectedConsumed => Buffer.TotalConsumed >= ExpectedConsumed.Item1).ToArray())
                    {
                        BufferEvents.Remove(Event);
                        Event.Item2();
                    }
                }
            }
        }

        /// <summary>
        /// Converts a buffer containing mono samples
        /// into a buffer containing of stereo samples
        /// </summary>
        /// <param name="MonoSamples">Buffer that contains mono samples.</param>
        /// <returns>A buffer that contains stereo samples.</returns>
        private short[] MonoToStereo(short[] MonoSamples)
        {
            var StereoSamples = StereoSamplesBuffer;
            for (int n = 0; n < MonoSamples.Length; n++)
            {
                StereoSamples[n * 2 + 0] = MonoSamples[n];
                StereoSamples[n * 2 + 1] = MonoSamples[n];
            }
            return StereoSamples;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Samples"></param>
        /// <param name="ActionCallbackOnReaded"></param>
        public void Write(short[] Samples, Action ActionCallbackOnReaded)
        {
            if (Samples == null) throw (new InvalidOperationException("short[] Samples is null"));

            short[] StereoSamples;

            switch (Format)
            {
                case Audio.FormatEnum.Mono:
                    StereoSamples = MonoToStereo(Samples);
                    break;
                default:
                case Audio.FormatEnum.Stereo:
                    StereoSamples = Samples;
                    break;
            }

            lock (this)
            {
                if (Buffer.ConsumeRemaining < FillSamples)
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(1);
                        ActionCallbackOnReaded();
                    });
                }
                else
                {
                    BufferEvents.Add(new Tuple<long, Action>(Buffer.TotalConsumed + StereoSamples.Length,
                        ActionCallbackOnReaded));
                }
                //Console.WriteLine(Format);
                Buffer.Produce(StereoSamples);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SamplePointer"></param>
        /// <param name="VolumeLeft"></param>
        /// <param name="VolumeRight"></param>
        /// <param name="ActionCallbackOnReaded"></param>
        public void Write(short* SamplePointer, int VolumeLeft, int VolumeRight, Action ActionCallbackOnReaded)
        {
            //Console.WriteLine("{0}", this.Frequency);
            VolumeLeft = VolumeLeft * this.VolumeLeft / Audio.MaxVolume;
            VolumeRight = VolumeRight * this.VolumeRight / Audio.MaxVolume;

            if (SamplePointer != null)
            {
                if (NumberOfChannels == 1)
                {
                    int Volume = (VolumeLeft + VolumeRight) / 2;
                    for (int n = 0; n < Samples.Length; n++)
                    {
                        Samples[n + 0] = (short)(((int)SamplePointer[n + 0] * Volume) / Audio.MaxVolume);
                    }
                }
                else
                {
                    for (int n = 0; n < Samples.Length; n += 2)
                    {
                        Samples[n + 0] = (short)(((int)SamplePointer[n + 0] * VolumeLeft) / Audio.MaxVolume);
                        Samples[n + 1] = (short)(((int)SamplePointer[n + 1] * VolumeRight) / Audio.MaxVolume);
                    }
                }
            }

            Write(Samples, ActionCallbackOnReaded);
        }

        /// <summary>
        /// Available channels that can be read.
        /// </summary>
        public int AvailableChannelsForRead => Buffer.ConsumeRemaining;

        public override string ToString()
        {
            return
                $"AudioChannel(Index={Index},Frequency={Frequency},Format={Format},Channels={NumberOfChannels},SampleCount={SampleCount})";
        }
    }

    public class InvalidAudioFormatException : Exception
    {
    }

    public abstract class PspAudioImpl 
    {
        /// <summary>
        /// Called periodically on a thread.
        /// </summary>
        public abstract void Update(Action<short[]> ReadStream);

        /// <summary>
        /// 
        /// </summary>
        public abstract void StopSynchronized();

        /// <summary>
        /// 
        /// </summary>
        //public void __TestAudio()
        //{
        //	int m = 0;
        //	Action<short[]> Generator = (Data) =>
        //	{
        //		//Console.WriteLine("aaaa");
        //		for (int n = 0; n < Data.Length; n++)
        //		{
        //			Data[n] = (short)(Math.Cos(((double)m) / 100) * short.MaxValue);
        //			m++;
        //			//Console.WriteLine(Data[n]);
        //		}
        //	};
        //	byte[] GcTestData;
        //	while (true)
        //	{
        //		GcTestData = new byte[4 * 1024 * 1024];
        //		this.Update(Generator);
        //		Thread.Sleep(2);
        //		GC.Collect();
        //	}
        //}
    }
}
