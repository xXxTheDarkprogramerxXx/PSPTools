using System.Threading;

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
//	import static pspsharp.Allegrex.compiler.RuntimeContext.setLog4jMDC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.HLEModuleManager.HLESyscallNid;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_SEEK_SET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.installHLESyscall;


	using Logger = org.apache.log4j.Logger;

	using IVirtualFileSystem = pspsharp.HLE.VFS.IVirtualFileSystem;
	using Fat32VirtualFile = pspsharp.HLE.VFS.fat.Fat32VirtualFile;
	using LocalVirtualFileSystem = pspsharp.HLE.VFS.local.LocalVirtualFileSystem;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using Managers = pspsharp.HLE.kernel.Managers;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using pspIoDrv = pspsharp.HLE.kernel.types.pspIoDrv;
	using pspIoDrvArg = pspsharp.HLE.kernel.types.pspIoDrvArg;
	using pspIoDrvFileArg = pspsharp.HLE.kernel.types.pspIoDrvFileArg;
	using pspIoDrvFuncs = pspsharp.HLE.kernel.types.pspIoDrvFuncs;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using Settings = pspsharp.settings.Settings;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;
	using Utilities = pspsharp.util.Utilities;

	public class sceMSstor : HLEModule
	{
		public static Logger log = Modules.getLogger("sceMSstor");
		private const int STATE_VERSION = 0;
		private sbyte[] dumpIoIoctl_0x02125803;
		private long position;
		private Fat32VirtualFile vFile;
		private Fat32ScanThread scanThread;

		private class AfterAddDrvController : IAction
		{
			internal SceKernelThreadInfo thread;
			internal int sceIoAddDrv;
			internal int storageDrvAddr;
			internal int partitionDrvAddr;

			public AfterAddDrvController(SceKernelThreadInfo thread, int sceIoAddDrv, int storageDrvAddr, int partitionDrvAddr)
			{
				this.thread = thread;
				this.sceIoAddDrv = sceIoAddDrv;
				this.storageDrvAddr = storageDrvAddr;
				this.partitionDrvAddr = partitionDrvAddr;
			}

			public virtual void execute()
			{
				Modules.ThreadManForUserModule.executeCallback(thread, sceIoAddDrv, new AfterAddDrvStorage(thread, sceIoAddDrv, partitionDrvAddr), false, storageDrvAddr);
			}
		}

		private class AfterAddDrvStorage : IAction
		{
			internal SceKernelThreadInfo thread;
			internal int sceIoAddDrv;
			internal int partitionDrvAddr;

			public AfterAddDrvStorage(SceKernelThreadInfo thread, int sceIoAddDrv, int partitionDrvAddr)
			{
				this.thread = thread;
				this.sceIoAddDrv = sceIoAddDrv;
				this.partitionDrvAddr = partitionDrvAddr;
			}

			public virtual void execute()
			{
				Modules.ThreadManForUserModule.executeCallback(thread, sceIoAddDrv, null, false, partitionDrvAddr);
			}
		}

		private class Fat32ScanThread : Thread
		{
			internal Fat32VirtualFile vFile;
			internal bool completed;

			public Fat32ScanThread(Fat32VirtualFile vFile)
			{
				this.vFile = vFile;
				completed = false;
			}

			public override void run()
			{
				setLog4jMDC();
				vFile.scan();
				completed = true;
			}

			public virtual bool Completed
			{
				get
				{
					return completed;
				}
			}

			public virtual void waitForCompletion()
			{
				while (!Completed)
				{
					Utilities.sleep(1, 0);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			dumpIoIoctl_0x02125803 = stream.readBytesWithLength();
			position = stream.readLong();
			bool vFilePresent = stream.readBoolean();
			if (vFilePresent)
			{
				openFile();
				vFile.read(stream);
			}

			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeBytesWithLength(dumpIoIoctl_0x02125803);
			stream.writeLong(position);

			if (vFile != null)
			{
				stream.writeBoolean(true);
				vFile.write(stream);
			}
			else
			{
				stream.writeBoolean(false);
			}

			base.write(stream);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorControllerIoInit(pspsharp.HLE.kernel.types.pspIoDrvArg drvArg)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorControllerIoInit(pspIoDrvArg drvArg)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorControllerIoDevctl(pspsharp.HLE.kernel.types.pspIoDrvFileArg drvFileArg, pspsharp.HLE.PspString devicename, int cmd, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer indata, int inlen, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer outdata, int outlen)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorControllerIoDevctl(pspIoDrvFileArg drvFileArg, PspString devicename, int cmd, TPointer indata, int inlen, TPointer outdata, int outlen)
		{
			switch (cmd)
			{
				case 0x0203D802: // Set EventFlag named SceFatfsDetectMedium
					int eventFlag = indata.getValue32();
					// Event 0x1: memory stick inserted?
					// Event 0x2: memory stick ejected?
					Managers.eventFlags.sceKernelSetEventFlag(eventFlag, 0x1);
					outdata.setValue32(0);
					break;
				case 0x02025801: // Check the MemoryStick's driver status
					outdata.setValue32(4);
					break;
				case 0x2015807:
					outdata.setValue32(1); // Unknown value: seems to be 0 or 1?
					break;
				case 0x0202580A:
					outdata.clear(outlen);
					break;
				case 0x201580B:
					// inlen == 20, outlen == 0
					break;
				case 0x02025806: // Check if the device is inserted
					// 0 = Not inserted.
					// 1 = Inserted.
					outdata.setValue32(1);
					break;
				default:
					log.warn(string.Format("hleMSstorControllerIoDevctl 0x{0:X8} unknown command on device '{1}'", cmd, devicename));
					break;
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorStorageIoInit(pspsharp.HLE.kernel.types.pspIoDrvArg drvArg)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorStorageIoInit(pspIoDrvArg drvArg)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorStorageIoDevctl(pspsharp.HLE.kernel.types.pspIoDrvFileArg drvFileArg, pspsharp.HLE.PspString devicename, int cmd, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer indata, int inlen, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer outdata, int outlen)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorStorageIoDevctl(pspIoDrvFileArg drvFileArg, PspString devicename, int cmd, TPointer indata, int inlen, TPointer outdata, int outlen)
		{
			switch (cmd)
			{
				case 0x02125802:
					outdata.setValue32(0); // ???
					break;
				case 0x0211D814:
					// inlen == 4, outlen == 0
					break;
				case 0x0210D816: // Format Memory Stick
					// inlen == 0, outlen == 0
					log.warn(string.Format("A FORMAT of the Memory Stick was requested, ignoring the request"));
					break;
				case 0x02025806: // Check if the device is inserted
					// 0 = Not inserted.
					// 1 = Inserted.
					outdata.setValue32(1);
					break;
				default:
					log.warn(string.Format("hleMSstorStorageIoDevctl 0x{0:X8} unknown command on device '{1}'", cmd, devicename));
					break;
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorStorageIoOpen(pspsharp.HLE.kernel.types.pspIoDrvFileArg drvFileArg, pspsharp.HLE.PspString fileName, int flags, int mode)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorStorageIoOpen(pspIoDrvFileArg drvFileArg, PspString fileName, int flags, int mode)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorStorageIoIoctl(pspsharp.HLE.kernel.types.pspIoDrvFileArg drvFileArg, int cmd, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer indata, int inlen, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer outdata, int outlen)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorStorageIoIoctl(pspIoDrvFileArg drvFileArg, int cmd, TPointer indata, int inlen, TPointer outdata, int outlen)
		{
			switch (cmd)
			{
				case 0x02125008:
					outdata.setValue32(1); // 0 or != 0
					break;
				case 0x02125009:
					outdata.setValue32(0); // 0 or != 0
					break;
				default:
					log.warn(string.Format("hleMSstorStorageIoIoctl 0x{0:X8} unknown command", cmd));
					break;
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorStorageIoClose(pspsharp.HLE.kernel.types.pspIoDrvFileArg drvFileArg)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorStorageIoClose(pspIoDrvFileArg drvFileArg)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorPartitionIoInit(pspsharp.HLE.kernel.types.pspIoDrvArg drvArg)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorPartitionIoInit(pspIoDrvArg drvArg)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorPartitionIoDevctl(pspsharp.HLE.kernel.types.pspIoDrvFileArg drvFileArg, pspsharp.HLE.PspString devicename, int cmd, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer indata, int inlen, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer outdata, int outlen)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorPartitionIoDevctl(pspIoDrvFileArg drvFileArg, PspString devicename, int cmd, TPointer indata, int inlen, TPointer outdata, int outlen)
		{
			switch (cmd)
			{
				case 0x02125802:
					outdata.setValue32(0); // ???
					break;
				default:
					log.warn(string.Format("hleMSstorPartitionIoDevctl 0x{0:X8} unknown command on device '{1}'", cmd, devicename));
					break;
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorPartitionIoIoctl(pspsharp.HLE.kernel.types.pspIoDrvFileArg drvFileArg, int cmd, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer indata, int inlen, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer outdata, int outlen)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorPartitionIoIoctl(pspIoDrvFileArg drvFileArg, int cmd, TPointer indata, int inlen, TPointer outdata, int outlen)
		{
			switch (cmd)
			{
				case 0x02125001: // Mounted?
					outdata.setValue32(1); // When returning 0, ERROR_ERRNO_DEVICE_NOT_FOUND is raised
					break;
				case 0x02125803:
					outdata.clear(outlen);
					if (dumpIoIoctl_0x02125803 != null)
					{
						Utilities.writeBytes(outdata.Address, outlen, dumpIoIoctl_0x02125803, 0);
					}
					else
					{
						outdata.setValue8(0, (sbyte) 0x02);
					}
					break;
				case 0x02125008:
					outdata.setValue32(1); // 0 or != 0
					break;
				case 0x02125009:
					outdata.setValue32(0); // 0 or != 0
					break;
				default:
					log.warn(string.Format("hleMSstorPartitionIoIoctl 0x{0:X8} unknown command", cmd));
					break;
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorPartitionIoOpen(pspsharp.HLE.kernel.types.pspIoDrvFileArg drvFileArg, pspsharp.HLE.PspString fileName, int flags, int mode)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorPartitionIoOpen(pspIoDrvFileArg drvFileArg, PspString fileName, int flags, int mode)
		{
			position = 0L;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorPartitionIoClose(pspsharp.HLE.kernel.types.pspIoDrvFileArg drvFileArg)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorPartitionIoClose(pspIoDrvFileArg drvFileArg)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public long hleMSstorPartitionIoLseek(pspsharp.HLE.kernel.types.pspIoDrvFileArg drvFileArg, long offset, int whence)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual long hleMSstorPartitionIoLseek(pspIoDrvFileArg drvFileArg, long offset, int whence)
		{
			switch (whence)
			{
				case PSP_SEEK_SET:
					position = offset;
					break;
				default:
					log.warn(string.Format("hleMSstorPartitionIoLseek unimplemented whence=0x{0:X}", whence));
					break;
			}

			if (vFile != null)
			{
				position = vFile.ioLseek(position);
			}

			return position;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorPartitionIoRead(pspsharp.HLE.kernel.types.pspIoDrvFileArg drvFileArg, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.returnValue, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer data, int len)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorPartitionIoRead(pspIoDrvFileArg drvFileArg, TPointer data, int len)
		{
			data.clear(len);

			if (vFile != null)
			{
				scanThread.waitForCompletion();
				len = vFile.ioRead(data, len);
			}

			return len;
		}

		public virtual int hleMSstorPartitionIoRead(sbyte[] buffer, int bufferOffset, int len)
		{
			if (vFile != null)
			{
				scanThread.waitForCompletion();
				len = vFile.ioRead(buffer, bufferOffset, len);
			}

			return len;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = HLESyscallNid, version = 150) public int hleMSstorPartitionIoWrite(pspsharp.HLE.kernel.types.pspIoDrvFileArg drvFileArg, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer data, int len)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleMSstorPartitionIoWrite(pspIoDrvFileArg drvFileArg, TPointer data, int len)
		{
			if (vFile != null)
			{
				scanThread.waitForCompletion();
				len = vFile.ioWrite(data, len);
			}

			return len;
		}

		private static sbyte[] readBytes(string fileName)
		{
			sbyte[] bytes = null;
			try
			{
				File file = new File(fileName);
				System.IO.Stream @is = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				bytes = new sbyte[(int) file.length()];
				@is.Read(bytes, 0, bytes.Length);
				@is.Close();
			}
			catch (FileNotFoundException)
			{
			}
			catch (IOException)
			{
			}

			return bytes;
		}

		private void installIoFunctions(pspIoDrvFuncs controllerFuncs, pspIoDrvFuncs storageFuncs, pspIoDrvFuncs partitionFuncs)
		{
			const int sizeIoFunctionStub = 12;
			const int numberIoFunctions = 15;
			SysMemInfo memInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "sceMSstor-IoFunctions", SysMemUserForUser.PSP_SMEM_Low, sizeIoFunctionStub * numberIoFunctions, 0);
			int addr = memInfo.addr;

			controllerFuncs.ioInit = addr;
			addr += sizeIoFunctionStub;
			controllerFuncs.ioDevctl = addr;
			addr += sizeIoFunctionStub;
			installHLESyscall(controllerFuncs.ioInit, this, "hleMSstorControllerIoInit");
			installHLESyscall(controllerFuncs.ioDevctl, this, "hleMSstorControllerIoDevctl");

			storageFuncs.ioInit = addr;
			addr += sizeIoFunctionStub;
			storageFuncs.ioDevctl = addr;
			addr += sizeIoFunctionStub;
			storageFuncs.ioOpen = addr;
			addr += sizeIoFunctionStub;
			storageFuncs.ioIoctl = addr;
			addr += sizeIoFunctionStub;
			storageFuncs.ioClose = addr;
			addr += sizeIoFunctionStub;
			installHLESyscall(storageFuncs.ioInit, this, "hleMSstorStorageIoInit");
			installHLESyscall(storageFuncs.ioDevctl, this, "hleMSstorStorageIoDevctl");
			installHLESyscall(storageFuncs.ioOpen, this, "hleMSstorStorageIoOpen");
			installHLESyscall(storageFuncs.ioIoctl, this, "hleMSstorStorageIoIoctl");
			installHLESyscall(storageFuncs.ioClose, this, "hleMSstorStorageIoClose");

			partitionFuncs.ioInit = addr;
			addr += sizeIoFunctionStub;
			partitionFuncs.ioDevctl = addr;
			addr += sizeIoFunctionStub;
			partitionFuncs.ioOpen = addr;
			addr += sizeIoFunctionStub;
			partitionFuncs.ioClose = addr;
			addr += sizeIoFunctionStub;
			partitionFuncs.ioIoctl = addr;
			addr += sizeIoFunctionStub;
			partitionFuncs.ioLseek = addr;
			addr += sizeIoFunctionStub;
			partitionFuncs.ioRead = addr;
			addr += sizeIoFunctionStub;
			partitionFuncs.ioWrite = addr;
			addr += sizeIoFunctionStub;
			installHLESyscall(partitionFuncs.ioInit, this, "hleMSstorPartitionIoInit");
			installHLESyscall(partitionFuncs.ioDevctl, this, "hleMSstorPartitionIoDevctl");
			installHLESyscall(partitionFuncs.ioOpen, this, "hleMSstorPartitionIoOpen");
			installHLESyscall(partitionFuncs.ioClose, this, "hleMSstorPartitionIoClose");
			installHLESyscall(partitionFuncs.ioIoctl, this, "hleMSstorPartitionIoIoctl");
			installHLESyscall(partitionFuncs.ioLseek, this, "hleMSstorPartitionIoLseek");
			installHLESyscall(partitionFuncs.ioRead, this, "hleMSstorPartitionIoRead");
			installHLESyscall(partitionFuncs.ioWrite, this, "hleMSstorPartitionIoWrite");
		}

		private void openFile()
		{
			IVirtualFileSystem vfs = new LocalVirtualFileSystem(Settings.Instance.getDirectoryMapping("ms0"), true);
			vFile = new Fat32VirtualFile("ms0:", vfs);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("openFile vFile={0}", vFile));
			}
		}

		public virtual void hleInit()
		{
			openFile();

			scanThread = new Fat32ScanThread(vFile);
			scanThread.Name = "Fat32VirtualFile Scan Thread";
			scanThread.Daemon = true;
			scanThread.Start();
		}

		public virtual void installDrivers()
		{
			Memory mem = Memory.Instance;

			dumpIoIoctl_0x02125803 = readBytes("ms.ioctl.0x02125803");

			hleInit();

			pspIoDrv controllerDrv = new pspIoDrv();
			pspIoDrvFuncs controllerFuncs = new pspIoDrvFuncs();
			string controllerName = "mscmhc";
			string controllerDescription = "MS host controller";

			pspIoDrv storageDrv = new pspIoDrv();
			pspIoDrvFuncs storageFuncs = new pspIoDrvFuncs();
			string storageName = "msstor";
			string storageDescription = "MSstor whole dev";

			pspIoDrv partitionDrv = new pspIoDrv();
			pspIoDrvFuncs partitionFuncs = new pspIoDrvFuncs();
			string partitionName = "msstor0p";
			string partitionDescription = "MSstor partition #1";

			int length = 0;
			length += controllerDrv.@sizeof() + controllerFuncs.@sizeof() + controllerName.Length + 1 + controllerDescription.Length + 1;
			length += storageDrv.@sizeof() + storageFuncs.@sizeof() + storageName.Length + 1 + storageDescription.Length + 1;
			length += partitionDrv.@sizeof() + partitionFuncs.@sizeof() + partitionName.Length + 1 + partitionDescription.Length + 1;
			SysMemInfo memInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "sceMSstor-mscmhc", SysMemUserForUser.PSP_SMEM_Low, length, 0);

			int controllerDrvAddr = memInfo.addr;
			int controllerFuncsAddr = controllerDrvAddr + controllerDrv.@sizeof();
			int storageDrvAddr = controllerFuncsAddr + controllerFuncs.@sizeof();
			int storageFuncsAddr = storageDrvAddr + storageDrv.@sizeof();
			int partitionDrvAddr = storageFuncsAddr + controllerFuncs.@sizeof();
			int partitionFuncsAddr = partitionDrvAddr + partitionDrv.@sizeof();
			int controllerNameAddr = partitionFuncsAddr + partitionFuncs.@sizeof();
			int controllerDescriptionAddr = controllerNameAddr + controllerName.Length + 1;
			int storageNameAddr = controllerDescriptionAddr + controllerDescription.Length + 1;
			int storageDescriptionAddr = storageNameAddr + storageName.Length + 1;
			int partitionNameAddr = storageDescriptionAddr + storageDescription.Length + 1;
			int partitionDescriptionAddr = partitionNameAddr + partitionName.Length + 1;

			installIoFunctions(controllerFuncs, storageFuncs, partitionFuncs);

			Utilities.writeStringZ(mem, controllerNameAddr, controllerName);
			Utilities.writeStringZ(mem, controllerDescriptionAddr, controllerDescription);
			controllerDrv.nameAddr = controllerNameAddr;
			controllerDrv.name = controllerName;
			controllerDrv.devType = 1;
			controllerDrv.unknown = 0;
			controllerDrv.descriptionAddr = controllerDescriptionAddr;
			controllerDrv.description = controllerDescription;
			controllerDrv.funcsAddr = controllerFuncsAddr;
			controllerDrv.ioDrvFuncs = controllerFuncs;
			controllerDrv.write(mem, controllerDrvAddr);

			Utilities.writeStringZ(mem, storageNameAddr, storageName);
			Utilities.writeStringZ(mem, storageDescriptionAddr, storageDescription);
			storageDrv.nameAddr = storageNameAddr;
			storageDrv.name = storageName;
			storageDrv.devType = 1;
			storageDrv.unknown = 0;
			storageDrv.descriptionAddr = storageDescriptionAddr;
			storageDrv.description = storageDescription;
			storageDrv.funcsAddr = storageFuncsAddr;
			storageDrv.ioDrvFuncs = storageFuncs;
			storageDrv.write(mem, storageDrvAddr);

			Utilities.writeStringZ(mem, partitionNameAddr, partitionName);
			Utilities.writeStringZ(mem, partitionDescriptionAddr, partitionDescription);
			partitionDrv.nameAddr = partitionNameAddr;
			partitionDrv.name = partitionName;
			partitionDrv.devType = 1;
			partitionDrv.unknown = 0;
			partitionDrv.descriptionAddr = partitionDescriptionAddr;
			partitionDrv.description = partitionDescription;
			partitionDrv.funcsAddr = partitionFuncsAddr;
			partitionDrv.ioDrvFuncs = partitionFuncs;
			partitionDrv.write(mem, partitionDrvAddr);

			int sceIoAddDrv = NIDMapper.Instance.getAddressByName("sceIoAddDrv");
			if (sceIoAddDrv != 0)
			{
				SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
				Modules.ThreadManForUserModule.executeCallback(thread, sceIoAddDrv, new AfterAddDrvController(thread, sceIoAddDrv, storageDrvAddr, partitionDrvAddr), false, controllerDrvAddr);
			}
		}

		/// <summary>
		/// This is the function executed at module start (alias to "module_start")
		/// </summary>
		/// <param name="unknown1"> </param>
		/// <param name="unknown2"> </param>
		/// <param name="unknown3">
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6FC1E8AE, version = 150) public int sceMSstorEntry(int unknown1, int unknown2, int unknown3)
		[HLEFunction(nid : 0x6FC1E8AE, version : 150)]
		public virtual int sceMSstorEntry(int unknown1, int unknown2, int unknown3)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x714782D6, version = 150) public int sceMSstorRegisterCLDMSelf(int unknown)
		[HLEFunction(nid : 0x714782D6, version : 150)]
		public virtual int sceMSstorRegisterCLDMSelf(int unknown)
		{
			return 0;
		}
	}

}