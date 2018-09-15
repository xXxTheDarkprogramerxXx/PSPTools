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
namespace pspsharp.memory.mmio
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.Modules.sceMSstorModule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_MSCM0_INTR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_SEEK_SET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickBootAttributesInfo.MS_SYSINF_CARDTYPE_RDWR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickBootAttributesInfo.MS_SYSINF_FORMAT_FAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickBootAttributesInfo.MS_SYSINF_MSCLASS_TYPE_1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickBootAttributesInfo.MS_SYSINF_USAGE_GENERAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickBootHeader.MS_BOOT_BLOCK_DATA_ENTRIES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickBootHeader.MS_BOOT_BLOCK_FORMAT_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickBootHeader.MS_BOOT_BLOCK_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickProAttributeEntry.MSPRO_BLOCK_ID_DEVINFO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickProAttributeEntry.MSPRO_BLOCK_ID_MBR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickProAttributeEntry.MSPRO_BLOCK_ID_PBR32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickProAttributeEntry.MSPRO_BLOCK_ID_SYSINFO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickSysInfo.MEMORY_STICK_CLASS_PRO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickSystemItem.MS_SYSENT_TYPE_CIS_IDI;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.memorystick.MemoryStickSystemItem.MS_SYSENT_TYPE_INVALID_BLOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.endianSwap16;


	//using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using TPointer = pspsharp.HLE.TPointer;
	using pspAbstractMemoryMappedStructure = pspsharp.HLE.kernel.types.pspAbstractMemoryMappedStructure;
	using sceMScm = pspsharp.HLE.modules.sceMScm;
	using MemoryStick = pspsharp.hardware.MemoryStick;
	using MemoryStickBootPage = pspsharp.memory.mmio.memorystick.MemoryStickBootPage;
	using MemoryStickDeviceInfo = pspsharp.memory.mmio.memorystick.MemoryStickDeviceInfo;
	using MemoryStickMbr = pspsharp.memory.mmio.memorystick.MemoryStickMbr;
	using MemoryStickPbr32 = pspsharp.memory.mmio.memorystick.MemoryStickPbr32;
	using MemoryStickProAttribute = pspsharp.memory.mmio.memorystick.MemoryStickProAttribute;
	using MemoryStickSysInfo = pspsharp.memory.mmio.memorystick.MemoryStickSysInfo;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// MMIO for Memory Stick.
	/// Based on information from
	///     https://github.com/torvalds/linux/tree/master/drivers/memstick/core
	/// and https://github.com/torvalds/linux/blob/master/drivers/usb/storage/ene_ub6250.c
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class MMIOHandlerMemoryStick : MMIOHandlerBase
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			pageBufferMemory = new IntArrayMemory(pageBuffer);
			pageBufferPointer = pageBufferMemory.Pointer;
			bootPageMemory = new IntArrayMemory(new int[bootPage.@sizeof() >> 2]);
			bootPageBackupMemory = new IntArrayMemory(new int[bootPageBackup.@sizeof() >> 2]);
		}

		public static new Logger log = sceMScm.log;
		private const int STATE_VERSION = 0;
		private const bool simulateMemoryStickPro = true;
		// Overwrite area
		public const int MS_REG_OVR_BKST = 0x80; // Block status
		public const int MS_REG_OVR_BKST_OK = MS_REG_OVR_BKST; // Block status OK
		public const int MS_REG_OVR_BKST_NG = 0; // Block status NG
		public const int MS_REG_OVR_PGST0 = 0x40; // Page status, bit 0
		public const int MS_REG_OVR_PGST1 = 0x20; // Page status, bit 1
		public const int MS_REG_OVR_PGST_MASK = MS_REG_OVR_PGST0 | MS_REG_OVR_PGST1;
		public const int MS_REG_OVR_PGST_OK = MS_REG_OVR_PGST0 | MS_REG_OVR_PGST1; // Page status OK
		public const int MS_REG_OVR_PGST_NG = MS_REG_OVR_PGST1; // Page status NG
		public const int MS_REG_OVR_PGST_DATA_ERROR = 0; // Page status data error
		public const int MS_REG_OVR_UDST = 0x10; // Update status
		public const int MS_REG_OVR_UDST_UPDATING = 0; // Update status updating
		public const int MS_REG_OVR_UDST_NO_UPDATE = MS_REG_OVR_UDST; // Update status no update
		public const int MS_REG_OVR_RESERVED = 0x08;
		public const int MS_REG_OVR_DEFAULT = MS_REG_OVR_BKST_OK | MS_REG_OVR_PGST_OK | MS_REG_OVR_UDST_NO_UPDATE | MS_REG_OVR_RESERVED;
		// Management flag
		public const int MS_REG_MNG_SCMS0 = 0x20; // Serial copy management system, bit 0
		public const int MS_REG_MNG_SCMS1 = 0x10; // Serial copy management system, bit 1
		public const int MS_REG_MNG_SCMS_MASK = MS_REG_MNG_SCMS0 | MS_REG_MNG_SCMS1;
		public const int MS_REG_MNG_SCMS_COPY_OK = MS_REG_MNG_SCMS0 | MS_REG_MNG_SCMS1;
		public const int MS_REG_MNG_SCMS_ONE_COPY = MS_REG_MNG_SCMS1;
		public const int MS_REG_MNG_SCMS_NO_COPY = 0;
		public const int MS_REG_MNG_ATFLG = 0x08; // Address transfer table flag
		public const int MS_REG_MNG_ATFLG_OTHER = MS_REG_MNG_ATFLG; // Address transfer other
		public const int MS_REG_MNG_ATFLG_ATTBL = 0; // Address transfer table
		public const int MS_REG_MNG_SYSFLG = 0x04; // System flag
		public const int MS_REG_MNG_SYSFLG_USER = MS_REG_MNG_SYSFLG; // User block
		public const int MS_REG_MNG_SYSFLG_BOOT = 0; // System block
		public const int MS_REG_MNG_RESERVED = 0xC3;
		public const int MS_REG_MNG_DEFAULT = MS_REG_MNG_SCMS_COPY_OK | MS_REG_MNG_ATFLG_OTHER | MS_REG_MNG_SYSFLG_USER | MS_REG_MNG_RESERVED;
		// commandState bit
		public const int MS_COMMANDSTATE_BUSY = 0x0001;
		// SYS bit
		public const int MS_SYS_RESET = 0x8000;
		// STATUS bit
		public const int MS_STATUS_TIMEOUT = 0x0100;
		public const int MS_STATUS_CRC_ERROR = 0x0200;
		public const int MS_STATUS_READY = 0x1000;
		public const int MS_STATUS_UNKNOWN = 0x2000;
		public const int MS_STATUS_FIFO_RW = 0x4000;
		// MS TPC code
		public const int MS_TPC_READ_PAGE_DATA = 0x2;
		public const int MS_TPC_READ_REG = 0x4;
		public const int MS_TPC_GET_INT = 0x7;
		public const int MS_TPC_SET_RW_REG_ADDRESS = 0x8;
		public const int MS_TPC_EX_SET_CMD = 0x9;
		public const int MS_TPC_WRITE_REG = 0xB;
		public const int MS_TPC_WRITE_PAGE_DATA = 0xD;
		public const int MS_TPC_SET_CMD = 0xE;
		// MS INT (register #1)
		public const int MS_INT_REG_ADDRESS = 0x01;
		public const int MS_INT_REG_CMDNK = 0x10;
		public const int MS_INT_REG_BREAK = 0x20;
		public const int MS_INT_REG_ERR = 0x40;
		public const int MS_INT_REG_CED = 0x80;
		// MS Status (register #2)
		public const int MS_STATUS_REG_ADDRESS = 0x02;
		public const int MS_STATUS_REG_READONLY = 0x01;
		// MS Type (register #4)
		public const int MS_TYPE_ADDRESS = 0x04;
		public const int MS_TYPE_MEMORY_STICK_PRO = 0x01;
		// MS System parameter (register #16)
		public const int MS_SYSTEM_ADDRESS = 0x10;
		public const int MS_SYSTEM_SERIAL_MODE = 0x80;
		// MS commands
		public const int MS_CMD_BLOCK_END = 0x33;
		public const int MS_CMD_RESET = 0x3C;
		public const int MS_CMD_BLOCK_WRITE = 0x55;
		public const int MS_CMD_SLEEP = 0x5A;
		public const int MS_CMD_LOAD_ID = 0x60;
		public const int MS_CMD_CMP_ICV = 0x7F;
		public const int MS_CMD_BLOCK_ERASE = 0x99;
		public const int MS_CMD_BLOCK_READ = 0xAA;
		public const int MS_CMD_CLEAR_BUF = 0xC3;
		public const int MS_CMD_FLASH_STOP = 0xCC;
		// MS Pro commands
		public const int MSPRO_CMD_FORMAT = 0x10;
		public const int MSPRO_CMD_SLEEP = 0x11;
		public const int MSPRO_CMD_WAKEUP = 0x12;
		public const int MSPRO_CMD_READ_DATA = 0x20;
		public const int MSPRO_CMD_WRITE_DATA = 0x21;
		public const int MSPRO_CMD_READ_ATRB = 0x24;
		public const int MSPRO_CMD_STOP = 0x25;
		public const int MSPRO_CMD_ERASE = 0x26;
		public const int MSPRO_CMD_READ_QUAD = 0x27;
		public const int MSPRO_CMD_WRITE_QUAD = 0x28;
		public const int MSPRO_CMD_SET_IBD = 0x46;
		public const int MSPRO_CMD_GET_IBD = 0x47;
		public const int MSPRO_CMD_IN_IO_DATA = 0xB0;
		public const int MSPRO_CMD_OUT_IO_DATA = 0xB1;
		public const int MSPRO_CMD_READ_IO_ATRB = 0xB2;
		public const int MSPRO_CMD_IN_IO_FIFO = 0xB3;
		public const int MSPRO_CMD_OUT_IO_FIFO = 0xB4;
		public const int MSPRO_CMD_IN_IOM = 0xB5;
		public const int MSPRO_CMD_OUT_IOM = 0xB6;
		private int interrupt; // Possible bits: 0x000D
		private int commandState;
		private int unk08; // Possible bits: 0x03F0
		private int tpc;
		private int status;
		private int sys;
		private readonly int[] registers = new int[256];
		private int readAddress;
		private int readSize;
		private int writeAddress;
		private int writeSize;
		private int tpcExSetCmdIndex;
		private int cmd;
		private int oobLength;
		private int startBlock;
		private int oobIndex;
		private readonly int[] pageBuffer = new int[(PAGE_SIZE) >> 2];
		private IntArrayMemory pageBufferMemory;
		private TPointer pageBufferPointer;
		private int pageLba;
		private int numberOfPages;
		private int pageDataIndex;
		private int pageIndex;
		private int commandDataIndex;
		private readonly MemoryStickBootPage bootPage = new MemoryStickBootPage();
		private IntArrayMemory bootPageMemory;
		private readonly MemoryStickBootPage bootPageBackup = new MemoryStickBootPage();
		private IntArrayMemory bootPageBackupMemory;
		private readonly IntArrayMemory disabledBlocksPageMemory = new IntArrayMemory(new int[PAGE_SIZE >> 2]);
		private readonly MemoryStickProAttribute msproAttribute = new MemoryStickProAttribute();
		private readonly IntArrayMemory msproAttributeMemory = new IntArrayMemory(new int[(2 * PAGE_SIZE) >> 2]);
		private const int PAGE_SIZE = 0x200;
		private const int DISABLED_BLOCKS_PAGE = 1;
		private const int CIS_IDI_PAGE = 2;
		private static readonly long MAX_MEMORY_STICK_SIZE = 128L * 1024 * 1024; // The maximum size of a Memory Stick (i.e. non-PRO) is 128MB
		private int PAGES_PER_BLOCK;
		private int BLOCK_SIZE;
		private int NUMBER_OF_PHYSICAL_BLOCKS;
		private int NUMBER_OF_LOGICAL_BLOCKS;
		private int FIRST_PAGE_LBA;
		private int NUMBER_OF_PAGES;

		public MMIOHandlerMemoryStick(int baseAddress) : base(baseAddress)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}

			sceMSstorModule.hleInit();
			reset();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			interrupt = stream.readInt();
			commandState = stream.readInt();
			unk08 = stream.readInt();
			tpc = stream.readInt();
			status = stream.readInt();
			sys = stream.readInt();
			stream.readInts(registers);
			readAddress = stream.readInt();
			readSize = stream.readInt();
			writeAddress = stream.readInt();
			writeSize = stream.readInt();
			cmd = stream.readInt();
			oobLength = stream.readInt();
			startBlock = stream.readInt();
			oobIndex = stream.readInt();
			stream.readInts(pageBuffer);
			pageLba = stream.readInt();
			numberOfPages = stream.readInt();
			pageDataIndex = stream.readInt();
			pageIndex = stream.readInt();
			commandDataIndex = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(interrupt);
			stream.writeInt(commandState);
			stream.writeInt(unk08);
			stream.writeInt(tpc);
			stream.writeInt(status);
			stream.writeInt(sys);
			stream.writeInts(registers);
			stream.writeInt(readAddress);
			stream.writeInt(readSize);
			stream.writeInt(writeAddress);
			stream.writeInt(writeSize);
			stream.writeInt(cmd);
			stream.writeInt(oobLength);
			stream.writeInt(startBlock);
			stream.writeInt(oobIndex);
			stream.writeInts(pageBuffer);
			stream.writeInt(pageLba);
			stream.writeInt(numberOfPages);
			stream.writeInt(pageDataIndex);
			stream.writeInt(pageIndex);
			stream.writeInt(commandDataIndex);
			base.write(stream);
		}

		private static string getTPCName(int tpc)
		{
			switch (tpc)
			{
				case MS_TPC_READ_PAGE_DATA :
					return "READ_PAGE_DATA";
				case MS_TPC_READ_REG :
					return "READ_REG";
				case MS_TPC_GET_INT :
					return "GET_INT";
				case MS_TPC_SET_RW_REG_ADDRESS:
					return "SET_RW_REG_ADDRESS";
				case MS_TPC_EX_SET_CMD :
					return "EX_SET_CMD";
				case MS_TPC_WRITE_REG :
					return "WRITE_REG";
				case MS_TPC_WRITE_PAGE_DATA :
					return "WRITE_PAGE_DATA";
				case MS_TPC_SET_CMD :
					return "SET_CMD";
			}

			return string.Format("UNKNOWN_TPC_{0:X}", tpc);
		}

		private static string getCommandName(int cmd)
		{
			switch (cmd)
			{
				case MS_CMD_BLOCK_END :
					return "BLOCK_END";
				case MS_CMD_RESET :
					return "RESET";
				case MS_CMD_BLOCK_WRITE :
					return "BLOCK_WRITE";
				case MS_CMD_SLEEP :
					return "SLEEP";
				case MS_CMD_LOAD_ID :
					return "LOAD_ID";
				case MS_CMD_CMP_ICV :
					return "CMP_ICV";
				case MS_CMD_BLOCK_ERASE :
					return "BLOCK_ERASE";
				case MS_CMD_BLOCK_READ :
					return "BLOCK_READ";
				case MS_CMD_CLEAR_BUF :
					return "CLEAR_BUF";
				case MS_CMD_FLASH_STOP :
					return "FLASH_STOP";
				case MSPRO_CMD_FORMAT :
					return "FORMAT";
				case MSPRO_CMD_SLEEP :
					return "MSPRO_SLEEP";
				case MSPRO_CMD_WAKEUP :
					return "WAKEUP";
				case MSPRO_CMD_READ_DATA :
					return "READ_DATA";
				case MSPRO_CMD_WRITE_DATA :
					return "WRITE_DATA";
				case MSPRO_CMD_READ_ATRB :
					return "READ_ATRB";
				case MSPRO_CMD_STOP :
					return "STOP";
				case MSPRO_CMD_ERASE :
					return "ERASE";
				case MSPRO_CMD_READ_QUAD :
					return "READ_QUAD";
				case MSPRO_CMD_WRITE_QUAD :
					return "WRITE_QUAD";
				case MSPRO_CMD_SET_IBD :
					return "SET_IBD";
				case MSPRO_CMD_GET_IBD :
					return "GET_IBD";
				case MSPRO_CMD_IN_IO_DATA :
					return "IN_IO_DATA";
				case MSPRO_CMD_OUT_IO_DATA :
					return "OUT_IO_DATA";
				case MSPRO_CMD_READ_IO_ATRB:
					return "READ_IO_ATRB";
				case MSPRO_CMD_IN_IO_FIFO :
					return "IN_IO_FIFO";
				case MSPRO_CMD_OUT_IO_FIFO :
					return "OUT_IO_FIFO";
				case MSPRO_CMD_IN_IOM :
					return "IN_IOM";
				case MSPRO_CMD_OUT_IOM :
					return "OUT_IOM";
			}

			return string.Format("UNKNOWN_CMD_{0:X}", cmd);
		}

		private void reset()
		{
			long totalSize = MemoryStick.TotalSize;

			if (!simulateMemoryStickPro)
			{
				if (totalSize > MAX_MEMORY_STICK_SIZE)
				{
					totalSize = MAX_MEMORY_STICK_SIZE;
					MemoryStick.TotalSize = MAX_MEMORY_STICK_SIZE;

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Limiting the size of the Memory Stick (i.e. non-PRO) to {0}", MemoryStick.getSizeKbString((int)(totalSize / 1024))));
					}
				}
			}

			long totalNumberOfPages = totalSize / PAGE_SIZE;
			// 16 pages per block is only valid up to 4MB Memory Stick
			if (!simulateMemoryStickPro && totalNumberOfPages <= 8192L)
			{
				PAGES_PER_BLOCK = 16;
				NUMBER_OF_PHYSICAL_BLOCKS = (int)(totalNumberOfPages / PAGES_PER_BLOCK);
			}
			else
			{
				for (PAGES_PER_BLOCK = 32; PAGES_PER_BLOCK < 0x8000; PAGES_PER_BLOCK <<= 1)
				{
					NUMBER_OF_PHYSICAL_BLOCKS = (int)(totalNumberOfPages / PAGES_PER_BLOCK);
					if (NUMBER_OF_PHYSICAL_BLOCKS < 0x10000)
					{
						break;
					}
				}
			}
			BLOCK_SIZE = PAGES_PER_BLOCK * PAGE_SIZE / 1024; // Number of KB per block
			NUMBER_OF_LOGICAL_BLOCKS = NUMBER_OF_PHYSICAL_BLOCKS - (NUMBER_OF_PHYSICAL_BLOCKS / 512 * 16);
			FIRST_PAGE_LBA = 2 * PAGES_PER_BLOCK;
			NUMBER_OF_PAGES = (NUMBER_OF_PHYSICAL_BLOCKS / 512 * 496 - 2) * BLOCK_SIZE * 2;
			if (!simulateMemoryStickPro)
			{
				if (NUMBER_OF_PAGES > 0x3DF00)
				{
					NUMBER_OF_PAGES = 0x3DF00; // For 128MB Memory Stick
				}
				else if (NUMBER_OF_PAGES > 0x1EF80)
				{
					NUMBER_OF_PAGES = 0x1EF80; // For 64MB Memory Stick
				}
			}
			NUMBER_OF_PAGES -= FIRST_PAGE_LBA;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("MMIOHandlerMemoryStick.reset totalSize=0x{0:X}({1}), pagesPerBlock=0x{2:X}, numberOfPhysicalBlocks=0x{3:X}", totalSize, MemoryStick.getSizeKbString((int)(totalSize / 1024)), PAGES_PER_BLOCK, NUMBER_OF_PHYSICAL_BLOCKS));
			}

			if (!simulateMemoryStickPro)
			{
				if (BLOCK_SIZE != 8 && BLOCK_SIZE != 16)
				{
					Console.WriteLine(string.Format("The size of a Memory Stick (i.e. non-PRO) is limited to 512MB, the current size of {0} cannot be supported", MemoryStick.getSizeKbString((int)(totalSize / 1024))));
				}
			}

			Arrays.Fill(registers, 0);
			interrupt = 0;
			commandState = 0;
			unk08 = 0;
			tpc = 0;
			status = 0;
			sys = 0;
			readAddress = 0;
			readSize = 0;
			writeAddress = 0;
			writeSize = 0;
			cmd = 0;
			numberOfPages = 0;
			pageLba = 0;
			pageIndex = 0;
			pageDataIndex = 0;
			startBlock = 0;
			oobLength = 0;
			oobIndex = 0;
			commandDataIndex = 0;

			registers[MS_INT_REG_ADDRESS] = MS_INT_REG_CED;
			if (MemoryStick.Inserted)
			{
				status |= MS_STATUS_READY | 0x0020;
			}
			if (MemoryStick.Locked)
			{
				registers[MS_STATUS_REG_ADDRESS] |= MS_STATUS_REG_READONLY;
			}

			if (simulateMemoryStickPro)
			{
				registers[MS_TYPE_ADDRESS] = MS_TYPE_MEMORY_STICK_PRO;
				registers[MS_SYSTEM_ADDRESS] = MS_SYSTEM_SERIAL_MODE;
			}

			bootPage.header.blockId = MS_BOOT_BLOCK_ID;
			bootPage.header.formatVersion = MS_BOOT_BLOCK_FORMAT_VERSION;
			bootPage.header.numberOfDataEntry = MS_BOOT_BLOCK_DATA_ENTRIES;
			bootPage.entry.disabledBlock.startAddr = DISABLED_BLOCKS_PAGE * PAGE_SIZE - bootPage.@sizeof();
			bootPage.entry.disabledBlock.dataSize = 4;
			bootPage.entry.disabledBlock.dataTypeId = MS_SYSENT_TYPE_INVALID_BLOCK;
			bootPage.entry.cisIdi.startAddr = CIS_IDI_PAGE * PAGE_SIZE - bootPage.@sizeof();
			bootPage.entry.cisIdi.dataSize = PAGE_SIZE;
			bootPage.entry.cisIdi.dataTypeId = MS_SYSENT_TYPE_CIS_IDI;
			bootPage.attr.memorystickClass = MS_SYSINF_MSCLASS_TYPE_1; // must be 1
			bootPage.attr.cardType = MS_SYSINF_CARDTYPE_RDWR;
			bootPage.attr.blockSize = BLOCK_SIZE; // Number of KB per block
			bootPage.attr.numberOfBlocks = NUMBER_OF_PHYSICAL_BLOCKS; // Number of physical blocks
			bootPage.attr.numberOfEffectiveBlocks = NUMBER_OF_LOGICAL_BLOCKS; // Number of logical blocks
			bootPage.attr.pageSize = PAGE_SIZE; // Must be 0x200
			bootPage.attr.extraDataSize = 0x10;
			bootPage.attr.securitySupport = 0x01; // 1 means no security support
			bootPage.attr.formatUniqueValue4[0] = 1;
			bootPage.attr.formatUniqueValue4[1] = 1;
			bootPage.attr.transferSupporting = 0;
			bootPage.attr.formatType = MS_SYSINF_FORMAT_FAT;
			bootPage.attr.memorystickApplication = MS_SYSINF_USAGE_GENERAL;
			bootPage.attr.deviceType = 0;
			bootPage.write(bootPageMemory);

			bootPageBackup.header.blockId = MS_BOOT_BLOCK_ID;
			bootPageBackup.write(bootPageBackupMemory);

			disabledBlocksPageMemory.write16(0, (short) endianSwap16(0x0000));
			disabledBlocksPageMemory.write16(2, (short) endianSwap16(0x0001));
			disabledBlocksPageMemory.write16(4, (short) endianSwap16(NUMBER_OF_PHYSICAL_BLOCKS - 1));

			if (simulateMemoryStickPro)
			{
				msproAttribute.signature = 0xA5C3;
				msproAttribute.count = 0;
				int entryAddress = 0x1A0; // Only accepting attribute entries starting at that address

				MemoryStickSysInfo memoryStickSysInfo = new MemoryStickSysInfo();
				memoryStickSysInfo.memoryStickClass = MEMORY_STICK_CLASS_PRO;
				memoryStickSysInfo.blockSize = PAGES_PER_BLOCK;
				memoryStickSysInfo.blockCount = NUMBER_OF_PHYSICAL_BLOCKS;
				memoryStickSysInfo.userBlockCount = NUMBER_OF_LOGICAL_BLOCKS;
				memoryStickSysInfo.unitSize = PAGE_SIZE;
				memoryStickSysInfo.deviceType = 0;
				memoryStickSysInfo.interfaceType = 1;
				memoryStickSysInfo.memoryStickSubClass = 0;
				entryAddress = addMsproAttributeEntry(entryAddress, MSPRO_BLOCK_ID_SYSINFO, memoryStickSysInfo);

				MemoryStickDeviceInfo memoryStickDeviceInfo = new MemoryStickDeviceInfo();
				entryAddress = addMsproAttributeEntry(entryAddress, MSPRO_BLOCK_ID_DEVINFO, memoryStickDeviceInfo);

				MemoryStickMbr memoryStickMbr = new MemoryStickMbr();
				entryAddress = addMsproAttributeEntry(entryAddress, MSPRO_BLOCK_ID_MBR, memoryStickMbr);

				MemoryStickPbr32 memoryStickPbr32 = new MemoryStickPbr32();
				sceMSstorModule.hleMSstorPartitionIoLseek(null, 0L, PSP_SEEK_SET);
				sceMSstorModule.hleMSstorPartitionIoRead(memoryStickPbr32.bootSector, 0, memoryStickPbr32.bootSector.Length);
				entryAddress = addMsproAttributeEntry(entryAddress, MSPRO_BLOCK_ID_PBR32, memoryStickPbr32);

				msproAttribute.write(msproAttributeMemory);
			}
		}

		private int addMsproAttributeEntry(int address, int id, pspAbstractMemoryMappedStructure attributeInfo)
		{
			int size = attributeInfo.@sizeof();

			msproAttribute.entries[msproAttribute.count].address = address;
			msproAttribute.entries[msproAttribute.count].size = size;
			msproAttribute.entries[msproAttribute.count].id = id;

			attributeInfo.write(msproAttributeMemory, address);

			msproAttribute.count++;

			return address + size;
		}

		private void writeSys(int sys)
		{
			this.sys = sys;

			if ((sys & MS_SYS_RESET) != 0)
			{
				// Reset
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("MMIOHandlerMemoryStick.writeSys reset triggered"));
				}
				reset();
			}
		}

		private void writeCommandState(int commandState)
		{
			this.commandState = commandState;

			if (Busy)
			{
				clearBusy();
			}
		}

		private int Status
		{
			get
			{
				// Lowest 4 bits of the status are identical to bits 4..7 from MS_INT_REG_ADDRESS register
				return (status & ~0xF) | ((registers[MS_INT_REG_ADDRESS] >> 4) & 0xF);
			}
		}

		private void setBusy()
		{
			commandState |= MS_COMMANDSTATE_BUSY;
		}

		public virtual void clearBusy()
		{
			commandState &= ~MS_COMMANDSTATE_BUSY;
		}

		private bool Busy
		{
			get
			{
				return (commandState & MS_COMMANDSTATE_BUSY) != 0;
			}
		}

		private int Interrupt
		{
			set
			{
				this.interrupt |= value;
				checkInterrupt();
			}
		}

		private void clearInterrupt(int interrupt)
		{
			// TODO Not sure if this is the correct behavior. (gid15)
			//
			// The PSP seems to want to clear only bit 0x1 of the interrupt,
			// but sometimes (due to a race condition), it is also clearing
			// bit 0x4, which should not.
			//
			// The PSP code is executing:
			//    ((u16 *) 0xBD200000) |= 0x1;
			// which will actually clear all the interrupt bits...
			if ((interrupt & 0x1) != 0)
			{
				interrupt &= ~0x4;
			}

			this.interrupt &= ~interrupt;
			checkInterrupt();
		}

		private void checkInterrupt()
		{
			if (interrupt != 0)
			{
				RuntimeContextLLE.triggerInterrupt(Processor, PSP_MSCM0_INTR);
			}
			else
			{
				RuntimeContextLLE.clearInterrupt(Processor, PSP_MSCM0_INTR);
			}
		}

		private bool MemoryStickPro
		{
			get
			{
				return (registers[MS_TYPE_ADDRESS] & MS_TYPE_MEMORY_STICK_PRO) != 0;
			}
		}

		private bool SerialMode
		{
			get
			{
				if (!MemoryStickPro)
				{
					// A non-PRO memory stick is always in serial mode
					return true;
				}
				return (registers[MS_SYSTEM_ADDRESS] & MS_SYSTEM_SERIAL_MODE) != 0;
			}
		}

		private int TPCCode
		{
			get
			{
				return tpc >> 12;
			}
		}

		private void startTPC(int tpc)
		{
			this.tpc = tpc;

			int tpcCode = TPCCode;
			int unknown = tpc & 0x3FF;

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("startTPC tpcCode=0x{0:X1}({1}), unknown=0x{2:X3}", tpcCode, getTPCName(tpcCode), unknown));
			}

			switch (tpcCode)
			{
				case MS_TPC_SET_RW_REG_ADDRESS:
					// Data will be set at next writeData()
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startTPC MS_TPC_SET_RW_REG_ADDRESS"));
					}
					break;
				case MS_TPC_READ_REG:
					// Data will be retrieve at next readData()
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startTPC MS_TPC_READ_REG readAddress=0x{0:X2}, readSize=0x{1:X}", readAddress, readSize));
					}
					break;
				case MS_TPC_WRITE_REG:
					// Register will be written during writeData()
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startTPC MS_TPC_WRITE_REG writeAddress=0x{0:X2}, writeSize=0x{1:X}", writeAddress, writeSize));
					}
					break;
				case MS_TPC_SET_CMD:
					// Clear the CED (Command EnD) bit in the INT register
					registers[MS_INT_REG_ADDRESS] &= ~MS_INT_REG_CED;
					// Register will be written during writeData()
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startTPC MS_TPC_SET_CMD"));
					}
					break;
				case MS_TPC_EX_SET_CMD:
					// Clear the CED (Command EnD) bit in the INT register
					registers[MS_INT_REG_ADDRESS] &= ~MS_INT_REG_CED;
					// Parameters will be written during writeTPCData()
					tpcExSetCmdIndex = 0;
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startTPC MS_TPC_EX_SET_CMD"));
					}
					break;
				case MS_TPC_GET_INT:
					// Data will be retrieved at next readData()
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startTPC MS_TPC_GET_INT"));
					}
					break;
				case MS_TPC_READ_PAGE_DATA:
					// Data will be retrieved through readData16()
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startTPC MS_TPC_READ_PAGE_DATA"));
					}
					break;
				default:
					Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startTPC unknown TPC 0x{0:X1}", tpcCode));
					break;
			}

			status |= MS_STATUS_FIFO_RW;
		}

		private int getRegisterValue(int reg, int Length)
		{
			int value = 0;
			for (int i = 0; i < Length; i++)
			{
				value = (value << 8) | (registers[reg + i] & 0xFF);
			}

			return value;
		}

		private int DataCount
		{
			get
			{
				if (MemoryStickPro)
				{
					return getRegisterValue(0x11, 2);
				}
				return PAGE_SIZE;
			}
		}

		private int DataAddress
		{
			get
			{
				if (MemoryStickPro)
				{
					return getRegisterValue(0x13, 4);
				}
    
				int blockAddress = getRegisterValue(0x11, 3);
				int pageAddress = getRegisterValue(0x15, 1);
				return blockAddress * PAGES_PER_BLOCK + pageAddress;
			}
		}

		private void startCmd(int cmd)
		{
			Cmd = cmd;

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startCmd cmd=0x{0:X2}({1})", cmd, getCommandName(cmd)));
			}

			switch (cmd)
			{
				case MS_CMD_BLOCK_READ:
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startCmd MS_CMD_BLOCK_READ dataCount=0x{0:X4}, dataAddress=0x{1:X8}, cp=0x{2:X2}", DataCount, DataAddress, getRegisterValue(0x14, 1)));
					}
					setBusy();
					if (!MemoryStickPro)
					{
						registers[0x16] = 0x80;
						registers[0x17] = 0x00;
					}
					break;
				case MS_CMD_BLOCK_ERASE:
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startCmd MS_CMD_BLOCK_ERASE dataCount=0x{0:X4}, dataAddress=0x{1:X8}, cp=0x{2:X2}", DataCount, DataAddress, getRegisterValue(0x14, 1)));
					}
					clearBusy();
					break;
				case MS_CMD_BLOCK_WRITE:
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startCmd MS_CMD_BLOCK_WRITE dataCount=0x{0:X4}, dataAddress=0x{1:X8}, cp=0x{2:X2}", DataCount, DataAddress, getRegisterValue(0x14, 1)));
					}
					break;
				case MS_CMD_SLEEP:
				case MSPRO_CMD_SLEEP:
					// Simply ignore these commands
					break;
				case MSPRO_CMD_WRITE_DATA:
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startCmd MSPRO_CMD_WRITE_DATA dataCount=0x{0:X4}, dataAddress=0x{1:X8}", DataCount, DataAddress));
					}
					NumberOfPages = DataCount;
					StartBlock = 0;
					PageLba = DataAddress;
					break;
				default:
					Console.WriteLine(string.Format("MMIOHandlerMemoryStick.startCmd unknown cmd=0x{0:X2}", cmd));
					break;
			}

			// Set only the CED (Command EnD) bit in the INT register,
			// indicating a successful completion
			registers[MS_INT_REG_ADDRESS] = MS_INT_REG_CED;

			pageIndex = 0;
			pageDataIndex = 0;
			status |= 0x2000;
			sys |= 0x4000;
			Interrupt = 0x4;
		}

		private int readData16()
		{
			if (!SerialMode)
			{
				Console.WriteLine(string.Format("MMIOHandlerMemoryStick.readData16 not supported for parallel mode"));
				return 0;
			}

			int value = 0;
			int dataAddress = DataAddress;
			if (dataAddress == 0)
			{
				value = bootPageMemory.read16(pageDataIndex);
			}
			else if (dataAddress == PAGES_PER_BLOCK)
			{
				value = bootPageBackupMemory.read16(pageDataIndex);
			}
			else if (dataAddress == DISABLED_BLOCKS_PAGE)
			{
				value = disabledBlocksPageMemory.read16(pageDataIndex);
			}
			else if (dataAddress == CIS_IDI_PAGE)
			{
				Console.WriteLine(string.Format("MMIOHandlerMemoryStick.readData16 unimplemented reading from CIS_IDI_PAGE"));
			}

			pageDataIndex += 2;
			if (pageDataIndex >= PAGE_SIZE)
			{
				clearBusy();
				status |= 0x2000;
				sys |= 0x4000;
				Interrupt = 0x0004;
			}

			return value;
		}

		private int readOOBData16()
		{
			int value;
			if ((oobIndex & 3) == 0)
			{
				// Overwrite area and management flag
				value = (MS_REG_MNG_DEFAULT << 8) | MS_REG_OVR_DEFAULT;
			}
			else
			{
				// Logical address
				value = endianSwap16(startBlock + (oobIndex >> 2));
			}

			oobIndex += 2;
			if (oobIndex >= (oobLength << 2))
			{
				oobIndex = 0;
				clearBusy();
				status |= 0x2000;
				sys |= 0x4000;
				unk08 |= 0x0040;
				unk08 &= ~0x000F; // Clear error code
				Interrupt = 0x0004;
			}

			return value;
		}

		private int readPageData16()
		{
			if (!SerialMode)
			{
				Console.WriteLine(string.Format("MMIOHandlerMemoryStick.readPageData16 not supported for parallel mode"));
				return 0;
			}

			int value;

			switch (cmd)
			{
				case MS_CMD_BLOCK_READ:
					if (pageDataIndex == 0)
					{
						readPageBuffer();
					}
					value = pageBufferMemory.read16(pageDataIndex);
					break;
				default:
					Console.WriteLine(string.Format("MMIOHandlerMemoryStick.readPageData16 unimplemented cmd=0x{0:X2}({1})", cmd, getCommandName(cmd)));
					value = 0;
					break;
			}

			pageDataIndex += 2;
			if (pageDataIndex >= PAGE_SIZE)
			{
				pageDataIndex = 0;
				pageIndex++;
				if (pageIndex >= numberOfPages)
				{
					pageIndex = 0;
					clearBusy();
					status |= 0x2000;
					sys |= 0x4000;
					unk08 |= 0x0040;
					unk08 &= ~0x000F; // Clear error code
					Interrupt = 0x0004;
				}
			}

			return value;
		}

		private int NumberOfPages
		{
			set
			{
				this.numberOfPages = value;
				pageIndex = 0;
				pageDataIndex = 0;
			}
		}

		private int PageLba
		{
			set
			{
				this.pageLba = value;
				pageIndex = 0;
				pageDataIndex = 0;
			}
		}

		private int StartBlock
		{
			set
			{
				this.startBlock = value;
				pageIndex = 0;
				pageDataIndex = 0;
				oobIndex = 0;
			}
		}

		private void readMasterBootRecord()
		{
			// See description of MBR at
			// https://en.wikipedia.org/wiki/Master_boot_record
			pageBufferPointer.clear(PAGE_SIZE);

			// First partition entry
			TPointer partitionPointer = new TPointer(pageBufferPointer, 446);
			// Active partition
			partitionPointer.setValue8(0, unchecked((sbyte) 0x80));
			// CHS address of first absolute sector in partition (not used by the PSP)
			partitionPointer.setValue8(1, (sbyte) 0x00);
			partitionPointer.setValue8(2, (sbyte) 0x00);
			partitionPointer.setValue8(3, (sbyte) 0x00);
			// Partition type: FAT32 with LBA
			partitionPointer.setValue8(4, (sbyte) 0x0C);
			// CHS address of last absolute sector in partition (not used by the PSP)
			partitionPointer.setValue8(5, (sbyte) 0x00);
			partitionPointer.setValue8(6, (sbyte) 0x00);
			partitionPointer.setValue8(7, (sbyte) 0x00);
			// LBA of first absolute sector in the partition
			partitionPointer.setUnalignedValue32(8, FIRST_PAGE_LBA);
			// Number of sectors in partition
			partitionPointer.setUnalignedValue32(12, NUMBER_OF_PAGES);

			// Signature
			pageBufferPointer.setValue8(510, (sbyte) 0x55);
			pageBufferPointer.setValue8(511, unchecked((sbyte) 0xAA));
		}

		private int Cmd
		{
			set
			{
				this.cmd = value;
			}
		}

		private void readPageBuffer()
		{
			if (cmd == MS_CMD_BLOCK_READ || cmd == MSPRO_CMD_READ_DATA)
			{
				long offset = 0L;
				int lba = pageLba;
				if (startBlock != 0xFFFF)
				{
					pageLba += startBlock * PAGES_PER_BLOCK;
				}

				// Invalid page number set during boot sequence
				if (pageLba == 0x4000)
				{
					pageBufferPointer.clear(PAGE_SIZE);
				}
				else if (lba == 0)
				{
					readMasterBootRecord();
				}
				else if (lba >= FIRST_PAGE_LBA)
				{
					offset = (lba - FIRST_PAGE_LBA) * (long) PAGE_SIZE;

					sceMSstorModule.hleMSstorPartitionIoLseek(null, offset, PSP_SEEK_SET);
					sceMSstorModule.hleMSstorPartitionIoRead(null, pageBufferPointer, PAGE_SIZE);
				}
				else
				{
					pageBufferPointer.clear(PAGE_SIZE);
				}

				//if (log.DebugEnabled)
				{
					sbyte[] buffer = new sbyte[PAGE_SIZE];
					for (int i = 0; i < buffer.Length; i++)
					{
						buffer[i] = pageBufferPointer.getValue8(i);
					}
					Console.WriteLine(string.Format("MMIOHandlerMemoryStick.readPageBuffer startBlock=0x{0:X}, lba=0x{1:X}, offset=0x{2:X}: {3}", startBlock, lba, offset, Utilities.getMemoryDump(buffer)));
				}

				pageLba++;
			}
		}

		private int readTPCData32()
		{
			int data = 0;
			switch (TPCCode)
			{
				case MS_TPC_GET_INT:
					data = registers[MS_INT_REG_ADDRESS] & 0xFF;
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.readTPCData32 MS_TPC_GET_INT registers[0x{0:X2}]=0x{1:X2}", MS_INT_REG_ADDRESS, data));
					}
					break;
				case MS_TPC_READ_REG:
					for (int i = 0; i < 32; i += 8)
					{
						if (readSize <= 0 || readAddress >= registers.Length)
						{
							break;
						}
						data |= (registers[readAddress] & 0xFF) << i;
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("MMIOHandlerMemoryStick.readTPCData32 MS_TPC_READ_REG registers[0x{0:X2}]=0x{1:X2}", readAddress, registers[readAddress]));
						}
						readSize--;
						readAddress++;
					}
					break;
			}

			return data;
		}

		private int readPageData32()
		{
			if (SerialMode)
			{
				Console.WriteLine(string.Format("MMIOHandlerMemoryStick.readPageData32 not supported for serial mode"));
				return 0;
			}

			int value;

			switch (cmd)
			{
				case MSPRO_CMD_READ_ATRB:
					value = msproAttributeMemory.read32((pageLba * PAGE_SIZE) + pageDataIndex);
					break;
				case MSPRO_CMD_READ_DATA:
					if (pageDataIndex == 0)
					{
						readPageBuffer();
					}
					value = pageBufferMemory.read32(pageDataIndex);
					break;
				default:
					Console.WriteLine(string.Format("MMIOHandlerMemoryStick.readPageData32 unimplemented cmd=0x{0:X2}({1})", cmd, getCommandName(cmd)));
					value = 0;
					break;
			}

			pageDataIndex += 4;
			if (pageDataIndex >= PAGE_SIZE)
			{
				pageDataIndex = 0;
				pageIndex++;
				if (pageIndex >= numberOfPages)
				{
					pageIndex = 0;
					commandDataIndex = 0;
					clearBusy();
					status |= 0x2000;
					sys |= 0x4000;
					unk08 |= 0x0040;
					unk08 &= ~0x000F; // Clear error code
					Interrupt = 0x0004;
				}
			}

			return value;
		}

		private void writeTPCData(int value)
		{
			if (tpc < 0)
			{
				// Ignore this data
				return;
			}

			switch (TPCCode)
			{
				case MS_TPC_SET_RW_REG_ADDRESS:
					// Sets the read address & size, write address & size
					// for a subsequent MS_CMD_READ_REG/MS_CMD_WRITE_REG command
					readAddress = value & 0xFF;
					readSize = (value >> 8) & 0xFF;
					writeAddress = (value >> 16) & 0xFF;
					writeSize = (value >> 24) & 0xFF;
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.writeTPCData MS_TPC_SET_RW_REG_ADDRESS readAddress=0x{0:X2}, readSize=0x{1:X}, writeAddress=0x{2:X2}, writeSize=0x{3:X}", readAddress, readSize, writeAddress, writeSize));
					}
					// Ignore further data
					tpc = -1;
					break;
				case MS_TPC_WRITE_REG:
					for (int i = 0; i < 4; i++)
					{
						if (writeSize <= 0 || writeAddress >= registers.Length)
						{
							break;
						}
						registers[writeAddress] = value & 0xFF;
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("MMIOHandlerMemoryStick.writeTPCData MS_TPC_WRITE_REG registers[0x{0:X2}]=0x{1:X2}", writeAddress, registers[writeAddress]));
						}
						writeAddress++;
						writeSize--;
						value = (int)((uint)value >> 8);
					}
					break;
				case MS_TPC_SET_CMD:
					startCmd(value & 0xFF);
					// Ignore further data
					tpc = -1;
					break;
				case MS_TPC_EX_SET_CMD:
					switch (tpcExSetCmdIndex)
					{
						case 0:
							Cmd = value & 0xFF;
							registers[0x11] = (value >> 8) & 0xFF;
							registers[0x12] = (value >> 16) & 0xFF;
							registers[0x13] = (value >> 24) & 0xFF;
							break;
						case 1:
							registers[0x14] = (value >> 0) & 0xFF;
							registers[0x15] = (value >> 8) & 0xFF;
							registers[0x16] = (value >> 16) & 0xFF;
							startCmd(cmd);
							break;
						default:
							Console.WriteLine(string.Format("Too many parameters to MS_TPC_EX_SET_CMD: 0x{0:X}", value));
							break;
					}
					tpcExSetCmdIndex++;
					break;
			}
		}

		private void writeCommandData8(int value)
		{
			switch (commandDataIndex)
			{
				case 0:
					Cmd = value;
					tpc = MS_TPC_SET_CMD;
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("MMIOHandlerMemoryStick.writeCommandData8 cmd=0x{0:X2}({1})", cmd, getCommandName(cmd)));
					}

					switch (cmd)
					{
						case MSPRO_CMD_READ_ATRB:
							break;
						case MSPRO_CMD_READ_DATA:
							break;
						case MSPRO_CMD_WRITE_DATA:
							break;
						default:
							Console.WriteLine(string.Format("MMIOHandlerMemoryStick.writeCommandData8 unimplemented cmd=0x{0:X2}({1})", cmd, getCommandName(cmd)));
							break;
					}
					break;
				case 1:
					numberOfPages = (numberOfPages & 0x00FF) | (value << 8);
					break;
				case 2:
					NumberOfPages = (numberOfPages & 0xFF00) | value;
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("numberOfPages=0x{0:X}", numberOfPages));
					}
					break;
				case 3:
					pageLba = (pageLba & 0x00FFFFFF) | (value << 24);
					break;
				case 4:
					pageLba = (pageLba & unchecked((int)0xFF00FFFF)) | (value << 16);
					break;
				case 5:
					pageLba = (pageLba & unchecked((int)0xFFFF00FF)) | (value << 8);
					break;
				case 6:
					StartBlock = 0;
					PageLba = (pageLba & unchecked((int)0xFFFFFF00)) | value;
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("pageLba=0x{0:X}", pageLba));
					}
					break;
				default:
					Console.WriteLine(string.Format("MMIOHandlerMemoryStick.writeCommandData8 unknown data 0x{0:X2} written at index 0x{1:X}", value, commandDataIndex));
					break;
			}
			commandDataIndex++;
		}

		private void writePageBuffer()
		{
			if (cmd == MSPRO_CMD_WRITE_DATA)
			{
				long offset = 0L;
				int lba = pageLba;
				if (startBlock != 0xFFFF)
				{
					lba += startBlock * PAGES_PER_BLOCK;
				}

				//if (log.DebugEnabled)
				{
					sbyte[] buffer = new sbyte[PAGE_SIZE];
					for (int i = 0; i < buffer.Length; i++)
					{
						buffer[i] = pageBufferPointer.getValue8(i);
					}
					Console.WriteLine(string.Format("MMIOHandlerMemoryStick.writePageBuffer startBlock=0x{0:X}, lba=0x{1:X}, offset=0x{2:X}: {3}", startBlock, lba, offset, Utilities.getMemoryDump(buffer)));
				}

				if (lba >= FIRST_PAGE_LBA)
				{
					offset = (lba - FIRST_PAGE_LBA) * (long) PAGE_SIZE;

					sceMSstorModule.hleMSstorPartitionIoLseek(null, offset, PSP_SEEK_SET);
					sceMSstorModule.hleMSstorPartitionIoWrite(null, pageBufferPointer, PAGE_SIZE);
				}
				else
				{
					Console.WriteLine(string.Format("MMIOHandlerMemoryStick.writePageBuffer invalid lba=0x{0:X}", lba));
				}

				pageLba++;
			}
		}

		private void writePageData32(int value)
		{
			if (SerialMode)
			{
				Console.WriteLine(string.Format("MMIOHandlerMemoryStick.writePageData32 not supported for serial mode"));
				return;
			}

			switch (cmd)
			{
				case MSPRO_CMD_WRITE_DATA:
					pageBufferMemory.write32(pageDataIndex, value);
					break;
				default:
					Console.WriteLine(string.Format("MMIOHandlerMemoryStick.writePageData32 unimplemented cmd=0x{0:X2}({1})", cmd, getCommandName(cmd)));
					break;
			}

			pageDataIndex += 4;
			if (pageDataIndex >= PAGE_SIZE)
			{
				pageDataIndex = 0;
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("MMIOHandlerMemoryStick.writePageData32 writing page 0x{0:X}/0x{1:X}", pageIndex, numberOfPages));
				}
				writePageBuffer();
				pageIndex++;
				if (pageIndex >= numberOfPages)
				{
					pageIndex = 0;
					commandDataIndex = 0;
					clearBusy();
					status |= 0x2000;
					sys |= 0x4000;
					unk08 |= 0x0040;
					unk08 &= ~0x000F; // Clear error code
					Interrupt = 0x0004;
				}
			}
		}

		public override int read16(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
					value = interrupt;
					break;
				case 0x04:
					value = commandState;
					break;
				case 0x08:
					value = unk08;
					break;
				case 0x24:
					value = readOOBData16();
					break;
				case 0x28:
					value = readPageData16();
					break;
				case 0x30:
					value = tpc;
					break;
				case 0x34:
					value = readData16();
					break;
				case 0x38:
					value = Status;
					break;
				case 0x3C:
					value = sys;
					break;
				default:
					value = base.read16(address);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read16(0x{1:X8}) returning 0x{2:X4}", Pc, address, value));
			}

			return value;
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x28:
					value = readPageData32();
					break;
				case 0x34:
					value = readTPCData32();
					break;
				default:
					value = base.read32(address);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read32(0x{1:X8}) returning 0x{2:X8}", Pc, address, value));
			}

			return value;
		}

		public override void write16(int address, short value)
		{
			switch (address - baseAddress)
			{
				case 0x00:
					clearInterrupt(value & 0xFFFF);
					break;
				case 0x02:
					break; // TODO Unknown
					goto case 0x04;
				case 0x04:
					writeCommandState(value & 0xFFFF);
					break;
				case 0x10:
					NumberOfPages = value;
					break;
				case 0x12:
					oobLength = value;
					break;
				case 0x14:
					StartBlock = value;
					break;
				case 0x16:
					Cmd = MS_CMD_BLOCK_READ;
					PageLba = value;
					break;
				case 0x20:
					break; // TODO Unknown
					goto case 0x30;
				case 0x30:
					startTPC(value & 0xFFFF);
					break;
				case 0x38:
					status = value & 0xFFFF;
					break;
				case 0x3C:
					writeSys(value & 0xFFFF);
					break;
				default:
					base.write16(address, value);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write16(0x{1:X8}, 0x{2:X4}) on {3}", Pc, address, value & 0xFFFF, this));
			}
		}

		public override void write32(int address, int value)
		{
			switch (address - baseAddress)
			{
				case 0x28:
					writePageData32(value);
					break;
				case 0x34:
					writeTPCData(value);
					break;
				case 0x40:
					break; // TODO Unknown
					goto default;
				default:
					base.write32(address, value);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write32(0x{1:X8}, 0x{2:X8}) on {3}", Pc, address, value, this));
			}
		}

		public override void write8(int address, sbyte value)
		{
			switch (address - baseAddress)
			{
				case 0x24:
					writeCommandData8(value & 0xFF);
					break;
				default:
					base.write8(address, value);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write8(0x{1:X8}, 0x{2:X2}) on {3}", Pc, address, value & 0xFF, this));
			}
		}
	}

}