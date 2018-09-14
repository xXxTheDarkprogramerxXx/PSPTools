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
//	import static pspsharp.Allegrex.Common._a1;

	using Logger = org.apache.log4j.Logger;

	using Common = pspsharp.Allegrex.Common;
	using CpuState = pspsharp.Allegrex.CpuState;

	public class sceMeCore : HLEModule
	{
		public static Logger log = Modules.getLogger("sceMeCore");

		private string logParameters(CpuState cpu, int firstParameter, int numberParameters)
		{
			StringBuilder s = new StringBuilder();
			for (int i = 0; i < numberParameters; i++)
			{
				int reg = firstParameter + i;
				if (s.Length > 0)
				{
					s.Append(", ");
				}
				s.Append(string.Format("{0}=0x{1:X8}", Common.gprNames[reg], cpu.getRegister(reg)));
			}

			return s.ToString();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x635397BB, version = 150) public int sceMeCore_driver_635397BB(pspsharp.Allegrex.CpuState cpu, int cmd)
		[HLEFunction(nid : 0x635397BB, version : 150)]
		public virtual int sceMeCore_driver_635397BB(CpuState cpu, int cmd)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFA398D71, version = 150) public int sceMeCore_driver_FA398D71(pspsharp.Allegrex.CpuState cpu, int cmd)
		[HLEFunction(nid : 0xFA398D71, version : 150)]
		public virtual int sceMeCore_driver_FA398D71(CpuState cpu, int cmd)
		{
			switch (cmd)
			{
				case 0x100: // Called by __sceSasCore
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceMeCore_driver_FA398D71 cmd=0x{0:X}(__sceSasCore), {1}", cmd, logParameters(cpu, _a1, 3)));
					}
					break;
				case 0x101: // Called by __sceSasCoreWithMix
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceMeCore_driver_FA398D71 cmd=0x{0:X}(__sceSasCoreWithMix), {1}", cmd, logParameters(cpu, _a1, 5)));
					}
					break;
				default:
					log.warn(string.Format("sceMeCore_driver_FA398D71 unknown cmd=0x{0:X}, {1}", cmd, logParameters(cpu, _a1, 7)));
				break;
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x905A7500, version = 150) public int sceMeCore_driver_905A7500()
		[HLEFunction(nid : 0x905A7500, version : 150)]
		public virtual int sceMeCore_driver_905A7500()
		{
			return 0;
		}
	}

}