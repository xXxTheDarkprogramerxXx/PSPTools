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
namespace pspsharp.format.rco.anim
{
	using FloatType = pspsharp.format.rco.type.FloatType;
	using IntType = pspsharp.format.rco.type.IntType;
	using ObjectType = pspsharp.format.rco.type.ObjectType;

	public class SlideOutAnim : BaseAnim
	{
		[ObjectField(order : 1)]
		public ObjectType @ref;
		[ObjectField(order : 2)]
		public FloatType duration;
		[ObjectField(order : 3)]
		public IntType accelMode;
		[ObjectField(order : 4)]
		public FloatType xspeed;
		[ObjectField(order : 5)]
		public FloatType yspeed;
		[ObjectField(order : 6)]
		public FloatType xcompress;
		[ObjectField(order : 7)]
		public FloatType ycompress;
	}

}