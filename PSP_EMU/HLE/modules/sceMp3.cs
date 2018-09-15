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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_MP3_DECODING_ERROR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_MP3_ID_NOT_RESERVED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_MP3_INVALID_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceAudiocodec.PSP_CODEC_MP3;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.endianSwap32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUnaligned16;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUnaligned32;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using pspFileBuffer = pspsharp.HLE.kernel.types.pspFileBuffer;
	using AudiocodecInfo = pspsharp.HLE.modules.sceAudiocodec.AudiocodecInfo;
	using Mp3Decoder = pspsharp.media.codec.mp3.Mp3Decoder;
	using Mp3Header = pspsharp.media.codec.mp3.Mp3Header;
	using Utilities = pspsharp.util.Utilities;

	//using Logger = org.apache.log4j.Logger;

	public class sceMp3 : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceMp3");
		private Mp3Info[] ids;
		private const int ID3 = 0x00334449; // "ID3"
		private const int TAG_Xing = 0x676E6958; // "Xing"
		private const int TAG_Info = 0x6F666E49; // "Info"
		private const int TAG_VBRI = 0x49524256; // "VBRI"
		private static readonly int[][] infoTagOffsets = new int[][]
		{
			new int[] {17, 32},
			new int[] {9, 17}
		};
		private const int mp3DecodeDelay = 4000; // Microseconds
		private const int maxSamplesBytesStereo = 0x1200;

		public override void start()
		{
			ids = new Mp3Info[2];
			for (int i = 0; i < ids.Length; i++)
			{
				ids[i] = new Mp3Info(i);
			}

			base.start();
		}

		public virtual int checkId(int id)
		{
			if (ids == null || ids.Length == 0)
			{
				throw new SceKernelErrorException(ERROR_MP3_INVALID_ID);
			}
			if (id < 0 || id >= ids.Length)
			{
				throw new SceKernelErrorException(ERROR_MP3_INVALID_ID);
			}

			return id;
		}

		public virtual int checkInitId(int id)
		{
			id = checkId(id);
			if (!ids[id].Reserved)
			{
				throw new SceKernelErrorException(ERROR_MP3_ID_NOT_RESERVED);
			}

			return id;
		}

		public virtual Mp3Info getMp3Info(int id)
		{
			return ids[id];
		}

		public static bool isMp3Magic(int magic)
		{
			return (magic & 0xE0FF) == 0xE0FF;
		}

		public class Mp3Info : AudiocodecInfo
		{
			//
			// The Buffer layout is the following:
			// - mp3BufSize: maximum buffer size, cannot be changed
			// - mp3InputBufSize: the number of bytes available for reading
			// - mp3InputFileReadPos: the index of the first byte available for reading
			// - mp3InputBufWritePos: the index of the first byte available for writing
			//                          (i.e. the index of the first byte after the last byte
			//                           available for reading)
			// The buffer is cyclic, i.e. the byte following the last byte is the first byte.
			// The following conditions are always true:
			// - 0 <= mp3InputFileReadPos < mp3BufSize
			// - 0 <= mp3InputBufWritePos < mp3BufSize
			// - mp3InputFileReadPos + mp3InputBufSize == mp3InputBufWritePos
			//   or (for cyclic buffer)
			//   mp3InputFileReadPos + mp3InputBufSize == mp3InputBufWritePos + mp3BufSize
			//
			// For example:
			//   [................R..........W.......]
			//                    |          +-> mp3InputBufWritePos
			//                    +-> mp3InputFileReadPos
			//                    <----------> mp3InputBufSize
			//   <-----------------------------------> mp3BufSize
			//
			//   mp3BufSize = 8192
			//   mp3InputFileReadPos = 4096
			//   mp3InputBufWritePos = 6144
			//   mp3InputBufSize = 2048
			//
			// MP3 Frame Header (4 bytes):
			// - Bits 31 to 21: Frame sync (all 1);
			// - Bits 20 to 19: MPEG Audio version;
			// - Bits 18 and 17: Layer;
			// - Bit 16: Protection bit;
			// - Bits 15 to 12: Bitrate;
			// - Bits 11 and 10: Sample rate;
			// - Bit 9: Padding;
			// - Bit 8: Reserved;
			// - Bits 7 and 6: Channels;
			// - Bits 5 and 4: Channel extension;
			// - Bit 3: Copyrigth;
			// - Bit 2: Original;
			// - Bits 1 and 0: Emphasis.
			//
			// NOTE: sceMp3 is only capable of handling MPEG Version 1 Layer III data.
			//

			// The PSP is always reserving this size at the beginning of the input buffer
			internal const int reservedBufferSize = 0x5C0;
			internal const int minimumInputBufferSize = reservedBufferSize;
			internal bool reserved;
			internal pspFileBuffer inputBuffer;
			internal int bufferAddr;
			internal int outputAddr;
			internal int outputSize;
			internal int sumDecodedSamples;
			internal int halfBufferSize;
			internal int outputIndex;
			internal int loopNum;
			internal int startPos;
			internal long endPos;
			internal int sampleRate;
			internal int bitRate;
			internal int maxSamples;
			internal int channels;
			internal int version;
			internal int numberOfFrames;

			public Mp3Info(int id) : base(id)
			{
			}

			public virtual bool Reserved
			{
				get
				{
					return reserved;
				}
			}

			public virtual void reserve(int bufferAddr, int bufferSize, int outputAddr, int outputSize, long startPos, long endPos)
			{
				reserved = true;
				this.bufferAddr = bufferAddr;
				this.outputAddr = outputAddr;
				this.outputSize = outputSize;
				this.startPos = (int) startPos;
				this.endPos = endPos;
				inputBuffer = new pspFileBuffer(bufferAddr + reservedBufferSize, bufferSize - reservedBufferSize, 0, this.startPos);
				inputBuffer.FileMaxSize = (int) endPos;
				loopNum = -1; // Looping indefinitely by default
				initCodec();

				halfBufferSize = (bufferSize - reservedBufferSize) >> 1;
			}

			public override void release()
			{
				base.release();
				reserved = false;
			}

			public virtual void initCodec()
			{
				initCodec(PSP_CODEC_MP3);
			}

			public virtual int notifyAddStream(int bytesToAdd)
			{
				bytesToAdd = System.Math.Min(bytesToAdd, WritableBytes);

				if (log.TraceEnabled)
				{
					log.trace(string.Format("notifyAddStream inputBuffer {0}: {1}", inputBuffer, Utilities.getMemoryDump(inputBuffer.WriteAddr, bytesToAdd)));
				}

				inputBuffer.notifyWrite(bytesToAdd);

				return 0;
			}

			public virtual pspFileBuffer InputBuffer
			{
				get
				{
					return inputBuffer;
				}
			}

			public virtual bool StreamDataNeeded
			{
				get
				{
					bool isDataNeeded;
					if (inputBuffer.FileEnd)
					{
						isDataNeeded = false;
					}
					else
					{
						isDataNeeded = WritableBytes > 0;
					}
    
					return isDataNeeded;
				}
			}

			public virtual int SumDecodedSamples
			{
				get
				{
					return sumDecodedSamples;
				}
			}

			public virtual int decode(TPointer32 outputBufferAddress)
			{
				int result;
				int decodeOutputAddr = outputAddr + outputIndex;
				if (inputBuffer.FileEnd && inputBuffer.CurrentSize <= 0)
				{
					int outputBytes = codec.NumberOfSamples * 4;
					Memory mem = Memory.Instance;
					mem.memset(decodeOutputAddr, (sbyte) 0, outputBytes);
					result = outputBytes;
				}
				else
				{
					int decodeInputAddr = inputBuffer.ReadAddr;
					int decodeInputLength = inputBuffer.ReadSize;

					// Reaching the end of the input buffer (wrapping to its beginning)?
					if (decodeInputLength < minimumInputBufferSize && decodeInputLength < inputBuffer.CurrentSize)
					{
						// Concatenate the input into a temporary buffer
						Memory mem = Memory.Instance;
						mem.memcpy(bufferAddr, decodeInputAddr, decodeInputLength);
						int wrapLength = System.Math.Min(inputBuffer.CurrentSize, minimumInputBufferSize) - decodeInputLength;
						mem.memcpy(bufferAddr + decodeInputLength, inputBuffer.Addr, wrapLength);

						decodeInputAddr = bufferAddr;
						decodeInputLength += wrapLength;
					}

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Decoding from 0x{0:X8}, Length=0x{1:X} to 0x{2:X8}, inputBuffer {3}", decodeInputAddr, decodeInputLength, decodeOutputAddr, inputBuffer));
					}

					result = codec.decode(decodeInputAddr, decodeInputLength, decodeOutputAddr);

					if (result < 0)
					{
						result = ERROR_MP3_DECODING_ERROR;
					}
					else
					{
						int readSize = result;
						int samples = codec.NumberOfSamples;
						int outputBytes = samples * outputChannels * 2;

						inputBuffer.notifyRead(readSize);

						sumDecodedSamples += samples;

						// Update index in output buffer for next decode()
						outputIndex += outputBytes;
						if (outputIndex + outputBytes > outputSize)
						{
							// No space enough to store the same amount of output bytes,
							// reset to beginning of output buffer
							outputIndex = 0;
						}

						result = outputBytes;
					}

					if (inputBuffer.FileEnd && loopNum != 0)
					{
						if (inputBuffer.CurrentSize < minimumInputBufferSize || (inputBuffer.FilePosition - inputBuffer.CurrentSize) > endPos)
						{
							//if (log.DebugEnabled)
							{
								Console.WriteLine(string.Format("Looping loopNum={0:D}", loopNum));
							}

							if (loopNum > 0)
							{
								loopNum--;
							}

							resetPlayPosition(0);
						}
					}
				}

				outputBufferAddress.setValue(decodeOutputAddr);

				return result;
			}

			public virtual int WritableBytes
			{
				get
				{
					int writeSize = inputBuffer.NoFileWriteSize;
    
					// Never return more than halfBufferSize (tested on PSP using JpcspTrace),
					// even when 2*halfBufferSize would be free.
					if (writeSize >= halfBufferSize)
					{
						return halfBufferSize;
					}
    
					return 0;
				}
			}

			public virtual int LoopNum
			{
				get
				{
					return loopNum;
				}
				set
				{
					this.loopNum = value;
				}
			}


			public virtual int resetPlayPosition(int position)
			{
				inputBuffer.reset(0, startPos);
				sumDecodedSamples = 0;

				return 0;
			}

			internal virtual void parseMp3FrameHeader()
			{
				Memory mem = Memory.Instance;
				int startAddr = inputBuffer.Addr;
				int headerAddr = startAddr;
				int header = readUnaligned32(mem, headerAddr);

				// Skip the ID3 tags
				if ((header & 0x00FFFFFF) == ID3)
				{
					int size = endianSwap32(readUnaligned32(mem, startAddr + 6));
					// Highest bit of each byte has to be ignored (format: 0x7F7F7F7F)
					size = (size & 0x7F) | ((size & 0x7F00) >> 1) | ((size & 0x7F0000) >> 2) | ((size & 0x7F000000) >> 3);
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Skipping ID3 of size 0x{0:X}", size));
					}
					inputBuffer.notifyRead(10 + size);
					headerAddr = startAddr + 10 + size;
					header = readUnaligned32(mem, headerAddr);
				}

				if (!isMp3Magic(header))
				{
					Console.WriteLine(string.Format("Invalid MP3 header 0x{0:X8}", header));
					return;
				}

				header = Utilities.endianSwap32(header);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Mp3 header: 0x{0:X8}", header));
				}

				Mp3Header mp3Header = new Mp3Header();
				Mp3Decoder.decodeHeader(mp3Header, header);
				version = mp3Header.version;
				channels = mp3Header.nbChannels;
				sampleRate = mp3Header.sampleRate;
				bitRate = mp3Header.bitRate;
				maxSamples = mp3Header.maxSamples;

				parseInfoTag(headerAddr + 4 + infoTagOffsets[mp3Header.lsf][mp3Header.nbChannels - 1]);
				parseVbriTag(headerAddr + 4 + 32);
			}

			internal virtual void parseInfoTag(int addr)
			{
				Memory mem = Memory.Instance;
				int tag = readUnaligned32(mem, addr);
				if (tag == TAG_Xing || tag == TAG_Info)
				{
					int numberOfBytes = 0;
					int flags = endianSwap32(readUnaligned32(mem, addr + 4));
					addr += 8;
					if ((flags & 0x1) != 0)
					{
						numberOfFrames = endianSwap32(readUnaligned32(mem, addr));
						addr += 4;
					}
					if ((flags & 0x2) != 0)
					{
						numberOfBytes = endianSwap32(readUnaligned32(mem, addr));
						addr += 4;
					}

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Found TAG 0x{0:X8}, numberOfFrames={1:D}, numberOfBytes=0x{2:X}", tag, numberOfFrames, numberOfBytes));
					}
				}
			}

			internal virtual void parseVbriTag(int addr)
			{
				Memory mem = Memory.Instance;
				int tag = readUnaligned32(mem, addr);
				if (tag == TAG_VBRI)
				{
					int version = readUnaligned16(mem, addr + 4);
					if (version == 1)
					{
						int numberOfBytes = endianSwap32(readUnaligned32(mem, addr + 10));
						numberOfFrames = endianSwap32(readUnaligned32(mem, addr + 14));

						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Found TAG 0x{0:X8}, numberOfFrames={1:D}, numberOfBytes=0x{2:X}", tag, numberOfFrames, numberOfBytes));
						}
					}
				}
			}

			public virtual void init()
			{
				parseMp3FrameHeader();

				codec.init(0, channels, outputChannels, 0);

				sumDecodedSamples = 0;
			}

			public virtual int ChannelNum
			{
				get
				{
					return channels;
				}
			}

			public virtual int SampleRate
			{
				get
				{
					return sampleRate;
				}
			}

			public virtual int BitRate
			{
				get
				{
					return bitRate;
				}
			}

			public virtual int MaxSamples
			{
				get
				{
					return maxSamples;
				}
			}

			public virtual int Version
			{
				get
				{
					return version;
				}
			}

			public virtual int NumberOfFrames
			{
				get
				{
					return numberOfFrames;
				}
			}
		}

		public virtual int FreeMp3Id
		{
			get
			{
				int id = -1;
				for (int i = 0; i < ids.Length; i++)
				{
					if (!ids[i].Reserved)
					{
						id = i;
						break;
					}
				}
				if (id < 0)
				{
					return -1;
				}
    
				return id;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x07EC321A, version = 150, checkInsideInterrupt = true) public int sceMp3ReserveMp3Handle(@CanBeNull pspsharp.HLE.TPointer parameters)
		[HLEFunction(nid : 0x07EC321A, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3ReserveMp3Handle(TPointer parameters)
		{
			long startPos = 0;
			long endPos = 0;
			int bufferAddr = 0;
			int bufferSize = 0;
			int outputAddr = 0;
			int outputSize = 0;
			if (parameters.NotNull)
			{
				startPos = parameters.getValue64(0); // Audio data frame start position.
				endPos = parameters.getValue64(8); // Audio data frame end position.
				bufferAddr = parameters.getValue32(16); // Input AAC data buffer.
				bufferSize = parameters.getValue32(20); // Input AAC data buffer size.
				outputAddr = parameters.getValue32(24); // Output PCM data buffer.
				outputSize = parameters.getValue32(28); // Output PCM data buffer size.

				if (bufferAddr == 0 || outputAddr == 0)
				{
					return SceKernelErrors.ERROR_MP3_INVALID_ADDRESS;
				}
				if (startPos < 0 || startPos > endPos)
				{
					return SceKernelErrors.ERROR_MP3_INVALID_PARAMETER;
				}
				if (bufferSize < 8192 || outputSize < maxSamplesBytesStereo * 2)
				{
					return SceKernelErrors.ERROR_MP3_INVALID_PARAMETER;
				}
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceMp3ReserveMp3Handle parameters: startPos=0x{0:X}, endPos=0x{1:X}, " + "bufferAddr=0x{2:X8}, bufferSize=0x{3:X}, outputAddr=0x{4:X8}, outputSize=0x{5:X}", startPos, endPos, bufferAddr, bufferSize, outputAddr, outputSize));
			}

			int id = FreeMp3Id;
			if (id < 0)
			{
				return id;
			}

			ids[id].reserve(bufferAddr, bufferSize, outputAddr, outputSize, startPos, endPos);

			return id;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0DB149F4, version = 150, checkInsideInterrupt = true) public int sceMp3NotifyAddStreamData(@CheckArgument("checkInitId") int id, int bytesToAdd)
		[HLEFunction(nid : 0x0DB149F4, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3NotifyAddStreamData(int id, int bytesToAdd)
		{
			return getMp3Info(id).notifyAddStream(bytesToAdd);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x2A368661, version = 150, checkInsideInterrupt = true) public int sceMp3ResetPlayPosition(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0x2A368661, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3ResetPlayPosition(int id)
		{
			return getMp3Info(id).resetPlayPosition(0);
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x35750070, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3InitResource()
		{
			return 0;
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x3C2FA058, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3TermResource()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x3CEF484F, version = 150, checkInsideInterrupt = true) public int sceMp3SetLoopNum(@CheckArgument("checkInitId") int id, int loopNum)
		[HLEFunction(nid : 0x3CEF484F, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3SetLoopNum(int id, int loopNum)
		{
			getMp3Info(id).LoopNum = loopNum;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x44E07129, version = 150, checkInsideInterrupt = true) public int sceMp3Init(@CheckArgument("checkId") int id)
		[HLEFunction(nid : 0x44E07129, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3Init(int id)
		{
			Mp3Info mp3Info = getMp3Info(id);
			mp3Info.init();
			if (log.InfoEnabled)
			{
				log.info(string.Format("Initializing Mp3 data: channels={0:D}, samplerate={1:D}kHz, bitrate={2:D}kbps.", mp3Info.ChannelNum, mp3Info.SampleRate, mp3Info.BitRate));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7F696782, version = 150, checkInsideInterrupt = true) public int sceMp3GetMp3ChannelNum(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0x7F696782, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3GetMp3ChannelNum(int id)
		{
			return getMp3Info(id).ChannelNum;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8F450998, version = 150, checkInsideInterrupt = true) public int sceMp3GetSamplingRate(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0x8F450998, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3GetSamplingRate(int id)
		{
			Mp3Info mp3Info = getMp3Info(id);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceMp3GetSamplingRate returning 0x{0:X}", mp3Info.SampleRate));
			}
			return mp3Info.SampleRate;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA703FE0F, version = 150, checkInsideInterrupt = true) public int sceMp3GetInfoToAddStreamData(@CheckArgument("checkInitId") int id, @CanBeNull pspsharp.HLE.TPointer32 writeAddr, @CanBeNull pspsharp.HLE.TPointer32 writableBytesAddr, @CanBeNull pspsharp.HLE.TPointer32 readOffsetAddr)
		[HLEFunction(nid : 0xA703FE0F, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3GetInfoToAddStreamData(int id, TPointer32 writeAddr, TPointer32 writableBytesAddr, TPointer32 readOffsetAddr)
		{
			Mp3Info info = getMp3Info(id);
			writeAddr.setValue(info.InputBuffer.WriteAddr);
			writableBytesAddr.setValue(info.WritableBytes);
			readOffsetAddr.setValue(info.InputBuffer.FilePosition);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceMp3GetInfoToAddStreamData returning writeAddr=0x{0:X8}, writableBytes=0x{1:X}, readOffset=0x{2:X}", writeAddr.getValue(), writableBytesAddr.getValue(), readOffsetAddr.getValue()));
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD021C0FB, version = 150, checkInsideInterrupt = true) public int sceMp3Decode(@CheckArgument("checkInitId") int id, pspsharp.HLE.TPointer32 bufferAddress)
		[HLEFunction(nid : 0xD021C0FB, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3Decode(int id, TPointer32 bufferAddress)
		{
			int result = getMp3Info(id).decode(bufferAddress);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceMp3Decode bufferAddress={0}(0x{1:X8}) returning 0x{2:X}", bufferAddress, bufferAddress.getValue(), result));
			}

			if (result >= 0)
			{
				Modules.ThreadManForUserModule.hleKernelDelayThread(mp3DecodeDelay, false);
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD0A56296, version = 150, checkInsideInterrupt = true) public boolean sceMp3CheckStreamDataNeeded(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0xD0A56296, version : 150, checkInsideInterrupt : true)]
		public virtual bool sceMp3CheckStreamDataNeeded(int id)
		{
			return getMp3Info(id).StreamDataNeeded;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF5478233, version = 150, checkInsideInterrupt = true) public int sceMp3ReleaseMp3Handle(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0xF5478233, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3ReleaseMp3Handle(int id)
		{
			getMp3Info(id).release();

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x354D27EA, version = 150) public int sceMp3GetSumDecodedSample(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0x354D27EA, version : 150)]
		public virtual int sceMp3GetSumDecodedSample(int id)
		{
			int sumDecodedSamples = getMp3Info(id).SumDecodedSamples;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceMp3GetSumDecodedSample returning 0x{0:X}", sumDecodedSamples));
			}

			return sumDecodedSamples;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x87677E40, version = 150, checkInsideInterrupt = true) public int sceMp3GetBitRate(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0x87677E40, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3GetBitRate(int id)
		{
			return getMp3Info(id).BitRate;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x87C263D1, version = 150, checkInsideInterrupt = true) public int sceMp3GetMaxOutputSample(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0x87C263D1, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3GetMaxOutputSample(int id)
		{
			Mp3Info mp3Info = getMp3Info(id);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceMp3GetMaxOutputSample returning 0x{0:X}", mp3Info.MaxSamples));
			}
			return mp3Info.MaxSamples;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD8F54A51, version = 150, checkInsideInterrupt = true) public int sceMp3GetLoopNum(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0xD8F54A51, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3GetLoopNum(int id)
		{
			return getMp3Info(id).LoopNum;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x3548AEC8, version = 150) public int sceMp3GetFrameNum(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0x3548AEC8, version : 150)]
		public virtual int sceMp3GetFrameNum(int id)
		{
			return getMp3Info(id).NumberOfFrames;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xAE6D2027, version = 150) public int sceMp3GetVersion(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0xAE6D2027, version : 150)]
		public virtual int sceMp3GetVersion(int id)
		{
			return getMp3Info(id).Version;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0840E808, version = 150, checkInsideInterrupt = true) public int sceMp3ResetPlayPosition2(@CheckArgument("checkInitId") int id, int position)
		[HLEFunction(nid : 0x0840E808, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp3ResetPlayPosition2(int id, int position)
		{
			return getMp3Info(id).resetPlayPosition(position);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1B839B83, version = 620) public int sceMp3LowLevelInit(@CheckArgument("checkInitId") int id, int unknown)
		[HLEFunction(nid : 0x1B839B83, version : 620)]
		public virtual int sceMp3LowLevelInit(int id, int unknown)
		{
			Mp3Info mp3Info = getMp3Info(id);
			// Always output in stereo, even if the input is mono
			mp3Info.Codec.init(0, 2, 2, 0);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE3EE2C81, version = 620) public int sceMp3LowLevelDecode(@CheckArgument("checkInitId") int id, pspsharp.HLE.TPointer sourceAddr, pspsharp.HLE.TPointer32 sourceBytesConsumedAddr, pspsharp.HLE.TPointer samplesAddr, pspsharp.HLE.TPointer32 sampleBytesAddr)
		[HLEFunction(nid : 0xE3EE2C81, version : 620)]
		public virtual int sceMp3LowLevelDecode(int id, TPointer sourceAddr, TPointer32 sourceBytesConsumedAddr, TPointer samplesAddr, TPointer32 sampleBytesAddr)
		{
			Mp3Info mp3Info = getMp3Info(id);
			int result = mp3Info.Codec.decode(sourceAddr.Address, 10000, samplesAddr.Address);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceMp3LowLevelDecode result=0x{0:X8}, samples=0x{1:X}", result, mp3Info.Codec.NumberOfSamples));
			}
			if (result < 0)
			{
				return SceKernelErrors.ERROR_MP3_LOW_LEVEL_DECODING_ERROR;
			}

			sourceBytesConsumedAddr.setValue(result);
			sampleBytesAddr.setValue(mp3Info.Codec.NumberOfSamples * 4);

			return 0;
		}
	}
}