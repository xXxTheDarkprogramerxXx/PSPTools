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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._s0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocctl.fillNextPointersInLinkedList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeBytes;


	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using INetworkAdapter = pspsharp.network.INetworkAdapter;
	using MatchingObject = pspsharp.network.adhoc.MatchingObject;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class sceNetAdhocMatching : HLEModule
	{
		public static Logger log = Modules.getLogger("sceNetAdhocMatching");
		protected internal Dictionary<int, MatchingObject> matchingObjects;
		public static readonly int loopThreadRegisterArgument = _s0; // $s0 is preserved across calls
		private bool isInitialized;

		/// <summary>
		/// Matching events used in pspAdhocMatchingCallback
		/// </summary>
		/// <summary>
		/// Hello event. optdata contains data if optlen > 0. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_HELLO = 1;
		/// <summary>
		/// Join request. optdata contains data if optlen > 0. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_JOIN = 2;
		/// <summary>
		/// Target left matching. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_LEFT = 3;
		/// <summary>
		/// Join request rejected. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_REJECT = 4;
		/// <summary>
		/// Join request cancelled. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_CANCEL = 5;
		/// <summary>
		/// Join request accepted. optdata contains data if optlen > 0. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_ACCEPT = 6;
		/// <summary>
		/// Matching is complete. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_COMPLETE = 7;
		/// <summary>
		/// Ping timeout event. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_TIMEOUT = 8;
		/// <summary>
		/// Error event. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_ERROR = 9;
		/// <summary>
		/// Peer disconnect event. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_DISCONNECT = 10;
		/// <summary>
		/// Data received event. optdata contains data if optlen > 0. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_DATA = 11;
		/// <summary>
		/// Data acknowledged event. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_DATA_CONFIRM = 12;
		/// <summary>
		/// Data timeout event. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_DATA_TIMEOUT = 13;

		/// <summary>
		/// Internal ping message. </summary>
		public const int PSP_ADHOC_MATCHING_EVENT_INTERNAL_PING = 100;

		/// <summary>
		/// Matching modes used in sceNetAdhocMatchingCreate
		/// </summary>
		/// <summary>
		/// Host </summary>
		public const int PSP_ADHOC_MATCHING_MODE_HOST = 1;
		/// <summary>
		/// Client </summary>
		public const int PSP_ADHOC_MATCHING_MODE_CLIENT = 2;
		/// <summary>
		/// Peer to peer </summary>
		public const int PSP_ADHOC_MATCHING_MODE_PTP = 3;

		public override void start()
		{
			matchingObjects = new Dictionary<int, MatchingObject>();
			isInitialized = false;

			base.start();
		}

		protected internal virtual INetworkAdapter NetworkAdapter
		{
			get
			{
				return Modules.sceNetModule.NetworkAdapter;
			}
		}

		public virtual int checkMatchingId(int matchingId)
		{
			checkInitialized();

			if (!matchingObjects.ContainsKey(matchingId))
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_NET_ADHOC_INVALID_MATCHING_ID);
			}

			return matchingId;
		}

		public virtual void hleNetAdhocMatchingEventThread(Processor processor)
		{
			int matchingId = processor.cpu.getRegister(loopThreadRegisterArgument);
			if (log.TraceEnabled)
			{
				log.trace(string.Format("hleNetAdhocMatchingEventThread matchingId={0:D}", matchingId));
			}

			MatchingObject matchingObject = matchingObjects[matchingId];
			if (matchingObject != null && matchingObject.eventLoop())
			{
				Modules.ThreadManForUserModule.hleKernelDelayThread(10000, false);
			}
			else
			{
				// Exit thread with status 0
				processor.cpu._v0 = 0;
				Modules.ThreadManForUserModule.hleKernelExitDeleteThread();
			}
		}

		public virtual void hleNetAdhocMatchingInputThread(Processor processor)
		{
			int matchingId = processor.cpu.getRegister(loopThreadRegisterArgument);
			if (log.TraceEnabled)
			{
				log.trace(string.Format("hleNetAdhocMatchingInputThread matchingId={0:D}", matchingId));
			}

			MatchingObject matchingObject = matchingObjects[matchingId];
			if (matchingObject != null && matchingObject.inputLoop())
			{
				Modules.ThreadManForUserModule.hleKernelDelayThread(10000, false);
			}
			else
			{
				// Exit thread with status 0
				processor.cpu._v0 = 0;
				Modules.ThreadManForUserModule.hleKernelExitDeleteThread();
			}
		}

		private static string getModeName(int mode)
		{
			switch (mode)
			{
				case PSP_ADHOC_MATCHING_MODE_HOST:
					return "HOST";
				case PSP_ADHOC_MATCHING_MODE_CLIENT:
					return "CLIENT";
				case PSP_ADHOC_MATCHING_MODE_PTP:
					return "PTP";
			}

			return string.Format("Unknown mode {0:D}", mode);
		}

		protected internal virtual void checkInitialized()
		{
			if (!isInitialized)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_NET_ADHOC_MATCHING_NOT_INITIALIZED);
			}
		}

		/// <summary>
		/// Initialize the Adhoc matching library
		/// </summary>
		/// <param name="memsize"> - Internal memory pool size. Lumines uses 0x20000
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
		[HLEFunction(nid : 0x2A2A1E07, version : 150)]
		public virtual int sceNetAdhocMatchingInit(int memsize)
		{
			if (isInitialized)
			{
				return SceKernelErrors.ERROR_NET_ADHOC_MATCHING_ALREADY_INITIALIZED;
			}

			isInitialized = true;

			return 0;
		}

		/// <summary>
		/// Terminate the Adhoc matching library
		/// </summary>
		/// <returns> 0 on success, < 0 on error </returns>
		[HLEFunction(nid : 0x7945ECDA, version : 150)]
		public virtual int sceNetAdhocMatchingTerm()
		{
			isInitialized = false;

			return 0;
		}

		/// <summary>
		/// Create an Adhoc matching object
		/// </summary>
		/// <param name="mode"> - One of ::pspAdhocMatchingModes </param>
		/// <param name="maxpeers"> - Maximum number of peers to match (only used when mode is PSP_ADHOC_MATCHING_MODE_HOST) </param>
		/// <param name="port"> - Port. Lumines uses 0x22B </param>
		/// <param name="bufsize"> - Receiving buffer size </param>
		/// <param name="hellodelay"> - Hello message send delay in microseconds (only used when mode is PSP_ADHOC_MATCHING_MODE_HOST or PSP_ADHOC_MATCHING_MODE_PTP) </param>
		/// <param name="pingdelay"> - Ping send delay in microseconds. Lumines uses 0x5B8D80 (only used when mode is PSP_ADHOC_MATCHING_MODE_HOST or PSP_ADHOC_MATCHING_MODE_PTP) </param>
		/// <param name="initcount"> - Initial count of the of the resend counter. Lumines uses 3 </param>
		/// <param name="msgdelay"> - Message send delay in microseconds </param>
		/// <param name="callback"> - Callback to be called for matching
		/// </param>
		/// <returns> ID of object on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xCA5EDA6F, version = 150) public int sceNetAdhocMatchingCreate(int mode, int maxPeers, int port, int bufSize, int helloDelay, int pingDelay, int initCount, int msgDelay, @CanBeNull pspsharp.HLE.TPointer callback)
		[HLEFunction(nid : 0xCA5EDA6F, version : 150)]
		public virtual int sceNetAdhocMatchingCreate(int mode, int maxPeers, int port, int bufSize, int helloDelay, int pingDelay, int initCount, int msgDelay, TPointer callback)
		{
			checkInitialized();

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetAdhocMatchingCreate mode={0}", getModeName(mode)));
			}

			MatchingObject matchingObject = NetworkAdapter.createMatchingObject();
			matchingObject.Mode = mode;
			matchingObject.MaxPeers = maxPeers;
			matchingObject.Port = port;
			matchingObject.BufSize = bufSize;
			matchingObject.HelloDelay = helloDelay;
			matchingObject.PingDelay = pingDelay;
			matchingObject.InitCount = initCount;
			matchingObject.MsgDelay = msgDelay;
			matchingObject.Callback = callback.Address;
			matchingObject.create();
			matchingObjects[matchingObject.Id] = matchingObject;

			return matchingObject.Id;
		}

		/// <summary>
		/// Start a matching object
		/// </summary>
		/// <param name="matchingid"> - The ID returned from ::sceNetAdhocMatchingCreate </param>
		/// <param name="evthpri"> - Priority of the event handler thread. Lumines uses 0x10 </param>
		/// <param name="evthstack"> - Stack size of the event handler thread. Lumines uses 0x2000 </param>
		/// <param name="inthpri"> - Priority of the input handler thread. Lumines uses 0x10 </param>
		/// <param name="inthstack"> - Stack size of the input handler thread. Lumines uses 0x2000 </param>
		/// <param name="optlen"> - Size of hellodata </param>
		/// <param name="optdata"> - Pointer to block of data passed to callback
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x93EF3843, version = 150) public int sceNetAdhocMatchingStart(@CheckArgument("checkMatchingId") int matchingId, int evthPri, int evthStack, int inthPri, int inthStack, int optLen, @CanBeNull pspsharp.HLE.TPointer optData)
		[HLEFunction(nid : 0x93EF3843, version : 150)]
		public virtual int sceNetAdhocMatchingStart(int matchingId, int evthPri, int evthStack, int inthPri, int inthStack, int optLen, TPointer optData)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("Matching opt data: {0}", Utilities.getMemoryDump(optData.Address, optLen)));
			}

			return matchingObjects[matchingId].start(evthPri, evthStack, inthPri, inthStack, optLen, optData.Address);
		}

		/// <summary>
		/// Stop a matching object
		/// </summary>
		/// <param name="matchingid"> - The ID returned from ::sceNetAdhocMatchingCreate
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x32B156B3, version = 150) public int sceNetAdhocMatchingStop(@CheckArgument("checkMatchingId") int matchingId)
		[HLEFunction(nid : 0x32B156B3, version : 150)]
		public virtual int sceNetAdhocMatchingStop(int matchingId)
		{
			return matchingObjects[matchingId].stop();
		}

		/// <summary>
		/// Delete an Adhoc matching object
		/// </summary>
		/// <param name="matchingid"> - The ID returned from ::sceNetAdhocMatchingCreate
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF16EAF4F, version = 150) public int sceNetAdhocMatchingDelete(@CheckArgument("checkMatchingId") int matchingId)
		[HLEFunction(nid : 0xF16EAF4F, version : 150)]
		public virtual int sceNetAdhocMatchingDelete(int matchingId)
		{
			matchingObjects.Remove(matchingId).delete();

			return 0;
		}

		/// <summary>
		/// Send data to a matching target
		/// </summary>
		/// <param name="matchingid"> - The ID returned from ::sceNetAdhocMatchingCreate </param>
		/// <param name="mac"> - The MAC address to send the data to </param>
		/// <param name="datalen"> - Length of the data </param>
		/// <param name="data"> - Pointer to the data
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF79472D7, version = 150) public int sceNetAdhocMatchingSendData(@CheckArgument("checkMatchingId") int matchingId, pspsharp.HLE.kernel.types.pspNetMacAddress macAddress, int dataLen, pspsharp.HLE.TPointer data)
		[HLEFunction(nid : 0xF79472D7, version : 150)]
		public virtual int sceNetAdhocMatchingSendData(int matchingId, pspNetMacAddress macAddress, int dataLen, TPointer data)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("Send data: {0}", Utilities.getMemoryDump(data.Address, dataLen)));
			}

			return matchingObjects[matchingId].send(macAddress, dataLen, data.Address);
		}

		/// <summary>
		/// Abort a data send to a matching target
		/// </summary>
		/// <param name="matchingid"> - The ID returned from ::sceNetAdhocMatchingCreate </param>
		/// <param name="mac"> - The MAC address to send the data to
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEC19337D, version = 150) public int sceNetAdhocMatchingAbortSendData(@CheckArgument("checkMatchingId") int matchingId, pspsharp.HLE.kernel.types.pspNetMacAddress macAddress)
		[HLEFunction(nid : 0xEC19337D, version : 150)]
		public virtual int sceNetAdhocMatchingAbortSendData(int matchingId, pspNetMacAddress macAddress)
		{
			return 0;
		}

		/// <summary>
		/// Select a matching target
		/// </summary>
		/// <param name="matchingid"> - The ID returned from ::sceNetAdhocMatchingCreate </param>
		/// <param name="mac"> - MAC address to select </param>
		/// <param name="optlen"> - Optional data length </param>
		/// <param name="optdata"> - Pointer to the optional data
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x5E3D4B79, version = 150) public int sceNetAdhocMatchingSelectTarget(@CheckArgument("checkMatchingId") int matchingId, pspsharp.HLE.kernel.types.pspNetMacAddress macAddress, int optLen, @CanBeNull pspsharp.HLE.TPointer optData)
		[HLEFunction(nid : 0x5E3D4B79, version : 150)]
		public virtual int sceNetAdhocMatchingSelectTarget(int matchingId, pspNetMacAddress macAddress, int optLen, TPointer optData)
		{
			return matchingObjects[matchingId].selectTarget(macAddress, optLen, optData.Address);
		}

		/// <summary>
		/// Cancel a matching target
		/// </summary>
		/// <param name="matchingid"> - The ID returned from ::sceNetAdhocMatchingCreate </param>
		/// <param name="mac"> - The MAC address to cancel
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xEA3C6108, version = 150) public int sceNetAdhocMatchingCancelTarget(@CheckArgument("checkMatchingId") int matchingId, pspsharp.HLE.kernel.types.pspNetMacAddress macAddress)
		[HLEFunction(nid : 0xEA3C6108, version : 150)]
		public virtual int sceNetAdhocMatchingCancelTarget(int matchingId, pspNetMacAddress macAddress)
		{
			return matchingObjects[matchingId].cancelTarget(macAddress);
		}

		/// <summary>
		/// Cancel a matching target (with optional data)
		/// </summary>
		/// <param name="matchingid"> - The ID returned from ::sceNetAdhocMatchingCreate </param>
		/// <param name="mac"> - The MAC address to cancel </param>
		/// <param name="optlen"> - Optional data length </param>
		/// <param name="optdata"> - Pointer to the optional data
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8F58BEDF, version = 150) public int sceNetAdhocMatchingCancelTargetWithOpt(@CheckArgument("checkMatchingId") int matchingId, pspsharp.HLE.kernel.types.pspNetMacAddress macAddress, int optLen, @CanBeNull pspsharp.HLE.TPointer optData)
		[HLEFunction(nid : 0x8F58BEDF, version : 150)]
		public virtual int sceNetAdhocMatchingCancelTargetWithOpt(int matchingId, pspNetMacAddress macAddress, int optLen, TPointer optData)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("Opt data: {0}", Utilities.getMemoryDump(optData.Address, optLen)));
			}

			return matchingObjects[matchingId].cancelTarget(macAddress, optLen, optData.Address);
		}

		/// <summary>
		/// Get the optional hello message
		/// </summary>
		/// <param name="matchingid"> - The ID returned from ::sceNetAdhocMatchingCreate </param>
		/// <param name="optlenAddr"> - Length of the hello data (input/output) </param>
		/// <param name="optdata"> - Pointer to the hello data
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB5D96C2A, version = 150) public int sceNetAdhocMatchingGetHelloOpt(@CheckArgument("checkMatchingId") int matchingId, pspsharp.HLE.TPointer32 optLenAddr, @CanBeNull pspsharp.HLE.TPointer optData)
		[HLEFunction(nid : 0xB5D96C2A, version : 150)]
		public virtual int sceNetAdhocMatchingGetHelloOpt(int matchingId, TPointer32 optLenAddr, TPointer optData)
		{
			MatchingObject matchingObject = matchingObjects[matchingId];
			int helloOptLen = matchingObject.HelloOptLen;

			int bufSize = optLenAddr.getValue();
			optLenAddr.setValue(helloOptLen);

			if (helloOptLen > 0 && optData.Address != 0 && bufSize > 0)
			{
				int length = System.Math.Min(bufSize, helloOptLen);
				writeBytes(optData.Address, length, matchingObject.HelloOptData, 0);
			}

			return 0;
		}

		/// <summary>
		/// Set the optional hello message
		/// </summary>
		/// <param name="matchingid"> - The ID returned from ::sceNetAdhocMatchingCreate </param>
		/// <param name="optlen"> - Length of the hello data </param>
		/// <param name="optdata"> - Pointer to the hello data
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB58E61B7, version = 150) public int sceNetAdhocMatchingSetHelloOpt(@CheckArgument("checkMatchingId") int matchingId, int optLen, @CanBeNull pspsharp.HLE.TPointer optData)
		[HLEFunction(nid : 0xB58E61B7, version : 150)]
		public virtual int sceNetAdhocMatchingSetHelloOpt(int matchingId, int optLen, TPointer optData)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("Hello opt data: {0}", Utilities.getMemoryDump(optData.Address, optLen)));
			}

			matchingObjects[matchingId].setHelloOpt(optLen, optData.Address);

			return 0;
		}

		/// <summary>
		/// Get a list of matching members
		/// </summary>
		/// <param name="matchingid"> - The ID returned from ::sceNetAdhocMatchingCreate </param>
		/// <param name="length"> - The length of the list. </param>
		/// <param name="buf"> - An allocated area of size length.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC58BCD9E, version = 150) public int sceNetAdhocMatchingGetMembers(@CheckArgument("checkMatchingId") int matchingId, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer32 sizeAddr, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=12, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buf)
		[HLEFunction(nid : 0xC58BCD9E, version : 150)]
		public virtual int sceNetAdhocMatchingGetMembers(int matchingId, TPointer32 sizeAddr, TPointer buf)
		{
			const int matchingMemberSize = 12;

			MatchingObject matchingObject = matchingObjects[matchingId];
			IList<pspNetMacAddress> members = matchingObject.Members;

			int size = sizeAddr.getValue();
			sizeAddr.setValue(matchingMemberSize * members.Count);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetAdhocMatchingGetMembers returning size={0:D}", sizeAddr.getValue()));
			}

			if (buf.NotNull)
			{
				int offset = 0;
				foreach (pspNetMacAddress member in members)
				{
					// Check if enough space available to write the next structure
					if (offset + matchingMemberSize > size || member == null)
					{
						break;
					}

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetAdhocMatchingGetMembers returning {0} at 0x{1:X8}", member, buf.Address + offset));
					}

					/// <summary>
					/// Pointer to next Member structure in list: will be written later </summary>
					offset += 4;

					/// <summary>
					/// MAC address </summary>
					member.write(buf, offset);
					offset += member.@sizeof();

					/// <summary>
					/// Padding </summary>
					buf.setValue16(offset, (short) 0);
					offset += 2;
				}

				fillNextPointersInLinkedList(buf, offset, matchingMemberSize);
			}

			return 0;
		}

		/// <summary>
		/// Get the status of the memory pool used by the matching library
		/// </summary>
		/// <param name="poolstat"> - A ::pspAdhocPoolStat.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9C5CFB7D, version = 150) public int sceNetAdhocMatchingGetPoolStat()
		[HLEFunction(nid : 0x9C5CFB7D, version : 150)]
		public virtual int sceNetAdhocMatchingGetPoolStat()
		{
			checkInitialized();

			return 0;
		}

		/// <summary>
		/// Get the maximum memory usage by the matching library
		/// </summary>
		/// <returns> The memory usage on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x40F8F435, version = 150) public int sceNetAdhocMatchingGetPoolMaxAlloc()
		[HLEFunction(nid : 0x40F8F435, version : 150)]
		public virtual int sceNetAdhocMatchingGetPoolMaxAlloc()
		{
			checkInitialized();

			return 0;
		}
	}
}