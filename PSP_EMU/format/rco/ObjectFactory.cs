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
namespace pspsharp.format.rco
{
	using ActionObject = pspsharp.format.rco.@object.ActionObject;
	using BaseObject = pspsharp.format.rco.@object.BaseObject;
	using ButtonObject = pspsharp.format.rco.@object.ButtonObject;
	using ClockObject = pspsharp.format.rco.@object.ClockObject;
	using EditObject = pspsharp.format.rco.@object.EditObject;
	using GroupObject = pspsharp.format.rco.@object.GroupObject;
	using IItemObject = pspsharp.format.rco.@object.IItemObject;
	using IListObject = pspsharp.format.rco.@object.IListObject;
	using IconObject = pspsharp.format.rco.@object.IconObject;
	using ItemSpinObject = pspsharp.format.rco.@object.ItemSpinObject;
	using LItemObject = pspsharp.format.rco.@object.LItemObject;
	using LListObject = pspsharp.format.rco.@object.LListObject;
	using MItemObject = pspsharp.format.rco.@object.MItemObject;
	using MListObject = pspsharp.format.rco.@object.MListObject;
	using ModelObject = pspsharp.format.rco.@object.ModelObject;
	using PageObject = pspsharp.format.rco.@object.PageObject;
	using PlaneObject = pspsharp.format.rco.@object.PlaneObject;
	using ProgressObject = pspsharp.format.rco.@object.ProgressObject;
	using ScrollObject = pspsharp.format.rco.@object.ScrollObject;
	using SpinObject = pspsharp.format.rco.@object.SpinObject;
	using TextObject = pspsharp.format.rco.@object.TextObject;
	using UButtonObject = pspsharp.format.rco.@object.UButtonObject;
	using XItemObject = pspsharp.format.rco.@object.XItemObject;
	using XListObject = pspsharp.format.rco.@object.XListObject;
	using XMListObject = pspsharp.format.rco.@object.XMListObject;
	using XMenuObject = pspsharp.format.rco.@object.XMenuObject;

	public class ObjectFactory
	{
		public static BaseObject newObject(int type)
		{
			switch (type)
			{
				case 1:
					return new PageObject();
				case 2:
					return new PlaneObject();
				case 3:
					return new ButtonObject();
				case 4:
					return new XMenuObject();
				case 5:
					return new XMListObject();
				case 6:
					return new XListObject();
				case 7:
					return new ProgressObject();
				case 8:
					return new ScrollObject();
				case 9:
					return new MListObject();
				case 10:
					return new MItemObject();
				case 11:
					break; // Unknown object 11
					goto case 12;
				case 12:
					return new XItemObject();
				case 13:
					return new TextObject();
				case 14:
					return new ModelObject();
				case 15:
					return new SpinObject();
				case 16:
					return new ActionObject();
				case 17:
					return new ItemSpinObject();
				case 18:
					return new GroupObject();
				case 19:
					return new LListObject();
				case 20:
					return new LItemObject();
				case 21:
					return new EditObject();
				case 22:
					return new ClockObject();
				case 23:
					return new IListObject();
				case 24:
					return new IItemObject();
				case 25:
					return new IconObject();
				case 26:
					return new UButtonObject();
			}

			RCO.log.warn(string.Format("ObjectFactory.newObject unknown type 0x{0:X}", type));

			return null;
		}
	}

}