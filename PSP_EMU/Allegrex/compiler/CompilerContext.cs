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
namespace pspsharp.Allegrex.compiler
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._a0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._a1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._a2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._a3;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._f0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._ra;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._sp;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._t0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._t1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._t2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._t3;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._t4;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._t5;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._t6;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._t7;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._t8;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._t9;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._v0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._v1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._zr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_MODIFIES_INTERRUPT_STATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.HLEModuleManager.HLESyscallNid;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.HLEModuleManager.InternalSyscallNid;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.SyscallHandler.syscallLoadCoreUnmappedImport;


	using Instruction = pspsharp.Allegrex.Common.Instruction;
	using Fcr31 = pspsharp.Allegrex.FpuState.Fcr31;
	using Vcr = pspsharp.Allegrex.VfpuState.Vcr;
	using PfxDst = pspsharp.Allegrex.VfpuState.Vcr.PfxDst;
	using PfxSrc = pspsharp.Allegrex.VfpuState.Vcr.PfxSrc;
	using NativeCodeInstruction = pspsharp.Allegrex.compiler.nativeCode.NativeCodeInstruction;
	using NativeCodeManager = pspsharp.Allegrex.compiler.nativeCode.NativeCodeManager;
	using NativeCodeSequence = pspsharp.Allegrex.compiler.nativeCode.NativeCodeSequence;
	using Nop = pspsharp.Allegrex.compiler.nativeCode.Nop;
	using BufferInfo = pspsharp.HLE.BufferInfo;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using CanBeNull = pspsharp.HLE.CanBeNull;
	using CheckArgument = pspsharp.HLE.CheckArgument;
	using DebugMemory = pspsharp.HLE.DebugMemory;
	using HLEModuleFunction = pspsharp.HLE.HLEModuleFunction;
	using HLEModuleManager = pspsharp.HLE.HLEModuleManager;
	using HLEUidClass = pspsharp.HLE.HLEUidClass;
	using HLEUidObjectMapping = pspsharp.HLE.HLEUidObjectMapping;
	using Modules = pspsharp.HLE.Modules;
	using PspString = pspsharp.HLE.PspString;
	using SceKernelErrorException = pspsharp.HLE.SceKernelErrorException;
	using StringInfo = pspsharp.HLE.StringInfo;
	using TErrorPointer32 = pspsharp.HLE.TErrorPointer32;
	using TPointer = pspsharp.HLE.TPointer;
	using TPointer16 = pspsharp.HLE.TPointer16;
	using TPointer32 = pspsharp.HLE.TPointer32;
	using TPointer64 = pspsharp.HLE.TPointer64;
	using TPointer8 = pspsharp.HLE.TPointer8;
	using Managers = pspsharp.HLE.kernel.Managers;
	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using pspAbstractMemoryMappedStructure = pspsharp.HLE.kernel.types.pspAbstractMemoryMappedStructure;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;
	using reboot = pspsharp.HLE.modules.reboot;
	using DebuggerMemory = pspsharp.memory.DebuggerMemory;
	using FastMemory = pspsharp.memory.FastMemory;
	using SafeFastMemory = pspsharp.memory.SafeFastMemory;
	using ClassAnalyzer = pspsharp.util.ClassAnalyzer;
	using DurationStatistics = pspsharp.util.DurationStatistics;
	using ParameterInfo = pspsharp.util.ClassAnalyzer.ParameterInfo;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;
	using ClassVisitor = org.objectweb.asm.ClassVisitor;
	using Label = org.objectweb.asm.Label;
	using MethodVisitor = org.objectweb.asm.MethodVisitor;
	using Opcodes = org.objectweb.asm.Opcodes;
	using Type = org.objectweb.asm.Type;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class CompilerContext : ICompilerContext
	{
		protected internal static Logger log = Compiler.log;
		private CompilerClassLoader classLoader;
		private CodeBlock codeBlock;
		private int numberInstructionsToBeSkipped;
		private bool skipDelaySlot;
		private MethodVisitor mv;
		private CodeInstruction codeInstruction;
		private const bool storeCpuLocal = true;
		private const bool storeMemoryIntLocal = false;
		private const int LOCAL_CPU = 0;
		private const int LOCAL_INSTRUCTION_COUNT = 1;
		private const int LOCAL_MEMORY_INT = 2;
		private const int LOCAL_TMP1 = 3;
		private const int LOCAL_TMP2 = 4;
		private const int LOCAL_TMP3 = 5;
		private const int LOCAL_TMP4 = 6;
		private const int LOCAL_TMP_VD0 = 7;
		private const int LOCAL_TMP_VD1 = 8;
		private const int LOCAL_TMP_VD2 = 9;
		private const int LOCAL_MAX = 10;
		private const int LOCAL_FIRST_SAVED_PARAMETER = LOCAL_MAX;
		private const int LOCAL_NUMBER_SAVED_PARAMETERS = 8;
		private static readonly int LOCAL_MAX_WITH_SAVED_PARAMETERS = LOCAL_FIRST_SAVED_PARAMETER + LOCAL_NUMBER_SAVED_PARAMETERS;
		private const int DEFAULT_MAX_STACK_SIZE = 11;
		private const int SYSCALL_MAX_STACK_SIZE = 100;
		private const int LOCAL_ERROR_POINTER = LOCAL_TMP3;
		private bool enableIntructionCounting = false;
		public ISet<int> analysedAddresses = new HashSet<int>();
		public Stack<int> blocksToBeAnalysed = new Stack<int>();
		private int currentInstructionCount;
		private int preparedRegisterForStore = -1;
		private bool memWritePrepared = false;
		private bool hiloPrepared = false;
		private int methodMaxInstructions;
		private NativeCodeManager nativeCodeManager;
		private readonly VfpuPfxSrcState vfpuPfxsState = new VfpuPfxSrcState();
		private readonly VfpuPfxSrcState vfpuPfxtState = new VfpuPfxSrcState();
		private readonly VfpuPfxDstState vfpuPfxdState = new VfpuPfxDstState();
		private Label interpretPfxLabel = null;
		private bool pfxVdOverlap = false;
		public static readonly string runtimeContextInternalName = Type.getInternalName(typeof(RuntimeContext));
		public static readonly string runtimeContextLLEInternalName = Type.getInternalName(typeof(RuntimeContextLLE));
		private static readonly string processorDescriptor = Type.getDescriptor(typeof(Processor));
		private static readonly string cpuDescriptor = Type.getDescriptor(typeof(CpuState));
		private static readonly string cpuInternalName = Type.getInternalName(typeof(CpuState));
		private static readonly string instructionsInternalName = Type.getInternalName(typeof(Instructions));
		private static readonly string instructionInternalName = Type.getInternalName(typeof(Common.Instruction));
		private static readonly string instructionDescriptor = Type.getDescriptor(typeof(Common.Instruction));
		private static readonly string sceKernalThreadInfoInternalName = Type.getInternalName(typeof(SceKernelThreadInfo));
		private static readonly string sceKernalThreadInfoDescriptor = Type.getDescriptor(typeof(SceKernelThreadInfo));
		private static readonly string stringDescriptor = Type.getDescriptor(typeof(string));
		private static readonly string memoryDescriptor = Type.getDescriptor(typeof(Memory));
		private static readonly string memoryInternalName = Type.getInternalName(typeof(Memory));
		private static readonly string profilerInternalName = Type.getInternalName(typeof(Profiler));
		public static readonly string executableDescriptor = Type.getDescriptor(typeof(IExecutable));
		public static readonly string executableInternalName = Type.getInternalName(typeof(IExecutable));
		public static readonly string arraycopyDescriptor = "(" + Type.getDescriptor(typeof(object)) + "I" + Type.getDescriptor(typeof(object)) + "II)V";
		private static ISet<int> fastSyscalls;
		private int instanceIndex;
		private NativeCodeSequence preparedCallNativeCodeBlock = null;
		private int maxStackSize = DEFAULT_MAX_STACK_SIZE;
		private int maxLocalSize = LOCAL_MAX;
		private bool parametersSavedToLocals;
		private CompilerTypeManager compilerTypeManager;

		public CompilerContext(CompilerClassLoader classLoader, int instanceIndex)
		{
			Compiler compiler = Compiler.Instance;
			this.classLoader = classLoader;
			this.instanceIndex = instanceIndex;
			nativeCodeManager = compiler.NativeCodeManager;
			methodMaxInstructions = compiler.DefaultMethodMaxInstructions;
			compilerTypeManager = compiler.CompilerTypeManager;

			// Count instructions only when the profile is enabled or
			// when the statistics are enabled
			if (Profiler.ProfilerEnabled || DurationStatistics.collectStatistics)
			{
				enableIntructionCounting = true;
			}

			if (fastSyscalls == null)
			{
				fastSyscalls = new SortedSet<int>();
				addFastSyscall(0x3AD58B8C); // sceKernelSuspendDispatchThread
				addFastSyscall(0x110DEC9A); // sceKernelUSec2SysClock
				addFastSyscall(unchecked((int)0xC8CD158C)); // sceKernelUSec2SysClockWide
				addFastSyscall(unchecked((int)0xBA6B92E2)); // sceKernelSysClock2USec
				addFastSyscall(unchecked((int)0xE1619D7C)); // sceKernelSysClock2USecWide
				addFastSyscall(unchecked((int)0xDB738F35)); // sceKernelGetSystemTime
				addFastSyscall(unchecked((int)0x82BC5777)); // sceKernelGetSystemTimeWide
				addFastSyscall(0x369ED59D); // sceKernelGetSystemTimeLow
				addFastSyscall(unchecked((int)0xB5F6DC87)); // sceMpegRingbufferAvailableSize
				addFastSyscall(unchecked((int)0xE0D68148)); // sceGeListUpdateStallAddr
				addFastSyscall(0x34B9FA9E); // sceKernelDcacheWritebackInvalidateRange
				addFastSyscall(unchecked((int)0xE47E40E4)); // sceGeEdramGetAddr
				addFastSyscall(0x1F6752AD); // sceGeEdramGetSize
				addFastSyscall(0x74AE582A); // __sceSasGetEnvelopeHeight
				addFastSyscall(0x68A46B95); // __sceSasGetEndFlag
			}
		}

		private void addFastSyscall(int nid)
		{
			int syscallCode = NIDMapper.Instance.getSyscallByNid(nid);
			if (syscallCode >= 0)
			{
				fastSyscalls.Add(syscallCode);
			}
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


		public virtual CodeBlock CodeBlock
		{
			get
			{
				return codeBlock;
			}
			set
			{
				this.codeBlock = value;
			}
		}


		public virtual NativeCodeManager NativeCodeManager
		{
			get
			{
				return nativeCodeManager;
			}
		}

		private void loadCpu()
		{
			if (storeCpuLocal)
			{
				mv.visitVarInsn(Opcodes.ALOAD, LOCAL_CPU);
			}
			else
			{
				mv.visitFieldInsn(Opcodes.GETSTATIC, runtimeContextInternalName, "cpu", cpuDescriptor);
			}
		}

		public virtual void loadProcessor()
		{
			mv.visitFieldInsn(Opcodes.GETSTATIC, runtimeContextInternalName, "processor", processorDescriptor);
		}

		private void loadMemory()
		{
			mv.visitFieldInsn(Opcodes.GETSTATIC, runtimeContextInternalName, "memory", memoryDescriptor);
		}

		private void loadMMIO()
		{
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextLLEInternalName, "getMMIO", "()" + memoryDescriptor);
		}

		private void loadModule(string moduleName)
		{
			mv.visitFieldInsn(Opcodes.GETSTATIC, Type.getInternalName(typeof(Modules)), moduleName + "Module", "Ljpcsp/HLE/modules/" + moduleName + ";");
		}

		private void loadFpr()
		{
			mv.visitFieldInsn(Opcodes.GETSTATIC, runtimeContextInternalName, "fpr", "[F");
		}

		public virtual void loadVprFloat()
		{
			mv.visitFieldInsn(Opcodes.GETSTATIC, runtimeContextInternalName, "vprFloat", "[F");
		}

		public virtual void loadVprInt()
		{
			mv.visitFieldInsn(Opcodes.GETSTATIC, runtimeContextInternalName, "vprInt", "[I");
		}

		public virtual void loadRegister(int reg)
		{
			if (reg == _zr)
			{
				loadImm(0);
			}
			else
			{
				loadCpu();
				mv.visitFieldInsn(Opcodes.GETFIELD, cpuInternalName, getGprFieldName(reg), "I");
			}
		}

		public virtual void loadFRegister(int reg)
		{
			loadFpr();
			loadImm(reg);
			mv.visitInsn(Opcodes.FALOAD);
		}

		private float? getPfxSrcCstValue(VfpuPfxSrcState pfxSrcState, int n)
		{
			if (pfxSrcState == null || pfxSrcState.Unknown || !pfxSrcState.pfxSrc.enabled || !pfxSrcState.pfxSrc.cst[n])
			{
				return null;
			}

			float value = 0.0f;
			switch (pfxSrcState.pfxSrc.swz[n])
			{
				case 0:
					value = pfxSrcState.pfxSrc.abs[n] ? 3.0f : 0.0f;
					break;
				case 1:
					value = pfxSrcState.pfxSrc.abs[n] ? (1.0f / 3.0f) : 1.0f;
					break;
				case 2:
					value = pfxSrcState.pfxSrc.abs[n] ? (1.0f / 4.0f) : 2.0f;
					break;
				case 3:
					value = pfxSrcState.pfxSrc.abs[n] ? (1.0f / 6.0f) : 0.5f;
					break;
			}

			if (pfxSrcState.pfxSrc.neg[n])
			{
				value = 0.0f - value;
			}

			if (log.TraceEnabled && pfxSrcState.Known && pfxSrcState.pfxSrc.enabled)
			{
				log.trace(string.Format("PFX    {0:X8} - getPfxSrcCstValue {1:D} -> {2:F}", CodeInstruction.Address, n, value));
			}

			return new float?(value);
		}

		private void convertVFloatToInt()
		{
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "floatToRawIntBits", "(F)I");
		}

		private void convertVIntToFloat()
		{
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "intBitsToFloat", "(I)F");
		}

		private void applyPfxSrcPostfix(VfpuPfxSrcState pfxSrcState, int n, bool isFloat)
		{
			if (pfxSrcState == null || pfxSrcState.Unknown || !pfxSrcState.pfxSrc.enabled)
			{
				return;
			}

			if (pfxSrcState.pfxSrc.abs[n])
			{
				if (log.TraceEnabled && pfxSrcState.Known && pfxSrcState.pfxSrc.enabled)
				{
					log.trace(string.Format("PFX    {0:X8} - applyPfxSrcPostfix abs({1:D})", CodeInstruction.Address, n));
				}

				if (isFloat)
				{
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "abs", "(F)F");
				}
				else
				{
					loadImm(0x7FFFFFFF);
					mv.visitInsn(Opcodes.IAND);
				}
			}
			if (pfxSrcState.pfxSrc.neg[n])
			{
				if (log.TraceEnabled && pfxSrcState.Known && pfxSrcState.pfxSrc.enabled)
				{
					log.trace(string.Format("PFX    {0:X8} - applyPfxSrcPostfix neg({1:D})", CodeInstruction.Address, n));
				}

				if (isFloat)
				{
					mv.visitInsn(Opcodes.FNEG);
				}
				else
				{
					loadImm(0x80000000);
					mv.visitInsn(Opcodes.IXOR);
				}
			}
		}

		private int getPfxSrcIndex(VfpuPfxSrcState pfxSrcState, int n)
		{
			if (pfxSrcState == null || pfxSrcState.Unknown || !pfxSrcState.pfxSrc.enabled || pfxSrcState.pfxSrc.cst[n])
			{
				return n;
			}

			if (log.TraceEnabled && pfxSrcState.Known && pfxSrcState.pfxSrc.enabled)
			{
				log.trace(string.Format("PFX    {0:X8} - getPfxSrcIndex {1:D} -> {2:D}", CodeInstruction.Address, n, pfxSrcState.pfxSrc.swz[n]));
			}
			return pfxSrcState.pfxSrc.swz[n];
		}

		private void loadVRegister(int m, int c, int r, bool isFloat)
		{
			int index = VfpuState.getVprIndex(m, c, r);
			if (isFloat)
			{
				loadVprFloat();
				loadImm(index);
				mv.visitInsn(Opcodes.FALOAD);
			}
			else
			{
				loadVprInt();
				loadImm(index);
				mv.visitInsn(Opcodes.IALOAD);
			}
		}

		private void loadCstValue(float? cstValue, bool isFloat)
		{
			if (isFloat)
			{
				mv.visitLdcInsn(cstValue.Value);
			}
			else
			{
				loadImm(Float.floatToRawIntBits(cstValue.Value));
			}
		}

		private void loadVRegister(int vsize, int reg, int n, VfpuPfxSrcState pfxSrcState, bool isFloat)
		{
			if (log.TraceEnabled && pfxSrcState != null && pfxSrcState.Known && pfxSrcState.pfxSrc.enabled)
			{
				log.trace(string.Format("PFX    {0:X8} - loadVRegister {1:D}, {2:D}, {3:D}", CodeInstruction.Address, vsize, reg, n));
			}

			int m = (reg >> 2) & 7;
			int i = (reg >> 0) & 3;
			int s;
			switch (vsize)
			{
				case 1:
				{
					s = (reg >> 5) & 3;
					float? cstValue = getPfxSrcCstValue(pfxSrcState, n);
					if (cstValue != null)
					{
						loadCstValue(cstValue, isFloat);
					}
					else
					{
						loadVRegister(m, i, s, isFloat);
						applyPfxSrcPostfix(pfxSrcState, n, isFloat);
					}
					break;
				}
				case 2:
				{
					s = (reg & 64) >> 5;
					float? cstValue = getPfxSrcCstValue(pfxSrcState, n);
					if (cstValue != null)
					{
						loadCstValue(cstValue, isFloat);
					}
					else
					{
						int index = getPfxSrcIndex(pfxSrcState, n);
						if ((reg & 32) != 0)
						{
							loadVRegister(m, s + index, i, isFloat);
						}
						else
						{
							loadVRegister(m, i, s + index, isFloat);
						}
						applyPfxSrcPostfix(pfxSrcState, n, isFloat);
					}
					break;
				}
				case 3:
				{
					s = (reg & 64) >> 6;
					float? cstValue = getPfxSrcCstValue(pfxSrcState, n);
					if (cstValue != null)
					{
						loadCstValue(cstValue, isFloat);
					}
					else
					{
						int index = getPfxSrcIndex(pfxSrcState, n);
						if ((reg & 32) != 0)
						{
							loadVRegister(m, s + index, i, isFloat);
						}
						else
						{
							loadVRegister(m, i, s + index, isFloat);
						}
						applyPfxSrcPostfix(pfxSrcState, n, isFloat);
					}
					break;
				}
				case 4:
				{
					s = (reg & 64) >> 5;
					float? cstValue = getPfxSrcCstValue(pfxSrcState, n);
					if (cstValue != null)
					{
						loadCstValue(cstValue, isFloat);
					}
					else
					{
						int index = getPfxSrcIndex(pfxSrcState, (n + s) & 3);
						if ((reg & 32) != 0)
						{
							loadVRegister(m, index, i, isFloat);
						}
						else
						{
							loadVRegister(m, i, index, isFloat);
						}
						applyPfxSrcPostfix(pfxSrcState, n, isFloat);
					}
					break;
				}
			}
		}

		public virtual void prepareRegisterForStore(int reg)
		{
			if (preparedRegisterForStore < 0)
			{
				loadCpu();
				preparedRegisterForStore = reg;
			}
		}

		private string getGprFieldName(int reg)
		{
			return Common.gprNames[reg].Replace('$', '_');
		}

		public virtual void storeRegister(int reg)
		{
			if (preparedRegisterForStore == reg)
			{
				mv.visitFieldInsn(Opcodes.PUTFIELD, cpuInternalName, getGprFieldName(reg), "I");
				preparedRegisterForStore = -1;
			}
			else
			{
				loadCpu();
				mv.visitInsn(Opcodes.SWAP);
				mv.visitFieldInsn(Opcodes.PUTFIELD, cpuInternalName, getGprFieldName(reg), "I");
			}
		}

		public virtual void storeRegister(int reg, int constantValue)
		{
			if (preparedRegisterForStore == reg)
			{
				preparedRegisterForStore = -1;
			}
			else
			{
				loadCpu();
			}
			loadImm(constantValue);
			mv.visitFieldInsn(Opcodes.PUTFIELD, cpuInternalName, getGprFieldName(reg), "I");
		}

		public virtual void prepareFRegisterForStore(int reg)
		{
			if (preparedRegisterForStore < 0)
			{
				loadFpr();
				loadImm(reg);
				preparedRegisterForStore = reg;
			}
		}

		public virtual void storeFRegister(int reg)
		{
			if (preparedRegisterForStore == reg)
			{
				mv.visitInsn(Opcodes.FASTORE);
				preparedRegisterForStore = -1;
			}
			else
			{
				loadFpr();
				mv.visitInsn(Opcodes.SWAP);
				loadImm(reg);
				mv.visitInsn(Opcodes.SWAP);
				mv.visitInsn(Opcodes.FASTORE);
			}
		}

		public virtual bool hasNoPfx()
		{
			if (vfpuPfxdState != null && vfpuPfxdState.Known && vfpuPfxdState.pfxDst.enabled)
			{
				return false;
			}
			if (vfpuPfxsState != null && vfpuPfxsState.Known && vfpuPfxsState.pfxSrc.enabled)
			{
				return false;
			}
			if (vfpuPfxtState != null && vfpuPfxtState.Known && vfpuPfxtState.pfxSrc.enabled)
			{
				return false;
			}

			return true;
		}

		private bool isPfxDstMasked(VfpuPfxDstState pfxDstState, int n)
		{
			if (pfxDstState == null || pfxDstState.Unknown || !pfxDstState.pfxDst.enabled)
			{
				return false;
			}

			return pfxDstState.pfxDst.msk[n];
		}

		private void applyPfxDstPostfix(VfpuPfxDstState pfxDstState, int n, bool isFloat)
		{
			if (pfxDstState == null || pfxDstState.Unknown || !pfxDstState.pfxDst.enabled)
			{
				return;
			}

			switch (pfxDstState.pfxDst.sat[n])
			{
				case 1:
					if (log.TraceEnabled && pfxDstState != null && pfxDstState.Known && pfxDstState.pfxDst.enabled)
					{
						log.trace(string.Format("PFX    {0:X8} - applyPfxDstPostfix {1:D} [0:1]", CodeInstruction.Address, n));
					}
					if (!isFloat)
					{
						convertVIntToFloat();
					}
					mv.visitLdcInsn(1.0f);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "min", "(FF)F");
					mv.visitLdcInsn(0.0f);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "max", "(FF)F");
					if (!isFloat)
					{
						convertVFloatToInt();
					}
					break;
				case 3:
					if (log.TraceEnabled && pfxDstState != null && pfxDstState.Known && pfxDstState.pfxDst.enabled)
					{
						log.trace(string.Format("PFX    {0:X8} - applyPfxDstPostfix {1:D} [-1:1]", CodeInstruction.Address, n));
					}
					if (!isFloat)
					{
						convertVIntToFloat();
					}
					mv.visitLdcInsn(1.0f);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "min", "(FF)F");
					mv.visitLdcInsn(-1.0f);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "max", "(FF)F");
					if (!isFloat)
					{
						convertVFloatToInt();
					}
					break;
			}
		}

		private void prepareVRegisterForStore(int m, int c, int r, bool isFloat)
		{
			int index = VfpuState.getVprIndex(m, c, r);
			if (isFloat)
			{
				// Prepare the array and index for the int value
				loadVprInt();
				loadImm(index);

				// Prepare the array and index for the float value
				loadVprFloat();
				loadImm(index);
			}
			else
			{
				// Prepare the array and index for the float value
				loadVprFloat();
				loadImm(index);

				// Prepare the array and index for the int value
				loadVprInt();
				loadImm(index);
			}
		}

		public virtual void prepareVRegisterForStore(int vsize, int reg, int n, VfpuPfxDstState pfxDstState, bool isFloat)
		{
			if (preparedRegisterForStore < 0)
			{
				if (!isPfxDstMasked(pfxDstState, n))
				{
					int m = (reg >> 2) & 7;
					int i = (reg >> 0) & 3;
					int s;
					switch (vsize)
					{
						case 1:
						{
							s = (reg >> 5) & 3;
							prepareVRegisterForStore(m, i, s, isFloat);
							break;
						}
						case 2:
						{
							s = (reg & 64) >> 5;
							if ((reg & 32) != 0)
							{
								prepareVRegisterForStore(m, s + n, i, isFloat);
							}
							else
							{
								prepareVRegisterForStore(m, i, s + n, isFloat);
							}
							break;
						}
						case 3:
						{
							s = (reg & 64) >> 6;
							if ((reg & 32) != 0)
							{
								prepareVRegisterForStore(m, s + n, i, isFloat);
							}
							else
							{
								prepareVRegisterForStore(m, i, s + n, isFloat);
							}
							break;
						}
						case 4:
						{
							s = (reg & 64) >> 5;
							if ((reg & 32) != 0)
							{
								prepareVRegisterForStore(m, (n + s) & 3, i, isFloat);
							}
							else
							{
								prepareVRegisterForStore(m, i, (n + s) & 3, isFloat);
							}
							break;
						}
					}
				}
				preparedRegisterForStore = reg;
			}
		}

		private void storeVRegister(int vsize, int reg, int n, VfpuPfxDstState pfxDstState, bool isFloat)
		{
			if (log.TraceEnabled && pfxDstState != null && pfxDstState.Known && pfxDstState.pfxDst.enabled)
			{
				log.trace(string.Format("PFX    {0:X8} - storeVRegister {1:D}, {2:D}, {3:D}", CodeInstruction.Address, vsize, reg, n));
			}

			if (preparedRegisterForStore == reg)
			{
				if (isPfxDstMasked(pfxDstState, n))
				{
					if (log.TraceEnabled && pfxDstState != null && pfxDstState.Known && pfxDstState.pfxDst.enabled)
					{
						log.trace(string.Format("PFX    {0:X8} - storeVRegister {1:D} masked", CodeInstruction.Address, n));
					}

					mv.visitInsn(Opcodes.POP);
				}
				else
				{
					applyPfxDstPostfix(pfxDstState, n, isFloat);
					if (isFloat)
					{
						// Keep a copy of the value for the int value
						mv.visitInsn(Opcodes.DUP_X2);
						mv.visitInsn(Opcodes.FASTORE); // First store the float value
						convertVFloatToInt();
						mv.visitInsn(Opcodes.IASTORE); // Second store the int value
					}
					else
					{
						// Keep a copy of the value for the float value
						mv.visitInsn(Opcodes.DUP_X2);
						mv.visitInsn(Opcodes.IASTORE); // First store the int value
						convertVIntToFloat();
						mv.visitInsn(Opcodes.FASTORE); // Second store the float value
					}
				}
				preparedRegisterForStore = -1;
			}
			else
			{
				log.error("storeVRegister with non-prepared register is not supported");
			}
		}

		public virtual void loadFcr31()
		{
			loadCpu();
			mv.visitFieldInsn(Opcodes.GETFIELD, cpuInternalName, "fcr31", Type.getDescriptor(typeof(Fcr31)));
		}

		public virtual void loadVcr()
		{
			loadCpu();
			mv.visitFieldInsn(Opcodes.GETFIELD, cpuInternalName, "vcr", Type.getDescriptor(typeof(VfpuState.Vcr)));
		}

		public virtual void loadHilo()
		{
			loadCpu();
			mv.visitFieldInsn(Opcodes.GETFIELD, cpuInternalName, "hilo", Type.getDescriptor(typeof(long)));
		}

		public virtual void prepareHiloForStore()
		{
			loadCpu();
			hiloPrepared = true;
		}

		public virtual void storeHilo()
		{
			if (!hiloPrepared)
			{
				loadCpu();
				mv.visitInsn(Opcodes.DUP_X2);
				mv.visitInsn(Opcodes.POP);
			}
			mv.visitFieldInsn(Opcodes.PUTFIELD, cpuInternalName, "hilo", Type.getDescriptor(typeof(long)));

			hiloPrepared = false;
		}

		public virtual void loadFcr31c()
		{
			loadFcr31();
			mv.visitFieldInsn(Opcodes.GETFIELD, Type.getInternalName(typeof(Fcr31)), "c", "Z");
		}

		public virtual void prepareFcr31cForStore()
		{
			loadFcr31();
		}

		public virtual void storeFcr31c()
		{
			mv.visitFieldInsn(Opcodes.PUTFIELD, Type.getInternalName(typeof(Fcr31)), "c", "Z");
		}

		public virtual void loadVcrCc()
		{
			loadVcrCc((codeInstruction.Opcode >> 18) & 7);
		}

		public virtual void loadVcrCc(int cc)
		{
			loadVcr();
			mv.visitFieldInsn(Opcodes.GETFIELD, Type.getInternalName(typeof(VfpuState.Vcr)), "cc", "[Z");
			loadImm(cc);
			mv.visitInsn(Opcodes.BALOAD);
		}

		public virtual void loadLocalVar(int localVar)
		{
			mv.visitVarInsn(Opcodes.ILOAD, localVar);
		}

		private void storeLocalVar(int localVar)
		{
			mv.visitVarInsn(Opcodes.ISTORE, localVar);
		}

		private void loadInstruction(Common.Instruction insn)
		{
			string classInternalName = instructionsInternalName;

			if (insn == Common.UNK)
			{
				// UNK instruction is in Common class, not Instructions
				classInternalName = Type.getInternalName(typeof(Common));
			}

			mv.visitFieldInsn(Opcodes.GETSTATIC, classInternalName, insn.name().Replace('.', '_').Replace(' ', '_'), instructionDescriptor);
		}

		public virtual void storePc()
		{
			loadCpu();
			loadImm(codeInstruction.Address);
			mv.visitFieldInsn(Opcodes.PUTFIELD, cpuInternalName, "pc", "I");
		}

		private void visitContinueToAddress(int returnAddress, bool returnOnUnknownAddress)
		{
			//      if (x != returnAddress) {
			//          RuntimeContext.jump(x, returnAddress);
			//      }
			Label continueLabel = new Label();
			Label isReturnAddress = new Label();

			mv.visitInsn(Opcodes.DUP);
			loadImm(returnAddress);
			visitJump(Opcodes.IF_ICMPEQ, isReturnAddress);

			if (returnOnUnknownAddress)
			{
				visitJump();
			}
			else
			{
				loadImm(returnAddress);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "jump", "(II)V");
				mv.visitJumpInsn(Opcodes.GOTO, continueLabel);
			}

			mv.visitLabel(isReturnAddress);
			mv.visitInsn(Opcodes.POP);
			mv.visitLabel(continueLabel);
		}

		private void visitContinueToAddressInRegister(int reg)
		{
			//      if (x != cpu.reg) {
			//          RuntimeContext.jump(x, cpu.reg);
			//      }
			Label continueLabel = new Label();
			Label isReturnAddress = new Label();

			mv.visitInsn(Opcodes.DUP);
			loadRegister(reg);
			visitJump(Opcodes.IF_ICMPEQ, isReturnAddress);

			loadRegister(reg);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "jump", "(II)V");
			mv.visitJumpInsn(Opcodes.GOTO, continueLabel);

			mv.visitLabel(isReturnAddress);
			mv.visitInsn(Opcodes.POP);
			mv.visitLabel(continueLabel);
		}

		public virtual void visitJump()
		{
			flushInstructionCount(true, false);
			checkSync();

			endMethod();
			mv.visitInsn(Opcodes.IRETURN);
		}

		public virtual void prepareCall(int address, int returnAddress, int returnRegister)
		{
			preparedCallNativeCodeBlock = null;

			// Do not call native block directly if we are profiling,
			// this would loose profiler information
			if (!Profiler.ProfilerEnabled)
			{
				// Is a native equivalent for this CodeBlock available?
				preparedCallNativeCodeBlock = nativeCodeManager.getCompiledNativeCodeBlock(address);
			}

			if (preparedCallNativeCodeBlock == null)
			{
				if (returnRegister != _zr)
				{
					// Load the return register ($ra) with the return address
					// before the delay slot is executed. The delay slot might overwrite it.
					// For example:
					//     addiu      $sp, $sp, -16
					//     sw         $ra, 0($sp)
					//     jal        0x0XXXXXXX
					//     lw         $ra, 0($sp)
					//     jr         $ra
					//     addiu      $sp, $sp, 16
					prepareRegisterForStore(returnRegister);
					loadImm(returnAddress);
					storeRegister(returnRegister);
				}
			}
		}

		public virtual void visitCall(int address, int returnAddress, int returnRegister, bool returnRegisterModified, bool returnOnUnknownAddress)
		{
			flushInstructionCount(false, false);

			if (preparedCallNativeCodeBlock != null)
			{
				if (preparedCallNativeCodeBlock.NativeCodeSequenceClass.Equals(typeof(Nop)))
				{
					// NativeCodeSequence Nop means nothing to do!
				}
				else
				{
					// Call NativeCodeSequence
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Inlining call at 0x{0:X8} to {1}", CodeInstruction.Address, preparedCallNativeCodeBlock));
					}

					visitNativeCodeSequence(preparedCallNativeCodeBlock, address, null);
				}
			}
			else
			{
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, getClassName(address, instanceIndex), StaticExecMethodName, StaticExecMethodDesc);
				visitContinueToAddress(returnAddress, returnOnUnknownAddress);
			}

			preparedCallNativeCodeBlock = null;
		}

		public virtual void visitCall(int returnAddress, int returnRegister)
		{
			flushInstructionCount(false, false);
			if (returnRegister != _zr)
			{
				storeRegister(returnRegister, returnAddress);
			}
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "call", "(I)I");
			visitContinueToAddress(returnAddress, false);
		}

		public virtual void visitCall(int address, string methodName)
		{
			flushInstructionCount(false, false);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, getClassName(address, instanceIndex), methodName, "()V");
		}

		public virtual void visitIntepreterCall(int opcode, Common.Instruction insn)
		{
			loadInstruction(insn);
			loadProcessor();
			loadImm(opcode);
			mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, instructionInternalName, "interpret", "(" + processorDescriptor + "I)V");
		}

		private bool isFastSyscall(int code)
		{
			return fastSyscalls.Contains(code);
		}

		/// <summary>
		/// Generate the required Java code to load one parameter for
		/// the syscall function from the CPU registers.
		/// 
		/// The following code is generated based on the parameter type:
		/// Processor: parameterValue = RuntimeContext.processor
		/// int:       parameterValue = cpu.gpr[paramIndex++]
		/// float:     parameterValue = cpu.fpr[paramFloatIndex++]
		/// long:      parameterValue = (cpu.gpr[paramIndex++] & 0xFFFFFFFFL) + ((long) cpu.gpr[paramIndex++]) << 32)
		/// boolean:   parameterValue = cpu.gpr[paramIndex++]
		/// TPointer,
		/// TPointer8,
		/// TPointer16,
		/// TPointer32,
		/// TPointer64,
		/// TErrorPointer32:
		///            if (checkMemoryAccess()) {
		///                if (canBeNullParam && address == 0) {
		///                    goto addressGood;
		///                }
		///                if (RuntimeContext.checkMemoryPointer(address)) {
		///                    goto addressGood;
		///                }
		///                cpu.gpr[_v0] = SceKernelErrors.ERROR_INVALID_POINTER;
		///                pop all the parameters already prepared on the stack;
		///                goto afterSyscall;
		///                addressGood:
		///            }
		///            <parameterType> pointer = new <parameterType>(address);
		///            if (parameterType == TErrorPointer32.class) {
		///                parameterReader.setHasErrorPointer(true);
		///                localVar[LOCAL_ERROR_POINTER] = pointer;
		///            }
		///            parameterValue = pointer
		/// HLEUidClass defined in annotation:
		///            <parameterType> uidObject = HLEUidObjectMapping.getObject("<parameterType>", uid);
		///            if (uidObject == null) {
		///                cpu.gpr[_v0] = errorValueOnNotFound;
		///                pop all the parameters already prepared on the stack;
		///                goto afterSyscall;
		///            }
		///            parameterValue = uidObject
		/// 
		/// And then common for all the types:
		///            try {
		///                parameterValue = <module>.<methodToCheck>(parameterValue);
		///            } catch (SceKernelErrorException e) {
		///                goto catchSceKernelErrorException;
		///            }
		///            push parameterValue on stack
		/// </summary>
		/// <param name="parameterReader">               the current parameter state </param>
		/// <param name="func">                          the syscall function </param>
		/// <param name="parameterType">                 the type of the parameter </param>
		/// <param name="afterSyscallLabel">             the Label pointing after the call to the syscall function </param>
		/// <param name="catchSceKernelErrorException">  the Label pointing to the SceKernelErrorException catch handler </param>
		private void loadParameter(CompilerParameterReader parameterReader, HLEModuleFunction func, Type parameterType, Annotation[] parameterAnnotations, Label afterSyscallLabel, Label catchSceKernelErrorException)
		{
			if (parameterType == typeof(Processor))
			{
				loadProcessor();
				parameterReader.incrementCurrentStackSize();
			}
			else if (parameterType == typeof(CpuState))
			{
				loadCpu();
				parameterReader.incrementCurrentStackSize();
			}
			else if (parameterType == typeof(int))
			{
				parameterReader.loadNextInt();
				parameterReader.incrementCurrentStackSize();
			}
			else if (parameterType == typeof(float))
			{
				parameterReader.loadNextFloat();
				parameterReader.incrementCurrentStackSize();
			}
			else if (parameterType == typeof(long))
			{
				parameterReader.loadNextLong();
				parameterReader.incrementCurrentStackSize(2);
			}
			else if (parameterType == typeof(bool))
			{
				parameterReader.loadNextInt();
				parameterReader.incrementCurrentStackSize();
			}
			else if (parameterType == typeof(string))
			{
				parameterReader.loadNextInt();

				int maxLength = 16 * 1024;
				foreach (Annotation parameterAnnotation in parameterAnnotations)
				{
					if (parameterAnnotation is StringInfo)
					{
						StringInfo stringInfo = ((StringInfo)parameterAnnotation);
						maxLength = stringInfo.maxLength();
						break;
					}
				}
				loadImm(maxLength);
				   mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "readStringNZ", "(II)" + Type.getDescriptor(typeof(string)));
				parameterReader.incrementCurrentStackSize();
			}
			else if (parameterType == typeof(PspString))
			{
				parameterReader.loadNextInt();

				int maxLength = 16 * 1024;
				bool canBeNull = false;
				foreach (Annotation parameterAnnotation in parameterAnnotations)
				{
					if (parameterAnnotation is StringInfo)
					{
						StringInfo stringInfo = ((StringInfo)parameterAnnotation);
						maxLength = stringInfo.maxLength();
					}
					if (parameterAnnotation is CanBeNull)
					{
						canBeNull = true;
					}
				}
				loadImm(maxLength);
				loadImm(canBeNull);
				   mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "readPspStringNZ", "(IIZ)" + Type.getDescriptor(typeof(PspString)));
				parameterReader.incrementCurrentStackSize();
			}
			else if (parameterType == typeof(TPointer) || parameterType == typeof(TPointer8) || parameterType == typeof(TPointer16) || parameterType == typeof(TPointer32) || parameterType == typeof(TPointer64) || parameterType == typeof(TErrorPointer32))
			{
				// if (checkMemoryAccess()) {
				//     if (canBeNullParam && address == 0) {
				//         goto addressGood;
				//     }
				//     if (RuntimeContext.checkMemoryPointer(address)) {
				//         goto addressGood;
				//     }
				//     cpu.gpr[_v0] = SceKernelErrors.ERROR_INVALID_POINTER;
				//     pop all the parameters already prepared on the stack;
				//     goto afterSyscall;
				//     addressGood:
				// }
				// <parameterType> pointer = new <parameterType>(address);
				// if (parameterType == TErrorPointer32.class) {
				//     parameterReader.setHasErrorPointer(true);
				//     localVar[LOCAL_ERROR_POINTER] = pointer;
				// }
				mv.visitTypeInsn(Opcodes.NEW, Type.getInternalName(parameterType));
				mv.visitInsn(Opcodes.DUP);
				loadMemory();
				parameterReader.loadNextInt();

				bool canBeNull = false;
				foreach (Annotation parameterAnnotation in parameterAnnotations)
				{
					if (parameterAnnotation is CanBeNull)
					{
						canBeNull = true;
						break;
					}
				}

				if (checkMemoryAccess() && afterSyscallLabel != null)
				{
					Label addressGood = new Label();
					if (canBeNull)
					{
						mv.visitInsn(Opcodes.DUP);
						mv.visitJumpInsn(Opcodes.IFEQ, addressGood);
					}
					mv.visitInsn(Opcodes.DUP);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "checkMemoryPointer", "(I)Z");
					mv.visitJumpInsn(Opcodes.IFNE, addressGood);
					storeRegister(_v0, SceKernelErrors.ERROR_INVALID_POINTER);
					parameterReader.popAllStack(4);
					mv.visitJumpInsn(Opcodes.GOTO, afterSyscallLabel);
					mv.visitLabel(addressGood);
				}
				if (parameterType == typeof(TPointer8) || parameterType == typeof(TPointer16) || parameterType == typeof(TPointer32) || parameterType == typeof(TPointer64))
				{
					loadImm(canBeNull);
					mv.visitMethodInsn(Opcodes.INVOKESPECIAL, Type.getInternalName(parameterType), "<init>", "(" + memoryDescriptor + "IZ)V");
				}
				else
				{
					mv.visitMethodInsn(Opcodes.INVOKESPECIAL, Type.getInternalName(parameterType), "<init>", "(" + memoryDescriptor + "I)V");
				}
				if (parameterType == typeof(TErrorPointer32))
				{
					parameterReader.HasErrorPointer = true;
					mv.visitInsn(Opcodes.DUP);
					mv.visitVarInsn(Opcodes.ASTORE, LOCAL_ERROR_POINTER);
				}
				parameterReader.incrementCurrentStackSize();
			}
			else if (parameterType.IsAssignableFrom(typeof(pspAbstractMemoryMappedStructure)))
			{
				parameterReader.loadNextInt();

				bool canBeNull = false;
				foreach (Annotation parameterAnnotation in parameterAnnotations)
				{
					if (parameterAnnotation is CanBeNull)
					{
						canBeNull = true;
						break;
					}
				}

				if (checkMemoryAccess() && afterSyscallLabel != null)
				{
					Label addressGood = new Label();
					if (canBeNull)
					{
						mv.visitInsn(Opcodes.DUP);
						mv.visitJumpInsn(Opcodes.IFEQ, addressGood);
					}
					mv.visitInsn(Opcodes.DUP);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "checkMemoryPointer", "(I)Z");
					mv.visitJumpInsn(Opcodes.IFNE, addressGood);
					storeRegister(_v0, SceKernelErrors.ERROR_INVALID_POINTER);
					parameterReader.popAllStack(1);
					mv.visitJumpInsn(Opcodes.GOTO, afterSyscallLabel);
					mv.visitLabel(addressGood);
				}

				mv.visitTypeInsn(Opcodes.NEW, Type.getInternalName(parameterType));
				mv.visitInsn(Opcodes.DUP);
				mv.visitMethodInsn(Opcodes.INVOKESPECIAL, Type.getInternalName(parameterType), "<init>", "()V");
				mv.visitInsn(Opcodes.DUP_X1);
				mv.visitInsn(Opcodes.SWAP);
				loadMemory();
				mv.visitInsn(Opcodes.SWAP);
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(parameterType), "read", "(" + memoryDescriptor + "I)V");
				parameterReader.incrementCurrentStackSize();
			}
			else
			{
				HLEUidClass hleUidClass = parameterType.getAnnotation(typeof(HLEUidClass));
				if (hleUidClass != null)
				{
					   int errorValueOnNotFound = hleUidClass.errorValueOnNotFound();

					// <parameterType> uidObject = HLEUidObjectMapping.getObject("<parameterType>", uid);
					// if (uidObject == null) {
					//     cpu.gpr[_v0] = errorValueOnNotFound;
					//     pop all the parameters already prepared on the stack;
					//     goto afterSyscall;
					// }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					mv.visitLdcInsn(parameterType.FullName);
					// Load the UID
					parameterReader.loadNextInt();

					// Load the UID Object
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(HLEUidObjectMapping)), "getObject", "(" + Type.getDescriptor(typeof(string)) + "I)" + Type.getDescriptor(typeof(object)));
					if (afterSyscallLabel != null)
					{
						Label foundUid = new Label();
						mv.visitInsn(Opcodes.DUP);
						mv.visitJumpInsn(Opcodes.IFNONNULL, foundUid);
						storeRegister(_v0, errorValueOnNotFound);
						parameterReader.popAllStack(1);
						mv.visitJumpInsn(Opcodes.GOTO, afterSyscallLabel);
						mv.visitLabel(foundUid);
					}
					mv.visitTypeInsn(Opcodes.CHECKCAST, Type.getInternalName(parameterType));
					parameterReader.incrementCurrentStackSize();
				}
				else
				{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					log.error(string.Format("Unsupported sycall parameter type '{0}'", parameterType.FullName));
					Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_UNIMPLEMENTED);
				}
			}

			Method methodToCheck = null;
			if (afterSyscallLabel != null)
			{
				foreach (Annotation parameterAnnotation in parameterAnnotations)
				{
					if (parameterAnnotation is CheckArgument)
					{
						CheckArgument checkArgument = (CheckArgument) parameterAnnotation;
						try
						{
							methodToCheck = func.HLEModule.GetType().GetMethod(checkArgument.value(), parameterType);
						}
						catch (Exception e)
						{
							log.error(string.Format("CheckArgument method '{0}' not found in {1}", checkArgument.value(), func.ModuleName), e);
						}
						break;
					}
				}
			}

			if (methodToCheck != null)
			{
				// try {
				//     parameterValue = <module>.<methodToCheck>(parameterValue);
				// } catch (SceKernelErrorException e) {
				//     goto catchSceKernelErrorException;
				// }
				loadModule(func.ModuleName);
				mv.visitInsn(Opcodes.SWAP);

				Label tryStart = new Label();
				Label tryEnd = new Label();
				mv.visitTryCatchBlock(tryStart, tryEnd, catchSceKernelErrorException, Type.getInternalName(typeof(SceKernelErrorException)));

				mv.visitLabel(tryStart);
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(methodToCheck.DeclaringClass), methodToCheck.Name, "(" + Type.getDescriptor(parameterType) + ")" + Type.getDescriptor(parameterType));
				mv.visitLabel(tryEnd);
			}

			parameterReader.incrementCurrentParameterIndex();
		}

		/// <summary>
		/// Generate the required Java code to store the return value of
		/// the syscall function into the CPU registers.
		/// 
		/// The following code is generated depending on the return type:
		/// void:         -
		/// int:          cpu.gpr[_v0] = intValue
		/// boolean:      cpu.gpr[_v0] = booleanValue
		/// long:         cpu.gpr[_v0] = (int) (longValue & 0xFFFFFFFFL)
		///               cpu.gpr[_v1] = (int) (longValue >>> 32)
		/// float:        cpu.fpr[_f0] = floatValue
		/// HLEUidClass:  if (moduleMethodUidGenerator == "") {
		///                   cpu.gpr[_v0] = HLEUidObjectMapping.createUidForObject("<return type>", returnValue);
		///               } else {
		///                   int uid = <module>.<moduleMethodUidGenerator>();
		///                   cpu.gpr[_v0] = HLEUidObjectMapping.addObjectMap("<return type>", uid, returnValue);
		///               }
		/// </summary>
		/// <param name="func">        the syscall function </param>
		/// <param name="returnType">  the type of the return value </param>
		private void storeReturnValue(HLEModuleFunction func, Type returnType)
		{
			if (returnType == typeof(void))
			{
				// Nothing to do
			}
			else if (returnType == typeof(int))
			{
				// cpu.gpr[_v0] = intValue
				storeRegister(_v0);
			}
			else if (returnType == typeof(bool))
			{
				// cpu.gpr[_v0] = booleanValue
				storeRegister(_v0);
			}
			else if (returnType == typeof(long))
			{
				// cpu.gpr[_v0] = (int) (longValue & 0xFFFFFFFFL)
				// cpu.gpr[_v1] = (int) (longValue >>> 32)
				mv.visitInsn(Opcodes.DUP2);
				mv.visitLdcInsn(0xFFFFFFFFL);
				mv.visitInsn(Opcodes.LAND);
				mv.visitInsn(Opcodes.L2I);
				storeRegister(_v0);
				loadImm(32);
				mv.visitInsn(Opcodes.LSHR);
				mv.visitInsn(Opcodes.L2I);
				storeRegister(_v1);
			}
			else if (returnType == typeof(float))
			{
				// cpu.fpr[_f0] = floatValue
				storeFRegister(_f0);
			}
			else
			{
				HLEUidClass hleUidClass = returnType.getAnnotation(typeof(HLEUidClass));
				if (hleUidClass != null)
				{
					// if (moduleMethodUidGenerator == "") {
					//     cpu.gpr[_v0] = HLEUidObjectMapping.createUidForObject("<return type>", returnValue);
					// } else {
					//     int uid = <module>.<moduleMethodUidGenerator>();
					//     cpu.gpr[_v0] = HLEUidObjectMapping.addObjectMap("<return type>", uid, returnValue);
					// }
					if (hleUidClass.moduleMethodUidGenerator().length() <= 0)
					{
						// No UID generator method, use the default one
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						mv.visitLdcInsn(returnType.FullName);
						mv.visitInsn(Opcodes.SWAP);
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(HLEUidObjectMapping)), "createUidForObject", "(" + Type.getDescriptor(typeof(string)) + Type.getDescriptor(typeof(object)) + ")I");
						storeRegister(_v0);
					}
					else
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						mv.visitLdcInsn(returnType.FullName);
						mv.visitInsn(Opcodes.SWAP);
						loadModule(func.ModuleName);
						mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(func.HLEModuleMethod.DeclaringClass), hleUidClass.moduleMethodUidGenerator(), "()I");
						mv.visitInsn(Opcodes.SWAP);
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(HLEUidObjectMapping)), "addObjectMap", "(" + Type.getDescriptor(typeof(string)) + "I" + Type.getDescriptor(typeof(object)) + ")I");
						storeRegister(_v0);
					}
				}
				else
				{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					log.error(string.Format("Unsupported sycall return value type '{0}'", returnType.FullName));
				}
			}
		}

		private void loadModuleLoggger(HLEModuleFunction func)
		{
			mv.visitFieldInsn(Opcodes.GETSTATIC, Type.getInternalName(func.HLEModuleMethod.DeclaringClass), "log", Type.getDescriptor(typeof(Logger)));
		}

		private void logSyscall(HLEModuleFunction func, string logPrefix, string logCheckFunction, string logFunction)
		{
			// Modules.getLogger(func.getModuleName()).warn("Unimplemented...");
			loadModuleLoggger(func);
			mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(Logger)), logCheckFunction, "()Z");
			Label loggingDisabled = new Label();
			mv.visitJumpInsn(Opcodes.IFEQ, loggingDisabled);

			loadModuleLoggger(func);

			StringBuilder formatString = new StringBuilder();
			if (!string.ReferenceEquals(logPrefix, null))
			{
				formatString.Append(logPrefix);
			}
			formatString.Append(func.FunctionName);
			ClassAnalyzer.ParameterInfo[] parameters = (new ClassAnalyzer()).getParameters(func.FunctionName, func.HLEModuleMethod.DeclaringClass);
			if (parameters != null)
			{
				// Log message:
				//    String.format(
				//       "Unimplemented <function name>
				//                 <parameterIntegerName>=0x%X,
				//                 <parameterBooleanName>=%b,
				//                 <parameterLongName>=0x%X,
				//                 <parameterFloatName>=%f,
				//                 <parameterOtherTypeName>=%s",
				//       new Object[] {
				//                 new Integer(parameterValueInteger),
				//                 new Boolean(parameterValueBoolean),
				//                 new Long(parameterValueLong),
				//                 new Float(parameterValueFloat),
				//                 parameterValueOtherTypes
				//       })
				loadImm(parameters.Length);
				mv.visitTypeInsn(Opcodes.ANEWARRAY, Type.getInternalName(typeof(object)));
				CompilerParameterReader parameterReader = new CompilerParameterReader(this);
				Annotation[][] paramsAnotations = func.HLEModuleMethod.ParameterAnnotations;
				int objectArrayIndex = 0;
				for (int paramIndex = 0; paramIndex < parameters.Length; paramIndex++)
				{
					ClassAnalyzer.ParameterInfo parameter = parameters[paramIndex];
					Type parameterType = parameter.type;
					CompilerTypeInformation typeInformation = compilerTypeManager.getCompilerTypeInformation(parameterType);

					mv.visitInsn(Opcodes.DUP);
					loadImm(objectArrayIndex);

					formatString.Append(paramIndex > 0 ? ", " : " ");
					formatString.Append(parameter.name);
					formatString.Append("=");
					formatString.Append(typeInformation.formatString);

					if (!string.ReferenceEquals(typeInformation.boxingTypeInternalName, null))
					{
						mv.visitTypeInsn(Opcodes.NEW, typeInformation.boxingTypeInternalName);
						mv.visitInsn(Opcodes.DUP);
					}

					loadParameter(parameterReader, func, parameterType, paramsAnotations[paramIndex], null, null);

					if (!string.ReferenceEquals(typeInformation.boxingTypeInternalName, null))
					{
						mv.visitMethodInsn(Opcodes.INVOKESPECIAL, typeInformation.boxingTypeInternalName, "<init>", typeInformation.boxingMethodDescriptor);
					}
					mv.visitInsn(Opcodes.AASTORE);

					objectArrayIndex++;
				}
				mv.visitLdcInsn(formatString.ToString());
				mv.visitInsn(Opcodes.SWAP);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(string)), "format", "(" + Type.getDescriptor(typeof(string)) + "[" + Type.getDescriptor(typeof(object)) + ")" + Type.getDescriptor(typeof(string)));
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(Logger)), logFunction, "(" + Type.getDescriptor(typeof(object)) + ")V");

				parameterReader = new CompilerParameterReader(this);
				for (int paramIndex = 0; paramIndex < parameters.Length; paramIndex++)
				{
					ClassAnalyzer.ParameterInfo parameter = parameters[paramIndex];
					Type parameterType = parameter.type;

					BufferInfo.LengthInfo lengthInfo = BufferInfo.defaultLengthInfo;
					int length = BufferInfo.defaultLength;
					BufferInfo.Usage usage = BufferInfo.defaultUsage;
					int maxDumpLength = BufferInfo.defaultMaxDumpLength;
					foreach (Annotation parameterAnnotation in paramsAnotations[paramIndex])
					{
						if (parameterAnnotation is BufferInfo)
						{
							BufferInfo bufferInfo = (BufferInfo) parameterAnnotation;
							lengthInfo = bufferInfo.lengthInfo();
							length = bufferInfo.length();
							usage = bufferInfo.usage();
							maxDumpLength = bufferInfo.maxDumpLength();
						}
					}

					bool parameterRead = false;
					if ((usage == BufferInfo.Usage.@in || usage == BufferInfo.Usage.inout) && (lengthInfo != BufferInfo.LengthInfo.unknown || parameterType == typeof(TPointer16) || parameterType == typeof(TPointer32) || parameterType == typeof(TPointer64)))
					{
						loadModuleLoggger(func);
						loadImm(1);
						mv.visitTypeInsn(Opcodes.ANEWARRAY, Type.getInternalName(typeof(object)));
						mv.visitInsn(Opcodes.DUP);
						loadImm(0);

						Label done = new Label();
						Label addressNull = new Label();
						parameterReader.loadNextInt();
						parameterRead = true;
						mv.visitInsn(Opcodes.DUP);
						mv.visitJumpInsn(Opcodes.IFEQ, addressNull);

						string format = string.Format("{0}[{1}]:%s", parameter.name, usage);
						bool useMemoryDump = true;

						switch (lengthInfo)
						{
							case fixedLength:
								loadImm(length);
								break;
							case nextNextParameter:
								parameterReader.skipNextInt();
								paramIndex++;
								parameterReader.loadNextInt();
								paramIndex++;
								break;
							case nextParameter:
								parameterReader.loadNextInt();
								paramIndex++;
								break;
							case previousParameter:
								// Go back to the address parameter
								parameterReader.rewindPreviousInt();
								// Go back to the previous parameter
								parameterReader.rewindPreviousInt();
								// Load the length from the previous parameter
								parameterReader.loadNextInt();
								// Skip again the address parameter
								// to come back to the above situation
								parameterReader.skipNextInt();
								break;
							case variableLength:
								mv.visitInsn(Opcodes.DUP);
								loadMemory();
								mv.visitInsn(Opcodes.SWAP);
								mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "read32", "(I)I");
								break;
							case unknown:
								useMemoryDump = false;
								format = string.Format("{0}[{1}]: 0x%X", parameter.name, usage);
								loadMemory();
								mv.visitInsn(Opcodes.SWAP);
								if (parameterType == typeof(TPointer64))
								{
									mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "read64", "(I)J");
									mv.visitTypeInsn(Opcodes.NEW, Type.getInternalName(typeof(Long)));
									mv.visitInsn(Opcodes.DUP);
									mv.visitInsn(Opcodes.DUP2_X2);
									mv.visitInsn(Opcodes.POP2);
									mv.visitMethodInsn(Opcodes.INVOKESPECIAL, Type.getInternalName(typeof(Long)), "<init>", "(J)V");
								}
								else if (parameterType == typeof(TPointer16))
								{
									mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "read16", "(I)I");
									mv.visitTypeInsn(Opcodes.NEW, Type.getInternalName(typeof(Integer)));
									mv.visitInsn(Opcodes.DUP_X1);
									mv.visitInsn(Opcodes.SWAP);
									mv.visitMethodInsn(Opcodes.INVOKESPECIAL, Type.getInternalName(typeof(Integer)), "<init>", "(I)V");
								}
								else
								{
									mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "read32", "(I)I");
									mv.visitTypeInsn(Opcodes.NEW, Type.getInternalName(typeof(Integer)));
									mv.visitInsn(Opcodes.DUP_X1);
									mv.visitInsn(Opcodes.SWAP);
									mv.visitMethodInsn(Opcodes.INVOKESPECIAL, Type.getInternalName(typeof(Integer)), "<init>", "(I)V");
								}
								break;
							default:
								log.error(string.Format("Unimplemented lengthInfo={0}", lengthInfo));
								break;
						}

						if (useMemoryDump)
						{
							if (maxDumpLength >= 0)
							{
								loadImm(maxDumpLength);
								mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "min", "(II)I");
							}
							mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Utilities)), "getMemoryDump", "(II)" + Type.getDescriptor(typeof(string)));
						}
						mv.visitInsn(Opcodes.AASTORE);

						mv.visitLdcInsn(format);
						mv.visitInsn(Opcodes.SWAP);
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(string)), "format", "(" + Type.getDescriptor(typeof(string)) + "[" + Type.getDescriptor(typeof(object)) + ")" + Type.getDescriptor(typeof(string)));
						mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(Logger)), logFunction, "(" + Type.getDescriptor(typeof(object)) + ")V");
						mv.visitJumpInsn(Opcodes.GOTO, done);

						mv.visitLabel(addressNull);
						mv.visitInsn(Opcodes.POP);
						mv.visitInsn(Opcodes.POP2);
						mv.visitInsn(Opcodes.POP2);
						mv.visitLabel(done);
					}

					if (!parameterRead)
					{
						if (parameterType == typeof(long))
						{
							parameterReader.skipNextLong();
						}
						else if (parameterType == typeof(float))
						{
							parameterReader.skipNextFloat();
						}
						else
						{
							parameterReader.skipNextInt();
						}
					}
				}
			}
			else
			{
				mv.visitLdcInsn(formatString.ToString());
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(Logger)), logFunction, "(" + Type.getDescriptor(typeof(object)) + ")V");
			}

			mv.visitLabel(loggingDisabled);
		}

		private string getLogCheckFunction(string loggingLevel)
		{
			string logCheckFunction = "isInfoEnabled";
			if ("trace".Equals(loggingLevel))
			{
				logCheckFunction = "isTraceEnabled";
			}
			else if ("debug".Equals(loggingLevel))
			{
				logCheckFunction = "isDebugEnabled";
			}
			return logCheckFunction;
		}

		private string getLoggingLevel(HLEModuleFunction func)
		{
			string loggingLevel = func.LoggingLevel;
			if (!string.ReferenceEquals(loggingLevel, null))
			{
				if (func.Unimplemented && codeBlock.HLEFunction)
				{
					// Do not log at the WARN level HLE methods that are
					// unimplemented but have been overwritten by real PSP modules
					if ("warn".Equals(loggingLevel))
					{
						loggingLevel = "debug";
					}
				}
			}

			return loggingLevel;
		}

		private void logSyscallStart(HLEModuleFunction func)
		{
			string loggingLevel = getLoggingLevel(func);
			if (!string.ReferenceEquals(loggingLevel, null))
			{
				string prefix = null;
				if (func.Unimplemented && !codeBlock.HLEFunction)
				{
					prefix = "Unimplemented ";
				}
				logSyscall(func, prefix, getLogCheckFunction(loggingLevel), loggingLevel);
			}
		}

		private void logSyscallEnd(HLEModuleFunction func, bool isErrorCode)
		{
			string loggingLevel = getLoggingLevel(func);
			if (string.ReferenceEquals(loggingLevel, null))
			{
				return;
			}
			string logCheckFunction = getLogCheckFunction(loggingLevel);

			// if (Modules.getLogger(func.getModuleName()).isDebugEnabled()) {
			//     Modules.getLogger(func.getModuleName()).debug(String.format("<function name> returning 0x%X", new Object[1] { new Integer(returnValue) }));
			// }
			loadModuleLoggger(func);
			mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(Logger)), logCheckFunction, "()Z");
			Label notDebug = new Label();
			mv.visitJumpInsn(Opcodes.IFEQ, notDebug);

			bool isReturningVoid = func.HLEModuleMethod.ReturnType == typeof(void);

			mv.visitInsn(Opcodes.DUP);
			mv.visitTypeInsn(Opcodes.NEW, Type.getInternalName(typeof(Integer)));
			mv.visitInsn(Opcodes.DUP_X1);
			mv.visitInsn(Opcodes.SWAP);
			mv.visitMethodInsn(Opcodes.INVOKESPECIAL, Type.getInternalName(typeof(Integer)), "<init>", "(I)V");
			loadImm(1);
			mv.visitTypeInsn(Opcodes.ANEWARRAY, Type.getInternalName(typeof(object)));
			mv.visitInsn(Opcodes.DUP_X1);
			mv.visitInsn(Opcodes.SWAP);
			loadImm(0);
			mv.visitInsn(Opcodes.SWAP);
			mv.visitInsn(Opcodes.AASTORE);
			string prefix = func.Unimplemented && !codeBlock.HLEFunction ? "Unimplemented " : "";
			mv.visitLdcInsn(string.Format("{0}{1} returning {2}{3}", prefix, func.FunctionName, isErrorCode ? "errorCode " : "", isReturningVoid ? "void" : "0x%X"));
			mv.visitInsn(Opcodes.SWAP);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(string)), "format", "(" + Type.getDescriptor(typeof(string)) + "[" + Type.getDescriptor(typeof(object)) + ")" + Type.getDescriptor(typeof(string)));
			loadModuleLoggger(func);
			mv.visitInsn(Opcodes.SWAP);
			mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(Logger)), loggingLevel, "(" + Type.getDescriptor(typeof(object)) + ")V");

			if (!isErrorCode)
			{
				ClassAnalyzer.ParameterInfo[] parameters = (new ClassAnalyzer()).getParameters(func.FunctionName, func.HLEModuleMethod.DeclaringClass);
				if (parameters != null)
				{
					CompilerParameterReader parameterReader;
					if (parametersSavedToLocals)
					{
						parameterReader = new CompilerLocalVarParameterReader(this, LOCAL_FIRST_SAVED_PARAMETER);
					}
					else
					{
						parameterReader = new CompilerParameterReader(this);
					}

					Annotation[][] paramsAnotations = func.HLEModuleMethod.ParameterAnnotations;
					for (int paramIndex = 0; paramIndex < parameters.Length; paramIndex++)
					{
						ClassAnalyzer.ParameterInfo parameter = parameters[paramIndex];
						Type parameterType = parameter.type;

						BufferInfo.LengthInfo lengthInfo = BufferInfo.defaultLengthInfo;
						int length = BufferInfo.defaultLength;
						BufferInfo.Usage usage = BufferInfo.defaultUsage;
						int maxDumpLength = BufferInfo.defaultMaxDumpLength;
						bool debugMemory = false;
						foreach (Annotation parameterAnnotation in paramsAnotations[paramIndex])
						{
							if (parameterAnnotation is BufferInfo)
							{
								BufferInfo bufferInfo = (BufferInfo) parameterAnnotation;
								lengthInfo = bufferInfo.lengthInfo();
								length = bufferInfo.length();
								usage = bufferInfo.usage();
								maxDumpLength = bufferInfo.maxDumpLength();
							}
							else if (parameterAnnotation is DebugMemory)
							{
								debugMemory = true;
							}
						}

						bool parameterRead = false;
						if ((usage == BufferInfo.Usage.@out || usage == BufferInfo.Usage.inout) && (lengthInfo != BufferInfo.LengthInfo.unknown || parameterType == typeof(TPointer16) || parameterType == typeof(TPointer32) || parameterType == typeof(TPointer64)))
						{
							loadModuleLoggger(func);
							loadImm(1);
							mv.visitTypeInsn(Opcodes.ANEWARRAY, Type.getInternalName(typeof(object)));
							mv.visitInsn(Opcodes.DUP);
							loadImm(0);

							Label done = new Label();
							Label addressNull = new Label();
							parameterReader.loadNextInt();
							parameterRead = true;
							mv.visitInsn(Opcodes.DUP);
							mv.visitJumpInsn(Opcodes.IFEQ, addressNull);

							string format = string.Format("{0}[{1}]:%s", parameter.name, usage);
							bool useMemoryDump = true;

							switch (lengthInfo)
							{
								case fixedLength:
									loadImm(length);
									break;
								case nextNextParameter:
									parameterReader.skipNextInt();
									paramIndex++;
									parameterReader.loadNextInt();
									paramIndex++;
									break;
								case nextParameter:
									parameterReader.loadNextInt();
									paramIndex++;
									break;
								case previousParameter:
									// Go back to the address parameter
									parameterReader.rewindPreviousInt();
									// Go back to the previous parameter
									parameterReader.rewindPreviousInt();
									// Load the length from the previous parameter
									parameterReader.loadNextInt();
									// Skip again the address parameter
									// to come back to the above situation
									parameterReader.skipNextInt();
									break;
								case variableLength:
									mv.visitInsn(Opcodes.DUP);
									loadMemory();
									mv.visitInsn(Opcodes.SWAP);
									mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "read32", "(I)I");
									break;
								case returnValue:
									loadRegister(_v0);
									break;
								case unknown:
									useMemoryDump = false;
									format = string.Format("{0}[{1}]: 0x%X", parameter.name, usage);
									loadMemory();
									mv.visitInsn(Opcodes.SWAP);
									if (parameterType == typeof(TPointer64))
									{
										if (debugMemory)
										{
											mv.visitInsn(Opcodes.DUP);
											loadImm(8);
											mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "debugMemory", "(II)V");
										}
										mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "read64", "(I)J");
										mv.visitTypeInsn(Opcodes.NEW, Type.getInternalName(typeof(Long)));
										mv.visitInsn(Opcodes.DUP);
										mv.visitInsn(Opcodes.DUP2_X2);
										mv.visitInsn(Opcodes.POP2);
										mv.visitMethodInsn(Opcodes.INVOKESPECIAL, Type.getInternalName(typeof(Long)), "<init>", "(J)V");
									}
									else if (parameterType == typeof(TPointer16))
									{
										if (debugMemory)
										{
											mv.visitInsn(Opcodes.DUP);
											loadImm(2);
											mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "debugMemory", "(II)V");
										}
										mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "read16", "(I)I");
										mv.visitTypeInsn(Opcodes.NEW, Type.getInternalName(typeof(Integer)));
										mv.visitInsn(Opcodes.DUP_X1);
										mv.visitInsn(Opcodes.SWAP);
										mv.visitMethodInsn(Opcodes.INVOKESPECIAL, Type.getInternalName(typeof(Integer)), "<init>", "(I)V");
									}
									else
									{
										if (debugMemory)
										{
											mv.visitInsn(Opcodes.DUP);
											loadImm(4);
											mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "debugMemory", "(II)V");
										}
										mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "read32", "(I)I");
										mv.visitTypeInsn(Opcodes.NEW, Type.getInternalName(typeof(Integer)));
										mv.visitInsn(Opcodes.DUP_X1);
										mv.visitInsn(Opcodes.SWAP);
										mv.visitMethodInsn(Opcodes.INVOKESPECIAL, Type.getInternalName(typeof(Integer)), "<init>", "(I)V");
									}
									break;
								default:
									log.error(string.Format("Unimplemented lengthInfo={0}", lengthInfo));
									break;
							}

							if (useMemoryDump)
							{
								if (debugMemory)
								{
									mv.visitInsn(Opcodes.DUP2);
									mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "debugMemory", "(II)V");
								}
								if (maxDumpLength >= 0)
								{
									loadImm(maxDumpLength);
									mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "min", "(II)I");
								}
								mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Utilities)), "getMemoryDump", "(II)" + Type.getDescriptor(typeof(string)));
							}
							mv.visitInsn(Opcodes.AASTORE);

							mv.visitLdcInsn(format);
							mv.visitInsn(Opcodes.SWAP);
							mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(string)), "format", "(" + Type.getDescriptor(typeof(string)) + "[" + Type.getDescriptor(typeof(object)) + ")" + Type.getDescriptor(typeof(string)));
							mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(Logger)), loggingLevel, "(" + Type.getDescriptor(typeof(object)) + ")V");
							mv.visitJumpInsn(Opcodes.GOTO, done);

							mv.visitLabel(addressNull);
							mv.visitInsn(Opcodes.POP);
							mv.visitInsn(Opcodes.POP2);
							mv.visitInsn(Opcodes.POP2);
							mv.visitLabel(done);
						}

						if (!parameterRead)
						{
							if (parameterType == typeof(long))
							{
								parameterReader.skipNextLong();
							}
							else if (parameterType == typeof(float))
							{
								parameterReader.skipNextFloat();
							}
							else
							{
								parameterReader.skipNextInt();
							}
						}
					}
				}
			}

			mv.visitLabel(notDebug);
		}

		private bool CodeInstructionInKernelMemory
		{
			get
			{
				if (codeInstruction == null)
				{
					return false;
				}
				if (reboot.enableReboot)
				{
					return true;
				}
				return codeInstruction.Address < MemoryMap.START_USERSPACE;
			}
		}

		/// <summary>
		/// Generate the required Java code to call a syscall function.
		/// The code generated must match the Java behavior implemented in
		/// pspsharp.HLE.modules.HLEModuleFunctionReflection
		/// 
		/// The following code is generated:
		///     if (func.getFirmwareVersion() <= RuntimeContext.firmwareVersion) {
		///         if (!fastSyscall) {
		///             RuntimeContext.preSyscall();
		///         }
		///         if (func.checkInsideInterrupt()) {
		///             if (IntrManager.getInstance.isInsideInterrupt()) {
		///                 cpu.gpr[_v0] = SceKernelErrors.ERROR_KERNEL_CANNOT_BE_CALLED_FROM_INTERRUPT;
		///                 goto afterSyscall;
		///             }
		///         }
		///         if (func.checkDispatchThreadEnabled()) {
		///             if (!Modules.ThreadManForUserModule.isDispatchThreadEnabled()) {
		///                 cpu.gpr[_v0] = SceKernelErrors.ERROR_KERNEL_WAIT_CAN_NOT_WAIT;
		///                 goto afterSyscall;
		///             }
		///         }
		///         if (func.isUnimplemented()) {
		///             Modules.getLogger(func.getModuleName()).warn("Unimplemented <function name> parameterName1=parameterValue1, parameterName2=parameterValue2, ...");
		///         }
		///         foreach parameter {
		///             loadParameter(parameter);
		///         }
		///         try {
		///             returnValue = <module name>.<function name>(...parameters...);
		///             storeReturnValue();
		///             if (parameterReader.hasErrorPointer()) {
		///                 errorPointer.setValue(0);
		///             }
		///         } catch (SceKernelErrorException e) {
		///             errorCode = e.errorCode;
		///             if (Modules.getLogger(func.getModuleName()).isDebugEnabled()) {
		///                 Modules.getLogger(func.getModuleName()).debug(String.format("<function name> return errorCode 0x%08X", errorCode));
		///             }
		///             if (parameterReader.hasErrorPointer()) {
		///                 errorPointer.setValue(errorCode);
		///                 cpu.gpr[_v0] = 0;
		///             } else {
		///                 cpu.gpr[_v0] = errorCode;
		///             }
		///             reload cpu.gpr[_ra]; // an exception is always clearing the whole stack
		///         }
		///         afterSyscall:
		///         if (fastSyscall) {
		///             RuntimeContext.postSyscallFast();
		///         } else {
		///             RuntimeContext.postSyscall();
		///         }
		///     } else {
		///         Modules.getLogger(func.getModuleName()).warn("<function name> is not supported in firmware version <firmwareVersion>, it requires at least firmware version <function firmwareVersion>");
		///         cpu.gpr[_v0] = -1;
		///     }
		/// </summary>
		/// <param name="func">         the syscall function </param>
		/// <param name="fastSyscall">  true if this is a fast syscall (i.e. without context switching)
		///                     false if not (i.e. a syscall where context switching could happen) </param>
		private void visitSyscall(HLEModuleFunction func, bool fastSyscall)
		{
			// The compilation of a syscall requires more stack size than usual
			maxStackSize = SYSCALL_MAX_STACK_SIZE;

			bool needFirmwareVersionCheck = true;
			if (func.FirmwareVersion >= 999)
			{
				// Dummy version number meaning valid for all versions
				needFirmwareVersionCheck = false;
			}
			else if (CodeInstructionInKernelMemory)
			{
				// When compiling code in the kernel memory space, do not perform any version check.
				// This is used by overwritten HLE functions.
				needFirmwareVersionCheck = false;
			}
			else
			{
				// When compiling code loaded from flash0, do not perform any version check.
				// This is used by overwritten HLE functions.
				SceModule module = Managers.modules.getModuleByAddress(codeInstruction.Address);
				if (module != null && !string.ReferenceEquals(module.pspfilename, null) && module.pspfilename.StartsWith("flash0:", StringComparison.Ordinal))
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("syscall from a flash0 module({0}, '{1}'), no firmware version check", module, module.pspfilename));
					}
					needFirmwareVersionCheck = false;
				}
			}

			Label unsupportedVersionLabel = null;
			if (needFirmwareVersionCheck)
			{
				unsupportedVersionLabel = new Label();
				loadImm(func.FirmwareVersion);
				mv.visitFieldInsn(Opcodes.GETSTATIC, runtimeContextInternalName, "firmwareVersion", "I");
				mv.visitJumpInsn(Opcodes.IF_ICMPGT, unsupportedVersionLabel);
			}

			// Save the syscall parameter to locals for debugging
			if (!fastSyscall)
			{
				saveParametersToLocals();
			}

			if (!fastSyscall)
			{
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "preSyscall", "()V");
			}

			Label afterSyscallLabel = new Label();

			if (func.checkInsideInterrupt())
			{
				// if (IntrManager.getInstance().isInsideInterrupt()) {
				//     if (Modules.getLogger(func.getModuleName()).isDebugEnabled()) {
				//         Modules.getLogger(func.getModuleName()).debug("<function name> return errorCode 0x80020064 (ERROR_KERNEL_CANNOT_BE_CALLED_FROM_INTERRUPT)");
				//     }
				//     cpu.gpr[_v0] = SceKernelErrors.ERROR_KERNEL_CANNOT_BE_CALLED_FROM_INTERRUPT;
				//     goto afterSyscall
				// }
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(IntrManager)), "getInstance", "()" + Type.getDescriptor(typeof(IntrManager)));
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(IntrManager)), "isInsideInterrupt", "()Z");
				Label notInsideInterrupt = new Label();
				mv.visitJumpInsn(Opcodes.IFEQ, notInsideInterrupt);

				loadModuleLoggger(func);
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(Logger)), "isDebugEnabled", "()Z");
				Label notDebug = new Label();
				mv.visitJumpInsn(Opcodes.IFEQ, notDebug);
				loadModuleLoggger(func);
				mv.visitLdcInsn(string.Format("{0} returning errorCode 0x{1:X8} (ERROR_KERNEL_CANNOT_BE_CALLED_FROM_INTERRUPT)", func.FunctionName, SceKernelErrors.ERROR_KERNEL_CANNOT_BE_CALLED_FROM_INTERRUPT));
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(Logger)), "debug", "(" + Type.getDescriptor(typeof(object)) + ")V");
				mv.visitLabel(notDebug);

				storeRegister(_v0, SceKernelErrors.ERROR_KERNEL_CANNOT_BE_CALLED_FROM_INTERRUPT);
				mv.visitJumpInsn(Opcodes.GOTO, afterSyscallLabel);
				mv.visitLabel(notInsideInterrupt);
			}

			if (func.checkDispatchThreadEnabled())
			{
				// if (!Modules.ThreadManForUserModule.isDispatchThreadEnabled() || !Interrupts.isInterruptsEnabled()) {
				//     if (Modules.getLogger(func.getModuleName()).isDebugEnabled()) {
				//         Modules.getLogger(func.getModuleName()).debug("<function name> return errorCode 0x800201A7 (ERROR_KERNEL_WAIT_CAN_NOT_WAIT)");
				//     }
				//     cpu.gpr[_v0] = SceKernelErrors.ERROR_KERNEL_WAIT_CAN_NOT_WAIT;
				//     goto afterSyscall
				// }
				loadModule("ThreadManForUser");
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(ThreadManForUser)), "isDispatchThreadEnabled", "()Z");
				Label returnError = new Label();
				mv.visitJumpInsn(Opcodes.IFEQ, returnError);
				loadProcessor();
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(Processor)), "isInterruptsEnabled", "()Z");
				Label noError = new Label();
				mv.visitJumpInsn(Opcodes.IFNE, noError);

				mv.visitLabel(returnError);
				loadModuleLoggger(func);
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(Logger)), "isDebugEnabled", "()Z");
				Label notDebug = new Label();
				mv.visitJumpInsn(Opcodes.IFEQ, notDebug);
				loadModuleLoggger(func);
				mv.visitLdcInsn(string.Format("{0} returning errorCode 0x{1:X8} (ERROR_KERNEL_WAIT_CAN_NOT_WAIT)", func.FunctionName, SceKernelErrors.ERROR_KERNEL_WAIT_CAN_NOT_WAIT));
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(Logger)), "debug", "(" + Type.getDescriptor(typeof(object)) + ")V");
				mv.visitLabel(notDebug);

				storeRegister(_v0, SceKernelErrors.ERROR_KERNEL_WAIT_CAN_NOT_WAIT);
				mv.visitJumpInsn(Opcodes.GOTO, afterSyscallLabel);
				mv.visitLabel(noError);
			}

			logSyscallStart(func);

			if (func.hasStackUsage())
			{
				loadMemory();
				loadRegister(_sp);
				loadImm(func.StackUsage);
				mv.visitInsn(Opcodes.ISUB);
				loadImm(0);
				loadImm(func.StackUsage);
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "memset", "(IBI)V");
			}

			// Collecting the parameters and calling the module function...
			CompilerParameterReader parameterReader = new CompilerParameterReader(this);

			loadModule(func.ModuleName);
			parameterReader.incrementCurrentStackSize();

			Label tryStart = new Label();
			Label tryEnd = new Label();
			Label catchSceKernelErrorException = new Label();
			mv.visitTryCatchBlock(tryStart, tryEnd, catchSceKernelErrorException, Type.getInternalName(typeof(SceKernelErrorException)));

			Type[] parameterTypes = func.HLEModuleMethod.ParameterTypes;
			Type returnType = func.HLEModuleMethod.ReturnType;
			StringBuilder methodDescriptor = new StringBuilder();
			methodDescriptor.Append("(");

			Annotation[][] paramsAnotations = func.HLEModuleMethod.ParameterAnnotations;
			int paramIndex = 0;
			foreach (Type parameterType in parameterTypes)
			{
				methodDescriptor.Append(Type.getDescriptor(parameterType));
				loadParameter(parameterReader, func, parameterType, paramsAnotations[paramIndex], afterSyscallLabel, catchSceKernelErrorException);
				paramIndex++;
			}
			methodDescriptor.Append(")");
			methodDescriptor.Append(Type.getDescriptor(returnType));

			mv.visitLabel(tryStart);

			mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(func.HLEModuleMethod.DeclaringClass), func.FunctionName, methodDescriptor.ToString());

			storeReturnValue(func, returnType);

			if (parameterReader.hasErrorPointer())
			{
				// errorPointer.setValue(0);
				mv.visitVarInsn(Opcodes.ALOAD, LOCAL_ERROR_POINTER);
				loadImm(0);
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(TErrorPointer32)), "setValue", "(I)V");
			}

			loadRegister(_v0);
			logSyscallEnd(func, false);
			mv.visitInsn(Opcodes.POP);

			mv.visitLabel(tryEnd);
			mv.visitJumpInsn(Opcodes.GOTO, afterSyscallLabel);

			// catch (SceKernelErrorException e) {
			//     errorCode = e.errorCode;
			//     if (Modules.log.isDebugEnabled()) {
			//         Modules.log.debug(String.format("<function name> return errorCode 0x%08X", errorCode));
			//     }
			//     if (hasErrorPointer()) {
			//         errorPointer.setValue(errorCode);
			//         cpu.gpr[_v0] = 0;
			//     } else {
			//         cpu.gpr[_v0] = errorCode;
			//     }
			// }
			mv.visitLabel(catchSceKernelErrorException);
			mv.visitFieldInsn(Opcodes.GETFIELD, Type.getInternalName(typeof(SceKernelErrorException)), "errorCode", "I");
			logSyscallEnd(func, true);
			if (parameterReader.hasErrorPointer())
			{
				// errorPointer.setValue(errorCode);
				// cpu.gpr[_v0] = 0;
				mv.visitVarInsn(Opcodes.ALOAD, LOCAL_ERROR_POINTER);
				mv.visitInsn(Opcodes.SWAP);
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(TErrorPointer32)), "setValue", "(I)V");
				storeRegister(_v0, 0);
			}
			else
			{
				// cpu.gpr[_v0] = errorCode;
				storeRegister(_v0);
			}

			// Reload the $ra register, the stack is lost after an exception
			CodeInstruction previousInstruction = codeBlock.getCodeInstruction(codeInstruction.Address - 4);
			if (previousInstruction != null && previousInstruction.Insn == Instructions.JR)
			{
				int jumpRegister = (previousInstruction.Opcode >> 21) & 0x1F;
				loadRegister(jumpRegister);
			}

			mv.visitLabel(afterSyscallLabel);

			if (fastSyscall)
			{
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "postSyscallFast", "()V");
			}
			else
			{
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "postSyscall", "()V");
			}

			if (needFirmwareVersionCheck)
			{
				Label afterVersionCheckLabel = new Label();
				mv.visitJumpInsn(Opcodes.GOTO, afterVersionCheckLabel);

				mv.visitLabel(unsupportedVersionLabel);
				loadModuleLoggger(func);
				mv.visitLdcInsn(string.Format("{0} is not supported in firmware version {1:D}, it requires at least firmware version {2:D}", func.FunctionName, RuntimeContext.firmwareVersion, func.FirmwareVersion));
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, Type.getInternalName(typeof(Logger)), "warn", "(" + Type.getDescriptor(typeof(object)) + ")V");
				storeRegister(_v0, -1);

				mv.visitLabel(afterVersionCheckLabel);
			}
		}

		/// <summary>
		/// Generate the required Java code to perform a syscall.
		/// 
		/// When the syscall function is an HLEModuleFunctionReflection,
		/// generate the code for calling the module function directly, as
		/// HLEModuleFunctionReflection.execute() would.
		/// 
		/// Otherwise, generate the code for calling
		///     RuntimeContext.syscall()
		/// or
		///     RuntimeContext.syscallFast()
		/// </summary>
		/// <param name="opcode">    opcode of the instruction </param>
		public virtual void visitSyscall(int opcode)
		{
			flushInstructionCount(false, false);

			int code = (opcode >> 6) & 0x000FFFFF;
			int syscallAddr = NIDMapper.Instance.getAddressBySyscall(code);
			// Call the HLE method only when it has not been overwritten
			if (syscallAddr != 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Calling overwritten HLE method '{0}' instead of syscall", NIDMapper.Instance.getNameBySyscall(code)));
				}
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, getClassName(syscallAddr, instanceIndex), StaticExecMethodName, StaticExecMethodDesc);
			}
			else
			{
				HLEModuleFunction func = HLEModuleManager.Instance.getFunctionFromSyscallCode(code);

				bool fastSyscall = isFastSyscall(code);
				bool lleSyscall = func == null && RuntimeContextLLE.LLEActive;

				if (!fastSyscall && !lleSyscall)
				{
					storePc();
				}

				bool destroyTempRegisters = true;
				if (code == syscallLoadCoreUnmappedImport)
				{
					// We do not destroy the temp registers for special syscalls
					destroyTempRegisters = false;
				}

				if (func == null)
				{
					bool inDelaySlot;
					if (CodeInstruction != null)
					{
						inDelaySlot = CodeInstruction.DelaySlot;
					}
					else
					{
						inDelaySlot = false;
					}

					loadImm(code);
					loadImm(inDelaySlot);
					if (lleSyscall)
					{
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "syscallLLE", "(IZ)I");
					}
					else if (fastSyscall)
					{
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "syscallFast", "(IZ)I");
					}
					else
					{
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "syscall", "(IZ)I");
					}

					if (CodeInstruction != null)
					{
						if (inDelaySlot)
						{
							visitContinueToAddressInRegister(_ra);
						}
						else
						{
							visitContinueToAddress(CodeInstruction.Address + 4, false);
						}
					}
					else
					{
						mv.visitInsn(Opcodes.POP);
					}
				}
				else
				{
					visitSyscall(func, fastSyscall);

					if (func.Nid == HLESyscallNid || func.Nid == InternalSyscallNid)
					{
						// We do not destroy the temp registers for special syscalls
						destroyTempRegisters = false;
					}
				}

				if (destroyTempRegisters && !lleSyscall)
				{
					// The following registers are always set to 0xDEADBEEF after a syscall
					int deadbeef = unchecked((int)0xDEADBEEF);
					storeRegister(_a0, deadbeef);
					storeRegister(_a1, deadbeef);
					storeRegister(_a2, deadbeef);
					storeRegister(_a3, deadbeef);
					storeRegister(_t0, deadbeef);
					storeRegister(_t1, deadbeef);
					storeRegister(_t2, deadbeef);
					storeRegister(_t3, deadbeef);
					storeRegister(_t4, deadbeef);
					storeRegister(_t5, deadbeef);
					storeRegister(_t6, deadbeef);
					storeRegister(_t7, deadbeef);
					storeRegister(_t8, deadbeef);
					storeRegister(_t9, deadbeef);
					prepareHiloForStore();
					mv.visitLdcInsn(new long?(unchecked((long)0xDEADBEEFDEADBEEFL)));
					storeHilo();
				}
			}

			// For code blocks consisting of a single syscall instruction
			// or a syscall without any preceding instruction,
			// generate an end for the code block.
			if (CodeBlock.Length == 1 || CodeBlock.getCodeInstruction(codeInstruction.Address - 4) == null)
			{
				loadImm(codeInstruction.Address + 4); // Returning to the instruction following the syscall
				visitJump();
			}
		}

		public virtual void startClass(ClassVisitor cv)
		{
			if (RuntimeContext.enableLineNumbers)
			{
				cv.visitSource(CodeBlock.ClassName + ".java", null);
			}
		}

		public virtual void startSequenceMethod()
		{
			if (storeCpuLocal)
			{
				mv.visitFieldInsn(Opcodes.GETSTATIC, runtimeContextInternalName, "cpu", cpuDescriptor);
				mv.visitVarInsn(Opcodes.ASTORE, LOCAL_CPU);
			}

			if (storeMemoryIntLocal)
			{
				mv.visitFieldInsn(Opcodes.GETSTATIC, runtimeContextInternalName, "memoryInt", "[I");
				mv.visitVarInsn(Opcodes.ASTORE, LOCAL_MEMORY_INT);
			}

			if (enableIntructionCounting)
			{
				currentInstructionCount = 0;
				mv.visitInsn(Opcodes.ICONST_0);
				storeLocalVar(LOCAL_INSTRUCTION_COUNT);
			}

			startNonBranchingCodeSequence();
		}

		public virtual void endSequenceMethod()
		{
			flushInstructionCount(false, true);
			mv.visitInsn(Opcodes.RETURN);
		}

		public virtual void checkSync()
		{
			if (RuntimeContext.enableDaemonThreadSync)
			{
				Label doNotWantSync = new Label();
				mv.visitFieldInsn(Opcodes.GETSTATIC, runtimeContextInternalName, "wantSync", "Z");
				mv.visitJumpInsn(Opcodes.IFEQ, doNotWantSync);
				storePc();
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, RuntimeContext.syncName, "()V");
				mv.visitLabel(doNotWantSync);
			}
		}

		private void saveParametersToLocals()
		{
			// Store all register parameters ($a0..$a3, $t0..$t3) in local variables.
			// These values will be used at the end of the HLE method for debugging buffers.
			for (int i = 0; i < LOCAL_NUMBER_SAVED_PARAMETERS; i++)
			{
				loadRegister(_a0 + i);
				storeLocalVar(LOCAL_FIRST_SAVED_PARAMETER + i);
			}
			maxLocalSize = LOCAL_MAX_WITH_SAVED_PARAMETERS;
			parametersSavedToLocals = true;
		}

		private void startHLEMethod()
		{
			HLEModuleFunction func = Utilities.getHLEFunctionByAddress(codeBlock.StartAddress);
			codeBlock.setHLEFunction(func);

			if (codeBlock.HLEFunction)
			{
				saveParametersToLocals();
				logSyscallStart(codeBlock.HLEFunction);
			}
		}

		private void endHLEMethod()
		{
			if (codeBlock.HLEFunction)
			{
				loadRegister(_v0);
				logSyscallEnd(codeBlock.HLEFunction, false);
				mv.visitInsn(Opcodes.POP);
			}
		}

		private void startInternalMethod()
		{
			// if (e != null)
			Label notReplacedLabel = new Label();
			mv.visitFieldInsn(Opcodes.GETSTATIC, codeBlock.ClassName, ReplaceFieldName, executableDescriptor);
			mv.visitJumpInsn(Opcodes.IFNULL, notReplacedLabel);
			{
				// return e.exec(returnAddress, alternativeReturnAddress, isJump);
				mv.visitFieldInsn(Opcodes.GETSTATIC, codeBlock.ClassName, ReplaceFieldName, executableDescriptor);
				mv.visitMethodInsn(Opcodes.INVOKEINTERFACE, executableInternalName, ExecMethodName, ExecMethodDesc);
				mv.visitInsn(Opcodes.IRETURN);
			}
			mv.visitLabel(notReplacedLabel);

			if (Profiler.ProfilerEnabled)
			{
				loadImm(CodeBlock.StartAddress);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, profilerInternalName, "addCall", "(I)V");
			}

			if (RuntimeContext.debugCodeBlockCalls)
			{
				loadImm(CodeBlock.StartAddress);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, RuntimeContext.debugCodeBlockStart_Renamed, "(I)V");
			}
		}

		public virtual void startMethod()
		{
			startInternalMethod();
			startSequenceMethod();
			startHLEMethod();
		}

		private void flushInstructionCount(bool local, bool last)
		{
			if (enableIntructionCounting)
			{
				if (local)
				{
					if (currentInstructionCount > 0)
					{
						mv.visitIincInsn(LOCAL_INSTRUCTION_COUNT, currentInstructionCount);
					}
				}
				else
				{
					mv.visitFieldInsn(Opcodes.GETSTATIC, runtimeContextInternalName, "currentThread", sceKernalThreadInfoDescriptor);
					mv.visitInsn(Opcodes.DUP);
					mv.visitFieldInsn(Opcodes.GETFIELD, sceKernalThreadInfoInternalName, "runClocks", "J");
					loadLocalVar(LOCAL_INSTRUCTION_COUNT);
					if (currentInstructionCount > 0)
					{
						loadImm(currentInstructionCount);
						mv.visitInsn(Opcodes.IADD);
					}
					if (Profiler.ProfilerEnabled)
					{
						mv.visitInsn(Opcodes.DUP);
						loadImm(CodeBlock.StartAddress);
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, profilerInternalName, "addInstructionCount", "(II)V");
					}
					mv.visitInsn(Opcodes.I2L);
					mv.visitInsn(Opcodes.LADD);
					mv.visitFieldInsn(Opcodes.PUTFIELD, sceKernalThreadInfoInternalName, "runClocks", "J");
					if (!last)
					{
						mv.visitInsn(Opcodes.ICONST_0);
						storeLocalVar(LOCAL_INSTRUCTION_COUNT);
					}
				}
				currentInstructionCount = 0;
			}
		}

		private void endInternalMethod()
		{
			if (RuntimeContext.debugCodeBlockCalls)
			{
				mv.visitInsn(Opcodes.DUP);
				loadImm(CodeBlock.StartAddress);
				mv.visitInsn(Opcodes.SWAP);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, RuntimeContext.debugCodeBlockEnd_Renamed, "(II)V");
			}
		}

		public virtual void endMethod()
		{
			endInternalMethod();
			endHLEMethod();
			flushInstructionCount(false, true);
		}

		public virtual void beforeInstruction(CodeInstruction codeInstruction)
		{
			if (enableIntructionCounting)
			{
				if (codeInstruction.BranchTarget)
				{
					flushInstructionCount(true, false);
				}
				currentInstructionCount++;
			}

			if (RuntimeContext.enableLineNumbers)
			{
				// Force the instruction to emit a label
				codeInstruction.getLabel(false);
			}
		}

		private void startNonBranchingCodeSequence()
		{
			vfpuPfxsState.reset();
			vfpuPfxtState.reset();
			vfpuPfxdState.reset();
		}

		private bool isNonBranchingCodeSequence(CodeInstruction codeInstruction)
		{
			return !codeInstruction.BranchTarget && !codeInstruction.Branching;
		}

		private bool previousInstructionModifiesInterruptState(CodeInstruction codeInstruction)
		{
			CodeInstruction previousInstruction = CodeBlock.getCodeInstruction(codeInstruction.Address - 4);
			if (previousInstruction == null)
			{
				return false;
			}

			return previousInstruction.hasFlags(FLAG_MODIFIES_INTERRUPT_STATE);
		}

		private void startInstructionLLE(CodeInstruction codeInstruction)
		{
			// Check for a pending interrupt only for instructions not being in a delay slot
			if (codeInstruction.DelaySlot)
			{
				return;
			}

			// TO avoid checking too often for a pending interrupt, check
			// only for instructions being the target of the branch or for those marked
			// as potentially modifying the interrupt state.
			if (!codeInstruction.BranchTarget && !previousInstructionModifiesInterruptState(codeInstruction))
			{
				return;
			}

			Label noPendingInterrupt = new Label();
			mv.visitFieldInsn(Opcodes.GETSTATIC, runtimeContextLLEInternalName, "pendingInterruptIPbits", "I");
			mv.visitJumpInsn(Opcodes.IFEQ, noPendingInterrupt);
			int returnAddress = codeInstruction.Address;
			loadImm(returnAddress);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextLLEInternalName, "checkPendingInterruptException", "(I)I");
			visitContinueToAddress(returnAddress, false);
			mv.visitLabel(noPendingInterrupt);
		}

		public virtual void startInstruction(CodeInstruction codeInstruction)
		{
			if (RuntimeContext.enableLineNumbers)
			{
				int lineNumber = codeInstruction.Address - CodeBlock.LowestAddress;
				// Java line number is unsigned 16bits
				if (lineNumber >= 0 && lineNumber <= 0xFFFF)
				{
					mv.visitLineNumber(lineNumber, codeInstruction.Label);
				}
			}

			// The pc is used by the DebuggerMemory or the LLE/MMIO
			if (Memory.Instance is DebuggerMemory || RuntimeContextLLE.LLEActive)
			{
				storePc();
			}

			if (RuntimeContextLLE.LLEActive)
			{
				startInstructionLLE(codeInstruction);
			}

			if (RuntimeContext.debugCodeInstruction_Renamed)
			{
				loadImm(codeInstruction.Address);
				loadImm(codeInstruction.Opcode);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, RuntimeContext.debugCodeInstructionName, "(II)V");
			}

			if (RuntimeContext.enableInstructionTypeCounting)
			{
				if (codeInstruction.Insn != null)
				{
					loadInstruction(codeInstruction.Insn);
					loadImm(codeInstruction.Opcode);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, RuntimeContext.instructionTypeCount_Renamed, "(" + instructionDescriptor + "I)V");
				}
			}

			if (RuntimeContext.enableDebugger)
			{
				loadImm(codeInstruction.Address);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, RuntimeContext.debuggerName, "(I)V");
			}

			if (RuntimeContext.checkCodeModification && !(codeInstruction is NativeCodeInstruction))
			{
				// Generate the following sequence:
				//
				//     if (memory.read32(pc) != opcode) {
				//         RuntimeContext.onCodeModification(pc, opcode);
				//     }
				//
				loadMemory();
				loadImm(codeInstruction.Address);
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "read32", "(I)I");
				loadImm(codeInstruction.Opcode);
				Label codeUnchanged = new Label();
				mv.visitJumpInsn(Opcodes.IF_ICMPEQ, codeUnchanged);

				loadImm(codeInstruction.Address);
				loadImm(codeInstruction.Opcode);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "onCodeModification", "(II)V");

				mv.visitLabel(codeUnchanged);
			}

			if (!isNonBranchingCodeSequence(codeInstruction))
			{
				startNonBranchingCodeSequence();
			}

			// This instructions consumes the PFXT prefix but does not use it.
			if (codeInstruction.hasFlags(Common.Instruction.FLAG_CONSUMES_VFPU_PFXT))
			{
				disablePfxSrc(vfpuPfxtState);
			}
		}

		private void disablePfxSrc(VfpuPfxSrcState pfxSrcState)
		{
			pfxSrcState.pfxSrc.enabled = false;
			pfxSrcState.Known = true;
		}

		private void disablePfxDst(VfpuPfxDstState pfxDstState)
		{
			pfxDstState.pfxDst.enabled = false;
			pfxDstState.Known = true;
		}

		public virtual void endInstruction()
		{
			if (codeInstruction != null)
			{
				if (codeInstruction.hasFlags(Common.Instruction.FLAG_USES_VFPU_PFXS))
				{
					disablePfxSrc(vfpuPfxsState);
				}

				if (codeInstruction.hasFlags(Common.Instruction.FLAG_USES_VFPU_PFXT))
				{
					disablePfxSrc(vfpuPfxtState);
				}

				if (codeInstruction.hasFlags(Common.Instruction.FLAG_USES_VFPU_PFXD))
				{
					disablePfxDst(vfpuPfxdState);
				}
			}
		}

		public virtual void startJump(int targetAddress)
		{
			// Back branch? i.e probably a loop
			if (targetAddress <= CodeInstruction.Address)
			{
				checkSync();

				if (Profiler.ProfilerEnabled)
				{
					loadImm(CodeInstruction.Address);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, profilerInternalName, "addBackBranch", "(I)V");
				}
			}
		}

		public virtual void visitJump(int opcode, CodeInstruction target)
		{
			visitJump(opcode, target.Label);
		}

		public virtual void visitJump(int opcode, Label label)
		{
			flushInstructionCount(true, false);
			mv.visitJumpInsn(opcode, label);
		}

		public virtual void visitJump(int opcode, int address)
		{
			flushInstructionCount(true, false);
			if (opcode == Opcodes.GOTO)
			{
				loadImm(address);
				visitJump();
			}
			else
			{
				Label jumpTarget = new Label();
				Label notJumpTarget = new Label();
				mv.visitJumpInsn(opcode, jumpTarget);
				mv.visitJumpInsn(Opcodes.GOTO, notJumpTarget);
				mv.visitLabel(jumpTarget);
				loadImm(address);
				visitJump();
				mv.visitLabel(notJumpTarget);
			}
		}

		public static string getClassName(int address, int instanceIndex)
		{
			return string.Format("_S1_{0:D}_0x{1:X8}", instanceIndex, address);
		}

		public static int getClassAddress(string name)
		{
			string hexAddress = name.Substring(name.LastIndexOf("0x", StringComparison.Ordinal) + 2);
			if (hexAddress.Length == 8)
			{
				return (int) Convert.ToInt64(hexAddress, 16);
			}

			return Convert.ToInt32(hexAddress, 16);
		}

		public static int getClassInstanceIndex(string name)
		{
			int startIndex = name.IndexOf("_", 1, StringComparison.Ordinal);
			int endIndex = name.LastIndexOf("_", StringComparison.Ordinal);
			string instanceIndex = name.Substring(startIndex + 1, endIndex - (startIndex + 1));

			return int.Parse(instanceIndex);
		}

		public virtual string ExecMethodName
		{
			get
			{
				return "exec";
			}
		}

		public virtual string ExecMethodDesc
		{
			get
			{
				return "()I";
			}
		}

		public virtual string ReplaceFieldName
		{
			get
			{
				return "e";
			}
		}

		public virtual string ReplaceMethodName
		{
			get
			{
				return "setExecutable";
			}
		}

		public virtual string ReplaceMethodDesc
		{
			get
			{
				return "(" + executableDescriptor + ")V";
			}
		}

		public virtual string GetMethodName
		{
			get
			{
				return "getExecutable";
			}
		}

		public virtual string GetMethodDesc
		{
			get
			{
				return "()" + executableDescriptor;
			}
		}

		public virtual string StaticExecMethodName
		{
			get
			{
				return "s";
			}
		}

		public virtual string StaticExecMethodDesc
		{
			get
			{
				return "()I";
			}
		}

		public virtual bool AutomaticMaxLocals
		{
			get
			{
				return false;
			}
		}

		public virtual int MaxLocals
		{
			get
			{
				return maxLocalSize;
			}
		}

		public virtual bool AutomaticMaxStack
		{
			get
			{
				return false;
			}
		}

		public virtual int MaxStack
		{
			get
			{
				return maxStackSize;
			}
		}

		public virtual void visitPauseEmuWithStatus(MethodVisitor mv, int status)
		{
			loadImm(status);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, RuntimeContext.pauseEmuWithStatus_Renamed, "(I)V");
		}

		public virtual void visitLogInfo(MethodVisitor mv, string message)
		{
			mv.visitLdcInsn(message);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, RuntimeContext.logInfo_Renamed, "(" + stringDescriptor + ")V");
		}

		public virtual MethodVisitor MethodVisitor
		{
			get
			{
				return mv;
			}
			set
			{
				this.mv = value;
			}
		}


		public virtual CodeInstruction CodeInstruction
		{
			get
			{
				return codeInstruction;
			}
			set
			{
				this.codeInstruction = value;
			}
		}

		public virtual CodeInstruction getCodeInstruction(int address)
		{
			return CodeBlock.getCodeInstruction(address);
		}


		public virtual int SaValue
		{
			get
			{
				return codeInstruction.SaValue;
			}
		}

		public virtual int RsRegisterIndex
		{
			get
			{
				return codeInstruction.RsRegisterIndex;
			}
		}

		public virtual int RtRegisterIndex
		{
			get
			{
				return codeInstruction.RtRegisterIndex;
			}
		}

		public virtual int RdRegisterIndex
		{
			get
			{
				return codeInstruction.RdRegisterIndex;
			}
		}

		public virtual void loadRs()
		{
			loadRegister(RsRegisterIndex);
		}

		public virtual void loadRt()
		{
			loadRegister(RtRegisterIndex);
		}

		public virtual void loadRd()
		{
			loadRegister(RdRegisterIndex);
		}

		public virtual void loadSaValue()
		{
			loadImm(SaValue);
		}

		public virtual void loadRegisterIndex(int registerIndex)
		{
			loadImm(registerIndex);
		}

		public virtual void loadRsIndex()
		{
			loadRegisterIndex(RsRegisterIndex);
		}

		public virtual void loadRtIndex()
		{
			loadRegisterIndex(RtRegisterIndex);
		}

		public virtual void loadRdIndex()
		{
			loadRegisterIndex(RdRegisterIndex);
		}

		public virtual void loadFdIndex()
		{
			loadRegisterIndex(FdRegisterIndex);
		}

		public virtual void loadFsIndex()
		{
			loadRegisterIndex(FsRegisterIndex);
		}

		public virtual void loadFtIndex()
		{
			loadRegisterIndex(FtRegisterIndex);
		}

		public virtual int getImm16(bool signedImm)
		{
			return codeInstruction.getImm16(signedImm);
		}

		public virtual int getImm14(bool signedImm)
		{
			return codeInstruction.getImm14(signedImm);
		}

		public virtual void loadImm16(bool signedImm)
		{
			loadImm(getImm16(signedImm));
		}

		public virtual void loadImm(int imm)
		{
			switch (imm)
			{
				case -1:
					mv.visitInsn(Opcodes.ICONST_M1);
					break;
				case 0:
					mv.visitInsn(Opcodes.ICONST_0);
					break;
				case 1:
					mv.visitInsn(Opcodes.ICONST_1);
					break;
				case 2:
					mv.visitInsn(Opcodes.ICONST_2);
					break;
				case 3:
					mv.visitInsn(Opcodes.ICONST_3);
					break;
				case 4:
					mv.visitInsn(Opcodes.ICONST_4);
					break;
				case 5:
					mv.visitInsn(Opcodes.ICONST_5);
					break;
				default:
					if (sbyte.MinValue <= imm && imm < sbyte.MaxValue)
					{
						mv.visitIntInsn(Opcodes.BIPUSH, imm);
					}
					else if (short.MinValue <= imm && imm < short.MaxValue)
					{
						mv.visitIntInsn(Opcodes.SIPUSH, imm);
					}
					else
					{
						mv.visitLdcInsn(new int?(imm));
					}
					break;
			}
		}

		public virtual void loadImm(bool imm)
		{
			mv.visitInsn(imm ? Opcodes.ICONST_1 : Opcodes.ICONST_0);
		}

		public virtual void loadPspNaNInt()
		{
			mv.visitFieldInsn(Opcodes.GETSTATIC, Type.getInternalName(typeof(VfpuState)), "pspNaNint", "I");
		}

		public virtual void compileInterpreterInstruction()
		{
			visitIntepreterCall(codeInstruction.Opcode, codeInstruction.Insn);
		}

		public virtual void compileRTRSIMM(string method, bool signedImm)
		{
			loadCpu();
			loadRtIndex();
			loadRsIndex();
			loadImm16(signedImm);
			mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, cpuInternalName, method, "(III)V");
		}

		public virtual void compileRDRT(string method)
		{
			loadCpu();
			loadRdIndex();
			loadRtIndex();
			mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, cpuInternalName, method, "(II)V");
		}

		public virtual void compileFDFSFT(string method)
		{
			loadCpu();
			loadFdIndex();
			loadFsIndex();
			loadFtIndex();
			mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, cpuInternalName, method, "(III)V");
		}

		public virtual void storeRd()
		{
			storeRegister(RdRegisterIndex);
		}

		public virtual void storeRd(int constantValue)
		{
			storeRegister(RdRegisterIndex, constantValue);
		}

		public virtual void storeRt()
		{
			storeRegister(RtRegisterIndex);
		}

		public virtual void storeRt(int constantValue)
		{
			storeRegister(RtRegisterIndex, constantValue);
		}

		public virtual bool RdRegister0
		{
			get
			{
				return RdRegisterIndex == _zr;
			}
		}

		public virtual bool RtRegister0
		{
			get
			{
				return RtRegisterIndex == _zr;
			}
		}

		public virtual bool RsRegister0
		{
			get
			{
				return RsRegisterIndex == _zr;
			}
		}

		public virtual void prepareRdForStore()
		{
			prepareRegisterForStore(RdRegisterIndex);
		}

		public virtual void prepareRtForStore()
		{
			prepareRegisterForStore(RtRegisterIndex);
		}

		private void loadMemoryInt()
		{
			if (storeMemoryIntLocal)
			{
				mv.visitVarInsn(Opcodes.ALOAD, LOCAL_MEMORY_INT);
			}
			else
			{
				mv.visitFieldInsn(Opcodes.GETSTATIC, runtimeContextInternalName, "memoryInt", "[I");
			}
		}

		private bool useMMIO()
		{
			if (codeInstruction == null)
			{
				return false;
			}
			return codeInstruction.useMMIO();
		}

		public virtual void memRead32(int registerIndex, int offset)
		{
			if (useMMIO())
			{
				loadMMIO();
			}
			else if (!RuntimeContext.hasMemoryInt())
			{
				loadMemory();
			}
			else
			{
				loadMemoryInt();
			}

			prepareMemIndex(registerIndex, offset, true, 32);

			if (useMMIO() || !RuntimeContext.hasMemoryInt())
			{
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "read32", "(I)I");
			}
			else
			{
				mv.visitInsn(Opcodes.IALOAD);
			}
		}

		public virtual void memRead16(int registerIndex, int offset)
		{
			if (useMMIO())
			{
				loadMMIO();
			}
			else if (!RuntimeContext.hasMemoryInt())
			{
				loadMemory();
			}
			else
			{
				loadMemoryInt();
			}

			loadRegister(registerIndex);
			if (offset != 0)
			{
				loadImm(offset);
				mv.visitInsn(Opcodes.IADD);
			}

			if (RuntimeContext.debugMemoryRead)
			{
				mv.visitInsn(Opcodes.DUP);
				loadImm(0);
				loadImm(codeInstruction.Address);
				loadImm(1);
				loadImm(16);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "debugMemoryReadWrite", "(IIIZI)V");
			}

			if (useMMIO() || !RuntimeContext.hasMemoryInt())
			{
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "read16", "(I)I");
			}
			else
			{
				if (checkMemoryAccess())
				{
					loadImm(codeInstruction.Address);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "checkMemoryRead16", "(II)I");
					loadImm(1);
					mv.visitInsn(Opcodes.IUSHR);
				}
				else
				{
					// memoryInt[(address & 0x1FFFFFFF) / 4] == memoryInt[(address << 3) >>> 5]
					loadImm(3);
					mv.visitInsn(Opcodes.ISHL);
					loadImm(4);
					mv.visitInsn(Opcodes.IUSHR);
				}
				mv.visitInsn(Opcodes.DUP);
				loadImm(1);
				mv.visitInsn(Opcodes.IAND);
				loadImm(4);
				mv.visitInsn(Opcodes.ISHL);
				storeTmp1();
				loadImm(1);
				mv.visitInsn(Opcodes.IUSHR);
				mv.visitInsn(Opcodes.IALOAD);
				loadTmp1();
				mv.visitInsn(Opcodes.IUSHR);
				loadImm(0xFFFF);
				mv.visitInsn(Opcodes.IAND);
			}
		}

		public virtual void memRead8(int registerIndex, int offset)
		{
			if (useMMIO())
			{
				loadMMIO();
			}
			else if (!RuntimeContext.hasMemoryInt())
			{
				loadMemory();
			}
			else
			{
				loadMemoryInt();
			}

			loadRegister(registerIndex);
			if (offset != 0)
			{
				loadImm(offset);
				mv.visitInsn(Opcodes.IADD);
			}

			if (RuntimeContext.debugMemoryRead)
			{
				mv.visitInsn(Opcodes.DUP);
				loadImm(0);
				loadImm(codeInstruction.Address);
				loadImm(1);
				loadImm(8);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "debugMemoryReadWrite", "(IIIZI)V");
			}

			if (useMMIO() || !RuntimeContext.hasMemoryInt())
			{
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "read8", "(I)I");
			}
			else
			{
				if (checkMemoryAccess())
				{
					loadImm(codeInstruction.Address);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "checkMemoryRead8", "(II)I");
				}
				else
				{
					// memoryInt[(address & 0x1FFFFFFF) / 4] == memoryInt[(address << 3) >>> 5]
					loadImm(3);
					mv.visitInsn(Opcodes.ISHL);
					loadImm(3);
					mv.visitInsn(Opcodes.IUSHR);
				}
				mv.visitInsn(Opcodes.DUP);
				loadImm(3);
				mv.visitInsn(Opcodes.IAND);
				loadImm(3);
				mv.visitInsn(Opcodes.ISHL);
				storeTmp1();
				loadImm(2);
				mv.visitInsn(Opcodes.IUSHR);
				mv.visitInsn(Opcodes.IALOAD);
				loadTmp1();
				mv.visitInsn(Opcodes.IUSHR);
				loadImm(0xFF);
				mv.visitInsn(Opcodes.IAND);
			}
		}

		private void prepareMemIndex(int registerIndex, int offset, bool isRead, int width)
		{
			loadRegister(registerIndex);
			if (offset != 0)
			{
				loadImm(offset);
				mv.visitInsn(Opcodes.IADD);
			}

			if (RuntimeContext.debugMemoryRead && isRead)
			{
				if (!RuntimeContext.debugMemoryReadWriteNoSP || registerIndex != _sp)
				{
					mv.visitInsn(Opcodes.DUP);
					loadImm(0);
					loadImm(codeInstruction.Address);
					loadImm(isRead);
					loadImm(width);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "debugMemoryReadWrite", "(IIIZI)V");
				}
			}

			if (!useMMIO() && RuntimeContext.hasMemoryInt())
			{
				if (registerIndex == _sp)
				{
					if (CodeInstructionInKernelMemory)
					{
						// In kernel memory, the $sp value can have the flag 0x80000000.
						// memoryInt[(address & 0x1FFFFFFF) / 4] == memoryInt[(address << 3) >>> 5]
						loadImm(3);
						mv.visitInsn(Opcodes.ISHL);
						loadImm(5);
						mv.visitInsn(Opcodes.IUSHR);
					}
					else
					{
						// No need to check for a valid memory access when referencing the $sp register
						loadImm(2);
						mv.visitInsn(Opcodes.IUSHR);
					}
				}
				else if (checkMemoryAccess())
				{
					loadImm(codeInstruction.Address);
					string checkMethodName = string.Format("checkMemory{0}{1:D}", isRead ? "Read" : "Write", width);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, checkMethodName, "(II)I");
					loadImm(2);
					mv.visitInsn(Opcodes.IUSHR);
				}
				else
				{
					// memoryInt[(address & 0x1FFFFFFF) / 4] == memoryInt[(address << 3) >>> 5]
					loadImm(3);
					mv.visitInsn(Opcodes.ISHL);
					loadImm(5);
					mv.visitInsn(Opcodes.IUSHR);
				}
			}
		}

		public virtual void prepareMemWrite32(int registerIndex, int offset)
		{
			if (useMMIO())
			{
				loadMMIO();
			}
			else if (!RuntimeContext.hasMemoryInt())
			{
				loadMemory();
			}
			else
			{
				loadMemoryInt();
			}

			prepareMemIndex(registerIndex, offset, false, 32);

			memWritePrepared = true;
		}

		public virtual void memWrite32(int registerIndex, int offset)
		{
			if (!memWritePrepared)
			{
				if (useMMIO())
				{
					loadMMIO();
				}
				else if (!RuntimeContext.hasMemoryInt())
				{
					loadMemory();
				}
				else
				{
					loadMemoryInt();
				}
				mv.visitInsn(Opcodes.SWAP);

				loadRegister(registerIndex);
				if (offset != 0)
				{
					loadImm(offset);
					mv.visitInsn(Opcodes.IADD);
				}
				if (checkMemoryAccess())
				{
					loadImm(codeInstruction.Address);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "checkMemoryWrite32", "(II)I");
				}
				mv.visitInsn(Opcodes.SWAP);
			}

			if (RuntimeContext.debugMemoryWrite)
			{
				if (!RuntimeContext.debugMemoryReadWriteNoSP || registerIndex != _sp)
				{
					mv.visitInsn(Opcodes.DUP2);
					mv.visitInsn(Opcodes.SWAP);
					loadImm(2);
					mv.visitInsn(Opcodes.ISHL);
					mv.visitInsn(Opcodes.SWAP);
					loadImm(codeInstruction.Address);
					loadImm(0);
					loadImm(32);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "debugMemoryReadWrite", "(IIIZI)V");
				}
			}

			if (useMMIO() || !RuntimeContext.hasMemoryInt())
			{
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "write32", "(II)V");
			}
			else
			{
				mv.visitInsn(Opcodes.IASTORE);
			}

			memWritePrepared = false;
		}

		public virtual void prepareMemWrite16(int registerIndex, int offset)
		{
			if (useMMIO())
			{
				loadMMIO();
			}
			else if (!RuntimeContext.hasMemoryInt())
			{
				loadMemory();
			}
			else
			{
				loadMemoryInt();
			}

			loadRegister(registerIndex);
			if (offset != 0)
			{
				loadImm(offset);
				mv.visitInsn(Opcodes.IADD);
			}

			if (!useMMIO() && RuntimeContext.hasMemoryInt())
			{
				if (checkMemoryAccess())
				{
					loadImm(codeInstruction.Address);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "checkMemoryWrite16", "(II)I");
				}
			}

			memWritePrepared = true;
		}

		public virtual void memWrite16(int registerIndex, int offset)
		{
			if (!memWritePrepared)
			{
				if (useMMIO())
				{
					loadMMIO();
				}
				else if (!RuntimeContext.hasMemoryInt())
				{
					loadMemory();
				}
				else
				{
					loadMemoryInt();
				}
				mv.visitInsn(Opcodes.SWAP);

				loadRegister(registerIndex);
				if (offset != 0)
				{
					loadImm(offset);
					mv.visitInsn(Opcodes.IADD);
				}

				if (RuntimeContext.hasMemoryInt())
				{
					if (checkMemoryAccess())
					{
						loadImm(codeInstruction.Address);
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "checkMemoryWrite16", "(II)I");
					}
				}
				mv.visitInsn(Opcodes.SWAP);
			}

			if (useMMIO() || !RuntimeContext.hasMemoryInt())
			{
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "write16", "(IS)V");
			}
			else
			{
				// tmp2 = value & 0xFFFF;
				// tmp1 = (address & 2) << 3;
				// memoryInt[address >> 2] = (memoryInt[address >> 2] & ((0xFFFF << tmp1) ^ 0xFFFFFFFF)) | (tmp2 << tmp1);
				loadImm(0xFFFF);
				mv.visitInsn(Opcodes.IAND);
				storeTmp2();
				mv.visitInsn(Opcodes.DUP);
				loadImm(2);
				mv.visitInsn(Opcodes.IAND);
				loadImm(3);
				mv.visitInsn(Opcodes.ISHL);
				storeTmp1();
				if (checkMemoryAccess())
				{
					loadImm(2);
					mv.visitInsn(Opcodes.ISHR);
				}
				else
				{
					loadImm(3);
					mv.visitInsn(Opcodes.ISHL);
					loadImm(5);
					mv.visitInsn(Opcodes.IUSHR);
				}
				mv.visitInsn(Opcodes.DUP2);
				mv.visitInsn(Opcodes.IALOAD);
				loadImm(0xFFFF);
				loadTmp1();
				mv.visitInsn(Opcodes.ISHL);
				loadImm(-1);
				mv.visitInsn(Opcodes.IXOR);
				mv.visitInsn(Opcodes.IAND);
				loadTmp2();
				loadTmp1();
				mv.visitInsn(Opcodes.ISHL);
				mv.visitInsn(Opcodes.IOR);
				mv.visitInsn(Opcodes.IASTORE);
			}

			memWritePrepared = false;
		}

		public virtual void prepareMemWrite8(int registerIndex, int offset)
		{
			if (useMMIO())
			{
				loadMMIO();
			}
			else if (!RuntimeContext.hasMemoryInt())
			{
				loadMemory();
			}
			else
			{
				loadMemoryInt();
			}

			loadRegister(registerIndex);
			if (offset != 0)
			{
				loadImm(offset);
				mv.visitInsn(Opcodes.IADD);
			}

			if (!useMMIO() && RuntimeContext.hasMemoryInt())
			{
				if (checkMemoryAccess())
				{
					loadImm(codeInstruction.Address);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "checkMemoryWrite8", "(II)I");
				}
			}

			memWritePrepared = true;
		}

		public virtual void memWrite8(int registerIndex, int offset)
		{
			if (!memWritePrepared)
			{
				if (useMMIO())
				{
					loadMMIO();
				}
				else if (!RuntimeContext.hasMemoryInt())
				{
					loadMemory();
				}
				else
				{
					loadMemoryInt();
				}
				mv.visitInsn(Opcodes.SWAP);

				loadRegister(registerIndex);
				if (offset != 0)
				{
					loadImm(offset);
					mv.visitInsn(Opcodes.IADD);
				}

				if (RuntimeContext.hasMemoryInt())
				{
					if (checkMemoryAccess())
					{
						loadImm(codeInstruction.Address);
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "checkMemoryWrite8", "(II)I");
					}
				}
				mv.visitInsn(Opcodes.SWAP);
			}

			if (useMMIO() || !RuntimeContext.hasMemoryInt())
			{
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "write8", "(IB)V");
			}
			else
			{
				// tmp2 = value & 0xFF;
				// tmp1 = (address & 3) << 3;
				// memoryInt[address >> 2] = (memoryInt[address >> 2] & ((0xFF << tmp1) ^ 0xFFFFFFFF)) | (tmp2 << tmp1);
				loadImm(0xFF);
				mv.visitInsn(Opcodes.IAND);
				storeTmp2();
				mv.visitInsn(Opcodes.DUP);
				loadImm(3);
				mv.visitInsn(Opcodes.IAND);
				loadImm(3);
				mv.visitInsn(Opcodes.ISHL);
				storeTmp1();
				if (checkMemoryAccess())
				{
					loadImm(2);
					mv.visitInsn(Opcodes.ISHR);
				}
				else
				{
					loadImm(3);
					mv.visitInsn(Opcodes.ISHL);
					loadImm(5);
					mv.visitInsn(Opcodes.IUSHR);
				}
				mv.visitInsn(Opcodes.DUP2);
				mv.visitInsn(Opcodes.IALOAD);
				loadImm(0xFF);
				loadTmp1();
				mv.visitInsn(Opcodes.ISHL);
				loadImm(-1);
				mv.visitInsn(Opcodes.IXOR);
				mv.visitInsn(Opcodes.IAND);
				loadTmp2();
				loadTmp1();
				mv.visitInsn(Opcodes.ISHL);
				mv.visitInsn(Opcodes.IOR);
				mv.visitInsn(Opcodes.IASTORE);
			}

			memWritePrepared = false;
		}

		public virtual void memWriteZero8(int registerIndex, int offset)
		{
			if (useMMIO())
			{
				loadMMIO();
			}
			else if (!RuntimeContext.hasMemoryInt())
			{
				loadMemory();
			}
			else
			{
				loadMemoryInt();
			}

			loadRegister(registerIndex);
			if (offset != 0)
			{
				loadImm(offset);
				mv.visitInsn(Opcodes.IADD);
			}

			if (!useMMIO() && RuntimeContext.hasMemoryInt())
			{
				if (checkMemoryAccess())
				{
					loadImm(codeInstruction.Address);
					mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "checkMemoryWrite8", "(II)I");
				}
			}

			if (useMMIO() || !RuntimeContext.hasMemoryInt())
			{
				loadImm(0);
				mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, memoryInternalName, "write8", "(IB)V");
			}
			else
			{
				// tmp1 = (address & 3) << 3;
				// memoryInt[address >> 2] = (memoryInt[address >> 2] & ((0xFF << tmp1) ^ 0xFFFFFFFF));
				mv.visitInsn(Opcodes.DUP);
				loadImm(3);
				mv.visitInsn(Opcodes.IAND);
				loadImm(3);
				mv.visitInsn(Opcodes.ISHL);
				storeTmp1();
				if (checkMemoryAccess())
				{
					loadImm(2);
					mv.visitInsn(Opcodes.ISHR);
				}
				else
				{
					loadImm(3);
					mv.visitInsn(Opcodes.ISHL);
					loadImm(5);
					mv.visitInsn(Opcodes.IUSHR);
				}
				mv.visitInsn(Opcodes.DUP2);
				mv.visitInsn(Opcodes.IALOAD);
				loadImm(0xFF);
				loadTmp1();
				mv.visitInsn(Opcodes.ISHL);
				loadImm(-1);
				mv.visitInsn(Opcodes.IXOR);
				mv.visitInsn(Opcodes.IAND);
				mv.visitInsn(Opcodes.IASTORE);
			}
		}

		public virtual void compileSyscall()
		{
			visitSyscall(codeInstruction.Opcode);
		}

		public virtual void convertUnsignedIntToLong()
		{
			mv.visitInsn(Opcodes.I2L);
			mv.visitLdcInsn(0xFFFFFFFFL);
			mv.visitInsn(Opcodes.LAND);
		}

		public virtual int MethodMaxInstructions
		{
			get
			{
				return methodMaxInstructions;
			}
			set
			{
				this.methodMaxInstructions = value;
			}
		}


		private bool checkMemoryAccess()
		{
			if (!RuntimeContext.hasMemoryInt())
			{
				return false;
			}

			if (RuntimeContext.memory is SafeFastMemory)
			{
				return true;
			}

			return false;
		}

		public virtual void compileDelaySlotAsBranchTarget(CodeInstruction codeInstruction)
		{
			if (codeInstruction.Insn == Instructions.NOP)
			{
				// NOP nothing to do
				return;
			}

			bool skipDelaySlotInstruction = true;
			CodeInstruction previousInstruction = CodeBlock.getCodeInstruction(codeInstruction.Address - 4);
			if (previousInstruction != null)
			{
				if (Compiler.isEndBlockInsn(previousInstruction.Address, previousInstruction.Opcode, previousInstruction.Insn))
				{
					// The previous instruction was a J, JR or unconditional branch
					// instruction, we do not need to skip the delay slot instruction
					skipDelaySlotInstruction = false;
				}
			}

			Label afterDelaySlot = null;
			if (skipDelaySlotInstruction)
			{
				afterDelaySlot = new Label();
				mv.visitJumpInsn(Opcodes.GOTO, afterDelaySlot);
			}
			codeInstruction.compile(this, mv);
			if (skipDelaySlotInstruction)
			{
				mv.visitLabel(afterDelaySlot);
			}
		}

		public virtual void compileExecuteInterpreter(int startAddress)
		{
			loadImm(startAddress);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "executeInterpreter", "(I)I");
			endMethod();
			mv.visitInsn(Opcodes.IRETURN);
		}

		private void visitNativeCodeSequence(NativeCodeSequence nativeCodeSequence, int address, NativeCodeInstruction nativeCodeInstruction)
		{
			StringBuilder methodSignature = new StringBuilder("(");
			int numberParameters = nativeCodeSequence.NumberParameters;
			for (int i = 0; i < numberParameters; i++)
			{
				loadImm(nativeCodeSequence.getParameterValue(i, address));
				methodSignature.Append("I");
			}
			if (nativeCodeSequence.MethodReturning)
			{
				methodSignature.Append(")I");
			}
			else
			{
				methodSignature.Append(")V");
			}
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(nativeCodeSequence.NativeCodeSequenceClass), nativeCodeSequence.MethodName, methodSignature.ToString());

			if (nativeCodeInstruction != null && nativeCodeInstruction.Branching)
			{
				startJump(nativeCodeInstruction.BranchingTo);
				CodeInstruction targetInstruction = CodeBlock.getCodeInstruction(nativeCodeInstruction.BranchingTo);
				if (targetInstruction != null)
				{
					visitJump(Opcodes.GOTO, targetInstruction);
				}
				else
				{
					visitJump(Opcodes.GOTO, nativeCodeInstruction.BranchingTo);
				}
			}
		}

		public virtual void compileNativeCodeSequence(NativeCodeSequence nativeCodeSequence, NativeCodeInstruction nativeCodeInstruction)
		{
			// The pc can be used by native code sequences, set it to the start address of the sequence
			storePc();

			visitNativeCodeSequence(nativeCodeSequence, nativeCodeInstruction.Address, nativeCodeInstruction);

			if (nativeCodeSequence.Returning)
			{
				loadRegister(_ra);
				endInternalMethod();
				mv.visitInsn(Opcodes.IRETURN);
			}
			else if (nativeCodeSequence.MethodReturning)
			{
				endInternalMethod();
				mv.visitInsn(Opcodes.IRETURN);
			}

			// Replacing the whole CodeBlock?
			if (CodeBlock.Length == nativeCodeSequence.NumOpcodes && !nativeCodeSequence.hasBranchInstruction())
			{
				nativeCodeManager.setCompiledNativeCodeBlock(CodeBlock.StartAddress, nativeCodeSequence);

				// Be more verbose when Debug enabled.
				// Only log "Nop" native code sequence in debug.
				if (log.DebugEnabled || nativeCodeSequence.NativeCodeSequenceClass.Equals(typeof(Nop)))
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Replacing CodeBlock at 0x{0:X8} ({1:X8}-0x{2:X8}, length {3:D}) by {4}", CodeBlock.StartAddress, CodeBlock.LowestAddress, codeBlock.HighestAddress, codeBlock.Length, nativeCodeSequence));
					}
				}
				else if (log.InfoEnabled)
				{
					log.info(string.Format("Replacing CodeBlock at 0x{0:X8} by Native Code '{1}'", CodeBlock.StartAddress, nativeCodeSequence.Name));
				}
			}
			else
			{
				// Be more verbose when Debug enabled
				int endAddress = CodeInstruction.Address + (nativeCodeSequence.NumOpcodes - 1) * 4;
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Replacing CodeSequence at 0x{0:X8}-0x{1:X8} by Native Code {2}", CodeInstruction.Address, endAddress, nativeCodeSequence));
				}
				else if (log.InfoEnabled)
				{
					log.info(string.Format("Replacing CodeSequence at 0x{0:X8}-0x{1:X8} by Native Code '{2}'", CodeInstruction.Address, endAddress, nativeCodeSequence.Name));
				}
			}
		}

		public virtual int NumberInstructionsToBeSkipped
		{
			get
			{
				return numberInstructionsToBeSkipped;
			}
		}

		public virtual bool SkipDelaySlot
		{
			get
			{
				return skipDelaySlot;
			}
		}

		public virtual void skipInstructions(int numberInstructionsToBeSkipped, bool skipDelaySlot)
		{
			this.numberInstructionsToBeSkipped = numberInstructionsToBeSkipped;
			this.skipDelaySlot = skipDelaySlot;
		}

		public virtual int FdRegisterIndex
		{
			get
			{
				return codeInstruction.FdRegisterIndex;
			}
		}

		public virtual int FsRegisterIndex
		{
			get
			{
				return codeInstruction.FsRegisterIndex;
			}
		}

		public virtual int FtRegisterIndex
		{
			get
			{
				return codeInstruction.FtRegisterIndex;
			}
		}

		public virtual void loadFd()
		{
			loadFRegister(FdRegisterIndex);
		}

		public virtual void loadFs()
		{
			loadFRegister(FsRegisterIndex);
		}

		public virtual void loadFt()
		{
			loadFRegister(FtRegisterIndex);
		}

		public virtual void prepareFdForStore()
		{
			prepareFRegisterForStore(FdRegisterIndex);
		}

		public virtual void prepareFtForStore()
		{
			prepareFRegisterForStore(FtRegisterIndex);
		}

		public virtual void storeFd()
		{
			storeFRegister(FdRegisterIndex);
		}

		public virtual void storeFt()
		{
			storeFRegister(FtRegisterIndex);
		}

		public virtual void loadFCr()
		{
			loadFRegister(CrValue);
		}

		public virtual void prepareFCrForStore()
		{
			prepareFRegisterForStore(CrValue);
		}

		public virtual void prepareVcrCcForStore(int cc)
		{
			if (preparedRegisterForStore < 0)
			{
				loadVcr();
				mv.visitFieldInsn(Opcodes.GETFIELD, Type.getInternalName(typeof(VfpuState.Vcr)), "cc", "[Z");
				loadImm(cc);
				preparedRegisterForStore = cc;
			}
		}

		public virtual void storeVcrCc(int cc)
		{
			if (preparedRegisterForStore == cc)
			{
				mv.visitInsn(Opcodes.BASTORE);
				preparedRegisterForStore = -1;
			}
			else
			{
				loadVcr();
				mv.visitFieldInsn(Opcodes.GETFIELD, Type.getInternalName(typeof(VfpuState.Vcr)), "cc", "[Z");
				mv.visitInsn(Opcodes.SWAP);
				loadImm(cc);
				mv.visitInsn(Opcodes.SWAP);
				mv.visitInsn(Opcodes.BASTORE);
			}
		}

		public virtual int CrValue
		{
			get
			{
				return codeInstruction.CrValue;
			}
		}

		public virtual void storeFCr()
		{
			storeFRegister(CrValue);
		}

		public virtual int VdRegisterIndex
		{
			get
			{
				return codeInstruction.VdRegisterIndex;
			}
		}

		public virtual int VsRegisterIndex
		{
			get
			{
				return codeInstruction.VsRegisterIndex;
			}
		}

		public virtual int VtRegisterIndex
		{
			get
			{
				return codeInstruction.VtRegisterIndex;
			}
		}

		public virtual int Vsize
		{
			get
			{
				return codeInstruction.Vsize;
			}
		}

		public virtual void loadVs(int n)
		{
			loadVRegister(Vsize, VsRegisterIndex, n, vfpuPfxsState, true);
		}

		public virtual void loadVsInt(int n)
		{
			loadVRegister(Vsize, VsRegisterIndex, n, vfpuPfxsState, false);
		}

		public virtual void loadVt(int n)
		{
			loadVRegister(Vsize, VtRegisterIndex, n, vfpuPfxtState, true);
		}

		public virtual void loadVtInt(int n)
		{
			loadVRegister(Vsize, VtRegisterIndex, n, vfpuPfxtState, false);
		}

		public virtual void loadVt(int vsize, int n)
		{
			loadVRegister(vsize, VtRegisterIndex, n, vfpuPfxtState, true);
		}

		public virtual void loadVtInt(int vsize, int n)
		{
			loadVRegister(vsize, VtRegisterIndex, n, vfpuPfxtState, false);
		}

		public virtual void loadVt(int vsize, int vt, int n)
		{
			loadVRegister(vsize, vt, n, vfpuPfxtState, true);
		}

		public virtual void loadVtInt(int vsize, int vt, int n)
		{
			loadVRegister(vsize, vt, n, vfpuPfxtState, false);
		}

		public virtual void loadVd(int n)
		{
			loadVRegister(Vsize, VdRegisterIndex, n, null, true);
		}

		public virtual void loadVdInt(int n)
		{
			loadVRegister(Vsize, VdRegisterIndex, n, null, false);
		}

		public virtual void loadVd(int vsize, int n)
		{
			loadVRegister(vsize, VdRegisterIndex, n, null, true);
		}

		public virtual void loadVdInt(int vsize, int n)
		{
			loadVRegister(vsize, VdRegisterIndex, n, null, false);
		}

		public virtual void loadVd(int vsize, int vd, int n)
		{
			loadVRegister(vsize, vd, n, null, true);
		}

		public virtual void loadVdInt(int vsize, int vd, int n)
		{
			loadVRegister(vsize, vd, n, null, false);
		}

		public virtual void prepareVdForStore(int n)
		{
			prepareVdForStore(Vsize, n);
		}

		public virtual void prepareVdForStore(int vsize, int n)
		{
			prepareVdForStore(vsize, VdRegisterIndex, n);
		}

		public virtual void prepareVdForStore(int vsize, int vd, int n)
		{
			if (pfxVdOverlap && n < vsize - 1)
			{
				// Do nothing, value will be store in tmp local variable
			}
			else
			{
				prepareVRegisterForStore(vsize, vd, n, vfpuPfxdState, true);
			}
		}

		public virtual void prepareVdForStoreInt(int n)
		{
			prepareVdForStoreInt(Vsize, n);
		}

		public virtual void prepareVdForStoreInt(int vsize, int n)
		{
			prepareVdForStoreInt(vsize, VdRegisterIndex, n);
		}

		public virtual void prepareVdForStoreInt(int vsize, int vd, int n)
		{
			if (pfxVdOverlap && n < vsize - 1)
			{
				// Do nothing, value will be stored in tmp local variable
			}
			else
			{
				prepareVRegisterForStore(vsize, vd, n, vfpuPfxdState, false);
			}
		}

		public virtual void prepareVtForStore(int n)
		{
			prepareVRegisterForStore(Vsize, VtRegisterIndex, n, null, true);
		}

		public virtual void prepareVtForStore(int vsize, int n)
		{
			prepareVRegisterForStore(vsize, VtRegisterIndex, n, null, true);
		}

		public virtual void prepareVtForStoreInt(int n)
		{
			prepareVRegisterForStore(Vsize, VtRegisterIndex, n, null, false);
		}

		public virtual void prepareVtForStoreInt(int vsize, int n)
		{
			prepareVRegisterForStore(vsize, VtRegisterIndex, n, null, false);
		}

		public virtual void storeVd(int n)
		{
			storeVd(Vsize, n);
		}

		public virtual void storeVdInt(int n)
		{
			storeVdInt(Vsize, n);
		}

		public virtual void storeVd(int vsize, int n)
		{
			storeVd(vsize, VdRegisterIndex, n);
		}

		public virtual void storeVdInt(int vsize, int n)
		{
			storeVdInt(vsize, VdRegisterIndex, n);
		}

		public virtual void storeVd(int vsize, int vd, int n)
		{
			if (pfxVdOverlap && n < vsize - 1)
			{
				storeFTmpVd(n, true);
			}
			else
			{
				storeVRegister(vsize, vd, n, vfpuPfxdState, true);
			}
		}

		public virtual void storeVdInt(int vsize, int vd, int n)
		{
			if (pfxVdOverlap && n < vsize - 1)
			{
				storeFTmpVd(n, false);
			}
			else
			{
				storeVRegister(vsize, vd, n, vfpuPfxdState, false);
			}
		}

		public virtual void storeVt(int n)
		{
			storeVRegister(Vsize, VtRegisterIndex, n, null, true);
		}

		public virtual void storeVtInt(int n)
		{
			storeVRegister(Vsize, VtRegisterIndex, n, null, false);
		}

		public virtual void storeVt(int vsize, int n)
		{
			storeVRegister(vsize, VtRegisterIndex, n, null, true);
		}

		public virtual void storeVtInt(int vsize, int n)
		{
			storeVRegister(vsize, VtRegisterIndex, n, null, false);
		}

		public virtual void storeVt(int vsize, int vt, int n)
		{
			storeVRegister(vsize, vt, n, null, true);
		}

		public virtual void storeVtInt(int vsize, int vt, int n)
		{
			storeVRegister(vsize, vt, n, null, false);
		}

		public virtual void prepareVtForStore(int vsize, int vt, int n)
		{
			prepareVRegisterForStore(vsize, vt, n, null, true);
		}

		public virtual void prepareVtForStoreInt(int vsize, int vt, int n)
		{
			prepareVRegisterForStore(vsize, vt, n, null, false);
		}

		public virtual int Imm7
		{
			get
			{
				return codeInstruction.Imm7;
			}
		}

		public virtual int Imm5
		{
			get
			{
				return codeInstruction.Imm5;
			}
		}

		public virtual int Imm4
		{
			get
			{
				return codeInstruction.Imm4;
			}
		}

		public virtual int Imm3
		{
			get
			{
				return codeInstruction.Imm3;
			}
		}

		public virtual void loadVs(int vsize, int n)
		{
			loadVRegister(vsize, VsRegisterIndex, n, vfpuPfxsState, true);
		}

		public virtual void loadVsInt(int vsize, int n)
		{
			loadVRegister(vsize, VsRegisterIndex, n, vfpuPfxsState, false);
		}

		public virtual void loadVs(int vsize, int vs, int n)
		{
			loadVRegister(vsize, vs, n, vfpuPfxsState, true);
		}

		public virtual void loadVsInt(int vsize, int vs, int n)
		{
			loadVRegister(vsize, vs, n, vfpuPfxsState, false);
		}

		public virtual void loadTmp1()
		{
			loadLocalVar(LOCAL_TMP1);
		}

		public virtual void loadTmp2()
		{
			loadLocalVar(LOCAL_TMP2);
		}

		public virtual void loadLTmp1()
		{
			mv.visitVarInsn(Opcodes.LLOAD, LOCAL_TMP1);
		}

		public virtual void loadFTmp1()
		{
			mv.visitVarInsn(Opcodes.FLOAD, LOCAL_TMP1);
		}

		public virtual void loadFTmp2()
		{
			mv.visitVarInsn(Opcodes.FLOAD, LOCAL_TMP2);
		}

		public virtual void loadFTmp3()
		{
			mv.visitVarInsn(Opcodes.FLOAD, LOCAL_TMP3);
		}

		public virtual void loadFTmp4()
		{
			mv.visitVarInsn(Opcodes.FLOAD, LOCAL_TMP4);
		}

		private void loadFTmpVd(int n, bool isFloat)
		{
			int opcode = isFloat ? Opcodes.FLOAD : Opcodes.ILOAD;
			if (n == 0)
			{
				mv.visitVarInsn(opcode, LOCAL_TMP_VD0);
			}
			else if (n == 1)
			{
				mv.visitVarInsn(opcode, LOCAL_TMP_VD1);
			}
			else
			{
				mv.visitVarInsn(opcode, LOCAL_TMP_VD2);
			}
		}

		public virtual void storeTmp1()
		{
			storeLocalVar(LOCAL_TMP1);
		}

		public virtual void storeTmp2()
		{
			storeLocalVar(LOCAL_TMP2);
		}

		public virtual void storeLTmp1()
		{
			mv.visitVarInsn(Opcodes.LSTORE, LOCAL_TMP1);
		}

		public virtual void storeFTmp1()
		{
			mv.visitVarInsn(Opcodes.FSTORE, LOCAL_TMP1);
		}

		public virtual void storeFTmp2()
		{
			mv.visitVarInsn(Opcodes.FSTORE, LOCAL_TMP2);
		}

		public virtual void storeFTmp3()
		{
			mv.visitVarInsn(Opcodes.FSTORE, LOCAL_TMP3);
		}

		public virtual void storeFTmp4()
		{
			mv.visitVarInsn(Opcodes.FSTORE, LOCAL_TMP4);
		}

		private void storeFTmpVd(int n, bool isFloat)
		{
			int opcode = isFloat ? Opcodes.FSTORE : Opcodes.ISTORE;
			if (n == 0)
			{
				mv.visitVarInsn(opcode, LOCAL_TMP_VD0);
			}
			else if (n == 1)
			{
				mv.visitVarInsn(opcode, LOCAL_TMP_VD1);
			}
			else
			{
				mv.visitVarInsn(opcode, LOCAL_TMP_VD2);
			}
		}

		public virtual VfpuPfxDstState PfxdState
		{
			get
			{
				return vfpuPfxdState;
			}
		}

		public virtual VfpuPfxSrcState PfxsState
		{
			get
			{
				return vfpuPfxsState;
			}
		}

		public virtual VfpuPfxSrcState PfxtState
		{
			get
			{
				return vfpuPfxtState;
			}
		}

		private void startPfxCompiled(VfpuPfxState vfpuPfxState, string name, string descriptor, string internalName, bool isFloat)
		{
			if (vfpuPfxState.Unknown)
			{
				if (interpretPfxLabel == null)
				{
					interpretPfxLabel = new Label();
				}

				loadVcr();
				mv.visitFieldInsn(Opcodes.GETFIELD, Type.getInternalName(typeof(VfpuState.Vcr)), name, descriptor);
				mv.visitFieldInsn(Opcodes.GETFIELD, internalName, "enabled", "Z");
				mv.visitJumpInsn(Opcodes.IFNE, interpretPfxLabel);
			}
		}

		public virtual void startPfxCompiled()
		{
			startPfxCompiled(true);
		}

		public virtual void startPfxCompiled(bool isFloat)
		{
			interpretPfxLabel = null;

			if (codeInstruction.hasFlags(Common.Instruction.FLAG_USES_VFPU_PFXS))
			{
				startPfxCompiled(vfpuPfxsState, "pfxs", Type.getDescriptor(typeof(VfpuState.Vcr.PfxSrc)), Type.getInternalName(typeof(VfpuState.Vcr.PfxSrc)), isFloat);
			}

			if (codeInstruction.hasFlags(Common.Instruction.FLAG_USES_VFPU_PFXT))
			{
				startPfxCompiled(vfpuPfxtState, "pfxt", Type.getDescriptor(typeof(VfpuState.Vcr.PfxSrc)), Type.getInternalName(typeof(VfpuState.Vcr.PfxSrc)), isFloat);
			}

			if (codeInstruction.hasFlags(Common.Instruction.FLAG_USES_VFPU_PFXD))
			{
				startPfxCompiled(vfpuPfxdState, "pfxd", Type.getDescriptor(typeof(VfpuState.Vcr.PfxDst)), Type.getInternalName(typeof(VfpuState.Vcr.PfxDst)), isFloat);
			}

			pfxVdOverlap = false;
			if (CodeInstruction.hasFlags(Common.Instruction.FLAG_USES_VFPU_PFXS | Common.Instruction.FLAG_USES_VFPU_PFXD))
			{
				pfxVdOverlap |= VsVdOverlap;
			}
			if (CodeInstruction.hasFlags(Common.Instruction.FLAG_USES_VFPU_PFXT | Common.Instruction.FLAG_USES_VFPU_PFXD))
			{
				pfxVdOverlap |= VtVdOverlap;
			}
		}

		public virtual void endPfxCompiled()
		{
			endPfxCompiled(true);
		}

		public virtual void endPfxCompiled(bool isFloat)
		{
			endPfxCompiled(Vsize, isFloat);
		}

		public virtual void endPfxCompiled(int vsize)
		{
			endPfxCompiled(vsize, true);
		}

		public virtual void endPfxCompiled(int vsize, bool isFloat)
		{
			endPfxCompiled(vsize, isFloat, true);
		}

		public virtual void endPfxCompiled(int vsize, bool isFloat, bool doFlush)
		{
			if (doFlush)
			{
				flushPfxCompiled(vsize, VdRegisterIndex, isFloat);
			}

			if (interpretPfxLabel != null)
			{
				Label continueLabel = new Label();
				mv.visitJumpInsn(Opcodes.GOTO, continueLabel);
				mv.visitLabel(interpretPfxLabel);
				compileInterpreterInstruction();
				mv.visitLabel(continueLabel);

				interpretPfxLabel = null;
			}

			pfxVdOverlap = false;
		}

		public virtual void flushPfxCompiled(int vsize, int vd, bool isFloat)
		{
			if (pfxVdOverlap)
			{
				// Write back the temporary overlap variables
				pfxVdOverlap = false;
				for (int n = 0; n < vsize - 1; n++)
				{
					if (isFloat)
					{
						prepareVdForStore(vsize, vd, n);
					}
					else
					{
						prepareVdForStoreInt(vsize, vd, n);
					}
					loadFTmpVd(n, isFloat);
					if (isFloat)
					{
						storeVd(vsize, vd, n);
					}
					else
					{
						storeVdInt(vsize, vd, n);
					}
				}
				pfxVdOverlap = true;
			}
		}

		public virtual bool isPfxConsumed(int flag)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("PFX -> {0:X8}: {1}", CodeInstruction.Address, CodeInstruction.Insn.disasm(CodeInstruction.Address, CodeInstruction.Opcode)));
			}

			int address = CodeInstruction.Address;
			while (true)
			{
				address += 4;
				CodeInstruction codeInstruction = CodeBlock.getCodeInstruction(address);

				if (log.TraceEnabled)
				{
					log.trace(string.Format("PFX    {0:X8}: {1}", codeInstruction.Address, codeInstruction.Insn.disasm(codeInstruction.Address, codeInstruction.Opcode)));
				}

				if (codeInstruction == null || !isNonBranchingCodeSequence(codeInstruction))
				{
					return false;
				}
				if (codeInstruction.hasFlags(flag))
				{
					return codeInstruction.hasFlags(Common.Instruction.FLAG_COMPILED_PFX);
				}
			}
		}

		private bool isVxVdOverlap(VfpuPfxSrcState pfxSrcState, int registerIndex)
		{
			if (!pfxSrcState.Known)
			{
				return false;
			}

			int vsize = Vsize;
			int vd = VdRegisterIndex;
			// Check if registers are overlapping
			if (registerIndex != vd)
			{
				if (vsize != 3)
				{
					// Different register numbers, no overlap possible
					return false;
				}
				// For vsize==3, a possible overlap exist. E.g.
				//    C000.t and C001.t
				// are partially overlapping.
				if ((registerIndex & 63) != (vd & 63))
				{
					return false;
				}
			}

			if (!pfxSrcState.pfxSrc.enabled)
			{
				return true;
			}

			for (int n = 0; n < vsize; n++)
			{
				if (!pfxSrcState.pfxSrc.cst[n] && pfxSrcState.pfxSrc.swz[n] != n)
				{
					return true;
				}
			}

			return false;
		}

		public virtual bool VsVdOverlap
		{
			get
			{
				return isVxVdOverlap(vfpuPfxsState, VsRegisterIndex);
			}
		}

		public virtual bool VtVdOverlap
		{
			get
			{
				return isVxVdOverlap(vfpuPfxtState, VtRegisterIndex);
			}
		}

		private bool canUseVFPUInt(int vsize)
		{
			if (vfpuPfxsState.Known && vfpuPfxsState.pfxSrc.enabled)
			{
				for (int i = 0; i < vsize; i++)
				{
					// abs, neg and cst source prefixes can be handled as int
					if (vfpuPfxsState.pfxSrc.swz[i] != i)
					{
						return false;
					}
				}
			}

			if (vfpuPfxdState.Known && vfpuPfxdState.pfxDst.enabled)
			{
				return false;
			}

			return true;
		}

		public virtual void compileVFPUInstr(object cstBefore, int opcode, string mathFunction)
		{
			int vsize = Vsize;
			bool useVt = CodeInstruction.hasFlags(Common.Instruction.FLAG_USES_VFPU_PFXT);

			if (string.ReferenceEquals(mathFunction, null) && opcode == Opcodes.NOP && !useVt && cstBefore == null && canUseVFPUInt(vsize))
			{
				// VMOV should use int instead of float
				startPfxCompiled(false);

				for (int n = 0; n < vsize; n++)
				{
					prepareVdForStoreInt(n);
					loadVsInt(n);
					storeVdInt(n);
				}

				endPfxCompiled(vsize, false);
			}
			else
			{
				startPfxCompiled(true);

				for (int n = 0; n < vsize; n++)
				{
					prepareVdForStore(n);
					if (cstBefore != null)
					{
						mv.visitLdcInsn(cstBefore);
					}

					loadVs(n);
					if (useVt)
					{
						loadVt(n);
					}
					if (!string.ReferenceEquals(mathFunction, null))
					{
						if ("abs".Equals(mathFunction))
						{
							mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), mathFunction, "(F)F");
						}
						else if ("max".Equals(mathFunction) || "min".Equals(mathFunction))
						{
							mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), mathFunction, "(FF)F");
						}
						else
						{
							mv.visitInsn(Opcodes.F2D);
							mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), mathFunction, "(D)D");
							mv.visitInsn(Opcodes.D2F);
						}
					}

					Label doneStore = null;
					if (opcode != Opcodes.NOP)
					{
						Label doneOpcode = null;

						if (opcode == Opcodes.FDIV && cstBefore == null)
						{
							// if (value1 == 0f && value2 == 0f) {
							//     result = PSP-NaN | (sign(value1) ^ sign(value2));
							// } else {
							//     result = value1 / value2;
							// }
							doneOpcode = new Label();
							doneStore = new Label();
							Label notZeroByZero = new Label();
							Label notZeroByZeroPop = new Label();
							mv.visitInsn(Opcodes.DUP2);
							mv.visitInsn(Opcodes.FCONST_0);
							mv.visitInsn(Opcodes.FCMPG);
							mv.visitJumpInsn(Opcodes.IFNE, notZeroByZeroPop);
							mv.visitInsn(Opcodes.FCONST_0);
							mv.visitInsn(Opcodes.FCMPG);
							mv.visitJumpInsn(Opcodes.IFNE, notZeroByZero);
							convertVFloatToInt();
							loadImm(0x80000000);
							mv.visitInsn(Opcodes.IAND);
							mv.visitInsn(Opcodes.SWAP);
							convertVFloatToInt();
							loadImm(0x80000000);
							mv.visitInsn(Opcodes.IAND);
							mv.visitInsn(Opcodes.IXOR);
							storeTmp1();
							// Store the NaN value as an "int" to not loose any bit.
							// Storing as float results in 0x7FC00001 instead of 0x7F800001.
							mv.visitInsn(Opcodes.DUP2_X2);
							mv.visitInsn(Opcodes.POP2);
							loadPspNaNInt();
							loadTmp1();
							mv.visitInsn(Opcodes.IOR);
							int preparedRegister = preparedRegisterForStore;
							storeVdInt(n);
							preparedRegisterForStore = preparedRegister;
							mv.visitJumpInsn(Opcodes.GOTO, doneStore);

							mv.visitLabel(notZeroByZeroPop);
							mv.visitInsn(Opcodes.POP);
							mv.visitLabel(notZeroByZero);
						}

						mv.visitInsn(opcode);

						if (doneOpcode != null)
						{
							mv.visitLabel(doneOpcode);
						}
					}

					storeVd(n);

					if (doneStore != null)
					{
						mv.visitLabel(doneStore);
					}
				}

				endPfxCompiled(vsize, true);
			}
		}

		public virtual void visitHook(NativeCodeSequence nativeCodeSequence)
		{
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(nativeCodeSequence.NativeCodeSequenceClass), nativeCodeSequence.MethodName, "()V");
		}

		public virtual bool compileVFPULoad(int registerIndex, int offset, int vt, int count)
		{
			if (!RuntimeContext.hasMemoryInt())
			{
				// Can only generate an optimized code sequence for memoryInt
				return false;
			}

			if ((vt & 32) != 0)
			{
				// Optimization possible only for column access
				return false;
			}

			// Build parameters for
			//    System.arraycopy(Object src, int srcPos, Object dest, int destPos, int length)
			// i.e.
			//    System.arraycopy(RuntimeContext.memoryInt,
			//                     RuntimeContext.checkMemoryRead32(rs + simm14, pc) >>> 2,
			//                     RuntimeContext.vprInt,
			//                     vprIndex,
			//                     countSequence * 4);
			loadMemoryInt();

			loadRegister(registerIndex);
			if (offset != 0)
			{
				loadImm(offset);
				mv.visitInsn(Opcodes.IADD);
			}
			if (checkMemoryAccess())
			{
				loadImm(CodeInstruction.Address);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(RuntimeContext)), "checkMemoryRead32", "(II)I");
				loadImm(2);
				mv.visitInsn(Opcodes.IUSHR);
			}
			else
			{
				loadImm(3);
				mv.visitInsn(Opcodes.ISHL);
				loadImm(5);
				mv.visitInsn(Opcodes.IUSHR);
			}

			loadVprInt();
			int vprIndex = VfpuState.getVprIndex((vt >> 2) & 7, vt & 3, (vt & 64) >> 6);
			loadImm(vprIndex);
			loadImm(count);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(System)), "arraycopy", arraycopyDescriptor);

			// Set the VPR float values
			for (int i = 0; i < count; i++)
			{
				loadVprFloat();
				loadImm(vprIndex + i);
				loadVprInt();
				loadImm(vprIndex + i);
				mv.visitInsn(Opcodes.IALOAD);
				convertVIntToFloat();
				mv.visitInsn(Opcodes.FASTORE);
			}

			return true;
		}

		public virtual bool compileVFPUStore(int registerIndex, int offset, int vt, int count)
		{
			if (!RuntimeContext.hasMemoryInt())
			{
				// Can only generate an optimized code sequence for memoryInt
				return false;
			}

			if ((vt & 32) != 0)
			{
				// Optimization possible only for column access
				return false;
			}

			// Build parameters for
			//    System.arraycopy(Object src, int srcPos, Object dest, int destPos, int length)
			// i.e.
			//    System.arraycopy(RuntimeContext.vprInt,
			//                     vprIndex,
			//                     RuntimeContext.memoryInt,
			//                     RuntimeContext.checkMemoryWrite32(rs + simm14, pc) >>> 2,
			//                     countSequence * 4);
			loadVprInt();
			int vprIndex = VfpuState.getVprIndex((vt >> 2) & 7, vt & 3, (vt & 64) >> 6);
			loadImm(vprIndex);
			loadMemoryInt();

			loadRegister(registerIndex);
			if (offset != 0)
			{
				loadImm(offset);
				mv.visitInsn(Opcodes.IADD);
			}
			if (checkMemoryAccess())
			{
				loadImm(CodeInstruction.Address);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(RuntimeContext)), "checkMemoryWrite32", "(II)I");
				loadImm(2);
				mv.visitInsn(Opcodes.IUSHR);
			}
			else
			{
				loadImm(3);
				mv.visitInsn(Opcodes.ISHL);
				loadImm(5);
				mv.visitInsn(Opcodes.IUSHR);
			}

			loadImm(count);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(System)), "arraycopy", arraycopyDescriptor);

			return true;
		}

		/// <summary>
		/// Search for a sequence of instructions saving registers onto the stack
		/// at the beginning of a code block and replace them by a meta
		/// instruction allowing a more efficient compiled code.
		/// 
		/// For example, a typical code block sequence looks like this:
		///		addiu      $sp, $sp, -32
		///		sw         $s3, 12($sp)
		///		addu       $s3, $a0, $zr <=> move $s3, $a0
		///		lui        $a0, 0x0307 <=> li $a0, 0x03070000
		///		ori        $a0, $a0, 16
		///		sw         $s2, 8($sp)
		///		addu       $s2, $a1, $zr <=> move $s2, $a1
		///		sw         $s1, 4($sp)
		///		sw         $s0, 0($sp)
		///		lui        $s0, 0x0000 <=> li $s0, 0x00000000
		///		sw         $ra, 16($sp)
		///		jal        0x0nnnnnnn
		///		...
		/// 
		/// This method will identify the "sw" instructions saving registers onto the
		/// stack and will merge them into a single meta instruction (SequenceSWCodeInstruction).
		/// 
		/// In the above example:
		/// 		addiu      $sp, $sp, -32
		/// 		sw         $s0/$s1/$s2/$s3/$ra, 0/4/8/12/16($sp)
		/// 		addu       $s3, $a0, $zr <=> move $s3, $a0
		/// 		lui        $a0, 0x0307 <=> li $a0, 0x03070000
		/// 		ori        $a0, $a0, 16
		/// 		addu       $s2, $a1, $zr <=> move $s2, $a1
		/// 		lui        $s0, 0x0000 <=> li $s0, 0x00000000
		/// 		jal        0x0nnnnnnn
		/// 		...
		/// </summary>
		/// <param name="codeInstructions"> the list of code instruction to be optimized. </param>
		public virtual void optimizeSequence(IList<CodeInstruction> codeInstructions)
		{
			// Optimization only possible for memoryInt
			if (!RuntimeContext.hasMemoryInt())
			{
				return;
			}
			// Disable optimizations when the profiler is enabled.
			if (Profiler.ProfilerEnabled)
			{
				return;
			}
			// Disable optimizations when the debugger is open
			if (State.debugger != null)
			{
				return;
			}

			int decreaseSpInstruction = -1;
			int stackSize = 0;
			int currentInstructionIndex = 0;
			int maxSpOffset = int.MaxValue;
			int swSequenceCount = 0;

			int[] storeSpInstructions = null;
			int[] storeSpRegisters = null;
			IList<CodeInstruction> storeSpCodeInstructions = null;
			bool[] modifiedRegisters = new bool[GprState.NUMBER_REGISTERS];
			Arrays.fill(modifiedRegisters, false);

			foreach (CodeInstruction codeInstruction in codeInstructions)
			{
				// Stop optimization when reaching a branch, branch target or delay slot
				if (codeInstruction.Branching || codeInstruction.hasFlags(Common.Instruction.FLAG_HAS_DELAY_SLOT))
				{
					break;
				}
				if (codeInstruction.BranchTarget && codeBlock.StartAddress != codeInstruction.Address)
				{
					break;
				}

				Common.Instruction insn = codeInstruction.Insn;

				// Check for a "sw" instruction if we have already seen an "addiu $sp, $sp, -nn".
				if (decreaseSpInstruction >= 0)
				{
					// Check for a "sw" instruction...
					if (insn == Instructions.SW)
					{
						int rs = codeInstruction.RsRegisterIndex;
						int rt = codeInstruction.RtRegisterIndex;
						// ...saving an unmodified register to the stack...
						if (rs == _sp)
						{
							int simm16 = codeInstruction.getImm16(true);
							if (!modifiedRegisters[rt])
							{
								// ...at a valid stack offset...
								if (simm16 >= 0 && simm16 < stackSize && (simm16 & 3) == 0 && simm16 < maxSpOffset)
								{
									int index = simm16 >> 2;
									// ...at a still ununsed stack offset
									if (storeSpInstructions[index] < 0)
									{
										storeSpCodeInstructions.Add(codeInstruction);
										storeSpInstructions[index] = currentInstructionIndex;
										storeSpRegisters[index] = rt;
										swSequenceCount++;
									}
								}
							}
							else
							{
								// The register saved to the stack has already been modified.
								// Do not optimize values above this stack offset.
								maxSpOffset = min(maxSpOffset, simm16);
							}
						}
					}
				}

				// Check for a "addiu $sp, $sp, -nn" instruction
				if (insn == Instructions.ADDI || insn == Instructions.ADDIU)
				{
					int rs = codeInstruction.RsRegisterIndex;
					int rt = codeInstruction.RtRegisterIndex;
					int simm16 = codeInstruction.getImm16(true);
					if (rt == _sp && rs == _sp && simm16 < 0)
					{
						// 2 times a $sp adjustment in the same code sequence?
						if (decreaseSpInstruction >= 0)
						{
							break;
						}

						decreaseSpInstruction = currentInstructionIndex;
						stackSize = -codeInstruction.getImm16(true);
						storeSpInstructions = new int[stackSize >> 2];
						Arrays.fill(storeSpInstructions, -1);
						storeSpRegisters = new int[storeSpInstructions.Length];
						Arrays.fill(storeSpRegisters, -1);
						storeSpCodeInstructions = new LinkedList<CodeInstruction>();
					}
					else if (rs == _sp && simm16 >= 0)
					{
						// Loading a stack address into a register (e.g. "addiu $xx, $sp, nnn").
						// Do not optimize values above this stack offset (nnn).
						maxSpOffset = min(maxSpOffset, simm16);
					}
				// Check for a "addu $reg, $sp, $reg" instruction
				}
				else if (insn == Instructions.ADD || insn == Instructions.ADDU)
				{
					int rs = codeInstruction.RsRegisterIndex;
					int rt = codeInstruction.RtRegisterIndex;
					if (rs == _sp || rt == _sp)
					{
						// Loading the stack address into a register (e.g. "addu $reg, $sp, $zr").
						// The stack could be accessed at any address, stop optimizing.
						break;
					}
				}
				else if (insn == Instructions.LW || insn == Instructions.SWC1 || insn == Instructions.LWC1)
				{
					int rs = codeInstruction.RsRegisterIndex;
					int simm16 = codeInstruction.getImm16(true);
					if (rs == _sp && simm16 >= 0)
					{
						// Accessing the stack, do not optimize values above this stack offset.
						maxSpOffset = min(maxSpOffset, simm16);
					}
				}
				else if (insn == Instructions.SVQ || insn == Instructions.LVQ)
				{
					int rs = codeInstruction.RsRegisterIndex;
					int simm14 = codeInstruction.getImm14(true);
					if (rs == _sp && simm14 >= 0)
					{
						// Accessing the stack, do not optimize values above this stack offset.
						maxSpOffset = min(maxSpOffset, simm14);
					}
				}

				if (codeInstruction.hasFlags(Common.Instruction.FLAG_WRITES_RT))
				{
					modifiedRegisters[codeInstruction.RtRegisterIndex] = true;
				}
				if (codeInstruction.hasFlags(Common.Instruction.FLAG_WRITES_RD))
				{
					modifiedRegisters[codeInstruction.RdRegisterIndex] = true;
				}

				if (maxSpOffset <= 0)
				{
					// Nothing more to do if the complete stack should not be optimized
					break;
				}

				currentInstructionIndex++;
			}

			// If we have found more than one "sw" instructions, replace them by a meta code instruction.
			if (swSequenceCount > 1)
			{
				int[] offsets = new int[swSequenceCount];
				int[] registers = new int[swSequenceCount];

				int index = 0;
				for (int i = 0; i < storeSpInstructions.Length && index < swSequenceCount; i++)
				{
					if (storeSpInstructions[i] >= 0)
					{
						offsets[index] = i << 2;
						registers[index] = storeSpRegisters[i];
						index++;
					}
				}

				// Remove all the "sw" instructions...
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
				codeInstructions.removeAll(storeSpCodeInstructions);

				// ... and replace them by a meta code instruction
				SequenceSWCodeInstruction sequenceSWCodeInstruction = new SequenceSWCodeInstruction(_sp, offsets, registers);
				sequenceSWCodeInstruction.Address = storeSpCodeInstructions[0].Address;
				codeInstructions.Insert(decreaseSpInstruction + 1, sequenceSWCodeInstruction);
			}
		}

		/// <summary>
		/// Compile a sequence
		///     sw  $zr, n($reg)
		///     sw  $zr, n+4($reg)
		///     sw  $zr, n+8($reg)
		///     ...
		/// into
		///     System.arraycopy(FastMemory.zero, 0, memoryInt, (n + $reg) >> 2, length)
		/// </summary>
		/// <param name="baseRegister"> </param>
		/// <param name="offsets"> </param>
		/// <param name="registers"> </param>
		/// <returns> true  if the sequence could be compiled
		///         false if the sequence could not be compiled </returns>
		private bool compileSWsequenceZR(int baseRegister, int[] offsets, int[] registers)
		{
			for (int i = 0; i < registers.Length; i++)
			{
				if (registers[i] != _zr)
				{
					return false;
				}
			}

			for (int i = 1; i < offsets.Length; i++)
			{
				if (offsets[i] != offsets[i - 1] + 4)
				{
					return false;
				}
			}

			int offset = offsets[0];
			int length = offsets.Length;
			do
			{
				int copyLength = System.Math.Min(length, FastMemory.zero.Length);
				// Build parameters for
				//    System.arraycopy(Object src, int srcPos, Object dest, int destPos, int length)
				// i.e.
				//    System.arraycopy(FastMemory.zero,
				//                     0,
				//                     RuntimeContext.memoryInt,
				//                     RuntimeContext.checkMemoryRead32(baseRegister + offset, pc) >>> 2,
				//                     copyLength);
				mv.visitFieldInsn(Opcodes.GETSTATIC, Type.getInternalName(typeof(FastMemory)), "zero", "[I");
				loadImm(0);
				loadMemoryInt();
				prepareMemIndex(baseRegister, offset, false, 32);
				loadImm(copyLength);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(System)), "arraycopy", arraycopyDescriptor);

				length -= copyLength;
				offset += copyLength;
			} while (length > 0);

			return true;
		}

		private bool compileSWLWsequence(int baseRegister, int[] offsets, int[] registers, bool isLW)
		{
			// Optimization only possible for memoryInt
			if (useMMIO() || !RuntimeContext.hasMemoryInt())
			{
				return false;
			}
			// Disable optimizations when the profiler is enabled.
			if (Profiler.ProfilerEnabled)
			{
				return false;
			}
			// Disable optimizations when the debugger is open
			if (State.debugger != null)
			{
				return false;
			}

			if (!isLW)
			{
				if (compileSWsequenceZR(baseRegister, offsets, registers))
				{
					return true;
				}
			}

			int offset = offsets[0];
			prepareMemIndex(baseRegister, offset, isLW, 32);
			storeTmp1();

			for (int i = 0; i < offsets.Length; i++)
			{
				int rt = registers[i];

				if (offset != offsets[i])
				{
					mv.visitIincInsn(LOCAL_TMP1, (offsets[i] - offset) >> 2);
					offset = offsets[i];
				}

				if (isLW)
				{
					if (rt != _zr)
					{
						prepareRegisterForStore(rt);
						loadMemoryInt();
						loadTmp1();
						mv.visitInsn(Opcodes.IALOAD);
						storeRegister(rt);
					}
				}
				else
				{
					loadMemoryInt();
					loadTmp1();
					loadRegister(rt);
					mv.visitInsn(Opcodes.IASTORE);
				}
			}

			return true;
		}

		public virtual bool compileSWsequence(int baseRegister, int[] offsets, int[] registers)
		{
			return compileSWLWsequence(baseRegister, offsets, registers, false);
		}

		public virtual bool compileLWsequence(int baseRegister, int[] offsets, int[] registers)
		{
			return compileSWLWsequence(baseRegister, offsets, registers, true);
		}

		public virtual void compileEret()
		{
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "executeEret", "()I");
			visitJump();
		}
	}

}