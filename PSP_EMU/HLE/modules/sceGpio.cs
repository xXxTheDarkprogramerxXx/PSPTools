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
	using Logger = org.apache.log4j.Logger;


	public class sceGpio : HLEModule
	{
		public static Logger log = Modules.getLogger("sceGpio");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x317D9D2C, version = 150) public int sceGpioSetPortMode(int port, int mode)
		[HLEFunction(nid : 0x317D9D2C, version : 150)]
		public virtual int sceGpioSetPortMode(int port, int mode)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFBC85E74, version = 660) public int sceGpioSetIntrMode(int interruptNumber, int mode)
		[HLEFunction(nid : 0xFBC85E74, version : 660)]
		public virtual int sceGpioSetIntrMode(int interruptNumber, int mode)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4250D44A, version = 150) public int sceGpioPortRead()
		[HLEFunction(nid : 0x4250D44A, version : 150)]
		public virtual int sceGpioPortRead()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x103C3EB2, version = 150) public void sceGpioPortClear(int mask)
		[HLEFunction(nid : 0x103C3EB2, version : 150)]
		public virtual void sceGpioPortClear(int mask)
		{
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x310F0CCF, version = 150) public void sceGpioPortSet(int mask)
		[HLEFunction(nid : 0x310F0CCF, version : 150)]
		public virtual void sceGpioPortSet(int mask)
		{
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1A730F20, version = 660) public int sceGpioAcquireIntr(int interruptNumber)
		[HLEFunction(nid : 0x1A730F20, version : 660)]
		public virtual int sceGpioAcquireIntr(int interruptNumber)
		{
			return 0;
		}
	}

}