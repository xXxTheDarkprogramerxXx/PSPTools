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
	public class SceUtilityInstallParams : pspUtilityBaseDialog
	{
		public int unknown1;
		public string gameName; // DISCID
		internal sbyte[] key = new sbyte[0x10];

		protected internal override void read()
		{
			@base = new pspUtilityDialogCommon();
			read(@base);
			MaxSize = @base.totalSizeof();

			unknown1 = read32();
			gameName = readStringNZ(13);
			readUnknown(3);
			readUnknown(16);
			read8Array(key);
		}

		protected internal override void write()
		{
			write(@base);
			MaxSize = @base.totalSizeof();

			write32(unknown1);
			writeStringNZ(13, gameName);
			writeUnknown(3);
			writeUnknown(16);
			write8Array(key);
		}

		public override int @sizeof()
		{
			return @base.totalSizeof();
		}

		public override string ToString()
		{
			return string.Format("Address 0x{0:X8}, unknown1={1:D}, gameName={2}", BaseAddress, unknown1, gameName);
		}
	}

}