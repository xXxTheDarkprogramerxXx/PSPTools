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

	/// <summary>
	/// This annotation marks a function as a kernel function from a module.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false]
	public class HLEFunction : System.Attribute
	{
		/// <summary>
		/// Unique 32-bit identifier of the function for that module.
		/// Initially was the 32 last bits of the SHA1's function's name.
		/// </summary>
		public int nid;

		/// <summary>
		/// Checks if the cpu is inside an interrupt and if so, 
		/// raises SceKernelErrors.ERROR_KERNEL_CANNOT_BE_CALLED_FROM_INTERRUPT. 
		/// </summary>
		public bool checkInsideInterrupt;

		/// <summary>
		/// Checks if the dispatch thread is enabled and if disabled, 
		/// raises SceKernelErrors.ERROR_KERNEL_WAIT_CAN_NOT_WAIT. 
		/// </summary>
		public bool checkDispatchThreadEnabled;

		/// <summary>
		/// The minimum kernel version where this module function is found. 
		/// </summary>
		public int version;

		/// <summary>
		/// Name of the module. The default is the name of the class.
		/// </summary>
		public string moduleName;

		/// <summary>
		/// Name of the function. The default is the name of the method.
		/// </summary>
		public string functionName;

		/// <summary>
		/// Size of the stack used by the function.
		/// </summary>
		public int stackUsage;

		public HLEFunction(public int nid, public boolean checkInsideInterrupt = false, public boolean checkDispatchThreadEnabled = false, public int version = 150, public String moduleName = "", public String functionName = "", public int stackUsage = 0)
		{
			this.nid = nid;
			this.checkInsideInterrupt = checkInsideInterrupt;
			this.checkDispatchThreadEnabled = checkDispatchThreadEnabled;
			this.version = version;
			this.moduleName = moduleName;
			this.functionName = functionName;
			this.stackUsage = stackUsage;
		}
	}

}