namespace pspsharp.GUI
{

	public interface IMainGUI
	{
		void run();
		void pause();
		void reset();
		string MainTitle {set;}
		void RefreshButtons();
		void setLocation();
		DisplayMode DisplayMode {get;}
		bool FullScreen {get;}
		bool Visible {get;}
		void pack();
		void setFullScreenDisplaySize();

		/// <summary>
		/// Display a new window in front of the main window.
		/// If the main window is the full screen window, disable the full screen mode
		/// so that the new window can be displayed (no other window can be displayed
		/// in front of a full screen window).
		/// </summary>
		/// <param name="window">     the window to be displayed </param>
		void startWindowDialog(Window window);

		/// <summary>
		/// Display a new window but keep the focus on the main window.
		/// If the main window is the full screen window, disable the full screen mode
		/// so that the new window can be displayed (no other window can be displayed
		/// in front of a full screen window).
		/// </summary>
		/// <param name="window">     the window to be displayed </param>
		void startBackgroundWindowDialog(Window window);

		/// <summary>
		/// Restore the full screen window if required.
		/// </summary>
		void endWindowDialog();

		Rectangle CaptureRectangle {get;}
		void onUmdChange();
		void onMemoryStickChange();

		void setDisplayMinimumSize(int width, int height);
		void setDisplaySize(int width, int height);

		bool RunningFromVsh {get;}
		bool RunningReboot {get;}
	}

}