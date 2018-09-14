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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocctl.GROUP_NAME_LENGTH;

	public class SceNetAdhocDiscoverParam : pspAbstractMemoryMappedStructure
	{
		public int unknown1;
		public string groupName;
		public int unknown2;
		public int result;

		public const int NET_ADHOC_DISCOVER_RESULT_NO_PEER_FOUND = 0;
		public const int NET_ADHOC_DISCOVER_RESULT_PEER_FOUND = 2;
		public const int NET_ADHOC_DISCOVER_RESULT_ABORTED = 3;

		protected internal override void read()
		{
			unknown1 = read32();
			groupName = readStringNZ(GROUP_NAME_LENGTH);
			unknown2 = read32();
			result = read32();
		}

		protected internal override void write()
		{
			write32(unknown1);
			writeStringNZ(GROUP_NAME_LENGTH, groupName);
			write32(unknown2);
			write32(result);
		}

		public override int @sizeof()
		{
			return 20;
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();

			// When the base address is not set, return the MAC address only:
			// "nn:nn:nn:nn:nn:nn"
			if (BaseAddress != 0)
			{
				s.Append(string.Format("0x{0:X8}(", BaseAddress));
			}
			s.Append(string.Format("SceNetAdhocDiscoverParam unknown1=0x{0:X8}, groupName='{1}', unknown2=0x{2:X8}, result=0x{3:X8}", unknown1, groupName, unknown2, result));
			if (BaseAddress != 0)
			{
				s.Append(")");
			}

			return s.ToString();
		}
	}

}