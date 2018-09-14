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
	/// This will register the class as a class that will be serialized as UIDs. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false]
	public class HLEUidClass : System.Attribute
	{
		/// <summary>
		/// Method name of the module class, without parameter, returning an int, that will generate
		/// an ID for this annotated class.
		/// </summary>
		public string moduleMethodUidGenerator;

		/// <summary>
		/// Error code that will be returned or stored in a TErrorPointer when UID not found for this
		/// annotated class. 
		/// </summary>
		public int errorValueOnNotFound;

		public HLEUidClass(public String moduleMethodUidGenerator = "", public int errorValueOnNotFound = 0)
		{
			this.moduleMethodUidGenerator = moduleMethodUidGenerator;
			this.errorValueOnNotFound = errorValueOnNotFound;
		}
	}

}