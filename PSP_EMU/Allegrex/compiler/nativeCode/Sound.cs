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
	public class Sound : AbstractNativeCodeSequence
	{
		public static void adjustVolume(int dstAddrReg, int srcAddrReg, int samplesReg, int volReg)
		{
			float vol = getFRegisterValue(volReg);
			if (vol != 1f)
			{
				int samples = getRegisterValue(samplesReg);
				int srcAddr = getRegisterValue(srcAddrReg);
				int dstAddr = getRegisterValue(dstAddrReg);
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(srcAddr, samples << 1, 2);
				IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(dstAddr, samples << 1, 2);

				if (vol == .5f)
				{
					for (int i = 0; i < samples; i++)
					{
						int sample = memoryReader.readNext();
						sample = (sample << 16) >> 17;
						memoryWriter.writeNext(sample);
					}
				}
				else
				{
					for (int i = 0; i < samples; i++)
					{
						int sample = (short) memoryReader.readNext();
						sample = (int)(sample * vol);
						memoryWriter.writeNext(sample);
					}
				}
				memoryWriter.flush();
			}
		}

		public static void stereoToMono(int dstAddrReg, int srcAddrReg, int samplesReg)
		{
			int samples = getRegisterValue(samplesReg);
			int srcAddr = getRegisterValue(srcAddrReg);
			int dstAddr = getRegisterValue(dstAddrReg);
			int srcAddrAlignment = srcAddr & 0x2;
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(srcAddr - srcAddrAlignment, samples << 2, 4);
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(dstAddr, samples << 1, 2);

			if (srcAddrAlignment == 0)
			{
				// Taking left samples as mono samples
				for (int i = 0; i < samples; i++)
				{
					int sample = memoryReader.readNext();
					memoryWriter.writeNext(sample);
				}
			}
			else
			{
				// Taking right samples as mono samples
				for (int i = 0; i < samples; i++)
				{
					int sample = memoryReader.readNext();
					sample = (int)((uint)sample >> 16);
					memoryWriter.writeNext(sample);
				}
			}
			memoryWriter.flush();
		}
	}

}