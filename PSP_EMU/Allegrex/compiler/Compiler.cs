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
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_ENDS_BLOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_IS_BRANCHING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_IS_JUMPING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_STARTS_NEW_BLOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_SYSCALL;



	using Instruction = pspsharp.Allegrex.Common.Instruction;
	using NativeCodeManager = pspsharp.Allegrex.compiler.nativeCode.NativeCodeManager;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemorySections = pspsharp.memory.MemorySections;
	using MMIO = pspsharp.memory.mmio.MMIO;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using AbstractIntSettingsListener = pspsharp.settings.AbstractIntSettingsListener;
	using Settings = pspsharp.settings.Settings;
	using CpuDurationStatistics = pspsharp.util.CpuDurationStatistics;
	using DurationStatistics = pspsharp.util.DurationStatistics;

	//using Logger = org.apache.log4j.Logger;
	using Document = org.w3c.dom.Document;
	using SAXException = org.xml.sax.SAXException;

	/*
	 * TODO to cleanup the code:
	 * - add flags to Common.Instruction:
	 *     - isBranching (see branchBlockInstructions list below) [DONE]
	 *     - is end of CodeBlock (see endBlockInstructions list below) [DONE]
	 *     - is starting a new CodeBlock (see newBlockInstructions list below) [DONE]
	 *     - isBranching unconditional [DONE]
	 *     - isBranching with 16 bits target [DONE]
	 *     - isBranching with 26 bits target (see jumpBlockInstructions list below) [DONE]
	 *     - is jump or call [DONE]
	 *     - is compiled or interpreted [DONE]
	 *     - is referencing $pc register
	 *     - can switch thread context
	 *     - is loading $sp address to another register
	 *     - is reading Rs
	 *     - is reading Rt
	 *     - is writing Rt [DONE]
	 *     - is writing Rd [DONE]
	 *
	 * TODO Ideas to further enhance performance:
	 * - store stack variables "nn(sp)" into local variables
	 *   if CodeBlock is following standard MIPS call conventions and
	 *   if sp is not loaded into another register
	 * - store registers in local variables and flush registers back to gpr[] when calling
	 *   subroutine (if CodeBlock is following standard MIPS call conventions).
	 *     subroutine parameters: $a0-$a3
	 *     callee-saved registers: $fp, $s0-$s7
	 *     caller-saved registers: $ra, $t0-$t9, $a0-$a3
	 *     Register description:
	 *       $zr (0): 0
	 *       $at (1): reserved for assembler
	 *       $v0-$v1 (2-3): expression evaluation & function results
	 *       $a0-$a3 (4-7): arguments, not preserved across subroutine calls
	 *       $t0-$t7 (8-15): temporary: caller saves, not preserved across subroutine calls
	 *       $s0-$s7 (16-23): callee saves, must be preserved across subroutine calls
	 *       $t8-$t9 (24-25): temporary (cont'd)
	 *       $k0-$k1 (26-27): reserved for OS kernel
	 *       $gp (28): Pointer to global area (Global Pointer)
	 *       $sp (29): Stack Pointer
	 *       $fp (30): Frame Pointer, must be preserved across subroutine calls
	 *       $ra (31): Return Address
	 *  - provide bytecode compilation for mostly used instructions
	 *    e.g.: typical program is using the following instructions based on frequency usage:
	 *            ADDIU 8979678 (14,1%)
	 *             ADDU 6340735 ( 9,9%)
	 *               LW 5263292 ( 8,3%)
	 *               SW 5223074 ( 8,2%)
	 *              LUI 3930978 ( 6,2%)
	 *             ANDI 2833815 ( 4,4%)
	 *               SB 2812802 ( 4,4%)
	 *              BNE 2176061 ( 3,4%)
	 *              SRL 1919480 ( 3,0%)
	 *               OR 1888281 ( 3,0%)
	 *              AND 1411970 ( 2,2%)
	 *              SLL 1406954 ( 2,2%)
	 *              JAL 1297959 ( 2,0%)
	 *              NOP 1272929 ( 2,0%)
	 *             SRAV 1132740
	 *               JR 1124095
	 *              LBU 1068533
	 *              BEQ 1032768
	 *              LHU 1011791
	 *            BGTZL 943264
	 *             MFLO 680659
	 *             MULT 679357
	 *              ORI 503067
	 *                J 453007
	 *             SLTI 432961
	 *              SRA 293213
	 *             BEQL 228478
	 *          SYSCALL 215713
	 *             BGEZ 168494
	 *             BNEL 159020
	 *             SLTU 124580
	 *  - implement subroutine arguments as Java arguments instead of $a0-$a3.
	 *    Automatically detect how many arguments are expected by a subroutine.
	 *    Generate different Java classes if number of expected arguments is refined
	 *    over time (include number in Class name).
	 */
	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class Compiler : ICompiler
	{
		//public static Logger log = Logger.getLogger("compiler");
		private static Compiler instance;
		private static int resetCount = 0;
		private CompilerClassLoader classLoader;
		public static CpuDurationStatistics compileDuration = new CpuDurationStatistics("Compilation Time");
		private Document configuration;
		private NativeCodeManager nativeCodeManager;
		private bool ignoreInvalidMemory = false;
		public int defaultMethodMaxInstructions = 3000;
		private const int maxRecompileExecutable = 50;
		private CompilerTypeManager compilerTypeManager;
		private HashSet<int> interpretedAddresses = new HashSet<int>();
		private ISet<int> useMMIOAddresses = new HashSet<int>();

		private class IgnoreInvalidMemoryAccessSettingsListerner : AbstractBoolSettingsListener
		{
			private readonly Compiler outerInstance;

			public IgnoreInvalidMemoryAccessSettingsListerner(Compiler outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override void settingsValueChanged(bool value)
			{
				outerInstance.IgnoreInvalidMemory = value;
			}
		}

		private class MethodMaxInstructionsSettingsListerner : AbstractIntSettingsListener
		{
			private readonly Compiler outerInstance;

			public MethodMaxInstructionsSettingsListerner(Compiler outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override void settingsValueChanged(int value)
			{
				outerInstance.DefaultMethodMaxInstructions = value;
			}
		}

		private bool IgnoreInvalidMemory
		{
			get
			{
				return ignoreInvalidMemory;
			}
			set
			{
				ignoreInvalidMemory = value;
			}
		}


		public static Compiler Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Compiler();
				}
    
				return instance;
			}
		}

		private Compiler()
		{
			Initialise();
		}

		public static void exit()
		{
			if (instance != null)
			{
				if (DurationStatistics.collectStatistics)
				{
					log.info(compileDuration);
				}
			}
		}

		public virtual void reset()
		{
			resetCount++;
			classLoader = new CompilerClassLoader(this);
			compileDuration.reset();
			nativeCodeManager.reset();
		}

		public virtual void invalidateAll()
		{
			// Simply generate a new class loader.
			log.info("Compiler: invalidating all compiled classes");
			classLoader = new CompilerClassLoader(this);
		}

		public virtual bool checkSimpleInterpretedCodeBlock(CodeBlock codeBlock)
		{
			bool isSimple = true;
			int insnCount = 0;
			Common.Instruction[] insns = new Common.Instruction[100];
			int[] opcodes = new int[100];
			int opcodeJrRa = AllegrexOpcodes.JR | (Common._ra << 21); // jr $ra

			IMemoryReader memoryReader = MemoryReader.getMemoryReader(codeBlock.StartAddress, 4);
			int notSimpleFlags = FLAG_IS_BRANCHING | FLAG_IS_JUMPING | FLAG_STARTS_NEW_BLOCK | FLAG_ENDS_BLOCK;
			while (true)
			{
				if (insnCount >= insns.Length)
				{
					// Extend insns array
					Common.Instruction[] newInsns = new Common.Instruction[insnCount + 100];
					Array.Copy(insns, 0, newInsns, 0, insnCount);
					insns = newInsns;
					// Extend opcodes array
					int[] newOpcodes = new int[newInsns.Length];
					Array.Copy(opcodes, 0, newOpcodes, 0, insnCount);
					opcodes = newOpcodes;
				}

				int opcode = memoryReader.readNext();

				if (opcode == opcodeJrRa)
				{
					int delaySlotOpcode = memoryReader.readNext();
					Common.Instruction delaySlotInsn = Decoder.instruction(delaySlotOpcode);
					insns[insnCount] = delaySlotInsn;
					opcodes[insnCount] = delaySlotOpcode;
					insnCount++;
					break;
				}

				Common.Instruction insn = Decoder.instruction(opcode);
				if ((insn.Flags & notSimpleFlags) != 0)
				{
					isSimple = false;
					break;
				}

				insns[insnCount] = insn;
				opcodes[insnCount] = opcode;
				insnCount++;
			}

			if (isSimple)
			{
				if (insnCount < insns.Length)
				{
					// Compact insns array
					Common.Instruction[] newInsns = new Common.Instruction[insnCount];
					Array.Copy(insns, 0, newInsns, 0, insnCount);
					insns = newInsns;
					// Compact opcodes array
					int[] newOpcodes = new int[insnCount];
					Array.Copy(opcodes, 0, newOpcodes, 0, insnCount);
					opcodes = newOpcodes;
				}
				codeBlock.InterpretedInstructions = insns;
				codeBlock.InterpretedOpcodes = opcodes;
			}
			else
			{
				codeBlock.InterpretedInstructions = null;
				codeBlock.InterpretedOpcodes = null;
			}

			return isSimple;
		}

		public virtual void invalidateCodeBlock(CodeBlock codeBlock)
		{
			IExecutable executable = codeBlock.Executable;
			if (executable != null)
			{
				// If the application is invalidating the same code block too many times,
				// do no longer try to recompile it each time, interpret it.
				if (codeBlock.InstanceIndex > maxRecompileExecutable)
				{
					executable.Executable = new InterpretExecutable(codeBlock);
				}
				else
				{
					// Force a recompilation of the codeBlock at the next execution
					executable.Executable = new RecompileExecutable(codeBlock);
				}
			}

			NativeCodeManager.invalidateCompiledNativeCodeBlocks(codeBlock.LowestAddress, codeBlock.HighestAddress);
		}

		public virtual void checkCodeBlockValidity(CodeBlock codeBlock)
		{
			if (codeBlock.Executable == null)
			{
				// This code block is interpreted, no need to check
				return;
			}
			if (codeBlock.Executable.Executable is InvalidatedExecutable)
			{
				// This code block has already been invalidated (will be checked for changes or recompiled)
				return;
			}

			if (codeBlock.areOpcodesChanged())
			{
				IAction updateOpcodesAction = codeBlock.UpdateOpcodesAction;
				if (updateOpcodesAction != null)
				{
					// Execute the action provided by the code block to update the opcodes
					updateOpcodesAction.execute();
				}
				else
				{
					// This is the default action when the opcodes has been updated
					invalidateCodeBlock(codeBlock);
				}
			}
			else
			{
				// The opcodes of the code block could get updated by the application "after" calling an icache instruction.
				// Check if the opcodes have been updated the next time the code block is executed.
				codeBlock.Executable.Executable = new CheckChangedExecutable(codeBlock);
			}
		}

		private void Initialise()
		{
			Settings.Instance.registerSettingsListener("Compiler", "emu.ignoreInvalidMemoryAccess", new IgnoreInvalidMemoryAccessSettingsListerner(this));
			Settings.Instance.registerSettingsListener("Compiler", "emu.compiler.methodMaxInstructions", new MethodMaxInstructionsSettingsListerner(this));

			DocumentBuilderFactory documentBuilderFactory = DocumentBuilderFactory.newInstance();
			documentBuilderFactory.IgnoringElementContentWhitespace = true;
			documentBuilderFactory.IgnoringComments = true;
			documentBuilderFactory.Coalescing = true;
			configuration = null;
			try
			{
				DocumentBuilder documentBuilder = documentBuilderFactory.newDocumentBuilder();
				configuration = documentBuilder.parse(new File("Compiler.xml"));
			}
			catch (ParserConfigurationException e)
			{
				Console.WriteLine(e);
			}
			catch (SAXException e)
			{
				Console.WriteLine(e);
			}
			catch (IOException e)
			{
				Console.WriteLine(e);
			}

			if (configuration != null)
			{
				nativeCodeManager = new NativeCodeManager(configuration.DocumentElement);
			}
			else
			{
				nativeCodeManager = new NativeCodeManager(null);
			}

			compilerTypeManager = new CompilerTypeManager();

			reset();
		}

		public static int jumpTarget(int pc, int opcode)
		{
			return (pc & unchecked((int)0xF0000000)) | ((opcode & 0x03FFFFFF) << 2);
		}

		public static int branchTarget(int pc, int opcode)
		{
			return pc + ((unchecked((short)(opcode & 0x0000FFFF))) << 2);
		}

		private IExecutable interpret(CompilerContext context, int startAddress, int instanceIndex)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Compiler.interpret Block 0x{0:X8}", startAddress));
			}
			CodeBlock codeBlock = new CodeBlock(startAddress, instanceIndex);

			codeBlock.addCodeBlock();

			IExecutable executable = codeBlock.getInterpretedExecutable(context);
			//if (log.DebugEnabled)
			{
				Console.WriteLine("Executable: " + executable);
			}

			return executable;
		}

		public static bool isEndBlockInsn(int pc, int opcode, Common.Instruction insn)
		{
			if (insn.hasFlags(Common.Instruction.FLAG_ENDS_BLOCK))
			{
				if (insn.hasFlags(Common.Instruction.FLAG_IS_CONDITIONAL | Common.Instruction.FLAG_IS_BRANCHING))
				{
					// Detect the conditional
					//    "BEQ $xx, $xx, target"
					// which is equivalent to the unconditional
					//    "B target"
					if (insn == Instructions.BEQ)
					{
						int rt = (opcode >> 16) & 0x1F;
						int rs = (opcode >> 21) & 0x1F;
						if (rs == rt)
						{
							return true;
						}
					}
					else
					{
						Console.WriteLine(string.Format("Unknown conditional instruction ending a block: {0}", insn.disasm(pc, opcode)));
					}
				}
				else
				{
					return true;
				}
			}

			return false;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private IExecutable analyse(CompilerContext context, int startAddress, boolean recursive, int instanceIndex) throws ClassFormatError
		private IExecutable analyse(CompilerContext context, int startAddress, bool recursive, int instanceIndex)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("Compiler.analyse Block 0x{0:X8}", startAddress));
			}
			int maxBranchInstructions = int.MaxValue; // 5 for FRONTIER_1337 homebrew
			MemorySections memorySections = MemorySections.Instance;
			CodeBlock codeBlock = new CodeBlock(startAddress, instanceIndex);
			Stack<int> pendingBlockAddresses = new Stack<int>();
			pendingBlockAddresses.Clear();
			pendingBlockAddresses.Push(startAddress);
			ISet<int> branchingToAddresses = new HashSet<int>();
			while (pendingBlockAddresses.Count > 0)
			{
				int pc = pendingBlockAddresses.Pop();
				if (!isAddressGood(pc))
				{
					if (IgnoreInvalidMemory)
					{
						Console.WriteLine(string.Format("IGNORING: Trying to compile an invalid address 0x{0:X8} while compiling from 0x{1:X8}", pc, startAddress));
					}
					else
					{
						Console.WriteLine(string.Format("Trying to compile an invalid address 0x{0:X8} while compiling from 0x{1:X8}", pc, startAddress));
					}
					return null;
				}
				bool isBranchTarget = true;
				int endPc = MemoryMap.END_RAM;

				// Handle branching to a delayed instruction.
				// The delayed instruction has already been analysed, but the next
				// address maybe not.
				if (context.analysedAddresses.Contains(pc) && !context.analysedAddresses.Contains(pc + 4))
				{
					pc += 4;
				}

				if (context.analysedAddresses.Contains(pc) && isBranchTarget)
				{
					codeBlock.IsBranchTarget = pc;
				}
				else
				{
					IMemoryReader memoryReader = MemoryReader.getMemoryReader(pc, 4);
					while (!context.analysedAddresses.Contains(pc) && pc <= endPc)
					{
						int opcode = memoryReader.readNext();

						Common.Instruction insn = Decoder.instruction(opcode);

						context.analysedAddresses.Add(pc);
						int npc = pc + 4;

						int branchingTo = 0;
						bool isBranching = false;
						bool checkDynamicBranching = false;
						if (insn.hasFlags(Common.Instruction.FLAG_IS_BRANCHING))
						{
							branchingTo = branchTarget(npc, opcode);
							isBranching = true;
						}
						else if (insn.hasFlags(Common.Instruction.FLAG_IS_JUMPING))
						{
							branchingTo = jumpTarget(npc, opcode);
							isBranching = true;
							checkDynamicBranching = true;
						}

						if (isEndBlockInsn(pc, opcode, insn))
						{
							endPc = npc;
						}
						else if (pc < endPc && insn.hasFlags(Common.Instruction.FLAG_SYSCALL))
						{
							endPc = pc;
						}

						if (insn.hasFlags(Common.Instruction.FLAG_STARTS_NEW_BLOCK))
						{
							if (recursive)
							{
								context.blocksToBeAnalysed.Push(branchingTo);
							}
						}
						else if (isBranching)
						{
							if (branchingTo != 0)
							{ // Ignore "J 0x00000000" instruction
								bool analyseBranch = true;
								if (maxBranchInstructions < 0)
								{
									analyseBranch = false;
								}
								else
								{
									maxBranchInstructions--;

									// Analyse only the jump instructions that are jumping to
									// non-writable memory sections. A jump to a writable memory
									// section has to be interpreted at runtime to check if the
									// reached code has not been changed (i.e. invalidated).
									if (checkDynamicBranching && memorySections.canWrite(branchingTo, false))
									{
										analyseBranch = false;
									}
								}

								if (analyseBranch)
								{
									pendingBlockAddresses.Push(branchingTo);
								}
								else
								{
									branchingToAddresses.Add(branchingTo);
								}
							}
						}

						bool useMMIO = useMMIOAddresses.Contains(pc & Memory.addressMask);

						codeBlock.addInstruction(pc, opcode, insn, isBranchTarget, isBranching, branchingTo, useMMIO);
						pc = npc;

						isBranchTarget = false;
					}
				}

				foreach (int branchingTo in branchingToAddresses)
				{
					codeBlock.IsBranchTarget = branchingTo;
				}
			}

			codeBlock.addCodeBlock();

			IExecutable executable;
			if (RuntimeContext.CompilerEnabled || codeBlock.hasFlags(FLAG_SYSCALL))
			{
				executable = codeBlock.getExecutable(context);
			}
			else
			{
				executable = null;
			}
			if (log.TraceEnabled)
			{
				log.trace("Executable: " + executable);
			}

			return executable;
		}

		public virtual void analyseRecursive(int startAddress, int instanceIndex)
		{
			if (RuntimeContext.hasCodeBlock(startAddress))
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine("Compiler.analyse 0x" + startAddress.ToString("x").ToUpper() + " - already analysed");
				}
				return;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine("Compiler.analyse 0x" + startAddress.ToString("x").ToUpper());
			}

			CompilerContext context = new CompilerContext(classLoader, instanceIndex);
			context.blocksToBeAnalysed.Push(startAddress);
			while (context.blocksToBeAnalysed.Count > 0)
			{
				int blockStartAddress = context.blocksToBeAnalysed.Pop();
				analyse(context, blockStartAddress, true, instanceIndex);
			}
		}

		public virtual IExecutable compile(string name)
		{
			return compile(CompilerContext.getClassAddress(name), CompilerContext.getClassInstanceIndex(name));
		}

		public virtual IExecutable compile(int address)
		{
			return compile(address, ResetCount);
		}

		private CompilerContext retryCompilation(CompilerContext context, int instanceIndex, int retries, Exception e)
		{
			// Try again with stricter methodMaxInstructions (75% of current value)
			int methodMaxInstructions = context.MethodMaxInstructions * 3 / 4;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Catched exception '{0}' (can be ignored)", e.ToString()));
				Console.WriteLine(string.Format("Retrying compilation again with maxInstruction={0:D}, retries left={1:D}...", methodMaxInstructions, retries - 1));
			}
			context = new CompilerContext(classLoader, instanceIndex);
			context.MethodMaxInstructions = methodMaxInstructions;

			return context;
		}

		private bool isAddressGood(int address)
		{
			if (Memory.isAddressGood(address))
			{
				return true;
			}
			if (interpretedAddresses.Contains(address))
			{
				return true;
			}
			if (RuntimeContextLLE.MMIO != null)
			{
				return MMIO.isAddressGood(address);
			}
			return false;
		}

		public virtual IExecutable compile(int address, int instanceIndex)
		{
			if (!isAddressGood(address))
			{
				if (IgnoreInvalidMemory)
				{
					Console.WriteLine(string.Format("IGNORING: Trying to compile an invalid address 0x{0:X8}", address));
				}
				else
				{
					Console.WriteLine(string.Format("Trying to compile an invalid address 0x{0:X8}", address));
					Emulator.PauseEmu();
				}
				return null;
			}

			// Disable the PSP clock while compiling. This could cause timing problems
			// in some applications while compiling large MIPS functions.
			Emulator.Clock.pause();

			long compilationStartMicros = 0;
			if (Profiler.ProfilerEnabled)
			{
				compilationStartMicros = System.nanoTime() / 1000;
			}

			CompilerContext context = null;
			CompilerContext lastContext = null;
			IExecutable executable = null;
			ClassFormatError error = null;
			Exception exception = null;

			if (interpretedAddresses.Contains(address))
			{
				// Force an interpreter call
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Forcing an interpreter call for address 0x{0:X8}", address));
				}
			}
			else
			{
				compileDuration.start();
				context = new CompilerContext(classLoader, instanceIndex);
				for (int retries = 2; retries > 0; retries--)
				{
					try
					{
						lastContext = context;
						executable = analyse(context, address, false, instanceIndex);
						break;
					}
					catch (ClassFormatError e)
					{
						// Catch exception
						//     java.lang.ClassFormatError: Invalid method Code Length nnnnnn in class file XXXX
						//
						error = e;

						context = retryCompilation(context, instanceIndex, retries, e);
					}
					catch (System.NullReferenceException e)
					{
						Console.WriteLine(string.Format("Catched exception '{0}' while compiling 0x{1:X8} (0x{2:X8}-0x{3:X8})", e.ToString(), address, context.CodeBlock.LowestAddress, context.CodeBlock.HighestAddress));
						break;
					}
					catch (VerifyError e)
					{
						Console.WriteLine(string.Format("Catched exception '{0}' while compiling 0x{1:X8} (0x{2:X8}-0x{3:X8})", e.ToString(), address, context.CodeBlock.LowestAddress, context.CodeBlock.HighestAddress));
						break;
					}
					catch (Exception e)
					{
						// Catch exception
						//     java.lang.RuntimeException: Method code too large!
						exception = e;

						context = retryCompilation(context, instanceIndex, retries, e);
					}
				}
				compileDuration.end();
			}

			if (Profiler.ProfilerEnabled)
			{
				long compilationEndMicros = System.nanoTime() / 1000;
				Profiler.addCompilation(compilationEndMicros - compilationStartMicros);
			}

			if (executable == null)
			{
				if (log.DebugEnabled && context != null)
				{
					Console.WriteLine(string.Format("Compilation failed with maxInstruction={0:D}", context.MethodMaxInstructions));
				}
				if (lastContext != null)
				{
					interpretedAddresses.addAll(lastContext.analysedAddresses);
				}
				context = new CompilerContext(classLoader, instanceIndex);
				executable = interpret(context, address, instanceIndex);
				if (executable == null)
				{
					if (error != null)
					{
						throw error;
					}
					if (exception != null)
					{
						throw exception;
					}
				}
			}
			else if (error != null)
			{
				if (log.DebugEnabled && context != null)
				{
					Console.WriteLine(string.Format("Compilation was now correct with maxInstruction={0:D}", context.MethodMaxInstructions));
				}
			}

			// Resume the PSP clock after compilation
			Emulator.Clock.resume();

			return executable;
		}

		public virtual CompilerClassLoader ClassLoader
		{
			get
			{
				return classLoader;
			}
			set
			{
				this.classLoader = value;
			}
		}


		public static int ResetCount
		{
			get
			{
				return resetCount;
			}
			set
			{
				Compiler.resetCount = value;
			}
		}


		public virtual NativeCodeManager NativeCodeManager
		{
			get
			{
				return nativeCodeManager;
			}
		}

		public virtual int DefaultMethodMaxInstructions
		{
			get
			{
				return defaultMethodMaxInstructions;
			}
			set
			{
				if (value > 0)
				{
					this.defaultMethodMaxInstructions = value;
    
					log.info(string.Format("Compiler MethodMaxInstructions: {0:D}", value));
				}
			}
		}


		public virtual CompilerTypeManager CompilerTypeManager
		{
			get
			{
				return compilerTypeManager;
			}
		}

		public virtual void addMMIORange(int startAddress, int Length)
		{
			startAddress &= Memory.addressMask;

			for (int i = 0; i < Length; i += 4)
			{
				useMMIOAddresses.Add(startAddress + i);
			}
		}
	}

}