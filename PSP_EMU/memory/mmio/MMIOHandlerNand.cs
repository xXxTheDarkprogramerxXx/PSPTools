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
//	import static pspsharp.HLE.Modules.sceNandModule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.Modules.sceSysregModule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_NAND_INTR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNand.pagesPerBlock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.endianSwap16;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.lineSeparator;

	//using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using TPointer = pspsharp.HLE.TPointer;
	using sceNand = pspsharp.HLE.modules.sceNand;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// PSP NAND hardware interface
	/// See http://www.alldatasheet.com/datasheet-pdf/pdf/37107/SAMSUNG/K9F5608U0C.html
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class MMIOHandlerNand : MMIOHandlerBase
	{
		public static new Logger log = sceNand.log;
		private const int STATE_VERSION = 0;
		public const int BASE_ADDRESS = unchecked((int)0xBD101000);
		public const int PSP_NAND_CONTROL_AUTO_USER_ECC = 0x10000;
		public const int PSP_NAND_STATUS_READY = 0x01;
		public const int PSP_NAND_STATUS_WRITE_ENABLED = 0x80;
		public const int PSP_NAND_INTR_WRITE_COMPLETED = 0x002;
		public const int PSP_NAND_INTR_READ_COMPLETED = 0x001;
		public const int PSP_NAND_COMMAND_READ_EXTRA = 0x50;
		public const int PSP_NAND_COMMAND_ERASE_BLOCK = 0x60;
		public const int PSP_NAND_COMMAND_GET_READ_STATUS = 0x70;
		public const int PSP_NAND_COMMAND_READ_ID = 0x90;
		public const int PSP_NAND_COMMAND_ERASE_BLOCK_CONFIRM = 0xD0;
		public const int PSP_NAND_COMMAND_RESET = 0xFF;
		private const int DMA_CONTROL_START = 0x0001;
		private const int DMA_CONTROL_WRITE = 0x0002;
		private static MMIOHandlerNand instance;
		private readonly IntArrayMemory pageDataMemory;
		private readonly IntArrayMemory pageEccMemory;
		private readonly IntArrayMemory dataMemory;
		private readonly IntArrayMemory scrambleBufferMemory;
		private int control;
		private int status;
		private int command;
		private int pageAddress;
		private readonly int[] data = new int[4];
		private int dataIndex;
		private int dmaAddress;
		private int dmaControl;
		private int dmaStatus;
		private int dmaInterrupt;
		private int unknown200;
		private bool needPageAddress;
		private readonly int[] scrambleBuffer = new int[sceNand.pageSize >> 2];

		public static MMIOHandlerNand Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MMIOHandlerNand(BASE_ADDRESS);
				}
				return instance;
			}
		}

		private MMIOHandlerNand(int baseAddress) : base(baseAddress)
		{

			pageDataMemory = new IntArrayMemory(MMIOHandlerNandPage.Instance.Data);
			pageEccMemory = new IntArrayMemory(MMIOHandlerNandPage.Instance.Ecc);
			dataMemory = new IntArrayMemory(data);
			scrambleBufferMemory = new IntArrayMemory(scrambleBuffer);

			status = PSP_NAND_STATUS_READY;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			control = stream.readInt();
			status = stream.readInt();
			command = stream.readInt();
			pageAddress = stream.readInt();
			stream.readInts(data);
			dataIndex = stream.readInt();
			dmaAddress = stream.readInt();
			dmaControl = stream.readInt();
			dmaStatus = stream.readInt();
			dmaInterrupt = stream.readInt();
			unknown200 = stream.readInt();
			needPageAddress = stream.readBoolean();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(control);
			stream.writeInt(status);
			stream.writeInt(command);
			stream.writeInt(pageAddress);
			stream.writeInts(data);
			stream.writeInt(dataIndex);
			stream.writeInt(dmaAddress);
			stream.writeInt(dmaControl);
			stream.writeInt(dmaStatus);
			stream.writeInt(dmaInterrupt);
			stream.writeInt(unknown200);
			stream.writeBoolean(needPageAddress);
			base.write(stream);
		}

		private void startCommand(int command)
		{
			this.command = command;

			dataIndex = 0;
			needPageAddress = false;
			switch (command)
			{
				case PSP_NAND_COMMAND_RESET:
					break;
				case PSP_NAND_COMMAND_GET_READ_STATUS:
					data[0] = status & PSP_NAND_STATUS_WRITE_ENABLED;
					break;
				case PSP_NAND_COMMAND_READ_ID:
					// The pageAddress is written after the command
					needPageAddress = true;
					break;
				case PSP_NAND_COMMAND_READ_EXTRA:
					// The pageAddress is written after the command
					needPageAddress = true;
					break;
				case PSP_NAND_COMMAND_ERASE_BLOCK:
					// The pageAddress is written after the command
					needPageAddress = true;
					break;
				case PSP_NAND_COMMAND_ERASE_BLOCK_CONFIRM:
					// We don't need to erase blocks...
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("PSP_NAND_COMMAND_ERASE_BLOCK ppn=0x{0:X}", pageAddress >> 10));
					}
	//				triggerInterrupt(PSP_NAND_INTR_WRITE_COMPLETED); // TODO Unknown value
					break;
				default:
					Console.WriteLine(string.Format("MMIOHandlerNand.startCommand unknown command 0x{0:X}", command));
					break;
			}
		}

		private void startCommandWithPageAddress()
		{
			needPageAddress = false;

			switch (command)
			{
				case PSP_NAND_COMMAND_READ_ID:
					if (pageAddress == 0)
					{
						// This ID will configure:
						// - sceNandGetPageSize returning 0x200
						// - sceNandGetPagesPerBlock returning 0x20
						// - sceNandGetTotalBlocks returning 0x800
						data[0] = 0xEC; // Manufacturer's code (SAMSUNG)
						data[1] = 0x75; // Device code (K9F5608U0C)
					}
					break;
				case PSP_NAND_COMMAND_READ_EXTRA:
					TPointer spare = dataMemory.Pointer;
					sceNandModule.hleNandReadPages(pageAddress >> 10, TPointer.NULL, spare, 1, true, true, true);
					break;
			}
		}

		private void endCommand(int value)
		{
			if (value == 1)
			{
				dataIndex = 0;
			}
		}

		private void writePageAddress(int pageAddress)
		{
			this.pageAddress = pageAddress;

			if (needPageAddress)
			{
				startCommandWithPageAddress();
			}
		}

		private int readData()
		{
			return data[dataIndex++];
		}

		private int getScrambleBootSector(long fuseId, int partitionNumber)
		{
			int scramble = ((int) fuseId) ^ Integer.rotateLeft((int)(fuseId >> 32), partitionNumber * 2);
			if (scramble == 0)
			{
				scramble = Integer.rotateLeft(0xC4536DE6, partitionNumber);
			}

			return scramble;
		}

		private int getScrambleDataSector(long fuseId, int partitionNumber)
		{
			if (partitionNumber == 3)
			{
				return 0x3C22812A;
			}

			int scramble = ((int) fuseId) ^ Integer.rotateRight((int)(fuseId >> 32), partitionNumber * 3);
			scramble ^= 0x556D81FE;
			if (scramble == 0)
			{
				scramble = Integer.rotateRight(0x556D81FE, partitionNumber);
			}

			return scramble;
		}

		private int getScramble(int ppn)
		{
			long fuseId = sceSysregModule.sceSysregGetFuseId();
			int lbn = sceNandModule.getLbnFromPpn(ppn);
			int sector = ppn % pagesPerBlock;

			int scramble = 0;
			if (lbn == 0x003 && sector == 0)
			{
				scramble = getScrambleBootSector(fuseId, 0); // flash0 boot sector
			}
			else if (lbn >= 0x004 && lbn < 0x601)
			{
				scramble = getScrambleDataSector(fuseId, 0); // flash0
			}
			else if (lbn >= 0x602 && lbn < 0x702)
			{
				scramble = 0; // flash1 is not scrambled
			}
			else if (lbn == 0x703 && sector == 0)
			{
				scramble = getScrambleBootSector(fuseId, 2); // flash2 boot sector
			}
			else if (lbn >= 0x704 && lbn < 0x742)
			{
				scramble = getScrambleDataSector(fuseId, 2); // flash2
			}
			else if (lbn == 0x742 && sector == 0)
			{
				scramble = getScrambleBootSector(fuseId, 3); // flash3 boot sector
			}
			else if (lbn >= 0x743)
			{
				scramble = getScrambleDataSector(fuseId, 3); // flash3
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("getScramble ppn=0x{0:X}, lbn=0x{1:X}, sector=0x{2:X}, scramble=0x{3:X}", ppn, lbn, sector, scramble));
			}

			return scramble;
		}

		private void startDma(int dmaControl)
		{
			this.dmaControl = dmaControl;

			if ((dmaControl & DMA_CONTROL_START) != 0)
			{
				int ppn = dmaAddress >> 10;
				int scramble = getScramble(ppn);

				// Read or write operation?
				if ((dmaControl & DMA_CONTROL_WRITE) != 0)
				{
					int lbn = endianSwap16(pageEccMemory.read16(6) & 0xFFFF);
					if (lbn == 0xFFFF)
					{
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("writing to ppn=0x{0:X} with lbn=0x{1:X} ignored", ppn, lbn));
						}
					}
					else
					{
						TPointer spare = pageEccMemory.Pointer;
						sceNandModule.hleNandWriteSparePages(ppn, spare, 1, true, true, true);

						TPointer user = pageDataMemory.Pointer;

						// Updating the scramble as the LBN corresponding to the PPN might have been moved
						scramble = getScramble(ppn);
						if (scramble != 0)
						{
							sceNand.descramblePage(scramble, ppn, MMIOHandlerNandPage.Instance.Data, scrambleBuffer);
							user = scrambleBufferMemory.Pointer;
						}

						sceNandModule.hleNandWriteUserPages(ppn, user, 1, true, true);

						//if (log.DebugEnabled)
						{
							sbyte[] userBytes = new sbyte[sceNand.pageSize];
							user = scramble != 0 ? scrambleBufferMemory.Pointer : pageDataMemory.Pointer;
							for (int i = 0; i < userBytes.Length; i++)
							{
								userBytes[i] = user.getValue8(i);
							}
							sbyte[] spareBytes = new sbyte[16];
							spare = pageEccMemory.Pointer;
							for (int i = 0; i < spareBytes.Length; i++)
							{
								spareBytes[i] = spare.getValue8(i);
							}
							Console.WriteLine(string.Format("hleNandWritePages ppn=0x{0:X}, lbn=0x{1:X}, scramble=0x{2:X}: {3}{4}Spare: {5}", ppn, lbn, scramble, Utilities.getMemoryDump(userBytes), lineSeparator, Utilities.getMemoryDump(spareBytes)));
						}
					}

					triggerInterrupt(PSP_NAND_INTR_WRITE_COMPLETED);
				}
				else
				{
					TPointer user = scramble != 0 ? scrambleBufferMemory.Pointer : pageDataMemory.Pointer;
					TPointer spare = pageEccMemory.Pointer;
					sceNandModule.hleNandReadPages(ppn, user, spare, 1, true, true, true);

					//if (log.DebugEnabled)
					{
						sbyte[] bytes = new sbyte[sceNand.pageSize];
						user = scramble != 0 ? scrambleBufferMemory.Pointer : pageDataMemory.Pointer;
						for (int i = 0; i < bytes.Length; i++)
						{
							bytes[i] = user.getValue8(i);
						}
						Console.WriteLine(string.Format("hleNandReadPages ppn=0x{0:X}, scramble=0x{1:X}: {2}", ppn, scramble, Utilities.getMemoryDump(bytes)));
					}

					if (scramble != 0)
					{
						sceNand.scramblePage(scramble, ppn, scrambleBuffer, MMIOHandlerNandPage.Instance.Data);
					}

					triggerInterrupt(PSP_NAND_INTR_READ_COMPLETED);
				}
			}
		}

		private void writeDmaInterrupt(int dmaInterrupt)
		{
			// No idea how writing to the dmaInterrupt field is working.
			// I was not able to change any information in this field on a real PSP.
			// The following seems to make the PSP code happy...
			this.dmaInterrupt &= ~0x003;
			checkInterrupt();
		}

		private void triggerInterrupt(int dmaInterrupt)
		{
			// No idea how triggering an interrupt is working.
			// I was not able to reproduce it on a real PSP.
			// The following seems to make the PSP code happy...
			this.dmaInterrupt |= 0x300 | dmaInterrupt;
			checkInterrupt();
		}

		private void checkInterrupt()
		{
			if ((dmaInterrupt & 0x003) != 0)
			{
				RuntimeContextLLE.triggerInterrupt(Processor, PSP_NAND_INTR);
			}
			else
			{
				RuntimeContextLLE.clearInterrupt(Processor, PSP_NAND_INTR);
			}
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x000:
					value = control;
					break;
				case 0x004:
					value = status;
					break;
				case 0x008:
					value = command;
					break;
				case 0x00C:
					value = pageAddress;
					break;
				case 0x020:
					value = dmaAddress;
					break;
				case 0x024:
					value = dmaControl;
					break;
				case 0x028:
					value = dmaStatus;
					break;
				case 0x038:
					value = dmaInterrupt;
					break;
				case 0x200:
					value = unknown200;
					break;
				case 0x300:
					value = readData();
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

		public override void write32(int address, int value)
		{
			switch (address - baseAddress)
			{
				case 0x000:
					control = value;
					break;
				case 0x004:
					status = value;
					break;
				case 0x008:
					startCommand(value);
					break;
				case 0x00C:
					writePageAddress(value);
					break;
				case 0x014:
					endCommand(value);
					break;
				case 0x020:
					dmaAddress = value;
					break;
				case 0x024:
					startDma(value);
					break;
				case 0x028:
					dmaStatus = value;
					break;
				case 0x038:
					writeDmaInterrupt(value);
					break;
				case 0x200:
					unknown200 = value;
					break;
				default:
					base.write32(address, value);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write32(0x{1:X8}, 0x{2:X8}) on {3}", Pc, address, value, this));
			}
		}
	}

}