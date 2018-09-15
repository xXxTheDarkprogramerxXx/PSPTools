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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_PSMFPLAYER_NOT_INITIALIZED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceMpegRingbuffer.ringbufferPacketSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.KERNEL_PARTITION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_Low;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceMpeg.MPEG_MEMSIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceMpeg.PSMF_MAGIC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceMpeg.PSMF_MAGIC_OFFSET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceMpeg.PSMF_STREAM_OFFSET_OFFSET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceMpeg.PSMF_STREAM_SIZE_OFFSET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceMpeg.read32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR4444;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR5551;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.endianSwap32;

	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceMpegRingbuffer = pspsharp.HLE.kernel.types.SceMpegRingbuffer;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using SeekableDataInput = pspsharp.filesystems.SeekableDataInput;
	using ISectorDevice = pspsharp.filesystems.umdiso.ISectorDevice;
	using VideoEngine = pspsharp.graphics.VideoEngine;
	using Screen = pspsharp.hardware.Screen;
	using Atrac3plusDecoder = pspsharp.media.codec.atrac3plus.Atrac3plusDecoder;
	using Utilities = pspsharp.util.Utilities;

	//using Logger = org.apache.log4j.Logger;

	public class scePsmfPlayer : HLEModule
	{
		private bool InstanceFieldsInitialized = false;

		public scePsmfPlayer()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			audioSamplesBytes = audioSamples * 4;
		}

		//public static Logger log = Modules.getLogger("scePsmfPlayer");

		// PSMF Player timing management.
		protected internal const int psmfPlayerVideoTimestampStep = sceMpeg.videoTimestampStep;
		protected internal const int psmfPlayerAudioTimestampStep = sceMpeg.audioTimestampStep;
		protected internal const int psmfTimestampPerSecond = sceMpeg.mpegTimestampPerSecond;

		// PSMF Player status.
		protected internal const int PSMF_PLAYER_STATUS_NONE = 0x0;
		protected internal const int PSMF_PLAYER_STATUS_INIT = 0x1;
		protected internal const int PSMF_PLAYER_STATUS_STANDBY = 0x2;
		protected internal const int PSMF_PLAYER_STATUS_PLAYING = 0x4;
		protected internal const int PSMF_PLAYER_STATUS_ERROR = 0x100;
		protected internal const int PSMF_PLAYER_STATUS_PLAYING_FINISHED = 0x200;

		// PSMF Player status vars.
		protected internal int psmfPlayerStatus;

		// PSMF Player mode.
		protected internal const int PSMF_PLAYER_MODE_PLAY = 0;
		protected internal const int PSMF_PLAYER_MODE_SLOWMOTION = 1;
		protected internal const int PSMF_PLAYER_MODE_STEPFRAME = 2;
		protected internal const int PSMF_PLAYER_MODE_PAUSE = 3;
		protected internal const int PSMF_PLAYER_MODE_FORWARD = 4;
		protected internal const int PSMF_PLAYER_MODE_REWIND = 5;

		// PSMF Player stream type.
		protected internal const int PSMF_PLAYER_STREAM_VIDEO = 14;

		// PSMF Player playback speed.
		protected internal const int PSMF_PLAYER_SPEED_SLOW = 1;
		protected internal const int PSMF_PLAYER_SPEED_NORMAL = 2;
		protected internal const int PSMF_PLAYER_SPEED_FAST = 3;

		// PSMF Player config mode.
		protected internal const int PSMF_PLAYER_CONFIG_MODE_LOOP = 0;
		protected internal const int PSMF_PLAYER_CONFIG_MODE_PIXEL_TYPE = 1;

		// PSMF Player config loop.
		protected internal const int PSMF_PLAYER_CONFIG_LOOP = 0;
		protected internal const int PSMF_PLAYER_CONFIG_NO_LOOP = 1;

		// PSMF Player config pixel type.
		protected internal const int PSMF_PLAYER_PIXEL_TYPE_NONE = -1;

		// PSMF Player version.
		protected internal const int PSMF_PLAYER_VERSION_FULL = 0;
		protected internal const int PSMF_PLAYER_VERSION_BASIC = 1;
		protected internal const int PSMF_PLAYER_VERSION_NET = 2;

		// PMF file vars.
		protected internal string pmfFilePath;
		protected internal sbyte[] pmfFileData;

		// PMSF info.
		protected internal int psmfAvcStreamNum = 1;
		protected internal int psmfAtracStreamNum = 1;
		protected internal int psmfPcmStreamNum = 0;
		protected internal int psmfPlayerVersion = PSMF_PLAYER_VERSION_FULL;

		// PSMF Player playback params.
		protected internal int displayBuffer;
		protected internal int displayBufferSize;
		protected internal int playbackThreadPriority;

		// PSMF Player playback info.
		protected internal int videoCodec;
		protected internal int videoStreamNum;
		protected internal int audioCodec;
		protected internal int audioStreamNum;
		protected internal int playMode;
		protected internal int playSpeed;
		protected internal int initPts;

		// PSMF Player video data.
		protected internal int videoDataFrameWidth = 512; // Default.
		protected internal int videoDataDisplayBuffer;
		protected internal int videoDataDisplayPts;

		// PSMF Player config.
		protected internal int videoPixelMode = TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888; // Default.
		protected internal int videoLoopStatus = PSMF_PLAYER_CONFIG_NO_LOOP; // Default.

		// PSMF Player audio size
		protected internal readonly int audioSamples = Atrac3plusDecoder.ATRAC3P_FRAME_SAMPLES;
		protected internal int audioSamplesBytes;

		// Internal vars.
		protected internal SysMemInfo mpegMem;
		protected internal SysMemInfo ringbufferMem;
		protected internal int pmfFileDataRingbufferPosition;
		private static readonly int MAX_TIMESTAMP_DIFFERENCE = sceMpeg.audioTimestampStep * 2;
		protected internal int lastMpegGetAtracAuResult;

		public virtual int checkPlayerInitialized(int psmfPlayer)
		{
			if (psmfPlayerStatus == PSMF_PLAYER_STATUS_NONE)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("checkPlayerInitialized player not initialized (status=0x{0:X})", psmfPlayerStatus));
				}
				throw new SceKernelErrorException(ERROR_PSMFPLAYER_NOT_INITIALIZED);
			}

			return psmfPlayer;
		}

		public virtual int checkPlayerPlaying(int psmfPlayer)
		{
			psmfPlayer = checkPlayerInitialized(psmfPlayer);
			if (psmfPlayerStatus != PSMF_PLAYER_STATUS_PLAYING && psmfPlayerStatus != PSMF_PLAYER_STATUS_PLAYING_FINISHED && psmfPlayerStatus != PSMF_PLAYER_STATUS_ERROR)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("checkPlayerInitialized player not playing (status=0x{0:X})", psmfPlayerStatus));
				}
				throw new SceKernelErrorException(ERROR_PSMFPLAYER_NOT_INITIALIZED);
			}

			return psmfPlayer;
		}

		public virtual long CurrentVideoTimestamp
		{
			get
			{
				return Modules.sceMpegModule.CurrentVideoTimestamp;
			}
		}

		public virtual long CurrentAudioTimestamp
		{
			get
			{
				return Modules.sceMpegModule.CurrentAudioTimestamp;
			}
		}

		protected internal virtual int MaxTimestampDifference
		{
			get
			{
				int maxTimestampDifference = MAX_TIMESTAMP_DIFFERENCE;
    
				// At video startup, allow for a longer timestamp difference to avoid audio stuttering.
				long firstTimestamp = Modules.sceMpegModule.PsmfHeader.mpegFirstTimestamp;
				if (CurrentVideoTimestamp < firstTimestamp + sceMpeg.videoTimestampStep * 10)
				{
					maxTimestampDifference *= 2;
				}
    
				return maxTimestampDifference;
			}
		}

		protected internal virtual int hlePsmfPlayerSetPsmf(int psmfPlayer, PspString fileAddr, int offset, bool doCallbacks, bool useSizeFromPsmfHeader)
		{
			if (psmfPlayerStatus != PSMF_PLAYER_STATUS_INIT)
			{
				return ERROR_PSMFPLAYER_NOT_INITIALIZED;
			}

			if (offset != 0)
			{
				Console.WriteLine(string.Format("hlePsmfPlayerSetPsmf unimplemented offset=0x{0:X}", offset));
			}

			pmfFilePath = fileAddr.String;

			// Get the file and read it to a buffer.
			try
			{
				if (log.InfoEnabled)
				{
					log.info(string.Format("Loading PSMF file '{0}'", pmfFilePath));
				}

				SeekableDataInput psmfFile = Modules.IoFileMgrForUserModule.getFile(pmfFilePath, 0);
				psmfFile.seek(offset);

				int Length = (int) psmfFile.Length() - offset;
				// Some PSMF files have an incorrect size stored into their header.
				// It seems that the PSP is ignoring this size when using scePsmfPlayerSetPsmf().
				// However, the size is probably not ignored when using scePsmfPlayerSetPsmfOffset().
				if (useSizeFromPsmfHeader)
				{
					// Try to find the Length of the PSMF file by reading the PSMF header
					sbyte[] header = new sbyte[pspsharp.filesystems.umdiso.ISectorDevice_Fields.sectorLength];
					psmfFile.readFully(header);
					int psmfMagic = read32(null, 0, header, PSMF_MAGIC_OFFSET);
					if (psmfMagic == PSMF_MAGIC)
					{
						// Found the PSMF header, extract the file size from the stream size and offset.
						Length = endianSwap32(read32(null, 0, header, PSMF_STREAM_SIZE_OFFSET));
						Length += endianSwap32(read32(null, 0, header, PSMF_STREAM_OFFSET_OFFSET));
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("PSMF Length=0x{0:X}, header: {1}", Length, Utilities.getMemoryDump(header, 0, header.Length)));
						}
					}
				}

				psmfFile.seek(offset);
				pmfFileData = new sbyte[Length];
				psmfFile.readFully(pmfFileData);
				psmfFile.Dispose();

				Modules.sceMpegModule.analyseMpeg(0, pmfFileData);
				pmfFileDataRingbufferPosition = Modules.sceMpegModule.PsmfHeader.mpegOffset;
			}
			catch (System.OutOfMemoryException e)
			{
				Console.WriteLine("hlePsmfPlayerSetPsmf", e);
			}
			catch (IOException e)
			{
				Console.WriteLine("hlePsmfPlayerSetPsmf", e);
			}

			// Switch to STANDBY.
			psmfPlayerStatus = PSMF_PLAYER_STATUS_STANDBY;

			// Delay the thread for 100ms
			Modules.ThreadManForUserModule.hleKernelDelayThread(100000, doCallbacks);

			return 0;
		}

		protected internal virtual int RemainingFileData
		{
			get
			{
				SceMpegRingbuffer ringbuffer = Modules.sceMpegModule.MpegRingbuffer;
				int packetSize = ringbuffer.PacketSize;
				int packetsInRingbuffer = ringbuffer.PacketsInRingbuffer;
				int bytesInRingbuffer = packetsInRingbuffer * packetSize;
				int bytesRemainingInFileData = pmfFileData.Length - pmfFileDataRingbufferPosition;
				int bytesRemaining = bytesRemainingInFileData + bytesInRingbuffer;
				if (log.TraceEnabled)
				{
					log.trace(string.Format("getRemainingFileData packetsInRingbuffer=0x{0:X}, bytesRemainingInFileData=0x{1:X}, bytesRemaining=0x{2:X}", packetsInRingbuffer, bytesRemainingInFileData, bytesRemaining));
				}
    
				return bytesRemaining;
			}
		}

		protected internal virtual void hlePsmfFillRingbuffer(Memory mem)
		{
			SceMpegRingbuffer ringbuffer = Modules.sceMpegModule.MpegRingbuffer;
			ringbuffer.notifyConsumed();
			if (ringbuffer.PutSequentialPackets > 0)
			{
				int packetSize = ringbuffer.PacketSize;
				int addr = ringbuffer.PutDataAddr;
				int size = ringbuffer.PutSequentialPackets * packetSize;
				size = System.Math.Min(size, pmfFileData.Length - pmfFileDataRingbufferPosition);

				if (log.TraceEnabled)
				{
					log.trace(string.Format("Filling ringbuffer at 0x{0:X8}, size=0x{1:X} with file data from offset 0x{2:X}", addr, size, pmfFileDataRingbufferPosition));
					log.trace(string.Format("Ringbuffer putSequentialPackets={0:D}, file data Length=0x{1:X}, position=0x{2:X}", ringbuffer.PutSequentialPackets, pmfFileData.Length, pmfFileDataRingbufferPosition));
				}
				for (int i = 0; i < size; i++)
				{
					mem.write8(addr + i, pmfFileData[pmfFileDataRingbufferPosition + i]);
				}
				ringbuffer.addPackets((size + packetSize - 1) / packetSize);
				pmfFileDataRingbufferPosition += size;

				Modules.sceMpegModule.hleMpegNotifyVideoDecoderThread();
			}
		}

		[HLEFunction(nid : 0x235D8787, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerCreate(int psmfPlayer, TPointer32 psmfPlayerDataAddr)
		{
			// The psmfDataAddr contains three fields that are manually set before
			// scePsmfPlayerCreate is called.
			displayBuffer = psmfPlayerDataAddr.getValue(0) & Memory.addressMask; // The buffer allocated for scePsmf, which is ported into scePsmfPlayer.
			displayBufferSize = psmfPlayerDataAddr.getValue(4); // The buffer's size.
			playbackThreadPriority = psmfPlayerDataAddr.getValue(8); // Priority of the "START" thread.
			if (log.InfoEnabled)
			{
				log.info(string.Format("PSMF Player Data: displayBuffer=0x{0:X8}, displayBufferSize=0x{1:X}, playbackThreadPriority={2:D}", displayBuffer, displayBufferSize, playbackThreadPriority));
			}

			// Allocate memory for the MPEG structure
			Memory mem = Memory.Instance;
			mpegMem = Modules.SysMemUserForUserModule.malloc(KERNEL_PARTITION_ID, Name + "-Mpeg", PSP_SMEM_Low, MPEG_MEMSIZE, 0);
			int result = Modules.sceMpegModule.hleMpegCreate(TPointer.NULL, new TPointer(mem, mpegMem.addr), MPEG_MEMSIZE, null, Screen.width, 0, 0);
			if (result < 0)
			{
				Console.WriteLine(string.Format("scePsmfPlayerCreate: error 0x{0:X8} while calling hleMpegCreate", result));
			}

			// Allocate memory for the ringbuffer, scePsmfPlayer creates a ringbuffer with 581 packets
			const int packets = 581;
			ringbufferMem = Modules.SysMemUserForUserModule.malloc(KERNEL_PARTITION_ID, Name + "-Ringbuffer", PSP_SMEM_Low, packets * ringbufferPacketSize, 0);
			Modules.sceMpegModule.hleCreateRingbuffer(packets, ringbufferMem.addr, ringbufferMem.size);
			SceMpegRingbuffer ringbuffer = Modules.sceMpegModule.MpegRingbuffer;
			// This ringbuffer is used both for audio and video
			ringbuffer.HasAudio = true;
			ringbuffer.HasVideo = true;

			// Start with INIT.
			psmfPlayerStatus = PSMF_PLAYER_STATUS_INIT;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x9B71A274, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerDelete(@CheckArgument("checkPlayerInitialized") int psmfPlayer)
		[HLEFunction(nid : 0x9B71A274, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerDelete(int psmfPlayer)
		{
			VideoEngine.Instance.resetVideoTextures();

			if (ringbufferMem != null)
			{
				Modules.SysMemUserForUserModule.free(ringbufferMem);
				ringbufferMem = null;
			}
			if (mpegMem != null)
			{
				Modules.SysMemUserForUserModule.free(mpegMem);
				mpegMem = null;
			}

			// Set to NONE.
			psmfPlayerStatus = PSMF_PLAYER_STATUS_NONE;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x3D6D25A9, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerSetPsmf(@CheckArgument("checkPlayerInitialized") int psmfPlayer, pspsharp.HLE.PspString fileAddr)
		[HLEFunction(nid : 0x3D6D25A9, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerSetPsmf(int psmfPlayer, PspString fileAddr)
		{
			return hlePsmfPlayerSetPsmf(psmfPlayer, fileAddr, 0, false, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x58B83577, version = 150) public int scePsmfPlayerSetPsmfCB(@CheckArgument("checkPlayerInitialized") int psmfPlayer, pspsharp.HLE.PspString fileAddr)
		[HLEFunction(nid : 0x58B83577, version : 150)]
		public virtual int scePsmfPlayerSetPsmfCB(int psmfPlayer, PspString fileAddr)
		{
			return hlePsmfPlayerSetPsmf(psmfPlayer, fileAddr, 0, true, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE792CD94, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerReleasePsmf(@CheckArgument("checkPlayerInitialized") int psmfPlayer)
		[HLEFunction(nid : 0xE792CD94, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerReleasePsmf(int psmfPlayer)
		{
			if (psmfPlayerStatus != PSMF_PLAYER_STATUS_STANDBY)
			{
				return ERROR_PSMFPLAYER_NOT_INITIALIZED;
			}

			Modules.sceMpegModule.finishMpeg();

			VideoEngine.Instance.resetVideoTextures();

			// Go back to INIT, because some applications recognize that another file can be
			// loaded after scePsmfPlayerReleasePsmf has been called.
			psmfPlayerStatus = PSMF_PLAYER_STATUS_INIT;

			Modules.ThreadManForUserModule.hleKernelDelayThread(10000, false);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x95A84EE5, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerStart(@CheckArgument("checkPlayerInitialized") int psmfPlayer, @CanBeNull pspsharp.HLE.TPointer32 initPlayInfoAddr, int initPts)
		[HLEFunction(nid : 0x95A84EE5, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerStart(int psmfPlayer, TPointer32 initPlayInfoAddr, int initPts)
		{
			// Read the playback parameters.
			if (initPlayInfoAddr.NotNull)
			{
				videoCodec = initPlayInfoAddr.getValue(0);
				videoStreamNum = initPlayInfoAddr.getValue(4);
				audioCodec = initPlayInfoAddr.getValue(8);
				audioStreamNum = initPlayInfoAddr.getValue(12);
				playMode = initPlayInfoAddr.getValue(16);
				playSpeed = initPlayInfoAddr.getValue(20);

				Modules.sceMpegModule.RegisteredVideoChannel = videoStreamNum;
				Modules.sceMpegModule.RegisteredAudioChannel = audioStreamNum;

				if (log.InfoEnabled)
				{
					log.info(string.Format("Found play info data: videoCodec=0x{0:X}, videoStreamNum={1:D}, audioCodec=0x{2:X}, audioStreamNum={3:D}, playMode={4:D}, playSpeed={5:D}", videoCodec, videoStreamNum, audioCodec, audioStreamNum, playMode, playSpeed));
				}
			}

			this.initPts = initPts;

			// Switch to PLAYING.
			psmfPlayerStatus = PSMF_PLAYER_STATUS_PLAYING;

			lastMpegGetAtracAuResult = 0;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x3EA82A4B, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerGetAudioOutSize(@CheckArgument("checkPlayerInitialized") int psmfPlayer)
		[HLEFunction(nid : 0x3EA82A4B, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerGetAudioOutSize(int psmfPlayer)
		{
			return audioSamplesBytes;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1078C008, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerStop(@CheckArgument("checkPlayerInitialized") int psmfPlayer)
		[HLEFunction(nid : 0x1078C008, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerStop(int psmfPlayer)
		{
			VideoEngine.Instance.resetVideoTextures();

			// Always switch to STANDBY, because this PSMF can still be resumed.
			psmfPlayerStatus = PSMF_PLAYER_STATUS_STANDBY;

			// scePsmfPlayerStop does not reschedule threads

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA0B8CA55, version = 150) public int scePsmfPlayerUpdate(@CheckArgument("checkPlayerPlaying") int psmfPlayer)
		[HLEFunction(nid : 0xA0B8CA55, version : 150)]
		public virtual int scePsmfPlayerUpdate(int psmfPlayer)
		{
			// Can be called from interrupt.
			// Check playback status.
			int remainingFileData = RemainingFileData;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePsmfPlayerUpdate remainingFileData=0x{0:X}", remainingFileData));
			}

			if (remainingFileData <= 0)
			{
				// If we've reached the end of the file data, change the status to PLAYING_FINISHED.
				// Remark: do not use the PSMF header last timestamp as it may contain an incorrect
				//         value which seems to be ignored by the PSP.
				psmfPlayerStatus = PSMF_PLAYER_STATUS_PLAYING_FINISHED;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x46F61F8B, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerGetVideoData(@CheckArgument("checkPlayerPlaying") int psmfPlayer, @CanBeNull pspsharp.HLE.TPointer32 videoDataAddr)
		[HLEFunction(nid : 0x46F61F8B, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerGetVideoData(int psmfPlayer, TPointer32 videoDataAddr)
		{
			int result = 0;

			if (psmfPlayerStatus != PSMF_PLAYER_STATUS_PLAYING && psmfPlayerStatus != PSMF_PLAYER_STATUS_PLAYING_FINISHED)
			{
				return ERROR_PSMFPLAYER_NOT_INITIALIZED;
			}

			if (playMode == PSMF_PLAYER_MODE_PAUSE)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("scePsmfPlayerGetVideoData in pause mode, returning 0x{0:X8}", result));
				}
				return result;
			}

			if (videoDataAddr.NotNull)
			{
				videoDataFrameWidth = videoDataAddr.getValue(0);
				videoDataDisplayBuffer = videoDataAddr.getValue(4) & Memory.addressMask;
				videoDataDisplayPts = videoDataAddr.getValue(8);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("scePsmfPlayerGetVideoData videoDataFrameWidth={0:D}, videoDataDisplayBuffer=0x{1:X8}, videoDataDisplayPts={2:D}", videoDataFrameWidth, videoDataDisplayBuffer, videoDataDisplayPts));
				}
			}

			// Check if there's already a valid pointer at videoDataAddr.
			// If not, use the displayBuffer from scePsmfPlayerCreate.
			if (Memory.isAddressGood(videoDataDisplayBuffer))
			{
				displayBuffer = videoDataDisplayBuffer;
			}
			else if (videoDataAddr.NotNull)
			{
				videoDataAddr.setValue(4, displayBuffer);
				// Valid frame width?
				if (videoDataFrameWidth <= 0 || videoDataFrameWidth > 512)
				{
					videoDataFrameWidth = 512;
					videoDataAddr.setValue(0, videoDataFrameWidth);
				}
			}

			if (CurrentAudioTimestamp > 0 && CurrentVideoTimestamp > 0 && CurrentVideoTimestamp > CurrentAudioTimestamp + MaxTimestampDifference && lastMpegGetAtracAuResult == 0)
			{
				//result = SceKernelErrors.ERROR_PSMFPLAYER_AUDIO_VIDEO_OUT_OF_SYNC;
				Modules.sceMpegModule.writeLastFrameABGR(displayBuffer, videoDataFrameWidth, videoPixelMode);
			}
			else
			{
				// Check if the ringbuffer needs additional data
				hlePsmfFillRingbuffer(Emulator.Memory);

				// Retrieve the video Au
				result = Modules.sceMpegModule.hleMpegGetAvcAu(null);
				if (result < 0)
				{
					// We have reached the end of the file...
					if (pmfFileDataRingbufferPosition >= pmfFileData.Length)
					{
						SceMpegRingbuffer ringbuffer = Modules.sceMpegModule.MpegRingbuffer;
						ringbuffer.consumeAllPackets();
					}
				}
				else
				{
					// Write the video data
					result = Modules.sceMpegModule.hleMpegAvcDecode(displayBuffer, videoDataFrameWidth, videoPixelMode, null, true, TPointer.NULL);
				}
			}

			// Do not cache the video image as a texture in the VideoEngine to allow fluid rendering
			VideoEngine.Instance.addVideoTexture(displayBuffer, displayBuffer + 272 * videoDataFrameWidth * sceDisplay.getPixelFormatBytes(videoPixelMode));

			// Return updated timestamp
			videoDataAddr.setValue(8, (int) CurrentVideoTimestamp);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePsmfPlayerGetVideoData currentVideoTimestamp={0:D}, returning 0x{1:X8}", CurrentVideoTimestamp, result));
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB9848A74, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerGetAudioData(@CheckArgument("checkPlayerPlaying") int psmfPlayer, pspsharp.HLE.TPointer audioDataAddr)
		[HLEFunction(nid : 0xB9848A74, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerGetAudioData(int psmfPlayer, TPointer audioDataAddr)
		{
			int result = 0;

			if (playMode == PSMF_PLAYER_MODE_PAUSE)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("scePsmfPlayerGetAudioData in pause mode, returning 0x{0:X8}", result));
				}
				// Clear the audio buffer (silent audio returned)
				audioDataAddr.clear(audioSamplesBytes);
				return result;
			}

			if (CurrentAudioTimestamp > 0 && CurrentVideoTimestamp > 0 && CurrentAudioTimestamp > CurrentVideoTimestamp + MaxTimestampDifference && lastMpegGetAtracAuResult == 0)
			{
				result = SceKernelErrors.ERROR_PSMFPLAYER_AUDIO_VIDEO_OUT_OF_SYNC;
			}
			else
			{
				// Check if the ringbuffer needs additional data
				hlePsmfFillRingbuffer(audioDataAddr.Memory);

				// Retrieve the audio Au
				result = Modules.sceMpegModule.hleMpegGetAtracAu(null);
				lastMpegGetAtracAuResult = result;

				// Write the audio data
				result = Modules.sceMpegModule.hleMpegAtracDecode(null, audioDataAddr, audioSamplesBytes);
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePsmfPlayerGetAudioData currentAudioTimestamp={0:D}, returning 0x{1:X8}", CurrentAudioTimestamp, result));
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF8EF08A6, version = 150) public int scePsmfPlayerGetCurrentStatus(@CheckArgument("checkPlayerInitialized") int psmfPlayer)
		[HLEFunction(nid : 0xF8EF08A6, version : 150)]
		public virtual int scePsmfPlayerGetCurrentStatus(int psmfPlayer)
		{
			// scePsmfPlayerGetCurrentStatus can be called from an interrupt
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePsmfPlayerGetCurrentStatus returning status 0x{0:X}", psmfPlayerStatus));
			}

			return psmfPlayerStatus;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xDF089680, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerGetPsmfInfo(@CheckArgument("checkPlayerInitialized") int psmfPlayer, pspsharp.HLE.TPointer32 psmfInfoAddr)
		[HLEFunction(nid : 0xDF089680, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerGetPsmfInfo(int psmfPlayer, TPointer32 psmfInfoAddr)
		{
			if (psmfPlayerStatus < PSMF_PLAYER_STATUS_STANDBY)
			{
				return ERROR_PSMFPLAYER_NOT_INITIALIZED;
			}

			psmfInfoAddr.setValue(0, (int) Modules.sceMpegModule.psmfHeader.mpegLastTimestamp);
			psmfInfoAddr.setValue(4, psmfAvcStreamNum);
			psmfInfoAddr.setValue(8, psmfAtracStreamNum);
			psmfInfoAddr.setValue(12, psmfPcmStreamNum);
			psmfInfoAddr.setValue(16, psmfPlayerVersion);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1E57A8E7, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerConfigPlayer(@CheckArgument("checkPlayerInitialized") int psmfPlayer, int configMode, int configAttr)
		[HLEFunction(nid : 0x1E57A8E7, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerConfigPlayer(int psmfPlayer, int configMode, int configAttr)
		{
			if (psmfPlayerStatus == PSMF_PLAYER_STATUS_NONE)
			{
				return ERROR_PSMFPLAYER_NOT_INITIALIZED;
			}

			if (configMode == PSMF_PLAYER_CONFIG_MODE_LOOP)
			{ // Sets if the video is looped or not.
				if (configAttr < 0 || configAttr > 1)
				{
					return SceKernelErrors.ERROR_PSMFPLAYER_INVALID_CONFIG_VALUE;
				}
				videoLoopStatus = configAttr;
			}
			else if (configMode == PSMF_PLAYER_CONFIG_MODE_PIXEL_TYPE)
			{ // Sets the display's pixel type.
				switch (configAttr)
				{
					case PSMF_PLAYER_PIXEL_TYPE_NONE:
						// -1 means nothing to change
						break;
					case TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650:
					case TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR5551:
					case TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR4444:
					case TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888:
						videoPixelMode = configAttr;
						break;
					case 4:
						// This value is accepted, but its function is unknown
						Console.WriteLine(string.Format("scePsmfPlayerConfigPlayer unknown pixelMode={0:D}", configAttr));
						break;
					default:
						return SceKernelErrors.ERROR_PSMFPLAYER_INVALID_CONFIG_VALUE;
				}
			}
			else
			{
				Console.WriteLine(string.Format("scePsmfPlayerConfigPlayer invalid configMode={0:D}, configAttr={1:D}", configMode, configAttr));
				return SceKernelErrors.ERROR_PSMFPLAYER_INVALID_CONFIG_MODE;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA3D81169, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerChangePlayMode(@CheckArgument("checkPlayerInitialized") int psmfPlayer, int playMode, int playSpeed)
		[HLEFunction(nid : 0xA3D81169, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerChangePlayMode(int psmfPlayer, int playMode, int playSpeed)
		{
			this.playMode = playMode;
			this.playSpeed = playSpeed;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x68F07175, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerGetCurrentAudioStream(@CheckArgument("checkPlayerInitialized") int psmfPlayer, @CanBeNull pspsharp.HLE.TPointer32 audioCodecAddr, @CanBeNull pspsharp.HLE.TPointer32 audioStreamNumAddr)
		[HLEFunction(nid : 0x68F07175, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerGetCurrentAudioStream(int psmfPlayer, TPointer32 audioCodecAddr, TPointer32 audioStreamNumAddr)
		{
			if (psmfPlayerStatus < PSMF_PLAYER_STATUS_STANDBY)
			{
				return ERROR_PSMFPLAYER_NOT_INITIALIZED;
			}

			audioCodecAddr.setValue(audioCodec);
			audioStreamNumAddr.setValue(audioStreamNum);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF3EFAA91, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerGetCurrentPlayMode(@CheckArgument("checkPlayerInitialized") int psmfPlayer, @CanBeNull pspsharp.HLE.TPointer32 playModeAddr, @CanBeNull pspsharp.HLE.TPointer32 playSpeedAddr)
		[HLEFunction(nid : 0xF3EFAA91, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerGetCurrentPlayMode(int psmfPlayer, TPointer32 playModeAddr, TPointer32 playSpeedAddr)
		{
			playModeAddr.setValue(playMode);
			playSpeedAddr.setValue(playSpeed);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x3ED62233, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerGetCurrentPts(@CheckArgument("checkPlayerInitialized") int psmfPlayer, pspsharp.HLE.TPointer32 currentPtsAddr)
		[HLEFunction(nid : 0x3ED62233, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerGetCurrentPts(int psmfPlayer, TPointer32 currentPtsAddr)
		{
			if (psmfPlayerStatus < PSMF_PLAYER_STATUS_STANDBY)
			{
				return ERROR_PSMFPLAYER_NOT_INITIALIZED;
			}

			// Write our current video presentation timestamp.
			currentPtsAddr.setValue((int) CurrentVideoTimestamp);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x9FF2B2E7, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerGetCurrentVideoStream(@CheckArgument("checkPlayerInitialized") int psmfPlayer, @CanBeNull pspsharp.HLE.TPointer32 videoCodecAddr, @CanBeNull pspsharp.HLE.TPointer32 videoStreamNumAddr)
		[HLEFunction(nid : 0x9FF2B2E7, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerGetCurrentVideoStream(int psmfPlayer, TPointer32 videoCodecAddr, TPointer32 videoStreamNumAddr)
		{
			if (psmfPlayerStatus < PSMF_PLAYER_STATUS_STANDBY)
			{
				return ERROR_PSMFPLAYER_NOT_INITIALIZED;
			}

			videoCodecAddr.setValue(videoCodec);
			videoStreamNumAddr.setValue(videoStreamNum);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2BEB1569, version = 150) public int scePsmfPlayerBreak(@CheckArgument("checkPlayerInitialized") int psmfPlayer)
		[HLEFunction(nid : 0x2BEB1569, version : 150)]
		public virtual int scePsmfPlayerBreak(int psmfPlayer)
		{
			// Can be called from interrupt.
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x76C0F4AE, version = 150) public int scePsmfPlayerSetPsmfOffset(@CheckArgument("checkPlayerInitialized") int psmfPlayer, pspsharp.HLE.PspString fileAddr, int offset)
		[HLEFunction(nid : 0x76C0F4AE, version : 150)]
		public virtual int scePsmfPlayerSetPsmfOffset(int psmfPlayer, PspString fileAddr, int offset)
		{
			return hlePsmfPlayerSetPsmf(psmfPlayer, fileAddr, offset, false, true);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA72DB4F9, version = 150) public int scePsmfPlayerSetPsmfOffsetCB(@CheckArgument("checkPlayerInitialized") int psmfPlayer, pspsharp.HLE.PspString fileAddr, int offset)
		[HLEFunction(nid : 0xA72DB4F9, version : 150)]
		public virtual int scePsmfPlayerSetPsmfOffsetCB(int psmfPlayer, PspString fileAddr, int offset)
		{
			return hlePsmfPlayerSetPsmf(psmfPlayer, fileAddr, offset, true, true);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2D0E4E0A, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerSetTempBuf(int psmfPlayer, pspsharp.HLE.TPointer tempBufAddr, int tempBufSize)
		[HLEFunction(nid : 0x2D0E4E0A, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerSetTempBuf(int psmfPlayer, TPointer tempBufAddr, int tempBufSize)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x75F03FA2, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerSelectSpecificVideo(@CheckArgument("checkPlayerInitialized") int psmfPlayer, int videoCodec, int videoStreamNum)
		[HLEFunction(nid : 0x75F03FA2, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerSelectSpecificVideo(int psmfPlayer, int videoCodec, int videoStreamNum)
		{
			this.videoCodec = videoCodec;
			this.videoStreamNum = videoStreamNum;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x85461EFF, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerSelectSpecificAudio(@CheckArgument("checkPlayerInitialized") int psmfPlayer, int audioCodec, int audioStreamNum)
		[HLEFunction(nid : 0x85461EFF, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerSelectSpecificAudio(int psmfPlayer, int audioCodec, int audioStreamNum)
		{
			this.audioCodec = audioCodec;
			this.audioStreamNum = audioStreamNum;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8A9EBDCD, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerSelectVideo(@CheckArgument("checkPlayerInitialized") int psmfPlayer)
		[HLEFunction(nid : 0x8A9EBDCD, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerSelectVideo(int psmfPlayer)
		{
			// Advances to the next video stream number.
			videoStreamNum++;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB8D10C56, version = 150, checkInsideInterrupt = true) public int scePsmfPlayerSelectAudio(@CheckArgument("checkPlayerInitialized") int psmfPlayer)
		[HLEFunction(nid : 0xB8D10C56, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfPlayerSelectAudio(int psmfPlayer)
		{
			// Advances to the next audio stream number.
			audioStreamNum++;

			return 0;
		}
	}
}