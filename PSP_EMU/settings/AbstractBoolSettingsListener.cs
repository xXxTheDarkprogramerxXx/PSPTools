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
	/// Base abstract class for a settings listener with boolean value.
	/// One of the "settingsValueChanged" method has to be overwritten by a concrete class.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class AbstractBoolSettingsListener : AbstractStringSettingsListener
	{
		/* (non-Javadoc)
		 * @see pspsharp.settings.AbstractStringSettingsListener#settingsValueChanged(java.lang.String, java.lang.String)
		 *
		 * This method should not be overwritten. Overwrite one the methods where
		 * the value is defined as a "boolean".
		 */
		public sealed override void settingsValueChanged(string option, string value)
		{
			settingsValueChanged(option, Settings.parseBool(value));
		}

		/// <summary>
		/// This method is called when the value of the registered settings option
		/// changes.
		/// </summary>
		/// <param name="option">    the option name </param>
		/// <param name="value">     the new option value </param>
		protected internal virtual void settingsValueChanged(string option, bool value)
		{
			settingsValueChanged(value);
		}

		/// <summary>
		/// This method is called when the value of the registered settings option
		/// changes.
		/// This method is equivalent to
		///     settingsValueChanged(String option, boolean value)
		/// but for simplicity, the option name is omitted.
		/// The option name is only relevant when the same settings listener is registered
		/// for multiple option name which is, for readability reasons, not recommended.
		/// </summary>
		/// <param name="value">     the new option value </param>
		protected internal virtual void settingsValueChanged(bool value)
		{
		}
	}

}