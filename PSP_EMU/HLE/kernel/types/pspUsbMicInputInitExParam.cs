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
	/*
	 * Parameter Structure for sceUsbMicInputInitEx().
	 */
	public class pspUsbMicInputInitExParam : pspAbstractMemoryMappedStructure
	{
		public int unknown1;
		public int unknown2;
		public int unknown3;
		public int unknown4;
		public int unknown5;
		public int unknown6;

		protected internal override void read()
		{
			unknown1 = read16();
			readUnknown(2);
			unknown2 = read16();
			readUnknown(2);
			unknown3 = read16();
			readUnknown(2);
			unknown4 = read16();
			readUnknown(2);
			unknown5 = read16();
			readUnknown(2);
			unknown6 = read16();
			readUnknown(2);
		}

		protected internal override void write()
		{
			write16((short) unknown1);
			write16((short) 0);
			write16((short) unknown2);
			write16((short) 0);
			write16((short) unknown3);
			write16((short) 0);
			write16((short) unknown4);
			write16((short) 0);
			write16((short) unknown5);
			write16((short) 0);
			write16((short) unknown6);
			write16((short) 0);
		}

		public override int @sizeof()
		{
			return 24;
		}

		public override string ToString()
		{
			return string.Format("unknown1=0x{0:X}, unknown2=0x{1:X}, unknown3=0x{2:X}, unknown4=0x{3:X}, unknown5=0x{4:X}, unknown6=0x{5:X}", unknown1, unknown2, unknown3, unknown4, unknown5, unknown6);
		}
	}

}