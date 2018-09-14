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
	/// Simple container for a registered settings listener.
	/// 
	/// See
	///     Settings.registerSettingsListener()
	/// for the the registration of settings listeners.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class SettingsListenerInfo
	{
		private string name;
		private string key;
		private ISettingsListener listener;

		public SettingsListenerInfo(string name, string key, ISettingsListener listener)
		{
			this.name = name;
			this.key = key;
			this.listener = listener;
		}

		public virtual string Name
		{
			get
			{
				return name;
			}
		}

		public virtual string Key
		{
			get
			{
				return key;
			}
		}

		public virtual ISettingsListener Listener
		{
			get
			{
				return listener;
			}
		}

		/// <summary>
		/// Test if the current object is matching the given name and key values.
		/// A null value matches any value.
		/// </summary>
		/// <param name="name">     name, or null to match any name. </param>
		/// <param name="key">      key, or null to match any key.
		/// @return </param>
		public virtual bool Equals(string name, string key)
		{
			if (!string.ReferenceEquals(name, null))
			{
				if (!this.name.Equals(name))
				{
					return false;
				}
			}

			if (!string.ReferenceEquals(key, null))
			{
				if (!this.key.Equals(key))
				{
					return false;
				}
			}

			return true;
		}
	}

}