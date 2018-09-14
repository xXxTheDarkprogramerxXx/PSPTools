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

	public class sceI2c : HLEModule
	{
		public static Logger log = Modules.getLogger("sceI2c");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x47BDEAAA, version = 150) public int sceI2cMasterTransmitReceive(int transmitI2cAddress, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer transmitBuffer, int transmitBufferSize, int receiveI2cAddress, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer receiveBuffer, int receiveBufferSize)
		[HLEFunction(nid : 0x47BDEAAA, version : 150)]
		public virtual int sceI2cMasterTransmitReceive(int transmitI2cAddress, TPointer transmitBuffer, int transmitBufferSize, int receiveI2cAddress, TPointer receiveBuffer, int receiveBufferSize)
		{
			receiveBuffer.clear(receiveBufferSize);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8CBD8CCF, version = 150) public int sceI2cMasterTransmit(int transmitI2cAddress, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer transmitBuffer, int transmitBufferSize)
		[HLEFunction(nid : 0x8CBD8CCF, version : 150)]
		public virtual int sceI2cMasterTransmit(int transmitI2cAddress, TPointer transmitBuffer, int transmitBufferSize)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4020DC7E, version = 150) public int sceI2cSetPollingMode()
		[HLEFunction(nid : 0x4020DC7E, version : 150)]
		public virtual int sceI2cSetPollingMode()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x49B159DE, version = 150) public int sceI2cMasterReceive()
		[HLEFunction(nid : 0x49B159DE, version : 150)]
		public virtual int sceI2cMasterReceive()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x62C7E1E4, version = 150) public int sceI2cSetClock(int unknown1, int unknown2)
		[HLEFunction(nid : 0x62C7E1E4, version : 150)]
		public virtual int sceI2cSetClock(int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD35FC17D, version = 150) public int sceI2cReset()
		[HLEFunction(nid : 0xD35FC17D, version : 150)]
		public virtual int sceI2cReset()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDBE12CED, version = 150) public int sceI2cSetDebugHandlers()
		[HLEFunction(nid : 0xDBE12CED, version : 150)]
		public virtual int sceI2cSetDebugHandlers()
		{
			return 0;
		}
	}

}