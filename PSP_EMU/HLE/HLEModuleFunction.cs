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

namespace pspsharp.HLE
{

	/// 
	/// <summary>
	/// @author fiveofhearts
	/// </summary>
	public class HLEModuleFunction
	{
		private int syscallCode;
		private readonly string moduleName;
		private readonly string functionName;
		private int nid;
		private bool unimplemented;
		private string loggingLevel;
		private Method hleModuleMethod;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool checkInsideInterrupt_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool checkDispatchThreadEnabled_Renamed;
		private int stackUsage;
		private HLEModule hleModule;
		private int firmwareVersion;

		public HLEModuleFunction(string moduleName, string functionName, int nid, HLEModule hleModule, Method hleModuleMethod, bool checkInsideInterrupt, bool checkDispatchThreadEnabled, int stackUsage, int firmwareVersion)
		{
			this.moduleName = moduleName;
			this.functionName = functionName;
			this.nid = nid;
			this.checkInsideInterrupt_Renamed = checkInsideInterrupt;
			this.checkDispatchThreadEnabled_Renamed = checkDispatchThreadEnabled;
			this.stackUsage = stackUsage;
			this.hleModuleMethod = hleModuleMethod;
			this.hleModule = hleModule;
			this.firmwareVersion = firmwareVersion;
		}

		public int SyscallCode
		{
			set
			{
				this.syscallCode = value;
			}
			get
			{
				return syscallCode;
			}
		}


		public string ModuleName
		{
			get
			{
				return moduleName;
			}
		}

		public string FunctionName
		{
			get
			{
				return functionName;
			}
		}

		public int Nid
		{
			set
			{
				this.nid = value;
			}
			get
			{
				return nid;
			}
		}


		public bool Unimplemented
		{
			set
			{
				this.unimplemented = value;
			}
			get
			{
				return unimplemented;
			}
		}


		public virtual string LoggingLevel
		{
			get
			{
				return loggingLevel;
			}
			set
			{
				this.loggingLevel = value;
			}
		}


		public virtual bool checkDispatchThreadEnabled()
		{
			return checkDispatchThreadEnabled_Renamed;
		}

		public virtual bool checkInsideInterrupt()
		{
			return checkInsideInterrupt_Renamed;
		}

		public virtual int StackUsage
		{
			get
			{
				return stackUsage;
			}
		}

		public virtual bool hasStackUsage()
		{
			return stackUsage > 0;
		}

		public virtual Method HLEModuleMethod
		{
			get
			{
				return hleModuleMethod;
			}
		}

		public virtual HLEModule HLEModule
		{
			get
			{
				return hleModule;
			}
		}

		public virtual int FirmwareVersion
		{
			get
			{
				return firmwareVersion;
			}
		}

		public override string ToString()
		{
			return string.Format("HLEModuleFunction(moduleName='{0}', functionName='{1}', nid=0x{2:X8}, syscallCode={3:D})", moduleName, functionName, nid, syscallCode);
		}
	}

}