using System;
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
namespace pspsharp.Allegrex.compiler
{

	public class CodeSequence : IComparable<CodeSequence>
	{
		private int startAddress;
		private int endAddress;
		private LinkedList<CodeInstruction> codeInstructions = new LinkedList<CodeInstruction>();

		public CodeSequence(int startAddress)
		{
			this.startAddress = startAddress;
			this.endAddress = startAddress;
		}

		public virtual int StartAddress
		{
			get
			{
				return startAddress;
			}
		}

		public virtual int EndAddress
		{
			get
			{
				return endAddress;
			}
			set
			{
				this.endAddress = value;
			}
		}


		public virtual int Length
		{
			get
			{
				return ((endAddress - startAddress) >> 2) + 1;
			}
		}

		public virtual bool isInside(int address)
		{
			return (startAddress <= address && address <= endAddress);
		}

		public virtual void addInstruction(CodeInstruction codeInstruction)
		{
			codeInstructions.AddLast(codeInstruction);
		}

		public virtual IList<CodeInstruction> Instructions
		{
			get
			{
				return codeInstructions;
			}
		}

		public virtual CodeInstruction getCodeInstruction(int address)
		{
			foreach (CodeInstruction codeInstruction in codeInstructions)
			{
				if (codeInstruction.Address == address)
				{
					return codeInstruction;
				}
			}

			return null;
		}

		public virtual int CompareTo(CodeSequence codeSequence)
		{
			if (codeSequence == null)
			{
				return -1;
			}

			int length1 = Length;
			int length2 = codeSequence.Length;

			if (length1 < length2)
			{
				return 1;
			}
			else if (length1 > length2)
			{
				return -1;
			}

			return 0;
		}

		public override string ToString()
		{
			return string.Format("CodeSequence 0x{0:X} - 0x{1:X} (Length {2:D})", StartAddress, EndAddress, Length);
		}
	}

}