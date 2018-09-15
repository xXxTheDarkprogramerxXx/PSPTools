using System.Text;

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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_AA3_INVALID_CODEC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_AA3_INVALID_HEADER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_AA3_INVALID_HEADER_FLAGS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_AA3_INVALID_HEADER_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ATRAC_ALL_DATA_DECODED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ATRAC_API_FAIL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ATRAC_BAD_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ATRAC_BUFFER_IS_EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ATRAC_INCORRECT_READ_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ATRAC_INVALID_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ATRAC_NO_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ATRAC_UNKNOWN_FORMAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_BUSY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.KERNEL_PARTITION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_Low;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceAudiocodec.PSP_CODEC_AT3;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceAudiocodec.PSP_CODEC_AT3PLUS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUnaligned32;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using pspFileBuffer = pspsharp.HLE.kernel.types.pspFileBuffer;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using AudiocodecInfo = pspsharp.HLE.modules.sceAudiocodec.AudiocodecInfo;
	using ICodec = pspsharp.media.codec.ICodec;
	using Atrac3Decoder = pspsharp.media.codec.atrac3.Atrac3Decoder;
	using Atrac3plusDecoder = pspsharp.media.codec.atrac3plus.Atrac3plusDecoder;
	using Utilities = pspsharp.util.Utilities;

	//using Logger = org.apache.log4j.Logger;

	public class sceAtrac3plus : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceAtrac3plus");

		public override void start()
		{
			for (int i = 0; i < atracIDs.Length; i++)
			{
				atracIDs[i] = new AtracID(i);
			}

			// Tested on PSP:
			// Only 2 atracIDs per format can be registered at the same time.
			// Note: After firmware 2.50, these limits can be changed by sceAtracReinit.
			hleAtracReinit(2, 2);

			base.start();
		}

		public override void stop()
		{
			if (temporaryDecodeArea != null)
			{
				Modules.SysMemUserForUserModule.free(temporaryDecodeArea);
				temporaryDecodeArea = null;
			}

			base.stop();
		}

		public override int MemoryUsage
		{
			get
			{
				// No need to allocate additional memory when the module has been
				// loaded using sceKernelLoadModuleToBlock()
				// by the PSP "flash0:/kd/utility.prx".
				// The memory has already been allocated in that case.
				if (Modules.ModuleMgrForKernelModule.isMemoryAllocatedForModule("flash0:/kd/libatrac3plus.prx"))
				{
					return 0;
				}
    
				return 0x8000;
			}
		}

		public const int AT3_MAGIC = 0x0270; // "AT3"
		public const int AT3_PLUS_MAGIC = 0xFFFE; // "AT3PLUS"
		public const int RIFF_MAGIC = 0x46464952; // "RIFF"
		public const int WAVE_MAGIC = 0x45564157; // "WAVE"
		public const int FMT_CHUNK_MAGIC = 0x20746D66; // "FMT "
		protected internal const int FACT_CHUNK_MAGIC = 0x74636166; // "FACT"
		protected internal const int SMPL_CHUNK_MAGIC = 0x6C706D73; // "SMPL"
		public const int DATA_CHUNK_MAGIC = 0x61746164; // "DATA"

		private const int ATRAC3_CONTEXT_READ_SIZE_OFFSET = 160;
		private const int ATRAC3_CONTEXT_REQUIRED_SIZE_OFFSET = 164;
		private const int ATRAC3_CONTEXT_DECODE_RESULT_OFFSET = 188;

		public const int PSP_ATRAC_ALLDATA_IS_ON_MEMORY = -1;
		public const int PSP_ATRAC_NONLOOP_STREAM_DATA_IS_ON_MEMORY = -2;
		public const int PSP_ATRAC_LOOP_STREAM_DATA_IS_ON_MEMORY = -3;

		protected internal const int PSP_ATRAC_STATUS_NONLOOP_STREAM_DATA = 0;
		protected internal const int PSP_ATRAC_STATUS_LOOP_STREAM_DATA = 1;

		public const int ATRAC_HEADER_HASH_LENGTH = 512;

		public const int atracDecodeDelay = 2300; // Microseconds, based on PSP tests

		protected internal AtracID[] atracIDs = new AtracID[6];

		private static SysMemInfo temporaryDecodeArea;

		protected internal class LoopInfo
		{
			protected internal int cuePointID;
			protected internal int type;
			protected internal int startSample;
			protected internal int endSample;
			protected internal int fraction;
			protected internal int playCount;

			public override string ToString()
			{
				return string.Format("LoopInfo[cuePointID {0:D}, type {1:D}, startSample 0x{2:X}, endSample 0x{3:X}, fraction {4:D}, playCount {5:D}]", cuePointID, type, startSample, endSample, fraction, playCount);
			}
		}

		public class AtracFileInfo
		{
			public int atracBitrate = 64;
			public int atracChannels = 2;
			public int atracSampleRate = 0xAC44;
			public int atracBytesPerFrame = 0x0230;
			public int atracEndSample;
			public int atracSampleOffset;
			public int atracCodingMode;
			public int inputFileDataOffset;
			public int inputFileSize;
			public int inputDataSize;

			public int loopNum;
			public int numLoops;
			public LoopInfo[] loops;
		}

		public class AtracID : AudiocodecInfo
		{
			// Internal info.
			protected internal int codecType;
			protected internal bool inUse;
			protected internal int currentReadPosition;
			// Context (used only from firmware 6.00)
			protected internal SysMemInfo atracContext;
			protected internal SysMemInfo internalBuffer;
			// Sound data.
			protected internal AtracFileInfo info;
			protected internal int atracCurrentSample;
			protected internal int maxSamples;
			protected internal int skippedSamples;
			protected internal int skippedEndSamples;
			internal int startSkippedSamples;
			protected internal int lastDecodedSamples;
			protected internal int channels;
			// First buffer.
			protected internal pspFileBuffer inputBuffer;
			protected internal bool reloadingFromLoopStart;
			// Second buffer.
			protected internal int secondBufferAddr = -1;
			protected internal int secondBufferSize;
			// Input file.
			protected internal int secondInputFileSize;
			protected internal bool isSecondBufferNeeded;
			protected internal bool isSecondBufferSet;
			protected internal int internalErrorInfo;
			// Loops
			protected internal int currentLoopNum = -1;
			// LowLevel decoding
			protected internal int sourceBufferLength;
			// AddStreamData
			protected internal int getStreamDataInfoCurrentSample;

			public AtracID(int id) : base(id)
			{
				info = new AtracFileInfo();
			}

			public override void release()
			{
				base.release();
				InUse = false;
				releaseContext();
				releaseInternalBuffer();
			}

			public virtual int setHalfwayBuffer(int addr, int readSize, int bufferSize, bool isMonoOutput, AtracFileInfo info)
			{
				this.info = info;
				channels = info.atracChannels;
				inputBuffer = new pspFileBuffer(addr, bufferSize, readSize, readSize);
				inputBuffer.notifyRead(info.inputFileDataOffset);
				inputBuffer.FileMaxSize = info.inputFileSize;
				currentReadPosition = info.inputFileDataOffset;
				atracCurrentSample = 0;
				currentLoopNum = -1;
				lastDecodedSamples = 0;

				OutputChannels = isMonoOutput ? 1 : 2;

				int result = codec.init(info.atracBytesPerFrame, channels, outputChannels, info.atracCodingMode);
				if (result < 0)
				{
					return result;
				}

				setCodecInitialized();

				return 0;
			}

			public virtual int decodeData(int samplesAddr, TPointer32 outEndAddr)
			{
				skippedEndSamples = 0;

				if (currentReadPosition + info.atracBytesPerFrame > info.inputFileSize || AtracCurrentSample - info.atracSampleOffset > info.atracEndSample)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("decodeData returning ERROR_ATRAC_ALL_DATA_DECODED"));
					}
					outEndAddr.setValue(true);
					return ERROR_ATRAC_ALL_DATA_DECODED;
				}

				if (inputBuffer.CurrentSize < info.atracBytesPerFrame)
				{
					if (SecondBufferAddr > 0 && SecondBufferSize >= info.atracBytesPerFrame)
					{
						addSecondBufferStreamData();
					}
					else
					{
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("decodeData returning ERROR_ATRAC_BUFFER_IS_EMPTY"));
						}
						outEndAddr.setValue(false);
						return ERROR_ATRAC_BUFFER_IS_EMPTY;
					}
				}

				int currentSample = AtracCurrentSample;
				int nextCurrentSample = currentSample;
				if (currentSample == 0)
				{
					skippedSamples = startSkippedSamples + info.atracSampleOffset;
				}
				else
				{
					// When looping or changing the play position, the PSP re-aligns
					// on multiples of maxSamples
					skippedSamples = (currentSample + startSkippedSamples + info.atracSampleOffset) % maxSamples;
				}

				// Skip complete frames
				while (skippedSamples >= maxSamples)
				{
					inputBuffer.notifyRead(info.atracBytesPerFrame);
					currentReadPosition += info.atracBytesPerFrame;
					skippedSamples -= maxSamples;
					nextCurrentSample += maxSamples;
				}

				int readAddr = inputBuffer.ReadAddr;
				if (inputBuffer.ReadSize < info.atracBytesPerFrame)
				{
					if (temporaryDecodeArea == null || temporaryDecodeArea.allocatedSize < info.atracBytesPerFrame)
					{
						if (temporaryDecodeArea != null)
						{
							Modules.SysMemUserForUserModule.free(temporaryDecodeArea);
						}
						temporaryDecodeArea = Modules.SysMemUserForUserModule.malloc(KERNEL_PARTITION_ID, "Temporary-sceAtrac3plus-DecodeData", PSP_SMEM_Low, info.atracBytesPerFrame, 0);
					}
					if (temporaryDecodeArea != null)
					{
						Memory mem = Memory.Instance;
						readAddr = temporaryDecodeArea.addr;
						int wrapLength = inputBuffer.ReadSize;
						mem.memcpy(readAddr, inputBuffer.ReadAddr, wrapLength);
						mem.memcpy(readAddr + wrapLength, inputBuffer.Addr, info.atracBytesPerFrame - wrapLength);
					}
				}

				SysMemInfo tempBuffer = null;
				int decodedSamplesAddr = samplesAddr;
				int bytesPerSample = 2 * OutputChannels;
				if (skippedSamples > 0)
				{
					// Decode to a temporary buffer if we need to skip the first samples.
					int tempBufferSize = MaxSamples * bytesPerSample;
					tempBuffer = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "sceAtrac3plus-temp-decode-buffer", SysMemUserForUser.PSP_SMEM_Low, tempBufferSize, 0);
					if (tempBuffer == null)
					{
						Console.WriteLine(string.Format("decodeData cannot allocate required temporary buffer of size=0x{0:X}", tempBufferSize));
					}
					else
					{
						decodedSamplesAddr = tempBuffer.addr;
					}
				}

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("decodeData from 0x{0:X8}(0x{1:X}) to 0x{2:X8}(0x{3:X}), skippedSamples=0x{4:X}, currentSample=0x{5:X}, outputChannels={6:D}", readAddr, info.atracBytesPerFrame, decodedSamplesAddr, maxSamples, skippedSamples, currentSample, outputChannels));
				}

				int result = codec.decode(readAddr, info.atracBytesPerFrame, decodedSamplesAddr);
				if (result < 0)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("decodeData received codec decode error 0x{0:X8}", result));
					}
					outEndAddr.setValue(false);
					if (tempBuffer != null)
					{
						Modules.SysMemUserForUserModule.free(tempBuffer);
					}
					return ERROR_ATRAC_API_FAIL;
				}

				inputBuffer.notifyRead(info.atracBytesPerFrame);
				currentReadPosition += info.atracBytesPerFrame;

				nextCurrentSample += codec.NumberOfSamples - skippedSamples;

				if (nextCurrentSample - info.atracSampleOffset > info.atracEndSample)
				{
					outEndAddr.setValue(info.loopNum == 0);
					skippedEndSamples = nextCurrentSample - info.atracSampleOffset - info.atracEndSample - 1;
				}
				else
				{
					outEndAddr.setValue(false);
				}
				AtracCurrentSample = nextCurrentSample;

				if (skippedSamples > 0)
				{
					Memory mem = Memory.Instance;
					int returnedSamples = NumberOfSamples;
					// Move the sample buffer to skip the needed samples
					mem.memmove(samplesAddr, decodedSamplesAddr + skippedSamples * bytesPerSample, returnedSamples * bytesPerSample);
				}

				if (tempBuffer != null)
				{
					Modules.SysMemUserForUserModule.free(tempBuffer);
					tempBuffer = null;
				}

				for (int i = 0; i < info.numLoops; i++)
				{
					LoopInfo loop = info.loops[i];
					if (currentSample <= loop.startSample && loop.startSample < nextCurrentSample)
					{
						// We are just starting a loop
						currentLoopNum = i;
						break;
					}
					else if (currentSample <= loop.endSample && loop.endSample < nextCurrentSample && currentLoopNum == i)
					{
						// We are just ending the current loop
						if (info.loopNum == 0)
						{
							// No more loop playback
							currentLoopNum = -1;
						}
						else
						{
							// Replay the loop
							log.info(string.Format("Replaying atrac loop atracID={0:D}, loopStart=0x{1:X}, loopEnd=0x{2:X}", id, loop.startSample, loop.endSample));
							PlayPosition = loop.startSample;
							nextCurrentSample = loop.startSample;
							// loopNum < 0: endless loop playback
							// loopNum > 0: play the loop loopNum times
							if (info.loopNum > 0)
							{
								info.loopNum--;
							}
							break;
						}
					}
				}

				return 0;
			}

			public virtual void getStreamDataInfo(TPointer32 writeAddr, TPointer32 writableBytesAddr, TPointer32 readOffsetAddr)
			{
				if (inputBuffer.FileWriteSize <= 0 && currentLoopNum >= 0 && info.loopNum != 0)
				{
					// Read ahead to restart the loop
					inputBuffer.FilePosition = getFilePositionFromSample(info.loops[currentLoopNum].startSample);
					reloadingFromLoopStart = true;
				}

				// Remember the CurrentSample at the time of the getStreamDataInfo
				getStreamDataInfoCurrentSample = AtracCurrentSample;

				writeAddr.setValue(inputBuffer.WriteAddr);
				writableBytesAddr.setValue(inputBuffer.WriteSize);
				readOffsetAddr.setValue(inputBuffer.FilePosition);
			}

			protected internal virtual void addStreamData(int Length)
			{
				if (Length > 0)
				{
					if (AtracCurrentSample < getStreamDataInfoCurrentSample)
					{
						// The atrac has looped since sceAtracGetStreamDataInfo() has been called.
						// Ignore sceAtracAddStreamData as we now need atrac data from the loop start.
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("addStreamData ignored as the atrac has looped inbetween: sample 0x{0:X} -> 0x{1:X}", getStreamDataInfoCurrentSample, AtracCurrentSample));
						}
					}
					else
					{
						inputBuffer.notifyWrite(Length);
					}
				}
			}

			internal virtual void addSecondBufferStreamData()
			{
				while (inputBuffer.WriteSize > 0 && secondBufferSize > 0)
				{
					int Length = System.Math.Min(inputBuffer.WriteSize, secondBufferSize);
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("addSecondBufferStreamData from 0x{0:X8} to 0x{1:X8}, Length=0x{2:X}", secondBufferAddr, inputBuffer.WriteAddr, Length));
					}
					Memory.Instance.memcpy(inputBuffer.WriteAddr, secondBufferAddr, Length);
					addStreamData(Length);
					secondBufferAddr += Length;
					secondBufferSize -= Length;
				}

				if (secondBufferSize <= 0)
				{
					secondBufferAddr = -1;
					secondBufferSize = 0;
				}
			}

			public virtual void setSecondBuffer(int address, int size)
			{
				secondBufferAddr = address;
				secondBufferSize = size;
			}

			public virtual int SecondBufferAddr
			{
				get
				{
					return secondBufferAddr;
				}
			}

			public virtual int SecondBufferSize
			{
				get
				{
					return secondBufferSize;
				}
			}

			public virtual int SecondBufferReadPosition
			{
				get
				{
					// TODO
					return 0;
				}
			}

			public virtual void createContext()
			{
				if (atracContext == null)
				{
					atracContext = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.USER_PARTITION_ID, string.Format("ThreadMan-AtracCtx-{0:D}", id), SysMemUserForUser.PSP_SMEM_High, 200, 0);
					if (atracContext != null)
					{
						Memory mem = Memory.Instance;
						int contextAddr = atracContext.addr;
						mem.memset(contextAddr, (sbyte) 0, atracContext.size);

						if (hasLoop())
						{
							mem.write32(contextAddr + 140, info.loops[0].endSample); // loop end
						}
						else
						{
							mem.write32(contextAddr + 140, 0); // no loop
						}
						mem.write8(contextAddr + 149, (sbyte) 2); // state
						mem.write8(contextAddr + 151, (sbyte) Channels); // number of channels
						mem.write16(contextAddr + 154, (short) CodecType);
						//mem.write32(contextAddr + 168, 0); // Voice associated to this Atrac context using __sceSasSetVoiceATRAC3?

						// Used by SampleSourceAtrac3 (input for __sceSasConcatenateATRAC3):
						mem.write32(contextAddr + ATRAC3_CONTEXT_READ_SIZE_OFFSET, InputBuffer.FilePosition);
						mem.write32(contextAddr + ATRAC3_CONTEXT_REQUIRED_SIZE_OFFSET, InputBuffer.FilePosition);
						mem.write32(contextAddr + ATRAC3_CONTEXT_DECODE_RESULT_OFFSET, 0);
					}
				}
			}

			internal virtual void releaseContext()
			{
				if (atracContext != null)
				{
					Modules.SysMemUserForUserModule.free(atracContext);
					atracContext = null;
				}
			}

			public virtual SysMemInfo Context
			{
				get
				{
					return atracContext;
				}
			}

			public virtual void createInternalBuffer(int size)
			{
				if (internalBuffer == null)
				{
					internalBuffer = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.USER_PARTITION_ID, string.Format("ThreadMan-AtracBuf-{0:D}", id), SysMemUserForUser.PSP_SMEM_Low, size, 0);
				}
			}

			internal virtual void releaseInternalBuffer()
			{
				if (internalBuffer != null)
				{
					Modules.SysMemUserForUserModule.free(internalBuffer);
					internalBuffer = null;
				}
			}

			public virtual SysMemInfo InternalBuffer
			{
				get
				{
					return internalBuffer;
				}
			}

			public virtual int CodecType
			{
				get
				{
					return codecType;
				}
				set
				{
					this.codecType = value;
					if (value == PSP_CODEC_AT3)
					{
						maxSamples = Atrac3Decoder.SAMPLES_PER_FRAME;
						startSkippedSamples = 69;
					}
					else if (value == PSP_CODEC_AT3PLUS)
					{
						maxSamples = Atrac3plusDecoder.ATRAC3P_FRAME_SAMPLES;
						startSkippedSamples = 368;
					}
					else
					{
						maxSamples = 0;
						startSkippedSamples = 0;
					}
				}
			}


			public virtual int AtracBitrate
			{
				get
				{
					return info.atracBitrate;
				}
			}

			public virtual int Channels
			{
				get
				{
					return channels;
				}
				set
				{
					this.channels = value;
				}
			}


			public virtual int AtracSampleRate
			{
				get
				{
					return info.atracSampleRate;
				}
			}

			public virtual int AtracEndSample
			{
				get
				{
					return info.atracEndSample;
				}
			}

			public virtual int AtracCurrentSample
			{
				get
				{
					return atracCurrentSample;
				}
				set
				{
					atracCurrentSample = value;
				}
			}

			public virtual int AtracBytesPerFrame
			{
				get
				{
					return info.atracBytesPerFrame;
				}
			}


			public virtual int LoopNum
			{
				get
				{
					if (!hasLoop())
					{
						return 0;
					}
					return info.loopNum;
				}
				set
				{
					info.loopNum = value;
				}
			}


			public virtual int MaxSamples
			{
				get
				{
					return maxSamples;
				}
			}

			public virtual pspFileBuffer InputBuffer
			{
				get
				{
					return inputBuffer;
				}
			}

			public virtual int InputFileSize
			{
				get
				{
					return info.inputFileSize;
				}
				set
				{
					info.inputFileSize = value;
				}
			}


			public virtual int SecondInputFileSize
			{
				get
				{
					return secondInputFileSize;
				}
			}

			public virtual bool SecondBufferNeeded
			{
				get
				{
					return isSecondBufferNeeded;
				}
			}

			public virtual bool SecondBufferSet
			{
				get
				{
					return isSecondBufferSet;
				}
			}

			public virtual int InternalErrorInfo
			{
				get
				{
					return internalErrorInfo;
				}
			}

			public virtual int RemainFrames
			{
				get
				{
					if (inputBuffer == null)
					{
						return 0;
					}
					if (inputBufferContainsAllData())
					{
						return PSP_ATRAC_ALLDATA_IS_ON_MEMORY;
					}
					if ((!hasLoop() || info.loopNum == 0) && inputBuffer.FileWriteSize <= 0)
					{
						return PSP_ATRAC_NONLOOP_STREAM_DATA_IS_ON_MEMORY;
					}
					int remainFrames = inputBuffer.CurrentSize / info.atracBytesPerFrame;
    
					return remainFrames;
				}
			}

			public virtual int getBufferInfoForResetting(int sample, TPointer32 bufferInfoAddr)
			{
				if (sample > AtracEndSample)
				{
					return SceKernelErrors.ERROR_ATRAC_BAD_SAMPLE;
				}

				int writableBytes;
				int minimumWriteBytes;
				int readPosition;
				if (inputBufferContainsAllData())
				{
					writableBytes = 0;
					minimumWriteBytes = 0;
					readPosition = 0;
				}
				else
				{
					writableBytes = inputBuffer.MaxSize;
					minimumWriteBytes = info.atracBytesPerFrame * 2;
					if (sample == 0)
					{
						readPosition = 0;
					}
					else
					{
						readPosition = getFilePositionFromSample(sample);
					}
				}
				// Holds buffer related parameters.
				// Main buffer.
				bufferInfoAddr.setValue(0, inputBuffer.Addr); // Pointer to current writing position in the buffer.
				bufferInfoAddr.setValue(4, writableBytes); // Number of bytes which can be written to the buffer.
				bufferInfoAddr.setValue(8, minimumWriteBytes); // Number of bytes that must to be written to the buffer.
				bufferInfoAddr.setValue(12, readPosition); // Read offset in the input file for the given sample.
				// Secondary buffer.
				bufferInfoAddr.setValue(16, SecondBufferAddr); // Pointer to current writing position in the buffer.
				bufferInfoAddr.setValue(20, SecondBufferSize); // Number of bytes which can be written to the buffer.
				bufferInfoAddr.setValue(24, SecondBufferSize); // Number of bytes that must to be written to the buffer.
				bufferInfoAddr.setValue(28, SecondBufferReadPosition); // Read offset for input file.

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceAtracGetBufferInfoForReseting returning writeAddr=0x{0:X8}, writeMaxSize=0x{1:X}, writeMinSize=0x{2:X}, readPosition=0x{3:X}", bufferInfoAddr.getValue(0), bufferInfoAddr.getValue(4), bufferInfoAddr.getValue(8), bufferInfoAddr.getValue(12)));
				}
				return 0;
			}

			public virtual void setPlayPosition(int sample, int bytesWrittenFirstBuf, int bytesWrittenSecondBuf)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("sceAtracResetPlayPosition: {0}", Utilities.getMemoryDump(inputBuffer.WriteAddr, bytesWrittenFirstBuf)));
				}

				if (sample != atracCurrentSample)
				{
					// Do not change the position of the inputBuffer when it contains all the Atrac data
					if (!inputBufferContainsAllData())
					{
						currentReadPosition = getFilePositionFromSample(sample);
						inputBuffer.reset(bytesWrittenFirstBuf, currentReadPosition);
					}
					else
					{
						currentReadPosition = getFilePositionFromSample(sample);
						inputBuffer.reset(inputBuffer.FilePosition, 0);
						inputBuffer.notifyRead(currentReadPosition);
					}
					AtracCurrentSample = sample;
				}
			}

			internal virtual bool inputBufferContainsAllData()
			{
				if (inputBuffer != null && inputBuffer.MaxSize >= info.inputFileSize)
				{
					if (inputBuffer.ReadSize + currentReadPosition >= info.inputFileSize)
					{
						return true;
					}
				}

				return false;
			}

			internal virtual int getFilePositionFromSample(int sample)
			{
				return info.inputFileDataOffset + sample / maxSamples * info.atracBytesPerFrame;
			}

			public virtual int PlayPosition
			{
				set
				{
					if ((value / maxSamples * maxSamples) != AtracCurrentSample)
					{
						if (inputBufferContainsAllData())
						{
							InputBuffer.reset(inputBuffer.FilePosition, 0);
							InputBuffer.notifyRead(getFilePositionFromSample(value));
						}
						else if (reloadingFromLoopStart && currentLoopNum >= 0 && value == info.loops[currentLoopNum].startSample)
						{
							// We have already started to reload data from the loop start
							reloadingFromLoopStart = false;
						}
						else
						{
							InputBuffer.reset(0, getFilePositionFromSample(value));
						}
						currentReadPosition = getFilePositionFromSample(value);
						AtracCurrentSample = value;
					}
				}
			}

			public virtual bool hasLoop()
			{
				return info.numLoops > 0;
			}

			public virtual int LoopStatus
			{
				get
				{
					if (!hasLoop())
					{
						return PSP_ATRAC_STATUS_NONLOOP_STREAM_DATA;
					}
					return PSP_ATRAC_STATUS_LOOP_STREAM_DATA;
				}
			}

			public virtual int LoopStartSample
			{
				get
				{
					if (!hasLoop())
					{
						return -1;
					}
					return info.loops[0].startSample;
				}
			}

			public virtual int LoopEndSample
			{
				get
				{
					if (!hasLoop())
					{
						return -1;
					}
    
					return info.loops[0].endSample;
				}
			}

			public virtual void setContextDecodeResult(int result, int requestedSize)
			{
				if (Context != null)
				{
					Memory mem = Memory.Instance;
					int contextAddr = Context.addr;
					mem.write32(contextAddr + ATRAC3_CONTEXT_DECODE_RESULT_OFFSET, result);
					int readSize = mem.read32(contextAddr + ATRAC3_CONTEXT_READ_SIZE_OFFSET);
					mem.write32(contextAddr + ATRAC3_CONTEXT_REQUIRED_SIZE_OFFSET, readSize + requestedSize);
				}
			}

			public virtual int SourceBufferLength
			{
				get
				{
					return sourceBufferLength;
				}
				set
				{
					this.sourceBufferLength = value;
				}
			}


			public virtual int LastDecodedSamples
			{
				get
				{
					return lastDecodedSamples;
				}
				set
				{
					this.lastDecodedSamples = value;
				}
			}


			public virtual int OutputChannels
			{
				get
				{
					return outputChannels;
				}
				set
				{
					this.outputChannels = value;
				}
			}


			public virtual int NumberOfSamples
			{
				get
				{
					return codec.NumberOfSamples - skippedSamples - skippedEndSamples;
				}
			}

			public virtual bool InUse
			{
				get
				{
					return inUse;
				}
				set
				{
					this.inUse = value;
    
					if (value)
					{
						initCodec();
					}
					else
					{
						CodecInitialized = false;
						codec = null;
					}
				}
			}


			public virtual void initCodec()
			{
				initCodec(CodecType);
			}

			public override string ToString()
			{
				return string.Format("AtracID[id={0:D}, inputBuffer={1}, channels={2:D}, outputChannels={3:D}]", id, inputBuffer, Channels, OutputChannels);
			}
		}

		protected internal static int read28(Memory mem, int address)
		{
			return ((mem.read8(address + 0) & 0x7F) << 21) | ((mem.read8(address + 1) & 0x7F) << 14) | ((mem.read8(address + 2) & 0x7F) << 7) | ((mem.read8(address + 3) & 0x7F) << 0);
		}

		protected internal static string getStringFromInt32(int n)
		{
			char c1 = (char)((n) & 0xFF);
			char c2 = (char)((n >> 8) & 0xFF);
			char c3 = (char)((n >> 16) & 0xFF);
			char c4 = (char)((n >> 24) & 0xFF);

			return string.Format("{0}{1}{2}{3}", c1, c2, c3, c4);
		}

		protected internal virtual int hleAtracReinit(int numAT3IdCount, int numAT3plusIdCount)
		{
			for (int i = 0; i < atracIDs.Length; i++)
			{
				if (atracIDs[i].InUse)
				{
					return ERROR_BUSY;
				}
			}

			int i;
			for (i = 0; i < numAT3plusIdCount && i < atracIDs.Length; i++)
			{
				atracIDs[i].CodecType = PSP_CODEC_AT3PLUS;
			}
			for (int j = 0; j < numAT3IdCount && i < atracIDs.Length; j++, i++)
			{
				atracIDs[i].CodecType = PSP_CODEC_AT3;
			}
			// The rest is unused
			for (; i < atracIDs.Length; i++)
			{
				atracIDs[i].CodecType = 0;
			}

			return 0;
		}

		public virtual int hleGetAtracID(int codecType)
		{
			for (int i = 0; i < atracIDs.Length; i++)
			{
				if (atracIDs[i].CodecType == codecType && !atracIDs[i].InUse)
				{
					atracIDs[i].InUse = true;
					return i;
				}
			}

			return ERROR_ATRAC_NO_ID;
		}

		public virtual AtracID getAtracID(int atID)
		{
			return atracIDs[atID];
		}

		public static int analyzeRiffFile(Memory mem, int addr, int Length, AtracFileInfo info)
		{
			int result = ERROR_ATRAC_UNKNOWN_FORMAT;

			int currentAddr = addr;
			int bufferSize = Length;
			info.atracEndSample = -1;
			info.numLoops = 0;
			info.inputFileDataOffset = 0;

			if (bufferSize < 12)
			{
				Console.WriteLine(string.Format("Atrac buffer too small {0:D}", bufferSize));
				return ERROR_ATRAC_INVALID_SIZE;
			}

			// RIFF file format:
			// Offset 0: 'RIFF'
			// Offset 4: file Length - 8
			// Offset 8: 'WAVE'
			int magic = readUnaligned32(mem, currentAddr);
			int WAVEMagic = readUnaligned32(mem, currentAddr + 8);
			if (magic != RIFF_MAGIC || WAVEMagic != WAVE_MAGIC)
			{
				Console.WriteLine(string.Format("Not a RIFF/WAVE format! {0}", Utilities.getMemoryDump(currentAddr, 16)));
				return ERROR_ATRAC_UNKNOWN_FORMAT;
			}

			info.inputFileSize = readUnaligned32(mem, currentAddr + 4) + 8;
			info.inputDataSize = info.inputFileSize;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("FileSize 0x{0:X}", info.inputFileSize));
			}
			currentAddr += 12;
			bufferSize -= 12;

			bool foundData = false;
			while (bufferSize >= 8 && !foundData)
			{
				int chunkMagic = readUnaligned32(mem, currentAddr);
				int chunkSize = readUnaligned32(mem, currentAddr + 4);
				currentAddr += 8;
				bufferSize -= 8;

				switch (chunkMagic)
				{
					case DATA_CHUNK_MAGIC:
						foundData = true;
						// Offset of the data chunk in the input file
						info.inputFileDataOffset = currentAddr - addr;
						info.inputDataSize = chunkSize;
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("DATA Chunk: data offset=0x{0:X}, data size=0x{1:X}", info.inputFileDataOffset, info.inputDataSize));
						}
						break;
					case FMT_CHUNK_MAGIC:
					{
						if (chunkSize >= 16)
						{
							int compressionCode = mem.read16(currentAddr);
							info.atracChannels = mem.read16(currentAddr + 2);
							info.atracSampleRate = readUnaligned32(mem, currentAddr + 4);
							info.atracBitrate = readUnaligned32(mem, currentAddr + 8);
							info.atracBytesPerFrame = mem.read16(currentAddr + 12);
							int hiBytesPerSample = mem.read16(currentAddr + 14);
							int extraDataSize = mem.read16(currentAddr + 16);
							if (extraDataSize == 14)
							{
								info.atracCodingMode = mem.read16(currentAddr + 18 + 6);
							}
							//if (log.DebugEnabled)
							{
								Console.WriteLine(string.Format("WAVE format: magic=0x{0:X8}('{1}'), chunkSize={2:D}, compressionCode=0x{3:X4}, channels={4:D}, sampleRate={5:D}, bitrate={6:D}, bytesPerFrame=0x{7:X}, hiBytesPerSample={8:D}, codingMode={9:D}", chunkMagic, getStringFromInt32(chunkMagic), chunkSize, compressionCode, info.atracChannels, info.atracSampleRate, info.atracBitrate, info.atracBytesPerFrame, hiBytesPerSample, info.atracCodingMode));
								// Display rest of chunk as debug information
								StringBuilder restChunk = new StringBuilder();
								for (int i = 16; i < chunkSize; i++)
								{
									int b = mem.read8(currentAddr + i);
									restChunk.Append(string.Format(" {0:X2}", b));
								}
								if (restChunk.Length > 0)
								{
									Console.WriteLine(string.Format("Additional chunk data:{0}", restChunk));
								}
							}

							if (compressionCode == AT3_MAGIC)
							{
								result = PSP_CODEC_AT3;
							}
							else if (compressionCode == AT3_PLUS_MAGIC)
							{
								result = PSP_CODEC_AT3PLUS;
							}
							else
							{
								return ERROR_ATRAC_UNKNOWN_FORMAT;
							}
						}
						break;
					}
					case FACT_CHUNK_MAGIC:
					{
						if (chunkSize >= 8)
						{
							info.atracEndSample = readUnaligned32(mem, currentAddr);
							if (info.atracEndSample > 0)
							{
								info.atracEndSample -= 1;
							}
							if (chunkSize >= 12)
							{
								// Is the value at offset 4 ignored?
								info.atracSampleOffset = readUnaligned32(mem, currentAddr + 8); // The loop samples are offset by this value
							}
							else
							{
								info.atracSampleOffset = readUnaligned32(mem, currentAddr + 4); // The loop samples are offset by this value
							}
							//if (log.DebugEnabled)
							{
								Console.WriteLine(string.Format("FACT Chunk: chunkSize={0:D}, endSample=0x{1:X}, sampleOffset=0x{2:X}", chunkSize, info.atracEndSample, info.atracSampleOffset));
							}
						}
						break;
					}
					case SMPL_CHUNK_MAGIC:
					{
						if (chunkSize >= 36)
						{
							int checkNumLoops = readUnaligned32(mem, currentAddr + 28);
							if (chunkSize >= 36 + checkNumLoops * 24)
							{
								info.numLoops = checkNumLoops;
								info.loops = new LoopInfo[info.numLoops];
								int loopInfoAddr = currentAddr + 36;
								for (int i = 0; i < info.numLoops; i++)
								{
									LoopInfo loop = new LoopInfo();
									info.loops[i] = loop;
									loop.cuePointID = readUnaligned32(mem, loopInfoAddr);
									loop.type = readUnaligned32(mem, loopInfoAddr + 4);
									loop.startSample = readUnaligned32(mem, loopInfoAddr + 8) - info.atracSampleOffset;
									loop.endSample = readUnaligned32(mem, loopInfoAddr + 12) - info.atracSampleOffset;
									loop.fraction = readUnaligned32(mem, loopInfoAddr + 16);
									loop.playCount = readUnaligned32(mem, loopInfoAddr + 20);

									//if (log.DebugEnabled)
									{
										Console.WriteLine(string.Format("Loop #{0:D}: {1}", i, loop.ToString()));
									}
									loopInfoAddr += 24;
								}
								// TODO Second buffer processing disabled because still incomplete
								//isSecondBufferNeeded = true;
							}
						}
						break;
					}
				}

				if (chunkSize > bufferSize)
				{
					break;
				}

				currentAddr += chunkSize;
				bufferSize -= chunkSize;
			}

			if (info.loops != null)
			{
				// If a loop end is past the atrac end, assume the atrac end
				foreach (LoopInfo loop in info.loops)
				{
					if (loop.endSample > info.atracEndSample)
					{
						loop.endSample = info.atracEndSample;
					}
				}
			}

			return result;
		}

		protected internal virtual int hleSetHalfwayBuffer(int atID, TPointer buffer, int readSize, int bufferSize, bool isMonoOutput)
		{
			if (readSize > bufferSize)
			{
				return SceKernelErrors.ERROR_ATRAC_INCORRECT_READ_SIZE;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("hleSetHalfwayBuffer buffer: {0}", Utilities.getMemoryDump(buffer.Address, readSize)));
			}

			AtracFileInfo info = new AtracFileInfo();
			int codecType = analyzeRiffFile(buffer.Memory, buffer.Address, readSize, info);
			if (codecType < 0)
			{
				return codecType;
			}

			AtracID id = atracIDs[atID];
			if (codecType != id.CodecType)
			{
				return SceKernelErrors.ERROR_ATRAC_WRONG_CODEC;
			}

			int result = id.setHalfwayBuffer(buffer.Address, readSize, bufferSize, isMonoOutput, info);
			if (result < 0)
			{
				return result;
			}

			// Reschedule
			Modules.ThreadManForUserModule.hleYieldCurrentThread();

			return result;
		}

		protected internal virtual int hleSetHalfwayBufferAndGetID(TPointer buffer, int readSize, int bufferSize, bool isMonoOutput)
		{
			if (readSize > bufferSize)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleSetHalfwayBufferAndGetID returning 0x{0:X}", ERROR_ATRAC_INCORRECT_READ_SIZE));
				}
				return ERROR_ATRAC_INCORRECT_READ_SIZE;
			}

			// readSize and bufferSize are unsigned int's.
			// Allow negative values.
			// "Tales of VS - ULJS00209" is even passing an uninitialized value bufferSize=0xDEADBEEF

			AtracFileInfo info = new AtracFileInfo();
			int codecType = analyzeRiffFile(buffer.Memory, buffer.Address, readSize, info);
			if (codecType < 0)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleSetHalfwayBufferAndGetID returning 0x{0:X}", codecType));
				}
				return codecType;
			}

			int atID = hleGetAtracID(codecType);
			if (atID < 0)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleSetHalfwayBufferAndGetID returning 0x{0:X}", atID));
				}
				return atID;
			}

			AtracID id = atracIDs[atID];
			int result = id.setHalfwayBuffer(buffer.Address, readSize, bufferSize, isMonoOutput, info);
			if (result < 0)
			{
				hleReleaseAtracID(atID);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleSetHalfwayBufferAndGetID returning 0x{0:X}", result));
				}
				return result;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("hleSetHalfwayBufferAndGetID returning atID=0x{0:X}", atID));
			}

			// Reschedule
			Modules.ThreadManForUserModule.hleYieldCurrentThread();

			return atID;
		}

		protected internal virtual void hleReleaseAtracID(int atracID)
		{
			atracIDs[atracID].release();
		}

		public virtual int checkAtracID(int atID)
		{
			if (atID < 0 || atID >= atracIDs.Length || !atracIDs[atID].InUse)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("checkAtracID invalid atracID=0x{0:X}", atID));
				}
				throw new SceKernelErrorException(ERROR_ATRAC_BAD_ID);
			}

			return atID;
		}


		private static int read24(Memory mem, int address)
		{
			return (mem.read8(address + 0) << 16) | (mem.read8(address + 1) << 8) | (mem.read8(address + 2) << 0);
		}

		private static int read16(Memory mem, int address)
		{
			return (mem.read8(address) << 8) | mem.read8(address + 1);
		}

		private static int analyzeAA3File(TPointer buffer, int fileSize, AtracFileInfo info)
		{
			Memory mem = buffer.Memory;
			int address = buffer.Address;
			int codecType = 0;

			int magic = read24(mem, address);
			address += 3;
			if (magic != 0x656133 && magic != 0x494433)
			{ // 3ae | 3AE
				Console.WriteLine(string.Format("Unknown AA3 magic 0x{0:X6}", magic));
				return codecType;
			}

			if (mem.read8(address) != 3 || mem.read8(address + 1) != 0)
			{
				Console.WriteLine(string.Format("Unknown AA3 bytes 0x{0:X8} 0x{1:X8}", mem.read8(address), mem.read8(address + 1)));
				return ERROR_AA3_INVALID_HEADER_VERSION;
			}
			address += 3;

			int headerSize = read28(mem, address);
			address += 4 + headerSize;
			if (mem.read8(address) == 0)
			{
				address += 16;
			}
			info.inputFileDataOffset = address - buffer.Address;

			magic = read24(mem, address);
			if (magic != 0x454133)
			{ // 3AE
				Console.WriteLine(string.Format("Unknown AA3 magic 0x{0:X6}", magic));
				return ERROR_AA3_INVALID_HEADER;
			}
			address += 4;

			int dataOffset = read16(mem, address);
			if (dataOffset == 0xFFFF)
			{
				return ERROR_AA3_INVALID_HEADER;
			}
			address += 2;

			int unknown2 = read16(mem, address);
			if (unknown2 != 0xFFFF)
			{
				return ERROR_AA3_INVALID_HEADER;
			}
			address += 2;

			address += 24;

			int samplesPerFrame;
			int flags = read16(mem, address + 2);
			switch (mem.read8(address))
			{
				case 0: // AT3
					if ((flags & 0xE000) != 0x2000)
					{ // Number of channels?
						return ERROR_AA3_INVALID_HEADER_FLAGS;
					}
					codecType = PSP_CODEC_AT3;
					samplesPerFrame = Atrac3Decoder.SAMPLES_PER_FRAME;
					info.atracChannels = 2;
					info.atracCodingMode = (mem.read8(address + 1) & 0x2) >> 1;
					info.atracBytesPerFrame = ((flags & 0x3FF) << 3);
					break;
				case 1: // AT3+
					if ((flags & 0x1C00) != 0x0800)
					{
						return ERROR_AA3_INVALID_HEADER_FLAGS;
					}
					if ((flags & 0xE000) != 0x2000)
					{ // Number of channels?
						return ERROR_AA3_INVALID_HEADER_FLAGS;
					}
					codecType = PSP_CODEC_AT3PLUS;
					samplesPerFrame = Atrac3plusDecoder.ATRAC3P_FRAME_SAMPLES;
					info.atracChannels = 2;
					info.atracBytesPerFrame = ((flags & 0x3FF) << 3) + 8;
					break;
				default:
					return ERROR_AA3_INVALID_CODEC;
			}

			info.inputFileDataOffset += dataOffset;
			info.inputFileSize = fileSize;
			info.inputDataSize = fileSize - info.inputFileDataOffset;
			info.atracEndSample = (info.inputDataSize / info.atracBytesPerFrame) * samplesPerFrame;

			return codecType;
		}

		protected internal virtual int hleSetAA3HalfwayBufferAndGetID(TPointer buffer, int readSize, int bufferSize, bool isMonoOutput, int fileSize)
		{
			if (readSize > bufferSize)
			{
				return ERROR_ATRAC_INCORRECT_READ_SIZE;
			}

			// readSize and bufferSize are unsigned int's.
			// Allow negative values.
			// "Tales of VS - ULJS00209" is even passing an uninitialized value bufferSize=0xDEADBEEF

			AtracFileInfo info = new AtracFileInfo();
			int codecType = analyzeAA3File(buffer, fileSize, info);
			if (codecType < 0)
			{
				return codecType;
			}

			int atID = hleGetAtracID(codecType);
			if (atID < 0)
			{
				return atID;
			}

			AtracID id = atracIDs[atID];
			int result = id.setHalfwayBuffer(buffer.Address, readSize, bufferSize, isMonoOutput, info);
			if (result < 0)
			{
				hleReleaseAtracID(atID);
				return result;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("hleSetHalfwayBufferAndGetID returning atID=0x{0:X}", atID));
			}

			// Reschedule
			Modules.ThreadManForUserModule.hleYieldCurrentThread();

			return atID;
		}

		public virtual AtracID getAtracIdFromContext(int atrac3Context)
		{
			for (int i = 0; i < atracIDs.Length; i++)
			{
				AtracID id = atracIDs[i];
				if (id.InUse)
				{
					SysMemInfo context = id.Context;
					if (context != null && context.addr == atrac3Context)
					{
						return id;
					}
				}
			}

			return null;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD1F59FDB, version = 150, checkInsideInterrupt = true) public int sceAtracStartEntry()
		[HLEFunction(nid : 0xD1F59FDB, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracStartEntry()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD5C28CC0, version = 150, checkInsideInterrupt = true) public int sceAtracEndEntry()
		[HLEFunction(nid : 0xD5C28CC0, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracEndEntry()
		{
			return 0;
		}

		[HLEFunction(nid : 0x780F88D1, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracGetAtracID(int codecType)
		{
			int atId = hleGetAtracID(codecType);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracGetAtracID: returning atID=0x{0:X}", atId));
			}

			return atId;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x61EB33F5, version = 150, checkInsideInterrupt = true) public int sceAtracReleaseAtracID(@CheckArgument("checkAtracID") int atID)
		[HLEFunction(nid : 0x61EB33F5, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracReleaseAtracID(int atID)
		{
			hleReleaseAtracID(atID);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0E2A73AB, version = 150, checkInsideInterrupt = true) public int sceAtracSetData(@CheckArgument("checkAtracID") int atID, pspsharp.HLE.TPointer buffer, int bufferSize)
		[HLEFunction(nid : 0x0E2A73AB, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracSetData(int atID, TPointer buffer, int bufferSize)
		{
			return hleSetHalfwayBuffer(atID, buffer, bufferSize, bufferSize, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x3F6E26B5, version = 150, checkInsideInterrupt = true) public int sceAtracSetHalfwayBuffer(@CheckArgument("checkAtracID") int atID, pspsharp.HLE.TPointer halfBuffer, int readSize, int halfBufferSize)
		[HLEFunction(nid : 0x3F6E26B5, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracSetHalfwayBuffer(int atID, TPointer halfBuffer, int readSize, int halfBufferSize)
		{
			return hleSetHalfwayBuffer(atID, halfBuffer, readSize, halfBufferSize, false);
		}

		[HLEFunction(nid : 0x7A20E7AF, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracSetDataAndGetID(TPointer buffer, int bufferSize)
		{
			return hleSetHalfwayBufferAndGetID(buffer, bufferSize, bufferSize, false);
		}

		[HLEFunction(nid : 0x0FAE370E, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracSetHalfwayBufferAndGetID(TPointer halfBuffer, int readSize, int halfBufferSize)
		{
			return hleSetHalfwayBufferAndGetID(halfBuffer, readSize, halfBufferSize, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x6A8C3CD5, version = 150, checkInsideInterrupt = true) public int sceAtracDecodeData(@CheckArgument("checkAtracID") int atID, @CanBeNull pspsharp.HLE.TPointer16 samplesAddr, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 samplesNbrAddr, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 outEndAddr, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 remainFramesAddr)
		[HLEFunction(nid : 0x6A8C3CD5, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracDecodeData(int atID, TPointer16 samplesAddr, TPointer32 samplesNbrAddr, TPointer32 outEndAddr, TPointer32 remainFramesAddr)
		{
			AtracID id = atracIDs[atID];
			if (id.SecondBufferNeeded && !id.SecondBufferSet)
			{
				Console.WriteLine(string.Format("sceAtracDecodeData atracID=0x{0:X} needs second buffer!", atID));
				return SceKernelErrors.ERROR_ATRAC_SECOND_BUFFER_NEEDED;
			}

			int result = id.decodeData(samplesAddr.Address, outEndAddr);
			if (result < 0)
			{
				samplesNbrAddr.setValue(0);
				return result;
			}

			samplesNbrAddr.setValue(id.NumberOfSamples);
			remainFramesAddr.setValue(id.RemainFrames);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracDecodeData returning 0x{0:X8}, samples=0x{1:X}, end={2:D}, remainFrames={3:D}, currentSample=0x{4:X}/0x{5:X}, {6}", result, samplesNbrAddr.getValue(), outEndAddr.getValue(), remainFramesAddr.getValue(), id.AtracCurrentSample, id.AtracEndSample, id));
			}

			// Delay the thread decoding the Atrac data,
			// the thread is also blocking using semaphores/event flags on a real PSP.
			if (result == 0)
			{
				Modules.ThreadManForUserModule.hleKernelDelayThread(atracDecodeDelay, false);
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x9AE849A7, version = 150, checkInsideInterrupt = true) public int sceAtracGetRemainFrame(@CheckArgument("checkAtracID") int atID, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 remainFramesAddr)
		[HLEFunction(nid : 0x9AE849A7, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracGetRemainFrame(int atID, TPointer32 remainFramesAddr)
		{
			AtracID id = atracIDs[atID];
			remainFramesAddr.setValue(id.RemainFrames);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracGetRemainFrame returning {0:D}, {1}", remainFramesAddr.getValue(), id));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x5D268707, version = 150, checkInsideInterrupt = true) public int sceAtracGetStreamDataInfo(@CheckArgument("checkAtracID") int atID, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 writeAddr, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 writableBytesAddr, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 readOffsetAddr)
		[HLEFunction(nid : 0x5D268707, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracGetStreamDataInfo(int atID, TPointer32 writeAddr, TPointer32 writableBytesAddr, TPointer32 readOffsetAddr)
		{
			AtracID id = atracIDs[atID];
			id.getStreamDataInfo(writeAddr, writableBytesAddr, readOffsetAddr);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracGetStreamDataInfo write=0x{0:X8}, writableBytes=0x{1:X}, readOffset=0x{2:X}, {3}", writeAddr.getValue(), writableBytesAddr.getValue(), readOffsetAddr.getValue(), id));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7DB31251, version = 150, checkInsideInterrupt = true) public int sceAtracAddStreamData(@CheckArgument("checkAtracID") int atID, int bytesToAdd)
		[HLEFunction(nid : 0x7DB31251, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracAddStreamData(int atID, int bytesToAdd)
		{
			AtracID id = atracIDs[atID];
			id.addStreamData(bytesToAdd);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracAddStreamData: {0}", id));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x83E85EA0, version = 150, checkInsideInterrupt = true) public int sceAtracGetSecondBufferInfo(@CheckArgument("checkAtracID") int atID, pspsharp.HLE.TPointer32 outPosition, pspsharp.HLE.TPointer32 outBytes)
		[HLEFunction(nid : 0x83E85EA0, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracGetSecondBufferInfo(int atID, TPointer32 outPosition, TPointer32 outBytes)
		{
			// Checked: outPosition and outBytes have to be non-NULL
			AtracID id = atracIDs[atID];
			if (!id.SecondBufferNeeded)
			{
				// PSP clears both values when returning this error code.
				outPosition.setValue(0);
				outBytes.setValue(0);
				return SceKernelErrors.ERROR_ATRAC_SECOND_BUFFER_NOT_NEEDED;
			}

			outPosition.setValue(id.SecondBufferReadPosition);
			outBytes.setValue(id.SecondBufferSize);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x83BF7AFD, version = 150, checkInsideInterrupt = true) public int sceAtracSetSecondBuffer(@CheckArgument("checkAtracID") int atID, pspsharp.HLE.TPointer secondBuffer, int secondBufferSize)
		[HLEFunction(nid : 0x83BF7AFD, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracSetSecondBuffer(int atID, TPointer secondBuffer, int secondBufferSize)
		{
			AtracID id = atracIDs[atID];
			id.setSecondBuffer(secondBuffer.Address, secondBufferSize);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE23E3A35, version = 150, checkInsideInterrupt = true) public int sceAtracGetNextDecodePosition(@CheckArgument("checkAtracID") int atID, pspsharp.HLE.TPointer32 posAddr)
		[HLEFunction(nid : 0xE23E3A35, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracGetNextDecodePosition(int atID, TPointer32 posAddr)
		{
			AtracID id = atracIDs[atID];
			if (id.AtracCurrentSample >= id.AtracEndSample)
			{
				return SceKernelErrors.ERROR_ATRAC_ALL_DATA_DECODED;
			}

			posAddr.setValue(id.AtracCurrentSample);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracGetNextDecodePosition returning pos={0:D}", posAddr.getValue()));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA2BBA8BE, version = 150, checkInsideInterrupt = true) public int sceAtracGetSoundSample(@CheckArgument("checkAtracID") int atID, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 endSampleAddr, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 loopStartSampleAddr, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 loopEndSampleAddr)
		[HLEFunction(nid : 0xA2BBA8BE, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracGetSoundSample(int atID, TPointer32 endSampleAddr, TPointer32 loopStartSampleAddr, TPointer32 loopEndSampleAddr)
		{
			AtracID id = atracIDs[atID];
			int endSample = id.AtracEndSample;
			int loopStartSample = id.LoopStartSample;
			int loopEndSample = id.LoopEndSample;
			if (endSample < 0)
			{
				endSample = id.AtracEndSample;
			}
			if (endSample < 0)
			{
				endSample = id.InputFileSize;
			}
			endSampleAddr.setValue(endSample);
			loopStartSampleAddr.setValue(loopStartSample);
			loopEndSampleAddr.setValue(loopEndSample);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracGetSoundSample returning endSample=0x{0:X}, loopStartSample=0x{1:X}, loopEndSample=0x{2:X}", endSample, loopStartSample, loopEndSample));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x31668BAA, version = 150, checkInsideInterrupt = true) public int sceAtracGetChannel(@CheckArgument("checkAtracID") int atID, pspsharp.HLE.TPointer32 channelAddr)
		[HLEFunction(nid : 0x31668BAA, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracGetChannel(int atID, TPointer32 channelAddr)
		{
			AtracID id = atracIDs[atID];
			channelAddr.setValue(id.Channels);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD6A5F2F7, version = 150, checkInsideInterrupt = true) public int sceAtracGetMaxSample(@CheckArgument("checkAtracID") int atID, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 maxSamplesAddr)
		[HLEFunction(nid : 0xD6A5F2F7, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracGetMaxSample(int atID, TPointer32 maxSamplesAddr)
		{
			AtracID id = atracIDs[atID];
			maxSamplesAddr.setValue(id.MaxSamples);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracGetMaxSample returning maxSamples=0x{0:X}", id.MaxSamples));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x36FAABFB, version = 150, checkInsideInterrupt = true) public int sceAtracGetNextSample(@CheckArgument("checkAtracID") int atID, pspsharp.HLE.TPointer32 nbrSamplesAddr)
		[HLEFunction(nid : 0x36FAABFB, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracGetNextSample(int atID, TPointer32 nbrSamplesAddr)
		{
			AtracID id = atracIDs[atID];
			int samples = id.MaxSamples;
			if (id.InputBuffer.Empty)
			{
				samples = 0; // No more data available in input buffer
			}
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracGetNextSample returning {0:D} samples", samples));
			}
			nbrSamplesAddr.setValue(samples);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA554A158, version = 150, checkInsideInterrupt = true) public int sceAtracGetBitrate(@CheckArgument("checkAtracID") int atID, pspsharp.HLE.TPointer32 bitrateAddr)
		[HLEFunction(nid : 0xA554A158, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracGetBitrate(int atID, TPointer32 bitrateAddr)
		{
			AtracID id = atracIDs[atID];

			// Bitrate based on https://github.com/uofw/uofw/blob/master/src/libatrac3plus/libatrac3plus.c
			int bitrate = (id.AtracBytesPerFrame * 352800) / 1000;
			if (id.CodecType == PSP_CODEC_AT3PLUS)
			{
				bitrate = ((bitrate >> 11) + 8) & unchecked((int)0xFFFFFFF0);
			}
			else
			{
				bitrate = (bitrate + 511) >> 10;
			}

			bitrateAddr.setValue(bitrate);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracGetBitrate returning bitRate=0x{0:X}", bitrate));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xFAA4F89B, version = 150, checkInsideInterrupt = true) public int sceAtracGetLoopStatus(@CheckArgument("checkAtracID") int atID, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 loopNbr, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 statusAddr)
		[HLEFunction(nid : 0xFAA4F89B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracGetLoopStatus(int atID, TPointer32 loopNbr, TPointer32 statusAddr)
		{
			AtracID id = atracIDs[atID];
			loopNbr.setValue(id.LoopNum);
			statusAddr.setValue(id.LoopStatus);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x868120B5, version = 150, checkInsideInterrupt = true) public int sceAtracSetLoopNum(@CheckArgument("checkAtracID") int atID, int loopNbr)
		[HLEFunction(nid : 0x868120B5, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracSetLoopNum(int atID, int loopNbr)
		{
			AtracID id = atracIDs[atID];
			if (!id.hasLoop())
			{
				return SceKernelErrors.ERROR_ATRAC_NO_LOOP_INFORMATION;
			}
			id.LoopNum = loopNbr;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLELogging(level = "info") @HLEFunction(nid = 0xCA3CA3D2, version = 150, checkInsideInterrupt = true) public int sceAtracGetBufferInfoForReseting(@CheckArgument("checkAtracID") int atID, int sample, pspsharp.HLE.TPointer32 bufferInfoAddr)
		[HLELogging(level : "info"), HLEFunction(nid : 0xCA3CA3D2, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracGetBufferInfoForReseting(int atID, int sample, TPointer32 bufferInfoAddr)
		{
			AtracID id = atracIDs[atID];
			return id.getBufferInfoForResetting(sample, bufferInfoAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x644E5607, version = 150, checkInsideInterrupt = true) public int sceAtracResetPlayPosition(@CheckArgument("checkAtracID") int atID, int sample, int bytesWrittenFirstBuf, int bytesWrittenSecondBuf)
		[HLEFunction(nid : 0x644E5607, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracResetPlayPosition(int atID, int sample, int bytesWrittenFirstBuf, int bytesWrittenSecondBuf)
		{
			AtracID id = atracIDs[atID];
			id.setPlayPosition(sample, bytesWrittenFirstBuf, bytesWrittenSecondBuf);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE88F759B, version = 150, checkInsideInterrupt = true) public int sceAtracGetInternalErrorInfo(@CheckArgument("checkAtracID") int atID, pspsharp.HLE.TPointer32 errorAddr)
		[HLEFunction(nid : 0xE88F759B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAtracGetInternalErrorInfo(int atID, TPointer32 errorAddr)
		{
			AtracID id = atracIDs[atID];
			errorAddr.setValue(id.InternalErrorInfo);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB3B5D042, version = 250, checkInsideInterrupt = true) public int sceAtracGetOutputChannel(@CheckArgument("checkAtracID") int atID, pspsharp.HLE.TPointer32 outputChannelAddr)
		[HLEFunction(nid : 0xB3B5D042, version : 250, checkInsideInterrupt : true)]
		public virtual int sceAtracGetOutputChannel(int atID, TPointer32 outputChannelAddr)
		{
			AtracID id = atracIDs[atID];
			outputChannelAddr.setValue(id.OutputChannels);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xECA32A99, version = 250, checkInsideInterrupt = true) public boolean sceAtracIsSecondBufferNeeded(@CheckArgument("checkAtracID") int atID)
		[HLEFunction(nid : 0xECA32A99, version : 250, checkInsideInterrupt : true)]
		public virtual bool sceAtracIsSecondBufferNeeded(int atID)
		{
			AtracID id = atracIDs[atID];
			// 0 -> Second buffer isn't needed.
			// 1 -> Second buffer is needed.
			return id.SecondBufferNeeded;
		}

		[HLEFunction(nid : 0x132F1ECA, version : 250, checkInsideInterrupt : true)]
		public virtual int sceAtracReinit(int at3IDNum, int at3plusIDNum)
		{
			int result = 0;

			if (at3IDNum != 0 || at3plusIDNum != 0)
			{
				result = hleAtracReinit(at3IDNum, at3plusIDNum);

				if (result >= 0)
				{
					Modules.ThreadManForUserModule.hleYieldCurrentThread();
				}
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x2DD3E298, version = 250, checkInsideInterrupt = true) public int sceAtracGetBufferInfoForResetting(@CheckArgument("checkAtracID") int atID, int sample, pspsharp.HLE.TPointer32 bufferInfoAddr)
		[HLEFunction(nid : 0x2DD3E298, version : 250, checkInsideInterrupt : true)]
		public virtual int sceAtracGetBufferInfoForResetting(int atID, int sample, TPointer32 bufferInfoAddr)
		{
			AtracID id = atracIDs[atID];
			return id.getBufferInfoForResetting(sample, bufferInfoAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x5CF9D852, version = 250, checkInsideInterrupt = true) public int sceAtracSetMOutHalfwayBuffer(@CheckArgument("checkAtracID") int atID, pspsharp.HLE.TPointer MOutHalfBuffer, int readSize, int MOutHalfBufferSize)
		[HLEFunction(nid : 0x5CF9D852, version : 250, checkInsideInterrupt : true)]
		public virtual int sceAtracSetMOutHalfwayBuffer(int atID, TPointer MOutHalfBuffer, int readSize, int MOutHalfBufferSize)
		{
			return hleSetHalfwayBuffer(atID, MOutHalfBuffer, readSize, MOutHalfBufferSize, true);
		}

		// Not sure if this function does really exist
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF6837A1A, version = 250, checkInsideInterrupt = true) public int sceAtracSetMOutData(@CheckArgument("checkAtracID") int atID, int unknown2, int unknown3, int unknown4, int unknown5, int unknown6)
		[HLEFunction(nid : 0xF6837A1A, version : 250, checkInsideInterrupt : true)]
		public virtual int sceAtracSetMOutData(int atID, int unknown2, int unknown3, int unknown4, int unknown5, int unknown6)
		{
			return 0;
		}

		// Not sure if this function does really exist
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x472E3825, version = 250, checkInsideInterrupt = true) public int sceAtracSetMOutDataAndGetID(int unknown1, int unknown2, int unknown3, int unknown4, int unknown5, int unknown6)
		[HLEFunction(nid : 0x472E3825, version : 250, checkInsideInterrupt : true)]
		public virtual int sceAtracSetMOutDataAndGetID(int unknown1, int unknown2, int unknown3, int unknown4, int unknown5, int unknown6)
		{
			return 0;
		}

		[HLEFunction(nid : 0x9CD7DE03, version : 250, checkInsideInterrupt : true)]
		public virtual int sceAtracSetMOutHalfwayBufferAndGetID(TPointer MOutHalfBuffer, int readSize, int MOutHalfBufferSize)
		{
			return hleSetHalfwayBufferAndGetID(MOutHalfBuffer, readSize, MOutHalfBufferSize, true);
		}

		[HLEFunction(nid : 0x5622B7C1, version : 250, checkInsideInterrupt : true)]
		public virtual int sceAtracSetAA3DataAndGetID(TPointer buffer, int bufferSize, int fileSize, int unused)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracSetAA3DataAndGetID buffer:{0}", Utilities.getMemoryDump(buffer.Address, bufferSize)));
			}
			return hleSetAA3HalfwayBufferAndGetID(buffer, bufferSize, bufferSize, false, fileSize);
		}

		[HLEFunction(nid : 0x5DD66588, version : 250)]
		public virtual int sceAtracSetAA3HalfwayBufferAndGetID(TPointer buffer, int readSize, int bufferSize, int fileSize, int unused)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracSetAA3HalfwayBufferAndGetID buffer:{0}", Utilities.getMemoryDump(buffer.Address, readSize)));
			}
			return hleSetAA3HalfwayBufferAndGetID(buffer, readSize, bufferSize, false, fileSize);
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x231FC6B7, version : 600, checkInsideInterrupt : true)]
		public virtual int _sceAtracGetContextAddress(int atID)
		{
			if (atID < 0 || atID >= atracIDs.Length || !atracIDs[atID].InUse)
			{
				return 0;
			}
			AtracID id = atracIDs[atID];

			id.createContext();
			SysMemInfo atracContext = id.Context;
			if (atracContext == null)
			{
				return 0;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("_sceAtracGetContextAddress returning 0x{0:X8}", atracContext.addr));
			}

			return atracContext.addr;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0C116E1B, version = 620) public int sceAtracLowLevelDecode(@CheckArgument("checkAtracID") int atID, pspsharp.HLE.TPointer sourceAddr, pspsharp.HLE.TPointer32 sourceBytesConsumedAddr, pspsharp.HLE.TPointer samplesAddr, pspsharp.HLE.TPointer32 sampleBytesAddr)
		[HLEFunction(nid : 0x0C116E1B, version : 620)]
		public virtual int sceAtracLowLevelDecode(int atID, TPointer sourceAddr, TPointer32 sourceBytesConsumedAddr, TPointer samplesAddr, TPointer32 sampleBytesAddr)
		{
			AtracID id = atracIDs[atID];
			ICodec codec = id.Codec;

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceAtracLowLevelDecode input:{0}", Utilities.getMemoryDump(sourceAddr.Address, id.SourceBufferLength)));
			}

			int sourceBytesConsumed = 0;
			int bytesPerSample = id.OutputChannels << 1;
			int result = codec.decode(sourceAddr.Address, id.SourceBufferLength, samplesAddr.Address);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracLowLevelDecode codec returned 0x{0:X8}", result));
			}
			if (result < 0)
			{
				log.info(string.Format("sceAtracLowLevelDecode codec returning 0x{0:X8}", result));
				return result;
			}
			sourceBytesConsumed = result > 0 ? id.SourceBufferLength : 0;
			sampleBytesAddr.setValue(codec.NumberOfSamples * bytesPerSample);

			// Consume a part of the Atrac3 source buffer
			sourceBytesConsumedAddr.setValue(sourceBytesConsumed);

			Modules.ThreadManForUserModule.hleKernelDelayThread(atracDecodeDelay, false);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLELogging(level="info") @HLEFunction(nid = 0x1575D64B, version = 620) public int sceAtracLowLevelInitDecoder(@CheckArgument("checkAtracID") int atID, pspsharp.HLE.TPointer32 paramsAddr)
		[HLELogging(level:"info"), HLEFunction(nid : 0x1575D64B, version : 620)]
		public virtual int sceAtracLowLevelInitDecoder(int atID, TPointer32 paramsAddr)
		{
			int numberOfChannels = paramsAddr.getValue(0);
			int outputChannels = paramsAddr.getValue(4);
			int sourceBufferLength = paramsAddr.getValue(8);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAtracLowLevelInitDecoder values at {0}: numberOfChannels={1:D}, outputChannels={2:D}, sourceBufferLength=0x{3:X8}", paramsAddr, numberOfChannels, outputChannels, sourceBufferLength));
			}

			AtracID id = atracIDs[atID];

			int result = 0;

			id.Channels = numberOfChannels;
			id.OutputChannels = outputChannels;
			id.SourceBufferLength = sourceBufferLength;

			// TODO How to find out the codingMode for AT3 audio? Assume STEREO, not JOINT_STEREO
			result = id.Codec.init(sourceBufferLength, numberOfChannels, outputChannels, 0);
			id.setCodecInitialized();

			return result;
		}
	}
}