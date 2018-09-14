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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.min;

	/// <summary>
	/// Implements a circular buffer in PSP memory that can
	/// be fed from a file.
	/// Used by sceAtrac3plus and sceMp3 modules.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class pspFileBuffer
	{
		private int addr;
		private int maxSize;
		private int currentSize;
		private int readPosition;
		private int writePosition;
		private int filePosition;
		private int fileMaxSize;

		public pspFileBuffer()
		{
		}

		public pspFileBuffer(int addr, int maxSize)
		{
			this.addr = addr;
			this.maxSize = maxSize;
		}

		public pspFileBuffer(int addr, int maxSize, int readSize)
		{
			this.addr = addr;
			this.maxSize = maxSize;
			notifyWrite(readSize);
		}

		public pspFileBuffer(int addr, int maxSize, int readSize, int filePosition)
		{
			this.addr = addr;
			this.maxSize = maxSize;
			notifyWrite(readSize);
			this.filePosition = filePosition;
		}

		public virtual int FileMaxSize
		{
			set
			{
				this.fileMaxSize = value;
			}
		}

		public virtual bool FileEnd
		{
			get
			{
				return filePosition >= fileMaxSize;
			}
		}

		public virtual int WriteAddr
		{
			get
			{
				return addr + writePosition;
			}
		}

		public virtual int WriteSize
		{
			get
			{
				return min(NoFileWriteSize, FileWriteSize);
			}
		}

		public virtual int FileWriteSize
		{
			get
			{
				return fileMaxSize - filePosition;
			}
		}

		public virtual int NoFileWriteSize
		{
			get
			{
				return min(maxSize - currentSize, maxSize - writePosition);
			}
		}

		public virtual int FilePosition
		{
			get
			{
				return filePosition;
			}
			set
			{
				this.filePosition = value;
			}
		}


		public virtual int ReadAddr
		{
			get
			{
				return addr + readPosition;
			}
		}

		public virtual int ReadSize
		{
			get
			{
				return min(currentSize, maxSize - readPosition);
			}
		}

		public virtual int CurrentSize
		{
			get
			{
				return currentSize;
			}
		}

		public virtual void reset(int readSize, int filePosition)
		{
			lock (this)
			{
				currentSize = 0;
				readPosition = 0;
				writePosition = 0;
				this.filePosition = filePosition;
				notifyWrite(readSize);
			}
		}

		public virtual void notifyRead(int size)
		{
			lock (this)
			{
				if (size > 0)
				{
					size = min(size, currentSize);
					readPosition = incrementPosition(readPosition, size);
					currentSize -= size;
				}
			}
		}

		public virtual void notifyReadAll()
		{
			lock (this)
			{
				notifyRead(currentSize);
			}
		}

		public virtual void notifyWrite(int size)
		{
			lock (this)
			{
				if (size > 0)
				{
					size = min(size, MaxSize - currentSize);
					writePosition = incrementPosition(writePosition, size);
					filePosition += size;
					currentSize += size;
				}
			}
		}

		private int incrementPosition(int position, int size)
		{
			position += size;
			if (position >= maxSize)
			{
				position -= maxSize;
			}

			return position;
		}

		public virtual int MaxSize
		{
			get
			{
				return maxSize;
			}
		}

		public virtual int Addr
		{
			get
			{
				return addr;
			}
			set
			{
				this.addr = value;
			}
		}


		public virtual bool Empty
		{
			get
			{
				return currentSize == 0;
			}
		}

		public override string ToString()
		{
			return string.Format("pspFileBuffer(addr=0x{0:X8}, maxSize=0x{1:X}, currentSize=0x{2:X}, readPosition=0x{3:X}, writePosition=0x{4:X}, filePosition=0x{5:X}, fileMaxSize=0x{6:X})", Addr, MaxSize, currentSize, readPosition, writePosition, FilePosition, fileMaxSize);
		}
	}

}