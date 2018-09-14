using System;
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
//	import static pspsharp.util.Utilities.endianSwap16;


	using CpuState = pspsharp.Allegrex.CpuState;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceNetIfMessage = pspsharp.HLE.kernel.types.SceNetIfMessage;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using Wlan = pspsharp.hardware.Wlan;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using INetworkAdapter = pspsharp.network.INetworkAdapter;
	using NetworkAdapterFactory = pspsharp.network.NetworkAdapterFactory;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class sceNet : HLEModule
	{
		public static Logger log = Modules.getLogger("sceNet");
		private INetworkAdapter networkAdapter;
		protected internal int netMemSize;
		private static readonly int[] look_ctype_table = new int[] {0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x08, 0x08, 0x08, 0x08, 0x08, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x18, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x42, 0x42, 0x42, 0x42, 0x42, 0x42, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x10, 0x10, 0x10, 0x10, 0x20};
		protected internal IDictionary<int, int> allocatedThreadStructures;
		protected internal readonly System.Random random = new System.Random();
		protected internal int readCallback;
		protected internal int unknownCallback1;
		protected internal int adhocSocketAlertCallback;
		protected internal int getReadContextCallback;
		protected internal TPointer32 readContextAddr;
		protected internal TPointer readMessage;
		protected internal IDictionary<int, int> blockedThreads;

		private class AfterReadContextCallback : IAction
		{
			private readonly sceNet outerInstance;

			public AfterReadContextCallback(sceNet outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.hleAfterReadContextCallback();
			}
		}

		public override void start()
		{
			networkAdapter = NetworkAdapterFactory.createNetworkAdapter();
			networkAdapter.start();
			allocatedThreadStructures = new Dictionary<int, int>();
			readCallback = 0;
			unknownCallback1 = 0;
			adhocSocketAlertCallback = 0;
			getReadContextCallback = 0;
			blockedThreads = new Dictionary<int, int>();

			base.start();
		}

		public override void stop()
		{
			networkAdapter.stop();
			networkAdapter = null;

			base.stop();
		}

		public virtual INetworkAdapter NetworkAdapter
		{
			get
			{
				return networkAdapter;
			}
		}

		/// <summary>
		/// Convert a 6-byte MAC address into a string representation (xx:xx:xx:xx:xx:xx)
		/// in lower-case.
		/// The PSP always returns MAC addresses in lower-case.
		/// </summary>
		/// <param name="macAddress">  MAC address </param>
		/// <returns>            string representation of the MAC address: xx:xx:xx:xx:xx:xx (in lower-case). </returns>
		public static string convertMacAddressToString(sbyte[] macAddress)
		{
			return string.Format("{0:x2}:{1:x2}:{2:x2}:{3:x2}:{4:x2}:{5:x2}", macAddress[0], macAddress[1], macAddress[2], macAddress[3], macAddress[4], macAddress[5]);
		}

		protected internal static int parseHexDigit(char c)
		{
			if (c >= '0' && c <= '9')
			{
				return c - '0';
			}
			else if (c >= 'A' && c <= 'F')
			{
				return c - 'A' + 10;
			}
			else if (c >= 'a' && c <= 'f')
			{
				return c - 'a' + 10;
			}
			else
			{
				log.error(string.Format("Cannot parse hex digit '{0}'", c));
				return 0;
			}
		}

		/// <summary>
		/// Convert a string MAC address representation (xx:xx:xx:xx:xx:x)
		/// into a 6-byte representation.
		/// Both lower and uppercase representations are accepted.
		/// </summary>
		/// <param name="str">    String representation in format xx:xx:xx:xx:xx:xx (in lower or uppercase) </param>
		/// <returns>       6-byte representation </returns>
		public static sbyte[] convertStringToMacAddress(string str)
		{
			sbyte[] macAddress = new sbyte[Wlan.MAC_ADDRESS_LENGTH];
			for (int i = 0, n = 0; i < macAddress.Length; i++)
			{
				int n1 = parseHexDigit(str[n++]);
				int n2 = parseHexDigit(str[n++]);
				n++; // skip ':'
				macAddress[i] = (sbyte)((n1 << 4) + n2);
			}

			return macAddress;
		}

		protected internal virtual int networkSwap32(int value)
		{
			return Utilities.endianSwap32(value);
		}

		protected internal virtual int networkSwap16(int value)
		{
			return Utilities.endianSwap16(value);
		}

		protected internal virtual void sendDummyMessage(SceKernelThreadInfo thread)
		{
			if (readContextAddr == null)
			{
				int mem = Modules.sceNetIfhandleModule.hleNetMallocInternal(4);
				if (mem > 0)
				{
					readContextAddr = new TPointer32(Memory.Instance, mem);
				}
			}
			if (readContextAddr != null)
			{
				Modules.ThreadManForUserModule.executeCallback(thread, getReadContextCallback, new AfterReadContextCallback(this), true, 0, 0, readContextAddr.Address);
			}
		}

		protected internal virtual void hleAfterReadContextCallback()
		{
			if (readMessage == null)
			{
				int size = 256;
				int mem = Modules.sceNetIfhandleModule.hleNetMallocInternal(size);
				if (mem > 0)
				{
					readMessage = new TPointer(Memory.Instance, mem);
					readMessage.clear(size);
					RuntimeContext.debugMemory(mem, size);
				}
			}

			if (readMessage != null)
			{
				// Store dummy message
				SceNetIfMessage message = new SceNetIfMessage();
				TPointer data = new TPointer(Memory.Instance, readMessage.Address + message.@sizeof());
				TPointer header = new TPointer(data.Memory, data.Address);
				TPointer content = new TPointer(data.Memory, data.Address + 60);
				const int contentLength = 8;
				// Header information:
				header.setArray(0, Wlan.MacAddress, 6); // destination MAC address
				header.setArray(6, new sbyte[] {0x11, 0x22, 0x33, 0x44, 0x55, 0x66}, 6); // source MAC address
				header.setValue8(48, (sbyte) 1); // 1 or 2
				header.setValue8(49, (sbyte) 0);
				header.setValue16(50, (short) endianSwap16(12 + contentLength)); // value must be >= 12
				header.setValue16(52, (short) endianSwap16(0x22C)); // source port
				header.setValue16(54, (short) endianSwap16(0x22C)); // destination port
				header.setValue8(58, (sbyte) 0);
				header.setValue8(59, (sbyte) 0);

				// Real message content:
				content.setValue8(0, (sbyte) 1);
				content.setValue8(1, (sbyte) 1);
				content.setValue16(2, (short) endianSwap16(contentLength - 4)); // endian-swapped value, length of following data
				content.setValue8(4, (sbyte) 0); // Dummy data
				content.setValue8(5, (sbyte) 0);
				content.setValue8(6, (sbyte) 0);
				content.setValue8(7, (sbyte) 0);

				message.dataAddr = data.Address;
				message.dataLength = 60 + contentLength;
				message.unknown24 = 60 + contentLength;
				message.write(readMessage);

				TPointer readContext = new TPointer(Memory.Instance, readContextAddr.getValue());
				readContext.setValue32(0, readMessage.Address);
				readContext.setValue32(8, readContext.getValue32(8) + 1);
			}

			SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
			Modules.ThreadManForUserModule.executeCallback(thread, readCallback, null, true);
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x39AF39A6, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetInit(int poolSize, int calloutThreadPri, int calloutThreadStack, int netinitThreadPri, int netinitThreadStack)
		{
			netMemSize = poolSize;
			return 0;
		}

		[HLEFunction(nid : 0x281928A9, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetTerm()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x50647530, version = 150, checkInsideInterrupt = true) public int sceNetFreeThreadinfo(int threadID)
		[HLEFunction(nid : 0x50647530, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetFreeThreadinfo(int threadID)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAD6844c6, version = 150, checkInsideInterrupt = true) public int sceNetThreadAbort(int threadID)
		[HLEFunction(nid : 0xAD6844c6, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetThreadAbort(int threadID)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x89360950, version = 150, checkInsideInterrupt = true) public void sceNetEtherNtostr(@CanBeNull pspsharp.HLE.kernel.types.pspNetMacAddress macAddress, @CanBeNull pspsharp.HLE.TPointer strAddr)
		[HLEFunction(nid : 0x89360950, version : 150, checkInsideInterrupt : true)]
		public virtual void sceNetEtherNtostr(pspNetMacAddress macAddress, TPointer strAddr)
		{
			// This syscall is only doing something when both parameters are not 0.
			if (macAddress.NotNull && strAddr.NotNull)
			{
				// Convert 6-byte Mac address into string representation (XX:XX:XX:XX:XX:XX).
				Utilities.writeStringZ(Memory.Instance, strAddr.Address, convertMacAddressToString(macAddress.macAddress));
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD27961C9, version = 150, checkInsideInterrupt = true) public void sceNetEtherStrton(@StringInfo(maxLength=17) @CanBeNull pspsharp.HLE.PspString str, @CanBeNull pspsharp.HLE.TPointer etherAddr)
		[HLEFunction(nid : 0xD27961C9, version : 150, checkInsideInterrupt : true)]
		public virtual void sceNetEtherStrton(PspString str, TPointer etherAddr)
		{
			// This syscall is only doing something when both parameters are not 0.
			if (str.NotNull && etherAddr.NotNull)
			{
				// Convert string Mac address string representation (XX:XX:XX:XX:XX:XX)
				// into 6-byte representation.
				pspNetMacAddress macAddress = new pspNetMacAddress();
				macAddress.MacAddress = convertStringToMacAddress(str.String);
				macAddress.write(etherAddr);
			}
		}

		[HLEFunction(nid : 0xF5805EFE, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetHtonl(int host32)
		{
			// Convert host 32-bits to network 32-bits
			return networkSwap32(host32);
		}

		[HLEFunction(nid : 0x39C1BF02, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetHtons(int host16)
		{
			// Convert host 16-bits to network 16-bits
			return networkSwap16(host16);
		}

		[HLEFunction(nid : 0x93C4AF7E, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetNtohl(int net32)
		{
			// Convert network 32-bits to host 32-bits
			return networkSwap32(net32);
		}

		[HLEFunction(nid : 0x4CE03207, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetNtohs(int net16)
		{
			// Convert network 16-bits to host 16-bits
			return networkSwap16(net16);
		}

		[HLEFunction(nid : 0x0BF0A3AE, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetGetLocalEtherAddr(TPointer etherAddr)
		{
			// Return WLAN MAC address
			pspNetMacAddress macAddress = new pspNetMacAddress();
			macAddress.MacAddress = Wlan.MacAddress;
			macAddress.write(etherAddr);

			return 0;
		}

		[HLEFunction(nid : 0xCC393E48, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetGetMallocStat(TPointer32 statAddr)
		{
			// Faking. Assume the pool is half free.
			int freeSize = netMemSize / 2;

			statAddr.setValue(0, netMemSize); // Poolsize from sceNetInit.
			statAddr.setValue(4, netMemSize - freeSize); // Currently in use size.
			statAddr.setValue(8, freeSize); // Free size.

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD8722983, version = 150) public int sceNetStrlen(@CanBeNull pspsharp.HLE.TPointer srcAddr)
		[HLEFunction(nid : 0xD8722983, version : 150)]
		public virtual int sceNetStrlen(TPointer srcAddr)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetStrlen '{0}'", srcAddr.StringZ));
			}
			return Modules.SysclibForKernelModule.strlen(srcAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x80C9F02A, version = 150) public int sceNetStrcpy(@CanBeNull pspsharp.HLE.TPointer destAddr, @CanBeNull pspsharp.HLE.TPointer srcAddr)
		[HLEFunction(nid : 0x80C9F02A, version : 150)]
		public virtual int sceNetStrcpy(TPointer destAddr, TPointer srcAddr)
		{
			return Modules.SysclibForKernelModule.strcpy(destAddr, srcAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA0F16ABD, version = 150) public int sceNetStrcmp(@CanBeNull pspsharp.HLE.TPointer src1Addr, @CanBeNull pspsharp.HLE.TPointer src2Addr)
		[HLEFunction(nid : 0xA0F16ABD, version : 150)]
		public virtual int sceNetStrcmp(TPointer src1Addr, TPointer src2Addr)
		{
			return Modules.SysclibForKernelModule.strcmp(src1Addr, src2Addr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x94DCA9F0, version = 150) public int sceNetStrncmp(@CanBeNull pspsharp.HLE.TPointer src1Addr, @CanBeNull pspsharp.HLE.TPointer src2Addr, int size)
		[HLEFunction(nid : 0x94DCA9F0, version : 150)]
		public virtual int sceNetStrncmp(TPointer src1Addr, TPointer src2Addr, int size)
		{
			return Modules.SysclibForKernelModule.strncmp(src1Addr, src2Addr, size);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB5CE388A, version = 150) public int sceNetStrncpy(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextNextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer destAddr, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer srcAddr, int size)
		[HLEFunction(nid : 0xB5CE388A, version : 150)]
		public virtual int sceNetStrncpy(TPointer destAddr, TPointer srcAddr, int size)
		{
			return Modules.SysclibForKernelModule.strncpy(destAddr, srcAddr, size);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xBCBE14CF, version = 150) public int sceNetStrchr(@CanBeNull pspsharp.HLE.TPointer srcAddr, int c1)
		[HLEFunction(nid : 0xBCBE14CF, version : 150)]
		public virtual int sceNetStrchr(TPointer srcAddr, int c1)
		{
			if (srcAddr.Null)
			{
				return 0;
			}
			c1 = c1 & 0xFF;

			IMemoryReader memoryReader = MemoryReader.getMemoryReader(srcAddr.Address, 1);
			for (int i = 0; true; i++)
			{
				int c2 = memoryReader.readNext();
				if (c1 == c2)
				{
					// Character found
					return srcAddr.Address + i;
				}
				else if (c2 == 0)
				{
					// End of string
					break;
				}
			}

			return 0;
		}

		[HLEFunction(nid : 0x750F705D, version : 150)]
		public virtual int sceNetLook_ctype_table(int c)
		{
			int ctype = look_ctype_table[c & 0xFF];

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetLook_ctype_table c='{0}' = 0x{1:X2}", (char) c, ctype));
			}

			return ctype;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x5705F6F9, version = 150) public int sceNetStrcat(@CanBeNull pspsharp.HLE.TPointer destAddr, @CanBeNull pspsharp.HLE.TPointer srcAddr)
		[HLEFunction(nid : 0x5705F6F9, version : 150)]
		public virtual int sceNetStrcat(TPointer destAddr, TPointer srcAddr)
		{
			return Modules.SysclibForKernelModule.strcat(destAddr, srcAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x9CFBC7E3, version = 150) public int sceNetStrcasecmp(@CanBeNull pspsharp.HLE.PspString src1Addr, @CanBeNull pspsharp.HLE.PspString src2Addr)
		[HLEFunction(nid : 0x9CFBC7E3, version : 150)]
		public virtual int sceNetStrcasecmp(PspString src1Addr, PspString src2Addr)
		{
			if (src1Addr.Null || src2Addr.Null)
			{
				if (src1Addr.Address == src2Addr.Address)
				{
					return 0;
				}
				if (src1Addr.NotNull)
				{
					return 1;
				}
				return -1;
			}

			return string.Compare(src1Addr.String, src2Addr.String, StringComparison.OrdinalIgnoreCase);
		}

		[HLEFunction(nid : 0x96EF9DA1, version : 150)]
		public virtual int sceNetTolower(int c)
		{
			int ctype = look_ctype_table[c & 0xFF];
			if ((ctype & 0x01) != 0)
			{
				c += 0x20;
			}

			return c;
		}

		[HLEFunction(nid : 0xC13C9307, version : 150)]
		public virtual int sceNetToupper(int c)
		{
			int ctype = look_ctype_table[c & 0xFF];
			if ((ctype & 0x02) != 0)
			{
				c -= 0x20;
			}

			return c;
		}

		[HLEFunction(nid : 0xCF705E46, version : 150)]
		public virtual int sceNetSprintf(CpuState cpu, TPointer buffer, string format)
		{
			return Modules.SysclibForKernelModule.sprintf(cpu, buffer, format);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB9085A96, version = 150) public int sceNetStrncasecmp(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextNextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer src1Addr, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer src2Addr, int size)
		[HLEFunction(nid : 0xB9085A96, version : 150)]
		public virtual int sceNetStrncasecmp(TPointer src1Addr, TPointer src2Addr, int size)
		{
			if (src1Addr.Null || src2Addr.Null)
			{
				if (src1Addr.Address == src2Addr.Address)
				{
					return 0;
				}
				if (src1Addr.NotNull)
				{
					return 1;
				}
				return -1;
			}

			string s1 = src1Addr.getStringNZ(size);
			string s2 = src2Addr.getStringNZ(size);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetStrncasecmp s1='{0}', s2='{1}'", s1, s2));
			}

			return string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Convert a string to an integer. The base is 10.
		/// </summary>
		/// <param name="string">   the string to be converted </param>
		/// <returns>         the integer value represented by the string </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1FB2FDDD, version = 150) public long sceNetAtoi(@CanBeNull pspsharp.HLE.PspString string)
		[HLEFunction(nid : 0x1FB2FDDD, version : 150)]
		public virtual long sceNetAtoi(PspString @string)
		{
			return int.Parse(@string.String);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2A73ADDC, version = 150) public long sceNetStrtoul(@CanBeNull pspsharp.HLE.PspString string, @CanBeNull pspsharp.HLE.TPointer32 endString, int super)
		[HLEFunction(nid : 0x2A73ADDC, version : 150)]
		public virtual long sceNetStrtoul(PspString @string, TPointer32 endString, int @base)
		{
			return Modules.SysclibForKernelModule.strtoul(@string, endString, @base);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE0A81C7C, version = 150) public int sceNetMemcmp(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextNextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer src1Addr, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer src2Addr, int size)
		[HLEFunction(nid : 0xE0A81C7C, version : 150)]
		public virtual int sceNetMemcmp(TPointer src1Addr, TPointer src2Addr, int size)
		{
			return Modules.SysclibForKernelModule.memcmp(src1Addr, src2Addr, size);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF48963C6, version = 150) public int sceNetStrrchr(@CanBeNull pspsharp.HLE.TPointer srcAddr, int c1)
		[HLEFunction(nid : 0xF48963C6, version : 150)]
		public virtual int sceNetStrrchr(TPointer srcAddr, int c1)
		{
			if (srcAddr.Null)
			{
				return 0;
			}
			c1 = c1 & 0xFF;

			IMemoryReader memoryReader = MemoryReader.getMemoryReader(srcAddr.Address, 1);
			int lastOccurence = -1;
			for (int i = 0; true; i++)
			{
				int c2 = memoryReader.readNext();
				if (c1 == c2)
				{
					// Character found
					lastOccurence = i;
				}
				else if (c2 == 0)
				{
					// End of string
					break;
				}
			}

			if (lastOccurence < 0)
			{
				return 0;
			}

			return srcAddr.Address + lastOccurence;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x384EFE14, version = 150) public int sceNet_lib_384EFE14(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer in1Addr, int in1Size, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer in2Addr, int in2Size, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=20, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer outAddr)
		[HLEFunction(nid : 0x384EFE14, version : 150)]
		public virtual int sceNet_lib_384EFE14(TPointer in1Addr, int in1Size, TPointer in2Addr, int in2Size, TPointer outAddr)
		{
			if (in2Size > 64)
			{
				log.warn(string.Format("sceNet_lib_384EFE14 not implemented for size=0x{0:X}", in2Size));
			}

			MessageDigest md;
			try
			{
				md = MessageDigest.getInstance("SHA-1");
			}
			catch (NoSuchAlgorithmException e)
			{
				log.error("sceNet_lib_384EFE14", e);
				return -1;
			}

			sbyte[] in1 = in1Addr.getArray8(in1Size);
			sbyte[] in2 = in2Addr.getArray8(in2Size);

			sbyte[] tmp1 = new sbyte[64];
			sbyte[] tmp2 = new sbyte[64];
			Array.Copy(in2, 0, tmp1, 0, System.Math.Min(in2Size, tmp1.Length));
			Array.Copy(in2, 0, tmp2, 0, System.Math.Min(in2Size, tmp2.Length));
			for (int i = 0; i < tmp1.Length; i++)
			{
				tmp1[i] = (sbyte)(tmp1[i] ^ 0x36);
				tmp2[i] = (sbyte)(tmp2[i] ^ 0x5C);
			}

			md.update(tmp1);
			md.update(in1);
			sbyte[] tmp3 = md.digest();
			md.reset();
			md.update(tmp2);
			md.update(tmp3);
			sbyte[] result = md.digest();

			outAddr.setArray(result, 20);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x4753D878, version = 150) public int sceNetMemmove(@CanBeNull pspsharp.HLE.TPointer dstAddr, pspsharp.HLE.TPointer srcAddr, int size)
		[HLEFunction(nid : 0x4753D878, version : 150)]
		public virtual int sceNetMemmove(TPointer dstAddr, TPointer srcAddr, int size)
		{
			return Modules.SysclibForKernelModule.memmove(dstAddr, srcAddr, size);
		}

		[HLEFunction(nid : 0x8687B5AB, version : 150)]
		public virtual int sceNetVsprintf(CpuState cpu, TPointer buffer, string format, TPointer32 parameters)
		{
			object[] formatParameters = new object[10]; // Assume max. 10 parameters
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(parameters.Address, 4 * formatParameters.Length, 4);
			for (int i = 0; i < formatParameters.Length; i++)
			{
				formatParameters[i] = memoryReader.readNext();
			}

			string formattedString = Modules.SysMemUserForUserModule.hleKernelSprintf(cpu, format, formatParameters);
			Utilities.writeStringZ(buffer.Memory, buffer.Address, formattedString);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetVsprintf returning '{0}'", formattedString));
			}

			return formattedString.Length;
		}

		[HLEFunction(nid : 0x1858883D, version : 150)]
		public virtual int sceNetRand()
		{
			// Has no parameters
			return random.Next();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA93A93E9, version = 150) public int _sce_pspnet_callout_stop(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=36, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer unknown)
		[HLEFunction(nid : 0xA93A93E9, version : 150)]
		public virtual int _sce_pspnet_callout_stop(TPointer unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA8B6205A, version = 150) public int sceNet_lib_A8B6205A(pspsharp.HLE.TPointer unknown1, int unknown2, pspsharp.HLE.TPointer unknown3, int unknown4)
		[HLEFunction(nid : 0xA8B6205A, version : 150)]
		public virtual int sceNet_lib_A8B6205A(TPointer unknown1, int unknown2, TPointer unknown3, int unknown4)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x94B44F26, version = 150) public int _sce_pspnet_spllock()
		[HLEFunction(nid : 0x94B44F26, version : 150)]
		public virtual int _sce_pspnet_spllock()
		{
			// Has no parameters
			return 1;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x515B2F33, version = 150) public int _sce_pspnet_splunlock(int resultFromLock)
		[HLEFunction(nid : 0x515B2F33, version : 150)]
		public virtual int _sce_pspnet_splunlock(int resultFromLock)
		{
			if (resultFromLock <= 0)
			{
				return resultFromLock;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2E005032, version = 150) public int sceNet_lib_2E005032(int unknownCallback)
		[HLEFunction(nid : 0x2E005032, version : 150)]
		public virtual int sceNet_lib_2E005032(int unknownCallback)
		{
			this.adhocSocketAlertCallback = unknownCallback;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB3A48B7F, version = 150) public int sceNet_lib_B3A48B7F(int readCallback, int unknownCallback1)
		[HLEFunction(nid : 0xB3A48B7F, version : 150)]
		public virtual int sceNet_lib_B3A48B7F(int readCallback, int unknownCallback1)
		{
			this.readCallback = readCallback;
			this.unknownCallback1 = unknownCallback1;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1F94AFD9, version = 150) public int sceNet_lib_1F94AFD9(int unknownCallback)
		[HLEFunction(nid : 0x1F94AFD9, version : 150)]
		public virtual int sceNet_lib_1F94AFD9(int unknownCallback)
		{
			this.getReadContextCallback = unknownCallback;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5216CBF5, version = 150) public int sceNetConfigUpInterface(pspsharp.HLE.PspString interfaceName)
		[HLEFunction(nid : 0x5216CBF5, version : 150)]
		public virtual int sceNetConfigUpInterface(PspString interfaceName)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD2422E4D, version = 150) public int sceNetConfigDownInterface(pspsharp.HLE.PspString interfaceName)
		[HLEFunction(nid : 0xD2422E4D, version : 150)]
		public virtual int sceNetConfigDownInterface(PspString interfaceName)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAB7DD9A5, version = 150) public int sceNetConfigSetIfEventFlag(pspsharp.HLE.PspString interfaceName, int eventFlagUid, int bitsToSet)
		[HLEFunction(nid : 0xAB7DD9A5, version : 150)]
		public virtual int sceNetConfigSetIfEventFlag(PspString interfaceName, int eventFlagUid, int bitsToSet)
		{
			if (eventFlagUid == 0)
			{
				return 0;
			}
			return Modules.ThreadManForUserModule.sceKernelSetEventFlag(eventFlagUid, bitsToSet);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDA02F383, version = 150) public int sceNet_lib_DA02F383(pspsharp.HLE.PspString interfaceName, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown)
		[HLEFunction(nid : 0xDA02F383, version : 150)]
		public virtual int sceNet_lib_DA02F383(PspString interfaceName, TPointer32 unknown)
		{
			unknown.setValue(0); // Unknown possible values

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD5B64E37, version = 150) public int sceNet_lib_D5B64E37(pspsharp.HLE.PspString interfaceName, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer ssid, int ssidLength, int adhocChannel)
		[HLEFunction(nid : 0xD5B64E37, version : 150)]
		public virtual int sceNet_lib_D5B64E37(PspString interfaceName, TPointer ssid, int ssidLength, int adhocChannel)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x05D525E4, version = 150) public int sceNet_lib_05D525E4()
		[HLEFunction(nid : 0x05D525E4, version : 150)]
		public virtual int sceNet_lib_05D525E4()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0A5A8751, version = 150) public int sceNet_lib_0A5A8751()
		[HLEFunction(nid : 0x0A5A8751, version : 150)]
		public virtual int sceNet_lib_0A5A8751()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x25CC373A, version = 150) public int _sce_pspnet_callout_init()
		[HLEFunction(nid : 0x25CC373A, version : 150)]
		public virtual int _sce_pspnet_callout_init()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x33B230BD, version = 150) public int sceNet_lib_33B230BD()
		[HLEFunction(nid : 0x33B230BD, version : 150)]
		public virtual int sceNet_lib_33B230BD()
		{
			// Has no parameters
			adhocSocketAlertCallback = 0;
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6B294EE4, version = 150) public int sceNet_lib_6B294EE4(int unknown1, int unknown2)
		[HLEFunction(nid : 0x6B294EE4, version : 150)]
		public virtual int sceNet_lib_6B294EE4(int unknown1, int unknown2)
		{
			// calls adhocSocketAlertCallback
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x757085B0, version = 150) public int sceNet_lib_757085B0(pspsharp.HLE.TPointer unknown1, int unkown2)
		[HLEFunction(nid : 0x757085B0, version : 150)]
		public virtual int sceNet_lib_757085B0(TPointer unknown1, int unkown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7574FDA1, version = 150) public int _sce_pspnet_wakeup(pspsharp.HLE.TPointer32 receivedMessage)
		[HLEFunction(nid : 0x7574FDA1, version : 150)]
		public virtual int _sce_pspnet_wakeup(TPointer32 receivedMessage)
		{
			if (blockedThreads.ContainsKey(receivedMessage.Address))
			{
				int threadUid = blockedThreads[receivedMessage.Address];
				Modules.ThreadManForUserModule.hleUnblockThread(threadUid);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x949F1FBB, version = 150) public int sceNet_lib_949F1FBB()
		[HLEFunction(nid : 0x949F1FBB, version : 150)]
		public virtual int sceNet_lib_949F1FBB()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCA3CF5EB, version = 150) public int _sce_pspnet_thread_enter(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 errorAddr)
		[HLEFunction(nid : 0xCA3CF5EB, version : 150)]
		public virtual int _sce_pspnet_thread_enter(TPointer32 errorAddr)
		{
			int currentThreadId = Modules.ThreadManForUserModule.CurrentThreadID;
			if (!allocatedThreadStructures.ContainsKey(currentThreadId))
			{
				int size = 92;
				int allocateMem = Modules.sceNetIfhandleModule.hleNetMallocInternal(size);
				if (allocateMem < 0)
				{
					errorAddr.setValue(allocateMem);
					return 0;
				}

				RuntimeContext.debugMemory(allocateMem, size);

				Memory.Instance.memset(allocateMem, (sbyte) 0, size);

				allocatedThreadStructures[currentThreadId] = allocateMem;
			}

			errorAddr.setValue(0);

			return allocatedThreadStructures[currentThreadId];
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD60225A3, version = 150) public int sceNet_lib_D60225A3(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=6, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer macAddr)
		[HLEFunction(nid : 0xD60225A3, version : 150)]
		public virtual int sceNet_lib_D60225A3(TPointer macAddr)
		{
			pspNetMacAddress macAddress = new pspNetMacAddress();
			macAddress.read(macAddr);

			return 0x11223344;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF6DB0A0B, version = 150) public int sceNet_lib_F6DB0A0B(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 receivedMessage, int timeout)
		[HLEFunction(nid : 0xF6DB0A0B, version : 150)]
		public virtual int sceNet_lib_F6DB0A0B(TPointer32 receivedMessage, int timeout)
		{
			// Possible return values are 0, 4, 11
			// 4: sceNetAdhocPdpRecv will then return ERROR_NET_ADHOC_THREAD_ABORTED = 0x80410719
			// 11: sceNetAdhocPdpRecv will then return ERROR_NET_ADHOC_TIMEOUT = 0x80410715
			// 5: sceNetAdhocPdpRecv will then return ERROR_NET_ADHOC_SOCKET_ALERTED = 0x80410708
			// 32: sceNetAdhocPdpRecv will then return ERROR_NET_ADHOC_SOCKET_DELETED = 0x80410707
			SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
			thread.wait.Semaphore_id = -1;
			Modules.ThreadManForUserModule.hleBlockCurrentThread(SceKernelThreadInfo.PSP_WAIT_SEMA);
			blockedThreads[receivedMessage.Address] = thread.uid;

	//    	sendDummyMessage(thread);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x03164B12, version = 150) public int sceNet_lib_03164B12()
		[HLEFunction(nid : 0x03164B12, version : 150)]
		public virtual int sceNet_lib_03164B12()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0D633F53, version = 150) public int sceNet_lib_0D633F53()
		[HLEFunction(nid : 0x0D633F53, version : 150)]
		public virtual int sceNet_lib_0D633F53()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x389728AB, version = 150) public int sceNet_lib_389728AB()
		[HLEFunction(nid : 0x389728AB, version : 150)]
		public virtual int sceNet_lib_389728AB()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7BA3ED91, version = 150) public int sceNet_lib_7BA3ED91()
		[HLEFunction(nid : 0x7BA3ED91, version : 150)]
		public virtual int sceNet_lib_7BA3ED91()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA55C914F, version = 150) public int sceNet_lib_A55C914F()
		[HLEFunction(nid : 0xA55C914F, version : 150)]
		public virtual int sceNet_lib_A55C914F()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAFA11338, version = 150) public int sceNet_lib_AFA11338()
		[HLEFunction(nid : 0xAFA11338, version : 150)]
		public virtual int sceNet_lib_AFA11338()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB20F84F8, version = 150) public int sceNet_lib_B20F84F8()
		[HLEFunction(nid : 0xB20F84F8, version : 150)]
		public virtual int sceNet_lib_B20F84F8()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD1BE2CE9, version = 150) public int sceNetConfigGetIfEvent(pspsharp.HLE.PspString interfaceName, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 eventAddr, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown)
		[HLEFunction(nid : 0xD1BE2CE9, version : 150)]
		public virtual int sceNetConfigGetIfEvent(PspString interfaceName, TPointer32 eventAddr, TPointer32 unknown)
		{
			// Possible return values in eventAddr:
			// - 4 (WLAN switch off / 0x80410B03)
			// - 5 (WLAN beacon lost / 0x80410B0E)
			// - 7 (???)

			// Returns 0x80410184 if no event is available?
			return SceKernelErrors.ERROR_NET_NO_EVENT;
		}

		/*
		 * Same as sceNetMemmove, but with src and dst pointers swapped
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x2F305274, version = 150) public int sceNetBcopy(@CanBeNull pspsharp.HLE.TPointer srcAddr, pspsharp.HLE.TPointer dstAddr, int size)
		[HLEFunction(nid : 0x2F305274, version : 150)]
		public virtual int sceNetBcopy(TPointer srcAddr, TPointer dstAddr, int size)
		{
			return sceNetMemmove(dstAddr, srcAddr, size);
		}
	}
}