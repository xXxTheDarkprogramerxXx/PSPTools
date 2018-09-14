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
namespace pspsharp.HLE.kernel.types
{
	public class SceNetIfHandle : pspAbstractMemoryMappedStructure
	{
		public int callbackArg4;
		public int upCallbackAddr;
		public int downCallbackAddr;
		public int sendCallbackAddr;
		public int ioctlCallbackAddr;
		public int addrFirstMessageToBeSent;
		public int addrLastMessageToBeSent;
		public int numberOfMessagesToBeSent;
		public int unknown36;
		public int unknown40;
		public int handleInternalAddr;
		public SceNetIfHandleInternal handleInternal;

		public class SceNetIfHandleInternal : pspAbstractMemoryMappedStructure
		{
			public string interfaceName;
			public int ioctlSemaId;
			public int ioctlOKSemaId;
			public pspNetMacAddress macAddress = new pspNetMacAddress();
			public int unknownCallbackAddr184;
			public int unknownCallbackAddr188;
			public int sceNetIfhandleIfUp;
			public int sceNetIfhandleIfDown;
			public int sceNetIfhandleIfIoctl;
			public int errorCode;

			protected internal override void read()
			{
				readUnknown(20); // Offset 0
				interfaceName = readStringNZ(16); // Offset 20
				readUnknown(148); // Offset 36
				unknownCallbackAddr184 = read32(); // Offset 184
				unknownCallbackAddr188 = read32(); // Offset 188
				readUnknown(44); // Offset 192
				macAddress = new pspNetMacAddress();
				read(macAddress); // Offset 236
				readUnknown(2); // Offset 242
				sceNetIfhandleIfUp = read32(); // Offset 244
				sceNetIfhandleIfDown = read32(); // Offset 248
				sceNetIfhandleIfIoctl = read32(); // Offset 252
				ioctlSemaId = read32(); // Offset 256
				ioctlOKSemaId = read32(); // Offset 260
				readUnknown(4); // Offset 264
				errorCode = read32(); // Offset 268
			}

			protected internal override void write()
			{
				writeSkip(20); // Offset 0
				writeStringNZ(16, interfaceName); // Offset 20
				writeSkip(148); // Offset 36
				write32(unknownCallbackAddr184); // Offset 184
				write32(unknownCallbackAddr188); // Offset 188
				writeSkip(44); // Offset 192
				write(macAddress); // Offset 236
				writeSkip(2); // Offset 242
				write32(sceNetIfhandleIfUp); // Offset 244
				write32(sceNetIfhandleIfDown); // Offset 248
				write32(sceNetIfhandleIfIoctl); // Offset 252
				write32(ioctlSemaId); // Offset 256
				write32(ioctlOKSemaId); // Offset 260
				writeSkip(4); // Offset 264
				write32(errorCode); // Offset 268
			}

			public override int @sizeof()
			{
				return 320;
			}

			public override string ToString()
			{
				return string.Format("interfaceName='{0}', unknownCallbackAddr184=0x{1:X8}, unknownCallbackAddr188=0x{2:X8}, sceNetIfhandleIfUp=0x{3:X8}, sceNetIfhandleIfDown=0x{4:X8}, sceNetIfhandleIfIoctl=0x{5:X8}, ioctlSemaId=0x{6:X}, ioctlOKSemaId=0x{7:X}, macAddress={8}", interfaceName, unknownCallbackAddr184, unknownCallbackAddr188, sceNetIfhandleIfUp, sceNetIfhandleIfDown, sceNetIfhandleIfIoctl, ioctlSemaId, ioctlOKSemaId, macAddress);
			}
		}

		protected internal override void read()
		{
			handleInternalAddr = read32(); // Offset 0
			if (handleInternalAddr != 0)
			{
				handleInternal = new SceNetIfHandleInternal();
				handleInternal.read(mem, handleInternalAddr);
			}
			callbackArg4 = read32(); // Offset 4
			upCallbackAddr = read32(); // Offset 8
			downCallbackAddr = read32(); // Offset 12
			sendCallbackAddr = read32(); // Offset 16
			ioctlCallbackAddr = read32(); // Offset 20
			addrFirstMessageToBeSent = read32(); // Offset 24
			addrLastMessageToBeSent = read32(); // Offset 28
			numberOfMessagesToBeSent = read32(); // Offset 32
			unknown36 = read32(); // Offset 36
			unknown40 = read32(); // Offset 40
		}

		protected internal override void write()
		{
			write32(handleInternalAddr); // Offset 0
			if (handleInternalAddr != 0 && handleInternal != null)
			{
				handleInternal.write(mem, handleInternalAddr);
			}
			write32(callbackArg4); // Offset 4
			write32(upCallbackAddr); // Offset 8
			write32(downCallbackAddr); // Offset 12
			write32(sendCallbackAddr); // Offset 16
			write32(ioctlCallbackAddr); // Offset 20
			write32(addrFirstMessageToBeSent); // Offset 24
			write32(addrLastMessageToBeSent); // Offset 28
			write32(numberOfMessagesToBeSent); // Offset 32
			write32(unknown36); // Offset 36
			write32(unknown40); // Offset 40
		}

		public override int @sizeof()
		{
			return 44;
		}

		public override string ToString()
		{
			return string.Format("callbackArg4=0x{0:X}, upCallbackAddr=0x{1:X8}, downCallbackAddr=0x{2:X8}, sendCallbackAddr=0x{3:X8}, ioctlCallbackAddr=0x{4:X8}, firstMessage=0x{5:X8}, lastMessage=0x{6:X8}, nbrMessages=0x{7:X}, unknown36=0x{8:X}, unknown40=0x{9:X}, internalStructure: {10}", callbackArg4, upCallbackAddr, downCallbackAddr, sendCallbackAddr, ioctlCallbackAddr, addrFirstMessageToBeSent, addrLastMessageToBeSent, numberOfMessagesToBeSent, unknown36, unknown40, handleInternal);
		}
	}

}