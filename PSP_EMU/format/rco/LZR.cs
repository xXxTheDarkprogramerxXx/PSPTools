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
namespace pspsharp.format.rco
{

	/// <summary>
	/// LZR decompression
	/// Based on libLZR Version 0.11 by BenHur - http://www.psp-programming.com/benhur
	/// https://github.com/Grumbel/rfactortools/blob/master/other/quickbms/src/compression/libLZR.c
	/// </summary>
	public class LZR
	{
		private static int u8(sbyte b)
		{
			return b & 0xFF;
		}

		private static long u32(int i)
		{
			return i & 0xFFFFFFFFL;
		}

		private class IntObject
		{
			internal static readonly IntObject Null = new IntObject(0);
			internal int value;

			public IntObject(int value)
			{
				this.value = value;
			}

			public virtual int Value
			{
				get
				{
					return value;
				}
				set
				{
					this.value = value;
				}
			}


			public virtual int incr()
			{
				return value++;
			}

			public virtual void sub(int sub)
			{
				value -= sub;
			}

			public override string ToString()
			{
				return string.Format("0x{0:X8}", value);
			}
		}

		private class BoolObject
		{
			internal bool value;

			public virtual bool Value
			{
				get
				{
					return value;
				}
				set
				{
					this.value = value;
				}
			}


			public override string ToString()
			{
				return Convert.ToString(value);
			}
		}

		private static void fillBuffer(IntObject testMask, IntObject mask, IntObject buffer, sbyte[] @in, IntObject nextIn)
		{
			// if necessary, fill up in buffer and shift mask
			if (testMask.Value >= 0 && testMask.Value <= 0x00FFFFFF)
			{
				buffer.Value = (buffer.Value << 8) + u8(@in[nextIn.incr()]);
				mask.Value = testMask.Value << 8;
			}
		}

		private static bool nextBit(sbyte[] buf, int bufPtr1, IntObject number, IntObject testMask, IntObject mask, IntObject buffer, sbyte[] @in, IntObject nextIn)
		{
			fillBuffer(testMask, mask, buffer, @in, nextIn);
			int value = ((int)((uint)mask.Value >> 8)) * u8(buf[bufPtr1]);
			if (testMask != mask)
			{
				testMask.Value = value;
			}
			buf[bufPtr1] -= (sbyte)(u8(buf[bufPtr1]) >> 3);
			number.Value = number.Value << 1;
			if (u32(buffer.Value) < u32(value))
			{
				mask.Value = value;
				buf[bufPtr1] += 31;
				number.incr();
				return true;
			}

			buffer.sub(value);
			mask.sub(value);

			return false;
		}

		private static int getNumber(int nBits, sbyte[] buf, int bufPtr, int inc, BoolObject flag, IntObject mask, IntObject buffer, sbyte[] @in, IntObject nextIn)
		{
			// Extract and return a number (consisting of n_bits bits) from in stream
			IntObject number = new IntObject(1);
			if (nBits >= 3)
			{
				nextBit(buf, bufPtr + 3 * inc, number, mask, mask, buffer, @in, nextIn);
				if (nBits >= 4)
				{
					nextBit(buf, bufPtr + 3 * inc, number, mask, mask, buffer, @in, nextIn);
					if (nBits >= 5)
					{
						fillBuffer(mask, mask, buffer, @in, nextIn);
						for (; nBits >= 5; nBits--)
						{
							number.Value = number.Value << 1;
							mask.Value = (int)((uint)mask.Value >> 1);
							if (u32(buffer.Value) < u32(mask.Value))
							{
								number.incr();
							}
							else
							{
								buffer.sub(mask.Value);
							}
						}
					}
				}
			}
			flag.Value = nextBit(buf, bufPtr, number, mask, mask, buffer, @in, nextIn);
			if (nBits >= 1)
			{
				nextBit(buf, bufPtr + inc, number, mask, mask, buffer, @in, nextIn);
				if (nBits >= 2)
				{
					nextBit(buf, bufPtr + 2 * inc, number, mask, mask, buffer, @in, nextIn);
				}
			}

			return number.Value;
		}

