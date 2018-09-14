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
namespace pspsharp.format.rco.vsmx
{
	public class VSMXHeader
	{
		public int sig;
		public int ver;
		public int codeOffset;
		public int codeLength;
		public int textOffset;
		public int textLength;
		public int textEntries;
		public int propOffset;
		public int propLength;
		public int propEntries;
		public int namesOffset;
		public int namesLength;
		public int namesEntries;

		public virtual int size()
		{
			return 13 * 4;
		}
	}

}