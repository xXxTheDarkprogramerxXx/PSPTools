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
namespace pspsharp.media.codec.util
{

	public class BitReader : IBitReader
	{
		private Memory mem;
		private int addr;
		private int initialAddr;
		private int initialSize;
		private int size;
		private int bits;
		private int value;
		private int direction;

		public BitReader(int addr, int size)
		{
			this.addr = addr;
			this.size = size;
			initialAddr = addr;
			initialSize = size;
			mem = Memory.Instance;
			bits = 0;
			direction = 1;
		}

		public virtual bool readBool()
		{
			return read1() != 0;
		}

		public virtual int read1()
		{
			if (bits <= 0)
			{
				value = mem.read8(addr);
				addr += direction;
				size--;
				bits = 8;
			}
			int bit = value >> 7;
			bits--;
			value = (value << 1) & 0xFF;

			return bit;
		}

		public virtual int read(int n)
		{
			int read;
			if (n <= bits)
			{
				read = value >> (8 - n);
				bits -= n;
				value = (value << n) & 0xFF;
			}
			else
			{
				read = 0;
				for (; n > 0; n--)
				{
					read = (read << 1) + read1();
				}
			}

			return read;
		}

		public virtual int readByte()
		{
			if (bits == 8)
			{
				bits = 0;
				return value;
			}
			if (bits > 0)
			{
				skip(bits);
			}
			int read = mem.read8(addr);
			addr += direction;
			size--;

			return read;
		}

		public virtual int BitsLeft
		{
			get
			{
				return (size << 3) + bits;
			}
		}

		public virtual int BytesRead
		{
			get
			{
				int bytesRead = addr - initialAddr;
				if (bits == 8)
				{
					bytesRead--;
				}
    
				return bytesRead;
			}
		}

		public virtual int BitsRead
		{
			get
			{
				return (addr - initialAddr) * 8 - bits;
			}
		}

		public virtual int peek(int n)
		{
			int read = this.read(n);
			skip(-n);
			return read;
		}

		public virtual void skip(int n)
		{
			bits -= n;
			if (n >= 0)
			{
				while (bits < 0)
				{
					addr += direction;
					size--;
					bits += 8;
				}
			}
			else
			{
				while (bits > 8)
				{
					addr -= direction;
					size++;
					bits -= 8;
				}
			}

			if (bits > 0)
			{
				value = mem.read8(addr - direction);
				value = (value << (8 - bits)) & 0xFF;
			}
		}

		public virtual void seek(int n)
		{
			addr = initialAddr + n;
			size = initialSize - n;
			bits = 0;
		}

		public virtual int Direction
		{
			set
			{
				this.direction = value;
				bits = 0;
			}
		}

		public virtual void byteAlign()
		{
			if (bits > 0 && bits < 8)
			{
				skip(bits);
			}
		}

		public override string ToString()
		{
			return string.Format("BitReader addr=0x{0:X8}, bits={1:D}, size=0x{2:X}, bits read {3:D}", addr, bits, size, BitsRead);
		}
	}

}