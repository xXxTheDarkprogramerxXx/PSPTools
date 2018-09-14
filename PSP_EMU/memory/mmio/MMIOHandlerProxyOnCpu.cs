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

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerProxyOnCpu : IMMIOHandler
	{
		private IMMIOHandler proxyOnMain;
		private IMMIOHandler proxyOnMe;

		public MMIOHandlerProxyOnCpu(IMMIOHandler proxyOnMain, IMMIOHandler proxyOnMe)
		{
			this.proxyOnMain = proxyOnMain;
			this.proxyOnMe = proxyOnMe;
		}

		public virtual IMMIOHandler Instance
		{
			get
			{
				if (RuntimeContextLLE.MediaEngineCpu)
				{
					return proxyOnMe;
				}
				return proxyOnMain;
			}
		}

		public virtual IMMIOHandler getInstance(Processor processor)
		{
			if (processor.cp0.MediaEngineCpu)
			{
				return proxyOnMe;
			}
			return proxyOnMain;
		}

		public virtual int read8(int address)
		{
			return Instance.read8(address);
		}

		public virtual int read16(int address)
		{
			return Instance.read16(address);
		}

		public virtual int read32(int address)
		{
			return Instance.read32(address);
		}

		public virtual void write8(int address, sbyte value)
		{
			Instance.write8(address, value);
		}

		public virtual void write16(int address, short value)
		{
			Instance.write16(address, value);
		}

		public virtual void write32(int address, int value)
		{
			Instance.write32(address, value);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			proxyOnMain.read(stream);
			proxyOnMe.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			proxyOnMain.write(stream);
			proxyOnMe.write(stream);
		}
	}

}