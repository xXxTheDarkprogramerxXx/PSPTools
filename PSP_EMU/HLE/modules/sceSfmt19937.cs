using System.Collections.Generic;

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



	public class sceSfmt19937 : HLEModule
	{
		public static Logger log = Modules.getLogger("sceSfmt19937");

		protected internal const int PSP_SFMT19937_LENGTH = 156;
		private Dictionary<TPointer, sfmt19937Ctx> ctxMap = new Dictionary<TPointer, sfmt19937Ctx>();

		private class sfmt19937Ctx
		{
			internal int index;
			internal int indexPos;
			internal int[][] sfmt;
			internal TPointer addr;
			internal int seed;
			internal int[] seedArray;

			public sfmt19937Ctx(TPointer ctxAddr, int seed)
			{
				this.index = 0;
				this.indexPos = 0;
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: this.sfmt = new int[PSP_SFMT19937_LENGTH][4];
				this.sfmt = RectangularArrays.ReturnRectangularIntArray(PSP_SFMT19937_LENGTH, 4);
				this.addr = ctxAddr;
				this.seed = seed;
			}

			public sfmt19937Ctx(TPointer ctxAddr, int[] seeds)
			{
				this.index = 0;
				this.indexPos = 0;
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: this.sfmt = new int[PSP_SFMT19937_LENGTH][4];
				this.sfmt = RectangularArrays.ReturnRectangularIntArray(PSP_SFMT19937_LENGTH, 4);
				this.addr = ctxAddr;
				this.seedArray = seeds;
			}

			internal virtual void generate()
			{
				// If using a seed array, just assign the first seed.
				if (this.seedArray != null)
				{
					this.seed = seedArray[0];
				}

				// Store the current index.
				this.addr.setValue32(this.index);

				// Generate a SFMT19937 context with Random and write the values.
				System.Random rand = new System.Random(this.seed);
				for (int i = 0; i < PSP_SFMT19937_LENGTH; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						sfmt[i][j] = rand.Next();
						this.addr.setValue32(sfmt[i][j]);
					}
				}
			}

			internal virtual int NextRand
			{
				get
				{
					int r = sfmt[index][indexPos];
					if ((this.indexPos + 1) < 4)
					{
						this.indexPos++;
					}
					else
					{
						this.indexPos = 0;
						this.index++;
					}
					return r;
				}
			}

			internal virtual long NextRand64
			{
				get
				{
					long r1 = NextRand;
					long r2 = NextRand;
					return ((r1 << 32) | r2);
				}
			}

		}

		[HLEFunction(nid : 0x161ACEB2, version : 500)]
		public virtual int sceSfmt19937InitGenRand(TPointer sfmtctx, int seed)
		{
			// Assign and store the current context.
			sfmt19937Ctx ctx = new sfmt19937Ctx(sfmtctx, seed);
			ctxMap[sfmtctx] = ctx;
			return 0;
		}

		[HLEFunction(nid : 0xDD5A5D6C, version : 500)]
		public virtual int sceSfmt19937InitByArray(TPointer sfmtctx, TPointer seeds, int seedsLength)
		{
			// Read and store the seeds.
			int[] s = new int[seedsLength];
			for (int i = 0; i < seedsLength; i++)
			{
				s[i] = seeds.getValue32();
			}
			// Assign and store the current context.
			sfmt19937Ctx ctx = new sfmt19937Ctx(sfmtctx, s);
			ctxMap[sfmtctx] = ctx;
			return 0;
		}

		[HLEFunction(nid : 0xB33FE749, version : 500)]
		public virtual int sceSfmt19937GenRand32(TPointer sfmtctx)
		{
			int result = 0;
			// Check if the context has been initialized.
			if (ctxMap.ContainsKey(sfmtctx))
			{
				sfmt19937Ctx ctx = ctxMap[sfmtctx];
				ctx.generate();
				result = ctx.NextRand;
			}
			return result;
		}

		[HLEFunction(nid : 0xD5AC9F99, version : 500)]
		public virtual long sceSfmt19937GenRand64(TPointer sfmtctx)
		{
			long result = 0;
			// Check if the context has been initialized.
			if (ctxMap.ContainsKey(sfmtctx))
			{
				sfmt19937Ctx ctx = ctxMap[sfmtctx];
				ctx.generate();
				result = ctx.NextRand64;
			}
			return result;
		}

		[HLEFunction(nid : 0xDB025BFA, version : 500)]
		public virtual int sceSfmt19937FillArray32(TPointer sfmtctx, TPointer array, int arrayLength)
		{
			// Check if the context has been initialized.
			if (ctxMap.ContainsKey(sfmtctx))
			{
				sfmt19937Ctx ctx = ctxMap[sfmtctx];
				ctx.generate();
				// Fill the array with the random values.
				for (int i = 0; i < arrayLength; i++)
				{
					array.setValue32(i, ctx.NextRand);
				}
			}
			return 0;
		}

		[HLEFunction(nid : 0xEE2938C4, version : 500)]
		public virtual int sceSfmt19937FillArray64(TPointer sfmtctx, TPointer array, int arrayLength)
		{
			// Check if the context has been initialized.
			if (ctxMap.ContainsKey(sfmtctx))
			{
				sfmt19937Ctx ctx = ctxMap[sfmtctx];
				ctx.generate();
				// Fill the array with the random values.
				for (int i = 0; i < arrayLength; i++)
				{
					array.setValue64(i, ctx.NextRand64);
				}
			}
			return 0;
		}
	}

}