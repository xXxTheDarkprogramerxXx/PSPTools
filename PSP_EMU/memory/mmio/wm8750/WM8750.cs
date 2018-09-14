using System;
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
namespace pspsharp.memory.mmio.wm8750
{

	using Logger = org.apache.log4j.Logger;

	using IState = pspsharp.state.IState;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	/// <summary>
	/// The  WM8750L  is  a  low  power,  high  quality  stereo  CODEC
	/// designed for portable digital audio applications.
	/// 
	/// See documentation at
	///    http://hitmen.c02.at/files/docs/psp/WM8750.pdf
	/// </summary>
	public class WM8750 : IState
	{
		public static Logger log = Logger.getLogger("WM8750");
		private const int STATE_VERSION = 0;
		private static WM8750 instance;
		private const int NUMBER_REGISTERS = 43;
		private readonly int[] registers = new int[NUMBER_REGISTERS];
		private static readonly int[] defaultRegisterValues = new int[] {0x097, 0x097, 0x079, 0x079, 0x000, 0x008, 0x000, 0x00A, 0x000, 0x000, 0x0FF, 0x0FF, 0x00F, 0x00F, 0x000, 0x000, 0x000, 0x07B, 0x000, 0x032, 0x000, 0x0C3, 0x0C3, 0x0C0, 0x000, 0x000, 0x000, 0x000, 0x000, 0x000, 0x000, 0x000, 0x000, 0x000, 0x050, 0x050, 0x050, 0x050, 0x050, 0x050, 0x079, 0x079, 0x079};
		public const int REGISTER_RESET = 15;

		public static WM8750 Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new WM8750();
				}
				return instance;
			}
		}

		private WM8750()
		{
			resetToDefaultValues();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			stream.readInts(registers);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInts(registers);
		}

		private void resetToDefaultValues()
		{
			Array.Copy(defaultRegisterValues, 0, registers, 0, NUMBER_REGISTERS);
		}

		public virtual void executeTransmitReceiveCommand(int[] transmitData, int[] receiveData)
		{
			// This seems to not be used for WM8750
			log.error(string.Format("Unimplemented executeTransmitReceiveCommand transmitData: 0x{0:X2} 0x{1:X2}", transmitData[0], transmitData[1]));
		}

		public virtual void executeTransmitCommand(int[] transmitData)
		{
			int register = (transmitData[0] >> 1) & 0x7F;
			int value = transmitData[1] | ((transmitData[0] & 0x01) << 8);

			if (register >= 0 && register < NUMBER_REGISTERS)
			{
				setRegisterValue(register, value);
			}
			else
			{
				log.error(string.Format("executeTransmitCommand unknown register {0:D}", register));
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("executeTransmitCommand register={0:D}, value=0x{1:X3} on {2}", register, value, this));
			}
		}

		private void setRegisterValue(int register, int value)
		{
			if (register == REGISTER_RESET)
			{
				// Writing to this register resets all registers to their default state
				resetToDefaultValues();
			}
			else
			{
				registers[register] = value & 0x1FF;
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("WM8750[");

			bool first = true;
			for (int i = 0; i < NUMBER_REGISTERS; i++)
			{
				if (registers[i] != defaultRegisterValues[i])
				{
					if (first)
					{
						first = false;
					}
					else
					{
						sb.Append(", ");
					}
					sb.Append(string.Format("R{0:D}=0x{1:X3}", i, registers[i]));
				}
			}
			sb.Append("]");

			return sb.ToString();
		}
	}

}