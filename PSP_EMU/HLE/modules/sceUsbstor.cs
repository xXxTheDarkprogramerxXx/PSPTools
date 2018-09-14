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

	public class sceUsbstor : HLEModule
	{
		public static Logger log = Modules.getLogger("sceUsbstor");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7B810720, version = 150) public int sceUsbstorMsSetWorkBuf(pspsharp.HLE.TPointer workBuffer, int workBufferSize)
		[HLEFunction(nid : 0x7B810720, version : 150)]
		public virtual int sceUsbstorMsSetWorkBuf(TPointer workBuffer, int workBufferSize)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9C029B16, version = 150) public int sceUsbstorMs_9C029B16(pspsharp.HLE.TPointer buffer, int bufferSize)
		[HLEFunction(nid : 0x9C029B16, version : 150)]
		public virtual int sceUsbstorMs_9C029B16(TPointer buffer, int bufferSize)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9569F268, version = 150) public int sceUsbstorMsSetVSHInfo(pspsharp.HLE.PspString version)
		[HLEFunction(nid : 0x9569F268, version : 150)]
		public virtual int sceUsbstorMsSetVSHInfo(PspString version)
		{
			// E.g. version == "6.60"
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x576E7F6F, version = 150) public int sceUsbstorMsSetProductInfo(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=44, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer productInfo)
		[HLEFunction(nid : 0x576E7F6F, version : 150)]
		public virtual int sceUsbstorMsSetProductInfo(TPointer productInfo)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4B10A7F5, version = 150) public int sceUsbstorMsRegisterEventFlag(int eventFlagUid)
		[HLEFunction(nid : 0x4B10A7F5, version : 150)]
		public virtual int sceUsbstorMsRegisterEventFlag(int eventFlagUid)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFF0C3873, version = 150) public int sceUsbstorMsUnregisterEventFlag(int eventFlagUid)
		[HLEFunction(nid : 0xFF0C3873, version : 150)]
		public virtual int sceUsbstorMsUnregisterEventFlag(int eventFlagUid)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x382898DE, version = 150) public int sceUsbstormlnRegisterBuffer(int buffer, int bufferSize)
		[HLEFunction(nid : 0x382898DE, version : 150)]
		public virtual int sceUsbstormlnRegisterBuffer(int buffer, int bufferSize)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x25B6F372, version = 150) public int sceUsbstormlnUnregisterBuffer(int buffer)
		[HLEFunction(nid : 0x25B6F372, version : 150)]
		public virtual int sceUsbstormlnUnregisterBuffer(int buffer)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDEC0FE8C, version = 150) public int sceUsbstormlnWaitStatus()
		[HLEFunction(nid : 0xDEC0FE8C, version : 150)]
		public virtual int sceUsbstormlnWaitStatus()
		{
			// Has no parameters
			return 0x10;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE11DEFDF, version = 150) public int sceUsbstormlnCancelWaitStatus()
		[HLEFunction(nid : 0xE11DEFDF, version : 150)]
		public virtual int sceUsbstormlnCancelWaitStatus()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x60066CFE, version = 150) public int sceUsbstorGetStatus()
		[HLEFunction(nid : 0x60066CFE, version : 150)]
		public virtual int sceUsbstorGetStatus()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x56AA41EA, version = 150) public int sceUsbstorMs_56AA41EA(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=32, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown)
		[HLEFunction(nid : 0x56AA41EA, version : 150)]
		public virtual int sceUsbstorMs_56AA41EA(TPointer unknown)
		{
			unknown.clear(32);
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x762F7FDF, version = 150) public int sceUsbstorMsNotifyEventDone(int unknown1, int unknown2)
		[HLEFunction(nid : 0x762F7FDF, version : 150)]
		public virtual int sceUsbstorMsNotifyEventDone(int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xABE9F2C7, version = 150) public int sceUsbstorMsGetApInfo(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.variableLength, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer apInfo)
		[HLEFunction(nid : 0xABE9F2C7, version : 150)]
		public virtual int sceUsbstorMsGetApInfo(TPointer apInfo)
		{
			int length = apInfo.getValue32();
			apInfo.clear(4, length - 4);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1F4AC19C, version = 150) public int sceUsbstormlnGetCommand(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=12, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown)
		[HLEFunction(nid : 0x1F4AC19C, version : 150)]
		public virtual int sceUsbstormlnGetCommand(TPointer unknown)
		{
			unknown.clear(12);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5821060D, version = 150) public int sceUsbstormlnNotifyResponse(int unknown1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=3, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer8 unknown2)
		[HLEFunction(nid : 0x5821060D, version : 150)]
		public virtual int sceUsbstormlnNotifyResponse(int unknown1, TPointer8 unknown2)
		{
			return 0;
		}
	}

}