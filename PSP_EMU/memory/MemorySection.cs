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
namespace pspsharp.memory
{

	public class MemorySection
	{
		private int baseAddress;
		private int length;
		private bool read;
		private bool write;
		private bool execute;

		public MemorySection(int baseAddress, int length, bool read, bool write, bool execute)
		{
			this.baseAddress = baseAddress & Memory.addressMask;
			this.length = length;
			this.read = read;
			this.write = write;
			this.execute = execute;
		}

		public virtual int BaseAddress
		{
			get
			{
				return baseAddress;
			}
		}

		public virtual int EndAddress
		{
			get
			{
				return baseAddress + length - 1;
			}
		}

		public virtual int Length
		{
			get
			{
				return length;
			}
		}

		public virtual bool canRead()
		{
			return read;
		}

		public virtual bool canWrite()
		{
			return write;
		}

		public virtual bool canExecute()
		{
			return execute;
		}

		public virtual bool contains(int address)
		{
			address &= Memory.addressMask;
			return BaseAddress <= address && address <= EndAddress;
		}

		public override string ToString()
		{
			return string.Format("MemorySection[0x{0:X8}-0x{1:X8}(length=0x{2:X}) {3}{4}{5}]", BaseAddress, EndAddress, Length, canRead() ? "R" : "", canWrite() ? "W" : "", canExecute() ? "X" : "");
		}
	}

}