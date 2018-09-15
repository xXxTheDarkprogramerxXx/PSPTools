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
namespace pspsharp.memory.mmio
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.MemoryMap.END_IO_1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.MemoryMap.START_IO_0;


	//using Logger = org.apache.log4j.Logger;

	using CY27040 = pspsharp.memory.mmio.cy27040.CY27040;
	using MMIOHandlerUart3 = pspsharp.memory.mmio.uart.MMIOHandlerUart3;
	using MMIOHandlerUart4 = pspsharp.memory.mmio.uart.MMIOHandlerUart4;
	using MMIOHandlerUartBase = pspsharp.memory.mmio.uart.MMIOHandlerUartBase;
	using WM8750 = pspsharp.memory.mmio.wm8750.WM8750;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIO : Memory
	{
		private const int STATE_VERSION = 0;
		private readonly Memory mem;
		private readonly IDictionary<int, IMMIOHandler> handlers = new Dictionary<int, IMMIOHandler>(40000);
		protected internal new static readonly bool[] validMemoryPage = new bool[Memory.validMemoryPage.Length];
		private readonly IDictionary<int, IMMIOHandler> sortedHandlers = new SortedDictionary<int, IMMIOHandler>();

		public MMIO(Memory mem)
		{
			this.mem = mem;
		}

		public override bool allocate()
		{
			Array.Copy(Memory.validMemoryPage, 0, validMemoryPage, 0, validMemoryPage.Length);
			Arrays.Fill(validMemoryPage, (int)((uint)START_IO_0 >> MEMORY_PAGE_SHIFT), ((int)((uint)MemoryMap.END_EXCEPTIO_VEC >> MEMORY_PAGE_SHIFT)) + 1, true);

			return true;
		}

		public override void Initialise()
		{
			handlers.Clear();

			addHandler(unchecked((int)0xBC000000), 0x54, new MMIOHandlerMemoryAccessControl(unchecked((int)0xBC000000)));
			addHandler(MMIOHandlerSystemControl.BASE_ADDRESS, 0x9C, MMIOHandlerSystemControl.Instance);
			addHandler(unchecked((int)0xBC200000), 0x8, new MMIOHandlerCpuBusFrequency(unchecked((int)0xBC200000)));
			addHandler(MMIOHandlerInterruptMan.BASE_ADDRESS, 0x30, MMIOHandlerInterruptMan.ProxyInstance);
			addHandler(unchecked((int)0xBC500000), 0x10, new int[] {0x0100}, new MMIOHandlerTimer(unchecked((int)0xBC500000)));
			addHandler(unchecked((int)0xBC500010), 0x10, new int[] {0x0100}, new MMIOHandlerTimer(unchecked((int)0xBC500010)));
			addHandler(unchecked((int)0xBC500020), 0x10, new int[] {0x0100}, new MMIOHandlerTimer(unchecked((int)0xBC500020)));
			addHandler(unchecked((int)0xBC500030), 0x10, new int[] {0x0100}, new MMIOHandlerTimer(unchecked((int)0xBC500030)));
			addHandler(unchecked((int)0xBC600000), 0x14, new MMIOHandlerSystemTime(unchecked((int)0xBC600000)));
			addHandler(unchecked((int)0xBC800000), 0x1D4, new MMIOHandlerDmacplus(unchecked((int)0xBC800000)));
			addHandler(unchecked((int)0xBC900000), 0x1F4, new MMIOHandlerDmac(unchecked((int)0xBC900000)));
			addHandler(unchecked((int)0xBCA00000), 0x1F4, new MMIOHandlerDmac(unchecked((int)0xBCA00000)));
			addHandler(unchecked((int)0xBCC00000), 0x74, new MMIOHandlerMeController(unchecked((int)0xBCC00000)));
			addHandler(MMIOHandlerDdr.BASE_ADDRESS, 0x48, MMIOHandlerDdr.Instance);
			addHandler(MMIOHandlerNand.BASE_ADDRESS, 0x304, MMIOHandlerNand.Instance);
			addHandler(unchecked((int)0xBD200000), 0x44, new MMIOHandlerMemoryStick(unchecked((int)0xBD200000)));
	//    	addHandler(0xBD300000, 0x44, new MMIOHandlerWlan(0xBD300000));
			addHandlerRW(unchecked((int)0xBD300000), 0x44);
			addHandler(MMIOHandlerGe.BASE_ADDRESS, 0xE50, MMIOHandlerGe.Instance);
			addHandler(unchecked((int)0xBD500000), 0x94, new MMIOHandlerGeEdram(unchecked((int)0xBD500000)));
			addHandler(unchecked((int)0xBD600000), 0x50, new MMIOHandlerAta2(unchecked((int)0xBD600000)));
			addHandler(MMIOHandlerAta.BASE_ADDRESS, 0xF, MMIOHandlerAta.Instance);
			addHandler(MMIOHandlerUsb.BASE_ADDRESS, 0x420, MMIOHandlerUsb.Instance);
			addHandler(unchecked((int)0xBDE00000), 0x3C, new MMIOHandlerKirk(unchecked((int)0xBDE00000)));
			addHandler(MMIOHandlerUmd.BASE_ADDRESS, 0x98, MMIOHandlerUmd.Instance);
			addHandler(unchecked((int)0xBE000000), 0x80, new MMIOHandlerAudio(unchecked((int)0xBE000000)));
			addHandler(unchecked((int)0xBE140000), 0x204, new MMIOHandlerLcdc(unchecked((int)0xBE140000)));
			addHandler(unchecked((int)0xBE200000), 0x30, new MMIOHandlerI2c(unchecked((int)0xBE200000)));
			addHandler(MMIOHandlerGpio.BASE_ADDRESS, 0x4C, MMIOHandlerGpio.Instance);
			addHandler(unchecked((int)0xBE300000), 0x60, new MMIOHandlerPower(unchecked((int)0xBE300000)));
			addHandler(unchecked((int)0xBE4C0000), MMIOHandlerUartBase.SIZE_OF, new MMIOHandlerUart4(unchecked((int)0xBE4C0000)));
			addHandler(unchecked((int)0xBE500000), MMIOHandlerUartBase.SIZE_OF, new MMIOHandlerUart3(unchecked((int)0xBE500000)));
			addHandler(MMIOHandlerSyscon.BASE_ADDRESS, 0x28, MMIOHandlerSyscon.Instance);
			addHandler(MMIOHandlerDisplayController.BASE_ADDRESS, 0x28, MMIOHandlerDisplayController.Instance);
			addHandlerRW(unchecked((int)0xBFC00000), 0x1000);
			write32(unchecked((int)0xBFC00200), 0x2E547106);
			write32(unchecked((int)0xBFC00204), unchecked((int)0xFBDFC08B));
			write32(unchecked((int)0xBFC00208), 0x087FCC08);
			write32(unchecked((int)0xBFC0020C), unchecked((int)0xAA60334E));
			addHandler(MMIOHandlerMeCore.BASE_ADDRESS, 0x2C, MMIOHandlerMeCore.Instance);
			addHandler(MMIOHandlerNandPage.BASE_ADDRESS1, 0x90C, MMIOHandlerNandPage.Instance);
			addHandler(MMIOHandlerNandPage.BASE_ADDRESS2, 0x90C, MMIOHandlerNandPage.Instance);
		}

		protected internal virtual void addHandler(int baseAddress, int Length, IMMIOHandler handler)
		{
			addHandler(baseAddress, Length, null, handler);
		}

		private void addHandler(int baseAddress, int Length, int[] additionalOffsets, IMMIOHandler handler)
		{
			// The handlers will be kept sorted based on their baseAddress
			sortedHandlers[baseAddress] = handler;

			for (int i = 0; i < Length; i++)
			{
				handlers[baseAddress + i] = handler;
			}

			if (additionalOffsets != null)
			{
				foreach (int offset in additionalOffsets)
				{
					handlers[baseAddress + offset] = handler;
				}
			}
		}

		protected internal virtual void addHandlerRW(int baseAddress, int Length)
		{
			addHandlerRW(baseAddress, Length, null);
		}

		protected internal virtual void addHandlerRW(int baseAddress, int Length, Logger log)
		{
			MMIOHandlerReadWrite handler = new MMIOHandlerReadWrite(baseAddress, Length);
			if (log != null)
			{
				handler.Logger = log;
			}
			addHandler(baseAddress, Length, handler);
		}

		protected internal virtual IMMIOHandler getHandler(int address)
		{
			return handlers[address];
		}

		private bool hasHandler(int address)
		{
			return handlers.ContainsKey(address);
		}

		public static bool isAddressGood(int address)
		{
			return validMemoryPage[(int)((uint)address >> MEMORY_PAGE_SHIFT)];
		}

		public override int normalize(int address)
		{
			if (hasHandler(address))
			{
				return address;
			}
			return mem.normalize(address);
		}

		public static int normalizeAddress(int addr)
		{
			// Transform address 0x1nnnnnnn into 0xBnnnnnnn
			if (addr >= (START_IO_0 & Memory.addressMask) && addr <= (END_IO_1 & Memory.addressMask))
			{
				addr |= (START_IO_0 & ~Memory.addressMask);
			}

			return addr;
		}

		public override int read8(int address)
		{
			IMMIOHandler handler = getHandler(address);
			if (handler != null)
			{
				return handler.read8(address);
			}
			return mem.read8(address);
		}

		public override int read16(int address)
		{
			IMMIOHandler handler = getHandler(address);
			if (handler != null)
			{
				return handler.read16(address);
			}
			return mem.read16(address);
		}

		public override int read32(int address)
		{
			IMMIOHandler handler = getHandler(address);
			if (handler != null)
			{
				return handler.read32(address);
			}
			return mem.read32(address);
		}

		public override void write8(int address, sbyte data)
		{
			IMMIOHandler handler = getHandler(address);
			if (handler != null)
			{
				handler.write8(address, data);
			}
			else
			{
				mem.write8(address, data);
			}
		}

		public override void write16(int address, short data)
		{
			IMMIOHandler handler = getHandler(address);
			if (handler != null)
			{
				handler.write16(address, data);
			}
			else
			{
				mem.write16(address, data);
			}
		}

		public override void write32(int address, int data)
		{
			IMMIOHandler handler = getHandler(address);
			if (handler != null)
			{
				handler.write32(address, data);
			}
			else
			{
				mem.write32(address, data);
			}
		}

		public override void memset(int address, sbyte data, int Length)
		{
			mem.memset(address, data, Length);
		}

		public override Buffer MainMemoryByteBuffer
		{
			get
			{
				return mem.MainMemoryByteBuffer;
			}
		}

		public override Buffer getBuffer(int address, int Length)
		{
			return mem.getBuffer(address, Length);
		}

		public override void copyToMemory(int address, ByteBuffer source, int Length)
		{
			mem.copyToMemory(address, source, Length);
		}

		protected internal override void memcpy(int destination, int source, int Length, bool checkOverlap)
		{
			if (((destination | source | Length) & 0x3) == 0 && !checkOverlap)
			{
				for (int i = 0; i < Length; i += 4)
				{
					write32(destination + i, read32(source + i));
				}
			}
			else
			{
				if (checkOverlap)
				{
					mem.memmove(destination, source, Length);
				}
				else
				{
					mem.memcpy(destination, source, Length);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			// The handlers are kept sorted based on their base address
			foreach (int? baseAddress in sortedHandlers.Keys)
			{
				IMMIOHandler handler = sortedHandlers[baseAddress];
				handler.read(stream);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Read State for {0} at 0x{1:X8}", handler, baseAddress));
				}
			}
			CY27040.Instance.read(stream);
			WM8750.Instance.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			// The handlers are kept sorted based on their base address
			foreach (int? baseAddress in sortedHandlers.Keys)
			{
				IMMIOHandler handler = sortedHandlers[baseAddress];
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Writing State for {0} at 0x{1:X8}", handler, baseAddress));
				}
				handler.write(stream);
			}
			CY27040.Instance.write(stream);
			WM8750.Instance.write(stream);
		}
	}

}