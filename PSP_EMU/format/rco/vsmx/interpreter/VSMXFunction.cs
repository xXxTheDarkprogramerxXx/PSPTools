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
	public class VSMXFunction : VSMXBaseObject
	{
		private int args;
		private int localVars;
		private int startLine;
		private VSMXObject prototype;

		public VSMXFunction(VSMXInterpreter interpreter, int args, int localVars, int startLine) : base(interpreter)
		{
			this.args = args;
			this.localVars = localVars;
			this.startLine = startLine;
			prototype = new VSMXObject(interpreter, "Function");
		}

		public virtual int Args
		{
			get
			{
				return args;
			}
		}

		public virtual int LocalVars
		{
			get
			{
				return localVars;
			}
		}

		public virtual int StartLine
		{
			get
			{
				return startLine;
			}
		}

		public virtual void call(VSMXCallState callState)
		{
		}

		public virtual VSMXBaseObject ReturnValue
		{
			get
			{
				return null;
			}
		}

		public override string typeOf()
		{
			return "function";
		}

		public override string ClassName
		{
			get
			{
				return "Function";
			}
		}

		protected internal override VSMXObject Prototype
		{
			get
			{
				return prototype;
			}
		}

		public override string ToString()
		{
			return string.Format("Function(args={0:D}, localVars={1:D}, startLine={2:D})", args, localVars, startLine);
		}
	}

}