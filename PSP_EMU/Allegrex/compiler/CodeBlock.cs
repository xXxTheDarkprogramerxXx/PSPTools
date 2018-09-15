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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.CompilerContext.executableDescriptor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.INTERNAL_THREAD_ADDRESS_END;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.INTERNAL_THREAD_ADDRESS_START;


	using Instruction = pspsharp.Allegrex.Common.Instruction;
	using HookCodeInstruction = pspsharp.Allegrex.compiler.nativeCode.HookCodeInstruction;
	using NativeCodeInstruction = pspsharp.Allegrex.compiler.nativeCode.NativeCodeInstruction;
	using NativeCodeManager = pspsharp.Allegrex.compiler.nativeCode.NativeCodeManager;
	using NativeCodeSequence = pspsharp.Allegrex.compiler.nativeCode.NativeCodeSequence;
	using HLEModuleFunction = pspsharp.HLE.HLEModuleFunction;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using Utilities = pspsharp.util.Utilities;

	//using Logger = org.apache.log4j.Logger;
	using ClassVisitor = org.objectweb.asm.ClassVisitor;
	using ClassWriter = org.objectweb.asm.ClassWriter;
	using FieldVisitor = org.objectweb.asm.FieldVisitor;
	using MethodVisitor = org.objectweb.asm.MethodVisitor;
	using Opcodes = org.objectweb.asm.Opcodes;
	using Type = org.objectweb.asm.Type;
	using CheckClassAdapter = org.objectweb.asm.util.CheckClassAdapter;
	using TraceClassVisitor = org.objectweb.asm.util.TraceClassVisitor;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class CodeBlock
	{
		private static Logger log = Compiler.log;
		private int startAddress;
		private int lowestAddress;
		private int highestAddress;
		private LinkedList<CodeInstruction> codeInstructions = new LinkedList<CodeInstruction>();
		private LinkedList<SequenceCodeInstruction> sequenceCodeInstructions = new LinkedList<SequenceCodeInstruction>();
		private SequenceCodeInstruction currentSequence = null;
		private IExecutable executable = null;
		private static readonly string objectInternalName = Type.getInternalName(typeof(object));
		private static readonly string[] interfacesForExecutable = new string[] {Type.getInternalName(typeof(IExecutable))};
		private static readonly string[] exceptions = new string[] {Type.getInternalName(typeof(Exception))};
		private int instanceIndex;
		private Instruction[] interpretedInstructions;
		private int[] interpretedOpcodes;
		private MemoryRanges memoryRanges = new MemoryRanges();
		private int flags;
		private HLEModuleFunction hleFunction;
		private IAction updateOpcodesAction;

		public CodeBlock(int startAddress, int instanceCount)
		{
			this.startAddress = startAddress;
			this.instanceIndex = instanceCount;
			lowestAddress = startAddress;
			highestAddress = startAddress;
		}

		public virtual void addInstruction(int address, int opcode, Instruction insn, bool isBranchTarget, bool isBranching, int branchingTo, bool useMMIO)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("CodeBlock.addInstruction 0x{0:X} - {1}", address, insn.disasm(address, opcode)));
			}

			CodeInstruction codeInstruction = new CodeInstruction(address, opcode, insn, isBranchTarget, isBranching, branchingTo);

			codeInstruction.UseMMIO = useMMIO;

			// Insert the codeInstruction in the codeInstructions list
			// and keep the list sorted by address.
			if (codeInstructions.Count == 0 || codeInstructions.Last.Value.Address < address)
			{
				codeInstructions.AddLast(codeInstruction);
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
				for (IEnumerator<CodeInstruction> lit = codeInstructions.GetEnumerator(); lit.MoveNext();)
				{
					CodeInstruction listItem = lit.Current;
					if (listItem.Address > address)
					{
						lit.previous();
						lit.add(codeInstruction);
						break;
					}
				}

				if (address < lowestAddress)
				{
					lowestAddress = address;
				}
			}

			if (address > highestAddress)
			{
				highestAddress = address;
			}

			memoryRanges.addAddress(address);

			flags |= insn.Flags;
		}

		public virtual int IsBranchTarget
		{
			set
			{
				if (log.TraceEnabled)
				{
					log.trace("CodeBlock.setIsBranchTarget 0x" + value.ToString("x").ToUpper());
				}
    
				CodeInstruction codeInstruction = getCodeInstruction(value);
				if (codeInstruction != null)
				{
					codeInstruction.BranchTarget = true;
				}
			}
		}

		public virtual int StartAddress
		{
			get
			{
				return startAddress;
			}
		}

		public virtual int LowestAddress
		{
			get
			{
				return lowestAddress;
			}
		}

		public virtual int HighestAddress
		{
			get
			{
				return highestAddress;
			}
		}

		public virtual int Length
		{
			get
			{
				return (HighestAddress - LowestAddress) / 4 + 1;
			}
		}

		public virtual CodeInstruction getCodeInstruction(int address)
		{
			if (currentSequence != null)
			{
				return currentSequence.CodeSequence.getCodeInstruction(address);
			}

			foreach (CodeInstruction codeInstruction in codeInstructions)
			{
				if (codeInstruction.Address == address)
				{
					return codeInstruction;
				}
			}

			return null;
		}

		public virtual int getCodeInstructionOpcode(int rawAddress)
		{
			int address = rawAddress & Memory.addressMask;
			return memoryRanges.getValue(address);
		}

		public virtual string ClassName
		{
			get
			{
				return CompilerContext.getClassName(StartAddress, InstanceIndex);
			}
		}

		public virtual string InternalClassName
		{
			get
			{
				return getInternalName(ClassName);
			}
		}

		private string getInternalName(string name)
		{
			return name.Replace('.', '/');
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private Class<IExecutable> loadExecutable(CompilerContext context, String className, byte[] bytes) throws ClassFormatError
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
		private Type<IExecutable> loadExecutable(CompilerContext context, string className, sbyte[] bytes)
		{
			try
			{
				// Try to define a new class for this executable.
				return (Type<IExecutable>) context.ClassLoader.defineClass(className, bytes);
			}
			catch (ClassFormatError e)
			{
				// This exception is catched by the Compiler
				throw e;
			}
			catch (LinkageError)
			{
				// If the class already exists, try finding it in this context.
				try
				{
					return (Type<IExecutable>)context.ClassLoader.findClass(className);
				}
				catch (ClassNotFoundException)
				{
					// Return null if none of the above work.
					return null;
				}
			}
		}

		private void addConstructor(ClassVisitor cv)
		{
			MethodVisitor mv = cv.visitMethod(Opcodes.ACC_PUBLIC, "<init>", "()V", null, null);
			mv.visitCode();
			mv.visitVarInsn(Opcodes.ALOAD, 0);
			mv.visitMethodInsn(Opcodes.INVOKESPECIAL, objectInternalName, "<init>", "()V");
			mv.visitInsn(Opcodes.RETURN);
			mv.visitMaxs(1, 1);
			mv.visitEnd();
		}

		private void addNonStaticMethods(CompilerContext context, ClassVisitor cv)
		{
			MethodVisitor mv;

			// public int exec(int returnAddress, int alternativeReturnAddress, boolean isJump) throws Exception;
			mv = cv.visitMethod(Opcodes.ACC_PUBLIC, context.ExecMethodName, context.ExecMethodDesc, null, exceptions);
			mv.visitCode();
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, ClassName, context.StaticExecMethodName, context.StaticExecMethodDesc);
			mv.visitInsn(Opcodes.IRETURN);
			mv.visitMaxs(1, 1);
			mv.visitEnd();

			// private static IExecutable e;
			FieldVisitor fv = cv.visitField(Opcodes.ACC_PRIVATE | Opcodes.ACC_STATIC, context.ReplaceFieldName, executableDescriptor, null, null);
			fv.visitEnd();

			// public void setExecutable(IExecutable e);
			mv = cv.visitMethod(Opcodes.ACC_PUBLIC, context.ReplaceMethodName, context.ReplaceMethodDesc, null, exceptions);
			mv.visitCode();
			mv.visitVarInsn(Opcodes.ALOAD, 1);
			mv.visitFieldInsn(Opcodes.PUTSTATIC, ClassName, context.ReplaceFieldName, executableDescriptor);
			mv.visitInsn(Opcodes.RETURN);
			mv.visitMaxs(1, 2);
			mv.visitEnd();

			// public IExecutable getExecutable();
			mv = cv.visitMethod(Opcodes.ACC_PUBLIC, context.GetMethodName, context.GetMethodDesc, null, exceptions);
			mv.visitCode();
			mv.visitFieldInsn(Opcodes.GETSTATIC, ClassName, context.ReplaceFieldName, executableDescriptor);
			mv.visitInsn(Opcodes.ARETURN);
			mv.visitMaxs(1, 1);
			mv.visitEnd();
		}

		private void addCodeSequence(IList<CodeSequence> codeSequences, CodeSequence codeSequence)
		{
			if (codeSequence != null)
			{
				if (codeSequence.Length > 1)
				{
					codeSequences.Add(codeSequence);
				}
			}
		}

		private void generateCodeSequences(IList<CodeSequence> codeSequences, int sequenceMaxInstructions)
		{
			CodeSequence currentCodeSequence = null;

			int nextAddress = 0;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int sequenceMaxInstructionsWithDelay = sequenceMaxInstructions - 1;
			int sequenceMaxInstructionsWithDelay = sequenceMaxInstructions - 1;
			foreach (CodeInstruction codeInstruction in codeInstructions)
			{
				int address = codeInstruction.Address;
				if (address < nextAddress)
				{
					// Skip it
				}
				else
				{
					if (codeInstruction.hasFlags(Instruction.FLAG_CANNOT_BE_SPLIT))
					{
						addCodeSequence(codeSequences, currentCodeSequence);
						currentCodeSequence = null;
						if (codeInstruction.hasFlags(Instruction.FLAG_HAS_DELAY_SLOT))
						{
							nextAddress = address + 8;
						}
					}
					else if (codeInstruction.BranchTarget)
					{
						addCodeSequence(codeSequences, currentCodeSequence);
						currentCodeSequence = new CodeSequence(address);
					}
					else
					{
						if (currentCodeSequence == null)
						{
							currentCodeSequence = new CodeSequence(address);
						}
						else if (currentCodeSequence.Length + codeInstruction.Length > sequenceMaxInstructionsWithDelay)
						{
							bool doSplit = false;
							if (currentCodeSequence.Length + codeInstruction.Length > sequenceMaxInstructions)
							{
								doSplit = true;
							}
							else if (codeInstruction.hasFlags(Instruction.FLAG_HAS_DELAY_SLOT))
							{
								doSplit = true;
							}
							if (doSplit)
							{
								addCodeSequence(codeSequences, currentCodeSequence);
								currentCodeSequence = new CodeSequence(address);
							}
						}
						currentCodeSequence.EndAddress = codeInstruction.EndAddress;
					}
				}
			}

			addCodeSequence(codeSequences, currentCodeSequence);
		}

		private CodeSequence findCodeSequence(CodeInstruction codeInstruction, IList<CodeSequence> codeSequences, CodeSequence currentCodeSequence)
		{
			int address = codeInstruction.Address;

			if (currentCodeSequence != null)
			{
				if (currentCodeSequence.isInside(address))
				{
					return currentCodeSequence;
				}
			}

			foreach (CodeSequence codeSequence in codeSequences)
			{
				if (codeSequence.isInside(address))
				{
					return codeSequence;
				}
			}

			return null;
		}

		private void splitCodeSequences(CompilerContext context, int methodMaxInstructions)
		{
			IList<CodeSequence> codeSequences = new List<CodeSequence>();

			generateCodeSequences(codeSequences, methodMaxInstructions);
			codeSequences.Sort();

			int currentMethodInstructions = codeInstructions.Count;
			IList<CodeSequence> sequencesToBeSplit = new List<CodeSequence>();
			foreach (CodeSequence codeSequence in codeSequences)
			{
				sequencesToBeSplit.Add(codeSequence);
				//if (log.DebugEnabled)
				{
					Console.WriteLine("Sequence to be split: " + codeSequence.ToString());
				}
				currentMethodInstructions -= codeSequence.Length;
				if (currentMethodInstructions <= methodMaxInstructions)
				{
					break;
				}
			}

			CodeSequence currentCodeSequence = null;
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
			for (IEnumerator<CodeInstruction> lit = codeInstructions.GetEnumerator(); lit.MoveNext();)
			{
				CodeInstruction codeInstruction = lit.Current;
				CodeSequence codeSequence = findCodeSequence(codeInstruction, sequencesToBeSplit, currentCodeSequence);
				if (codeSequence != null)
				{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					lit.remove();
					if (codeSequence.Instructions.Count == 0)
					{
						codeSequence.addInstruction(codeInstruction);
						SequenceCodeInstruction sequenceCodeInstruction = new SequenceCodeInstruction(codeSequence);
						lit.add(sequenceCodeInstruction);
						sequenceCodeInstructions.AddLast(sequenceCodeInstruction);
					}
					else
					{
						codeSequence.addInstruction(codeInstruction);
					}
					currentCodeSequence = codeSequence;
				}
			}
		}

		private void scanNativeCodeSequences(CompilerContext context)
		{
			NativeCodeManager nativeCodeManager = context.NativeCodeManager;
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
			for (IEnumerator<CodeInstruction> lit = codeInstructions.GetEnumerator(); lit.MoveNext();)
			{
				CodeInstruction codeInstruction = lit.Current;
				NativeCodeSequence nativeCodeSequence = nativeCodeManager.getNativeCodeSequence(codeInstruction, this);
				if (nativeCodeSequence != null)
				{
					if (nativeCodeSequence.Hook)
					{
						HookCodeInstruction hookCodeInstruction = new HookCodeInstruction(nativeCodeSequence, codeInstruction);

						// Replace the current code instruction by the hook code instruction
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						lit.remove();
						lit.add(hookCodeInstruction);
					}
					else
					{
						NativeCodeInstruction nativeCodeInstruction = new NativeCodeInstruction(codeInstruction.Address, nativeCodeSequence);

						if (nativeCodeInstruction.Branching)
						{
							IsBranchTarget = nativeCodeInstruction.BranchingTo;
						}

						if (nativeCodeSequence.WholeCodeBlock)
						{
							codeInstructions.Clear();
							codeInstructions.AddLast(nativeCodeInstruction);
						}
						else
						{
							// Remove the first opcode that started this native code sequence
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
							lit.remove();

							// Add any code instructions that need to be inserted before
							// the native code sequence
							IList<CodeInstruction> beforeCodeInstructions = nativeCodeSequence.BeforeCodeInstructions;
							if (beforeCodeInstructions != null)
							{
								foreach (CodeInstruction beforeCodeInstruction in beforeCodeInstructions)
								{
									CodeInstruction newCodeInstruction = new CodeInstruction(beforeCodeInstruction);
									newCodeInstruction.Address = codeInstruction.Address;

									lit.add(newCodeInstruction);
								}
							}

							// Add the native code sequence itself
							lit.add(nativeCodeInstruction);

							// Remove the further opcodes from the native code sequence
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							for (int i = nativeCodeSequence.NumOpcodes - 1; i > 0 && lit.hasNext(); i--)
							{
								lit.Current;
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
								lit.remove();
							}
						}
					}
				}
			}
		}

		private void prepare(CompilerContext context, int methodMaxInstructions)
		{
			memoryRanges.updateValues();

			scanNativeCodeSequences(context);

			if (codeInstructions.Count > methodMaxInstructions)
			{
				if (log.InfoEnabled)
				{
					log.info("Splitting " + ClassName + " (" + codeInstructions.Count + "/" + methodMaxInstructions + ")");
				}
				splitCodeSequences(context, methodMaxInstructions);
			}
		}

		private void compile(CompilerContext context, MethodVisitor mv, IList<CodeInstruction> codeInstructions)
		{
			context.optimizeSequence(codeInstructions);

			int numberInstructionsToBeSkipped = 0;
			foreach (CodeInstruction codeInstruction in codeInstructions)
			{
				if (numberInstructionsToBeSkipped > 0)
				{
					if (!context.SkipDelaySlot && codeInstruction.BranchTarget)
					{
						context.compileDelaySlotAsBranchTarget(codeInstruction);
					}
					numberInstructionsToBeSkipped--;

					if (numberInstructionsToBeSkipped <= 0)
					{
						context.skipInstructions(0, false);
					}
				}
				else
				{
					codeInstruction.compile(context, mv);
					numberInstructionsToBeSkipped = context.NumberInstructionsToBeSkipped;
				}
			}
		}

		private Type<IExecutable> interpret(CompilerContext context)
		{
			Type<IExecutable> compiledClass = null;

			context.CodeBlock = this;
			string className = InternalClassName;
			if (log.InfoEnabled)
			{
				log.info("Compiling for Interpreter " + className);
			}

			int computeFlag = ClassWriter.COMPUTE_FRAMES;
			if (context.AutomaticMaxLocals || context.AutomaticMaxStack)
			{
				computeFlag |= ClassWriter.COMPUTE_MAXS;
			}
			ClassWriter cw = new ClassWriter(computeFlag);
			ClassVisitor cv = cw;
			//if (log.DebugEnabled)
			{
				cv = new CheckClassAdapter(cv);
			}

			StringWriter debugOutput = null;
			//if (log.DebugEnabled)
			{
				debugOutput = new StringWriter();
				PrintWriter debugPrintWriter = new PrintWriter(debugOutput);
				cv = new TraceClassVisitor(cv, debugPrintWriter);
			}
			cv.visit(Opcodes.V1_6, Opcodes.ACC_PUBLIC | Opcodes.ACC_SUPER, className, null, objectInternalName, interfacesForExecutable);
			context.startClass(cv);

			addConstructor(cv);
			addNonStaticMethods(context, cv);

			MethodVisitor mv = cv.visitMethod(Opcodes.ACC_PUBLIC | Opcodes.ACC_STATIC, context.StaticExecMethodName, context.StaticExecMethodDesc, null, exceptions);
			mv.visitCode();
			context.MethodVisitor = mv;
			context.startMethod();

			context.compileExecuteInterpreter(StartAddress);

			mv.visitMaxs(context.MaxStack, context.MaxLocals);
			mv.visitEnd();

			cv.visitEnd();

			if (debugOutput != null)
			{
				Console.WriteLine(debugOutput.ToString());
			}

			compiledClass = loadExecutable(context, className, cw.toByteArray());

			return compiledClass;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Class<IExecutable> compile(CompilerContext context) throws ClassFormatError
		private Type<IExecutable> compile(CompilerContext context)
		{
			Type<IExecutable> compiledClass = null;

			context.CodeBlock = this;
			string className = InternalClassName;
			//if (log.DebugEnabled)
			{
				string functionName = Utilities.getFunctionNameByAddress(StartAddress);

				if (!string.ReferenceEquals(functionName, null))
				{
					Console.WriteLine(string.Format("Compiling {0} ({1})", className, functionName));
				}
				else
				{
					Console.WriteLine(string.Format("Compiling {0}", className));
				}
			}

			prepare(context, context.MethodMaxInstructions);

			currentSequence = null;
			int computeFlag = ClassWriter.COMPUTE_FRAMES;
			if (context.AutomaticMaxLocals || context.AutomaticMaxStack)
			{
				computeFlag |= ClassWriter.COMPUTE_MAXS;
			}
			ClassWriter cw = new ClassWriter(computeFlag);
			ClassVisitor cv = cw;
			//if (log.DebugEnabled)
			{
				cv = new CheckClassAdapter(cv);
			}
			StringWriter debugOutput = null;
			if (log.TraceEnabled)
			{
				debugOutput = new StringWriter();
				PrintWriter debugPrintWriter = new PrintWriter(debugOutput);
				cv = new TraceClassVisitor(cv, debugPrintWriter);
			}
			cv.visit(Opcodes.V1_6, Opcodes.ACC_PUBLIC | Opcodes.ACC_SUPER, className, null, objectInternalName, interfacesForExecutable);
			context.startClass(cv);

			addConstructor(cv);
			addNonStaticMethods(context, cv);

			MethodVisitor mv = cv.visitMethod(Opcodes.ACC_PUBLIC | Opcodes.ACC_STATIC, context.StaticExecMethodName, context.StaticExecMethodDesc, null, exceptions);
			mv.visitCode();
			context.MethodVisitor = mv;
			context.startMethod();

			// Jump to the block start if other instructions have been inserted in front
			if (codeInstructions.Count > 0 && codeInstructions.First.Value.Address != StartAddress)
			{
				mv.visitJumpInsn(Opcodes.GOTO, getCodeInstruction(StartAddress).Label);
			}

			compile(context, mv, codeInstructions);
			mv.visitMaxs(context.MaxStack, context.MaxLocals);
			mv.visitEnd();

			foreach (SequenceCodeInstruction sequenceCodeInstruction in sequenceCodeInstructions)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine("Compiling Sequence " + sequenceCodeInstruction.getMethodName(context));
				}
				currentSequence = sequenceCodeInstruction;
				mv = cv.visitMethod(Opcodes.ACC_PUBLIC | Opcodes.ACC_STATIC, sequenceCodeInstruction.getMethodName(context), "()V", null, exceptions);
				mv.visitCode();
				context.MethodVisitor = mv;
				context.startSequenceMethod();

				compile(context, mv, sequenceCodeInstruction.CodeSequence.Instructions);

				context.endSequenceMethod();
				mv.visitMaxs(context.MaxStack, context.MaxLocals);
				mv.visitEnd();
			}
			currentSequence = null;

			cv.visitEnd();

			if (debugOutput != null)
			{
				log.trace(debugOutput.ToString());
			}

			try
			{
				compiledClass = loadExecutable(context, className, cw.toByteArray());
			}
			catch (System.NullReferenceException e)
			{
				Console.WriteLine("Error while compiling " + className + ": " + e);
			}

			return compiledClass;
		}

		public virtual IExecutable Executable
		{
			get
			{
				return executable;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public synchronized IExecutable getExecutable(CompilerContext context) throws ClassFormatError
		public virtual IExecutable getExecutable(CompilerContext context)
		{
			lock (this)
			{
				if (executable == null)
				{
					Type<IExecutable> classExecutable = compile(context);
					if (classExecutable != null)
					{
						try
						{
							executable = System.Activator.CreateInstance(classExecutable);
						}
						catch (InstantiationException e)
						{
							Console.WriteLine(e);
						}
						catch (IllegalAccessException e)
						{
							Console.WriteLine(e);
						}
					}
				}
        
				return executable;
			}
		}

		public virtual IExecutable getInterpretedExecutable(CompilerContext context)
		{
			lock (this)
			{
				if (executable == null)
				{
					Type<IExecutable> classExecutable = interpret(context);
					if (classExecutable != null)
					{
						try
						{
							executable = System.Activator.CreateInstance(classExecutable);
						}
						catch (InstantiationException e)
						{
							Console.WriteLine(e);
						}
						catch (IllegalAccessException e)
						{
							Console.WriteLine(e);
						}
					}
				}
        
				return executable;
			}
		}

		public virtual int InstanceIndex
		{
			get
			{
				return instanceIndex;
			}
		}

		public virtual int NewInstanceIndex
		{
			get
			{
				instanceIndex++;
				return instanceIndex;
			}
		}

		public virtual Instruction[] InterpretedInstructions
		{
			get
			{
				return interpretedInstructions;
			}
			set
			{
				this.interpretedInstructions = value;
			}
		}


		public virtual int[] InterpretedOpcodes
		{
			get
			{
				return interpretedOpcodes;
			}
			set
			{
				this.interpretedOpcodes = value;
			}
		}


		public virtual bool areOpcodesChanged()
		{
			return memoryRanges.areValuesChanged();
		}

		public virtual bool isOverlappingWithAddressRange(int address, int size)
		{
			// Fast check against the lowest & highest addresses
			if (address > (highestAddress & Memory.addressMask))
			{
				return false;
			}
			if (address + size < (lowestAddress & Memory.addressMask))
			{
				return false;
			}

			// Full check on all the memory ranges
			return memoryRanges.isOverlappingWithAddressRange(address, size);
		}

		public virtual bool Internal
		{
			get
			{
				int addr = StartAddress;
				return addr < INTERNAL_THREAD_ADDRESS_END && addr >= INTERNAL_THREAD_ADDRESS_START;
			}
		}

		public virtual int Flags
		{
			get
			{
				return flags;
			}
		}

		public virtual bool hasFlags(int testFlags)
		{
			return (flags & testFlags) == testFlags;
		}

		public virtual void addCodeBlock()
		{
			RuntimeContext.addCodeBlock(startAddress, this);
		}

		public virtual HLEModuleFunction HLEFunction
		{
			get
			{
				return hleFunction;
			}
		}

		public virtual void setHLEFunction(HLEModuleFunction hleFunction)
		{
			this.hleFunction = hleFunction;
		}

		public virtual bool isHLEFunction()
		{
			return hleFunction != null;
		}

		public virtual IAction UpdateOpcodesAction
		{
			get
			{
				return updateOpcodesAction;
			}
			set
			{
				this.updateOpcodesAction = value;
			}
		}


		public override string ToString()
		{
			return string.Format("CodeBlock 0x{0:X8}[0x{1:X8}-0x{2:X8}]", StartAddress, LowestAddress, HighestAddress);
		}
	}

}