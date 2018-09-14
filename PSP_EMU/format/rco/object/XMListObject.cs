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
	using ImageType = pspsharp.format.rco.type.ImageType;
	using RefType = pspsharp.format.rco.type.RefType;
	using UnknownType = pspsharp.format.rco.type.UnknownType;

	public class XMListObject : BaseObject
	{
		[ObjectField(order : 1)]
		public UnknownType unknown0;
		[ObjectField(order : 2)]
		public ImageType image;
		[ObjectField(order : 3)]
		public RefType unknownRef3;
	}

}