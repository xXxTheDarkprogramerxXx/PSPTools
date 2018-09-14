using System;
using System.Collections.Generic;
using System.Threading;

/*
This file is part of pspsharp.

pspsharp is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

pspsharp is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with pspsharp.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace pspsharp.HLE.modules
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.RuntimeContext.getMemoryInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.RuntimeContext.hasMemoryInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_High;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.USER_PARTITION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceAudiocodec.PSP_CODEC_AT3PLUS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.psmf.PsmfAudioDemuxVirtualFile.PACK_START_CODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.psmf.PsmfAudioDemuxVirtualFile.PADDING_STREAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.psmf.PsmfAudioDemuxVirtualFile.PRIVATE_STREAM_1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.psmf.PsmfAudioDemuxVirtualFile.PRIVATE_STREAM_2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.psmf.PsmfAudioDemuxVirtualFile.SYSTEM_HEADER_START_CODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.endianSwap16;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.endianSwap32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUnaligned16;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;


	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceMp4AvcNalStruct = pspsharp.HLE.kernel.types.SceMp4AvcNalStruct;
	using SceMpegAu = pspsharp.HLE.kernel.types.SceMpegAu;
	using SceMpegRingbuffer = pspsharp.HLE.kernel.types.SceMpegRingbuffer;
	using pspFileBuffer = pspsharp.HLE.kernel.types.pspFileBuffer;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using PesHeader = pspsharp.format.psmf.PesHeader;
	using VideoEngine = pspsharp.graphics.VideoEngine;
	using Screen = pspsharp.hardware.Screen;
	using CodecFactory = pspsharp.media.codec.CodecFactory;
	using ICodec = pspsharp.media.codec.ICodec;
	using IVideoCodec = pspsharp.media.codec.IVideoCodec;
	using H264Utils = pspsharp.media.codec.h264.H264Utils;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using DelayThreadAction = pspsharp.scheduler.DelayThreadAction;
	using UnblockThreadAction = pspsharp.scheduler.UnblockThreadAction;
	using Debug = pspsharp.util.Debug;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	using H264Context = com.twilight.h264.decoder.H264Context;

	//
	// The stackUsage values are based on tests performed using JpcspTrace
	//

	public class sceMpeg : HLEModule
	{
		public static Logger log = Modules.getLogger("sceMpeg");

		public override int MemoryUsage
		{
			get
			{
				// No need to allocate additional memory when the module has been
				// loaded using sceKernelLoadModuleToBlock()
				// by the PSP "flash0:/kd/utility.prx".
				// The memory has already been allocated in that case.
				if (Modules.ModuleMgrForKernelModule.isMemoryAllocatedForModule("flash0:/kd/mpeg.prx"))
				{
					return 0;
				}
    
				return 0xC000;
			}
		}

		public override void start()
		{
			mpegHandle = 0;
			mpegRingbuffer = null;
			mpegRingbufferAddr = null;
			avcAuAddr = 0;
			atracAuAddr = 0;
			mpegAtracAu = new SceMpegAu();
			mpegAvcAu = new SceMpegAu();
			mpegUserDataAu = new SceMpegAu();
			psmfHeader = null;

			intBuffers = new HashSet<int[]>();
			lastFrameABGR = null;

			audioDecodeBuffer = new sbyte[MPEG_ATRAC_ES_OUTPUT_SIZE];
			allocatedEsBuffers = new bool[2];
			streamMap = new Dictionary<int, StreamInfo>();

			videoCodecExtraData = null;

			base.start();
		}

		public override void stop()
		{
			// Free the temporary arrays
			intBuffers.Clear();

			// Free objects no longer used
			audioDecodeBuffer = null;
			allocatedEsBuffers = null;
			streamMap = null;
			mpegAtracAu = null;
			mpegAvcAu = null;

			base.stop();
		}

		// MPEG statics.
		public const int PSMF_MAGIC = 0x464D5350;
		public const int PSMF_MAGIC_LITTLE_ENDIAN = 0x50534D46;
		public const int PSMF_VERSION_0012 = 0x32313030;
		public const int PSMF_VERSION_0013 = 0x33313030;
		public const int PSMF_VERSION_0014 = 0x34313030;
		public const int PSMF_VERSION_0015 = 0x35313030;
		public const int PSMF_MAGIC_OFFSET = 0x0;
		public const int PSMF_STREAM_VERSION_OFFSET = 0x4;
		public const int PSMF_STREAM_OFFSET_OFFSET = 0x8;
		public const int PSMF_STREAM_SIZE_OFFSET = 0xC;
		public const int PSMF_FIRST_TIMESTAMP_OFFSET = 0x54;
		public const int PSMF_LAST_TIMESTAMP_OFFSET = 0x5A;
		public const int PSMF_NUMBER_STREAMS_OFFSET = 0x80;
		public const int PSMF_FRAME_WIDTH_OFFSET = 0x8E;
		public const int PSMF_FRAME_HEIGHT_OFFSET = 0x8F;
		protected internal const int MPEG_MEMSIZE = 0x10000; // 64k.
		private const int AUDIO_BUFFER_OFFSET = 0x100; // Offset of the audio buffer inside MPEG structure
		private const int AUDIO_BUFFER_SIZE = 0x1000; // Size of the audio buffer
		public const int atracDecodeDelay = 3000; // Microseconds
		public const int avcDecodeDelay = 5400; // Microseconds
		public const int mpegDecodeErrorDelay = 100; // Delay in Microseconds in case of decode error
		public const int mpegTimestampPerSecond = 90000; // How many MPEG Timestamp units in a second.
		public const int videoTimestampStep = 3003; // Value based on pmfplayer (mpegTimestampPerSecond / 29.970 (fps)).
		public const int audioTimestampStep = 4180; // For audio play at 44100 Hz (2048 samples / 44100 * mpegTimestampPerSecond == 4180)
		public const long UNKNOWN_TIMESTAMP = -1;
		public const int PSMF_AVC_STREAM = 0;
		public const int PSMF_ATRAC_STREAM = 1;
		public const int PSMF_PCM_STREAM = 2;
		public const int PSMF_DATA_STREAM = 3;
		public const int PSMF_AUDIO_STREAM = 15;
		public const int PSMF_VIDEO_STREAM_ID = 0xE0;
		public const int PSMF_AUDIO_STREAM_ID = 0xBD;
		// The YCbCr buffer is starting with 128 bytes of unknown data
		protected internal const int YCBCR_DATA_OFFSET = 128;

		// At least 2048 bytes of MPEG data is provided when analysing the MPEG header
		public const int MPEG_HEADER_BUFFER_MINIMUM_SIZE = 2048;

		// MPEG processing vars.
		protected internal int mpegHandle;
		protected internal TPointer mpegAvcDetail2Struct;
		protected internal TPointer mpegAvcInfoStruct;
		protected internal TPointer mpegAvcYuvStruct;
		protected internal SceMpegRingbuffer mpegRingbuffer;
		protected internal TPointer mpegRingbufferAddr;
		protected internal SceMpegAu mpegAtracAu;
		protected internal SceMpegAu mpegAvcAu;
		protected internal SceMpegAu mpegUserDataAu;
		protected internal long lastAtracSystemTime;
		protected internal long lastAvcSystemTime;
		protected internal int avcAuAddr;
		protected internal int atracAuAddr;
		protected internal int videoFrameCount;
		protected internal int audioFrameCount;
		private long currentVideoTimestamp;
		private long currentAudioTimestamp;
		protected internal int videoPixelMode;
		protected internal int defaultFrameWidth;
		// MPEG AVC elementary stream.
		public const int MPEG_AVC_ES_SIZE = 2048; // MPEG packet size.
		// MPEG ATRAC elementary stream.
		protected internal const int MPEG_ATRAC_ES_SIZE = 2112;
		public const int MPEG_ATRAC_ES_OUTPUT_SIZE = 8192;
		// MPEG PCM elementary stream.
		protected internal const int MPEG_PCM_ES_SIZE = 320;
		protected internal const int MPEG_PCM_ES_OUTPUT_SIZE = 320;
		// MPEG Userdata elementary stream.
		protected internal const int MPEG_DATA_ES_SIZE = 0xA0000;
		protected internal const int MPEG_DATA_ES_OUTPUT_SIZE = 0xA0000;
		// MPEG analysis results.
		public const int MPEG_VERSION_0012 = 0;
		public const int MPEG_VERSION_0013 = 1;
		public const int MPEG_VERSION_0014 = 2;
		public const int MPEG_VERSION_0015 = 3;
		protected internal const int MPEG_AU_MODE_DECODE = 0;
		protected internal const int MPEG_AU_MODE_SKIP = 1;
		protected internal int registeredVideoChannel = -1;
		protected internal int registeredAudioChannel = -1;
		// MPEG decoding results.
		protected internal const int MPEG_AVC_DECODE_SUCCESS = 1; // Internal value.
		protected internal const int MPEG_AVC_DECODE_ERROR_FATAL = -8;
		protected internal int avcDecodeResult;
		protected internal bool avcGotFrame;
		protected internal bool startedMpeg;
		protected internal sbyte[] audioDecodeBuffer;
		protected internal bool[] allocatedEsBuffers;
		protected internal Dictionary<int, StreamInfo> streamMap;
		protected internal const string streamPurpose = "sceMpeg-Stream";
		protected internal const int mpegAudioOutputChannels = 2;
		protected internal SysMemInfo avcEsBuf;
		public PSMFHeader psmfHeader;
		private AudioBuffer audioBuffer;
		private int audioFrameLength;
		private readonly int[] frameHeader = new int[8];
		private int frameHeaderLength;
		private ICodec audioCodec;
		private VideoBuffer videoBuffer;
		private IVideoCodec videoCodec;
		private int[] videoCodecExtraData;
		private const int MAX_INT_BUFFERS_SIZE = 12;
		private static ISet<int[]> intBuffers;
		private PesHeader audioPesHeader;
		private PesHeader videoPesHeader;
		private readonly PesHeader dummyPesHeader = new PesHeader(0);
		private VideoDecoderThread videoDecoderThread;
		private LinkedList<DecodedImageInfo> decodedImages;
		private int[] lastFrameABGR;
		private int lastFrameWidth;
		private int lastFrameHeight;
		private PesHeader userDataPesHeader;
		private UserDataBuffer userDataBuffer;
		private readonly int[] userDataHeader = new int[8];
		private int userDataLength;
		private int videoFrameHeight;
		// Not sure about the real size.
		public const int AVC_ES_BUF_SIZE = 0x2000;

		private class DecodedImageInfo
		{
			public PesHeader pesHeader;
			public int frameEnd;
			public bool gotFrame;
			public int imageWidth;
			public int imageHeight;
			public int[] luma;
			public int[] cr;
			public int[] cb;
			public int[] abgr;

			public override string ToString()
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("pesHeader=%s, frameEnd=0x%X, gotFrame=%b, image %dx%d", pesHeader, frameEnd, gotFrame, imageWidth, imageHeight);
				return string.Format("pesHeader=%s, frameEnd=0x%X, gotFrame=%b, image %dx%d", pesHeader, frameEnd, gotFrame, imageWidth, imageHeight);
			}
		}

		private class AudioBuffer
		{
			internal int addr;
			internal int size;
			internal int length;

			public AudioBuffer(int addr, int size)
			{
				this.addr = addr;
				this.size = size;
				length = 0;
			}

			public virtual int write(Memory mem, int dataAddr, int size)
			{
				size = System.Math.Min(size, FreeLength);
				mem.memcpy(addr + length, dataAddr, size);
				length += size;

				return size;
			}

			public virtual int Length
			{
				get
				{
					return length;
				}
			}

			public virtual int ReadAddr
			{
				get
				{
					return addr;
				}
			}

			public virtual int FreeLength
			{
				get
				{
					return size - length;
				}
			}

			public virtual int notifyRead(Memory mem, int size)
			{
				size = System.Math.Min(size, length);
				length -= size;
				mem.memcpy(addr, addr + size, length);

				return size;
			}

			public virtual bool Empty
			{
				get
				{
					return length == 0;
				}
			}

			public virtual void reset()
			{
				length = 0;
			}
		}

		private class VideoBuffer
		{
			internal int[] buffer = new int[10000];
			internal int length;
			internal static int[] quickSearch;
			internal int[] frameSizes;
			internal int frame;

			public VideoBuffer()
			{
				length = 0;
				frame = 0;
				frameSizes = null;

				initQuickSearch();
			}

			internal static void initQuickSearch()
			{
				if (quickSearch != null)
				{
					return;
				}

				quickSearch = new int[256];
				Arrays.fill(quickSearch, 5);
				quickSearch[0] = 2;
				quickSearch[1] = 1;
			}

			public virtual void write(Memory mem, int dataAddr, int size)
			{
				lock (this)
				{
					if (log.TraceEnabled)
					{
						log.trace(string.Format("VideoBuffer.write addr=0x{0:X8}, size=0x{1:X}, {2}", dataAddr, size, this));
					}
        
					if (size + length > buffer.Length)
					{
						int[] extendedBuffer = new int[size + length];
						Array.Copy(buffer, 0, extendedBuffer, 0, length);
						buffer = extendedBuffer;
					}
        
					IMemoryReader memoryReader = MemoryReader.getMemoryReader(dataAddr, size, 1);
					for (int i = 0; i < size; i++)
					{
						buffer[length++] = memoryReader.readNext();
					}
				}
			}

			public virtual int[] Buffer
			{
				get
				{
					return buffer;
				}
			}

			public virtual int BufferOffset
			{
				get
				{
					return 0;
				}
			}

			public virtual void notifyRead(int size)
			{
				lock (this)
				{
					if (log.TraceEnabled)
					{
						log.trace(string.Format("VideoBuffer.notifyRead size=0x{0:X}, {1}", size, this));
					}
        
					size = System.Math.Min(size, length);
					length -= size;
					Array.Copy(buffer, size, buffer, 0, length);
        
					frame++;
				}
			}

			public virtual void reset()
			{
				lock (this)
				{
					length = 0;
				}
			}

			public virtual int Length
			{
				get
				{
					return length;
				}
			}

			public virtual bool Empty
			{
				get
				{
					return length == 0;
				}
			}

			public virtual int findFrameEnd()
			{
				lock (this)
				{
					if (frameSizes != null && frame < frameSizes.Length)
					{
						if (log.TraceEnabled)
						{
							log.trace(string.Format("VideoBuffer.findFrameEnd frameSize=0x{0:X}, {1}", frameSizes[frame], this));
						}
        
						return frameSizes[frame];
					}
        
					for (int i = 5; i < length;)
					{
						int value = buffer[i];
						if (buffer[i - 4] == 0x00 && buffer[i - 3] == 0x00 && buffer[i - 2] == 0x00 && buffer[i - 1] == 0x01)
						{
							int nalUnitType = value & 0x1F;
							if (nalUnitType == H264Context.NAL_AUD)
							{
								return i - 4;
							}
						}
						i += quickSearch[value];
					}
        
					return -1;
				}
			}

			public virtual int[] FrameSizes
			{
				set
				{
					this.frameSizes = value;
					frame = 0;
				}
			}

			public virtual int Frame
			{
				set
				{
					lock (this)
					{
						if (frameSizes != null)
						{
							if (log.TraceEnabled)
							{
								log.trace(string.Format("VideoBuffer.setFrame newFrame=0x{0:X}, {1}", value, this));
							}
            
							if (value < frame)
							{
								reset();
							}
							else
							{
								// Skip frames up to new frame
								while (frame < value && !Empty)
								{
									notifyRead(frameSizes[frame]);
								}
							}
						}
						this.frame = value;
					}
				}
			}

			public override string ToString()
			{
				return string.Format("VideoBuffer[length=0x{0:X}, frame=0x{1:X}]", length, frame);
			}
		}

		private class UserDataBuffer
		{
			internal int addr;
			internal int size;
			internal int length;

			public UserDataBuffer(int addr, int size)
			{
				this.addr = addr;
				this.size = size;
				length = 0;
			}

			public virtual int write(Memory mem, int dataAddr, int size)
			{
				size = System.Math.Min(size, FreeLength);
				mem.memcpy(addr + length, dataAddr, size);
				length += size;

				return size;
			}

			public virtual int Length
			{
				get
				{
					return length;
				}
			}

			public virtual int FreeLength
			{
				get
				{
					return size - length;
				}
			}

			public virtual int notifyRead(Memory mem, int size)
			{
				size = System.Math.Min(size, length);
				length -= size;
				mem.memcpy(addr, addr + size, length);

				return size;
			}
		}

		// Entry class for the PSMF streams.
		public class PSMFStream
		{
			internal int streamType = -1;
			internal int streamChannel = -1;
			internal int streamNumber;
			internal int EPMapNumEntries;
			internal int EPMapOffset;
			internal IList<PSMFEntry> EPMap;
			internal int frameWidth;
			internal int frameHeight;

			public PSMFStream(int streamNumber)
			{
				this.streamNumber = streamNumber;
			}

			public virtual int StreamType
			{
				get
				{
					return streamType;
				}
			}

			public virtual int StreamChannel
			{
				get
				{
					return streamChannel;
				}
			}

			public virtual int StreamNumber
			{
				get
				{
					return streamNumber;
				}
			}

			public virtual bool isStreamOfType(int type)
			{
				if (streamType == type)
				{
					return true;
				}
				if (type == PSMF_AUDIO_STREAM)
				{
					// Atrac or PCM
					return streamType == PSMF_ATRAC_STREAM || streamType == PSMF_PCM_STREAM;
				}

				return false;
			}

			public virtual void readMPEGVideoStreamParams(Memory mem, int addr, sbyte[] mpegHeader, int offset, PSMFHeader psmfHeader)
			{
				int streamID = read8(mem, addr, mpegHeader, offset); // 0xE0
				int privateStreamID = read8(mem, addr, mpegHeader, offset + 1); // 0x00
				int unk1 = read8(mem, addr, mpegHeader, offset + 2); // Found values: 0x20/0x21
				int unk2 = read8(mem, addr, mpegHeader, offset + 3); // Found values: 0x44/0xFB/0x75
				EPMapOffset = endianSwap32(sceMpeg.readUnaligned32(mem, addr, mpegHeader, offset + 4));
				EPMapNumEntries = endianSwap32(sceMpeg.readUnaligned32(mem, addr, mpegHeader, offset + 8));
				frameWidth = read8(mem, addr, mpegHeader, offset + 12) * 0x10; // PSMF video width (bytes per line).
				frameHeight = read8(mem, addr, mpegHeader, offset + 13) * 0x10; // PSMF video heigth (bytes per line).

				if (log.InfoEnabled)
				{
					log.info(string.Format("Found PSMF MPEG video stream data: streamID=0x{0:X}, privateStreamID=0x{1:X}, unk1=0x{2:X}, unk2=0x{3:X}, EPMapOffset=0x{4:x}, EPMapNumEntries={5:D}, frameWidth={6:D}, frameHeight={7:D}", streamID, privateStreamID, unk1, unk2, EPMapOffset, EPMapNumEntries, frameWidth, frameHeight));
				}

				streamType = PSMF_AVC_STREAM;
				streamChannel = streamID & 0x0F;
			}

			public virtual void readPrivateAudioStreamParams(Memory mem, int addr, sbyte[] mpegHeader, int offset, PSMFHeader psmfHeader)
			{
				int streamID = read8(mem, addr, mpegHeader, offset); // 0xBD
				int privateStreamID = read8(mem, addr, mpegHeader, offset + 1); // 0x00
				int unk1 = read8(mem, addr, mpegHeader, offset + 2); // Always 0x20
				int unk2 = read8(mem, addr, mpegHeader, offset + 3); // Always 0x04
				int audioChannelConfig = read8(mem, addr, mpegHeader, offset + 14); // 1 - mono, 2 - stereo
				int audioSampleFrequency = read8(mem, addr, mpegHeader, offset + 15); // 2 - 44khz

				if (log.InfoEnabled)
				{
					log.info(string.Format("Found PSMF MPEG audio stream data: streamID=0x{0:X}, privateStreamID=0x{1:X}, unk1=0x{2:X}, unk2=0x{3:X}, audioChannelConfig={4:D}, audioSampleFrequency={5:D}", streamID, privateStreamID, unk1, unk2, audioChannelConfig, audioSampleFrequency));
				}

				if (psmfHeader != null)
				{
					psmfHeader.audioChannelConfig = audioChannelConfig;
					psmfHeader.audioSampleFrequency = audioSampleFrequency;
				}

				streamType = ((privateStreamID & 0xF0) == 0 ? PSMF_ATRAC_STREAM : PSMF_PCM_STREAM);
				streamChannel = privateStreamID & 0x0F;
			}

			public virtual void readUserDataStreamParams(Memory mem, int addr, sbyte[] mpegHeader, int offset, PSMFHeader psmfHeader)
			{
				log.warn(string.Format("Unknown User Data stream format"));
				streamType = PSMF_DATA_STREAM;
			}
		}

		// Entry class for the EPMap.
		protected internal class PSMFEntry
		{
			internal int EPIndex;
			internal int EPPicOffset;
			internal int EPPts;
			internal int EPOffset;
			internal int id;

			public PSMFEntry(int id, int index, int picOffset, int pts, int offset)
			{
				this.id = id;
				EPIndex = index;
				EPPicOffset = picOffset;
				EPPts = pts;
				EPOffset = offset;
			}

			public virtual int EntryIndex
			{
				get
				{
					return EPIndex;
				}
			}

			public virtual int EntryPicOffset
			{
				get
				{
					return EPPicOffset;
				}
			}

			public virtual int EntryPTS
			{
				get
				{
					return EPPts;
				}
			}

			public virtual int EntryOffset
			{
				get
				{
					return EPOffset * MPEG_AVC_ES_SIZE;
				}
			}

			public virtual int Id
			{
				get
				{
					return id;
				}
			}

			public override string ToString()
			{
				return string.Format("id={0:D}, index=0x{1:X}, picOffset=0x{2:X}, PTS=0x{3:X}, offset=0x{4:X}", Id, EntryIndex, EntryPicOffset, EntryPTS, EntryOffset);
			}
		}

		public class PSMFHeader
		{
			internal const int size = 2048;

			// Header vars.
			public int mpegMagic;
			public int mpegRawVersion;
			public int mpegVersion;
			public int mpegOffset;
			public int mpegStreamSize;
			public long mpegFirstTimestamp;
			public long mpegLastTimestamp;
			public DateTime mpegFirstDate;
			public DateTime mpegLastDate;
			internal int streamNum;
			internal int audioSampleFrequency;
			internal int audioChannelConfig;
			internal int avcDetailFrameWidth;
			internal int avcDetailFrameHeight;

			// Stream map.
			public IList<PSMFStream> psmfStreams;
			internal PSMFStream currentStream = null;
			internal PSMFStream currentVideoStream = null;

			public PSMFHeader()
			{
			}

			public PSMFHeader(int bufferAddr, sbyte[] mpegHeader)
			{
				Memory mem = Memory.Instance;

				int streamDataTotalSize = endianSwap32(readUnaligned32(mem, bufferAddr, mpegHeader, 0x50));
				int unk = endianSwap32(readUnaligned32(mem, bufferAddr, mpegHeader, 0x60));
				int streamDataNextBlockSize = endianSwap32(readUnaligned32(mem, bufferAddr, mpegHeader, 0x6A)); // General stream information block size.
				int streamDataNextInnerBlockSize = endianSwap32(readUnaligned32(mem, bufferAddr, mpegHeader, 0x7C)); // Inner stream information block size.
				streamNum = endianSwap16(read16(mem, bufferAddr, mpegHeader, PSMF_NUMBER_STREAMS_OFFSET)); // Number of total registered streams.

				mpegMagic = read32(mem, bufferAddr, mpegHeader, PSMF_MAGIC_OFFSET);
				mpegRawVersion = read32(mem, bufferAddr, mpegHeader, PSMF_STREAM_VERSION_OFFSET);
				mpegVersion = getMpegVersion(mpegRawVersion);
				mpegOffset = endianSwap32(read32(mem, bufferAddr, mpegHeader, PSMF_STREAM_OFFSET_OFFSET));
				mpegStreamSize = endianSwap32(read32(mem, bufferAddr, mpegHeader, PSMF_STREAM_SIZE_OFFSET));
				mpegFirstTimestamp = readTimestamp(mem, bufferAddr, mpegHeader, PSMF_FIRST_TIMESTAMP_OFFSET);
				mpegLastTimestamp = readTimestamp(mem, bufferAddr, mpegHeader, PSMF_LAST_TIMESTAMP_OFFSET);
				avcDetailFrameWidth = read8(mem, bufferAddr, mpegHeader, PSMF_FRAME_WIDTH_OFFSET) << 4;
				avcDetailFrameHeight = read8(mem, bufferAddr, mpegHeader, PSMF_FRAME_HEIGHT_OFFSET) << 4;

				mpegFirstDate = convertTimestampToDate(mpegFirstTimestamp);
				mpegLastDate = convertTimestampToDate(mpegLastTimestamp);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("PSMFHeader: version=0x{0:X4}, firstTimestamp={1:D}, lastTimestamp={2:D}, streamDataTotalSize={3:D}, unk=0x{4:X8}, streamDataNextBlockSize={5:D}, streamDataNextInnerBlockSize={6:D}, streamNum={7:D}", Version, mpegFirstTimestamp, mpegLastTimestamp, streamDataTotalSize, unk, streamDataNextBlockSize, streamDataNextInnerBlockSize, streamNum));
				}

				if (Valid)
				{
					psmfStreams = readPsmfStreams(mem, bufferAddr, mpegHeader, this);

					// PSP seems to default to stream 0.
					if (psmfStreams.Count > 0)
					{
						StreamNum = 0;
					}

					// EPMap info:
					// - Located at EPMapOffset (set by the AVC stream);
					// - Each entry is composed by a total of 10 bytes:
					//      - 1 byte: Reference picture index (RAPI);
					//      - 1 byte: Reference picture offset from the current index;
					//      - 4 bytes: PTS of the entry point;
					//      - 4 bytes: Relative offset of the entry point in the MPEG data.
					foreach (PSMFStream stream in psmfStreams)
					{
						// Do not read the EPMap when it is too large to be contained in the
						// first 2048 bytes of the header. Applications do usually only provide
						// this amount data.
						if (stream.EPMapOffset + stream.EPMapNumEntries * 10 <= MPEG_HEADER_BUFFER_MINIMUM_SIZE)
						{
							stream.EPMap = new LinkedList<sceMpeg.PSMFEntry>();
							int EPMapOffset = stream.EPMapOffset;
							for (int i = 0; i < stream.EPMapNumEntries; i++)
							{
								int index = read8(mem, bufferAddr, mpegHeader, EPMapOffset + i * 10);
								int picOffset = read8(mem, bufferAddr, mpegHeader, EPMapOffset + 1 + i * 10);
								int pts = endianSwap32(readUnaligned32(mem, bufferAddr, mpegHeader, EPMapOffset + 2 + i * 10));
								int offset = endianSwap32(readUnaligned32(mem, bufferAddr, mpegHeader, EPMapOffset + 6 + i * 10));
								PSMFEntry psmfEntry = new PSMFEntry(i, index, picOffset, pts, offset);
								stream.EPMap.Add(psmfEntry);
								if (log.DebugEnabled)
								{
									log.debug(string.Format("EPMap stream {0:D}, entry#{1:D}: {2}", stream.StreamChannel, i, psmfEntry));
								}
							}
						}
					}
				}
			}

			internal virtual long readTimestamp(Memory mem, int bufferAddr, sbyte[] mpegHeader, int offset)
			{
				long timestamp = endianSwap32(readUnaligned32(mem, bufferAddr, mpegHeader, offset + 2)) & 0xFFFFFFFFL;
				timestamp |= ((long) read8(mem, bufferAddr, mpegHeader, offset + 1)) << 32;
				timestamp |= ((long) read8(mem, bufferAddr, mpegHeader, offset + 0)) << 40;

				return timestamp;
			}

			public virtual bool Valid
			{
				get
				{
					return mpegFirstTimestamp == 90000 && mpegFirstTimestamp < mpegLastTimestamp && mpegLastTimestamp > 0;
				}
			}

			public virtual bool Invalid
			{
				get
				{
					return !Valid;
				}
			}

			public virtual int Version
			{
				get
				{
					return mpegRawVersion;
				}
			}

			public virtual int HeaderSize
			{
				get
				{
					return size;
				}
			}

			public virtual int StreamOffset
			{
				get
				{
					return mpegOffset;
				}
			}

			public virtual int StreamSize
			{
				get
				{
					return mpegStreamSize;
				}
			}

			public virtual int PresentationStartTime
			{
				get
				{
					return (int) mpegFirstTimestamp;
				}
			}

			public virtual int PresentationEndTime
			{
				get
				{
					return (int) mpegLastTimestamp;
				}
			}

			public virtual int VideoWidth
			{
				get
				{
					return avcDetailFrameWidth;
				}
			}

			public virtual int VideoHeight
			{
				get
				{
					return avcDetailFrameHeight;
				}
			}

			public virtual int AudioSampleFrequency
			{
				get
				{
					return audioSampleFrequency;
				}
			}

			public virtual int AudioChannelConfig
			{
				get
				{
					return audioChannelConfig;
				}
			}

			public virtual int EPMapEntriesNum
			{
				get
				{
					if (currentVideoStream == null)
					{
						return 0;
					}
					return currentVideoStream.EPMapNumEntries;
				}
			}

			public virtual bool hasEPMap()
			{
				return EPMapEntriesNum > 0;
			}

			public virtual PSMFEntry getEPMapEntry(int id)
			{
				if (!hasEPMap())
				{
					return null;
				}
				if (id < 0 || id >= currentVideoStream.EPMap.Count)
				{
					return null;
				}
				return currentVideoStream.EPMap[id];
			}

			public virtual PSMFEntry getEPMapEntryWithTimestamp(int ts)
			{
				if (!hasEPMap())
				{
					return null;
				}

				PSMFEntry foundEntry = null;
				foreach (PSMFEntry entry in currentVideoStream.EPMap)
				{
					if (foundEntry == null || entry.EntryPTS <= ts)
					{
						foundEntry = entry;
					}
					else if (entry.EntryPTS > ts)
					{
						break;
					}
				}

				return foundEntry;
			}

			public virtual PSMFEntry getEPMapEntryWithOffset(int offset)
			{
				if (!hasEPMap())
				{
					return null;
				}

				PSMFEntry foundEntry = null;
				foreach (PSMFEntry entry in currentVideoStream.EPMap)
				{
					if (foundEntry == null || entry.EntryOffset <= offset)
					{
						foundEntry = entry;
					}
					else if (entry.EntryOffset > offset)
					{
						break;
					}
				}

				return foundEntry;
			}

			public virtual int NumberOfStreams
			{
				get
				{
					return streamNum;
				}
			}

			public virtual int CurrentStreamNumber
			{
				get
				{
					if (!ValidCurrentStreamNumber)
					{
						return -1;
					}
					return currentStream.StreamNumber;
				}
			}

			public virtual bool ValidCurrentStreamNumber
			{
				get
				{
					return currentStream != null;
				}
			}

			public virtual int CurrentStreamType
			{
				get
				{
					if (!ValidCurrentStreamNumber)
					{
						return -1;
					}
					return currentStream.StreamType;
				}
			}

			public virtual int CurrentStreamChannel
			{
				get
				{
					if (!ValidCurrentStreamNumber)
					{
						return -1;
					}
					return currentStream.StreamChannel;
				}
			}

			public virtual int getSpecificStreamNum(int type)
			{
				int num = 0;
				if (psmfStreams != null)
				{
					foreach (PSMFStream stream in psmfStreams)
					{
						if (stream.isStreamOfType(type))
						{
							num++;
						}
					}
				}

				return num;
			}

			public virtual int StreamNum
			{
				set
				{
					if (value < 0 || value >= psmfStreams.Count)
					{
						currentStream = null;
					}
					else
					{
						currentStream = psmfStreams[value];
    
						int type = CurrentStreamType;
						int channel = CurrentStreamChannel;
						switch (type)
						{
							case PSMF_AVC_STREAM:
								currentVideoStream = currentStream;
								Modules.sceMpegModule.RegisteredVideoChannel = channel;
								break;
							case PSMF_PCM_STREAM:
							case PSMF_ATRAC_STREAM:
								Modules.sceMpegModule.RegisteredAudioChannel = channel;
								break;
						}
					}
				}
			}

			internal virtual int getStreamNumber(int type, int typeNum, int channel)
			{
				if (psmfStreams != null)
				{
					foreach (PSMFStream stream in psmfStreams)
					{
						if (stream.isStreamOfType(type))
						{
							if (typeNum <= 0)
							{
								if (channel < 0 || stream.StreamChannel == channel)
								{
									return stream.StreamNumber;
								}
							}
							typeNum--;
						}
					}
				}

				return -1;
			}

			public virtual bool setStreamWithType(int type, int channel)
			{
				int streamNumber = getStreamNumber(type, 0, channel);
				if (streamNumber < 0)
				{
					return false;
				}
				StreamNum = streamNumber;

				return true;
			}

			public virtual bool setStreamWithTypeNum(int type, int typeNum)
			{
				int streamNumber = getStreamNumber(type, typeNum, -1);
				if (streamNumber < 0)
				{
					return false;
				}
				StreamNum = streamNumber;

				return true;
			}
		}

		public static int getPsmfNumStreams(Memory mem, int addr, sbyte[] mpegHeader)
		{
			return endianSwap16(read16(mem, addr, mpegHeader, sceMpeg.PSMF_NUMBER_STREAMS_OFFSET));
		}

		public static LinkedList<PSMFStream> readPsmfStreams(Memory mem, int addr, sbyte[] mpegHeader, PSMFHeader psmfHeader)
		{
			int numStreams = getPsmfNumStreams(mem, addr, mpegHeader);

			// Stream area:
			// At offset 0x82, each 16 bytes represent one stream.
			LinkedList<PSMFStream> streams = new LinkedList<PSMFStream>();

			// Parse the stream field and assign each one to it's type.
			int numberOfStreams = 0;
			for (int i = 0; i < numStreams; i++)
			{
				PSMFStream stream = null;
				int currentStreamOffset = 0x82 + i * 16;
				int streamID = read8(mem, addr, mpegHeader, currentStreamOffset);
				int subStreamID = read8(mem, addr, mpegHeader, currentStreamOffset + 1);
				if ((streamID & 0xF0) == PSMF_VIDEO_STREAM_ID)
				{
					stream = new PSMFStream(numberOfStreams);
					stream.readMPEGVideoStreamParams(mem, addr, mpegHeader, currentStreamOffset, psmfHeader);
				}
				else if (streamID == PSMF_AUDIO_STREAM_ID && subStreamID < 0x20)
				{
					stream = new PSMFStream(numberOfStreams);
					stream.readPrivateAudioStreamParams(mem, addr, mpegHeader, currentStreamOffset, psmfHeader);
				}
				else
				{
					stream = new PSMFStream(numberOfStreams);
					stream.readUserDataStreamParams(mem, addr, mpegHeader, currentStreamOffset, psmfHeader);
				}

				if (stream != null)
				{
					streams.AddLast(stream);
					numberOfStreams++;
				}
			}

			return streams;
		}

		private class StreamInfo
		{
			private readonly sceMpeg outerInstance;

			internal int uid;
			internal readonly int type;
			internal readonly int channel;
			internal int auMode;

			public StreamInfo(sceMpeg outerInstance, int type, int channel)
			{
				this.outerInstance = outerInstance;
				this.type = type;
				this.channel = channel;
				uid = SceUidManager.getNewUid(streamPurpose);
				AuMode = MPEG_AU_MODE_DECODE;

				outerInstance.streamMap[uid] = this;
			}

			public virtual int Uid
			{
				get
				{
					return uid;
				}
			}

			public virtual int Type
			{
				get
				{
					return type;
				}
			}

			public virtual int Channel
			{
				get
				{
					return channel;
				}
			}

			public virtual void release()
			{
				SceUidManager.releaseUid(uid, streamPurpose);
				outerInstance.streamMap.Remove(uid);
			}

			public virtual int AuMode
			{
				get
				{
					return auMode;
				}
				set
				{
					this.auMode = value;
				}
			}


			public virtual bool isStreamType(int type)
			{
				if (this.type == type)
				{
					return true;
				}

				if (this.type == PSMF_ATRAC_STREAM && type == PSMF_AUDIO_STREAM)
				{
					return true;
				}

				return false;
			}

			public override string ToString()
			{
				return string.Format("StreamInfo(uid=0x{0:X}, type={1:D}, auMode={2:D})", Uid, Type, AuMode);
			}
		}

		public class AfterRingbufferPutCallback : IAction
		{
			private readonly sceMpeg outerInstance;

			internal int putDataAddr;
			internal int remainingPackets;
			internal int totalPacketsAdded;

			public AfterRingbufferPutCallback(sceMpeg outerInstance, int putDataAddr, int remainingPackets)
			{
				this.outerInstance = outerInstance;
				this.putDataAddr = putDataAddr;
				this.remainingPackets = remainingPackets;
			}

			public virtual void execute()
			{
				outerInstance.hleMpegRingbufferPostPut(this, Emulator.Processor.cpu._v0);
			}

			public virtual int PutDataAddr
			{
				get
				{
					return putDataAddr;
				}
				set
				{
					this.putDataAddr = value;
				}
			}


			public virtual int RemainingPackets
			{
				get
				{
					return remainingPackets;
				}
				set
				{
					this.remainingPackets = value;
				}
			}


			public virtual int TotalPacketsAdded
			{
				get
				{
					return totalPacketsAdded;
				}
			}

			public virtual void addPacketsAdded(int packetsAdded)
			{
				// Add only if we don't return an error code
				if (packetsAdded > 0 && totalPacketsAdded >= 0)
				{
					totalPacketsAdded += packetsAdded;
				}
			}

			public virtual int ErrorCode
			{
				set
				{
					totalPacketsAdded = value;
				}
			}
		}

		/// <summary>
		/// Always decode one frame in advance so that sceMpegAvcDecode
		/// can be timed like on a real PSP.
		/// </summary>
		private class VideoDecoderThread : Thread
		{
			private readonly sceMpeg outerInstance;

			public VideoDecoderThread(sceMpeg outerInstance)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal volatile bool exit_Renamed = false;
			internal volatile bool done = false;
			// Start with one semaphore permit to not wait on the first loop
			internal Semaphore sema = new Semaphore(1);
			internal int threadUid = -1;
			internal int buffer;
			internal int frameWidth;
			internal int pixelMode;
			internal TPointer32 gotFrameAddr;
			internal bool writeAbgr;
			internal TPointer auAddr;
			internal long threadWakeupMicroTime;

			public override void run()
			{
				while (!exit_Renamed)
				{
					if (waitForTrigger(100) && !exit_Renamed)
					{
						outerInstance.hleVideoDecoderStep(threadUid, buffer, frameWidth, pixelMode, gotFrameAddr, writeAbgr, auAddr, threadWakeupMicroTime);
					}
				}

				if (log.DebugEnabled)
				{
					log.debug("Exiting the VideoDecoderThread");
				}
				done = true;
			}

			public virtual void exit()
			{
				exit_Renamed = true;
				trigger(-1, 0, 0, -1, null, false, TPointer.NULL, 0L);

				while (!done)
				{
					Utilities.sleep(1);
				}
			}

			public virtual void trigger(int threadUid, int buffer, int frameWidth, int pixelMode, TPointer32 gotFrameAddr, bool writeAbgr, TPointer auAddr, long threadWakeupMicroTime)
			{
				this.threadUid = threadUid;
				this.buffer = buffer;
				this.frameWidth = frameWidth;
				this.pixelMode = pixelMode;
				this.gotFrameAddr = gotFrameAddr;
				this.writeAbgr = writeAbgr;
				this.auAddr = auAddr;
				this.threadWakeupMicroTime = threadWakeupMicroTime;

				trigger();
			}

			public virtual void resetWaitingThreadInfo()
			{
				threadUid = -1;
				buffer = 0;
				frameWidth = 0;
				pixelMode = -1;
				gotFrameAddr = null;
				threadWakeupMicroTime = 0;
			}

			public virtual void trigger()
			{
				if (sema != null)
				{
					sema.release();
				}
			}

			internal virtual bool waitForTrigger(int millis)
			{
				while (true)
				{
					try
					{
						int availablePermits = sema.drainPermits();
						if (availablePermits > 0)
						{
							break;
						}

						if (sema.tryAcquire(millis, TimeUnit.MILLISECONDS))
						{
							break;
						}

						return false;
					}
					catch (InterruptedException)
					{
						// Ignore exception and retry
					}
				}

				return true;
			}
		}

		protected internal virtual void mpegRingbufferWrite()
		{
			if (mpegRingbuffer != null && mpegRingbufferAddr != null && mpegRingbufferAddr.NotNull)
			{
				lock (mpegRingbuffer)
				{
					mpegRingbuffer.notifyConsumed();
					mpegRingbuffer.write(mpegRingbufferAddr);
				}
			}
		}

		protected internal virtual void mpegRingbufferNotifyRead()
		{
			int numberDecodedImages = decodedImages.Count;
			// Assume we have one more pending image when the videoBuffer is not empty
			if (!videoBuffer.Empty)
			{
				numberDecodedImages++;
			}

			lock (mpegRingbuffer)
			{
				mpegRingbuffer.notifyRead(numberDecodedImages);
			}
		}

		protected internal virtual void mpegRingbufferRead()
		{
			if (mpegRingbuffer != null)
			{
				if (mpegRingbufferAddr != null && mpegRingbufferAddr.NotNull)
				{
					lock (mpegRingbuffer)
					{
						mpegRingbuffer.read(mpegRingbufferAddr);
					}
				}
				mpegRingbuffer.HasAudio = RegisteredAudioChannel;
				mpegRingbuffer.HasVideo = RegisteredVideoChannel;
				mpegRingbuffer.HasUserData = RegisteredUserDataChannel;
			}
		}

		private void rememberLastFrame(int imageWidth, int imageHeight, int[] abgr)
		{
			if (abgr != null)
			{
				releaseIntBuffer(lastFrameABGR);
				int length = imageWidth * imageHeight;
				lastFrameABGR = getIntBuffer(length);
				Array.Copy(abgr, 0, lastFrameABGR, 0, length);
				lastFrameWidth = imageWidth;
				lastFrameHeight = imageHeight;
			}
		}

		public virtual void writeLastFrameABGR(int buffer, int frameWidth, int pixelMode)
		{
			if (lastFrameABGR != null)
			{
				writeImageABGR(buffer, frameWidth, lastFrameWidth, lastFrameHeight, pixelMode, lastFrameABGR);
			}
		}

		internal SysMemInfo mpegAvcYuvMemInfo;

		private bool decodeImage(int buffer, int frameWidth, int pixelMode, TPointer32 gotFrameAddr, bool writeAbgr)
		{
			if (pixelMode < 0)
			{
				return false;
			}

			DecodedImageInfo decodedImageInfo;
			lock (decodedImages)
			{
				decodedImageInfo = decodedImages.pollFirst();
			}

			if (decodedImageInfo == null)
			{
				avcGotFrame = false;
				if (gotFrameAddr != null)
				{
					gotFrameAddr.setValue(avcGotFrame);
				}
				return false;
			}

			avcGotFrame = decodedImageInfo.gotFrame;
			if (gotFrameAddr != null)
			{
				if (log.TraceEnabled)
				{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.trace(String.format("decodeImage returning avcGotFrame(0x%08X)=%b", gotFrameAddr.getAddress(), avcGotFrame));
					log.trace(string.Format("decodeImage returning avcGotFrame(0x%08X)=%b", gotFrameAddr.Address, avcGotFrame));
				}
				gotFrameAddr.setValue(avcGotFrame);
			}

			if (decodedImageInfo.gotFrame)
			{
				if (buffer == 0)
				{
					int width = decodedImageInfo.imageWidth;
					int height = decodedImageInfo.imageHeight - 18;
					mpegAvcInfoStruct.setValue32(8, width);
					mpegAvcInfoStruct.setValue32(12, height);
					mpegAvcInfoStruct.setValue32(28, 1);
					mpegAvcInfoStruct.setValue32(32, decodedImageInfo.gotFrame);
					mpegAvcInfoStruct.setValue32(36, !decodedImageInfo.gotFrame);

					int size1 = ((width + 16) >> 5) * (height >> 1) * 16;
					int size2 = (width >> 5) * (height >> 1) * 16;
					if (mpegAvcYuvMemInfo == null)
					{
						int size = (size1 + size2) * 3 + 2 * 164;
						mpegAvcYuvMemInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.USER_PARTITION_ID, "mpegAvcYuv", SysMemUserForUser.PSP_SMEM_Low, size, 0);
					}

					int addr = mpegAvcYuvMemInfo.addr;

					mpegAvcYuvStruct.setValue32(0, addr);
					sceVideocodec.write(addr, size2, decodedImageInfo.luma, 0);
					addr += size1;

					mpegAvcYuvStruct.setValue32(4, addr);
					sceVideocodec.write(addr, size2, decodedImageInfo.luma, 1 * size2);
					addr += size2;

					mpegAvcYuvStruct.setValue32(8, addr);
					sceVideocodec.write(addr, size2, decodedImageInfo.luma, 2 * size2);
					addr += size1;

					mpegAvcYuvStruct.setValue32(12, addr);
					sceVideocodec.write(addr, size2, decodedImageInfo.luma, 3 * size2);
					addr += size2;

					mpegAvcYuvStruct.setValue32(16, addr);
					sceVideocodec.write(addr, size2 >> 1, decodedImageInfo.cr, 0);
					addr += size1 >> 1;

					mpegAvcYuvStruct.setValue32(20, addr);
					sceVideocodec.write(addr, size2 >> 1, decodedImageInfo.cr, size2 >> 1);
					addr += size2 >> 1;

					mpegAvcYuvStruct.setValue32(24, addr);
					sceVideocodec.write(addr, size2 >> 1, decodedImageInfo.cb, 0);
					addr += size1 >> 1;

					mpegAvcYuvStruct.setValue32(28, addr);
					sceVideocodec.write(addr, size2 >> 1, decodedImageInfo.cb, size2 >> 1);
					addr += size2 >> 1;
				}
				else
				{
					if (writeAbgr)
					{
						writeImageABGR(buffer, frameWidth, decodedImageInfo.imageWidth, decodedImageInfo.imageHeight, pixelMode, decodedImageInfo.abgr);
					}
					else
					{
						writeImageYCbCr(buffer, decodedImageInfo.imageWidth, decodedImageInfo.imageHeight, decodedImageInfo.luma, decodedImageInfo.cb, decodedImageInfo.cr);
					}
				}

				rememberLastFrame(decodedImageInfo.imageWidth, decodedImageInfo.imageHeight, decodedImageInfo.abgr);

				releaseIntBuffer(decodedImageInfo.luma);
				releaseIntBuffer(decodedImageInfo.cb);
				releaseIntBuffer(decodedImageInfo.cr);
				releaseIntBuffer(decodedImageInfo.abgr);
				decodedImageInfo.luma = null;
				decodedImageInfo.cb = null;
				decodedImageInfo.cr = null;
				decodedImageInfo.abgr = null;

				videoFrameCount++;
			}

			return true;
		}

		private void restartThread(int threadUid, int buffer, int frameWidth, int pixelMode, TPointer32 gotFrameAddr, bool writeAbgr, long threadWakeupMicroTime)
		{
			if (threadUid < 0)
			{
				return;
			}

			if (!decodeImage(buffer, frameWidth, pixelMode, gotFrameAddr, writeAbgr))
			{
				return;
			}

			videoDecoderThread.resetWaitingThreadInfo();

			IAction action;
			long delayMicros = threadWakeupMicroTime - Emulator.Clock.microTime();
			if (delayMicros > 0L)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Further delaying thread=0x{0:X} by {1:D} microseconds", threadUid, delayMicros));
				}
				action = new DelayThreadAction(threadUid, (int) delayMicros, false, true);
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Unblocking thread=0x{0:X}", threadUid));
				}
				action = new UnblockThreadAction(threadUid);
			}
			// The action cannot be executed immediately as we are running
			// in a non-PSP thread. The action has to be executed by the scheduler
			// as soon as possible.
			Emulator.Scheduler.addAction(action);
		}

		private bool DecoderInErrorCondition
		{
			get
			{
				lock (decodedImages)
				{
					if (decodedImages.Count == 0)
					{
						return false;
					}
    
					DecodedImageInfo decodedImageInfo = decodedImages.First.Value;
					if (decodedImageInfo.frameEnd >= 0 || decodedImageInfo.gotFrame)
					{
						return false;
					}
    
					return true;
				}
			}
		}

		private void removeErrorImages()
		{
			lock (decodedImages)
			{
				while (DecoderInErrorCondition)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Removing error image {0}", decodedImages.First.Value));
					}
					decodedImages.RemoveFirst();
				}
			}
		}

		private void decodeNextImage(TPointer auAddr)
		{
			PesHeader pesHeader = new PesHeader(RegisteredVideoChannel);
			pesHeader.DtsPts = UNKNOWN_TIMESTAMP;

			DecodedImageInfo decodedImageInfo = new DecodedImageInfo();
			decodedImageInfo.frameEnd = readNextVideoFrame(pesHeader, auAddr);

			if (decodedImageInfo.frameEnd >= 0)
			{
				if (videoBuffer.Length < decodedImageInfo.frameEnd)
				{
					// The content of the frame is not yet completely available in the videoBuffer
					return;
				}

				if (videoCodec == null)
				{
					videoCodec = CodecFactory.VideoCodec;
					videoCodec.init(videoCodecExtraData);
				}

				lock (videoBuffer)
				{
					int result = videoCodec.decode(videoBuffer.Buffer, videoBuffer.BufferOffset, decodedImageInfo.frameEnd);

					if (log.TraceEnabled)
					{
						sbyte[] bytes = new sbyte[decodedImageInfo.frameEnd];
						int[] inputBuffer = videoBuffer.Buffer;
						int inputOffset = videoBuffer.BufferOffset;
						for (int i = 0; i < decodedImageInfo.frameEnd; i++)
						{
							bytes[i] = (sbyte) inputBuffer[inputOffset + i];
						}
						log.trace(string.Format("decodeNextImage codec returned 0x{0:X}. Decoding 0x{1:X} bytes from {2}", result, decodedImageInfo.frameEnd, Utilities.getMemoryDump(bytes, 0, decodedImageInfo.frameEnd)));
					}

					if (result < 0)
					{
						log.error(string.Format("decodeNextImage codec returned 0x{0:X8}", result));
						// Skip this incorrect frame
						videoBuffer.notifyRead(decodedImageInfo.frameEnd);
						decodedImageInfo.gotFrame = false;
					}
					else
					{
						videoBuffer.notifyRead(result);

						decodedImageInfo.gotFrame = videoCodec.hasImage();
						if (decodedImageInfo.gotFrame)
						{
							decodedImageInfo.imageWidth = videoCodec.ImageWidth;
							decodedImageInfo.imageHeight = videoCodec.ImageHeight;
							if (!getImage(decodedImageInfo))
							{
								return;
							}
						}
					}
				}
			}
			else if (mpegRingbuffer != null && mpegRingbuffer.PacketSize == 0)
			{
				// Do not add a new decoded image when we are decoding in low level mode
				return;
			}

			if (videoPesHeader == null)
			{
				videoPesHeader = new PesHeader(pesHeader);
				pesHeader.DtsPts = UNKNOWN_TIMESTAMP;
			}

			decodedImageInfo.pesHeader = videoPesHeader;

			videoPesHeader = pesHeader;

			if (decodedImageInfo.gotFrame)
			{
				removeErrorImages();
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Adding decoded image {0}", decodedImageInfo));
			}
			lock (decodedImages)
			{
				decodedImages.AddLast(decodedImageInfo);
			}
		}

		private void hleVideoDecoderStep(int threadUid, int buffer, int frameWidth, int pixelMode, TPointer32 gotFrameAddr, bool writeAbgr, TPointer auAddr, long threadWakeupMicroTime)
		{
			if (log.DebugEnabled)
			{
				if (threadUid >= 0)
				{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("hleVideoDecoderStep threadUid=0x%X, buffer=0x%08X, frameWidth=%d, pixelMode=%d, gotFrameAddr=%s, writeAbgr=%b, %d decoded images", threadUid, buffer, frameWidth, pixelMode, gotFrameAddr, writeAbgr, decodedImages.size()));
					log.debug(string.Format("hleVideoDecoderStep threadUid=0x%X, buffer=0x%08X, frameWidth=%d, pixelMode=%d, gotFrameAddr=%s, writeAbgr=%b, %d decoded images", threadUid, buffer, frameWidth, pixelMode, gotFrameAddr, writeAbgr, decodedImages.Count));
				}
				else
				{
					log.debug(string.Format("hleVideoDecoderStep {0:D} decoded images", decodedImages.Count));
				}
			}

			if (buffer == 0)
			{
				decodeNextImage(auAddr);
			}

			restartThread(threadUid, buffer, frameWidth, pixelMode, gotFrameAddr, writeAbgr, threadWakeupMicroTime);

			// Always decode one frame in advance
			if (decodedImages.Count <= 1 || DecoderInErrorCondition)
			{
				decodeNextImage(auAddr);
			}
		}

		private int read32(Memory mem, pspFileBuffer buffer)
		{
			if (buffer.CurrentSize < 4)
			{
				return 0;
			}

			int addr = buffer.ReadAddr;
			int value;

			if (buffer.ReadSize >= 4)
			{
				value = endianSwap32(Utilities.readUnaligned32(mem, addr));
				buffer.notifyRead(4);
			}
			else
			{
				value = read8(mem, buffer);
				value = (value << 8) | read8(mem, buffer);
				value = (value << 8) | read8(mem, buffer);
				value = (value << 8) | read8(mem, buffer);
			}

			return value;
		}

		private int read16(Memory mem, pspFileBuffer buffer)
		{
			if (buffer.CurrentSize < 2)
			{
				return 0;
			}

			int addr = buffer.ReadAddr;
			int value;

			if (buffer.ReadSize >= 2)
			{
				value = endianSwap16(readUnaligned16(mem, addr));
				buffer.notifyRead(2);
			}
			else
			{
				value = (read8(mem, buffer) << 8) | read8(mem, buffer);
			}

			return value;
		}

		private int read8(Memory mem, pspFileBuffer buffer)
		{
			if (buffer.CurrentSize < 1)
			{
				return 0;
			}

			int addr = buffer.ReadAddr;
			int value = mem.read8(addr);
			buffer.notifyRead(1);

			return value;
		}

		private void skip(pspFileBuffer buffer, int n)
		{
			buffer.notifyRead(n);
		}

		private void addToAudioBuffer(Memory mem, pspFileBuffer buffer, int length)
		{
			while (length > 0)
			{
				int currentFrameLength = audioFrameLength == 0 ? 0 : audioBuffer.Length % audioFrameLength;
				if (currentFrameLength == 0)
				{
					// 8 bytes header:
					// - byte 0: 0x0F
					// - byte 1: 0xD0
					// - byte 2: 0x28
					// - byte 3: (frameLength - 8) / 8
					// - bytes 4-7: 0x00
					if (log.TraceEnabled)
					{
						log.trace(string.Format("Reading an audio frame from 0x{0:X8} (length=0x{1:X}) to the Audio buffer (already read {2:D})", buffer.ReadAddr, length, frameHeaderLength));
					}

					while (frameHeaderLength < frameHeader.Length && length > 0)
					{
						frameHeader[frameHeaderLength++] = read8(mem, buffer);
						length--;
					}
					if (frameHeaderLength < frameHeader.Length)
					{
						// Frame header not yet complete
						break;
					}
					if (length == 0)
					{
						// Frame header is complete but no data is following the header.
						// Retry when some data is available
						break;
					}

					if (frameHeader[0] != 0x0F || frameHeader[1] != 0xD0)
					{
						if (log.InfoEnabled)
						{
							log.warn(string.Format("Audio frame length 0x{0:X} with incorrect header (header: {1:X2} {2:X2} {3:X2} {4:X2} {5:X2} {6:X2} {7:X2} {8:X2})", audioFrameLength, frameHeader[0], frameHeader[1], frameHeader[2], frameHeader[3], frameHeader[4], frameHeader[5], frameHeader[6], frameHeader[7]));
						}
					}
					else
					{
						// Use values from the frame header only if it is valid
						int frameHeader23 = (frameHeader[2] << 8) | frameHeader[3];
						audioFrameLength = ((frameHeader23 & 0x3FF) << 3) + frameHeader.Length;

						if (log.TraceEnabled)
						{
							log.trace(string.Format("Audio frame length 0x{0:X} (header: {1:X2} {2:X2} {3:X2} {4:X2} {5:X2} {6:X2} {7:X2} {8:X2})", audioFrameLength, frameHeader[0], frameHeader[1], frameHeader[2], frameHeader[3], frameHeader[4], frameHeader[5], frameHeader[6], frameHeader[7]));
						}
					}

					frameHeaderLength = 0;
				}
				int lengthToNextFrame = audioFrameLength - currentFrameLength;
				int readLength = Utilities.min(length, buffer.ReadSize, lengthToNextFrame);
				int addr = buffer.ReadAddr;
				if (audioBuffer.write(mem, addr, readLength) != readLength)
				{
					log.error(string.Format("AudioBuffer too small"));
				}
				buffer.notifyRead(readLength);
				length -= readLength;
			}
		}

		private void addToVideoBuffer(Memory mem, pspFileBuffer buffer, int length)
		{
			while (length > 0)
			{
				int readLength = System.Math.Min(length, buffer.ReadSize);
				int addr = buffer.ReadAddr;
				addToVideoBuffer(mem, addr, readLength);
				buffer.notifyRead(readLength);
				length -= readLength;
			}
		}

		public virtual void addToVideoBuffer(Memory mem, int addr, int length)
		{
			if (videoBuffer != null)
			{
				videoBuffer.write(mem, addr, length);
			}
		}

		private void addToUserDataBuffer(Memory mem, pspFileBuffer buffer, int length)
		{
			while (length > 0)
			{
				int readLength = System.Math.Min(length, buffer.ReadSize);
				int addr = buffer.ReadAddr;
				userDataBuffer.write(mem, addr, readLength);
				buffer.notifyRead(readLength);
				length -= readLength;
			}
		}

		private long readPts(Memory mem, pspFileBuffer buffer)
		{
			return readPts(mem, buffer, read8(mem, buffer));
		}

		private long readPts(Memory mem, pspFileBuffer buffer, int c)
		{
			return (((long)(c & 0x0E)) << 29) | ((read16(mem, buffer) >> 1) << 15) | (read16(mem, buffer) >> 1);
		}

		private int readPesHeader(Memory mem, pspFileBuffer buffer, PesHeader pesHeader, int length, int startCode)
		{
			int c = 0;
			while (length > 0)
			{
				c = read8(mem, buffer);
				length--;
				if (c != 0xFF)
				{
					break;
				}
			}

			if ((c & 0xC0) == 0x40)
			{
				skip(buffer, 1);
				c = read8(mem, buffer);
				length -= 2;
			}
			pesHeader.DtsPts = UNKNOWN_TIMESTAMP;
			if ((c & 0xE0) == 0x20)
			{
				pesHeader.DtsPts = readPts(mem, buffer, c);
				length -= 4;
				if ((c & 0x10) != 0)
				{
					pesHeader.Dts = readPts(mem, buffer);
					length -= 5;
				}
			}
			else if ((c & 0xC0) == 0x80)
			{
				int flags = read8(mem, buffer);
				int headerLength = read8(mem, buffer);
				length -= 2;
				length -= headerLength;
				if ((flags & 0x80) != 0)
				{
					pesHeader.DtsPts = readPts(mem, buffer);
					headerLength -= 5;
					if ((flags & 0x40) != 0)
					{
						pesHeader.Dts = readPts(mem, buffer);
						headerLength -= 5;
					}
				}
				if ((flags & 0x3F) != 0 && headerLength == 0)
				{
					flags &= 0xC0;
				}
				if ((flags & 0x01) != 0)
				{
					int pesExt = read8(mem, buffer);
					headerLength--;
					int skip = (pesExt >> 4) & 0x0B;
					skip += skip & 0x09;
					if ((pesExt & 0x40) != 0 || skip > headerLength)
					{
						pesExt = skip = 0;
					}
					this.skip(buffer, skip);
					headerLength -= skip;
					if ((pesExt & 0x01) != 0)
					{
						int ext2Length = read8(mem, buffer);
						headerLength--;
						 if ((ext2Length & 0x7F) != 0)
						 {
							 int idExt = read8(mem, buffer);
							 headerLength--;
							 if ((idExt & 0x80) == 0)
							 {
								 startCode = ((startCode & 0xFF) << 8) | idExt;
							 }
						 }
					}
				}
				skip(buffer, headerLength);
			}

			if (startCode == PRIVATE_STREAM_1)
			{
				int channel = read8(mem, buffer);
				pesHeader.Channel = channel;
				length--;
				if (channel >= 0x80 && channel <= 0xCF)
				{
					// Skip audio header
					skip(buffer, 3);
					length -= 3;
					if (channel >= 0xB0 && channel <= 0xBF)
					{
						skip(buffer, 1);
						length--;
					}
				}
				else if (channel >= 0x20)
				{
					// Userdata
					skip(buffer, 1);
					length--;
				}
				else
				{
					// PSP audio has additional 3 bytes in header
					skip(buffer, 3);
					length -= 3;
				}
			}

			return length;
		}

		private void readNextAudioFrame(PesHeader pesHeader)
		{
			if (mpegRingbuffer == null)
			{
				return;
			}

			Memory mem = Memory.Instance;
			pspFileBuffer buffer = mpegRingbuffer.AudioBuffer;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("readNextAudioFrame {0}", mpegRingbuffer));
			}
			int audioChannel = RegisteredAudioChannel;

			bool endOfAudio = false;
			while (!endOfAudio && !buffer.Empty && (audioFrameLength == 0 || audioBuffer.Length < audioFrameLength))
			{
				int startCode = read32(mem, buffer);
				int codeLength;
				switch (startCode)
				{
					case PACK_START_CODE:
						skip(buffer, 10);
						break;
					case SYSTEM_HEADER_START_CODE:
						skip(buffer, 14);
						break;
					case PADDING_STREAM:
					case PRIVATE_STREAM_2:
					case 0x1E0:
				case 0x1E1:
			case 0x1E2:
		case 0x1E3: // Video streams
					case 0x1E4:
				case 0x1E5:
			case 0x1E6:
		case 0x1E7:
					case 0x1E8:
				case 0x1E9:
			case 0x1EA:
		case 0x1EB:
					case 0x1EC:
				case 0x1ED:
			case 0x1EE:
		case 0x1EF:
						codeLength = read16(mem, buffer);
						skip(buffer, codeLength);
						break;
					case PRIVATE_STREAM_1:
						// Audio stream
						codeLength = read16(mem, buffer);
						codeLength = readPesHeader(mem, buffer, pesHeader, codeLength, startCode);
						if (pesHeader.Channel == audioChannel || audioChannel < 0)
						{
							addToAudioBuffer(mem, buffer, codeLength);
						}
						else
						{
							skip(buffer, codeLength);
						}
						break;
					case PSMF_MAGIC_LITTLE_ENDIAN:
						// Skip any PSMF header
						skip(buffer, PSMF_STREAM_OFFSET_OFFSET - 4);
						int streamOffset = read32(mem, buffer);
						skip(buffer, streamOffset - PSMF_STREAM_OFFSET_OFFSET - 4);
						break;
					default:
						endOfAudio = true;
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Unknown StartCode 0x{0:X8} at 0x{1:X8}", startCode, buffer.ReadAddr - 4));
						}
						break;
				}
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("After readNextAudioFrame {0}", mpegRingbuffer));
			}
		}

		private bool reachedEndOfVideo()
		{
			if (psmfHeader == null)
			{
				return true;
			}

			int pendingVideoFrame = decodedImages.Count + (videoBuffer.Length > 0 ? 1 : 0);
			if (currentVideoTimestamp + pendingVideoFrame * videoTimestampStep >= psmfHeader.mpegLastTimestamp)
			{
				return true;
			}

			return false;
		}

		private int readNextVideoFrame(PesHeader pesHeader, TPointer auAddr)
		{
			Memory mem = Memory.Instance;
			if (mpegRingbuffer == null || mpegRingbuffer.PacketSize == 0)
			{
				if (mpegAvcAu.esSize <= 0)
				{
					return -1;
				}
				int esSize = mpegAvcAu.esSize;
				addToVideoBuffer(mem, mpegAvcAu.esBuffer, esSize);
				mpegAvcAu.esSize = 0;
				if (auAddr.NotNull)
				{
					mpegAvcAu.write(auAddr);
				}

				return esSize;
			}

			pspFileBuffer buffer = mpegRingbuffer.VideoBuffer;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("readNextVideoFrame {0}", mpegRingbuffer));
			}
			int videoChannel = RegisteredVideoChannel;

			int frameEnd = videoBuffer.findFrameEnd();
			bool endOfVideo = false;
			while (!endOfVideo && !buffer.Empty && frameEnd < 0)
			{
				int startCode = read32(mem, buffer);
				int codeLength;
				switch (startCode)
				{
					case PACK_START_CODE:
						skip(buffer, 10);
						break;
					case SYSTEM_HEADER_START_CODE:
						skip(buffer, 14);
						break;
					case 0x1E0:
				case 0x1E1:
			case 0x1E2:
		case 0x1E3: // Video streams
					case 0x1E4:
				case 0x1E5:
			case 0x1E6:
		case 0x1E7:
					case 0x1E8:
				case 0x1E9:
			case 0x1EA:
		case 0x1EB:
					case 0x1EC:
				case 0x1ED:
			case 0x1EE:
		case 0x1EF:
						codeLength = read16(mem, buffer);
						codeLength = readPesHeader(mem, buffer, pesHeader, codeLength, startCode);
						if (videoChannel < 0 || videoChannel == startCode - 0x1E0)
						{
							addToVideoBuffer(mem, buffer, codeLength);
							frameEnd = videoBuffer.findFrameEnd();
							// Ignore next PES headers for this current video frame
							pesHeader = dummyPesHeader;
						}
						else
						{
							skip(buffer, codeLength);
						}
						break;
					case PADDING_STREAM:
					case PRIVATE_STREAM_2:
					case PRIVATE_STREAM_1: // Audio stream
						codeLength = read16(mem, buffer);
						skip(buffer, codeLength);
						break;
					case PSMF_MAGIC_LITTLE_ENDIAN:
						// Skip any PSMF header, only at the start of the stream
						if (videoFrameCount == 0)
						{
							skip(buffer, PSMF_STREAM_OFFSET_OFFSET - 4);
							int streamOffset = read32(mem, buffer);
							skip(buffer, streamOffset - PSMF_STREAM_OFFSET_OFFSET - 4);
						}
						else
						{
							if (mpegRingbuffer != null)
							{
								mpegRingbuffer.consumeAllPackets();
							}
							endOfVideo = true;
							if (log.DebugEnabled)
							{
								log.debug(string.Format("Unknown StartCode 0x{0:X8} at 0x{1:X8}", startCode, buffer.ReadAddr - 4));
							}
						}
						break;
					default:
						endOfVideo = true;
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Unknown StartCode 0x{0:X8} at 0x{1:X8}", startCode, buffer.ReadAddr - 4));
						}
						break;
				}
			}

			// Reaching the last frame?
			if (frameEnd < 0 && (buffer.Empty || endOfVideo) && !videoBuffer.Empty)
			{
				if (endOfVideo || reachedEndOfVideo())
				{
					// There is no next frame any more but the video buffer is not yet empty,
					// so use the rest of the video buffer
					frameEnd = videoBuffer.Length;
				}
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("After readNextVideoFrame frameEnd=0x{0:X}, {1}", frameEnd, mpegRingbuffer));
			}

			return frameEnd;
		}

		private void readNextUserDataFrame(PesHeader pesHeader)
		{
			if (mpegRingbuffer == null)
			{
				return;
			}

			Memory mem = Memory.Instance;
			pspFileBuffer buffer = mpegRingbuffer.UserDataBuffer;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("readNextUserDataFrame {0}", mpegRingbuffer));
			}
			int userDataChannel = 0x20 + RegisteredUserDataChannel;

			while (!buffer.Empty && (userDataLength == 0 || userDataBuffer.Length < userDataLength))
			{
				int startCode = read32(mem, buffer);
				int codeLength;
				switch (startCode)
				{
					case PACK_START_CODE:
						skip(buffer, 10);
						break;
					case SYSTEM_HEADER_START_CODE:
						skip(buffer, 14);
						break;
					case PADDING_STREAM:
					case PRIVATE_STREAM_2:
					case 0x1E0:
				case 0x1E1:
			case 0x1E2:
		case 0x1E3: // Video streams
					case 0x1E4:
				case 0x1E5:
			case 0x1E6:
		case 0x1E7:
					case 0x1E8:
				case 0x1E9:
			case 0x1EA:
		case 0x1EB:
					case 0x1EC:
				case 0x1ED:
			case 0x1EE:
		case 0x1EF:
						codeLength = read16(mem, buffer);
						skip(buffer, codeLength);
						break;
					case PRIVATE_STREAM_1:
						// Audio/Userdata stream
						if (userDataLength > 0)
						{
							// Keep only the PES header of the first data chunk
							pesHeader = dummyPesHeader;
						}
						codeLength = read16(mem, buffer);
						codeLength = readPesHeader(mem, buffer, pesHeader, codeLength, startCode);
						if (pesHeader.Channel == userDataChannel)
						{
							if (userDataLength == 0)
							{
								for (int i = 0; i < userDataHeader.Length; i++)
								{
									userDataHeader[i] = read8(mem, buffer);
									codeLength--;
								}
								userDataLength = ((userDataHeader[0] << 24) | (userDataHeader[1] << 16) | (userDataHeader[2] << 8) | (userDataHeader[3] << 0)) - 4;
							}
							addToUserDataBuffer(mem, buffer, codeLength);
						}
						else
						{
							skip(buffer, codeLength);
						}
						break;
					case PSMF_MAGIC_LITTLE_ENDIAN:
						// Skip any PSMF header
						skip(buffer, PSMF_STREAM_OFFSET_OFFSET - 4);
						int streamOffset = read32(mem, buffer);
						skip(buffer, streamOffset - PSMF_STREAM_OFFSET_OFFSET - 4);
						break;
					default:
						log.warn(string.Format("Unknown StartCode 0x{0:X8} at 0x{1:X8}", startCode, buffer.ReadAddr - 4));
						break;
				}
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("After readNextUserDataFrame {0}", mpegRingbuffer));
			}
		}

		public virtual int VideoFrameHeight
		{
			set
			{
				this.videoFrameHeight = value;
			}
		}

		private int getFrameHeight(int imageHeight)
		{
			int frameHeight = imageHeight;
			if (psmfHeader != null && psmfHeader.Valid)
			{
				// The decoded image height can be 290 while the header
				// gives an height of 272.
				frameHeight = System.Math.Min(frameHeight, psmfHeader.VideoHeight);
			}
			else if (videoFrameHeight >= 0)
			{
				// The decoded image height can be 290 while the MP4 header
				// gives an height of 272.
				frameHeight = System.Math.Min(frameHeight, videoFrameHeight);
			}
			else if (imageHeight == 290)
			{
				// No valid PSMF header is available, but a decoded image height
				// of 290 usually means an height of 272.
				frameHeight = Screen.height;
			}

			return frameHeight;
		}

		private void writeImageABGR(int addr, int frameWidth, int imageWidth, int imageHeight, int pixelMode, int[] abgr)
		{
			int frameHeight = getFrameHeight(imageHeight);
			int bytesPerPixel = sceDisplay.getPixelFormatBytes(pixelMode);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("writeImageABGR addr=0x{0:X8}-0x{1:X8}, frameWidth={2:D}, frameHeight={3:D}, width={4:D}, height={5:D}, pixelMode={6:D}", addr, addr + frameWidth * frameHeight * bytesPerPixel, frameWidth, frameHeight, imageWidth, imageHeight, pixelMode));
			}

			int lineWidth = System.Math.Min(imageWidth, frameWidth);

			if (pixelMode == TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888 && hasMemoryInt())
			{
				// Optimize the most common case
				int offset = 0;
				int memoryIntOffset = addr >> 2;
				for (int y = 0; y < frameHeight; y++)
				{
					Array.Copy(abgr, offset, MemoryInt, memoryIntOffset, lineWidth);
					memoryIntOffset += frameWidth;
					offset += imageWidth;
				}
			}
			else
			{
				// The general case with color format transformation
				int lineSkip = frameWidth - lineWidth;
				IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(addr, frameWidth * frameHeight * bytesPerPixel, bytesPerPixel);
				for (int y = 0; y < frameHeight; y++)
				{
					int offset = y * imageWidth;
					for (int x = 0; x < lineWidth; x++, offset++)
					{
						int pixelColor = Debug.getPixelColor(abgr[offset], pixelMode);
						memoryWriter.writeNext(pixelColor);
					}
					memoryWriter.skip(lineSkip);
				}
				memoryWriter.flush();
			}
		}

		private void writeImageYCbCr(int addr, int imageWidth, int imageHeight, int[] luma, int[] cb, int[] cr)
		{
			int frameWidth = imageWidth;
			int frameHeight = imageHeight;
			if (psmfHeader != null)
			{
				// The decoded image height can be 290 while the header
				// gives an height of 272.
				frameHeight = System.Math.Min(frameHeight, psmfHeader.VideoHeight);
			}
			else
			{
				// No PSMF header is available assume the video is not higher than the PSP screen height
				frameHeight = System.Math.Min(frameHeight, Screen.height);
			}

			int width2 = frameWidth >> 1;
			int height2 = frameHeight >> 1;
			int length = frameWidth * frameHeight;
			int length2 = width2 * height2;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("writeImageYCbCr addr=0x{0:X8}-0x{1:X8}, frameWidth={2:D}, frameHeight={3:D}", addr, addr + length + length2 + length2, frameWidth, frameHeight));
			}

			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(addr, length + length2 + length2, 1);
			for (int i = 0; i < length; i++)
			{
				memoryWriter.writeNext(luma[i] & 0xFF);
			}
			for (int i = 0; i < length2; i++)
			{
				memoryWriter.writeNext(cb[i] & 0xFF);
			}
			for (int i = 0; i < length2; i++)
			{
				memoryWriter.writeNext(cr[i] & 0xFF);
			}
			memoryWriter.flush();
		}

		public static int[] getIntBuffer(int length)
		{
			lock (intBuffers)
			{
				foreach (int[] intBuffer in intBuffers)
				{
					if (intBuffer.Length >= length)
					{
						intBuffers.remove(intBuffer);
						return intBuffer;
					}
				}
			}

			return new int[length];
		}

		public static void releaseIntBuffer(int[] intBuffer)
		{
			if (intBuffer == null)
			{
				return;
			}

			lock (intBuffers)
			{
				intBuffers.Add(intBuffer);

				if (intBuffers.Count > MAX_INT_BUFFERS_SIZE)
				{
					// Remove the smallest int buffer
					int[] smallestIntBuffer = null;
					foreach (int[] buffer in intBuffers)
					{
						if (smallestIntBuffer == null || buffer.Length < smallestIntBuffer.Length)
						{
							smallestIntBuffer = buffer;
						}
					}

					intBuffers.remove(smallestIntBuffer);
				}
			}
		}

		private bool getImage(DecodedImageInfo decodedImageInfo)
		{
			int width = videoCodec.ImageWidth;
			int height = videoCodec.ImageHeight;
			int width2 = width >> 1;
			int height2 = height >> 1;
			int length = width * height;
			int length2 = width2 * height2;

			// Allocate buffers
			decodedImageInfo.luma = getIntBuffer(length);
			decodedImageInfo.cb = getIntBuffer(length2);
			decodedImageInfo.cr = getIntBuffer(length2);
			int result = videoCodec.getImage(decodedImageInfo.luma, decodedImageInfo.cb, decodedImageInfo.cr);
			if (result < 0)
			{
				log.error(string.Format("VideoCodec error 0x{0:X8} while retrieving the image", result));
				return false;
			}

			decodedImageInfo.abgr = getIntBuffer(length);
			H264Utils.YUV2ABGR(width, height, decodedImageInfo.luma, decodedImageInfo.cb, decodedImageInfo.cr, decodedImageInfo.abgr);

			return true;
		}

		private void resetMpegRingbuffer()
		{
			if (mpegRingbuffer != null)
			{
				mpegRingbuffer.reset();
			}

			if (videoBuffer != null)
			{
				videoBuffer.reset();
			}
			if (audioBuffer != null)
			{
				audioBuffer.reset();
			}

			userDataBuffer = null;
			audioPesHeader = null;
			videoPesHeader = null;
			userDataPesHeader = null;
			audioFrameLength = 0;
			frameHeaderLength = 0;
			userDataLength = 0;

			if (decodedImages != null)
			{
				lock (decodedImages)
				{
					decodedImages.Clear();
				}
			}
		}

		protected internal virtual void startVideoDecoderThread()
		{
			if (videoDecoderThread == null)
			{
				videoDecoderThread = new VideoDecoderThread(this);
				videoDecoderThread.Daemon = true;
				videoDecoderThread.Name = "Video Decoder Thread";
				videoDecoderThread.Start();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public int hleMpegCreate(pspsharp.HLE.TPointer mpeg, pspsharp.HLE.TPointer data, int size, @CanBeNull pspsharp.HLE.TPointer ringbufferAddr, int frameWidth, int mode, int ddrtop)
		public virtual int hleMpegCreate(TPointer mpeg, TPointer data, int size, TPointer ringbufferAddr, int frameWidth, int mode, int ddrtop)
		{
			Memory mem = data.Memory;

			// Check size.
			if (size < MPEG_MEMSIZE)
			{
				log.warn("sceMpegCreate bad size " + size);
				return SceKernelErrors.ERROR_MPEG_NO_MEMORY;
			}

			finishStreams();

			// Update the ring buffer struct.
			if (ringbufferAddr != null && ringbufferAddr.NotNull)
			{
				mpegRingbuffer = SceMpegRingbuffer.fromMem(ringbufferAddr);
				resetMpegRingbuffer();
				mpegRingbuffer.Mpeg = mpeg.Address;
				mpegRingbufferWrite();
			}

			// Write mpeg system handle.
			mpegHandle = data.Address + 0x30;
			mpeg.setValue32(mpegHandle);

			// Initialize fake mpeg struct.
			Utilities.writeStringZ(mem, mpegHandle, "LIBMPEG.001");
			mem.write32(mpegHandle + 12, -1);
			if (ringbufferAddr != null)
			{
				mem.write32(mpegHandle + 16, ringbufferAddr.Address);
			}
			if (mpegRingbuffer != null)
			{
				mem.write32(mpegHandle + 20, mpegRingbuffer.UpperDataAddr);
			}

			// Initialize mpeg values.
			mpegRingbufferAddr = ringbufferAddr;
			videoFrameCount = 0;
			audioFrameCount = 0;
			currentVideoTimestamp = 0;
			currentAudioTimestamp = 0;
			videoPixelMode = TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
			defaultFrameWidth = frameWidth;

			audioBuffer = new AudioBuffer(data.Address + AUDIO_BUFFER_OFFSET, AUDIO_BUFFER_SIZE);
			videoBuffer = new VideoBuffer();

			decodedImages = new LinkedList<sceMpeg.DecodedImageInfo>();

			startVideoDecoderThread();
			   videoDecoderThread.resetWaitingThreadInfo();

			// Initialize the memory structure used by sceMpegAvcDecodeDetail2()
			mpegAvcInfoStruct = new TPointer(mem, mpegHandle + 0x200); // We need a structure of 40 bytes
			mpegAvcInfoStruct.clear(40);
			mpegAvcYuvStruct = new TPointer(mem, mpegHandle + 0x300); // We need a structure of 44 bytes
			mpegAvcYuvStruct.clear(44);

			// This is the structure passed to sceVideocodecDecode
			mpegAvcDetail2Struct = new TPointer(mem, mpegHandle + 0x400); // We need a structure of 96 bytes
			mpegAvcDetail2Struct.clear(96);
			mpegAvcDetail2Struct.setValue32(16, mpegAvcInfoStruct.Address);
			mpegAvcDetail2Struct.setValue32(44, mpegAvcYuvStruct.Address);
			mpegAvcDetail2Struct.setValue32(48, 0); // Unknown value

			return 0;
		}

		public virtual void hleMpegNotifyVideoDecoderThread()
		{
			if (videoDecoderThread != null)
			{
				videoDecoderThread.trigger();
			}
		}

		protected internal virtual void hleMpegRingbufferPostPut(AfterRingbufferPutCallback afterRingbufferPutCallback, int packetsAdded)
		{
			int putDataAddr = afterRingbufferPutCallback.PutDataAddr;
			int remainingPackets = afterRingbufferPutCallback.RemainingPackets;
			mpegRingbufferRead();

			if (packetsAdded > 0)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("hleMpegRingbufferPostPut:{0}", Utilities.getMemoryDump(putDataAddr, packetsAdded * mpegRingbuffer.PacketSize)));
				}

				if (packetsAdded > mpegRingbuffer.FreePackets)
				{
					log.warn(string.Format("sceMpegRingbufferPut clamping packetsAdded old={0:D}, new={1:D}", packetsAdded, mpegRingbuffer.FreePackets));
					packetsAdded = mpegRingbuffer.FreePackets;
				}
				mpegRingbuffer.addPackets(packetsAdded);
				mpegRingbufferWrite();

				afterRingbufferPutCallback.addPacketsAdded(packetsAdded);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMpegRingbufferPut packetsAdded=0x{0:X}, packetsRead=0x{1:X}, new availableSize=0x{2:X}", packetsAdded, mpegRingbuffer.ReadPackets, mpegRingbuffer.FreePackets));
				}

				int dataSizeInRingbuffer = mpegRingbuffer.PacketsInRingbuffer * mpegRingbuffer.PacketSize;
				if (psmfHeader != null && dataSizeInRingbuffer > psmfHeader.mpegStreamSize)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceMpegRingbufferPut returning ERROR_MPEG_INVALID_VALUE, size of data in ringbuffer=0x{0:X}, mpegStreamSize=0x{1:X}", dataSizeInRingbuffer, psmfHeader.mpegStreamSize));
					}
					afterRingbufferPutCallback.ErrorCode = SceKernelErrors.ERROR_MPEG_INVALID_VALUE;
					// No further callbacks
					remainingPackets = 0;
				}

				removeErrorImages();
				hleMpegNotifyVideoDecoderThread();

				if (remainingPackets > 0)
				{
					int putNumberPackets = System.Math.Min(remainingPackets, mpegRingbuffer.PutSequentialPackets);
					putDataAddr = mpegRingbuffer.PutDataAddr;
					afterRingbufferPutCallback.PutDataAddr = putDataAddr;
					afterRingbufferPutCallback.RemainingPackets = remainingPackets - putNumberPackets;

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceMpegRingbufferPut executing callback 0x{0:X8} to read 0x{1:X} packets at 0x{2:X8}", mpegRingbuffer.CallbackAddr, putNumberPackets, putDataAddr));
					}
					Modules.ThreadManForUserModule.executeCallback(null, mpegRingbuffer.CallbackAddr, afterRingbufferPutCallback, false, putDataAddr, putNumberPackets, mpegRingbuffer.CallbackArgs);
				}
			}
			else
			{
				afterRingbufferPutCallback.ErrorCode = packetsAdded;
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMpegRingbufferPut callback returning packetsAdded=0x{0:X}", packetsAdded));
				}
			}
		}

		public virtual void hleCreateRingbuffer()
		{
			mpegRingbuffer = new SceMpegRingbuffer(0, 0, 0, 0, 0);
			mpegRingbuffer.ReadPackets = 1;
			mpegRingbufferAddr = null;
		}

		public virtual void hleMpegNotifyRingbufferRead()
		{
			if (mpegRingbuffer != null && !mpegRingbuffer.hasReadPackets())
			{
				mpegRingbuffer.ReadPackets = 1;
				mpegRingbufferWrite();
			}
		}

		public virtual int[] VideoCodecExtraData
		{
			set
			{
				this.videoCodecExtraData = value;
			}
		}

		public virtual bool hasVideoCodecExtraData()
		{
			return videoCodecExtraData != null;
		}

		public virtual int[] VideoFrameSizes
		{
			set
			{
				videoBuffer.FrameSizes = value;
			}
		}

		public virtual void flushVideoFrameData()
		{
			videoBuffer.reset();
		}

		public virtual int VideoFrame
		{
			set
			{
				videoBuffer.Frame = value;
			}
		}

		public virtual void hleCreateRingbuffer(int packets, int data, int size)
		{
			mpegRingbuffer = new SceMpegRingbuffer(packets, data, size, 0, 0);
			mpegRingbufferAddr = null;
		}

		public virtual SceMpegRingbuffer MpegRingbuffer
		{
			get
			{
				return mpegRingbuffer;
			}
		}

		public virtual PSMFHeader PsmfHeader
		{
			get
			{
				return psmfHeader;
			}
		}

		private int getRegisteredChannel(int streamType, int registeredChannel)
		{
			int channel = -1;
			foreach (StreamInfo stream in streamMap.Values)
			{
				if (stream != null && stream.isStreamType(streamType) && stream.AuMode == MPEG_AU_MODE_DECODE)
				{
					if (channel < 0 || stream.Channel < channel)
					{
						channel = stream.Channel;
						if (channel == registeredChannel)
						{
							// We have found the registered channel
							break;
						}
					}
				}
			}

			if (channel < 0)
			{
				channel = registeredChannel;
			}

			return channel;
		}

		public virtual int RegisteredAudioChannel
		{
			get
			{
				return getRegisteredChannel(PSMF_ATRAC_STREAM, registeredAudioChannel);
			}
			set
			{
				this.registeredAudioChannel = value;
			}
		}

		public virtual bool RegisteredAudioChannel
		{
			get
			{
				return RegisteredAudioChannel >= 0;
			}
		}

		public virtual int RegisteredVideoChannel
		{
			get
			{
				return getRegisteredChannel(PSMF_AVC_STREAM, registeredVideoChannel);
			}
			set
			{
				if (this.registeredVideoChannel != value)
				{
					this.registeredVideoChannel = value;
				}
			}
		}

		public virtual bool RegisteredVideoChannel
		{
			get
			{
				return RegisteredVideoChannel >= 0;
			}
		}

		public virtual bool RegisteredUserDataChannel
		{
			get
			{
				return RegisteredUserDataChannel >= 0;
			}
		}

		public virtual int RegisteredPcmChannel
		{
			get
			{
				return getRegisteredChannel(PSMF_PCM_STREAM, -1);
			}
		}

		public virtual int RegisteredUserDataChannel
		{
			get
			{
				return getRegisteredChannel(PSMF_DATA_STREAM, -1);
			}
		}



		public virtual long CurrentVideoTimestamp
		{
			get
			{
				return currentVideoTimestamp;
			}
		}

		public virtual long CurrentAudioTimestamp
		{
			get
			{
				return currentAudioTimestamp;
			}
		}

		public virtual int hleMpegGetAvcAu(TPointer auAddr)
		{
			int result = 0;

			// Read Au of next Avc frame
			if (RegisteredVideoChannel)
			{
				startVideoDecoderThread();
				DecodedImageInfo decodedImageInfo;
				while (true)
				{
					lock (decodedImages)
					{
						decodedImageInfo = decodedImages.First.Value;
					}
					if (decodedImageInfo != null)
					{
						break;
					}
					// Wait for the video decoder thread
					if (log.DebugEnabled)
					{
						log.debug(string.Format("hleMpegGetAvcAu waiting for the video decoder thread..."));
					}
					Utilities.sleep(1);
				}

				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleMpegGetAvcAu decodedImageInfo: {0}", decodedImageInfo));
				}
				mpegAvcAu.pts = decodedImageInfo.pesHeader.Pts;
				mpegAvcAu.dts = decodedImageInfo.pesHeader.Dts;
				mpegAvcAu.esSize = System.Math.Max(0, decodedImageInfo.frameEnd);
				if (auAddr != null && auAddr.NotNull)
				{
					mpegAvcAu.write(auAddr);
				}

				// Packets from the ringbuffer are consumed during sceMpegGetXXXAu(),
				// they are not consumed during sceMpegXXXDecode().
				mpegRingbufferNotifyRead();

				mpegRingbufferWrite();

				if (decodedImageInfo.frameEnd < 0)
				{
					// Return an error only past the last video timestamp
					if (psmfHeader == null || currentVideoTimestamp > psmfHeader.mpegLastTimestamp || !RegisteredAudioChannel)
					{
						if (log.DebugEnabled)
						{
							if (psmfHeader == null)
							{
								log.debug(string.Format("hleMpegGetAvcAu with psmfHeader==null, returning ERROR_MPEG_NO_DATA"));
							}
							else
							{
								log.debug(string.Format("hleMpegGetAvcAu with currentVideoTimestamp={0:D} and psmfHeader.mpegLastTimestamp={1:D}, returning ERROR_MPEG_NO_DATA", currentVideoTimestamp, psmfHeader.mpegLastTimestamp));
							}
						}
						result = SceKernelErrors.ERROR_MPEG_NO_DATA;
					}
				}
			}

			if (result == 0)
			{
				if (mpegAvcAu.pts != UNKNOWN_TIMESTAMP)
				{
					currentVideoTimestamp = mpegAvcAu.pts;
				}
				else
				{
					currentVideoTimestamp += videoTimestampStep;
				}
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleMpegGetAvcAu returning 0x{0:X8}, AvcAu={1}", result, mpegAvcAu));
			}

			if (result != 0)
			{
				delayThread(mpegDecodeErrorDelay);
			}

			startedMpeg = true;

			return result;
		}

		public virtual int hleMpegGetAtracAu(TPointer auAddr)
		{
			int result = 0;
			if (RegisteredAudioChannel)
			{
				mpegAtracAu.esSize = audioFrameLength == 0 ? 0 : audioFrameLength + 8;

				mpegRingbufferNotifyRead();

				if (audioFrameLength == 0 || audioBuffer == null || audioBuffer.Length < audioFrameLength)
				{
					bool needUpdateAu;
					if (audioPesHeader == null)
					{
						audioPesHeader = new PesHeader(RegisteredAudioChannel);
						needUpdateAu = true;
					}
					else
					{
						// Take the PTS from the previous PES header.
						mpegAtracAu.pts = audioPesHeader.Pts;
						// On PSP, the audio DTS is always set to -1
						mpegAtracAu.dts = UNKNOWN_TIMESTAMP;
						if (auAddr != null && auAddr.NotNull)
						{
							mpegAtracAu.write(auAddr);
						}
						needUpdateAu = false;
					}

					audioPesHeader.DtsPts = UNKNOWN_TIMESTAMP;
					readNextAudioFrame(audioPesHeader);

					if (needUpdateAu)
					{
						mpegAtracAu.esSize = audioFrameLength == 0 ? 0 : audioFrameLength + 8;
						// Take the PTS from the first PES header and reset it.
						mpegAtracAu.pts = audioPesHeader.Pts;
						audioPesHeader.DtsPts = UNKNOWN_TIMESTAMP;
						// On PSP, the audio DTS is always set to -1
						mpegAtracAu.dts = UNKNOWN_TIMESTAMP;
						if (auAddr != null && auAddr.NotNull)
						{
							mpegAtracAu.write(auAddr);
						}
					}

					if (audioBuffer.Length < audioFrameLength)
					{
						// It seems that sceMpegGetAtracAu returns ERROR_MPEG_NO_DATA only when both
						// the audio and the video have reached the end of the stream.
						// No error is returned when only the audio has reached the end of the stream.
						if (psmfHeader == null || currentVideoTimestamp > psmfHeader.mpegLastTimestamp)
						{
							result = SceKernelErrors.ERROR_MPEG_NO_DATA;
						}
					}
					else if (audioFrameLength <= 0 && (psmfHeader == null || psmfHeader.getSpecificStreamNum(PSMF_AUDIO_STREAM) <= 0))
					{
						// There is no audio stream, return ERROR_MPEG_NO_DATA
						result = SceKernelErrors.ERROR_MPEG_NO_DATA;
					}
					else
					{
						// Update the ringbuffer only in case of no error
						mpegRingbufferWrite();
					}
				}
				else
				{
					// Take the PTS from the previous PES header and reset it.
					mpegAtracAu.pts = audioPesHeader.Pts;
					audioPesHeader.DtsPts = UNKNOWN_TIMESTAMP;
					// On PSP, the audio DTS is always set to -1
					mpegAtracAu.dts = UNKNOWN_TIMESTAMP;
					if (auAddr != null && auAddr.NotNull)
					{
						mpegAtracAu.write(auAddr);
					}
					mpegRingbufferWrite();
				}
			}

			if (result == 0)
			{
				if (mpegAtracAu.pts != UNKNOWN_TIMESTAMP)
				{
					currentAudioTimestamp = mpegAtracAu.pts;
				}
				else
				{
					currentAudioTimestamp += audioTimestampStep;
				}
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleMpegGetAtracAu returning result=0x{0:X8}, {1}", result, mpegAtracAu));
			}

			return result;
		}

		public virtual int hleMpegAtracDecode(TPointer auAddr, TPointer bufferAddr, int bufferSize)
		{
			int result = 0;
			int bytes = 0;

			if (audioBuffer != null && audioFrameLength > 0 && audioBuffer.Length >= audioFrameLength)
			{
				int channels = psmfHeader == null ? 2 : psmfHeader.AudioChannelConfig;
				if (audioCodec == null)
				{
					audioCodec = CodecFactory.getCodec(PSP_CODEC_AT3PLUS);
					result = audioCodec.init(audioFrameLength, channels, mpegAudioOutputChannels, 0);
				}
				result = audioCodec.decode(audioBuffer.ReadAddr, audioFrameLength, bufferAddr.Address);
				if (result < 0)
				{
					log.error(string.Format("Error received from codec.decode: 0x{0:X8}", result));
				}
				else
				{
					if (log.TraceEnabled)
					{
						log.trace(string.Format("sceMpegAtracDecode codec returned 0x{0:X}. Decoding from {1}", result, Utilities.getMemoryDump(audioBuffer.ReadAddr, audioFrameLength)));
					}
					bytes = audioCodec.NumberOfSamples * 2 * channels;
				}
				if (audioBuffer.notifyRead(Memory.Instance, audioFrameLength) != audioFrameLength)
				{
					log.error(string.Format("Internal error while consuming from the audio buffer"));
				}

				startedMpeg = true;
				audioFrameCount++;

				delayThread(atracDecodeDelay);

				result = 0;
			}

			// Fill the rest of the buffer with 0's
			bufferAddr.clear(bytes, bufferSize - bytes);

			if (auAddr != null && auAddr.NotNull)
			{
				mpegAtracAu.write(auAddr);
			}

			return 0;
		}

		public virtual int hleMpegAvcDecode(int buffer, int frameWidth, int pixelMode, TPointer32 gotFrameAddr, bool writeAbgr, TPointer auAddr)
		{
			startVideoDecoderThread();

			int threadUid = Modules.ThreadManForUserModule.CurrentThreadID;
			Modules.ThreadManForUserModule.hleBlockCurrentThread(SceKernelThreadInfo.JPCSP_WAIT_VIDEO_DECODER);
			videoDecoderThread.trigger(threadUid, buffer, frameWidth, pixelMode, gotFrameAddr, writeAbgr, auAddr, Emulator.Clock.microTime() + avcDecodeDelay);

			return 0;
		}

		public static DateTime convertTimestampToDate(long timestamp)
		{
			long millis = timestamp / (mpegTimestampPerSecond / 1000);
			return new DateTime(millis);
		}

		protected internal virtual StreamInfo getStreamInfo(int uid)
		{
			return streamMap[uid];
		}

		protected internal virtual int getMpegHandle(int mpegAddr)
		{
			if (Memory.isAddressGood(mpegAddr))
			{
				return Processor.memory.read32(mpegAddr);
			}

			return -1;
		}

		public virtual int checkMpegHandle(int mpeg)
		{
			if (getMpegHandle(mpeg) != mpegHandle)
			{
				log.warn(string.Format("checkMpegHandler bad mpeg handle 0x{0:X8}", mpeg));
				throw new SceKernelErrorException(-1);
			}
			return mpeg;
		}

		protected internal virtual void writeTimestamp(Memory mem, int address, long ts)
		{
			mem.write32(address, (int)((ts >> 32) & 0x1));
			mem.write32(address + 4, (int) ts);
		}

		public static int getMpegVersion(int mpegRawVersion)
		{
			switch (mpegRawVersion)
			{
				case PSMF_VERSION_0012:
					return MPEG_VERSION_0012;
				case PSMF_VERSION_0013:
					return MPEG_VERSION_0013;
				case PSMF_VERSION_0014:
					return MPEG_VERSION_0014;
				case PSMF_VERSION_0015:
					return MPEG_VERSION_0015;
			}

			return -1;
		}

		public static int read8(Memory mem, int bufferAddr, sbyte[] buffer, int offset)
		{
			if (buffer != null)
			{
				return Utilities.read8(buffer, offset);
			}
			return mem.read8(bufferAddr + offset);
		}

		public static int readUnaligned32(Memory mem, int bufferAddr, sbyte[] buffer, int offset)
		{
			if (buffer != null)
			{
				return Utilities.readUnaligned32(buffer, offset);
			}
			return Utilities.readUnaligned32(mem, bufferAddr + offset);
		}

		public static int read32(Memory mem, int bufferAddr, sbyte[] buffer, int offset)
		{
			if (buffer != null)
			{
				return Utilities.readUnaligned32(buffer, offset);
			}
			return mem.read32(bufferAddr + offset);
		}

		public static int read16(Memory mem, int bufferAddr, sbyte[] buffer, int offset)
		{
			if (buffer != null)
			{
				return Utilities.readUnaligned16(buffer, offset);
			}
			return mem.read16(bufferAddr + offset);
		}

		protected internal virtual void analyseMpeg(int bufferAddr, sbyte[] mpegHeader)
		{
			psmfHeader = new PSMFHeader(bufferAddr, mpegHeader);

			avcDecodeResult = MPEG_AVC_DECODE_SUCCESS;
			avcGotFrame = false;
			if (mpegRingbuffer != null)
			{
				resetMpegRingbuffer();
				mpegRingbufferWrite();
			}
			mpegAtracAu.dts = UNKNOWN_TIMESTAMP;
			mpegAtracAu.pts = 0;
			mpegAvcAu.dts = 0;
			mpegAvcAu.pts = 0;
			videoFrameCount = 0;
			audioFrameCount = 0;
			currentVideoTimestamp = 0;
			currentAudioTimestamp = 0;
		}

		protected internal virtual void analyseMpeg(int bufferAddr)
		{
			analyseMpeg(bufferAddr, null);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Stream offset: 0x{0:X}, Stream size: 0x{1:X}", psmfHeader.mpegOffset, psmfHeader.mpegStreamSize));
				log.debug(string.Format("First timestamp: {0:D}, Last timestamp: {1:D}", psmfHeader.mpegFirstTimestamp, psmfHeader.mpegLastTimestamp));
				if (log.TraceEnabled)
				{
					log.trace(Utilities.getMemoryDump(bufferAddr, MPEG_HEADER_BUFFER_MINIMUM_SIZE));
				}
			}
		}

		protected internal virtual bool hasPsmfStream(int streamType)
		{
			if (psmfHeader == null || psmfHeader.psmfStreams == null)
			{
				// Header not analyzed, assume that the PSMF has the given stream
				return true;
			}

			foreach (PSMFStream stream in psmfHeader.psmfStreams)
			{
				if (stream.isStreamOfType(streamType))
				{
					return true;
				}
			}

			return false;
		}

		protected internal virtual bool hasPsmfVideoStream()
		{
			return hasPsmfStream(PSMF_AVC_STREAM);
		}

		protected internal virtual bool hasPsmfAudioStream()
		{
			return hasPsmfStream(PSMF_AUDIO_STREAM);
		}

		protected internal virtual bool hasPsmfUserdataStream()
		{
			return hasPsmfStream(PSMF_DATA_STREAM);
		}

		public static void generateFakeImage(int dest_addr, int frameWidth, int imageWidth, int imageHeight, int pixelMode)
		{
			Memory mem = Memory.Instance;

			System.Random random = new System.Random();
			const int pixelSize = 3;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int bytesPerPixel = sceDisplay.getPixelFormatBytes(pixelMode);
			int bytesPerPixel = sceDisplay.getPixelFormatBytes(pixelMode);
			for (int y = 0; y < imageHeight - pixelSize + 1; y += pixelSize)
			{
				int address = dest_addr + y * frameWidth * bytesPerPixel;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = Math.min(imageWidth, frameWidth);
				int width = System.Math.Min(imageWidth, frameWidth);
				for (int x = 0; x < width; x += pixelSize)
				{
					int n = random.Next(256);
					int color = unchecked((int)0xFF000000) | (n << 16) | (n << 8) | n;
					int pixelColor = Debug.getPixelColor(color, pixelMode);
					if (bytesPerPixel == 4)
					{
						for (int i = 0; i < pixelSize; i++)
						{
							for (int j = 0; j < pixelSize; j++)
							{
								mem.write32(address + (i * frameWidth + j) * 4, pixelColor);
							}
						}
					}
					else if (bytesPerPixel == 2)
					{
						for (int i = 0; i < pixelSize; i++)
						{
							for (int j = 0; j < pixelSize; j++)
							{
								mem.write16(address + (i * frameWidth + j) * 2, (short) pixelColor);
							}
						}
					}
					address += pixelSize * bytesPerPixel;
				}
			}
		}

		public static void delayThread(long startMicros, int delayMicros)
		{
			long now = Emulator.Clock.microTime();
			int threadDelayMicros = delayMicros - (int)(now - startMicros);
			delayThread(threadDelayMicros);
		}

		public static void delayThread(int delayMicros)
		{
			if (delayMicros > 0)
			{
				Modules.ThreadManForUserModule.hleKernelDelayThread(delayMicros, false);
			}
			else
			{
				Modules.ThreadManForUserModule.hleRescheduleCurrentThread();
			}
		}

		protected internal virtual void finishStreams()
		{
			if (log.DebugEnabled)
			{
				log.debug("finishStreams");
			}

			// Release all the streams (can't loop on streamMap as release() modifies it)
			IList<StreamInfo> streams = new LinkedList<sceMpeg.StreamInfo>();
			((List<StreamInfo>)streams).AddRange(streamMap.Values);
			foreach (StreamInfo stream in streams)
			{
				stream.release();
			}
		}

		protected internal virtual void finishMpeg()
		{
			if (log.DebugEnabled)
			{
				log.debug("finishMpeg");
			}

			resetMpegRingbuffer();
			mpegRingbufferWrite();
			VideoEngine.Instance.resetVideoTextures();

			registeredVideoChannel = -1;
			registeredAudioChannel = -1;
			mpegAtracAu.dts = UNKNOWN_TIMESTAMP;
			mpegAtracAu.pts = 0;
			mpegAvcAu.dts = 0;
			mpegAvcAu.pts = 0;
			videoFrameCount = 0;
			audioFrameCount = 0;
			currentVideoTimestamp = 0;
			currentAudioTimestamp = 0;
			startedMpeg = false;

			videoFrameHeight = -1;

			if (videoDecoderThread != null)
			{
				videoDecoderThread.resetWaitingThreadInfo();
			}
			videoCodec = null;
		}

		protected internal virtual void checkEmptyVideoRingbuffer()
		{
			if (mpegAvcAu.esSize > 0)
			{
				return;
			}

			if (mpegRingbuffer == null)
			{
				log.warn("ringbuffer not created");
				throw new SceKernelErrorException(SceKernelErrors.ERROR_MPEG_NO_DATA); // No more data in ringbuffer.
			}

			if (!mpegRingbuffer.hasReadPackets() || (mpegRingbuffer.Empty && videoBuffer.Empty && decodedImages.Count == 0))
			{
				delayThread(mpegDecodeErrorDelay);
				log.debug("ringbuffer and video buffer are empty");
				throw new SceKernelErrorException(SceKernelErrors.ERROR_MPEG_NO_DATA); // No more data in ringbuffer.
			}
		}

		protected internal virtual void checkEmptyAudioRingbuffer()
		{
			if (!mpegRingbuffer.hasReadPackets() || (mpegRingbuffer.Empty && audioBuffer.Empty))
			{
				log.debug("ringbuffer and audio buffer are empty");
				delayThread(mpegDecodeErrorDelay);
				throw new SceKernelErrorException(SceKernelErrors.ERROR_MPEG_NO_DATA); // No more data in ringbuffer.
			}
		}

		protected internal virtual int YCbCrSize
		{
			get
			{
				int width = psmfHeader == null ? Screen.width : psmfHeader.VideoWidth;
				int height = psmfHeader == null ? Screen.height : psmfHeader.VideoHeight;
    
				return getYCbCrSize(width, height);
			}
		}

		protected internal virtual int getYCbCrSize(int width, int height)
		{
			return (width / 2) * (height / 2) * 6; // 12 bits per pixel
		}

		public virtual SceMpegAu MpegAvcAu
		{
			set
			{
				mpegAvcAu.esBuffer = value.esBuffer;
				mpegAvcAu.esSize = value.esSize;
			}
		}

		/// <summary>
		/// sceMpegQueryStreamOffset
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="bufferAddr"> </param>
		/// <param name="offsetAddr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x21FF80E4, version = 150, checkInsideInterrupt = true, stackUsage = 0x18) public int sceMpegQueryStreamOffset(@CheckArgument("checkMpegHandle") int mpeg, pspsharp.HLE.TPointer bufferAddr, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 offsetAddr)
		[HLEFunction(nid : 0x21FF80E4, version : 150, checkInsideInterrupt : true, stackUsage : 0x18)]
		public virtual int sceMpegQueryStreamOffset(int mpeg, TPointer bufferAddr, TPointer32 offsetAddr)
		{
			analyseMpeg(bufferAddr.Address);

			// Check magic.
			if (psmfHeader.mpegMagic != PSMF_MAGIC)
			{
				log.warn("sceMpegQueryStreamOffset bad magic " + string.Format("0x{0:X8}", psmfHeader.mpegMagic));
				offsetAddr.setValue(0);
				return SceKernelErrors.ERROR_MPEG_INVALID_VALUE;
			}

			// Check version.
			if (psmfHeader.mpegVersion < 0)
			{
				log.warn("sceMpegQueryStreamOffset bad version " + string.Format("0x{0:X8}", psmfHeader.mpegRawVersion));
				offsetAddr.setValue(0);
				return SceKernelErrors.ERROR_MPEG_BAD_VERSION;
			}

			// Check offset.
			if ((psmfHeader.mpegOffset & 2047) != 0 || psmfHeader.mpegOffset == 0)
			{
				log.warn("sceMpegQueryStreamOffset bad offset " + string.Format("0x{0:X8}", psmfHeader.mpegOffset));
				offsetAddr.setValue(0);
				return SceKernelErrors.ERROR_MPEG_INVALID_VALUE;
			}

			offsetAddr.setValue(psmfHeader.mpegOffset);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMpegQueryStreamOffset returning 0x{0:X}", offsetAddr.getValue()));
			}

			return 0;
		}

		/// <summary>
		/// sceMpegQueryStreamSize
		/// </summary>
		/// <param name="bufferAddr"> </param>
		/// <param name="sizeAddr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x611E9E11, version = 150, checkInsideInterrupt = true, stackUsage = 0x8) public int sceMpegQueryStreamSize(pspsharp.HLE.TPointer bufferAddr, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 sizeAddr)
		[HLEFunction(nid : 0x611E9E11, version : 150, checkInsideInterrupt : true, stackUsage : 0x8)]
		public virtual int sceMpegQueryStreamSize(TPointer bufferAddr, TPointer32 sizeAddr)
		{
			analyseMpeg(bufferAddr.Address);

			// Check magic.
			if (psmfHeader.mpegMagic != PSMF_MAGIC)
			{
				log.warn(string.Format("sceMpegQueryStreamSize bad magic 0x{0:X8}", psmfHeader.mpegMagic));
				return -1;
			}

			// Check alignment.
			if ((psmfHeader.mpegStreamSize & 2047) != 0)
			{
				sizeAddr.setValue(0);
				return SceKernelErrors.ERROR_MPEG_INVALID_VALUE;
			}

			sizeAddr.setValue(psmfHeader.mpegStreamSize);
			return 0;
		}

		/// <summary>
		/// sceMpegInit
		/// 
		/// @return
		/// </summary>
		[HLELogging(level:"info"), HLEFunction(nid : 0x682A619B, version : 150, checkInsideInterrupt : true, stackUsage : 0x48)]
		public virtual int sceMpegInit()
		{
			finishMpeg();
			finishStreams();

			return 0;
		}

		/// <summary>
		/// sceMpegFinish
		/// 
		/// @return
		/// </summary>
		[HLELogging(level:"info"), HLEFunction(nid : 0x874624D6, version : 150, checkInsideInterrupt : true, stackUsage : 0x18)]
		public virtual int sceMpegFinish()
		{
			finishMpeg();
			finishStreams();

			return 0;
		}

		/// <summary>
		/// sceMpegQueryMemSize
		/// </summary>
		/// <param name="mode">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xC132E22F, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegQueryMemSize(int mode)
		{
			// Mode = 0 -> 64k (constant).
			return MPEG_MEMSIZE;
		}

		/// <summary>
		/// sceMpegCreate
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="data"> </param>
		/// <param name="size"> </param>
		/// <param name="ringbufferAddr"> </param>
		/// <param name="frameWidth"> </param>
		/// <param name="mode"> </param>
		/// <param name="ddrtop">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD8C5F121, version = 150, checkInsideInterrupt = true, stackUsage = 0xA8) public int sceMpegCreate(pspsharp.HLE.TPointer mpeg, pspsharp.HLE.TPointer data, int size, @CanBeNull pspsharp.HLE.TPointer ringbufferAddr, int frameWidth, int mode, int ddrtop)
		[HLEFunction(nid : 0xD8C5F121, version : 150, checkInsideInterrupt : true, stackUsage : 0xA8)]
		public virtual int sceMpegCreate(TPointer mpeg, TPointer data, int size, TPointer ringbufferAddr, int frameWidth, int mode, int ddrtop)
		{
			return hleMpegCreate(mpeg, data, size, ringbufferAddr, frameWidth, mode, ddrtop);
		}

		/// <summary>
		/// sceMpegDelete
		/// </summary>
		/// <param name="mpeg">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x606A4649, version = 150, checkInsideInterrupt = true, stackUsage = 0x28) public int sceMpegDelete(@CheckArgument("checkMpegHandle") int mpeg)
		[HLEFunction(nid : 0x606A4649, version : 150, checkInsideInterrupt : true, stackUsage : 0x28)]
		public virtual int sceMpegDelete(int mpeg)
		{
			if (videoDecoderThread != null)
			{
				videoDecoderThread.exit();
				videoDecoderThread = null;
			}

			finishMpeg();
			finishStreams();

			Modules.ThreadManForUserModule.hleKernelDelayThread(sceVideocodec.videocodecDeleteDelay, false);

			return 0;
		}

		/// <summary>
		/// sceMpegRegistStream
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="streamType"> </param>
		/// <param name="streamNum">
		/// </param>
		/// <returns> stream Uid </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x42560F23, version = 150, checkInsideInterrupt = true, stackUsage = 0x48) public int sceMpegRegistStream(@CheckArgument("checkMpegHandle") int mpeg, int streamType, int streamChannelNum)
		[HLEFunction(nid : 0x42560F23, version : 150, checkInsideInterrupt : true, stackUsage : 0x48)]
		public virtual int sceMpegRegistStream(int mpeg, int streamType, int streamChannelNum)
		{
			StreamInfo info = new StreamInfo(this, streamType, streamChannelNum);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMpegRegistStream returning 0x{0:X}", info.Uid));
			}

			return info.Uid;
		}

		/// <summary>
		/// sceMpegUnRegistStream
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="streamUid">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x591A4AA2, version = 150, checkInsideInterrupt = true, stackUsage = 0x18) public int sceMpegUnRegistStream(@CheckArgument("checkMpegHandle") int mpeg, int streamUid)
		[HLEFunction(nid : 0x591A4AA2, version : 150, checkInsideInterrupt : true, stackUsage : 0x18)]
		public virtual int sceMpegUnRegistStream(int mpeg, int streamUid)
		{
			StreamInfo info = getStreamInfo(streamUid);
			if (info == null)
			{
				log.warn(string.Format("sceMpegUnRegistStream unknown stream=0x{0:X}", streamUid));
				return SceKernelErrors.ERROR_MPEG_UNKNOWN_STREAM_ID;
			}

			info.release();

			return 0;
		}

		/// <summary>
		/// sceMpegMallocAvcEsBuf
		/// </summary>
		/// <param name="mpeg">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA780CF7E, version = 150, checkInsideInterrupt = true, stackUsage = 0x18) public int sceMpegMallocAvcEsBuf(@CheckArgument("checkMpegHandle") int mpeg)
		[HLEFunction(nid : 0xA780CF7E, version : 150, checkInsideInterrupt : true, stackUsage : 0x18)]
		public virtual int sceMpegMallocAvcEsBuf(int mpeg)
		{
			// sceMpegMallocAvcEsBuf does not allocate any memory.
			// It returns 0x00000001 for the first call,
			// 0x00000002 for the second call
			// and 0x00000000 for subsequent calls.
			int esBufferId = 0;
			for (int i = 0; i < allocatedEsBuffers.Length; i++)
			{
				if (!allocatedEsBuffers[i])
				{
					esBufferId = i + 1;
					allocatedEsBuffers[i] = true;
					break;
				}
			}

			return esBufferId;
		}

		/// <summary>
		/// sceMpegFreeAvcEsBuf
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="esBuf">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xCEB870B1, version = 150, checkInsideInterrupt = true, stackUsage = 0x28) public int sceMpegFreeAvcEsBuf(@CheckArgument("checkMpegHandle") int mpeg, int esBuf)
		[HLEFunction(nid : 0xCEB870B1, version : 150, checkInsideInterrupt : true, stackUsage : 0x28)]
		public virtual int sceMpegFreeAvcEsBuf(int mpeg, int esBuf)
		{
			if (esBuf == 0)
			{
				log.warn("sceMpegFreeAvcEsBuf(mpeg=0x" + mpeg.ToString("x") + ", esBuf=0x" + esBuf.ToString("x") + ") bad esBuf handle");
				return SceKernelErrors.ERROR_MPEG_INVALID_VALUE;
			}

			if (esBuf >= 1 && esBuf <= allocatedEsBuffers.Length)
			{
				allocatedEsBuffers[esBuf - 1] = false;
			}
			return 0;
		}

		/// <summary>
		/// sceMpegQueryAtracEsSize
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="esSize_addr"> </param>
		/// <param name="outSize_addr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF8DCB679, version = 150, checkInsideInterrupt = true, stackUsage = 0x18) public int sceMpegQueryAtracEsSize(@CheckArgument("checkMpegHandle") int mpeg, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 esSizeAddr, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 outSizeAddr)
		[HLEFunction(nid : 0xF8DCB679, version : 150, checkInsideInterrupt : true, stackUsage : 0x18)]
		public virtual int sceMpegQueryAtracEsSize(int mpeg, TPointer32 esSizeAddr, TPointer32 outSizeAddr)
		{
			esSizeAddr.setValue(MPEG_ATRAC_ES_SIZE);
			outSizeAddr.setValue(MPEG_ATRAC_ES_OUTPUT_SIZE);

			return 0;
		}

		/// <summary>
		/// sceMpegQueryPcmEsSize
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="esSize_addr"> </param>
		/// <param name="outSize_addr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC02CF6B5, version = 150, checkInsideInterrupt = true) public int sceMpegQueryPcmEsSize(@CheckArgument("checkMpegHandle") int mpeg, pspsharp.HLE.TPointer32 esSizeAddr, pspsharp.HLE.TPointer32 outSizeAddr)
		[HLEFunction(nid : 0xC02CF6B5, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegQueryPcmEsSize(int mpeg, TPointer32 esSizeAddr, TPointer32 outSizeAddr)
		{
			esSizeAddr.setValue(MPEG_PCM_ES_SIZE);
			outSizeAddr.setValue(MPEG_PCM_ES_OUTPUT_SIZE);

			return 0;
		}

		/// <summary>
		/// sceMpegInitAu
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="buffer_addr"> </param>
		/// <param name="auAddr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x167AFD9E, version = 150, checkInsideInterrupt = true, stackUsage = 0x18) public int sceMpegInitAu(@CheckArgument("checkMpegHandle") int mpeg, int buffer_addr, pspsharp.HLE.TPointer auAddr)
		[HLEFunction(nid : 0x167AFD9E, version : 150, checkInsideInterrupt : true, stackUsage : 0x18)]
		public virtual int sceMpegInitAu(int mpeg, int buffer_addr, TPointer auAddr)
		{
			// Check if sceMpegInitAu is being called for AVC or ATRAC
			// and write the proper AU (access unit) struct.
			if (buffer_addr >= 1 && buffer_addr <= allocatedEsBuffers.Length && allocatedEsBuffers[buffer_addr - 1])
			{
				mpegAvcAu.esBuffer = buffer_addr;
				mpegAvcAu.esSize = 0;
				mpegAvcAu.write(auAddr);
			}
			else
			{
				mpegAtracAu.esBuffer = buffer_addr;
				mpegAtracAu.esSize = 0;
				mpegAtracAu.write(auAddr);
			}
			return 0;
		}

		/// <summary>
		/// sceMpegChangeGetAvcAuMode
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="stream_addr"> </param>
		/// <param name="mode">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x234586AE, version = 150, checkInsideInterrupt = true) public int sceMpegChangeGetAvcAuMode(int mpeg, int stream_addr, int mode)
		[HLEFunction(nid : 0x234586AE, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegChangeGetAvcAuMode(int mpeg, int stream_addr, int mode)
		{
			return 0;
		}

		/// <summary>
		/// sceMpegChangeGetAuMode
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="streamUid"> </param>
		/// <param name="mode">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x9DCFB7EA, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegChangeGetAuMode(int mpeg, int streamUid, int mode)
		{
			StreamInfo info = getStreamInfo(streamUid);
			if (info == null)
			{
				log.warn(string.Format("sceMpegChangeGetAuMode unknown stream=0x{0:X}", streamUid));
				return -1;
			}

			// When changing a stream from SKIP to DECODE mode,
			// change all the other streams of the same type to SKIP mode.
			// There is only on stream of a given type in DECODE mode.
			if (info.AuMode == MPEG_AU_MODE_SKIP && mode == MPEG_AU_MODE_DECODE)
			{
				foreach (StreamInfo stream in streamMap.Values)
				{
					if (stream != null && stream != info && stream.isStreamType(info.Type) && stream.AuMode == MPEG_AU_MODE_DECODE)
					{
						stream.AuMode = MPEG_AU_MODE_SKIP;
					}
				}
			}

			info.AuMode = mode;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMpegChangeGetAuMode mode={0}: {1}", mode == MPEG_AU_MODE_DECODE ? "DECODE" : "SKIP", info));
			}

			return 0;
		}

		/// <summary>
		/// sceMpegGetAvcAu
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="streamUid"> </param>
		/// <param name="au_addr"> </param>
		/// <param name="attr_addr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xFE246728, version = 150, checkInsideInterrupt = true) public int sceMpegGetAvcAu(@CheckArgument("checkMpegHandle") int mpeg, int streamUid, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=24, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer auAddr, @CanBeNull pspsharp.HLE.TPointer32 attrAddr)
		[HLEFunction(nid : 0xFE246728, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegGetAvcAu(int mpeg, int streamUid, TPointer auAddr, TPointer32 attrAddr)
		{
			mpegRingbufferRead();

			if (auAddr != null && auAddr.NotNull)
			{
				mpegAvcAu.read(auAddr);
			}

			checkEmptyVideoRingbuffer();

			// @NOTE: Shouldn't this be negated?
			if (Memory.isAddressGood(streamUid))
			{
				log.warn("sceMpegGetAvcAu didn't get a fake stream");
				return SceKernelErrors.ERROR_MPEG_INVALID_ADDR;
			}

			if (!streamMap.ContainsKey(streamUid))
			{
				log.warn(string.Format("sceMpegGetAvcAu bad stream 0x{0:X}", streamUid));
				return -1;
			}

			int result = 0;
			// Update the video timestamp (AVC).
			if (RegisteredVideoChannel)
			{
				result = hleMpegGetAvcAu(auAddr);
			}

			attrAddr.setValue(1); // Unknown.

			if (log.DebugEnabled)
			{
				log.debug(string.Format("videoFrameCount={0:D}(pts={1:D}), audioFrameCount={2:D}(pts={3:D}), pts difference {4:D}, vcount={5:D}", videoFrameCount, currentVideoTimestamp, audioFrameCount, currentAudioTimestamp, currentAudioTimestamp - currentVideoTimestamp, Modules.sceDisplayModule.Vcount));
			}

			return result;
		}

		/// <summary>
		/// sceMpegGetPcmAu
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="streamUid"> </param>
		/// <param name="au_addr"> </param>
		/// <param name="attr_addr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8C1E027D, version = 150, checkInsideInterrupt = true) public int sceMpegGetPcmAu(@CheckArgument("checkMpegHandle") int mpeg, int streamUid, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=24, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer auAddr, @CanBeNull pspsharp.HLE.TPointer32 attrAddr)
		[HLEFunction(nid : 0x8C1E027D, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegGetPcmAu(int mpeg, int streamUid, TPointer auAddr, TPointer32 attrAddr)
		{
			mpegRingbufferRead();

			if (!mpegRingbuffer.hasReadPackets() || mpegRingbuffer.Empty)
			{
				delayThread(mpegDecodeErrorDelay);
				return SceKernelErrors.ERROR_MPEG_NO_DATA; // No more data in ringbuffer.
			}

			// Should be negated?
			if (Memory.isAddressGood(streamUid))
			{
				log.warn("sceMpegGetPcmAu didn't get a fake stream");
				return SceKernelErrors.ERROR_MPEG_INVALID_ADDR;
			}

			if (!streamMap.ContainsKey(streamUid))
			{
				log.warn(string.Format("sceMpegGetPcmAu bad streamUid 0x{0:X8}", streamUid));
				return -1;
			}
			int result = 0;
			// Update the audio timestamp (Atrac).
			if (RegisteredPcmChannel >= 0)
			{
				// Read Au of next Atrac frame
				mpegAtracAu.write(auAddr);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMpegGetPcmAu returning AtracAu={0}", mpegAtracAu.ToString()));
				}
			}
			// Bitfield used to store data attributes.
			// Uses same bitfield as the one in the PSMF header.
			int attr = 1 << 7; // Sampling rate (1 = 44.1kHz).
			attr |= 2; // Number of channels (1 - MONO / 2 - STEREO).
			attrAddr.setValue(attr);

			if (result != 0)
			{
				delayThread(mpegDecodeErrorDelay);
			}
			return result;
		}

		/// <summary>
		/// sceMpegGetAtracAu
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="streamUid"> </param>
		/// <param name="auAddr"> </param>
		/// <param name="attrAddr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE1CE83A7, version = 150, checkInsideInterrupt = true) public int sceMpegGetAtracAu(@CheckArgument("checkMpegHandle") int mpeg, int streamUid, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=24, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer auAddr, @CanBeNull pspsharp.HLE.TPointer32 attrAddr)
		[HLEFunction(nid : 0xE1CE83A7, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegGetAtracAu(int mpeg, int streamUid, TPointer auAddr, TPointer32 attrAddr)
		{
			mpegRingbufferRead();

			checkEmptyAudioRingbuffer();

			if (Memory.isAddressGood(streamUid))
			{
				log.warn("sceMpegGetAtracAu didn't get a fake stream");
				return SceKernelErrors.ERROR_MPEG_INVALID_ADDR;
			}

			if (!streamMap.ContainsKey(streamUid))
			{
				log.warn("sceMpegGetAtracAu bad address " + string.Format("0x{0:X8} 0x{1:X8}", streamUid, auAddr));
				return -1;
			}

			// Update the audio timestamp (Atrac).
			int result = hleMpegGetAtracAu(auAddr);

			// Bitfield used to store data attributes.
			attrAddr.setValue(0); // Pointer to ATRAC3plus stream (from PSMF file).

			if (log.DebugEnabled)
			{
				log.debug(string.Format("videoFrameCount={0:D}(pts={1:D}), audioFrameCount={2:D}(pts={3:D}), pts difference {4:D}, vcount={5:D}", videoFrameCount, currentVideoTimestamp, audioFrameCount, currentAudioTimestamp, currentAudioTimestamp - currentVideoTimestamp, Modules.sceDisplayModule.Vcount));
			}

			return result;
		}

		/// <summary>
		/// sceMpegFlushStream
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="stream_addr">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x500F0429, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegFlushStream(int mpeg, int stream_addr)
		{
			// This call is not deleting the registered streams.
			finishMpeg();
			return 0;
		}

		/// <summary>
		/// sceMpegFlushAllStream
		/// </summary>
		/// <param name="mpeg">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x707B7629, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegFlushAllStream(int mpeg)
		{
			// Finish the Mpeg only if we are not at the start of a new video,
			// otherwise the analyzed video could be lost.
			// This call is not deleting the registered streams.
			if (startedMpeg)
			{
				finishMpeg();
			}

			return 0;
		}

		/// <summary>
		/// sceMpegAvcDecode
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="au_addr"> </param>
		/// <param name="frameWidth"> </param>
		/// <param name="buffer_addr"> </param>
		/// <param name="init_addr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0E3C2E9D, version = 150, checkInsideInterrupt = true) public int sceMpegAvcDecode(@CheckArgument("checkMpegHandle") int mpeg, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=24, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer auAddr, int frameWidth, @CanBeNull pspsharp.HLE.TPointer32 bufferAddr, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 gotFrameAddr)
		[HLEFunction(nid : 0x0E3C2E9D, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegAvcDecode(int mpeg, TPointer auAddr, int frameWidth, TPointer32 bufferAddr, TPointer32 gotFrameAddr)
		{
			int au = auAddr.getValue32();
			int buffer = 0;
			if (bufferAddr.NotNull)
			{
				buffer = bufferAddr.getValue();
			}

			if (avcEsBuf != null && au == -1 && mpegRingbuffer == null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = frameWidth;
				int width = frameWidth;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int height = width < 480 ? 160 : 272;
				int height = width < 480 ? 160 : 272; // How to retrieve the real video height?

				// The application seems to stream the MPEG data into the avcEsBuf.addr buffer,
				// probably only one frame at a time.
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMpegAvcDecode buffer=0x{0:X8}, avcEsBuf: {1}", buffer, Utilities.getMemoryDump(avcEsBuf.addr, AVC_ES_BUF_SIZE)));
				}

				// Generate a faked image. We cannot use the MediaEngine at this point
				// as we have not enough MPEG data buffered in advance.
				generateFakeImage(buffer, frameWidth, width, height, videoPixelMode);

				// Clear the avcEsBuf buffer to better recognize the new MPEG data sent next time
				Processor.memory.memset(avcEsBuf.addr, (sbyte) 0, AVC_ES_BUF_SIZE);

				return 0;
			}

			// When frameWidth is 0, take the frameWidth specified at sceMpegCreate.
			if (frameWidth == 0)
			{
				if (defaultFrameWidth == 0)
				{
					frameWidth = psmfHeader.VideoWidth;
				}
				else
				{
					frameWidth = defaultFrameWidth;
				}
			}
			mpegRingbufferRead();

			if (auAddr.NotNull)
			{
				mpegAvcAu.read(auAddr);
			}

			if (mpegRingbuffer == null && mpegAvcAu.esSize == 0)
			{
				gotFrameAddr.setValue(false);
				return 0;
			}

			checkEmptyVideoRingbuffer();

			hleMpegAvcDecode(buffer, frameWidth, videoPixelMode, gotFrameAddr, true, auAddr);

			startedMpeg = true;

			if (buffer != 0)
			{
				// Do not cache the video image as a texture in the VideoEngine to allow fluid rendering
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int height = psmfHeader != null ? psmfHeader.getVideoHeight() : 272;
				int height = psmfHeader != null ? psmfHeader.VideoHeight : 272;
				VideoEngine.Instance.addVideoTexture(buffer, buffer + height * frameWidth * sceDisplay.getPixelFormatBytes(videoPixelMode));
			}

			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("sceMpegAvcDecode buffer=0x%08X, dts=0x%X, pts=0x%X, gotFrame=%b", buffer, mpegAvcAu.dts, mpegAvcAu.pts, avcGotFrame));
				log.debug(string.Format("sceMpegAvcDecode buffer=0x%08X, dts=0x%X, pts=0x%X, gotFrame=%b", buffer, mpegAvcAu.dts, mpegAvcAu.pts, avcGotFrame));
			}

			// Correct decoding.
			avcDecodeResult = MPEG_AVC_DECODE_SUCCESS;

			if (mpegRingbuffer != null && mpegRingbuffer.PacketSize > 0)
			{
				mpegAvcAu.esSize = 0;
			}

			if (auAddr.NotNull)
			{
				mpegAvcAu.write(auAddr);
			}

			return 0;
		}

		/// <summary>
		/// sceMpegAvcDecodeDetail
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="detailPointer">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0F6C18D7, version = 150, checkInsideInterrupt = true) public int sceMpegAvcDecodeDetail(@CheckArgument("checkMpegHandle") int mpeg, pspsharp.HLE.TPointer detailPointer)
		[HLEFunction(nid : 0x0F6C18D7, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegAvcDecodeDetail(int mpeg, TPointer detailPointer)
		{
			detailPointer.setValue32(0, avcDecodeResult); // Stores the result
			detailPointer.setValue32(4, videoFrameCount); // Last decoded frame
			detailPointer.setValue32(8, psmfHeader != null ? psmfHeader.VideoWidth : lastFrameWidth); // Frame width
			detailPointer.setValue32(12, psmfHeader != null ? psmfHeader.VideoHeight : (videoFrameHeight < 0 ? System.Math.Min(lastFrameHeight, Screen.height) : videoFrameHeight)); // Frame height
			detailPointer.setValue32(16, 0); // Frame crop rect (left)
			detailPointer.setValue32(20, 0); // Frame crop rect (right)
			detailPointer.setValue32(24, 0); // Frame crop rect (top)
			detailPointer.setValue32(28, 0); // Frame crop rect (bottom)
			detailPointer.setValue32(32, avcGotFrame); // Status of the last decoded frame

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMpegAvcDecodeDetail returning decodeResult=0x{0:X}, frameCount={1:D}, width={2:D}, height={3:D}, gotFrame=0x{4:X}", detailPointer.getValue32(0), detailPointer.getValue32(4), detailPointer.getValue32(8), detailPointer.getValue32(12), detailPointer.getValue32(32)));
			}
			return 0;
		}

		/// <summary>
		/// sceMpegAvcDecodeMode
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="mode_addr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA11C7026, version = 150, checkInsideInterrupt = true) public int sceMpegAvcDecodeMode(@CheckArgument("checkMpegHandle") int mpeg, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=8, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 modeAddr)
		[HLEFunction(nid : 0xA11C7026, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegAvcDecodeMode(int mpeg, TPointer32 modeAddr)
		{
			// -1 is a default value.
			int mode = modeAddr.getValue(0);
			int pixelMode = modeAddr.getValue(4);
			if (pixelMode >= TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650 && pixelMode <= TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMpegAvcDecodeMode mode=0x{0:X}, pixelMode=0x{1:X}", mode, pixelMode));
				}
				videoPixelMode = pixelMode;
			}
			else
			{
				log.warn(string.Format("sceMpegAvcDecodeMode mode=0x{0:X}, pixel mode=0x{1:X}: unknown pixel mode", mode, pixelMode));
			}
			return 0;
		}

		/// <summary>
		/// sceMpegAvcDecodeStop
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="frameWidth"> </param>
		/// <param name="buffer_addr"> </param>
		/// <param name="status_addr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x740FCCD1, version = 150, checkInsideInterrupt = true) public int sceMpegAvcDecodeStop(@CheckArgument("checkMpegHandle") int mpeg, int frameWidth, @CanBeNull pspsharp.HLE.TPointer32 bufferAddr, pspsharp.HLE.TPointer32 gotFrameAddr)
		[HLEFunction(nid : 0x740FCCD1, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegAvcDecodeStop(int mpeg, int frameWidth, TPointer32 bufferAddr, TPointer32 gotFrameAddr)
		{
			int buffer = 0;
			if (bufferAddr.NotNull)
			{
				buffer = bufferAddr.getValue();
			}

			// Decode any pending image
			decodeImage(buffer, frameWidth, videoPixelMode, gotFrameAddr, true);

			if (videoDecoderThread != null)
			{
				videoDecoderThread.exit();
				videoDecoderThread = null;
			}

			return 0;
		}

		/// <summary>
		/// sceMpegAvcDecodeFlush
		/// </summary>
		/// <param name="mpeg">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x4571CC64, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegAvcDecodeFlush(int mpeg)
		{
			// Finish the Mpeg if it had no audio.
			// Finish the Mpeg only if we are not at the start of a new video,
			// otherwise the analyzed video could be lost.
			if (startedMpeg && audioFrameCount <= 0)
			{
				finishMpeg();
			}

			return 0;
		}

		/// <summary>
		/// sceMpegAvcQueryYCbCrSize
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="mode">         - 1 -> Loaded from file. 2 -> Loaded from memory. </param>
		/// <param name="width">        - 480. </param>
		/// <param name="height">       - 272. </param>
		/// <param name="resultAddr">   - Where to store the result.
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x211A057C, version = 150, checkInsideInterrupt = true) public int sceMpegAvcQueryYCbCrSize(@CheckArgument("checkMpegHandle") int mpeg, int mode, int width, int height, pspsharp.HLE.TPointer32 resultAddr)
		[HLEFunction(nid : 0x211A057C, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegAvcQueryYCbCrSize(int mpeg, int mode, int width, int height, TPointer32 resultAddr)
		{
			if ((width & 15) != 0 || (height & 15) != 0 || width > 480 || height > 272)
			{
				log.warn("sceMpegAvcQueryYCbCrSize invalid size width=" + width + ", height=" + height);
				return SceKernelErrors.ERROR_MPEG_INVALID_VALUE;
			}

			// Write the size of the buffer used by sceMpegAvcDecodeYCbCr
			int size = YCBCR_DATA_OFFSET + getYCbCrSize(width, height);
			resultAddr.setValue(size);

			return 0;
		}

		/// <summary>
		/// sceMpegAvcInitYCbCr
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="mode"> </param>
		/// <param name="width"> </param>
		/// <param name="height"> </param>
		/// <param name="ycbcr_addr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x67179B1B, version = 150, checkInsideInterrupt = true) public int sceMpegAvcInitYCbCr(@CheckArgument("checkMpegHandle") int mpeg, int mode, int width, int height, pspsharp.HLE.TPointer yCbCrBuffer)
		[HLEFunction(nid : 0x67179B1B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegAvcInitYCbCr(int mpeg, int mode, int width, int height, TPointer yCbCrBuffer)
		{
			yCbCrBuffer.memset((sbyte) 0, YCBCR_DATA_OFFSET);

			return 0;
		}

		/// <summary>
		/// sceMpegAvcDecodeYCbCr
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="au_addr"> </param>
		/// <param name="buffer_addr"> </param>
		/// <param name="init_addr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF0EB1125, version = 150, checkInsideInterrupt = true) public int sceMpegAvcDecodeYCbCr(@CheckArgument("checkMpegHandle") int mpeg, pspsharp.HLE.TPointer auAddr, pspsharp.HLE.TPointer32 bufferAddr, pspsharp.HLE.TPointer32 gotFrameAddr)
		[HLEFunction(nid : 0xF0EB1125, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegAvcDecodeYCbCr(int mpeg, TPointer auAddr, TPointer32 bufferAddr, TPointer32 gotFrameAddr)
		{
			mpegRingbufferRead();

			if (auAddr.NotNull)
			{
				mpegAvcAu.read(auAddr);
			}

			checkEmptyVideoRingbuffer();

			// sceMpegAvcDecodeYCbCr() is performing the video decoding and
			// sceMpegAvcCsc() is transforming the YCbCr image into ABGR.
			hleMpegAvcDecode(bufferAddr.getValue() + YCBCR_DATA_OFFSET, 0, videoPixelMode, gotFrameAddr, false, auAddr);

			startedMpeg = true;

			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("sceMpegAvcDecodeYCbCr *buffer=0x%08X, currentTimestamp=%d, avcGotFrame=%b", bufferAddr.getValue(), mpegAvcAu.pts, avcGotFrame));
				log.debug(string.Format("sceMpegAvcDecodeYCbCr *buffer=0x%08X, currentTimestamp=%d, avcGotFrame=%b", bufferAddr.getValue(), mpegAvcAu.pts, avcGotFrame));
			}

			// Correct decoding.
			avcDecodeResult = MPEG_AVC_DECODE_SUCCESS;

			if (auAddr.NotNull)
			{
				mpegAvcAu.esSize = 0;
				mpegAvcAu.write(auAddr);
			}

			return 0;
		}

		/// <summary>
		/// sceMpegAvcDecodeStopYCbCr
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="buffer_addr"> </param>
		/// <param name="status_addr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF2930C9C, version = 150, checkInsideInterrupt = true) public int sceMpegAvcDecodeStopYCbCr(@CheckArgument("checkMpegHandle") int mpeg, @CanBeNull pspsharp.HLE.TPointer32 bufferAddr, pspsharp.HLE.TPointer32 gotFrameAddr)
		[HLEFunction(nid : 0xF2930C9C, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegAvcDecodeStopYCbCr(int mpeg, TPointer32 bufferAddr, TPointer32 gotFrameAddr)
		{
			int buffer = 0;
			if (bufferAddr.NotNull)
			{
				buffer = bufferAddr.getValue();
			}

			// Decode any pending image
			decodeImage(buffer, 0, videoPixelMode, gotFrameAddr, false);

			return 0;
		}

		/// <summary>
		/// sceMpegAvcCsc
		/// 
		/// sceMpegAvcDecodeYCbCr() is performing the video decoding and
		/// sceMpegAvcCsc() is transforming the YCbCr image into ABGR.
		/// </summary>
		/// <param name="mpeg">          - </param>
		/// <param name="source_addr">   - YCbCr data. </param>
		/// <param name="range_addr">    - YCbCr range. </param>
		/// <param name="frameWidth">    - </param>
		/// <param name="dest_addr">     - Converted data (RGB).
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x31BD0272, version = 150, checkInsideInterrupt = true) public int sceMpegAvcCsc(@CheckArgument("checkMpegHandle") int mpeg, pspsharp.HLE.TPointer sourceAddr, pspsharp.HLE.TPointer32 rangeAddr, int frameWidth, pspsharp.HLE.TPointer destAddr)
		[HLEFunction(nid : 0x31BD0272, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegAvcCsc(int mpeg, TPointer sourceAddr, TPointer32 rangeAddr, int frameWidth, TPointer destAddr)
		{
			// When frameWidth is 0, take the frameWidth specified at sceMpegCreate.
			if (frameWidth == 0)
			{
				if (defaultFrameWidth == 0)
				{
					frameWidth = psmfHeader.VideoWidth;
				}
				else
				{
					frameWidth = defaultFrameWidth;
				}
			}

			int rangeX = rangeAddr.getValue(0);
			int rangeY = rangeAddr.getValue(4);
			int rangeWidth = rangeAddr.getValue(8);
			int rangeHeight = rangeAddr.getValue(12);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMpegAvcCsc range x={0:D}, y={1:D}, width={2:D}, height={3:D}", rangeX, rangeY, rangeWidth, rangeHeight));
			}

			if (((rangeX | rangeY | rangeWidth | rangeHeight) & 0xF) != 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMpegAvcCsc returning ERROR_MPEG_INVALID_VALUE"));
				}
				return SceKernelErrors.ERROR_MPEG_INVALID_VALUE;
			}

			if (rangeX < 0 || rangeY < 0 || rangeWidth < 0 || rangeHeight < 0)
			{
				// Returning ERROR_INVALID_VALUE and not ERROR_MPEG_INVALID_VALUE
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMpegAvcCsc returning ERROR_INVALID_VALUE"));
				}
				return SceKernelErrors.ERROR_INVALID_VALUE;
			}

			int width = psmfHeader == null ? Screen.width : psmfHeader.VideoWidth;
			int height = psmfHeader == null ? Screen.height : psmfHeader.VideoHeight;

			if (rangeX + rangeWidth > width || rangeY + rangeHeight > height)
			{
				// Returning ERROR_INVALID_VALUE and not ERROR_MPEG_INVALID_VALUE
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMpegAvcCsc returning ERROR_INVALID_VALUE"));
				}
				return SceKernelErrors.ERROR_INVALID_VALUE;
			}

			int width2 = width >> 1;
			int height2 = height >> 1;
			int length = width * height;
			int length2 = width2 * height2;

			// Read the YCbCr image
			int[] luma = getIntBuffer(length);
			int[] cb = getIntBuffer(length2);
			int[] cr = getIntBuffer(length2);
			int dataAddr = sourceAddr.Address + YCBCR_DATA_OFFSET;
			if (hasMemoryInt())
			{
				// Optimize the most common case
				int length4 = length >> 2;
				int offset = dataAddr >> 2;
				int[] memoryInt = MemoryInt;
				for (int i = 0, j = 0; i < length4; i++)
				{
					int value = memoryInt[offset++];
					luma[j++] = (value) & 0xFF;
					luma[j++] = (value >> 8) & 0xFF;
					luma[j++] = (value >> 16) & 0xFF;
					luma[j++] = (value >> 24) & 0xFF;
				}
				int length16 = length2 >> 2;
				for (int i = 0, j = 0; i < length16; i++)
				{
					int value = memoryInt[offset++];
					cb[j++] = (value) & 0xFF;
					cb[j++] = (value >> 8) & 0xFF;
					cb[j++] = (value >> 16) & 0xFF;
					cb[j++] = (value >> 24) & 0xFF;
				}
				for (int i = 0, j = 0; i < length16; i++)
				{
					int value = memoryInt[offset++];
					cr[j++] = (value) & 0xFF;
					cr[j++] = (value >> 8) & 0xFF;
					cr[j++] = (value >> 16) & 0xFF;
					cr[j++] = (value >> 24) & 0xFF;
				}
			}
			else
			{
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(dataAddr, length + length2 + length2, 1);
				for (int i = 0; i < length; i++)
				{
					luma[i] = memoryReader.readNext();
				}
				for (int i = 0; i < length2; i++)
				{
					cb[i] = memoryReader.readNext();
				}
				for (int i = 0; i < length2; i++)
				{
					cr[i] = memoryReader.readNext();
				}
			}

			// Convert YCbCr to ABGR
			int[] abgr = getIntBuffer(length);
			H264Utils.YUV2ABGR(width, height, luma, cb, cr, abgr);

			releaseIntBuffer(luma);
			releaseIntBuffer(cb);
			releaseIntBuffer(cr);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int bytesPerPixel = sceDisplay.getPixelFormatBytes(videoPixelMode);
			int bytesPerPixel = sceDisplay.getPixelFormatBytes(videoPixelMode);

			// Do not cache the video image as a texture in the VideoEngine to allow fluid rendering
			VideoEngine.Instance.addVideoTexture(destAddr.Address, destAddr.Address + (rangeY + rangeHeight) * frameWidth * bytesPerPixel);

			// Write the ABGR image
			if (videoPixelMode == TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888 && hasMemoryInt())
			{
				// Optimize the most common case
				int pixelIndex = rangeY * width + rangeX;
				for (int i = 0; i < rangeHeight; i++)
				{
					int addr = destAddr.Address + (i * frameWidth) * bytesPerPixel;
					Array.Copy(abgr, pixelIndex, MemoryInt, addr >> 2, rangeWidth);
					pixelIndex += width;
				}
			}
			else
			{
				int addr = destAddr.Address;
				for (int i = 0; i < rangeHeight; i++)
				{
					IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(addr, rangeWidth * bytesPerPixel, bytesPerPixel);
					int pixelIndex = (i + rangeY) * width + rangeX;
					for (int j = 0; j < rangeWidth; j++, pixelIndex++)
					{
						int abgr8888 = abgr[pixelIndex];
						int pixelColor = Debug.getPixelColor(abgr8888, videoPixelMode);
						memoryWriter.writeNext(pixelColor);
					}
					memoryWriter.flush();
					addr += frameWidth * bytesPerPixel;
				}
			}
			releaseIntBuffer(abgr);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMpegAvcCsc writing to 0x{0:X8}-0x{1:X8}, vcount={2:D}", destAddr.Address, destAddr.Address + (rangeY + rangeHeight) * frameWidth * bytesPerPixel, Modules.sceDisplayModule.Vcount));
			}

			delayThread(avcDecodeDelay);

			return 0;
		}

		/// <summary>
		/// sceMpegAtracDecode
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="au_addr"> </param>
		/// <param name="buffer_addr"> </param>
		/// <param name="init">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x800C44DF, version = 150, checkInsideInterrupt = true) public int sceMpegAtracDecode(@CheckArgument("checkMpegHandle") int mpeg, pspsharp.HLE.TPointer auAddr, pspsharp.HLE.TPointer bufferAddr, int init)
		[HLEFunction(nid : 0x800C44DF, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegAtracDecode(int mpeg, TPointer auAddr, TPointer bufferAddr, int init)
		{
			int result = hleMpegAtracDecode(auAddr, bufferAddr, MPEG_ATRAC_ES_SIZE);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMpegAtracDecode currentTimestamp={0:D}", mpegAtracAu.pts));
			}

			return result;
		}

		protected internal virtual int getPacketsFromSize(int size)
		{
			int packets = size / (2048 + 104);

			return packets;
		}

		private int getSizeFromPackets(int packets)
		{
			int size = (packets * 104) + (packets * 2048);

			return size;
		}

		/// <summary>
		/// sceMpegRingbufferQueryMemSize
		/// </summary>
		/// <param name="packets">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xD7A29F46, version : 150, checkInsideInterrupt : true, stackUsage : 0x8)]
		public virtual int sceMpegRingbufferQueryMemSize(int packets)
		{
			return getSizeFromPackets(packets);
		}

		/// <summary>
		/// sceMpegRingbufferConstruct
		/// </summary>
		/// <param name="ringbuffer_addr"> </param>
		/// <param name="packets"> </param>
		/// <param name="data"> </param>
		/// <param name="size"> </param>
		/// <param name="callback_addr"> </param>
		/// <param name="callback_args">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x37295ED8, version = 150, checkInsideInterrupt = true, stackUsage = 0x38) public int sceMpegRingbufferConstruct(pspsharp.HLE.TPointer ringbufferAddr, int packets, @CanBeNull pspsharp.HLE.TPointer data, int size, @CanBeNull pspsharp.HLE.TPointer callbackAddr, int callbackArgs)
		[HLEFunction(nid : 0x37295ED8, version : 150, checkInsideInterrupt : true, stackUsage : 0x38)]
		public virtual int sceMpegRingbufferConstruct(TPointer ringbufferAddr, int packets, TPointer data, int size, TPointer callbackAddr, int callbackArgs)
		{
			if (size < getSizeFromPackets(packets))
			{
				log.warn(string.Format("sceMpegRingbufferConstruct insufficient space: size={0:D}, packets={1:D}", size, packets));
				return SceKernelErrors.ERROR_MPEG_NO_MEMORY;
			}

			SceMpegRingbuffer ringbuffer = new SceMpegRingbuffer(packets, data.Address, size, callbackAddr.Address, callbackArgs);
			ringbuffer.write(ringbufferAddr);

			return 0;
		}

		/// <summary>
		/// sceMpegRingbufferDestruct
		/// </summary>
		/// <param name="ringbuffer_addr">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x13407F13, version : 150, checkInsideInterrupt : true, stackUsage : 0x8)]
		public virtual int sceMpegRingbufferDestruct(TPointer ringbufferAddr)
		{
			if (mpegRingbuffer != null)
			{
				mpegRingbuffer.read(ringbufferAddr);
				resetMpegRingbuffer();
				mpegRingbuffer.write(ringbufferAddr);
				mpegRingbuffer = null;
				mpegRingbufferAddr = null;
			}

			return 0;
		}

		/// <summary>
		/// sceMpegRingbufferPut
		/// </summary>
		/// <param name="_mpegRingbufferAddr"> </param>
		/// <param name="numPackets"> </param>
		/// <param name="available">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xB240A59E, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegRingbufferPut(TPointer ringbufferAddr, int numPackets, int available)
		{
			mpegRingbufferAddr = ringbufferAddr;
			mpegRingbufferRead();

			if (numPackets < 0)
			{
				return 0;
			}

			int numberPackets = System.Math.Min(available, numPackets);
			if (numberPackets <= 0)
			{
				return 0;
			}

			// Note: we can read more packets than available in the Mpeg stream: the application
			// can loop the video by putting previous packets back into the ringbuffer.

			int putNumberPackets = System.Math.Min(numberPackets, mpegRingbuffer.PutSequentialPackets);
			int putDataAddr = mpegRingbuffer.PutDataAddr;
			AfterRingbufferPutCallback afterRingbufferPutCallback = new AfterRingbufferPutCallback(this, putDataAddr, numberPackets - putNumberPackets);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMpegRingbufferPut executing callback 0x{0:X8} to read 0x{1:X} packets at 0x{2:X8}, Ringbuffer={3}", mpegRingbuffer.CallbackAddr, putNumberPackets, putDataAddr, mpegRingbuffer));
			}
			Modules.ThreadManForUserModule.executeCallback(null, mpegRingbuffer.CallbackAddr, afterRingbufferPutCallback, false, putDataAddr, putNumberPackets, mpegRingbuffer.CallbackArgs);

			return afterRingbufferPutCallback.TotalPacketsAdded;
		}

		/// <summary>
		/// sceMpegRingbufferAvailableSize
		/// </summary>
		/// <param name="_mpegRingbufferAddr">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xB5F6DC87, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegRingbufferAvailableSize(TPointer ringbufferAddr)
		{
			mpegRingbufferAddr = ringbufferAddr;

			mpegRingbufferRead();

			return mpegRingbuffer.FreePackets;
		}

		/// <summary>
		/// sceMpegNextAvcRpAu - skip one video frame
		/// </summary>
		/// <param name="mpeg"> </param>
		/// <param name="unknown60">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3C37A7A6, version = 150, checkInsideInterrupt = true) public int sceMpegNextAvcRpAu(@CheckArgument("checkMpegHandle") int mpeg, int streamUid)
		[HLEFunction(nid : 0x3C37A7A6, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMpegNextAvcRpAu(int mpeg, int streamUid)
		{
			if (!streamMap.ContainsKey(streamUid))
			{
				log.warn(string.Format("sceMpegNextAvcRpAu bad stream 0x{0:X}", streamUid));
				return -1;
			}

			int result = hleMpegGetAvcAu(null);
			if (result != 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMpegNextAvcRpAu returning 0x{0:X8}", result));
				}
				return result;
			}

			videoFrameCount++;
			startedMpeg = true;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x01977054, version = 150) public int sceMpegGetUserdataAu(@CheckArgument("checkMpegHandle") int mpeg, int streamUid, pspsharp.HLE.TPointer auAddr, @CanBeNull pspsharp.HLE.TPointer headerAddr)
		[HLEFunction(nid : 0x01977054, version : 150)]
		public virtual int sceMpegGetUserdataAu(int mpeg, int streamUid, TPointer auAddr, TPointer headerAddr)
		{
			if (!hasPsmfUserdataStream())
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMpegGetUserdataAu no registered user data stream, returning 0x{0:X8}", SceKernelErrors.ERROR_MPEG_NO_DATA));
				}
				return SceKernelErrors.ERROR_MPEG_NO_DATA;
			}

			if (userDataPesHeader == null)
			{
				userDataPesHeader = new PesHeader(RegisteredUserDataChannel);
				userDataPesHeader.DtsPts = UNKNOWN_TIMESTAMP;
			}

			mpegUserDataAu.read(auAddr);

			if (userDataBuffer == null)
			{
				userDataBuffer = new UserDataBuffer(mpegUserDataAu.esBuffer, MPEG_DATA_ES_SIZE);
			}

			readNextUserDataFrame(userDataPesHeader);
			if (userDataLength == 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMpegGetUserdataAu no user data available, returning 0x{0:X8}", SceKernelErrors.ERROR_MPEG_NO_DATA));
				}
				return SceKernelErrors.ERROR_MPEG_NO_DATA;
			}
			if (userDataBuffer.Length < userDataLength)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMpegGetUserdataAu no enough user data available (0x{0:X} from 0x{1:X}), returning 0x{2:X8}", userDataBuffer.Length, userDataLength, SceKernelErrors.ERROR_MPEG_NO_DATA));
				}
				return SceKernelErrors.ERROR_MPEG_NO_DATA;
			}

			mpegRingbufferNotifyRead();

			Memory mem = auAddr.Memory;
			mpegUserDataAu.pts = userDataPesHeader.Pts;
			mpegUserDataAu.dts = UNKNOWN_TIMESTAMP; // dts is always -1
			mpegUserDataAu.esSize = userDataLength;
			mpegUserDataAu.write(auAddr);
			userDataBuffer.notifyRead(mem, mpegUserDataAu.esSize);
			userDataLength = 0;

			if (headerAddr.NotNull)
			{
				// First 8 bytes of the user data header
				for (int i = 0; i < userDataHeader.Length; i++)
				{
					headerAddr.setValue8(i, (sbyte) userDataHeader[i]);
				}
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMpegGetUserdataAu returning au={0}", mpegUserDataAu));
				if (log.TraceEnabled)
				{
					log.trace(string.Format("mpegUserDataAu.esBuffer: {0}", Utilities.getMemoryDump(mpegUserDataAu.esBuffer, mpegUserDataAu.esSize)));
					if (headerAddr.NotNull)
					{
						log.trace(string.Format("headerAddr: {0}", Utilities.getMemoryDump(headerAddr.Address, 8)));
					}
				}
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC45C99CC, version = 150) public int sceMpegQueryUserdataEsSize(@CheckArgument("checkMpegHandle") int mpeg, pspsharp.HLE.TPointer32 esSizeAddr, pspsharp.HLE.TPointer32 outSizeAddr)
		[HLEFunction(nid : 0xC45C99CC, version : 150)]
		public virtual int sceMpegQueryUserdataEsSize(int mpeg, TPointer32 esSizeAddr, TPointer32 outSizeAddr)
		{
			esSizeAddr.setValue(MPEG_DATA_ES_SIZE);
			outSizeAddr.setValue(MPEG_DATA_ES_OUTPUT_SIZE);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0558B075, version = 150) public int sceMpegAvcCopyYCbCr(@CheckArgument("checkMpegHandle") int mpeg, pspsharp.HLE.TPointer destinationAddr, pspsharp.HLE.TPointer sourceAddr)
		[HLEFunction(nid : 0x0558B075, version : 150)]
		public virtual int sceMpegAvcCopyYCbCr(int mpeg, TPointer destinationAddr, TPointer sourceAddr)
		{
			int size = YCbCrSize + YCBCR_DATA_OFFSET;

			destinationAddr.Memory.memcpy(destinationAddr.Address, sourceAddr.Address, size);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMpegAvcCopyYCbCr from 0x{0:X8}-0x{1:X8} to 0x{2:X8}-0x{3:X8}", sourceAddr.Address, sourceAddr.Address + size, destinationAddr.Address, destinationAddr.Address + size));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x11F95CF1, version = 150) public int sceMpegGetAvcNalAu(int mpeg, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=32, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer mp4AvcNalStructAddr, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=24, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer auAddr)
		[HLEFunction(nid : 0x11F95CF1, version : 150)]
		public virtual int sceMpegGetAvcNalAu(int mpeg, TPointer mp4AvcNalStructAddr, TPointer auAddr)
		{
			// Based on information found in
			//    https://github.com/Rinnegatamante/lua-player-plus/blob/master/lpp-c%2B%2B/Libs/Mp4/Mp4.c
			SceMp4AvcNalStruct mp4AvcNalStruct = new SceMp4AvcNalStruct();
			mp4AvcNalStruct.read(mp4AvcNalStructAddr);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMpegGetAvcNalAu mp4AvcNalStruct: {0}", mp4AvcNalStruct));
			}

			SceMpegAu au = new SceMpegAu();
			au.read(auAddr);

			au.esBuffer = mp4AvcNalStruct.nalBuffer;
			au.esSize = mp4AvcNalStruct.nalSize;
			// PSP is returning 0 for pts & dts
			au.pts = 0L;
			au.dts = 0L;

			au.write(auAddr);

			if (!hasVideoCodecExtraData())
			{
				// Build the video codec "extradata" in the expected format
				int[] videoCodecExtraData = new int[8 + mp4AvcNalStruct.spsSize + 3 + mp4AvcNalStruct.ppsSize];
				int offset = 0;
				videoCodecExtraData[offset++] = 0x01; // Need to start with 1
				offset += 3; // Unused
				videoCodecExtraData[offset++] = (mp4AvcNalStruct.nalPrefixSize - 1) & 0x03; // nal length size
				videoCodecExtraData[offset++] = 0x01; // Number of sps
				videoCodecExtraData[offset++] = (mp4AvcNalStruct.spsSize >> 8) & 0xFF;
				videoCodecExtraData[offset++] = (mp4AvcNalStruct.spsSize) & 0xFF;
				IMemoryReader spsReader = MemoryReader.getMemoryReader(mp4AvcNalStruct.spsBuffer, mp4AvcNalStruct.spsSize, 1);
				for (int i = 0; i < mp4AvcNalStruct.spsSize; i++)
				{
					videoCodecExtraData[offset++] = spsReader.readNext();
				}
				videoCodecExtraData[offset++] = 0x01; // Number of pps
				videoCodecExtraData[offset++] = (mp4AvcNalStruct.ppsSize >> 8) & 0xFF;
				videoCodecExtraData[offset++] = (mp4AvcNalStruct.ppsSize) & 0xFF;
				IMemoryReader ppsReader = MemoryReader.getMemoryReader(mp4AvcNalStruct.ppsBuffer, mp4AvcNalStruct.ppsSize, 1);
				for (int i = 0; i < mp4AvcNalStruct.ppsSize; i++)
				{
					videoCodecExtraData[offset++] = ppsReader.readNext();
				}

				VideoCodecExtraData = videoCodecExtraData;

				if (log.DebugEnabled)
				{
					sbyte[] buffer = new sbyte[videoCodecExtraData.Length];
					for (int i = 0; i < buffer.Length; i++)
					{
						buffer[i] = (sbyte) videoCodecExtraData[i];
					}
					log.debug(string.Format("sceMpegGetAvcNalAu videoCodecExtraData: {0}", Utilities.getMemoryDump(buffer, 0, buffer.Length)));
				}
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x921FCCCF, version = 150) public int sceMpegGetAvcEsAu()
		[HLEFunction(nid : 0x921FCCCF, version : 150)]
		public virtual int sceMpegGetAvcEsAu()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6F314410, version = 150) public int sceMpegAvcDecodeGetDecodeSEI(int mpeg, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 decodeSEIAddr)
		[HLEFunction(nid : 0x6F314410, version : 150)]
		public virtual int sceMpegAvcDecodeGetDecodeSEI(int mpeg, TPointer32 decodeSEIAddr)
		{
			decodeSEIAddr.setValue(0);
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAB0E9556, version = 150) public int sceMpegAvcDecodeDetailIndex(int mpeg, int index, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=52, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 detail)
		[HLEFunction(nid : 0xAB0E9556, version : 150)]
		public virtual int sceMpegAvcDecodeDetailIndex(int mpeg, int index, TPointer32 detail)
		{
			detail.setValue(8, mpegAvcInfoStruct.getValue32(8)); // image width
			detail.setValue(12, mpegAvcInfoStruct.getValue32(12)); // image height

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMpegAvcDecodeDetailIndex returning width={0:D}, height={1:D}", detail.getValue(8), detail.getValue(12)));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xCF3547A2, version = 150) public int sceMpegAvcDecodeDetail2(int mpeg, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 detail)
		[HLEFunction(nid : 0xCF3547A2, version : 150)]
		public virtual int sceMpegAvcDecodeDetail2(int mpeg, TPointer32 detail)
		{
			detail.setValue(mpegAvcDetail2Struct.Address);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMpegAvcDecodeDetail2 detail2 structure: {0}", Utilities.getMemoryDump(mpegAvcDetail2Struct.Address, 96)));
			}

			return 0;
		}

		[HLEFunction(nid : 0xF5E7EA31, version : 150)]
		public virtual int sceMpegAvcConvertToYuv420(int mpeg, TPointer yuv420Buffer, TPointer yCbCrBuffer, int unknown2)
		{
			int size = YCbCrSize;

			yCbCrBuffer.Memory.memcpy(yuv420Buffer.Address, yCbCrBuffer.Address + YCBCR_DATA_OFFSET, size);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMpegAvcConvertToYuv420 from 0x{0:X8}-0x{1:X8} to 0x{2:X8}-0x{3:X8}", yCbCrBuffer.Address, yCbCrBuffer.Address + size, yuv420Buffer.Address, yuv420Buffer.Address + size));
			}

			// The YUV420 image will be decoded and saved to memory by sceJpegCsc

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD1CE4950, version = 150) public int sceMpegAvcCscMode(int mpeg, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 modeAddr)
		[HLEFunction(nid : 0xD1CE4950, version : 150)]
		public virtual int sceMpegAvcCscMode(int mpeg, TPointer32 modeAddr)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDBB60658, version = 150) public int sceMpegFlushAu()
		[HLEFunction(nid : 0xDBB60658, version : 150)]
		public virtual int sceMpegFlushAu()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE95838F6, version = 150) public int sceMpegAvcCscInfo()
		[HLEFunction(nid : 0xE95838F6, version : 150)]
		public virtual int sceMpegAvcCscInfo()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x11CAB459, version = 150) public int sceMpeg_11CAB459()
		[HLEFunction(nid : 0x11CAB459, version : 150)]
		public virtual int sceMpeg_11CAB459()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB27711A8, version = 150) public int sceMpeg_B27711A8()
		[HLEFunction(nid : 0xB27711A8, version : 150)]
		public virtual int sceMpeg_B27711A8()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD4DD6E75, version = 150) public int sceMpeg_D4DD6E75()
		[HLEFunction(nid : 0xD4DD6E75, version : 150)]
		public virtual int sceMpeg_D4DD6E75()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC345DED2, version = 150) public int sceMpeg_C345DED2()
		[HLEFunction(nid : 0xC345DED2, version : 150)]
		public virtual int sceMpeg_C345DED2()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x988E9E12, version = 150) public int sceMpeg_988E9E12()
		[HLEFunction(nid : 0x988E9E12, version : 150)]
		public virtual int sceMpeg_988E9E12()
		{
			return 0;
		}

		[HLEFunction(nid : 0x769BEBB6, version : 250)]
		public virtual int sceMpegRingbufferQueryPackNum(int memorySize)
		{
			return getPacketsFromSize(memorySize);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x63B9536A, version = 600) public int sceMpegAvcResourceGetAvcDecTopAddr(int unknown)
		[HLEFunction(nid : 0x63B9536A, version : 600)]
		public virtual int sceMpegAvcResourceGetAvcDecTopAddr(int unknown)
		{
			// Unknown value, passed to sceMpegCreate(ddttop)
			return 0x12345678;
		}

		[HLEFunction(nid : 0x8160A2FE, version : 600)]
		public virtual int sceMpegAvcResourceFinish()
		{
			if (avcEsBuf != null)
			{
				Modules.SysMemUserForUserModule.free(avcEsBuf);
				avcEsBuf = null;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAF26BB01, version = 600) public int sceMpegAvcResourceGetAvcEsBuf()
		[HLEFunction(nid : 0xAF26BB01, version : 600)]
		public virtual int sceMpegAvcResourceGetAvcEsBuf()
		{
			if (avcEsBuf == null)
			{
				log.warn(string.Format("sceMpegAvcResourceGetAvcEsBuf avcEsBuf not allocated"));
				return -1;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMpegAvcResourceGetAvcEsBuf returning 0x{0:X8}", avcEsBuf.addr));
			}

			return avcEsBuf.addr;
		}

		[HLELogging(level:"warn"), HLEFunction(nid : 0xFCBDB5AD, version : 600)]
		public virtual int sceMpegAvcResourceInit(int unknown)
		{
			if (unknown != 1)
			{
				return SceKernelErrors.ERROR_MPEG_INVALID_VALUE;
			}

			avcEsBuf = Modules.SysMemUserForUserModule.malloc(USER_PARTITION_ID, "sceMpegAvcEsBuf", PSP_SMEM_High, AVC_ES_BUF_SIZE, 0);
			if (avcEsBuf != null)
			{
				Processor.memory.memset(avcEsBuf.addr, (sbyte) 0, avcEsBuf.size);
			}

			return 0;
		}
	}
}