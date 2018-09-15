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

	//using Logger = org.apache.log4j.Logger;

	using IState = pspsharp.state.IState;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	/// <summary>
	/// General Purpose Registers, handles integer operations like ALU, shifter, etc.
	/// 
	/// @author hli
	/// </summary>
	public class GprState : IState
	{
		private const int STATE_VERSION = 0;
		protected internal Logger log;
		public const int NUMBER_REGISTERS = 32;
		// Use fields to store the 32 registers, and not an array like
		//    int[] gpr = new int[32]
		// This allows the Java JIT to produce more efficient code by avoiding the array bounds checks on "gpr".
		public readonly int _zr = 0;
		public int _at;
		public int _v0;
		public int _v1;
		public int _a0;
		public int _a1;
		public int _a2;
		public int _a3;
		public int _t0;
		public int _t1;
		public int _t2;
		public int _t3;
		public int _t4;
		public int _t5;
		public int _t6;
		public int _t7;
		public int _s0;
		public int _s1;
		public int _s2;
		public int _s3;
		public int _s4;
		public int _s5;
		public int _s6;
		public int _s7;
		public int _t8;
		public int _t9;
		public int _k0;
		public int _k1;
		public int _gp;
		public int _sp;
		public int _fp;
		public int _ra;

		public virtual Logger Logger
		{
			set
			{
				this.log = value;
			}
		}

		public virtual void reset()
		{
			_at = 0;
			_v0 = 0;
			_v1 = 0;
			_a0 = 0;
			_a1 = 0;
			_a2 = 0;
			_a3 = 0;
			_t0 = 0;
			_t1 = 0;
			_t2 = 0;
			_t3 = 0;
			_t4 = 0;
			_t5 = 0;
			_t6 = 0;
			_t7 = 0;
			_s0 = 0;
			_s1 = 0;
			_s2 = 0;
			_s3 = 0;
			_s4 = 0;
			_s5 = 0;
			_s6 = 0;
			_s7 = 0;
			_t8 = 0;
			_t9 = 0;
			_k0 = 0;
			_k1 = 0;
			_gp = 0;
			_sp = 0;
			_fp = 0;
			_ra = 0;
		}

		public virtual void resetAll()
		{
			_at = 0;
			_v0 = 0;
			_v1 = 0;
			_a0 = 0;
			_a1 = 0;
			_a2 = 0;
			_a3 = 0;
			_t0 = 0;
			_t1 = 0;
			_t2 = 0;
			_t3 = 0;
			_t4 = 0;
			_t5 = 0;
			_t6 = 0;
			_t7 = 0;
			_s0 = 0;
			_s1 = 0;
			_s2 = 0;
			_s3 = 0;
			_s4 = 0;
			_s5 = 0;
			_s6 = 0;
			_s7 = 0;
			_t8 = 0;
			_t9 = 0;
			_k0 = 0;
			_k1 = 0;
			_gp = 0;
			_sp = 0;
			_fp = 0;
			_ra = 0;
		}

		public GprState()
		{
		}

		public virtual void copy(GprState that)
		{
			_at = that._at;
			_v0 = that._v0;
			_v1 = that._v1;
			_a0 = that._a0;
			_a1 = that._a1;
			_a2 = that._a2;
			_a3 = that._a3;
			_t0 = that._t0;
			_t1 = that._t1;
			_t2 = that._t2;
			_t3 = that._t3;
			_t4 = that._t4;
			_t5 = that._t5;
			_t6 = that._t6;
			_t7 = that._t7;
			_s0 = that._s0;
			_s1 = that._s1;
			_s2 = that._s2;
			_s3 = that._s3;
			_s4 = that._s4;
			_s5 = that._s5;
			_s6 = that._s6;
			_s7 = that._s7;
			_t8 = that._t8;
			_t9 = that._t9;
			_k0 = that._k0;
			_k1 = that._k1;
			_gp = that._gp;
			_sp = that._sp;
			_fp = that._fp;
			_ra = that._ra;
		}

		public GprState(GprState that)
		{
			_at = that._at;
			_v0 = that._v0;
			_v1 = that._v1;
			_a0 = that._a0;
			_a1 = that._a1;
			_a2 = that._a2;
			_a3 = that._a3;
			_t0 = that._t0;
			_t1 = that._t1;
			_t2 = that._t2;
			_t3 = that._t3;
			_t4 = that._t4;
			_t5 = that._t5;
			_t6 = that._t6;
			_t7 = that._t7;
			_s0 = that._s0;
			_s1 = that._s1;
			_s2 = that._s2;
			_s3 = that._s3;
			_s4 = that._s4;
			_s5 = that._s5;
			_s6 = that._s6;
			_s7 = that._s7;
			_t8 = that._t8;
			_t9 = that._t9;
			_k0 = that._k0;
			_k1 = that._k1;
			_gp = that._gp;
			_sp = that._sp;
			_fp = that._fp;
			_ra = that._ra;
		}

		public virtual int getRegister(int reg)
		{
			switch (reg)
			{
				case Common._zr:
					return 0;
				case Common._at:
					return _at;
				case Common._v0:
					return _v0;
				case Common._v1:
					return _v1;
				case Common._a0:
					return _a0;
				case Common._a1:
					return _a1;
				case Common._a2:
					return _a2;
				case Common._a3:
					return _a3;
				case Common._t0:
					return _t0;
				case Common._t1:
					return _t1;
				case Common._t2:
					return _t2;
				case Common._t3:
					return _t3;
				case Common._t4:
					return _t4;
				case Common._t5:
					return _t5;
				case Common._t6:
					return _t6;
				case Common._t7:
					return _t7;
				case Common._s0:
					return _s0;
				case Common._s1:
					return _s1;
				case Common._s2:
					return _s2;
				case Common._s3:
					return _s3;
				case Common._s4:
					return _s4;
				case Common._s5:
					return _s5;
				case Common._s6:
					return _s6;
				case Common._s7:
					return _s7;
				case Common._t8:
					return _t8;
				case Common._t9:
					return _t9;
				case Common._k0:
					return _k0;
				case Common._k1:
					return _k1;
				case Common._gp:
					return _gp;
				case Common._sp:
					return _sp;
				case Common._fp:
					return _fp;
				case Common._ra:
					return _ra;
			}

			Emulator.Console.WriteLine(string.Format("Unknown register {0:D}", reg));
			return 0;
		}

		public virtual void setRegister(int reg, int value)
		{
			switch (reg)
			{
				case Common._zr:
					return;
				case Common._at:
					_at = value;
					return;
				case Common._v0:
					_v0 = value;
					return;
				case Common._v1:
					_v1 = value;
					return;
				case Common._a0:
					_a0 = value;
					return;
				case Common._a1:
					_a1 = value;
					return;
				case Common._a2:
					_a2 = value;
					return;
				case Common._a3:
					_a3 = value;
					return;
				case Common._t0:
					_t0 = value;
					return;
				case Common._t1:
					_t1 = value;
					return;
				case Common._t2:
					_t2 = value;
					return;
				case Common._t3:
					_t3 = value;
					return;
				case Common._t4:
					_t4 = value;
					return;
				case Common._t5:
					_t5 = value;
					return;
				case Common._t6:
					_t6 = value;
					return;
				case Common._t7:
					_t7 = value;
					return;
				case Common._s0:
					_s0 = value;
					return;
				case Common._s1:
					_s1 = value;
					return;
				case Common._s2:
					_s2 = value;
					return;
				case Common._s3:
					_s3 = value;
					return;
				case Common._s4:
					_s4 = value;
					return;
				case Common._s5:
					_s5 = value;
					return;
				case Common._s6:
					_s6 = value;
					return;
				case Common._s7:
					_s7 = value;
					return;
				case Common._t8:
					_t8 = value;
					return;
				case Common._t9:
					_t9 = value;
					return;
				case Common._k0:
					_k0 = value;
					return;
				case Common._k1:
					_k1 = value;
					return;
				case Common._gp:
					_gp = value;
					return;
				case Common._sp:
					_sp = value;
					return;
				case Common._fp:
					_fp = value;
					return;
				case Common._ra:
					_ra = value;
					return;
			}

			Emulator.Console.WriteLine(string.Format("Unknown register {0:D}, value=0x{1:X8}", reg, value));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			// Skip register $zr
			for (int i = 1; i < NUMBER_REGISTERS; i++)
			{
				setRegister(i, stream.readInt());
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			// Skip register $zr
			for (int i = 1; i < NUMBER_REGISTERS; i++)
			{
				stream.writeInt(getRegister(i));
			}
		}

		public virtual void doUNK(string reason)
		{
			Emulator.Console.WriteLine("Interpreter : " + reason);
		}

		public static int extractBits(int x, int pos, int len)
		{
			return ((int)((uint)x >> pos)) & ~(~0 << len);
		}

		public static int insertBits(int x, int y, int lsb, int msb)
		{
			int mask = ~(~0 << (msb - lsb + 1)) << lsb;
			return (x & ~mask) | ((y << lsb) & mask);
		}

		public static int signExtend(int value)
		{
			return (value << 16) >> 16;
		}

		public static int signExtend8(int value)
		{
			return (value << 24) >> 24;
		}

		public static int zeroExtend(int value)
		{
			return (value & 0xffff);
		}

		public static int zeroExtend8(int value)
		{
			return (value & 0xff);
		}

		public static int signedCompare(int i, int j)
		{
			return (i < j) ? 1 : 0;
		}

		public static int unsignedCompare(long i, long j)
		{
			return ((i & 0xffffffffL) < (j & 0xffffffffL)) ? 1 : 0;
		}

		// not sure about it
		//public static final int unsignedCompare(int i, int j) {
		//    return (i - j) >>> 31;
		//}

		public void doSLL(int rd, int rt, int sa)
		{
			if (rd != 0)
			{
				setRegister(rd, getRegister(rt) << sa);
			}
		}

		public void doSRL(int rd, int rt, int sa)
		{
			if (rd != 0)
			{
				setRegister(rd, (int)((uint)getRegister(rt) >> sa));
			}
		}

		public void doSRA(int rd, int rt, int sa)
		{
			if (rd != 0)
			{
				setRegister(rd, getRegister(rt) >> sa);
			}
		}

		public void doSLLV(int rd, int rt, int rs)
		{
			if (rd != 0)
			{
				setRegister(rd, getRegister(rt) << (getRegister(rs) & 31));
			}
		}

		public void doSRLV(int rd, int rt, int rs)
		{
			if (rd != 0)
			{
				setRegister(rd, (int)((uint)getRegister(rt) >> (getRegister(rs) & 31)));
			}
		}

		public void doSRAV(int rd, int rt, int rs)
		{
			if (rd != 0)
			{
				setRegister(rd, getRegister(rt) >> (getRegister(rs) & 31));
			}
		}

		public void doADDU(int rd, int rs, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, getRegister(rs) + getRegister(rt));
			}
		}

		public void doSUBU(int rd, int rs, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, getRegister(rs) - getRegister(rt));
			}
		}

		public void doAND(int rd, int rs, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, getRegister(rs) & getRegister(rt));
			}
		}

		public void doOR(int rd, int rs, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, getRegister(rs) | getRegister(rt));
			}
		}

		public void doXOR(int rd, int rs, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, getRegister(rs) ^ getRegister(rt));
			}
		}

		public void doNOR(int rd, int rs, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, ~(getRegister(rs) | getRegister(rt)));
			}
		}

		public void doSLT(int rd, int rs, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, signedCompare(getRegister(rs), getRegister(rt)));
			}
		}

		public void doSLTU(int rd, int rs, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, unsignedCompare(getRegister(rs), getRegister(rt)));
			}
		}

		public void doADDIU(int rt, int rs, int simm16)
		{
			if (rt != 0)
			{
				setRegister(rt, getRegister(rs) + simm16);
			}
		}

		public void doSLTI(int rt, int rs, int simm16)
		{
			if (rt != 0)
			{
				setRegister(rt, signedCompare(getRegister(rs), simm16));
			}
		}

		public void doSLTIU(int rt, int rs, int simm16)
		{
			if (rt != 0)
			{
				setRegister(rt, unsignedCompare(getRegister(rs), simm16));
			}
		}

		public void doANDI(int rt, int rs, int uimm16)
		{
			if (rt != 0)
			{
				setRegister(rt, getRegister(rs) & uimm16);
			}
		}

		public void doORI(int rt, int rs, int uimm16)
		{
			if (rt != 0)
			{
				setRegister(rt, getRegister(rs) | uimm16);
			}
		}

		public void doXORI(int rt, int rs, int uimm16)
		{
			if (rt != 0)
			{
				setRegister(rt, getRegister(rs) ^ uimm16);
			}
		}

		public void doLUI(int rt, int uimm16)
		{
			if (rt != 0)
			{
				setRegister(rt, uimm16 << 16);
			}
		}

		public void doROTR(int rd, int rt, int sa)
		{
			if (rd != 0)
			{
				setRegister(rd, Integer.rotateRight(getRegister(rt), sa));
			}
		}

		public void doROTRV(int rd, int rt, int rs)
		{
			if (rd != 0)
			{
				// no need of "getRegister(rs) & 31", rotateRight does it for us
				setRegister(rd, Integer.rotateRight(getRegister(rt), getRegister(rs)));
			}
		}

		public void doMOVZ(int rd, int rs, int rt)
		{
			if ((rd != 0) && (getRegister(rt) == 0))
			{
				setRegister(rd, getRegister(rs));
			}
		}

		public void doMOVN(int rd, int rs, int rt)
		{
			if ((rd != 0) && (getRegister(rt) != 0))
			{
				setRegister(rd, getRegister(rs));
			}
		}

		public void doCLZ(int rd, int rs)
		{
			if (rd != 0)
			{
				setRegister(rd, Integer.numberOfLeadingZeros(getRegister(rs)));
			}
		}

		public void doCLO(int rd, int rs)
		{
			if (rd != 0)
			{
				setRegister(rd, Integer.numberOfLeadingZeros(~getRegister(rs)));
			}
		}

		public void doMAX(int rd, int rs, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, System.Math.Max(getRegister(rs), getRegister(rt)));
			}
		}

		public void doMIN(int rd, int rs, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, System.Math.Min(getRegister(rs), getRegister(rt)));
			}
		}

		public void doEXT(int rt, int rs, int lsb, int msbd)
		{
			if (rt != 0)
			{
				setRegister(rt, extractBits(getRegister(rs), lsb, (msbd + 1)));
			}
		}

		public void doINS(int rt, int rs, int lsb, int msb)
		{
			if (rt != 0)
			{
				setRegister(rt, insertBits(getRegister(rt), getRegister(rs), lsb, msb));
			}
		}

		public void doWSBH(int rd, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, Integer.rotateRight(Integer.reverseBytes(getRegister(rt)), 16));
			}
		}

		public void doWSBW(int rd, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, Integer.reverseBytes(getRegister(rt)));
			}
		}

		public void doSEB(int rd, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, (sbyte)getRegister(rt));
			}
		}

		public void doBITREV(int rd, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, Integer.reverse(getRegister(rt)));
			}
		}

		public void doSEH(int rd, int rt)
		{
			if (rd != 0)
			{
				setRegister(rd, (short)getRegister(rt));
			}
		}
	}
}