		public static int decompress(sbyte[] @out, int outCapacity, sbyte[] @in)
		{
			int type = @in[0];
			IntObject buffer = new IntObject((u8(@in[1]) << 24) | (u8(@in[2]) << 16) | (u8(@in[3]) << 8) | u8(@in[4]));

			IntObject nextIn = new IntObject(5);
			int nextOut = 0;
			int outEnd = outCapacity;

			if (type < 0)
			{
				// copy from stream without decompression
				int seqEnd = nextOut + buffer.Value;
				if (seqEnd > outEnd)
				{
					return -1;
				}
				while (nextOut < seqEnd)
				{
					@out[nextOut++] = @in[nextIn.incr()];
				}
				return nextOut;
			}

			// Create and inti buffer
			sbyte[] buf = new sbyte[2800];
			Arrays.fill(buf, unchecked((sbyte) 0x80));
			int bufOff = 0;

			IntObject mask = new IntObject(unchecked((int)0xFFFFFFFF));
			IntObject testMask = new IntObject(0);
			int lastChar = 0;

			while (true)
			{
				int bufPtr1 = bufOff + 2488;
				if (!nextBit(buf, bufPtr1, IntObject.Null, mask, mask, buffer, @in, nextIn))
				{
					// Single new char
					if (bufOff > 0)
					{
						bufOff--;
					}
					if (nextOut == outEnd)
					{
						return -1;
					}
					bufPtr1 = (((((nextOut & 0x07) << 8) + lastChar) >> type) & 0x07) * 0xFF - 0x01;
					IntObject j = new IntObject(1);
					while (j.Value <= 0xFF)
					{
						nextBit(buf, bufPtr1 + j.Value, j, mask, mask, buffer, @in, nextIn);
					}
					@out[nextOut++] = (sbyte) j.Value;
				}
				else
				{
					// Sequence of chars that exists in out stream

					// Find number of bits of sequence length
					testMask.Value = mask.Value;
					int nBits = -1;
					BoolObject flag = new BoolObject();
					do
					{
						bufPtr1 += 8;
						flag.Value = nextBit(buf, bufPtr1, IntObject.Null, testMask, mask, buffer, @in, nextIn);
						if (flag.Value)
						{
							nBits++;
						}
					} while (flag.Value && nBits < 6);

					// Find sequence length
					int bufPtr2 = nBits + 2033;
					int j = 64;
					int seqLen;
					if (flag.Value || nBits >= 0)
					{
						bufPtr1 = (nBits << 5) + (((nextOut << nBits) & 0x03) << 3) + bufOff + 2552;
						seqLen = getNumber(nBits, buf, bufPtr1, 8, flag, mask, buffer, @in, nextIn);
						if (seqLen == 0xFF)
						{
							return nextOut; // End of data stream
						}
						if (flag.Value || nBits > 0)
						{
							bufPtr2 += 56;
							j = 352;
						}
					}
					else
					{
						seqLen = 1;
					}

					// Find number of bits of sequence offset
					IntObject i = new IntObject(1);
					do
					{
						nBits = (i.Value << 4) - j;
						flag.Value = nextBit(buf, bufPtr2 + (i.Value << 3), i, mask, mask, buffer, @in, nextIn);
					} while (nBits < 0);

					// Find sequence offset
					int seqOff;
					if (flag.Value || nBits > 0)
					{
						if (!flag.Value)
						{
							nBits -= 8;
						}
						seqOff = getNumber(nBits / 8, buf, nBits + 2344, 1, flag, mask, buffer, @in, nextIn);
					}
					else
					{
						seqOff = 1;
					}

					// Copy sequence
					int nextSeq = nextOut - seqOff;
					if (nextSeq < 0)
					{
						return -1;
					}
					int seqEnd = nextOut + seqLen + 1;
					if (seqEnd > outEnd)
					{
						return -1;
					}
					bufOff = ((seqEnd + 1) & 0x01) + 0x06;
					do
					{
						@out[nextOut++] = @out[nextSeq++];
					} while (nextOut < seqEnd);
				}
				lastChar = u8(@out[nextOut - 1]);
			}
		}
	}

}