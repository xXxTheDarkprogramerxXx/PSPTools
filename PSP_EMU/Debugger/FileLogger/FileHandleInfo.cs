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
namespace pspsharp.Debugger.FileLogger
{
	/// 
	/// <summary>
	/// @author fiveofhearts
	/// </summary>
	public class FileHandleInfo : IComparable<FileHandleInfo>
	{
		public int fd;
		public readonly string filename;
		public int bytesRead;
		public int bytesWritten;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool isOpen_Renamed;
		private int sortId;
		private static int nextSortId = 0;

		public FileHandleInfo(int fd, string filename)
		{
			this.fd = fd;
			this.filename = filename;
			bytesRead = 0;
			bytesWritten = 0;

			isOpen_Renamed = true;
			sortId = nextSortId++;
		}

		public virtual void isOpen(bool isOpen)
		{
			this.isOpen_Renamed = isOpen;
		}

		public virtual bool Open
		{
			get
			{
				return isOpen_Renamed;
			}
		}

		/// <summary>
		/// For sort by time opened </summary>
		public virtual int CompareTo(FileHandleInfo obj)
		{
			return (sortId - obj.sortId);
		}
	}

}