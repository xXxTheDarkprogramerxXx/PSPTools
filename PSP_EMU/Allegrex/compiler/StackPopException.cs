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
namespace pspsharp.Allegrex.compiler
{
	public class StackPopException : Exception
	{
		private const long serialVersionUID = -5573324070282200237L;
		private int ra;

		public StackPopException(int ra)
		{
			this.ra = ra;
		}

		public virtual int Ra
		{
			get
			{
				return ra;
			}
		}

		public override string ToString()
		{
			return string.Format("StackPopException(0x{0:X8})", ra);
		}
	}

}