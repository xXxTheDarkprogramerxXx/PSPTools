using System;

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

	public class MetaInformation
	{
		public static string NAME = "pspsharp";
		public static string VERSION = "v0.7";
		public static string FULL_NAME = NAME + " " + VERSION;
		public static string OFFICIAL_SITE = "http://pspsharp.org/";
		public static string OFFICIAL_FORUM = "http://www.emunewz.net/forum/forumdisplay.php?fid=51";
		public static string OFFICIAL_REPOSITORY = "https://github.com/pspsharp/pspsharp";
		public static string TEAM = "pspsharp Team: gid15, Hykem, Orphis, shadow.<br/>" +
				"Past members and contributors: hlide, mad, dreampeppers99, wrayal,<br/> fiveofhearts, Nutzje, aisesal, shashClp, spip2, mozvip, gigaherz, <br/>Drakon, raziel1000, theball, J_BYYX, i30817, tempura.san.";

		private MetaInformation()
		{
			try
			{
				System.IO.Stream @is = this.GetType().getResourceAsStream("/pspsharp/title.txt");
				if (@is != null)
				{
					string customName = Utilities.ToString(@is, true).Trim();
					if (customName.Length == 0)
					{
						Console.Error.WriteLine("first line of title.txt is blank or file is empty");
					}
					else
					{
						FULL_NAME = NAME + " " + VERSION + " " + customName;
					}
				}
			}
			catch (IOException e)
			{
				Console.Error.WriteLine("something went wrong: " + e.Message);
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		static MetaInformation()
		{
			new MetaInformation();
		}
	}

}