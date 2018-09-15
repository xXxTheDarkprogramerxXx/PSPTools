using System.Collections.Generic;
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
namespace pspsharp.format.rco.vsmx.interpreter
{

	public class VSMXObject : VSMXBaseObject
	{
		protected internal readonly IDictionary<string, VSMXBaseObject> properties;
		private string className;
		private readonly IList<string> sortedPropertyNames;

		public VSMXObject(VSMXInterpreter interpreter, string className) : base(interpreter)
		{
			this.className = className;
			properties = new Dictionary<string, VSMXBaseObject>();
			sortedPropertyNames = new LinkedList<string>();
		}

		private void addProperty(string name, VSMXBaseObject value)
		{
			properties[name] = value;
			sortedPropertyNames.Add(name);
		}

		protected internal static int getIndex(string name)
		{
			int index;
			try
			{
				index = int.Parse(name);
			}
			catch (System.FormatException)
			{
				index = -1;
			}

			return index;
		}

		public override VSMXBaseObject getPropertyValue(string name)
		{
			if (lengthName.Equals(name))
			{
				return new VSMXNumber(interpreter, properties.Count);
			}

			if (prototypeName.Equals(name))
			{
				VSMXObject prototype = Prototype;
				if (prototype != null)
				{
					return prototype;
				}
				return VSMXUndefined.singleton;
			}

			VSMXBaseObject value = properties[name];
			if (value == null)
			{
				int index = getIndex(name);
				if (index >= 0)
				{
					if (index < properties.Count)
					{
						value = properties[sortedPropertyNames[index]];
					}
					else
					{
						value = VSMXUndefined.singleton;
					}
				}
				else
				{
					VSMXObject prototype = Prototype;
					if (prototype != null && prototype.properties.ContainsKey(name))
					{
						value = prototype.getPropertyValue(name);
					}
					else
					{
						value = VSMXUndefined.singleton;
						addProperty(name, value);
					}
				}
			}

			return value;
		}

		public override IList<string> PropertyNames
		{
			get
			{
				return sortedPropertyNames;
			}
		}

		public override void setPropertyValue(string name, VSMXBaseObject value)
		{
			if (properties.ContainsKey(name))
			{
				properties[name] = value;
			}
			else
			{
				addProperty(name, value);
			}
		}

		public override void deletePropertyValue(string name)
		{
			properties.Remove(name);
			sortedPropertyNames.Remove(name);
		}

		public override bool hasPropertyValue(string name)
		{
			if (prototypeName.Equals(name))
			{
				return true;
			}

			if (properties.ContainsKey(name))
			{
				return true;
			}

			VSMXObject prototype = Prototype;
			if (prototype != null)
			{
				return prototype.hasPropertyValue(name);
			}

			return false;
		}

		public override bool Equals(VSMXBaseObject value)
		{
			if (value is VSMXObject)
			{
				// Return true if both values refer to the same object
				return this == value;
			}
			return false;
		}

		public override string typeOf()
		{
			return "object";
		}

		public override string ClassName
		{
			get
			{
				return className;
			}
		}

		public override string StringValue
		{
			get
			{
				if (hasPropertyValue("toString"))
				{
					Console.WriteLine(string.Format("getStringValue on VSMXObject should be calling existing toString: {0}", getPropertyValue("toString")));
				}
				return base.StringValue;
			}
		}

		protected internal virtual void ToString(StringBuilder s)
		{
			string[] keys = properties.Keys.toArray(new string[0]);
			Array.Sort(keys);
			foreach (string key in keys)
			{
				VSMXBaseObject value = properties[key];
				if (s.Length > 1)
				{
					s.Append(",\n");
				}
				s.Append(string.Format("{0}={1}", key, value));
			}
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			s.Append("[");
			ToString(s);
			s.Append("]");

			return s.ToString();
		}
	}

}