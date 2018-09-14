using System.Windows.Forms;

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

	public class JpcspDialogManager
	{

		public static void showInformation(Component compo, string message)
		{
			MessageBox.Show(compo, message, MetaInformation.FULL_NAME, MessageBoxIcon.Information);
		}

		public static void showWarning(Component compo, string message)
		{
			MessageBox.Show(compo, message, MetaInformation.FULL_NAME, MessageBoxIcon.Exclamation);
		}

		public static void showError(Component compo, string message)
		{
			MessageBox.Show(compo, message, MetaInformation.FULL_NAME, MessageBoxIcon.Error);
		}
	}

}