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

	public class PGDVirtualFile : BufferedVirtualFile
	{
		private bool isValid;
		private bool isHeaderPresent;

		public PGDVirtualFile(sbyte[] key, IVirtualFile pgdFile)
		{
			init(key, pgdFile, 0);
		}

		protected internal PGDVirtualFile(sbyte[] key, IVirtualFile pgdFile, int dataOffset)
		{
			init(key, pgdFile, dataOffset);
		}

		private void init(sbyte[] key, IVirtualFile pgdFile, int dataOffset)
		{
			isValid = false;

			long position = pgdFile.Position;
			if (isHeaderValid(pgdFile))
			{
				PGDBlockVirtualFile pgdBlockFile = new PGDBlockVirtualFile(pgdFile, key, dataOffset);

				isHeaderPresent = pgdBlockFile.HeaderPresent;
				if (pgdBlockFile.HeaderValid)
				{
					setBufferedVirtualFile(pgdBlockFile, pgdBlockFile.BlockSize);
					isValid = true;
				}
			}

			if (!isValid)
			{
				pgdFile.ioLseek(position);
				setBufferedVirtualFile(pgdFile, 0x1000);
			}
		}

		protected internal virtual bool isHeaderValid(IVirtualFile pgdFile)
		{
			return true;
		}

		public virtual bool Valid
		{
			get
			{
				return isValid;
			}
		}

		public virtual bool HeaderPresent
		{
			get
			{
				return isHeaderPresent;
			}
		}
	}

}