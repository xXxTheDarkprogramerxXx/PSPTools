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
//	import static pspsharp.HLE.Modules.sceDisplayModule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_GE_INTR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.NativeUtils.CTRL_ACTIVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.NativeUtils.INTR_STAT_END;

	using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using sceGe_user = pspsharp.HLE.modules.sceGe_user;
	using GeCommands = pspsharp.graphics.GeCommands;
	using CoreThreadMMIO = pspsharp.graphics.RE.externalge.CoreThreadMMIO;
	using ExternalGE = pspsharp.graphics.RE.externalge.ExternalGE;
	using NativeUtils = pspsharp.graphics.RE.externalge.NativeUtils;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerGe : MMIOHandlerBase
	{
		public static new Logger log = sceGe_user.log;
		private const int STATE_VERSION = 0;
		public const int BASE_ADDRESS = unchecked((int)0xBD400000);
		private static MMIOHandlerGe instance;
		private int ctrl;
		private int status;
		private int list;
		private int stall;
		private int raddr1;
		private int raddr2;
		private int vaddr;
		private int iaddr;
		private int oaddr;
		private int oaddr1;
		private int oaddr2;
		private int cmdStatus;
		private int interrupt;

		public static MMIOHandlerGe Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MMIOHandlerGe(BASE_ADDRESS);
				}
				return instance;
			}
		}

		private MMIOHandlerGe(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			Status = stream.readInt();
			List = stream.readInt();
			Stall = stream.readInt();
			Raddr1 = stream.readInt();
			Raddr2 = stream.readInt();
			Vaddr = stream.readInt();
			Iaddr = stream.readInt();
			Oaddr = stream.readInt();
			Oaddr1 = stream.readInt();
			Oaddr2 = stream.readInt();
			CmdStatus = stream.readInt();
			Interrupt = stream.readInt();
			for (int cmd = 0x00; cmd <= 0xFF; cmd++)
			{
				int value = stream.readInt();
				writeGeCmd(cmd, value);
			}
			for (int i = 0; i < 8 * 12; i++)
			{
				writeGeBone(i, stream.readInt());
			}
			for (int i = 0; i < 12; i++)
			{
				writeGeWorld(i, stream.readInt());
			}
			for (int i = 0; i < 12; i++)
			{
				writeGeView(i, stream.readInt());
			}
			for (int i = 0; i < 16; i++)
			{
				writeGeProjection(i, stream.readInt());
			}
			for (int i = 0; i < 12; i++)
			{
				writeGeTexture(i, stream.readInt());
			}
			// Setting the ctrl must be the last action as it might trigger a GE list execution
			Ctrl = stream.readInt();
			base.read(stream);

			sceDisplayModule.GeDirty = true;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(Status);
			stream.writeInt(List);
			stream.writeInt(Stall);
			stream.writeInt(Raddr1);
			stream.writeInt(Raddr2);
			stream.writeInt(Vaddr);
			stream.writeInt(Iaddr);
			stream.writeInt(Oaddr);
			stream.writeInt(Oaddr1);
			stream.writeInt(Oaddr2);
			stream.writeInt(CmdStatus);
			stream.writeInt(Interrupt);
			for (int cmd = 0x00; cmd <= 0xFF; cmd++)
			{
				stream.writeInt(readGeCmd(cmd));
			}
			for (int i = 0; i < 8 * 12; i++)
			{
				stream.writeInt(readGeBone(i));
			}
			for (int i = 0; i < 12; i++)
			{
				stream.writeInt(readGeWorld(i));
			}
			for (int i = 0; i < 12; i++)
			{
				stream.writeInt(readGeView(i));
			}
			for (int i = 0; i < 16; i++)
			{
				stream.writeInt(readGeProjection(i));
			}
			for (int i = 0; i < 12; i++)
			{
				stream.writeInt(readGeTexture(i));
			}
			stream.writeInt(Ctrl);
			base.write(stream);
		}

		public virtual void onGeInterrupt()
		{
			checkInterrupt();
			sceDisplayModule.GeDirty = true;
		}

		private int Status
		{
			get
			{
				if (ExternalGE.Active)
				{
					status = NativeUtils.CoreStat;
				}
				return status;
			}
			set
			{
				this.status = value;
				if (ExternalGE.Active)
				{
					NativeUtils.CoreStat = value;
				}
			}
		}


		private int List
		{
			get
			{
				if (ExternalGE.Active)
				{
					list = NativeUtils.CoreMadr;
				}
				return list;
			}
			set
			{
				this.list = value;
				if (ExternalGE.Active)
				{
					NativeUtils.CoreMadr = value;
				}
			}
		}


		private int Stall
		{
			get
			{
				if (ExternalGE.Active)
				{
					stall = NativeUtils.CoreSadr;
				}
				return stall;
			}
			set
			{
				this.stall = value;
				if (ExternalGE.Active)
				{
					NativeUtils.CoreSadr = value;
					CoreThreadMMIO.Instance.sync();
				}
			}
		}


		private int Raddr1
		{
			get
			{
				if (ExternalGE.Active)
				{
					raddr1 = NativeUtils.CoreRadr1;
				}
				return raddr1;
			}
			set
			{
				this.raddr1 = value;
				if (ExternalGE.Active)
				{
					NativeUtils.CoreRadr1 = value;
				}
			}
		}


		private int Raddr2
		{
			get
			{
				if (ExternalGE.Active)
				{
					raddr2 = NativeUtils.CoreRadr2;
				}
				return raddr2;
			}
			set
			{
				this.raddr2 = value;
				if (ExternalGE.Active)
				{
					NativeUtils.CoreRadr2 = value;
				}
			}
		}


		private int Vaddr
		{
			get
			{
				if (ExternalGE.Active)
				{
					vaddr = NativeUtils.CoreVadr;
				}
				return vaddr;
			}
			set
			{
				this.vaddr = value;
				if (ExternalGE.Active)
				{
					NativeUtils.CoreVadr = value;
				}
			}
		}


		private int Iaddr
		{
			get
			{
				if (ExternalGE.Active)
				{
					iaddr = NativeUtils.CoreIadr;
				}
				return iaddr;
			}
			set
			{
				this.iaddr = value;
				if (ExternalGE.Active)
				{
					NativeUtils.CoreIadr = value;
				}
			}
		}


		private int Oaddr
		{
			get
			{
				if (ExternalGE.Active)
				{
					oaddr = NativeUtils.CoreOadr;
				}
				return oaddr;
			}
			set
			{
				this.oaddr = value;
				if (ExternalGE.Active)
				{
					NativeUtils.CoreOadr = value;
				}
			}
		}


		private int Oaddr1
		{
			get
			{
				if (ExternalGE.Active)
				{
					oaddr1 = NativeUtils.CoreOadr1;
				}
				return oaddr1;
			}
			set
			{
				this.oaddr1 = value;
				if (ExternalGE.Active)
				{
					NativeUtils.CoreOadr1 = value;
				}
			}
		}


		private int Oaddr2
		{
			get
			{
				if (ExternalGE.Active)
				{
					oaddr2 = NativeUtils.CoreOadr2;
				}
				return oaddr2;
			}
			set
			{
				this.oaddr2 = value;
				if (ExternalGE.Active)
				{
					NativeUtils.CoreOadr2 = value;
				}
			}
		}


		private int CmdStatus
		{
			get
			{
				if (ExternalGE.Active)
				{
					cmdStatus = NativeUtils.CoreIntrStat;
				}
				return cmdStatus;
			}
			set
			{
				this.cmdStatus = value;
				if (ExternalGE.Active)
				{
					NativeUtils.CoreIntrStat = value;
				}
    
				// Clearing some flags from value is also clearing the related interrupt flags
				Interrupt = Interrupt & value;
			}
		}


		private void changeCmdStatus(int mask)
		{
			CmdStatus = cmdStatus ^ mask;
		}

		// All methods reading/writing the interrupt field are synchronized as they can be called from multiple threads
		private int Interrupt
		{
			get
			{
				lock (this)
				{
					if (ExternalGE.Active)
					{
						interrupt = NativeUtils.CoreInterrupt;
					}
					return interrupt;
				}
			}
			set
			{
				lock (this)
				{
					this.interrupt = value;
					if (ExternalGE.Active)
					{
						NativeUtils.CoreInterrupt = value;
					}
            
					checkInterrupt();
				}
			}
		}

		// All methods reading/writing the interrupt field are synchronized as they can be called from multiple threads
		private void checkInterrupt()
		{
			lock (this)
			{
				if ((Interrupt & INTR_STAT_END) == 0)
				{
					RuntimeContextLLE.clearInterrupt(Processor, PSP_GE_INTR);
				}
				else
				{
					RuntimeContextLLE.triggerInterrupt(Processor, PSP_GE_INTR);
				}
			}
		}


		// All methods reading/writing the interrupt field are synchronized as they can be called from multiple threads
		private void changeInterrupt(int mask)
		{
			lock (this)
			{
				Interrupt = Interrupt ^ mask;
			}
		}

		// All methods reading/writing the interrupt field are synchronized as they can be called from multiple threads
		private void clearInterrupt(int mask)
		{
			lock (this)
			{
				Interrupt = Interrupt & ~mask;
			}
		}

		private void startGeList()
		{
			if (ExternalGE.Active)
			{
				NativeUtils.setLogLevel();
				// Update the screen scale only at the start of a new list
				NativeUtils.ScreenScale = ExternalGE.ScreenScale;
				CoreThreadMMIO.Instance.sync();
			}
		}

		private void stopGeList()
		{
			// TODO
		}

		private int Ctrl
		{
			get
			{
				if (ExternalGE.Active)
				{
					ctrl = NativeUtils.CoreCtrl;
				}
				return ctrl;
			}
			set
			{
				int oldCtrl = this.ctrl;
				this.ctrl = value;
    
				if (ExternalGE.Active)
				{
					NativeUtils.CoreCtrl = value;
				}
    
				if ((oldCtrl & CTRL_ACTIVE) != (value & CTRL_ACTIVE))
				{
					if ((value & CTRL_ACTIVE) != 0)
					{
						startGeList();
					}
					else
					{
						stopGeList();
					}
				}
			}
		}


		private int readGeCmd(int cmd)
		{
			int value = 0;
			if (ExternalGE.Active)
			{
				value = ExternalGE.getCmd(cmd);
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("readGeCmd({0})=0x{1:X8}", GeCommands.Instance.getCommandString(cmd), value));
			}

			return value;
		}

		private void writeGeCmd(int cmd, int value)
		{
			if (ExternalGE.Active)
			{
				ExternalGE.setCmd(cmd, value);
				if (GeCommands.pureStateCommands[cmd])
				{
					ExternalGE.interpretCmd(cmd, value);
				}
			}
		}

		private int readMatrix(int matrixType, int matrixIndex)
		{
			float[] matrix = null;
			if (ExternalGE.Active)
			{
				 matrix = ExternalGE.getMatrix(matrixType);
			}

			if (matrix == null || matrixIndex < 0 || matrixIndex >= matrix.Length)
			{
				return 0;
			}

			int value = (int)((uint)Float.floatToRawIntBits(matrix[matrixIndex]) >> 8);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("readMatrix(matrixType={0:D}, matrixIndex={1:D})=0x{2:X}({3:F})", matrixType, matrixIndex, value, matrix[matrixIndex]));
			}

			return value;
		}

		private int readGeBone(int bone)
		{
			return readMatrix(sceGe_user.PSP_GE_MATRIX_BONE0 + (bone / 12), bone % 12);
		}

		private void writeGeBone(int bone, int value)
		{
			if (ExternalGE.Active)
			{
				ExternalGE.setMatrix(sceGe_user.PSP_GE_MATRIX_BONE0 + (bone / 12), bone % 12, Float.intBitsToFloat(value << 8));
			}
		}

		private int readGeWorld(int world)
		{
			return readMatrix(sceGe_user.PSP_GE_MATRIX_WORLD, world);
		}

		private void writeGeWorld(int world, int value)
		{
			if (ExternalGE.Active)
			{
				ExternalGE.setMatrix(sceGe_user.PSP_GE_MATRIX_WORLD, world, Float.intBitsToFloat(value << 8));
			}
		}

		private int readGeView(int view)
		{
			return readMatrix(sceGe_user.PSP_GE_MATRIX_VIEW, view);
		}

		private void writeGeView(int view, int value)
		{
			if (ExternalGE.Active)
			{
				ExternalGE.setMatrix(sceGe_user.PSP_GE_MATRIX_VIEW, view, Float.intBitsToFloat(value << 8));
			}
		}

		private int readGeProjection(int projection)
		{
			return readMatrix(sceGe_user.PSP_GE_MATRIX_PROJECTION, projection);
		}

		private void writeGeProjection(int projection, int value)
		{
			if (ExternalGE.Active)
			{
				ExternalGE.setMatrix(sceGe_user.PSP_GE_MATRIX_PROJECTION, projection, Float.intBitsToFloat(value << 8));
			}
		}

		private int readGeTexture(int texture)
		{
			return readMatrix(sceGe_user.PSP_GE_MATRIX_TEXGEN, texture);
		}

		private void writeGeTexture(int texture, int value)
		{
			if (ExternalGE.Active)
			{
				ExternalGE.setMatrix(sceGe_user.PSP_GE_MATRIX_TEXGEN, texture, Float.intBitsToFloat(value << 8));
			}
		}

		public override int read32(int address)
		{
			int value;
			int localAddress = address - baseAddress;
			switch (localAddress)
			{
				case 0x004:
					value = 0;
					break; // Unknown
					goto case 0x008;
				case 0x008:
					value = MemoryMap.SIZE_VRAM >> 10;
					break;
				case 0x100:
					value = Ctrl;
					break;
				case 0x104:
					value = Status;
					break;
				case 0x108:
					value = List;
					break;
				case 0x10C:
					value = Stall;
					break;
				case 0x110:
					value = Raddr1;
					break;
				case 0x114:
					value = Raddr2;
					break;
				case 0x118:
					value = Vaddr;
					break;
				case 0x11C:
					value = Iaddr;
					break;
				case 0x120:
					value = Oaddr;
					break;
				case 0x124:
					value = Oaddr1;
					break;
				case 0x128:
					value = Oaddr2;
					break;
				case 0x304:
					value = CmdStatus;
					break;
				case 0x308:
					value = Interrupt;
					break;
				case 0x400:
					value = 0;
					break; // Unknown
					goto default;
				default:
					if (localAddress >= 0x800 && localAddress < 0xC00)
					{
						value = readGeCmd((localAddress - 0x800) >> 2);
					}
					else if (localAddress >= 0xC00 && localAddress < 0xD80)
					{
						value = readGeBone((localAddress - 0xC00) >> 2);
					}
					else if (localAddress >= 0xD80 && localAddress < 0xDB0)
					{
						value = readGeWorld((localAddress - 0xD80) >> 2);
					}
					else if (localAddress >= 0xDB0 && localAddress < 0xDE0)
					{
						value = readGeView((localAddress - 0xDB0) >> 2);
					}
					else if (localAddress >= 0xDE0 && localAddress < 0xE20)
					{
						value = readGeProjection((localAddress - 0xDE0) >> 2);
					}
					else if (localAddress >= 0xE20 && localAddress < 0xE50)
					{
						value = readGeTexture((localAddress - 0xE20) >> 2);
					}
					else
					{
						value = base.read32(address);
					}
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
				case 0x100:
					Ctrl = value;
					break;
				case 0x104:
					Status = value;
					break;
				case 0x108:
					List = value;
					break;
				case 0x10C:
					Stall = value;
					break;
				case 0x110:
					Raddr1 = value;
					break;
				case 0x114:
					Raddr2 = value;
					break;
				case 0x118:
					Vaddr = value;
					break;
				case 0x11C:
					Iaddr = value;
					break;
				case 0x120:
					Oaddr = value;
					break;
				case 0x124:
					Oaddr1 = value;
					break;
				case 0x128:
					Oaddr2 = value;
					break;
				case 0x304:
					CmdStatus = value;
					break;
				case 0x308:
					clearInterrupt(value);
					break;
				case 0x30C:
					changeInterrupt(value);
					break;
				case 0x310:
					changeCmdStatus(value);
					break;
				case 0x400:
					break; // Unknown
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

		public override string ToString()
		{
			return string.Format("MMIOHandlerGe ctrl=0x{0:X}, status=0x{1:X}, list=0x{2:X8}, interrupt=0x{3:X}, cmdStatus=0x{4:X}", ctrl, status, list, interrupt, cmdStatus);
		}
	}

}