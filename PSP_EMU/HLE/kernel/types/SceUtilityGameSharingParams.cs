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
	using sceNetAdhocctl = pspsharp.HLE.modules.sceNetAdhocctl;
	using Utilities = pspsharp.util.Utilities;

	public class SceUtilityGameSharingParams : pspUtilityBaseDialog
	{
		public string gameSharingName;
		public int uploadCallbackArg;
		public int uploadCallbackAddr; // Predefined callback used for data upload (params: uploadCallbackArg, pointer to gameSharingDataAddr, pointer to gameSharingDataSize).
		public int result;
		public int gameSharingFilepathAddr;
		public string gameSharingFilepath; // File path to the game's EBOOT.BIN.
		public int gameSharingMode; // GameSharing mode: 1 - Single send; 2 - Multiple sends (up to 4).
		public int gameSharingDataType; // GameSharing data type: 1 - game's EBOOT is a file; 2 - game's EBOOT is in memory.
		public int gameSharingDataAddr; // Pointer to EBOOT.BIN data.
		public int gameSharingDataSize; // EBOOT.BIN data's size.

		protected internal override void read()
		{
			@base = new pspUtilityDialogCommon();
			read(@base);
			MaxSize = @base.totalSizeof();

			readUnknown(8);
			gameSharingName = readStringNZ(sceNetAdhocctl.GROUP_NAME_LENGTH);
			readUnknown(4);
			uploadCallbackArg = read32();
			uploadCallbackAddr = read32();
			result = read32();
			gameSharingFilepathAddr = read32();
			if (gameSharingFilepathAddr != 0)
			{
				gameSharingFilepath = Utilities.readStringNZ(gameSharingFilepathAddr, 32);
			}
			else
			{
				gameSharingFilepath = null;
			}
			gameSharingMode = read32();
			gameSharingDataType = read32();
			gameSharingDataAddr = read32();
			gameSharingDataSize = read32();
		}

		protected internal override void write()
		{
			write(@base);
			MaxSize = @base.totalSizeof();

			writeUnknown(8);
			writeStringNZ(8, gameSharingName);
			writeUnknown(4);
			write32(uploadCallbackArg);
			write32(uploadCallbackAddr);
			write32(result);
			write32(gameSharingFilepathAddr);
			write32(gameSharingMode);
			write32(gameSharingDataType);
			write32(gameSharingDataAddr);
			write32(gameSharingDataSize);
		}

		public override int @sizeof()
		{
			return @base.totalSizeof();
		}

		public override string ToString()
		{
			return string.Format("title={0}, EBOOTAddr=0x{1:X8}, EBOOTSize={2:D}", gameSharingName, gameSharingDataAddr, gameSharingDataSize);
		}
	}
}