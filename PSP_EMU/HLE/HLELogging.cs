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
	/// This annotation tells the compiler to log the kernel function call using a generic format.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false]
	public class HLELogging : System.Attribute
	{
		/// <summary>
		/// Sets the logging level.
		/// Possible values are "trace", "debug", "info", "warn", "error".
		/// </summary>
		public string level;

		public HLELogging(public String level = "debug")
		{
			this.level = level;
		}
	}

}