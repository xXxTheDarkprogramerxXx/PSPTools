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
	public class Iso9660Handler : Iso9660Directory
	{

		private Iso9660Directory internalDir;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Iso9660Handler(pspsharp.filesystems.umdiso.UmdIsoReader r) throws java.io.IOException
		public Iso9660Handler(UmdIsoReader r) : base(r, 0, 0)
		{

			sbyte[] sector;
			if (r.hasJolietExtension())
			{
				sector = r.readSector(UmdIsoReader.startSectorJoliet);
			}
			else
			{
				sector = r.readSector(UmdIsoReader.startSector);
			}
			System.IO.MemoryStream byteStream = new System.IO.MemoryStream(sector);

			byteStream.skip(157); // reach rootDirTocHeader

			sbyte[] b = new sbyte[38];

			byteStream.Read(b, 0, b.Length);
			Iso9660File rootDirEntry = new Iso9660File(b, b.Length, r.hasJolietExtension());

			int rootLBA = rootDirEntry.LBA;
			long rootSize = rootDirEntry.Size;

			internalDir = new Iso9660Directory(r, rootLBA, rootSize);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Iso9660File getEntryByIndex(int index) throws ArrayIndexOutOfBoundsException
		public override Iso9660File getEntryByIndex(int index)
		{
			return internalDir.getEntryByIndex(index);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int getFileIndex(String fileName) throws java.io.FileNotFoundException
		public override int getFileIndex(string fileName)
		{
			return internalDir.getFileIndex(fileName);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public String[] getFileList() throws java.io.FileNotFoundException
		public override string[] FileList
		{
			get
			{
				return internalDir.FileList;
			}
		}
	}
}