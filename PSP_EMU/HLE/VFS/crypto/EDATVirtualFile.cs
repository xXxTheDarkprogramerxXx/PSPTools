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
namespace pspsharp.HLE.VFS.crypto
{

	public class EDATVirtualFile : PGDVirtualFile
	{
		private const int edatHeaderSize = 0x90;

		public EDATVirtualFile(IVirtualFile pgdFile) : base(null, pgdFile, edatHeaderSize)
		{
		}

		protected internal override bool isHeaderValid(IVirtualFile pgdFile)
		{
			sbyte[] header = new sbyte[edatHeaderSize];
			long position = pgdFile.Position;
			int Length = pgdFile.ioRead(header, 0, edatHeaderSize);
			pgdFile.ioLseek(position);

			if (Length != edatHeaderSize)
			{
				return false;
			}

			if (header[0] != 0 || header[1] != (sbyte)'P' || header[2] != (sbyte)'S' || header[3] != (sbyte)'P' || header[4] != (sbyte)'E' || header[5] != (sbyte)'D' || header[6] != (sbyte)'A' || header[7] != (sbyte)'T')
			{
				// No "EDAT" found in the header,
				Console.WriteLine("PSPEDAT header not found!");
				return false;
			}

			return base.isHeaderValid(pgdFile);
		}
	}

}