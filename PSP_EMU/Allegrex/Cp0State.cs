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
namespace pspsharp.Allegrex
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.COP0_STATE_CAUSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.COP0_STATE_CONFIG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.COP0_STATE_CPUID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.COP0_STATE_EBASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.COP0_STATE_EPC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.COP0_STATE_ERROR_EPC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.COP0_STATE_SCCODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.COP0_STATE_STATUS;

	using MEProcessor = pspsharp.mediaengine.MEProcessor;
	using IState = pspsharp.state.IState;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	/// <summary>
	/// System Control Coprocessor 0
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class Cp0State : IState
	{
		private const int STATE_VERSION = 0;
		private readonly int[] data = new int[32];
		private readonly int[] control = new int[32];

		public Cp0State()
		{
			const int dataCacheSize = 16 * 1024; // 16KB
			const int instructionCacheSize = 16 * 1024; // 16KB

			int config = 0;
			// 3 bits to indicate the data cache size
			config |= System.Math.Min(Integer.numberOfTrailingZeros(dataCacheSize) - 12, 7) << 6;
			// 3 bits to indicate the instruction cache size
			config |= System.Math.Min(Integer.numberOfTrailingZeros(instructionCacheSize) - 12, 7) << 9;
			Config = config;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			stream.readInts(data);
			stream.readInts(control);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInts(data);
			stream.writeInts(control);
		}

		public virtual int getDataRegister(int n)
		{
			return data[n];
		}

		public virtual void setDataRegister(int n, int value)
		{
			data[n] = value;
		}

		public virtual int getControlRegister(int n)
		{
			return control[n];
		}

		public virtual void setControlRegister(int n, int value)
		{
			control[n] = value;
		}

		public virtual int Epc
		{
			get
			{
				return getDataRegister(COP0_STATE_EPC);
			}
			set
			{
				setDataRegister(COP0_STATE_EPC, value);
			}
		}


		public virtual int ErrorEpc
		{
			get
			{
				return getDataRegister(COP0_STATE_ERROR_EPC);
			}
			set
			{
				setDataRegister(COP0_STATE_ERROR_EPC, value);
			}
		}


		public virtual int Status
		{
			get
			{
				return getDataRegister(COP0_STATE_STATUS);
			}
			set
			{
				setDataRegister(COP0_STATE_STATUS, value);
			}
		}


		public virtual int Cause
		{
			get
			{
				return getDataRegister(COP0_STATE_CAUSE);
			}
			set
			{
				setDataRegister(COP0_STATE_CAUSE, value);
			}
		}


		public virtual int Ebase
		{
			get
			{
				return getDataRegister(COP0_STATE_EBASE);
			}
			set
			{
				setDataRegister(COP0_STATE_EBASE, value);
			}
		}


		public virtual int SyscallCode
		{
			set
			{
				setDataRegister(COP0_STATE_SCCODE, value);
			}
		}

		public virtual int Config
		{
			set
			{
				setDataRegister(COP0_STATE_CONFIG, value);
			}
		}

		public virtual int Cpuid
		{
			set
			{
				setDataRegister(COP0_STATE_CPUID, value);
			}
			get
			{
				return getDataRegister(COP0_STATE_CPUID);
			}
		}


		public virtual bool MediaEngineCpu
		{
			get
			{
				return Cpuid == MEProcessor.CPUID_ME;
			}
		}

		public virtual bool MainCpu
		{
			get
			{
				return !MediaEngineCpu;
			}
		}
	}

}