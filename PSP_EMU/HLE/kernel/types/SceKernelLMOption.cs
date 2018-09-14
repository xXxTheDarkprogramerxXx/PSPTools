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
	public class SceKernelLMOption : pspAbstractMemoryMappedStructureVariableLength
	{
		public int mpidText;
		public int mpidData;
		public int flags;
		public int position;
		public int access;
		public int creserved;

		protected internal override void read()
		{
			base.read();
			mpidText = read32();
			mpidData = read32();
			flags = read32();
			position = read8();
			access = read8();
			creserved = read16();
		}

		protected internal override void write()
		{
			base.write();
			write32(mpidText);
			write32(mpidData);
			write32(flags);
			write8((sbyte)position);
			write8((sbyte)access);
			write16((short)creserved);
		}

		public override string ToString()
		{
			return string.Format("mpidText=0x{0:X}, mpidData=0x{1:X}, flags=0x{2:X}, position=0x{3:X}, access=0x{4:X}, creserved=0x{5:X}", mpidText, mpidData, flags, position, access, creserved);
		}
	}
}