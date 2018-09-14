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
//	import static pspsharp.HLE.kernel.types.pspAbstractMemoryMappedStructure.charset16;

	using Logger = org.apache.log4j.Logger;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;

	public class sceCodepage : HLEModule
	{
		public static Logger log = Modules.getLogger("sceCodepage");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEE932176, version = 150) public int sceCodepage_driver_EE932176()
		[HLEFunction(nid : 0xEE932176, version : 150)]
		public virtual int sceCodepage_driver_EE932176()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1D0DE569, version = 150) public int sceCodepage_driver_1D0DE569(pspsharp.HLE.TPointer32 unknown1, pspsharp.HLE.TPointer32 unknown2, pspsharp.HLE.TPointer32 unknown3, pspsharp.HLE.TPointer32 unknown4)
		[HLEFunction(nid : 0x1D0DE569, version : 150)]
		public virtual int sceCodepage_driver_1D0DE569(TPointer32 unknown1, TPointer32 unknown2, TPointer32 unknown3, TPointer32 unknown4)
		{
			unknown1.setValue(0);
			unknown2.setValue(0);
			unknown3.setValue(47880);
			unknown4.setValue(128);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x039BF9E9, version = 150) public int sceCodepage_driver_039BF9E9(pspsharp.HLE.TPointer unknown1, int unknown2, pspsharp.HLE.TPointer unknown3, int unknown4, pspsharp.HLE.TPointer unknown5, int unknown6, pspsharp.HLE.TPointer unknown7, int unknown8)
		[HLEFunction(nid : 0x039BF9E9, version : 150)]
		public virtual int sceCodepage_driver_039BF9E9(TPointer unknown1, int unknown2, TPointer unknown3, int unknown4, TPointer unknown5, int unknown6, TPointer unknown7, int unknown8)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB0AE63AA, version = 150) public int sceCodepage_driver_B0AE63AA(int c)
		[HLEFunction(nid : 0xB0AE63AA, version : 150)]
		public virtual int sceCodepage_driver_B0AE63AA(int c)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x855C5C2E, version = 150) public int sceCodepage_driver_855C5C2E(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer destAddr, int destLength, String src)
		[HLEFunction(nid : 0x855C5C2E, version : 150)]
		public virtual int sceCodepage_driver_855C5C2E(TPointer destAddr, int destLength, string src)
		{
			sbyte[] destBytes = src.GetBytes(charset16);
			int length = System.Math.Min(destLength, destBytes.Length);
			destAddr.setArray(destBytes, length);
			// Add trailing "\0\0"
			if (length <= destLength - 2)
			{
				destAddr.clear(length, 2);
			}

			return src.Length;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x11123ED1, version = 150) public boolean sceCodepage_driver_11123ED1(int char16)
		[HLEFunction(nid : 0x11123ED1, version : 150)]
		public virtual bool sceCodepage_driver_11123ED1(int char16)
		{
			if (char16 <= 0 || char16 > 0x7E)
			{
				return true;
			}
			return false;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x47BDF633, version = 150) public int sceCodepage_driver_47BDF633(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer8 destAddr, int destLength, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=32, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer16 srcAddr)
		[HLEFunction(nid : 0x47BDF633, version : 150)]
		public virtual int sceCodepage_driver_47BDF633(TPointer8 destAddr, int destLength, TPointer16 srcAddr)
		{
			int result = destLength;
			sbyte[] bytes = new sbyte[2];
			for (int i = 0, j = 0; j < destLength; i += 2, j++)
			{
				int char16 = srcAddr.getValue(i);
				if (char16 == 0)
				{
					result = j;
					break;
				}

				bytes[0] = (sbyte) char16;
				bytes[1] = (sbyte)(char16 >> 8);
				sbyte char8 = (sbyte) (StringHelper.NewString(bytes, charset16))[0];

				destAddr.setValue(j, char8);
			}

			if (result < destLength)
			{
				// Add trailing '\0'
				destAddr.setValue(result, (sbyte) 0);
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x014E0C72, version = 150) public boolean sceCodepage_driver_014E0C72(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=2, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer8 srcAddr)
		[HLEFunction(nid : 0x014E0C72, version : 150)]
		public virtual bool sceCodepage_driver_014E0C72(TPointer8 srcAddr)
		{
			return false;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC899572E, version = 150) public int sceCodepage_driver_C899572E(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer destAddr, int destLength, String src)
		[HLEFunction(nid : 0xC899572E, version : 150)]
		public virtual int sceCodepage_driver_C899572E(TPointer destAddr, int destLength, string src)
		{
			sbyte[] destBytes = src.GetBytes(charset16);
			int length = System.Math.Min(destLength, destBytes.Length);
			destAddr.setArray(destBytes, length);
			// Add trailing "\0\0"
			if (length <= destLength - 2)
			{
				destAddr.clear(length, 2);
			}

			return src.Length;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x907CBFD2, version = 150) public int sceCodepage_driver_907CBFD2(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer8 destAddr, int destLength, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=32, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer16 srcAddr)
		[HLEFunction(nid : 0x907CBFD2, version : 150)]
		public virtual int sceCodepage_driver_907CBFD2(TPointer8 destAddr, int destLength, TPointer16 srcAddr)
		{
			int result = destLength;
			sbyte[] bytes = new sbyte[2];
			for (int i = 0, j = 0; j < destLength; i += 2, j++)
			{
				int char16 = srcAddr.getValue(i);
				if (char16 == 0)
				{
					result = j;
					break;
				}

				bytes[0] = (sbyte) char16;
				bytes[1] = (sbyte)(char16 >> 8);
				sbyte char8 = (sbyte) (StringHelper.NewString(bytes, charset16))[0];

				destAddr.setValue(j, char8);
			}

			if (result < destLength)
			{
				// Add trailing '\0'
				destAddr.setValue(result, (sbyte) 0);
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0AA54D6D, version = 150) public int sceCodepage_driver_0AA54D6D(pspsharp.HLE.TPointer32 unknown)
		[HLEFunction(nid : 0x0AA54D6D, version : 150)]
		public virtual int sceCodepage_driver_0AA54D6D(TPointer32 unknown)
		{
			unknown.setValue(0);
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x266ABDD8, version = 150) public int sceCodepage_driver_266ABDD8()
		[HLEFunction(nid : 0x266ABDD8, version : 150)]
		public virtual int sceCodepage_driver_266ABDD8()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDCD95405, version = 150) public int sceCodepage_driver_DCD95405(int unknown)
		[HLEFunction(nid : 0xDCD95405, version : 150)]
		public virtual int sceCodepage_driver_DCD95405(int unknown)
		{
			return 0;
		}
	}

}