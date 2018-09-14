using System;
using System.Collections.Generic;
using System.Text;

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
namespace pspsharp.Allegrex.compiler.nativeCode
{


	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class NativeCodeSequence
	{
		protected internal string name;
		protected internal NativeOpcodeInfo[] opcodes = new NativeOpcodeInfo[0];
		private Type<INativeCodeSequence> nativeCodeSequenceClass;
		private ParameterInfo[] parameters = new ParameterInfo[0];
		private int branchInstruction = -1;
		private bool isReturning = false;
		private bool wholeCodeBlock = false;
		private string methodName = "call";
		private IList<CodeInstruction> beforeCodeInstructions;
		private bool isHook = false;
		private bool isMethodReturning = false;

		private class NativeOpcodeInfo
		{
			internal int opcode;
			internal int mask;
			internal int notMask;
			internal string label;
			internal int maskedOpcode;

			public NativeOpcodeInfo(int opcode, int mask, string label)
			{
				this.opcode = opcode;
				this.mask = mask;
				this.label = label;
				maskedOpcode = opcode & mask;
				notMask = ~mask;
			}

			public virtual bool isMatching(int opcode)
			{
				return (opcode & mask) == maskedOpcode;
			}

			public virtual int Opcode
			{
				get
				{
					return opcode;
				}
			}

			public virtual string Label
			{
				get
				{
					return label;
				}
			}

			public virtual int Mask
			{
				get
				{
					return mask;
				}
			}

			public virtual int NotMask
			{
				get
				{
					return notMask;
				}
			}
		}

		private class ParameterInfo
		{
			internal int value;
			internal bool isLabelIndex;

			public ParameterInfo(int value, bool isLabelIndex)
			{
				this.value = value;
				this.isLabelIndex = isLabelIndex;
			}

			public virtual int Value
			{
				get
				{
					return value;
				}
			}

			public virtual int getValue(int address, NativeOpcodeInfo[] opcodes)
			{
				if (isLabelIndex && value >= 0 && value < opcodes.Length)
				{
					int labelAddress = address + (value << 2);
					int targetOpcode = Memory.Instance.read32(labelAddress);
					NativeOpcodeInfo opcode = opcodes[value];
					return targetOpcode & opcode.NotMask;
				}

				return value;
			}
		}

		public NativeCodeSequence(string name, Type<INativeCodeSequence> nativeCodeSequenceClass)
		{
			this.name = name;
			this.nativeCodeSequenceClass = nativeCodeSequenceClass;
		}

		public virtual string Name
		{
			get
			{
				return name;
			}
			set
			{
				this.name = value;
			}
		}


		public virtual void addOpcode(int opcode, int mask, string label)
		{
			NativeOpcodeInfo[] newOpcodes = new NativeOpcodeInfo[opcodes.Length + 1];
			Array.Copy(opcodes, 0, newOpcodes, 0, opcodes.Length);
			opcodes = newOpcodes;

			opcodes[opcodes.Length - 1] = new NativeOpcodeInfo(opcode, mask, label);
		}

		public virtual int FirstOpcode
		{
			get
			{
				return opcodes[0].Opcode;
			}
		}

		public virtual int FirstOpcodeMask
		{
			get
			{
				return opcodes[0].Mask;
			}
		}

		public virtual int NumOpcodes
		{
			get
			{
				return opcodes.Length;
			}
		}

		public virtual Type<INativeCodeSequence> NativeCodeSequenceClass
		{
			get
			{
				return nativeCodeSequenceClass;
			}
			set
			{
				this.nativeCodeSequenceClass = value;
			}
		}


		public virtual int getLabelIndex(string label)
		{
			int value = -1;

			if (string.ReferenceEquals(label, null))
			{
				return value;
			}

			for (int i = 0; i < opcodes.Length; i++)
			{
				if (label.Equals(opcodes[i].Label, StringComparison.OrdinalIgnoreCase))
				{
					value = i;
					break;
				}
			}

			return value;
		}

		public virtual bool isMatching(int opcodeIndex, int opcode)
		{
			return opcodes[opcodeIndex].isMatching(opcode);
		}

		public virtual void setParameter(int parameter, int value, bool isLabelIndex)
		{
			if (parameter >= parameters.Length)
			{
				ParameterInfo[] newParameters = new ParameterInfo[parameter + 1];
				Array.Copy(parameters, 0, newParameters, 0, parameters.Length);
				for (int i = parameters.Length; i < parameter; i++)
				{
					newParameters[i] = null;
				}
				parameters = newParameters;
			}

			parameters[parameter] = new ParameterInfo(value, isLabelIndex);
		}

		public virtual int getParameterValue(int parameter, int address)
		{
			if (parameter >= parameters.Length)
			{
				return 0;
			}

			ParameterInfo parameterInfo = parameters[parameter];
			if (address == 0)
			{
				return parameterInfo.Value;
			}

			return parameterInfo.getValue(address, opcodes);
		}

		public virtual int NumberParameters
		{
			get
			{
				return parameters.Length;
			}
		}

		public virtual int BranchInstruction
		{
			get
			{
				return branchInstruction;
			}
			set
			{
				this.branchInstruction = value;
			}
		}


		public virtual bool hasBranchInstruction()
		{
			return branchInstruction >= 0;
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			result.Append(name);

			result.Append("[");
			for (int i = 0; opcodes != null && i < opcodes.Length; i++)
			{
				if (i > 0)
				{
					result.Append(",");
				}
				result.Append(string.Format("{0:X8}", opcodes[i].Opcode));
			}
			result.Append("]");

			result.Append("(");
			for (int i = 0; i < NumberParameters; i++)
			{
				if (i > 0)
				{
					result.Append(",");
				}
				result.Append(getParameterValue(i, 0));
			}
			result.Append(")");

			return result.ToString();
		}

		public virtual bool Returning
		{
			get
			{
				return isReturning;
			}
			set
			{
				this.isReturning = value;
			}
		}


		public virtual bool WholeCodeBlock
		{
			get
			{
				return wholeCodeBlock;
			}
			set
			{
				this.wholeCodeBlock = value;
			}
		}


		public virtual string MethodName
		{
			get
			{
				return methodName;
			}
			set
			{
				this.methodName = value;
			}
		}


		public virtual IList<CodeInstruction> BeforeCodeInstructions
		{
			get
			{
				return beforeCodeInstructions;
			}
		}

		public virtual void addBeforeCodeInstruction(CodeInstruction codeInstruction)
		{
			if (beforeCodeInstructions == null)
			{
				beforeCodeInstructions = new LinkedList<CodeInstruction>();
			}
			beforeCodeInstructions.Add(codeInstruction);
		}

		public virtual bool Hook
		{
			get
			{
				return isHook;
			}
			set
			{
				this.isHook = value;
			}
		}


		public virtual bool MethodReturning
		{
			get
			{
				return isMethodReturning;
			}
			set
			{
				this.isMethodReturning = value;
			}
		}

	}

}