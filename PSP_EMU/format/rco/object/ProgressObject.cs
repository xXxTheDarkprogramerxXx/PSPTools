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
	using FloatType = pspsharp.format.rco.type.FloatType;
	using RefType = pspsharp.format.rco.type.RefType;
	using UnknownType = pspsharp.format.rco.type.UnknownType;

	public class ProgressObject : BasePositionObject
	{
		[ObjectField(order : 201)]
		public FloatType unknownFloat16;
		[ObjectField(order : 202)]
		public UnknownType unknown17;
		[ObjectField(order : 203)]
		public RefType unknownRef18;
		[ObjectField(order : 204)]
		public RefType unknownRef20;
		[ObjectField(order : 205)]
		public RefType unknownRef22;
	}

}