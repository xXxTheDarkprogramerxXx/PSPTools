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
namespace pspsharp.Debugger.MemoryBreakpoints
{
	using DebuggerMemory = pspsharp.memory.DebuggerMemory;

	public class MemoryBreakpoint
	{

		public enum AccessType
		{
			READ,
			WRITE,
			READWRITE
		}
		private int start_address;
		private int end_address;
		private AccessType access;
		private bool installed;

		public MemoryBreakpoint()
		{
			start_address = 0x00000000;
			end_address = 0x00000000;
			access = AccessType.READ;
			installed = false;
		}

		public MemoryBreakpoint(DebuggerMemory debuggerMemory, int start_address, int end_address, AccessType access)
		{
			StartAddress = start_address;
			EndAddress = end_address;
			this.access = access;

			install(debuggerMemory);
		}

		public MemoryBreakpoint(DebuggerMemory debuggerMemory, int address, AccessType access)
		{
			StartAddress = address;
			EndAddress = address;
			this.access = access;

			install(debuggerMemory);
		}

		public int StartAddress
		{
			get
			{
				return start_address;
			}
			set
			{
				this.start_address = Memory.normalizeAddress(value);
			}
		}


		public int EndAddress
		{
			get
			{
				return end_address;
			}
			set
			{
				this.end_address = Memory.normalizeAddress(value);
			}
		}


		private static DebuggerMemory DebuggerMemory
		{
			get
			{
				Memory mem = Memory.Instance;
				if (mem is DebuggerMemory)
				{
					return (DebuggerMemory) mem;
				}
    
				return null;
			}
		}

		public bool Enabled
		{
			set
			{
				if (installed & !value)
				{
					uninstall(DebuggerMemory);
				}
				else if (!installed & value)
				{
					install(DebuggerMemory);
				}
			}
			get
			{
				return installed;
			}
		}


		public virtual AccessType Access
		{
			get
			{
				return access;
			}
			set
			{
				this.access = value;
			}
		}


		private void install(DebuggerMemory debuggerMemory)
		{
			if (debuggerMemory != null)
			{
				switch (access)
				{
					case pspsharp.Debugger.MemoryBreakpoints.MemoryBreakpoint.AccessType.READ:
						debuggerMemory.addRangeReadBreakpoint(start_address, end_address);
						break;
					case pspsharp.Debugger.MemoryBreakpoints.MemoryBreakpoint.AccessType.WRITE:
						debuggerMemory.addRangeWriteBreakpoint(start_address, end_address);
						break;
					case pspsharp.Debugger.MemoryBreakpoints.MemoryBreakpoint.AccessType.READWRITE:
						debuggerMemory.addRangeReadWriteBreakpoint(start_address, end_address);
						break;
				}
				installed = true;
			}
		}

		private void uninstall(DebuggerMemory debuggerMemory)
		{
			if (debuggerMemory != null)
			{
				switch (access)
				{
					case pspsharp.Debugger.MemoryBreakpoints.MemoryBreakpoint.AccessType.READ:
						debuggerMemory.removeRangeReadBreakpoint(start_address, end_address);
						break;
					case pspsharp.Debugger.MemoryBreakpoints.MemoryBreakpoint.AccessType.WRITE:
						debuggerMemory.removeRangeWriteBreakpoint(start_address, end_address);
						break;
					case pspsharp.Debugger.MemoryBreakpoints.MemoryBreakpoint.AccessType.READWRITE:
						debuggerMemory.removeRangeReadWriteBreakpoint(start_address, end_address);
						break;
				}
				installed = false;
			}
		}
	}

}