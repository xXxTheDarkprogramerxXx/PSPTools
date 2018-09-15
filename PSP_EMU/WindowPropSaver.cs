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
namespace pspsharp
{
	using Settings = pspsharp.settings.Settings;

	/// <summary>
	/// This class implements the automatic storing and loading of window positions
	/// for Frames and Dialogs.
	/// 
	/// @note Loading the window position only on the WINDOW_OPENED event might lead
	/// to flicker. See the comment on loadWindowProperties() for details.
	/// 
	/// @author tempura
	/// </summary>
	public class WindowPropSaver : AWTEventListener
	{

		public override void eventDispatched(AWTEvent awte)
		{
			Window window = (Window)((WindowEvent) awte).Component;
			switch (awte.ID)
			{
				case WindowEvent.WINDOW_DEACTIVATED:
					onSavePosition(window);
					break;
				case WindowEvent.WINDOW_OPENED:
					// this is only a fall-back for windows which do not call
					// loadWindowProperties() on GUI construction time
					// failing to do so will lead to a short flicker, as the window
					// is placed at the default position and moved afterwards after
					// signaling the OPENED event
					onLoadPosition(window);
					break;
			}
		}

		/// <summary>
		/// Put a call to this function into the Frame's contructor in order to
		/// ensure a flicker-free placing of the Frame.
		/// 
		/// @note Call this function as last line of the constructor.
		/// </summary>
		/// <param name="window"> The window to place and resize. </param>
		public static void loadWindowProperties(Window window)
		{
			onLoadPosition(window);
		}

		/// <summary>
		/// Only use this for MainGUI - as this is the only window which can be
		/// closed without being deactivated first.
		/// </summary>
		/// <param name="window"> The MainGUI window. </param>
		public static void saveWindowProperties(MainGUI mainGUI)
		{
			onSavePosition(mainGUI);
		}

		/// <summary>
		/// This will set the window position to either the stored position or to the
		/// screen center if not position was found.
		/// 
		/// @note It uses the class name as identifier in the settings.
		/// </summary>
		/// <param name="frame"> The frame to initialise. </param>
		/// <param name="identifierForConfig"> The identifier used in the settings file. </param>
		private static void onLoadPosition(Window window)
		{
			if (!isWindowFrameOrDialog(window))
			{
				return;
			}

			string identifierForConfig = window.GetType().Name;

			// do not load positions for standard dialogs (like file open)
			if (identifierForConfig.Equals("JDialog"))
			{
				return;
			}

			// MainGUI needs special handling due to being able to go fullscreen
			if (identifierForConfig.Equals("MainGUI") && Emulator.MainGUI.FullScreen)
			{
				return;
			}

			if (Settings.Instance.readBool("gui.saveWindowPos") && Settings.Instance.readWindowPos(identifierForConfig) != null)
			{

				Emulator.Console.WriteLine("loading window position of '" + identifierForConfig + "'");

				// LogWindow needs special handling if it shall be attached to the MainGUI
				if (!(identifierForConfig.Equals("LogWindow") && Settings.Instance.readBool("gui.snapLogwindow")))
				{
					window.Location = Settings.Instance.readWindowPos(identifierForConfig);
				}

				// read the size only if the frame is resizeable
				if (isWindowResizeable(window) && Settings.Instance.readWindowSize(identifierForConfig) != null)
				{
					window.Size = Settings.Instance.readWindowSize(identifierForConfig);
				}
			}
			else
			{
				// show the frame simply centered
				window.LocationRelativeTo = null;
			}
		}

		/// <summary>
		/// Store the current position of the window.
		/// 
		/// @note It uses the class name as identifier in the settings.
		/// </summary>
		/// <param name="frame"> The frame to initialise. </param>
		/// <param name="identifierForConfig"> The identifier used in the settings file. </param>
		private static void onSavePosition(Window window)
		{
			if (!isWindowFrameOrDialog(window))
			{
				return;
			}

			if (Settings.Instance.readBool("gui.saveWindowPos"))
			{
				string identifierForConfig = window.GetType().Name;

				// do not save positions for standard dialogs (like file open)
				if (identifierForConfig.Equals("JDialog"))
				{
					return;
				}

				// MainGUI needs special handling due to being able to go fullscreen
				if (identifierForConfig.Equals("MainGUI") && Emulator.MainGUI.FullScreen)
				{
					return;
				}

				Emulator.Console.WriteLine("saving window position of '" + identifierForConfig + "'");

				Settings.Instance.writeWindowPos(identifierForConfig, window.Location);

				// write the window size only if the window is resizeable
				if (isWindowResizeable(window))
				{
					Settings.Instance.writeWindowSize(identifierForConfig, window.Size);
				}
			}
		}

		/// <summary>
		/// Check if the given window object is resizeable.
		/// 
		/// @note Only done for Frames and Dialogs.
		/// </summary>
		/// <param name="window"> The window instance to check. </param>
		/// <returns> true if the window is resizable, false otherwise. </returns>
		private static bool isWindowResizeable(Window window)
		{
			return (window is Frame && ((Frame) window).Resizable) || (window is Dialog && ((Dialog) window).Resizable);
		}

		/// <summary>
		/// Check if the given window is an instance of Frame or Dialog.
		/// </summary>
		/// <param name="window"> The window instance to check. </param>
		/// <returns> true if the window is an instance of Frame or Dialog, else false. </returns>
		private static bool isWindowFrameOrDialog(Window window)
		{
			return (window is Frame) || (window is Dialog);
		}
	}

}