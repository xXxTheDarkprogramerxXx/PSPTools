using System;
using System.Collections.Generic;

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
namespace pspsharp.filesystems.umdiso.iso9660
{


	/// 
	/// <summary>
	/// @author gigaherz
	/// </summary>
	public class Iso9660Directory
	{
		private readonly IList<Iso9660File> files;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Iso9660Directory(pspsharp.filesystems.umdiso.UmdIsoReader r, int directorySector, long directorySize) throws java.io.IOException
		public Iso9660Directory(UmdIsoReader r, int directorySector, long directorySize)
		{
			// parse directory sector
			UmdIsoFile dataStream = new UmdIsoFile(r, directorySector, directorySize, null, null);

			files = new List<Iso9660File>();

			sbyte[] b = new sbyte[256];

			while (directorySize >= 1)
			{
				int entryLength = dataStream.read();

				// This is assuming that the padding bytes are always filled with 0's.
				if (entryLength == 0)
				{
					directorySize--;
					continue;
				}

				directorySize -= entryLength;
				int readLength = dataStream.read(b, 0, entryLength - 1);
				Iso9660File file = new Iso9660File(b, readLength, r.hasJolietExtension());

				files.Add(file);
			}

			dataStream.Dispose();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Iso9660File getEntryByIndex(int index) throws ArrayIndexOutOfBoundsException
		public virtual Iso9660File getEntryByIndex(int index)
		{
			return files[index];
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getFileIndex(String fileName) throws java.io.FileNotFoundException
		public virtual int getFileIndex(string fileName)
		{
			int i = 0;
			foreach (Iso9660File file in files)
			{
				if (file.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
				i++;
			}

			throw new FileNotFoundException(string.Format("File '{0}' not found in directory.", fileName));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String[] getFileList() throws java.io.FileNotFoundException
		public virtual string[] FileList
		{
			get
			{
				string[] list = new string[files.Count];
				int i = 0;
				foreach (Iso9660File file in files)
				{
					list[i] = file.FileName;
					i++;
				}
				return list;
			}
		}
	}
}