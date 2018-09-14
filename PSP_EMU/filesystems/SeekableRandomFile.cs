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
namespace pspsharp.filesystems
{

	/// 
	/// <summary>
	/// @author gigaherz
	/// </summary>
	public class SeekableRandomFile : RandomAccessFile, SeekableDataInput
	{
		private string fileName;
		private string mode;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SeekableRandomFile(String fileName, String mode) throws java.io.FileNotFoundException
		public SeekableRandomFile(string fileName, string mode) : base(fileName, mode)
		{
			this.fileName = fileName;
			this.mode = mode;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SeekableRandomFile(java.io.File name, String mode) throws java.io.FileNotFoundException
		public SeekableRandomFile(File name, string mode) : base(name, mode)
		{
			this.fileName = name.ToString();
			this.mode = mode;
		}

		public virtual string FileName
		{
			get
			{
				return fileName;
			}
		}

		public virtual string Mode
		{
			get
			{
				return mode;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SeekableRandomFile duplicate() throws java.io.IOException
		public virtual SeekableRandomFile duplicate()
		{
			SeekableRandomFile duplicate = new SeekableRandomFile(fileName, mode);
			duplicate.seek(FilePointer);

			return duplicate;
		}

		public override string ToString()
		{
			return string.Format("SeekableRandomFile '{0}'", fileName);
		}
	}
}