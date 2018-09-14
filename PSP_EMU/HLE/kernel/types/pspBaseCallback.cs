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
namespace pspsharp.HLE.kernel.types
{
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;

	public class pspBaseCallback
	{
		public const string callbackUidPurpose = "ThreadMan-callback";
		private readonly int[] arguments;
		private readonly int callbackFunction;
		private readonly int uid;

		public pspBaseCallback(int callbackFunction, int numberArguments)
		{
			this.callbackFunction = callbackFunction;
			arguments = new int[numberArguments];
			uid = SceUidManager.getNewUid(callbackUidPurpose);
		}

		public virtual int getArgument(int n)
		{
			return arguments[n];
		}

		public virtual void setArgument(int n, int value)
		{
			arguments[n] = value;
		}

		public virtual int CallbackFunction
		{
			get
			{
				return callbackFunction;
			}
		}

		public virtual bool hasCallbackFunction()
		{
			return callbackFunction != 0;
		}

		public virtual int Uid
		{
			get
			{
				return uid;
			}
		}

		public virtual void call(SceKernelThreadInfo thread, IAction afterAction)
		{
			Modules.ThreadManForUserModule.executeCallback(thread, callbackFunction, afterAction, true, arguments);
		}
	}

}