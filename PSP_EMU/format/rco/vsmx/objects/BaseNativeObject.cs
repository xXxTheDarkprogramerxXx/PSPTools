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
namespace pspsharp.format.rco.vsmx.objects
{
	using Logger = org.apache.log4j.Logger;

	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXFunction = pspsharp.format.rco.vsmx.interpreter.VSMXFunction;
	using VSMXInterpreter = pspsharp.format.rco.vsmx.interpreter.VSMXInterpreter;
	using VSMXObject = pspsharp.format.rco.vsmx.interpreter.VSMXObject;

	public class BaseNativeObject
	{
		protected internal static Logger log = VSMX.log;
		private VSMXObject @object;
		private BaseNativeObject parent;

		public virtual VSMXObject Object
		{
			get
			{
				return @object;
			}
			set
			{
				this.@object = value;
			}
		}


		public virtual BaseNativeObject Parent
		{
			get
			{
				return parent;
			}
			set
			{
				this.parent = value;
			}
		}


		public virtual void callCallback(VSMXInterpreter interpreter, string name, VSMXBaseObject[] arguments)
		{
			VSMXBaseObject function = object.getPropertyValue(name);
			if (function is VSMXFunction)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("callCallback {0}, arguments={1}", name, arguments));
				}

				interpreter.interpretFunction((VSMXFunction) function, null, arguments);
			}
		}

		public override string ToString()
		{
			return this.GetType().Name;
		}
	}

}