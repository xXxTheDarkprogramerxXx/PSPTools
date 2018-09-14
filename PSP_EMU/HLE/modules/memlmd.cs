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
namespace pspsharp.HLE.modules
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_DECRYPT_FUSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_MODE_DECRYPT_CBC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeUnaligned32;

	using Logger = org.apache.log4j.Logger;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;

	public class memlmd : HLEModule
	{
		public static Logger log = Modules.getLogger("memlmd");

		public static sbyte[] getKey(int[] intKey)
		{
			sbyte[] key = new sbyte[intKey.Length << 2];
			for (int i = 0, j = 0; i < intKey.Length; i++)
			{
				key[j++] = (sbyte) intKey[i];
				key[j++] = (sbyte)(intKey[i] >> 8);
				key[j++] = (sbyte)(intKey[i] >> 16);
				key[j++] = (sbyte)(intKey[i] >> 24);
			}

			return key;
		}

		public virtual int hleMemlmd_6192F715(sbyte[] buffer, int size)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] xorInputKey = getKey(new int[] { 0xB04D85AA, 0xEB47CAFF, 0xE4D77F38, 0x10B0623D });
			sbyte[] xorInputKey = getKey(new int[] {unchecked((int)0xB04D85AA), unchecked((int)0xEB47CAFF), unchecked((int)0xE4D77F38), 0x10B0623D});
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] xorOutputKey = getKey(new int[] { 0x31A8F671, 0x1EFFE01E, 0xD26CBA50, 0x2DD62D98 });
			sbyte[] xorOutputKey = getKey(new int[] {0x31A8F671, 0x1EFFE01E, unchecked((int)0xD26CBA50), 0x2DD62D98});
			const int outSize = 0xD0;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] tmpBuffer = new byte[0x14 + outSize];
			sbyte[] tmpBuffer = new sbyte[0x14 + outSize];

			writeUnaligned32(tmpBuffer, 0x00, PSP_KIRK_CMD_MODE_DECRYPT_CBC);
			writeUnaligned32(tmpBuffer, 0x04, 0);
			writeUnaligned32(tmpBuffer, 0x08, 0);
			writeUnaligned32(tmpBuffer, 0x0C, 0x100);
			writeUnaligned32(tmpBuffer, 0x10, outSize);

			for (int i = 0; i < outSize; i++)
			{
				tmpBuffer[0x14 + i] = (sbyte)(buffer[0x80 + i] ^ xorInputKey[i & 0xF]);
			}

			int result = Modules.semaphoreModule.hleUtilsBufferCopyWithRange(tmpBuffer, 0, outSize, tmpBuffer, 0, tmpBuffer.Length, PSP_KIRK_CMD_DECRYPT_FUSE);
			if (result != 0)
			{
				return -1;
			}

			for (int i = 0; i < outSize; i++)
			{
				tmpBuffer[i] ^= xorOutputKey[i & 0xF];
			}

			Array.Copy(tmpBuffer, 0x40, buffer, 0x80, 0x90);
			Array.Copy(tmpBuffer, 0, buffer, 0x110, 0x40);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6192F715, version = 660) public int memlmd_6192F715(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.inout, maxDumpLength=512) pspsharp.HLE.TPointer buffer, int size)
		[HLEFunction(nid : 0x6192F715, version : 660)]
		public virtual int memlmd_6192F715(TPointer buffer, int size)
		{
			sbyte[] byteBuffer = new sbyte[0x80 + 0xD0];
			buffer.getArray8(byteBuffer);
			int result = hleMemlmd_6192F715(byteBuffer, size);
			buffer.Array = byteBuffer;

			return result;
		}

		/*
		 * See
		 *    https://github.com/uofw/uofw/blob/master/src/loadcore/loadcore.c
		 * CheckLatestSubType()
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9D36A439, version = 660) public boolean memlmd_9D36A439(int subType)
		[HLEFunction(nid : 0x9D36A439, version : 660)]
		public virtual bool memlmd_9D36A439(int subType)
		{
			return true;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF26A33C3, version = 660) public int memlmd_F26A33C3(int unknown, int hardwarePtr)
		[HLEFunction(nid : 0xF26A33C3, version : 660)]
		public virtual int memlmd_F26A33C3(int unknown, int hardwarePtr)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEF73E85B, version = 660) public int memlmd_EF73E85B(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer buffer, int size, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 resultSize)
		[HLEFunction(nid : 0xEF73E85B, version : 660)]
		public virtual int memlmd_EF73E85B(TPointer buffer, int size, TPointer32 resultSize)
		{
			resultSize.setValue(size);

			Modules.LoadCoreForKernelModule.decodeInitModuleData(buffer, size, resultSize);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCF03556B, version = 660) public int memlmd_CF03556B(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer buffer, int size, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 resultSize)
		[HLEFunction(nid : 0xCF03556B, version : 660)]
		public virtual int memlmd_CF03556B(TPointer buffer, int size, TPointer32 resultSize)
		{
			// Same as memlmd_EF73E85B?
			resultSize.setValue(size);

			Modules.LoadCoreForKernelModule.decodeInitModuleData(buffer, size, resultSize);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2F3D7E2D, version = 660) public int memlmd_2F3D7E2D()
		[HLEFunction(nid : 0x2F3D7E2D, version : 660)]
		public virtual int memlmd_2F3D7E2D()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2AE425D2, version = 660) public boolean memlmd_2AE425D2(int subType)
		[HLEFunction(nid : 0x2AE425D2, version : 660)]
		public virtual bool memlmd_2AE425D2(int subType)
		{
			return true;
		}
	}

}