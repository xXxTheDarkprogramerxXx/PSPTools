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


	public class scePsheet : HLEModule
	{
		//public static Logger log = Modules.getLogger("scePsheet");
		protected internal TPointer address;
		protected internal int size;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x302AB4B8, version = 150) public int sceDRMInstallInit(pspsharp.HLE.TPointer address, int size)
		[HLEFunction(nid : 0x302AB4B8, version : 150)]
		public virtual int sceDRMInstallInit(TPointer address, int size)
		{
			this.address = address;
			this.size = size;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3CEC4078, version = 150) public int sceDRMInstallEnd()
		[HLEFunction(nid : 0x3CEC4078, version : 150)]
		public virtual int sceDRMInstallEnd()
		{
			address = TPointer.NULL;
			size = 0;

			return 0;
		}
	}

}