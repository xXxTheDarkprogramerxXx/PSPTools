using System.Collections.Generic;
using System.Text;

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
namespace pspsharp.format.rco.vsmx.interpreter
{

	public class VSMXCallState
	{
		private VSMXBaseObject thisObject;
		private VSMXBaseObject[] localVariables;
		private Stack<VSMXBaseObject> stack;
		private int returnPc;
		private bool returnThis;
		private bool exitAfterCall;

		public VSMXCallState(VSMXBaseObject thisObject, int numberOfLocalVariables, int returnPc, bool returnThis, bool exitAfterCall)
		{
			this.thisObject = thisObject;
			localVariables = new VSMXBaseObject[numberOfLocalVariables];
			Arrays.fill(localVariables, VSMXUndefined.singleton);
			stack = new Stack<VSMXBaseObject>();
			this.returnPc = returnPc;
			this.returnThis = returnThis;
			this.exitAfterCall = exitAfterCall;
		}

		public virtual int ReturnPc
		{
			get
			{
				return returnPc;
			}
		}

		public virtual VSMXBaseObject ThisObject
		{
			get
			{
				return thisObject;
			}
		}

		public virtual VSMXBaseObject getLocalVar(int i)
		{
			if (i <= 0 || i > localVariables.Length)
			{
				return VSMXUndefined.singleton;
			}

			return localVariables[i - 1];
		}

		public virtual void setLocalVar(int i, VSMXBaseObject value)
		{
			if (i > 0 && i <= localVariables.Length)
			{
				localVariables[i - 1] = value;
			}
		}

		public virtual Stack<VSMXBaseObject> Stack
		{
			get
			{
				return stack;
			}
		}

		public virtual bool ReturnThis
		{
			get
			{
				return returnThis;
			}
		}

		public virtual bool ExitAfterCall
		{
			get
			{
				return exitAfterCall;
			}
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();

			s.Append(string.Format("CallState[returnPc={0:D}, this={1}", returnPc, thisObject));
			for (int i = 1; i <= localVariables.Length; i++)
			{
				s.Append(string.Format(", var{0:D}={1}", i, getLocalVar(i)));
			}
			s.Append("]");

			return s.ToString();
		}
	}

}