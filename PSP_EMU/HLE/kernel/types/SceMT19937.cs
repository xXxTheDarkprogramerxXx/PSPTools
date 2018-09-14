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
	using sceMt19937 = pspsharp.HLE.modules.sceMt19937;

	public class SceMT19937 : pspAbstractMemoryMappedStructure
	{
		public int mti = sceMt19937.MT19937.N + 1;
		public int[] mt = new int[sceMt19937.MT19937.N];

		protected internal override void read()
		{
			mti = read32();
			read32Array(mt);
		}

		protected internal override void write()
		{
			write32(mti);
			write32Array(mt);
		}

		public override int @sizeof()
		{
			return sceMt19937.MT19937.N * 4 + 4;
		}
	}

}