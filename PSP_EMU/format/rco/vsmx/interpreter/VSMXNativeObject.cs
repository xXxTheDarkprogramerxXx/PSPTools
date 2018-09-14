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

	using BaseNativeObject = pspsharp.format.rco.vsmx.objects.BaseNativeObject;

	public class VSMXNativeObject : VSMXObject
	{
		private BaseNativeObject @object;

		public VSMXNativeObject(VSMXInterpreter interpreter, BaseNativeObject @object) : base(interpreter, null)
		{
			this.@object = @object;
		}

		public virtual BaseNativeObject Object
		{
			get
			{
				return @object;
			}
		}

		public override void setPropertyValue(string name, VSMXBaseObject value)
		{
			if (@object != null && !string.ReferenceEquals(name, null) && name.Length >= 1 && value is VSMXNativeObject)
			{
				string setterName = "set" + char.ToUpper(name[0]) + name.Substring(1);
				try
				{
					Method setterMethod = @object.GetType().GetMethod(setterName, typeof(BaseNativeObject));
					setterMethod.invoke(@object, ((VSMXNativeObject) value).Object);
					return;
				}
				catch (NoSuchMethodException)
				{
					// Ignore exception
				}
				catch (SecurityException)
				{
					// Ignore exception
				}
				catch (IllegalAccessException)
				{
					// Ignore exception
				}
				catch (System.ArgumentException)
				{
					// Ignore exception
				}
				catch (InvocationTargetException)
				{
					// Ignore exception
				}
			}

			base.setPropertyValue(name, value);
		}

		protected internal override void ToString(StringBuilder s)
		{
			s.Append(@object.ToString());
			base.ToString(s);
		}
	}

}