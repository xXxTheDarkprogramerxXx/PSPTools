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

	public class VSMXNativeFunction : VSMXFunction
	{
		private INativeFunction nativeFunction;
		private VSMXBaseObject returnValue;
		private VSMXBaseObject[] arguments;

		public VSMXNativeFunction(VSMXInterpreter interpreter, INativeFunction nativeFunction) : base(interpreter, nativeFunction.Args, 0, -1)
		{
			this.nativeFunction = nativeFunction;
			arguments = new VSMXBaseObject[nativeFunction.Args + 1];
		}

		public override void call(VSMXCallState callState)
		{
			arguments[0] = callState.ThisObject;
			for (int i = 1; i < arguments.Length; i++)
			{
				arguments[i] = callState.getLocalVar(i);
			}
			returnValue = nativeFunction.call(arguments);
		}

		public override VSMXBaseObject ReturnValue
		{
			get
			{
				return returnValue;
			}
		}

		public override string ToString()
		{
			return string.Format("VSMXNativeFunction[{0}, returnValue={1}]", nativeFunction, returnValue);
		}
	}

}