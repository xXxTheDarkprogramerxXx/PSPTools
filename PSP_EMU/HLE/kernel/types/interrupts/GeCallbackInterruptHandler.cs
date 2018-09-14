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
namespace pspsharp.HLE.kernel.types.interrupts
{
	public class GeCallbackInterruptHandler : AbstractAllegrexInterruptHandler
	{
		public GeCallbackInterruptHandler(int address, int argument, int listPc) : base(address, 0, argument, listPc)
		{
			// call: PspGeCallback(int id, void *argument, void *listAddr)
			// Callback arguments:
			// - $a0: signal/finish ID (will be set dynamically by the GeInterruptHandler)
			// - $a1: callback argument provided by sceGeSetCallback
			// - $a2: current PC of the list (this argument is not described in the PSPSDK)
		}

		public virtual int Id
		{
			get
			{
				return getArgument(0);
			}
			set
			{
				setArgument(0, value);
			}
		}


		public virtual int Argument
		{
			get
			{
				return getArgument(1);
			}
			set
			{
				setArgument(1, value);
			}
		}

	}

}