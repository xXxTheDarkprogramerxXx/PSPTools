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
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;

	using Logger = org.apache.log4j.Logger;

	public class UtilsForKernel : HLEModule
	{
		public static Logger log = Modules.getLogger("UtilsForKernel");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA6B0A6B8, version = 150) public int UtilsForKernel_A6B0A6B8()
		[HLEFunction(nid : 0xA6B0A6B8, version : 150)]
		public virtual int UtilsForKernel_A6B0A6B8()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x39FFB756, version = 150) public int UtilsForKernel_39FFB756(int unknown)
		[HLEFunction(nid : 0x39FFB756, version : 150)]
		public virtual int UtilsForKernel_39FFB756(int unknown)
		{
			return 0;
		}

		/// <summary>
		/// KL4E decompression.
		/// </summary>
		/// <param name="dest"> </param>
		/// <param name="destSize"> </param>
		/// <param name="src"> </param>
		/// <param name="decompressedSizeAddr">
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6C6887EE, version = 150) public int UtilsForKernel_6C6887EE(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer dest, int destSize, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=0x100, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer src, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 endOfDecompressedDestAddr)
		[HLEFunction(nid : 0x6C6887EE, version : 150)]
		public virtual int UtilsForKernel_6C6887EE(TPointer dest, int destSize, TPointer src, TPointer32 endOfDecompressedDestAddr)
		{
			return 0;
		}
	}

}