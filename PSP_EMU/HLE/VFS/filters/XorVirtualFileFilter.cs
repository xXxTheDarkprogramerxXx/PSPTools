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
namespace pspsharp.HLE.VFS.filters
{
	using IMemoryReaderWriter = pspsharp.memory.IMemoryReaderWriter;
	using MemoryReaderWriter = pspsharp.memory.MemoryReaderWriter;

	public class XorVirtualFileFilter : AbstractProxyVirtualFile, IVirtualFileFilter
	{
		private int xor;

		protected internal XorVirtualFileFilter(sbyte xor)
		{
			this.xor = xor & 0xFF;
		}

		public override int ioRead(TPointer outputPointer, int outputLength)
		{
			int readLength = base.ioRead(outputPointer, outputLength);
			if (readLength > 0)
			{
				IMemoryReaderWriter memoryReaderWriter = MemoryReaderWriter.getMemoryReaderWriter(outputPointer.Address, readLength, 1);
				for (int i = 0; i < readLength; i++)
				{
					int value = memoryReaderWriter.readCurrent();
					value ^= xor;
					memoryReaderWriter.writeNext(value);
				}
				memoryReaderWriter.flush();
			}

			return readLength;
		}

		public override int ioRead(sbyte[] outputBuffer, int outputOffset, int outputLength)
		{
			int readLength = base.ioRead(outputBuffer, outputOffset, outputLength);
			if (readLength > 0)
			{
				filter(outputBuffer, outputOffset, readLength);
			}

			return readLength;
		}

		public virtual void filter(sbyte[] data, int offset, int Length)
		{
			for (int i = 0; i < Length; i++)
			{
				data[offset + i] ^= (sbyte)xor;
			}
		}

		public virtual IVirtualFile VirtualFile
		{
			set
			{
				ProxyVirtualFile = value;
			}
		}

		public override string ToString()
		{
			return string.Format("XorVirtualFileFilter(xor=0x{0:X2}, {1})", xor, vFile);
		}
	}

}