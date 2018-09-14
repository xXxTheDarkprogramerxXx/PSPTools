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
	public class VSMXLocalVarReference : VSMXReference
	{
		private VSMXCallState callState;
		private int index;

		public VSMXLocalVarReference(VSMXInterpreter interpreter, VSMXCallState callState, int index) : base(interpreter, null, null)
		{
			this.callState = callState;
			this.index = index;
		}

		protected internal override VSMXBaseObject Ref
		{
			get
			{
				return callState.getLocalVar(index);
			}
		}

		public override void assign(VSMXBaseObject value)
		{
			callState.setLocalVar(index, value);
		}

		public override string ToString()
		{
			return string.Format("LocalVar#{0:D}", index);
		}
	}

}