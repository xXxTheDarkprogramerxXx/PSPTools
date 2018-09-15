using System;
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
namespace pspsharp.settings
{

	using keyCode = pspsharp.Controller.keyCode;
	using RecentElement = pspsharp.GUI.RecentElement;
	using Utilities = pspsharp.util.Utilities;

	/// 
	/// <summary>
	/// @author spip2001, gid15
	/// </summary>
	public class Settings
	{

		private const string SETTINGS_FILE_NAME = "Settings.properties";
		private const string DEFAULT_SETTINGS_FILE_NAME = "/pspsharp/DefaultSettings.properties";
		private static Settings instance = null;
		private Properties defaultSettings;
		private SortedProperties loadedSettings;
		private Properties patchSettings;
		private Dictionary<string, IList<ISettingsListener>> listenersByKey;
		private IList<SettingsListenerInfo> allListeners;
		private bool useUmdIdForDiscDirectory;
		private IDictionary<string, string> directoryMapping;

		public static Settings Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Settings();
				}
				return instance;
			}
		}

		private Settings()
		{
			listenersByKey = new Dictionary<string, IList<ISettingsListener>>();
			allListeners = new LinkedList<SettingsListenerInfo>();
			defaultSettings = new Properties();
			patchSettings = new Properties();
			directoryMapping = new Dictionary<string, string>();
			System.IO.Stream defaultSettingsStream = null;
			System.IO.Stream loadedSettingsStream = null;
			try
			{
				defaultSettingsStream = this.GetType().getResourceAsStream(DEFAULT_SETTINGS_FILE_NAME);
				defaultSettings.load(defaultSettingsStream);
				loadedSettings = new SortedProperties(defaultSettings);
				File settingsFile = new File(SETTINGS_FILE_NAME);
				settingsFile.createNewFile();
				loadedSettingsStream = new BufferedInputStream(new System.IO.FileStream(settingsFile, System.IO.FileMode.Open, System.IO.FileAccess.Read));
				loadedSettings.load(loadedSettingsStream);
			}
			catch (FileNotFoundException e)
			{
				Emulator.Console.WriteLine("Settings file not found:", e);
			}
			catch (IOException e)
			{
				Emulator.Console.WriteLine("Problem loading settings:", e);
			}
			catch (System.NullReferenceException e)
			{
				// This except is thrown by java.util.Properties when the directory
				// contains special characters or is too long.
				Emulator.Console.WriteLine("Could not initialize properly pspsharp, try to install pspsharp directly under C:\\pspsharp", e);
			}
			finally
			{
				Utilities.close(defaultSettingsStream, loadedSettingsStream);
			}

			// Set default directory mappings
			foreach (string directoryName in new string[] {"flash0", "flash1", "flash2", "ms0", "exdata0"})
			{
				setDirectoryMapping(directoryName, readString(directoryName, directoryName + "/"));
			}
		}

		public virtual string TmpDirectory
		{
			get
			{
				return readString("emu.tmppath") + System.IO.Path.DirectorySeparatorChar;
			}
		}

		public virtual string DiscTmpDirectory
		{
			get
			{
				return TmpDirectory + DiscDirectory;
			}
		}

		public virtual string DiscDirectory
		{
			get
			{
				if (useUmdIdForDiscDirectory)
				{
					return string.Format("{0}-{1}{2}", State.discId, State.umdId, System.IO.Path.DirectorySeparatorChar);
				}
				return string.Format("{0}{1}", State.discId, System.IO.Path.DirectorySeparatorChar);
			}
		}

		public virtual void loadPatchSettings()
		{
			Properties previousPatchSettings = new Properties(patchSettings);
			patchSettings.clear();

			string discId = State.discId;
			if (!discId.Equals(State.DISCID_UNKNOWN_FILE) && !discId.Equals(State.DISCID_UNKNOWN_NOTHING_LOADED))
			{
				// Try to read patch settings using the Disc ID and the UMD ID.
				string patchFileName = string.Format("patches/{0}-{1}.properties", discId, State.umdId);
				File patchFile = new File(patchFileName);
				if (!patchFile.exists())
				{
					// If no patch settings are found using the UMD ID, try with only the Disc ID
					patchFileName = string.Format("patches/{0}.properties", discId);
					patchFile = new File(patchFileName);
					useUmdIdForDiscDirectory = false;
				}
				else
				{
					useUmdIdForDiscDirectory = true;
				}

				System.IO.Stream patchSettingsStream = null;
				try
				{
					patchSettingsStream = new BufferedInputStream(new System.IO.FileStream(patchFile, System.IO.FileMode.Open, System.IO.FileAccess.Read));
					patchSettings.load(patchSettingsStream);
					Emulator.log.info(string.Format("Overwriting default settings with patch file '{0}'", patchFileName));
				}
				catch (FileNotFoundException e)
				{
					Emulator.Console.WriteLine(string.Format("Patch file not found: {0}", e.ToString()));
				}
				catch (IOException e)
				{
					Emulator.Console.WriteLine("Problem loading patch:", e);
				}
				finally
				{
					Utilities.close(patchSettingsStream);
				}
			}

			// Trigger the settings listener for all values modified
			// by the new patch settings.
			for (IEnumerator<object> e = patchSettings.keys(); e.MoveNext();)
			{
				string key = e.Current.ToString();
				previousPatchSettings.remove(key);
				string value = patchSettings.getProperty(key);
				if (!value.Equals(loadedSettings.getProperty(key)))
				{
					triggerSettingsListener(key, value);
				}
			}

			// Trigger the settings listener for all values that disappeared from the
			// previous patch settings.
			for (IEnumerator<object> e = previousPatchSettings.keys(); e.MoveNext();)
			{
				string key = e.Current.ToString();
				string oldValue = previousPatchSettings.getProperty(key);
				string newValue = getProperty(key);
				if (!oldValue.Equals(newValue))
				{
					triggerSettingsListener(key, newValue);
				}
			}
		}

		/// <summary>
		/// Write settings in file
		/// </summary>
		/// <param name="doc"> Settings as XML document </param>
		private void writeSettings()
		{
			BufferedOutputStream @out = null;
			try
			{
				@out = new BufferedOutputStream(new System.IO.FileStream(SETTINGS_FILE_NAME, System.IO.FileMode.Create, System.IO.FileAccess.Write));
				loadedSettings.store(@out, null);
			}
			catch (FileNotFoundException e)
			{
				Emulator.Console.WriteLine("Settings file not found:", e);
			}
			catch (IOException e)
			{
				Emulator.Console.WriteLine("Problem saving settings:", e);
			}
			finally
			{
				Utilities.close(@out);
			}
		}

		private string getProperty(string key)
		{
			string value = patchSettings.getProperty(key);
			if (string.ReferenceEquals(value, null))
			{
				value = loadedSettings.getProperty(key);
			}

			return value;
		}

		private string getProperty(string key, string defaultValue)
		{
			string value = patchSettings.getProperty(key);
			if (string.ReferenceEquals(value, null))
			{
				value = loadedSettings.getProperty(key, defaultValue);
			}

			return value;
		}

		private void setProperty(string key, string value)
		{
			string previousValue = getProperty(key);

			// Store the value in the loadedSettings,
			// the patchSettings staying unchanged.
			loadedSettings.setProperty(key, value);

			// Retrieve the new value (might be different from the value
			// just set in the loadedSettings as it might be overwritten by
			// a patchSettings).
			string newValue = getProperty(key);

			// Trigger the settings listener if this resulted in a changed value
			if (string.ReferenceEquals(previousValue, null) || !previousValue.Equals(newValue))
			{
				triggerSettingsListener(key, newValue);
			}
		}

		public virtual bool hasProperty(string key)
		{
			return loadedSettings.containsKey(key);
		}

		public virtual void clearProperty(string key)
		{
			loadedSettings.remove(key);
		}

		public virtual Point readWindowPos(string windowname)
		{
			string x = getProperty("gui.windows." + windowname + ".x");
			string y = getProperty("gui.windows." + windowname + ".y");

			// check if the read position is valid - i.e. already exists
			if (string.ReferenceEquals(x, null) || string.ReferenceEquals(y, null))
			{
				return null;
			}

			Point position = new Point();
			position.x = int.Parse(x);
			position.y = int.Parse(y);

			return position;
		}

		public virtual Dimension readWindowSize(string windowname)
		{
			string w = getProperty("gui.windows." + windowname + ".w");
			string h = getProperty("gui.windows." + windowname + ".h");

			// check if the read size is valid - i.e. already exists
			if (string.ReferenceEquals(w, null) || string.ReferenceEquals(h, null))
			{
				return null;
			}

			Dimension dimension = new Dimension();
			dimension.width = int.Parse(w);
			dimension.height = int.Parse(h);

			return dimension;
		}

		public virtual void writeWindowPos(string windowname, Point pos)
		{
			setProperty("gui.windows." + windowname + ".x", Convert.ToString(pos.x));
			setProperty("gui.windows." + windowname + ".y", Convert.ToString(pos.y));
			writeSettings();
		}

		public virtual void writeWindowSize(string windowname, Dimension dimension)
		{
			setProperty("gui.windows." + windowname + ".w", Convert.ToString(dimension.width));
			setProperty("gui.windows." + windowname + ".h", Convert.ToString(dimension.height));
			writeSettings();
		}

		public static bool parseBool(string value)
		{
			if ("true".Equals(value, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if ("false".Equals(value, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			return int.Parse(value) != 0;
		}

		public static int parseInt(string value)
		{
			value = value.Trim();
			if (value.StartsWith("0x", StringComparison.Ordinal))
			{
				return Convert.ToInt32(value.Substring(2), 16);
			}
			return int.Parse(value);
		}

		public static float parseFloat(string value)
		{
			return float.Parse(value);
		}

		public virtual bool readBool(string option)
		{
			string @bool = getProperty(option);
			if (string.ReferenceEquals(@bool, null))
			{
				return false;
			}

			return parseBool(@bool);
		}

		public virtual int readInt(string option)
		{
			return readInt(option, 0);
		}

		public virtual int readInt(string option, int defaultValue)
		{
			string value = getProperty(option);
			if (string.ReferenceEquals(value, null))
			{
				return defaultValue;
			}

			return parseInt(value);
		}

		public virtual void writeBool(string option, bool value)
		{
			string state = value ? "1" : "0";
			setProperty(option, state);
			writeSettings();
		}

		public virtual void writeInt(string option, int value)
		{
			string state = Convert.ToString(value);
			setProperty(option, state);
			writeSettings();
		}

		public virtual string readString(string option)
		{
			return readString(option, "");
		}

		public virtual string readString(string option, string defaultValue)
		{
			return getProperty(option, defaultValue);
		}

		public virtual bool isOptionFromPatch(string option)
		{
			return patchSettings.containsKey(option);
		}

		public virtual void writeString(string option, string value)
		{
			setProperty(option, value);
			writeSettings();
		}

		public virtual void writeFloat(string option, float value)
		{
			string state = Convert.ToString(value);
			setProperty(option, state);
			writeSettings();
		}

		public virtual float readFloat(string option, float defaultValue)
		{
			string value = getProperty(option);
			if (string.ReferenceEquals(value, null))
			{
				return defaultValue;
			}

			return parseFloat(value);
		}

		public virtual Dictionary<int, Controller.keyCode> loadKeys()
		{
			Dictionary<int, Controller.keyCode> m = new Dictionary<int, Controller.keyCode>(22);

			m[readKey("up")] = Controller.keyCode.UP;
			m[readKey("down")] = Controller.keyCode.DOWN;
			m[readKey("left")] = Controller.keyCode.LEFT;
			m[readKey("right")] = Controller.keyCode.RIGHT;
			m[readKey("analogUp")] = Controller.keyCode.LANUP;
			m[readKey("analogDown")] = Controller.keyCode.LANDOWN;
			m[readKey("analogLeft")] = Controller.keyCode.LANLEFT;
			m[readKey("analogRight")] = Controller.keyCode.LANRIGHT;
			if (Controller.Instance.hasRightAnalogController())
			{
				m[readKey("rightAnalogUp")] = Controller.keyCode.RANUP;
				m[readKey("rightAnalogDown")] = Controller.keyCode.RANDOWN;
				m[readKey("rightAnalogLeft")] = Controller.keyCode.RANLEFT;
				m[readKey("rightAnalogRight")] = Controller.keyCode.RANRIGHT;
			}
			m[readKey("start")] = Controller.keyCode.START;
			m[readKey("select")] = Controller.keyCode.SELECT;
			m[readKey("triangle")] = Controller.keyCode.TRIANGLE;
			m[readKey("square")] = Controller.keyCode.SQUARE;
			m[readKey("circle")] = Controller.keyCode.CIRCLE;
			m[readKey("cross")] = Controller.keyCode.CROSS;
			m[readKey("lTrigger")] = Controller.keyCode.L1;
			m[readKey("rTrigger")] = Controller.keyCode.R1;
			m[readKey("home")] = Controller.keyCode.HOME;
			m[readKey("hold")] = Controller.keyCode.HOLD;
			m[readKey("volPlus")] = Controller.keyCode.VOLPLUS;
			m[readKey("volMin")] = Controller.keyCode.VOLMIN;
			m[readKey("screen")] = Controller.keyCode.SCREEN;
			m[readKey("music")] = Controller.keyCode.MUSIC;

			return m;
		}

		public virtual IDictionary<Controller.keyCode, string> loadController()
		{
			IDictionary<Controller.keyCode, string> m = new Dictionary<Controller.keyCode, string>(typeof(Controller.keyCode));

			m[Controller.keyCode.UP] = readController("up");
			m[Controller.keyCode.DOWN] = readController("down");
			m[Controller.keyCode.LEFT] = readController("left");
			m[Controller.keyCode.RIGHT] = readController("right");
			m[Controller.keyCode.LANUP] = readController("analogUp");
			m[Controller.keyCode.LANDOWN] = readController("analogDown");
			m[Controller.keyCode.LANLEFT] = readController("analogLeft");
			m[Controller.keyCode.LANRIGHT] = readController("analogRight");
			if (Controller.Instance.hasRightAnalogController())
			{
				m[Controller.keyCode.RANUP] = readController("rightAnalogUp");
				m[Controller.keyCode.RANDOWN] = readController("rightAnalogDown");
				m[Controller.keyCode.RANLEFT] = readController("rightAnalogLeft");
				m[Controller.keyCode.RANRIGHT] = readController("rightAnalogRight");
			}
			m[Controller.keyCode.START] = readController("start");
			m[Controller.keyCode.SELECT] = readController("select");
			m[Controller.keyCode.TRIANGLE] = readController("triangle");
			m[Controller.keyCode.SQUARE] = readController("square");
			m[Controller.keyCode.CIRCLE] = readController("circle");
			m[Controller.keyCode.CROSS] = readController("cross");
			m[Controller.keyCode.L1] = readController("lTrigger");
			m[Controller.keyCode.R1] = readController("rTrigger");
			m[Controller.keyCode.HOME] = readController("home");
			m[Controller.keyCode.HOLD] = readController("hold");
			m[Controller.keyCode.VOLPLUS] = readController("volPlus");
			m[Controller.keyCode.VOLMIN] = readController("volMin");
			m[Controller.keyCode.SCREEN] = readController("screen");
			m[Controller.keyCode.MUSIC] = readController("music");

			// Removed unset entries
			foreach (Controller.keyCode key in Enum.GetValues(typeof(Controller.keyCode)))
			{
				if (string.ReferenceEquals(m[key], null))
				{
					m.Remove(key);
				}
			}

			return m;
		}

		public virtual void writeKeys(IDictionary<int, Controller.keyCode> keys)
		{
			foreach (KeyValuePair<int, Controller.keyCode> entry in keys.SetOfKeyValuePairs())
			{
				Controller.keyCode key = entry.Value;
				int value = entry.Key;

				switch (key)
				{
					case Controller.keyCode.DOWN:
						writeKey("down", value);
						break;
					case Controller.keyCode.UP:
						writeKey("up", value);
						break;
					case Controller.keyCode.LEFT:
						writeKey("left", value);
						break;
					case Controller.keyCode.RIGHT:
						writeKey("right", value);
						break;
					case Controller.keyCode.LANDOWN:
						writeKey("analogDown", value);
						break;
					case Controller.keyCode.LANUP:
						writeKey("analogUp", value);
						break;
					case Controller.keyCode.LANLEFT:
						writeKey("analogLeft", value);
						break;
					case Controller.keyCode.LANRIGHT:
						writeKey("analogRight", value);
						break;
					case Controller.keyCode.RANDOWN:
						writeKey("rightAnalogDown", value);
						break;
					case Controller.keyCode.RANUP:
						writeKey("rightAnalogUp", value);
						break;
					case Controller.keyCode.RANLEFT:
						writeKey("rightAnalogLeft", value);
						break;
					case Controller.keyCode.RANRIGHT:
						writeKey("rightAnalogRight", value);
						break;
					case Controller.keyCode.TRIANGLE:
						writeKey("triangle", value);
						break;
					case Controller.keyCode.SQUARE:
						writeKey("square", value);
						break;
					case Controller.keyCode.CIRCLE:
						writeKey("circle", value);
						break;
					case Controller.keyCode.CROSS:
						writeKey("cross", value);
						break;
					case Controller.keyCode.L1:
						writeKey("lTrigger", value);
						break;
					case Controller.keyCode.R1:
						writeKey("rTrigger", value);
						break;
					case Controller.keyCode.START:
						writeKey("start", value);
						break;
					case Controller.keyCode.SELECT:
						writeKey("select", value);
						break;
					case Controller.keyCode.HOME:
						writeKey("home", value);
						break;
					case Controller.keyCode.HOLD:
						writeKey("hold", value);
						break;
					case Controller.keyCode.VOLMIN:
						writeKey("volMin", value);
						break;
					case Controller.keyCode.VOLPLUS:
						writeKey("volPlus", value);
						break;
					case Controller.keyCode.SCREEN:
						writeKey("screen", value);
						break;
					case Controller.keyCode.MUSIC:
						writeKey("music", value);
						break;
					case Controller.keyCode.RELEASED:
						break;
				}
			}
			writeSettings();
		}

		public virtual void writeController(IDictionary<Controller.keyCode, string> keys)
		{
			foreach (KeyValuePair<Controller.keyCode, string> entry in keys.SetOfKeyValuePairs())
			{
				Controller.keyCode key = entry.Key;
				string value = entry.Value;

				switch (key)
				{
					case Controller.keyCode.DOWN:
						writeController("down", value);
						break;
					case Controller.keyCode.UP:
						writeController("up", value);
						break;
					case Controller.keyCode.LEFT:
						writeController("left", value);
						break;
					case Controller.keyCode.RIGHT:
						writeController("right", value);
						break;
					case Controller.keyCode.LANDOWN:
						writeController("analogDown", value);
						break;
					case Controller.keyCode.LANUP:
						writeController("analogUp", value);
						break;
					case Controller.keyCode.LANLEFT:
						writeController("analogLeft", value);
						break;
					case Controller.keyCode.LANRIGHT:
						writeController("analogRight", value);
						break;
					case Controller.keyCode.RANDOWN:
						writeController("rightAnalogDown", value);
						break;
					case Controller.keyCode.RANUP:
						writeController("rightAnalogUp", value);
						break;
					case Controller.keyCode.RANLEFT:
						writeController("rightAnalogLeft", value);
						break;
					case Controller.keyCode.RANRIGHT:
						writeController("rightAnalogRight", value);
						break;
					case Controller.keyCode.TRIANGLE:
						writeController("triangle", value);
						break;
					case Controller.keyCode.SQUARE:
						writeController("square", value);
						break;
					case Controller.keyCode.CIRCLE:
						writeController("circle", value);
						break;
					case Controller.keyCode.CROSS:
						writeController("cross", value);
						break;
					case Controller.keyCode.L1:
						writeController("lTrigger", value);
						break;
					case Controller.keyCode.R1:
						writeController("rTrigger", value);
						break;
					case Controller.keyCode.START:
						writeController("start", value);
						break;
					case Controller.keyCode.SELECT:
						writeController("select", value);
						break;
					case Controller.keyCode.HOME:
						writeController("home", value);
						break;
					case Controller.keyCode.HOLD:
						writeController("hold", value);
						break;
					case Controller.keyCode.VOLMIN:
						writeController("volMin", value);
						break;
					case Controller.keyCode.VOLPLUS:
						writeController("volPlus", value);
						break;
					case Controller.keyCode.SCREEN:
						writeController("screen", value);
						break;
					case Controller.keyCode.MUSIC:
						writeController("music", value);
						break;
					case Controller.keyCode.RELEASED:
						break;
				}
			}
			writeSettings();
		}

		private int readKey(string keyName)
		{
			string str = getProperty("keys." + keyName);
			if (string.ReferenceEquals(str, null))
			{
				return KeyEvent.VK_UNDEFINED;
			}
			return int.Parse(str);
		}

		private void writeKey(string keyName, int key)
		{
			setProperty("keys." + keyName, Convert.ToString(key));
		}

		private string readController(string name)
		{
			return getProperty("controller." + name);
		}

		private void writeController(string name, string value)
		{
			setProperty("controller." + name, value);
		}

		private class SortedProperties : Properties
		{

			internal const long serialVersionUID = -8127868945637348944L;

			public SortedProperties(Properties defaultSettings) : base(defaultSettings)
			{
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings({"unchecked", "rawtypes"}) public synchronized java.util.Iterator<Object> keys()
			public override IEnumerator<object> keys()
			{
				lock (this)
				{
					System.Collections.IEnumerator keysEnum = base.keys();
					System.Collections.IList keyList = Collections.list(keysEnum);
					keyList.Sort();
        
					return Collections.enumeration(keyList);
				}
			}
		}

		public virtual void readRecent(string cat, IList<RecentElement> recent)
		{
			for (int i = 0; true; ++i)
			{
				string r = getProperty("gui.recent." + cat + "." + i);
				if (string.ReferenceEquals(r, null))
				{
					break;
				}
				string title = getProperty("gui.recent." + cat + "." + i + ".title");
				recent.Add(new RecentElement(r, title));
			}
		}

		public virtual void writeRecent(string cat, IList<RecentElement> recent)
		{
			IEnumerator<object> keys = loadedSettings.keys();
			while (keys.MoveNext())
			{
				string key = (string) keys.Current;
				if (key.StartsWith("gui.recent." + cat, StringComparison.Ordinal))
				{
					loadedSettings.remove(key);
				}
			}
			int index = 0;
			foreach (RecentElement elem in recent)
			{
				setProperty("gui.recent." + cat + "." + index, elem.path);
				if (!string.ReferenceEquals(elem.title, null))
				{
					setProperty("gui.recent." + cat + "." + index + ".title", elem.title);
				}
				index++;
			}
			writeSettings();
		}
		/// <summary>
		/// Reads the following settings: gui.memStickBrowser.font.name=SansSerif
		/// gui.memStickBrowser.font.file= gui.memStickBrowser.font.size=11
		/// </summary>
		/// <returns> Tries to return a font in this order: - Font from local file
		/// (somefont.ttf), - Font registered with the operating system, - SansSerif,
		/// Plain, 11. </returns>
		private Font loadedFont = null;

		public virtual Font Font
		{
			get
			{
				if (loadedFont != null)
				{
					return loadedFont;
				}
    
				Font font = new Font("SansSerif", Font.PLAIN, 1);
				int fontsize = 11;
    
				try
				{
					Font @base = font; // Default font
					string fontname = readString("gui.font.name");
					string fontfilename = readString("gui.font.file");
					string fontsizestr = readString("gui.font.size");
    
					if (fontfilename.Length != 0)
					{
						// Load file font
						File fontfile = new File(fontfilename);
						if (fontfile.exists())
						{
							@base = Font.createFont(Font.TRUETYPE_FONT, fontfile);
						}
						else
						{
							Console.Error.WriteLine("gui.font.file '" + fontfilename + "' doesn't exist.");
						}
					}
					else if (fontname.Length != 0)
					{
						// Load system font
						@base = new Font(fontname, Font.PLAIN, 1);
					}
    
					// Set font size
					if (fontsizestr.Length > 0)
					{
						fontsize = int.Parse(fontsizestr);
					}
					else
					{
						Console.Error.WriteLine("gui.font.size setting is missing.");
					}
    
					font = @base.deriveFont(Font.PLAIN, fontsize);
    
					// register font as a font family so we can use it in StyledDocument's
					java.awt.GraphicsEnvironment ge = java.awt.GraphicsEnvironment.LocalGraphicsEnvironment;
					ge.registerFont(@base);
				}
				catch (System.FormatException)
				{
					Console.Error.WriteLine("gui.font.size setting is invalid.");
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					Console.WriteLine(e.Message);
				}
    
				loadedFont = font;
				return font;
			}
		}

		/// <summary>
		/// Register a settings listener for a specific option. The settings listener
		/// will be called as soon as the option value changes, e.g. when modifying
		/// the configuration through the GUI, or when loading a game having a patch
		/// file defined. The settings listener is also called immediately by this
		/// method while registering.
		/// 
		/// Only one settings listener can be defined for each name/option
		/// combination. This allows to call this method for the same listener
		/// multiple times and have it registered only once.
		/// </summary>
		/// <param name="name"> the name of the settings listener </param>
		/// <param name="option"> the settings option </param>
		/// <param name="listener"> the listener to be called when the settings option value
		/// changes </param>
		public virtual void registerSettingsListener(string name, string option, ISettingsListener listener)
		{
			removeSettingsListener(name, option);

			SettingsListenerInfo info = new SettingsListenerInfo(name, option, listener);
			allListeners.Add(info);
			IList<ISettingsListener> listenersForKey = listenersByKey[option];
			if (listenersForKey == null)
			{
				listenersForKey = new LinkedList<ISettingsListener>();
				listenersByKey[option] = listenersForKey;
			}
			listenersForKey.Add(listener);

			// Trigger the settings listener immediately if a value is defined
			string value = getProperty(option);
			if (!string.ReferenceEquals(value, null))
			{
				listener.settingsValueChanged(option, value);
			}
		}

		/// <summary>
		/// Remove the settings listeners matching the name and option parameters.
		/// </summary>
		/// <param name="name"> the name of the settings listener, or null to match any name </param>
		/// <param name="option"> the settings open, or null to match any settings option </param>
		public virtual void removeSettingsListener(string name, string option)
		{
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
			for (IEnumerator<SettingsListenerInfo> lit = allListeners.GetEnumerator(); lit.MoveNext();)
			{
				SettingsListenerInfo info = lit.Current;
				if (info.Equals(name, option))
				{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					lit.remove();
					string key = info.Key;
					IList<ISettingsListener> listenersForKey = listenersByKey[key];
					listenersForKey.Remove(info.Listener);
					if (listenersForKey.Count == 0)
					{
						listenersByKey.Remove(key);
					}
				}
			}
		}

		/// <summary>
		/// Remove all the settings listeners matching the name parameter.
		/// </summary>
		/// <param name="name"> the name of the settings listener, or null to match any name
		/// (in which case all the settings listeners will be removed). </param>
		public virtual void removeSettingsListener(string name)
		{
			removeSettingsListener(name, null);
		}

		/// <summary>
		/// Trigger the settings listener for a given settings key. This method has
		/// to be called when the value of a settings key changes.
		/// </summary>
		/// <param name="key"> the key </param>
		/// <param name="value"> the settings value </param>
		private void triggerSettingsListener(string key, string value)
		{
			IList<ISettingsListener> listenersForKey = listenersByKey[key];
			if (listenersForKey != null)
			{
				foreach (ISettingsListener listener in listenersForKey)
				{
					listener.settingsValueChanged(key, value);
				}
			}
		}

		public virtual void setDirectoryMapping(string directoryName, string mappedDirectoryName)
		{
			directoryMapping[directoryName] = mappedDirectoryName;
		}

		public virtual string getDirectoryMapping(string directoryName)
		{
			return directoryMapping[directoryName];
		}
	}

}