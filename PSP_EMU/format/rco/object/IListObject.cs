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
	using EventType = pspsharp.format.rco.type.EventType;
	using FloatType = pspsharp.format.rco.type.FloatType;

	public class IListObject : BasePositionObject
	{
		[ObjectField(order : 201)]
		public FloatType unknownFloat16;
		[ObjectField(order : 202)]
		public EventType onFocusIn;
		[ObjectField(order : 203)]
		public EventType onFocusOut;
		[ObjectField(order : 204)]
		public EventType onFocusLeft;
		[ObjectField(order : 205)]
		public EventType onFocusRight;
	}

}