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
	public class SceNetIfMessage : pspAbstractMemoryMappedStructure
	{
		public const int TYPE_SHORT_MESSAGE = 2;
		public static readonly int TYPE_LONG_MESSAGE = 9 | TYPE_SHORT_MESSAGE;
		public const int TYPE_UNKNOWN_100 = 0x100;
		public int nextDataAddr;
		public int nextMessageAddr;
		public int dataAddr;
		public int dataLength;
		public int unknown16;
		public int unknown18;
		public int unknown20;
		public int unknown24;
		public int unknown28;
		public int unknown48;
		public int unknown60;
		public int unknown68;
		public int unknown72;

		protected internal override void read()
		{
			nextDataAddr = read32(); // Offset 0
			nextMessageAddr = read32(); // Offset 4
			dataAddr = read32(); // Offset 8
			dataLength = read32(); // Offset 12
			unknown16 = read16(); // Offset 16
			unknown18 = read16(); // Offset 18
			unknown20 = read32(); // Offset 20
			unknown24 = read32(); // Offset 24
			unknown28 = read32(); // Offset 28
			readUnknown(16); // Offset 32
			unknown48 = read32(); // Offset 48
			readUnknown(8); // Offset 52
			unknown60 = read32(); // Offset 60
			readUnknown(4); // Offset 64
			unknown68 = read32(); // Offset 68
			unknown72 = read32(); // Offset 72
		}

		protected internal override void write()
		{
			write32(nextDataAddr);
			write32(nextMessageAddr);
			write32(dataAddr);
			write32(dataLength);
			write16((short) unknown16);
			write16((short) unknown18);
			write32(unknown20);
			write32(unknown24);
			write32(unknown28);
			writeSkip(16);
			write32(unknown48);
			writeSkip(8);
			write32(unknown60);
			writeSkip(4);
			write32(unknown68);
			write32(unknown72);
		}

		public override int @sizeof()
		{
			return 76;
		}

		public override string ToString()
		{
			return string.Format("nextDataAddr=0x{0:X8}, nextMessageAddr=0x{1:X8}, dataAddr=0x{2:X8}, dataLength=0x{3:X}, unknown16=0x{4:X}, unknown18=0x{5:X}, unknown20=0x{6:X}, unknown24=0x{7:X}, unknown28=0x{8:X}", nextDataAddr, nextMessageAddr, dataAddr, dataLength, unknown16, unknown18, unknown20, unknown24, unknown28);
		}
	}

}