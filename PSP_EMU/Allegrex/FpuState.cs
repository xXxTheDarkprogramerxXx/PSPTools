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
namespace pspsharp.Allegrex
{

	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	/// <summary>
	/// Floating Point Unit, handles floating point operations, including BCU and LSU
	/// 
	/// @author hli, gid15
	/// </summary>
	public class FpuState : BcuState
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			fcr31 = new Fcr31(this);
		}

		private const int STATE_VERSION = 0;
		public const bool IMPLEMENT_ROUNDING_MODES = true;
		private static readonly string[] roundingModeNames = new string[] {"Round to neareast number", "Round toward zero", "Round toward positive infinity", "Round toward negative infinity"};
		public const int ROUNDING_MODE_NEAREST = 0;
		public const int ROUNDING_MODE_TOWARD_ZERO = 1;
		public const int ROUNDING_MODE_TOWARD_POSITIVE_INF = 2;
		public const int ROUNDING_MODE_TOWARD_NEGATIVE_INF = 3;

		public sealed class Fcr0
		{
			public const int imp = 0; // FPU design number
			public const int rev = 0; // FPU revision bumber
		}

		public class Fcr31
		{
			private readonly FpuState outerInstance;

			internal const int STATE_VERSION = 0;
			public int rm;
			public bool c;
			public bool fs;

			public virtual void reset()
			{
				rm = 0;
				c = false;
				fs = false;
			}

			public Fcr31(FpuState outerInstance)
			{
				this.outerInstance = outerInstance;
				reset();
			}

			public Fcr31(FpuState outerInstance, Fcr31 that)
			{
				this.outerInstance = outerInstance;
				rm = that.rm;
				c = that.c;
				fs = that.fs;
			}

			public virtual void copy(Fcr31 that)
			{
				rm = that.rm;
				c = that.c;
				fs = that.fs;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
			public virtual void read(StateInputStream stream)
			{
				stream.readVersion(STATE_VERSION);
				rm = stream.readInt();
				c = stream.readBoolean();
				fs = stream.readBoolean();
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
			public virtual void write(StateOutputStream stream)
			{
				stream.writeVersion(STATE_VERSION);
				stream.writeInt(rm);
				stream.writeBoolean(c);
				stream.writeBoolean(fs);
			}
		}

		public readonly float[] fpr = new float[32];
		public Fcr31 fcr31;

		public override void reset()
		{
			Arrays.fill(fpr, 0.0f);
			fcr31.reset();
		}

		public override void resetAll()
		{
			base.resetAll();
			Arrays.fill(fpr, 0.0f);
			fcr31.reset();
		}

		public FpuState()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		public virtual void copy(FpuState that)
		{
			base.copy(that);
			Array.Copy(that.fpr, 0, fpr, 0, fpr.Length);
			fcr31.copy(that.fcr31);
		}

		public FpuState(FpuState that) : base(that)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			Array.Copy(that.fpr, 0, fpr, 0, fpr.Length);
			fcr31.copy(that.fcr31);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			stream.readFloats(fpr);
			fcr31.read(stream);
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeFloats(fpr);
			fcr31.write(stream);
			base.write(stream);
		}

		public virtual float round(double d)
		{
			float f = (float) d;

			if (float.IsInfinity(f) || float.IsNaN(f))
			{
				return f;
			}

			if (fcr31.fs)
			{
				// Flush-to-zero for denormalized numbers
				int exp = Math.getExponent(f);
				if (exp < Float.MIN_EXPONENT)
				{
					return 0f;
				}
			}

			switch (fcr31.rm)
			{
				case ROUNDING_MODE_NEAREST:
					// This is the java default rounding mode, nothing more to do.
					break;
				case ROUNDING_MODE_TOWARD_ZERO:
					if (d < 0.0)
					{
						if (d > f)
						{
							f = Math.nextUp(f);
						}
					}
					else
					{
						if (d < f)
						{
							f = Math.nextAfter(f, 0.0);
						}
					}
					break;
				case ROUNDING_MODE_TOWARD_POSITIVE_INF:
					if (d > f)
					{
						f = Math.nextUp(f);
					}
					break;
				case ROUNDING_MODE_TOWARD_NEGATIVE_INF:
					if (d < f)
					{
						f = Math.nextAfter(f, double.NegativeInfinity);
					}
					break;
				default:
					Emulator.log.error(string.Format("Unknown rounding mode {0:D}", fcr31.rm));
					break;
			}

			return f;
		}

		public virtual void doMFC1(int rt, int c1dr)
		{
			if (rt != 0)
			{
				setRegister(rt, Float.floatToRawIntBits(fpr[c1dr]));
			}
		}

		public virtual void doCFC1(int rt, int c1cr)
		{
			if (rt != 0)
			{
				switch (c1cr)
				{
					case 0:
						setRegister(rt, (Fcr0.imp << 8) | (Fcr0.rev));
						break;

					case 31:
						setRegister(rt, (fcr31.fs ? (1 << 24) : 0) | (fcr31.c ? (1 << 23) : 0) | (fcr31.rm & 3));
						break;

					default:
						doUNK(string.Format("Unsupported cfc1 instruction for fcr{0:D}", c1cr));
					break;
				}
			}
		}

		public virtual void doMTC1(int rt, int c1dr)
		{
			fpr[c1dr] = Float.intBitsToFloat(getRegister(rt));
		}

		public virtual void doCTC1(int rt, int c1cr)
		{
			switch (c1cr)
			{
				case 31:
					int bits = getRegister(rt) & 0x01800003;
					fcr31.rm = bits & 3;
					fcr31.fs = ((bits >> 24) & 1) != 0;
					fcr31.c = ((bits >> 23) & 1) != 0;
					if (fcr31.rm != ROUNDING_MODE_NEAREST)
					{
						// Only rounding mode 0 is supported in Java
						Emulator.log.warn(string.Format("CTC1 unsupported rounding mode '{0}' (rm={1:D})", roundingModeNames[fcr31.rm], fcr31.rm));
					}
					if (fcr31.fs)
					{
						// Flush-to-zero is not supported in Java
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: pspsharp.Emulator.log.warn(String.format("CTC1 unsupported flush-to-zero fs=%b", fcr31.fs));
						Emulator.log.warn(string.Format("CTC1 unsupported flush-to-zero fs=%b", fcr31.fs));
					}
					break;

				default:
					doUNK(string.Format("Unsupported ctc1 instruction for fcr{0:D}", c1cr));
				break;
			}
		}

		public virtual bool doBC1F(int simm16)
		{
			npc = !fcr31.c ? branchTarget(pc, simm16) : (pc + 4);
			return true;
		}

		public virtual bool doBC1T(int simm16)
		{
			npc = fcr31.c ? branchTarget(pc, simm16) : (pc + 4);
			return true;
		}

		public virtual bool doBC1FL(int simm16)
		{
			if (!fcr31.c)
			{
				npc = branchTarget(pc, simm16);
				return true;
			}
			pc += 4;
			return false;
		}

		public virtual bool doBC1TL(int simm16)
		{
			if (fcr31.c)
			{
				npc = branchTarget(pc, simm16);
				return true;
			}
			pc += 4;
			return false;
		}

		public virtual void doADDS(int fd, int fs, int ft)
		{
			fpr[fd] = fpr[fs] + fpr[ft];
		}

		public virtual void doSUBS(int fd, int fs, int ft)
		{
			fpr[fd] = fpr[fs] - fpr[ft];
		}

		public virtual void doMULS(int fd, int fs, int ft)
		{
			if (IMPLEMENT_ROUNDING_MODES)
			{
				fpr[fd] = round(fpr[fs] * (double) fpr[ft]);
			}
			else
			{
				fpr[fd] = fpr[fs] * fpr[ft];
			}
		}

		public virtual void doDIVS(int fd, int fs, int ft)
		{
			fpr[fd] = fpr[fs] / fpr[ft];
		}

		public virtual void doSQRTS(int fd, int fs)
		{
			fpr[fd] = (float) System.Math.Sqrt(fpr[fs]);
		}

		public virtual void doABSS(int fd, int fs)
		{
			fpr[fd] = System.Math.Abs(fpr[fs]);
		}

		public virtual void doMOVS(int fd, int fs)
		{
			fpr[fd] = fpr[fs];
		}

		public virtual void doNEGS(int fd, int fs)
		{
			fpr[fd] = 0.0f - fpr[fs];
		}

		public virtual void doROUNDWS(int fd, int fs)
		{
			fpr[fd] = Float.intBitsToFloat(System.Math.Round(fpr[fs]));
		}

		public virtual void doTRUNCWS(int fd, int fs)
		{
			fpr[fd] = Float.intBitsToFloat((int)(fpr[fs]));
		}

		public virtual void doCEILWS(int fd, int fs)
		{
			fpr[fd] = Float.intBitsToFloat((int) System.Math.Ceiling(fpr[fs]));
		}

		public virtual void doFLOORWS(int fd, int fs)
		{
			fpr[fd] = Float.intBitsToFloat((int) System.Math.Floor(fpr[fs]));
		}

		public virtual void doCVTSW(int fd, int fs)
		{
			fpr[fd] = Float.floatToRawIntBits(fpr[fs]);
		}

		public virtual void doCVTWS(int fd, int fs)
		{
			switch (fcr31.rm)
			{
				case ROUNDING_MODE_TOWARD_ZERO:
					fpr[fd] = Float.intBitsToFloat((int)(fpr[fs]));
					break;
				case ROUNDING_MODE_TOWARD_POSITIVE_INF:
					fpr[fd] = Float.intBitsToFloat((int) System.Math.Ceiling(fpr[fs]));
					break;
				case ROUNDING_MODE_TOWARD_NEGATIVE_INF:
					fpr[fd] = Float.intBitsToFloat((int) System.Math.Floor(fpr[fs]));
					break;
				default:
					fpr[fd] = Float.intBitsToFloat((int) Math.rint(fpr[fs]));
					break;
			}
		}

		public virtual void doCCONDS(int fs, int ft, int cond)
		{
			float x = fpr[fs];
			float y = fpr[ft];

			if (float.IsNaN(x) || float.IsNaN(y))
			{
				fcr31.c = (cond & 1) != 0;
			}
			else
			{
				bool equal = ((cond & 2) != 0) && (x == y);
				bool less = ((cond & 4) != 0) && (x < y);

				fcr31.c = less || equal;
			}
		}

		public virtual void doLWC1(int ft, int rs, int simm16)
		{
			fpr[ft] = Float.intBitsToFloat(memory.read32(getRegister(rs) + simm16));
		}

		public virtual void doSWC1(int ft, int rs, int simm16)
		{
			memory.write32(getRegister(rs) + simm16, Float.floatToRawIntBits(fpr[ft]));
		}
	}
}