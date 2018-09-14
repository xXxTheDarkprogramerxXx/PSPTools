using System;
using System.Collections.Generic;
using System.Text;

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
//	import static pspsharp.Allegrex.Common._s0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.local.LocalVirtualFileSystem.getMsFileName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ERRNO_DEVICE_NOT_FOUND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ERRNO_FILE_ALREADY_EXISTS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ERRNO_FILE_NOT_FOUND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ERRNO_READ_ONLY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_INVALID_ARGUMENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_ASYNC_BUSY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_FILE_READ_ERROR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NOCWD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NO_ASYNC_OP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NO_SUCH_DEVICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_TOO_MANY_OPEN_FILES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_UNSUPPORTED_OPERATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.JPCSP_WAIT_IO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_READY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.THREAD_CALLBACK_MEMORYSTICK_FAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readStringNZ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readStringZ;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;


	using CpuState = pspsharp.Allegrex.CpuState;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using IVirtualFileSystem = pspsharp.HLE.VFS.IVirtualFileSystem;
	using SeekableDataInputVirtualFile = pspsharp.HLE.VFS.SeekableDataInputVirtualFile;
	using VirtualFileSystemManager = pspsharp.HLE.VFS.VirtualFileSystemManager;
	using PGDVirtualFile = pspsharp.HLE.VFS.crypto.PGDVirtualFile;
	using EmulatorVirtualFileSystem = pspsharp.HLE.VFS.emulator.EmulatorVirtualFileSystem;
	using UmdIsoVirtualFile = pspsharp.HLE.VFS.iso.UmdIsoVirtualFile;
	using UmdIsoVirtualFileSystem = pspsharp.HLE.VFS.iso.UmdIsoVirtualFileSystem;
	using LocalVirtualFileSystem = pspsharp.HLE.VFS.local.LocalVirtualFileSystem;
	using MemoryStickStorageVirtualFileSystem = pspsharp.HLE.VFS.memoryStick.MemoryStickStorageVirtualFileSystem;
	using MemoryStickVirtualFileSystem = pspsharp.HLE.VFS.memoryStick.MemoryStickVirtualFileSystem;
	using Managers = pspsharp.HLE.kernel.Managers;
	using MsgPipeManager = pspsharp.HLE.kernel.managers.MsgPipeManager;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using SceIoDirent = pspsharp.HLE.kernel.types.SceIoDirent;
	using SceIoStat = pspsharp.HLE.kernel.types.SceIoStat;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelMppInfo = pspsharp.HLE.kernel.types.SceKernelMppInfo;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ScePspDateTime = pspsharp.HLE.kernel.types.ScePspDateTime;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using SeekableDataInput = pspsharp.filesystems.SeekableDataInput;
	using SeekableRandomFile = pspsharp.filesystems.SeekableRandomFile;
	using UmdIsoFile = pspsharp.filesystems.umdiso.UmdIsoFile;
	using UmdIsoReader = pspsharp.filesystems.umdiso.UmdIsoReader;
	using MemoryStick = pspsharp.hardware.MemoryStick;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using Settings = pspsharp.settings.Settings;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class IoFileMgrForUser : HLEModule
	{
		public static Logger log = Modules.getLogger("IoFileMgrForUser");
		private static Logger stdout = Logger.getLogger("stdout");
		private static Logger stderr = Logger.getLogger("stderr");
		public const int PSP_O_RDONLY = 0x0001;
		public const int PSP_O_WRONLY = 0x0002;
		public static readonly int PSP_O_RDWR = (PSP_O_RDONLY | PSP_O_WRONLY);
		public const int PSP_O_NBLOCK = 0x0004;
		public const int PSP_O_DIROPEN = 0x0008;
		public const int PSP_O_APPEND = 0x0100;
		public const int PSP_O_CREAT = 0x0200;
		public const int PSP_O_TRUNC = 0x0400;
		public const int PSP_O_EXCL = 0x0800;
		public const int PSP_O_NBUF = 0x4000; // Used on the PSP to bypass the internal disc cache (commonly seen in media files that need to maintain a fixed bitrate).
		public const int PSP_O_NOWAIT = 0x8000;
		public const int PSP_O_PLOCK = 0x2000000; // Used on the PSP to open the file inside a power lock (safe).
		public const int PSP_O_FGAMEDATA = 0x40000000; // Used on the PSP to handle encrypted data (used by NPDRM module).

		//Every flag seems to be ORed with a retry count.
		//In Activision Hits Remixed, an error is produced after
		//the retry count (0xf0000/15) is over.
		public const int PSP_O_RETRY_0 = 0x00000;
		public const int PSP_O_RETRY_1 = 0x10000;
		public const int PSP_O_RETRY_2 = 0x20000;
		public const int PSP_O_RETRY_3 = 0x30000;
		public const int PSP_O_RETRY_4 = 0x40000;
		public const int PSP_O_RETRY_5 = 0x50000;
		public const int PSP_O_RETRY_6 = 0x60000;
		public const int PSP_O_RETRY_7 = 0x70000;
		public const int PSP_O_RETRY_8 = 0x80000;
		public const int PSP_O_RETRY_9 = 0x90000;
		public const int PSP_O_RETRY_10 = 0xa0000;
		public const int PSP_O_RETRY_11 = 0xb0000;
		public const int PSP_O_RETRY_12 = 0xc0000;
		public const int PSP_O_RETRY_13 = 0xd0000;
		public const int PSP_O_RETRY_14 = 0xe0000;
		public const int PSP_O_RETRY_15 = 0xf0000;

		public const int PSP_SEEK_SET = 0;
		public const int PSP_SEEK_CUR = 1;
		public const int PSP_SEEK_END = 2;

		// Type of device used (abstract).
		// Can symbolize physical character or block devices, logical filesystem devices
		// or devices represented by an alias or even mount point devices also represented by an alias.
		public const int PSP_DEV_TYPE_NONE = 0x0;
		public const int PSP_DEV_TYPE_CHARACTER = 0x1;
		public const int PSP_DEV_TYPE_BLOCK = 0x4;
		public const int PSP_DEV_TYPE_FILESYSTEM = 0x10;
		public const int PSP_DEV_TYPE_ALIAS = 0x20;
		public const int PSP_DEV_TYPE_MOUNT = 0x40;

		// PSP opens STDIN, STDOUT, STDERR in this order:
		public const int STDIN_ID = 0;
		public const int STDOUT_ID = 1;
		public const int STDERR_ID = 2;
		protected internal SceKernelMppInfo[] stdRedirects;

		private const int MIN_ID = 3;
		private const int MAX_ID = 63;
		private const string idPurpose = "IOFileManager-File";

		private const bool useVirtualFileSystem = false;
		protected internal VirtualFileSystemManager vfsManager;
		protected internal IDictionary<string, string> assignedDevices;

		public class IoOperationTiming
		{
			internal int delayMillis;
			internal int sizeUnit;

			public IoOperationTiming()
			{
				this.delayMillis = 0;
			}

			public IoOperationTiming(int delayMillis)
			{
				this.delayMillis = delayMillis;
				this.sizeUnit = 0;
			}

			public IoOperationTiming(int delayMillis, int sizeUnit)
			{
				this.delayMillis = delayMillis;
				this.sizeUnit = sizeUnit;
			}

			/// <summary>
			/// Return a delay in milliseconds for the IoOperation.
			/// </summary>
			/// <returns>       the delay in milliseconds </returns>
			internal virtual int DelayMillis
			{
				get
				{
					return delayMillis;
				}
				set
				{
					this.delayMillis = value;
				}
			}

			/// <summary>
			/// Return a delay in milliseconds based on the size of the
			/// processed data.
			/// </summary>
			/// <param name="size">   size of the processed data.
			///               0 if no size is available. </param>
			/// <returns>       the delay in milliseconds </returns>
			internal virtual int getDelayMillis(int size)
			{
				if (sizeUnit == 0 || size <= 0)
				{
					return DelayMillis;
				}

				// Return a delay based on the given size.
				// Return at least the delayMillis.
				return System.Math.Max((int)(((long) delayMillis) * size / sizeUnit), delayMillis);
			}

		}

		public enum IoOperation
		{
			open,
			close,
			seek,
			ioctl,
			remove,
			rename,
			mkdir,
			dread,
			iodevctl,
			read,
			write
		}

		public static readonly IDictionary<IoOperation, IoOperationTiming> defaultTimings = new Dictionary<IoFileMgrForUser.IoOperation, IoFileMgrForUser.IoOperationTiming>();
		public static readonly IDictionary<IoOperation, IoOperationTiming> noDelayTimings = new Dictionary<IoFileMgrForUser.IoOperation, IoFileMgrForUser.IoOperationTiming>();

		// modeStrings indexed by [0, PSP_O_RDONLY, PSP_O_WRONLY, PSP_O_RDWR]
		// SeekableRandomFile doesn't support write only: take "rw",
		private static readonly string[] modeStrings = new string[] {"r", "r", "rw", "rw"};
		public Dictionary<int, IoInfo> fileIds;
		public Dictionary<int, IoInfo> fileUids;
		public Dictionary<int, IoDirInfo> dirIds;
		private string filepath; // current working directory on PC
		private UmdIsoReader iso;
		private IoWaitStateChecker ioWaitStateChecker;
		private string host0Path;
		private int previousFatMsState;

		private int defaultAsyncPriority;
		private static readonly int asyncThreadRegisterArgument = _s0; // $s0 is preserved across calls
		private bool noDelayIoOperation;

		private bool allowExtractPGD;

		// Implement the list of IIoListener as an array to improve the performance
		// when iterating over all the entries (most common action).
		private IIoListener[] ioListeners;

		public class IoInfo
		{
			internal bool InstanceFieldsInitialized = false;

			internal virtual void InitializeInstanceFields()
			{
				asyncThreadPriority = outerInstance.defaultAsyncPriority;
			}

			private readonly IoFileMgrForUser outerInstance;

			// PSP settings

			public readonly int flags;
			public readonly int permissions;

			// Internal settings
			public readonly string filename;
			public readonly SeekableRandomFile msFile; // on memory stick, should either be identical to readOnlyFile or null
			public SeekableDataInput readOnlyFile; // on memory stick or umd
			public IVirtualFile vFile;
			public readonly string mode;
			public long position; // virtual position, beyond the end is allowed, before the start is an error
			public bool sectorBlockMode;
			public readonly int id;
			public readonly int uid;
			public long result; // The return value from the last operation on this file, used by sceIoWaitAsync
			public bool closePending = false; // sceIoCloseAsync has been called on this file
			public bool asyncPending; // Thread has not switched since an async operation was called on this file
			public bool asyncResultPending; // Async IO result is available and has not yet been retrieved
			public long asyncDoneMillis; // When the async operation can be completed
			public int asyncThreadPriority;
			public SceKernelThreadInfo asyncThread;
			public IAction asyncAction;
			internal bool truncateAtNextWrite;

			// Async callback
			public int cbid = -1;
			public int notifyArg = 0;

			/// <summary>
			/// Memory stick version </summary>
			public IoInfo(IoFileMgrForUser outerInstance, string filename, SeekableRandomFile f, string mode, int flags, int permissions)
			{
				this.outerInstance = outerInstance;

				if (!InstanceFieldsInitialized)
				{
					InitializeInstanceFields();
					InstanceFieldsInitialized = true;
				}
				vFile = null;
				this.filename = filename;
				msFile = f;
				readOnlyFile = f;
				this.mode = mode;
				this.flags = flags;
				this.permissions = permissions;
				sectorBlockMode = false;
				id = NewId;
				if (ValidId)
				{
					uid = NewUid;
					outerInstance.fileIds[id] = this;
					outerInstance.fileUids[uid] = this;
				}
				else
				{
					uid = -1;
				}
			}

			/// <summary>
			/// UMD version (read only) </summary>
			public IoInfo(IoFileMgrForUser outerInstance, string filename, SeekableDataInput f, string mode, int flags, int permissions)
			{
				this.outerInstance = outerInstance;

				if (!InstanceFieldsInitialized)
				{
					InitializeInstanceFields();
					InstanceFieldsInitialized = true;
				}
				vFile = null;
				this.filename = filename;
				msFile = null;
				readOnlyFile = f;
				this.mode = mode;
				this.flags = flags;
				this.permissions = permissions;
				sectorBlockMode = false;
				id = NewId;
				if (ValidId)
				{
					uid = NewUid;
					outerInstance.fileIds[id] = this;
					outerInstance.fileUids[uid] = this;
				}
				else
				{
					uid = -1;
				}
			}

			/// <summary>
			/// VirtualFile version </summary>
			public IoInfo(IoFileMgrForUser outerInstance, string filename, IVirtualFile f, string mode, int flags, int permissions)
			{
				this.outerInstance = outerInstance;

				if (!InstanceFieldsInitialized)
				{
					InitializeInstanceFields();
					InstanceFieldsInitialized = true;
				}
				vFile = f;
				this.filename = filename;
				msFile = null;
				readOnlyFile = null;
				this.mode = mode;
				this.flags = flags;
				this.permissions = permissions;
				sectorBlockMode = false;
				id = NewId;
				if (ValidId)
				{
					uid = NewUid;
					outerInstance.fileIds[id] = this;
					outerInstance.fileUids[uid] = this;
				}
				else
				{
					uid = -1;
				}
			}

			public virtual bool ValidId
			{
				get
				{
					return id != SceUidManager.INVALID_ID;
				}
			}

			public virtual bool UmdFile
			{
				get
				{
					return (msFile == null);
				}
			}

			public virtual int AsyncRestMillis
			{
				get
				{
					long now = Emulator.Clock.currentTimeMillis();
					if (now >= asyncDoneMillis)
					{
						return 0;
					}
    
					return (int)(asyncDoneMillis - now);
				}
			}

			public virtual void truncate(int length)
			{
				try
				{
					// Only valid for msFile.
					if (msFile != null)
					{
						msFile.Length = length;
					}
				}
				catch (IOException ioe)
				{
					log.debug("truncate", ioe);
				}
			}

			public virtual IoInfo close()
			{
				IoInfo info = outerInstance.fileIds.Remove(id);
				if (info != null)
				{
					outerInstance.fileUids.Remove(uid);
					releaseId(id);
					releaseUid(uid);
				}

				return info;
			}

			public virtual bool TruncateAtNextWrite
			{
				get
				{
					return truncateAtNextWrite;
				}
				set
				{
					this.truncateAtNextWrite = value;
				}
			}


			public override string ToString()
			{
				return string.Format("id=0x{0:X}, fileName='{1}'", id, filename);
			}
		}

		public class IoDirInfo
		{
			private readonly IoFileMgrForUser outerInstance;


			internal readonly string path;
			internal readonly string[] filenames;
			internal int position;
			internal int printableposition;
			internal readonly int id;
			internal readonly IVirtualFileSystem vfs;
			internal string fileNameFilter;

			public IoDirInfo(IoFileMgrForUser outerInstance, string path, string[] filenames)
			{
				this.outerInstance = outerInstance;
				vfs = null;
				id = NewId;
				// iso reader doesn't like path//filename, so trim trailing /
				// (it's like doing cd somedir/ instead of cd somedir, makes little difference)
				if (path.EndsWith("/", StringComparison.Ordinal))
				{
					path = path.Substring(0, path.Length - 1);
				}
				this.path = path;
				this.filenames = filenames;

				init();
			}

			public IoDirInfo(IoFileMgrForUser outerInstance, string path, string[] filenames, IVirtualFileSystem vfs)
			{
				this.outerInstance = outerInstance;
				this.vfs = vfs;
				id = NewId;
				this.path = path;
				this.filenames = filenames;

				init();
			}

			internal virtual void init()
			{
				position = 0;
				printableposition = 0;
				// Hide iso special files
				if (filenames != null)
				{
					if (filenames.Length > position && filenames[position].Equals("."))
					{
						position++;
					}
					if (filenames.Length > position && filenames[position].Equals("\x0001"))
					{
						position++;
					}
				}
				outerInstance.dirIds[id] = this;
			}

			public virtual bool hasNext()
			{
				return (position < filenames.Length);
			}

			public virtual string next()
			{
				string filename = null;
				if (position < filenames.Length)
				{
					filename = filenames[position];
					position++;
					printableposition++;
				}
				return filename;
			}

			public virtual IoDirInfo close()
			{
				IoDirInfo info = outerInstance.dirIds.Remove(id);
				if (info != null)
				{
					releaseId(id);
				}

				return info;
			}
		}

		private class PatternFilter : FilenameFilter
		{

			internal Pattern pattern;

			public PatternFilter(string pattern)
			{
				this.pattern = Pattern.compile(pattern);
			}

			public override bool accept(File dir, string name)
			{
				return pattern.matcher(name).matches();
			}
		}

		public interface IIoListener
		{

			void sceIoSync(int result, int device_addr, string device, int unknown);

			void sceIoPollAsync(int result, int uid, int res_addr);

			void sceIoWaitAsync(int result, int uid, int res_addr);

			void sceIoOpen(int result, int filename_addr, string filename, int flags, int permissions, string mode);

			void sceIoClose(int result, int uid);

			void sceIoWrite(int result, int uid, int data_addr, int size, int bytesWritten);

			void sceIoRead(int result, int uid, int data_addr, int size, int bytesRead, long position, SeekableDataInput dataInput, IVirtualFile vFile);

			void sceIoCancel(int result, int uid);

			void sceIoSeek32(int result, int uid, int offset, int whence);

			void sceIoSeek64(long result, int uid, long offset, int whence);

			void sceIoMkdir(int result, int path_addr, string path, int permissions);

			void sceIoRmdir(int result, int path_addr, string path);

			void sceIoChdir(int result, int path_addr, string path);

			void sceIoDopen(int result, int path_addr, string path);

			void sceIoDread(int result, int uid, int dirent_addr);

			void sceIoDclose(int result, int uid);

			void sceIoDevctl(int result, int device_addr, string device, int cmd, int indata_addr, int inlen, int outdata_addr, int outlen);

			void sceIoIoctl(int result, int uid, int cmd, int indata_addr, int inlen, int outdata_addr, int outlen);

			void sceIoAssign(int result, int dev1_addr, string dev1, int dev2_addr, string dev2, int dev3_addr, string dev3, int mode, int unk1, int unk2);

			void sceIoGetStat(int result, int path_addr, string path, int stat_addr);

			void sceIoRemove(int result, int path_addr, string path);

			void sceIoChstat(int result, int path_addr, string path, int stat_addr, int bits);

			void sceIoRename(int result, int path_addr, string path, int new_path_addr, string newpath);
		}

		private class IoWaitStateChecker : IWaitStateChecker
		{
			private readonly IoFileMgrForUser outerInstance;

			public IoWaitStateChecker(IoFileMgrForUser outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				IoInfo info = outerInstance.fileIds[wait.Io_id];
				if (info == null)
				{
					return false;
				}
				if (!info.asyncPending)
				{
					// Async IO is already completed
					if (info.asyncResultPending)
					{
						if (Memory.isAddressGood(wait.Io_resultAddr))
						{
							if (log.DebugEnabled)
							{
								log.debug(string.Format("IoWaitStateChecker - async completed, writing pending result 0x{0:X}", info.result));
							}
							Memory.Instance.write64(wait.Io_resultAddr, info.result);
						}
						info.asyncResultPending = false;
						info.result = ERROR_KERNEL_NO_ASYNC_OP;
					}
					return false;
				}

				return true;
			}
		}

		private class IOAsyncReadAction : IAction
		{
			private readonly IoFileMgrForUser outerInstance;

			internal IoInfo info;
			internal int address;
			internal int size;
			internal int requestedSize;

			public IOAsyncReadAction(IoFileMgrForUser outerInstance, IoInfo info, int address, int requestedSize, int size)
			{
				this.outerInstance = outerInstance;
				this.info = info;
				this.address = address;
				this.requestedSize = requestedSize;
				this.size = size;
			}

			public virtual void execute()
			{
				long position = info.position;
				int result = 0;

				if (info.vFile != null)
				{
					result = info.vFile.ioRead(new TPointer(Memory.Instance, address), size);
					if (result >= 0)
					{
						info.position += result;
						size = result;
						if (info.sectorBlockMode)
						{
							result /= UmdIsoFile.sectorLength;
						}
					}
					else
					{
						size = 0;
					}
				}
				else
				{
					try
					{
						Utilities.readFully(info.readOnlyFile, address, size);
						info.position += size;
						result = size;
						if (info.sectorBlockMode)
						{
							result /= UmdIsoFile.sectorLength;
						}
					}
					catch (IOException e)
					{
						log.error(e);
						result = ERROR_KERNEL_FILE_READ_ERROR;
					}
				}

				info.result = result;

				// Invalidate any compiled code in the read range
				RuntimeContext.invalidateRange(address, size);

				foreach (IIoListener ioListener in outerInstance.ioListeners)
				{
					ioListener.sceIoRead(result, info.id, address, requestedSize, size, position, info.readOnlyFile, info.vFile);
				}
			}
		}

		private class ExtractPGDSettingsListerner : AbstractBoolSettingsListener
		{
			private readonly IoFileMgrForUser outerInstance;

			public ExtractPGDSettingsListerner(IoFileMgrForUser outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override void settingsValueChanged(bool value)
			{
				outerInstance.AllowExtractPGDStatus = value;
			}
		}

		public virtual void registerUmdIso()
		{
			if (vfsManager != null && useVirtualFileSystem)
			{
				if (iso != null && Modules.sceUmdUserModule.UmdActivated)
				{
					IVirtualFileSystem vfsIso = new UmdIsoVirtualFileSystem(iso);
					vfsManager.register("disc0", vfsIso);
					vfsManager.register("umd0", vfsIso);
					vfsManager.register("umd1", vfsIso);
					vfsManager.register("umd", vfsIso);
					vfsManager.register("isofs", vfsIso);
				}
				else
				{
					vfsManager.unregister("disc0");
					vfsManager.unregister("umd0");
					vfsManager.unregister("umd1");
					vfsManager.unregister("umd");
					vfsManager.unregister("isofs");

					// Register the local path if the application has been loaded as a file (and not as an UMD).
					if (!string.ReferenceEquals(filepath, null))
					{
						int colon = filepath.IndexOf(':');
						if (colon >= 0)
						{
							string device = filepath.Substring(0, colon);
							device = device.ToLower();
							vfsManager.register(device, new LocalVirtualFileSystem(device + ":\\", false));
						}
					}
				}
			}
		}

		private void registerVfsMs0()
		{
			if (vfsManager == null)
			{
				vfsManager = new VirtualFileSystemManager();
			}
			vfsManager.register("ms0", new LocalVirtualFileSystem(Settings.Instance.getDirectoryMapping("ms0"), true));
			vfsManager.register("fatms0", new LocalVirtualFileSystem(Settings.Instance.getDirectoryMapping("ms0"), true));
			vfsManager.register("flash0", new LocalVirtualFileSystem(Settings.Instance.getDirectoryMapping("flash0"), false));
			vfsManager.register("flash1", new LocalVirtualFileSystem(Settings.Instance.getDirectoryMapping("flash1"), false));
			vfsManager.register("exdata0", new LocalVirtualFileSystem(Settings.Instance.getDirectoryMapping("exdata0"), false));
			vfsManager.register("mscmhc0", new MemoryStickVirtualFileSystem());
			vfsManager.register("msstor0p1", new MemoryStickStorageVirtualFileSystem());
			vfsManager.register("msstor0", new MemoryStickStorageVirtualFileSystem());
		}

		public override void start()
		{
			if (fileIds != null)
			{
				// Close open files
				for (IEnumerator<IoInfo> it = fileIds.Values.GetEnumerator(); it.MoveNext();)
				{
					IoInfo info = it.Current;
					try
					{
						info.readOnlyFile.Dispose();
					}
					catch (IOException e)
					{
						log.error("pspiofilemgr - error closing file: " + e.Message);
					}
				}
			}
			fileIds = new Dictionary<int, IoInfo>();
			fileUids = new Dictionary<int, IoInfo>();
			dirIds = new Dictionary<int, IoDirInfo>();
			MemoryStick.StateMs = MemoryStick.PSP_MEMORYSTICK_STATE_DRIVER_READY;
			defaultAsyncPriority = -1;
			if (ioListeners == null)
			{
				ioListeners = new IIoListener[0];
			}
			ioWaitStateChecker = new IoWaitStateChecker(this);
			host0Path = null;
			noDelayIoOperation = false;
			stdRedirects = new SceKernelMppInfo[3];
			previousFatMsState = MemoryStick.PSP_FAT_MEMORYSTICK_STATE_UNASSIGNED;

			vfsManager = new VirtualFileSystemManager();
			vfsManager.register("emulator", new EmulatorVirtualFileSystem());
			vfsManager.register("kemulator", new EmulatorVirtualFileSystem());
			if (useVirtualFileSystem)
			{
				registerVfsMs0();
				registerUmdIso();
			}

			assignedDevices = new Dictionary<string, string>();

			setSettingsListener("emu.extractPGD", new ExtractPGDSettingsListerner(this));

			defaultTimings[IoOperation.open] = new IoFileMgrForUser.IoOperationTiming(5);
			defaultTimings[IoOperation.close] = new IoFileMgrForUser.IoOperationTiming(1);
			defaultTimings[IoOperation.seek] = new IoFileMgrForUser.IoOperationTiming(1);
			defaultTimings[IoOperation.ioctl] = new IoFileMgrForUser.IoOperationTiming(20);
			defaultTimings[IoOperation.remove] = new IoFileMgrForUser.IoOperationTiming();
			defaultTimings[IoOperation.rename] = new IoFileMgrForUser.IoOperationTiming();
			defaultTimings[IoOperation.mkdir] = new IoFileMgrForUser.IoOperationTiming();
			defaultTimings[IoOperation.dread] = new IoFileMgrForUser.IoOperationTiming();
			defaultTimings[IoOperation.iodevctl] = new IoFileMgrForUser.IoOperationTiming(2);
			// Duration of read operation: approx. 7 ms per 0x10000 bytes (tested on real PSP)
			defaultTimings[IoOperation.read] = new IoFileMgrForUser.IoOperationTiming(7, 0x10000);
			// Duration of write operation: approx. 5 ms per 0x10000 bytes
			defaultTimings[IoOperation.write] = new IoFileMgrForUser.IoOperationTiming(5, 0x10000);

			noDelayTimings[IoOperation.open] = new IoFileMgrForUser.IoOperationTiming();
			noDelayTimings[IoOperation.close] = new IoFileMgrForUser.IoOperationTiming();
			noDelayTimings[IoOperation.seek] = new IoFileMgrForUser.IoOperationTiming();
			noDelayTimings[IoOperation.ioctl] = new IoFileMgrForUser.IoOperationTiming();
			noDelayTimings[IoOperation.remove] = new IoFileMgrForUser.IoOperationTiming();
			noDelayTimings[IoOperation.rename] = new IoFileMgrForUser.IoOperationTiming();
			noDelayTimings[IoOperation.mkdir] = new IoFileMgrForUser.IoOperationTiming();
			noDelayTimings[IoOperation.dread] = new IoFileMgrForUser.IoOperationTiming();
			noDelayTimings[IoOperation.iodevctl] = new IoFileMgrForUser.IoOperationTiming();
			noDelayTimings[IoOperation.read] = new IoFileMgrForUser.IoOperationTiming();
			noDelayTimings[IoOperation.write] = new IoFileMgrForUser.IoOperationTiming();

			base.start();
		}

		public virtual string Host0Path
		{
			set
			{
				host0Path = value;
			}
		}

		public virtual bool AllowExtractPGDStatus
		{
			set
			{
				allowExtractPGD = value;
			}
			get
			{
				return allowExtractPGD;
			}
		}


		public virtual IoInfo getFileIoInfo(int id)
		{
			return fileIds[id];
		}

		private static int NewUid
		{
			get
			{
				return SceUidManager.getNewUid(idPurpose);
			}
		}

		private static void releaseUid(int uid)
		{
			SceUidManager.releaseUid(uid, idPurpose);
		}

		private static int NewId
		{
			get
			{
				return SceUidManager.getNewId(idPurpose, MIN_ID, MAX_ID);
			}
		}

		private static void releaseId(int id)
		{
			SceUidManager.releaseId(id, idPurpose);
		}

		/// <summary>
		/// Resolve and remove the "/.." in file names.
		/// E.g.:
		///   disc0:/PSP_GAME/USRDIR/A/../B
		/// transformed into
		///   disc0:/PSP_GAME/USRDIR/B
		/// </summary>
		/// <param name="fileName">    File name, possibly containing "/.." </param>
		/// <returns>            File name without "/.." </returns>
		private string removeDotDotInFilename(string fileName)
		{
			while (true)
			{
				int dotDotIndex = fileName.IndexOf("/..", StringComparison.Ordinal);
				if (dotDotIndex < 0)
				{
					break;
				}
				int parentIndex = fileName.Substring(0, dotDotIndex).LastIndexOf("/", StringComparison.Ordinal);
				if (parentIndex < 0)
				{
					break;
				}
				fileName = fileName.Substring(0, parentIndex) + fileName.Substring(dotDotIndex + 3);
			}

			return fileName;
		}

		private string getAbsoluteFileName(string fileName)
		{
			if (string.ReferenceEquals(filepath, null) || fileName.Contains(":"))
			{
				return fileName;
			}

			string absoluteFileName = filepath;
			if (!absoluteFileName.EndsWith("/", StringComparison.Ordinal) && !fileName.StartsWith("/", StringComparison.Ordinal))
			{
				absoluteFileName += "/";
			}
			absoluteFileName += fileName;
			absoluteFileName = absoluteFileName.replaceFirst("^disc0/", "disc0:");
			absoluteFileName = absoluteFileName.replaceFirst("^" + Settings.Instance.getDirectoryMapping("ms0"), "ms0:");

			return absoluteFileName;
		}

		/*
		 *  Local file handling functions.
		 */
		public virtual string getDeviceFilePath(string pspfilename)
		{
			pspfilename = pspfilename.replaceAll("\\\\", "/");
			string device = null;
			string cwd = "";
			string filename = null;

			if (pspfilename.StartsWith("flash0:", StringComparison.Ordinal))
			{
				if (pspfilename.StartsWith("flash0:/", StringComparison.Ordinal))
				{
					return pspfilename.Replace("flash0:/", Settings.Instance.getDirectoryMapping("flash0"));
				}
				return pspfilename.Replace("flash0:", Settings.Instance.getDirectoryMapping("flash0"));
			}

			if (pspfilename.StartsWith("exdata0:", StringComparison.Ordinal))
			{
				if (pspfilename.StartsWith("exdata0:/", StringComparison.Ordinal))
				{
					return pspfilename.Replace("exdata0:/", Settings.Instance.getDirectoryMapping("exdata0"));
				}
				return pspfilename.Replace("exdata0:", Settings.Instance.getDirectoryMapping("exdata0"));
			}

			if (!string.ReferenceEquals(host0Path, null) && pspfilename.StartsWith("host0:", StringComparison.Ordinal) && !pspfilename.StartsWith("host0:/", StringComparison.Ordinal))
			{
				pspfilename = pspfilename.Replace("host0:", host0Path);
				pspfilename = removeDotDotInFilename(pspfilename);
			}

			if (string.ReferenceEquals(filepath, null))
			{
				return pspfilename;
			}

			int findcolon = pspfilename.IndexOf(":", StringComparison.Ordinal);
			if (findcolon != -1)
			{
				device = pspfilename.Substring(0, findcolon);
				pspfilename = pspfilename.Substring(findcolon + 1);
			}
			else
			{
				int findslash = filepath.IndexOf("/", StringComparison.Ordinal);
				if (findslash != -1)
				{
					device = filepath.Substring(0, findslash);
					cwd = filepath.Substring(findslash + 1);

					if (cwd.StartsWith("/", StringComparison.Ordinal))
					{
						cwd = cwd.Substring(1);
					}
					if (cwd.EndsWith("/", StringComparison.Ordinal))
					{
						cwd = cwd.Substring(0, cwd.Length - 1);
					}
				}
				else
				{
					device = filepath;
				}
			}

			// Map assigned devices, e.g.
			// Fire Up:
			//     sceIoAssign alias=0x0898EFC0('pfat0:'), physicalDev=0x0898F000('msstor0p1:/'), filesystemDev=0x0898F00C('fatms0:'), mode=0x0, arg_addr=0x0, argSize=0x0
			//     sceIoOpen filename='pfat0:PSP/SAVEDATA/PPCD00001DLS001/DATA2.BIN'
			if (assignedDevices != null && assignedDevices.ContainsKey(device))
			{
				device = assignedDevices[device];
			}

			// remap host0
			// - Bliss Island - ULES00616
			if (device.Equals("host0"))
			{
				if (iso != null)
				{
					device = "disc0";
				}
				else
				{
					device = "ms0";
				}
			}

			// remap fatms0
			// - Wipeout Pure - UCUS98612
			if (device.Equals("fatms0"))
			{
				device = "ms0";
			}

			// Ignore the filename in "umd0:xxx".
			// Using umd0: is always opening the whole UMD in sector block mode,
			// ignoring the file name specified after the colon.
			if (device.StartsWith("umd", StringComparison.Ordinal))
			{
				pspfilename = "";
			}

			// strip leading and trailing slash from supplied path
			// this step is common to absolute and relative paths
			if (pspfilename.StartsWith("/", StringComparison.Ordinal))
			{
				pspfilename = pspfilename.Substring(1);
			}
			if (pspfilename.EndsWith("/", StringComparison.Ordinal))
			{
				pspfilename = pspfilename.Substring(0, pspfilename.Length - 1);
			}
			// assemble final path
			// convert device to lower case here for case sensitive file systems (linux) and also for isUmdPath and trimUmdPrefix regex
			// - GTA: LCS uses upper case device DISC0
			// - The Fast and the Furious uses upper case device DISC0
			filename = Settings.Instance.getDirectoryMapping(device.ToLower());
			if (string.ReferenceEquals(filename, null))
			{
				filename = device.ToLower();
			}
			if (cwd.Length > 0)
			{
				filename += "/" + cwd;
			}
			if (pspfilename.Length > 0)
			{
				filename += "/" + pspfilename;
			}
			return filename;
		}

		private static readonly string[] umdPrefixes = new string[] {"disc[0-9]+", "umd[0-9]+", "umd", "isofs"};

		private bool isUmdPath(string deviceFilePath)
		{
			foreach (string umdPrefix in umdPrefixes)
			{
				if (deviceFilePath.matches(umdPrefix))
				{
					return true;
				}
				else if (deviceFilePath.matches(umdPrefix + "/.*"))
				{
					return true;
				}
			}

			return false;
		}

		private string trimUmdPrefix(string pcfilename)
		{
			// Assume the device name is always lower case (ensured by getDeviceFilePath)
			// Handle case where file path is blank so there is no slash after the device name
			foreach (string umdPrefix in umdPrefixes)
			{
				if (pcfilename.matches(umdPrefix))
				{
					return "";
				}
				else if (pcfilename.matches(umdPrefix + "/.*"))
				{
					return pcfilename.Substring(pcfilename.IndexOf("/", StringComparison.Ordinal) + 1);
				}
			}

			return pcfilename;
		}

		public virtual void mkdirs(string dir)
		{
			string pcfilename = getDeviceFilePath(dir);
			if (!string.ReferenceEquals(pcfilename, null))
			{
				File f = new File(pcfilename);
				f.mkdirs();
			}
		}

		private bool rmdir(File f, bool recursive)
		{
			bool subDirResult = true;
			if (recursive && f.Directory)
			{
				File[] subFiles = f.listFiles();
				for (int i = 0; subFiles != null && i < subFiles.Length; i++)
				{
					if (!rmdir(subFiles[i], recursive))
					{
						subDirResult = false;
					}
				}
			}

			return f.delete() && subDirResult;
		}

		public virtual bool rmdir(string dir, bool recursive)
		{
			string pcfilename = getDeviceFilePath(dir);
			if (string.ReferenceEquals(pcfilename, null))
			{
				return false;
			}

			File f = new File(pcfilename);
			return rmdir(f, recursive);
		}

		public virtual bool deleteFile(string pspfilename)
		{
			string pcfilename = getDeviceFilePath(pspfilename);
			if (string.ReferenceEquals(pcfilename, null))
			{
				return false;
			}

			string absoluteFileName = getAbsoluteFileName(pspfilename);
			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = vfsManager.getVirtualFileSystem(absoluteFileName, localFileName);
			bool fileDeleted;
			if (vfs != null)
			{
				int result = vfs.ioRemove(localFileName.ToString());
				fileDeleted = result >= 0;
			}
			else
			{
				File f = new File(pcfilename);
				fileDeleted = f.delete();
			}

			return fileDeleted;
		}

		public virtual string[] listFiles(string dir, string pattern)
		{
			string pcfilename = getDeviceFilePath(dir);
			if (string.ReferenceEquals(pcfilename, null))
			{
				return null;
			}
			File f = new File(pcfilename);
			return string.ReferenceEquals(pattern, null) ? f.list() : f.list(new PatternFilter(pattern));
		}

		public virtual SceIoStat statFile(string pspfilename)
		{
			string pcfilename = getDeviceFilePath(pspfilename);
			if (string.ReferenceEquals(pcfilename, null))
			{
				return null;
			}
			SceIoStat stat = null;
			string absoluteFileName = getAbsoluteFileName(pspfilename);
			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = vfsManager.getVirtualFileSystem(absoluteFileName, localFileName);
			if (vfs != null)
			{
				stat = new SceIoStat();
				int result = vfs.ioGetstat(localFileName.ToString(), stat);
				if (result < 0)
				{
					stat = null;
				}
			}
			else
			{
				stat = this.stat(pcfilename);
			}

			return stat;
		}

		/// <param name="pcfilename"> can be null for convenience
		/// @returns null on error </param>
		private SceIoStat stat(string pcfilename)
		{
			SceIoStat stat = null;
			if (!string.ReferenceEquals(pcfilename, null))
			{
				if (isUmdPath(pcfilename))
				{
					// check umd is mounted
					if (iso == null)
					{
						log.error("stat - no umd mounted");
						Emulator.Processor.cpu._v0 = ERROR_ERRNO_DEVICE_NOT_FOUND;
					// check umd is activated
					}
					else if (!Modules.sceUmdUserModule.UmdActivated)
					{
						log.warn("stat - umd mounted but not activated");
						Emulator.Processor.cpu._v0 = ERROR_KERNEL_NO_SUCH_DEVICE;
					}
					else
					{
						string isofilename = trimUmdPrefix(pcfilename);
						int mode = 4; // 4 = readable
						int attr = 0;
						long size = 0;
						long timestamp = 0;
						int startSector = 0;
						try
						{
							// Check for files first.
							UmdIsoFile file = iso.getFile(isofilename);
							attr = 0x20;
							size = file.length();
							timestamp = file.Timestamp.Ticks;
							startSector = file.StartSector;
							// Octal extend into user and group
							mode = mode + mode * 8 + mode * 64;
							// Copy attr into mode
							mode |= attr << 8;
							stat = new SceIoStat(mode, attr, size, ScePspDateTime.fromUnixTime(timestamp), ScePspDateTime.fromUnixTime(0), ScePspDateTime.fromUnixTime(timestamp));
							if (startSector > 0)
							{
								stat.StartSector = startSector;
							}
						}
						catch (FileNotFoundException)
						{
							// If file wasn't found, try looking for a directory.
							try
							{
								if (iso.isDirectory(isofilename))
								{
									attr |= 0x10;
									mode |= 1; // 1 = executable
								}
								// Octal extend into user and group
								mode = mode + mode * 8 + mode * 64;
								// Copy attr into mode
								mode |= attr << 8;
								stat = new SceIoStat(mode, attr, size, ScePspDateTime.fromUnixTime(timestamp), ScePspDateTime.fromUnixTime(0), ScePspDateTime.fromUnixTime(timestamp));
								if (startSector > 0)
								{
									stat.StartSector = startSector;
								}
							}
							catch (FileNotFoundException)
							{
								log.warn("stat - '" + isofilename + "' umd file/dir not found");
							}
							catch (IOException e)
							{
								log.warn("stat - umd io error: " + e.Message);
							}
						}
						catch (IOException e)
						{
							log.warn("stat - umd io error: " + e.Message);
						}
					}
				}
				else
				{
					File file = new File(pcfilename);
					if (file.exists())
					{
						int mode = (file.canRead() ? 4 : 0) + (file.canWrite() ? 2 : 0) + (file.canExecute() ? 1 : 0);
						int attr = 0;
						long size = file.length();
						long mtime = file.lastModified();
						// Octal extend into user and group
						mode = mode + mode * 8 + mode * 64;
						// Set attr (dir/file) and copy into mode
						if (file.Directory)
						{
							attr |= 0x10;
						}
						if (file.File)
						{
							attr |= 0x20;
						}
						mode |= attr << 8;
						// Java can't see file create/access time
						stat = new SceIoStat(mode, attr, size, ScePspDateTime.fromUnixTime(mtime), ScePspDateTime.fromUnixTime(0), ScePspDateTime.fromUnixTime(mtime));
					}
				}
			}
			return stat;
		}

		public virtual IVirtualFile getVirtualFile(string filename, int flags, int permissions)
		{
			string absoluteFileName = getAbsoluteFileName(filename);
			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = vfsManager.getVirtualFileSystem(absoluteFileName, localFileName);
			if (vfs != null)
			{
				return vfs.ioOpen(localFileName.ToString(), flags, permissions);
			}

			return null;
		}

		public virtual SeekableDataInput getFile(string filename, int flags)
		{
			SeekableDataInput resultFile = null;
			string pcfilename = getDeviceFilePath(filename);
			if (!string.ReferenceEquals(pcfilename, null))
			{
				if (isUmdPath(pcfilename))
				{
					// check umd is mounted
					if (iso == null)
					{
						log.error("getFile - no umd mounted");
						return resultFile;
					// check flags are valid
					}
					else if ((flags & PSP_O_WRONLY) == PSP_O_WRONLY || (flags & PSP_O_CREAT) == PSP_O_CREAT || (flags & PSP_O_TRUNC) == PSP_O_TRUNC)
					{
						log.error("getFile - refusing to open umd media for write");
						return resultFile;
					}
					else
					{
						// open file
						try
						{
							UmdIsoFile file = iso.getFile(trimUmdPrefix(pcfilename));
							resultFile = file;
						}
						catch (FileNotFoundException)
						{
							if (log.DebugEnabled)
							{
								log.debug("getFile - umd file not found '" + pcfilename + "' (ok to ignore this message, debug purpose only)");
							}
						}
						catch (IOException e)
						{
							log.error("getFile - error opening umd media: " + e.Message);
						}
					}
				}
				else
				{
					// First check if the file already exists
					File file = new File(pcfilename);
					if (file.exists() && (flags & PSP_O_CREAT) == PSP_O_CREAT && (flags & PSP_O_EXCL) == PSP_O_EXCL)
					{
						if (log.DebugEnabled)
						{
							log.debug("getFile - file already exists (PSP_O_CREAT + PSP_O_EXCL)");
						}
					}
					else
					{
						if (file.exists() && (flags & PSP_O_TRUNC) == PSP_O_TRUNC)
						{
							log.warn("getFile - file already exists, deleting UNIMPLEMENT (PSP_O_TRUNC)");
						}
						string mode = getMode(flags);

						try
						{
							resultFile = new SeekableRandomFile(pcfilename, mode);
						}
						catch (FileNotFoundException)
						{
							if (log.DebugEnabled)
							{
								log.debug("getFile - file not found '" + pcfilename + "' (ok to ignore this message, debug purpose only)");
							}
						}
					}
				}
			}

			return resultFile;
		}

		public virtual SeekableDataInput getFile(int id)
		{
			IoInfo info = fileIds[id];
			if (info == null)
			{
				return null;
			}
			return info.readOnlyFile;
		}

		public virtual string getFileFilename(int id)
		{
			IoInfo info = fileIds[id];
			if (info == null)
			{
				return null;
			}
			return info.filename;
		}

		private string getMode(int flags)
		{
			return modeStrings[flags & PSP_O_RDWR];
		}

		private long updateResult(IoInfo info, long result, bool async, bool resultIs64bit, IoOperationTiming ioOperationTiming)
		{
			return updateResult(info, result, async, resultIs64bit, ioOperationTiming, null, 0);
		}

		// Handle returning/storing result for sync/async operations
		private long updateResult(IoInfo info, long result, bool async, bool resultIs64bit, IoOperationTiming ioOperationTiming, IAction asyncAction, int size)
		{
			// No async IO is started when returning error code ERROR_KERNEL_ASYNC_BUSY
			if (info != null && result != ERROR_KERNEL_ASYNC_BUSY)
			{
				if (async)
				{
					if (!info.asyncPending)
					{
						result = startIoAsync(info, result, ioOperationTiming, asyncAction, size);
					}
				}
				else
				{
					info.result = ERROR_KERNEL_NO_ASYNC_OP;
				}
			}

			return result;
		}

		public static string getWhenceName(int whence)
		{
			switch (whence)
			{
				case PSP_SEEK_SET:
					return "PSP_SEEK_SET";
				case PSP_SEEK_CUR:
					return "PSP_SEEK_CUR";
				case PSP_SEEK_END:
					return "PSP_SEEK_END";
				default:
					return "UNHANDLED " + whence;
			}
		}

		public virtual void setfilepath(string filepath)
		{
			filepath = filepath.replaceAll("\\\\", "/");
			if (log.DebugEnabled)
			{
				log.debug(string.Format("filepath set to '{0}'", filepath));
			}
			this.filepath = filepath;
		}

		public virtual void exit()
		{
			closeIsoReader();
		}

		private void closeIsoReader()
		{
			if (iso != null)
			{
				try
				{
					iso.close();
				}
				catch (IOException e)
				{
					log.error("Error closing ISO reader", e);
				}
				iso = null;
			}
		}

		public virtual UmdIsoReader IsoReader
		{
			set
			{
				closeIsoReader();
    
				this.iso = value;
    
				registerUmdIso();
			}
			get
			{
				return iso;
			}
		}


		private void delayIoOpertation(int delayMillis)
		{
			if (!noDelayIoOperation && delayMillis > 0)
			{
				Modules.ThreadManForUserModule.hleKernelDelayThread(delayMillis * 1000, false);
			}
		}

		protected internal virtual void delayIoOperation(IoOperationTiming ioOperationTiming)
		{
			delayIoOpertation(ioOperationTiming.DelayMillis);
		}

		protected internal virtual void delayIoOperation(IoOperationTiming ioOperationTiming, int size)
		{
			delayIoOpertation(ioOperationTiming.getDelayMillis(size));
		}

		public virtual void hleSetNoDelayIoOperation(bool noDelayIoOperation)
		{
			this.noDelayIoOperation = noDelayIoOperation;
		}

		/*
		 * Async thread functions.
		 */
		public virtual void hleAsyncThread(Processor processor)
		{
			CpuState cpu = processor.cpu;
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;

			int uid = cpu.getRegister(asyncThreadRegisterArgument);

			IoInfo info = fileUids[uid];
			if (info == null)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleAsyncThread non-existing uid={0:x}", uid));
				}
				cpu._v0 = 0; // Exit status
				// Exit and delete the thread to free its resources (e.g. its stack)
				threadMan.hleKernelExitDeleteThread();
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleAsyncThread id={0:x}", info.id));
				}
				bool asyncCompleted = doStepAsync(info);
				if (threadMan.CurrentThread == info.asyncThread)
				{
					if (asyncCompleted)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Async IO completed"));
						}
						// Wait for a new Async IO... wakeup is done by triggerAsyncThread()
						threadMan.hleKernelSleepThread(false);
					}
					else
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Async IO not yet completed"));
						}
						// Wait for the Async IO to complete...
						threadMan.hleKernelDelayThread(info.AsyncRestMillis * 1000, false);
					}
				}
			}
		}

		/// <summary>
		/// Trigger the activation of the async thread if one is defined.
		/// </summary>
		/// <param name="info"> the file info </param>
		private void triggerAsyncThread(IoInfo info)
		{
			if (info.asyncThread != null)
			{
				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				threadMan.hleKernelWakeupThread(info.asyncThread);
			}
		}

		/// <summary>
		/// Start the async IO thread if not yet started.
		/// </summary>
		/// <param name="info">   the file </param>
		/// <param name="result"> the result the async IO should return </param>
		private int startIoAsync(IoInfo info, long result, IoOperationTiming ioOperationTiming, IAction asyncAction, int size)
		{
			int startResult = 0;
			if (info == null)
			{
				return startResult;
			}
			info.asyncPending = true;
			info.asyncResultPending = false;
			long now = Emulator.Clock.currentTimeMillis();
			info.asyncDoneMillis = now + ioOperationTiming.getDelayMillis(size);
			info.asyncAction = asyncAction;
			info.result = result;
			if (info.asyncThread == null)
			{
				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				// Inherit priority from current thread if no default priority set
				int asyncPriority = info.asyncThreadPriority;
				if (asyncPriority < 0)
				{
					// Take the priority of the thread executing the first async operation.
					asyncPriority = threadMan.CurrentThread.currentPriority;
				}

				int stackSize = 0x2000;
				// On FW 1.50, the stack size for the async thread is 0x2000,
				// on FW 5.00, the stack size is 0x800.
				// When did it change?
				if (Emulator.Instance.FirmwareVersion > 150)
				{
					stackSize = 0x800;
				}

				// The stack of the async thread is always allocated in the kernel partition
				info.asyncThread = threadMan.hleKernelCreateThread("SceIofileAsync", ThreadManForUser.ASYNC_LOOP_ADDRESS, asyncPriority, stackSize, threadMan.CurrentThread.attr, 0, SysMemUserForUser.KERNEL_PARTITION_ID);

				if (info.asyncThread.StackAddr == 0)
				{
					log.warn(string.Format("Cannot start the Async IO thread, not enough memory to create its stack"));
					threadMan.hleDeleteThread(info.asyncThread);
					info.asyncThread = null;
					startResult = SceKernelErrors.ERROR_KERNEL_NO_MEMORY;
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Starting Async IO thread {0}", info.asyncThread));
					}
					// This must be the last action of the hleIoXXX call because it can context-switch
					// Inherit $gp from this process ($gp can be used by interrupts)
					threadMan.hleKernelStartThread(info.asyncThread, 0, 0, info.asyncThread.gpReg_addr);

					// Copy uid to Async Thread argument register after starting the thread
					// (all registers are reset when starting the thread).
					info.asyncThread.cpuContext.setRegister(asyncThreadRegisterArgument, info.uid);
				}
			}
			else
			{
				triggerAsyncThread(info);
			}

			return startResult;
		}

		private bool doStepAsync(IoInfo info)
		{
			bool done = true;

			if (info.asyncPending)
			{
				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				if (info.AsyncRestMillis > 0)
				{
					done = false;
				}
				else
				{
					// Execute any pending async action and remove it.
					// Execute the action only when the async operation can be completed
					// as its execution can be time consuming (e.g. code block cache invalidation).
					if (info.asyncAction != null)
					{
						IAction asyncAction = info.asyncAction;
						info.asyncAction = null;
						asyncAction.execute();
					}

					info.asyncPending = false;
					info.asyncResultPending = true;
					if (info.cbid >= 0)
					{
						// Trigger Async callback.
						threadMan.hleKernelNotifyCallback(SceKernelThreadInfo.THREAD_CALLBACK_IO, info.cbid, info.notifyArg);
					}
					// Find threads waiting on this id and wake them up.
					for (IEnumerator<SceKernelThreadInfo> it = threadMan.GetEnumerator(); it.MoveNext();)
					{
						SceKernelThreadInfo thread = it.Current;
						if (thread.waitType == JPCSP_WAIT_IO && thread.wait.Io_id == info.id)
						{
							if (log.DebugEnabled)
							{
								log.debug("IoFileMgrForUser.doStepAsync - onContextSwitch waking " + thread.uid.ToString("x") + " thread:'" + thread.name + "'");
							}

							// Write result
							Memory mem = Memory.Instance;
							if (Memory.isAddressGood(thread.wait.Io_resultAddr))
							{
								if (log.DebugEnabled)
								{
									log.debug(string.Format("IoFileMgrForUser.doStepAsync - storing result 0x{0:X}", info.result));
								}
								mem.write64(thread.wait.Io_resultAddr, info.result);
							}

							// Return error at next call to sceIoWaitAsync
							info.result = ERROR_KERNEL_NO_ASYNC_OP;
							info.asyncResultPending = false;

							// Return success
							thread.cpuContext._v0 = 0;
							// Wakeup
							threadMan.hleChangeThreadState(thread, PSP_THREAD_READY);
						}
					}
				}
			}
			return done;
		}

		public virtual void unregisterIoListener(IIoListener ioListener)
		{
			if (ioListeners != null)
			{
				for (int i = 0; i < ioListeners.Length; i++)
				{
					if (ioListeners[i] == ioListener)
					{
						IIoListener[] newIoListeners = new IIoListener[ioListeners.Length - 1];
						Array.Copy(ioListeners, 0, newIoListeners, 0, i);
						Array.Copy(ioListeners, i + 1, newIoListeners, i, ioListeners.Length - i - 1);
						ioListeners = newIoListeners;
						break;
					}
				}
			}
		}

		public virtual void registerIoListener(IIoListener ioListener)
		{
			if (ioListeners == null)
			{
				ioListeners = new IIoListener[1];
				ioListeners[0] = ioListener;
			}
			else
			{
				for (int i = 0; i < ioListeners.Length; i++)
				{
					if (ioListeners[i] == ioListener)
					{
						// The listener is already registered
						return;
					}
				}

				IIoListener[] newIoListeners = new IIoListener[ioListeners.Length + 1];
				Array.Copy(ioListeners, 0, newIoListeners, 0, ioListeners.Length);
				newIoListeners[ioListeners.Length] = ioListener;
				ioListeners = newIoListeners;
			}
		}

		private IoInfo getInfo(IVirtualFile vFile)
		{
			foreach (IoInfo info in fileIds.Values)
			{
				if (info.vFile == vFile)
				{
					return info;
				}
			}

			return null;
		}

		public virtual long getPosition(IVirtualFile vFile)
		{
			IoInfo info = getInfo(vFile);
			if (info == null)
			{
				return -1;
			}

			return info.position;
		}

		public virtual void setPosition(IVirtualFile vFile, long position)
		{
			IoInfo info = getInfo(vFile);
			if (info != null)
			{
				info.position = position;
			}
		}

		public virtual VirtualFileSystemManager VirtualFileSystemManager
		{
			get
			{
				return vfsManager;
			}
		}

		public virtual IVirtualFileSystem getVirtualFileSystem(string pspfilename, StringBuilder localFileName)
		{
			bool umdRegistered = false;
			bool msRegistered = false;

			// This call wants to use the VFS.
			// If the UMD has not been registered, register it just for this call
			if (!useVirtualFileSystem)
			{
				if (iso != null && Modules.sceUmdUserModule.UmdActivated)
				{
					IVirtualFileSystem vfsIso = new UmdIsoVirtualFileSystem(iso);
					vfsManager.register("disc0", vfsIso);
					vfsManager.register("umd0", vfsIso);
					vfsManager.register("umd1", vfsIso);
					vfsManager.register("umd", vfsIso);
					vfsManager.register("isofs", vfsIso);
					umdRegistered = true;
				}

				registerVfsMs0();
				msRegistered = true;
			}

			string absoluteFileName = getAbsoluteFileName(pspfilename);
			IVirtualFileSystem vfs = vfsManager.getVirtualFileSystem(absoluteFileName, localFileName);

			if (umdRegistered)
			{
				vfsManager.unregister("disc0");
				vfsManager.unregister("umd0");
				vfsManager.unregister("umd1");
				vfsManager.unregister("umd");
				vfsManager.unregister("isofs");
			}
			if (msRegistered)
			{
				vfsManager.unregister("ms0");
				vfsManager.unregister("fatms0");
				vfsManager.unregister("flash0");
				vfsManager.unregister("flash1");
				vfsManager.unregister("exdata0");
				vfsManager.unregister("mscmhc0");
				vfsManager.unregister("msstor0");
				vfsManager.unregister("msstor0");
			}

			return vfs;
		}

		/*
		 * HLE functions.
		 */
		public virtual int hleIoWaitAsync(int id, TPointer64 resAddr, bool wait, bool callbacks)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("hleIoWaitAsync id=0x%X, res=%s, wait=%b, callbacks=%b", id, resAddr, wait, callbacks));
				log.debug(string.Format("hleIoWaitAsync id=0x%X, res=%s, wait=%b, callbacks=%b", id, resAddr, wait, callbacks));
			}

			IoInfo info = fileIds[id];

			if (info == null)
			{
				if (id == 0)
				{
					// Avoid WARN spam messages
					if (log.DebugEnabled)
					{
						log.debug(string.Format("hleIoWaitAsync - unknown id 0x{0:X}", id));
					}
				}
				else
				{
					log.warn(string.Format("hleIoWaitAsync - unknown id 0x{0:X}", id));
				}
				return ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
			}

			if (info.result == ERROR_KERNEL_NO_ASYNC_OP || info.asyncThread == null)
			{
				log.debug("hleIoWaitAsync - PSP_ERROR_NO_ASYNC_OP");
				return ERROR_KERNEL_NO_ASYNC_OP;
			}

			if (info.asyncPending && !wait)
			{
				// Polling returns 1 when async is busy.
				log.debug("hleIoWaitAsync - poll return = 1(busy)");
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleIoWaitAsync info.result=0x{0:X}", info.result));
				}
				return 1;
			}

			bool waitForAsync = false;

			// Check for the waiting condition first.
			if (wait)
			{
				waitForAsync = true;
			}

			if (!info.asyncPending)
			{
				log.debug("hleIoWaitAsync - async already completed, not waiting");
				waitForAsync = false;
			}

			// The file was marked as closePending, so close it right away to avoid delays.
			if (info.closePending)
			{
				log.debug("hleIoWaitAsync - file marked with closePending, calling hleIoClose, not waiting");
				info.asyncPending = false;
				info.asyncResultPending = false;
				hleIoClose(info.id, false);
				waitForAsync = false;
			}

			// The file was not found at sceIoOpenAsync.
			if (info.result == ERROR_ERRNO_FILE_NOT_FOUND)
			{
				log.debug("hleIoWaitAsync - file not found, not waiting");
				info.close();
				triggerAsyncThread(info);
				waitForAsync = false;
			}

			if (waitForAsync)
			{
				// Call the ioListeners.
				foreach (IIoListener ioListener in ioListeners)
				{
					ioListener.sceIoWaitAsync(0, id, resAddr.Address);
				}
				// Start the waiting mode.
				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				SceKernelThreadInfo currentThread = threadMan.CurrentThread;
				currentThread.wait.Io_id = info.id;
				currentThread.wait.Io_resultAddr = resAddr.Address;
				threadMan.hleKernelThreadEnterWaitState(JPCSP_WAIT_IO, info.id, ioWaitStateChecker, callbacks);
			}
			else
			{
				// Store the result
				if (resAddr.NotNull)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("hleIoWaitAsync - storing result 0x{0:X}", info.result));
					}
					resAddr.Value = info.result;
				}

				// Async result can only be retrieved once
				info.asyncResultPending = false;
				info.result = ERROR_KERNEL_NO_ASYNC_OP;

				// For sceIoPollAsync, only call the ioListeners.
				foreach (IIoListener ioListener in ioListeners)
				{
					ioListener.sceIoPollAsync(0, id, resAddr.Address);
				}
			}

			return 0;
		}

		public virtual int hleIoOpen(PspString filename, int flags, int permissions, bool async)
		{
			return hleIoOpen(filename.Address, filename.String, flags, permissions, async);
		}

		public virtual int hleIoOpen(int filename_addr, string filename, int flags, int permissions, bool async)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;

			if (log.InfoEnabled)
			{
				log.info("hleIoOpen filename = " + filename + " flags = " + flags.ToString("x") + " permissions = 0" + Integer.toOctalString(permissions));
			}
			if (log.DebugEnabled)
			{
				if ((flags & PSP_O_RDONLY) == PSP_O_RDONLY)
				{
					log.debug("PSP_O_RDONLY");
				}
				if ((flags & PSP_O_WRONLY) == PSP_O_WRONLY)
				{
					log.debug("PSP_O_WRONLY");
				}
				if ((flags & PSP_O_NBLOCK) == PSP_O_NBLOCK)
				{
					log.debug("PSP_O_NBLOCK");
				}
				if ((flags & PSP_O_DIROPEN) == PSP_O_DIROPEN)
				{
					log.debug("PSP_O_DIROPEN");
				}
				if ((flags & PSP_O_APPEND) == PSP_O_APPEND)
				{
					log.debug("PSP_O_APPEND");
				}
				if ((flags & PSP_O_CREAT) == PSP_O_CREAT)
				{
					log.debug("PSP_O_CREAT");
				}
				if ((flags & PSP_O_TRUNC) == PSP_O_TRUNC)
				{
					log.debug("PSP_O_TRUNC");
				}
				if ((flags & PSP_O_EXCL) == PSP_O_EXCL)
				{
					log.debug("PSP_O_EXCL");
				}
				if ((flags & PSP_O_NBUF) == PSP_O_NBUF)
				{
					log.debug("PSP_O_NBUF");
				}
				if ((flags & PSP_O_NOWAIT) == PSP_O_NOWAIT)
				{
					log.debug("PSP_O_NOWAIT");
				}
				if ((flags & PSP_O_PLOCK) == PSP_O_PLOCK)
				{
					log.debug("PSP_O_PLOCK");
				}
				if ((flags & PSP_O_FGAMEDATA) == PSP_O_FGAMEDATA)
				{
					log.debug("PSP_O_FGAMEDATA");
				}
			}
			string mode = getMode(flags);
			if (string.ReferenceEquals(mode, null))
			{
				log.error("hleIoOpen - unhandled flags " + flags.ToString("x"));
				foreach (IIoListener ioListener in ioListeners)
				{
					ioListener.sceIoOpen(-1, filename_addr, filename, flags, permissions, mode);
				}
				return -1;
			}
			//Retry count.
			int retry = (flags >> 16) & 0x000F;

			if (retry != 0)
			{
				log.info("hleIoOpen - retry count is " + retry);
			}
			if ((flags & PSP_O_RDONLY) == PSP_O_RDONLY && (flags & PSP_O_APPEND) == PSP_O_APPEND)
			{
				log.warn("hleIoOpen - read and append flags both set!");
			}

			IoInfo info = null;
			int result;
			try
			{
				string pcfilename = getDeviceFilePath(filename);

				string absoluteFileName = getAbsoluteFileName(filename);
				StringBuilder localFileName = new StringBuilder();
				IVirtualFileSystem vfs = vfsManager.getVirtualFileSystem(absoluteFileName, localFileName);
				if (vfs != null)
				{
					timings = vfs.Timings;
					IVirtualFile vFile = vfs.ioOpen(localFileName.ToString(), flags, permissions);
					if (vFile == null)
					{
						result = ERROR_ERRNO_FILE_NOT_FOUND;
					}
					else
					{
						info = new IoInfo(this, filename, vFile, mode, flags, permissions);
						info.sectorBlockMode = vFile.SectorBlockMode;
						info.result = ERROR_KERNEL_NO_ASYNC_OP;
						result = info.id;
						if (log.DebugEnabled)
						{
							log.debug("hleIoOpen assigned id = 0x" + info.id.ToString("x"));
						}
					}
				}
				else if (useVirtualFileSystem)
				{
					log.error(string.Format("hleIoOpen - device not found '{0}'", filename));
					result = ERROR_ERRNO_DEVICE_NOT_FOUND;
				}
				else if (!string.ReferenceEquals(pcfilename, null))
				{
					if (log.DebugEnabled)
					{
						log.debug("hleIoOpen - opening file " + pcfilename);
					}
					if (isUmdPath(pcfilename))
					{
						// Check umd is mounted.
						if (iso == null)
						{
							log.error("hleIoOpen - no umd mounted");
							result = ERROR_ERRNO_DEVICE_NOT_FOUND;
						// Check umd is activated.
						}
						else if (!Modules.sceUmdUserModule.UmdActivated)
						{
							log.warn("hleIoOpen - umd mounted but not activated");
							result = ERROR_KERNEL_NO_SUCH_DEVICE;
						// Check flags are valid.
						}
						else if ((flags & PSP_O_WRONLY) == PSP_O_WRONLY || (flags & PSP_O_CREAT) == PSP_O_CREAT || (flags & PSP_O_TRUNC) == PSP_O_TRUNC)
						{
							log.error("hleIoOpen - refusing to open umd media for write");
							result = ERROR_ERRNO_READ_ONLY;
						}
						else
						{
							// Open file.
							try
							{
								string trimmedFileName = trimUmdPrefix(pcfilename);

								// Opening an empty file name with no current working directory set
								// should return ERROR_ERRNO_FILE_NOT_FOUND
								if (!string.ReferenceEquals(trimmedFileName, null) && trimmedFileName.Length == 0 && filename.Length == 0)
								{
									throw new FileNotFoundException(filename);
								}

								UmdIsoFile file = iso.getFile(trimmedFileName);
								info = new IoInfo(this, filename, file, mode, flags, permissions);
								if (!info.ValidId)
								{
									// Too many open files...
									log.warn(string.Format("hleIoOpen - too many open files"));
									result = ERROR_KERNEL_TOO_MANY_OPEN_FILES;
									// Return immediately the error, even in async mode
									async = false;
								}
								else
								{
									if (!string.ReferenceEquals(trimmedFileName, null) && trimmedFileName.Length == 0)
									{
										// Opening "umd0:" is allowing to read the whole UMD per sectors.
										info.sectorBlockMode = true;
									}
									info.result = ERROR_KERNEL_NO_ASYNC_OP;
									result = info.id;
									if (log.DebugEnabled)
									{
										log.debug("hleIoOpen assigned id = 0x" + info.id.ToString("x"));
									}
								}
							}
							catch (FileNotFoundException)
							{
								if (log.DebugEnabled)
								{
									log.debug("hleIoOpen - umd file not found (ok to ignore this message, debug purpose only)");
								}
								result = ERROR_ERRNO_FILE_NOT_FOUND;
							}
							catch (IOException e)
							{
								log.error("hleIoOpen - error opening umd media: " + e.Message);
								result = -1;
							}
						}
					}
					else
					{
						// First check if the file already exists
						File file = new File(pcfilename);
						if (file.exists() && (flags & PSP_O_CREAT) == PSP_O_CREAT && (flags & PSP_O_EXCL) == PSP_O_EXCL)
						{
							if (log.DebugEnabled)
							{
								log.debug("hleIoOpen - file already exists (PSP_O_CREAT + PSP_O_EXCL)");
							}
							result = ERROR_ERRNO_FILE_ALREADY_EXISTS;
						}
						else
						{
							// When PSP_O_CREAT is specified, create the parent directories
							// if they do not yet exist.
							if (!file.exists() && ((flags & PSP_O_CREAT) == PSP_O_CREAT))
							{
								string parentDir = System.IO.Path.GetDirectoryName(pcfilename);
								System.IO.Directory.CreateDirectory(parentDir);
							}

							SeekableRandomFile raf = new SeekableRandomFile(pcfilename, mode);
							info = new IoInfo(this, filename, raf, mode, flags, permissions);
							if ((flags & PSP_O_WRONLY) == PSP_O_WRONLY && (flags & PSP_O_TRUNC) == PSP_O_TRUNC)
							{
								// When writing, PSP_O_TRUNC truncates the file at the position of the first write.
								// E.g.:
								//    open(PSP_O_TRUNC)
								//    seek(0x1000)
								//    write()  -> truncates the file at the position 0x1000 before writing
								info.TruncateAtNextWrite = true;
							}
							info.result = ERROR_KERNEL_NO_ASYNC_OP; // sceIoOpenAsync will set this properly
							result = info.id;
							if (log.DebugEnabled)
							{
								log.debug("hleIoOpen assigned id = 0x" + info.id.ToString("x"));
							}
						}
					}
				}
				else
				{
					result = -1;
				}
			}
			catch (FileNotFoundException)
			{
				// To be expected under mode="r" and file doesn't exist
				if (log.DebugEnabled)
				{
					log.debug("hleIoOpen - file not found (ok to ignore this message, debug purpose only)");
				}
				result = ERROR_ERRNO_FILE_NOT_FOUND;
			}

			if (result == ERROR_ERRNO_FILE_NOT_FOUND && filepath.Equals("disc0/") && !filename.Contains(":") && !filename.Contains("/"))
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleIoOpen - no current directory set filepath='{0}', filename='{1}'", filepath, filename));
				}
				result = ERROR_KERNEL_NOCWD;
			}

			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoOpen(result, filename_addr, filename, flags, permissions, mode);
			}

			if (async)
			{
				int realResult = result;
				if (info == null)
				{
					log.debug("sceIoOpenAsync - file not found (ok to ignore this message, debug purpose only)");
					// For async we still need to make and return a file handle even if we couldn't open the file,
					// this is so the game can query on the handle (wait/async stat/io callback).
					info = new IoInfo(this, readStringZ(filename_addr), (SeekableDataInput) null, null, flags, permissions);
					result = info.id;
				}

				int startResult = startIoAsync(info, realResult, timings[IoOperation.open], null, 0);
				if (startResult < 0)
				{
					result = startResult;
				}
			}
			else
			{
				delayIoOperation(timings[IoOperation.open]);
			}

			return result;
		}

		private int hleIoClose(int id, bool async)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;
			int result;

			IoInfo info = fileIds[id];
			if (id == STDIN_ID || id == STDOUT_ID || id == STDERR_ID)
			{
				// Cannot close stdin, stdout, stderr
				result = SceKernelErrors.ERROR_KERNEL_ILLEGAL_PERMISSION;
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceIoClose id=0x{0:X} returning ERROR_KERNEL_ILLEGAL_PERMISSION(0x{1:X8})", id, result));
				}
			}
			else if (async)
			{
				if (info != null)
				{
					if (info.asyncPending || info.asyncResultPending)
					{
						result = ERROR_KERNEL_ASYNC_BUSY;
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceIoClose id=0x{0:X} returning ERROR_KERNEL_ASYNC_BUSY(0x{1:X8})", id, result));
						}
					}
					else
					{
						info.closePending = true;
						result = (int) updateResult(info, 0, true, false, timings[IoOperation.close]);
					}
				}
				else
				{
					result = ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceIoClose id=0x{0:X} returning ERROR_KERNEL_BAD_FILE_DESCRIPTOR(0x{1:X8})", id, result));
					}
				}
			}
			else
			{
				try
				{
					if (info == null)
					{
						result = ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceIoClose id=0x{0:X} returning ERROR_KERNEL_BAD_FILE_DESCRIPTOR(0x{1:X8})", id, result));
						}
					}
					else if (info.asyncPending || info.asyncResultPending)
					{
						// Cannot close while an async operation is running
						result = ERROR_KERNEL_ASYNC_BUSY;
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceIoClose id=0x{0:X} returning ERROR_KERNEL_ASYNC_BUSY(0x{1:X8})", id, result));
						}
					}
					else
					{
						if (info.vFile != null)
						{
							timings = info.vFile.Timings;
							info.vFile.ioClose();
						}
						else if (info.readOnlyFile != null)
						{
							// Can be just closing an empty handle, because hleIoOpen(async==true)
							// generates a dummy IoInfo when the file could not be opened.
							info.readOnlyFile.Dispose();
						}
						info.close();
						triggerAsyncThread(info);
						info.result = 0;
						result = 0;
					}
				}
				catch (IOException e)
				{
					log.error("pspiofilemgr - error closing file: " + e.Message);
					result = -1;
				}
				foreach (IIoListener ioListener in ioListeners)
				{
					ioListener.sceIoClose(result, id);
				}
			}

			if (!async)
			{
				delayIoOperation(timings[IoOperation.close]);
			}

			return result;
		}

		private int hleIoWrite(int id, TPointer dataAddr, int size, bool async)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;
			IoInfo info = null;
			int result;

			if (id == STDOUT_ID)
			{
				// stdout
				string message = Utilities.stripNL(readStringNZ(dataAddr.Address, size));
				stdout.info(message);
				if (stdRedirects[id] != null)
				{
					Managers.msgPipes.hleKernelSendMsgPipe(stdRedirects[id].uid, dataAddr, size, MsgPipeManager.PSP_MPP_WAIT_MODE_COMPLETE, TPointer32.NULL, TPointer32.NULL, false, false);
				}
				result = size;
			}
			else if (id == STDERR_ID)
			{
				// stderr
				string message = Utilities.stripNL(readStringNZ(dataAddr.Address, size));
				stderr.info(message);
				result = size;
			}
			else
			{
				if (log.DebugEnabled)
				{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("hleIoWrite(id=0x%X, data=%s, size=0x%X) async=%b", id, dataAddr, size, async));
					log.debug(string.Format("hleIoWrite(id=0x%X, data=%s, size=0x%X) async=%b", id, dataAddr, size, async));
					if (log.TraceEnabled)
					{
						log.trace(string.Format("hleIoWrite: {0}", Utilities.getMemoryDump(dataAddr.Address, System.Math.Min(size, 32))));
					}
				}
				try
				{
					info = fileIds[id];
					if (info == null)
					{
						log.warn("hleIoWrite - unknown id " + id.ToString("x"));
						result = ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
					}
					else if (info.asyncPending || info.asyncResultPending)
					{
						log.warn("hleIoWrite - id " + id.ToString("x") + " PSP_ERROR_ASYNC_BUSY");
						result = ERROR_KERNEL_ASYNC_BUSY;
					}
					else if ((dataAddr.Address < MemoryMap.START_RAM) && (dataAddr.Address + size > MemoryMap.END_RAM))
					{
						log.warn("hleIoWrite - id " + id.ToString("x") + " data is outside of ram 0x" + dataAddr.Address.ToString("x") + " - 0x" + (dataAddr.Address + size).ToString("x"));
						result = -1;
					}
					else if ((info.flags & PSP_O_RDWR) == PSP_O_RDONLY)
					{
						result = ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
					}
					else if (info.vFile != null)
					{
						timings = info.vFile.Timings;
						if ((info.flags & PSP_O_APPEND) == PSP_O_APPEND)
						{
							info.vFile.ioLseek(info.vFile.length());
							info.position = info.vFile.length();
						}

						if (info.position > info.vFile.length())
						{
							int towrite = (int)(info.position - info.vFile.length());

							info.vFile.ioLseek(info.vFile.length());
							while (towrite > 0)
							{
								result = info.vFile.ioWrite(dataAddr, 1);
								if (result < 0)
								{
									break;
								}
								towrite -= result;
							}
						}

						result = info.vFile.ioWrite(dataAddr, size);
						if (result > 0)
						{
							info.position += result;
						}
					}
					else
					{
						if ((info.flags & PSP_O_APPEND) == PSP_O_APPEND)
						{
							info.msFile.seek(info.msFile.length());
							info.position = info.msFile.length();
						}

						if (info.position > info.readOnlyFile.length())
						{
							sbyte[] junk = new sbyte[512];
							int towrite = (int)(info.position - info.readOnlyFile.length());

							info.msFile.seek(info.msFile.length());
							while (towrite >= 512)
							{
								info.msFile.write(junk, 0, 512);
								towrite -= 512;
							}
							if (towrite > 0)
							{
								info.msFile.write(junk, 0, towrite);
							}
						}

						if (info.TruncateAtNextWrite)
						{
							// The file was open with PSP_O_TRUNC: truncate the file at the first write
							if (info.position < info.readOnlyFile.length())
							{
								info.truncate((int) info.position);
							}
							info.TruncateAtNextWrite = false;
						}

						info.position += size;

						Utilities.write(info.msFile, dataAddr.Address, size);
						result = size;
					}
				}
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					result = -1;
				}
			}
			result = (int) updateResult(info, result, async, false, timings[IoOperation.write], null, size);
			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoWrite(result, id, dataAddr.Address, size, size);
			}

			if (!async)
			{
				// Do not delay output on stdout/stderr
				if (id != STDOUT_ID && id != STDERR_ID)
				{
					delayIoOperation(timings[IoOperation.write], size);
				}
			}

			return result;
		}

		public virtual int hleIoRead(int id, int data_addr, int size, bool async)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;
			IoInfo info = null;
			int result;
			long position = 0;
			SeekableDataInput dataInput = null;
			IVirtualFile vFile = null;
			int requestedSize = size;
			IAction asyncAction = null;

			if (id == STDIN_ID)
			{ // stdin
				log.warn("UNIMPLEMENTED:hleIoRead id = stdin");
				result = 0;
			}
			else
			{
				try
				{
					info = fileIds[id];
					if (info == null)
					{
						log.warn("hleIoRead - unknown id " + id.ToString("x"));
						result = ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
					}
					else if (info.asyncPending || info.asyncResultPending)
					{
						log.warn("hleIoRead - id " + id.ToString("x") + " PSP_ERROR_ASYNC_BUSY");
						result = ERROR_KERNEL_ASYNC_BUSY;
					}
					else if ((data_addr < MemoryMap.START_RAM) && (data_addr + size > MemoryMap.END_RAM))
					{
						log.warn("hleIoRead - id " + id.ToString("x") + " data is outside of ram 0x" + data_addr.ToString("x") + " - 0x" + (data_addr + size).ToString("x"));
						result = ERROR_KERNEL_FILE_READ_ERROR;
					}
					else if ((info.flags & PSP_O_RDWR) == PSP_O_WRONLY)
					{
						result = ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
					}
					else if (info.vFile != null)
					{
						timings = info.vFile.Timings;
						if (info.sectorBlockMode)
						{
							// In sectorBlockMode, the size is a number of sectors
							size *= UmdIsoFile.sectorLength;
						}
						// Using readFully for ms/umd compatibility, but now we must
						// manually make sure it doesn't read off the end of the file.
						if (info.position + size > info.vFile.length())
						{
							int oldSize = size;
							size = (int)(info.vFile.length() - info.position);
							if (log.DebugEnabled)
							{
								log.debug(string.Format("hleIoRead - clamping size old={0:D}, new={1:D}, position={2:D}, len={3:D}", oldSize, size, info.position, info.vFile.length()));
							}
						}

						if (async)
						{
							// Execute the read operation in the IO async thread
							asyncAction = new IOAsyncReadAction(this, info, data_addr, requestedSize, size);
							result = 0;
						}
						else
						{
							position = info.position;
							vFile = info.vFile;
							result = info.vFile.ioRead(new TPointer(Memory.Instance, data_addr), size);
							if (result >= 0)
							{
								size = result;
								info.position += result;
							}
							else
							{
								size = 0;
							}

							if (log.TraceEnabled)
							{
								log.trace(string.Format("hleIoRead: {0}", Utilities.getMemoryDump(data_addr, System.Math.Min(16, size))));
							}

							// Invalidate any compiled code in the read range
							RuntimeContext.invalidateRange(data_addr, size);

							if (info.sectorBlockMode && result > 0)
							{
								result /= UmdIsoFile.sectorLength;
							}
						}
					}
					else if ((info.readOnlyFile == null) || (info.position >= info.readOnlyFile.length()))
					{
						// Ignore empty handles and allow seeking off the end of the file, just return 0 bytes read/written.
						result = 0;
					}
					else
					{
						if (info.sectorBlockMode)
						{
							// In sectorBlockMode, the size is a number of sectors
							size *= UmdIsoFile.sectorLength;
						}
						// Using readFully for ms/umd compatibility, but now we must
						// manually make sure it doesn't read off the end of the file.
						if (info.readOnlyFile.FilePointer + size > info.readOnlyFile.length())
						{
							int oldSize = size;
							size = (int)(info.readOnlyFile.length() - info.readOnlyFile.FilePointer);
							if (log.DebugEnabled)
							{
								log.debug("hleIoRead - clamping size old=" + oldSize + " new=" + size + " fp=" + info.readOnlyFile.FilePointer + " len=" + info.readOnlyFile.length());
							}
						}

						if (async)
						{
							// Execute the read operation in the IO async thread
							asyncAction = new IOAsyncReadAction(this, info, data_addr, requestedSize, size);
							result = 0;
						}
						else
						{
							position = info.position;
							dataInput = info.readOnlyFile;
							Utilities.readFully(info.readOnlyFile, data_addr, size);
							info.position += size;
							result = size;

							if (log.TraceEnabled)
							{
								log.trace(string.Format("hleIoRead: {0}", Utilities.getMemoryDump(data_addr, System.Math.Min(16, size))));
							}

							// Invalidate any compiled code in the read range
							RuntimeContext.invalidateRange(data_addr, size);

							if (info.sectorBlockMode)
							{
								result /= UmdIsoFile.sectorLength;
							}
						}
					}
				}
				catch (IOException e)
				{
					log.error("hleIoRead", e);
					result = ERROR_KERNEL_FILE_READ_ERROR;
				}
				catch (Exception e)
				{
					log.error("hleIoRead", e);
					result = ERROR_KERNEL_FILE_READ_ERROR;
				}
			}
			result = (int) updateResult(info, result, async, false, timings[IoOperation.read], asyncAction, size);
			// Call the IO listeners (performed in the async action if one is provided, otherwise call them here)
			if (asyncAction == null)
			{
				foreach (IIoListener ioListener in ioListeners)
				{
					ioListener.sceIoRead(result, id, data_addr, requestedSize, size, position, dataInput, vFile);
				}
			}

			if (!async)
			{
				if (size > 0x100)
				{
					delayIoOperation(timings[IoOperation.read], size);
				}
			}

			return result;
		}

		private long hleIoLseek(int id, long offset, int whence, bool resultIs64bit, bool async)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;
			IoInfo info = null;
			long result;

			if (id == STDOUT_ID || id == STDERR_ID || id == STDIN_ID)
			{ // stdio
				log.error("seek - can't seek on stdio id " + id.ToString("x"));
				result = -1;
			}
			else
			{
				try
				{
					info = fileIds[id];
					if (info == null)
					{
						log.warn("seek - unknown id " + id.ToString("x"));
						result = ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
					}
					else if (info.asyncPending || info.asyncResultPending)
					{
						log.warn("seek - id " + id.ToString("x") + " PSP_ERROR_ASYNC_BUSY");
						result = ERROR_KERNEL_ASYNC_BUSY;
					}
					else if (info.vFile != null)
					{
						timings = info.vFile.Timings;
						if (info.sectorBlockMode)
						{
							// In sectorBlockMode, the offset is a sector number
							offset *= UmdIsoFile.sectorLength;
						}

						long newPosition;
						switch (whence)
						{
							case PSP_SEEK_SET:
								newPosition = offset;
								break;
							case PSP_SEEK_CUR:
								newPosition = info.position + offset;
								break;
							case PSP_SEEK_END:
								newPosition = info.vFile.length() + offset;
								break;
							default:
								log.error(string.Format("seek - unhandled whence {0:D}", whence));
								// Force an invalid argument error
								newPosition = -1;
								break;
						}

						if (newPosition >= 0)
						{
							info.position = newPosition;

							if (info.position <= info.vFile.length())
							{
								info.vFile.ioLseek(info.position);
							}

							result = info.position;
							if (info.sectorBlockMode)
							{
								result /= UmdIsoFile.sectorLength;
							}
						}
						else
						{
							// PSP returns -1 for this case
							result = -1;
						}
					}
					else if (info.readOnlyFile == null)
					{
						// Ignore empty handles.
						result = 0;
					}
					else
					{
						if (info.sectorBlockMode)
						{
							// In sectorBlockMode, the offset is a sector number
							offset *= UmdIsoFile.sectorLength;
						}

						switch (whence)
						{
							case PSP_SEEK_SET:
								if (offset < 0)
								{
									log.warn("SEEK_SET id " + id.ToString("x") + " filename:'" + info.filename + "' offset=0x" + offset.ToString("x") + " (less than 0!)");
									// PSP returns -1 for this case
									result = -1;
									foreach (IIoListener ioListener in ioListeners)
									{
										ioListener.sceIoSeek64(ERROR_INVALID_ARGUMENT, id, offset, whence);
									}
									return result;
								}
								info.position = offset;

								if (offset <= info.readOnlyFile.length())
								{
									info.readOnlyFile.seek(offset);
								}
								break;
							case PSP_SEEK_CUR:
								if (info.position + offset < 0)
								{
									log.warn("SEEK_CUR id " + id.ToString("x") + " filename:'" + info.filename + "' newposition=0x" + (info.position + offset).ToString("x") + " (less than 0!)");
									// PSP returns -1 for this case
									result = -1;
									foreach (IIoListener ioListener in ioListeners)
									{
										ioListener.sceIoSeek64(ERROR_INVALID_ARGUMENT, id, offset, whence);
									}
									return result;
								}
								info.position += offset;

								if (info.position <= info.readOnlyFile.length())
								{
									info.readOnlyFile.seek(info.position);
								}
								break;
							case PSP_SEEK_END:
								if (info.readOnlyFile.length() + offset < 0)
								{
									log.warn("SEEK_END id " + id.ToString("x") + " filename:'" + info.filename + "' newposition=0x" + (info.position + offset).ToString("x") + " (less than 0!)");
									// PSP returns -1 for this case
									result = -1;
									foreach (IIoListener ioListener in ioListeners)
									{
										ioListener.sceIoSeek64(ERROR_INVALID_ARGUMENT, id, offset, whence);
									}
									return result;
								}
								info.position = info.readOnlyFile.length() + offset;

								if (info.position <= info.readOnlyFile.length())
								{
									info.readOnlyFile.seek(info.position);
								}
								break;
							default:
								log.error("seek - unhandled whence " + whence);
								break;
						}
						result = info.position;
						if (info.sectorBlockMode)
						{
							result /= UmdIsoFile.sectorLength;
						}
					}
				}
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					result = -1;
				}
			}
			result = updateResult(info, result, async, resultIs64bit, timings[IoOperation.seek]);

			if (resultIs64bit)
			{
				foreach (IIoListener ioListener in ioListeners)
				{
					ioListener.sceIoSeek64(result, id, offset, whence);
				}
			}
			else
			{
				foreach (IIoListener ioListener in ioListeners)
				{
					ioListener.sceIoSeek32((int) result, id, (int) offset, whence);
				}
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleIoLseek returning 0x{0:X}", result));
			}

			if (!async)
			{
				delayIoOperation(timings[IoOperation.seek]);
			}

			return result;
		}

		public virtual int hleIoIoctl(int id, int cmd, int indata_addr, int inlen, int outdata_addr, int outlen, bool async)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;
			IoInfo info = null;
			int result;
			Memory mem = Memory.Instance;
			bool needDelayIoOperation = true;

			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("hleIoIoctl(id=%x, cmd=0x%08X, indata=0x%08X, inlen=%d, outdata=0x%08X, outlen=%d, async=%b", id, cmd, indata_addr, inlen, outdata_addr, outlen, async));
				log.debug(string.Format("hleIoIoctl(id=%x, cmd=0x%08X, indata=0x%08X, inlen=%d, outdata=0x%08X, outlen=%d, async=%b", id, cmd, indata_addr, inlen, outdata_addr, outlen, async));
				if (Memory.isAddressGood(indata_addr))
				{
					for (int i = 0; i < inlen; i += 4)
					{
						log.debug(string.Format("hleIoIoctl indata[{0:D}]=0x{1:X8}", i / 4, mem.read32(indata_addr + i)));
					}
				}
				if (Memory.isAddressGood(outdata_addr))
				{
					for (int i = 0; i < System.Math.Min(outlen, 256); i += 4)
					{
						log.debug(string.Format("hleIoIoctl outdata[{0:D}]=0x{1:X8}", i / 4, mem.read32(outdata_addr + i)));
					}
				}
			}

			info = fileIds[id];
			if (info == null)
			{
				IoDirInfo dirInfo = dirIds[id];
				if (dirInfo != null)
				{
					switch (cmd)
					{
						// Set sceIoDread file name filter
						case 0x02415050:
							if (inlen == 4)
							{
								int fileNameFilterAddr = mem.read32(indata_addr);
								dirInfo.fileNameFilter = Utilities.readStringZ(mem, fileNameFilterAddr);
								if (log.DebugEnabled)
								{
									log.debug(string.Format("hleIoIoctl settings sceIoDread file name filter '{0}'", dirInfo.fileNameFilter));
								}
								result = 0;
							}
							else
							{
								log.warn(string.Format("hleIoIoctl cmd=0x{0:X8} 0x{1:X8} {2:D} unsupported parameters", cmd, indata_addr, inlen));
								result = ERROR_INVALID_ARGUMENT;
							}
							break;
						default:
							result = -1;
							log.warn(string.Format("hleIoIoctl 0x{0:X8} unknown command on IoDirInfo, inlen={1:D}, outlen={2:D}", cmd, inlen, outlen));
							if (Memory.isAddressGood(indata_addr))
							{
								for (int i = 0; i < inlen; i += 4)
								{
									log.warn(string.Format("hleIoIoctl indata[{0:D}]=0x{1:X8}", i / 4, mem.read32(indata_addr + i)));
								}
							}
							if (Memory.isAddressGood(outdata_addr))
							{
								for (int i = 0; i < System.Math.Min(outlen, 256); i += 4)
								{
									log.warn(string.Format("hleIoIoctl outdata[{0:D}]=0x{1:X8}", i / 4, mem.read32(outdata_addr + i)));
								}
							}
							break;
					}
				}
				else
				{
					log.warn(string.Format("hleIoIoctl - unknown id 0x{0:X}", id));
					result = ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
				}
			}
			else if (info.asyncPending || info.asyncResultPending)
			{
				// Can't execute another operation until the previous one completed
				log.warn(string.Format("hleIoIoctl - id 0x{0:X} PSP_ERROR_ASYNC_BUSY", id));
				result = ERROR_KERNEL_ASYNC_BUSY;
			}
			else if (info.vFile != null && cmd != 0x04100001)
			{
				timings = info.vFile.Timings;
				result = info.vFile.ioIoctl(cmd, new TPointer(mem, indata_addr), inlen, new TPointer(mem, outdata_addr), outlen);
			}
			else
			{
				switch (cmd)
				{
					// UMD file seek set.
					case 0x01010005:
					{
						if (Memory.isAddressGood(indata_addr) && inlen >= 4)
						{
							if (info.UmdFile)
							{
								try
								{
									int offset = mem.read32(indata_addr);
									log.debug("hleIoIoctl umd file seek set " + offset);
									info.readOnlyFile.seek(offset);
									info.position = offset;
									result = 0;
								}
								catch (IOException e)
								{
									// Should never happen?
									log.warn("hleIoIoctl cmd=0x01010005 exception: " + e.Message);
									result = ERROR_KERNEL_FILE_READ_ERROR;
								}
							}
							else
							{
								log.warn("hleIoIoctl cmd=0x01010005 only allowed on UMD files");
								result = ERROR_INVALID_ARGUMENT;
							}
						}
						else
						{
							log.warn("hleIoIoctl cmd=0x01010005 " + string.Format("0x{0:X8} {1:D}", indata_addr, inlen) + " unsupported parameters");
							result = ERROR_INVALID_ARGUMENT;
						}
						break;
					}
					// UMD file ahead (from info listed in the log file of "The Legend of Heroes: Trails in the Sky SC")
					case 0x0101000A:
					{
						if (Memory.isAddressGood(indata_addr) && inlen >= 4)
						{
							int length = mem.read32(indata_addr);
							if (log.InfoEnabled)
							{
								log.info(string.Format("hleIoIoctl cmd=0x{0:X8} length=0x{1:X}", cmd, length));
							}
							result = 0;
						}
						else
						{
							log.warn(string.Format("hleIoIoctl cmd=0x{0:X8} 0x{1:X8} {2:D} unsupported parameters", cmd, indata_addr, inlen));
							result = ERROR_INVALID_ARGUMENT;
						}
						break;
					}
					// Get UMD Primary Volume Descriptor
					case 0x01020001:
					{
						if (Memory.isAddressGood(outdata_addr) && outlen == UmdIsoFile.sectorLength)
						{
							if (info.UmdFile && iso != null)
							{
								try
								{
									sbyte[] primaryVolumeSector = iso.readSector(UmdIsoReader.startSector);
									IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(outdata_addr, outlen, 1);
									for (int i = 0; i < outlen; i++)
									{
										memoryWriter.writeNext(primaryVolumeSector[i] & 0xFF);
									}
									memoryWriter.flush();
									result = 0;
								}
								catch (IOException e)
								{
									log.error(e);
									result = ERROR_KERNEL_FILE_READ_ERROR;
								}
							}
							else
							{
								log.warn("hleIoIoctl cmd=0x01020001 only allowed on UMD files");
								result = ERROR_INVALID_ARGUMENT;
							}
						}
						else
						{
							log.warn("hleIoIoctl cmd=0x01020001 " + string.Format("0x{0:X8} {1:D}", outdata_addr, outlen) + " unsupported parameters");
							result = SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
						}
						break;
					}
					// Get UMD Path Table
					case 0x01020002:
					{
						if (Memory.isAddressGood(outdata_addr) && outlen <= UmdIsoFile.sectorLength)
						{
							if (info.UmdFile && iso != null)
							{
								try
								{
									sbyte[] primaryVolumeSector = iso.readSector(UmdIsoReader.startSector);
									ByteBuffer primaryVolume = ByteBuffer.wrap(primaryVolumeSector);
									primaryVolume.position(140);
									int pathTableLocation = Utilities.readWord(primaryVolume);
									sbyte[] pathTableSector = iso.readSector(pathTableLocation);
									IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(outdata_addr, outlen, 1);
									for (int i = 0; i < outlen; i++)
									{
										memoryWriter.writeNext(pathTableSector[i] & 0xFF);
									}
									memoryWriter.flush();
									result = 0;
								}
								catch (IOException e)
								{
									log.error(e);
									result = ERROR_KERNEL_FILE_READ_ERROR;
								}
							}
							else
							{
								log.warn("hleIoIoctl cmd=0x01020002 only allowed on UMD files");
								result = ERROR_INVALID_ARGUMENT;
							}
						}
						else
						{
							log.warn("hleIoIoctl cmd=0x01020002 " + string.Format("0x{0:X8} {1:D}", outdata_addr, outlen) + " unsupported parameters");
							result = SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
						}
						break;
					}
					// Get Sector size
					case 0x01020003:
					{
						if (Memory.isAddressGood(outdata_addr) && outlen == 4)
						{
							if (info.UmdFile && iso != null)
							{
								mem.write32(outdata_addr, UmdIsoFile.sectorLength);
								result = 0;
							}
							else
							{
								log.warn("hleIoIoctl cmd=0x01020003 only allowed on UMD files");
								result = ERROR_INVALID_ARGUMENT;
							}
						}
						else
						{
							log.warn("hleIoIoctl cmd=0x01020003 " + string.Format("0x{0:X8} {1:D}", outdata_addr, outlen) + " unsupported parameters");
							result = SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
						}
						needDelayIoOperation = false;
						break;
					}
					// Get UMD file pointer.
					case 0x01020004:
					{
						if (Memory.isAddressGood(outdata_addr) && outlen >= 4)
						{
							if (info.UmdFile)
							{
								try
								{
									int fPointer = (int) info.readOnlyFile.FilePointer;
									// TODO for block files, does it return a number of blocks or a number of bytes?
									mem.write32(outdata_addr, fPointer);
									if (log.DebugEnabled)
									{
										log.debug(string.Format("hleIoIoctl umd file get file pointer 0x{0:X}", fPointer));
									}
									result = 0;
								}
								catch (IOException e)
								{
									log.warn("hleIoIoctl cmd=0x01020004 exception: " + e.Message);
									result = ERROR_KERNEL_FILE_READ_ERROR;
								}
							}
							else
							{
								log.warn("hleIoIoctl cmd=0x01020004 only allowed on UMD files");
								result = ERROR_INVALID_ARGUMENT;
							}
						}
						else
						{
							log.warn("hleIoIoctl cmd=0x01020004 " + string.Format("0x{0:X8} {1:D}", outdata_addr, outlen) + " unsupported parameters");
							result = ERROR_INVALID_ARGUMENT;
						}
						break;
					}
					// Get UMD file start sector.
					case 0x01020006:
					{
						if (Memory.isAddressGood(outdata_addr) && outlen >= 4)
						{
							int startSector = 0;
							if (info.UmdFile && info.readOnlyFile is UmdIsoFile)
							{
								UmdIsoFile file = (UmdIsoFile) info.readOnlyFile;
								startSector = file.StartSector;
								log.debug("hleIoIoctl umd file get start sector " + startSector);
								mem.write32(outdata_addr, startSector);
								result = 0;
							}
							else
							{
								log.warn("hleIoIoctl cmd=0x01020006 only allowed on UMD files and only implemented for UmdIsoFile");
								result = ERROR_INVALID_ARGUMENT;
							}
						}
						else
						{
							log.warn("hleIoIoctl cmd=0x01020006 " + string.Format("0x{0:X8} {1:D}", outdata_addr, outlen) + " unsupported parameters");
							result = ERROR_INVALID_ARGUMENT;
						}
						break;
					}
					// Get UMD file length in bytes.
					case 0x01020007:
					{
						if (Memory.isAddressGood(outdata_addr) && outlen >= 8)
						{
							if (info.UmdFile)
							{
								try
								{
									long length = info.readOnlyFile.length();
									mem.write64(outdata_addr, length);
									log.debug("hleIoIoctl get file size " + length);
									result = 0;
								}
								catch (IOException e)
								{
									// Should never happen?
									log.warn("hleIoIoctl cmd=0x01020007 exception: " + e.Message);
									result = ERROR_KERNEL_FILE_READ_ERROR;
								}
							}
							else
							{
								log.warn("hleIoIoctl cmd=0x01020007 only allowed on UMD files");
								result = ERROR_INVALID_ARGUMENT;
							}
						}
						else
						{
							log.warn("hleIoIoctl cmd=0x01020007 " + string.Format("0x{0:X8} {1:D}", outdata_addr, outlen) + " unsupported parameters");
							result = ERROR_INVALID_ARGUMENT;
						}
						break;
					}
					// Read UMD file.
					case 0x01030008:
					{
						if (Memory.isAddressGood(indata_addr) && inlen >= 4)
						{
							int length = mem.read32(indata_addr);
							if (length > 0)
							{
								if (Memory.isAddressGood(outdata_addr) && outlen >= length)
								{
									try
									{
										Utilities.readFully(info.readOnlyFile, outdata_addr, length);
										info.position += length;
										result = length;
									}
									catch (IOException e)
									{
										log.error(e);
										result = ERROR_KERNEL_FILE_READ_ERROR;
									}
								}
								else
								{
									log.warn(string.Format("hleIoIoctl cmd=0x{0:X8} inlen={1:D} unsupported output parameters 0x{2:X8} {3:D}", cmd, inlen, outdata_addr, outlen));
									result = ERROR_INVALID_ARGUMENT;
								}
							}
							else
							{
								log.warn(string.Format("hleIoIoctl cmd=0x{0:X8} unsupported input parameters 0x{1:X8} {2:D}, length={3:D}", cmd, indata_addr, inlen, length));
								result = ERROR_INVALID_ARGUMENT;
							}
						}
						else
						{
							log.warn(string.Format("hleIoIoctl cmd=0x{0:X8} unsupported input parameters 0x{1:X8} {2:D}", cmd, indata_addr, inlen));
							result = ERROR_INVALID_ARGUMENT;
						}
						break;
					}
					// UMD disc read sectors operation.
					case 0x01F30003:
					{
						if (Memory.isAddressGood(indata_addr) && inlen >= 4)
						{
							int numberOfSectors = mem.read32(indata_addr);
							if (numberOfSectors > 0)
							{
								if (Memory.isAddressGood(outdata_addr) && outlen >= numberOfSectors)
								{
									try
									{
										int length = numberOfSectors * UmdIsoFile.sectorLength;
										Utilities.readFully(info.readOnlyFile, outdata_addr, length);
										info.position += length;
										result = length / UmdIsoFile.sectorLength;
									}
									catch (IOException e)
									{
										log.error(e);
										result = ERROR_KERNEL_FILE_READ_ERROR;
									}
								}
								else
								{
									log.warn(string.Format("hleIoIoctl cmd=0x{0:X8} inlen={1:D} unsupported output parameters 0x{2:X8} {3:D}", cmd, inlen, outdata_addr, outlen));
									result = ERROR_ERRNO_INVALID_ARGUMENT;
								}
							}
							else
							{
								log.warn(string.Format("hleIoIoctl cmd=0x{0:X8} unsupported input parameters 0x{1:X8} {2:D} numberOfSectors={3:D}", cmd, indata_addr, inlen, numberOfSectors));
								result = ERROR_ERRNO_INVALID_ARGUMENT;
							}
						}
						else
						{
							log.warn(string.Format("hleIoIoctl cmd=0x{0:X8} unsupported input parameters 0x{1:X8} {2:D}", cmd, indata_addr, inlen));
							result = ERROR_ERRNO_INVALID_ARGUMENT;
						}
						break;
					}
					// UMD file seek whence.
					case 0x01F100A6:
					{
						if (Memory.isAddressGood(indata_addr) && inlen >= 16)
						{
							if (info.UmdFile)
							{
								try
								{
									long offset = mem.read64(indata_addr);
									int whence = mem.read32(indata_addr + 12);
									if (info.sectorBlockMode)
									{
										offset *= UmdIsoFile.sectorLength;
									}
									if (log.DebugEnabled)
									{
										log.debug("hleIoIoctl UMD file seek offset " + offset + ", whence " + whence);
									}
									switch (whence)
									{
										case PSP_SEEK_SET:
										{
											info.position = offset;
											info.readOnlyFile.seek(info.position);
											result = 0;
											break;
										}
										case PSP_SEEK_CUR:
										{
											info.position = info.position + offset;
											info.readOnlyFile.seek(info.position);
											result = 0;
											break;
										}
										case PSP_SEEK_END:
										{
											info.position = info.readOnlyFile.length() + offset;
											info.readOnlyFile.seek(info.position);
											result = 0;
											break;
										}
										default:
										{
											log.error("hleIoIoctl - unhandled whence " + whence);
											result = -1;
											break;
										}
									}
								}
								catch (IOException e)
								{
									// Should never happen?
									log.warn("hleIoIoctl cmd=0x01F100A6 exception: " + e.Message);
									result = -1;
								}
							}
							else
							{
								log.warn("hleIoIoctl cmd=0x01F100A6 only allowed on UMD files");
								result = ERROR_INVALID_ARGUMENT;
							}
						}
						else
						{
							log.warn("hleIoIoctl cmd=0x01F100A6 " + string.Format("0x{0:X8} {1:D}", indata_addr, inlen) + " unsupported parameters");
							result = ERROR_INVALID_ARGUMENT;
						}
						break;
					}
					// Define decryption key (DRM by amctrl.prx).
					case 0x04100001:
					{
						if (Memory.isAddressGood(indata_addr) && inlen == 16)
						{
							// Store the key.
							sbyte[] keyBuf = new sbyte[0x10];
							StringBuilder keyHex = new StringBuilder();
							for (int i = 0; i < 0x10; i++)
							{
								keyBuf[i] = (sbyte) mem.read8(indata_addr + i);
								keyHex.Append(string.Format("{0:X2}", keyBuf[i] & 0xFF));
							}

							if (log.DebugEnabled)
							{
								log.debug(string.Format("hleIoIoctl get AES key {0}", keyHex.ToString()));
							}

							IVirtualFile ioctlFile = null;
							if (info.readOnlyFile is UmdIsoFile)
							{
								ioctlFile = new UmdIsoVirtualFile((UmdIsoFile) info.readOnlyFile);
							}
							PGDVirtualFile pgdFile = new PGDVirtualFile(keyBuf, new SeekableDataInputVirtualFile(info.readOnlyFile, ioctlFile));
							if (!pgdFile.HeaderPresent)
							{
								// No "PGD" found in the header, leave the file unchanged
								result = 0;
							}
							else if (pgdFile.Valid)
							{
								info.vFile = pgdFile;
								result = 0;
							}
							else
							{
								result = SceKernelErrors.ERROR_PGD_INVALID_HEADER;
							}
						}
						else
						{
							log.warn(string.Format("hleIoIoctl cmd=0x04100001 indata=0x{0:X8} inlen={1:D} unsupported parameters", indata_addr, inlen));
							result = ERROR_INVALID_ARGUMENT;
						}
						break;
					}
					// Check if LoadExec is allowed on the file
					case 0x00208013:
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Checking if LoadExec is allowed on '{0}'", info));
						}
						// Result == 0: LoadExec allowed
						// Result != 0: LoadExec prohibited
						result = 0;
						break;
					}
					// Check if LoadModule is allowed on the file
					case 0x00208003:
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Checking if LoadModule is allowed on '{0}'", info));
						}
						// Result == 0: LoadModule allowed
						// Result != 0: LoadModule prohibited
						result = 0;
						break;
					}
					// Check if PRX type is allowed on the file
					case 0x00208081:
					case 0x00208082:
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Checking if PRX type is allowed on '{0}'", info));
						}
						// Result == 0: PRX type allowed
						// Result != 0: PRX type prohibited
						result = 0;
						break;
					}
					default:
					{
						result = -1;
						log.warn(string.Format("hleIoIoctl 0x{0:X8} unknown command, inlen={1:D}, outlen={2:D}", cmd, inlen, outlen));
						if (Memory.isAddressGood(indata_addr))
						{
							for (int i = 0; i < inlen; i += 4)
							{
								log.warn(string.Format("hleIoIoctl indata[{0:D}]=0x{1:X8}", i / 4, mem.read32(indata_addr + i)));
							}
						}
						if (Memory.isAddressGood(outdata_addr))
						{
							for (int i = 0; i < System.Math.Min(outlen, 256); i += 4)
							{
								log.warn(string.Format("hleIoIoctl outdata[{0:D}]=0x{1:X8}", i / 4, mem.read32(outdata_addr + i)));
							}
						}
						break;
					}
				}
			}

			result = (int) updateResult(info, result, async, false, timings[IoOperation.ioctl]);
			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoIoctl(result, id, cmd, indata_addr, inlen, outdata_addr, outlen);
			}

			if (needDelayIoOperation && !async)
			{
				delayIoOperation(timings[IoOperation.ioctl]);
			}

			return result;
		}

		public virtual void hleRegisterStdPipe(int id, SceKernelMppInfo msgPipeInfo)
		{
			if (id < 0 || id >= stdRedirects.Length)
			{
				return;
			}

			stdRedirects[id] = msgPipeInfo;
		}

		public virtual void hleEjectMemoryStick()
		{
			if (MemoryStick.Inserted)
			{
				previousFatMsState = MemoryStick.StateFatMs;
				MemoryStick.StateFatMs = MemoryStick.PSP_FAT_MEMORYSTICK_STATE_REMOVED;
				Modules.ThreadManForUserModule.hleKernelNotifyCallback(THREAD_CALLBACK_MEMORYSTICK_FAT, -1, MemoryStick.StateFatMs);
				Emulator.MainGUI.onMemoryStickChange();
			}
		}

		public virtual void hleInsertMemoryStick()
		{
			if (!MemoryStick.Inserted)
			{
				MemoryStick.StateFatMs = previousFatMsState;
				Modules.ThreadManForUserModule.hleKernelNotifyCallback(THREAD_CALLBACK_MEMORYSTICK_FAT, -1, MemoryStick.StateFatMs);
				Emulator.MainGUI.onMemoryStickChange();
			}
		}

		private int hleIoRename(int oldFileNameAddr, string oldFileName, int newFileNameAddr, string newFileName)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;

			// The new file name can omit the file directory, in which case the directory
			// of the old file name is used.
			// I.e., when renaming "ms0:/PSP/SAVEDATA/xxxx" into "yyyy",
			// actually rename into "ms0:/PSP/SAVEDATA/yyyy".
			if (!newFileName.Contains("/"))
			{
				int prefixOffset = oldFileName.LastIndexOf("/", StringComparison.Ordinal);
				if (prefixOffset >= 0)
				{
					newFileName = oldFileName.Substring(0, prefixOffset + 1) + newFileName;
				}
			}

			string oldpcfilename = getDeviceFilePath(oldFileName);
			string newpcfilename = getDeviceFilePath(newFileName);
			int result;

			string absoluteOldFileName = getAbsoluteFileName(oldFileName);
			StringBuilder localOldFileName = new StringBuilder();
			IVirtualFileSystem oldVfs = vfsManager.getVirtualFileSystem(absoluteOldFileName, localOldFileName);
			if (oldVfs != null)
			{
				string absoluteNewFileName = getAbsoluteFileName(newFileName);
				StringBuilder localNewFileName = new StringBuilder();
				IVirtualFileSystem newVfs = vfsManager.getVirtualFileSystem(absoluteNewFileName, localNewFileName);
				if (oldVfs != newVfs)
				{
					log.error(string.Format("sceIoRename - renaming across devices not allowed '{0}' - '{1}'", oldFileName, newFileName));
					result = ERROR_ERRNO_DEVICE_NOT_FOUND;
				}
				else
				{
					timings = oldVfs.Timings;
					result = oldVfs.ioRename(localOldFileName.ToString(), localNewFileName.ToString());
				}
			}
			else if (useVirtualFileSystem)
			{
				log.error(string.Format("sceIoRename - device not found '{0}'", oldFileName));
				result = ERROR_ERRNO_DEVICE_NOT_FOUND;
			}
			else if (!string.ReferenceEquals(oldpcfilename, null))
			{
				if (isUmdPath(oldpcfilename))
				{
					result = -1;
				}
				else
				{
					File file = new File(oldpcfilename);
					File newfile = new File(newpcfilename);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceIoRename: renaming file '{0}' to '{1}'", oldpcfilename, newpcfilename));
					}
					if (file.renameTo(newfile))
					{
						result = 0;
					}
					else
					{
						log.warn(string.Format("sceIoRename failed: {0}({1}) to {2}({3})", oldFileName, oldpcfilename, newFileName, newpcfilename));
						if (file.exists())
						{
							result = -1;
						}
						else
						{
							result = ERROR_ERRNO_FILE_NOT_FOUND;
						}
					}
				}
			}
			else
			{
				result = -1;
			}

			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoRename(result, oldFileNameAddr, oldFileName, newFileNameAddr, newFileName);
			}

			delayIoOperation(timings[IoOperation.rename]);

			return result;
		}

		public virtual int hleIoGetstat(int filenameAddr, string filename, TPointer statAddr)
		{
			int result;
			SceIoStat stat = null;

			string absoluteFileName = getAbsoluteFileName(filename);
			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = vfsManager.getVirtualFileSystem(absoluteFileName, localFileName);
			if (vfs != null)
			{
				stat = new SceIoStat();
				result = vfs.ioGetstat(localFileName.ToString(), stat);
			}
			else if (useVirtualFileSystem)
			{
				log.error(string.Format("sceIoGetstat - device not found '{0}'", filename));
				result = ERROR_ERRNO_DEVICE_NOT_FOUND;
			}
			else
			{
				string pcfilename = getDeviceFilePath(filename);
				stat = this.stat(pcfilename);
				result = (stat != null) ? 0 : ERROR_ERRNO_FILE_NOT_FOUND;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceIoGetstat returning 0x{0:X8}, {1}", result, stat));
			}

			if (stat != null && result == 0)
			{
				stat.write(statAddr);
			}

			if (filenameAddr != 0)
			{
				foreach (IIoListener ioListener in ioListeners)
				{
					ioListener.sceIoGetStat(result, filenameAddr, filename, statAddr.Address);
				}
			}

			return result;
		}

		/// <summary>
		/// sceIoPollAsync
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="resAddr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x3251EA56, version = 150, checkInsideInterrupt = true) public int sceIoPollAsync(int id, @CanBeNull pspsharp.HLE.TPointer64 resAddr)
		[HLEFunction(nid : 0x3251EA56, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoPollAsync(int id, TPointer64 resAddr)
		{
			return hleIoWaitAsync(id, resAddr, false, false);
		}

		/// <summary>
		/// sceIoWaitAsync
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="resAddr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE23EEC33, version = 150, checkInsideInterrupt = true) public int sceIoWaitAsync(int id, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer64 resAddr)
		[HLEFunction(nid : 0xE23EEC33, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoWaitAsync(int id, TPointer64 resAddr)
		{
			return hleIoWaitAsync(id, resAddr, true, false);
		}

		/// <summary>
		/// sceIoWaitAsyncCB
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="resAddr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x35DBD746, version = 150, checkInsideInterrupt = true) public int sceIoWaitAsyncCB(int id, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer64 resAddr)
		[HLEFunction(nid : 0x35DBD746, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoWaitAsyncCB(int id, TPointer64 resAddr)
		{
			return hleIoWaitAsync(id, resAddr, true, true);
		}

		/// <summary>
		/// sceIoGetAsyncStat
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="poll"> </param>
		/// <param name="res_addr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xCB05F8D6, version = 150, checkInsideInterrupt = true) public int sceIoGetAsyncStat(int id, int poll, @CanBeNull pspsharp.HLE.TPointer64 res_addr)
		[HLEFunction(nid : 0xCB05F8D6, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoGetAsyncStat(int id, int poll, TPointer64 res_addr)
		{
			return hleIoWaitAsync(id, res_addr, (poll == 0), false);
		}

		/// <summary>
		/// sceIoChangeAsyncPriority
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="priority">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xB293727F, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoChangeAsyncPriority(int id, int priority)
		{
			if (priority == -1)
			{
				// Take the priority of the thread executing the first async operation,
				// do not take the priority of the thread executing sceIoChangeAsyncPriority().
			}
			else if (priority < 0)
			{
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_PRIORITY;
			}

			if (id == -1)
			{
				defaultAsyncPriority = priority;
				return 0;
			}

			IoInfo info = fileIds[id];
			if (info == null)
			{
				log.warn("sceIoChangeAsyncPriority invalid fd=" + id);
				return SceKernelErrors.ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
			}

			info.asyncThreadPriority = priority;
			if (info.asyncThread != null)
			{
				if (priority < 0)
				{
					// If the async thread has already been started,
					// change its priority to the priority of the current thread,
					// i.e. to the priority of the thread having called sceIoChangeAsyncPriority().
					priority = Modules.ThreadManForUserModule.CurrentThread.currentPriority;
				}
				Modules.ThreadManForUserModule.hleKernelChangeThreadPriority(info.asyncThread, priority);
			}

			return 0;
		}

		/// <summary>
		/// sceIoSetAsyncCallback
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="cbid"> </param>
		/// <param name="notifyArg">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xA12A0514, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoSetAsyncCallback(int id, int cbid, int notifyArg)
		{
			IoInfo info = fileIds[id];
			if (info == null)
			{
				log.warn("sceIoSetAsyncCallback - unknown id " + id.ToString("x"));
				return ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
			}

			if (!Modules.ThreadManForUserModule.hleKernelRegisterCallback(SceKernelThreadInfo.THREAD_CALLBACK_IO, cbid))
			{
				log.warn("sceIoSetAsyncCallback - not a callback id " + id.ToString("x"));
				return -1;
			}

			info.cbid = cbid;
			info.notifyArg = notifyArg;
			triggerAsyncThread(info);

			return 0;
		}

		/// <summary>
		/// sceIoClose
		/// </summary>
		/// <param name="id">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x810C4BC3, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoClose(int id)
		{
			return hleIoClose(id, false);
		}

		/// <summary>
		/// sceIoCloseAsync
		/// </summary>
		/// <param name="id">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xFF5940B6, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoCloseAsync(int id)
		{
			return hleIoClose(id, true);
		}

		/// <summary>
		/// sceIoOpen
		/// </summary>
		/// <param name="filename"> </param>
		/// <param name="flags"> </param>
		/// <param name="permissions">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x109F50BC, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoOpen(PspString filename, int flags, int permissions)
		{
			return hleIoOpen(filename, flags, permissions, false);
		}

		/// <summary>
		/// sceIoOpenAsync
		/// </summary>
		/// <param name="filename"> </param>
		/// <param name="flags"> </param>
		/// <param name="permissions">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x89AA9906, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoOpenAsync(PspString filename, int flags, int permissions)
		{
			return hleIoOpen(filename, flags, permissions, true);
		}

		/// <summary>
		/// sceIoRead
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="data_addr"> </param>
		/// <param name="size">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x6A638D83, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoRead(int id, TPointer data_addr, int size)
		{
			return hleIoRead(id, data_addr.Address, size, false);
		}

		/// <summary>
		/// sceIoReadAsync
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="data_addr"> </param>
		/// <param name="size">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xA0B5A7C2, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoReadAsync(int id, TPointer data_addr, int size)
		{
			return hleIoRead(id, data_addr.Address, size, true);
		}

		/// <summary>
		/// sceIoWrite
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="data_addr"> </param>
		/// <param name="size">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x42EC03AC, version = 150, checkInsideInterrupt = true) public int sceIoWrite(int id, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer dataAddr, int size)
		[HLEFunction(nid : 0x42EC03AC, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoWrite(int id, TPointer dataAddr, int size)
		{
			return hleIoWrite(id, dataAddr, size, false);
		}

		/// <summary>
		/// sceIoWriteAsync
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="data_addr"> </param>
		/// <param name="size">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x0FACAB19, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoWriteAsync(int id, TPointer dataAddr, int size)
		{
			return hleIoWrite(id, dataAddr, size, true);
		}

		/// <summary>
		/// sceIoLseek
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="offset"> </param>
		/// <param name="whence">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x27EB27B8, version : 150, checkInsideInterrupt : true)]
		public virtual long sceIoLseek(int id, long offset, int whence)
		{
			return hleIoLseek(id, offset, whence, true, false);
		}

		/// <summary>
		/// sceIoLseekAsync
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="offset"> </param>
		/// <param name="whence">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x71B19E77, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoLseekAsync(int id, long offset, int whence)
		{
			return (int) hleIoLseek(id, offset, whence, true, true);
		}

		/// <summary>
		/// sceIoLseek32
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="offset"> </param>
		/// <param name="whence">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x68963324, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoLseek32(int id, int offset, int whence)
		{
			return (int) hleIoLseek(id, (long) offset, whence, false, false);
		}

		/// <summary>
		/// sceIoLseek32Async
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="offset"> </param>
		/// <param name="whence"> </param>
		[HLEFunction(nid : 0x1B385D8F, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoLseek32Async(int id, int offset, int whence)
		{
			return (int) hleIoLseek(id, (long) offset, whence, false, true);
		}

		/// <summary>
		/// sceIoIoctl
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="cmd"> </param>
		/// <param name="indata_addr"> </param>
		/// <param name="inlen"> </param>
		/// <param name="outdata_addr"> </param>
		/// <param name="outlen">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x63632449, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoIoctl(int id, int cmd, int indata_addr, int inlen, int outdata_addr, int outlen)
		{
			return hleIoIoctl(id, cmd, indata_addr, inlen, outdata_addr, outlen, false);
		}

		/// <summary>
		/// sceIoIoctlAsync
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="cmd"> </param>
		/// <param name="indata_addr"> </param>
		/// <param name="inlen"> </param>
		/// <param name="outdata_addr"> </param>
		/// <param name="outlen">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xE95A012B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoIoctlAsync(int id, int cmd, int indata_addr, int inlen, int outdata_addr, int outlen)
		{
			return hleIoIoctl(id, cmd, indata_addr, inlen, outdata_addr, outlen, true);
		}

		/// <summary>
		/// Opens a directory for listing.
		/// </summary>
		/// <param name="dirname">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xB29DDF9C, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoDopen(PspString dirname)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;
			int result;

			string pcfilename = getDeviceFilePath(dirname.String);
			string absoluteFileName = getAbsoluteFileName(dirname.String);
			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = vfsManager.getVirtualFileSystem(absoluteFileName, localFileName);
			if (vfs != null)
			{
				timings = vfs.Timings;
				string[] fileNames = vfs.ioDopen(localFileName.ToString());
				if (fileNames == null)
				{
					result = ERROR_ERRNO_FILE_NOT_FOUND;
				}
				else
				{
					IoDirInfo info = new IoDirInfo(this, localFileName.ToString(), fileNames, vfs);
					result = info.id;
				}
			}
			else if (!string.ReferenceEquals(pcfilename, null))
			{
				if (isUmdPath(pcfilename))
				{
					// Files in our iso virtual file system
					string isofilename = trimUmdPrefix(pcfilename);
					if (log.DebugEnabled)
					{
						log.debug("sceIoDopen - isofilename = " + isofilename);
					}
					if (iso == null)
					{ // check umd is mounted
						log.error("sceIoDopen - no umd mounted");
						result = ERROR_ERRNO_DEVICE_NOT_FOUND;
					}
					else if (!Modules.sceUmdUserModule.UmdActivated)
					{ // check umd is activated
						log.warn("sceIoDopen - umd mounted but not activated");
						result = ERROR_KERNEL_NO_SUCH_DEVICE;
					}
					else
					{
						try
						{
							if (iso.isDirectory(isofilename))
							{
								string[] filenames = iso.listDirectory(isofilename);
								IoDirInfo info = new IoDirInfo(this, pcfilename, filenames);
								result = info.id;
							}
							else
							{
								log.warn("sceIoDopen '" + isofilename + "' not a umd directory!");
								result = ERROR_ERRNO_FILE_NOT_FOUND;
							}
						}
						catch (FileNotFoundException)
						{
							log.warn("sceIoDopen - '" + isofilename + "' umd file not found");
							result = ERROR_ERRNO_FILE_NOT_FOUND;
						}
						catch (IOException e)
						{
							log.warn("sceIoDopen - umd io error: " + e.Message);
							result = ERROR_ERRNO_FILE_NOT_FOUND;
						}
					}
				}
				else if (dirname.String.StartsWith("/", StringComparison.Ordinal) && dirname.String.IndexOf(":", StringComparison.Ordinal) != -1)
				{
					log.warn("sceIoDopen apps running outside of ms0 dir are not fully supported, relative child paths should still work");
					result = -1;
				}
				else
				{
					// Regular apps run from inside mstick dir or absolute path given
					if (log.DebugEnabled)
					{
						log.debug("sceIoDopen - pcfilename = " + pcfilename);
					}
					File f = new File(pcfilename);
					if (f.Directory)
					{
						string[] files = f.list();
						if (files != null)
						{
							for (int i = 0; i < files.Length; i++)
							{
								files[i] = getMsFileName(files[i]);
							}
						}
						IoDirInfo info = new IoDirInfo(this, pcfilename, files);
						result = info.id;
					}
					else
					{
						log.warn("sceIoDopen '" + pcfilename + "' not a directory! (could be missing)");
						result = ERROR_ERRNO_FILE_NOT_FOUND;
					}
				}
			}
			else
			{
				result = ERROR_ERRNO_FILE_NOT_FOUND;
			}
			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoDopen(result, dirname.Address, dirname.String);
			}

			delayIoOperation(timings[IoOperation.open]);

			return result;
		}

		/// <summary>
		/// sceIoDread
		/// </summary>
		/// <param name="id"> </param>
		/// <param name="direntAddr">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xE3EB004C, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoDread(int id, TPointer direntAddr)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;
			IoDirInfo info = dirIds[id];

			int result;
			if (info == null)
			{
				log.warn("sceIoDread unknown id " + id.ToString("x"));
				result = ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
			}
			else if (info.hasNext())
			{
				string filename = info.next();
				if (!string.ReferenceEquals(info.fileNameFilter, null))
				{
					// PSP file name pattern:
					//   '?' matches one character
					//   '*' matches any character sequence
					// To convert to regular expressions:
					//   replace '?' with '.'
					//   replace '*' with '.*'
					string pattern = info.fileNameFilter.Replace('?', '.');
					pattern = pattern.Replace("*", ".*");
					while (!filename.matches(pattern))
					{
						if (!info.hasNext())
						{
							if (log.DebugEnabled)
							{
								log.debug(string.Format("sceIoDread id=0x{0:X} no more files matching pattern '{1}'", id, info.fileNameFilter));
							}
							filename = null;
							break;
						}
						filename = info.next();
					}
				}

				SceIoDirent dirent = null;
				if (string.ReferenceEquals(filename, null))
				{
					result = 0;
				}
				else if (info.vfs != null)
				{
					timings = info.vfs.Timings;
					SceIoStat stat = new SceIoStat();
					dirent = new SceIoDirent(stat, filename);
					result = info.vfs.ioDread(info.path, dirent);
				}
				else
				{
					SceIoStat stat = this.stat(info.path + "/" + filename);
					if (stat != null)
					{
						dirent = new SceIoDirent(stat, filename);
						result = 1;
					}
					else
					{
						log.warn("sceIoDread id=" + id.ToString("x") + " stat failed (" + info.path + "/" + filename + ")");
						result = -1;
					}
				}

				if (dirent != null && result > 0)
				{
					if (log.DebugEnabled)
					{
						string type = (dirent.stat.attr & 0x10) != 0 ? "dir" : "file";
						log.debug(string.Format("sceIoDread id=0x{0:X} #{1:D} {2}='{3}', dir='{4}'", id, info.printableposition, type, info.path, filename));
					}

					if (info.vfs == null)
					{
						// Write only the extended info for the MemoryStick
						dirent.UseExtendedInfo = !info.path.StartsWith("disc", StringComparison.Ordinal);
					}
					dirent.write(direntAddr);
				}
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceIoDread id=0x{0:X} no more files", id));
				}
				result = 0;
			}
			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoDread(result, id, direntAddr.Address);
			}

			delayIoOperation(timings[IoOperation.dread]);

			return result;
		}

		/// <summary>
		/// sceIoDclose
		/// </summary>
		/// <param name="id">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xEB092469, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoDclose(int id)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;
			IoDirInfo info = dirIds[id];
			int result;

			if (info == null)
			{
				log.warn("sceIoDclose - unknown id " + id.ToString("x"));
				result = ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
			}
			else if (info.vfs != null)
			{
				result = info.vfs.ioDclose(info.path);
				info.close();
			}
			else
			{
				info.close();
				result = 0;
			}

			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoDclose(result, id);
			}

			delayIoOperation(timings[IoOperation.close]);

			return result;
		}

		/// <summary>
		/// sceIoRemove
		/// </summary>
		/// <param name="filename">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xF27A9C51, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoRemove(PspString filename)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;
			string pcfilename = getDeviceFilePath(filename.String);
			int result;

			string absoluteFileName = getAbsoluteFileName(filename.String);
			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = vfsManager.getVirtualFileSystem(absoluteFileName, localFileName);
			if (vfs != null)
			{
				result = vfs.ioRemove(localFileName.ToString());
			}
			else if (useVirtualFileSystem)
			{
				log.error(string.Format("sceIoRemove - device not found '{0}'", filename));
				result = ERROR_ERRNO_DEVICE_NOT_FOUND;
			}
			else if (!string.ReferenceEquals(pcfilename, null))
			{
				if (isUmdPath(pcfilename))
				{
					result = -1;
				}
				else
				{
					File file = new File(pcfilename);
					if (file.delete())
					{
						result = 0;
					}
					else
					{
						if (file.exists())
						{
							result = -1;
						}
						else
						{
							result = ERROR_ERRNO_FILE_NOT_FOUND;
						}
					}
				}
			}
			else
			{
				result = -1;
			}

			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoRemove(result, filename.Address, filename.String);
			}

			delayIoOperation(timings[IoOperation.remove]);

			return result;
		}

		/// <summary>
		/// Creates a directory.
		/// </summary>
		/// <param name="dirname">      - Name of the directory </param>
		/// <param name="permissions">  - 
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x06A70004, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoMkdir(PspString dirname, int permissions)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;
			string pcfilename = getDeviceFilePath(dirname.String);
			int result;

			string absoluteFileName = getAbsoluteFileName(dirname.String);
			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = vfsManager.getVirtualFileSystem(absoluteFileName, localFileName);
			if (vfs != null)
			{
				result = vfs.ioMkdir(localFileName.ToString(), permissions);
			}
			else if (useVirtualFileSystem)
			{
				log.error(string.Format("sceIoMkdir - device not found '{0}'", dirname));
				result = ERROR_ERRNO_DEVICE_NOT_FOUND;
			}
			else if (!string.ReferenceEquals(pcfilename, null))
			{
				File f = new File(pcfilename);
				f.mkdir();
				result = 0;
			}
			else
			{
				result = -1;
			}

			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoMkdir(result, dirname.Address, dirname.String, permissions);
			}

			delayIoOperation(timings[IoOperation.mkdir]);

			return result;
		}

		/// <summary>
		/// Removes a directory.
		/// </summary>
		/// <param name="dirname">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x1117C65F, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoRmdir(PspString dirname)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;
			string pcfilename = getDeviceFilePath(dirname.String);
			int result;

			string absoluteFileName = getAbsoluteFileName(dirname.String);
			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = vfsManager.getVirtualFileSystem(absoluteFileName, localFileName);
			if (vfs != null)
			{
				result = vfs.ioRmdir(localFileName.ToString());
			}
			else if (useVirtualFileSystem)
			{
				log.error(string.Format("sceIoRmdir - device not found '{0}'", dirname));
				result = ERROR_ERRNO_DEVICE_NOT_FOUND;
			}
			else if (!string.ReferenceEquals(pcfilename, null))
			{
				File f = new File(pcfilename);
				if (f.delete())
				{
					result = 0;
				}
				else
				{
					result = -1;
				}
			}
			else
			{
				result = -1;
			}

			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoRmdir(result, dirname.Address, dirname.String);
			}

			delayIoOperation(timings[IoOperation.remove]);

			return result;
		}

		/// <summary>
		/// Changes the current directory.
		/// </summary>
		/// <param name="path">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x55F4717D, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoChdir(PspString path)
		{
			int result;

			if (path.String.Equals(".."))
			{
				int index = filepath.LastIndexOf("/", StringComparison.Ordinal);
				if (index != -1)
				{
					filepath = filepath.Substring(0, index);
				}

				log.info("pspiofilemgr - filepath " + filepath + " (going up one level)");
				result = 0;
			}
			else
			{
				string pcfilename = getDeviceFilePath(path.String);
				if (!string.ReferenceEquals(pcfilename, null))
				{
					filepath = pcfilename;
					log.info("pspiofilemgr - filepath " + filepath);
					result = 0;
				}
				else
				{
					result = -1;
				}
			}
			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoChdir(result, path.Address, path.String);
			}
			return result;
		}

		/// <summary>
		/// sceIoSync
		/// </summary>
		/// <param name="devicename"> </param>
		/// <param name="flag">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xAB96437F, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoSync(PspString devicename, int flag)
		{
			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoSync(0, devicename.Address, devicename.String, flag);
			}

			return 0;
		}

		/// <summary>
		/// sceIoGetstat
		/// </summary>
		/// <param name="filename"> </param>
		/// <param name="stat_addr">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xACE946E8, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoGetstat(PspString filename, TPointer statAddr)
		{
			return hleIoGetstat(filename.Address, filename.String, statAddr);
		}

		/// <summary>
		/// sceIoChstat
		/// </summary>
		/// <param name="filename"> </param>
		/// <param name="statAddr"> </param>
		/// <param name="bits">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xB8A740F4, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoChstat(PspString filename, TPointer statAddr, int bits)
		{
			string pcfilename = getDeviceFilePath(filename.String);
			int result;

			SceIoStat stat = new SceIoStat();
			stat.read(statAddr);

			string absoluteFileName = getAbsoluteFileName(filename.String);
			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = vfsManager.getVirtualFileSystem(absoluteFileName, localFileName);
			if (vfs != null)
			{
				result = vfs.ioChstat(localFileName.ToString(), stat, bits);
			}
			else if (useVirtualFileSystem)
			{
				log.error(string.Format("sceIoChstat - device not found '{0}'", filename));
				result = ERROR_ERRNO_DEVICE_NOT_FOUND;
			}
			else if (!string.ReferenceEquals(pcfilename, null))
			{
				if (isUmdPath(pcfilename))
				{
					result = -1;
				}
				else
				{
					File file = new File(pcfilename);

					int mode = stat.mode;
					bool successful = true;

					if ((bits & 0x0001) != 0)
					{ // Others execute permission
						if (!file.setExecutable((mode & 0x0001) != 0))
						{
							// This always fails under Windows
							// successful = false;
						}
					}
					if ((bits & 0x0002) != 0)
					{ // Others write permission
						if (!file.setWritable((mode & 0x0002) != 0))
						{
							successful = false;
						}
					}
					if ((bits & 0x0004) != 0)
					{ // Others read permission
						if (!file.setReadable((mode & 0x0004) != 0))
						{
							// This always fails under Windows
							// successful = false;
						}
					}

					if ((bits & 0x0040) != 0)
					{ // User execute permission
						if (!file.setExecutable((mode & 0x0040) != 0, true))
						{
							// This always fails under Windows
							// successful = false;
						}
					}
					if ((bits & 0x0080) != 0)
					{ // User write permission
						if (!file.setWritable((mode & 0x0080) != 0, true))
						{
							successful = false;
						}
					}
					if ((bits & 0x0100) != 0)
					{ // User read permission
						if (!file.setReadable((mode & 0x0100) != 0, true))
						{
							// This always fails under Windows
							// successful = false;
						}
					}

					if (successful)
					{
						result = 0;
					}
					else
					{
						result = -1;
					}
				}
			}
			else
			{
				result = -1;
			}
			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoChstat(result, filename.Address, filename.String, statAddr.Address, bits);
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceIoChstat returning 0x{0:X8}", result));
			}

			return result;
		}

		/// <summary>
		/// sceIoRename
		/// </summary>
		/// <param name="oldfilename"> </param>
		/// <param name="newfilename">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x779103A0, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoRename(PspString pspOldFileName, PspString pspNewFileName)
		{
			return hleIoRename(pspOldFileName.Address, pspOldFileName.String, pspNewFileName.Address, pspNewFileName.String);
		}

		/// <summary>
		/// sceIoDevctl
		/// </summary>
		/// <param name="processor"> </param>
		/// <param name="devicename"> </param>
		/// <param name="cmd"> </param>
		/// <param name="indata_addr"> </param>
		/// <param name="inlen"> </param>
		/// <param name="outdata_addr">
		/// </param>
		/// <param name="outlen"> </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x54F5FB11, version = 150, checkInsideInterrupt = true) public int sceIoDevctl(pspsharp.HLE.PspString devicename, int cmd, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer indata, int inlen, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer outdata, int outlen)
		[HLEFunction(nid : 0x54F5FB11, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoDevctl(PspString devicename, int cmd, TPointer indata, int inlen, TPointer outdata, int outlen)
		{
			IDictionary<IoOperation, IoOperationTiming> timings = defaultTimings;
			Memory mem = Processor.memory;
			int result = -1;
			int indataAddr = indata.Address;
			int outdataAddr = outdata.Address;

			IVirtualFileSystem vfs = vfsManager.getVirtualFileSystem(devicename.String, null);
			if (vfs != null)
			{
				result = vfs.ioDevctl(devicename.String, cmd, indata, inlen, outdata, outlen);

				foreach (IIoListener ioListener in ioListeners)
				{
					ioListener.sceIoDevctl(result, devicename.Address, devicename.String, cmd, indataAddr, inlen, outdataAddr, outlen);
				}
				delayIoOperation(timings[IoOperation.iodevctl]);

				return result;
			}
			else if (useVirtualFileSystem)
			{
				log.error(string.Format("sceIoDevctl - device not found '{0}'", devicename));
				result = ERROR_ERRNO_DEVICE_NOT_FOUND;

				foreach (IIoListener ioListener in ioListeners)
				{
					ioListener.sceIoDevctl(result, devicename.Address, devicename.String, cmd, indataAddr, inlen, outdataAddr, outlen);
				}
				delayIoOperation(timings[IoOperation.iodevctl]);

				return result;
			}

			bool needDelayIoOperation = true;

			switch (cmd)
			{
				// Check disk region
				case 0x01E18030:
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceIoDevctl 0x{0:X8} check disk region", cmd));
					}
					if (inlen >= 16)
					{
						int unknown1 = mem.read32(indataAddr + 0);
						int unknown2 = mem.read32(indataAddr + 4);
						int unknown3 = mem.read32(indataAddr + 8);
						int unknown4 = mem.read32(indataAddr + 12);
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceIoDevctl 0x{0:X8} check disk region unknown1=0x{1:X}, unknown2=0x{2:X}, unknown3=0x{3:X}, unknown4=0x{4:X}", cmd, unknown1, unknown2, unknown3, unknown4));
						}
						// Return 0 if the disk region is not matching,
						// return 1 if the disk region is matching.
						result = 1;
					}
					else
					{
						result = -1;
					}
					break;
				// Get UMD disc type.
				case 0x01F20001:
				{
					if (log.DebugEnabled)
					{
						log.debug("sceIoDevctl " + string.Format("0x{0:X8}", cmd) + " get disc type");
					}
					if (Memory.isAddressGood(outdataAddr) && outlen >= 8)
					{
						// 0 = No disc.
						// 0x10 = Game disc.
						// 0x20 = Video disc.
						// 0x40 = Audio disc.
						// 0x80 = Cleaning disc.
						int @out;
						if (iso == null)
						{
							@out = 0; // No disc
						}
						else
						{
							@out = 0x10; // Return game disc by default

							// Retrieve the disc type from the UMD_DATA.BIN file
							IVirtualFileSystem vfsIso = new UmdIsoVirtualFileSystem(iso);
							IVirtualFile vfsUmdData = vfsIso.ioOpen("UMD_DATA.BIN", 0, PSP_O_RDONLY);
							if (vfsUmdData != null)
							{
								sbyte[] buffer = new sbyte[(int) vfsUmdData.length()];
								int length = vfsUmdData.ioRead(buffer, 0, buffer.Length);
								if (length > 0)
								{
									string umdData = StringHelper.NewString(buffer);
									string[] umdDataParts = umdData.Split("\\|", true);
									if (umdDataParts != null && umdDataParts.Length >= 4)
									{
										string umdType = umdDataParts[3];
										if (!string.ReferenceEquals(umdType, null) && umdType.Length > 0)
										{
											switch (umdType[0])
											{
												case 'G':
													@out = 0x10;
													break; // Game disc
													goto case 'V';
												case 'V':
													@out = 0x20;
													break; // Video disc
													goto default;
												default:
													log.warn(string.Format("Unknown disc type '{0}' in UMD_DATA.BIN", umdType));
													break;
											}
										}
									}
								}
								vfsUmdData.ioClose();
							}
						}
						mem.write32(outdataAddr + 4, @out);
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				// Get UMD current LBA.
				case 0x01F20002:
				{
					if (log.DebugEnabled)
					{
						log.debug("sceIoDevctl " + string.Format("0x{0:X8}", cmd) + " get current LBA");
					}
					if (Memory.isAddressGood(outdataAddr) && outlen >= 4)
					{
						mem.write32(outdataAddr, 0); // Assume first sector.
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				// Seek UMD disc (raw).
				case 0x01F100A3:
				{
					if (log.DebugEnabled)
					{
						log.debug("sceIoDevctl " + string.Format("0x{0:X8}", cmd) + " seek UMD disc");
					}
					if ((Memory.isAddressGood(indataAddr) && inlen >= 4))
					{
						int sector = mem.read32(indataAddr);
						if (log.DebugEnabled)
						{
							log.debug("sector=" + sector);
						}
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				// Prepare UMD data into cache.
				case 0x01F100A4:
				{
					if (log.DebugEnabled)
					{
						log.debug("sceIoDevctl " + string.Format("0x{0:X8}", cmd) + " prepare UMD data to cache");
					}
					if ((Memory.isAddressGood(indataAddr) && inlen >= 4))
					{
						// UMD cache read struct (16-bytes).
						int unk1 = mem.read32(indataAddr); // NULL.
						int sector = mem.read32(indataAddr + 4); // First sector of data to read.
						int unk2 = mem.read32(indataAddr + 8); // NULL.
						int sectorNum = mem.read32(indataAddr + 12); // Length of data to read.
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sector={0:D}, sectorNum={1:D}, unk1={2:D}, unk2={3:D}", sector, sectorNum, unk1, unk2));
						}
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				// Prepare UMD data into cache and get status.
				case 0x01F300A5:
				{
					if (log.DebugEnabled)
					{
						log.debug("sceIoDevctl " + string.Format("0x{0:X8}", cmd) + " prepare UMD data to cache and get status");
					}
					if ((Memory.isAddressGood(indataAddr) && inlen >= 4) && (Memory.isAddressGood(outdataAddr) && outlen >= 4))
					{
						// UMD cache read struct (16-bytes).
						int unk1 = mem.read32(indataAddr); // NULL.
						int sector = mem.read32(indataAddr + 4); // First sector of data to read.
						int unk2 = mem.read32(indataAddr + 8); // NULL.
						int sectorNum = mem.read32(indataAddr + 12); // Length of data to read.
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sector={0:D}, sectorNum={1:D}, unk1={2:D}, unk2={3:D}", sector, sectorNum, unk1, unk2));
						}
						mem.write32(outdataAddr, 1); // Status (unitary index of the requested read, greater or equal to 1).
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				// Wait for the UMD data cache thread.
				case 0x01F300A7:
				{
					if (log.DebugEnabled)
					{
						log.debug("sceIoDevctl " + string.Format("0x{0:X8}", cmd) + " wait for the UMD data cache thread");
					}
					if ((Memory.isAddressGood(indataAddr) && inlen >= 4))
					{
						int index = mem.read32(indataAddr); // Index set by command 0x01F300A5.
						if (log.DebugEnabled)
						{
							log.debug(string.Format("index={0:D}", index));
						}
						// Place the calling thread in wait state.

						// Disabled the following lines as long as the UMD data cache thread has not been implemented.
						// Otherwise nobody would wake-up the thread.
						//ThreadManForUser threadMan = Modules.ThreadManForUserModule;
						//SceKernelThreadInfo currentThread = threadMan.getCurrentThread();
						//threadMan.hleKernelThreadEnterWaitState(JPCSP_WAIT_IO, currentThread.wait.Io_id, ioWaitStateChecker, true);
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				// Poll the UMD data cache thread.
				case 0x01F300A8:
				{
					if (log.DebugEnabled)
					{
						log.debug("sceIoDevctl " + string.Format("0x{0:X8}", cmd) + " poll the UMD data cache thread");
					}
					if ((Memory.isAddressGood(indataAddr) && inlen >= 4))
					{
						int index = mem.read32(indataAddr); // Index set by command 0x01F300A5.
						if (log.DebugEnabled)
						{
							log.debug(string.Format("index={0:D}", index));
						}
						// 0 - UMD data cache thread has finished.
						// 0x10 - UMD data cache thread is waiting.
						// 0x20 - UMD data cache thread is running.
						result = 0; // Return finished.
					}
					else
					{
						result = -1;
					}
					break;
				}
				// Cancel the UMD data cache thread.
				case 0x01F300A9:
				{
					if (log.DebugEnabled)
					{
						log.debug("sceIoDevctl " + string.Format("0x{0:X8}", cmd) + " cancel the UMD data cache thread");
					}
					if ((Memory.isAddressGood(indataAddr) && inlen >= 4))
					{
						int index = mem.read32(indataAddr); // Index set by command 0x01F300A5.
						if (log.DebugEnabled)
						{
							log.debug(string.Format("index={0:D}", index));
						}
						// Wake up the thread waiting for the UMD data cache handling.
						ThreadManForUser threadMan = Modules.ThreadManForUserModule;
						for (IEnumerator<SceKernelThreadInfo> it = threadMan.GetEnumerator(); it.MoveNext();)
						{
							SceKernelThreadInfo thread = it.Current;
							if (thread.isWaitingForType(JPCSP_WAIT_IO))
							{
								thread.cpuContext._v0 = SceKernelErrors.ERROR_KERNEL_WAIT_CANCELLED;
								threadMan.hleKernelWakeupThread(thread);
							}
						}
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				// Check the MemoryStick's driver status (mscmhc0).
				case 0x02025801:
				{
					log.debug("sceIoDevctl " + string.Format("0x{0:X8}", cmd) + " check ms driver status");
					if (!devicename.String.Equals("mscmhc0:"))
					{
						result = ERROR_KERNEL_UNSUPPORTED_OPERATION;
					}
					else if (Memory.isAddressGood(outdataAddr))
					{
						// 0 = Driver busy.
						// 4 = Driver ready.
						mem.write32(outdataAddr, 4);
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				// Register MemoryStick's insert/eject callback (mscmhc0).
				case 0x02015804:
				{
					log.debug("sceIoDevctl register memorystick insert/eject callback (mscmhc0)");
					ThreadManForUser threadMan = Modules.ThreadManForUserModule;
					if (!devicename.String.Equals("mscmhc0:"))
					{
						result = ERROR_KERNEL_UNSUPPORTED_OPERATION;
					}
					else if (Memory.isAddressGood(indataAddr) && inlen == 4)
					{
						int cbid = mem.read32(indataAddr);
						const int callbackType = SceKernelThreadInfo.THREAD_CALLBACK_MEMORYSTICK;
						if (threadMan.hleKernelRegisterCallback(callbackType, cbid))
						{
							// Trigger the registered callback immediately.
							threadMan.hleKernelNotifyCallback(callbackType, cbid, MemoryStick.StateMs);
							result = 0; // Success.
						}
						else
						{
							result = SceKernelErrors.ERROR_MEMSTICK_DEVCTL_TOO_MANY_CALLBACKS;
						}
					}
					else
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					break;
				}
				// Unregister MemoryStick's insert/eject callback (mscmhc0).
				case 0x02015805:
				{
					log.debug("sceIoDevctl unregister memorystick insert/eject callback (mscmhc0)");
					ThreadManForUser threadMan = Modules.ThreadManForUserModule;
					if (!devicename.String.Equals("mscmhc0:"))
					{
						result = ERROR_KERNEL_UNSUPPORTED_OPERATION;
					}
					else if (Memory.isAddressGood(indataAddr) && inlen == 4)
					{
						int cbid = mem.read32(indataAddr);
						if (threadMan.hleKernelUnRegisterCallback(SceKernelThreadInfo.THREAD_CALLBACK_MEMORYSTICK, cbid))
						{
							result = 0; // Success.
						}
						else
						{
							result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS; // No such callback.
						}
					}
					else
					{
						result = -1; // Invalid parameters.
					}
					break;
				}
				// ???
				case 0x02015807:
				{
					log.debug("sceIoDevctl ??? (mscmhc0)");
					if (!devicename.String.Equals("mscmhc0:"))
					{
						result = ERROR_KERNEL_UNSUPPORTED_OPERATION;
					}
					else if (Memory.isAddressGood(outdataAddr) && outlen == 4)
					{
						mem.write32(outdataAddr, 0); // Unknown value: seems to be 0 or 1?
						result = 0; // Success.
					}
					else
					{
						result = -1; // Invalid parameters.
					}
					break;
				}
				// ???
				case 0x0201580B:
				{
					log.debug("sceIoDevctl ??? (mscmhc0)");
					if (!devicename.String.Equals("mscmhc0:"))
					{
						result = ERROR_KERNEL_UNSUPPORTED_OPERATION;
					}
					else if (Memory.isAddressGood(indataAddr) && inlen == 20)
					{
						result = 0; // Success.
					}
					else
					{
						result = -1; // Invalid parameters.
					}
					break;
				}
				// Check if the device is inserted (mscmhc0).
				case 0x02025806:
				{
					log.debug("sceIoDevctl check ms inserted (mscmhc0)");
					if (!devicename.String.Equals("mscmhc0:"))
					{
						result = ERROR_KERNEL_UNSUPPORTED_OPERATION;
					}
					else if (Memory.isAddressGood(outdataAddr))
					{
						// 0 = Not inserted.
						// 1 = Inserted.
						mem.write32(outdataAddr, 1);
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				// ???
				case 0x0202580A:
				{
					log.debug("sceIoDevctl ??? (mscmhc0)");
					if (!devicename.String.Equals("mscmhc0:"))
					{
						result = ERROR_KERNEL_UNSUPPORTED_OPERATION;
					}
					else if (Memory.isAddressGood(outdataAddr) && outlen == 16)
					{
						// When value1 or value2 are < 10000, sceUtilitySavedata is
						// returning an error 0x8011032C (bad status).
						// When value1 or value2 are > 10000, sceUtilitySavedata is
						// returning an error 0x8011032A (the system has been shifted to sleep mode).
						const int value1 = 10000;
						const int value2 = 10000;
						// When value3 or value4 are < 10000, sceUtilitySavedata is
						// returning an error 0x8011032C (bad status)
						// When value3 or value4 are > 10000, sceUtilitySavedata is
						// returning an error 0x80110322 (the memory stick has been removed).
						const int value3 = 10000;
						const int value4 = 10000;
						// No error is returned by sceUtilitySavedata only when
						// all 4 values are set to 10000.

						mem.write32(outdataAddr + 0, value1);
						mem.write32(outdataAddr + 4, value2);
						mem.write32(outdataAddr + 8, value3);
						mem.write32(outdataAddr + 12, value4);
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				// Register memorystick insert/eject callback (fatms0).
				case 0x02415821:
				{
					log.debug("sceIoDevctl register memorystick insert/eject callback (fatms0)");
					needDelayIoOperation = false;
					ThreadManForUser threadMan = Modules.ThreadManForUserModule;
					if (!devicename.String.Equals("fatms0:"))
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					else if (Memory.isAddressGood(indataAddr) && inlen == 4)
					{
						int cbid = mem.read32(indataAddr);
						const int callbackType = SceKernelThreadInfo.THREAD_CALLBACK_MEMORYSTICK_FAT;
						if (threadMan.hleKernelRegisterCallback(callbackType, cbid))
						{
							// Trigger the registered callback immediately.
							// Only trigger this one callback, not all the MS callbacks.
							threadMan.hleKernelNotifyCallback(callbackType, cbid, MemoryStick.StateFatMs);
							result = 0; // Success.
						}
						else
						{
							result = SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
						}
					}
					else
					{
						result = SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				// Unregister memorystick insert/eject callback (fatms0).
				case 0x02415822:
				{
					log.debug("sceIoDevctl unregister memorystick insert/eject callback (fatms0)");
					needDelayIoOperation = false;
					ThreadManForUser threadMan = Modules.ThreadManForUserModule;
					if (!devicename.String.Equals("fatms0:"))
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					else if (Memory.isAddressGood(indataAddr) && inlen == 4)
					{
						int cbid = mem.read32(indataAddr);
						threadMan.hleKernelUnRegisterCallback(SceKernelThreadInfo.THREAD_CALLBACK_MEMORYSTICK_FAT, cbid);
						result = 0; // Success.
					}
					else
					{
						result = SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				// Set if the device is assigned/inserted or not (fatms0).
				case 0x02415823:
				{
					log.debug("sceIoDevctl set assigned device (fatms0)");
					if (!devicename.String.Equals("fatms0:"))
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					else if (Memory.isAddressGood(indataAddr) && inlen >= 4)
					{
						// 0 - Device is not assigned (callback not registered).
						// 1 - Device is assigned (callback registered).
						MemoryStick.StateFatMs = mem.read32(indataAddr);
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				case 0x02415857:
				{
					if (!devicename.String.Equals("ms0:"))
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					else if (Memory.isAddressGood(outdataAddr) && outlen == 4)
					{
						mem.write32(outdataAddr, 0); // Unknown value
						result = 0;
					}
					else
					{
						result = SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				// Check if the device is write protected (fatms0).
				case 0x02425824:
				{
					log.debug("sceIoDevctl check write protection (fatms0)");
					if (!devicename.String.Equals("fatms0:") && !devicename.String.Equals("ms0:"))
					{ // For this command the alias "ms0:" is also supported.
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					else if (Memory.isAddressGood(outdataAddr))
					{
						// 0 - Device is not protected.
						// 1 - Device is protected.
						mem.write32(outdataAddr, 0);
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				// Get MS capacity (fatms0).
				case 0x02425818:
				{
					log.debug("sceIoDevctl get MS capacity (fatms0)");
					int sectorSize = 0x200;
					int sectorCount = MemoryStick.SectorSize / sectorSize;
					int maxClusters = (int)(MemoryStick.FreeSize / (sectorSize * sectorCount));
					int freeClusters = maxClusters;
					int maxSectors = maxClusters;
					if (Memory.isAddressGood(indataAddr) && inlen >= 4)
					{
						int addr = mem.read32(indataAddr);
						if (Memory.isAddressGood(addr))
						{
							log.debug("sceIoDevctl refer ms free space");
							mem.write32(addr, maxClusters);
							mem.write32(addr + 4, freeClusters);
							mem.write32(addr + 8, maxSectors);
							mem.write32(addr + 12, sectorSize);
							mem.write32(addr + 16, sectorCount);
							result = 0;
						}
						else
						{
							log.warn("sceIoDevctl 0x02425818 bad save address " + string.Format("0x{0:X8}", addr));
							result = -1;
						}
					}
					else
					{
						log.warn("sceIoDevctl 0x02425818 bad param address " + string.Format("0x{0:X8}", indataAddr) + " or size " + inlen);
						result = -1;
					}
					break;
				}
				// Check if the device is assigned/inserted (fatms0).
				case 0x02425823:
				{
					log.debug("sceIoDevctl check assigned device (fatms0)");
					needDelayIoOperation = false;
					if (!devicename.String.Equals("fatms0:"))
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					else if (Memory.isAddressGood(outdataAddr) && outlen >= 4)
					{
						// 0 - Device is not assigned (callback not registered).
						// 1 - Device is assigned (callback registered).
						mem.write32(outdataAddr, MemoryStick.StateFatMs);
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				// Register USB thread.
				case 0x03415001:
				{
					log.debug("sceIoDevctl register usb thread");
					if (Memory.isAddressGood(indataAddr) && inlen >= 4)
					{
						// Unknown params.
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				// Unregister USB thread.
				case 0x03415002:
				{
					log.debug("sceIoDevctl unregister usb thread");
					if (Memory.isAddressGood(indataAddr) && inlen >= 4)
					{
						// Unknown params.
						result = 0;
					}
					else
					{
						result = -1;
					}
					break;
				}
				case 0x02425856:
				{
					if (Memory.isAddressGood(indataAddr) && inlen >= 4)
					{
						// This is the value contained in the registry entry
						//	"/CONFIG/SYSTEM/CHARACTER_SET/oem"
						int characterSet = mem.read32(indataAddr);
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceIoDevctl '{0}' set character set to 0x{1:X}", devicename.String, characterSet));
						}
						result = 0;
					}
					break;
				}
				case 0x02415830:
				{
					if (Memory.isAddressGood(indataAddr) && inlen >= 8)
					{
						int oldFileNameAddr = mem.read32(indataAddr);
						int newFileNameAddr = mem.read32(indataAddr + 4);
						string oldFileName = Utilities.readStringZ(mem, oldFileNameAddr);
						string newFileName = Utilities.readStringZ(mem, newFileNameAddr);

						result = hleIoRename(oldFileNameAddr, devicename.String + oldFileName, newFileNameAddr, devicename.String + newFileName);

						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceIoDevctl file rename oldFileName='{0}', newFileName='{1}', result=0x{2:X}", oldFileName, newFileName, result));
						}
					}
					break;
				}
				case 0x00005802:
				{
					if (!devicename.String.Equals("flash1:"))
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					else
					{
						result = 0;
					}
					break;
				}
				// Check if LoadExec is allowed on the device
				case 0x00208813:
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Checking if LoadExec is allowed on '{0}'", devicename));
					}
					// Result == 0: LoadExec allowed
					// Result != 0: LoadExec prohibited
					result = 0;
					break;
				}
				default:
					log.warn(string.Format("sceIoDevctl 0x{0:X8} unknown command", cmd));
					if (Memory.isAddressGood(indataAddr))
					{
						log.warn(string.Format("sceIoDevctl indata: {0}", Utilities.getMemoryDump(indataAddr, inlen)));
					}
					result = SceKernelErrors.ERROR_ERRNO_INVALID_IODEVCTL_CMD;
					break;
			}
			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoDevctl(result, devicename.Address, devicename.String, cmd, indataAddr, inlen, outdataAddr, outlen);
			}

			if (needDelayIoOperation)
			{
				delayIoOperation(timings[IoOperation.iodevctl]);
			}

			return result;
		}

		/// <summary>
		/// sceIoGetDevType
		/// </summary>
		/// <param name="id">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0x08BD7374, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoGetDevType(int id)
		{
			int result;

			if (id == STDIN_ID || id == STDOUT_ID || id == STDERR_ID)
			{
				result = PSP_DEV_TYPE_FILESYSTEM;
			}
			else
			{
				IoInfo info = fileIds[id];
				if (info == null)
				{
					log.warn("sceIoGetDevType - unknown id " + id.ToString("x"));
					result = ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
				}
				else
				{
					// For now, return alias type, since it's the most used.
					result = PSP_DEV_TYPE_ALIAS;
				}
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceIoGetDevType id=0x{0:X} returning 0x{1:X}", id, result));
			}

			return result;
		}

		/// <summary>
		/// sceIoAssign: mounts physicalDev on filesystemDev and sets an alias to represent it.
		/// </summary>
		/// <param name="alias"> </param>
		/// <param name="physicalDev"> </param>
		/// <param name="filesystemDev"> </param>
		/// <param name="mode"> 0 IOASSIGN_RDWR
		///             1 IOASSIGN_RDONLY </param>
		/// <param name="arg_addr"> </param>
		/// <param name="argSize">
		/// 
		/// @return </param>
		[HLELogging(level:"warn"), HLEFunction(nid : 0xB2A628C1, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoAssign(PspString alias, PspString physicalDev, PspString filesystemDev, int mode, int arg_addr, int argSize)
		{
			int result = 0;

			// Do not assign "disc0:".
			// Example from "Ridge Racer UCES00002":
			//   sceIoAssign alias=0x0899F1E0('disc0:'), physicalDev=0x0899F1D0('umd0:'), filesystemDev=0x0899F1D8('isofs0:'), mode=0x1, arg_addr=0x0, argSize=0x0
			if (!alias.String.Equals("disc0:"))
			{
				assignedDevices[alias.String.Replace(":", "")] = filesystemDev.String.Replace(":", "");
			}

			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoAssign(result, alias.Address, alias.String, physicalDev.Address, physicalDev.String, filesystemDev.Address, filesystemDev.String, mode, arg_addr, argSize);
			}

			return result;
		}

		/// <summary>
		/// sceIoUnassign
		/// </summary>
		/// <param name="alias">
		/// 
		/// @return </param>
		[HLELogging(level:"warn"), HLEFunction(nid : 0x6D08A871, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoUnassign(PspString alias)
		{
			assignedDevices.Remove(alias.String.Replace(":", ""));

			return 0;
		}

		/// <summary>
		/// sceIoCancel
		/// </summary>
		/// <param name="id">
		/// 
		/// @return </param>
		[HLEFunction(nid : 0xE8BC6571, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoCancel(int id)
		{
			IoInfo info = fileIds[id];
			int result;

			if (info == null)
			{
				log.warn("sceIoCancel - unknown id " + id.ToString("x"));
				result = ERROR_KERNEL_BAD_FILE_DESCRIPTOR;
			}
			else
			{
				info.closePending = true;
				result = 0;
			}

			foreach (IIoListener ioListener in ioListeners)
			{
				ioListener.sceIoCancel(result, id);
			}

			return result;
		}

		/// <summary>
		/// sceIoGetFdList
		/// </summary>
		/// <param name="outAddr"> </param>
		/// <param name="outSize"> </param>
		/// <param name="fdNumAddr">
		/// 
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x5C2BE2CC, version = 150, checkInsideInterrupt = true) public int sceIoGetFdList(@CanBeNull pspsharp.HLE.TPointer32 outAddr, int outSize, @CanBeNull pspsharp.HLE.TPointer32 fdNumAddr)
		[HLEFunction(nid : 0x5C2BE2CC, version : 150, checkInsideInterrupt : true)]
		public virtual int sceIoGetFdList(TPointer32 outAddr, int outSize, TPointer32 fdNumAddr)
		{
			int count = 0;
			if (outAddr.NotNull && outSize > 0)
			{
				int offset = 0;
				foreach (int? fd in fileIds.Keys)
				{
					if (offset >= outSize)
					{
						break;
					}
					outAddr.setValue(offset, fd.Value);
					offset += 4;
				}
				count = offset / 4;
			}

			// Return the total number of files open
			fdNumAddr.setValue(fileIds.Count);

			return count;
		}

		/// <summary>
		/// Reopens an existing file descriptor.
		/// </summary>
		/// <param name="filename">    the new file to open. </param>
		/// <param name="flags">       the open flags. </param>
		/// <param name="permissions"> the open mode. </param>
		/// <param name="id">          the old file descriptor to reopen. </param>
		/// <returns>            < 0 on error, otherwise the reopened file descriptor. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3C54E908, version = 150) public int sceIoReopen(pspsharp.HLE.PspString filename, int flags, int permissions, int id)
		[HLEFunction(nid : 0x3C54E908, version : 150)]
		public virtual int sceIoReopen(PspString filename, int flags, int permissions, int id)
		{
			return -1;
		}
	}
}