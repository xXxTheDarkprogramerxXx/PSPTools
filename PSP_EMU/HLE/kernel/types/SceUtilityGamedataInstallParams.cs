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
	public class SceUtilityGamedataInstallParams : pspUtilityBaseDialog
	{
		public int unk1;
		public string gameName;
		public string dataName;
		public string gamedataParamsGameTitle;
		public string gamedataParamsDataTitle;
		public string gamedataParamsData;
		public int unk2;
		public int unkResult1;
		public int unkResult2;

		protected internal override void read()
		{
			@base = new pspUtilityDialogCommon();
			read(@base);
			MaxSize = @base.totalSizeof();

			unk1 = read32();
			gameName = readStringNZ(13);
			readUnknown(3);
			dataName = readStringNZ(20);
			gamedataParamsGameTitle = readStringNZ(128);
			gamedataParamsDataTitle = readStringNZ(128);
			gamedataParamsData = readStringNZ(1024);
			unk2 = read8();
			readUnknown(7);
			unkResult1 = read32();
			unkResult2 = read32();
			readUnknown(48);
		}

		protected internal override void write()
		{
			write(@base);
			MaxSize = @base.totalSizeof();

			write32(unk1);
			writeStringNZ(13, gameName);
			writeUnknown(3);
			writeStringNZ(20, dataName);
			writeStringNZ(128, gamedataParamsGameTitle);
			writeStringNZ(128, gamedataParamsDataTitle);
			writeStringNZ(1024, gamedataParamsData);
			write8((sbyte) unk2);
			writeUnknown(7);
			write32(unkResult1);
			write32(unkResult2);
			writeUnknown(48);
		}

		public override int @sizeof()
		{
			return @base.totalSizeof();
		}

		public override string ToString()
		{
			return string.Format("unk1=0x{0:X8}, gameName='{1}', dataName='{2}', gameTitle='{3}', dataTitle='{4}', data='{5}', unk2=0x{6:X2}", unk1, gameName, dataName, gamedataParamsGameTitle, gamedataParamsDataTitle, gamedataParamsData, unk2);
		}
	}
}