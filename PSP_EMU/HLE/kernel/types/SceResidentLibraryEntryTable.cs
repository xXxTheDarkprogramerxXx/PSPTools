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
	using Utilities = pspsharp.util.Utilities;

	public class SceResidentLibraryEntryTable : pspAbstractMemoryMappedStructure
	{
		public TPointer libNameAddr;
		public string libName;
		public readonly sbyte[] version = new sbyte[2];
		public int attribute;
		public int len;
		public int vStubCount;
		public int stubCount;
		public TPointer entryTable;
		public int vStubCountNew;
		public int unknown18;
		public int unknown19;

		protected internal override void read()
		{
			libNameAddr = readPointer();
			if (libNameAddr != null && libNameAddr.NotNull)
			{
				libName = Utilities.readStringZ(libNameAddr.Address);
			}
			read8Array(version);
			attribute = read16();
			len = read8();
			vStubCount = read8();
			stubCount = read16();
			entryTable = readPointer();
			if (len > 4)
			{
				vStubCountNew = read16();
				unknown18 = read8();
				unknown19 = read8();
			}
		}

		protected internal override void write()
		{
			writePointer(libNameAddr);
			write8Array(version);
			write16((short) attribute);
			write8((sbyte) len);
			write8((sbyte) vStubCount);
			write16((short) stubCount);
			writePointer(entryTable);
			if (len > 4)
			{
				write16((short) vStubCountNew);
				write8((sbyte) unknown18);
				write8((sbyte) unknown19);
			}
		}

		public override int @sizeof()
		{
			return len << 2;
		}
	}

}