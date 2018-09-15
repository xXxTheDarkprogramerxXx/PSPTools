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
namespace pspsharp.Allegrex.compiler.nativeCode
{

	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class Sort : AbstractNativeCodeSequence
	{
		private class Float8ObjectReverse : IComparable<Float8ObjectReverse>
		{
			internal int n1;
			internal int n2;
			internal float f;

			public Float8ObjectReverse(IMemoryReader memoryReader)
			{
				n1 = memoryReader.readNext();
				n2 = memoryReader.readNext();
				f = Float.intBitsToFloat(n2);
			}

			public virtual void write(IMemoryWriter memoryWriter)
			{
				memoryWriter.writeNext(n1);
				memoryWriter.writeNext(n2);
			}

			public virtual int CompareTo(Float8ObjectReverse o)
			{
				return o.f.CompareTo(f);
			}
		}

		public static void sortFloatArray8Reverse()
		{
			int addr = GprA0;
			int size = GprA1;

			if (size < 2)
			{
				return;
			}

			// Read the objects from memory
			Float8ObjectReverse[] objects = new Float8ObjectReverse[size];
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(addr, size << 3, 4);
			for (int i = 0; i < size; i++)
			{
				objects[i] = new Float8ObjectReverse(memoryReader);
			}

			// Sort the objects
			Array.Sort(objects);

			// Write back the objects to memory
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(addr, size << 3, 4);
			for (int i = 0; i < size; i++)
			{
				objects[i].write(memoryWriter);
			}
			memoryWriter.flush();
		}
	}

}