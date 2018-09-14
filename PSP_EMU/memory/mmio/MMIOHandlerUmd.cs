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
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_UMD_INTR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.filesystems.umdiso.ISectorDevice_Fields.sectorLength;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.MMIOHandlerGpio.GPIO_PORT_BLUETOOTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.MMIOHandlerGpio.GPIO_PORT_UMD;

	using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using TPointer = pspsharp.HLE.TPointer;
	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using UmdIsoReaderVirtualFile = pspsharp.HLE.VFS.iso.UmdIsoReaderVirtualFile;
	using sceNand = pspsharp.HLE.modules.sceNand;
	using sceUmdMan = pspsharp.HLE.modules.sceUmdMan;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;
	using Utilities = pspsharp.util.Utilities;

	public class MMIOHandlerUmd : MMIOHandlerBase
	{
		public static new Logger log = sceUmdMan.log;
		private const int STATE_VERSION = 0;
		public const int BASE_ADDRESS = unchecked((int)0xBDF00000);
		private static MMIOHandlerUmd instance;
		private int command;
		private int reset;
		// Possible interrupt flags: 0x1, 0x2, 0x10, 0x20, 0x40, 0x80, 0x10000, 0x20000, 0x40000, 0x80000
		private int interrupt;
		private int interruptEnabled;
		private int totalTransferLength;
		protected internal readonly int[] transferAddresses = new int[10];
		protected internal readonly int[] transferSizes = new int[10];
		private static readonly int[] QTGP2 = new int[] {0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0};
		private static readonly int[] QTGP3 = new int[] {0x0F, 0xED, 0xCB, 0xA9, 0x87, 0x65, 0x43, 0x21, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0};
		private IVirtualFile vFile;

		public static MMIOHandlerUmd Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MMIOHandlerUmd(BASE_ADDRESS);
				}
				return instance;
			}
		}

		private MMIOHandlerUmd(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			command = stream.readInt();
			reset = stream.readInt();
			interrupt = stream.readInt();
			interruptEnabled = stream.readInt();
			totalTransferLength = stream.readInt();
			stream.readInts(transferAddresses);
			stream.readInts(transferSizes);
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(command);
			stream.writeInt(reset);
			stream.writeInt(interrupt);
			stream.writeInt(interruptEnabled);
			stream.writeInt(totalTransferLength);
			stream.writeInts(transferAddresses);
			stream.writeInts(transferSizes);
			base.write(stream);
		}

		private void closeFile()
		{
			if (vFile != null)
			{
				vFile.ioClose();
				vFile = null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void switchUmd(String fileName) throws java.io.IOException
		public virtual void switchUmd(string fileName)
		{
			closeFile();

			log.info(string.Format("Using UMD '{0}'", fileName));

			vFile = new UmdIsoReaderVirtualFile(fileName);
		}

		private int Reset
		{
			set
			{
				this.reset = value;
    
				if ((value & 0x1) != 0)
				{
					MMIOHandlerGpio.Instance.Port = GPIO_PORT_BLUETOOTH;
					MMIOHandlerGpio.Instance.Port = GPIO_PORT_UMD;
				}
			}
		}

		private int Command
		{
			set
			{
				this.command = value;
    
				if (log.DebugEnabled)
				{
					log.debug(string.Format("MMIOHandlerUmd.setCommand command 0x{0:X}", value));
				}
    
				switch (value & 0xFF)
				{
					case 0x01:
						interrupt |= 0x1;
						break;
					case 0x02:
						interrupt |= 0x1;
						break;
					case 0x03:
						interrupt |= 0x1;
						break;
					case 0x04:
						for (int i = 0; i < QTGP2.Length && i < transferSizes[0]; i++)
						{
							Memory.write8(transferAddresses[0] + i, (sbyte) QTGP2[i]);
						}
						interrupt |= 0x1;
						break;
					case 0x05:
						for (int i = 0; i < QTGP3.Length && i < transferSizes[0]; i++)
						{
							Memory.write8(transferAddresses[0] + i, (sbyte) QTGP3[i]);
						}
						interrupt |= 0x1;
						break;
					case 0x08:
						if (log.DebugEnabled)
						{
							log.debug(string.Format("MMIOHandlerUmd.setCommand command=0x{0:X}, transferLength=0x{1:X}", value, totalTransferLength));
						}
						TPointer result = new TPointer(Memory, transferAddresses[0]);
						result.setValue32(0, 0x12345678);
						result.setValue32(0, 0x00000000);
						// Number of region entries
						result.setValue32(12, 2);
						// Each region entry has 24 bytes
						const int regionSize = 24;
						TPointer region = new TPointer(result, 40);
    
						region.clear(regionSize);
						// Take any region code found in the IdStorage page 0x102
						region.setValue32(0, sceNand.regionCodes[2]); // Region code
						region.setValue32(4, sceNand.regionCodes[3]);
						region.add(regionSize);
    
						region.clear(regionSize);
						// Last region entry must be 1, 0
						region.setValue32(0, 1);
						region.setValue32(4, 0);
    
						interrupt |= 0x1;
						MMIOHandlerAta.Instance.packetCommandCompleted();
						break;
					case 0x09:
						interrupt |= 0x1;
						break;
					case 0x0A: // Called after ATA_CMD_OP_READ_BIG to read the data
						int lba = MMIOHandlerAta.Instance.LogicalBlockAddress;
						if (log.DebugEnabled)
						{
							log.debug(string.Format("MMIOHandlerUmd.setCommand command=0x{0:X}, transferLength=0x{1:X}, lba=0x{2:X}", value, totalTransferLength, lba));
						}
    
						if (vFile == null)
						{
							log.error(string.Format("MMIOHandlerUmd no UMD loaded"));
						}
						else
						{
							int fileLength = totalTransferLength / (sectorLength + 0x10) * sectorLength;
							long offset = (lba & 0xFFFFFFFFL) * sectorLength;
							long seekResult = vFile.ioLseek(offset);
							if (seekResult < 0)
							{
								log.error(string.Format("MMIOHandlerUmd.setCommand seek error 0x{0:X8}", seekResult));
							}
							else if (seekResult != offset)
							{
								log.error(string.Format("MMIOHandlerUmd.setCommand incorrect seek: offset=0x{0:X}, seekResult=0x{1:X}", offset, seekResult));
							}
							else
							{
								for (int i = 0; fileLength > 0 && i < transferAddresses.Length; i++)
								{
									int transferLength = transferSizes[i];
									if (transferLength > 0)
									{
										TPointer addr = new TPointer(Memory, transferAddresses[i]);
										int readResult = vFile.ioRead(addr, transferLength);
										if (readResult < 0)
										{
											log.error(string.Format("MMIOHandlerUmd.setCommand read error 0x{0:X8}", readResult));
											break;
										}
										else
										{
											if (readResult != transferLength)
											{
												log.error(string.Format("MMIOHandlerUmd.setCommand uncomplete read: transferLength=0x{0:X}, readLength=0x{1:X}", transferLength, readResult));
												break;
											}
    
											if (log.TraceEnabled)
											{
												log.trace(string.Format("MMIOHandlerUmd.setCommand read 0x{0:X} bytes: {1}", readResult, Utilities.getMemoryDump(addr.Address, readResult)));
											}
										}
										fileLength -= transferLength;
									}
								}
							}
						}
    
						interrupt |= 0x1;
						MMIOHandlerAta.Instance.packetCommandCompleted();
						break;
					case 0x0B:
						break;
					default:
						log.error(string.Format("MMIOHandlerUmd.setCommand unknown command 0x{0:X}", value));
						break;
				}
    
				checkInterrupt();
			}
		}

		private void checkInterrupt()
		{
			if ((interrupt & interruptEnabled) != 0)
			{
				RuntimeContextLLE.triggerInterrupt(Processor, PSP_UMD_INTR);
			}
			else
			{
				RuntimeContextLLE.clearInterrupt(Processor, PSP_UMD_INTR);
			}
		}

		private void clearInterrupt(int interrupt)
		{
			this.interrupt &= ~interrupt;

			checkInterrupt();
		}

		private void enableInterrupt(int interruptEnabled)
		{
			this.interruptEnabled |= interruptEnabled;

			checkInterrupt();
		}

		private void disableInterrupt(int interruptEnabled)
		{
			this.interruptEnabled &= ~interruptEnabled;

			checkInterrupt();
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x08:
					value = reset;
					break;
				case 0x10:
					value = 0;
					break; // Unknown value
					goto case 0x14;
				case 0x14:
					value = 0;
					break; // Unknown value
					goto case 0x18;
				case 0x18:
					value = 0;
					break; // flags 0x10 and 0x100 are being tested
					goto case 0x1C;
				case 0x1C:
					value = 0;
					break; // Tests: (value & 0x1) != 0 (meaning timeout occured?), value < 0x11
					goto case 0x20;
				case 0x20:
					value = interrupt;
					break;
				case 0x24:
					value = 0;
					break; // Unknown value
					goto case 0x28;
				case 0x28:
					value = interruptEnabled;
					break; // Unknown value
					goto case 0x2C;
				case 0x2C:
					value = 0;
					break; // Unknown value
					goto case 0x30;
				case 0x30:
					value = 0;
					break; // Unknown value, error code?
					goto case 0x38;
				case 0x38:
					value = 0;
					break; // Unknown value
					goto case 0x90;
				case 0x90:
					value = totalTransferLength;
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
				case 0x08:
					Reset = value;
					break;
				case 0x10:
					Command = value;
					break;
				case 0x24:
					clearInterrupt(value);
					break; // Not sure about the meaning
					goto case 0x28;
				case 0x28:
					enableInterrupt(value);
					break;
				case 0x2C:
					disableInterrupt(value);
					break; // Not sure about the meaning
					goto case 0x30;
				case 0x30:
					if (value != 0x4)
					{
						base.write32(address, value);
					}
					break; // Unknown value
					goto case 0x38;
				case 0x38:
					if (value != 0x4)
					{
						base.write32(address, value);
					}
					break; // Unknown value
					goto case 0x40;
				case 0x40:
					transferAddresses[0] = value;
					break;
				case 0x44:
					transferSizes[0] = value;
					break;
				case 0x48:
					transferAddresses[1] = value;
					break;
				case 0x4C:
					transferSizes[1] = value;
					break;
				case 0x50:
					transferAddresses[2] = value;
					break;
				case 0x54:
					transferSizes[2] = value;
					break;
				case 0x58:
					transferAddresses[3] = value;
					break;
				case 0x5C:
					transferSizes[3] = value;
					break;
				case 0x60:
					transferAddresses[4] = value;
					break;
				case 0x64:
					transferSizes[4] = value;
					break;
				case 0x68:
					transferAddresses[5] = value;
					break;
				case 0x6C:
					transferSizes[5] = value;
					break;
				case 0x70:
					transferAddresses[6] = value;
					break;
				case 0x74:
					transferSizes[6] = value;
					break;
				case 0x78:
					transferAddresses[7] = value;
					break;
				case 0x7C:
					transferSizes[7] = value;
					break;
				case 0x80:
					transferAddresses[8] = value;
					break;
				case 0x84:
					transferSizes[8] = value;
					break;
				case 0x88:
					transferAddresses[9] = value;
					break;
				case 0x8C:
					transferSizes[9] = value;
					break;
				case 0x90:
					totalTransferLength = value;
					break;
				case 0x94:
					if (value != 1)
					{
						base.write32(address, value);
					}
					break; // Unknown value, possible values: 0, 1
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
	}

}