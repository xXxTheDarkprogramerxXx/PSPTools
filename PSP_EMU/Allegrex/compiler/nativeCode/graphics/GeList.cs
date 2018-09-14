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
namespace pspsharp.Allegrex.compiler.nativeCode.graphics
{
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class GeList : AbstractNativeCodeSequence
	{
		public static void updateCommands(int @base, int startReg, int endReg, int offsetReg, int stepReg)
		{
			Memory mem = Memory;
			int start = getRegisterValue(startReg);
			int end = getRegisterValue(endReg);
			int offset = getRegisterValue(offsetReg);
			int step = getRegisterValue(stepReg);
			int skip = (step - 4) >> 2;
			IMemoryReader baseReader = MemoryReader.getMemoryReader(getRegisterValue(@base), (end - start) << 4, 4);
			for (int i = start; i < end; i++)
			{
				baseReader.skip(1);
				int addr = baseReader.readNext();
				int count = baseReader.readNext();
				int dest = baseReader.readNext();
				IMemoryReader addrReader = MemoryReader.getMemoryReader(addr, count << 2, 4);
				IMemoryWriter destWriter = MemoryWriter.getMemoryWriter(dest + offset, count * step, 4);
				for (int j = 0; j < count; j++)
				{
					int src = addrReader.readNext();
					destWriter.writeNext(mem.read32(src));
					destWriter.skip(skip);
				}
				destWriter.flush();
			}
		}
	}

}