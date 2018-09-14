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
namespace pspsharp.HLE.modules
{
	using Logger = org.apache.log4j.Logger;

	using CpuState = pspsharp.Allegrex.CpuState;

	public class sceFpu : HLEModule
	{
		public static Logger log = Modules.getLogger("sceFpu");

		private int getCfc1_31(CpuState cpu)
		{
			return (cpu.fcr31.fs ? (1 << 24) : 0) | (cpu.fcr31.c ? (1 << 23) : 0) | (cpu.fcr31.rm & 3);
		}

		private void setCtc1_31(CpuState cpu, int ctc1Bits)
		{
			cpu.fcr31.rm = ctc1Bits & 3;
			cpu.fcr31.fs = ((ctc1Bits >> 24) & 1) != 0;
			cpu.fcr31.c = ((ctc1Bits >> 23) & 1) != 0;
		}

		[HLEFunction(nid : 0x3AF6984A)]
		public virtual int sceFpu_3AF6984A(float value)
		{
			return System.Math.Round(value);
		}

		[HLEFunction(nid : 0x6CF7A73F)]
		public virtual int sceFpu_6CF7A73F(CpuState cpu)
		{
			return getCfc1_31(cpu);
		}

		[HLEFunction(nid : 0xB9EEFCEA)]
		public virtual int sceFpu_B9EEFCEA(CpuState cpu, int ctc1Bits)
		{
			int result = getCfc1_31(cpu);
			setCtc1_31(cpu, ctc1Bits);

			return result;
		}
	}

}