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
namespace pspsharp.HLE.kernel.types
{

	public class pspUtilityDialogCommon : pspAbstractMemoryMappedStructureVariableLength
	{
		public int language;
		public int buttonSwap;
			public const int BUTTON_ACCEPT_CIRCLE = 0;
			public const int BUTTON_ACCEPT_CROSS = 1;
		public int graphicsThread; // 0x11
		public int accessThread; // 0x13
		public int fontThread; // 0x12
		public int soundThread; // 0x10
		public int result;

		protected internal override void read()
		{
			base.read();
			language = read32();
			buttonSwap = read32();
			graphicsThread = read32();
			accessThread = read32();
			fontThread = read32();
			soundThread = read32();
			result = read32();
			readUnknown(16);
		}

		protected internal override void write()
		{
			base.write();
			write32(language);
			write32(buttonSwap);
			write32(graphicsThread);
			write32(accessThread);
			write32(fontThread);
			write32(soundThread);
			write32(result);
			writeUnknown(16);
		}

		public virtual void writeResult(TPointer baseAddress)
		{
			if (baseAddress != null)
			{
				writeResult(baseAddress.Memory, baseAddress.Address);
			}
		}

		public virtual void writeResult(Memory mem)
		{
			writeResult(mem, BaseAddress);
		}

		public virtual void writeResult(Memory mem, int address)
		{
			mem.write32(address + 28, result);
		}

		public override int @sizeof()
		{
			return System.Math.Min(12 * 4, base.@sizeof());
		}

		public virtual int totalSizeof()
		{
			return base.@sizeof();
		}
	}

}