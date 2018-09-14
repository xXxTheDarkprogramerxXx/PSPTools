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


	public class CancelButton : JButton
	{

		private const long serialVersionUID = 7544005354633954062L;
		private Window display;
		private const string escapeCommand = "Escape pressed";

		public CancelButton()
		{
			this.display = null;
		}

		public CancelButton(Window display)
		{
			Parent = display;
		}

		public Window Parent
		{
			set
			{
				this.display = value;
				init();
			}
		}

		private void init()
		{
			// dispose the display when the button is clicked
			addActionListener(new ActionListenerAnonymousInnerClass(this));

			// click the button when the ESC key is pressed
			registerKeyboardAction(new ActionListenerAnonymousInnerClass2(this)
		   , escapeCommand, KeyStroke.getKeyStroke(KeyEvent.VK_ESCAPE, 0), JComponent.WHEN_IN_FOCUSED_WINDOW);
		}

		private class ActionListenerAnonymousInnerClass : ActionListener
		{
			private readonly CancelButton outerInstance;

			public ActionListenerAnonymousInnerClass(CancelButton outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void actionPerformed(ActionEvent e)
			{
				if (outerInstance.display != null)
				{
					outerInstance.display.dispose();
				}
			}
		}

		private class ActionListenerAnonymousInnerClass2 : ActionListener
		{
			private readonly CancelButton outerInstance;

			public ActionListenerAnonymousInnerClass2(CancelButton outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void actionPerformed(ActionEvent @event)
			{
				if (escapeCommand.Equals(@event.ActionCommand))
				{
					doClick();
				}
			}
		}
	}

}