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
namespace pspsharp.format.rco
{

	using BaseObject = pspsharp.format.rco.@object.BaseObject;

	public class RCOContext
	{
		public sbyte[] buffer;
		public int offset;
		public IDictionary<int, string> events;
		public IDictionary<int, BufferedImage> images;
		public IDictionary<int, BaseObject> objects;

		public RCOContext(sbyte[] buffer, int offset, IDictionary<int, string> events, IDictionary<int, BufferedImage> images, IDictionary<int, BaseObject> objects)
		{
			this.buffer = buffer;
			this.offset = offset;
			this.events = events;
			this.images = images;
			this.objects = objects;
		}
	}

}