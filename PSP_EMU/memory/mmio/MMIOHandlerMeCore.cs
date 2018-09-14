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
namespace pspsharp.memory.mmio
{

	using Logger = org.apache.log4j.Logger;

	using sceMeCore = pspsharp.HLE.modules.sceMeCore;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerMeCore : MMIOHandlerBase
	{
		public static new Logger log = sceMeCore.log;
		private const int STATE_VERSION = 0;
		public const int BASE_ADDRESS = unchecked((int)0xBFC00600);
		private static MMIOHandlerMeCore instance;
		private int cmd;
		private int unknown;
		private readonly int[] parameters = new int[8];
		private int result;
		private sealed class MECommand
		{
			public static readonly MECommand ME_CMD_VIDEOCODEC_OPEN_TYPE0 = new MECommand("ME_CMD_VIDEOCODEC_OPEN_TYPE0", InnerEnum.ME_CMD_VIDEOCODEC_OPEN_TYPE0, 0x0, 1);
			public static readonly MECommand ME_CMD_VIDEOCODEC_INIT_TYPE0 = new MECommand("ME_CMD_VIDEOCODEC_INIT_TYPE0", InnerEnum.ME_CMD_VIDEOCODEC_INIT_TYPE0, 0x1, 3);
			public static readonly MECommand ME_CMD_VIDEOCODEC_DECODE_TYPE0 = new MECommand("ME_CMD_VIDEOCODEC_DECODE_TYPE0", InnerEnum.ME_CMD_VIDEOCODEC_DECODE_TYPE0, 0x2, 8);
			public static readonly MECommand ME_CMD_VIDEOCODEC_STOP_TYPE0 = new MECommand("ME_CMD_VIDEOCODEC_STOP_TYPE0", InnerEnum.ME_CMD_VIDEOCODEC_STOP_TYPE0, 0x3, 3);
			public static readonly MECommand ME_CMD_VIDEOCODEC_DELETE_TYPE0 = new MECommand("ME_CMD_VIDEOCODEC_DELETE_TYPE0", InnerEnum.ME_CMD_VIDEOCODEC_DELETE_TYPE0, 0x4, 1);
			public static readonly MECommand ME_CMD_VIDEOCODEC_SET_MEMORY_TYPE0 = new MECommand("ME_CMD_VIDEOCODEC_SET_MEMORY_TYPE0", InnerEnum.ME_CMD_VIDEOCODEC_SET_MEMORY_TYPE0, 0x5, 6);
			public static readonly MECommand ME_CMD_VIDEOCODEC_GET_VERSION_TYPE0 = new MECommand("ME_CMD_VIDEOCODEC_GET_VERSION_TYPE0", InnerEnum.ME_CMD_VIDEOCODEC_GET_VERSION_TYPE0, 0x6, 1);
			public static readonly MECommand ME_CMD_AVC_SELECT_CLOCK = new MECommand("ME_CMD_AVC_SELECT_CLOCK", InnerEnum.ME_CMD_AVC_SELECT_CLOCK, 0x7, 1);
			public static readonly MECommand ME_CMD_AVC_POWER_ENABLE = new MECommand("ME_CMD_AVC_POWER_ENABLE", InnerEnum.ME_CMD_AVC_POWER_ENABLE, 0x8, 0);
			public static readonly MECommand ME_CMD_VIDEOCODEC_GET_SEI_TYPE0 = new MECommand("ME_CMD_VIDEOCODEC_GET_SEI_TYPE0", InnerEnum.ME_CMD_VIDEOCODEC_GET_SEI_TYPE0, 0x9, 2);
			public static readonly MECommand ME_CMD_VIDEOCODEC_GET_FRAME_CROP_TYPE0 = new MECommand("ME_CMD_VIDEOCODEC_GET_FRAME_CROP_TYPE0", InnerEnum.ME_CMD_VIDEOCODEC_GET_FRAME_CROP_TYPE0, 0xA, 2);
			public static readonly MECommand ME_CMD_VIDEOCODEC_GET_UNKNOWN_TYPE0 = new MECommand("ME_CMD_VIDEOCODEC_GET_UNKNOWN_TYPE0", InnerEnum.ME_CMD_VIDEOCODEC_GET_UNKNOWN_TYPE0, 0xB, 2);
			public static readonly MECommand ME_CMD_VIDEOCODEC_UNKNOWN_CMD_0x10 = new MECommand("ME_CMD_VIDEOCODEC_UNKNOWN_CMD_0x10", InnerEnum.ME_CMD_VIDEOCODEC_UNKNOWN_CMD_0x10, 0x10, 1);
			public static readonly MECommand ME_CMD_VIDEOCODEC_DECODE_TYPE1 = new MECommand("ME_CMD_VIDEOCODEC_DECODE_TYPE1", InnerEnum.ME_CMD_VIDEOCODEC_DECODE_TYPE1, 0x20, 4);
			public static readonly MECommand ME_CMD_VIDEOCODEC_STOP_TYPE1 = new MECommand("ME_CMD_VIDEOCODEC_STOP_TYPE1", InnerEnum.ME_CMD_VIDEOCODEC_STOP_TYPE1, 0x21, 1);
			public static readonly MECommand ME_CMD_VIDEOCODEC_DELETE_TYPE1 = new MECommand("ME_CMD_VIDEOCODEC_DELETE_TYPE1", InnerEnum.ME_CMD_VIDEOCODEC_DELETE_TYPE1, 0x22, 1);
			public static readonly MECommand ME_CMD_VIDEOCODEC_OPEN_TYPE1_STEP2 = new MECommand("ME_CMD_VIDEOCODEC_OPEN_TYPE1_STEP2", InnerEnum.ME_CMD_VIDEOCODEC_OPEN_TYPE1_STEP2, 0x23, 3);
			public static readonly MECommand ME_CMD_VIDEOCODEC_OPEN_TYPE1 = new MECommand("ME_CMD_VIDEOCODEC_OPEN_TYPE1", InnerEnum.ME_CMD_VIDEOCODEC_OPEN_TYPE1, 0x24, 0);
			public static readonly MECommand ME_CMD_VIDEOCODEC_INIT_TYPE1 = new MECommand("ME_CMD_VIDEOCODEC_INIT_TYPE1", InnerEnum.ME_CMD_VIDEOCODEC_INIT_TYPE1, 0x25, 8);
			public static readonly MECommand ME_CMD_VIDEOCODEC_SCAN_HEADER_TYPE1 = new MECommand("ME_CMD_VIDEOCODEC_SCAN_HEADER_TYPE1", InnerEnum.ME_CMD_VIDEOCODEC_SCAN_HEADER_TYPE1, 0x26, 2);
			public static readonly MECommand ME_CMD_VIDEOCODEC_GET_VERSION_TYPE1 = new MECommand("ME_CMD_VIDEOCODEC_GET_VERSION_TYPE1", InnerEnum.ME_CMD_VIDEOCODEC_GET_VERSION_TYPE1, 0x27, 8);
			public static readonly MECommand ME_CMD_AT3P_DECODE = new MECommand("ME_CMD_AT3P_DECODE", InnerEnum.ME_CMD_AT3P_DECODE, 0x60, 1);
			public static readonly MECommand ME_CMD_AT3P_GET_INFO3 = new MECommand("ME_CMD_AT3P_GET_INFO3", InnerEnum.ME_CMD_AT3P_GET_INFO3, 0x61, 0);
			public static readonly MECommand ME_CMD_AT3P_CHECK_NEED_MEM1 = new MECommand("ME_CMD_AT3P_CHECK_NEED_MEM1", InnerEnum.ME_CMD_AT3P_CHECK_NEED_MEM1, 0x63, 4);
			public static readonly MECommand ME_CMD_AT3P_SET_UNK68 = new MECommand("ME_CMD_AT3P_SET_UNK68", InnerEnum.ME_CMD_AT3P_SET_UNK68, 0x64, 2);
			public static readonly MECommand ME_CMD_AT3P_CHECK_NEED_MEM2 = new MECommand("ME_CMD_AT3P_CHECK_NEED_MEM2", InnerEnum.ME_CMD_AT3P_CHECK_NEED_MEM2, 0x66, 3);
			public static readonly MECommand ME_CMD_AT3P_SETUP_CHANNEL = new MECommand("ME_CMD_AT3P_SETUP_CHANNEL", InnerEnum.ME_CMD_AT3P_SETUP_CHANNEL, 0x67, 5);
			public static readonly MECommand ME_CMD_AT3P_CHECK_UNK20 = new MECommand("ME_CMD_AT3P_CHECK_UNK20", InnerEnum.ME_CMD_AT3P_CHECK_UNK20, 0x68, 2);
			public static readonly MECommand ME_CMD_AT3P_SET_UNK44 = new MECommand("ME_CMD_AT3P_SET_UNK44", InnerEnum.ME_CMD_AT3P_SET_UNK44, 0x69, 2);
			public static readonly MECommand ME_CMD_AT3P_GET_INTERNAL_ERROR = new MECommand("ME_CMD_AT3P_GET_INTERNAL_ERROR", InnerEnum.ME_CMD_AT3P_GET_INTERNAL_ERROR, 0x6A, 2);
			public static readonly MECommand ME_CMD_AT3_DECODE = new MECommand("ME_CMD_AT3_DECODE", InnerEnum.ME_CMD_AT3_DECODE, 0x70, 1);
			public static readonly MECommand ME_CMD_AT3_GET_INTERNAL_ERROR = new MECommand("ME_CMD_AT3_GET_INTERNAL_ERROR", InnerEnum.ME_CMD_AT3_GET_INTERNAL_ERROR, 0x71, 2);
			public static readonly MECommand ME_CMD_AT3_CHECK_NEED_MEM = new MECommand("ME_CMD_AT3_CHECK_NEED_MEM", InnerEnum.ME_CMD_AT3_CHECK_NEED_MEM, 0x72, 3);
			public static readonly MECommand ME_CMD_AT3_INIT = new MECommand("ME_CMD_AT3_INIT", InnerEnum.ME_CMD_AT3_INIT, 0x73, 4);
			public static readonly MECommand ME_CMD_AT3_GET_INFO3 = new MECommand("ME_CMD_AT3_GET_INFO3", InnerEnum.ME_CMD_AT3_GET_INFO3, 0x74, 0);
			public static readonly MECommand ME_CMD_MP3_GET_INFO3 = new MECommand("ME_CMD_MP3_GET_INFO3", InnerEnum.ME_CMD_MP3_GET_INFO3, 0x81, 0);
			public static readonly MECommand ME_CMD_MP3_GET_INFO2 = new MECommand("ME_CMD_MP3_GET_INFO2", InnerEnum.ME_CMD_MP3_GET_INFO2, 0x82, 2);
			public static readonly MECommand ME_CMD_MP3_SET_VALUE_FOR_INFO2 = new MECommand("ME_CMD_MP3_SET_VALUE_FOR_INFO2", InnerEnum.ME_CMD_MP3_SET_VALUE_FOR_INFO2, 0x89, 2);
			public static readonly MECommand ME_CMD_MP3_CHECK_NEED_MEM = new MECommand("ME_CMD_MP3_CHECK_NEED_MEM", InnerEnum.ME_CMD_MP3_CHECK_NEED_MEM, 0x8A, 3);
			public static readonly MECommand ME_CMD_MP3_INIT = new MECommand("ME_CMD_MP3_INIT", InnerEnum.ME_CMD_MP3_INIT, 0x8B, 1);
			public static readonly MECommand ME_CMD_MP3_DECODE = new MECommand("ME_CMD_MP3_DECODE", InnerEnum.ME_CMD_MP3_DECODE, 0x8C, 1);
			public static readonly MECommand ME_CMD_AAC_DECODE = new MECommand("ME_CMD_AAC_DECODE", InnerEnum.ME_CMD_AAC_DECODE, 0x90, 5);
			public static readonly MECommand ME_CMD_AAC_GET_INTERNAL_ERROR = new MECommand("ME_CMD_AAC_GET_INTERNAL_ERROR", InnerEnum.ME_CMD_AAC_GET_INTERNAL_ERROR, 0x91, 2);
			public static readonly MECommand ME_CMD_AAC_CHECK_NEED_MEM = new MECommand("ME_CMD_AAC_CHECK_NEED_MEM", InnerEnum.ME_CMD_AAC_CHECK_NEED_MEM, 0x92, 0);
			public static readonly MECommand ME_CMD_AAC_INIT = new MECommand("ME_CMD_AAC_INIT", InnerEnum.ME_CMD_AAC_INIT, 0x93, 2);
			public static readonly MECommand ME_CMD_AAC_GET_INFO3 = new MECommand("ME_CMD_AAC_GET_INFO3", InnerEnum.ME_CMD_AAC_GET_INFO3, 0x94, 0);
			public static readonly MECommand ME_CMD_AAC_INIT_UNK44 = new MECommand("ME_CMD_AAC_INIT_UNK44", InnerEnum.ME_CMD_AAC_INIT_UNK44, 0x95, 2);
			public static readonly MECommand ME_CMD_AAC_INIT_UNK44_STEP2 = new MECommand("ME_CMD_AAC_INIT_UNK44_STEP2", InnerEnum.ME_CMD_AAC_INIT_UNK44_STEP2, 0x97, 4);
			public static readonly MECommand ME_CMD_WMA_GET_INFO3 = new MECommand("ME_CMD_WMA_GET_INFO3", InnerEnum.ME_CMD_WMA_GET_INFO3, 0xE1, 0);
			public static readonly MECommand ME_CMD_WMA_CHECK_NEED_MEM = new MECommand("ME_CMD_WMA_CHECK_NEED_MEM", InnerEnum.ME_CMD_WMA_CHECK_NEED_MEM, 0xE2, 0);
			public static readonly MECommand ME_CMD_WMA_INIT = new MECommand("ME_CMD_WMA_INIT", InnerEnum.ME_CMD_WMA_INIT, 0xE3, 2);
			public static readonly MECommand ME_CMD_WMA_DECODE = new MECommand("ME_CMD_WMA_DECODE", InnerEnum.ME_CMD_WMA_DECODE, 0xE5, 7);
			public static readonly MECommand ME_CMD_WMA_GET_INTERNAL_ERROR = new MECommand("ME_CMD_WMA_GET_INTERNAL_ERROR", InnerEnum.ME_CMD_WMA_GET_INTERNAL_ERROR, 0xE6, 2);
			public static readonly MECommand ME_CMD_SASCORE = new MECommand("ME_CMD_SASCORE", InnerEnum.ME_CMD_SASCORE, 0x100, 4);
			public static readonly MECommand ME_CMD_SASCORE_WITH_MIX = new MECommand("ME_CMD_SASCORE_WITH_MIX", InnerEnum.ME_CMD_SASCORE_WITH_MIX, 0x101, 6);
			public static readonly MECommand ME_CMD_MALLOC = new MECommand("ME_CMD_MALLOC", InnerEnum.ME_CMD_MALLOC, 0x180, 1);
			public static readonly MECommand ME_CMD_FREE = new MECommand("ME_CMD_FREE", InnerEnum.ME_CMD_FREE, 0x181, 1);
			public static readonly MECommand ME_CMD_CALLOC = new MECommand("ME_CMD_CALLOC", InnerEnum.ME_CMD_CALLOC, 0x182, 2);
			public static readonly MECommand ME_CMD_AW_EDRAM_BUS_CLOCK_ENABLE = new MECommand("ME_CMD_AW_EDRAM_BUS_CLOCK_ENABLE", InnerEnum.ME_CMD_AW_EDRAM_BUS_CLOCK_ENABLE, 0x183, 0);
			public static readonly MECommand ME_CMD_AW_EDRAM_BUS_CLOCK_DISABLE = new MECommand("ME_CMD_AW_EDRAM_BUS_CLOCK_DISABLE", InnerEnum.ME_CMD_AW_EDRAM_BUS_CLOCK_DISABLE, 0x184, 0);
			public static readonly MECommand ME_CMD_BOOT = new MECommand("ME_CMD_BOOT", InnerEnum.ME_CMD_BOOT, 0x185, 1);
			public static readonly MECommand ME_CMD_CPU = new MECommand("ME_CMD_CPU", InnerEnum.ME_CMD_CPU, 0x186, 2);
			public static readonly MECommand ME_CMD_POWER = new MECommand("ME_CMD_POWER", InnerEnum.ME_CMD_POWER, 0x187, 2);
			public static readonly MECommand ME_CMD_STANDBY = new MECommand("ME_CMD_STANDBY", InnerEnum.ME_CMD_STANDBY, 0x18F, 2);

			private static readonly IList<MECommand> valueList = new List<MECommand>();

			static MECommand()
			{
				valueList.Add(ME_CMD_VIDEOCODEC_OPEN_TYPE0);
				valueList.Add(ME_CMD_VIDEOCODEC_INIT_TYPE0);
				valueList.Add(ME_CMD_VIDEOCODEC_DECODE_TYPE0);
				valueList.Add(ME_CMD_VIDEOCODEC_STOP_TYPE0);
				valueList.Add(ME_CMD_VIDEOCODEC_DELETE_TYPE0);
				valueList.Add(ME_CMD_VIDEOCODEC_SET_MEMORY_TYPE0);
				valueList.Add(ME_CMD_VIDEOCODEC_GET_VERSION_TYPE0);
				valueList.Add(ME_CMD_AVC_SELECT_CLOCK);
				valueList.Add(ME_CMD_AVC_POWER_ENABLE);
				valueList.Add(ME_CMD_VIDEOCODEC_GET_SEI_TYPE0);
				valueList.Add(ME_CMD_VIDEOCODEC_GET_FRAME_CROP_TYPE0);
				valueList.Add(ME_CMD_VIDEOCODEC_GET_UNKNOWN_TYPE0);
				valueList.Add(ME_CMD_VIDEOCODEC_UNKNOWN_CMD_0x10);
				valueList.Add(ME_CMD_VIDEOCODEC_DECODE_TYPE1);
				valueList.Add(ME_CMD_VIDEOCODEC_STOP_TYPE1);
				valueList.Add(ME_CMD_VIDEOCODEC_DELETE_TYPE1);
				valueList.Add(ME_CMD_VIDEOCODEC_OPEN_TYPE1_STEP2);
				valueList.Add(ME_CMD_VIDEOCODEC_OPEN_TYPE1);
				valueList.Add(ME_CMD_VIDEOCODEC_INIT_TYPE1);
				valueList.Add(ME_CMD_VIDEOCODEC_SCAN_HEADER_TYPE1);
				valueList.Add(ME_CMD_VIDEOCODEC_GET_VERSION_TYPE1);
				valueList.Add(ME_CMD_AT3P_DECODE);
				valueList.Add(ME_CMD_AT3P_GET_INFO3);
				valueList.Add(ME_CMD_AT3P_CHECK_NEED_MEM1);
				valueList.Add(ME_CMD_AT3P_SET_UNK68);
				valueList.Add(ME_CMD_AT3P_CHECK_NEED_MEM2);
				valueList.Add(ME_CMD_AT3P_SETUP_CHANNEL);
				valueList.Add(ME_CMD_AT3P_CHECK_UNK20);
				valueList.Add(ME_CMD_AT3P_SET_UNK44);
				valueList.Add(ME_CMD_AT3P_GET_INTERNAL_ERROR);
				valueList.Add(ME_CMD_AT3_DECODE);
				valueList.Add(ME_CMD_AT3_GET_INTERNAL_ERROR);
				valueList.Add(ME_CMD_AT3_CHECK_NEED_MEM);
				valueList.Add(ME_CMD_AT3_INIT);
				valueList.Add(ME_CMD_AT3_GET_INFO3);
				valueList.Add(ME_CMD_MP3_GET_INFO3);
				valueList.Add(ME_CMD_MP3_GET_INFO2);
				valueList.Add(ME_CMD_MP3_SET_VALUE_FOR_INFO2);
				valueList.Add(ME_CMD_MP3_CHECK_NEED_MEM);
				valueList.Add(ME_CMD_MP3_INIT);
				valueList.Add(ME_CMD_MP3_DECODE);
				valueList.Add(ME_CMD_AAC_DECODE);
				valueList.Add(ME_CMD_AAC_GET_INTERNAL_ERROR);
				valueList.Add(ME_CMD_AAC_CHECK_NEED_MEM);
				valueList.Add(ME_CMD_AAC_INIT);
				valueList.Add(ME_CMD_AAC_GET_INFO3);
				valueList.Add(ME_CMD_AAC_INIT_UNK44);
				valueList.Add(ME_CMD_AAC_INIT_UNK44_STEP2);
				valueList.Add(ME_CMD_WMA_GET_INFO3);
				valueList.Add(ME_CMD_WMA_CHECK_NEED_MEM);
				valueList.Add(ME_CMD_WMA_INIT);
				valueList.Add(ME_CMD_WMA_DECODE);
				valueList.Add(ME_CMD_WMA_GET_INTERNAL_ERROR);
				valueList.Add(ME_CMD_SASCORE);
				valueList.Add(ME_CMD_SASCORE_WITH_MIX);
				valueList.Add(ME_CMD_MALLOC);
				valueList.Add(ME_CMD_FREE);
				valueList.Add(ME_CMD_CALLOC);
				valueList.Add(ME_CMD_AW_EDRAM_BUS_CLOCK_ENABLE);
				valueList.Add(ME_CMD_AW_EDRAM_BUS_CLOCK_DISABLE);
				valueList.Add(ME_CMD_BOOT);
				valueList.Add(ME_CMD_CPU);
				valueList.Add(ME_CMD_POWER);
				valueList.Add(ME_CMD_STANDBY);
			}

			public enum InnerEnum
			{
				ME_CMD_VIDEOCODEC_OPEN_TYPE0,
				ME_CMD_VIDEOCODEC_INIT_TYPE0,
				ME_CMD_VIDEOCODEC_DECODE_TYPE0,
				ME_CMD_VIDEOCODEC_STOP_TYPE0,
				ME_CMD_VIDEOCODEC_DELETE_TYPE0,
				ME_CMD_VIDEOCODEC_SET_MEMORY_TYPE0,
				ME_CMD_VIDEOCODEC_GET_VERSION_TYPE0,
				ME_CMD_AVC_SELECT_CLOCK,
				ME_CMD_AVC_POWER_ENABLE,
				ME_CMD_VIDEOCODEC_GET_SEI_TYPE0,
				ME_CMD_VIDEOCODEC_GET_FRAME_CROP_TYPE0,
				ME_CMD_VIDEOCODEC_GET_UNKNOWN_TYPE0,
				ME_CMD_VIDEOCODEC_UNKNOWN_CMD_0x10,
				ME_CMD_VIDEOCODEC_DECODE_TYPE1,
				ME_CMD_VIDEOCODEC_STOP_TYPE1,
				ME_CMD_VIDEOCODEC_DELETE_TYPE1,
				ME_CMD_VIDEOCODEC_OPEN_TYPE1_STEP2,
				ME_CMD_VIDEOCODEC_OPEN_TYPE1,
				ME_CMD_VIDEOCODEC_INIT_TYPE1,
				ME_CMD_VIDEOCODEC_SCAN_HEADER_TYPE1,
				ME_CMD_VIDEOCODEC_GET_VERSION_TYPE1,
				ME_CMD_AT3P_DECODE,
				ME_CMD_AT3P_GET_INFO3,
				ME_CMD_AT3P_CHECK_NEED_MEM1,
				ME_CMD_AT3P_SET_UNK68,
				ME_CMD_AT3P_CHECK_NEED_MEM2,
				ME_CMD_AT3P_SETUP_CHANNEL,
				ME_CMD_AT3P_CHECK_UNK20,
				ME_CMD_AT3P_SET_UNK44,
				ME_CMD_AT3P_GET_INTERNAL_ERROR,
				ME_CMD_AT3_DECODE,
				ME_CMD_AT3_GET_INTERNAL_ERROR,
				ME_CMD_AT3_CHECK_NEED_MEM,
				ME_CMD_AT3_INIT,
				ME_CMD_AT3_GET_INFO3,
				ME_CMD_MP3_GET_INFO3,
				ME_CMD_MP3_GET_INFO2,
				ME_CMD_MP3_SET_VALUE_FOR_INFO2,
				ME_CMD_MP3_CHECK_NEED_MEM,
				ME_CMD_MP3_INIT,
				ME_CMD_MP3_DECODE,
				ME_CMD_AAC_DECODE,
				ME_CMD_AAC_GET_INTERNAL_ERROR,
				ME_CMD_AAC_CHECK_NEED_MEM,
				ME_CMD_AAC_INIT,
				ME_CMD_AAC_GET_INFO3,
				ME_CMD_AAC_INIT_UNK44,
				ME_CMD_AAC_INIT_UNK44_STEP2,
				ME_CMD_WMA_GET_INFO3,
				ME_CMD_WMA_CHECK_NEED_MEM,
				ME_CMD_WMA_INIT,
				ME_CMD_WMA_DECODE,
				ME_CMD_WMA_GET_INTERNAL_ERROR,
				ME_CMD_SASCORE,
				ME_CMD_SASCORE_WITH_MIX,
				ME_CMD_MALLOC,
				ME_CMD_FREE,
				ME_CMD_CALLOC,
				ME_CMD_AW_EDRAM_BUS_CLOCK_ENABLE,
				ME_CMD_AW_EDRAM_BUS_CLOCK_DISABLE,
				ME_CMD_BOOT,
				ME_CMD_CPU,
				ME_CMD_POWER,
				ME_CMD_STANDBY
			}

			public readonly InnerEnum innerEnumValue;
			private readonly string nameValue;
			private readonly int ordinalValue;
			private static int nextOrdinal = 0;

			internal int cmd;
			internal int numberOfParameters;

			internal MECommand(string name, InnerEnum innerEnum, int cmd, int numberOfParameters)
			{
				this.cmd = cmd;
				this.numberOfParameters = numberOfParameters;

				nameValue = name;
				ordinalValue = nextOrdinal++;
				innerEnumValue = innerEnum;
			}

			public int Cmd
			{
				get
				{
					return cmd;
				}
			}

			public int NumberOfParameters
			{
				get
				{
					return numberOfParameters;
				}
			}

			public static string getCommandName(int cmd)
			{
				foreach (MECommand meCommand in MECommand.values())
				{
					if (meCommand.Cmd == cmd)
					{
						return meCommand.name();
					}
				}

				return string.Format("ME_CMD_UNKNOWN_{0:X}", cmd);
			}

			public static int getNumberOfParameters(int cmd)
			{
				foreach (MECommand meCommand in MECommand.values())
				{
					if (meCommand.Cmd == cmd)
					{
						return meCommand.NumberOfParameters;
					}
				}

				return 8;
			}

			public static IList<MECommand> values()
			{
				return valueList;
			}

			public int ordinal()
			{
				return ordinalValue;
			}

			public override string ToString()
			{
				return nameValue;
			}

			public static MECommand valueOf(string name)
			{
				foreach (MECommand enumInstance in MECommand.valueList)
				{
					if (enumInstance.nameValue == name)
					{
						return enumInstance;
					}
				}
				throw new System.ArgumentException(name);
			}
		}

		public static MMIOHandlerMeCore Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MMIOHandlerMeCore(BASE_ADDRESS);
				}
				return instance;
			}
		}

		private MMIOHandlerMeCore(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			cmd = stream.readInt();
			unknown = stream.readInt();
			stream.readInts(parameters);
			result = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(cmd);
			stream.writeInt(unknown);
			stream.writeInts(parameters);
			stream.writeInt(result);
			base.write(stream);
		}

		private void writeCmd(int cmd)
		{
			this.cmd = cmd;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Starting cmd=0x{0:X}({1})", cmd, MECommand.getCommandName(cmd)));
			}
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
					value = cmd;
					break;
				case 0x04:
					value = unknown;
					break;
				case 0x08:
					value = parameters[0];
					break;
				case 0x0C:
					value = parameters[1];
					break;
				case 0x10:
					value = parameters[2];
					break;
				case 0x14:
					value = parameters[3];
					break;
				case 0x18:
					value = parameters[4];
					break;
				case 0x1C:
					value = parameters[5];
					break;
				case 0x20:
					value = parameters[6];
					break;
				case 0x24:
					value = parameters[7];
					break;
				case 0x28:
					value = result;
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
				case 0x00:
					writeCmd(value);
					break;
				case 0x04:
					unknown = value;
					break;
				case 0x08:
					parameters[0] = value;
					break;
				case 0x0C:
					parameters[1] = value;
					break;
				case 0x10:
					parameters[2] = value;
					break;
				case 0x14:
					parameters[3] = value;
					break;
				case 0x18:
					parameters[4] = value;
					break;
				case 0x1C:
					parameters[5] = value;
					break;
				case 0x20:
					parameters[6] = value;
					break;
				case 0x24:
					parameters[7] = value;
					break;
				case 0x28:
					result = value;
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

		public override string ToString()
		{
			StringBuilder s = new StringBuilder(string.Format("cmd=0x{0:X}({1}), result=0x{2:X8}", cmd, MECommand.getCommandName(cmd), result));
			int numberOfParameters = MECommand.getNumberOfParameters(cmd);
			for (int i = 0; i < numberOfParameters; i++)
			{
				s.Append(string.Format(", parameters[{0:D}]=0x{1:X8}", i, parameters[i]));
			}
			return s.ToString();
		}
	}

}