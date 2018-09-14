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
namespace pspsharp.util
{

	public class Constants
	{

		public static readonly FileNameExtensionFilter fltTextFiles = new FileNameExtensionFilter("Text files", "txt");
		public static readonly FileNameExtensionFilter fltMemoryBreakpointFiles = new FileNameExtensionFilter("Memory breakpoint files", "mbrk");
		public static readonly FileNameExtensionFilter fltBreakpointFiles = new FileNameExtensionFilter("Breakpoint files", "brk");
		public static readonly FileNameExtensionFilter fltXMLFiles = new FileNameExtensionFilter("XML files", "xml");
		public const int ICON0_WIDTH = 144;
		public const int ICON0_HEIGHT = 80;
		public const int PSPSCREEN_WIDTH = 480;
		public const int PSPSCREEN_HEIGHT = 272;
		public static readonly Charset charset = Charset.forName("UTF-8");
	}

}