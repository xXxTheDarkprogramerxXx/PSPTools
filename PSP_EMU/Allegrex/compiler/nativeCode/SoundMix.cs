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
	using Utils = pspsharp.sound.Utils;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class SoundMix : AbstractNativeCodeSequence
	{
		public static void mixStereoInMemory(int inAddrReg, int inOutAddrReg, int countReg, int leftVolumeFReg, int rightVolumeFReg)
		{
			int inAddr = getRegisterValue(inAddrReg);
			int inOutAddr = getRegisterValue(inOutAddrReg);
			int count = getRegisterValue(countReg);
			float inLeftVolume = getFRegisterValue(leftVolumeFReg);
			float inRightVolume = getFRegisterValue(rightVolumeFReg);

			Utils.mixStereoInMemory(inAddr, inOutAddr, count, inLeftVolume, inRightVolume);
		}

		public static void mixStereoInMemory(int inAddrReg, int inOutAddrReg, int countReg, int maxCountAddrReg, int leftVolumeFReg, int rightVolumeFReg)
		{
			int inAddr = getRegisterValue(inAddrReg);
			int inOutAddr = getRegisterValue(inOutAddrReg);
			int count = getRegisterValue(countReg);
			int maxCount = Memory.read32(getRegisterValue(maxCountAddrReg));
			float inLeftVolume = getFRegisterValue(leftVolumeFReg);
			float inRightVolume = getFRegisterValue(rightVolumeFReg);

			Utils.mixStereoInMemory(inAddr, inOutAddr, maxCount - count, inLeftVolume, inRightVolume);
		}

		public static void mixMonoToStereo(int leftChannelAddrReg, int rightChannelAddrReg, int stereoChannelAddrReg, int lengthReg, int lengthStep)
		{
			int leftChannelAddr = getRegisterValue(leftChannelAddrReg);
			int rightChannelAddr = getRegisterValue(rightChannelAddrReg);
			int stereoChannelAddr = getRegisterValue(stereoChannelAddrReg);
			int length = getRegisterValue(lengthReg) * lengthStep;

			IMemoryReader leftChannelReader = MemoryReader.getMemoryReader(leftChannelAddr, length, 2);
			IMemoryReader rightChannelReader = MemoryReader.getMemoryReader(rightChannelAddr, length, 2);
			IMemoryWriter stereoChannelWriter = MemoryWriter.getMemoryWriter(stereoChannelAddr, length << 1, 2);

			for (int i = 0; i < length; i += 2)
			{
				int left = leftChannelReader.readNext();
				int right = rightChannelReader.readNext();
				stereoChannelWriter.writeNext(left);
				stereoChannelWriter.writeNext(right);
			}
			stereoChannelWriter.flush();
		}
	}

}