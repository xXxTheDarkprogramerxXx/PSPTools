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
namespace pspsharp.Allegrex.compiler
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._sp;

	using MethodVisitor = org.objectweb.asm.MethodVisitor;
	using Opcodes = org.objectweb.asm.Opcodes;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class CompilerParameterReader : ParameterReader
	{
		protected internal readonly ICompilerContext compilerContext;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool hasErrorPointer_Renamed = false;
		private int currentParameterIndex = 0;
		private int currentStackPopIndex = 0;
		private readonly int[] currentStackPop = new int[10];

		public CompilerParameterReader(ICompilerContext compilerContext) : base(null, null)
		{
			this.compilerContext = compilerContext;
		}

		private void loadParameterIntFromMemory(int index)
		{
			compilerContext.memRead32(_sp, (index - maxParameterInGprRegisters) << 2);
		}

		protected internal virtual void loadParameterIntFromRegister(int index)
		{
			compilerContext.loadRegister(firstParameterInGpr + index);
		}

		private void loadParameterIntAt(int index)
		{
			if (index >= maxParameterInGprRegisters)
			{
				loadParameterIntFromMemory(index);
			}
			else
			{
				loadParameterIntFromRegister(index);
			}
		}

		private void loadParameterFloatAt(int index)
		{
			if (index >= maxParameterInFprRegisters)
			{
				throw (new System.NotSupportedException());
			}
			compilerContext.loadFRegister(firstParameterInFpr + index);
		}

		private void loadParameterLongAt(int index)
		{
			if ((index % 2) != 0)
			{
				throw (new Exception("Parameter misalignment"));
			}
			loadParameterIntAt(index);
			compilerContext.MethodVisitor.visitInsn(Opcodes.I2L);
			compilerContext.MethodVisitor.visitLdcInsn(0xFFFFFFFFL);
			compilerContext.MethodVisitor.visitInsn(Opcodes.LAND);
			loadParameterIntAt(index + 1);
			compilerContext.MethodVisitor.visitInsn(Opcodes.I2L);
			compilerContext.loadImm(32);
			compilerContext.MethodVisitor.visitInsn(Opcodes.LSHL);
			compilerContext.MethodVisitor.visitInsn(Opcodes.LADD);
		}

		public virtual void loadNextInt()
		{
			loadParameterIntAt(moveParameterIndex(1));
		}

		public virtual void loadNextFloat()
		{
			loadParameterFloatAt(moveParameterIndexFloat(1));
		}

		public virtual void loadNextLong()
		{
			loadParameterLongAt(moveParameterIndex(2));
		}

		public virtual void skipNextInt()
		{
			moveParameterIndex(1);
		}

		public virtual void skipNextFloat()
		{
			moveParameterIndexFloat(1);
		}

		public virtual void skipNextLong()
		{
			moveParameterIndex(2);
		}

		public virtual void rewindPreviousInt()
		{
			moveParameterIndex(-1);
		}

		public virtual void popAllStack(int additionalCount)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.objectweb.asm.MethodVisitor mv = compilerContext.getMethodVisitor();
			MethodVisitor mv = compilerContext.MethodVisitor;

			while (additionalCount >= 2)
			{
				mv.visitInsn(Opcodes.POP2);
				additionalCount -= 2;
			}

			if (additionalCount > 0)
			{
				mv.visitInsn(Opcodes.POP);
			}

			for (int i = currentStackPopIndex - 1; i >= 0; i--)
			{
				mv.visitInsn(currentStackPop[i]);
			}
		}

		public virtual bool hasErrorPointer()
		{
			return hasErrorPointer_Renamed;
		}

		public virtual bool HasErrorPointer
		{
			set
			{
				this.hasErrorPointer_Renamed = value;
			}
		}

		public virtual int CurrentParameterIndex
		{
			get
			{
				return currentParameterIndex;
			}
		}

		public virtual void incrementCurrentParameterIndex()
		{
			currentParameterIndex++;
		}

		public virtual void incrementCurrentStackSize(int size)
		{
			if (size == 1 && currentStackPopIndex > 0 && currentStackPop[currentStackPopIndex - 1] == Opcodes.POP)
			{
				// Merge previous POP with this one into a POP2
				currentStackPop[currentStackPopIndex - 1] = Opcodes.POP2;
			}
			else
			{
				// When size == 2 (e.g. for a "long" value), do not merge with a previous POP,
				// use an own POP2 for this "long" value.
				// Otherwise, VerifyError would be raised with message
				// "Attempt to split long or double on the stack"
				while (size >= 2)
				{
					currentStackPop[currentStackPopIndex++] = Opcodes.POP2;
					size -= 2;
				}
				if (size > 0)
				{
					currentStackPop[currentStackPopIndex++] = Opcodes.POP;
				}
			}
		}

		public virtual void incrementCurrentStackSize()
		{
			incrementCurrentStackSize(1);
		}
	}

}