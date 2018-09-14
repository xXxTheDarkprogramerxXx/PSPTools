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
namespace pspsharp.mediaengine
{

	using Logger = org.apache.log4j.Logger;

	using IMMIOHandler = pspsharp.memory.mmio.IMMIOHandler;
	using MMIO = pspsharp.memory.mmio.MMIO;
	using MMIOHandlerReadWrite = pspsharp.memory.mmio.MMIOHandlerReadWrite;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	/// <summary>
	/// The PSP Media Engine memory:
	/// - 0x00000000 - 0x001FFFFF (2MB): ME internal RAM
	/// - access to the main memory
	/// - access to the MMIO
	/// - access to the Media Engine DSP processor
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class MEMemory : MMIO
	{
		private const int STATE_VERSION = 0;
		public const int START_ME_RAM = 0x00000000;
		public const int END_ME_RAM = 0x001FFFFF;
		public static readonly int SIZE_ME_RAM = END_ME_RAM - START_ME_RAM + 1;
		private readonly IMMIOHandler[] meRamHandlers = new IMMIOHandler[8];

		public MEMemory(Memory mem, Logger log) : base(mem)
		{

			// This array will store the contents of the ME RAM and
			// will be shared between several handlers at different addresses.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int meRam[] = new int[SIZE_ME_RAM >> 2];
			int[] meRam = new int[SIZE_ME_RAM >> 2];

			// The ME RAM  is visible at address range 0x00000000-0x001FFFFF
			addMeRamHandler(0x00000000, meRam, log);

			// The same memory is also visible at address range 0x40000000-0x401FFFFF
			addMeRamHandler(0x40000000, meRam, log);

			// The same memory is also visible at address range 0x80000000-0x801FFFFF
			addMeRamHandler(unchecked((int)0x80000000), meRam, log);

			// The same memory is also visible at address range 0xA0000000-0xA01FFFFF
			addMeRamHandler(unchecked((int)0xA0000000), meRam, log);

			addHandlerRW(0x44000000, 0x7070, log);
			addHandlerRW(0x44020000, 0x70, log);
			// TODO This address range is maybe some unknown MMIO?
			addHandlerRW(0x44020FF0, 0x4, log);
			addHandlerRW(0x44022FF0, 0x4, log);
			addHandlerRW(0x44024000, 0x8, log);
			addHandlerRW(0x44026000, 0x8, log);

			addHandler(0x440F8000, 0x194, new MMIOHandlerMe0F8000(0x440F8000));
			addHandler(0x440FF000, 0x30, new MMIOHandlerMe0FF000(0x440FF000));
			addHandler(0x44100000, 0x40, new MMIOHandlerMeDecoderQuSpectra(0x44100000));
		}

		private void addMeRamHandler(int address, int[] meRam, Logger log)
		{
			MMIOHandlerReadWrite handler = new MMIOHandlerReadWrite(START_ME_RAM | address, SIZE_ME_RAM, meRam);
			handler.Logger = log;
			meRamHandlers[(int)((uint)address >> 29)] = handler;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			// The ME RAM need to be read only once
			meRamHandlers[0].read(stream);
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			// The ME RAM need to be written only once
			meRamHandlers[0].write(stream);
			base.write(stream);
		}

		protected internal override IMMIOHandler getHandler(int address)
		{
			// Fast retrieval for the ME RAM
			if ((address & Memory.addressMask) <= END_ME_RAM)
			{
				return meRamHandlers[(int)((uint)address >> 29)];
			}
			return base.getHandler(address);
		}
	}

}