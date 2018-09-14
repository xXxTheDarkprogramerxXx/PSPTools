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
	using Logger = org.apache.log4j.Logger;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;

	public class sceBSMan : HLEModule
	{
		public static Logger log = Modules.getLogger("sceBSMan");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x46ACDAE3, version = 660) public int sceBSMan_46ACDAE3(@DebugMemory @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=11, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0x46ACDAE3, version : 660)]
		public virtual int sceBSMan_46ACDAE3(TPointer buffer)
		{
			buffer.setValue16(0, (short) 126); // Valid values are [68..126]
			buffer.setValue16(2, (short) 0); // 0 or 1

			// The following bytes are only evaluated when the value at offset 0 is 126.
			buffer.setValue16(4, (short) 5); // Only valid value is 5 (length of following structure?)

			int unknown678 = 0x080046; // Possible values: 0x080046, 0x001958
			buffer.setValue8(6, unchecked((sbyte)((unknown678 >> 16) & 0xFF)));
			buffer.setValue8(7, unchecked((sbyte)((unknown678 >> 8) & 0xFF)));
			buffer.setValue8(8, unchecked((sbyte)((unknown678 >> 0) & 0xFF)));

			int unknown9A = 0x0000; // Possible values: 0x0000, 0x0001, 0x0002
			buffer.setValue8(9, unchecked((sbyte)((unknown9A >> 8) & 0xFF)));
			buffer.setValue8(10, unchecked((sbyte)((unknown9A >> 0) & 0xFF)));

			return 0;
		}
	}

}