using System.Text;

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
	//using Logger = org.apache.log4j.Logger;

	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXNull = pspsharp.format.rco.vsmx.interpreter.VSMXNull;

	public abstract class BaseNativeFunction : INativeFunction
	{
		public static readonly Logger log = VSMX.log;
		protected internal string objectName;

		protected internal BaseNativeFunction(string objectName)
		{
			this.objectName = objectName;
		}

		public virtual int Args
		{
			get
			{
				return 0;
			}
		}

		public virtual VSMXBaseObject call(VSMXBaseObject[] arguments)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Calling {0}", ToString(arguments)));
			}
			return VSMXNull.singleton;
		}

		protected internal virtual string ToString(VSMXBaseObject[] arguments)
		{
			StringBuilder s = new StringBuilder();
			if (!string.ReferenceEquals(objectName, null))
			{
				s.Append(string.Format("{0}.", objectName));
			}
			s.Append(string.Format("{0}(", ToString()));
			if (arguments != null)
			{
				int firstArgument = !string.ReferenceEquals(objectName, null) ? 1 : 0;
				for (int i = firstArgument; i < arguments.Length; i++)
				{
					if (i > firstArgument)
					{
						s.Append(", ");
					}
					s.Append(arguments[i].ToString());
				}
			}
			s.Append(")");

			return s.ToString();
		}

		public override string ToString()
		{
			return this.GetType().Name;
		}
	}

}