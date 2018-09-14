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
//	import static pspsharp.HLE.modules.sceUmdUser.PSP_UMD_READABLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceUmdUser.PSP_UMD_READY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.filesystems.umdiso.ISectorDevice_Fields.sectorLength;

	using Logger = org.apache.log4j.Logger;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using ISectorDevice = pspsharp.filesystems.umdiso.ISectorDevice;
	using UmdIsoReader = pspsharp.filesystems.umdiso.UmdIsoReader;
	using Scheduler = pspsharp.scheduler.Scheduler;
	using Utilities = pspsharp.util.Utilities;

	public class sceUmdMan : HLEModule
	{
		public static Logger log = Modules.getLogger("sceUmdMan");
		protected internal SysMemInfo dummyAreaInfo;
		protected internal TPointer dummyArea;
		protected internal SysMemInfo dummyUmdDriveInfo;
		protected internal TPointer dummyUmdDrive;

		private class TriggerCallbackAction : IAction
		{
			internal int status;
			internal int callback;
			internal int callbackArg;
			internal int argument3;

			public TriggerCallbackAction(int status, int callback, int callbackArg, int argument3)
			{
				this.status = status;
				this.callback = callback;
				this.callbackArg = callbackArg;
				this.argument3 = argument3;
			}

			public virtual void execute()
			{
				SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
				Modules.ThreadManForUserModule.executeCallback(thread, callback, null, true, status, callbackArg, argument3);
			}
		}

		protected internal virtual ISectorDevice SectorDevice
		{
			get
			{
				UmdIsoReader iso = Modules.sceUmdUserModule.IsoReader;
				if (iso == null)
				{
					return null;
				}
				return iso.SectorDevice;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x65E2B3E0, version = 660) public int sceUmdMan_65E2B3E0()
		[HLEFunction(nid : 0x65E2B3E0, version : 660)]
		public virtual int sceUmdMan_65E2B3E0()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x47E2B6D8, version = 150) public int sceUmdManGetUmdDrive(int unknown)
		[HLEFunction(nid : 0x47E2B6D8, version : 150)]
		public virtual int sceUmdManGetUmdDrive(int unknown)
		{
			if (dummyUmdDriveInfo == null)
			{
				const int size = 36;
				dummyUmdDriveInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "sceUmdManGetUmdDrive", SysMemUserForUser.PSP_SMEM_Low, size, 0);
				if (dummyUmdDriveInfo == null)
				{
					return -1;
				}
				dummyUmdDrive = new TPointer(Memory.Instance, dummyUmdDriveInfo.addr);
				dummyUmdDrive.clear(size);
			}

			// This will be the value of the first parameter passed to sceUmdMan_E3716915()
			return dummyUmdDrive.Address;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3C8C523D, version = 150) public int sceUmdMan_3C8C523D(int wantedStatus, pspsharp.HLE.TPointer callback, pspsharp.HLE.TPointer callbackArg)
		[HLEFunction(nid : 0x3C8C523D, version : 150)]
		public virtual int sceUmdMan_3C8C523D(int wantedStatus, TPointer callback, TPointer callbackArg)
		{
			if (SectorDevice != null)
			{
				if (wantedStatus == PSP_UMD_READABLE || wantedStatus == PSP_UMD_READY)
				{
					TriggerCallbackAction triggerCallbackAction = new TriggerCallbackAction(wantedStatus, callback.Address, callbackArg.Address, 0);
					Emulator.Scheduler.addAction(Scheduler.Now + 1000, triggerCallbackAction);
				}
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3A05AC3C, version = 150) public boolean sceUmdMan_3A05AC3C()
		[HLEFunction(nid : 0x3A05AC3C, version : 150)]
		public virtual bool sceUmdMan_3A05AC3C()
		{
			// Has no parameters
			return false;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0DC8D26D, version = 150) public int sceUmdManWaitSema()
		[HLEFunction(nid : 0x0DC8D26D, version : 150)]
		public virtual int sceUmdManWaitSema()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9F106F73, version = 150) public int sceUmdManPollSema()
		[HLEFunction(nid : 0x9F106F73, version : 150)]
		public virtual int sceUmdManPollSema()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB0A43DA7, version = 150) public int sceUmdManSignalSema()
		[HLEFunction(nid : 0xB0A43DA7, version : 150)]
		public virtual int sceUmdManSignalSema()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF31D8208, version = 150) public int sceUmdMan_F31D8208()
		[HLEFunction(nid : 0xF31D8208, version : 150)]
		public virtual int sceUmdMan_F31D8208()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6519F8D1, version = 150) public int sceUmdMan_6519F8D1(int timeout)
		[HLEFunction(nid : 0x6519F8D1, version : 150)]
		public virtual int sceUmdMan_6519F8D1(int timeout)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE3716915, version = 150) public int sceUmdMan_E3716915(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=16, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer umdDrive, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=12, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer lbaParameters, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=sectorLength, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer readBuffer1, int readSize, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.previousParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer readBuffer2, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=sectorLength, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer readBuffer3, int flags)
		[HLEFunction(nid : 0xE3716915, version : 150)]
		public virtual int sceUmdMan_E3716915(TPointer umdDrive, TPointer lbaParameters, TPointer readBuffer1, int readSize, TPointer readBuffer2, TPointer readBuffer3, int flags)
		{
			int sectorNumber = lbaParameters.getValue32(0);
			int unknown1 = lbaParameters.getValue32(4);
			int unknown2 = lbaParameters.getValue16(8);
			int unknown3 = lbaParameters.getValue8(10);
			int unknown4 = lbaParameters.getValue8(11);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceUmdMan_E3716915 LBA parameters: sectorNumber=0x{0:X}, unknown1=0x{1:X}, unknown2=0x{2:X}, unknown3=0x{3:X}, unknown4=0x{4:X}", sectorNumber, unknown1, unknown2, unknown3, unknown4));
			}

			ISectorDevice sectorDevice = SectorDevice;
			if (sectorDevice == null)
			{
				log.warn(string.Format("sceUmdMan_E3716915 no SectorDevice available"));
				return -1;
			}

			int totalReadSize = 0;
			if ((flags & 1) != 0)
			{
				totalReadSize += sectorLength;
			}
			if ((flags & 2) != 0)
			{
				totalReadSize += readSize;
			}
			if ((flags & 4) != 0)
			{
				totalReadSize += sectorLength;
			}

			int totalReadSectors = Utilities.alignUp(totalReadSize, sectorLength - 1) / sectorLength;
			sbyte[] dataBuffer = new sbyte[totalReadSectors * sectorLength];
			try
			{
				sectorDevice.readSectors(sectorNumber, totalReadSectors, dataBuffer, 0);
			}
			catch (IOException e)
			{
				log.error(e);
				return -1;
			}

			int dataBufferOffset = 0;
			if ((flags & 1) != 0)
			{
				readBuffer1.setArray(0, dataBuffer, dataBufferOffset, sectorLength);
				dataBufferOffset += sectorLength;
			}
			if ((flags & 2) != 0)
			{
				readBuffer2.setArray(0, dataBuffer, dataBufferOffset, readSize);
				dataBufferOffset += readSize;
			}
			if ((flags & 4) != 0)
			{
				readBuffer3.setArray(0, dataBuffer, dataBufferOffset, sectorLength);
				dataBufferOffset += sectorLength;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x709E7035, version = 150) public int sceUmdMan_709E7035(int unknown)
		[HLEFunction(nid : 0x709E7035, version : 150)]
		public virtual int sceUmdMan_709E7035(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD1478023, version = 150) public int sceUmdMan_D1478023()
		[HLEFunction(nid : 0xD1478023, version : 150)]
		public virtual int sceUmdMan_D1478023()
		{
			// Has no parameters
			ISectorDevice sectorDevice = SectorDevice;
			if (sectorDevice == null)
			{
				log.warn(string.Format("sceUmdMan_D1478023 no SectorDevice available"));
				return -1;
			}

			int numSectors;
			try
			{
				numSectors = sectorDevice.NumSectors;
			}
			catch (IOException e)
			{
				log.warn(string.Format("sceUmdMan_D1478023 IO error {0}", e));
				return -1;
			}

			if (dummyAreaInfo == null)
			{
				dummyAreaInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "sceUmdMan_D1478023", SysMemUserForUser.PSP_SMEM_Low, 120, 0);
				if (dummyAreaInfo == null)
				{
					return -1;
				}
				dummyArea = new TPointer(Memory.Instance, dummyAreaInfo.addr);
			}

			dummyArea.setValue8(100, (sbyte) 0x12);
			dummyArea.setValue8(101, (sbyte) 0x34);
			dummyArea.setValue8(102, (sbyte) 0x56);
			dummyArea.setValue8(103, (sbyte) 0x78);
			dummyArea.setValue32(108, 0x11111111);
			dummyArea.setValue32(112, numSectors); // value returned by sceIoDevctl cmd=0x01F20003 (get total number of sectors)
			dummyArea.setValue32(116, numSectors); // value returned by sceIoDevctl cmd=0x01F20002 (get current LBA)

			return dummyArea.Address;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4FB913A3, version = 150) public int sceUmdManGetIntrStateFlag()
		[HLEFunction(nid : 0x4FB913A3, version : 150)]
		public virtual int sceUmdManGetIntrStateFlag()
		{
			// Has no parameters
			return 0; // Can return 0 or 4
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8FF7C13A, version = 150) public int sceUmdMan_8FF7C13A()
		[HLEFunction(nid : 0x8FF7C13A, version : 150)]
		public virtual int sceUmdMan_8FF7C13A()
		{
			// Has no parameters
			return 0; // Can return 0 or 4
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2CBE959B, version = 150) public int sceUmdExecReqSenseCmd(pspsharp.HLE.TPointer unknown1, pspsharp.HLE.TPointer unknown2, int cmd)
		[HLEFunction(nid : 0x2CBE959B, version : 150)]
		public virtual int sceUmdExecReqSenseCmd(TPointer unknown1, TPointer unknown2, int cmd)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC8D137FA, version = 150) public int sceUmdMan_C8D137FA(pspsharp.HLE.TPointer unknown)
		[HLEFunction(nid : 0xC8D137FA, version : 150)]
		public virtual int sceUmdMan_C8D137FA(TPointer unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2CE918B1, version = 150) public int sceUmdMan_2CE918B1()
		[HLEFunction(nid : 0x2CE918B1, version : 150)]
		public virtual int sceUmdMan_2CE918B1()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA7536109, version = 150) public int sceUmdMan_A7536109(int unknown)
		[HLEFunction(nid : 0xA7536109, version : 150)]
		public virtual int sceUmdMan_A7536109(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE779ECEF, version = 150) public int sceUmdManGetInquiry(int unknown, int outputBufferLength, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.previousParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer outputBuffer)
		[HLEFunction(nid : 0xE779ECEF, version : 150)]
		public virtual int sceUmdManGetInquiry(int unknown, int outputBufferLength, TPointer outputBuffer)
		{
			outputBuffer.clear(outputBufferLength);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x921E7B7D, version = 150) public int sceUmdMan_driver_921E7B7D()
		[HLEFunction(nid : 0x921E7B7D, version : 150)]
		public virtual int sceUmdMan_driver_921E7B7D()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8CFED611, version = 150) public int sceUmdManStart(int unknown)
		[HLEFunction(nid : 0x8CFED611, version : 150)]
		public virtual int sceUmdManStart(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1F9AFFF4, version = 150) public int sceUmdManMediaPresent(int unknown)
		[HLEFunction(nid : 0x1F9AFFF4, version : 150)]
		public virtual int sceUmdManMediaPresent(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD372D6F3, version = 150) public int sceUmdMan_driver_D372D6F3(pspsharp.HLE.TPointer driveInformationAddress)
		[HLEFunction(nid : 0xD372D6F3, version : 150)]
		public virtual int sceUmdMan_driver_D372D6F3(TPointer driveInformationAddress)
		{
			return 0;
		}
	}

}