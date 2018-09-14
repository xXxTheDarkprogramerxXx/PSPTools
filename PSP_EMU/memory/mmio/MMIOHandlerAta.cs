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
//	import static pspsharp.Allegrex.compiler.RuntimeContextLLE.clearInterrupt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.RuntimeContextLLE.triggerInterrupt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_ATA_INTR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.filesystems.umdiso.ISectorDevice_Fields.sectorLength;


	using Logger = org.apache.log4j.Logger;

	using IAction = pspsharp.HLE.kernel.types.IAction;
	using sceAta = pspsharp.HLE.modules.sceAta;
	using Scheduler = pspsharp.scheduler.Scheduler;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	/// <summary>
	/// See "ATA Packet Interface for CD-ROMs SFF-8020i" and ATAPI-4 specification
	/// (http://www.t13.org/documents/UploadedDocuments/project/d1153r18-ATA-ATAPI-4.pdf)
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class MMIOHandlerAta : MMIOHandlerBase
	{
		public static new Logger log = sceAta.log;
		private const int STATE_VERSION = 0;
		private static MMIOHandlerAta instance;
		public const int BASE_ADDRESS = unchecked((int)0xBD700000);
		public const int ATA_STATUS_ERROR = 0x01;
		public const int ATA_STATUS_DATA_REQUEST = 0x08;
		public const int ATA_STATUS_DEVICE_READY = 0x40;
		public const int ATA_STATUS_BUSY = 0x80;
		public const int ATA_INTERRUPT_REASON_CoD = 0x01;
		public const int ATA_INTERRUPT_REASON_IO = 0x02;
		public const int ATA_CONTROL_SOFT_RESET = 0x04;
		public const int ATA_CMD_PACKET = 0xA0;
		public const int ATA_CMD_OP_TEST_UNIT_READY = 0x00;
		public const int ATA_CMD_OP_REQUEST_SENSE = 0x03;
		public const int ATA_CMD_OP_INQUIRY = 0x12;
		public const int ATA_CMD_OP_START_STOP = 0x1B;
		public const int ATA_CMD_OP_PREVENT_ALLOW = 0x1E;
		public const int ATA_CMD_OP_READ_BIG = 0x28;
		public const int ATA_CMD_OP_SEEK = 0x2B;
		public const int ATA_CMD_OP_READ_POSITION = 0x34;
		public const int ATA_CMD_OP_READ_DISC_INFO = 0x51;
		public const int ATA_CMD_OP_MODE_SELECT_BIG = 0x55;
		public const int ATA_CMD_OP_MODE_SENSE_BIG = 0x5A;
		public const int ATA_CMD_OP_READ_STRUCTURE = 0xAD;
		public const int ATA_CMD_OP_SET_SPEED = 0xBB;
		public const int ATA_CMD_OP_UNKNOWN_F0 = 0xF0;
		public const int ATA_CMD_OP_UNKNOWN_F1 = 0xF1;
		public const int ATA_CMD_OP_UNKNOWN_F7 = 0xF7;
		public const int ATA_CMD_OP_UNKNOWN_FC = 0xFC;
		public const int ATA_INQUIRY_PERIPHERAL_DEVICE_TYPE_CDROM = 0x05;
		public const int ATA_SENSE_KEY_NO_SENSE = 0x0;
		public const int ATA_SENSE_KEY_UNKNOWN9 = 0x9;
		public const int ATA_PAGE_CODE_POWER_CONDITION = 0x1A;
		private readonly int[] data = new int[256];
		private int dataIndex;
		private int dataLength;
		private int error;
		private int features;
		private int sectorCount;
		private int sectorNumber;
		private int cylinderLow;
		private int cylinderHigh;
		private int drive;
		private int status;
		private int command;
		private int control;
		private int pendingOperationCodeParameters;
		private int logicalBlockAddress;

		private class PrepareDataEndAction : IAction
		{
			private readonly MMIOHandlerAta outerInstance;

			internal int allocationLength;

			public PrepareDataEndAction(MMIOHandlerAta outerInstance, int allocationLength)
			{
				this.outerInstance = outerInstance;
				this.allocationLength = allocationLength;
			}

			public virtual void execute()
			{
				outerInstance.prepareDataEnd(allocationLength);
			}
		}

		private class PacketCommandCompletedAction : IAction
		{
			private readonly MMIOHandlerAta outerInstance;

			public PacketCommandCompletedAction(MMIOHandlerAta outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.packetCommandCompleted();
			}
		}

		public static MMIOHandlerAta Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MMIOHandlerAta(BASE_ADDRESS);
				}
				return instance;
			}
		}

		private MMIOHandlerAta(int baseAddress) : base(baseAddress)
		{

			setSignature();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			stream.readInts(data);
			dataIndex = stream.readInt();
			dataLength = stream.readInt();
			error = stream.readInt();
			features = stream.readInt();
			sectorCount = stream.readInt();
			sectorNumber = stream.readInt();
			cylinderLow = stream.readInt();
			cylinderHigh = stream.readInt();
			drive = stream.readInt();
			status = stream.readInt();
			command = stream.readInt();
			control = stream.readInt();
			pendingOperationCodeParameters = stream.readInt();
			logicalBlockAddress = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInts(data);
			stream.writeInt(dataIndex);
			stream.writeInt(dataLength);
			stream.writeInt(error);
			stream.writeInt(features);
			stream.writeInt(sectorCount);
			stream.writeInt(sectorNumber);
			stream.writeInt(cylinderLow);
			stream.writeInt(cylinderHigh);
			stream.writeInt(drive);
			stream.writeInt(status);
			stream.writeInt(command);
			stream.writeInt(control);
			stream.writeInt(pendingOperationCodeParameters);
			stream.writeInt(logicalBlockAddress);
			base.write(stream);
		}

		private static string getCommandName(int command)
		{
			switch (command)
			{
				case ATA_CMD_PACKET:
					return "PACKET";
			}

			return string.Format("UNKNOWN_CMD_0x{0:X2}", command);
		}

		private static string getOperationCodeName(int operationCode)
		{
			switch (operationCode)
			{
				case ATA_CMD_OP_TEST_UNIT_READY:
					return "TEST_UNIT_READY";
				case ATA_CMD_OP_REQUEST_SENSE:
					return "REQUEST_SENSE";
				case ATA_CMD_OP_INQUIRY:
					return "INQUIRY";
				case ATA_CMD_OP_START_STOP:
					return "START_STOP";
				case ATA_CMD_OP_PREVENT_ALLOW:
					return "PREVENT_ALLOW";
				case ATA_CMD_OP_READ_BIG:
					return "READ_BIG";
				case ATA_CMD_OP_SEEK:
					return "SEEK";
				case ATA_CMD_OP_READ_POSITION:
					return "READ_POSITION";
				case ATA_CMD_OP_READ_DISC_INFO:
					return "READ_DISC_INFO";
				case ATA_CMD_OP_MODE_SELECT_BIG:
					return "MODE_SELECT_BIG";
				case ATA_CMD_OP_MODE_SENSE_BIG:
					return "MODE_SENSE_BIG";
				case ATA_CMD_OP_READ_STRUCTURE:
					return "READ_STRUCTURE";
				case ATA_CMD_OP_SET_SPEED:
					return "SET_SPEED";
			}

			return string.Format("UNKNOWN_OP_0x{0:X2}", operationCode);
		}

		private void setSignature()
		{
			// This is the required signature for a device supporting the ATA_CMD_PACKET command
			sectorCount = 0x01;
			sectorNumber = 0x01;
			cylinderLow = 0x14;
			cylinderHigh = 0xEB;
			drive = 0x00;
		}

		public virtual void reset()
		{
			setSignature();
		}

		private int ByteCount
		{
			set
			{
				cylinderLow = value & 0xFF;
				cylinderHigh = (value >> 8) & 0xFF;
			}
		}

		private void setInterruptReason(bool CoD, bool io)
		{
			if (CoD)
			{
				sectorCount |= ATA_INTERRUPT_REASON_CoD;
			}
			else
			{
				sectorCount &= ~ATA_INTERRUPT_REASON_CoD;
			}

			if (io)
			{
				sectorCount |= ATA_INTERRUPT_REASON_IO;
			}
			else
			{
				sectorCount &= ~ATA_INTERRUPT_REASON_IO;
			}
		}

		private int Command
		{
			set
			{
				this.command = value;
				dataIndex = 0;
    
				if (log.DebugEnabled)
				{
					log.debug(string.Format("MMIOHandlerAta.setCommand command 0x{0:X2}({1})", value, getCommandName(this.command)));
				}
    
				switch (value)
				{
					case ATA_CMD_PACKET:
						status |= ATA_STATUS_BUSY;
						setInterruptReason(true, false);
						status |= ATA_STATUS_DATA_REQUEST;
						status &= ~ATA_STATUS_BUSY;
						dataLength = 12;
						pendingOperationCodeParameters = -1;
						break;
					default:
						log.error(string.Format("MMIOHandlerAta.setCommand unknown command 0x{0:X2}", value));
						break;
				}
			}
		}

		private int Control
		{
			set
			{
				this.control = value;
    
				if (log.DebugEnabled)
				{
					log.debug(string.Format("MMIOHandlerAta.setControl control 0x{0:X2}", this.control));
				}
    
				if ((value & ATA_CONTROL_SOFT_RESET) != 0)
				{
					reset();
				}
			}
		}

		private int Features
		{
			set
			{
				this.features = value;
    
				if (log.DebugEnabled)
				{
					log.debug(string.Format("MMIOHandlerAta.setFeatures features 0x{0:X2}", this.features));
				}
			}
		}

		private void writeData16(int data16)
		{
			if (dataIndex < dataLength)
			{
				data[dataIndex++] = data16 & 0xFF;
				if (dataIndex < dataLength)
				{
					data[dataIndex++] = (data16 >> 8) & 0xFF;
				}
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("MMIOHandlerAta.writeData 0x{0:X4}", data16));
			}

			if (dataIndex >= dataLength)
			{
				dataLength = 0;
				dataIndex = 0;
				executeCommand();
			}
		}

		private int Data16
		{
			get
			{
				int data16 = 0;
				if (dataIndex < dataLength)
				{
					data16 = data[dataIndex++];
					if (dataIndex < dataLength)
					{
						data16 |= data[dataIndex++] << 8;
					}
				}
    
				if (log.DebugEnabled)
				{
					log.debug(string.Format("MMIOHandlerAta.getData16 returning 0x{0:X4}", data16));
				}
    
				if (dataIndex >= dataLength)
				{
					packetCommandCompleted();
				}
    
				return data16;
			}
		}

		private void prepareDataInit(int allocationLength)
		{
			dataIndex = 0;
			Arrays.fill(data, 0, allocationLength, 0);
		}

		private void prepareDataEnd(int allocationLength)
		{
			dataLength = System.Math.Min(allocationLength, dataIndex);
			dataIndex = 0;
			ByteCount = dataLength;
			setInterruptReason(false, true);
			status |= ATA_STATUS_DATA_REQUEST;
			status &= ~ATA_STATUS_BUSY;
			triggerInterrupt(Processor, PSP_ATA_INTR);
		}

		private void prepareDataEndWithDelay(int allocationLength, int delayUs)
		{
			if (delayUs <= 0)
			{
				prepareDataEnd(allocationLength);
			}
			else
			{
				Scheduler.Instance.addAction(Scheduler.Now + delayUs, new PrepareDataEndAction(this, allocationLength));
			}
		}

		private void prepareData8(int data8)
		{
			data[dataIndex++] = data8 & 0xFF;
		}

		private void prepareData16(int data16)
		{
			prepareData8(data16 >> 8);
			prepareData8(data16);
		}

		private void prepareData24(int data24)
		{
			prepareData8(data24 >> 16);
			prepareData8(data24 >> 8);
			prepareData8(data24);
		}

		private void prepareData32(int data32)
		{
			prepareData8(data32 >> 24);
			prepareData8(data32 >> 16);
			prepareData8(data32 >> 8);
			prepareData8(data32);
		}

		private void prepareData(string s)
		{
			if (!string.ReferenceEquals(s, null))
			{
				for (int i = 0; i < s.Length; i++)
				{
					prepareData8((int) s[i]);
				}
			}
		}

		public virtual void packetCommandCompleted()
		{
			dataLength = 0;
			dataIndex = 0;
			status &= ~ATA_STATUS_DATA_REQUEST;
			status &= ~ATA_STATUS_BUSY;
			status |= ATA_STATUS_DEVICE_READY;
			setInterruptReason(true, true);
			triggerInterrupt(Processor, PSP_ATA_INTR);
		}

		public virtual void packetCommandCompletedWithDelay(int delayUs)
		{
			if (delayUs <= 0)
			{
				packetCommandCompleted();
			}
			else
			{
				Scheduler.Instance.addAction(Scheduler.Now + delayUs, new PacketCommandCompletedAction(this));
			}
		}

		public virtual int LogicalBlockAddress
		{
			get
			{
				return logicalBlockAddress;
			}
		}

		private void preparePacketCommandParameterList(int parameterListLength, int operationCode)
		{
			dataIndex = 0;
			dataLength = parameterListLength;
			ByteCount = parameterListLength;
			pendingOperationCodeParameters = operationCode;
			setInterruptReason(false, false);
			status &= ~ATA_STATUS_BUSY;
			status |= ATA_STATUS_DATA_REQUEST;
			triggerInterrupt(Processor, PSP_ATA_INTR);
		}

		private void executeCommand()
		{
			status &= ~ATA_STATUS_DATA_REQUEST;
			status |= ATA_STATUS_BUSY;

			switch (command)
			{
				case ATA_CMD_PACKET:
					if (pendingOperationCodeParameters < 0)
					{
						executePacketCommand();
					}
					else
					{
						executePacketCommandParameterList(pendingOperationCodeParameters);
						pendingOperationCodeParameters = -1;
					}
					break;
				default:
					log.error(string.Format("MMIOHandlerAta.executeCommand unknown command 0x{0:X2}", command));
					break;
			}
		}

		private void executePacketCommand()
		{
			int operationCode = data[0];
			int allocationLength;
			int unknown;
			int delayUs;

			switch (operationCode)
			{
				case ATA_CMD_OP_INQUIRY:
					allocationLength = data[4];

					if (log.DebugEnabled)
					{
						log.debug(string.Format("ATA_CMD_OP_INQUIRY allocationLength=0x{0:X}", allocationLength));
					}

					prepareDataInit(allocationLength);
					prepareData8(ATA_INQUIRY_PERIPHERAL_DEVICE_TYPE_CDROM);
					prepareData8(0x80); // Medium is removable
					prepareData8(0x00); // ISO Version, ACMA Version, ANSI Version
					prepareData8(0x32); // ATAPI Version, Response Data Format
					prepareData8(0x5C); // Additional Length (number of bytes following this one)
					prepareData8(0x00); // Reserved
					prepareData8(0x00); // Reserved
					prepareData8(0x00); // Reserved
					prepareData("SCEI    "); // Vendor Identification
					prepareData("UMD ROM DRIVE   "); // Product Identification
					prepareData("    "); // Product Revision Level
					prepareData("1.090 Oct18 ,2004   "); // Vendor-specific
					// Duration 2ms
					prepareDataEndWithDelay(allocationLength, 2000);
					break;
				case ATA_CMD_OP_TEST_UNIT_READY:
					if (log.DebugEnabled)
					{
						log.debug(string.Format("ATA_CMD_OP_TEST_UNIT_READY"));
					}

					// Duration 1ms
					packetCommandCompletedWithDelay(1000);
					break;
				case ATA_CMD_OP_REQUEST_SENSE:
					allocationLength = data[4];

					if (log.DebugEnabled)
					{
						log.debug(string.Format("ATA_CMD_OP_REQUEST_SENSE allocationLength=0x{0:X}", allocationLength));
					}

					prepareDataInit(allocationLength);
					bool mediaPresent = true;
					prepareData8(0x80); // Valid bit, no Error Code
					prepareData8(0x00); // Reserved
					if (mediaPresent)
					{
						prepareData8(ATA_SENSE_KEY_NO_SENSE); // Successful command
					}
					else
					{
						prepareData8(ATA_SENSE_KEY_UNKNOWN9); // Media not present
					}
					prepareData32(0); // Information
					prepareData8(10); // Additional Sense Length
					prepareData32(0); // Command Specific Information
					if (mediaPresent)
					{
						prepareData8(0); // Additional Sense Code
					}
					else
					{
						prepareData8(5); // Additional Sense Code: media not present
					}
					prepareData8(0); // Additional Sense Code Qualifier
					prepareData8(0); // Field Replaceable Unit Code
					prepareData8(0); // SKSV / Sense Key Specific
					prepareData8(0); // Sense Key Specific
					prepareData8(0); // Sense Key Specific
					// Duration 2ms
					prepareDataEndWithDelay(allocationLength, 2000);
					break;
				case ATA_CMD_OP_READ_STRUCTURE:
					allocationLength = data[9] | (data[8] << 8);
					int formatCode = data[7];

					if (log.DebugEnabled)
					{
						log.debug(string.Format("ATA_CMD_OP_READ_STRUCTURE formatCode=0x{0:X}, allocationLength=0x{1:X}", formatCode, allocationLength));
					}

					prepareDataInit(allocationLength);
					delayUs = 0;
					switch (formatCode)
					{
						case 0x00:
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int numberOfSectors = 1800 * (1024 * 1024 / sectorLength);
							int numberOfSectors = 1800 * (1024 * 1024 / sectorLength); // 1.8GB
							const int startingSectorNumber = 0x030000;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int endSectorNumber = startingSectorNumber + numberOfSectors - 1;
							int endSectorNumber = startingSectorNumber + numberOfSectors - 1;
							prepareData16(allocationLength + 4); // DVD Structure Data Length
							prepareData8(0); // Reserved
							prepareData8(0); // Reserved
							prepareData8(0x80); // Book Type / Part Version
							prepareData8(0x00); // Disc Size (1 => 120mm) / Minimum Rate (0 => 2.52 Mbps)
							prepareData8(0x01); // Number of Layers / Track Path / Layer Type (1 => the Layer contains embossed user data area)
							prepareData8(0xE0); // Linear Density / Track Density
							prepareData8(0); // Reserved
							prepareData24(startingSectorNumber); // Starting Sector Number of Main Data (0x030000 is the only valid value)
							prepareData8(0); // Reserved
							prepareData24(endSectorNumber); // End Sector of Main Data
							prepareData8(0); // Reserved
							prepareData24(0x000000); // End Sector Number in Layer 0
							prepareData8(0); // BCA (Burst Cutting Area) Flag
							prepareData8(0x07); // Reserved
							// Duration 6ms
							delayUs = 6000;
							break;
						default:
							log.error(string.Format("ATA_CMD_OP_READ_STRUCTURE unknown formatCode=0x{0:X}", formatCode));
							break;
					}
					prepareDataEndWithDelay(allocationLength, delayUs);
					break;
				case ATA_CMD_OP_UNKNOWN_F0:
					unknown = data[1];

					if (log.DebugEnabled)
					{
						log.debug(string.Format("ATA_CMD_OP_UNKNOWN_F0 unknown=0x{0:X}", unknown));
					}

					// Unknown 1 byte is being returned
					allocationLength = 1;
					prepareDataInit(allocationLength);
					prepareData8(0x08); // Unknown value. The following values are accepted: 0x08, 0x47, 0x48, 0x50
					prepareDataEnd(allocationLength);
					break;
				case ATA_CMD_OP_MODE_SENSE_BIG:
					allocationLength = data[8] | (data[7] << 8);
					int pageCode = data[2] & 0x3F;

					if (log.DebugEnabled)
					{
						log.debug(string.Format("ATA_CMD_OP_MODE_SENSE_BIG pageCode=0x{0:X}, allocationLength=0x{1:X}", pageCode, allocationLength));
					}

					prepareDataInit(allocationLength);
					delayUs = 0;
					switch (pageCode)
					{
						case ATA_PAGE_CODE_POWER_CONDITION:
							prepareData16(26); // Length of following data
							prepareData8(0);
							prepareData8(0);
							prepareData8(0);
							prepareData8(0);
							prepareData8(0);
							prepareData8(0);
							// The following values are unknown, they are returned by a real PSP
							prepareData8(0x9A);
							prepareData8(0x12);
							prepareData8(0);
							prepareData8(0x02);
							prepareData8(0);
							prepareData8(0);
							prepareData8(0);
							prepareData8(0x06);
							prepareData8(0);
							prepareData8(0);
							prepareData8(0);
							prepareData8(0);
							prepareData8(0);
							prepareData8(0);
							prepareData8(0);
							prepareData8(0x04);
							prepareData8(0);
							prepareData8(0);
							prepareData8(0);
							prepareData8(0x04);
							// Duration 2ms
							delayUs = 2000;
							break;
						default:
							log.error(string.Format("ATA_CMD_OP_MODE_SENSE_BIG unknown pageCode=0x{0:X}", pageCode));
							break;
					}
					prepareDataEndWithDelay(allocationLength, delayUs);
					break;
				case ATA_CMD_OP_MODE_SELECT_BIG:
					int parameterListLength = data[8] | (data[7] << 8);
					unknown = data[1];

					if (log.DebugEnabled)
					{
						log.debug(string.Format("ATA_CMD_OP_MODE_SELECT_BIG parameterListLength=0x{0:X}, unknown=0x{1:X}", parameterListLength, unknown));
					}

					preparePacketCommandParameterList(parameterListLength, operationCode);
					break;
				case ATA_CMD_OP_UNKNOWN_F1:
					unknown = data[7] | (data[6] << 8);

					if (log.DebugEnabled)
					{
						log.debug(string.Format("ATA_CMD_OP_UNKNOWN_F1 unknown=0x{0:X}", unknown));
					}

					prepareDataInit(unknown);
					prepareDataEnd(unknown);
					break;
				case ATA_CMD_OP_UNKNOWN_F7:
					unknown = data[2];

					if (log.DebugEnabled)
					{
						log.debug(string.Format("ATA_CMD_OP_UNKNOWN_F7 unknown=0x{0:X}", unknown));
					}

					packetCommandCompleted();
					break;
				case ATA_CMD_OP_UNKNOWN_FC:
					allocationLength = data[8] | (data[7] << 8);

					if (log.DebugEnabled)
					{
						log.debug(string.Format("ATA_CMD_OP_UNKNOWN_FC allocationLength=0x{0:X}", allocationLength));
					}

					prepareDataInit(allocationLength);
					for (int i = 0; i < allocationLength; i++)
					{
						prepareData8(0);
					}
					prepareDataEnd(allocationLength);
					break;
				case ATA_CMD_OP_READ_BIG:
					logicalBlockAddress = data[5] | (data[4] << 8) | (data[3] << 16) | (data[2] << 24);
					int numberOfSectorsToTransfer = data[8] | (data[7] << 8);

					if (log.DebugEnabled)
					{
						log.debug(string.Format("ATA_CMD_OP_READ_BIG logicalBlockAddress=0x{0:X}, numberOfSectorsToTransfer=0x{1:X}", logicalBlockAddress, numberOfSectorsToTransfer));
					}

					prepareDataInit(0);
					// Duration 1ms (TODO duration not verified on a real PSP)
					prepareDataEndWithDelay(0, 1000);
					break;
				default:
					log.error(string.Format("MMIOHandlerAta.executePacketCommand unknown operation code 0x{0:X2}({1})", operationCode, getOperationCodeName(operationCode)));
					break;
			}
		}

		private void executePacketCommandParameterList(int operationCode)
		{
			switch (operationCode)
			{
				case ATA_CMD_OP_MODE_SELECT_BIG:
					int pageCode = data[0] & 0x3F;
					int pageLength = data[1];

					if (pageCode != 0)
					{
						log.error(string.Format("ATA_CMD_OP_MODE_SELECT_BIG parameter unknown pageCode=0x{0:X}", pageCode));
					}
					if (pageLength != 0x1A)
					{
						log.error(string.Format("ATA_CMD_OP_MODE_SELECT_BIG parameter unknown pageLength=0x{0:X}", pageLength));
					}

					if (log.DebugEnabled)
					{
						log.debug(string.Format("ATA_CMD_OP_MODE_SELECT_BIG parameters pageCode=0x{0:X}, pageLength=0x{1:X}", pageCode, pageLength));
					}
					break;
				default:
					log.error(string.Format("MMIOHandlerAta.executePacketCommandParameterList unknown operation code 0x{0:X2}({1})", operationCode, getOperationCodeName(operationCode)));
					break;
			}

			packetCommandCompleted();
		}

		private void endOfData(int value)
		{
			if (value != 0)
			{
				log.error(string.Format("MMIOHandlerAta.endOfData unknown value=0x{0:X2}", value));
			}
		}

		/*
		 * Returns the regular status and clears the interrupt
		 */
		private int RegularStatus
		{
			get
			{
				clearInterrupt(Processor, PSP_ATA_INTR);
				return status;
			}
		}

		/*
		 * Returns the regular status but does not clear the interrupt
		 */
		private int AlternateStatus
		{
			get
			{
				return status;
			}
		}

		public override int read8(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x1:
					value = error;
					break;
				case 0x2:
					value = sectorCount;
					break;
				case 0x3:
					value = sectorNumber;
					break;
				case 0x4:
					value = cylinderLow;
					break;
				case 0x5:
					value = cylinderHigh;
					break;
				case 0x6:
					value = drive;
					break;
				case 0x7:
					value = RegularStatus;
					break;
				case 0xE:
					value = AlternateStatus;
					break;
				default:
					value = base.read8(address);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read8(0x{1:X8}) returning 0x{2:X2}", Pc, address, value));
			}

			return value;
		}

		public override void write8(int address, sbyte value)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int value8 = value & 0xFF;
			int value8 = value & 0xFF;
			switch (address - baseAddress)
			{
				case 0x1:
					Features = value8;
					break;
				case 0x2:
					sectorCount = value8;
					break;
				case 0x3:
					sectorNumber = value8;
					break;
				case 0x4:
					cylinderLow = value8;
					break;
				case 0x5:
					cylinderHigh = value8;
					break;
				case 0x6:
					drive = value8;
					break;
				case 0x7:
					Command = value8;
					break;
				case 0x8:
					endOfData(value8);
					break;
				case 0xE:
					Control = value8;
					break;
				default:
					base.write8(address, value);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write8(0x{1:X8}, 0x{2:X2}) on {3}", Pc, address, value8, this));
			}
		}

		public override void write16(int address, short value)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int value16 = value & 0xFFFF;
			int value16 = value & 0xFFFF;
			switch (address - baseAddress)
			{
				case 0x0:
					writeData16(value16);
					break;
				default:
					base.write16(address, value);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write16(0x{1:X8}, 0x{2:X4}) on {3}", Pc, address, value16, this));
			}
		}

		public override int read16(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x0:
					value = Data16;
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
	}

}