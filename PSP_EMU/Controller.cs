using System.Collections.Generic;
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
namespace pspsharp
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_CIRCLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_CROSS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_DOWN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_HOLD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_HOME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_LEFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_LTRIGGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_NOTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_RIGHT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_RTRIGGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_SCREEN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_SELECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_SQUARE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_START;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_TRIANGLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_UP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_VOLDOWN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_VOLUP;
	using Audio = pspsharp.hardware.Audio;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using Settings = pspsharp.settings.Settings;
	using Modules = pspsharp.HLE.Modules;


	//using Logger = org.apache.log4j.Logger;

	using Component = net.java.games.input.Component;
	using ControllerEnvironment = net.java.games.input.ControllerEnvironment;
	using Event = net.java.games.input.Event;
	using EventQueue = net.java.games.input.EventQueue;
	using Identifier = net.java.games.input.Component.Identifier;
	using POV = net.java.games.input.Component.POV;
	using Axis = net.java.games.input.Component.Identifier.Axis;
	using Type = net.java.games.input.Controller.Type;

	public class Controller
	{

		//public static Logger log = Logger.getLogger("controller");
		private static Controller instance;
		public static readonly sbyte analogCenter = unchecked((sbyte) 128);
		// Left analog stick
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private sbyte Lx_Renamed = analogCenter;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private sbyte Ly_Renamed = analogCenter;
		// PSP emulator on PS3 can also provide the right analog stick
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private sbyte Rx_Renamed = analogCenter;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private sbyte Ry_Renamed = analogCenter;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private int Buttons_Renamed = 0;
		private keyCode lastKey = keyCode.RELEASED;
		private net.java.games.input.Controller inputController;
		private Dictionary<Component.Identifier, int> buttonComponents;
		private Component.Identifier analogLXAxis = Component.Identifier.Axis.X;
		private Component.Identifier analogLYAxis = Component.Identifier.Axis.Y;
		private Component.Identifier analogRXAxis = null;
		private Component.Identifier analogRYAxis = null;
		private Component.Identifier digitalXAxis = null;
		private Component.Identifier digitalYAxis = null;
		private Component.Identifier povArrows = Component.Identifier.Axis.POV;
		private const float minimumDeadZone = 0.1f;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool hasRightAnalogController_Renamed = false;
		private Dictionary<keyCode, string> controllerComponents;
		private Dictionary<int, keyCode> keys;

		public enum keyCode
		{
			UP,
			DOWN,
			LEFT,
			RIGHT,
			LANUP,
			LANDOWN,
			LANLEFT,
			LANRIGHT,
			RANUP,
			RANDOWN,
			RANLEFT,
			RANRIGHT,
			START,
			SELECT,
			TRIANGLE,
			SQUARE,
			CIRCLE,
			CROSS,
			L1,
			R1,
			HOME,
			HOLD,
			VOLMIN,
			VOLPLUS,
			SCREEN,
			MUSIC,
			RELEASED
		}

		private class RightAnalogControllerSettingsListener : AbstractBoolSettingsListener
		{
			private readonly Controller outerInstance;

			public RightAnalogControllerSettingsListener(Controller outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			protected internal override void settingsValueChanged(bool value)
			{
				outerInstance.HasRightAnalogController = value;
			}
		}

		protected internal Controller(net.java.games.input.Controller inputController)
		{
			this.inputController = inputController;
			Settings.Instance.registerSettingsListener("hasRightAnalogController", "hasRightAnalogController", new RightAnalogControllerSettingsListener(this));
		}

		private void init()
		{
			keys = new Dictionary<int, keyCode>(22);
			controllerComponents = new Dictionary<keyCode, string>(22);
			loadKeyConfig();
			loadControllerConfig();
		}

		public static bool isKeyboardController(net.java.games.input.Controller inputController)
		{
			return inputController == null || inputController.Type == Type.KEYBOARD;
		}

		public static Controller Instance
		{
			get
			{
				if (instance == null)
				{
					// Disable JInput messages sent to stdout...
					java.util.logging.Logger.getLogger("net.java.games.input.DefaultControllerEnvironment").Level = Level.WARNING;
    
					ControllerEnvironment ce = ControllerEnvironment.DefaultEnvironment;
					net.java.games.input.Controller[] controllers = ce.Controllers;
					net.java.games.input.Controller inputController = null;
    
					// Reuse the controller from the settings
					string controllerName = Settings.Instance.readString("controller.controllerName");
					// The controllerNameIndex is the index when several controllers have
					// the same name. 0 to use the first controller with the given name,
					// 1, to use the second...
					int controllerNameIndex = Settings.Instance.readInt("controller.controllerNameIndex", 0);
					if (!string.ReferenceEquals(controllerName, null))
					{
						for (int i = 0; controllers != null && i < controllers.Length; i++)
						{
							if (controllerName.Equals(controllers[i].Name))
							{
								inputController = controllers[i];
								if (controllerNameIndex <= 0)
								{
									break;
								}
								controllerNameIndex--;
							}
						}
					}
    
					if (inputController == null)
					{
						// Use the first KEYBOARD controller
						for (int i = 0; controllers != null && i < controllers.Length; i++)
						{
							if (isKeyboardController(controllers[i]))
							{
								inputController = controllers[i];
								break;
							}
						}
					}
    
					if (inputController == null)
					{
						log.info(string.Format("No KEYBOARD controller found"));
						for (int i = 0; controllers != null && i < controllers.Length; i++)
						{
							log.info(string.Format("    Controller: '{0}'", controllers[i].Name));
						}
					}
					else
					{
						log.info(string.Format("Using default controller '{0}'", inputController.Name));
					}
					instance = new Controller(inputController);
					instance.init();
				}
    
				return instance;
			}
		}

		public virtual net.java.games.input.Controller InputController
		{
			set
			{
				if (value != null)
				{
					log.info(string.Format("Using controller '{0}'", value.Name));
				}
				this.inputController = value;
				onInputControllerChanged();
			}
			get
			{
				return inputController;
			}
		}


		public virtual int InputControllerIndex
		{
			set
			{
				ControllerEnvironment ce = ControllerEnvironment.DefaultEnvironment;
				net.java.games.input.Controller[] controllers = ce.Controllers;
				if (controllers != null && value >= 0 && value < controllers.Length)
				{
					InputController = controllers[value];
				}
			}
		}

		public virtual void loadKeyConfig()
		{
			loadKeyConfig(Settings.Instance.loadKeys());
		}

		public virtual void loadKeyConfig(IDictionary<int, keyCode> newLayout)
		{
			keys.Clear();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			keys.putAll(newLayout);
		}

		public virtual void loadControllerConfig()
		{
			loadControllerConfig(Settings.Instance.loadController());
		}

		public virtual void loadControllerConfig(IDictionary<keyCode, string> newLayout)
		{
			controllerComponents.Clear();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			controllerComponents.putAll(newLayout);

			onInputControllerChanged();
		}

		private void onInputControllerChanged()
		{
			buttonComponents = new Dictionary<Component.Identifier, int>();
			foreach (KeyValuePair<keyCode, string> entry in controllerComponents.SetOfKeyValuePairs())
			{
				keyCode key = entry.Key;
				string controllerName = entry.Value;
				Component component = getControllerComponentByName(controllerName);
				if (component != null)
				{
					Component.Identifier identifier = component.Identifier;
					bool isAxis = identifier is Component.Identifier.Axis;

					if (isAxis && identifier == Component.Identifier.Axis.POV)
					{
						povArrows = identifier;
					}
					else
					{
						int keyCode = -1;
						switch (key)
						{
							//
							// PSP directional buttons can be mapped
							// to a controller Axis or to a controller Button
							//
							case pspsharp.Controller.keyCode.DOWN:
								if (isAxis)
								{
									digitalYAxis = identifier;
								}
								else
								{
									keyCode = PSP_CTRL_DOWN;
								}
								break;
							case pspsharp.Controller.keyCode.UP:
								if (isAxis)
								{
									digitalYAxis = identifier;
								}
								else
								{
									keyCode = PSP_CTRL_UP;
								}
								break;
							case pspsharp.Controller.keyCode.LEFT:
								if (isAxis)
								{
									digitalXAxis = identifier;
								}
								else
								{
									keyCode = PSP_CTRL_LEFT;
								}
								break;
							case pspsharp.Controller.keyCode.RIGHT:
								if (isAxis)
								{
									digitalXAxis = identifier;
								}
								else
								{
									keyCode = PSP_CTRL_RIGHT;
								}
								break;
							//
							// PSP analog controller can only be mapped to a controller Axis
							//
							case pspsharp.Controller.keyCode.LANDOWN:
							case pspsharp.Controller.keyCode.LANUP:
								if (isAxis)
								{
									analogLYAxis = identifier;
								}
								break;
							case pspsharp.Controller.keyCode.LANLEFT:
							case pspsharp.Controller.keyCode.LANRIGHT:
								if (isAxis)
								{
									analogLXAxis = identifier;
								}
								break;
							case pspsharp.Controller.keyCode.RANDOWN:
							case pspsharp.Controller.keyCode.RANUP:
								if (isAxis)
								{
									analogRYAxis = identifier;
								}
								break;
							case pspsharp.Controller.keyCode.RANLEFT:
							case pspsharp.Controller.keyCode.RANRIGHT:
								if (isAxis)
								{
									analogRXAxis = identifier;
								}
								break;
							//
							// PSP buttons can be mapped either to a controller Button
							// or to a controller Axis (e.g. a foot pedal)
							//
							case pspsharp.Controller.keyCode.TRIANGLE:
								keyCode = PSP_CTRL_TRIANGLE;
								break;
							case pspsharp.Controller.keyCode.SQUARE:
								keyCode = PSP_CTRL_SQUARE;
								break;
							case pspsharp.Controller.keyCode.CIRCLE:
								keyCode = PSP_CTRL_CIRCLE;
								break;
							case pspsharp.Controller.keyCode.CROSS:
								keyCode = PSP_CTRL_CROSS;
								break;
							case pspsharp.Controller.keyCode.L1:
								keyCode = PSP_CTRL_LTRIGGER;
								break;
							case pspsharp.Controller.keyCode.R1:
								keyCode = PSP_CTRL_RTRIGGER;
								break;
							case pspsharp.Controller.keyCode.START:
								keyCode = PSP_CTRL_START;
								break;
							case pspsharp.Controller.keyCode.SELECT:
								keyCode = PSP_CTRL_SELECT;
								break;
							case pspsharp.Controller.keyCode.HOME:
								keyCode = PSP_CTRL_HOME;
								break;
							case pspsharp.Controller.keyCode.HOLD:
								keyCode = PSP_CTRL_HOLD;
								break;
							case pspsharp.Controller.keyCode.VOLMIN:
								keyCode = PSP_CTRL_VOLDOWN;
								break;
							case pspsharp.Controller.keyCode.VOLPLUS:
								keyCode = PSP_CTRL_VOLUP;
								break;
							case pspsharp.Controller.keyCode.SCREEN:
								keyCode = PSP_CTRL_SCREEN;
								break;
							case pspsharp.Controller.keyCode.MUSIC:
								keyCode = PSP_CTRL_NOTE;
								break;
							case pspsharp.Controller.keyCode.RELEASED:
								break;
						}
						if (keyCode != -1)
						{
							buttonComponents[component.Identifier] = keyCode;
						}
					}
				}
			}
		}

		/// <summary>
		/// Called by sceCtrl at every VBLANK interrupt.
		/// </summary>
		public virtual void hleControllerPoll()
		{
			processSpecialKeys();
			pollController();
		}

		private void pollController()
		{
			if (inputController != null && inputController.poll())
			{
				EventQueue eventQueue = inputController.EventQueue;
				Event @event = new Event();
				while (eventQueue.getNextEvent(@event))
				{
					Component component = @event.Component;
					float value = @event.Value;
					processControllerEvent(component, value);
				}
			}
		}

		public virtual void keyPressed(KeyEvent keyEvent)
		{
			keyPressed(keyEvent.KeyCode);
		}

		public virtual void keyPressed(int keyCode)
		{
			keyCode key = keys[keyCode];
			keyPressed(key);
		}

		public virtual void keyPressed(keyCode key)
		{
			if (key == null || key == lastKey)
			{
				return;
			}

			switch (key)
			{
				case pspsharp.Controller.keyCode.DOWN:
					Buttons_Renamed |= PSP_CTRL_DOWN;
					break;
				case pspsharp.Controller.keyCode.UP:
					Buttons_Renamed |= PSP_CTRL_UP;
					break;
				case pspsharp.Controller.keyCode.LEFT:
					Buttons_Renamed |= PSP_CTRL_LEFT;
					break;
				case pspsharp.Controller.keyCode.RIGHT:
					Buttons_Renamed |= PSP_CTRL_RIGHT;
					break;
				case pspsharp.Controller.keyCode.LANDOWN:
					Ly_Renamed = unchecked((sbyte) 255);
					break;
				case pspsharp.Controller.keyCode.LANUP:
					Ly_Renamed = 0;
					break;
				case pspsharp.Controller.keyCode.LANLEFT:
					Lx_Renamed = 0;
					break;
				case pspsharp.Controller.keyCode.LANRIGHT:
					Lx_Renamed = unchecked((sbyte) 255);
					break;
				case pspsharp.Controller.keyCode.RANDOWN:
					Ry_Renamed = unchecked((sbyte) 255);
					break;
				case pspsharp.Controller.keyCode.RANUP:
					Ry_Renamed = 0;
					break;
				case pspsharp.Controller.keyCode.RANLEFT:
					Rx_Renamed = 0;
					break;
				case pspsharp.Controller.keyCode.RANRIGHT:
					Rx_Renamed = unchecked((sbyte) 255);
					break;

				case pspsharp.Controller.keyCode.TRIANGLE:
					Buttons_Renamed |= PSP_CTRL_TRIANGLE;
					break;
				case pspsharp.Controller.keyCode.SQUARE:
					Buttons_Renamed |= PSP_CTRL_SQUARE;
					break;
				case pspsharp.Controller.keyCode.CIRCLE:
					Buttons_Renamed |= PSP_CTRL_CIRCLE;
					break;
				case pspsharp.Controller.keyCode.CROSS:
					Buttons_Renamed |= PSP_CTRL_CROSS;
					break;
				case pspsharp.Controller.keyCode.L1:
					Buttons_Renamed |= PSP_CTRL_LTRIGGER;
					break;
				case pspsharp.Controller.keyCode.R1:
					Buttons_Renamed |= PSP_CTRL_RTRIGGER;
					break;
				case pspsharp.Controller.keyCode.START:
					Buttons_Renamed |= PSP_CTRL_START;
					break;
				case pspsharp.Controller.keyCode.SELECT:
					Buttons_Renamed |= PSP_CTRL_SELECT;
					break;

				case pspsharp.Controller.keyCode.HOME:
					Buttons_Renamed |= PSP_CTRL_HOME;
					break;
				case pspsharp.Controller.keyCode.HOLD:
					Buttons_Renamed |= PSP_CTRL_HOLD;
					break;
				case pspsharp.Controller.keyCode.VOLMIN:
					Buttons_Renamed |= PSP_CTRL_VOLDOWN;
					break;
				case pspsharp.Controller.keyCode.VOLPLUS:
					Buttons_Renamed |= PSP_CTRL_VOLUP;
					break;
				case pspsharp.Controller.keyCode.SCREEN:
					Buttons_Renamed |= PSP_CTRL_SCREEN;
					break;
				case pspsharp.Controller.keyCode.MUSIC:
					Buttons_Renamed |= PSP_CTRL_NOTE;
					break;

				default:
					return;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("keyPressed {0}", key.ToString()));
			}

			lastKey = key;
		}

		public virtual void keyReleased(KeyEvent keyEvent)
		{
			keyReleased(keyEvent.KeyCode);
		}

		public virtual void keyReleased(int keyCode)
		{
			keyCode key = keys[keyCode];
			keyReleased(key);
		}

		public virtual void keyReleased(keyCode key)
		{
			if (key == null)
			{
				return;
			}

			switch (key)
			{
				case pspsharp.Controller.keyCode.DOWN:
					Buttons_Renamed &= ~PSP_CTRL_DOWN;
					break;
				case pspsharp.Controller.keyCode.UP:
					Buttons_Renamed &= ~PSP_CTRL_UP;
					break;
				case pspsharp.Controller.keyCode.LEFT:
					Buttons_Renamed &= ~PSP_CTRL_LEFT;
					break;
				case pspsharp.Controller.keyCode.RIGHT:
					Buttons_Renamed &= ~PSP_CTRL_RIGHT;
					break;
				case pspsharp.Controller.keyCode.LANDOWN:
					Ly_Renamed = analogCenter;
					break;
				case pspsharp.Controller.keyCode.LANUP:
					Ly_Renamed = analogCenter;
					break;
				case pspsharp.Controller.keyCode.LANLEFT:
					Lx_Renamed = analogCenter;
					break;
				case pspsharp.Controller.keyCode.LANRIGHT:
					Lx_Renamed = analogCenter;
					break;
				case pspsharp.Controller.keyCode.RANDOWN:
					Ry_Renamed = analogCenter;
					break;
				case pspsharp.Controller.keyCode.RANUP:
					Ry_Renamed = analogCenter;
					break;
				case pspsharp.Controller.keyCode.RANLEFT:
					Rx_Renamed = analogCenter;
					break;
				case pspsharp.Controller.keyCode.RANRIGHT:
					Rx_Renamed = analogCenter;
					break;

				case pspsharp.Controller.keyCode.TRIANGLE:
					Buttons_Renamed &= ~PSP_CTRL_TRIANGLE;
					break;
				case pspsharp.Controller.keyCode.SQUARE:
					Buttons_Renamed &= ~PSP_CTRL_SQUARE;
					break;
				case pspsharp.Controller.keyCode.CIRCLE:
					Buttons_Renamed &= ~PSP_CTRL_CIRCLE;
					break;
				case pspsharp.Controller.keyCode.CROSS:
					Buttons_Renamed &= ~PSP_CTRL_CROSS;
					break;
				case pspsharp.Controller.keyCode.L1:
					Buttons_Renamed &= ~PSP_CTRL_LTRIGGER;
					break;
				case pspsharp.Controller.keyCode.R1:
					Buttons_Renamed &= ~PSP_CTRL_RTRIGGER;
					break;
				case pspsharp.Controller.keyCode.START:
					Buttons_Renamed &= ~PSP_CTRL_START;
					break;
				case pspsharp.Controller.keyCode.SELECT:
					Buttons_Renamed &= ~PSP_CTRL_SELECT;
					break;

				case pspsharp.Controller.keyCode.HOME:
					Buttons_Renamed &= ~PSP_CTRL_HOME;
					break;
				case pspsharp.Controller.keyCode.HOLD:
					Buttons_Renamed &= ~PSP_CTRL_HOLD;
					break;
				case pspsharp.Controller.keyCode.VOLMIN:
					Buttons_Renamed &= ~PSP_CTRL_VOLDOWN;
					break;
				case pspsharp.Controller.keyCode.VOLPLUS:
					Buttons_Renamed &= ~PSP_CTRL_VOLUP;
					break;
				case pspsharp.Controller.keyCode.SCREEN:
					Buttons_Renamed &= ~PSP_CTRL_SCREEN;
					break;
				case pspsharp.Controller.keyCode.MUSIC:
					Buttons_Renamed &= ~PSP_CTRL_NOTE;
					break;

				default:
					return;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("keyReleased {0}", key.ToString()));
			}

			lastKey = Controller.keyCode.RELEASED;
		}

		private void processSpecialKeys()
		{
			if (isSpecialKeyPressed(keyCode.VOLMIN))
			{
				Audio.setVolumeDown();
			}
			else if (isSpecialKeyPressed(keyCode.VOLPLUS))
			{
				Audio.setVolumeUp();
			}
			else if (isSpecialKeyPressed(keyCode.HOME))
			{
				Buttons_Renamed &= ~PSP_CTRL_HOME; // Release the HOME button to avoid dialog spamming.
				int opt = JOptionPane.showOptionDialog(null, "Exit the current application?", "HOME", JOptionPane.YES_NO_OPTION, JOptionPane.INFORMATION_MESSAGE, null, null, null);
				if (opt == DialogResult.Yes)
				{
					Modules.LoadExecForUserModule.triggerExitCallback();
				}
			}
		}

		// Check if a certain special key is pressed.
		private bool isSpecialKeyPressed(keyCode key)
		{
			bool res = false;
			switch (key)
			{
				case pspsharp.Controller.keyCode.HOME:
					if ((Buttons_Renamed & PSP_CTRL_HOME) == PSP_CTRL_HOME)
					{
						res = true;
					}
					break;
				case pspsharp.Controller.keyCode.HOLD:
					if ((Buttons_Renamed & PSP_CTRL_HOLD) == PSP_CTRL_HOLD)
					{
						res = true;
					}
					break;
				case pspsharp.Controller.keyCode.VOLMIN:
					if ((Buttons_Renamed & PSP_CTRL_VOLDOWN) == PSP_CTRL_VOLDOWN)
					{
						res = true;
					}
					break;
				case pspsharp.Controller.keyCode.VOLPLUS:
					if ((Buttons_Renamed & PSP_CTRL_VOLUP) == PSP_CTRL_VOLUP)
					{
						res = true;
					}
					break;
				case pspsharp.Controller.keyCode.SCREEN:
					if ((Buttons_Renamed & PSP_CTRL_SCREEN) == PSP_CTRL_SCREEN)
					{
						res = true;
					}
					break;
				case pspsharp.Controller.keyCode.MUSIC:
					if ((Buttons_Renamed & PSP_CTRL_NOTE) == PSP_CTRL_NOTE)
					{
						res = true;
					}
					break;
				default:
					break;
			}
			return res;
		}

		private Component getControllerComponentByName(string name)
		{
			Component[] components = inputController.Components;
			if (components != null)
			{
				// First search for the identifier name
				for (int i = 0; i < components.Length; i++)
				{
					if (name.Equals(components[i].Identifier.Name))
					{
						return components[i];
					}
				}

				// Second search for the component name
				for (int i = 0; i < components.Length; i++)
				{
					if (name.Equals(components[i].Name))
					{
						return components[i];
					}
				}
			}

			return null;
		}

		public static float getDeadZone(Component component)
		{
			float deadZone = component.DeadZone;
			if (deadZone < minimumDeadZone)
			{
				deadZone = minimumDeadZone;
			}

			return deadZone;
		}

		public static bool isInDeadZone(Component component, float value)
		{
			return System.Math.Abs(value) <= getDeadZone(component);
		}

		/// <summary>
		/// Convert a float value from the range [-1..1] to an analog byte value in
		/// the range [0..255]. -1 is converted to 0 0 is converted to 128 1 is
		/// converted to 255
		/// </summary>
		/// <param name="value"> value in the range [-1..1] </param>
		/// <returns> the corresponding byte value in the range [0..255]. </returns>
		private sbyte convertAnalogValue(float value)
		{
			return (sbyte)((value + 1f) * 127.5f);
		}

		private void processControllerEvent(Component component, float value)
		{
			Component.Identifier id = component.Identifier;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Controller Event on {0}({1}): {2:F}", component.Name, id.Name, value));
			}

			int? button = buttonComponents[id];
			if (button != null)
			{
				if (id is Component.Identifier.Axis)
				{
					// An Axis has been mapped to a PSP button.
					// E.g. for a foot pedal:
					//        value == 1.f when the pedal is not pressed
					//        value == 0.f when the pedal is halfway pressed
					//        value == -1.f when the pedal is pressed down
					if (!isInDeadZone(component, value))
					{
						if (value >= 0.0f)
						{
							// Axis is pressed less than halfway, assume the PSP button is not pressed
							Buttons_Renamed &= ~button;
						}
						else
						{
							// Axis is pressed more than halfway, assume the PSP button is pressed
							Buttons_Renamed |= button;
						}
					}
				}
				else
				{
					if (value == 0.0f)
					{
						Buttons_Renamed &= ~button;
					}
					else if (value == 1.0f)
					{
						Buttons_Renamed |= button;
					}
					else
					{
						Console.WriteLine(string.Format("Unknown Controller Button Event on {0}({1}): {2:F}", component.Name, id.Name, value));
					}
				}
			}
			else if (id == analogLXAxis)
			{
				if (isInDeadZone(component, value))
				{
					Lx_Renamed = analogCenter;
				}
				else
				{
					Lx_Renamed = convertAnalogValue(value);
				}
			}
			else if (id == analogLYAxis)
			{
				if (isInDeadZone(component, value))
				{
					Ly_Renamed = analogCenter;
				}
				else
				{
					Ly_Renamed = convertAnalogValue(value);
				}
			}
			else if (id == analogRXAxis)
			{
				if (isInDeadZone(component, value))
				{
					Rx_Renamed = analogCenter;
				}
				else
				{
					Rx_Renamed = convertAnalogValue(value);
				}
			}
			else if (id == analogRYAxis)
			{
				if (isInDeadZone(component, value))
				{
					Ry_Renamed = analogCenter;
				}
				else
				{
					Ry_Renamed = convertAnalogValue(value);
				}
			}
			else if (id == digitalXAxis)
			{
				if (isInDeadZone(component, value))
				{
					Buttons_Renamed &= ~(PSP_CTRL_LEFT | PSP_CTRL_RIGHT);
				}
				else if (value < 0.0f)
				{
					Buttons_Renamed |= PSP_CTRL_LEFT;
				}
				else
				{
					Buttons_Renamed |= PSP_CTRL_RIGHT;
				}
			}
			else if (id == digitalYAxis)
			{
				if (isInDeadZone(component, value))
				{
					Buttons_Renamed &= ~(PSP_CTRL_DOWN | PSP_CTRL_UP);
				}
				else if (value < 0.0f)
				{
					Buttons_Renamed |= PSP_CTRL_UP;
				}
				else
				{
					Buttons_Renamed |= PSP_CTRL_DOWN;
				}
			}
			else if (id == povArrows)
			{
				if (value == Component.POV.CENTER)
				{
					Buttons_Renamed &= ~(PSP_CTRL_RIGHT | PSP_CTRL_LEFT | PSP_CTRL_DOWN | PSP_CTRL_UP);
				}
				else if (value == Component.POV.UP)
				{
					Buttons_Renamed &= ~(PSP_CTRL_RIGHT | PSP_CTRL_LEFT | PSP_CTRL_DOWN | PSP_CTRL_UP);
					Buttons_Renamed |= PSP_CTRL_UP;
				}
				else if (value == Component.POV.RIGHT)
				{
					Buttons_Renamed &= ~(PSP_CTRL_RIGHT | PSP_CTRL_LEFT | PSP_CTRL_DOWN | PSP_CTRL_UP);
					Buttons_Renamed |= PSP_CTRL_RIGHT;
				}
				else if (value == Component.POV.DOWN)
				{
					Buttons_Renamed &= ~(PSP_CTRL_RIGHT | PSP_CTRL_LEFT | PSP_CTRL_DOWN | PSP_CTRL_UP);
					Buttons_Renamed |= PSP_CTRL_DOWN;
				}
				else if (value == Component.POV.LEFT)
				{
					Buttons_Renamed &= ~(PSP_CTRL_RIGHT | PSP_CTRL_LEFT | PSP_CTRL_DOWN | PSP_CTRL_UP);
					Buttons_Renamed |= PSP_CTRL_LEFT;
				}
				else if (value == Component.POV.DOWN_LEFT)
				{
					Buttons_Renamed &= ~(PSP_CTRL_RIGHT | PSP_CTRL_LEFT | PSP_CTRL_DOWN | PSP_CTRL_UP);
					Buttons_Renamed |= PSP_CTRL_DOWN | PSP_CTRL_LEFT;
				}
				else if (value == Component.POV.DOWN_RIGHT)
				{
					Buttons_Renamed &= ~(PSP_CTRL_RIGHT | PSP_CTRL_LEFT | PSP_CTRL_DOWN | PSP_CTRL_UP);
					Buttons_Renamed |= PSP_CTRL_DOWN | PSP_CTRL_RIGHT;
				}
				else if (value == Component.POV.UP_LEFT)
				{
					Buttons_Renamed &= ~(PSP_CTRL_RIGHT | PSP_CTRL_LEFT | PSP_CTRL_DOWN | PSP_CTRL_UP);
					Buttons_Renamed |= PSP_CTRL_UP | PSP_CTRL_LEFT;
				}
				else if (value == Component.POV.UP_RIGHT)
				{
					Buttons_Renamed &= ~(PSP_CTRL_RIGHT | PSP_CTRL_LEFT | PSP_CTRL_DOWN | PSP_CTRL_UP);
					Buttons_Renamed |= PSP_CTRL_UP | PSP_CTRL_RIGHT;
				}
				else
				{
					Console.WriteLine(string.Format("Unknown Controller Arrows Event on {0}({1}): {2:F}", component.Name, id.Name, value));
				}
			}
			else
			{
				// Unknown Axis components are allowed to move inside their dead zone
				// (e.g. due to small vibrations)
				if (id is Component.Identifier.Axis && (isInDeadZone(component, value) || id == Component.Identifier.Axis.Z || id == Component.Identifier.Axis.RZ))
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Unknown Controller Event in DeadZone on {0}({1}): {2:F}", component.Name, id.Name, value));
					}
				}
				else if (isKeyboardController(inputController))
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Unknown Keyboard Controller Event on {0}({1}): {2:F}", component.Name, id.Name, value));
					}
				}
				else
				{
					if (log.InfoEnabled)
					{
						Console.WriteLine(string.Format("Unknown Controller Event on {0}({1}): {2:F}", component.Name, id.Name, value));
					}
				}
			}
		}

		public virtual sbyte Lx
		{
			get
			{
				return Lx_Renamed;
			}
		}

		public virtual sbyte Ly
		{
			get
			{
				return Ly_Renamed;
			}
		}

		public virtual sbyte Rx
		{
			get
			{
				return Rx_Renamed;
			}
		}

		public virtual sbyte Ry
		{
			get
			{
				return Ry_Renamed;
			}
		}

		public virtual int Buttons
		{
			get
			{
				return Buttons_Renamed;
			}
		}

		public virtual bool hasRightAnalogController()
		{
			return hasRightAnalogController_Renamed;
		}

		public virtual bool HasRightAnalogController
		{
			set
			{
				if (this.hasRightAnalogController_Renamed != value)
				{
					this.hasRightAnalogController_Renamed = value;
					init();
				}
			}
		}
	}

}