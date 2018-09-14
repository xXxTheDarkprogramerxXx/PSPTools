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
namespace pspsharp.format.rco.vsmx.objects
{
	using Logger = org.apache.log4j.Logger;

	using UmdVideoPlayer = pspsharp.GUI.UmdVideoPlayer;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using BasePositionObject = pspsharp.format.rco.@object.BasePositionObject;
	using VSMXArray = pspsharp.format.rco.vsmx.interpreter.VSMXArray;
	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXBoolean = pspsharp.format.rco.vsmx.interpreter.VSMXBoolean;
	using VSMXFunction = pspsharp.format.rco.vsmx.interpreter.VSMXFunction;
	using VSMXInterpreter = pspsharp.format.rco.vsmx.interpreter.VSMXInterpreter;
	using VSMXNativeObject = pspsharp.format.rco.vsmx.interpreter.VSMXNativeObject;
	using VSMXNull = pspsharp.format.rco.vsmx.interpreter.VSMXNull;
	using VSMXString = pspsharp.format.rco.vsmx.interpreter.VSMXString;

	public class Controller : BaseNativeObject
	{
		private new static readonly Logger log = VSMX.log;
		public const string objectName = "controller";
		private VSMXBaseObject userData;
		private string resource; // E.g. "EN100000"
		private BasePositionObject focus;
		private VSMXInterpreter interpreter;
		private UmdVideoPlayer umdVideoPlayer;

		private class ChangeResourceAction : IAction
		{
			private readonly Controller outerInstance;

			internal string newResource;

			public ChangeResourceAction(Controller outerInstance, string newResource)
			{
				this.outerInstance = outerInstance;
				this.newResource = newResource;
			}

			public virtual void execute()
			{
				outerInstance.resource = newResource;

				outerInstance.umdVideoPlayer.changeResource(outerInstance.resource);

				// Call the "controller.onChangeResource" callback
				VSMXBaseObject callback = outerInstance.Object.getPropertyValue("onChangeResource");
				if (callback is VSMXFunction)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Executing Controller.onChangeResource '{0}' with function {1}", outerInstance.resource, callback));
					}
					VSMXBaseObject[] arguments = new VSMXBaseObject[1];
					arguments[0] = new VSMXString(outerInstance.interpreter, outerInstance.resource);
					outerInstance.interpreter.delayInterpretFunction((VSMXFunction) callback, null, arguments);
				}
			}
		}

		public static VSMXNativeObject create(VSMXInterpreter interpreter, UmdVideoPlayer umdVideoPlayer, string resource)
		{
			Controller controller = new Controller(interpreter, umdVideoPlayer);
			VSMXNativeObject @object = new VSMXNativeObject(interpreter, controller);
			controller.Object = @object;
			controller.resource = resource;

			// Callbacks
			@object.setPropertyValue("onChangeResource", VSMXNull.singleton);
			@object.setPropertyValue("onMenu", VSMXNull.singleton);
			@object.setPropertyValue("onExit", VSMXNull.singleton);
			@object.setPropertyValue("onAutoPlay", VSMXNull.singleton);
			@object.setPropertyValue("onContinuePlay", VSMXNull.singleton);

			return @object;
		}

		private Controller(VSMXInterpreter interpreter, UmdVideoPlayer umdVideoPlayer)
		{
			this.interpreter = interpreter;
			this.umdVideoPlayer = umdVideoPlayer;
			userData = new VSMXArray(interpreter);
		}

		public virtual VSMXInterpreter Interpreter
		{
			get
			{
				return interpreter;
			}
		}

		public virtual VSMXBaseObject getUserData(VSMXBaseObject @object)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Controller.getUserData() returning {0}", userData));
			}

			return userData;
		}

		public virtual VSMXBaseObject setUserData(VSMXBaseObject @object, VSMXBaseObject userData)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Controller.setUserData({0})", userData));
			}

			this.userData = userData;

			// Returning true/false?
			return VSMXBoolean.singletonTrue;
		}

		public virtual string Resource
		{
			get
			{
				return resource;
			}
		}

		public virtual VSMXBaseObject changeResource(VSMXBaseObject @object, VSMXBaseObject resource)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Controller.changeResource({0})", resource));
			}

			string newResource = resource.StringValue;
			if (!this.resource.Equals(newResource))
			{
				IAction action = new ChangeResourceAction(this, newResource);
				Emulator.Scheduler.addAction(action);
			}

			// Returning true/false?
			return VSMXBoolean.singletonTrue;
		}

		public virtual BasePositionObject Focus
		{
			set
			{
				if (focus != null)
				{
					focus.focusOut();
				}
				focus = value;
				umdVideoPlayer.RCODisplay.Focus = (IDisplay) value;
			}
		}

		public virtual void onUp()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Controller.onUp focus={0}", focus));
			}

			if (focus != null)
			{
				focus.onUp();
			}
		}

		public virtual void onDown()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Controller.onDown focus={0}", focus));
			}

			if (focus != null)
			{
				focus.onDown();
			}
		}

		public virtual void onLeft()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Controller.onLeft focus={0}", focus));
			}

			if (focus != null)
			{
				focus.onLeft();
			}
		}

		public virtual void onRight()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Controller.onRight focus={0}", focus));
			}

			if (focus != null)
			{
				focus.onRight();
			}
		}

		public virtual void onPush()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Controller.onPush focus={0}", focus));
			}

			if (focus != null)
			{
				focus.onPush();
			}
		}
	}

}