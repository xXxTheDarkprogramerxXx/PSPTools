using System.Collections.Generic;
using System.Threading;

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


	using keyCode = pspsharp.Controller.keyCode;
	using Settings = pspsharp.settings.Settings;

	using Component = net.java.games.input.Component;
	using Controller = net.java.games.input.Controller;
	using ControllerEnvironment = net.java.games.input.ControllerEnvironment;
	using Event = net.java.games.input.Event;
	using EventQueue = net.java.games.input.EventQueue;
	using Identifier = net.java.games.input.Component.Identifier;
	using Axis = net.java.games.input.Component.Identifier.Axis;
	using Button = net.java.games.input.Component.Identifier.Button;

	public class ControlsGUI : javax.swing.JFrame, KeyListener
	{

		private const long serialVersionUID = -732715495873159718L;
		public const string identifierForConfig = "controlsGUI";
		private bool getKey = false;
		private JTextField sender;
		private keyCode targetKey;
		private IDictionary<int, keyCode> currentKeys;
		private IDictionary<keyCode, int> revertKeys;
		private IDictionary<keyCode, string> currentController;
		private ControllerPollThread controllerPollThread;
		private const int maxControllerFieldValueLength = 9;

		private class ControllerPollThread : Thread
		{
			private readonly ControlsGUI outerInstance;

			public ControllerPollThread(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			volatile protected internal bool exit = false;

			public override void run()
			{
				while (!exit)
				{
					Controller controller = outerInstance.SelectedController;
					if (controller != null && controller.poll())
					{
						EventQueue eventQueue = controller.EventQueue;
						Event @event = new Event();
						while (eventQueue.getNextEvent(@event))
						{
							outerInstance.onControllerEvent(@event);
						}
					}

					// Wait a little bit before polling again...
					try
					{
						Thread.Sleep(10);
					}
					catch (InterruptedException)
					{
						// Ignore exception
					}
				}
			}
		}

		public ControlsGUI()
		{
			initComponents();
			loadKeys();

			Controller controller = pspsharp.Controller.Instance.InputController;
			if (controller != null)
			{
				for (int i = 0; i < controllerBox.ItemCount; i++)
				{
					if (controller == controllerBox.getItemAt(i))
					{
						controllerBox.SelectedIndex = i;
						break;
					}
				}
			}
			setFields();

			fieldCircle.addKeyListener(this);
			fieldCross.addKeyListener(this);
			fieldDown.addKeyListener(this);
			fieldLTrigger.addKeyListener(this);
			fieldLeft.addKeyListener(this);
			fieldRTrigger.addKeyListener(this);
			fieldRight.addKeyListener(this);
			fieldSelect.addKeyListener(this);
			fieldSquare.addKeyListener(this);
			fieldStart.addKeyListener(this);
			fieldTriangle.addKeyListener(this);
			fieldUp.addKeyListener(this);
			fieldHome.addKeyListener(this);
			fieldScreen.addKeyListener(this);
			fieldMusic.addKeyListener(this);
			fieldVolPlus.addKeyListener(this);
			fieldVolMin.addKeyListener(this);
			fieldHold.addKeyListener(this);
			fieldAnalogUp.addKeyListener(this);
			fieldAnalogDown.addKeyListener(this);
			fieldAnalogLeft.addKeyListener(this);
			fieldAnalogRight.addKeyListener(this);

			controllerBox.addItemListener(new ItemListenerAnonymousInnerClass(this));

			controllerPollThread = new ControllerPollThread(this);
			controllerPollThread.Name = "Controller Poll Thread";
			controllerPollThread.Daemon = true;
			controllerPollThread.Start();

			WindowPropSaver.loadWindowProperties(this);
		}

		private class ItemListenerAnonymousInnerClass : ItemListener
		{
			private readonly ControlsGUI outerInstance;

			public ItemListenerAnonymousInnerClass(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void itemStateChanged(ItemEvent e)
			{
				outerInstance.onControllerChange();
			}
		}

		public override void dispose()
		{
			if (controllerPollThread != null)
			{
				controllerPollThread.exit = true;
			}
			Emulator.MainGUI.endWindowDialog();
			base.dispose();
		}

		private void onControllerChange()
		{
			setFields();
		}

		private Controller SelectedController
		{
			get
			{
				if (controllerBox != null)
				{
					int controllerIndex = controllerBox.SelectedIndex;
					ControllerEnvironment ce = ControllerEnvironment.DefaultEnvironment;
					Controller[] controllers = ce.Controllers;
					if (controllers != null && controllerIndex >= 0 && controllerIndex < controllers.Length)
					{
						return controllers[controllerIndex];
					}
				}
    
				return null;
			}
		}

		private void loadKeys()
		{
			currentKeys = Settings.Instance.loadKeys();
			revertKeys = new Dictionary<keyCode, int>(typeof(keyCode));

			foreach (KeyValuePair<int, keyCode> entry in currentKeys.SetOfKeyValuePairs())
			{
				revertKeys[entry.Value] = entry.Key;
			}

			currentController = Settings.Instance.loadController();
		}

		private void setFieldValue(keyCode key, string value)
		{
			switch (key)
			{
				case keyCode.DOWN:
					fieldDown.Text = value;
					break;
				case keyCode.UP:
					fieldUp.Text = value;
					break;
				case keyCode.LEFT:
					fieldLeft.Text = value;
					break;
				case keyCode.RIGHT:
					fieldRight.Text = value;
					break;
				case keyCode.LANDOWN:
					fieldAnalogDown.Text = value;
					break;
				case keyCode.LANUP:
					fieldAnalogUp.Text = value;
					break;
				case keyCode.LANLEFT:
					fieldAnalogLeft.Text = value;
					break;
				case keyCode.LANRIGHT:
					fieldAnalogRight.Text = value;
					break;

				case keyCode.TRIANGLE:
					fieldTriangle.Text = value;
					break;
				case keyCode.SQUARE:
					fieldSquare.Text = value;
					break;
				case keyCode.CIRCLE:
					fieldCircle.Text = value;
					break;
				case keyCode.CROSS:
					fieldCross.Text = value;
					break;
				case keyCode.L1:
					fieldLTrigger.Text = value;
					break;
				case keyCode.R1:
					fieldRTrigger.Text = value;
					break;
				case keyCode.START:
					fieldStart.Text = value;
					break;
				case keyCode.SELECT:
					fieldSelect.Text = value;
					break;

				case keyCode.HOME:
					fieldHome.Text = value;
					break;
				case keyCode.HOLD:
					fieldHold.Text = value;
					break;
				case keyCode.VOLMIN:
					fieldVolMin.Text = value;
					break;
				case keyCode.VOLPLUS:
					fieldVolPlus.Text = value;
					break;
				case keyCode.SCREEN:
					fieldScreen.Text = value;
					break;
				case keyCode.MUSIC:
					fieldMusic.Text = value;
					break;
				case keyCode.RELEASED:
					break;
			}
		}

		private void setFields()
		{
			if (pspsharp.Controller.isKeyboardController(SelectedController))
			{
				foreach (KeyValuePair<int, keyCode> entry in currentKeys.SetOfKeyValuePairs())
				{
					setFieldValue(entry.Value, KeyEvent.getKeyText(entry.Key));
				}
			}
			else
			{
				foreach (KeyValuePair<keyCode, string> entry in currentController.SetOfKeyValuePairs())
				{
					string identifierName = entry.Value;
					setFieldValue(entry.Key, getControllerFieldText(identifierName));
				}
			}
		}

		public override void keyTyped(KeyEvent arg0)
		{
		}

		public override void keyReleased(KeyEvent arg0)
		{
		}

		public override void keyPressed(KeyEvent arg0)
		{
			if (!getKey)
			{
				return;
			}
			getKey = false;

			int pressedKey = arg0.KeyCode;
			keyCode k = currentKeys[pressedKey];

			if (k != null)
			{
				Emulator.Console.WriteLine("Key already used for " + k);
				sender.Text = KeyEvent.getKeyText(revertKeys[targetKey]);
				return;
			}

			int oldMapping = revertKeys[targetKey];
			revertKeys.Remove(targetKey);
			currentKeys.Remove(oldMapping);

			currentKeys[pressedKey] = targetKey;
			revertKeys[targetKey] = pressedKey;
			sender.Text = KeyEvent.getKeyText(pressedKey);

			getKey = false;
		}

		private void setKey(JTextField sender, keyCode targetKey)
		{
			if (getKey)
			{
				this.sender.Text = KeyEvent.getKeyText(revertKeys[this.targetKey]);
			}
			sender.Text = "PressKey";
			getKey = true;

			this.sender = sender;
			this.targetKey = targetKey;
		}

		private void setControllerMapping(keyCode targetKey, string identifierName, JTextField field)
		{
			currentController[targetKey] = identifierName;
			field.Text = getControllerFieldText(identifierName);
			getKey = false;
		}

		private Component getControllerComponent(string identifierName)
		{
			Controller controller = SelectedController;
			if (controller == null)
			{
				return null;
			}

			Component[] components = controller.Components;
			if (components == null)
			{
				return null;
			}

			for (int i = 0; i < components.Length; i++)
			{
				if (identifierName.Equals(components[i].Identifier.Name))
				{
					return components[i];
				}
			}

			return null;
		}

		private string getControllerFieldText(string identifierName)
		{
			Component component = getControllerComponent(identifierName);
			if (component == null)
			{
				return identifierName;
			}

			string name = component.Name;
			if (string.ReferenceEquals(name, null))
			{
				// Use the Identifier name if the component has no name
				name = identifierName;
			}
			else if (name.Length > maxControllerFieldValueLength && identifierName.Length < name.Length)
			{
				// Use the Identifier name if the component name is too long to fit
				// into the display field
				name = identifierName;
			}

			return name;
		}

		private void onControllerEvent(Event @event)
		{
			if (!getKey)
			{
				return;
			}

			Component component = @event.Component;
			float value = @event.Value;
			Component.Identifier identifier = component.Identifier;
			string identifierName = identifier.Name;

			if (identifier is Component.Identifier.Button && value == 1.0f)
			{
				setControllerMapping(targetKey, identifierName, sender);
			}
			else if (identifier == Component.Identifier.Axis.POV)
			{
				switch (targetKey)
				{
					case keyCode.DOWN:
					case keyCode.UP:
					case keyCode.LEFT:
					case keyCode.RIGHT:
						setControllerMapping(keyCode.DOWN, identifierName, fieldDown);
						setControllerMapping(keyCode.UP, identifierName, fieldUp);
						setControllerMapping(keyCode.LEFT, identifierName, fieldLeft);
						setControllerMapping(keyCode.RIGHT, identifierName, fieldRight);
						break;
					default:
						pspsharp.Controller.Console.WriteLine(string.Format("Unknown Controller POV Event on {0}({1}): {2:F} for {3}", component.Name, identifier.Name, value, targetKey.ToString()));
						break;
				}
			}
			else if (identifier is Component.Identifier.Axis && !pspsharp.Controller.isInDeadZone(component, value))
			{
				switch (targetKey)
				{
					case keyCode.DOWN:
					case keyCode.UP:
						setControllerMapping(keyCode.DOWN, identifierName, fieldDown);
						setControllerMapping(keyCode.UP, identifierName, fieldUp);
						break;
					case keyCode.LEFT:
					case keyCode.RIGHT:
						setControllerMapping(keyCode.LEFT, identifierName, fieldLeft);
						setControllerMapping(keyCode.RIGHT, identifierName, fieldRight);
						break;
					case keyCode.LANDOWN:
					case keyCode.LANUP:
						setControllerMapping(keyCode.LANDOWN, identifierName, fieldAnalogDown);
						setControllerMapping(keyCode.LANUP, identifierName, fieldAnalogUp);
						break;
					case keyCode.LANLEFT:
					case keyCode.LANRIGHT:
						setControllerMapping(keyCode.LANLEFT, identifierName, fieldAnalogLeft);
						setControllerMapping(keyCode.LANRIGHT, identifierName, fieldAnalogRight);
						break;
					default:
						setControllerMapping(targetKey, identifierName, sender);
						break;
				}
			}
			else
			{
				if (identifier is Component.Identifier.Axis && pspsharp.Controller.isInDeadZone(component, value))
				{
					pspsharp.Controller.Console.WriteLine(string.Format("Unknown Controller Event in DeadZone on {0}({1}): {2:F} for {3}", component.Name, identifier.Name, value, targetKey.ToString()));
				}
				else
				{
					pspsharp.Controller.Console.WriteLine(string.Format("Unknown Controller Event on {0}({1}): {2:F} for {3}", component.Name, identifier.Name, value, targetKey.ToString()));
				}
			}
		}

		public virtual ComboBoxModel makeControllerComboBoxModel()
		{
			MutableComboBoxModel comboBox = new DefaultComboBoxModel();
			ControllerEnvironment ce = ControllerEnvironment.DefaultEnvironment;
			Controller[] controllers = ce.Controllers;
			foreach (Controller c in controllers)
			{
				comboBox.addElement(c);
			}
			return comboBox;
		}

		/// <summary>
		/// This method is called from within the constructor to initialize the form.
		/// WARNING: Do NOT modify this code. The content of this method is always
		/// regenerated by the Form Editor.
		/// </summary>
		// <editor-fold defaultstate="collapsed" desc="Generated Code">//GEN-BEGIN:initComponents
		private void initComponents()
		{
			java.awt.GridBagConstraints gridBagConstraints;

			controllerLabel = new javax.swing.JLabel();
			controllerBox = new javax.swing.JComboBox();
			keyPanel = new javax.swing.JPanel();
			fgPanel = new javax.swing.JPanel();
			fieldStart = new JTextField();
			fieldSelect = new JTextField();
			fieldCross = new JTextField();
			fieldCircle = new JTextField();
			fieldTriangle = new JTextField();
			fieldSquare = new JTextField();
			fieldRight = new JTextField();
			fieldUp = new JTextField();
			fieldLeft = new JTextField();
			fieldDown = new JTextField();
			fieldHold = new JTextField();
			fieldHome = new JTextField();
			fieldVolMin = new JTextField();
			fieldVolPlus = new JTextField();
			fieldLTrigger = new JTextField();
			fieldRTrigger = new JTextField();
			fieldScreen = new JTextField();
			fieldMusic = new JTextField();
			fieldAnalogUp = new JTextField();
			fieldAnalogDown = new JTextField();
			fieldAnalogLeft = new JTextField();
			fieldAnalogRight = new JTextField();
			bgLabel1 = new javax.swing.JLabel();
			jButtonOK = new javax.swing.JButton();
			cancelButton = new pspsharp.GUI.CancelButton();

			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			Title = bundle.getString("ControlsGUI.title"); // NOI18N
			Resizable = false;

			controllerLabel.Text = bundle.getString("ControlsGUI.controllerLabel.text"); // NOI18N

			controllerBox.Model = makeControllerComboBoxModel();

			keyPanel.Background = new java.awt.Color(255, 255, 255);
			keyPanel.MinimumSize = new java.awt.Dimension(1, 1);
			keyPanel.Layout = new java.awt.GridBagLayout();

			fgPanel.Border = javax.swing.BorderFactory.createEtchedBorder(javax.swing.border.EtchedBorder.RAISED);
			fgPanel.Opaque = false;
			fgPanel.PreferredSize = new java.awt.Dimension(614, 312);

			fieldStart.Editable = false;
			fieldStart.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldStart.HorizontalAlignment = JTextField.CENTER;
			fieldStart.Text = "Enter"; // NOI18N
			fieldStart.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldStart.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldStart.addMouseListener(new MouseAdapterAnonymousInnerClass(this));

			fieldSelect.Editable = false;
			fieldSelect.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldSelect.HorizontalAlignment = JTextField.CENTER;
			fieldSelect.Text = "Space"; // NOI18N
			fieldSelect.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldSelect.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldSelect.addMouseListener(new MouseAdapterAnonymousInnerClass2(this));

			fieldCross.Editable = false;
			fieldCross.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldCross.HorizontalAlignment = JTextField.CENTER;
			fieldCross.Text = "S"; // NOI18N
			fieldCross.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldCross.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldCross.addMouseListener(new MouseAdapterAnonymousInnerClass3(this));

			fieldCircle.Editable = false;
			fieldCircle.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldCircle.HorizontalAlignment = JTextField.CENTER;
			fieldCircle.Text = "D"; // NOI18N
			fieldCircle.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldCircle.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldCircle.addMouseListener(new MouseAdapterAnonymousInnerClass4(this));

			fieldTriangle.Editable = false;
			fieldTriangle.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldTriangle.HorizontalAlignment = JTextField.CENTER;
			fieldTriangle.Text = "W"; // NOI18N
			fieldTriangle.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldTriangle.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldTriangle.addMouseListener(new MouseAdapterAnonymousInnerClass5(this));

			fieldSquare.Editable = false;
			fieldSquare.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldSquare.HorizontalAlignment = JTextField.CENTER;
			fieldSquare.Text = "A"; // NOI18N
			fieldSquare.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldSquare.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldSquare.addMouseListener(new MouseAdapterAnonymousInnerClass6(this));

			fieldRight.Editable = false;
			fieldRight.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldRight.HorizontalAlignment = JTextField.CENTER;
			fieldRight.Text = "Right"; // NOI18N
			fieldRight.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldRight.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldRight.addMouseListener(new MouseAdapterAnonymousInnerClass7(this));

			fieldUp.Editable = false;
			fieldUp.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldUp.HorizontalAlignment = JTextField.CENTER;
			fieldUp.Text = "Up"; // NOI18N
			fieldUp.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldUp.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldUp.addMouseListener(new MouseAdapterAnonymousInnerClass8(this));

			fieldLeft.Editable = false;
			fieldLeft.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldLeft.HorizontalAlignment = JTextField.CENTER;
			fieldLeft.Text = "Left"; // NOI18N
			fieldLeft.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldLeft.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldLeft.addMouseListener(new MouseAdapterAnonymousInnerClass9(this));

			fieldDown.Editable = false;
			fieldDown.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldDown.HorizontalAlignment = JTextField.CENTER;
			fieldDown.Text = "Down"; // NOI18N
			fieldDown.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldDown.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldDown.addMouseListener(new MouseAdapterAnonymousInnerClass10(this));

			fieldHold.Editable = false;
			fieldHold.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldHold.HorizontalAlignment = JTextField.CENTER;
			fieldHold.Text = "O"; // NOI18N
			fieldHold.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldHold.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldHold.addMouseListener(new MouseAdapterAnonymousInnerClass11(this));

			fieldHome.Editable = false;
			fieldHome.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldHome.HorizontalAlignment = JTextField.CENTER;
			fieldHome.Text = "H"; // NOI18N
			fieldHome.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldHome.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldHome.addMouseListener(new MouseAdapterAnonymousInnerClass12(this));

			fieldVolMin.Editable = false;
			fieldVolMin.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldVolMin.HorizontalAlignment = JTextField.CENTER;
			fieldVolMin.Text = "-"; // NOI18N
			fieldVolMin.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldVolMin.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldVolMin.addMouseListener(new MouseAdapterAnonymousInnerClass13(this));

			fieldVolPlus.Editable = false;
			fieldVolPlus.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldVolPlus.HorizontalAlignment = JTextField.CENTER;
			fieldVolPlus.Text = "+"; // NOI18N
			fieldVolPlus.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldVolPlus.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldVolPlus.addMouseListener(new MouseAdapterAnonymousInnerClass14(this));

			fieldLTrigger.Editable = false;
			fieldLTrigger.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldLTrigger.HorizontalAlignment = JTextField.CENTER;
			fieldLTrigger.Text = "Q"; // NOI18N
			fieldLTrigger.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldLTrigger.Border = new javax.swing.border.LineBorder(new java.awt.Color(0, 0, 0), 1, true);
			fieldLTrigger.addMouseListener(new MouseAdapterAnonymousInnerClass15(this));

			fieldRTrigger.Editable = false;
			fieldRTrigger.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldRTrigger.HorizontalAlignment = JTextField.CENTER;
			fieldRTrigger.Text = "E"; // NOI18N
			fieldRTrigger.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldRTrigger.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldRTrigger.addMouseListener(new MouseAdapterAnonymousInnerClass16(this));

			fieldScreen.Editable = false;
			fieldScreen.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldScreen.HorizontalAlignment = JTextField.CENTER;
			fieldScreen.Text = "N"; // NOI18N
			fieldScreen.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldScreen.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldScreen.addMouseListener(new MouseAdapterAnonymousInnerClass17(this));

			fieldMusic.Editable = false;
			fieldMusic.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldMusic.HorizontalAlignment = JTextField.CENTER;
			fieldMusic.Text = "M"; // NOI18N
			fieldMusic.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldMusic.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldMusic.addMouseListener(new MouseAdapterAnonymousInnerClass18(this));

			fieldAnalogUp.Editable = false;
			fieldAnalogUp.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldAnalogUp.HorizontalAlignment = JTextField.CENTER;
			fieldAnalogUp.Text = "I"; // NOI18N
			fieldAnalogUp.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldAnalogUp.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldAnalogUp.addMouseListener(new MouseAdapterAnonymousInnerClass19(this));

			fieldAnalogDown.Editable = false;
			fieldAnalogDown.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldAnalogDown.HorizontalAlignment = JTextField.CENTER;
			fieldAnalogDown.Text = "K"; // NOI18N
			fieldAnalogDown.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldAnalogDown.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldAnalogDown.addMouseListener(new MouseAdapterAnonymousInnerClass20(this));

			fieldAnalogLeft.Editable = false;
			fieldAnalogLeft.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldAnalogLeft.HorizontalAlignment = JTextField.CENTER;
			fieldAnalogLeft.Text = "J"; // NOI18N
			fieldAnalogLeft.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldAnalogLeft.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldAnalogLeft.addMouseListener(new MouseAdapterAnonymousInnerClass21(this));

			fieldAnalogRight.Editable = false;
			fieldAnalogRight.Font = new java.awt.Font("Courier New", 1, 12); // NOI18N
			fieldAnalogRight.HorizontalAlignment = JTextField.CENTER;
			fieldAnalogRight.Text = "L"; // NOI18N
			fieldAnalogRight.ToolTipText = bundle.getString("ControlsGUI.fieldPutKey.text"); // NOI18N
			fieldAnalogRight.Border = new javax.swing.border.LineBorder(new java.awt.Color(102, 102, 102), 1, true);
			fieldAnalogRight.addMouseListener(new MouseAdapterAnonymousInnerClass22(this));

			javax.swing.GroupLayout fgPanelLayout = new javax.swing.GroupLayout(fgPanel);
			fgPanel.Layout = fgPanelLayout;
			fgPanelLayout.HorizontalGroup = fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, fgPanelLayout.createSequentialGroup().addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(fgPanelLayout.createSequentialGroup().addComponent(fieldAnalogLeft, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(fieldAnalogRight, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE)).addGroup(fgPanelLayout.createSequentialGroup().addGap(35, 35, 35).addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.TRAILING).addGroup(fgPanelLayout.createSequentialGroup().addComponent(fieldAnalogDown, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(89, 89, 89).addComponent(fieldVolMin, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(46, 46, 46)).addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(fieldAnalogUp, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addGroup(fgPanelLayout.createSequentialGroup().addGap(103, 103, 103).addComponent(fieldHome, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(32, 32, 32).addComponent(fieldVolPlus, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE)))))).addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(fgPanelLayout.createSequentialGroup().addGap(35, 35, 35).addComponent(fieldScreen, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(0, 0, short.MaxValue)).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, fgPanelLayout.createSequentialGroup().addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, 76, short.MaxValue).addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, fgPanelLayout.createSequentialGroup().addComponent(fieldMusic, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(194, 194, 194)).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, fgPanelLayout.createSequentialGroup().addComponent(fieldSelect, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(72, 72, 72)))))).addGroup(fgPanelLayout.createSequentialGroup().addContainerGap().addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, fgPanelLayout.createSequentialGroup().addComponent(fieldDown, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(fieldCross, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE)).addGroup(fgPanelLayout.createSequentialGroup().addComponent(fieldLTrigger, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(fieldRTrigger, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE)).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, fgPanelLayout.createSequentialGroup().addComponent(fieldLeft, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(fieldCircle, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE)).addGroup(fgPanelLayout.createSequentialGroup().addComponent(fieldRight, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(fieldSquare, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE)).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, fgPanelLayout.createSequentialGroup().addComponent(fieldUp, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(fieldTriangle, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE)).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, fgPanelLayout.createSequentialGroup().addGap(0, 0, short.MaxValue).addComponent(fieldHold, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE))).addContainerGap()).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, fgPanelLayout.createSequentialGroup().addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(fieldStart, javax.swing.GroupLayout.PREFERRED_SIZE, 60, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(39, 39, 39));
			fgPanelLayout.VerticalGroup = fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(fgPanelLayout.createSequentialGroup().addGap(7, 7, 7).addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(fieldLTrigger, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(fieldRTrigger, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(fgPanelLayout.createSequentialGroup().addGap(18, 18, 18).addComponent(fieldRight, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(14, 14, 14).addComponent(fieldUp, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addGroup(fgPanelLayout.createSequentialGroup().addGap(24, 24, 24).addComponent(fieldSquare, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addComponent(fieldTriangle, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE))).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(fieldLeft, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(fieldCircle, javax.swing.GroupLayout.Alignment.TRAILING, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(fgPanelLayout.createSequentialGroup().addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(fgPanelLayout.createSequentialGroup().addGap(12, 12, 12).addComponent(fieldDown, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(82, 82, 82).addComponent(fieldAnalogUp, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addGroup(fgPanelLayout.createSequentialGroup().addGap(50, 50, 50).addComponent(fieldHold, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(18, 18, 18).addComponent(fieldStart, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(fieldSelect, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE))).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(fieldAnalogRight, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(fieldHome, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(fieldVolPlus, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addComponent(fieldAnalogLeft, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(fgPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(fieldAnalogDown, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(fieldVolMin, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addContainerGap(33, short.MaxValue)).addGroup(fgPanelLayout.createSequentialGroup().addGap(13, 13, 13).addComponent(fieldCross, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(fieldMusic, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(fieldScreen, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(36, 36, 36))));

			gridBagConstraints = new java.awt.GridBagConstraints();
			gridBagConstraints.gridx = 0;
			gridBagConstraints.gridy = 0;
			gridBagConstraints.ipadx = 10;
			gridBagConstraints.ipady = 12;
			gridBagConstraints.anchor = java.awt.GridBagConstraints.NORTHWEST;
			keyPanel.add(fgPanel, gridBagConstraints);

			bgLabel1.HorizontalAlignment = javax.swing.SwingConstants.CENTER;
			bgLabel1.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/images/controls.jpg")); // NOI18N
			bgLabel1.VerticalAlignment = javax.swing.SwingConstants.TOP;
			gridBagConstraints = new java.awt.GridBagConstraints();
			gridBagConstraints.gridx = 0;
			gridBagConstraints.gridy = 0;
			gridBagConstraints.anchor = java.awt.GridBagConstraints.NORTHWEST;
			keyPanel.add(bgLabel1, gridBagConstraints);

			jButtonOK.Text = bundle.getString("OkButton.text"); // NOI18N
			jButtonOK.addActionListener(new ActionListenerAnonymousInnerClass(this));

			cancelButton.Text = bundle.getString("CancelButton.text"); // NOI18N
			cancelButton.Parent = this;

			javax.swing.GroupLayout layout = new javax.swing.GroupLayout(ContentPane);
			ContentPane.Layout = layout;
			layout.HorizontalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addContainerGap(158, short.MaxValue).addComponent(controllerLabel).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(controllerBox, javax.swing.GroupLayout.PREFERRED_SIZE, 209, javax.swing.GroupLayout.PREFERRED_SIZE).addContainerGap(193, short.MaxValue)).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, layout.createSequentialGroup().addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(jButtonOK, javax.swing.GroupLayout.PREFERRED_SIZE, 88, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(cancelButton, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addContainerGap()).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addContainerGap().addComponent(keyPanel, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addContainerGap()));
			layout.VerticalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, layout.createSequentialGroup().addContainerGap().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(controllerBox, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(controllerLabel)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, 343, short.MaxValue).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(cancelButton, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(jButtonOK)).addContainerGap()).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addGap(46, 46, 46).addComponent(keyPanel, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(46, 46, 46)));

			pack();
		} // </editor-fold>//GEN-END:initComponents

		private class MouseAdapterAnonymousInnerClass : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldStartMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass2 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass2(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldSelectMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass3 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass3(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldCrossMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass4 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass4(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldCircleMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass5 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass5(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldTriangleMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass6 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass6(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldSquareMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass7 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass7(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldRightMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass8 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass8(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldUpMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass9 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass9(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldLeftMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass10 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass10(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldDownMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass11 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass11(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldHoldMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass12 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass12(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldHomeMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass13 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass13(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldVolMinMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass14 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass14(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldVolPlusMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass15 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass15(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldLTriggerMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass16 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass16(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldRTriggerMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass17 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass17(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldScreenMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass18 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass18(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldMusicMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass19 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass19(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldAnalogUpMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass20 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass20(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldAnalogDownMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass21 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass21(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldAnalogLeftMouseClicked(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass22 : java.awt.@event.MouseAdapter
		{
			private readonly ControlsGUI outerInstance;

			public MouseAdapterAnonymousInnerClass22(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.fieldAnalogRightMouseClicked(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass : java.awt.@event.ActionListener
		{
			private readonly ControlsGUI outerInstance;

			public ActionListenerAnonymousInnerClass(ControlsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.jButtonOKActionPerformed(evt);
			}
		}

	private void jButtonOKActionPerformed(java.awt.@event.ActionEvent evt)
	{ //GEN-FIRST:event_jButtonOKActionPerformed
			Settings.Instance.writeKeys(currentKeys);
			Settings.Instance.writeController(currentController);
			string controllerName = controllerBox.SelectedItem.ToString();
			Settings.Instance.writeString("controller.controllerName", controllerName);

			// Index when several controllers have the same name:
			// 0 refers to the first controller with the given name, 1, to the second...
			int controllerNameIndex = 0;
			int selectedIndex = controllerBox.SelectedIndex;
			for (int i = 0; i < controllerBox.ItemCount; i++)
			{
				if (controllerName.Equals(controllerBox.getItemAt(i).ToString()))
				{
					if (i < selectedIndex)
					{
						controllerNameIndex++;
					}
					else
					{
						break;
					}
				}
			}
			Settings.Instance.writeString("controller.controllerNameIndex", controllerNameIndex.ToString());

			State.controller.InputControllerIndex = controllerBox.SelectedIndex;
			State.controller.loadKeyConfig(currentKeys);
			State.controller.loadControllerConfig(currentController);

			dispose();
	} //GEN-LAST:event_jButtonOKActionPerformed

		private void fieldAnalogRightMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldAnalogRightMouseClicked
			setKey(fieldAnalogRight, keyCode.LANRIGHT);
		} //GEN-LAST:event_fieldAnalogRightMouseClicked

		private void fieldAnalogLeftMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldAnalogLeftMouseClicked
			setKey(fieldAnalogLeft, keyCode.LANLEFT);
		} //GEN-LAST:event_fieldAnalogLeftMouseClicked

		private void fieldAnalogDownMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldAnalogDownMouseClicked
			setKey(fieldAnalogDown, keyCode.LANDOWN);
		} //GEN-LAST:event_fieldAnalogDownMouseClicked

		private void fieldAnalogUpMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldAnalogUpMouseClicked
			setKey(fieldAnalogUp, keyCode.LANUP);
		} //GEN-LAST:event_fieldAnalogUpMouseClicked

		private void fieldMusicMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldMusicMouseClicked
			setKey(fieldMusic, keyCode.MUSIC);
		} //GEN-LAST:event_fieldMusicMouseClicked

		private void fieldScreenMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldScreenMouseClicked
			setKey(fieldScreen, keyCode.SCREEN);
		} //GEN-LAST:event_fieldScreenMouseClicked

		private void fieldRTriggerMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldRTriggerMouseClicked
			setKey(fieldRTrigger, keyCode.R1);
		} //GEN-LAST:event_fieldRTriggerMouseClicked

		private void fieldLTriggerMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldLTriggerMouseClicked
			setKey(fieldLTrigger, keyCode.L1);
		} //GEN-LAST:event_fieldLTriggerMouseClicked

		private void fieldVolPlusMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldVolPlusMouseClicked
			setKey(fieldVolPlus, keyCode.VOLPLUS);
		} //GEN-LAST:event_fieldVolPlusMouseClicked

		private void fieldVolMinMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldVolMinMouseClicked
			setKey(fieldVolMin, keyCode.VOLMIN);
		} //GEN-LAST:event_fieldVolMinMouseClicked

		private void fieldHomeMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldHomeMouseClicked
			setKey(fieldHome, keyCode.HOME);
		} //GEN-LAST:event_fieldHomeMouseClicked

		private void fieldHoldMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldHoldMouseClicked
			setKey(fieldHold, keyCode.HOLD);
		} //GEN-LAST:event_fieldHoldMouseClicked

		private void fieldDownMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldDownMouseClicked
			setKey(fieldDown, keyCode.DOWN);
		} //GEN-LAST:event_fieldDownMouseClicked

		private void fieldLeftMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldLeftMouseClicked
			setKey(fieldLeft, keyCode.LEFT);
		} //GEN-LAST:event_fieldLeftMouseClicked

		private void fieldUpMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldUpMouseClicked
			setKey(fieldUp, keyCode.UP);
		} //GEN-LAST:event_fieldUpMouseClicked

		private void fieldRightMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldRightMouseClicked
			setKey(fieldRight, keyCode.RIGHT);
		} //GEN-LAST:event_fieldRightMouseClicked

		private void fieldSquareMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldSquareMouseClicked
			setKey(fieldSquare, keyCode.SQUARE);
		} //GEN-LAST:event_fieldSquareMouseClicked

		private void fieldTriangleMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldTriangleMouseClicked
			setKey(fieldTriangle, keyCode.TRIANGLE);
		} //GEN-LAST:event_fieldTriangleMouseClicked

		private void fieldCircleMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldCircleMouseClicked
			setKey(fieldCircle, keyCode.CIRCLE);
		} //GEN-LAST:event_fieldCircleMouseClicked

		private void fieldCrossMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldCrossMouseClicked
			setKey(fieldCross, keyCode.CROSS);
		} //GEN-LAST:event_fieldCrossMouseClicked

		private void fieldSelectMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldSelectMouseClicked
			setKey(fieldSelect, keyCode.SELECT);
		} //GEN-LAST:event_fieldSelectMouseClicked

		private void fieldStartMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_fieldStartMouseClicked
			setKey(fieldStart, keyCode.START);
		} //GEN-LAST:event_fieldStartMouseClicked

		// Variables declaration - do not modify//GEN-BEGIN:variables
		private javax.swing.JLabel bgLabel1;
		private pspsharp.GUI.CancelButton cancelButton;
		private javax.swing.JComboBox controllerBox;
		private javax.swing.JLabel controllerLabel;
		private javax.swing.JPanel fgPanel;
		private JTextField fieldAnalogDown;
		private JTextField fieldAnalogLeft;
		private JTextField fieldAnalogRight;
		private JTextField fieldAnalogUp;
		private JTextField fieldCircle;
		private JTextField fieldCross;
		private JTextField fieldDown;
		private JTextField fieldHold;
		private JTextField fieldHome;
		private JTextField fieldLTrigger;
		private JTextField fieldLeft;
		private JTextField fieldMusic;
		private JTextField fieldRTrigger;
		private JTextField fieldRight;
		private JTextField fieldScreen;
		private JTextField fieldSelect;
		private JTextField fieldSquare;
		private JTextField fieldStart;
		private JTextField fieldTriangle;
		private JTextField fieldUp;
		private JTextField fieldVolMin;
		private JTextField fieldVolPlus;
		private javax.swing.JButton jButtonOK;
		private javax.swing.JPanel keyPanel;
		// End of variables declaration//GEN-END:variables
	}

}