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
namespace pspsharp.memory.mmio
{

	using Logger = org.apache.log4j.Logger;

	using sceNand = pspsharp.HLE.modules.sceNand;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerNandPage : MMIOHandlerBase
	{
		public static new Logger log = MMIOHandlerNand.log;
		private const int STATE_VERSION = 0;
		public const int BASE_ADDRESS1 = unchecked((int)0xBFF00000);
		public const int BASE_ADDRESS2 = unchecked((int)0x9FF00000);
		private static MMIOHandlerNandPage instance;
		private readonly int[] data = new int[sceNand.pageSize >> 2];
		private readonly int[] ecc = new int[4];

		public static MMIOHandlerNandPage Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MMIOHandlerNandPage(BASE_ADDRESS1);
				}
				return instance;
			}
		}

		private MMIOHandlerNandPage(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			stream.readInts(data);
			stream.readInts(ecc);
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInts(data);
			stream.writeInts(ecc);
			base.write(stream);
		}

		public virtual int[] Data
		{
			get
			{
				return data;
			}
		}

		public virtual int[] Ecc
		{
			get
			{
				return ecc;
			}
		}

		public override int read32(int address)
		{
			int value;
			int localAddress = (address - baseAddress) & 0xFFFFF;
			switch (localAddress)
			{
				case 0x800:
					value = ecc[0];
					break;
				case 0x900:
					value = ecc[1];
					break;
				case 0x904:
					value = ecc[2];
					break;
				case 0x908:
					value = ecc[3];
					break;
				default:
					if (localAddress >= 0 && localAddress < 0x200)
					{
						value = data[localAddress >> 2];
					}
					else
					{
						value = base.read32(address);
					}
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read32(0x{1:X8}) returning 0x{2:X8}", Pc, address, value));
			}

			return value;
		}

		public override void write32(int address, int value)
		{
			int localAddress = (address - baseAddress) & 0xFFFFF;
			switch (localAddress)
			{
				case 0x800:
					ecc[0] = value;
					break;
				case 0x900:
					ecc[1] = value;
					break;
				case 0x904:
					ecc[2] = value;
					break;
				case 0x908:
					ecc[3] = value;
					break;
				default:
					if (localAddress >= 0 && localAddress < 0x200)
					{
						data[localAddress >> 2] = value;
					}
					else
					{
						base.write32(address, value);
					}
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write32(0x{1:X8}, 0x{2:X8}) on {3}", Pc, address, value, this));
			}
		}
	}

}