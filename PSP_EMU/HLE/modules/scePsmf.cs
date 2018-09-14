using System.Collections.Generic;

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

	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using PSMFEntry = pspsharp.HLE.modules.sceMpeg.PSMFEntry;
	using PSMFHeader = pspsharp.HLE.modules.sceMpeg.PSMFHeader;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class scePsmf : HLEModule
	{
		public static Logger log = Modules.getLogger("scePsmf");

		public override void start()
		{
			psmfHeaderMap = new Dictionary<int, PSMFHeader>();

			base.start();
		}

		private Dictionary<int, PSMFHeader> psmfHeaderMap;

		public virtual TPointer32 checkPsmf(TPointer32 psmf)
		{
			int headerAddress = psmf.getValue(24);
			if (!psmfHeaderMap.ContainsKey(headerAddress))
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_PSMF_NOT_FOUND);
			}

			return psmf;
		}

		public virtual TPointer32 checkPsmfWithEPMap(TPointer32 psmf)
		{
			psmf = checkPsmf(psmf);
			PSMFHeader header = getPsmfHeader(psmf);
			if (!header.hasEPMap())
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("checkPsmfWithEPMap returning 0x{0:X8}(ERROR_PSMF_NOT_FOUND)", SceKernelErrors.ERROR_PSMF_NOT_FOUND));
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_PSMF_NOT_FOUND);
			}

			return psmf;
		}

		private PSMFHeader getPsmfHeader(TPointer32 psmf)
		{
			int headerAddress = psmf.getValue(24);
			return psmfHeaderMap[headerAddress];
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0xC22C8327, version : 150, checkInsideInterrupt : true, stackUsage : 0x50)]
		public virtual int scePsmfSetPsmf(TPointer32 psmf, TPointer bufferAddr)
		{
			Modules.sceMpegModule.analyseMpeg(bufferAddr.Address);
			PSMFHeader header = Modules.sceMpegModule.psmfHeader;
			psmfHeaderMap[bufferAddr.Address] = header;

			// PSMF struct:
			// This is an internal system data area which is used to store
			// several parameters of the file being handled.
			// It's size ranges from 28 bytes to 52 bytes, since when a pointer to
			// a certain PSMF area does not exist (NULL), it's omitted from the struct
			// (e.g.: no mark data or non existant EPMap).
			psmf.setValue(0, header.Version); // PSMF version.
			psmf.setValue(4, header.HeaderSize); // The PSMF header size (0x800).
			psmf.setValue(8, header.StreamSize); // The PSMF stream size.
			psmf.setValue(12, 0); // Grouping Period ID.
			psmf.setValue(16, 0); // Group ID.
			psmf.setValue(20, header.CurrentStreamNumber); // Current stream's number.
			psmf.setValue(24, bufferAddr.Address); // Pointer to PSMF header.
			// psmf + 28 - Pointer to current PSMF stream info (video/audio).
			// psmf + 32 - Pointer to mark data (used for chapters in UMD_VIDEO).
			// psmf + 36 - Pointer to current PSMF stream grouping period.
			// psmf + 40 - Pointer to current PSMF stream group.
			// psmf + 44 - Pointer to current PSMF stream.
			// psmf + 48 - Pointer to PSMF EPMap.

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC7DB3A5B, version = 150, checkInsideInterrupt = true, stackUsage = 0x50) public int scePsmfGetCurrentStreamType(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf, pspsharp.HLE.TPointer32 typeAddr, pspsharp.HLE.TPointer32 channelAddr)
		[HLEFunction(nid : 0xC7DB3A5B, version : 150, checkInsideInterrupt : true, stackUsage : 0x50)]
		public virtual int scePsmfGetCurrentStreamType(TPointer32 psmf, TPointer32 typeAddr, TPointer32 channelAddr)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			typeAddr.setValue(header.CurrentStreamType);
			channelAddr.setValue(header.CurrentStreamChannel);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("scePsmfGetCurrentStreamType returning type={0:D}, channel={1:D}", typeAddr.getValue(), channelAddr.getValue()));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x28240568, version = 150, checkInsideInterrupt = true, stackUsage = 0x0) public int scePsmfGetCurrentStreamNumber(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf)
		[HLEFunction(nid : 0x28240568, version : 150, checkInsideInterrupt : true, stackUsage : 0x0)]
		public virtual int scePsmfGetCurrentStreamNumber(TPointer32 psmf)
		{
			PSMFHeader header = getPsmfHeader(psmf);

			return header.CurrentStreamNumber;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1E6D9013, version = 150, checkInsideInterrupt = true, stackUsage = 0x20) public int scePsmfSpecifyStreamWithStreamType(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf, int type, int ch)
		[HLEFunction(nid : 0x1E6D9013, version : 150, checkInsideInterrupt : true, stackUsage : 0x20)]
		public virtual int scePsmfSpecifyStreamWithStreamType(TPointer32 psmf, int type, int ch)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			if (!header.setStreamWithType(type, ch))
			{
				// Do not return SceKernelErrors.ERROR_PSMF_INVALID_ID, but set an invalid stream number.
				header.StreamNum = -1;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x4BC9BDE0, version = 150, checkInsideInterrupt = true, stackUsage = 0x40) public int scePsmfSpecifyStream(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf, int streamNum)
		[HLEFunction(nid : 0x4BC9BDE0, version : 150, checkInsideInterrupt : true, stackUsage : 0x40)]
		public virtual int scePsmfSpecifyStream(TPointer32 psmf, int streamNum)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			header.StreamNum = streamNum;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x76D3AEBA, version = 150, checkInsideInterrupt = true, stackUsage = 0x10) public int scePsmfGetPresentationStartTime(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf, pspsharp.HLE.TPointer32 startTimeAddr)
		[HLEFunction(nid : 0x76D3AEBA, version : 150, checkInsideInterrupt : true, stackUsage : 0x10)]
		public virtual int scePsmfGetPresentationStartTime(TPointer32 psmf, TPointer32 startTimeAddr)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			int startTime = header.PresentationStartTime;
			startTimeAddr.setValue(startTime);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("scePsmfGetPresentationStartTime startTime={0:D}", startTime));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xBD8AE0D8, version = 150, checkInsideInterrupt = true, stackUsage = 0x10) public int scePsmfGetPresentationEndTime(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf, pspsharp.HLE.TPointer32 endTimeAddr)
		[HLEFunction(nid : 0xBD8AE0D8, version : 150, checkInsideInterrupt : true, stackUsage : 0x10)]
		public virtual int scePsmfGetPresentationEndTime(TPointer32 psmf, TPointer32 endTimeAddr)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			int endTime = header.PresentationEndTime;
			endTimeAddr.setValue(endTime);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("scePsmfGetPresentationEndTime endTime={0:D}", endTime));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xEAED89CD, version = 150, checkInsideInterrupt = true, stackUsage = 0x10) public int scePsmfGetNumberOfStreams(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf)
		[HLEFunction(nid : 0xEAED89CD, version : 150, checkInsideInterrupt : true, stackUsage : 0x10)]
		public virtual int scePsmfGetNumberOfStreams(TPointer32 psmf)
		{
			PSMFHeader header = getPsmfHeader(psmf);

			return header.NumberOfStreams;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7491C438, version = 150, checkInsideInterrupt = true, stackUsage = 0x10) public int scePsmfGetNumberOfEPentries(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf)
		[HLEFunction(nid : 0x7491C438, version : 150, checkInsideInterrupt : true, stackUsage : 0x10)]
		public virtual int scePsmfGetNumberOfEPentries(TPointer32 psmf)
		{
			PSMFHeader header = getPsmfHeader(psmf);

			return header.EPMapEntriesNum;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0BA514E5, version = 150, checkInsideInterrupt = true, stackUsage = 0x20) public int scePsmfGetVideoInfo(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf, pspsharp.HLE.TPointer32 videoInfoAddr)
		[HLEFunction(nid : 0x0BA514E5, version : 150, checkInsideInterrupt : true, stackUsage : 0x20)]
		public virtual int scePsmfGetVideoInfo(TPointer32 psmf, TPointer32 videoInfoAddr)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			if (!header.ValidCurrentStreamNumber)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("scePsmfGetVideoInfo returning 0x{0:X8}(ERROR_PSMF_INVALID_ID)", SceKernelErrors.ERROR_PSMF_INVALID_ID));
				}
				return SceKernelErrors.ERROR_PSMF_INVALID_ID;
			}
			videoInfoAddr.setValue(0, header.VideoWidth);
			videoInfoAddr.setValue(4, header.VideoHeight);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA83F7113, version = 150, checkInsideInterrupt = true, stackUsage = 0x20) public int scePsmfGetAudioInfo(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf, pspsharp.HLE.TPointer32 audioInfoAddr)
		[HLEFunction(nid : 0xA83F7113, version : 150, checkInsideInterrupt : true, stackUsage : 0x20)]
		public virtual int scePsmfGetAudioInfo(TPointer32 psmf, TPointer32 audioInfoAddr)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			if (!header.ValidCurrentStreamNumber)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("scePsmfGetAudioInfo returning 0x{0:X8}(ERROR_PSMF_INVALID_ID)", SceKernelErrors.ERROR_PSMF_INVALID_ID));
				}
				return SceKernelErrors.ERROR_PSMF_INVALID_ID;
			}
			audioInfoAddr.setValue(0, header.AudioChannelConfig);
			audioInfoAddr.setValue(4, header.AudioSampleFrequency);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x971A3A90, version = 150, checkInsideInterrupt = true, stackUsage = 0x10) public int scePsmfCheckEPmap(@CheckArgument("checkPsmfWithEPMap") pspsharp.HLE.TPointer32 psmf)
		[HLEFunction(nid : 0x971A3A90, version : 150, checkInsideInterrupt : true, stackUsage : 0x10)]
		public virtual int scePsmfCheckEPmap(TPointer32 psmf)
		{
			// checkPsmfWithEPMap is already returning the correct error code if no EPmap is present
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x4E624A34, version = 150, checkInsideInterrupt = true, stackUsage = 0x10) public int scePsmfGetEPWithId(@CheckArgument("checkPsmfWithEPMap") pspsharp.HLE.TPointer32 psmf, int id, pspsharp.HLE.TPointer32 outAddr)
		[HLEFunction(nid : 0x4E624A34, version : 150, checkInsideInterrupt : true, stackUsage : 0x10)]
		public virtual int scePsmfGetEPWithId(TPointer32 psmf, int id, TPointer32 outAddr)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			PSMFEntry entry = header.getEPMapEntry(id);
			if (entry == null)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("scePsmfGetEPWithId returning 0x{0:X8}(ERROR_PSMF_INVALID_ID)", SceKernelErrors.ERROR_PSMF_INVALID_ID));
				}
				return SceKernelErrors.ERROR_PSMF_INVALID_ID;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("scePsmfGetEPWithId returning {0}", entry));
			}
			outAddr.setValue(0, entry.EntryPTS);
			outAddr.setValue(4, entry.EntryOffset);
			outAddr.setValue(8, entry.EntryIndex);
			outAddr.setValue(12, entry.EntryPicOffset);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7C0E7AC3, version = 150, checkInsideInterrupt = true, stackUsage = 0x10) public int scePsmfGetEPWithTimestamp(@CheckArgument("checkPsmfWithEPMap") pspsharp.HLE.TPointer32 psmf, int ts, pspsharp.HLE.TPointer32 entryAddr)
		[HLEFunction(nid : 0x7C0E7AC3, version : 150, checkInsideInterrupt : true, stackUsage : 0x10)]
		public virtual int scePsmfGetEPWithTimestamp(TPointer32 psmf, int ts, TPointer32 entryAddr)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			if (ts < header.PresentationStartTime)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("scePsmfGetEPWithTimestamp returning 0x{0:X8}(ERROR_PSMF_INVALID_TIMESTAMP)", SceKernelErrors.ERROR_PSMF_INVALID_TIMESTAMP));
				}
				return SceKernelErrors.ERROR_PSMF_INVALID_TIMESTAMP;
			}

			PSMFEntry entry = header.getEPMapEntryWithTimestamp(ts);
			if (entry == null)
			{
				// Unknown error code
				if (log.DebugEnabled)
				{
					log.debug(string.Format("scePsmfGetEPWithTimestamp returning -1"));
				}
				return -1;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("scePsmfGetEPWithTimestamp returning {0}", entry));
			}
			entryAddr.setValue(0, entry.EntryPTS);
			entryAddr.setValue(4, entry.EntryOffset);
			entryAddr.setValue(8, entry.EntryIndex);
			entryAddr.setValue(12, entry.EntryPicOffset);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x5F457515, version = 150, checkInsideInterrupt = true, stackUsage = 0x20) public int scePsmfGetEPidWithTimestamp(@CheckArgument("checkPsmfWithEPMap") pspsharp.HLE.TPointer32 psmf, int ts)
		[HLEFunction(nid : 0x5F457515, version : 150, checkInsideInterrupt : true, stackUsage : 0x20)]
		public virtual int scePsmfGetEPidWithTimestamp(TPointer32 psmf, int ts)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			if (ts < header.PresentationStartTime)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("scePsmfGetEPidWithTimestamp returning 0x{0:X8}(ERROR_PSMF_INVALID_TIMESTAMP)", SceKernelErrors.ERROR_PSMF_INVALID_TIMESTAMP));
				}
				return SceKernelErrors.ERROR_PSMF_INVALID_TIMESTAMP;
			}

			PSMFEntry entry = header.getEPMapEntryWithTimestamp(ts);
			if (entry == null)
			{
				// Unknown error code
				if (log.DebugEnabled)
				{
					log.debug(string.Format("scePsmfGetEPidWithTimestamp returning -1"));
				}
				return -1;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("scePsmfGetEPidWithTimestamp returning id 0x{0:X}", entry.Id));
			}

			return entry.Id;
		}

		[HLEFunction(nid : 0x5B70FCC1, version : 150, checkInsideInterrupt : true, stackUsage : 0x20)]
		public virtual int scePsmfQueryStreamOffset(TPointer bufferAddr, TPointer32 offsetAddr)
		{
			// Always let sceMpeg handle the PSMF analysis.
			Modules.sceMpegModule.analyseMpeg(bufferAddr.Address);

			offsetAddr.setValue(Modules.sceMpegModule.psmfHeader.mpegOffset);

			return 0;
		}

		[HLEFunction(nid : 0x9553CC91, version : 150, checkInsideInterrupt : true, stackUsage : 0x0)]
		public virtual int scePsmfQueryStreamSize(TPointer bufferAddr, TPointer32 sizeAddr)
		{
			// Always let sceMpeg handle the PSMF analysis.
			Modules.sceMpegModule.analyseMpeg(bufferAddr.Address);

			sizeAddr.setValue(Modules.sceMpegModule.psmfHeader.mpegStreamSize);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x68D42328, version = 150, checkInsideInterrupt = true, stackUsage = 0xA0) public int scePsmfGetNumberOfSpecificStreams(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf, int streamType)
		[HLEFunction(nid : 0x68D42328, version : 150, checkInsideInterrupt : true, stackUsage : 0xA0)]
		public virtual int scePsmfGetNumberOfSpecificStreams(TPointer32 psmf, int streamType)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			int streamNum = header.getSpecificStreamNum(streamType);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("scePsmfGetNumberOfSpecificStreams returning {0:D}", streamNum));
			}

			return streamNum;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0C120E1D, version = 150, checkInsideInterrupt = true) public int scePsmfSpecifyStreamWithStreamTypeNumber(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf, int type, int typeNum)
		[HLEFunction(nid : 0x0C120E1D, version : 150, checkInsideInterrupt : true)]
		public virtual int scePsmfSpecifyStreamWithStreamTypeNumber(TPointer32 psmf, int type, int typeNum)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			if (!header.setStreamWithTypeNum(type, typeNum))
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("scePsmfSpecifyStreamWithStreamTypeNumber returning 0x{0:X8}(ERROR_PSMF_INVALID_ID)", SceKernelErrors.ERROR_PSMF_INVALID_ID));
				}
				return SceKernelErrors.ERROR_PSMF_INVALID_ID;
			}

			return 0;
		}

		[HLEFunction(nid : 0x2673646B, version : 150, checkInsideInterrupt : true, stackUsage : 0x100)]
		public virtual int scePsmfVerifyPsmf(TPointer bufferAddr)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("scePsmfVerifyPsmf {0}", Utilities.getMemoryDump(bufferAddr.Address, sceMpeg.MPEG_HEADER_BUFFER_MINIMUM_SIZE)));
			}

			int magic = bufferAddr.getValue32(sceMpeg.PSMF_MAGIC_OFFSET);
			if (magic != sceMpeg.PSMF_MAGIC)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("scePsmfVerifyPsmf returning 0x{0:X8}(ERROR_PSMF_INVALID_PSMF)", SceKernelErrors.ERROR_PSMF_INVALID_PSMF));
				}
				return SceKernelErrors.ERROR_PSMF_INVALID_PSMF;
			}

			int rawVersion = bufferAddr.getValue32(sceMpeg.PSMF_STREAM_VERSION_OFFSET);
			int version = sceMpeg.getMpegVersion(rawVersion);
			if (version < 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("scePsmfVerifyPsmf returning 0x{0:X8}(ERROR_PSMF_INVALID_PSMF)", SceKernelErrors.ERROR_PSMF_INVALID_PSMF));
				}
				return SceKernelErrors.ERROR_PSMF_INVALID_PSMF;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB78EB9E9, version = 150, checkInsideInterrupt = true, stackUsage = 0x0) public int scePsmfGetHeaderSize(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf, pspsharp.HLE.TPointer32 sizeAddr)
		[HLEFunction(nid : 0xB78EB9E9, version : 150, checkInsideInterrupt : true, stackUsage : 0x0)]
		public virtual int scePsmfGetHeaderSize(TPointer32 psmf, TPointer32 sizeAddr)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			sizeAddr.setValue(header.HeaderSize);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA5EBFE81, version = 150, checkInsideInterrupt = true, stackUsage = 0x0) public int scePsmfGetStreamSize(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf, pspsharp.HLE.TPointer32 sizeAddr)
		[HLEFunction(nid : 0xA5EBFE81, version : 150, checkInsideInterrupt : true, stackUsage : 0x0)]
		public virtual int scePsmfGetStreamSize(TPointer32 psmf, TPointer32 sizeAddr)
		{
			PSMFHeader header = getPsmfHeader(psmf);
			sizeAddr.setValue(header.StreamSize);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE1283895, version = 150, checkInsideInterrupt = true, stackUsage = 0x0) public int scePsmfGetPsmfVersion(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf)
		[HLEFunction(nid : 0xE1283895, version : 150, checkInsideInterrupt : true, stackUsage : 0x0)]
		public virtual int scePsmfGetPsmfVersion(TPointer32 psmf)
		{
			PSMFHeader header = getPsmfHeader(psmf);

			// Convert the header version into a decimal number, e.g. 0x0015 -> 15
			int headerVersion = header.Version;
			int version = 0;
			for (int i = 0; i < 4; i++, headerVersion >>= 8)
			{
				int digit = headerVersion & 0x0F;
				version = (version * 10) + digit;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("scePsmfGetPsmfVersion returning version={0:D} (headerVersion=0x{1:X4})", version, headerVersion));
			}

			return version;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDE78E9FC, version = 150) public int scePsmf_DE78E9FC(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf, int unknown)
		[HLEFunction(nid : 0xDE78E9FC, version : 150)]
		public virtual int scePsmf_DE78E9FC(TPointer32 psmf, int unknown)
		{
			// Get number of Psmf Marks
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x43AC7DBB, version = 150) public int scePsmf_43AC7DBB(@CheckArgument("checkPsmf") pspsharp.HLE.TPointer32 psmf, int unknown, int markNumber, pspsharp.HLE.TPointer markInfoAddr)
		[HLEFunction(nid : 0x43AC7DBB, version : 150)]
		public virtual int scePsmf_43AC7DBB(TPointer32 psmf, int unknown, int markNumber, TPointer markInfoAddr)
		{
			// Get Psmf Mark Information
			int markType = 0;
			int markTimestamp = 0;
			int markEntryEsStream = 0;
			int markData = 0;
			string markName = "Test";
			markInfoAddr.setValue32(0, markType);
			markInfoAddr.setValue32(4, markTimestamp);
			markInfoAddr.setValue32(8, markEntryEsStream);
			markInfoAddr.setValue32(12, markData);
			markInfoAddr.setValue32(16, markName.Length);
			markInfoAddr.setStringNZ(20, markName.Length, markName);

			return 0;
		}
	}
}