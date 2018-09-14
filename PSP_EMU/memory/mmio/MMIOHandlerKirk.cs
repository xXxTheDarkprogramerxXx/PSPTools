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
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_MEMLMD_INTR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_CERT_VERIFY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_DECRYPT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_DECRYPT_FUSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_DECRYPT_PRIVATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_ECDSA_GEN_KEYS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_ECDSA_MULTIPLY_POINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_ECDSA_SIGN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_ECDSA_VERIFY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_ENCRYPT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_ENCRYPT_FUSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_INIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_PRIV_SIG_CHECK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_PRNG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_CMD_SHA1_HASH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.KIRK.PSP_KIRK_INVALID_OPERATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.MMIO.normalizeAddress;


	using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using Modules = pspsharp.HLE.Modules;
	using TPointer = pspsharp.HLE.TPointer;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using semaphore = pspsharp.HLE.modules.semaphore;
	using Scheduler = pspsharp.scheduler.Scheduler;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;
	using Utilities = pspsharp.util.Utilities;

	public class MMIOHandlerKirk : MMIOHandlerBase
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			completePhase1Action = new CompletePhase1Action(this);
		}

		private static new Logger log = semaphore.log;
		private const int STATE_VERSION = 0;
		public const int RESULT_SUCCESS = 0;
		public const int STATUS_PHASE1_IN_PROGRESS = 0x00;
		public const int STATUS_PHASE1_COMPLETED = 0x01;
		public const int STATUS_PHASE1_ERROR = 0x10;
		public const int STATUS_PHASE1_MASK = STATUS_PHASE1_COMPLETED | STATUS_PHASE1_ERROR;
		public const int STATUS_PHASE2_IN_PROGRESS = 0x00;
		public const int STATUS_PHASE2_COMPLETED = 0x02;
		public const int STATUS_PHASE2_ERROR = 0x20;
		public const int STATUS_PHASE2_MASK = STATUS_PHASE2_COMPLETED | STATUS_PHASE2_ERROR;
		private static int dumpIndex = 0;
		private readonly int signature = 0x4B52494B; // "KIRK"
		private readonly int version = 0x30313030; // "0010"
		private int error;
		private int command;
		private int result = RESULT_SUCCESS;
		private int status = STATUS_PHASE1_IN_PROGRESS | STATUS_PHASE2_IN_PROGRESS;
		private int statusAsync;
		private int statusAsyncEnd;
		private int sourceAddr;
		private int destAddr;
		private CompletePhase1Action completePhase1Action;
		private long completePhase1Schedule = 0L;
		private static readonly int[] commandsToBeDumped = new int[] { };

		private class CompletePhase1Action : IAction
		{
			private readonly MMIOHandlerKirk outerInstance;

			public CompletePhase1Action(MMIOHandlerKirk outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.completePhase1();
			}
		}

		public MMIOHandlerKirk(int baseAddress) : base(baseAddress)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			error = stream.readInt();
			command = stream.readInt();
			result = stream.readInt();
			status = stream.readInt();
			statusAsync = stream.readInt();
			statusAsyncEnd = stream.readInt();
			sourceAddr = stream.readInt();
			destAddr = stream.readInt();
			base.read(stream);

			completePhase1Schedule = 0L;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(error);
			stream.writeInt(command);
			stream.writeInt(result);
			stream.writeInt(status);
			stream.writeInt(statusAsync);
			stream.writeInt(statusAsyncEnd);
			stream.writeInt(sourceAddr);
			stream.writeInt(destAddr);
			base.write(stream);
		}

		public override int read32(int address)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read32(0x{1:X8}) on {2}", Pc, address, this));
			}

			switch (address - baseAddress)
			{
				case 0x00:
					return signature;
				case 0x04:
					return version;
				case 0x08:
					return error;
				case 0x10:
					return command;
				case 0x14:
					return result;
				case 0x1C:
					return Status;
				case 0x20:
					return statusAsync;
				case 0x24:
					return statusAsyncEnd;
				case 0x2C:
					return sourceAddr;
				case 0x30:
					return destAddr;
			}
			return base.read32(address);
		}

		public override void write32(int address, int value)
		{
			switch (address - baseAddress)
			{
				case 0x08:
					error = value;
					break;
				case 0x0C:
					startProcessing(value);
					break;
				case 0x10:
					command = value;
					break;
				case 0x14:
					result = value;
					break;
				case 0x1C:
					status = value;
					break;
				case 0x20:
					statusAsync = value;
					break;
				case 0x24:
					statusAsyncEnd = value;
					break;
				case 0x28:
					endProcessing(value);
					break;
				case 0x2C:
					sourceAddr = value;
					break;
				case 0x30:
					destAddr = value;
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

		private int hleUtilsBufferCopyWithRange()
		{
			TPointer outAddr = new TPointer(Memory, normalizeAddress(destAddr));
			TPointer inAddr = new TPointer(Memory, normalizeAddress(sourceAddr));

			int inSize;
			int outSize;
			int dataSize;
			int dataOffset;
			switch (command)
			{
				case PSP_KIRK_CMD_ENCRYPT:
				case PSP_KIRK_CMD_ENCRYPT_FUSE:
					// AES128_CBC_Header
					dataSize = inAddr.getValue32(16);
					inSize = dataSize + 20;
					outSize = dataSize + 20;
					break;
				case PSP_KIRK_CMD_DECRYPT:
				case PSP_KIRK_CMD_DECRYPT_FUSE:
					// AES128_CBC_Header
					dataSize = inAddr.getValue32(16);
					inSize = dataSize + 20;
					outSize = dataSize;
					break;
				case PSP_KIRK_CMD_DECRYPT_PRIVATE:
					// AES128_CMAC_Header
					dataSize = inAddr.getValue32(112);
					dataOffset = inAddr.getValue32(116);
					inSize = 144 + Utilities.alignUp(dataSize, 15) + dataOffset;
					outSize = Utilities.alignUp(dataSize, 15);
					break;
				case PSP_KIRK_CMD_PRIV_SIG_CHECK:
					// AES128_CMAC_Header
					dataSize = inAddr.getValue32(112);
					dataOffset = inAddr.getValue32(116);
					inSize = 144 + Utilities.alignUp(dataSize, 15) + dataOffset;
					outSize = 0;
					break;
				case PSP_KIRK_CMD_SHA1_HASH:
					// SHA1_Header
					inSize = inAddr.getValue32(0) + 4;
					outSize = 20;
					break;
				case PSP_KIRK_CMD_ECDSA_GEN_KEYS:
					inSize = 0;
					outSize = 0x3C;
					break;
				case PSP_KIRK_CMD_ECDSA_MULTIPLY_POINT:
					inSize = 0x3C;
					outSize = 0x28;
					break;
				case PSP_KIRK_CMD_PRNG:
					inSize = 0;
					outSize = 0x10; // TODO Unknown outSize?
					break;
				case PSP_KIRK_CMD_ECDSA_SIGN:
					inSize = 0x34;
					outSize = 0x28;
					break;
				case PSP_KIRK_CMD_ECDSA_VERIFY:
					inSize = 0x64;
					outSize = 0;
					break;
				case PSP_KIRK_CMD_INIT:
					inSize = 0;
					outSize = 0;
					break;
				case PSP_KIRK_CMD_CERT_VERIFY:
					inSize = 0xB8;
					outSize = 0;
					break;
				default:
					log.error(string.Format("MMIOHandlerKirk.hleUtilsBufferCopyWithRange unimplemented KIRK command 0x{0:X}", command));
					result = PSP_KIRK_INVALID_OPERATION;
					return 0;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleUtilsBufferCopyWithRange input: {0}", Utilities.getMemoryDump(inAddr, inSize)));
			}

			foreach (int commandToBeDumped in commandsToBeDumped)
			{
				if (command == commandToBeDumped)
				{
					string dumpFileName = string.Format("dump.hleUtilsBufferCopyWithRange.{0:D}", dumpIndex++);
					log.warn(string.Format("MMIOHandlerKirk: hleUtilsBufferCopyWithRange dumping command=0x{0:X}, outputSize=0x{1:X}, inputSize=0x{2:X}, input dumped into file '{3}'", command, outSize, inSize, dumpFileName));
					try
					{
						System.IO.Stream dump = new System.IO.FileStream(dumpFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
						sbyte[] inputBuffer = new sbyte[inSize];
						for (int i = 0; i < inSize; i++)
						{
							inputBuffer[i] = inAddr.getValue8(i);
						}
						dump.Write(inputBuffer, 0, inputBuffer.Length);
						dump.Close();
					}
					catch (IOException)
					{
					}
				}
			}

			result = Modules.semaphoreModule.hleUtilsBufferCopyWithRange(outAddr, outSize, inAddr, inSize, command);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleUtilsBufferCopyWithRange result=0x{0:X}, output: {1}", result, Utilities.getMemoryDump(outAddr, outSize)));
			}

			if (result != 0)
			{
				string dumpFileName = string.Format("dump.hleUtilsBufferCopyWithRange.{0:D}", dumpIndex++);
				log.warn(string.Format("MMIOHandlerKirk: hleUtilsBufferCopyWithRange returned error result=0x{0:X} for command=0x{1:X}, outputSize=0x{2:X}, inputSize=0x{3:X}, input dumped into file '{4}'", result, command, outSize, inSize, dumpFileName));
				try
				{
					System.IO.Stream dump = new System.IO.FileStream(dumpFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
					sbyte[] inputBuffer = new sbyte[inSize];
					for (int i = 0; i < inSize; i++)
					{
						inputBuffer[i] = inAddr.getValue8(i);
					}
					dump.Write(inputBuffer, 0, inputBuffer.Length);
					dump.Close();
				}
				catch (IOException)
				{
				}
			}

			return System.Math.Max(inSize, outSize);
		}

		private void completePhase1()
		{
			if (completePhase1Schedule != 0L)
			{
				Scheduler.Instance.removeAction(completePhase1Schedule, completePhase1Action);
				completePhase1Schedule = 0L;
			}

			setStatus(STATUS_PHASE1_MASK, STATUS_PHASE1_COMPLETED);
			RuntimeContextLLE.triggerInterrupt(Processor, PSP_MEMLMD_INTR);
		}

		private void startProcessing(int value)
		{
			switch (value)
			{
				case 1:
					setStatus(STATUS_PHASE1_MASK, STATUS_PHASE1_IN_PROGRESS);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("KIRK startProcessing 1 on {0}", this));
					}

					int size = hleUtilsBufferCopyWithRange();

					if (result == 0)
					{
						// Duration: 360 us per 0x1000 bytes data
						int delayUs = System.Math.Max(0, size * 360 / 0x1000);
						completePhase1Schedule = Scheduler.Now + delayUs;
						Scheduler.Instance.addAction(completePhase1Schedule, completePhase1Action);
						if (log.DebugEnabled)
						{
							log.debug(string.Format("KIRK delaying completion of phase 1 by {0:D} us", delayUs));
						}
					}
					else
					{
						completePhase1();
					}
					break;
				case 2:
					setStatus(STATUS_PHASE2_MASK, STATUS_PHASE2_IN_PROGRESS);
					log.error(string.Format("Unimplemented Phase 2 KIRK command 0x{0:X} on {1}", command, this));
					log.error(string.Format("source: {0}", Utilities.getMemoryDump(Memory, normalizeAddress(sourceAddr), 0x100)));
					break;
				default:
					log.warn(string.Format("0x{0:X8} - KIRK unknown startProcessing value 0x{1:X} on {2}", Pc, value, this));
					break;
			}
		}

		private void endProcessing(int value)
		{
			switch (value)
			{
				case 1:
					if (log.DebugEnabled)
					{
						log.debug(string.Format("KIRK endProcessing 1 on {0}", this));
					}
					RuntimeContextLLE.clearInterrupt(Processor, PSP_MEMLMD_INTR);
					break;
				case 2:
					if (log.DebugEnabled)
					{
						log.debug(string.Format("KIRK endProcessing 2 on {0}", this));
					}
					break;
				default:
					log.warn(string.Format("0x{0:X8} - KIRK unknown endProcessing value 0x{1:X} on {2}", Pc, value, this));
					break;
			}
		}

		private void setStatus(int mask, int value)
		{
			status = (status & ~mask) | value;
		}

		private void updateStatus()
		{
			if (completePhase1Schedule != 0L)
			{
				if (completePhase1Schedule <= Scheduler.Now)
				{
					completePhase1();
				}
			}
		}

		private int Status
		{
			get
			{
				updateStatus();
    
				return status;
			}
		}

		public override string ToString()
		{
			return string.Format("KIRK error=0x{0:X}, command=0x{1:X}, result=0x{2:X}, status=0x{3:X}, statusAsync=0x{4:X}, statusAsyncEnd=0x{5:X}, sourceAddr=0x{6:X8}, destAddr=0x{7:X8}", error, command, result, status, statusAsync, statusAsyncEnd, sourceAddr, destAddr);
		}
	}

}