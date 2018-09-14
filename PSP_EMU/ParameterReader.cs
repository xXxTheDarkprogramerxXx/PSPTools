using System;

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
namespace pspsharp
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._a0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._f0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._f12;

	using CpuState = pspsharp.Allegrex.CpuState;

	public class ParameterReader
	{
		private CpuState cpu;
		private readonly Memory memory;
		private int parameterIndex = 0;
		private int parameterIndexFloat = 0;
		protected internal const int maxParameterInGprRegisters = 8;
		protected internal const int maxParameterInFprRegisters = 8;
		protected internal static readonly int firstParameterInGpr = _a0;
		protected internal static readonly int firstParameterInFpr = _f12;

		public ParameterReader(CpuState cpu, Memory memory)
		{
			this.cpu = cpu;
			this.memory = memory;
		}

		public virtual CpuState Cpu
		{
			set
			{
				this.cpu = value;
			}
		}

		public virtual void resetReading()
		{
			parameterIndex = 0;
			parameterIndexFloat = 0;
		}

		private int getParameterIntAt(int index)
		{
			if (index >= maxParameterInGprRegisters)
			{
				return memory.read32(cpu._sp + (index - maxParameterInGprRegisters) * 4);
			}
			return cpu.getRegister(firstParameterInGpr + index);
		}

		private float getParameterFloatAt(int index)
		{
			if (index >= maxParameterInFprRegisters)
			{
				throw (new System.NotSupportedException());
			}
			return cpu.fpr[firstParameterInFpr + index];
		}

		private long getParameterLongAt(int index)
		{
			if ((index % 2) != 0)
			{
				throw (new Exception("Parameter misalignment"));
			}
			return (long)getParameterIntAt(index) + (long)getParameterIntAt(index + 1) << 32;
		}

		protected internal virtual int moveParameterIndex(int size)
		{
			while (size > 0 && (parameterIndex % size) != 0)
			{
				parameterIndex++;
			}
			int retParameterIndex = parameterIndex;
			parameterIndex += size;
			return retParameterIndex;
		}

		protected internal virtual int moveParameterIndexFloat(int size)
		{
			while ((parameterIndexFloat % size) != 0)
			{
				parameterIndexFloat++;
			}
			int retParameterIndexFloat = parameterIndexFloat;
			parameterIndexFloat += size;
			return retParameterIndexFloat;
		}

		public virtual int NextInt
		{
			get
			{
				return getParameterIntAt(moveParameterIndex(1));
			}
		}

		public virtual long NextLong
		{
			get
			{
				return getParameterLongAt(moveParameterIndex(2));
			}
		}

		public virtual float NextFloat
		{
			get
			{
				return getParameterFloatAt(moveParameterIndexFloat(1));
			}
		}

		public virtual int ReturnValueInt
		{
			set
			{
				cpu._v0 = value;
			}
		}

		public virtual float ReturnValueFloat
		{
			set
			{
				// Float value is returned in $f0 register
				cpu.fpr[_f0] = value;
			}
		}

		public virtual long ReturnValueLong
		{
			set
			{
				cpu._v0 = unchecked((int)((value) & 0xFFFFFFFF));
				cpu._v1 = unchecked((int)((value >> 32) & 0xFFFFFFFF));
			}
		}
	}

}