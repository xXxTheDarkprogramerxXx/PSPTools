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
	/// <summary>
	/// http://psp.jim.sh/pspsdk-doc/structSceIoStat.html </summary>
	public class SceIoStat : pspAbstractMemoryMappedStructure
	{
		public static readonly int SIZEOF = 16 + ScePspDateTime.SIZEOF * 3 + 24;
		public int mode;
		public int attr;
		public long size;
		public ScePspDateTime ctime;
		public ScePspDateTime atime;
		public ScePspDateTime mtime;
		private int[] reserved = new int[] {0, 0, 0, 0, 0, 0};

		public SceIoStat()
		{
		}

		public SceIoStat(int mode, int attr, long size, ScePspDateTime ctime, ScePspDateTime atime, ScePspDateTime mtime)
		{
			init(mode, attr, size, ctime, atime, mtime);
		}

		public virtual void init(int mode, int attr, long size, ScePspDateTime ctime, ScePspDateTime atime, ScePspDateTime mtime)
		{
			this.mode = mode;
			this.attr = attr;
			this.size = size;
			this.ctime = ctime;
			this.atime = atime;
			this.mtime = mtime;
		}

		public virtual void setReserved(int index, int value)
		{
			reserved[index] = value;
		}

		public virtual int getReserved(int index)
		{
			return reserved[index];
		}

		public virtual int StartSector
		{
			set
			{
				setReserved(0, value);
			}
			get
			{
				return getReserved(0);
			}
		}


		protected internal override void read()
		{
			mode = read32();
			attr = read32();
			size = read64();

			ctime = new ScePspDateTime();
			atime = new ScePspDateTime();
			mtime = new ScePspDateTime();
			read(ctime);
			read(atime);
			read(mtime);

			read32Array(reserved);
		}

		protected internal override void write()
		{
			write32(mode);
			write32(attr);
			write64(size);

			write(ctime);
			write(atime);
			write(mtime);

			// 6 ints reserved
			write32Array(reserved);
		}

		public override int @sizeof()
		{
			return SIZEOF;
		}

		public override string ToString()
		{
			return string.Format("SceIoStat[mode=0x{0:X}, attr=0x{1:X}, size={2:D}, ctime={3}, atime={4}, mtime={5}]", mode, attr, size, ctime, atime, mtime);
		}
	}

}