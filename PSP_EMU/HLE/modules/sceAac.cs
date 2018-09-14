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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_AAC_DECODING_ERROR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_AAC_INVALID_ADDRESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_AAC_INVALID_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_AAC_INVALID_PARAMETER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_AAC_RESOURCE_NOT_INITIALIZED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceAudiocodec.PSP_CODEC_AAC;

	using Logger = org.apache.log4j.Logger;

	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using pspFileBuffer = pspsharp.HLE.kernel.types.pspFileBuffer;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using AudiocodecInfo = pspsharp.HLE.modules.sceAudiocodec.AudiocodecInfo;
	using Utilities = pspsharp.util.Utilities;

	public class sceAac : HLEModule
	{
		public static Logger log = Modules.getLogger("sceAac");
		protected internal SysMemInfo resourceMem;
		protected internal AacInfo[] ids;

		public class AacInfo : AudiocodecInfo
		{
			// The PSP is always reserving this size at the beginning of the input buffer
			internal const int reservedBufferSize = 1600;
			internal const int minimumInputBufferSize = reservedBufferSize;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal bool init_Renamed;
			internal pspFileBuffer inputBuffer;
			internal int bufferAddr;
			internal int outputAddr;
			internal int outputSize;
			internal int sumDecodedSamples;
			internal int halfBufferSize;
			internal int outputIndex;
			internal int loopNum;
			internal int startPos;

			protected internal AacInfo(int id) : base(id)
			{
			}

			public virtual bool Init
			{
				get
				{
					return init_Renamed;
				}
			}

			public virtual void init(int bufferAddr, int bufferSize, int outputAddr, int outputSize, long startPos, long endPos)
			{
				this.bufferAddr = bufferAddr;
				this.outputAddr = outputAddr;
				this.outputSize = outputSize;
				this.startPos = (int) startPos;
				inputBuffer = new pspFileBuffer(bufferAddr + reservedBufferSize, bufferSize - reservedBufferSize, 0, this.startPos);
				inputBuffer.FileMaxSize = (int) endPos;
				loopNum = -1; // Looping indefinitely by default
				initCodec();

				halfBufferSize = (bufferSize - reservedBufferSize) >> 1;
			}

			public virtual void initCodec()
			{
				initCodec(PSP_CODEC_AAC);
				init_Renamed = true;
				codec.init(0, outputChannels, outputChannels, 0);
				setCodecInitialized();
			}

			public override void release()
			{
				base.release();
				init_Renamed = false;
			}

			public virtual int notifyAddStream(int bytesToAdd)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("notifyAddStream: {0}", Utilities.getMemoryDump(inputBuffer.WriteAddr, bytesToAdd)));
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
					int writeSize = inputBuffer.WriteSize;
    
					if (writeSize <= 0)
					{
						return false;
					}
    
					if (writeSize >= halfBufferSize)
					{
						return true;
					}
    
					if (writeSize >= inputBuffer.FileWriteSize)
					{
						return true;
					}
    
					return false;
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
				else if (inputBuffer.CurrentSize <= 0)
				{
					int outputBytes = outputSize >> 1;
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

					if (log.DebugEnabled)
					{
						log.debug(string.Format("Decoding from 0x{0:X8}, length=0x{1:X} to 0x{2:X8}", decodeInputAddr, decodeInputLength, decodeOutputAddr));
					}

					result = codec.decode(decodeInputAddr, decodeInputLength, decodeOutputAddr);

					if (result < 0)
					{
						result = ERROR_AAC_DECODING_ERROR;
					}
					else
					{
						int readSize = result;
						int samples = codec.NumberOfSamples;
						int outputBytes = samples * 4;

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

					if (inputBuffer.CurrentSize < minimumInputBufferSize && inputBuffer.FileEnd && loopNum != 0)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Looping loopNum={0:D}", loopNum));
						}

						if (loopNum > 0)
						{
							loopNum--;
						}

						resetPlayPosition();
					}
				}

				outputBufferAddress.setValue(decodeOutputAddr);

				return result;
			}

			public virtual int WritableBytes
			{
				get
				{
					int writeSize = inputBuffer.WriteSize;
    
					if (writeSize >= 2 * halfBufferSize)
					{
						return 2 * halfBufferSize;
					}
    
					if (writeSize >= halfBufferSize)
					{
						return halfBufferSize;
					}
    
					if (writeSize >= inputBuffer.FileWriteSize)
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


			public virtual int resetPlayPosition()
			{
				inputBuffer.reset(0, startPos);
				sumDecodedSamples = 0;

				return 0;
			}
		}

		public override void start()
		{
			ids = null;

			base.start();
		}

		public virtual int checkId(int id)
		{
			if (ids == null || ids.Length == 0)
			{
				throw new SceKernelErrorException(ERROR_AAC_RESOURCE_NOT_INITIALIZED);
			}
			if (id < 0 || id >= ids.Length)
			{
				throw new SceKernelErrorException(ERROR_AAC_INVALID_ID);
			}

			return id;
		}

		public virtual int checkInitId(int id)
		{
			id = checkId(id);
			if (!ids[id].Init)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_AAC_ID_NOT_INITIALIZED);
			}

			return id;
		}

		public virtual int FreeAacId
		{
			get
			{
				int id = -1;
				for (int i = 0; i < ids.Length; i++)
				{
					if (!ids[i].Init)
					{
						id = i;
						break;
					}
				}
				if (id < 0)
				{
					return SceKernelErrors.ERROR_AAC_NO_MORE_FREE_ID;
				}
    
				return id;
			}
		}

		public virtual AacInfo getAacInfo(int id)
		{
			return ids[id];
		}

		public virtual void hleAacInit(int numberIds)
		{
			ids = new AacInfo[numberIds];
			for (int i = 0; i < numberIds; i++)
			{
				ids[i] = new AacInfo(i);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE0C89ACA, version = 395) public int sceAacInit(@CanBeNull pspsharp.HLE.TPointer parameters, int unknown1, int unknown2, int unknown3)
		[HLEFunction(nid : 0xE0C89ACA, version : 395)]
		public virtual int sceAacInit(TPointer parameters, int unknown1, int unknown2, int unknown3)
		{
			if (parameters.Null)
			{
				return ERROR_AAC_INVALID_ADDRESS;
			}

			long startPos = parameters.getValue64(0); // Audio data frame start position.
			long endPos = parameters.getValue64(8); // Audio data frame end position.
			int bufferAddr = parameters.getValue32(16); // Input AAC data buffer.
			int bufferSize = parameters.getValue32(20); // Input AAC data buffer size.
			int outputAddr = parameters.getValue32(24); // Output PCM data buffer.
			int outputSize = parameters.getValue32(28); // Output PCM data buffer size.
			int freq = parameters.getValue32(32); // Frequency.
			int reserved = parameters.getValue32(36); // Always null.

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceAacInit parameters: startPos=0x{0:X}, endPos=0x{1:X}, " + "bufferAddr=0x{2:X8}, bufferSize=0x{3:X}, outputAddr=0x{4:X8}, outputSize=0x{5:X}, freq={6:D}, reserved=0x{7:X8}", startPos, endPos, bufferAddr, bufferSize, outputAddr, outputSize, freq, reserved));
			}

			if (bufferAddr == 0 || outputAddr == 0)
			{
				return ERROR_AAC_INVALID_ADDRESS;
			}
			if (startPos < 0 || startPos > endPos)
			{
				return ERROR_AAC_INVALID_PARAMETER;
			}
			if (bufferSize < 8192 || outputSize < 8192 || reserved != 0)
			{
				return ERROR_AAC_INVALID_PARAMETER;
			}
			if (freq != 44100 && freq != 32000 && freq != 48000 && freq != 24000)
			{
				return ERROR_AAC_INVALID_PARAMETER;
			}

			int id = FreeAacId;
			if (id < 0)
			{
				return id;
			}

			ids[id].init(bufferAddr, bufferSize, outputAddr, outputSize, startPos, endPos);

			return id;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x33B8C009, version = 395) public int sceAacExit(@CheckArgument("checkId") int id)
		[HLEFunction(nid : 0x33B8C009, version : 395)]
		public virtual int sceAacExit(int id)
		{
			getAacInfo(id).release();

			return 0;
		}

		[HLEFunction(nid : 0x5CFFC57C, version : 395)]
		public virtual int sceAacInitResource(int numberIds)
		{
			int memSize = numberIds * 0x19000;
			resourceMem = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.USER_PARTITION_ID, "SceLibAacResource", SysMemUserForUser.PSP_SMEM_Low, memSize, 0);

			if (resourceMem == null)
			{
				return SceKernelErrors.ERROR_AAC_NOT_ENOUGH_MEMORY;
			}

			Memory.Instance.memset(resourceMem.addr, (sbyte) 0, memSize);

			hleAacInit(numberIds);

			return 0;
		}

		[HLEFunction(nid : 0x23D35CAE, version : 395)]
		public virtual int sceAacTermResource()
		{
			if (resourceMem != null)
			{
				Modules.SysMemUserForUserModule.free(resourceMem);
				resourceMem = null;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7E4CFEE4, version = 395) public int sceAacDecode(@CheckArgument("checkInitId") int id, @CanBeNull pspsharp.HLE.TPointer32 bufferAddress)
		[HLEFunction(nid : 0x7E4CFEE4, version : 395)]
		public virtual int sceAacDecode(int id, TPointer32 bufferAddress)
		{
			int result = getAacInfo(id).decode(bufferAddress);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceAacDecode bufferAddress={0}(0x{1:X8}) returning 0x{2:X}", bufferAddress, bufferAddress.getValue(), result));
			}

			if (result >= 0)
			{
				Modules.ThreadManForUserModule.hleKernelDelayThread(sceAtrac3plus.atracDecodeDelay, false);
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x523347D9, version = 395) public int sceAacGetLoopNum(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0x523347D9, version : 395)]
		public virtual int sceAacGetLoopNum(int id)
		{
			return getAacInfo(id).LoopNum;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xBBDD6403, version = 395) public int sceAacSetLoopNum(@CheckArgument("checkInitId") int id, int loopNum)
		[HLEFunction(nid : 0xBBDD6403, version : 395)]
		public virtual int sceAacSetLoopNum(int id, int loopNum)
		{
			getAacInfo(id).LoopNum = loopNum;
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD7C51541, version = 395) public boolean sceAacCheckStreamDataNeeded(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0xD7C51541, version : 395)]
		public virtual bool sceAacCheckStreamDataNeeded(int id)
		{
			return getAacInfo(id).StreamDataNeeded;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xAC6DCBE3, version = 395) public int sceAacNotifyAddStreamData(@CheckArgument("checkInitId") int id, int bytesToAdd)
		[HLEFunction(nid : 0xAC6DCBE3, version : 395)]
		public virtual int sceAacNotifyAddStreamData(int id, int bytesToAdd)
		{
			return getAacInfo(id).notifyAddStream(bytesToAdd);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x02098C69, version = 395) public int sceAacGetInfoToAddStreamData(@CheckArgument("checkInitId") int id, @CanBeNull pspsharp.HLE.TPointer32 writeAddr, @CanBeNull pspsharp.HLE.TPointer32 writableBytesAddr, @CanBeNull pspsharp.HLE.TPointer32 readOffsetAddr)
		[HLEFunction(nid : 0x02098C69, version : 395)]
		public virtual int sceAacGetInfoToAddStreamData(int id, TPointer32 writeAddr, TPointer32 writableBytesAddr, TPointer32 readOffsetAddr)
		{
			AacInfo info = getAacInfo(id);
			writeAddr.setValue(info.InputBuffer.WriteAddr);
			writableBytesAddr.setValue(info.WritableBytes);
			readOffsetAddr.setValue(info.InputBuffer.FilePosition);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceAacGetInfoToAddStreamData returning writeAddr=0x{0:X8}, writableBytes=0x{1:X}, readOffset=0x{2:X}", writeAddr.getValue(), writableBytesAddr.getValue(), readOffsetAddr.getValue()));
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6DC7758A, version = 395) public int sceAacGetMaxOutputSample(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0x6DC7758A, version : 395)]
		public virtual int sceAacGetMaxOutputSample(int id)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x506BF66C, version = 395) public int sceAacGetSumDecodedSample(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0x506BF66C, version : 395)]
		public virtual int sceAacGetSumDecodedSample(int id)
		{
			int sumDecodedSamples = getAacInfo(id).SumDecodedSamples;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceAacGetSumDecodedSample returning 0x{0:X}", sumDecodedSamples));
			}

			return sumDecodedSamples;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD2DA2BBA, version = 395) public int sceAacResetPlayPosition(@CheckArgument("checkInitId") int id)
		[HLEFunction(nid : 0xD2DA2BBA, version : 395)]
		public virtual int sceAacResetPlayPosition(int id)
		{
			return getAacInfo(id).resetPlayPosition();
		}
	}

}