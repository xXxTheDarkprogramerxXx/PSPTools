using System.Text;

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

	public class SceMp4AvcNalStruct : pspAbstractMemoryMappedStructure
	{
		public int spsBuffer;
		public int spsSize;
		public int ppsBuffer;
		public int ppsSize;
		public int nalPrefixSize;
		public int nalBuffer;
		public int nalSize;
		public int mode;

		protected internal override void read()
		{
			spsBuffer = read32();
			spsSize = read32();
			ppsBuffer = read32();
			ppsSize = read32();
			nalPrefixSize = read32();
			nalBuffer = read32();
			nalSize = read32();
			mode = read32();
		}

		protected internal override void write()
		{
			write32(spsBuffer);
			write32(spsSize);
			write32(ppsBuffer);
			write32(ppsSize);
			write32(nalPrefixSize);
			write32(nalBuffer);
			write32(nalSize);
			write32(mode);
		}

		public override int @sizeof()
		{
			return 32;
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();

			if (spsBuffer != 0 && spsSize > 0)
			{
				s.Append(string.Format("SPS Buffer: {0}", Utilities.getMemoryDump(spsBuffer, spsSize)));
			}
			if (ppsBuffer != 0 && ppsSize > 0)
			{
				s.Append(string.Format(", PPS Buffer: {0}", Utilities.getMemoryDump(ppsBuffer, ppsSize)));
			}
			s.Append(string.Format(", NAL prefix size 0x{0:X}", nalPrefixSize));
			if (nalBuffer != 0 && nalSize > 0)
			{
				s.Append(string.Format(", NAL Buffer: {0}", Utilities.getMemoryDump(nalBuffer, nalSize)));
			}
			s.Append(string.Format(", mode 0x{0:X}", mode));

			return s.ToString();
		}
	}

}