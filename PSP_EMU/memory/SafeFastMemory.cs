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
namespace pspsharp.memory
{


	public class SafeFastMemory : FastMemory
	{
		//
		// This class is using the FastMemory implementation but
		// additionally checks the validity of the address for each access.
		//
		private bool isAddressGood(int address, int Length)
		{
			return isAddressGood(address) && isAddressGood(address + Length - 1);
		}

		public override int read8(int address)
		{
			if (!isAddressGood(address))
			{
				int normalizedAddress = normalizeAddress(address);
				if (isRawAddressGood(normalizedAddress))
				{
					address = normalizedAddress;
				}
				else
				{
					invalidMemoryAddress(address, "read8", Emulator.EMU_STATUS_MEM_READ);
					return 0;
				}
			}

			return base.read8(address);
		}

		public override int read16(int address)
		{
			if (!isAddressGood(address))
			{
				int normalizedAddress = normalizeAddress(address);
				if (isRawAddressGood(normalizedAddress))
				{
					address = normalizedAddress;
				}
				else
				{
					invalidMemoryAddress(address, "read16", Emulator.EMU_STATUS_MEM_READ);
					return 0;
				}
			}

			return base.read16(address);
		}

		public override int read32(int address)
		{
			if (!isAddressGood(address))
			{
				if (read32AllowedInvalidAddress(address))
				{
					return 0;
				}

				int normalizedAddress = normalizeAddress(address);
				if (isRawAddressGood(normalizedAddress))
				{
					address = normalizedAddress;
				}
				else
				{
					invalidMemoryAddress(address, "read32", Emulator.EMU_STATUS_MEM_READ);
					return 0;
				}
			}

			return base.read32(address);
		}

		public override long read64(int address)
		{
			if (!isAddressGood(address))
			{
				int normalizedAddress = normalizeAddress(address);
				if (isRawAddressGood(normalizedAddress))
				{
					address = normalizedAddress;
				}
				else
				{
					invalidMemoryAddress(address, "read64", Emulator.EMU_STATUS_MEM_READ);
					return 0;
				}
			}

			return base.read64(address);
		}

		public override void write8(int address, sbyte data)
		{
			if (!isAddressGood(address))
			{
				int normalizedAddress = normalizeAddress(address);
				if (isRawAddressGood(normalizedAddress))
				{
					address = normalizedAddress;
				}
				else
				{
					invalidMemoryAddress(address, "write8", Emulator.EMU_STATUS_MEM_WRITE);
					return;
				}
			}

			base.write8(address, data);
		}

		public override void write16(int address, short data)
		{
			if (!isAddressGood(address))
			{
				int normalizedAddress = normalizeAddress(address);
				if (isRawAddressGood(normalizedAddress))
				{
					address = normalizedAddress;
				}
				else
				{
					invalidMemoryAddress(address, "write16", Emulator.EMU_STATUS_MEM_WRITE);
					return;
				}
			}

			base.write16(address, data);
		}

		public override void write32(int address, int data)
		{
			if (!isAddressGood(address))
			{
				int normalizedAddress = normalizeAddress(address);
				if (isRawAddressGood(normalizedAddress))
				{
					address = normalizedAddress;
				}
				else
				{
					invalidMemoryAddress(address, "write32", Emulator.EMU_STATUS_MEM_WRITE);
					return;
				}
			}

			base.write32(address, data);
		}

		public override void write64(int address, long data)
		{
			if (!isAddressGood(address))
			{
				int normalizedAddress = normalizeAddress(address);
				if (isRawAddressGood(normalizedAddress))
				{
					address = normalizedAddress;
				}
				else
				{
					invalidMemoryAddress(address, "write64", Emulator.EMU_STATUS_MEM_WRITE);
					return;
				}
			}

			base.write64(address, data);
		}

		public override void memset(int address, sbyte data, int Length)
		{
			if (Length <= 0)
			{
				return;
			}

			if (!isAddressGood(address, Length))
			{
				invalidMemoryAddress(address, "memset", Emulator.EMU_STATUS_MEM_WRITE);
				return;
			}

			base.memset(address, data, Length);
		}

		public override void copyToMemory(int address, ByteBuffer source, int Length)
		{
			if (!isAddressGood(address, Length))
			{
				invalidMemoryAddress(address, "copyToMemory", Emulator.EMU_STATUS_MEM_WRITE);
				return;
			}

			base.copyToMemory(address, source, Length);
		}

		public override IntBuffer getBuffer(int address, int Length)
		{
			if (!isAddressGood(address, Length))
			{
				if (isAddressGood(address) && address >= MemoryMap.START_VRAM && address <= MemoryMap.END_VRAM)
				{
					// Accept loading a texture e.g. at address 0x4154000 with Length 0x100000
					// The address 0x42xxxxx should map to 0x40xxxxx but we ignore this here
					// because we cannot build a buffer starting at 0x4154000 and ending
					// at 0x4054000.
				}
				else
				{
					invalidMemoryAddress(address, "getBuffer", Emulator.EMU_STATUS_MEM_READ);
					return null;
				}
			}

			return base.getBuffer(address, Length);
		}

		public override void memcpy(int destination, int source, int Length, bool checkOverlap)
		{
			if (Length <= 0)
			{
				return;
			}

			if (!isAddressGood(destination, Length))
			{
				invalidMemoryAddress(destination, Length, "memcpy", Emulator.EMU_STATUS_MEM_WRITE);
				return;
			}
			if (!isAddressGood(source, Length))
			{
				invalidMemoryAddress(source, Length, "memcpy", Emulator.EMU_STATUS_MEM_READ);
				return;
			}

			base.memcpy(destination, source, Length, checkOverlap);
		}
	}

}