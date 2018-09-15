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
	//using Logger = org.apache.log4j.Logger;

	using CpuState = pspsharp.Allegrex.CpuState;

	public class StdioForKernel : HLEModule
	{
		//public static Logger log = Modules.getLogger("StdioForKernel");

		[HLEFunction(nid : 0xCAB439DF, version : 150)]
		public virtual int StdioForKernel_printf(CpuState cpu, PspString formatString)
		{
			return Modules.SysMemUserForUserModule.hleKernelPrintf(cpu, formatString, log);
		}
	}

}