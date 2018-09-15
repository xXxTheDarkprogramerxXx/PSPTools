using System.Collections.Generic;
using System.Threading;

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

	using SysMemUserForUser = pspsharp.HLE.modules.SysMemUserForUser;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using ISettingsListener = pspsharp.settings.ISettingsListener;
	using Settings = pspsharp.settings.Settings;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	/// 
	/// <summary>
	/// @author fiveofhearts
	/// </summary>
	public abstract class HLEModule
	{
		private const int STATE_VERSION = 0;
		private SysMemUserForUser.SysMemInfo memory;
		private string name;
		private bool started = false;

		public Dictionary<string, HLEModuleFunction> installedHLEModuleFunctions = new Dictionary<string, HLEModuleFunction>();

		public string Name
		{
			get
			{
				if (string.ReferenceEquals(name, null))
				{
					name = this.GetType().Name;
				}
				return name;
			}
		}

		/// <summary>
		/// Returns an installed hle function by name.
		/// </summary>
		/// <param name="functionName"> the function name </param>
		/// <returns> the hle function corresponding to the functionName or null
		///         if the function was not found in this module. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public HLEModuleFunction getHleFunctionByName(String functionName) throws RuntimeException
		public virtual HLEModuleFunction getHleFunctionByName(string functionName)
		{
			if (!installedHLEModuleFunctions.ContainsKey(functionName))
			{
				Modules.Console.WriteLine(string.Format("Can't find HLE function '{0}' in module '{1}'", functionName, this));
				return null;
			}

			return installedHLEModuleFunctions[functionName];
		}

		public virtual bool Started
		{
			get
			{
				return started;
			}
		}

		public virtual void start()
		{
			started = true;
		}

		public virtual void stop()
		{
			// Remove all the settings listener defined for this module
			Settings.Instance.removeSettingsListener(Name);
			started = false;
		}

		protected internal virtual void setSettingsListener(string option, ISettingsListener settingsListener)
		{
			Settings.Instance.registerSettingsListener(Name, option, settingsListener);
		}

		protected internal static string getCallingFunctionName(int index)
		{
			StackTraceElement[] stack = Thread.CurrentThread.StackTrace;
			return stack[index + 1].MethodName;
		}

		public virtual int MemoryUsage
		{
			get
			{
				return 0;
			}
		}

		public virtual void load()
		{
			int memoryUsage = MemoryUsage;
			if (memoryUsage > 0)
			{
				if (Modules.log.DebugEnabled)
				{
					Modules.Console.WriteLine(string.Format("Allocating 0x{0:X} bytes for HLE module {1}", memoryUsage, Name));
				}

				memory = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.USER_PARTITION_ID, string.Format("Module-{0}", Name), SysMemUserForUser.PSP_SMEM_Low, memoryUsage, 0);
			}
			else
			{
				memory = null;
			}
		}

		public virtual void unload()
		{
			if (memory != null)
			{
				if (Modules.log.DebugEnabled)
				{
					Modules.Console.WriteLine(string.Format("Freeing 0x{0:X} bytes for HLE module {1}", memory.size, Name));
				}

				Modules.SysMemUserForUserModule.free(memory);
				memory = null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			name = stream.readString();
			started = stream.readBoolean();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeString(name);
			stream.writeBoolean(started);
		}

		public override string ToString()
		{
			return Name;
		}
	}

}