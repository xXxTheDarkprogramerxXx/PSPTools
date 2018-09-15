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

	//using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerBase : IMMIOHandler
	{
		protected internal Logger log = Logger.getLogger("mmio");
		private const int STATE_VERSION = 0;
		protected internal readonly int baseAddress;

		public MMIOHandlerBase(int baseAddress)
		{
			this.baseAddress = baseAddress;
		}

		protected internal virtual Memory Memory
		{
			get
			{
				return RuntimeContextLLE.MMIO;
			}
		}

		protected internal virtual Processor Processor
		{
			get
			{
				return RuntimeContextLLE.Processor;
			}
		}

		protected internal virtual int Pc
		{
			get
			{
				return Processor.cpu.pc;
			}
		}

		public virtual int read8(int address)
		{
			Console.WriteLine(string.Format("0x{0:X8} - Unimplemented read8(0x{1:X8})", Pc, address));
			return 0;
		}

		public virtual int read16(int address)
		{
			Console.WriteLine(string.Format("0x{0:X8} - Unimplemented read16(0x{1:X8})", Pc, address));
			return 0;
		}

		public virtual int read32(int address)
		{
			Console.WriteLine(string.Format("0x{0:X8} - Unimplemented read32(0x{1:X8})", Pc, address));
			return 0;
		}

		public virtual void write8(int address, sbyte value)
		{
			Console.WriteLine(string.Format("0x{0:X8} - Unimplemented write8(0x{1:X8}, 0x{2:X2})", Pc, address, value));
		}

		public virtual void write16(int address, short value)
		{
			Console.WriteLine(string.Format("0x{0:X8} - Unimplemented write16(0x{1:X8}, 0x{2:X4})", Pc, address, value));
		}

		public virtual void write32(int address, int value)
		{
			Console.WriteLine(string.Format("0x{0:X8} - Unimplemented write32(0x{1:X8}, 0x{2:X8})", Pc, address, value));
		}

		public virtual Logger Logger
		{
			set
			{
				this.log = value;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
		}

		public override string ToString()
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			return string.Format("{0} at 0x{1:X8}", this.GetType().FullName, baseAddress);
		}
	}

}