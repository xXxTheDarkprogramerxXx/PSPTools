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

	public class SceNandSpare : pspAbstractMemoryMappedStructure
	{
		public int[] userEcc = new int[3];
		public int reserved1;
		public int blockFmt;
		public int blockStat;
		public int lbn;
		public int id;
		public int spareEcc;
		public int[] reserved2 = new int[2];

		protected internal override void read()
		{
			userEcc[0] = read8(); // Offset 0
			userEcc[1] = read8(); // Offset 1
			userEcc[2] = read8(); // Offset 2
			reserved1 = read8(); // Offset 3
			blockFmt = read8(); // Offset 4
			blockStat = read8(); // Offset 5
			lbn = endianSwap16((short) read16()); // Offset 6
			id = read32(); // Offset 8
			spareEcc = read16(); // Offset 12
			reserved2[0] = read8(); // Offset 14
			reserved2[1] = read8(); // Offset 15
		}

		protected internal override void write()
		{
			write8((sbyte) userEcc[0]);
			write8((sbyte) userEcc[1]);
			write8((sbyte) userEcc[2]);
			write8((sbyte) reserved1);
			write8((sbyte) blockFmt);
			write8((sbyte) blockStat);
			write16((short) endianSwap16((short) lbn));
			write32(id);
			write16((short) spareEcc);
			write8((sbyte) reserved2[0]);
			write8((sbyte) reserved2[1]);
		}

		public virtual void readNoUserEcc(TPointer buffer, int offset)
		{
			blockFmt = buffer.getValue8(offset + 0) & 0xFF;
			blockStat = buffer.getValue8(offset + 1) & 0xFF;
			lbn = endianSwap16(buffer.getValue16(offset + 2));
			id = buffer.getValue32(offset + 4);
			spareEcc = buffer.getValue16(offset + 8) & 0xFFFF;
			reserved2[0] = buffer.getValue8(offset + 10) & 0xFF;
			reserved2[1] = buffer.getValue8(offset + 11) & 0xFF;
		}

		public virtual void writeNoUserEcc(TPointer buffer, int offset)
		{
			buffer.setValue8(offset + 0, (sbyte) blockFmt);
			buffer.setValue8(offset + 1, (sbyte) blockStat);
			buffer.setValue16(offset + 2, (short) endianSwap16((short) lbn));
			buffer.setValue32(offset + 4, id);
			buffer.setValue16(offset + 8, (short) spareEcc);
			buffer.setValue8(offset + 10, (sbyte) reserved2[0]);
			buffer.setValue8(offset + 11, (sbyte) reserved2[1]);
		}

		public virtual int sizeofNoEcc()
		{
			return 12;
		}

		public override int @sizeof()
		{
			return 16;
		}
	}

}