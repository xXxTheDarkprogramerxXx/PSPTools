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

	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	/// <summary>
	/// Multiply Divide Unit, handles accumulators.
	/// 
	/// @author hli
	/// </summary>
	public class MduState : GprState
	{
		private const int STATE_VERSION = 0;
		public long hilo;

		public virtual int Hi
		{
			set
			{
				hilo = (hilo & 0xffffffffL) | (((long) value) << 32);
			}
			get
			{
				return (int)((long)((ulong)hilo >> 32));
			}
		}


		public virtual int Lo
		{
			set
			{
				hilo = (hilo & ~0xffffffffL) | ((value) & 0xffffffffL);
			}
			get
			{
				return unchecked((int)(hilo & 0xffffffffL));
			}
		}


		public override void reset()
		{
			hilo = 0;
		}

		public override void resetAll()
		{
			base.resetAll();
			hilo = 0;
		}

		public MduState()
		{
			hilo = 0;
		}

		public virtual void copy(MduState that)
		{
			base.copy(that);
			hilo = that.hilo;
		}

		public MduState(MduState that) : base(that)
		{
			hilo = that.hilo;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			hilo = stream.readLong();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeLong(hilo);
			base.write(stream);
		}

		public static long signedDivMod(int x, int y)
		{
			return (((long)(x % y)) << 32) | (((x / y)) & 0xffffffffL);
		}

		public static long unsignedDivMod(long x, long y)
		{
			return ((x % y) << 32) | ((x / y) & 0xffffffffL);
		}

		public void doMFHI(int rd)
		{
			if (rd != 0)
			{
				setRegister(rd, Hi);
			}
		}

		public void doMTHI(int rs)
		{
			int hi = getRegister(rs);
			hilo = (((long) hi) << 32) | (hilo & 0xffffffffL);
		}

		public void doMFLO(int rd)
		{
			if (rd != 0)
			{
				setRegister(rd, Lo);
			}
		}

		public void doMTLO(int rs)
		{
			int lo = getRegister(rs);
			hilo = (hilo & unchecked((long)0xffffffff00000000L)) | ((lo) & 0x00000000ffffffffL);
		}

		public void doMULT(int rs, int rt)
		{
			hilo = ((long) getRegister(rs)) * ((long) getRegister(rt));
		}

		public void doMULTU(int rs, int rt)
		{
			hilo = (getRegister(rs) & 0xffffffffL) * (getRegister(rt) & 0xffffffffL);
		}

		public void doDIV(int rs, int rt)
		{
			int rsValue = getRegister(rs);
			int rtValue = getRegister(rt);
			if (rtValue == 0)
			{
				// According to MIPS spec., result is unpredictable when dividing by zero.
				// However on a PSP, hi is set to $rs register value and lo is set to 0x0000FFFF/0xFFFFFFFF.
				// This has been tested on a real PSP using vfputest.pbp.
				long lo = rsValue > 0xFFFF ? 0xFFFFFFFFL : 0x0000FFFFL;
				hilo = (((long) rsValue) << 32) | lo;
			}
			else
			{
				int lo = rsValue / rtValue;
				int hi = rsValue % rtValue;
				hilo = (((long) hi) << 32) | ((lo) & 0xffffffffL);
			}
		}

		public void doDIVU(int rs, int rt)
		{
			int rsValue = getRegister(rs);
			int rtValue = getRegister(rt);
			if (rtValue == 0)
			{
				// According to MIPS spec., result is unpredictable when dividing by zero.
				// However on a PSP, hi is set to $rs register value and lo is set to 0x0000FFFF/0xFFFFFFFF.
				// This has been tested on a real PSP using vfputest.pbp.
				long lo = rsValue > 0xFFFF ? 0xFFFFFFFFL : 0x0000FFFFL;
				hilo = (((long) rsValue) << 32) | lo;
			}
			else
			{
				long x = rsValue & 0xFFFFFFFFL;
				long y = rtValue & 0xFFFFFFFFL;
				hilo = ((x % y) << 32) | ((x / y) & 0xFFFFFFFFL);
			}
		}

		public void doMADD(int rs, int rt)
		{
			hilo += ((long) getRegister(rs)) * ((long) getRegister(rt));
		}

		public void doMADDU(int rs, int rt)
		{
			hilo += (getRegister(rs) & 0xffffffffL) * (getRegister(rt) & 0xffffffffL);
		}

		public void doMSUB(int rs, int rt)
		{
			hilo -= ((long) getRegister(rs)) * ((long) getRegister(rt));
		}

		public void doMSUBU(int rs, int rt)
		{
			hilo -= (getRegister(rs) & 0xffffffffL) * (getRegister(rt) & 0xffffffffL);
		}
	}
}