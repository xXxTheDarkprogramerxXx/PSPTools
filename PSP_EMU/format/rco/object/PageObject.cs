using System.Collections.Generic;

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
namespace pspsharp.format.rco.@object
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.rco.vsmx.objects.Resource.childrenName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.rco.vsmx.objects.Resource.rootName;

	using RCOEntry = pspsharp.format.RCO.RCOEntry;
	using EventType = pspsharp.format.rco.type.EventType;
	using IntType = pspsharp.format.rco.type.IntType;
	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXInterpreter = pspsharp.format.rco.vsmx.interpreter.VSMXInterpreter;
	using VSMXNativeObject = pspsharp.format.rco.vsmx.interpreter.VSMXNativeObject;
	using VSMXObject = pspsharp.format.rco.vsmx.interpreter.VSMXObject;
	using BaseNativeObject = pspsharp.format.rco.vsmx.objects.BaseNativeObject;
	using Resource = pspsharp.format.rco.vsmx.objects.Resource;

	public class PageObject : BaseObject
	{
		[ObjectField(order : 1)]
		public IntType unknownInt0;
		[ObjectField(order : 2)]
		public EventType onInit;
		[ObjectField(order : 3)]
		public EventType onCancel;
		[ObjectField(order : 4)]
		public EventType onContextMenu;
		[ObjectField(order : 5)]
		public EventType onActivate;

		public override VSMXBaseObject createVSMXObject(VSMXInterpreter interpreter, VSMXBaseObject parent, RCOEntry entry)
		{
			VSMXBaseObject @object = base.createVSMXObject(interpreter, parent, entry);

			VSMXObject root = new VSMXObject(interpreter, null);
			@object.setPropertyValue(Resource.rootName, root);

			return root;
		}

		private void display(VSMXBaseObject @object)
		{
			if (@object.hasPropertyValue(childrenName))
			{
				VSMXBaseObject children = @object.getPropertyValue(childrenName);
				IList<string> childrenNames = children.PropertyNames;
				foreach (string childName in childrenNames)
				{
					VSMXBaseObject child = children.getPropertyValue(childName);
					display.add(child);
					display(child);
				}
			}
		}

		private BasePositionObject getFirstButton(VSMXBaseObject @object)
		{
			if (@object.hasPropertyValue(childrenName))
			{
				VSMXBaseObject children = @object.getPropertyValue(childrenName);
				IList<string> childrenNames = children.PropertyNames;
				foreach (string childName in childrenNames)
				{
					VSMXBaseObject child = children.getPropertyValue(childName);
					if (child is VSMXNativeObject)
					{
						BaseNativeObject childObject = ((VSMXNativeObject) child).Object;
						if (childObject is UButtonObject)
						{
							return (BasePositionObject) childObject;
						}
					}
					BasePositionObject button = getFirstButton(child);
					if (button != null)
					{
						return button;
					}
				}
			}

			return null;
		}

		private void hide(VSMXBaseObject @object)
		{
			if (@object.hasPropertyValue(childrenName))
			{
				VSMXBaseObject children = @object.getPropertyValue(childrenName);
				IList<string> childrenNames = children.PropertyNames;
				foreach (string childName in childrenNames)
				{
					VSMXBaseObject child = children.getPropertyValue(childName);
					display.remove(child);
					hide(child);
				}
			}
		}

		public virtual VSMXBaseObject open(VSMXBaseObject @object)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("PageObject.open {0}, children: {1}", this, @object.getPropertyValue(rootName).getPropertyValue(childrenName)));
			}

			trigger(onInit);

			if (display != null)
			{
				display(@object.getPropertyValue(rootName));

				BasePositionObject button = getFirstButton(@object.getPropertyValue(rootName));
				if (button != null)
				{
					button.setFocus();
				}
			}

			return @object;
		}

		public virtual VSMXBaseObject activate(VSMXBaseObject @object)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("PageObject.activate"));
			}

			trigger(onActivate);

			return @object;
		}

		public virtual void close(VSMXBaseObject @object)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("PageObject.close"));
			}

			if (display != null)
			{
				hide(@object.getPropertyValue(rootName));
			}
		}
	}

}