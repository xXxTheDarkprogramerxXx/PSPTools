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
namespace pspsharp.HLE.kernel.types.interrupts
{
	using CpuState = pspsharp.Allegrex.CpuState;

	public class AbstractAllegrexInterruptHandler
	{
		private int address;
		private int[] arguments = new int[4];
		private int numberArguments;

		public AbstractAllegrexInterruptHandler(int address)
		{
			this.address = address;
			numberArguments = 0;
		}

		public AbstractAllegrexInterruptHandler(int address, int argument0)
		{
			this.address = address;
			arguments[0] = argument0;
			numberArguments = 1;
		}

		public AbstractAllegrexInterruptHandler(int address, int argument0, int argument1)
		{
			this.address = address;
			arguments[0] = argument0;
			arguments[1] = argument1;
			numberArguments = 2;
		}

		public AbstractAllegrexInterruptHandler(int address, int argument0, int argument1, int argument2)
		{
			this.address = address;
			arguments[0] = argument0;
			arguments[1] = argument1;
			arguments[2] = argument2;
			numberArguments = 3;
		}

		public AbstractAllegrexInterruptHandler(int address, int argument0, int argument1, int argument2, int argument3)
		{
			this.address = address;
			arguments[0] = argument0;
			arguments[1] = argument1;
			arguments[2] = argument2;
			arguments[3] = argument3;
			numberArguments = 4;
		}

		public virtual int Address
		{
			get
			{
				return address;
			}
			set
			{
				this.address = value;
			}
		}


		public virtual int getArgument(int index)
		{
			return arguments[index];
		}

		public virtual void setArgument(int index, int argument)
		{
			arguments[index] = argument;

			if (index >= numberArguments)
			{
				numberArguments = index + 1;
			}
		}

		public virtual int NumberArguments
		{
			get
			{
				return numberArguments;
			}
		}

		public virtual void copyArgumentsToCpu(CpuState cpu)
		{
			switch (numberArguments)
			{
				case 4:
					cpu._a3 = arguments[3];
					goto case 3;
				case 3:
					cpu._a2 = arguments[2];
					goto case 2;
				case 2:
					cpu._a1 = arguments[1];
					goto case 1;
				case 1:
					cpu._a0 = arguments[0];
				break;
			}
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			result.Append(string.Format("0x{0:X8}(", Address));
			for (int i = 0; i < numberArguments; i++)
			{
				if (i > 0)
				{
					result.Append(",");
				}
				result.Append(string.Format("0x{0:X8}", getArgument(i)));
			}
			result.Append(")");

			return result.ToString();
		}
	}

}