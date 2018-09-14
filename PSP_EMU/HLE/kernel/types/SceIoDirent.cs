using System;

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

	/// <summary>
	/// http://psp.jim.sh/pspsdk-doc/structSceIoDirent.html </summary>
	public class SceIoDirent : pspAbstractMemoryMappedStructure
	{
		public SceIoStat stat;
		public string filename;
		public int extendedInfoAddr; // This field is a pointer to user data and only used by the MemoryStick driver.
		public int dummy; // Always 0.
		private bool useExtendedInfo;

		public SceIoDirent(SceIoStat stat, string filename)
		{
			this.stat = stat;
			this.filename = filename;
		}

		protected internal override void read()
		{
			stat = new SceIoStat();
			read(stat);
			filename = readStringNZ(256);
			extendedInfoAddr = read32();
			dummy = read32();
		}

		// Convert a "long" file name into a 8.3 file name.
		// TODO Implement correct 8.3 truncation.
		private static string convertFileNameTo83(string fileName)
		{
			if (string.ReferenceEquals(fileName, null))
			{
				return null;
			}

			// Special character '+' is turned into '_'
			fileName = fileName.Replace("+", "_");
			// File name is upper-cased
			fileName = fileName.ToUpper();

			// Split into the name and extension parts
			int lastDot = fileName.LastIndexOf(".", StringComparison.Ordinal);
			string name;
			string ext;
			if (lastDot < 0)
			{
				name = fileName;
				ext = "";
			}
			else
			{
				name = fileName.Substring(0, lastDot);
				ext = fileName.Substring(lastDot + 1);
			}

			// All dots in name part are dropped
			name = name.Replace(".", "");

			// The file extension is truncated to 3 characters
			if (ext.Length > 3)
			{
				ext = ext.Substring(0, 3);
			}

			// The name is truncated to 6 characters (if longer than 8 characters)
			// followed by "~1"
			if (name.Length > 8)
			{
				name = name.Substring(0, 6) + "~1";
			}

			// TODO Check if there is a name collision with "~1"

			return name + "." + ext;
		}

		protected internal override void write()
		{
			write(stat);
			writeStringNZ(256, filename);
			// NOTE: Do not overwrite the reserved field.
			// Tests confirm that sceIoDread only writes the stat and filename.

			if (useExtendedInfo)
			{
				if (extendedInfoAddr == 0)
				{
					extendedInfoAddr = read32();
				}
				if (Memory.isAddressGood(extendedInfoAddr))
				{
					if (Modules.SysMemUserForUserModule.hleKernelGetCompiledSdkVersion() <= 0x0307FFFF)
					{
						// "reserved" is pointing to an area of unknown size
						// - [0..12] "8.3" file name (null-terminated)
						// - [13..???] long file name (null-terminated)
						Utilities.writeStringNZ(mem, extendedInfoAddr + 0, 13, convertFileNameTo83(filename));
						Utilities.writeStringZ(mem, extendedInfoAddr + 13, filename);
					}
					else
					{
						// "reserved" is pointing to an area of total size 1044
						// - [0..3] size of area
						// - [4..19] "8.3" file name (null-terminated)
						// - [20..???] long file name (null-terminated)
						int size = mem.read32(extendedInfoAddr);
						if (size == 1044)
						{
							Utilities.writeStringNZ(mem, extendedInfoAddr + 4, 13, convertFileNameTo83(filename));
							Utilities.writeStringNZ(mem, extendedInfoAddr + 20, 1024, filename);
						}
					}
				}
			}
		}

		public virtual bool UseExtendedInfo
		{
			set
			{
				this.useExtendedInfo = value;
			}
		}

		public override int @sizeof()
		{
			return SceIoStat.SIZEOF + 256 + 8;
		}
	}

}