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
namespace pspsharp.GUI
{
	public class RecentElement
	{
		public string path, title;

		public RecentElement(string path, string title)
		{
			this.path = path;
			this.title = title;
		}

		public override string ToString()
		{
			return (string.ReferenceEquals(title, null) ? "" : title + " - ") + path;
		}
	}

}