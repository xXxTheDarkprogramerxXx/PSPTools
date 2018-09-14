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
namespace pspsharp.settings
{
	/// <summary>
	/// Interface for a settings listener.
	/// See Settings.registerSettingsListener for the registration of settings listeners.
	/// 
	/// @author gid15
	/// </summary>
	public interface ISettingsListener
	{
		/// <summary>
		/// This method is called when the value of the registered settings option
		/// changes.
		/// </summary>
		/// <param name="option">    the option name </param>
		/// <param name="value">     the new option value </param>
		void settingsValueChanged(string option, string value);
	}

}