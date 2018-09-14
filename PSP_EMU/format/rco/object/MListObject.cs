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
	using IntType = pspsharp.format.rco.type.IntType;
	using RefType = pspsharp.format.rco.type.RefType;
	using UnknownType = pspsharp.format.rco.type.UnknownType;

	public class MListObject : BasePositionObject
	{
		[ObjectField(order : 201)]
		public IntType numVisibleElements;
		[ObjectField(order : 202)]
		public UnknownType unknown17;
		[ObjectField(order : 203)]
		public IntType initTopPadding;
		[ObjectField(order : 204)]
		public FloatType unknownFloat19;
		[ObjectField(order : 205)]
		public FloatType itemSpacing;
		[ObjectField(order : 206)]
		public RefType unknownRef21;
		[ObjectField(order : 207)]
		public EventType onPush;
		[ObjectField(order : 208)]
		public EventType onCursorMove;
		[ObjectField(order : 209)]
		public EventType onFocusIn;
		[ObjectField(order : 210)]
		public EventType onFocusOut;
		[ObjectField(order : 211)]
		public EventType onFocusLeft;
		[ObjectField(order : 212)]
		public EventType onFocusRight;
		[ObjectField(order : 213)]
		public EventType onFocusUp;
		[ObjectField(order : 214)]
		public EventType onFocusDown;
		[ObjectField(order : 215)]
		public EventType onScrollIn;
		[ObjectField(order : 216)]
		public EventType onScrollOut;
	}

}