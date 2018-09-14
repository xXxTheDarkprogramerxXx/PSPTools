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

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class MemcpySequence : AbstractNativeCodeSequence, INativeCodeSequence
	{
		public static void call(int dstAddrReg, int srcAddrReg, int lengthReg)
		{
			int dstAddr = getRegisterValue(dstAddrReg);
			int srcAddr = getRegisterValue(srcAddrReg);
			int length = getRegisterValue(lengthReg);

			Memory.memcpy(dstAddr, srcAddr, length);

			// Update registers
			setRegisterValue(dstAddrReg, dstAddr + length);
			setRegisterValue(srcAddrReg, srcAddr + length);
			setRegisterValue(lengthReg, 0);
		}

		public static void call(int dstAddrReg, int srcAddrReg, int lengthReg, int dstOffset, int srcOffset)
		{
			int dstAddr = getRegisterValue(dstAddrReg);
			int srcAddr = getRegisterValue(srcAddrReg);
			int length = getRegisterValue(lengthReg);

			Memory.memcpy(dstAddr + dstOffset, srcAddr + srcOffset, length);

			// Update registers
			setRegisterValue(dstAddrReg, dstAddr + length);
			setRegisterValue(srcAddrReg, srcAddr + length);
			setRegisterValue(lengthReg, 0);
		}

		public static void call(int dstAddrReg, int srcAddrReg, int targetAddrReg, int targetReg)
		{
			int dstAddr = getRegisterValue(dstAddrReg);
			int srcAddr = getRegisterValue(srcAddrReg);
			int targetAddr = getRegisterValue(targetAddrReg);

			int length = targetAddr - getRegisterValue(targetReg);
			Memory.memcpy(dstAddr, srcAddr, length);

			// Update registers
			setRegisterValue(dstAddrReg, dstAddr + length);
			setRegisterValue(srcAddrReg, srcAddr + length);
		}

		public static void call(int dstAddrReg, int srcAddrReg, int targetAddrReg, int targetReg, int dstOffset, int srcOffset, int valueReg, int valueBytes)
		{
			int dstAddr = getRegisterValue(dstAddrReg);
			int srcAddr = getRegisterValue(srcAddrReg);
			int targetAddr = getRegisterValue(targetAddrReg);

			int length = targetAddr - getRegisterValue(targetReg);
			Memory.memcpy(dstAddr + dstOffset, srcAddr + srcOffset, length);

			// Update registers
			setRegisterValue(dstAddrReg, dstAddr + length);
			setRegisterValue(srcAddrReg, srcAddr + length);

			// Update the register "valueReg" with the last value processed by the memcpy loop
			int valueAddr = srcAddr + length - valueBytes;
			int value;
			switch (valueBytes)
			{
			case 1:
				value = Memory.read8(valueAddr);
				break;
			case 2:
				value = Memory.read16(valueAddr);
				break;
			case 4:
				value = Memory.read32(valueAddr);
				break;
			default:
				value = 0;
				Compiler.log.error("MemcpySequence.call(): Unimplemented valueBytes=" + valueBytes);
				break;
			}
			setRegisterValue(valueReg, value);
		}

		public static void callWithStep(int dstAddrReg, int srcAddrReg, int lengthReg, int step, int lengthOffset)
		{
			int dstAddr = getRegisterValue(dstAddrReg);
			int srcAddr = getRegisterValue(srcAddrReg);
			int length = (getRegisterValue(lengthReg) + lengthOffset) * step;

			Memory.memcpy(dstAddr, srcAddr, length);

			// Update registers
			setRegisterValue(dstAddrReg, dstAddr + length);
			setRegisterValue(srcAddrReg, srcAddr + length);
			setRegisterValue(lengthReg, 0);
		}

		public static void callWithCountStep(int dstAddrReg, int srcAddrReg, int lengthReg, int count, int step)
		{
			int dstAddr = getRegisterValue(dstAddrReg);
			int srcAddr = getRegisterValue(srcAddrReg);
			int length = (count - getRegisterValue(lengthReg)) * step;

			Memory.memcpy(dstAddr, srcAddr, length);

			// Update registers
			setRegisterValue(dstAddrReg, dstAddr + length);
			setRegisterValue(srcAddrReg, srcAddr + length);
			setRegisterValue(lengthReg, count);
		}

		public static void callFixedLength(int dstAddrReg, int srcAddrReg, int dstOffset, int srcOffset, int length)
		{
			int dstAddr = getRegisterValue(dstAddrReg) + dstOffset;
			int srcAddr = getRegisterValue(srcAddrReg) + srcOffset;

			Memory.memcpy(dstAddr, srcAddr, length);
		}

		public static void callFixedLength(int dstAddrReg, int srcAddrReg, int dstOffset, int srcOffset, int length, int updatedSrcAddrReg)
		{
			int dstAddr = getRegisterValue(dstAddrReg) + dstOffset;
			int srcAddr = getRegisterValue(srcAddrReg) + srcOffset;

			Memory.memcpy(dstAddr, srcAddr, length);

			setRegisterValue(updatedSrcAddrReg, srcAddr);
		}

		public static void callIndirectLength(int dstAddrReg, int srcAddrReg, int lengthAddrReg, int lengthAddrOffset)
		{
			int dstAddr = getRegisterValue(dstAddrReg);
			int srcAddr = getRegisterValue(srcAddrReg);
			int length = Memory.read32(getRegisterValue(lengthAddrReg) + lengthAddrOffset);

			Memory.memcpy(dstAddr, srcAddr, length);

			// Update registers
			setRegisterValue(dstAddrReg, dstAddr + length);
			setRegisterValue(srcAddrReg, srcAddr + length);
		}
	}

}