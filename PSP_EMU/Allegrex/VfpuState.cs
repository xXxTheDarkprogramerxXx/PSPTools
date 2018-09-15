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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Float.isInfinite;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Float.isNaN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.abs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;


	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	/// <summary>
	/// Vectorial Floating Point Unit, handles scalar, vector and matrix operations.
	/// 
	/// @author hli, gid15
	/// </summary>
	public class VfpuState : FpuState
	{
		private const int STATE_VERSION = 0;
		// We have a problem using Float.intBitsToFloat():
		// extract from the JDK 1.6 documentation:
		//    "...Consequently, for some int values,
		//     floatToRawIntBits(intBitsToFloat(start)) may not equal start.
		//     Moreover, which particular bit patterns represent signaling NaNs
		//     is platform dependent..."
		// Furthermore, it seems that the Java interpreter and the Java JIT compiler
		// produce different results.
		//
		// The PSP does not alter data when loading/saving values to/from VFPU registers.
		// Some applications are using sequences of lv.q/vwb.q to implement a memcpy(),
		// so it is important to keep all the bits unchanged while loading & storing
		// values from VFPU registers.
		// This is the reason why this implementation is keeping VFPU values into
		// a arrays keeping both int and float representations, instead
		// of just using float values.
		//
		// This has a performance impact on VFPU instructions but provides accuracy.
		// The compilerPerf.pbp application reports around 50% duration increase
		// for the execution of a vadd instruction.
		//

		//
		// Use a linear version of the vpr, storing the 8 x 2D-matrix
		// in a 1D-array. This is giving a better performance for the compiler.
		// For the interpreter, the methods
		//    getVpr(m, c, r)
		// and
		//    setVpr(m, c, r, value)
		// are getting a value and setting a value into the vpr arrays,
		// doing the mapping from the matrix index to the linear index.
		//
		// Also, keep the int and float values in two different arrays to allow the
		// Java JIT compiler to generate more efficient native code.
		//
		public readonly float[] vprFloat = new float[128];
		public readonly int[] vprInt = new int[128];

		public static readonly float[] floatConstants = new float[] {0.0f, float.MaxValue, (float) System.Math.Sqrt(2.0f), (float) System.Math.Sqrt(0.5f), 2.0f / (float) System.Math.Sqrt(Math.PI), 2.0f / (float) Math.PI, 1.0f / (float) Math.PI, (float) Math.PI / 4.0f, (float) Math.PI / 2.0f, (float) Math.PI, (float) Math.E, (float)(System.Math.Log(Math.E) / System.Math.Log(2.0)), (float) System.Math.Log10(Math.E), (float) System.Math.Log(2.0), (float) System.Math.Log(10.0), (float) Math.PI * 2.0f, (float) Math.PI / 6.0f, (float) System.Math.Log10(2.0), (float)(System.Math.Log(10.0) / System.Math.Log(2.0)), (float) System.Math.Sqrt(3.0) / 2.0f};
		public const int pspNaNint = 0x7F800001;
		public const int pspNaNintBad = 0x7FC00001;
		public static readonly float pspNaNfloat = Float.intBitsToFloat(pspNaNint);
		public static readonly float PI_2 = (float)(Math.PI * 0.5);

		private class Random
		{
			internal const int STATE_VERSION = 0;
			internal long seed;

			internal const long multiplier = 0x5DEECE66DL;
			internal const long addend = 0xBL;
			internal static readonly long mask = (1L << 32) - 1;

			public Random() : this(0x3f800001)
			{
			}

			public Random(int seed)
			{
				Seed = seed;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
			public virtual void read(StateInputStream stream)
			{
				stream.readVersion(STATE_VERSION);
				seed = stream.readLong();
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
			public virtual void write(StateOutputStream stream)
			{
				stream.writeVersion(STATE_VERSION);
				stream.writeLong(seed);
			}

			public virtual int Seed
			{
				set
				{
					this.seed = (value) & mask;
				}
				get
				{
					return (int)seed;
				}
			}


			protected internal virtual int next(int bits)
			{
				seed = (seed * multiplier + addend) & mask;
				return (int)((long)((ulong)seed >> (32 - bits)));
			}

			public virtual int nextInt()
			{
				return next(32);
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public int nextInt(int n)
			public virtual int nextInt(int n)
			{
				if (n <= 0)
				{
					throw new System.ArgumentException("n must be positive");
				}

				if ((n & -n) == n) // i.e., n is a power of 2
				{
					return (int)((n * (long)next(31)) >> 31);
				}

				int bits, val;
				do
				{
					bits = next(31);
					val = bits % n;
				} while (bits - val + (n - 1) < 0);
				return val;
			}

			public virtual float nextFloat()
			{
				return next(24) / ((float)(1 << 24));
			}
		}

		private static Random rnd;

		public class Vcr
		{
			internal const int STATE_VERSION = 0;

			public class PfxSrc
			{
				internal const int STATE_VERSION = 0;

				public int[] swz;
				public bool[] abs;
				public bool[] cst;
				public bool[] neg;
				public bool enabled;

				public virtual void reset()
				{
					for (int i = 0; i < swz.Length; i++)
					{
						swz[i] = i;
					}
					Arrays.Fill(abs, false);
					Arrays.Fill(cst, false);
					Arrays.Fill(neg, false);
					enabled = false;
				}

				public PfxSrc()
				{
					swz = new int[4];
					abs = new bool[4];
					cst = new bool[4];
					neg = new bool[4];
					enabled = false;
				}

				public virtual void copy(PfxSrc that)
				{
					Array.Copy(that.swz, 0, swz, 0, swz.Length);
					Array.Copy(that.abs, 0, abs, 0, abs.Length);
					Array.Copy(that.cst, 0, cst, 0, cst.Length);
					Array.Copy(that.neg, 0, neg, 0, neg.Length);
					enabled = that.enabled;
				}

				public PfxSrc(PfxSrc that)
				{
					copy(that);
				}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
				public virtual void read(StateInputStream stream)
				{
					stream.readVersion(STATE_VERSION);
					stream.readInts(swz);
					stream.readBooleans(abs);
					stream.readBooleans(cst);
					stream.readBooleans(neg);
					enabled = stream.readBoolean();
				}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
				public virtual void write(StateOutputStream stream)
				{
					stream.writeVersion(STATE_VERSION);
					stream.writeInts(swz);
					stream.writeBooleans(abs);
					stream.writeBooleans(cst);
					stream.writeBooleans(neg);
					stream.writeBoolean(enabled);
				}
			}
			public PfxSrc pfxs;
			public PfxSrc pfxt;

			public class PfxDst
			{
				internal const int STATE_VERSION = 0;

				public int[] sat;
				public bool[] msk;
				public bool enabled;

				public virtual void reset()
				{
					Arrays.Fill(sat, 0);
					Arrays.Fill(msk, false);
					enabled = false;
				}

				public PfxDst()
				{
					sat = new int[4];
					msk = new bool[4];
					enabled = false;
				}

				public virtual void copy(PfxDst that)
				{
					Array.Copy(that.sat, 0, sat, 0, sat.Length);
					Array.Copy(that.msk, 0, msk, 0, msk.Length);
					enabled = that.enabled;
				}

				public PfxDst(PfxDst that)
				{
					copy(that);
				}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
				public virtual void read(StateInputStream stream)
				{
					stream.readVersion(STATE_VERSION);
					stream.readInts(sat);
					stream.readBooleans(msk);
					enabled = stream.readBoolean();
				}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
				public virtual void write(StateOutputStream stream)
				{
					stream.writeVersion(STATE_VERSION);
					stream.writeInts(sat);
					stream.writeBooleans(msk);
					stream.writeBoolean(enabled);
				}
			}
			public PfxDst pfxd;
			public bool[] cc;

			public virtual void reset()
			{
				pfxs.reset();
				pfxt.reset();
				pfxd.reset();
				Arrays.Fill(cc, false);
			}

			public Vcr()
			{
				pfxs = new PfxSrc();
				pfxt = new PfxSrc();
				pfxd = new PfxDst();
				cc = new bool[6];
			}

			public virtual void copy(Vcr that)
			{
				pfxs.copy(that.pfxs);
				pfxt.copy(that.pfxt);
				pfxd.copy(that.pfxd);
				cc = that.cc.Clone();
			}

			public Vcr(Vcr that)
			{
				pfxs = new PfxSrc(that.pfxs);
				pfxt = new PfxSrc(that.pfxt);
				pfxd = new PfxDst(that.pfxd);
				cc = that.cc.Clone();
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
			public virtual void read(StateInputStream stream)
			{
				stream.readVersion(STATE_VERSION);
				pfxs.read(stream);
				pfxt.read(stream);
				pfxd.read(stream);
				stream.readBooleans(cc);
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
			public virtual void write(StateOutputStream stream)
			{
				stream.writeVersion(STATE_VERSION);
				pfxs.write(stream);
				pfxt.write(stream);
				pfxd.write(stream);
				stream.writeBooleans(cc);
			}
		}
		public Vcr vcr;

		private void resetFpr()
		{
			for (int i = 0; i < vprFloat.Length; i++)
			{
				vprFloat[i] = 0f;
				vprInt[i] = 0;
			}
		}

		public override void reset()
		{
			resetFpr();
			vcr.reset();
		}

		public override void resetAll()
		{
			base.resetAll();
			resetFpr();
			vcr.reset();
		}

		public VfpuState()
		{
			vcr = new Vcr();
			rnd = new Random();
		}

		public virtual void copy(VfpuState that)
		{
			base.copy(that);
			Array.Copy(that.vprFloat, 0, vprFloat, 0, vprFloat.Length);
			Array.Copy(that.vprInt, 0, vprInt, 0, vprInt.Length);
			vcr.copy(that.vcr);
		}

		public VfpuState(VfpuState that) : base(that)
		{
			Array.Copy(that.vprFloat, 0, vprFloat, 0, vprFloat.Length);
			Array.Copy(that.vprInt, 0, vprInt, 0, vprInt.Length);
			vcr = new Vcr(that.vcr);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			stream.readFloats(vprFloat);
			stream.readInts(vprInt);
			rnd.read(stream);
			vcr.read(stream);
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeFloats(vprFloat);
			stream.writeInts(vprInt);
			rnd.write(stream);
			vcr.write(stream);
			base.write(stream);
		}

		/// <summary>
		/// Get the index of the Vpr register into the vprInt and vprFloat Array. </summary>
		/// <param name="m"> register matrix index </param>
		/// <param name="c"> register column index </param>
		/// <param name="r"> register row index </param>
		/// <returns> the index of the Vpr register into the vprInt and vprFloat Array. </returns>
		public static int getVprIndex(int m, int c, int r)
		{
			return (m << 4) + (c << 2) + r;
		}

		/// <summary>
		/// Return the value of the given Vpr register, as a float value. </summary>
		/// <param name="m"> register matrix index </param>
		/// <param name="c"> register column index </param>
		/// <param name="r"> register row index </param>
		/// <returns> the value of the Vpr register, as a float value. </returns>
		public virtual float getVprFloat(int m, int c, int r)
		{
			return vprFloat[getVprIndex(m, c, r)];
		}

		/// <summary>
		/// Return the value of the given Vpr register, as a 32-bit integer value. </summary>
		/// <param name="m"> register matrix index </param>
		/// <param name="c"> register column index </param>
		/// <param name="r"> register row index </param>
		/// <returns> the value of the Vpr register, as a 32-bit integer value. </returns>
		public virtual int getVprInt(int m, int c, int r)
		{
			return vprInt[getVprIndex(m, c, r)];
		}

		/// <summary>
		/// Set the value of the given Vpr register.
		/// The value is given as a float value. </summary>
		/// <param name="m"> register matrix index </param>
		/// <param name="c"> register column index </param>
		/// <param name="r"> register row index </param>
		/// <param name="value"> the value to be set to the Vpr register, as a float value. </param>
		public virtual void setVprFloat(int m, int c, int r, float value)
		{
			int index = getVprIndex(m, c, r);
			vprFloat[index] = value;
			vprInt[index] = Float.floatToRawIntBits(value);

			// Copying the pspNaNfloat value can change its internal representation.
			// Fix such a case here.
			if ((vprInt[index] & 0x7FFFFFFF) == pspNaNintBad)
			{
				vprInt[index] = pspNaNint | (vprInt[index] & unchecked((int)0x80000000));
			}
		}

		/// <summary>
		/// Set the value of the given Vpr register.
		/// The value is given as a 32-bit integer value. </summary>
		/// <param name="m"> register matrix index </param>
		/// <param name="c"> register column index </param>
		/// <param name="r"> register row index </param>
		/// <param name="value"> the value to be set to the Vpr register, as a 32-bit integer value. </param>
		public virtual void setVprInt(int m, int c, int r, int value)
		{
			int index = getVprIndex(m, c, r);
			vprFloat[index] = Float.intBitsToFloat(value);
			vprInt[index] = value;
		}

		// Temporary variables
		private readonly float[] v1 = new float[4];
		private readonly float[] v2 = new float[4];
		private readonly float[] v3 = new float[4];
		private readonly int[] v1i = new int[4];
		private readonly int[] v2i = new int[4];
		private readonly int[] v3i = new int[4];

		// VFPU stuff
		private float transformVr(int swz, bool abs, bool cst, bool neg, float[] x)
		{
			float value = 0.0f;
			if (cst)
			{
				switch (swz)
				{
					case 0:
						value = abs ? 3.0f : 0.0f;
						break;
					case 1:
						value = abs ? (1.0f / 3.0f) : 1.0f;
						break;
					case 2:
						value = abs ? (1.0f / 4.0f) : 2.0f;
						break;
					case 3:
						value = abs ? (1.0f / 6.0f) : 0.5f;
						break;
				}
			}
			else
			{
				value = x[swz];
			}

			if (abs)
			{
				value = System.Math.Abs(value);
			}
			return neg ? (0.0f - value) : value;
		}

		private int transformVrInt(int swz, bool abs, bool cst, bool neg, int[] x)
		{
			if (!cst && !abs && !neg)
			{
				return x[swz]; // Pure int value
			}

			float value = 0.0f;
			if (cst)
			{
				switch (swz)
				{
					case 0:
						value = abs ? 3.0f : 0.0f;
						break;
					case 1:
						value = abs ? (1.0f / 3.0f) : 1.0f;
						break;
					case 2:
						value = abs ? (1.0f / 4.0f) : 2.0f;
						break;
					case 3:
						value = abs ? (1.0f / 6.0f) : 0.5f;
						break;
				}
			}
			else
			{
				value = Float.intBitsToFloat(x[swz]);
			}

			if (abs)
			{
				value = System.Math.Abs(value);
			}
			if (neg)
			{
				value = 0.0f - value;
			}
			return Float.floatToRawIntBits(value);
		}

		private float applyPrefixVs(int i, float[] x)
		{
			return transformVr(vcr.pfxs.swz[i], vcr.pfxs.abs[i], vcr.pfxs.cst[i], vcr.pfxs.neg[i], x);
		}

		private int applyPrefixVsInt(int i, int[] x)
		{
			return transformVrInt(vcr.pfxs.swz[i], vcr.pfxs.abs[i], vcr.pfxs.cst[i], vcr.pfxs.neg[i], x);
		}

		private float applyPrefixVt(int i, float[] x)
		{
			return transformVr(vcr.pfxt.swz[i], vcr.pfxt.abs[i], vcr.pfxt.cst[i], vcr.pfxt.neg[i], x);
		}

		private int applyPrefixVtInt(int i, int[] x)
		{
			return transformVrInt(vcr.pfxt.swz[i], vcr.pfxt.abs[i], vcr.pfxt.cst[i], vcr.pfxt.neg[i], x);
		}

		private float applyPrefixVd(int i, float value)
		{
			switch (vcr.pfxd.sat[i])
			{
				case 1:
					return System.Math.Max(0.0f, System.Math.Min(1.0f, value));
				case 3:
					return System.Math.Max(-1.0f, System.Math.Min(1.0f, value));
			}
			return value;
		}

		private int applyPrefixVdInt(int i, int value)
		{
			switch (vcr.pfxd.sat[i])
			{
				case 1:
					return Float.floatToRawIntBits(System.Math.Max(0.0f, System.Math.Min(1.0f, Float.intBitsToFloat(value))));
				case 3:
					return Float.floatToRawIntBits(System.Math.Max(-1.0f, System.Math.Min(1.0f, Float.intBitsToFloat(value))));
			}
			return value;
		}

		public virtual void loadVs(int vsize, int vs)
		{
			int m, s, i;

			m = (vs >> 2) & 7;
			i = (vs >> 0) & 3;

			switch (vsize)
			{
				case 1:
					s = (vs >> 5) & 3;
					v1[0] = getVprFloat(m, i, s);
					if (vcr.pfxs.enabled)
					{
						v1[0] = applyPrefixVs(0, v1);
						vcr.pfxs.enabled = false;
					}
					break;

				case 2:
					s = (vs & 64) >> 5;
					if ((vs & 32) != 0)
					{
						v1[0] = getVprFloat(m, s + 0, i);
						v1[1] = getVprFloat(m, s + 1, i);
					}
					else
					{
						v1[0] = getVprFloat(m, i, s + 0);
						v1[1] = getVprFloat(m, i, s + 1);
					}
					if (vcr.pfxs.enabled)
					{
						v3[0] = applyPrefixVs(0, v1);
						v3[1] = applyPrefixVs(1, v1);
						v1[0] = v3[0];
						v1[1] = v3[1];
						vcr.pfxs.enabled = false;
					}
					break;

				case 3:
					s = (vs & 64) >> 6;
					if ((vs & 32) != 0)
					{
						v1[0] = getVprFloat(m, s + 0, i);
						v1[1] = getVprFloat(m, s + 1, i);
						v1[2] = getVprFloat(m, s + 2, i);
					}
					else
					{
						v1[0] = getVprFloat(m, i, s + 0);
						v1[1] = getVprFloat(m, i, s + 1);
						v1[2] = getVprFloat(m, i, s + 2);
					}
					if (vcr.pfxs.enabled)
					{
						v3[0] = applyPrefixVs(0, v1);
						v3[1] = applyPrefixVs(1, v1);
						v3[2] = applyPrefixVs(2, v1);
						v1[0] = v3[0];
						v1[1] = v3[1];
						v1[2] = v3[2];
						vcr.pfxs.enabled = false;
					}
					break;

				case 4:
					s = (vs & 64) >> 5;
					if ((vs & 32) != 0)
					{
						v1[0] = getVprFloat(m, (0 + s) & 3, i);
						v1[1] = getVprFloat(m, (1 + s) & 3, i);
						v1[2] = getVprFloat(m, (2 + s) & 3, i);
						v1[3] = getVprFloat(m, (3 + s) & 3, i);
					}
					else
					{
						v1[0] = getVprFloat(m, i, (0 + s) & 3);
						v1[1] = getVprFloat(m, i, (1 + s) & 3);
						v1[2] = getVprFloat(m, i, (2 + s) & 3);
						v1[3] = getVprFloat(m, i, (3 + s) & 3);
					}
					if (vcr.pfxs.enabled)
					{
						v3[0] = applyPrefixVs(0, v1);
						v3[1] = applyPrefixVs(1, v1);
						v3[2] = applyPrefixVs(2, v1);
						v3[3] = applyPrefixVs(3, v1);
						v1[0] = v3[0];
						v1[1] = v3[1];
						v1[2] = v3[2];
						v1[3] = v3[3];
						vcr.pfxs.enabled = false;
					}
					break;

				default:
					break;
			}
		}

		public virtual void loadVsInt(int vsize, int vs)
		{
			int m, s, i;

			m = (vs >> 2) & 7;
			i = (vs >> 0) & 3;

			switch (vsize)
			{
				case 1:
					s = (vs >> 5) & 3;
					v1i[0] = getVprInt(m, i, s);
					if (vcr.pfxs.enabled)
					{
						v1i[0] = applyPrefixVsInt(0, v1i);
						vcr.pfxs.enabled = false;
					}
					break;

				case 2:
					s = (vs & 64) >> 5;
					if ((vs & 32) != 0)
					{
						v1i[0] = getVprInt(m, s + 0, i);
						v1i[1] = getVprInt(m, s + 1, i);
					}
					else
					{
						v1i[0] = getVprInt(m, i, s + 0);
						v1i[1] = getVprInt(m, i, s + 1);
					}
					if (vcr.pfxs.enabled)
					{
						v3i[0] = applyPrefixVsInt(0, v1i);
						v3i[1] = applyPrefixVsInt(1, v1i);
						v1i[0] = v3i[0];
						v1i[1] = v3i[1];
						vcr.pfxs.enabled = false;
					}
					break;

				case 3:
					s = (vs & 64) >> 6;
					if ((vs & 32) != 0)
					{
						v1i[0] = getVprInt(m, s + 0, i);
						v1i[1] = getVprInt(m, s + 1, i);
						v1i[2] = getVprInt(m, s + 2, i);
					}
					else
					{
						v1i[0] = getVprInt(m, i, s + 0);
						v1i[1] = getVprInt(m, i, s + 1);
						v1i[2] = getVprInt(m, i, s + 2);
					}
					if (vcr.pfxs.enabled)
					{
						v3i[0] = applyPrefixVsInt(0, v1i);
						v3i[1] = applyPrefixVsInt(1, v1i);
						v3i[2] = applyPrefixVsInt(2, v1i);
						v1i[0] = v3i[0];
						v1i[1] = v3i[1];
						v1i[2] = v3i[2];
						vcr.pfxs.enabled = false;
					}
					break;

				case 4:
					s = (vs & 64) >> 5;
					if ((vs & 32) != 0)
					{
						v1i[0] = getVprInt(m, (0 + s) & 3, i);
						v1i[1] = getVprInt(m, (1 + s) & 3, i);
						v1i[2] = getVprInt(m, (2 + s) & 3, i);
						v1i[3] = getVprInt(m, (3 + s) & 3, i);
					}
					else
					{
						v1i[0] = getVprInt(m, i, (0 + s) & 3);
						v1i[1] = getVprInt(m, i, (1 + s) & 3);
						v1i[2] = getVprInt(m, i, (2 + s) & 3);
						v1i[3] = getVprInt(m, i, (3 + s) & 3);
					}
					if (vcr.pfxs.enabled)
					{
						v3i[0] = applyPrefixVsInt(0, v1i);
						v3i[1] = applyPrefixVsInt(1, v1i);
						v3i[2] = applyPrefixVsInt(2, v1i);
						v3i[3] = applyPrefixVsInt(3, v1i);
						v1i[0] = v3i[0];
						v1i[1] = v3i[1];
						v1i[2] = v3i[2];
						v1i[3] = v3i[3];
						vcr.pfxs.enabled = false;
					}
					break;

				default:
					break;
			}
		}

		public virtual void loadVt(int vsize, int vt)
		{
			int m, s, i;

			m = (vt >> 2) & 7;
			i = (vt >> 0) & 3;

			switch (vsize)
			{
				case 1:
					s = (vt >> 5) & 3;
					v2[0] = getVprFloat(m, i, s);
					if (vcr.pfxt.enabled)
					{
						v2[0] = applyPrefixVt(0, v2);
						vcr.pfxt.enabled = false;
					}
					break;

				case 2:
					s = (vt & 64) >> 5;
					if ((vt & 32) != 0)
					{
						v2[0] = getVprFloat(m, s + 0, i);
						v2[1] = getVprFloat(m, s + 1, i);
					}
					else
					{
						v2[0] = getVprFloat(m, i, s + 0);
						v2[1] = getVprFloat(m, i, s + 1);
					}
					if (vcr.pfxt.enabled)
					{
						v3[0] = applyPrefixVt(0, v2);
						v3[1] = applyPrefixVt(1, v2);
						v2[0] = v3[0];
						v2[1] = v3[1];
						vcr.pfxt.enabled = false;
					}
					break;

				case 3:
					s = (vt & 64) >> 6;
					if ((vt & 32) != 0)
					{
						v2[0] = getVprFloat(m, s + 0, i);
						v2[1] = getVprFloat(m, s + 1, i);
						v2[2] = getVprFloat(m, s + 2, i);
					}
					else
					{
						v2[0] = getVprFloat(m, i, s + 0);
						v2[1] = getVprFloat(m, i, s + 1);
						v2[2] = getVprFloat(m, i, s + 2);
					}
					if (vcr.pfxt.enabled)
					{
						v3[0] = applyPrefixVt(0, v2);
						v3[1] = applyPrefixVt(1, v2);
						v3[2] = applyPrefixVt(2, v2);
						v2[0] = v3[0];
						v2[1] = v3[1];
						v2[2] = v3[2];
						vcr.pfxt.enabled = false;
					}
					break;

				case 4:
					s = (vt & 64) >> 5;
					if ((vt & 32) != 0)
					{
						v2[0] = getVprFloat(m, (0 + s) & 3, i);
						v2[1] = getVprFloat(m, (1 + s) & 3, i);
						v2[2] = getVprFloat(m, (2 + s) & 3, i);
						v2[3] = getVprFloat(m, (3 + s) & 3, i);
					}
					else
					{
						v2[0] = getVprFloat(m, i, (0 + s) & 3);
						v2[1] = getVprFloat(m, i, (1 + s) & 3);
						v2[2] = getVprFloat(m, i, (2 + s) & 3);
						v2[3] = getVprFloat(m, i, (3 + s) & 3);
					}
					if (vcr.pfxt.enabled)
					{
						v3[0] = applyPrefixVt(0, v2);
						v3[1] = applyPrefixVt(1, v2);
						v3[2] = applyPrefixVt(2, v2);
						v3[3] = applyPrefixVt(3, v2);
						v2[0] = v3[0];
						v2[1] = v3[1];
						v2[2] = v3[2];
						v2[3] = v3[3];
						vcr.pfxt.enabled = false;
					}
					break;

				default:
					break;
			}
		}

		public virtual void loadVtInt(int vsize, int vt)
		{
			int m, s, i;

			m = (vt >> 2) & 7;
			i = (vt >> 0) & 3;

			switch (vsize)
			{
				case 1:
					s = (vt >> 5) & 3;
					v2i[0] = getVprInt(m, i, s);
					if (vcr.pfxt.enabled)
					{
						v2i[0] = applyPrefixVtInt(0, v2i);
						vcr.pfxt.enabled = false;
					}
					break;

				case 2:
					s = (vt & 64) >> 5;
					if ((vt & 32) != 0)
					{
						v2i[0] = getVprInt(m, s + 0, i);
						v2i[1] = getVprInt(m, s + 1, i);
					}
					else
					{
						v2i[0] = getVprInt(m, i, s + 0);
						v2i[1] = getVprInt(m, i, s + 1);
					}
					if (vcr.pfxt.enabled)
					{
						v3i[0] = applyPrefixVtInt(0, v2i);
						v3i[1] = applyPrefixVtInt(1, v2i);
						v2i[0] = v3i[0];
						v2i[1] = v3i[1];
						vcr.pfxt.enabled = false;
					}
					break;

				case 3:
					s = (vt & 64) >> 6;
					if ((vt & 32) != 0)
					{
						v2i[0] = getVprInt(m, s + 0, i);
						v2i[1] = getVprInt(m, s + 1, i);
						v2i[2] = getVprInt(m, s + 2, i);
					}
					else
					{
						v2i[0] = getVprInt(m, i, s + 0);
						v2i[1] = getVprInt(m, i, s + 1);
						v2i[2] = getVprInt(m, i, s + 2);
					}
					if (vcr.pfxt.enabled)
					{
						v3i[0] = applyPrefixVtInt(0, v2i);
						v3i[1] = applyPrefixVtInt(1, v2i);
						v3i[2] = applyPrefixVtInt(2, v2i);
						v2i[0] = v3i[0];
						v2i[1] = v3i[1];
						v2i[2] = v3i[2];
						vcr.pfxt.enabled = false;
					}
					break;

				case 4:
					s = (vt & 64) >> 5;
					if ((vt & 32) != 0)
					{
						v2i[0] = getVprInt(m, (0 + s) & 3, i);
						v2i[1] = getVprInt(m, (1 + s) & 3, i);
						v2i[2] = getVprInt(m, (2 + s) & 3, i);
						v2i[3] = getVprInt(m, (3 + s) & 3, i);
					}
					else
					{
						v2i[0] = getVprInt(m, i, (0 + s) & 3);
						v2i[1] = getVprInt(m, i, (1 + s) & 3);
						v2i[2] = getVprInt(m, i, (2 + s) & 3);
						v2i[3] = getVprInt(m, i, (3 + s) & 3);
					}
					if (vcr.pfxt.enabled)
					{
						v3i[0] = applyPrefixVtInt(0, v2i);
						v3i[1] = applyPrefixVtInt(1, v2i);
						v3i[2] = applyPrefixVtInt(2, v2i);
						v3i[3] = applyPrefixVtInt(3, v2i);
						v2i[0] = v3i[0];
						v2i[1] = v3i[1];
						v2i[2] = v3i[2];
						v2i[3] = v3i[3];
						vcr.pfxt.enabled = false;
					}
					break;

				default:
					break;
			}
		}

		public virtual void loadVd(int vsize, int vd)
		{
			int m, s, i;

			m = (vd >> 2) & 7;
			i = (vd >> 0) & 3;

			switch (vsize)
			{
				case 1:
					s = (vd >> 5) & 3;
					v3[0] = getVprFloat(m, i, s);
					break;

				case 2:
					s = (vd & 64) >> 5;
					if ((vd & 32) != 0)
					{
						v3[0] = getVprFloat(m, s + 0, i);
						v3[1] = getVprFloat(m, s + 1, i);
					}
					else
					{
						v3[0] = getVprFloat(m, i, s + 0);
						v3[1] = getVprFloat(m, i, s + 1);
					}
					break;

				case 3:
					s = (vd & 64) >> 6;
					if ((vd & 32) != 0)
					{
						v3[0] = getVprFloat(m, s + 0, i);
						v3[1] = getVprFloat(m, s + 1, i);
						v3[2] = getVprFloat(m, s + 2, i);
					}
					else
					{
						v3[0] = getVprFloat(m, i, s + 0);
						v3[1] = getVprFloat(m, i, s + 1);
						v3[2] = getVprFloat(m, i, s + 2);
					}
					break;

				case 4:
					s = (vd & 64) >> 5;
					if ((vd & 32) != 0)
					{
						v3[0] = getVprFloat(m, (0 + s) & 3, i);
						v3[1] = getVprFloat(m, (1 + s) & 3, i);
						v3[2] = getVprFloat(m, (2 + s) & 3, i);
						v3[3] = getVprFloat(m, (3 + s) & 3, i);
					}
					else
					{
						v3[0] = getVprFloat(m, i, (0 + s) & 3);
						v3[1] = getVprFloat(m, i, (1 + s) & 3);
						v3[2] = getVprFloat(m, i, (2 + s) & 3);
						v3[3] = getVprFloat(m, i, (3 + s) & 3);
					}
					break;

				default:
					break;
			}
		}

		public virtual void loadVdInt(int vsize, int vd)
		{
			int m, s, i;

			m = (vd >> 2) & 7;
			i = (vd >> 0) & 3;

			switch (vsize)
			{
				case 1:
					s = (vd >> 5) & 3;
					v3i[0] = getVprInt(m, i, s);
					break;

				case 2:
					s = (vd & 64) >> 5;
					if ((vd & 32) != 0)
					{
						v3i[0] = getVprInt(m, s + 0, i);
						v3i[1] = getVprInt(m, s + 1, i);
					}
					else
					{
						v3i[0] = getVprInt(m, i, s + 0);
						v3i[1] = getVprInt(m, i, s + 1);
					}
					break;

				case 3:
					s = (vd & 64) >> 6;
					if ((vd & 32) != 0)
					{
						v3i[0] = getVprInt(m, s + 0, i);
						v3i[1] = getVprInt(m, s + 1, i);
						v3i[2] = getVprInt(m, s + 2, i);
					}
					else
					{
						v3i[0] = getVprInt(m, i, s + 0);
						v3i[1] = getVprInt(m, i, s + 1);
						v3i[2] = getVprInt(m, i, s + 2);
					}
					break;

				case 4:
					s = (vd & 64) >> 5;
					if ((vd & 32) != 0)
					{
						v3i[0] = getVprInt(m, (0 + s) & 3, i);
						v3i[1] = getVprInt(m, (1 + s) & 3, i);
						v3i[2] = getVprInt(m, (2 + s) & 3, i);
						v3i[3] = getVprInt(m, (3 + s) & 3, i);
					}
					else
					{
						v3i[0] = getVprInt(m, i, (0 + s) & 3);
						v3i[1] = getVprInt(m, i, (1 + s) & 3);
						v3i[2] = getVprInt(m, i, (2 + s) & 3);
						v3i[3] = getVprInt(m, i, (3 + s) & 3);
					}
					break;

				default:
					break;
			}
		}

		public virtual void saveVd(int vsize, int vd, float[] vr)
		{
			int m, s, i;

			m = (vd >> 2) & 7;
			i = (vd >> 0) & 3;

			switch (vsize)
			{
				case 1:
					s = (vd >> 5) & 3;
					if (vcr.pfxd.enabled)
					{
						if (!vcr.pfxd.msk[0])
						{
							setVprFloat(m, i, s, applyPrefixVd(0, vr[0]));
						}
						vcr.pfxd.enabled = false;
					}
					else
					{
						setVprFloat(m, i, s, vr[0]);
					}
					break;

				case 2:
					s = (vd & 64) >> 5;
					if (vcr.pfxd.enabled)
					{
						if ((vd & 32) != 0)
						{
							for (int j = 0; j < 2; ++j)
							{
								if (!vcr.pfxd.msk[j])
								{
									setVprFloat(m, s + j, i, applyPrefixVd(j, vr[j]));
								}
							}
						}
						else
						{
							for (int j = 0; j < 2; ++j)
							{
								if (!vcr.pfxd.msk[j])
								{
									setVprFloat(m, i, s + j, applyPrefixVd(j, vr[j]));
								}
							}
						}
						vcr.pfxd.enabled = false;
					}
					else
					{
						if ((vd & 32) != 0)
						{
							for (int j = 0; j < 2; ++j)
							{
								setVprFloat(m, s + j, i, vr[j]);
							}
						}
						else
						{
							for (int j = 0; j < 2; ++j)
							{
								setVprFloat(m, i, s + j, vr[j]);
							}
						}
					}
					break;

				case 3:
					s = (vd & 64) >> 6;
					if (vcr.pfxd.enabled)
					{
						if ((vd & 32) != 0)
						{
							for (int j = 0; j < 3; ++j)
							{
								if (!vcr.pfxd.msk[j])
								{
									setVprFloat(m, s + j, i, applyPrefixVd(j, vr[j]));
								}
							}
						}
						else
						{
							for (int j = 0; j < 3; ++j)
							{
								if (!vcr.pfxd.msk[j])
								{
									setVprFloat(m, i, s + j, applyPrefixVd(j, vr[j]));
								}
							}
						}
						vcr.pfxd.enabled = false;
					}
					else
					{
						if ((vd & 32) != 0)
						{
							for (int j = 0; j < 3; ++j)
							{
								setVprFloat(m, s + j, i, vr[j]);
							}
						}
						else
						{
							for (int j = 0; j < 3; ++j)
							{
								setVprFloat(m, i, s + j, vr[j]);
							}
						}
					}
					break;

				case 4:
					s = (vd & 64) >> 5;
					if (vcr.pfxd.enabled)
					{
						if ((vd & 32) != 0)
						{
							for (int j = 0; j < 4; ++j)
							{
								if (!vcr.pfxd.msk[j])
								{
									setVprFloat(m, (j + s) & 3, i, applyPrefixVd(j, vr[j]));
								}
							}
						}
						else
						{
							for (int j = 0; j < 4; ++j)
							{
								if (!vcr.pfxd.msk[j])
								{
									setVprFloat(m, i, (j + s) & 3, applyPrefixVd(j, vr[j]));
								}
							}
						}
						vcr.pfxd.enabled = false;
					}
					else
					{
						if ((vd & 32) != 0)
						{
							for (int j = 0; j < 4; ++j)
							{
								setVprFloat(m, (j + s) & 3, i, vr[j]);
							}
						}
						else
						{
							for (int j = 0; j < 4; ++j)
							{
								setVprFloat(m, i, (j + s) & 3, vr[j]);
							}
						}
					}
					break;

				default:
					break;
			}
		}

		public virtual void saveVdInt(int vsize, int vd, int[] vr)
		{
			int m, s, i;

			m = (vd >> 2) & 7;
			i = (vd >> 0) & 3;

			switch (vsize)
			{
				case 1:
					s = (vd >> 5) & 3;
					if (vcr.pfxd.enabled)
					{
						if (!vcr.pfxd.msk[0])
						{
							setVprInt(m, i, s, applyPrefixVdInt(0, vr[0]));
						}
						vcr.pfxd.enabled = false;
					}
					else
					{
						setVprInt(m, i, s, vr[0]);
					}
					break;

				case 2:
					s = (vd & 64) >> 5;
					if (vcr.pfxd.enabled)
					{
						if ((vd & 32) != 0)
						{
							for (int j = 0; j < 2; ++j)
							{
								if (!vcr.pfxd.msk[j])
								{
									setVprInt(m, s + j, i, applyPrefixVdInt(j, vr[j]));
								}
							}
						}
						else
						{
							for (int j = 0; j < 2; ++j)
							{
								if (!vcr.pfxd.msk[j])
								{
									setVprInt(m, i, s + j, applyPrefixVdInt(j, vr[j]));
								}
							}
						}
						vcr.pfxd.enabled = false;
					}
					else
					{
						if ((vd & 32) != 0)
						{
							for (int j = 0; j < 2; ++j)
							{
								setVprInt(m, s + j, i, vr[j]);
							}
						}
						else
						{
							for (int j = 0; j < 2; ++j)
							{
								setVprInt(m, i, s + j, vr[j]);
							}
						}
					}
					break;

				case 3:
					s = (vd & 64) >> 6;
					if (vcr.pfxd.enabled)
					{
						if ((vd & 32) != 0)
						{
							for (int j = 0; j < 3; ++j)
							{
								if (!vcr.pfxd.msk[j])
								{
									setVprInt(m, s + j, i, applyPrefixVdInt(j, vr[j]));
								}
							}
						}
						else
						{
							for (int j = 0; j < 3; ++j)
							{
								if (!vcr.pfxd.msk[j])
								{
									setVprInt(m, i, s + j, applyPrefixVdInt(j, vr[j]));
								}
							}
						}
						vcr.pfxd.enabled = false;
					}
					else
					{
						if ((vd & 32) != 0)
						{
							for (int j = 0; j < 3; ++j)
							{
								setVprInt(m, s + j, i, vr[j]);
							}
						}
						else
						{
							for (int j = 0; j < 3; ++j)
							{
								setVprInt(m, i, s + j, vr[j]);
							}
						}
					}
					break;

				case 4:
					s = (vd & 64) >> 5;
					if (vcr.pfxd.enabled)
					{
						if ((vd & 32) != 0)
						{
							for (int j = 0; j < 4; ++j)
							{
								if (!vcr.pfxd.msk[j])
								{
									setVprInt(m, (j + s) & 3, i, applyPrefixVdInt(j, vr[j]));
								}
							}
						}
						else
						{
							for (int j = 0; j < 4; ++j)
							{
								if (!vcr.pfxd.msk[j])
								{
									setVprInt(m, i, (j + s) & 3, applyPrefixVdInt(j, vr[j]));
								}
							}
						}
						vcr.pfxd.enabled = false;
					}
					else
					{
						if ((vd & 32) != 0)
						{
							for (int j = 0; j < 4; ++j)
							{
								setVprInt(m, (j + s) & 3, i, vr[j]);
							}
						}
						else
						{
							for (int j = 0; j < 4; ++j)
							{
								setVprInt(m, i, (j + s) & 3, vr[j]);
							}
						}
					}
					break;

				default:
					break;
			}
		}

		private void consumeVpfxt()
		{
			vcr.pfxt.enabled = false;
		}

		public static int halffloatToFloat(int imm16)
		{
			int s = (imm16 >> 15) & 0x00000001; // sign
			int e = (imm16 >> 10) & 0x0000001f; // exponent
			int f = (imm16 >> 0) & 0x000003ff; // fraction

			// need to handle 0x7C00 INF and 0xFC00 -INF?
			if (e == 0)
			{
				// need to handle +-0 case f==0 or f=0x8000?
				if (f == 0)
				{
					// Plus or minus zero
					return s << 31;
				}
				// Denormalized number -- renormalize it
				while ((f & 0x00000400) == 0)
				{
					f <<= 1;
					e -= 1;
				}
				e += 1;
				f &= ~0x00000400;
			}
			else if (e == 31)
			{
				if (f == 0)
				{
					// Inf
					return (s << 31) | 0x7f800000;
				}
				// NaN
				return (s << 31) | 0x7f800000 | f; // fraction is not shifted by PSP
			}

			e = e + (127 - 15);
			f = f << 13;

			return (s << 31) | (e << 23) | f;
		}

		internal virtual int floatToHalffloat(int i)
		{
			int s = ((i >> 16) & 0x00008000); // sign
			int e = ((i >> 23) & 0x000000ff) - (127 - 15); // exponent
			int f = ((i >> 0) & 0x007fffff); // fraction

			// need to handle NaNs and Inf?
			if (e <= 0)
			{
				if (e < -10)
				{
					if (s != 0)
					{
						// handle -0.0
						return 0x8000;
					}
					return 0;
				}
				f = (f | 0x00800000) >> (1 - e);
				return s | (f >> 13);
			}
			else if (e == 0xff - (127 - 15))
			{
				if (f == 0)
				{
					// Inf
					return s | 0x7c00;
				}
				// NAN
				f >>= 13;
				f = 0x3ff; // PSP always encodes NaN with this value
				return s | 0x7c00 | f | ((f == 0) ? 1 : 0);
			}
			if (e > 30)
			{
				// Overflow
				return s | 0x7c00;
			}
			return s | (e << 10) | (f >> 13);
		}

		// group VFPU0
		// VFPU0:VADD
		public virtual void doVADD(int vsize, int vd, int vs, int vt)
		{
			loadVs(vsize, vs);
			loadVt(vsize, vt);

			for (int i = 0; i < vsize; ++i)
			{
				v1[i] += v2[i];
			}

			saveVd(vsize, vd, v1);
		}

		// VFPU0:VSUB
		public virtual void doVSUB(int vsize, int vd, int vs, int vt)
		{
			loadVs(vsize, vs);
			loadVt(vsize, vt);

			for (int i = 0; i < vsize; ++i)
			{
				v1[i] -= v2[i];
			}

			saveVd(vsize, vd, v1);
		}

		// VFPU0:VSBN
		public virtual void doVSBN(int vsize, int vd, int vs, int vt)
		{
			if (vsize != 1)
			{
				doUNK("Only supported VSBN.S");
			}

			loadVs(1, vs);
			loadVtInt(1, vt);

			v1[0] = Math.scalb(v1[0], v2i[0]);

			saveVd(1, vd, v1);
		}

		// VFPU0:VDIV
		public virtual void doVDIV(int vsize, int vd, int vs, int vt)
		{
			loadVs(vsize, vs);
			loadVt(vsize, vt);

			for (int i = 0; i < vsize; ++i)
			{
				if (v1[i] == 0f && v2[i] == 0f)
				{
					// Return the PSP NaN value 0x7F800001
					// (which is different from the NaN value generated in Java).
					// Be careful to use the sign of the zero's (+0 / -0):
					// - +0/+0 and -0/-0 return NaN 0x7F800001
					// - +0/-0 and -0/+0 return NaN 0xFF800001
					int sign1 = Float.floatToRawIntBits(v1[i]) & unchecked((int)0x80000000);
					int sign2 = Float.floatToRawIntBits(v2[i]) & unchecked((int)0x80000000);
					v1[i] = Float.intBitsToFloat(pspNaNint | (sign1 ^ sign2));
				}
				else
				{
					v1[i] /= v2[i];
				}
			}

			saveVd(vsize, vd, v1);
		}

		// group VFPU1
		// VFPU1:VMUL
		public virtual void doVMUL(int vsize, int vd, int vs, int vt)
		{
			loadVs(vsize, vs);
			loadVt(vsize, vt);

			for (int i = 0; i < vsize; ++i)
			{
				v1[i] *= v2[i];
			}

			saveVd(vsize, vd, v1);
		}

		// VFPU1:VDOT
		public virtual void doVDOT(int vsize, int vd, int vs, int vt)
		{
			if (vsize == 1)
			{
				doUNK("Unsupported VDOT.S");
			}

			loadVs(vsize, vs);
			loadVt(vsize, vt);

			float dot = v1[0] * v2[0];

			for (int i = 1; i < vsize; ++i)
			{
				dot += v1[i] * v2[i];
			}

			v3[0] = dot;

			saveVd(1, vd, v3);
		}

		// VFPU1:VSCL
		public virtual void doVSCL(int vsize, int vd, int vs, int vt)
		{
			if (vsize == 1)
			{
				doUNK("Unsupported VSCL.S");
			}

			loadVs(vsize, vs);
			loadVt(1, vt);

			float scale = v2[0];

			for (int i = 0; i < vsize; ++i)
			{
				v1[i] *= scale;
			}

			saveVd(vsize, vd, v1);
		}

		// VFPU1:VHDP
		public virtual void doVHDP(int vsize, int vd, int vs, int vt)
		{
			if (vsize == 1)
			{
				doUNK("Unsupported VHDP.S");
			}

			loadVs(vsize, vs);
			loadVt(vsize, vt);

			float hdp = v1[0] * v2[0];

			int i;

			for (i = 1; i < vsize - 1; ++i)
			{
				hdp += v1[i] * v2[i];
			}

			// Tested: last element is only v2[i] (and not v1[i]*v2[i])
			v2[0] = hdp + v2[i];

			saveVd(1, vd, v2);
		}

		// VFPU1:VCRS
		public virtual void doVCRS(int vsize, int vd, int vs, int vt)
		{
			if (vsize != 3)
			{
				doUNK("Only supported VCRS.T");
			}

			loadVs(3, vs);
			loadVt(3, vt);

			v3[0] = v1[1] * v2[2];
			v3[1] = v1[2] * v2[0];
			v3[2] = v1[0] * v2[1];

			saveVd(3, vd, v3);
		}

		// VFPU1:VDET
		public virtual void doVDET(int vsize, int vd, int vs, int vt)
		{
			if (vsize != 2)
			{
				doUNK("Only supported VDET.P");
				return;
			}

			loadVs(2, vs);
			loadVt(2, vt);

			v1[0] = v1[0] * v2[1] - v1[1] * v2[0];

			saveVd(1, vd, v1);
		}

		// VFPU2

		// VFPU2:MFV
		public virtual void doMFV(int rt, int imm7)
		{
			int r = (imm7 >> 5) & 3;
			int m = (imm7 >> 2) & 7;
			int c = (imm7 >> 0) & 3;

			setRegister(rt, getVprInt(m, c, r));
		}

		// VFPU2:MFVC
		public virtual void doMFVC(int rt, int imm7)
		{
			if (rt != 0)
			{
				int value = 0;
				switch (imm7)
				{
					case 0: // 128
						value |= vcr.pfxs.swz[0] << 0;
						value |= vcr.pfxs.swz[1] << 2;
						value |= vcr.pfxs.swz[2] << 4;
						value |= vcr.pfxs.swz[3] << 6;
						if (vcr.pfxs.abs[0])
						{
							value |= 1 << 8;
						}
						if (vcr.pfxs.abs[1])
						{
							value |= 1 << 9;
						}
						if (vcr.pfxs.abs[2])
						{
							value |= 1 << 10;
						}
						if (vcr.pfxs.abs[3])
						{
							value |= 1 << 11;
						}
						if (vcr.pfxs.cst[0])
						{
							value |= 1 << 12;
						}
						if (vcr.pfxs.cst[1])
						{
							value |= 1 << 13;
						}
						if (vcr.pfxs.cst[2])
						{
							value |= 1 << 14;
						}
						if (vcr.pfxs.cst[3])
						{
							value |= 1 << 15;
						}
						if (vcr.pfxs.neg[0])
						{
							value |= 1 << 16;
						}
						if (vcr.pfxs.neg[1])
						{
							value |= 1 << 17;
						}
						if (vcr.pfxs.neg[2])
						{
							value |= 1 << 18;
						}
						if (vcr.pfxs.neg[3])
						{
							value |= 1 << 19;
						}
						setRegister(rt, value);
						break;
					case 1: // 129
						value |= vcr.pfxt.swz[0] << 0;
						value |= vcr.pfxt.swz[1] << 2;
						value |= vcr.pfxt.swz[2] << 4;
						value |= vcr.pfxt.swz[3] << 6;
						if (vcr.pfxt.abs[0])
						{
							value |= 1 << 8;
						}
						if (vcr.pfxt.abs[1])
						{
							value |= 1 << 9;
						}
						if (vcr.pfxt.abs[2])
						{
							value |= 1 << 10;
						}
						if (vcr.pfxt.abs[3])
						{
							value |= 1 << 11;
						}
						if (vcr.pfxt.cst[0])
						{
							value |= 1 << 12;
						}
						if (vcr.pfxt.cst[1])
						{
							value |= 1 << 13;
						}
						if (vcr.pfxt.cst[2])
						{
							value |= 1 << 14;
						}
						if (vcr.pfxt.cst[3])
						{
							value |= 1 << 15;
						}
						if (vcr.pfxt.neg[0])
						{
							value |= 1 << 16;
						}
						if (vcr.pfxt.neg[1])
						{
							value |= 1 << 17;
						}
						if (vcr.pfxt.neg[2])
						{
							value |= 1 << 18;
						}
						if (vcr.pfxt.neg[3])
						{
							value |= 1 << 19;
						}
						setRegister(rt, value);
						break;
					case 2: // 130
						value |= vcr.pfxd.sat[0] << 0;
						value |= vcr.pfxd.sat[1] << 2;
						value |= vcr.pfxd.sat[2] << 4;
						value |= vcr.pfxd.sat[3] << 6;
						if (vcr.pfxd.msk[0])
						{
							value |= 1 << 8;
						}
						if (vcr.pfxd.msk[1])
						{
							value |= 1 << 9;
						}
						if (vcr.pfxd.msk[2])
						{
							value |= 1 << 10;
						}
						if (vcr.pfxd.msk[3])
						{
							value |= 1 << 11;
						}
						setRegister(rt, value);
						break;
					case 3: // 131
						for (int i = vcr.cc.Length - 1; i >= 0; i--)
						{
							value <<= 1;
							if (vcr.cc[i])
							{
								value |= 1;
							}
						}
						setRegister(rt, value);
						break;
					case 8: // 136 - RCX0
						setRegister(rt, rnd.Seed);
						break;
					case 9: // 137 - RCX1
					case 10: // 138 - RCX2
					case 11: // 139 - RCX3
					case 12: // 140 - RCX4
					case 13: // 141 - RCX5
					case 14: // 142 - RCX6
					case 15: // 143 - RCX7
						// as we do not know how VFPU generates a random number through those 8 registers, we ignore 7 of them
						setRegister(rt, 0x3f800000);
						break;
					default:
						// These values are not supported in pspsharp
						doUNK(string.Format("Unimplemented mfvc (rt={0}, imm7={1:D})", Common.gprNames[rt], imm7));
						break;
				}
			}
		}

		// VFPU2:MTV
		public virtual void doMTV(int rt, int imm7)
		{
			int r = (imm7 >> 5) & 3;
			int m = (imm7 >> 2) & 7;
			int c = (imm7 >> 0) & 3;

			setVprInt(m, c, r, getRegister(rt));
		}

		// VFPU2:MTVC
		public virtual void doMTVC(int rt, int imm7)
		{
			int value = getRegister(rt);

			switch (imm7)
			{
				case 0: // 128
					vcr.pfxs.swz[0] = ((value >> 0) & 3);
					vcr.pfxs.swz[1] = ((value >> 2) & 3);
					vcr.pfxs.swz[2] = ((value >> 4) & 3);
					vcr.pfxs.swz[3] = ((value >> 6) & 3);
					vcr.pfxs.abs[0] = ((value >> 8) & 1) == 1;
					vcr.pfxs.abs[1] = ((value >> 9) & 1) == 1;
					vcr.pfxs.abs[2] = ((value >> 10) & 1) == 1;
					vcr.pfxs.abs[3] = ((value >> 11) & 1) == 1;
					vcr.pfxs.cst[0] = ((value >> 12) & 1) == 1;
					vcr.pfxs.cst[1] = ((value >> 13) & 1) == 1;
					vcr.pfxs.cst[2] = ((value >> 14) & 1) == 1;
					vcr.pfxs.cst[3] = ((value >> 15) & 1) == 1;
					vcr.pfxs.neg[0] = ((value >> 16) & 1) == 1;
					vcr.pfxs.neg[1] = ((value >> 17) & 1) == 1;
					vcr.pfxs.neg[2] = ((value >> 18) & 1) == 1;
					vcr.pfxs.neg[3] = ((value >> 19) & 1) == 1;
					vcr.pfxs.enabled = true;
					break;
				case 1: // 129
					vcr.pfxt.swz[0] = ((value >> 0) & 3);
					vcr.pfxt.swz[1] = ((value >> 2) & 3);
					vcr.pfxt.swz[2] = ((value >> 4) & 3);
					vcr.pfxt.swz[3] = ((value >> 6) & 3);
					vcr.pfxt.abs[0] = ((value >> 8) & 1) == 1;
					vcr.pfxt.abs[1] = ((value >> 9) & 1) == 1;
					vcr.pfxt.abs[2] = ((value >> 10) & 1) == 1;
					vcr.pfxt.abs[3] = ((value >> 11) & 1) == 1;
					vcr.pfxt.cst[0] = ((value >> 12) & 1) == 1;
					vcr.pfxt.cst[1] = ((value >> 13) & 1) == 1;
					vcr.pfxt.cst[2] = ((value >> 14) & 1) == 1;
					vcr.pfxt.cst[3] = ((value >> 15) & 1) == 1;
					vcr.pfxt.neg[0] = ((value >> 16) & 1) == 1;
					vcr.pfxt.neg[1] = ((value >> 17) & 1) == 1;
					vcr.pfxt.neg[2] = ((value >> 18) & 1) == 1;
					vcr.pfxt.neg[3] = ((value >> 19) & 1) == 1;
					vcr.pfxt.enabled = true;
					break;
				case 2: // 130
					vcr.pfxd.sat[0] = ((value >> 0) & 3);
					vcr.pfxd.sat[1] = ((value >> 2) & 3);
					vcr.pfxd.sat[2] = ((value >> 4) & 3);
					vcr.pfxd.sat[3] = ((value >> 6) & 3);
					vcr.pfxd.msk[0] = ((value >> 8) & 1) == 1;
					vcr.pfxd.msk[1] = ((value >> 9) & 1) == 1;
					vcr.pfxd.msk[2] = ((value >> 10) & 1) == 1;
					vcr.pfxd.msk[3] = ((value >> 11) & 1) == 1;
					vcr.pfxd.enabled = true;
					break;
				case 3: // 131
					for (int i = 0; i < vcr.cc.Length; i++)
					{
						vcr.cc[i] = (value & 1) != 0;
						value = (int)((uint)value >> 1);
					}
					break;
				case 4:
					// The value 0 is used by the PSP reboot code, other values are unknown
					if (value != 0)
					{
						doUNK(string.Format("Unimplemented mtvc (rt={0}, imm7={1:D}, value=0x{2:X})", Common.gprNames[rt], imm7, value));
					}
					break;
				case 8: // 136 - RCX0
					rnd.Seed = value;
					break;
				case 9: // 137 - RCX1
				case 10: // 138 - RCX2
				case 11: // 139 - RCX3
				case 12: // 140 - RCX4
				case 13: // 141 - RCX5
				case 14: // 142 - RCX6
				case 15: // 143 - RCX7
					// as we do not know how VFPU generates a random number through those 8 registers, we ignore 7 of them
					break;
				default:
					// These values are not supported in pspsharp
					doUNK(string.Format("Unimplemented mtvc (rt={0}, imm7={1:D}, value=0x{2:X})", Common.gprNames[rt], imm7, value));
					break;
			}
		}

		// VFPU2:BVF
		public virtual bool doBVF(int imm3, int simm16)
		{
			npc = (!vcr.cc[imm3]) ? branchTarget(pc, simm16) : (pc + 4);
			return true;
		}
		// VFPU2:BVT
		public virtual bool doBVT(int imm3, int simm16)
		{
			npc = (vcr.cc[imm3]) ? branchTarget(pc, simm16) : (pc + 4);
			return true;
		}
		// VFPU2:BVFL
		public virtual bool doBVFL(int imm3, int simm16)
		{
			if (!vcr.cc[imm3])
			{
				npc = branchTarget(pc, simm16);
				return true;
			}
			pc = pc + 4;
			return false;
		}
		// VFPU2:BVTL
		public virtual bool doBVTL(int imm3, int simm16)
		{
			if (vcr.cc[imm3])
			{
				npc = branchTarget(pc, simm16);
				return true;
			}
			pc = pc + 4;
			return false;
		}
		// group VFPU3

		// VFPU3:VCMP
		public virtual void doVCMP(int vsize, int vs, int vt, int cond)
		{
			bool cc_or = false;
			bool cc_and = true;

			if ((cond & 8) == 0)
			{
				bool not = ((cond & 4) == 4);

				bool cc = false;

				loadVs(vsize, vs);
				loadVt(vsize, vt);

				for (int i = 0; i < vsize; ++i)
				{
					switch (cond & 3)
					{
						case 0:
							cc = not;
							break;
						case 1:
							cc = not ? (v1[i] != v2[i]) : (v1[i] == v2[i]);
							break;
						case 2:
							if (isNaN(v1[i]) || isNaN(v2[i]))
							{
								cc = false;
							}
							else
							{
								cc = not ? (v1[i] >= v2[i]) : (v1[i] < v2[i]);
							}
							break;
						case 3:
							if (isNaN(v1[i]) || isNaN(v2[i]))
							{
								cc = false;
							}
							else
							{
								cc = not ? (v1[i] > v2[i]) : (v1[i] <= v2[i]);
							}
							break;
					}

					vcr.cc[i] = cc;
					cc_or = cc_or || cc;
					cc_and = cc_and && cc;
				}

			}
			else
			{
				loadVs(vsize, vs);

				bool not = ((cond & 4) == 4);
				bool cc = false;
				for (int i = 0; i < vsize; ++i)
				{
					switch (cond & 3)
					{
						// Positive or negative 0
						case 0:
							cc = (abs(v1[i]) == 0f);
							break;
						// NaN
						case 1:
							cc = isNaN(v1[i]);
							break;
						// Positive or negative infinity
						case 2:
							cc = isInfinite(v1[i]);
							break;
						// NaN or positive or negative infinity
						case 3:
							cc = isNaN(abs(v1[i])) || isInfinite(v1[i]);
							break;
					}
					if (not)
					{
						cc = !cc;
					}
					vcr.cc[i] = cc;
					cc_or = cc_or || cc;
					cc_and = cc_and && cc;
				}

			}
			vcr.cc[4] = cc_or;
			vcr.cc[5] = cc_and;
		}

		// VFPU3:VMIN
		public virtual void doVMIN(int vsize, int vd, int vs, int vt)
		{
			loadVs(vsize, vs);
			loadVt(vsize, vt);

			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = System.Math.Min(v1[i], v2[i]);
			}

			saveVd(vsize, vd, v3);
		}

		// VFPU3:VMAX
		public virtual void doVMAX(int vsize, int vd, int vs, int vt)
		{
			loadVs(vsize, vs);
			loadVt(vsize, vt);

			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = System.Math.Max(v1[i], v2[i]);
			}

			saveVd(vsize, vd, v3);
		}

		// VFPU3:VSCMP
		public virtual void doVSCMP(int vsize, int vd, int vs, int vt)
		{
			loadVs(vsize, vs);
			loadVt(vsize, vt);

			for (int i = 0; i < vsize; ++i)
			{
				if (float.IsInfinity(v1[i]) && float.IsInfinity(v2[i]))
				{
					if (v1[i] == v2[i])
					{
						// +Inf and +Inf
						// -Inf and -Inf
						v3[i] = 0f;
					}
					else
					{
						// +Inf and -Inf
						// -Inf and +Inf
						v3[i] = System.Math.Sign(v1[i]);
					}
				}
				else
				{
					v3[i] = System.Math.Sign(v1[i] - v2[i]);
				}
			}

			saveVd(vsize, vd, v3);
		}

		// VFPU3:VSGE
		public virtual void doVSGE(int vsize, int vd, int vs, int vt)
		{
			loadVs(vsize, vs);
			loadVt(vsize, vt);

			for (int i = 0; i < vsize; ++i)
			{
				if (isNaN(v1[i]) || isNaN(v2[i]))
				{
					v3[i] = 0f;
				}
				else
				{
					v3[i] = (v1[i] >= v2[i]) ? 1f : 0f;
				}
			}

			saveVd(vsize, vd, v3);
		}

		// VFPU3:VSLT
		public virtual void doVSLT(int vsize, int vd, int vs, int vt)
		{
			loadVs(vsize, vs);
			loadVt(vsize, vt);

			for (int i = 0; i < vsize; ++i)
			{
				if (isNaN(v1[i]) || isNaN(v2[i]))
				{
					v3[i] = 0f;
				}
				else
				{
					v3[i] = (v1[i] < v2[i]) ? 1f : 0f;
				}
			}

			saveVd(vsize, vd, v3);
		}

		// group VFPU4
		// VFPU4:VMOV
		public virtual void doVMOV(int vsize, int vd, int vs)
		{
			loadVsInt(vsize, vs);
			saveVdInt(vsize, vd, v1i);

			// VMOV consumes VPFXT prefix.
			consumeVpfxt();
		}

		// VFPU4:VABS
		public virtual void doVABS(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = System.Math.Abs(v1[i]);
			}
			saveVd(vsize, vd, v3);
		}

		// VFPU4:VNEG
		public virtual void doVNEG(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = 0.0f - v1[i];
			}
			saveVd(vsize, vd, v3);
		}

		// VFPU4:VIDT
		public virtual void doVIDT(int vsize, int vd)
		{
			int id = vd % vsize;
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = (id == i) ? 1.0f : 0.0f;
			}
			saveVd(vsize, vd, v3);
		}

		// VFPU4:VSAT0
		public virtual void doVSAT0(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				if (isNaN(v1[i]))
				{
					v3[i] = v1[i];
				}
				else
				{
					v3[i] = min(max(0f, v1[i]), 1f);
				}
			}
			saveVd(vsize, vd, v3);
		}

		// VFPU4:VSAT1
		public virtual void doVSAT1(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				if (isNaN(v1[i]))
				{
					v3[i] = v1[i];
				}
				else
				{
					v3[i] = min(max(-1f, v1[i]), 1f);
				}
			}
			saveVd(vsize, vd, v3);
		}

		// VFPU4:VZERO
		public virtual void doVZERO(int vsize, int vd)
		{
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = 0.0f;
			}
			saveVd(vsize, vd, v3);
		}
		// VFPU4:VONE
		public virtual void doVONE(int vsize, int vd)
		{
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = 1.0f;
			}
			saveVd(vsize, vd, v3);
		}
		// VFPU4:VRCP
		public virtual void doVRCP(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = 1.0f / v1[i];
			}
			saveVd(vsize, vd, v3);
		}
		// VFPU4:VRSQ
		public virtual void doVRSQ(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = (float)(1.0 / System.Math.Sqrt(v1[i]));
			}
			saveVd(vsize, vd, v3);
		}
		// VFPU4:VSIN
		public virtual void doVSIN(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				float angle = v1[i];
				// Reducing the angle to [0..4[
				angle -= ((float) System.Math.Floor(angle * 0.25f)) * 4f;
				// Handling of specific values first to avoid precision loss in float value
				if (angle == 0f || angle == 2f)
				{
					v3[i] = 0f;
				}
				else if (angle == 1f)
				{
					v3[i] = 1f;
				}
				else if (angle == 3f)
				{
					v3[i] = -1f;
				}
				else
				{
					v3[i] = (float) System.Math.Sin(PI_2 * angle);
				}
			}
			saveVd(vsize, vd, v3);
		}
		// VFPU4:VCOS
		public virtual void doVCOS(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				float angle = v1[i];
				// Reducing the angle to [0..4[
				angle -= ((float) System.Math.Floor(angle * 0.25f)) * 4f;
				// Handling of specific values first to avoid precision loss in float value
				if (angle == 1f || angle == 3f)
				{
					v3[i] = 0f;
				}
				else if (angle == 0f)
				{
					v3[i] = 1f;
				}
				else if (angle == 2f)
				{
					v3[i] = -1f;
				}
				else
				{
					v3[i] = (float) System.Math.Cos(PI_2 * angle);
				}
			}
			saveVd(vsize, vd, v3);
		}
		// VFPU4:VEXP2
		public virtual void doVEXP2(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = (float) System.Math.Pow(2.0, v1[i]);
			}
			saveVd(vsize, vd, v3);
		}
		// VFPU4:VLOG2
		public virtual void doVLOG2(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = (float)(System.Math.Log(v1[i]) / System.Math.Log(2.0));
			}
			saveVd(vsize, vd, v3);
		}

		// VFPU4:VSQRT
		public virtual void doVSQRT(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = (float)(System.Math.Sqrt(v1[i]));
			}
			saveVd(vsize, vd, v3);
		}

		// VFPU4:VASIN
		public virtual void doVASIN(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = ((float) System.Math.Asin(v1[i])) / PI_2;
			}
			saveVd(vsize, vd, v3);
		}

		// VFPU4:VNRCP
		public virtual void doVNRCP(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = -1f / v1[i];
			}
			saveVd(vsize, vd, v3);
		}

		// VFPU4:VNSIN
		public virtual void doVNSIN(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = - (float) System.Math.Sin(PI_2 * v1[i]);
			}
			saveVd(vsize, vd, v3);
		}
		// VFPU4:VREXP2
		public virtual void doVREXP2(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = (float)(1.0 / System.Math.Pow(2.0, v1[i]));
			}
			saveVd(vsize, vd, v3);
		}
		// VFPU4:VRNDS
		public virtual void doVRNDS(int vsize, int vs)
		{
			// temporary solution
			if (vsize != 1)
			{
				doUNK("Only supported VRNDS.S");
				return;
			}

			loadVsInt(1, vs);
			rnd.Seed = v1i[0];
		}
		// VFPU4:VRNDI
		public virtual void doVRNDI(int vsize, int vd)
		{
			// temporary solution
			for (int i = 0; i < vsize; ++i)
			{
				v3i[i] = rnd.Next();
			}
			saveVdInt(vsize, vd, v3i);
		}
		// VFPU4:VRNDF1
		public virtual void doVRNDF1(int vsize, int vd)
		{
			// temporary solution
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = 1.0f + rnd.nextFloat();
			}
			saveVd(vsize, vd, v3);
		}
		// VFPU4:VRNDF2
		public virtual void doVRNDF2(int vsize, int vd)
		{
			// temporary solution
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = (1.0f + rnd.nextFloat()) * 2.0f;
			}
			saveVd(vsize, vd, v3);
		}
		// VFPU4:VF2H
		public virtual void doVF2H(int vsize, int vd, int vs)
		{
			if ((vsize & 1) == 1)
			{
				doUNK("Only supported VF2H.P or VF2H.Q");
				return;
			}
			loadVsInt(vsize, vs);
			for (int i = 0; i < vsize; i += 2)
			{
				v3i[i >> 1] = (floatToHalffloat(v1i[i + 1]) << 16) | floatToHalffloat(v1i[i]);
			}
			saveVdInt(vsize >> 1, vd, v3i);
		}
		// VFPU4:VH2F
		public virtual void doVH2F(int vsize, int vd, int vs)
		{
			if (vsize > 2)
			{
				doUNK("Only supported VH2F.S or VH2F.P");
				return;
			}
			loadVsInt(vsize, vs);
			for (int i = 0, j = 0; i < vsize; ++i, j += 2)
			{
				int imm32 = v1i[i];
				v3i[j] = halffloatToFloat(imm32 & 0xFFFF);
				v3i[j + 1] = halffloatToFloat((int)((uint)imm32 >> 16));
			}
			saveVdInt(vsize << 1, vd, v3i);
		}
		// VFPU4:VSBZ
		public virtual void doVSBZ(int vsize, int vd, int vs)
		{
			doUNK("Unimplemented VSBZ");
		}
		// VFPU4:VLGB
		public virtual void doVLGB(int vsize, int vd, int vs)
		{
			doUNK("Unimplemented VLGB");
		}
		// VFPU4:VUC2I
		public virtual void doVUC2I(int vsize, int vd, int vs)
		{
			if (vsize != 1)
			{
				doUNK("Only supported VUC2I.S");
				// PSP is ignoring the vsize
			}
			loadVsInt(1, vs);
			int n = v1i[0];
			// Performs pseudo-full-scale conversion
			v3i[0] = (int)((uint)(((n) & 0xFF) * 0x01010101) >> 1);
			v3i[1] = (int)((uint)(((n >> 8) & 0xFF) * 0x01010101) >> 1);
			v3i[2] = (int)((uint)(((n >> 16) & 0xFF) * 0x01010101) >> 1);
			v3i[3] = (int)((uint)(((n >> 24) & 0xFF) * 0x01010101) >> 1);
			saveVdInt(4, vd, v3i);
		}
		// VFPU4:VC2I
		public virtual void doVC2I(int vsize, int vd, int vs)
		{
			if (vsize != 1)
			{
				doUNK("Only supported VC2I.S");
				// PSP is ignoring the vsize
			}
			loadVsInt(1, vs);
			int n = v1i[0];
			v3i[0] = ((n) & 0xFF) << 24;
			v3i[1] = ((n >> 8) & 0xFF) << 24;
			v3i[2] = ((n >> 16) & 0xFF) << 24;
			v3i[3] = ((n >> 24) & 0xFF) << 24;
			saveVdInt(4, vd, v3i);
		}
		// VFPU4:VUS2I
		public virtual void doVUS2I(int vsize, int vd, int vs)
		{
			if (vsize > 2)
			{
				doUNK("Only supported VUS2I.S or VUS2I.P");
				return;
			}
			loadVsInt(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				int imm32 = v1i[i];
				v3i[0 + 2 * i] = ((imm32) & 0xFFFF) << 15;
				v3i[1 + 2 * i] = (((int)((uint)imm32 >> 16)) & 0xFFFF) << 15;
			}
			saveVdInt(vsize * 2, vd, v3i);
		}
		// VFPU4:VS2I
		public virtual void doVS2I(int vsize, int vd, int vs)
		{
			if (vsize > 2)
			{
				doUNK("Only supported VS2I.S or VS2I.P");
				return;
			}
			loadVsInt(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				int imm32 = v1i[i];
				v3i[0 + 2 * i] = ((imm32) & 0xFFFF) << 16;
				v3i[1 + 2 * i] = (((int)((uint)imm32 >> 16)) & 0xFFFF) << 16;
			}
			saveVdInt(vsize * 2, vd, v3i);
		}

		// VFPU4:VI2UC
		public virtual void doVI2UC(int vsize, int vd, int vs)
		{
			if (vsize != 4)
			{
				doUNK("Only supported VI2UC.Q");
				return;
			}

			loadVsInt(4, vs);

			int x = v1i[0];
			int y = v1i[1];
			int z = v1i[2];
			int w = v1i[3];

			v3i[0] = ((x < 0) ? 0 : ((x >> 23) << 0)) | ((y < 0) ? 0 : ((y >> 23) << 8)) | ((z < 0) ? 0 : ((z >> 23) << 16)) | ((w < 0) ? 0 : ((w >> 23) << 24));

			saveVdInt(1, vd, v3i);
		}

		// VFPU4:VI2C
		public virtual void doVI2C(int vsize, int vd, int vs)
		{
			if (vsize != 4)
			{
				doUNK("Only supported VI2C.Q");
				return;
			}

			loadVsInt(4, vs);

			int x = v1i[0];
			int y = v1i[1];
			int z = v1i[2];
			int w = v1i[3];

			v3i[0] = (((int)((uint)x >> 24)) << 0) | (((int)((uint)y >> 24)) << 8) | (((int)((uint)z >> 24)) << 16) | (((int)((uint)w >> 24)) << 24);

			saveVdInt(1, vd, v3i);
		}
		// VFPU4:VI2US
		public virtual void doVI2US(int vsize, int vd, int vs)
		{
			if ((vsize & 1) != 0)
			{
				doUNK("Only supported VI2US.P and VI2US.Q");
				return;
			}

			loadVsInt(vsize, vs);

			int x = v1i[0];
			int y = v1i[1];

			v3i[0] = ((x < 0) ? 0 : ((x >> 15) << 0)) | ((y < 0) ? 0 : ((y >> 15) << 16));

			if (vsize == 4)
			{
				int z = v1i[2];
				int w = v1i[3];

				v3i[1] = ((z < 0) ? 0 : ((z >> 15) << 0)) | ((w < 0) ? 0 : ((w >> 15) << 16));
				saveVdInt(2, vd, v3i);
			}
			else
			{
				saveVdInt(1, vd, v3i);
			}
		}
		// VFPU4:VI2S
		public virtual void doVI2S(int vsize, int vd, int vs)
		{
			if ((vsize & 1) != 0)
			{
				doUNK("Only supported VI2S.P and VI2S.Q");
				return;
			}

			loadVsInt(vsize, vs);

			int x = v1i[0];
			int y = v1i[1];

			v3i[0] = (((int)((uint)x >> 16)) << 0) | (((int)((uint)y >> 16)) << 16);

			if (vsize == 4)
			{
				int z = v1i[2];
				int w = v1i[3];

				v3i[1] = (((int)((uint)z >> 16)) << 0) | (((int)((uint)w >> 16)) << 16);
				saveVdInt(2, vd, v3i);
			}
			else
			{
				saveVdInt(1, vd, v3i);
			}
		}
		// VFPU4:VSRT1
		public virtual void doVSRT1(int vsize, int vd, int vs)
		{
			if (vsize != 4)
			{
				doUNK("Only supported VSRT1.Q");
				return;
			}

			loadVs(4, vs);
			float x = v1[0];
			float y = v1[1];
			float z = v1[2];
			float w = v1[3];
			v3[0] = System.Math.Min(x, y);
			v3[1] = System.Math.Max(x, y);
			v3[2] = System.Math.Min(z, w);
			v3[3] = System.Math.Max(z, w);
			saveVd(4, vd, v3);
		}
		// VFPU4:VSRT2
		public virtual void doVSRT2(int vsize, int vd, int vs)
		{
			if (vsize != 4)
			{
				doUNK("Only supported VSRT2.Q");
				return;
			}

			loadVs(4, vs);
			float x = v1[0];
			float y = v1[1];
			float z = v1[2];
			float w = v1[3];
			v3[0] = System.Math.Min(x, w);
			v3[1] = System.Math.Min(y, z);
			v3[2] = System.Math.Max(y, z);
			v3[3] = System.Math.Max(x, w);
			saveVd(4, vd, v3);
		}
		// VFPU4:VBFY1
		public virtual void doVBFY1(int vsize, int vd, int vs)
		{
			if ((vsize & 1) == 1)
			{
				doUNK("Only supported VBFY1.P or VBFY1.Q");
				return;
			}

			loadVs(vsize, vs);
			float x = v1[0];
			float y = v1[1];
			v3[0] = x + y;
			v3[1] = x - y;
			if (vsize > 2)
			{
				float z = v1[2];
				float w = v1[3];
				v3[2] = z + w;
				v3[3] = z - w;
				saveVd(4, vd, v3);
			}
			else
			{
				saveVd(2, vd, v3);
			}
		}
		// VFPU4:VBFY2
		public virtual void doVBFY2(int vsize, int vd, int vs)
		{
			if (vsize != 4)
			{
				doUNK("Only supported VBFY2.Q");
				return;
			}

			loadVs(vsize, vs);
			float x = v1[0];
			float y = v1[1];
			float z = v1[2];
			float w = v1[3];
			v3[0] = x + z;
			v3[1] = y + w;
			v3[2] = x - z;
			v3[3] = y - w;
			saveVd(4, vd, v3);
		}
		// VFPU4:VOCP
		public virtual void doVOCP(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);

			for (int i = 0; i < vsize; ++i)
			{
				v1[i] = 1f - v1[i];
			}

			saveVd(vsize, vd, v1);
		}
		// VFPU4:VSOCP
		public virtual void doVSOCP(int vsize, int vd, int vs)
		{
			if (vsize > 2)
			{
				doUNK("Only supported VSOCP.S or VSOCP.P");
				return;
			}

			loadVs(vsize, vs);
			float x = v1[0];
			v3[0] = System.Math.Min(System.Math.Max(0f, 1f - x), 1f);
			v3[1] = System.Math.Min(System.Math.Max(0f, x), 1f);
			if (vsize > 1)
			{
				float y = v1[1];
				v3[2] = System.Math.Min(System.Math.Max(0f, 1f - y), 1f);
				v3[3] = System.Math.Min(System.Math.Max(0f, y), 1f);
				saveVd(4, vd, v3);
			}
			else
			{
				saveVd(2, vd, v3);
			}
		}
		// VFPU4:VFAD
		public virtual void doVFAD(int vsize, int vd, int vs)
		{
			if (vsize == 1)
			{
				doUNK("Unsupported VFAD.S");
				return;
			}

			loadVs(vsize, vs);

			for (int i = 1; i < vsize; ++i)
			{
				v1[0] += v1[i];
			}

			saveVd(1, vd, v1);
		}
		// VFPU4:VAVG
		public virtual void doVAVG(int vsize, int vd, int vs)
		{
			if (vsize == 1)
			{
				doUNK("Unsupported VAVG.S");
				return;
			}

			loadVs(vsize, vs);

			v1[0] /= vsize;
			for (int i = 1; i < vsize; ++i)
			{
				v1[0] += v1[i] / vsize;
			}

			saveVd(1, vd, v1);
		}
		// VFPU4:VSRT3
		public virtual void doVSRT3(int vsize, int vd, int vs)
		{
			if (vsize != 4)
			{
				doUNK("Only supported VSRT3.Q (vsize=" + vsize + ")");
				// The instruction is somehow supported on the PSP (see VfpuTest),
				// but leave the error message here to help debugging the Decoder.
				return;
			}

			loadVs(4, vs);
			float x = v1[0];
			float y = v1[1];
			float z = v1[2];
			float w = v1[3];
			v3[0] = System.Math.Max(x, y);
			v3[1] = System.Math.Min(x, y);
			v3[2] = System.Math.Max(z, w);
			v3[3] = System.Math.Min(z, w);
			saveVd(4, vd, v3);
		}
		// VFPU4:VSGN
		public virtual void doVSGN(int vsize, int vd, int vs)
		{
			loadVs(vsize, vs);
			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = System.Math.Sign(v1[i]);
			}
			saveVd(vsize, vd, v3);
		}
		// VFPU4:VSRT4
		public virtual void doVSRT4(int vsize, int vd, int vs)
		{
			if (vsize != 4)
			{
				doUNK("Only supported VSRT4.Q");
				return;
			}

			loadVs(4, vs);
			float x = v1[0];
			float y = v1[1];
			float z = v1[2];
			float w = v1[3];
			v3[0] = System.Math.Max(x, w);
			v3[1] = System.Math.Max(y, z);
			v3[2] = System.Math.Min(y, z);
			v3[3] = System.Math.Min(x, w);
			saveVd(4, vd, v3);
		}
		// VFPU4:VMFVC
		public virtual void doVMFVC(int vd, int imm7)
		{
			int value = 0;
			switch (imm7)
			{
				case 0: // 128
					value |= vcr.pfxs.swz[0] << 0;
					value |= vcr.pfxs.swz[1] << 2;
					value |= vcr.pfxs.swz[2] << 4;
					value |= vcr.pfxs.swz[3] << 6;
					if (vcr.pfxs.abs[0])
					{
						value |= 1 << 8;
					}
					if (vcr.pfxs.abs[1])
					{
						value |= 1 << 9;
					}
					if (vcr.pfxs.abs[2])
					{
						value |= 1 << 10;
					}
					if (vcr.pfxs.abs[3])
					{
						value |= 1 << 11;
					}
					if (vcr.pfxs.cst[0])
					{
						value |= 1 << 12;
					}
					if (vcr.pfxs.cst[1])
					{
						value |= 1 << 13;
					}
					if (vcr.pfxs.cst[2])
					{
						value |= 1 << 14;
					}
					if (vcr.pfxs.cst[3])
					{
						value |= 1 << 15;
					}
					if (vcr.pfxs.neg[0])
					{
						value |= 1 << 16;
					}
					if (vcr.pfxs.neg[1])
					{
						value |= 1 << 17;
					}
					if (vcr.pfxs.neg[2])
					{
						value |= 1 << 18;
					}
					if (vcr.pfxs.neg[3])
					{
						value |= 1 << 19;
					}
					v3i[0] = value;
					saveVdInt(1, vd, v3i);
					break;
				case 1: // 129
					value |= vcr.pfxt.swz[0] << 0;
					value |= vcr.pfxt.swz[1] << 2;
					value |= vcr.pfxt.swz[2] << 4;
					value |= vcr.pfxt.swz[3] << 6;
					if (vcr.pfxt.abs[0])
					{
						value |= 1 << 8;
					}
					if (vcr.pfxt.abs[1])
					{
						value |= 1 << 9;
					}
					if (vcr.pfxt.abs[2])
					{
						value |= 1 << 10;
					}
					if (vcr.pfxt.abs[3])
					{
						value |= 1 << 11;
					}
					if (vcr.pfxt.cst[0])
					{
						value |= 1 << 12;
					}
					if (vcr.pfxt.cst[1])
					{
						value |= 1 << 13;
					}
					if (vcr.pfxt.cst[2])
					{
						value |= 1 << 14;
					}
					if (vcr.pfxt.cst[3])
					{
						value |= 1 << 15;
					}
					if (vcr.pfxt.neg[0])
					{
						value |= 1 << 16;
					}
					if (vcr.pfxt.neg[1])
					{
						value |= 1 << 17;
					}
					if (vcr.pfxt.neg[2])
					{
						value |= 1 << 18;
					}
					if (vcr.pfxt.neg[3])
					{
						value |= 1 << 19;
					}
					v3i[0] = value;
					saveVdInt(1, vd, v3i);
					break;
				case 2: // 130
					value |= vcr.pfxd.sat[0] << 0;
					value |= vcr.pfxd.sat[1] << 2;
					value |= vcr.pfxd.sat[2] << 4;
					value |= vcr.pfxd.sat[3] << 6;
					if (vcr.pfxd.msk[0])
					{
						value |= 1 << 8;
					}
					if (vcr.pfxd.msk[1])
					{
						value |= 1 << 9;
					}
					if (vcr.pfxd.msk[2])
					{
						value |= 1 << 10;
					}
					if (vcr.pfxd.msk[3])
					{
						value |= 1 << 11;
					}
					v3i[0] = value;
					saveVdInt(1, vd, v3i);
					break;
				case 3: // 131
					for (int i = vcr.cc.Length - 1; i >= 0; i--)
					{
						value <<= 1;
						if (vcr.cc[i])
						{
							value |= 1;
						}
					}
					v3i[0] = value;
					saveVdInt(1, vd, v3i);
					break;
				case 8: // 136 - RCX0
					v3i[0] = rnd.Seed;
					saveVdInt(1, vd, v3i);
					break;
				case 9: // 137 - RCX1
				case 10: // 138 - RCX2
				case 11: // 139 - RCX3
				case 12: // 140 - RCX4
				case 13: // 141 - RCX5
				case 14: // 142 - RCX6
				case 15: // 143 - RCX7
					// as we do not know how VFPU generates a random number through those 8 registers, we ignore 7 of them
					v3i[0] = 0x3f800000;
					saveVdInt(1, vd, v3i);
					break;
				default:
					// These values are not supported in pspsharp
					doUNK("Unimplemented VMFVC (vd=" + vd + ", imm7=" + imm7 + ")");
					break;
			}
		}
		// VFPU4:VMTVC
		public virtual void doVMTVC(int vd, int imm7)
		{
			loadVdInt(1, vd);
			int value = v1i[0];

			switch (imm7)
			{
				case 0: // 128
					vcr.pfxs.swz[0] = ((value >> 0) & 3);
					vcr.pfxs.swz[1] = ((value >> 2) & 3);
					vcr.pfxs.swz[2] = ((value >> 4) & 3);
					vcr.pfxs.swz[3] = ((value >> 6) & 3);
					vcr.pfxs.abs[0] = ((value >> 8) & 1) == 1;
					vcr.pfxs.abs[1] = ((value >> 9) & 1) == 1;
					vcr.pfxs.abs[2] = ((value >> 10) & 1) == 1;
					vcr.pfxs.abs[3] = ((value >> 11) & 1) == 1;
					vcr.pfxs.cst[0] = ((value >> 12) & 1) == 1;
					vcr.pfxs.cst[1] = ((value >> 13) & 1) == 1;
					vcr.pfxs.cst[2] = ((value >> 14) & 1) == 1;
					vcr.pfxs.cst[3] = ((value >> 15) & 1) == 1;
					vcr.pfxs.neg[0] = ((value >> 16) & 1) == 1;
					vcr.pfxs.neg[1] = ((value >> 17) & 1) == 1;
					vcr.pfxs.neg[2] = ((value >> 18) & 1) == 1;
					vcr.pfxs.neg[3] = ((value >> 19) & 1) == 1;
					vcr.pfxs.enabled = true;
					break;
				case 1: // 129
					vcr.pfxt.swz[0] = ((value >> 0) & 3);
					vcr.pfxt.swz[1] = ((value >> 2) & 3);
					vcr.pfxt.swz[2] = ((value >> 4) & 3);
					vcr.pfxt.swz[3] = ((value >> 6) & 3);
					vcr.pfxt.abs[0] = ((value >> 8) & 1) == 1;
					vcr.pfxt.abs[1] = ((value >> 9) & 1) == 1;
					vcr.pfxt.abs[2] = ((value >> 10) & 1) == 1;
					vcr.pfxt.abs[3] = ((value >> 11) & 1) == 1;
					vcr.pfxt.cst[0] = ((value >> 12) & 1) == 1;
					vcr.pfxt.cst[1] = ((value >> 13) & 1) == 1;
					vcr.pfxt.cst[2] = ((value >> 14) & 1) == 1;
					vcr.pfxt.cst[3] = ((value >> 15) & 1) == 1;
					vcr.pfxt.neg[0] = ((value >> 16) & 1) == 1;
					vcr.pfxt.neg[1] = ((value >> 17) & 1) == 1;
					vcr.pfxt.neg[2] = ((value >> 18) & 1) == 1;
					vcr.pfxt.neg[3] = ((value >> 19) & 1) == 1;
					vcr.pfxt.enabled = true;
					break;
				case 2: // 130
					vcr.pfxd.sat[0] = ((value >> 0) & 3);
					vcr.pfxd.sat[1] = ((value >> 2) & 3);
					vcr.pfxd.sat[2] = ((value >> 4) & 3);
					vcr.pfxd.sat[3] = ((value >> 6) & 3);
					vcr.pfxd.msk[0] = ((value >> 8) & 1) == 1;
					vcr.pfxd.msk[1] = ((value >> 9) & 1) == 1;
					vcr.pfxd.msk[2] = ((value >> 10) & 1) == 1;
					vcr.pfxd.msk[3] = ((value >> 11) & 1) == 1;
					vcr.pfxd.enabled = true;
					break;
				case 3: // 131
					for (int i = 0; i < vcr.cc.Length; i++)
					{
						vcr.cc[i] = (value & 1) != 0;
						value = (int)((uint)value >> 1);
					}
					break;
				case 8: // 136 - RCX0
					rnd.Seed = value;
					break;
				case 9: // 137 - RCX1
				case 10: // 138 - RCX2
				case 11: // 139 - RCX3
				case 12: // 140 - RCX4
				case 13: // 141 - RCX5
				case 14: // 142 - RCX6
				case 15: // 143 - RCX7
					// as we do not know how VFPU generates a random number through those 8 registers, we ignore 7 of them
					break;
				default:
					// These values are not supported in pspsharp
					doUNK("Unimplemented VMTVC (vd=" + vd + ", imm7=" + imm7 + ", value=0x" + value.ToString("x") + ")");
					break;
			}
		}
		// VFPU4:VT4444
		public virtual void doVT4444(int vsize, int vd, int vs)
		{
			loadVsInt(4, vs);
			int i0 = v1i[0];
			int i1 = v1i[1];
			int i2 = v1i[2];
			int i3 = v1i[3];
			int o0 = 0, o1 = 0;
			o0 |= ((i0 >> 4) & 15) << 0;
			o0 |= ((i0 >> 12) & 15) << 4;
			o0 |= ((i0 >> 20) & 15) << 8;
			o0 |= ((i0 >> 28) & 15) << 12;
			o0 |= ((i1 >> 4) & 15) << 16;
			o0 |= ((i1 >> 12) & 15) << 20;
			o0 |= ((i1 >> 20) & 15) << 24;
			o0 |= ((i1 >> 28) & 15) << 28;
			o1 |= ((i2 >> 4) & 15) << 0;
			o1 |= ((i2 >> 12) & 15) << 4;
			o1 |= ((i2 >> 20) & 15) << 8;
			o1 |= ((i2 >> 28) & 15) << 12;
			o1 |= ((i3 >> 4) & 15) << 16;
			o1 |= ((i3 >> 12) & 15) << 20;
			o1 |= ((i3 >> 20) & 15) << 24;
			o1 |= ((i3 >> 28) & 15) << 28;
			v3i[0] = o0;
			v3i[1] = o1;
			saveVdInt(2, vd, v3i);
		}
		// VFPU4:VT5551
		public virtual void doVT5551(int vsize, int vd, int vs)
		{
			loadVsInt(4, vs);
			int i0 = v1i[0];
			int i1 = v1i[1];
			int i2 = v1i[2];
			int i3 = v1i[3];
			int o0 = 0, o1 = 0;
			o0 |= ((i0 >> 3) & 31) << 0;
			o0 |= ((i0 >> 11) & 31) << 5;
			o0 |= ((i0 >> 19) & 31) << 10;
			o0 |= ((i0 >> 31) & 1) << 15;
			o0 |= ((i1 >> 3) & 31) << 16;
			o0 |= ((i1 >> 11) & 31) << 21;
			o0 |= ((i1 >> 19) & 31) << 26;
			o0 |= ((i1 >> 31) & 1) << 31;
			o1 |= ((i2 >> 3) & 31) << 0;
			o1 |= ((i2 >> 11) & 31) << 5;
			o1 |= ((i2 >> 19) & 31) << 10;
			o1 |= ((i2 >> 31) & 1) << 15;
			o1 |= ((i3 >> 3) & 31) << 16;
			o1 |= ((i3 >> 11) & 31) << 21;
			o1 |= ((i3 >> 19) & 31) << 26;
			o1 |= ((i3 >> 31) & 1) << 31;
			v3i[0] = o0;
			v3i[1] = o1;
			saveVdInt(2, vd, v3i);
		}
		// VFPU4:VT5650
		public virtual void doVT5650(int vsize, int vd, int vs)
		{
			loadVsInt(4, vs);
			int i0 = v1i[0];
			int i1 = v1i[1];
			int i2 = v1i[2];
			int i3 = v1i[3];
			int o0 = 0, o1 = 0;
			o0 |= ((i0 >> 3) & 31) << 0;
			o0 |= ((i0 >> 10) & 63) << 5;
			o0 |= ((i0 >> 19) & 31) << 11;
			o0 |= ((i1 >> 3) & 31) << 16;
			o0 |= ((i1 >> 10) & 63) << 21;
			o0 |= ((i1 >> 19) & 31) << 27;
			o1 |= ((i2 >> 3) & 31) << 0;
			o1 |= ((i2 >> 10) & 63) << 5;
			o1 |= ((i2 >> 19) & 31) << 11;
			o1 |= ((i3 >> 3) & 31) << 16;
			o1 |= ((i3 >> 10) & 63) << 21;
			o1 |= ((i3 >> 19) & 31) << 27;
			v3i[0] = o0;
			v3i[1] = o1;
			saveVdInt(2, vd, v3i);
		}
		// VFPU4:VCST
		public virtual void doVCST(int vsize, int vd, int imm5)
		{
			float constant = 0.0f;

			if (imm5 >= 0 && imm5 < floatConstants.Length)
			{
				constant = floatConstants[imm5];
			}

			for (int i = 0; i < vsize; ++i)
			{
				v3[i] = constant;
			}

			saveVd(vsize, vd, v3);
		}

		// VFPU4:VF2IN
		public virtual void doVF2IN(int vsize, int vd, int vs, int imm5)
		{
			loadVs(vsize, vs);

			for (int i = 0; i < vsize; ++i)
			{
				float value = Math.scalb(v1[i], imm5);
				if (float.IsNaN(value))
				{
					// PSP is returning this value for a NaN (normal case would return 0 for a NaN)
					v3i[i] = 0x7FFFFFFF;
				}
				else
				{
					// PSP is rounding using Math.Round and not using Math.round
					v3i[i] = (int) Math.Round(value);
				}
			}

			saveVdInt(vsize, vd, v3i);
		}
		// VFPU4:VF2IZ
		public virtual void doVF2IZ(int vsize, int vd, int vs, int imm5)
		{
			loadVs(vsize, vs);

			for (int i = 0; i < vsize; ++i)
			{
				float value = Math.scalb(v1[i], imm5);
				double dvalue = v1[i] >= 0 ? System.Math.Floor(value) : System.Math.Ceiling(value);
				if (double.IsNaN(dvalue))
				{
					// PSP is returning this value for a NaN (normal case would return 0 for a NaN)
					v3i[i] = 0x7FFFFFFF;
				}
				else
				{
					v3i[i] = (int) dvalue;
				}
			}

			saveVdInt(vsize, vd, v3i);
		}
		// VFPU4:VF2IU
		public virtual void doVF2IU(int vsize, int vd, int vs, int imm5)
		{
			loadVs(vsize, vs);

			for (int i = 0; i < vsize; ++i)
			{
				float value = Math.scalb(v1[i], imm5);
				double dvalue = System.Math.Ceiling(value);
				if (double.IsNaN(dvalue))
				{
					// PSP is returning this value for a NaN (normal case would return 0 for a NaN)
					v3i[i] = 0x7FFFFFFF;
				}
				else
				{
					v3i[i] = (int) dvalue;
				}
			}

			saveVdInt(vsize, vd, v3i);
		}
		// VFPU4:VF2ID
		public virtual void doVF2ID(int vsize, int vd, int vs, int imm5)
		{
			loadVs(vsize, vs);

			for (int i = 0; i < vsize; ++i)
			{
				float value = Math.scalb(v1[i], imm5);
				double dvalue = System.Math.Floor(value);
				if (double.IsNaN(dvalue))
				{
					// PSP is returning this value for a NaN (normal case would return 0 for a NaN)
					v3i[i] = 0x7FFFFFFF;
				}
				else
				{
					v3i[i] = (int) dvalue;
				}
			}

			saveVdInt(vsize, vd, v3i);
		}
		// VFPU4:VI2F
		public virtual void doVI2F(int vsize, int vd, int vs, int imm5)
		{
			loadVsInt(vsize, vs);

			for (int i = 0; i < vsize; ++i)
			{
				float value = (float) v1i[i];
				v3[i] = Math.scalb(value, -imm5);
			}

			saveVd(vsize, vd, v3);
		}
		// VFPU4:VCMOVT
		public virtual void doVCMOVT(int vsize, int imm3, int vd, int vs)
		{
			if (imm3 < 6)
			{
				if (vcr.cc[imm3])
				{
					loadVs(vsize, vs);
					saveVd(vsize, vd, v1);
				}
				else
				{
					// Clear the PFXS flag and process the PFXD transformation
					vcr.pfxs.enabled = false;
					if (vcr.pfxd.enabled)
					{
						loadVd(vsize, vd);
						saveVd(vsize, vd, v3);
					}
				}
			}
			else if (imm3 == 6)
			{
				loadVs(vsize, vs);
				loadVd(vsize, vd);
				for (int i = 0; i < vsize; ++i)
				{
					if (vcr.cc[i])
					{
						v3[i] = v1[i];
					}
				}
				saveVd(vsize, vd, v3);
			}
			else
			{
				// Never copy (checked on a PSP)
			}
		}
		// VFPU4:VCMOVF
		public virtual void doVCMOVF(int vsize, int imm3, int vd, int vs)
		{
			if (imm3 < 6)
			{
				if (!vcr.cc[imm3])
				{
					loadVs(vsize, vs);
					saveVd(vsize, vd, v1);
				}
				else
				{
					// Clear the PFXS flag and process the PFXD transformation
					vcr.pfxs.enabled = false;
					if (vcr.pfxd.enabled)
					{
						loadVd(vsize, vd);
						saveVd(vsize, vd, v3);
					}
				}
			}
			else if (imm3 == 6)
			{
				loadVs(vsize, vs);
				loadVd(vsize, vd);
				for (int i = 0; i < vsize; ++i)
				{
					if (!vcr.cc[i])
					{
						v3[i] = v1[i];
					}
				}
				saveVd(vsize, vd, v3);
			}
			else
			{
				// Always copy (checked on a PSP)
				loadVs(vsize, vs);
				saveVd(vsize, vd, v1);
			}
		}
		// VFPU4:VWBN
		public virtual void doVWBN(int vsize, int vd, int vs, int imm8)
		{
			// Wrap BigNum.
			if (vsize != 1)
			{
				doUNK("Only supported VWBN.S");
				return;
			}
			loadVs(vsize, vs);

			// Calculate modulus with exponent.
			System.Numerics.BigInteger exp = System.Numerics.BigInteger.valueOf((int) System.Math.Pow(2, 127 - imm8));
			System.Numerics.BigInteger bn = System.Numerics.BigInteger.valueOf((int) v1[0]);
			if (bn.intValue() > 0)
			{
				bn = bn.modPow(exp, bn);
			}
			v1[0] = (bn.floatValue() + (v1[0] < 0.0f ? -exp.intValue() : exp.intValue()));

			saveVd(vsize, vd, v1);
		}
		// group VFPU5
		// VFPU5:VPFXS
		public virtual void doVPFXS(int negw, int negz, int negy, int negx, int cstw, int cstz, int csty, int cstx, int absw, int absz, int absy, int absx, int swzw, int swzz, int swzy, int swzx)
		{
			vcr.pfxs.swz[0] = swzx;
			vcr.pfxs.swz[1] = swzy;
			vcr.pfxs.swz[2] = swzz;
			vcr.pfxs.swz[3] = swzw;
			vcr.pfxs.abs[0] = absx != 0;
			vcr.pfxs.abs[1] = absy != 0;
			vcr.pfxs.abs[2] = absz != 0;
			vcr.pfxs.abs[3] = absw != 0;
			vcr.pfxs.cst[0] = cstx != 0;
			vcr.pfxs.cst[1] = csty != 0;
			vcr.pfxs.cst[2] = cstz != 0;
			vcr.pfxs.cst[3] = cstw != 0;
			vcr.pfxs.neg[0] = negx != 0;
			vcr.pfxs.neg[1] = negy != 0;
			vcr.pfxs.neg[2] = negz != 0;
			vcr.pfxs.neg[3] = negw != 0;
			vcr.pfxs.enabled = true;
		}

		// VFPU5:VPFXT
		public virtual void doVPFXT(int negw, int negz, int negy, int negx, int cstw, int cstz, int csty, int cstx, int absw, int absz, int absy, int absx, int swzw, int swzz, int swzy, int swzx)
		{
			vcr.pfxt.swz[0] = swzx;
			vcr.pfxt.swz[1] = swzy;
			vcr.pfxt.swz[2] = swzz;
			vcr.pfxt.swz[3] = swzw;
			vcr.pfxt.abs[0] = absx != 0;
			vcr.pfxt.abs[1] = absy != 0;
			vcr.pfxt.abs[2] = absz != 0;
			vcr.pfxt.abs[3] = absw != 0;
			vcr.pfxt.cst[0] = cstx != 0;
			vcr.pfxt.cst[1] = csty != 0;
			vcr.pfxt.cst[2] = cstz != 0;
			vcr.pfxt.cst[3] = cstw != 0;
			vcr.pfxt.neg[0] = negx != 0;
			vcr.pfxt.neg[1] = negy != 0;
			vcr.pfxt.neg[2] = negz != 0;
			vcr.pfxt.neg[3] = negw != 0;
			vcr.pfxt.enabled = true;
		}

		// VFPU5:VPFXD
		public virtual void doVPFXD(int mskw, int mskz, int msky, int mskx, int satw, int satz, int saty, int satx)
		{
			vcr.pfxd.sat[0] = satx;
			vcr.pfxd.sat[1] = saty;
			vcr.pfxd.sat[2] = satz;
			vcr.pfxd.sat[3] = satw;
			vcr.pfxd.msk[0] = mskx != 0;
			vcr.pfxd.msk[1] = msky != 0;
			vcr.pfxd.msk[2] = mskz != 0;
			vcr.pfxd.msk[3] = mskw != 0;
			vcr.pfxd.enabled = true;
		}

		// VFPU5:VIIM
		public virtual void doVIIM(int vd, int imm16)
		{
			v3[0] = imm16;

			saveVd(1, vd, v3);
		}

		// VFPU5:VFIM
		public virtual void doVFIM(int vd, int imm16)
		{
			v3i[0] = halffloatToFloat(imm16);

			saveVdInt(1, vd, v3i);
		}

		// group VFPU6   
		// VFPU6:VMMUL
		public virtual void doVMMUL(int vsize, int vd, int vs, int vt)
		{
			if (vsize == 1)
			{
				doUNK("Not supported VMMUL.S");
				return;
			}

			// you must do it for disasm, not for emulation !
			//vs = vs ^ 32;

			for (int i = 0; i < vsize; ++i)
			{
				loadVt(vsize, vt + i);
				for (int j = 0; j < vsize; ++j)
				{
					loadVs(vsize, vs + j);
					float dot = v1[0] * v2[0];
					for (int k = 1; k < vsize; ++k)
					{
						dot += v1[k] * v2[k];
					}
					v3[j] = dot;
				}
				saveVd(vsize, vd + i, v3);
			}
		}

		// VFPU6:VHTFM2
		public virtual void doVHTFM2(int vd, int vs, int vt)
		{
			loadVt(1, vt);
			loadVs(2, vs + 0);
			v3[0] = v1[0] * v2[0] + v1[1];
			loadVs(2, vs + 1);
			v3[1] = v1[0] * v2[0] + v1[1];
			saveVd(2, vd, v3);
		}

		// VFPU6:VTFM2
		public virtual void doVTFM2(int vd, int vs, int vt)
		{
			loadVt(2, vt);
			loadVs(2, vs + 0);
			v3[0] = v1[0] * v2[0] + v1[1] * v2[1];
			loadVs(2, vs + 1);
			v3[1] = v1[0] * v2[0] + v1[1] * v2[1];
			saveVd(2, vd, v3);
		}

		// VFPU6:VHTFM3
		public virtual void doVHTFM3(int vd, int vs, int vt)
		{
			loadVt(2, vt);
			loadVs(3, vs + 0);
			v3[0] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2];
			loadVs(3, vs + 1);
			v3[1] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2];
			loadVs(3, vs + 2);
			v3[2] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2];
			saveVd(3, vd, v3);
		}

		// VFPU6:VTFM3
		public virtual void doVTFM3(int vd, int vs, int vt)
		{
			loadVt(3, vt);
			loadVs(3, vs + 0);
			v3[0] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
			loadVs(3, vs + 1);
			v3[1] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
			loadVs(3, vs + 2);
			v3[2] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
			saveVd(3, vd, v3);
		}

		// VFPU6:VHTFM4
		public virtual void doVHTFM4(int vd, int vs, int vt)
		{
			loadVt(3, vt);
			loadVs(4, vs + 0);
			v3[0] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2] + v1[3];
			loadVs(4, vs + 1);
			v3[1] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2] + v1[3];
			loadVs(4, vs + 2);
			v3[2] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2] + v1[3];
			loadVs(4, vs + 3);
			v3[3] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2] + v1[3];
			saveVd(4, vd, v3);
		}

		// VFPU6:VTFM4
		public virtual void doVTFM4(int vd, int vs, int vt)
		{
			loadVt(4, vt);
			loadVs(4, vs + 0);
			v3[0] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2] + v1[3] * v2[3];
			loadVs(4, vs + 1);
			v3[1] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2] + v1[3] * v2[3];
			loadVs(4, vs + 2);
			v3[2] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2] + v1[3] * v2[3];
			loadVs(4, vs + 3);
			v3[3] = v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2] + v1[3] * v2[3];
			saveVd(4, vd, v3);
		}

		// VFPU6:VMSCL
		public virtual void doVMSCL(int vsize, int vd, int vs, int vt)
		{
			for (int i = 0; i < vsize; ++i)
			{
				doVSCL(vsize, vd + i, vs + i, vt);
			}
		}

		// VFPU6:VCRSP
		public virtual void doVCRSP(int vd, int vs, int vt)
		{
			loadVs(3, vs);
			loadVt(3, vt);

			v3[0] = +v1[1] * v2[2] - v1[2] * v2[1];
			v3[1] = +v1[2] * v2[0] - v1[0] * v2[2];
			v3[2] = +v1[0] * v2[1] - v1[1] * v2[0];

			saveVd(3, vd, v3);
		}

		// VFPU6:VQMUL
		public virtual void doVQMUL(int vd, int vs, int vt)
		{
			loadVs(4, vs);
			loadVt(4, vt);

			v3[0] = +v1[0] * v2[3] + v1[1] * v2[2] - v1[2] * v2[1] + v1[3] * v2[0];
			v3[1] = -v1[0] * v2[2] + v1[1] * v2[3] + v1[2] * v2[0] + v1[3] * v2[1];
			v3[2] = +v1[0] * v2[1] - v1[1] * v2[0] + v1[2] * v2[3] + v1[3] * v2[2];
			v3[3] = -v1[0] * v2[0] - v1[1] * v2[1] - v1[2] * v2[2] + v1[3] * v2[3];

			saveVd(4, vd, v3);
		}

		// VFPU6:VMMOV
		public virtual void doVMMOV(int vsize, int vd, int vs)
		{
			for (int i = 0; i < vsize; ++i)
			{
				doVMOV(vsize, vd + i, vs + i);
			}
		}

		// VFPU6:VMIDT
		public virtual void doVMIDT(int vsize, int vd)
		{
			for (int i = 0; i < vsize; ++i)
			{
				for (int j = 0; j < vsize; ++j)
				{
					v3[j] = (i == j) ? 1.0f : 0.0f;
				}
				saveVd(vsize, vd + i, v3);
			}
		}

		// VFPU6:VMZERO
		public virtual void doVMZERO(int vsize, int vd)
		{
			for (int i = 0; i < vsize; ++i)
			{
				doVZERO(vsize, vd + i);
			}
		}

		// VFPU7:VMONE
		public virtual void doVMONE(int vsize, int vd)
		{
			for (int i = 0; i < vsize; ++i)
			{
				doVONE(vsize, vd + i);
			}
		}

		// VFPU6:VROT
		public virtual void doVROT(int vsize, int vd, int vs, int imm5)
		{
			loadVs(1, vs);
			float ca, sa;

			float angle = v1[0];
			// Reducing the angle to [0..4[
			angle -= ((float) System.Math.Floor(angle * 0.25f)) * 4f;

			// Handling of specific values first to avoid precision loss in float value
			if (angle == 0f)
			{
				ca = 1f;
				sa = 0f;
			}
			else if (angle == 1f)
			{
				ca = 0f;
				sa = 1f;
			}
			else if (angle == 2f)
			{
				ca = -1f;
				sa = 0f;
			}
			else if (angle == 3f)
			{
				ca = 0f;
				sa = -1f;
			}
			else
			{
				// General case
				double a = PI_2 * angle;
				ca = (float) System.Math.Cos(a);
				sa = (float) System.Math.Sin(a);
			}

			int i;
			int si = ((int)((uint)imm5 >> 2)) & 3;
			int ci = ((int)((uint)imm5 >> 0)) & 3;

			if (((imm5 & 16) != 0))
			{
				sa = 0f - sa;
			}

			if (si == ci)
			{
				for (i = 0; i < vsize; ++i)
				{
					v3[i] = sa;
				}
			}
			else
			{
				for (i = 0; i < vsize; ++i)
				{
					v3[i] = 0f;
				}
				v3[si] = sa;
			}
			v3[ci] = ca;

			saveVd(vsize, vd, v3);
		}

		// group VLSU     
		// LSU:LVS
		public virtual void doLVS(int vt, int rs, int simm14_a16)
		{
			int s = (vt >> 5) & 3;
			int m = (vt >> 2) & 7;
			int i = (vt >> 0) & 3;

			setVprInt(m, i, s, memory.read32(getRegister(rs) + simm14_a16));
		}

		// LSU:SVS
		public virtual void doSVS(int vt, int rs, int simm14_a16)
		{
			int s = (vt >> 5) & 3;
			int m = (vt >> 2) & 7;
			int i = (vt >> 0) & 3;

			if (CHECK_ALIGNMENT)
			{
				int address = getRegister(rs) + simm14_a16;
				if ((address & 3) != 0)
				{
					Memory.Console.WriteLine(string.Format("SV.S unaligned addr:0x{0:x8} pc:0x{1:x8}", address, pc));
				}
			}

			memory.write32(getRegister(rs) + simm14_a16, getVprInt(m, i, s));
		}

		// LSU:LVQ
		public virtual void doLVQ(int vt, int rs, int simm14_a16)
		{
			int m = (vt >> 2) & 7;
			int i = (vt >> 0) & 3;

			int address = getRegister(rs) + simm14_a16;

			if (CHECK_ALIGNMENT)
			{
				if ((address & 15) != 0)
				{
					Memory.Console.WriteLine(string.Format("LV.Q unaligned addr:0x{0:x8} pc:0x{1:x8}", address, pc));
				}
			}

			if ((vt & 32) != 0)
			{
				for (int j = 0; j < 4; ++j)
				{
					setVprInt(m, j, i, memory.read32(address + j * 4));
				}
			}
			else
			{
				for (int j = 0; j < 4; ++j)
				{
					setVprInt(m, i, j, memory.read32(address + j * 4));
				}
			}
		}

		// LSU:LVLQ
		public virtual void doLVLQ(int vt, int rs, int simm14_a16)
		{
			int m = (vt >> 2) & 7;
			int i = (vt >> 0) & 3;

			int address = getRegister(rs) + simm14_a16;

			if (CHECK_ALIGNMENT)
			{
				if ((address & 3) != 0)
				{
					Memory.Console.WriteLine(string.Format("LVL.Q unaligned addr:0x{0:x8} pc:0x{1:x8}", address, pc));
				}
			}

			int k = 3 - ((address >> 2) & 3);
			address &= ~0xF;
			if ((vt & 32) != 0)
			{
				for (int j = k; j < 4; ++j)
				{
					setVprInt(m, j, i, memory.read32(address));
					address += 4;
				}
			}
			else
			{
				for (int j = k; j < 4; ++j)
				{
					setVprInt(m, i, j, memory.read32(address));
					address += 4;
				}
			}
		}

		// LSU:LVRQ
		public virtual void doLVRQ(int vt, int rs, int simm14_a16)
		{
			int m = (vt >> 2) & 7;
			int i = (vt >> 0) & 3;

			int address = getRegister(rs) + simm14_a16;

			if (CHECK_ALIGNMENT)
			{
				if ((address & 3) != 0)
				{
					Memory.Console.WriteLine(string.Format("LVR.Q unaligned addr:0x{0:x8} pc:0x{1:x8}", address, pc));
				}
			}

			int k = 4 - ((address >> 2) & 3);
			if ((vt & 32) != 0)
			{
				for (int j = 0; j < k; ++j)
				{
					setVprInt(m, j, i, memory.read32(address));
					address += 4;
				}
			}
			else
			{
				for (int j = 0; j < k; ++j)
				{
					setVprInt(m, i, j, memory.read32(address));
					address += 4;
				}
			}
		}
		// LSU:SVQ
		public virtual void doSVQ(int vt, int rs, int simm14_a16)
		{
			int m = (vt >> 2) & 7;
			int i = (vt >> 0) & 3;

			int address = getRegister(rs) + simm14_a16;

			if (CHECK_ALIGNMENT)
			{
				if ((address & 15) != 0)
				{
					Memory.Console.WriteLine(string.Format("SV.Q unaligned addr:0x{0:x8} pc:0x{1:x8}", address, pc));
				}
			}

			if ((vt & 32) != 0)
			{
				for (int j = 0; j < 4; ++j)
				{
					memory.write32((address + j * 4), getVprInt(m, j, i));
				}
			}
			else
			{
				for (int j = 0; j < 4; ++j)
				{
					memory.write32((address + j * 4), getVprInt(m, i, j));
				}
			}
		}

		// LSU:SVLQ
		public virtual void doSVLQ(int vt, int rs, int simm14_a16)
		{
			int m = (vt >> 2) & 7;
			int i = (vt >> 0) & 3;

			int address = getRegister(rs) + simm14_a16;

			if (CHECK_ALIGNMENT)
			{
				if ((address & 3) != 0)
				{
					Memory.Console.WriteLine(string.Format("SVL.Q unaligned addr:0x{0:x8} pc:0x{1:x8}", address, pc));
				}
			}

			int k = 3 - ((address >> 2) & 3);
			address &= ~0xF;
			if ((vt & 32) != 0)
			{
				for (int j = k; j < 4; ++j)
				{
					memory.write32(address, getVprInt(m, j, i));
					address += 4;
				}
			}
			else
			{
				for (int j = k; j < 4; ++j)
				{
					memory.write32(address, getVprInt(m, i, j));
					address += 4;
				}
			}
		}

		// LSU:SVRQ
		public virtual void doSVRQ(int vt, int rs, int simm14_a16)
		{
			int m = (vt >> 2) & 7;
			int i = (vt >> 0) & 3;

			int address = getRegister(rs) + simm14_a16;

			if (CHECK_ALIGNMENT)
			{
				if ((address & 3) != 0)
				{
					Memory.Console.WriteLine(string.Format("SVR.Q unaligned addr:0x{0:x8} pc:0x{1:x8}", address, pc));
				}
			}

			int k = 4 - ((address >> 2) & 3);
			if ((vt & 32) != 0)
			{
				for (int j = 0; j < k; ++j)
				{
					memory.write32(address, getVprInt(m, j, i));
					address += 4;
				}
			}
			else
			{
				for (int j = 0; j < k; ++j)
				{
					memory.write32(address, getVprInt(m, i, j));
					address += 4;
				}
			}
		}
	}

}