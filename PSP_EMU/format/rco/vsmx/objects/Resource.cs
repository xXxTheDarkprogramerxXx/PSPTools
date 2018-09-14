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
	using RCOEntry = pspsharp.format.RCO.RCOEntry;
	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXInterpreter = pspsharp.format.rco.vsmx.interpreter.VSMXInterpreter;
	using VSMXNativeObject = pspsharp.format.rco.vsmx.interpreter.VSMXNativeObject;
	using VSMXObject = pspsharp.format.rco.vsmx.interpreter.VSMXObject;

	public class Resource : BaseNativeObject
	{
		public const string objectName = "resource";
		public const string rootName = "root";
		public const string childrenName = "children";
		public const string textureName = "texture";

		public static VSMXNativeObject create(VSMXInterpreter interpreter, Display display, VSMXNativeObject vsmxController, RCO.RCOEntry mainTable)
		{
			Resource resource = new Resource(interpreter);
			VSMXNativeObject @object = new VSMXNativeObject(interpreter, resource);
			resource.Object = @object;

			Controller controller = (Controller) vsmxController.Object;
			createTable(interpreter, display, controller, @object, mainTable, RCO.RCO_TABLE_OBJ, "pagetable");
			createTable(interpreter, display, controller, @object, mainTable, RCO.RCO_TABLE_ANIM, "animtable");
			createTable(interpreter, display, controller, @object, mainTable, RCO.RCO_TABLE_SOUND, "soundtable");
			createTable(interpreter, display, controller, @object, mainTable, RCO.RCO_TABLE_IMG, "texturetable");

			return @object;
		}

		private Resource(VSMXInterpreter interpreter)
		{
		}

		private static RCO.RCOEntry[] findEntries(RCO.RCOEntry mainTable, int id)
		{
			foreach (RCO.RCOEntry entry in mainTable.subEntries)
			{
				if (entry.id == id)
				{
					return entry.subEntries;
				}
			}

			return null;
		}

		private static void createObjectFromEntry(VSMXInterpreter interpreter, Display display, Controller controller, VSMXBaseObject parent, RCO.RCOEntry entry)
		{
			if (entry.obj == null)
			{
				return;
			}

			VSMXBaseObject @object;
			if (entry.obj != null)
			{
				entry.obj.Display = display;
				entry.obj.Controller = controller;
				@object = entry.obj.createVSMXObject(interpreter, parent, entry);
			}
			else
			{
				@object = parent;
			}

			if (entry.subEntries != null)
			{
				VSMXObject children = new VSMXObject(interpreter, null);
				@object.setPropertyValue(childrenName, children);
				for (int i = 0; i < entry.subEntries.Length; i++)
				{
					RCO.RCOEntry child = entry.subEntries[i];
					if (string.ReferenceEquals(child.label, null))
					{
						child.label = string.Format("{0:D4}", i);
					}
					createObjectFromEntry(interpreter, display, controller, children, child);
				}
			}
		}

		private static void createTable(VSMXInterpreter interpreter, Display display, Controller controller, VSMXObject parent, RCO.RCOEntry mainTable, int id, string name)
		{
			VSMXBaseObject table = new VSMXObject(interpreter, null);
			parent.setPropertyValue(name, table);

			RCO.RCOEntry[] entries = findEntries(mainTable, id);

			if (entries != null)
			{
				foreach (RCO.RCOEntry entry in entries)
				{
					createObjectFromEntry(interpreter, display, controller, table, entry);
				}
			}
		}
	}

}