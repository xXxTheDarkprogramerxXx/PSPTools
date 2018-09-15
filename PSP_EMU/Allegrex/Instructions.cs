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
//	import static pspsharp.Allegrex.Common.COP0_STATE_COMPARE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.COP0_STATE_CONFIG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.COP0_STATE_COUNT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAGS_BRANCH_INSTRUCTION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAGS_LINK_INSTRUCTION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_TRIGGERS_EXCEPTION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_CANNOT_BE_SPLIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_COMPILED_PFX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_CONSUMES_VFPU_PFXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_ENDS_BLOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_HAS_DELAY_SLOT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_IS_JUMPING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_MODIFIES_INTERRUPT_STATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_SYSCALL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_USES_VFPU_PFXD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_USES_VFPU_PFXS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_USES_VFPU_PFXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_WRITES_RD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_WRITES_RT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.FpuState.IMPLEMENT_ROUNDING_MODES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.CompilerContext.arraycopyDescriptor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.CompilerContext.runtimeContextInternalName;

	using Instruction = pspsharp.Allegrex.Common.Instruction;
	using PfxDst = pspsharp.Allegrex.VfpuState.Vcr.PfxDst;
	using PfxSrc = pspsharp.Allegrex.VfpuState.Vcr.PfxSrc;
	using CodeInstruction = pspsharp.Allegrex.compiler.CodeInstruction;
	using Compiler = pspsharp.Allegrex.compiler.Compiler;
	using ICompilerContext = pspsharp.Allegrex.compiler.ICompilerContext;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using SequenceLWCodeInstruction = pspsharp.Allegrex.compiler.SequenceLWCodeInstruction;
	using SequenceSWCodeInstruction = pspsharp.Allegrex.compiler.SequenceSWCodeInstruction;
	using StopThreadException = pspsharp.Allegrex.compiler.StopThreadException;
	using SyscallHandler = pspsharp.HLE.SyscallHandler;
	using reboot = pspsharp.HLE.modules.reboot;
	using MEProcessor = pspsharp.mediaengine.MEProcessor;
	using Utilities = pspsharp.util.Utilities;

	using Label = org.objectweb.asm.Label;
	using MethodVisitor = org.objectweb.asm.MethodVisitor;
	using Opcodes = org.objectweb.asm.Opcodes;
	using Type = org.objectweb.asm.Type;

	/// <summary>
	/// This file has been auto-generated from Allegrex.isa file.
	/// Changes are now performed directly in this file,
	/// Allegrex.isa is no longer used.
	/// 
	/// @author hli, gid15
	/// </summary>
	public class Instructions
	{


	public static readonly Instruction NOP = new InstructionAnonymousInnerClass();

	private class InstructionAnonymousInnerClass : Instruction
	{
		public InstructionAnonymousInnerClass() : base(0)
		{
		}


	public override sealed string name()
	{
		return "NOP";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{


	}
	public override void compile(ICompilerContext context, int insn)
	{
		// Nothing to compile
	}
	public override string disasm(int address, int insn)
	{

	return "nop";
	}
	}
	public static readonly Instruction ICACHE_INDEX_INVALIDATE = new InstructionAnonymousInnerClass2();

	private class InstructionAnonymousInnerClass2 : Instruction
	{
		public InstructionAnonymousInnerClass2() : base(1)
		{
		}


	public override sealed string name()
	{
		return "ICACHE INDEX INVALIDATE";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x04, (short)imm16, rs);
	}
	}
	public static readonly Instruction ICACHE_INDEX_UNLOCK = new InstructionAnonymousInnerClass3();

	private class InstructionAnonymousInnerClass3 : Instruction
	{
		public InstructionAnonymousInnerClass3() : base(2)
		{
		}


	public override sealed string name()
	{
		return "ICACHE INDEX UNLOCK";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x06, (short)imm16, rs);
	}
	}
	public static readonly Instruction ICACHE_HIT_INVALIDATE = new InstructionAnonymousInnerClass4();

	private class InstructionAnonymousInnerClass4 : Instruction
	{
		public InstructionAnonymousInnerClass4() : base(3)
		{
		}


	public override sealed string name()
	{
		return "ICACHE HIT INVALIDATE";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (processor.cp0.MainCpu)
		{
			int addr = processor.cpu.getRegister(rs) + (short) imm16;
			int size = 64;
			RuntimeContext.invalidateRange(addr, size);
		}

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x08, (short)imm16, rs);
	}
	}
	public static readonly Instruction ICACHE_FILL = new InstructionAnonymousInnerClass5();

	private class InstructionAnonymousInnerClass5 : Instruction
	{
		public InstructionAnonymousInnerClass5() : base(4)
		{
		}


	public override sealed string name()
	{
		return "ICACHE FILL";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x0A, (short)imm16, rs);
	}
	}
	public static readonly Instruction ICACHE_FILL_WITH_LOCK = new InstructionAnonymousInnerClass6();

	private class InstructionAnonymousInnerClass6 : Instruction
	{
		public InstructionAnonymousInnerClass6() : base(5)
		{
		}


	public override sealed string name()
	{
		return "ICACHE FILL WITH LOCK";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x0B, (short)imm16, rs);
	}
	}
	public static readonly Instruction ICACHE = new InstructionAnonymousInnerClass7();

	private class InstructionAnonymousInnerClass7 : Instruction
	{
		public InstructionAnonymousInnerClass7() : base(252)
		{
		}


	public override sealed string name()
	{
		return "ICACHE";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;
		int function = (insn >> 16) & 31;

		if (processor.cp0.MainCpu)
		{
			// The instructions icache 0x01 and icache 0x03 are used to implement sceKernelIcacheInvalidateAll().
			// They do clear all the cache lines (16KB cache size):
			//    icache 0x01, addr=0x0000
			//    icache 0x03, addr=0x0000
			//    icache 0x01, addr=0x0040
			//    icache 0x03, addr=0x0040
			//    icache 0x01, addr=0x0080
			//    icache 0x03, addr=0x0080
			//    ...
			//    icache 0x01, addr=0x3FC0
			//    icache 0x03, addr=0x3FC0
			// We only react on clearing the cache line at addr=0x0000.
			int addr = processor.cpu.getRegister(rs) + (short) imm16;
			if (function == 0x01 && addr == 0x0000)
			{
				RuntimeContext.invalidateAll();
			}
		}

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;
		int function = (insn >> 16) & 31;

		return Common.disasmCODEIMMRS("icache", function, (short)imm16, rs);
	}
	}
	public static readonly Instruction DCACHE_INDEX_WRITEBACK_INVALIDATE = new InstructionAnonymousInnerClass8();

	private class InstructionAnonymousInnerClass8 : Instruction
	{
		public InstructionAnonymousInnerClass8() : base(6)
		{
		}


	public override sealed string name()
	{
		return "DCACHE INDEX WRITEBACK INVALIDATE";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x14, (short)imm16, rs);
	}
	}
	public static readonly Instruction DCACHE_INDEX_UNLOCK = new InstructionAnonymousInnerClass9();

	private class InstructionAnonymousInnerClass9 : Instruction
	{
		public InstructionAnonymousInnerClass9() : base(7)
		{
		}


	public override sealed string name()
	{
		return "DCACHE INDEX UNLOCK";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x16, (short)imm16, rs);
	}
	}
	public static readonly Instruction DCACHE_CREATE_DIRTY_EXCLUSIVE = new InstructionAnonymousInnerClass10();

	private class InstructionAnonymousInnerClass10 : Instruction
	{
		public InstructionAnonymousInnerClass10() : base(8)
		{
		}


	public override sealed string name()
	{
		return "DCACHE CREATE DIRTY EXCLUSIVE";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x18, (short)imm16, rs);
	}
	}
	public static readonly Instruction DCACHE_HIT_INVALIDATE = new InstructionAnonymousInnerClass11();

	private class InstructionAnonymousInnerClass11 : Instruction
	{
		public InstructionAnonymousInnerClass11() : base(9)
		{
		}


	public override sealed string name()
	{
		return "DCACHE HIT INVALIDATE";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x19, (short)imm16, rs);
	}
	}
	public static readonly Instruction DCACHE_HIT_WRITEBACK = new InstructionAnonymousInnerClass12();

	private class InstructionAnonymousInnerClass12 : Instruction
	{
		public InstructionAnonymousInnerClass12() : base(10)
		{
		}


	public override sealed string name()
	{
		return "DCACHE HIT WRITEBACK";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x1A, (short)imm16, rs);
	}
	}
	public static readonly Instruction DCACHE_HIT_WRITEBACK_INVALIDATE = new InstructionAnonymousInnerClass13();

	private class InstructionAnonymousInnerClass13 : Instruction
	{
		public InstructionAnonymousInnerClass13() : base(11)
		{
		}


	public override sealed string name()
	{
		return "DCACHE HIT WRITEBACK INVALIDATE";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (processor.cp0.MainCpu)
		{
			// This instruction is used by loadcore.prx to invalidate the cache after updating stubs for linking.
			int addr = processor.cpu.getRegister(rs) + (short) imm16;
			int size = 64;
			RuntimeContext.invalidateRange(addr, size);
		}

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x1B, (short)imm16, rs);
	}
	}
	public static readonly Instruction DCACHE_CREATE_DIRTY_EXCLUSIVE_WITH_LOCK = new InstructionAnonymousInnerClass14();

	private class InstructionAnonymousInnerClass14 : Instruction
	{
		public InstructionAnonymousInnerClass14() : base(12)
		{
		}


	public override sealed string name()
	{
		return "DCACHE CREATE DIRTY EXCLUSIVE WITH LOCK";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x1C, (short)imm16, rs);
	}
	}
	public static readonly Instruction DCACHE_FILL = new InstructionAnonymousInnerClass15();

	private class InstructionAnonymousInnerClass15 : Instruction
	{
		public InstructionAnonymousInnerClass15() : base(13)
		{
		}


	public override sealed string name()
	{
		return "DCACHE FILL";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x1E, (short)imm16, rs);
	}
	}
	public static readonly Instruction DCACHE_FILL_WITH_LOCK = new InstructionAnonymousInnerClass16();

	private class InstructionAnonymousInnerClass16 : Instruction
	{
		public InstructionAnonymousInnerClass16() : base(14)
		{
		}


	public override sealed string name()
	{
		return "DCACHE FILL WITH LOCK";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		return Common.disasmCODEIMMRS("cache", 0x1F, (short)imm16, rs);
	}
	}
	public static readonly Instruction DCACHE = new InstructionAnonymousInnerClass17();

	private class InstructionAnonymousInnerClass17 : Instruction
	{
		public InstructionAnonymousInnerClass17() : base(253)
		{
		}


	public override sealed string name()
	{
		return "DCACHE";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

		if (logCache.TraceEnabled)
		{
			logCache.trace(string.Format("{0} 0x{1:X8}", name(), processor.cpu.getRegister(rs) + (short)imm16));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;
		int function = (insn >> 16) & 31;

		return Common.disasmCODEIMMRS("dcache", function, (short)imm16, rs);
	}
	}
	public static readonly Instruction SYSCALL = new InstructionAnonymousInnerClass18(FLAG_SYSCALL | FLAG_TRIGGERS_EXCEPTION);

	private class InstructionAnonymousInnerClass18 : Instruction
	{
		public InstructionAnonymousInnerClass18(int FLAG_SYSCALL) : base(15, FLAG_SYSCALL | FLAG_TRIGGERS_EXCEPTION)
		{
		}


	public override sealed string name()
	{
		return "SYSCALL";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm20 = (insn >> 6) & 1048575;

		try
		{
			SyscallHandler.syscall(imm20, Processor.isInstructionInDelaySlot(processor.cpu.memory, processor.cpu.pc));
		}
		catch (Exception e)
		{
			Console.WriteLine("syscall", e);
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileSyscall();
	}
	public override string disasm(int address, int insn)
	{
		int imm20 = (insn >> 6) & 1048575;

	return Common.disasmSYSCALL(imm20);
	}
	}
	public static readonly Instruction ERET = new InstructionAnonymousInnerClass19(FLAG_CANNOT_BE_SPLIT | FLAG_ENDS_BLOCK | FLAG_MODIFIES_INTERRUPT_STATE);

	private class InstructionAnonymousInnerClass19 : Instruction
	{
		public InstructionAnonymousInnerClass19(int FLAG_CANNOT_BE_SPLIT) : base(16, FLAG_CANNOT_BE_SPLIT | FLAG_ENDS_BLOCK | FLAG_MODIFIES_INTERRUPT_STATE)
		{
		}


	public override sealed string name()
	{
		return "ERET";
	}
	public override sealed string category()
	{
		return "MIPS III";
	}
	public override void interpret(Processor processor, int insn)
	{
		processor.cpu.pc = processor.cpu.doERET(processor);
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{

	return "eret";
	}
	}
	public static readonly Instruction BREAK = new InstructionAnonymousInnerClass20(FLAG_TRIGGERS_EXCEPTION);

	private class InstructionAnonymousInnerClass20 : Instruction
	{
		public InstructionAnonymousInnerClass20(UnknownType FLAG_TRIGGERS_EXCEPTION) : base(17, FLAG_TRIGGERS_EXCEPTION)
		{
		}


	public override sealed string name()
	{
		return "BREAK";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm20 = (insn >> 6) & 1048575;
		if (RuntimeContextLLE.LLEActive)
		{
			processor.cpu.pc = RuntimeContextLLE.triggerBreakException(processor, Processor.isInstructionInDelaySlot(processor.cpu.memory, processor.cpu.pc));
		}
		else
		{
			Console.WriteLine(string.Format("0x{0:X8} - Allegrex break 0x{1:X5}", processor.cpu.pc, imm20));

			// Pause the emulator only if not ignoring invalid memory accesses
			// (I'm too lazy to introduce a new configuration flag to ignore "break" instructions).
			if (!Processor.memory.IgnoreInvalidMemoryAccess)
			{
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_BREAK);
			}
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.storePc();
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm20 = (insn >> 6) & 1048575;

	return Common.disasmBREAK(imm20);
	}
	}
	public static readonly Instruction SYNC = new InstructionAnonymousInnerClass21();

	private class InstructionAnonymousInnerClass21 : Instruction
	{
		public InstructionAnonymousInnerClass21() : base(18)
		{
		}


	public override sealed string name()
	{
		return "SYNC";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{



	}
	public override void compile(ICompilerContext context, int insn)
	{
		// Nothing to compile
	}
	public override string disasm(int address, int insn)
	{

	return "sync";
	}
	}
	public static readonly Instruction HALT = new InstructionAnonymousInnerClass22();

	private class InstructionAnonymousInnerClass22 : Instruction
	{
		public InstructionAnonymousInnerClass22() : base(19)
		{
		}


	public override sealed string name()
	{
		return "HALT";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		try
		{
			RuntimeContext.executeHalt(processor);
		}
		catch (StopThreadException)
		{
			Console.WriteLine("Exception catched while interpreting the halt instruction");
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.storePc();
		context.loadProcessor();
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, runtimeContextInternalName, "executeHalt", "(" + Type.getDescriptor(typeof(Processor)) + ")V");
	}
	public override string disasm(int address, int insn)
	{

	return "halt";
	}
	}
	public static readonly Instruction MFIC = new InstructionAnonymousInnerClass23();

	private class InstructionAnonymousInnerClass23 : Instruction
	{
		public InstructionAnonymousInnerClass23() : base(20)
		{
		}


	public override sealed string name()
	{
		return "MFIC";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rt = (insn >> 16) & 31;

		if (log.TraceEnabled)
		{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.trace(String.format("0x%08X - mfic interruptsEnabled=%b", processor.cpu.pc, processor.isInterruptsEnabled()));
			log.trace(string.Format("0x%08X - mfic interruptsEnabled=%b", processor.cpu.pc, processor.InterruptsEnabled));
		}
		processor.cpu.setRegister(rt, processor.InterruptsEnabled ? 1 : 0);
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int rt = (insn >> 16) & 31;

	return Common.disasmRT("mfic", rt);
	}
	}
	public static readonly Instruction MTIC = new InstructionAnonymousInnerClass24(FLAG_MODIFIES_INTERRUPT_STATE);

	private class InstructionAnonymousInnerClass24 : Instruction
	{
		public InstructionAnonymousInnerClass24(UnknownType FLAG_MODIFIES_INTERRUPT_STATE) : base(21, FLAG_MODIFIES_INTERRUPT_STATE)
		{
		}


	public override sealed string name()
	{
		return "MTIC";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rt = (insn >> 16) & 31;

		int value = processor.cpu.getRegister(rt);
		if (log.TraceEnabled)
		{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.trace(String.format("0x%08X - mtic interruptEnabled=%b", processor.cpu.pc, value != 0));
			log.trace(string.Format("0x%08X - mtic interruptEnabled=%b", processor.cpu.pc, value != 0));
		}
		processor.InterruptsEnabled = value != 0;

		if (RuntimeContextLLE.LLEActive)
		{
			if (processor.InterruptsEnabled)
			{
				reboot.setLog4jMDC();
			}
			RuntimeContext.checkSync();
		}
		else if (processor.InterruptsEnabled)
		{
			try
			{
				RuntimeContext.sync();
			}
			catch (StopThreadException e)
			{
				Console.WriteLine("Catched exception while executing mtic instruction", e);
			}
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int rt = (insn >> 16) & 31;

	return Common.disasmRT("mtic", rt);
	}
	}
	public static readonly Instruction ADD = new InstructionAnonymousInnerClass25(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass25 : Instruction
	{
		public InstructionAnonymousInnerClass25(UnknownType FLAG_WRITES_RD) : base(22, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "ADD";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					// just ignore overflow exception as it is useless
					processor.cpu.doADDU(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RsRegister0 && context.RtRegister0)
			{
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				if (context.RsRegister0)
				{
					context.loadRt();
				}
				else
				{
					context.loadRs();
					if (!context.RtRegister0)
					{
						context.loadRt();
						context.MethodVisitor.visitInsn(Opcodes.IADD);
					}
				}
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("add", rd, rs, rt);
	}
	}
	public static readonly Instruction ADDU = new InstructionAnonymousInnerClass26(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass26 : Instruction
	{
		public InstructionAnonymousInnerClass26(UnknownType FLAG_WRITES_RD) : base(23, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "ADDU";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doADDU(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		ADD.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("addu", rd, rs, rt);
	}
	}
	public static readonly Instruction ADDI = new InstructionAnonymousInnerClass27(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass27 : Instruction
	{
		public InstructionAnonymousInnerClass27(UnknownType FLAG_WRITES_RT) : base(24, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "ADDI";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					// just ignore overflow exception as it is useless
					processor.cpu.doADDIU(rt, rs, (short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		ADDIU.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTRSIMM("addi", rt, rs, (short)imm16);
	}
	}
	public static readonly Instruction ADDIU = new InstructionAnonymousInnerClass28(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass28 : Instruction
	{
		public InstructionAnonymousInnerClass28(UnknownType FLAG_WRITES_RT) : base(25, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "ADDIU";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doADDIU(rt, rs, (short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			int imm = context.getImm16(true);
			if (context.RsRegister0)
			{
				context.storeRt(imm);
			}
			else if (imm == 0 && context.RsRegisterIndex == context.RtRegisterIndex)
			{
				// Incrementing a register by 0 is a No-OP:
				// ADDIU $reg, $reg, 0
			}
			else
			{
				context.prepareRtForStore();
				context.loadRs();
				if (imm != 0)
				{
					context.loadImm(imm);
					context.MethodVisitor.visitInsn(Opcodes.IADD);
				}
				context.storeRt();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTRSIMM("addiu", rt, rs, (short)imm16);
	}
	}
	public static readonly Instruction AND = new InstructionAnonymousInnerClass29(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass29 : Instruction
	{
		public InstructionAnonymousInnerClass29(UnknownType FLAG_WRITES_RD) : base(26, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "AND";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doAND(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RsRegister0 || context.RtRegister0)
			{
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				context.loadRs();
				context.loadRt();
				context.MethodVisitor.visitInsn(Opcodes.IAND);
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("and", rd, rs, rt);
	}
	}
	public static readonly Instruction ANDI = new InstructionAnonymousInnerClass30(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass30 : Instruction
	{
		public InstructionAnonymousInnerClass30(UnknownType FLAG_WRITES_RT) : base(27, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "ANDI";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doANDI(rt, rs, imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			int imm = context.getImm16(false);
			if (imm == 0 || context.RsRegister0)
			{
				context.storeRt(0);
			}
			else
			{
				context.prepareRtForStore();
				context.loadRs();
				context.loadImm(imm);
				context.MethodVisitor.visitInsn(Opcodes.IAND);
				context.storeRt();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTRSIMM("andi", rt, rs, imm16);
	}
	}
	public static readonly Instruction NOR = new InstructionAnonymousInnerClass31(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass31 : Instruction
	{
		public InstructionAnonymousInnerClass31(UnknownType FLAG_WRITES_RD) : base(28, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "NOR";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doNOR(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RsRegister0 && context.RtRegister0)
			{
				// nor $zr, $zr is equivalent to storing -1
				context.storeRd(-1);
			}
			else
			{
				context.prepareRdForStore();
				if (context.RsRegister0)
				{
					context.loadRt();
				}
				else
				{
					context.loadRs();
					if (!context.RtRegister0)
					{
						// OR-ing a register with itself is a simple move.
						if (context.RsRegisterIndex != context.RtRegisterIndex)
						{
							context.loadRt();
							context.MethodVisitor.visitInsn(Opcodes.IOR);
						}
					}
				}
				context.loadImm(-1);
				context.MethodVisitor.visitInsn(Opcodes.IXOR);
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("nor", rd, rs, rt);
	}
	}
	public static readonly Instruction OR = new InstructionAnonymousInnerClass32(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass32 : Instruction
	{
		public InstructionAnonymousInnerClass32(UnknownType FLAG_WRITES_RD) : base(29, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "OR";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doOR(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RsRegister0 && context.RtRegister0)
			{
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				if (context.RsRegister0)
				{
					context.loadRt();
				}
				else
				{
					context.loadRs();
					if (!context.RtRegister0)
					{
						// OR-ing a register with itself is a simple move.
						if (context.RsRegisterIndex != context.RtRegisterIndex)
						{
							context.loadRt();
							context.MethodVisitor.visitInsn(Opcodes.IOR);
						}
					}
				}
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("or", rd, rs, rt);
	}
	}
	public static readonly Instruction ORI = new InstructionAnonymousInnerClass33(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass33 : Instruction
	{
		public InstructionAnonymousInnerClass33(UnknownType FLAG_WRITES_RT) : base(30, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "ORI";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doORI(rt, rs, imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			int imm = context.getImm16(false);
			if (context.RsRegister0)
			{
				context.storeRt(imm);
			}
			else if (imm == 0 && context.RsRegisterIndex == context.RtRegisterIndex)
			{
				// Or-ing a register with 0 and himself is a No-OP:
				// ORI $reg, $reg, 0
			}
			else
			{
				context.prepareRtForStore();
				context.loadRs();
				if (imm != 0)
				{
					context.loadImm(imm);
					context.MethodVisitor.visitInsn(Opcodes.IOR);
				}
				context.storeRt();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTRSIMM("ori", rt, rs, imm16);
	}
	}
	public static readonly Instruction XOR = new InstructionAnonymousInnerClass34(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass34 : Instruction
	{
		public InstructionAnonymousInnerClass34(UnknownType FLAG_WRITES_RD) : base(31, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "XOR";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doXOR(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RsRegister0 && context.RtRegister0)
			{
				context.storeRd(0);
			}
			else if (context.RtRegisterIndex == context.RsRegisterIndex)
			{
				// XOR-ing a register with himself is equivalent to setting to 0.
				// XOR $rd, $rs, $rs
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				if (context.RsRegister0)
				{
					context.loadRt();
				}
				else
				{
					context.loadRs();
					if (!context.RtRegister0)
					{
						context.loadRt();
						context.MethodVisitor.visitInsn(Opcodes.IXOR);
					}
				}
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("xor", rd, rs, rt);
	}
	}
	public static readonly Instruction XORI = new InstructionAnonymousInnerClass35(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass35 : Instruction
	{
		public InstructionAnonymousInnerClass35(UnknownType FLAG_WRITES_RT) : base(32, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "XORI";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doXORI(rt, rs, imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			int imm = context.getImm16(false);
			if (context.RsRegister0)
			{
				context.storeRt(imm);
			}
			else if (imm == 0 && context.RtRegisterIndex == context.RsRegisterIndex)
			{
				// XOR-ing a register with 0 and storing the result in the same register is a No-OP.
				// XORI $reg, $reg, 0
			}
			else
			{
				context.prepareRtForStore();
				context.loadRs();
				if (imm != 0)
				{
					context.loadImm(imm);
					context.MethodVisitor.visitInsn(Opcodes.IXOR);
				}
				context.storeRt();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTRSIMM("xori", rt, rs, imm16);
	}
	}
	public static readonly Instruction SLL = new InstructionAnonymousInnerClass36(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass36 : Instruction
	{
		public InstructionAnonymousInnerClass36(UnknownType FLAG_WRITES_RD) : base(33, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "SLL";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int sa = (insn >> 6) & 31;
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;


					processor.cpu.doSLL(rd, rt, sa);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RtRegister0)
			{
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				context.loadRt();
				int sa = context.SaValue;
				if (sa != 0)
				{
					context.loadImm(sa);
					context.MethodVisitor.visitInsn(Opcodes.ISHL);
				}
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int sa = (insn >> 6) & 31;
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

	return Common.disasmRDRTSA("sll", rd, rt, sa);
	}
	}
	public static readonly Instruction SLLV = new InstructionAnonymousInnerClass37(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass37 : Instruction
	{
		public InstructionAnonymousInnerClass37(UnknownType FLAG_WRITES_RD) : base(34, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "SLLV";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSLLV(rd, rt, rs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RtRegister0)
			{
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				context.loadRt();
				if (!context.RsRegister0)
				{
					context.loadRs();
					context.loadImm(31);
					context.MethodVisitor.visitInsn(Opcodes.IAND);
					context.MethodVisitor.visitInsn(Opcodes.ISHL);
				}
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRTRS("sllv", rd, rt, rs);
	}
	}
	public static readonly Instruction SRA = new InstructionAnonymousInnerClass38(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass38 : Instruction
	{
		public InstructionAnonymousInnerClass38(UnknownType FLAG_WRITES_RD) : base(35, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "SRA";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int sa = (insn >> 6) & 31;
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;


					processor.cpu.doSRA(rd, rt, sa);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RtRegister0)
			{
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				context.loadRt();
				int sa = context.SaValue;
				if (sa != 0)
				{
					context.loadImm(sa);
					context.MethodVisitor.visitInsn(Opcodes.ISHR);
				}
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int sa = (insn >> 6) & 31;
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

	return Common.disasmRDRTSA("sra", rd, rt, sa);
	}
	}
	public static readonly Instruction SRAV = new InstructionAnonymousInnerClass39(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass39 : Instruction
	{
		public InstructionAnonymousInnerClass39(UnknownType FLAG_WRITES_RD) : base(36, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "SRAV";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSRAV(rd, rt, rs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RtRegister0)
			{
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				context.loadRt();
				if (!context.RsRegister0)
				{
					context.loadRs();
					context.loadImm(31);
					context.MethodVisitor.visitInsn(Opcodes.IAND);
					context.MethodVisitor.visitInsn(Opcodes.ISHR);
				}
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRTRS("srav", rd, rt, rs);
	}
	}
	public static readonly Instruction SRL = new InstructionAnonymousInnerClass40(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass40 : Instruction
	{
		public InstructionAnonymousInnerClass40(UnknownType FLAG_WRITES_RD) : base(37, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "SRL";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int sa = (insn >> 6) & 31;
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;


					processor.cpu.doSRL(rd, rt, sa);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RtRegister0)
			{
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				context.loadRt();
				int sa = context.SaValue;
				if (sa != 0)
				{
					context.loadImm(sa);
					context.MethodVisitor.visitInsn(Opcodes.IUSHR);
				}
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int sa = (insn >> 6) & 31;
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

	return Common.disasmRDRTSA("srl", rd, rt, sa);
	}
	}
	public static readonly Instruction SRLV = new InstructionAnonymousInnerClass41(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass41 : Instruction
	{
		public InstructionAnonymousInnerClass41(UnknownType FLAG_WRITES_RD) : base(38, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "SRLV";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSRLV(rd, rt, rs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RtRegister0)
			{
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				context.loadRt();
				if (!context.RsRegister0)
				{
					context.loadRs();
					context.loadImm(31);
					context.MethodVisitor.visitInsn(Opcodes.IAND);
					context.MethodVisitor.visitInsn(Opcodes.IUSHR);
				}
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRTRS("srlv", rd, rt, rs);
	}
	}
	public static readonly Instruction ROTR = new InstructionAnonymousInnerClass42(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass42 : Instruction
	{
		public InstructionAnonymousInnerClass42(UnknownType FLAG_WRITES_RD) : base(39, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "ROTR";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int sa = (insn >> 6) & 31;
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;


					processor.cpu.doROTR(rd, rt, sa);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RtRegister0)
			{
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				context.loadRt();
				int sa = context.SaValue;
				if (sa != 0)
				{
					// rotateRight(rt, sa) = (rt >>> sa | rt << (32-sa))
					context.MethodVisitor.visitInsn(Opcodes.DUP);
					context.loadImm(sa);
					context.MethodVisitor.visitInsn(Opcodes.IUSHR);
					context.MethodVisitor.visitInsn(Opcodes.SWAP);
					context.loadImm(32 - sa);
					context.MethodVisitor.visitInsn(Opcodes.ISHL);
					context.MethodVisitor.visitInsn(Opcodes.IOR);
				}
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int sa = (insn >> 6) & 31;
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

	return Common.disasmRDRTSA("rotr", rd, rt, sa);
	}
	}
	public static readonly Instruction ROTRV = new InstructionAnonymousInnerClass43(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass43 : Instruction
	{
		public InstructionAnonymousInnerClass43(UnknownType FLAG_WRITES_RD) : base(40, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "ROTRV";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doROTRV(rd, rt, rs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RtRegister0)
			{
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				context.loadRt();
				if (!context.RsRegister0)
				{
					// rotateRight(rt, rs) = (rt >>> rs | rt << -rs)
					context.loadRs();
					context.loadImm(31);
					context.MethodVisitor.visitInsn(Opcodes.IAND);
					context.MethodVisitor.visitInsn(Opcodes.DUP2);
					context.MethodVisitor.visitInsn(Opcodes.IUSHR);
					context.MethodVisitor.visitInsn(Opcodes.DUP_X2);
					context.MethodVisitor.visitInsn(Opcodes.POP);
					context.MethodVisitor.visitInsn(Opcodes.INEG);
					context.MethodVisitor.visitInsn(Opcodes.ISHL);
					context.MethodVisitor.visitInsn(Opcodes.IOR);
				}
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRTRS("rotrv", rd, rt, rs);
	}
	}
	public static readonly Instruction SLT = new InstructionAnonymousInnerClass44(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass44 : Instruction
	{
		public InstructionAnonymousInnerClass44(UnknownType FLAG_WRITES_RD) : base(41, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "SLT";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSLT(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RsRegisterIndex == context.RtRegisterIndex)
			{
				context.storeRd(0);
			}
			else
			{
				MethodVisitor mv = context.MethodVisitor;
				Label ifLtLabel = new Label();
				Label continueLabel = new Label();
				context.prepareRdForStore();
				if (context.RsRegister0)
				{
					// rd = 0 < rt ? 1 : 0
					context.loadRt();
					mv.visitJumpInsn(Opcodes.IFGT, ifLtLabel);
				}
				else if (context.RtRegister0)
				{
					// rd = rs < 0 ? 1 : 0
					context.loadRs();
					mv.visitJumpInsn(Opcodes.IFLT, ifLtLabel);
				}
				else
				{
					// rd = rs < rt ? 1 : 0
					context.loadRs();
					context.loadRt();
					mv.visitJumpInsn(Opcodes.IF_ICMPLT, ifLtLabel);
				}
				context.loadImm(0);
				mv.visitJumpInsn(Opcodes.GOTO, continueLabel);
				mv.visitLabel(ifLtLabel);
				context.loadImm(1);
				mv.visitLabel(continueLabel);
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("slt", rd, rs, rt);
	}
	}
	public static readonly Instruction SLTI = new InstructionAnonymousInnerClass45(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass45 : Instruction
	{
		public InstructionAnonymousInnerClass45(UnknownType FLAG_WRITES_RT) : base(42, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "SLTI";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSLTI(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			int simm16 = context.getImm16(true);
			if (context.RsRegister0)
			{
				context.storeRt(simm16 > 0 ? 1 : 0);
			}
			else
			{
				MethodVisitor mv = context.MethodVisitor;
				Label ifLtLabel = new Label();
				Label continueLabel = new Label();
				context.prepareRtForStore();
				// rt = rs < simm16 ? 1 : 0
				context.loadRs();
				if (simm16 == 0)
				{
					mv.visitJumpInsn(Opcodes.IFLT, ifLtLabel);
				}
				else
				{
					context.loadImm(simm16);
					mv.visitJumpInsn(Opcodes.IF_ICMPLT, ifLtLabel);
				}
				context.loadImm(0);
				mv.visitJumpInsn(Opcodes.GOTO, continueLabel);
				mv.visitLabel(ifLtLabel);
				context.loadImm(1);
				mv.visitLabel(continueLabel);
				context.storeRt();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTRSIMM("slti", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction SLTU = new InstructionAnonymousInnerClass46(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass46 : Instruction
	{
		public InstructionAnonymousInnerClass46(UnknownType FLAG_WRITES_RD) : base(43, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "SLTU";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSLTU(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RsRegisterIndex == context.RtRegisterIndex)
			{
				// rd = x < x
				context.storeRd(0);
			}
			else if (context.RtRegister0)
			{
				// rd = rs < 0
				context.storeRd(0);
			}
			else
			{
				MethodVisitor mv = context.MethodVisitor;
				Label ifLtLabel = new Label();
				Label continueLabel = new Label();
				context.prepareRdForStore();
				if (context.RsRegister0)
				{
					// rd = 0 < rt ? 1 : 0
					context.loadRt();
					mv.visitJumpInsn(Opcodes.IFNE, ifLtLabel);
				}
				else
				{
					// rd = rs < rt ? 1 : 0
					context.loadRs();
					context.convertUnsignedIntToLong();
					context.loadRt();
					context.convertUnsignedIntToLong();
					mv.visitInsn(Opcodes.LCMP); // -1 if rs < rt, 0 if rs == rt, 1 if rs > rt
					mv.visitJumpInsn(Opcodes.IFLT, ifLtLabel);
				}
				context.loadImm(0);
				mv.visitJumpInsn(Opcodes.GOTO, continueLabel);
				mv.visitLabel(ifLtLabel);
				context.loadImm(1);
				mv.visitLabel(continueLabel);
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("sltu", rd, rs, rt);
	}
	}
	public static readonly Instruction SLTIU = new InstructionAnonymousInnerClass47(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass47 : Instruction
	{
		public InstructionAnonymousInnerClass47(UnknownType FLAG_WRITES_RT) : base(44, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "SLTIU";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSLTIU(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			int simm16 = context.getImm16(true);
			if (context.RsRegister0)
			{
				// rt = 0 < simm16 ? 1 : 0
				context.storeRt(0 < simm16 ? 1 : 0);
			}
			else if (simm16 == 0)
			{
				// rt = rs < 0
				context.storeRt(0);
			}
			else
			{
				MethodVisitor mv = context.MethodVisitor;
				Label ifLtLabel = new Label();
				Label continueLabel = new Label();
				context.prepareRtForStore();
				if (simm16 == 1)
				{
					// rt = rs < 1 ? 1 : 0   <=> rt = rs == 0 ? 1 : 0 
					context.loadRs();
					mv.visitJumpInsn(Opcodes.IFEQ, ifLtLabel);
				}
				else
				{
					// rt = rs < simm16 ? 1 : 0
					context.loadRs();
					context.convertUnsignedIntToLong();
					mv.visitLdcInsn(((long) simm16) & 0xFFFFFFFFL);
					mv.visitInsn(Opcodes.LCMP); // -1 if rs < rt, 0 if rs == rt, 1 if rs > rt
					mv.visitJumpInsn(Opcodes.IFLT, ifLtLabel);
				}
				context.loadImm(0);
				mv.visitJumpInsn(Opcodes.GOTO, continueLabel);
				mv.visitLabel(ifLtLabel);
				context.loadImm(1);
				mv.visitLabel(continueLabel);
				context.storeRt();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTRSIMM("sltiu", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction SUB = new InstructionAnonymousInnerClass48(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass48 : Instruction
	{
		public InstructionAnonymousInnerClass48(UnknownType FLAG_WRITES_RD) : base(45, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "SUB";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					// just ignore overflow exception as it is useless
					processor.cpu.doSUBU(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			// SUB $rd, $rs, $rs <=> li $rd, 0
			if (context.RsRegisterIndex == context.RtRegisterIndex)
			{
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				if (context.RsRegister0)
				{
					context.loadRt();
					context.MethodVisitor.visitInsn(Opcodes.INEG);
				}
				else
				{
					context.loadRs();
					if (!context.RtRegister0)
					{
						context.loadRt();
						context.MethodVisitor.visitInsn(Opcodes.ISUB);
					}
				}
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("sub", rd, rs, rt);
	}
	}
	public static readonly Instruction SUBU = new InstructionAnonymousInnerClass49(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass49 : Instruction
	{
		public InstructionAnonymousInnerClass49(UnknownType FLAG_WRITES_RD) : base(46, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "SUBU";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSUBU(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		SUB.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("subu", rd, rs, rt);
	}
	}
	public static readonly Instruction LUI = new InstructionAnonymousInnerClass50(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass50 : Instruction
	{
		public InstructionAnonymousInnerClass50(UnknownType FLAG_WRITES_RT) : base(47, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "LUI";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;


					processor.cpu.doLUI(rt, imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			int uimm16 = context.getImm16(false);
			context.storeRt(uimm16 << 16);
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;

	return Common.disasmRTIMM("lui", rt, imm16);
	}
	}
	public static readonly Instruction SEB = new InstructionAnonymousInnerClass51(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass51 : Instruction
	{
		public InstructionAnonymousInnerClass51(UnknownType FLAG_WRITES_RD) : base(48, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "SEB";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;


					processor.cpu.doSEB(rd, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			context.prepareRdForStore();
			context.loadRt();
			context.MethodVisitor.visitInsn(Opcodes.I2B);
			context.storeRd();
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

	return Common.disasmRDRT("seb", rd, rt);
	}
	}
	public static readonly Instruction SEH = new InstructionAnonymousInnerClass52(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass52 : Instruction
	{
		public InstructionAnonymousInnerClass52(UnknownType FLAG_WRITES_RD) : base(49, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "SEH";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;


					processor.cpu.doSEH(rd, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			context.prepareRdForStore();
			context.loadRt();
			context.MethodVisitor.visitInsn(Opcodes.I2S);
			context.storeRd();
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

	return Common.disasmRDRT("seh", rd, rt);
	}
	}
	public static readonly Instruction BITREV = new InstructionAnonymousInnerClass53(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass53 : Instruction
	{
		public InstructionAnonymousInnerClass53(UnknownType FLAG_WRITES_RD) : base(50, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "BITREV";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;


					processor.cpu.doBITREV(rd, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileRDRT("doBITREV");
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

	return Common.disasmRDRT("bitrev", rd, rt);
	}
	}
	public static readonly Instruction WSBH = new InstructionAnonymousInnerClass54(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass54 : Instruction
	{
		public InstructionAnonymousInnerClass54(UnknownType FLAG_WRITES_RD) : base(51, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "WSBH";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;


					processor.cpu.doWSBH(rd, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileRDRT("doWSBH");
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

	return Common.disasmRDRT("wsbh", rd, rt);
	}
	}
	public static readonly Instruction WSBW = new InstructionAnonymousInnerClass55(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass55 : Instruction
	{
		public InstructionAnonymousInnerClass55(UnknownType FLAG_WRITES_RD) : base(52, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "WSBW";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;


					processor.cpu.doWSBW(rd, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileRDRT("doWSBW");
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

	return Common.disasmRDRT("wsbw", rd, rt);
	}
	}
	public static readonly Instruction MOVZ = new InstructionAnonymousInnerClass56(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass56 : Instruction
	{
		public InstructionAnonymousInnerClass56(UnknownType FLAG_WRITES_RD) : base(53, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "MOVZ";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doMOVZ(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.loadRt();
		Label doNotChange = new Label();
		context.MethodVisitor.visitJumpInsn(Opcodes.IFNE, doNotChange);
		context.prepareRdForStore();
		context.loadRs();
		context.storeRd();
		context.MethodVisitor.visitLabel(doNotChange);
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("movz", rd, rs, rt);
	}
	}
	public static readonly Instruction MOVN = new InstructionAnonymousInnerClass57(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass57 : Instruction
	{
		public InstructionAnonymousInnerClass57(UnknownType FLAG_WRITES_RD) : base(54, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "MOVN";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doMOVN(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.loadRt();
		Label doNotChange = new Label();
		context.MethodVisitor.visitJumpInsn(Opcodes.IFEQ, doNotChange);
		context.prepareRdForStore();
		context.loadRs();
		context.storeRd();
		context.MethodVisitor.visitLabel(doNotChange);
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("movn", rd, rs, rt);
	}
	}
	public static readonly Instruction MAX = new InstructionAnonymousInnerClass58(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass58 : Instruction
	{
		public InstructionAnonymousInnerClass58(UnknownType FLAG_WRITES_RD) : base(55, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "MAX";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doMAX(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			Label continueLabel = new Label();
			if (context.RdRegisterIndex == context.RtRegisterIndex)
			{
				// When $rd==$rt:
				// if ($rs > $rt) {
				//   $rd = $rs
				// }
				context.loadRs();
				context.loadRt();
				context.MethodVisitor.visitJumpInsn(Opcodes.IF_ICMPLE, continueLabel);
				context.prepareRdForStore();
				context.loadRs();
				context.storeRd();
				context.MethodVisitor.visitLabel(continueLabel);
			}
			else if (context.RdRegisterIndex == context.RsRegisterIndex)
			{
				// When $rd==$rs:
				// if ($rs < $rt) {
				//   $rd = $rt
				// }
				context.loadRs();
				context.loadRt();
				context.MethodVisitor.visitJumpInsn(Opcodes.IF_ICMPGE, continueLabel);
				context.prepareRdForStore();
				context.loadRt();
				context.storeRd();
				context.MethodVisitor.visitLabel(continueLabel);
			}
			else if (context.RsRegisterIndex == context.RtRegisterIndex)
			{
				// When $rs==$rt:
				// $rd = $rs
				context.prepareRdForStore();
				context.loadRs();
				context.storeRd();
			}
			else
			{
				// When $rd!=$rs and $rd!=$rt and $rs!=$rt:
				// if ($rs > $rt) {
				//   $rd = $rs
				// } else {
				//   $rd = $rt
				// }
				context.prepareRdForStore();
				context.loadRs();
				context.loadRt();
				Label case1Label = new Label();
				context.MethodVisitor.visitJumpInsn(Opcodes.IF_ICMPLE, case1Label);
				context.loadRs();
				context.MethodVisitor.visitJumpInsn(Opcodes.GOTO, continueLabel);
				context.MethodVisitor.visitLabel(case1Label);
				context.loadRt();
				context.MethodVisitor.visitLabel(continueLabel);
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("max", rd, rs, rt);
	}
	}
	public static readonly Instruction MIN = new InstructionAnonymousInnerClass59(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass59 : Instruction
	{
		public InstructionAnonymousInnerClass59(UnknownType FLAG_WRITES_RD) : base(56, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "MIN";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doMIN(rd, rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			Label continueLabel = new Label();
			if (context.RdRegisterIndex == context.RtRegisterIndex)
			{
				// When $rd==$rt:
				// if ($rs < $rt) {
				//   $rd = $rs
				// }
				context.loadRs();
				context.loadRt();
				context.MethodVisitor.visitJumpInsn(Opcodes.IF_ICMPGE, continueLabel);
				context.prepareRdForStore();
				context.loadRs();
				context.storeRd();
				context.MethodVisitor.visitLabel(continueLabel);
			}
			else if (context.RdRegisterIndex == context.RsRegisterIndex)
			{
				// When $rd==$rs:
				// if ($rs > $rt) {
				//   $rd = $rt
				// }
				context.loadRs();
				context.loadRt();
				context.MethodVisitor.visitJumpInsn(Opcodes.IF_ICMPLE, continueLabel);
				context.prepareRdForStore();
				context.loadRt();
				context.storeRd();
				context.MethodVisitor.visitLabel(continueLabel);
			}
			else if (context.RsRegisterIndex == context.RtRegisterIndex)
			{
				// When $rs==$rt:
				// $rd = $rs
				context.prepareRdForStore();
				context.loadRs();
				context.storeRd();
			}
			else
			{
				// When $rd!=$rs and $rd!=$rt and $rs!=$rt:
				// if ($rs < $rt) {
				//   $rd = $rs
				// } else {
				//   $rd = $rt
				// }
				context.prepareRdForStore();
				context.loadRs();
				context.loadRt();
				Label case1Label = new Label();
				context.MethodVisitor.visitJumpInsn(Opcodes.IF_ICMPGE, case1Label);
				context.loadRs();
				context.MethodVisitor.visitJumpInsn(Opcodes.GOTO, continueLabel);
				context.MethodVisitor.visitLabel(case1Label);
				context.loadRt();
				context.MethodVisitor.visitLabel(continueLabel);
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRSRT("min", rd, rs, rt);
	}
	}
	public static readonly Instruction CLZ = new InstructionAnonymousInnerClass60(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass60 : Instruction
	{
		public InstructionAnonymousInnerClass60(UnknownType FLAG_WRITES_RD) : base(57, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "CLZ";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doCLZ(rd, rs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RsRegister0)
			{
				context.storeRd(32);
			}
			else
			{
				context.prepareRdForStore();
				context.loadRs();
				context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Integer)), "numberOfLeadingZeros", "(I)I");
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRS("clz", rd, rs);
	}
	}
	public static readonly Instruction CLO = new InstructionAnonymousInnerClass61(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass61 : Instruction
	{
		public InstructionAnonymousInnerClass61(UnknownType FLAG_WRITES_RD) : base(58, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "CLO";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doCLO(rd, rs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			if (context.RsRegister0)
			{
				context.storeRd(0);
			}
			else
			{
				context.prepareRdForStore();
				context.loadRs();
				context.loadImm(-1);
				context.MethodVisitor.visitInsn(Opcodes.IXOR);
				context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Integer)), "numberOfLeadingZeros", "(I)I");
				context.storeRd();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRS("clo", rd, rs);
	}
	}
	public static readonly Instruction EXT = new InstructionAnonymousInnerClass62(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass62 : Instruction
	{
		public InstructionAnonymousInnerClass62(UnknownType FLAG_WRITES_RT) : base(59, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "EXT";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int lsb = (insn >> 6) & 31;
		int msb = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doEXT(rt, rs, lsb, msb);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			int lsb = (insn >> 6) & 31;
			int msb = (insn >> 11) & 31;
			int mask = ~(~0 << (msb + 1));
			if (context.RsRegister0 || mask == 0)
			{
				context.storeRt(0);
			}
			else
			{
				context.prepareRtForStore();
				context.loadRs();
				if (lsb != 0)
				{
					context.loadImm(lsb);
					context.MethodVisitor.visitInsn(Opcodes.IUSHR);
				}
				if (mask != unchecked((int)0xFFFFFFFF))
				{
					context.loadImm(mask);
					context.MethodVisitor.visitInsn(Opcodes.IAND);
				}
				context.storeRt();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int lsb = (insn >> 6) & 31;
		int msb = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmEXT(rt, rs, lsb, msb);
	}
	}
	public static readonly Instruction INS = new InstructionAnonymousInnerClass63(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass63 : Instruction
	{
		public InstructionAnonymousInnerClass63(UnknownType FLAG_WRITES_RT) : base(60, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "INS";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int lsb = (insn >> 6) & 31;
		int msb = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doINS(rt, rs, lsb, msb);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			int lsb = (insn >> 6) & 31;
			int msb = (insn >> 11) & 31;
			int mask = ~(~0 << (msb - lsb + 1)) << lsb;

			if (mask == unchecked((int)0xFFFFFFFF) && context.RsRegister0)
			{
				context.storeRt(0);
			}
			else if (mask != 0)
			{
				context.prepareRtForStore();
				if (mask == unchecked((int)0xFFFFFFFF))
				{
					context.loadRs();
					if (lsb != 0)
					{
						context.loadImm(lsb);
						context.MethodVisitor.visitInsn(Opcodes.ISHL);
					}
				}
				else
				{
					context.loadRt();
					context.loadImm(~mask);
					context.MethodVisitor.visitInsn(Opcodes.IAND);
					if (!context.RsRegister0)
					{
						context.loadRs();
						if (lsb != 0)
						{
							context.loadImm(lsb);
							context.MethodVisitor.visitInsn(Opcodes.ISHL);
						}
						if (mask != unchecked((int)0xFFFFFFFF))
						{
							context.loadImm(mask);
							context.MethodVisitor.visitInsn(Opcodes.IAND);
						}
						context.MethodVisitor.visitInsn(Opcodes.IOR);
					}
				}
				context.storeRt();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int lsb = (insn >> 6) & 31;
		int msb = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmINS(rt, rs, lsb, msb);
	}
	}
	public static readonly Instruction MULT = new InstructionAnonymousInnerClass64();

	private class InstructionAnonymousInnerClass64 : Instruction
	{
		public InstructionAnonymousInnerClass64() : base(61)
		{
		}


	public override sealed string name()
	{
		return "MULT";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doMULT(rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareHiloForStore();
		if (context.RsRegister0 || context.RtRegister0)
		{
			context.MethodVisitor.visitLdcInsn(0L);
		}
		else
		{
			context.loadRs();
			context.MethodVisitor.visitInsn(Opcodes.I2L);
			context.loadRt();
			context.MethodVisitor.visitInsn(Opcodes.I2L);
			context.MethodVisitor.visitInsn(Opcodes.LMUL);
		}
		context.storeHilo();
	}
	public override string disasm(int address, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSRT("mult", rs, rt);
	}
	}
	public static readonly Instruction MULTU = new InstructionAnonymousInnerClass65();

	private class InstructionAnonymousInnerClass65 : Instruction
	{
		public InstructionAnonymousInnerClass65() : base(62)
		{
		}


	public override sealed string name()
	{
		return "MULTU";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doMULTU(rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareHiloForStore();
		if (context.RsRegister0 || context.RtRegister0)
		{
			context.MethodVisitor.visitLdcInsn(0L);
		}
		else
		{
			context.loadRs();
			context.convertUnsignedIntToLong();
			context.loadRt();
			context.convertUnsignedIntToLong();
			context.MethodVisitor.visitInsn(Opcodes.LMUL);
		}
		context.storeHilo();
	}
	public override string disasm(int address, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSRT("multu", rs, rt);
	}
	}
	public static readonly Instruction MADD = new InstructionAnonymousInnerClass66();

	private class InstructionAnonymousInnerClass66 : Instruction
	{
		public InstructionAnonymousInnerClass66() : base(63)
		{
		}


	public override sealed string name()
	{
		return "MADD";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doMADD(rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RsRegister0 && !context.RtRegister0)
		{
			context.prepareHiloForStore();
			context.loadHilo();
			context.loadRs();
			context.MethodVisitor.visitInsn(Opcodes.I2L);
			context.loadRt();
			context.MethodVisitor.visitInsn(Opcodes.I2L);
			context.MethodVisitor.visitInsn(Opcodes.LMUL);
			context.MethodVisitor.visitInsn(Opcodes.LADD);
			context.storeHilo();
		}
	}
	public override string disasm(int address, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSRT("madd", rs, rt);
	}
	}
	public static readonly Instruction MADDU = new InstructionAnonymousInnerClass67();

	private class InstructionAnonymousInnerClass67 : Instruction
	{
		public InstructionAnonymousInnerClass67() : base(64)
		{
		}


	public override sealed string name()
	{
		return "MADDU";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doMADDU(rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RsRegister0 && !context.RtRegister0)
		{
			context.prepareHiloForStore();
			context.loadHilo();
			context.loadRs();
			context.convertUnsignedIntToLong();
			context.loadRt();
			context.convertUnsignedIntToLong();
			context.MethodVisitor.visitInsn(Opcodes.LMUL);
			context.MethodVisitor.visitInsn(Opcodes.LADD);
			context.storeHilo();
		}
	}
	public override string disasm(int address, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSRT("maddu", rs, rt);
	}
	}
	public static readonly Instruction MSUB = new InstructionAnonymousInnerClass68();

	private class InstructionAnonymousInnerClass68 : Instruction
	{
		public InstructionAnonymousInnerClass68() : base(65)
		{
		}


	public override sealed string name()
	{
		return "MSUB";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doMSUB(rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RsRegister0 && !context.RtRegister0)
		{
			context.prepareHiloForStore();
			context.loadHilo();
			context.loadRs();
			context.MethodVisitor.visitInsn(Opcodes.I2L);
			context.loadRt();
			context.MethodVisitor.visitInsn(Opcodes.I2L);
			context.MethodVisitor.visitInsn(Opcodes.LMUL);
			context.MethodVisitor.visitInsn(Opcodes.LSUB);
			context.storeHilo();
		}
	}
	public override string disasm(int address, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSRT("msub", rs, rt);
	}
	}
	public static readonly Instruction MSUBU = new InstructionAnonymousInnerClass69();

	private class InstructionAnonymousInnerClass69 : Instruction
	{
		public InstructionAnonymousInnerClass69() : base(66)
		{
		}


	public override sealed string name()
	{
		return "MSUBU";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doMSUBU(rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RsRegister0 && !context.RtRegister0)
		{
			context.prepareHiloForStore();
			context.loadHilo();
			context.loadRs();
			context.convertUnsignedIntToLong();
			context.loadRt();
			context.convertUnsignedIntToLong();
			context.MethodVisitor.visitInsn(Opcodes.LMUL);
			context.MethodVisitor.visitInsn(Opcodes.LSUB);
			context.storeHilo();
		}
	}
	public override string disasm(int address, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSRT("msubu", rs, rt);
	}
	}
	public static readonly Instruction DIV = new InstructionAnonymousInnerClass70();

	private class InstructionAnonymousInnerClass70 : Instruction
	{
		public InstructionAnonymousInnerClass70() : base(67)
		{
		}


	public override sealed string name()
	{
		return "DIV";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doDIV(rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		Label divideByZero = new Label();
		Label afterInstruction = new Label();
		context.loadRt();
		context.MethodVisitor.visitJumpInsn(Opcodes.IFEQ, divideByZero);

		context.loadRs();
		context.loadRt();
		context.MethodVisitor.visitInsn(Opcodes.DUP2);
		context.MethodVisitor.visitInsn(Opcodes.IREM);
		context.MethodVisitor.visitInsn(Opcodes.I2L);
		context.loadImm(32);
		context.MethodVisitor.visitInsn(Opcodes.LSHL);
		context.MethodVisitor.visitInsn(Opcodes.DUP2_X2);
		context.MethodVisitor.visitInsn(Opcodes.POP2);
		context.MethodVisitor.visitInsn(Opcodes.IDIV);
		context.MethodVisitor.visitInsn(Opcodes.I2L);
		context.MethodVisitor.visitLdcInsn(0x00000000FFFFFFFFL);
		context.MethodVisitor.visitInsn(Opcodes.LAND);
		context.MethodVisitor.visitInsn(Opcodes.LOR);
		context.storeHilo();
		context.MethodVisitor.visitJumpInsn(Opcodes.GOTO, afterInstruction);

		// Division by zero handled by the interpreted instruction
		context.MethodVisitor.visitLabel(divideByZero);
		context.compileInterpreterInstruction();

		context.MethodVisitor.visitLabel(afterInstruction);
	}
	public override string disasm(int address, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSRT("div", rs, rt);
	}
	}
	public static readonly Instruction DIVU = new InstructionAnonymousInnerClass71();

	private class InstructionAnonymousInnerClass71 : Instruction
	{
		public InstructionAnonymousInnerClass71() : base(68)
		{
		}


	public override sealed string name()
	{
		return "DIVU";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doDIVU(rs, rt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		Label divideByZero = new Label();
		Label afterInstruction = new Label();
		context.loadRt();
		context.MethodVisitor.visitJumpInsn(Opcodes.IFEQ, divideByZero);

		context.loadRs();
		context.MethodVisitor.visitInsn(Opcodes.I2L);
		context.MethodVisitor.visitLdcInsn(0x00000000FFFFFFFFL);
		context.MethodVisitor.visitInsn(Opcodes.LAND);
		context.MethodVisitor.visitInsn(Opcodes.DUP2);
		context.loadRt();
		context.MethodVisitor.visitInsn(Opcodes.I2L);
		context.MethodVisitor.visitLdcInsn(0x00000000FFFFFFFFL);
		context.MethodVisitor.visitInsn(Opcodes.LAND);
		context.MethodVisitor.visitInsn(Opcodes.DUP2_X2);
		context.MethodVisitor.visitInsn(Opcodes.LREM);
		context.loadImm(32);
		context.MethodVisitor.visitInsn(Opcodes.LSHL);
		context.storeLTmp1();
		context.MethodVisitor.visitInsn(Opcodes.LDIV);
		context.MethodVisitor.visitLdcInsn(0x00000000FFFFFFFFL);
		context.MethodVisitor.visitInsn(Opcodes.LAND);
		context.loadLTmp1();
		context.MethodVisitor.visitInsn(Opcodes.LOR);
		context.storeHilo();
		context.MethodVisitor.visitJumpInsn(Opcodes.GOTO, afterInstruction);

		// Division by zero handled by the interpreted instruction
		context.MethodVisitor.visitLabel(divideByZero);
		context.compileInterpreterInstruction();

		context.MethodVisitor.visitLabel(afterInstruction);
	}
	public override string disasm(int address, int insn)
	{
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSRT("divu", rs, rt);
	}
	}
	public static readonly Instruction MFHI = new InstructionAnonymousInnerClass72(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass72 : Instruction
	{
		public InstructionAnonymousInnerClass72(UnknownType FLAG_WRITES_RD) : base(69, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "MFHI";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;


					processor.cpu.doMFHI(rd);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			context.prepareRdForStore();
			context.loadHilo();
			context.loadImm(32);
			context.MethodVisitor.visitInsn(Opcodes.LUSHR);
			context.MethodVisitor.visitInsn(Opcodes.L2I);
			context.storeRd();
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;

	return Common.disasmRD("mfhi", rd);
	}
	}
	public static readonly Instruction MFLO = new InstructionAnonymousInnerClass73(FLAG_WRITES_RD);

	private class InstructionAnonymousInnerClass73 : Instruction
	{
		public InstructionAnonymousInnerClass73(UnknownType FLAG_WRITES_RD) : base(70, FLAG_WRITES_RD)
		{
		}


	public override sealed string name()
	{
		return "MFLO";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;


					processor.cpu.doMFLO(rd);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RdRegister0)
		{
			context.prepareRdForStore();
			context.loadHilo();
			context.MethodVisitor.visitLdcInsn(0xFFFFFFFFL);
			context.MethodVisitor.visitInsn(Opcodes.LAND);
			context.MethodVisitor.visitInsn(Opcodes.L2I);
			context.storeRd();
		}
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;

	return Common.disasmRD("mflo", rd);
	}
	}
	public static readonly Instruction MTHI = new InstructionAnonymousInnerClass74();

	private class InstructionAnonymousInnerClass74 : Instruction
	{
		public InstructionAnonymousInnerClass74() : base(71)
		{
		}


	public override sealed string name()
	{
		return "MTHI";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rs = (insn >> 21) & 31;


					processor.cpu.doMTHI(rs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int rs = (insn >> 21) & 31;

	return Common.disasmRS("mthi", rs);
	}
	}
	public static readonly Instruction MTLO = new InstructionAnonymousInnerClass75();

	private class InstructionAnonymousInnerClass75 : Instruction
	{
		public InstructionAnonymousInnerClass75() : base(72)
		{
		}


	public override sealed string name()
	{
		return "MTLO";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rs = (insn >> 21) & 31;


					processor.cpu.doMTLO(rs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.loadHilo();
		context.MethodVisitor.visitLdcInsn(0xFFFFFFFF00000000L);
		context.MethodVisitor.visitInsn(Opcodes.LAND);
		if (!context.RsRegister0)
		{
			context.loadRs();
			context.MethodVisitor.visitInsn(Opcodes.I2L);
			context.MethodVisitor.visitLdcInsn(0x00000000FFFFFFFFL);
			context.MethodVisitor.visitInsn(Opcodes.LAND);
			context.MethodVisitor.visitInsn(Opcodes.LOR);
		}
		context.storeHilo();
	}
	public override string disasm(int address, int insn)
	{
		int rs = (insn >> 21) & 31;

	return Common.disasmRS("mtlo", rs);
	}
	}

	//
	// BEQ has the flag "FLAG_ENDS_BLOCK" because it can end a block when the
	// conditional branch can be reduced to an unconditional branch (rt == rs).
	//    "BEQ $xx, $xx, target"
	// is equivalent to
	//    "B target"
	// This special case is recognized in the method Compiler.analyse().
	//
	public static readonly Instruction BEQ = new InstructionAnonymousInnerClass76(FLAGS_BRANCH_INSTRUCTION | FLAG_ENDS_BLOCK);

	private class InstructionAnonymousInnerClass76 : Instruction
	{
		public InstructionAnonymousInnerClass76(int FLAGS_BRANCH_INSTRUCTION) : base(73, FLAGS_BRANCH_INSTRUCTION | FLAG_ENDS_BLOCK)
		{
		}


	public override sealed string name()
	{
		return "BEQ";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBEQ(rs, rt, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSRTOFFSET("beq", rs, rt, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BEQL = new InstructionAnonymousInnerClass77(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass77 : Instruction
	{
		public InstructionAnonymousInnerClass77(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(74, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BEQL";
	}
	public override sealed string category()
	{
		return "MIPS II";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBEQL(rs, rt, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSRTOFFSET("beql", rs, rt, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BGEZ = new InstructionAnonymousInnerClass78(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass78 : Instruction
	{
		public InstructionAnonymousInnerClass78(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(75, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BGEZ";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBGEZ(rs, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSOFFSET("bgez", rs, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BGEZAL = new InstructionAnonymousInnerClass79(FLAGS_LINK_INSTRUCTION | FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass79 : Instruction
	{
		public InstructionAnonymousInnerClass79(int FLAGS_LINK_INSTRUCTION) : base(76, FLAGS_LINK_INSTRUCTION | FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BGEZAL";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBGEZAL(rs, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSOFFSET("bgezal", rs, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BGEZALL = new InstructionAnonymousInnerClass80(FLAGS_LINK_INSTRUCTION | FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass80 : Instruction
	{
		public InstructionAnonymousInnerClass80(int FLAGS_LINK_INSTRUCTION) : base(77, FLAGS_LINK_INSTRUCTION | FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BGEZALL";
	}
	public override sealed string category()
	{
		return "MIPS II";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBGEZALL(rs, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSOFFSET("bgezall", rs, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BGEZL = new InstructionAnonymousInnerClass81(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass81 : Instruction
	{
		public InstructionAnonymousInnerClass81(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(78, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BGEZL";
	}
	public override sealed string category()
	{
		return "MIPS II";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBGEZL(rs, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSOFFSET("bgezl", rs, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BGTZ = new InstructionAnonymousInnerClass82(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass82 : Instruction
	{
		public InstructionAnonymousInnerClass82(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(79, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BGTZ";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBGTZ(rs, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSOFFSET("bgtz", rs, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BGTZL = new InstructionAnonymousInnerClass83(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass83 : Instruction
	{
		public InstructionAnonymousInnerClass83(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(80, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BGTZL";
	}
	public override sealed string category()
	{
		return "MIPS II";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBGTZL(rs, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSOFFSET("bgtzl", rs, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BLEZ = new InstructionAnonymousInnerClass84(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass84 : Instruction
	{
		public InstructionAnonymousInnerClass84(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(81, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BLEZ";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBLEZ(rs, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSOFFSET("blez", rs, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BLEZL = new InstructionAnonymousInnerClass85(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass85 : Instruction
	{
		public InstructionAnonymousInnerClass85(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(82, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BLEZL";
	}
	public override sealed string category()
	{
		return "MIPS II";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBLEZL(rs, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSOFFSET("blezl", rs, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BLTZ = new InstructionAnonymousInnerClass86(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass86 : Instruction
	{
		public InstructionAnonymousInnerClass86(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(83, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BLTZ";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBLTZ(rs, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSOFFSET("bltz", rs, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BLTZAL = new InstructionAnonymousInnerClass87(FLAGS_LINK_INSTRUCTION | FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass87 : Instruction
	{
		public InstructionAnonymousInnerClass87(int FLAGS_LINK_INSTRUCTION) : base(84, FLAGS_LINK_INSTRUCTION | FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BLTZAL";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBLTZAL(rs, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSOFFSET("bltzal", rs, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BLTZALL = new InstructionAnonymousInnerClass88(FLAGS_LINK_INSTRUCTION | FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass88 : Instruction
	{
		public InstructionAnonymousInnerClass88(int FLAGS_LINK_INSTRUCTION) : base(85, FLAGS_LINK_INSTRUCTION | FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BLTZALL";
	}
	public override sealed string category()
	{
		return "MIPS II";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBLTZALL(rs, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSOFFSET("bltzall", rs, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BLTZL = new InstructionAnonymousInnerClass89(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass89 : Instruction
	{
		public InstructionAnonymousInnerClass89(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(86, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BLTZL";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBLTZL(rs, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSOFFSET("bltzl", rs, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BNE = new InstructionAnonymousInnerClass90(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass90 : Instruction
	{
		public InstructionAnonymousInnerClass90(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(87, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BNE";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBNE(rs, rt, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSRTOFFSET("bne", rs, rt, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BNEL = new InstructionAnonymousInnerClass91(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass91 : Instruction
	{
		public InstructionAnonymousInnerClass91(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(88, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BNEL";
	}
	public override sealed string category()
	{
		return "MIPS II";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doBNEL(rs, rt, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRSRTOFFSET("bnel", rs, rt, (int)(short)imm16, address);
	}
	}
	public static readonly Instruction J = new InstructionAnonymousInnerClass92(FLAG_HAS_DELAY_SLOT | FLAG_IS_JUMPING | FLAG_CANNOT_BE_SPLIT | FLAG_ENDS_BLOCK);

	private class InstructionAnonymousInnerClass92 : Instruction
	{
		public InstructionAnonymousInnerClass92(int FLAG_HAS_DELAY_SLOT) : base(89, FLAG_HAS_DELAY_SLOT | FLAG_IS_JUMPING | FLAG_CANNOT_BE_SPLIT | FLAG_ENDS_BLOCK)
		{
		}


	public override sealed string name()
	{
		return "J";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm26 = (insn >> 0) & 67108863;


					if (processor.cpu.doJ(imm26))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm26 = (insn >> 0) & 67108863;

	return Common.disasmJUMP("j", imm26, address);
	}
	}
	public static readonly Instruction JAL = new InstructionAnonymousInnerClass93(FLAGS_LINK_INSTRUCTION | FLAG_IS_JUMPING);

	private class InstructionAnonymousInnerClass93 : Instruction
	{
		public InstructionAnonymousInnerClass93(int FLAGS_LINK_INSTRUCTION) : base(90, FLAGS_LINK_INSTRUCTION | FLAG_IS_JUMPING)
		{
		}


	public override sealed string name()
	{
		return "JAL";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm26 = (insn >> 0) & 67108863;


					if (processor.cpu.doJAL(imm26))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm26 = (insn >> 0) & 67108863;

	return Common.disasmJUMP("jal", imm26, address);
	}
	}
	public static readonly Instruction JALR = new InstructionAnonymousInnerClass94(FLAG_HAS_DELAY_SLOT);

	private class InstructionAnonymousInnerClass94 : Instruction
	{
		public InstructionAnonymousInnerClass94(UnknownType FLAG_HAS_DELAY_SLOT) : base(91, FLAG_HAS_DELAY_SLOT)
		{
		}


	public override sealed string name()
	{
		return "JALR";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doJALR(rd, rs))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int rd = (insn >> 11) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRDRS("jalr", rd, rs);
	}
	}
	public static readonly Instruction JR = new InstructionAnonymousInnerClass95(FLAG_HAS_DELAY_SLOT | FLAG_CANNOT_BE_SPLIT | FLAG_ENDS_BLOCK);

	private class InstructionAnonymousInnerClass95 : Instruction
	{
		public InstructionAnonymousInnerClass95(int FLAG_HAS_DELAY_SLOT) : base(92, FLAG_HAS_DELAY_SLOT | FLAG_CANNOT_BE_SPLIT | FLAG_ENDS_BLOCK)
		{
		}


	public override sealed string name()
	{
		return "JR";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int rs = (insn >> 21) & 31;


					if (processor.cpu.doJR(rs))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int rs = (insn >> 21) & 31;

	return Common.disasmRS("jr", rs);
	}
	}
	public static readonly Instruction BC1F = new InstructionAnonymousInnerClass96(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass96 : Instruction
	{
		public InstructionAnonymousInnerClass96(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(93, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BC1F";
	}
	public override sealed string category()
	{
		return "MIPS I/FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;


					if (processor.cpu.doBC1F((int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;

	return Common.disasmOFFSET("bc1f", (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BC1T = new InstructionAnonymousInnerClass97(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass97 : Instruction
	{
		public InstructionAnonymousInnerClass97(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(94, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BC1T";
	}
	public override sealed string category()
	{
		return "MIPS I/FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;


					if (processor.cpu.doBC1T((int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;

	return Common.disasmOFFSET("bc1t", (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BC1FL = new InstructionAnonymousInnerClass98(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass98 : Instruction
	{
		public InstructionAnonymousInnerClass98(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(95, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BC1FL";
	}
	public override sealed string category()
	{
		return "MIPS II/FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;


					if (processor.cpu.doBC1FL((int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;

	return Common.disasmOFFSET("bc1fl", (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BC1TL = new InstructionAnonymousInnerClass99(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass99 : Instruction
	{
		public InstructionAnonymousInnerClass99(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(96, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BC1TL";
	}
	public override sealed string category()
	{
		return "MIPS II/FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;


					if (processor.cpu.doBC1TL((int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;

	return Common.disasmOFFSET("bc1tl", (int)(short)imm16, address);
	}
	}
	public static readonly Instruction BVF = new InstructionAnonymousInnerClass100(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass100 : Instruction
	{
		public InstructionAnonymousInnerClass100(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(97, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BVF";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int imm3 = (insn >> 18) & 7;


					if (processor.cpu.doBVF(imm3, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int imm3 = (insn >> 18) & 7;

		return Common.disasmVCCOFFSET("bvf", imm3, imm16, address);
	}
	}
	public static readonly Instruction BVT = new InstructionAnonymousInnerClass101(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass101 : Instruction
	{
		public InstructionAnonymousInnerClass101(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(98, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BVT";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int imm3 = (insn >> 18) & 7;


					if (processor.cpu.doBVT(imm3, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int imm3 = (insn >> 18) & 7;

		return Common.disasmVCCOFFSET("bvt", imm3, imm16, address);
	}
	}
	public static readonly Instruction BVFL = new InstructionAnonymousInnerClass102(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass102 : Instruction
	{
		public InstructionAnonymousInnerClass102(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(99, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BVFL";
	}
	public override sealed string category()
	{
		return "MIPS II/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int imm3 = (insn >> 18) & 7;


					if (processor.cpu.doBVFL(imm3, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int imm3 = (insn >> 18) & 7;

		return Common.disasmVCCOFFSET("bvfl", imm3, imm16, address);
	}
	}
	public static readonly Instruction BVTL = new InstructionAnonymousInnerClass103(FLAGS_BRANCH_INSTRUCTION);

	private class InstructionAnonymousInnerClass103 : Instruction
	{
		public InstructionAnonymousInnerClass103(UnknownType FLAGS_BRANCH_INSTRUCTION) : base(100, FLAGS_BRANCH_INSTRUCTION)
		{
		}


	public override sealed string name()
	{
		return "BVTL";
	}
	public override sealed string category()
	{
		return "MIPS II/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int imm3 = (insn >> 18) & 7;


					if (processor.cpu.doBVTL(imm3, (int)(short)imm16))
					{
						processor.interpretDelayslot();
					}

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int imm3 = (insn >> 18) & 7;

		return Common.disasmVCCOFFSET("bvtl", imm3, imm16, address);
	}
	}
	public static readonly Instruction LB = new InstructionAnonymousInnerClass104(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass104 : Instruction
	{
		public InstructionAnonymousInnerClass104(UnknownType FLAG_WRITES_RT) : base(101, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "LB";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doLB(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			context.prepareRtForStore();
			context.memRead8(context.RsRegisterIndex, context.getImm16(true));
			context.MethodVisitor.visitInsn(Opcodes.I2B);
			context.storeRt();
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("lb", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction LBU = new InstructionAnonymousInnerClass105(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass105 : Instruction
	{
		public InstructionAnonymousInnerClass105(UnknownType FLAG_WRITES_RT) : base(102, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "LBU";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doLBU(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			context.prepareRtForStore();
			context.memRead8(context.RsRegisterIndex, context.getImm16(true));
			context.storeRt();
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("lbu", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction LH = new InstructionAnonymousInnerClass106(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass106 : Instruction
	{
		public InstructionAnonymousInnerClass106(UnknownType FLAG_WRITES_RT) : base(103, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "LH";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doLH(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			context.prepareRtForStore();
			context.memRead16(context.RsRegisterIndex, context.getImm16(true));
			context.MethodVisitor.visitInsn(Opcodes.I2S);
			context.storeRt();
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("lh", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction LHU = new InstructionAnonymousInnerClass107(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass107 : Instruction
	{
		public InstructionAnonymousInnerClass107(UnknownType FLAG_WRITES_RT) : base(104, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "LHU";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doLHU(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			context.prepareRtForStore();
			context.memRead16(context.RsRegisterIndex, context.getImm16(true));
			context.storeRt();
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("lhu", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction LW = new InstructionAnonymousInnerClass108(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass108 : Instruction
	{
		public InstructionAnonymousInnerClass108(UnknownType FLAG_WRITES_RT) : base(105, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "LW";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doLW(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			int rs = context.RsRegisterIndex;
			int simm16 = context.getImm16(true);

			int countSequence = 1;
			int[] offsets = null;
			int[] registers = null;

			if (!context.CodeInstruction.DelaySlot && context.RtRegisterIndex != rs)
			{
				int address = context.CodeInstruction.Address + 4;
				// Compare LW opcode and rs register
				const int opcodeMask = unchecked((int)0xFFE00000);
				for (int i = 1; true; i++, address += 4)
				{
					CodeInstruction nextCodeInstruction = context.getCodeInstruction(address);
					bool isSequence = false;
					if (nextCodeInstruction != null && !nextCodeInstruction.BranchTarget)
					{
						if ((nextCodeInstruction.Opcode & opcodeMask) == (insn & opcodeMask))
						{
							if (nextCodeInstruction.RtRegisterIndex != rs)
							{
								if (offsets == null)
								{
									offsets = new int[2];
									registers = new int[2];
									offsets[0] = simm16;
									registers[0] = context.RtRegisterIndex;
								}
								else
								{
									offsets = Utilities.extendArray(offsets, 1);
									registers = Utilities.extendArray(registers, 1);
								}
								offsets[i] = nextCodeInstruction.getImm16(true);
								registers[i] = nextCodeInstruction.RtRegisterIndex;
								isSequence = true;
							}
						}
					}

					if (!isSequence)
					{
						break;
					}
					countSequence++;
				}
			}

			if (countSequence > 1 && context.compileLWsequence(rs, offsets, registers))
			{
				if (countSequence > 1)
				{
					if (Compiler.log.DebugEnabled)
					{
						CodeInstruction sequence = new SequenceLWCodeInstruction(rs, offsets, registers);
						sequence.Address = context.CodeInstruction.Address;
						Compiler.Console.WriteLine(sequence);
					}

					// Skip the next lw instructions
					context.skipInstructions(countSequence - 1, false);
				}
			}
			else
			{
				context.prepareRtForStore();
				context.memRead32(rs, simm16);
				context.storeRt();
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("lw", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction LWL = new InstructionAnonymousInnerClass109(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass109 : Instruction
	{
		public InstructionAnonymousInnerClass109(UnknownType FLAG_WRITES_RT) : base(106, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "LWL";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doLWL(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileRTRSIMM("doLWL", true);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("lwl", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction LWR = new InstructionAnonymousInnerClass110(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass110 : Instruction
	{
		public InstructionAnonymousInnerClass110(UnknownType FLAG_WRITES_RT) : base(107, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "LWR";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doLWR(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileRTRSIMM("doLWR", true);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("lwr", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction SB = new InstructionAnonymousInnerClass111();

	private class InstructionAnonymousInnerClass111 : Instruction
	{
		public InstructionAnonymousInnerClass111() : base(108)
		{
		}


	public override sealed string name()
	{
		return "SB";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSB(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int rs = context.RsRegisterIndex;
		int simm16 = context.getImm16(true);
		if (context.RtRegister0)
		{
			context.memWriteZero8(rs, simm16);
		}
		else
		{
			context.prepareMemWrite8(rs, simm16);
			context.loadRt();
			context.memWrite8(rs, simm16);
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("sb", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction SH = new InstructionAnonymousInnerClass112();

	private class InstructionAnonymousInnerClass112 : Instruction
	{
		public InstructionAnonymousInnerClass112() : base(109)
		{
		}


	public override sealed string name()
	{
		return "SH";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSH(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int rs = context.RsRegisterIndex;
		int simm16 = context.getImm16(true);
		context.prepareMemWrite16(rs, simm16);
		context.loadRt();
		context.memWrite16(rs, simm16);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("sh", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction SW = new InstructionAnonymousInnerClass113();

	private class InstructionAnonymousInnerClass113 : Instruction
	{
		public InstructionAnonymousInnerClass113() : base(110)
		{
		}


	public override sealed string name()
	{
		return "SW";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSW(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int rs = context.RsRegisterIndex;
		int simm16 = context.getImm16(true);

		int countSequence = 1;
		int[] offsets = null;
		int[] registers = null;

		if (!context.CodeInstruction.DelaySlot)
		{
			int address = context.CodeInstruction.Address + 4;
			// Compare SW opcode and rs register
			const int opcodeMask = unchecked((int)0xFFE00000);
			for (int i = 1; true; i++, address += 4)
			{
				CodeInstruction nextCodeInstruction = context.getCodeInstruction(address);
				bool isSequence = false;
				if (nextCodeInstruction != null && !nextCodeInstruction.BranchTarget)
				{
					if ((nextCodeInstruction.Opcode & opcodeMask) == (insn & opcodeMask))
					{
						if (offsets == null)
						{
							offsets = new int[2];
							registers = new int[2];
							offsets[0] = simm16;
							registers[0] = context.RtRegisterIndex;
						}
						else
						{
							offsets = Utilities.extendArray(offsets, 1);
							registers = Utilities.extendArray(registers, 1);
						}
						offsets[i] = nextCodeInstruction.getImm16(true);
						registers[i] = nextCodeInstruction.RtRegisterIndex;
						isSequence = true;
					}
				}

				if (!isSequence)
				{
					break;
				}
				countSequence++;
			}
		}

		if (countSequence > 1 && context.compileSWsequence(rs, offsets, registers))
		{
			if (countSequence > 1)
			{
				if (Compiler.log.DebugEnabled)
				{
					CodeInstruction sequence = new SequenceSWCodeInstruction(rs, offsets, registers);
					sequence.Address = context.CodeInstruction.Address;
					Compiler.Console.WriteLine(sequence);
				}

				// Skip the next sw instructions
				context.skipInstructions(countSequence - 1, false);
			}
		}
		else
		{
			context.prepareMemWrite32(rs, simm16);
			context.loadRt();
			context.memWrite32(rs, simm16);
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("sw", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction SWL = new InstructionAnonymousInnerClass114();

	private class InstructionAnonymousInnerClass114 : Instruction
	{
		public InstructionAnonymousInnerClass114() : base(111)
		{
		}


	public override sealed string name()
	{
		return "SWL";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSWL(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileRTRSIMM("doSWL", true);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("swl", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction SWR = new InstructionAnonymousInnerClass115();

	private class InstructionAnonymousInnerClass115 : Instruction
	{
		public InstructionAnonymousInnerClass115() : base(112)
		{
		}


	public override sealed string name()
	{
		return "SWR";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSWR(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileRTRSIMM("doSWR", true);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("swr", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction LL = new InstructionAnonymousInnerClass116();

	private class InstructionAnonymousInnerClass116 : Instruction
	{
		public InstructionAnonymousInnerClass116() : base(113)
		{
		}


	public override sealed string name()
	{
		return "LL";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doLL(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileRTRSIMM("doLL", true);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("ll", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction LWC1 = new InstructionAnonymousInnerClass117();

	private class InstructionAnonymousInnerClass117 : Instruction
	{
		public InstructionAnonymousInnerClass117() : base(114)
		{
		}


	public override sealed string name()
	{
		return "LWC1";
	}
	public override sealed string category()
	{
		return "MIPS I/FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int ft = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doLWC1(ft, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFtForStore();
		context.memRead32(context.RsRegisterIndex, context.getImm16(true));
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "intBitsToFloat", "(I)F");
		context.storeFt();
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int ft = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmFTIMMRS("lwc1", ft, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction LVS = new InstructionAnonymousInnerClass118();

	private class InstructionAnonymousInnerClass118 : Instruction
	{
		public InstructionAnonymousInnerClass118() : base(115)
		{
		}


	public override sealed string name()
	{
		return "LVS";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vt2 = (insn >> 0) & 3;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doLVS((vt5 | (vt2 << 5)), rs, (int)(short)(imm14 << 2));

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vt2 = (insn >> 0) & 3;
		int vt5 = (insn >> 16) & 31;
		int vt = vt5 | (vt2 << 5);
		int simm14 = context.getImm14(true);
		int rs = context.RsRegisterIndex;

		context.prepareVtForStoreInt(1, vt, 0);
		context.memRead32(rs, simm14);
		context.storeVtInt(1, vt, 0);
	}
	public override string disasm(int address, int insn)
	{
		int vt2 = (insn >> 0) & 3;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmVTIMMRS("lv", 1, (vt5 | (vt2 << 5)), rs, (int)(short)(imm14 << 2));
	}
	}
	public static readonly Instruction LVLQ = new InstructionAnonymousInnerClass119();

	private class InstructionAnonymousInnerClass119 : Instruction
	{
		public InstructionAnonymousInnerClass119() : base(116)
		{
		}


	public override sealed string name()
	{
		return "LVLQ";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doLVLQ((vt5 | (vt1 << 5)), rs, (int)(short)(imm14 << 2));

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmVTIMMRS("lvl", 4, (vt5 | (vt1 << 5)), rs, (int)(short)(imm14 << 2));
	}
	}
	public static readonly Instruction LVRQ = new InstructionAnonymousInnerClass120();

	private class InstructionAnonymousInnerClass120 : Instruction
	{
		public InstructionAnonymousInnerClass120() : base(117)
		{
		}


	public override sealed string name()
	{
		return "LVRQ";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doLVRQ((vt5 | (vt1 << 5)), rs, (int)(short)(imm14 << 2));

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmVTIMMRS("lvr", 4, (vt5 | (vt1 << 5)), rs, (int)(short)(imm14 << 2));
	}
	}
	public static readonly Instruction LVQ = new InstructionAnonymousInnerClass121();

	private class InstructionAnonymousInnerClass121 : Instruction
	{
		public InstructionAnonymousInnerClass121() : base(118)
		{
		}


	public override sealed string name()
	{
		return "LVQ";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doLVQ((vt5 | (vt1 << 5)), rs, (int)(short)(imm14 << 2));

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int vt5 = (insn >> 16) & 31;
		int vt = vt5 | (vt1 << 5);
		int simm14 = context.getImm14(true);
		int rs = context.RsRegisterIndex;
		const int vsize = 4;

		int countSequence = 1;
		int address = context.CodeInstruction.Address;

		// Compare LV.Q opcode and vt1 flag
		const int opcodeMask = unchecked((int)0xFFE00003);
		for (int i = 1; true; i++)
		{
			CodeInstruction nextCodeInstruction = context.getCodeInstruction(address + i * 4);
			bool isSequence = false;
			if (nextCodeInstruction != null)
			{
				int nextInsn = nextCodeInstruction.Opcode;
				if (nextCodeInstruction != null && (nextInsn & opcodeMask) == (insn & opcodeMask))
				{
					int nextSimm14 = nextCodeInstruction.getImm14(true);
					if (nextSimm14 == simm14 + i * 16)
					{
						int nextVt5 = (nextInsn >> 16) & 31;
						if (nextVt5 == vt5 + i)
						{
							isSequence = true;
						}
					}
				}
			}

			if (!isSequence)
			{
				break;
			}
			countSequence++;
		}

		if (context.compileVFPULoad(context.RsRegisterIndex, simm14, vt, countSequence * 4))
		{
			if (countSequence > 1)
			{
				if (Compiler.log.DebugEnabled)
				{
					Compiler.Console.WriteLine(string.Format("lv.q sequence 0x{0:X8}-0x{1:X8}", address, address + countSequence * 4 - 4));
				}

				// Skip the next lv.q instructions
				context.skipInstructions(countSequence - 1, false);
			}
		}
		else
		{
			for (int n = 0; n < vsize; n++)
			{
				context.prepareVtForStoreInt(vsize, vt, n);
				context.memRead32(rs, simm14 + n * 4);
				context.storeVtInt(vsize, vt, n);
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmVTIMMRS("lv", 4, (vt5 | (vt1 << 5)), rs, (int)(short)(imm14 << 2));
	}
	}
	public static readonly Instruction SC = new InstructionAnonymousInnerClass122();

	private class InstructionAnonymousInnerClass122 : Instruction
	{
		public InstructionAnonymousInnerClass122() : base(119)
		{
		}


	public override sealed string name()
	{
		return "SC";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSC(rt, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int rs = context.RsRegisterIndex;
		int simm16 = context.getImm16(true);
		context.prepareMemWrite32(rs, simm16);
		context.loadRt();
		context.memWrite32(rs, simm16);

		if (!context.RtRegister0)
		{
			context.storeRt(1);
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmRTIMMRS("sc", rt, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction SWC1 = new InstructionAnonymousInnerClass123();

	private class InstructionAnonymousInnerClass123 : Instruction
	{
		public InstructionAnonymousInnerClass123() : base(120)
		{
		}


	public override sealed string name()
	{
		return "SWC1";
	}
	public override sealed string category()
	{
		return "MIPS I/FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int ft = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSWC1(ft, rs, (int)(short)imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int rs = context.RsRegisterIndex;
		int simm16 = context.getImm16(true);
		context.prepareMemWrite32(rs, simm16);
		context.loadFt();
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "floatToRawIntBits", "(F)I");
		context.memWrite32(rs, simm16);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int ft = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmFTIMMRS("swc1", ft, rs, (int)(short)imm16);
	}
	}
	public static readonly Instruction SVS = new InstructionAnonymousInnerClass124();

	private class InstructionAnonymousInnerClass124 : Instruction
	{
		public InstructionAnonymousInnerClass124() : base(121)
		{
		}


	public override sealed string name()
	{
		return "SVS";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vt2 = (insn >> 0) & 3;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSVS((vt5 | (vt2 << 5)), rs, (int)(short)(imm14 << 2));

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vt2 = (insn >> 0) & 3;
		int vt5 = (insn >> 16) & 31;
		int vt = vt5 | (vt2 << 5);
		int simm14 = context.getImm14(true);
		int rs = context.RsRegisterIndex;
		context.prepareMemWrite32(rs, simm14);
		context.loadVtInt(1, vt, 0);
		context.memWrite32(rs, simm14);
	}
	public override string disasm(int address, int insn)
	{
		int vt2 = (insn >> 0) & 3;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmVTIMMRS("sv", 1, (vt5 | (vt2 << 5)), rs, (int)(short)(imm14 << 2));
	}
	}
	public static readonly Instruction SVLQ = new InstructionAnonymousInnerClass125();

	private class InstructionAnonymousInnerClass125 : Instruction
	{
		public InstructionAnonymousInnerClass125() : base(122)
		{
		}


	public override sealed string name()
	{
		return "SVLQ";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSVLQ((vt5 | (vt1 << 5)), rs, (int)(short)(imm14 << 2));

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmVTIMMRS("svl", 4, (vt5 | (vt1 << 5)), rs, (int)(short)(imm14 << 2));
	}
	}
	public static readonly Instruction SVRQ = new InstructionAnonymousInnerClass126();

	private class InstructionAnonymousInnerClass126 : Instruction
	{
		public InstructionAnonymousInnerClass126() : base(123)
		{
		}


	public override sealed string name()
	{
		return "SVRQ";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSVRQ((vt5 | (vt1 << 5)), rs, (int)(short)(imm14 << 2));

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmVTIMMRS("svr", 4, (vt5 | (vt1 << 5)), rs, (int)(short)(imm14 << 2));
	}
	}
	public static readonly Instruction SVQ = new InstructionAnonymousInnerClass127();

	private class InstructionAnonymousInnerClass127 : Instruction
	{
		public InstructionAnonymousInnerClass127() : base(124)
		{
		}


	public override sealed string name()
	{
		return "SVQ";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;


					processor.cpu.doSVQ((vt5 | (vt1 << 5)), rs, (int)(short)(imm14 << 2));

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int vt5 = (insn >> 16) & 31;
		int vt = vt5 | (vt1 << 5);
		int simm14 = context.getImm14(true);
		int rs = context.RsRegisterIndex;
		int vsize = 4;

		int countSequence = 1;
		int address = context.CodeInstruction.Address;

		// Compare SV.Q opcode and vt1 flag
		const int opcodeMask = unchecked((int)0xFFE00001);
		for (int i = 1; i < 4; i++)
		{
			CodeInstruction nextCodeInstruction = context.getCodeInstruction(address + i * 4);
			bool isSequence = false;
			if (nextCodeInstruction != null)
			{
				int nextInsn = nextCodeInstruction.Opcode;
				if (nextCodeInstruction != null && (nextInsn & opcodeMask) == (insn & opcodeMask))
				{
					int nextSimm14 = nextCodeInstruction.getImm14(true);
					if (nextSimm14 == simm14 + i * 16)
					{
						int nextVt5 = (nextInsn >> 16) & 31;
						if (nextVt5 == vt5 + i)
						{
							isSequence = true;
						}
					}
				}
			}

			if (!isSequence)
			{
				break;
			}
			countSequence++;
		}

		if (context.compileVFPUStore(context.RsRegisterIndex, simm14, vt, countSequence * 4))
		{
			if (countSequence > 1)
			{
				if (Compiler.log.DebugEnabled)
				{
					Compiler.Console.WriteLine(string.Format("   sv.q sequence 0x{0:X8}-0x{1:X8}", address, address + countSequence * 4 - 4));
				}

				// Skip the next sv.q instructions
				context.skipInstructions(countSequence - 1, false);
			}
		}
		else
		{
			for (int n = 0; n < vsize; n++)
			{
				context.prepareMemWrite32(rs, simm14 + n * 4);
				context.loadVtInt(vsize, vt, n);
				context.memWrite32(rs, simm14 + n * 4);
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmVTIMMRS("sv", 4, (vt5 | (vt1 << 5)), rs, (int)(short)(imm14 << 2));
	}
	}
	public static readonly Instruction VWB = new InstructionAnonymousInnerClass128();

	private class InstructionAnonymousInnerClass128 : Instruction
	{
		public InstructionAnonymousInnerClass128() : base(125)
		{
		}


	public override sealed string name()
	{
		return "VWB";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		// Checked using VfpuTest: VWB.Q is equivalent to SV.Q
		SVQ.interpret(processor, insn);
	}
	public override void compile(ICompilerContext context, int insn)
	{
		SVQ.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vt1 = (insn >> 0) & 1;
		int imm14 = (insn >> 2) & 16383;
		int vt5 = (insn >> 16) & 31;
		int rs = (insn >> 21) & 31;

	return Common.disasmVTIMMRS("vwb", 4, (vt5 | (vt1 << 5)), rs, (int)(short)(imm14 << 2));
	}
	}
	public static readonly Instruction ADD_S = new InstructionAnonymousInnerClass129();

	private class InstructionAnonymousInnerClass129 : Instruction
	{
		public InstructionAnonymousInnerClass129() : base(126)
		{
		}


	public override sealed string name()
	{
		return "ADD.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;
		int ft = (insn >> 16) & 31;


					processor.cpu.doADDS(fd, fs, ft);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFdForStore();
		context.loadFs();
		context.loadFt();
		context.MethodVisitor.visitInsn(Opcodes.FADD);
		context.storeFd();
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;
		int ft = (insn >> 16) & 31;

	return Common.disasmFDFSFT("add.s", fd, fs, ft);
	}
	}
	public static readonly Instruction SUB_S = new InstructionAnonymousInnerClass130();

	private class InstructionAnonymousInnerClass130 : Instruction
	{
		public InstructionAnonymousInnerClass130() : base(127)
		{
		}


	public override sealed string name()
	{
		return "SUB.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;
		int ft = (insn >> 16) & 31;


					processor.cpu.doSUBS(fd, fs, ft);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFdForStore();
		context.loadFs();
		context.loadFt();
		context.MethodVisitor.visitInsn(Opcodes.FSUB);
		context.storeFd();
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;
		int ft = (insn >> 16) & 31;

	return Common.disasmFDFSFT("sub.s", fd, fs, ft);
	}
	}
	public static readonly Instruction MUL_S = new InstructionAnonymousInnerClass131();

	private class InstructionAnonymousInnerClass131 : Instruction
	{
		public InstructionAnonymousInnerClass131() : base(128)
		{
		}


	public override sealed string name()
	{
		return "MUL.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;
		int ft = (insn >> 16) & 31;


					processor.cpu.doMULS(fd, fs, ft);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (IMPLEMENT_ROUNDING_MODES)
		{
			context.compileFDFSFT("doMULS");
		}
		else
		{
			context.prepareFdForStore();
			context.loadFs();
			context.loadFt();
			context.MethodVisitor.visitInsn(Opcodes.FMUL);
			context.storeFd();
		}
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;
		int ft = (insn >> 16) & 31;

	return Common.disasmFDFSFT("mul.s", fd, fs, ft);
	}
	}
	public static readonly Instruction DIV_S = new InstructionAnonymousInnerClass132();

	private class InstructionAnonymousInnerClass132 : Instruction
	{
		public InstructionAnonymousInnerClass132() : base(129)
		{
		}


	public override sealed string name()
	{
		return "DIV.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;
		int ft = (insn >> 16) & 31;


					processor.cpu.doDIVS(fd, fs, ft);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFdForStore();
		context.loadFs();
		context.loadFt();
		context.MethodVisitor.visitInsn(Opcodes.FDIV);
		context.storeFd();
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;
		int ft = (insn >> 16) & 31;

	return Common.disasmFDFSFT("div.s", fd, fs, ft);
	}
	}
	public static readonly Instruction SQRT_S = new InstructionAnonymousInnerClass133();

	private class InstructionAnonymousInnerClass133 : Instruction
	{
		public InstructionAnonymousInnerClass133() : base(130)
		{
		}


	public override sealed string name()
	{
		return "Sqrt.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;


					processor.cpu.doSQRTS(fd, fs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFdForStore();
		context.loadFs();
		context.MethodVisitor.visitInsn(Opcodes.F2D);
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "Sqrt", "(D)D");
		context.MethodVisitor.visitInsn(Opcodes.D2F);
		context.storeFd();
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;

	return Common.disasmFDFS("Sqrt.s", fd, fs);
	}
	}
	public static readonly Instruction ABS_S = new InstructionAnonymousInnerClass134();

	private class InstructionAnonymousInnerClass134 : Instruction
	{
		public InstructionAnonymousInnerClass134() : base(131)
		{
		}


	public override sealed string name()
	{
		return "ABS.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;


					processor.cpu.doABSS(fd, fs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFdForStore();
		context.loadFs();
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "abs", "(F)F");
		context.storeFd();
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;

	return Common.disasmFDFS("abs.s", fd, fs);
	}
	}
	public static readonly Instruction MOV_S = new InstructionAnonymousInnerClass135();

	private class InstructionAnonymousInnerClass135 : Instruction
	{
		public InstructionAnonymousInnerClass135() : base(132)
		{
		}


	public override sealed string name()
	{
		return "MOV.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;


					processor.cpu.doMOVS(fd, fs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFdForStore();
		context.loadFs();
		context.storeFd();
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;

	return Common.disasmFDFS("mov.s", fd, fs);
	}
	}
	public static readonly Instruction NEG_S = new InstructionAnonymousInnerClass136();

	private class InstructionAnonymousInnerClass136 : Instruction
	{
		public InstructionAnonymousInnerClass136() : base(133)
		{
		}


	public override sealed string name()
	{
		return "NEG.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;


					processor.cpu.doNEGS(fd, fs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFdForStore();
		context.loadFs();
		context.MethodVisitor.visitInsn(Opcodes.FNEG);
		context.storeFd();
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;

	return Common.disasmFDFS("neg.s", fd, fs);
	}
	}
	public static readonly Instruction ROUND_W_S = new InstructionAnonymousInnerClass137();

	private class InstructionAnonymousInnerClass137 : Instruction
	{
		public InstructionAnonymousInnerClass137() : base(134)
		{
		}


	public override sealed string name()
	{
		return "ROUND.W.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;


					processor.cpu.doROUNDWS(fd, fs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFdForStore();
		context.loadFs();
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "round", "(F)I");
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "intBitsToFloat", "(I)F");
		context.storeFd();
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;

	return Common.disasmFDFS("round.w.s", fd, fs);
	}
	}
	public static readonly Instruction TRUNC_W_S = new InstructionAnonymousInnerClass138();

	private class InstructionAnonymousInnerClass138 : Instruction
	{
		public InstructionAnonymousInnerClass138() : base(135)
		{
		}


	public override sealed string name()
	{
		return "TRUNC.W.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;


					processor.cpu.doTRUNCWS(fd, fs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFdForStore();
		context.loadFs();
		context.MethodVisitor.visitInsn(Opcodes.F2I);
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "intBitsToFloat", "(I)F");
		context.storeFd();
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;

	return Common.disasmFDFS("trunc.w.s", fd, fs);
	}
	}
	public static readonly Instruction CEIL_W_S = new InstructionAnonymousInnerClass139();

	private class InstructionAnonymousInnerClass139 : Instruction
	{
		public InstructionAnonymousInnerClass139() : base(136)
		{
		}


	public override sealed string name()
	{
		return "CEIL.W.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;


					processor.cpu.doCEILWS(fd, fs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFdForStore();
		context.loadFs();
		context.MethodVisitor.visitInsn(Opcodes.F2D);
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "ceil", "(D)D");
		context.MethodVisitor.visitInsn(Opcodes.D2I);
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "intBitsToFloat", "(I)F");
		context.storeFd();
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;

	return Common.disasmFDFS("ceil.w.s", fd, fs);
	}
	}
	public static readonly Instruction FLOOR_W_S = new InstructionAnonymousInnerClass140();

	private class InstructionAnonymousInnerClass140 : Instruction
	{
		public InstructionAnonymousInnerClass140() : base(137)
		{
		}


	public override sealed string name()
	{
		return "FLOOR.W.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;


					processor.cpu.doFLOORWS(fd, fs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFdForStore();
		context.loadFs();
		context.MethodVisitor.visitInsn(Opcodes.F2D);
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "floor", "(D)D");
		context.MethodVisitor.visitInsn(Opcodes.D2I);
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "intBitsToFloat", "(I)F");
		context.storeFd();
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;

	return Common.disasmFDFS("floor.w.s", fd, fs);
	}
	}
	public static readonly Instruction CVT_S_W = new InstructionAnonymousInnerClass141();

	private class InstructionAnonymousInnerClass141 : Instruction
	{
		public InstructionAnonymousInnerClass141() : base(138)
		{
		}


	public override sealed string name()
	{
		return "CVT.S.W";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;


					processor.cpu.doCVTSW(fd, fs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFdForStore();
		context.loadFs();
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "floatToRawIntBits", "(F)I");
		context.MethodVisitor.visitInsn(Opcodes.I2F);
		context.storeFd();
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;

	return Common.disasmFDFS("cvt.s.w", fd, fs);
	}
	}
	public static readonly Instruction CVT_W_S = new InstructionAnonymousInnerClass142();

	private class InstructionAnonymousInnerClass142 : Instruction
	{
		public InstructionAnonymousInnerClass142() : base(139)
		{
		}


	public override sealed string name()
	{
		return "CVT.W.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;


					processor.cpu.doCVTWS(fd, fs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int fd = (insn >> 6) & 31;
		int fs = (insn >> 11) & 31;

	return Common.disasmFDFS("cvt.w.s", fd, fs);
	}
	}
	public static readonly Instruction C_COND_S = new InstructionAnonymousInnerClass143();

	private class InstructionAnonymousInnerClass143 : Instruction
	{
		public InstructionAnonymousInnerClass143() : base(140)
		{
		}


	public override sealed string name()
	{
		return "C.COND.S";
	}
	public override sealed string category()
	{
		return "FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int fcond = (insn >> 0) & 15;
		int fs = (insn >> 11) & 31;
		int ft = (insn >> 16) & 31;


					processor.cpu.doCCONDS(fs, ft, fcond);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int fcond = (insn >> 0) & 15;

		Label isNaN = new Label();
		Label isNotNaN = new Label();
		Label continueLabel = new Label();
		Label trueLabel = new Label();

		context.prepareFcr31cForStore();
		context.loadFt();
		context.storeFTmp2();
		context.loadFs();
		context.MethodVisitor.visitInsn(Opcodes.DUP);
		context.storeFTmp1();
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "isNaN", "(F)Z");
		context.MethodVisitor.visitJumpInsn(Opcodes.IFNE, isNaN);
		context.loadFTmp2();
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "isNaN", "(F)Z");
		context.MethodVisitor.visitJumpInsn(Opcodes.IFEQ, isNotNaN);
		context.MethodVisitor.visitLabel(isNaN);
		context.MethodVisitor.visitInsn((fcond & 1) != 0 ? Opcodes.ICONST_1 : Opcodes.ICONST_0);
		context.MethodVisitor.visitJumpInsn(Opcodes.GOTO, continueLabel);
		context.MethodVisitor.visitLabel(isNotNaN);
		bool equal = (fcond & 2) != 0;
		bool less = (fcond & 4) != 0;
		if (!equal && !less)
		{
			context.MethodVisitor.visitInsn(Opcodes.ICONST_0);
		}
		else
		{
			int testOpcode = (equal ? (less ? Opcodes.IFLE : Opcodes.IFEQ) : Opcodes.IFLT);
			context.loadFTmp1();
			context.loadFTmp2();
			context.MethodVisitor.visitInsn(Opcodes.FCMPL); // FCMPG and FCMPL would produce the same result as both values are not NaN
			context.MethodVisitor.visitJumpInsn(testOpcode, trueLabel);
			context.MethodVisitor.visitInsn(Opcodes.ICONST_0);
			context.MethodVisitor.visitJumpInsn(Opcodes.GOTO, continueLabel);
			context.MethodVisitor.visitLabel(trueLabel);
			context.MethodVisitor.visitInsn(Opcodes.ICONST_1);
		}
		context.MethodVisitor.visitLabel(continueLabel);
		context.storeFcr31c();
	}
	public override string disasm(int address, int insn)
	{
		int fcond = (insn >> 0) & 15;
		int fs = (insn >> 11) & 31;
		int ft = (insn >> 16) & 31;

	return Common.disasmCcondS(fcond, fs, ft);
	}
	}
	public static readonly Instruction MFC1 = new InstructionAnonymousInnerClass144(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass144 : Instruction
	{
		public InstructionAnonymousInnerClass144(UnknownType FLAG_WRITES_RT) : base(141, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "MFC1";
	}
	public override sealed string category()
	{
		return "MIPS I/FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int c1dr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;


					processor.cpu.doMFC1(rt, c1dr);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			context.prepareRtForStore();
			context.loadFCr();
			context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "floatToRawIntBits", "(F)I");
			context.storeRt();
		}
	}
	public override string disasm(int address, int insn)
	{
		int c1dr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

	return Common.disasmRTFS("mfc1", rt, c1dr);
	}
	}
	public static readonly Instruction CFC1 = new InstructionAnonymousInnerClass145();

	private class InstructionAnonymousInnerClass145 : Instruction
	{
		public InstructionAnonymousInnerClass145() : base(142)
		{
		}


	public override sealed string name()
	{
		return "CFC1";
	}
	public override sealed string category()
	{
		return "MIPS I/FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int c1cr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;


					processor.cpu.doCFC1(rt, c1cr);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int c1cr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

	return Common.disasmRTFC("cfc1", rt, c1cr);
	}
	}
	public static readonly Instruction MTC1 = new InstructionAnonymousInnerClass146();

	private class InstructionAnonymousInnerClass146 : Instruction
	{
		public InstructionAnonymousInnerClass146() : base(143)
		{
		}


	public override sealed string name()
	{
		return "MTC1";
	}
	public override sealed string category()
	{
		return "MIPS I/FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int c1dr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;


					processor.cpu.doMTC1(rt, c1dr);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareFCrForStore();
		context.loadRt();
		context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "intBitsToFloat", "(I)F");
		context.storeFCr();
	}
	public override string disasm(int address, int insn)
	{
		int c1dr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

	return Common.disasmRTFS("mtc1", rt, c1dr);
	}
	}
	public static readonly Instruction CTC1 = new InstructionAnonymousInnerClass147();

	private class InstructionAnonymousInnerClass147 : Instruction
	{
		public InstructionAnonymousInnerClass147() : base(144)
		{
		}


	public override sealed string name()
	{
		return "CTC1";
	}
	public override sealed string category()
	{
		return "MIPS I/FPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int c1cr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;


					processor.cpu.doCTC1(rt, c1cr);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int c1cr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

	return Common.disasmRTFC("ctc1", rt, c1cr);
	}
	}
	public static readonly Instruction MFC0 = new InstructionAnonymousInnerClass148();

	private class InstructionAnonymousInnerClass148 : Instruction
	{
		public InstructionAnonymousInnerClass148() : base(145)
		{
		}


	public override sealed string name()
	{
		return "MFC0";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int c0dr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

		int value = processor.cp0.getDataRegister(c0dr);

		// Manipulate some special values
		switch (c0dr)
		{
			case COP0_STATE_COUNT: // System counter
				value = (int) Emulator.Clock.nanoTime();
				break;
		}

		processor.cpu.setRegister(rt, value);

		if (logCop0.TraceEnabled)
		{
			logCop0.trace(string.Format("0x{0:X8} - mfc0 reading data register#{1:D}({2}) having value 0x{3:X8}", processor.cpu.pc, c0dr, Common.cop0Names[c0dr], value));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int c0dr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

		return Common.disasmRTC0dr("mfc0", rt, c0dr);
	}
	}
	public static readonly Instruction CFC0 = new InstructionAnonymousInnerClass149();

	private class InstructionAnonymousInnerClass149 : Instruction
	{
		public InstructionAnonymousInnerClass149() : base(146)
		{
		}


	public override sealed string name()
	{
		return "CFC0";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int c0cr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

		int value = processor.cp0.getControlRegister(c0cr);
		processor.cpu.setRegister(rt, value);

		if (logCop0.TraceEnabled)
		{
			logCop0.trace(string.Format("0x{0:X8} - cfc0 reading control register#{1:D} having value 0x{2:X8}", processor.cpu.pc, c0cr, value));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int c0cr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

		return Common.disasmRTC0cr("cfc0", rt, c0cr);
	}
	}
	public static readonly Instruction MTC0 = new InstructionAnonymousInnerClass150(FLAG_MODIFIES_INTERRUPT_STATE);

	private class InstructionAnonymousInnerClass150 : Instruction
	{
		public InstructionAnonymousInnerClass150(UnknownType FLAG_MODIFIES_INTERRUPT_STATE) : base(147, FLAG_MODIFIES_INTERRUPT_STATE)
		{
		}


	public override sealed string name()
	{
		return "MTC0";
	}
	public override sealed string category()
	{
		return "MIPS I";
	}
	public override void interpret(Processor processor, int insn)
	{
		int c0dr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

		int value = processor.cpu.getRegister(rt);

		if (logCop0.TraceEnabled)
		{
			logCop0.trace(string.Format("0x{0:X8} - mtc0 setting data register#{1:D}({2}) to value 0x{3:X8}", processor.cpu.pc, c0dr, Common.cop0Names[c0dr], value));
		}

		switch (c0dr)
		{
			case COP0_STATE_COUNT:
				// Count is set to 0 at boot time
				if (value != 0)
				{
					processor.cpu.doUNK(string.Format("Unsupported mtc0 instruction for c0dr={0:D}({1}), value=0x{2:X}", c0dr, Common.cop0Names[c0dr], value));
				}
				break;
			case COP0_STATE_COMPARE:
				// Compare is set to 0x80000000 at boot time
				if (value != unchecked((int)0x80000000))
				{
					processor.cpu.doUNK(string.Format("Unsupported mtc0 instruction for c0dr={0:D}({1}), value=0x{2:X}", c0dr, Common.cop0Names[c0dr], value));
				}
				break;
			case COP0_STATE_CONFIG:
				processor.cpu.doUNK(string.Format("Unsupported mtc0 instruction for c0dr={0:D}({1}), value=0x{2:X}", c0dr, Common.cop0Names[c0dr], value));
				break;
		}
		processor.cp0.setDataRegister(c0dr, value);
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int c0dr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

		return Common.disasmRTC0dr("mtc0", rt, c0dr);
	}
	}
	public static readonly Instruction CTC0 = new InstructionAnonymousInnerClass151();

	private class InstructionAnonymousInnerClass151 : Instruction
	{
		public InstructionAnonymousInnerClass151() : base(148)
		{
		}


	public override sealed string name()
	{
		return "CTC0";
	}
	public override sealed string category()
	{
		return "ALLEGREX";
	}
	public override void interpret(Processor processor, int insn)
	{
		int c0cr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

		int value = processor.cpu.getRegister(rt);
		processor.cp0.setControlRegister(c0cr, value);

		if (c0cr == 13)
		{
			reboot.setLog4jMDC();
		}

		if (logCop0.TraceEnabled)
		{
			logCop0.trace(string.Format("0x{0:X8} - ctc0 setting control register#{1:D} to value 0x{2:X8}", processor.cpu.pc, c0cr, value));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int c0cr = (insn >> 11) & 31;
		int rt = (insn >> 16) & 31;

		return Common.disasmRTC0cr("ctc0", rt, c0cr);
	}
	}
	public static readonly Instruction VADD = new InstructionAnonymousInnerClass152(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass152 : Instruction
	{
		public InstructionAnonymousInnerClass152(int FLAG_USES_VFPU_PFXS) : base(149, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VADD";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVADD(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileVFPUInstr(null, Opcodes.FADD, null);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT("vadd", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VSUB = new InstructionAnonymousInnerClass153(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass153 : Instruction
	{
		public InstructionAnonymousInnerClass153(int FLAG_USES_VFPU_PFXS) : base(150, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VSUB";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVSUB(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileVFPUInstr(null, Opcodes.FSUB, null);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT("vsub", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VSBN = new InstructionAnonymousInnerClass154(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass154 : Instruction
	{
		public InstructionAnonymousInnerClass154(int FLAG_USES_VFPU_PFXS) : base(151, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VSBN";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVSBN(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		if (vsize == 1)
		{
			context.startPfxCompiled();
			for (int n = 0; n < vsize; n++)
			{
				context.prepareVdForStore(n);
				context.loadVs(n);
				context.loadVtInt(n);
				context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "scalb", "(FI)F");
				context.storeVd(n);
			}
			context.endPfxCompiled();
		}
		else
		{
			// Only VSBN.S is supported
			context.compileInterpreterInstruction();
		}
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT("vsbn", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VDIV = new InstructionAnonymousInnerClass155(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass155 : Instruction
	{
		public InstructionAnonymousInnerClass155(int FLAG_USES_VFPU_PFXS) : base(152, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VDIV";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVDIV(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileVFPUInstr(null, Opcodes.FDIV, null);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT("vdiv", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VMUL = new InstructionAnonymousInnerClass156(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass156 : Instruction
	{
		public InstructionAnonymousInnerClass156(int FLAG_USES_VFPU_PFXS) : base(153, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VMUL";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVMUL(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileVFPUInstr(null, Opcodes.FMUL, null);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT("vmul", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VDOT = new InstructionAnonymousInnerClass157(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass157 : Instruction
	{
		public InstructionAnonymousInnerClass157(int FLAG_USES_VFPU_PFXS) : base(154, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VDOT";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVDOT(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		if (vsize > 1)
		{
			context.startPfxCompiled();
			context.prepareVdForStore(1, 0);
			context.loadVs(0);
			context.loadVt(0);
			context.MethodVisitor.visitInsn(Opcodes.FMUL);
			for (int n = 1; n < vsize; n++)
			{
				context.loadVs(n);
				context.loadVt(n);
				context.MethodVisitor.visitInsn(Opcodes.FMUL);
				context.MethodVisitor.visitInsn(Opcodes.FADD);
			}
			context.storeVd(1, 0);
			context.endPfxCompiled(1);
		}
		else
		{
			// Unsupported VDOT.S
			context.compileInterpreterInstruction();
		}
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVD1VSVT("vdot", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VSCL = new InstructionAnonymousInnerClass158(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass158 : Instruction
	{
		public InstructionAnonymousInnerClass158(int FLAG_USES_VFPU_PFXS) : base(155, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VSCL";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVSCL(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		if (vsize > 1)
		{
			context.startPfxCompiled();
			context.loadVt(1, 0);
			context.storeFTmp1();
			for (int n = 0; n < vsize; n++)
			{
				context.prepareVdForStore(n);
				context.loadVs(n);
				context.loadFTmp1();
				context.MethodVisitor.visitInsn(Opcodes.FMUL);
				context.storeVd(n);
			}
			context.endPfxCompiled();
		}
		else
		{
			// Unsupported VSCL.S
			context.compileInterpreterInstruction();
		}
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT1("vscl", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VHDP = new InstructionAnonymousInnerClass159(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass159 : Instruction
	{
		public InstructionAnonymousInnerClass159(int FLAG_USES_VFPU_PFXS) : base(156, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VHDP";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVHDP(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		if (vsize > 1)
		{
			context.startPfxCompiled();
			context.prepareVdForStore(1, 0);
			context.loadVs(0);
			context.loadVt(0);
			context.MethodVisitor.visitInsn(Opcodes.FMUL);
			for (int n = 1; n < vsize - 1; n++)
			{
				context.loadVs(n);
				context.loadVt(n);
				context.MethodVisitor.visitInsn(Opcodes.FMUL);
				context.MethodVisitor.visitInsn(Opcodes.FADD);
			}
			context.loadVt(vsize - 1);
			context.MethodVisitor.visitInsn(Opcodes.FADD);
			context.storeVd(1, 0);
			context.endPfxCompiled(1);
		}
		else
		{
			// Unsupported VHDP.S
			context.compileInterpreterInstruction();
		}
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVD1VSVT("vhdp", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VCRS = new InstructionAnonymousInnerClass160(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass160 : Instruction
	{
		public InstructionAnonymousInnerClass160(int FLAG_USES_VFPU_PFXS) : base(157, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VCRS";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVCRS(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT("vcrs", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VDET = new InstructionAnonymousInnerClass161(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass161 : Instruction
	{
		public InstructionAnonymousInnerClass161(int FLAG_USES_VFPU_PFXS) : base(158, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VDET";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVDET(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVD1VSVT("vdet", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction MFV = new InstructionAnonymousInnerClass162(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass162 : Instruction
	{
		public InstructionAnonymousInnerClass162(UnknownType FLAG_WRITES_RT) : base(159, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "MFV";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm7 = (insn >> 0) & 127;
		int rt = (insn >> 16) & 31;


					processor.cpu.doMFV(rt, imm7);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			context.prepareRtForStore();
			context.loadVdInt(1, 0);
			context.storeRt();
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm7 = (insn >> 0) & 127;
		int rt = (insn >> 16) & 31;

		return Common.disasmVDRS("mfv", imm7, rt);
	}
	}
	public static readonly Instruction MFVC = new InstructionAnonymousInnerClass163(FLAG_WRITES_RT);

	private class InstructionAnonymousInnerClass163 : Instruction
	{
		public InstructionAnonymousInnerClass163(UnknownType FLAG_WRITES_RT) : base(160, FLAG_WRITES_RT)
		{
		}


	public override sealed string name()
	{
		return "MFVC";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm7 = (insn >> 0) & 127;
		int rt = (insn >> 16) & 31;


					processor.cpu.doMFVC(rt, imm7);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (!context.RtRegister0)
		{
			int imm7 = context.Imm7;
			MethodVisitor mv = context.MethodVisitor;
			switch (imm7)
			{
				case 3:
				{
					context.prepareRtForStore();
					context.loadVcrCc(5);
					for (int i = 4; i >= 0; i--)
					{
						context.loadImm(1);
						mv.visitInsn(Opcodes.ISHL);
						context.loadVcrCc(i);
						mv.visitInsn(Opcodes.IOR);
					}
					context.storeRt();
					break;
				}
				default:
					base.compile(context, insn);
					break;
			}
		}
	}
	public override string disasm(int address, int insn)
	{
		int imm7 = (insn >> 0) & 127;
		int rt = (insn >> 16) & 31;

		return Common.disasmRTIMM7("mfvc", rt, imm7);
	}
	}
	public static readonly Instruction MTV = new InstructionAnonymousInnerClass164();

	private class InstructionAnonymousInnerClass164 : Instruction
	{
		public InstructionAnonymousInnerClass164() : base(161)
		{
		}


	public override sealed string name()
	{
		return "MTV";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm7 = (insn >> 0) & 127;
		int rt = (insn >> 16) & 31;


					processor.cpu.doMTV(rt, imm7);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.prepareVdForStoreInt(1, 0);
		context.loadRt();
		context.storeVdInt(1, 0);
	}
	public override string disasm(int address, int insn)
	{
		int imm7 = (insn >> 0) & 127;
		int rt = (insn >> 16) & 31;

	return Common.disasmVDRS("MTV", imm7, rt);
	}
	}
	public static readonly Instruction MTVC = new InstructionAnonymousInnerClass165();

	private class InstructionAnonymousInnerClass165 : Instruction
	{
		public InstructionAnonymousInnerClass165() : base(162)
		{
		}


	public override sealed string name()
	{
		return "MTVC";
	}
	public override sealed string category()
	{
		return "MIPS I/VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm7 = (insn >> 0) & 127;
		int rt = (insn >> 16) & 31;


					processor.cpu.doMTVC(rt, imm7);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm7 = (insn >> 0) & 127;
		int rt = (insn >> 16) & 31;

		return Common.disasmRTIMM7("mtvc", rt, imm7);
	}
	}
	public static readonly Instruction VCMP = new InstructionAnonymousInnerClass166(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass166 : Instruction
	{
		public InstructionAnonymousInnerClass166(int FLAG_USES_VFPU_PFXS) : base(163, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VCMP";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm4 = (insn >> 0) & 15;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVCMP(1 + one + (two << 1), vs, vt, imm4);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int cond = context.Imm4;
		int vsize = context.Vsize;
		MethodVisitor mv = context.MethodVisitor;
		bool not = (cond & 4) != 0;
		context.startPfxCompiled();
		if ((cond & 8) == 0)
		{
			if ((cond & 3) == 0)
			{
				int value = not ? 1 : 0;
				for (int n = 0; n < vsize; n++)
				{
					context.prepareVcrCcForStore(n);
					context.loadImm(value);
					context.storeVcrCc(n);
				}
				context.prepareVcrCcForStore(4);
				context.loadImm(value);
				context.storeVcrCc(4);
				context.prepareVcrCcForStore(5);
				context.loadImm(value);
				context.storeVcrCc(5);
			}
			else
			{
				if (vsize > 1)
				{
					context.loadImm(0);
					context.storeTmp1();
					context.loadImm(1);
					context.storeTmp2();
				}
				for (int n = 0; n < vsize; n++)
				{
					context.prepareVcrCcForStore(n);
					context.loadVs(n);
					context.loadVt(n);
					// In Java, the two opcodes for float comparisons (FCMPG and FCMPL) differ
					// only in how they handle NaN ("not a number").
					// If one or both of the values is NaN, the FCMPG instruction pushes a 1,
					// whereas the FCMPL instruction pushes a -1.
					// On the PSP, comparing NaN values always returns false.
					// - vcmp GE  => use FCMPL (-1 will be interpreted as false)
					// - vcmp GT  => use FCMPL (-1 will be interpreted as false)
					// - vcmp LE  => use FCMPG (1 will be interpreted as false)
					// - vcmp LT  => use FCMPG (1 will be interpreted as false)
					// - vcmp EQ  => use either FCMPL or FCMPG
					// - vcmp NE  => use either FCMPL or FCMPG
					mv.visitInsn(not ? Opcodes.FCMPL : Opcodes.FCMPG);
					int opcodeCond = Opcodes.NOP;
					switch (cond & 3)
					{
						case 1:
							opcodeCond = not ? Opcodes.IFNE : Opcodes.IFEQ;
							break;
						case 2:
							opcodeCond = not ? Opcodes.IFGE : Opcodes.IFLT;
							break;
						case 3:
							opcodeCond = not ? Opcodes.IFGT : Opcodes.IFLE;
							break;
					}
					Label trueLabel = new Label();
					Label afterLabel = new Label();
					mv.visitJumpInsn(opcodeCond, trueLabel);
					context.loadImm(0);
					if (vsize > 1)
					{
						context.loadImm(0);
						context.storeTmp2();
					}
					else
					{
						context.prepareVcrCcForStore(4);
						context.loadImm(0);
						context.storeVcrCc(4);
						context.prepareVcrCcForStore(5);
						context.loadImm(0);
						context.storeVcrCc(5);
					}
					mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
					mv.visitLabel(trueLabel);
					context.loadImm(1);
					if (vsize > 1)
					{
						context.loadImm(1);
						context.storeTmp1();
					}
					else
					{
						context.prepareVcrCcForStore(4);
						context.loadImm(1);
						context.storeVcrCc(4);
						context.prepareVcrCcForStore(5);
						context.loadImm(1);
						context.storeVcrCc(5);
					}
					mv.visitLabel(afterLabel);
					context.storeVcrCc(n);
				}
				if (vsize > 1)
				{
					context.prepareVcrCcForStore(4);
					context.loadTmp1();
					context.storeVcrCc(4);
					context.prepareVcrCcForStore(5);
					context.loadTmp2();
					context.storeVcrCc(5);
				}
			}
		}
		else
		{
			if (vsize > 1)
			{
				context.loadImm(0);
				context.storeTmp1();
				context.loadImm(1);
				context.storeTmp2();
			}
			for (int n = 0; n < vsize; n++)
			{
				context.prepareVcrCcForStore(n);
				context.loadVs(n);
				bool updateOrAnd = false;
				switch (cond & 3)
				{
					case 0:
					{
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "abs", "(F)F");
						mv.visitInsn(Opcodes.FCONST_0);
						mv.visitInsn(Opcodes.FCMPL); // Use FCMPL or FCMPG, it doesn't matter for testing NE or EQ
						Label trueLabel = new Label();
						Label afterLabel = new Label();
						mv.visitJumpInsn(not ? Opcodes.IFNE : Opcodes.IFEQ, trueLabel);
						context.loadImm(0);
						if (vsize > 1)
						{
							context.loadImm(0);
							context.storeTmp2();
						}
						else
						{
							context.prepareVcrCcForStore(4);
							context.loadImm(0);
							context.storeVcrCc(4);
							context.prepareVcrCcForStore(5);
							context.loadImm(0);
							context.storeVcrCc(5);
						}
						mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
						mv.visitLabel(trueLabel);
						context.loadImm(1);
						if (vsize > 1)
						{
							context.loadImm(1);
							context.storeTmp1();
						}
						else
						{
							context.prepareVcrCcForStore(4);
							context.loadImm(1);
							context.storeVcrCc(4);
							context.prepareVcrCcForStore(5);
							context.loadImm(1);
							context.storeVcrCc(5);
						}
						mv.visitLabel(afterLabel);
						break;
					}
					case 1:
					{
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "isNaN", "(F)Z");
						updateOrAnd = true;
						break;
					}
					case 2:
					{
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "isInfinite", "(F)Z");
						updateOrAnd = true;
						break;
					}
					case 3:
					{
						mv.visitInsn(Opcodes.DUP);
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "isNaN", "(F)Z");
						mv.visitInsn(Opcodes.SWAP);
						mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "isInfinite", "(F)Z");
						mv.visitInsn(Opcodes.IOR);
						updateOrAnd = true;
						break;
					}
				}

				if (updateOrAnd)
				{
					if (not)
					{
						context.loadImm(1);
						mv.visitInsn(Opcodes.IXOR);
					}

					if (vsize > 1)
					{
						mv.visitInsn(Opcodes.DUP);
						context.loadTmp1();
						mv.visitInsn(Opcodes.IOR);
						context.storeTmp1();

						mv.visitInsn(Opcodes.DUP);
						context.loadTmp2();
						mv.visitInsn(Opcodes.IAND);
						context.storeTmp2();
					}
					else
					{
						mv.visitInsn(Opcodes.DUP);
						context.storeVcrCc(4);

						mv.visitInsn(Opcodes.DUP);
						context.storeVcrCc(5);
					}
				}

				context.storeVcrCc(n);
			}
			if (vsize > 1)
			{
				context.prepareVcrCcForStore(4);
				context.loadTmp1();
				context.storeVcrCc(4);
				context.prepareVcrCcForStore(5);
				context.loadTmp2();
				context.storeVcrCc(5);
			}
		}
		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int imm4 = (insn >> 0) & 15;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVCMP("vcmp", 1 + one + (two << 1), imm4, vs, vt);
	}
	}
	public static readonly Instruction VMIN = new InstructionAnonymousInnerClass167(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass167 : Instruction
	{
		public InstructionAnonymousInnerClass167(int FLAG_USES_VFPU_PFXS) : base(164, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VMIN";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVMIN(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileVFPUInstr(null, Opcodes.NOP, "min");
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT("vmin", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VMAX = new InstructionAnonymousInnerClass168(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass168 : Instruction
	{
		public InstructionAnonymousInnerClass168(int FLAG_USES_VFPU_PFXS) : base(165, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VMAX";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVMAX(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileVFPUInstr(null, Opcodes.NOP, "max");
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT("vmax", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VSCMP = new InstructionAnonymousInnerClass169(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass169 : Instruction
	{
		public InstructionAnonymousInnerClass169(int FLAG_USES_VFPU_PFXS) : base(166, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VSCMP";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVSCMP(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT("vscmp", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VSGE = new InstructionAnonymousInnerClass170(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass170 : Instruction
	{
		public InstructionAnonymousInnerClass170(int FLAG_USES_VFPU_PFXS) : base(167, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VSGE";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVSGE(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		context.startPfxCompiled();
		MethodVisitor mv = context.MethodVisitor;
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStore(n);
			context.loadVs(n);
			context.loadVt(n);
			mv.visitInsn(Opcodes.FCMPL); // Use FCMPL, not FCMPG: NaN has to return false.
			Label trueLabel = new Label();
			Label afterLabel = new Label();
			mv.visitJumpInsn(Opcodes.IFGE, trueLabel);
			mv.visitInsn(Opcodes.FCONST_0);
			mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
			mv.visitLabel(trueLabel);
			mv.visitInsn(Opcodes.FCONST_1);
			mv.visitLabel(afterLabel);
			context.storeVd(n);
		}
		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT("vsge", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VSLT = new InstructionAnonymousInnerClass171(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass171 : Instruction
	{
		public InstructionAnonymousInnerClass171(int FLAG_USES_VFPU_PFXS) : base(168, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VSLT";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVSLT(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		context.startPfxCompiled();
		MethodVisitor mv = context.MethodVisitor;
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStore(n);
			context.loadVs(n);
			context.loadVt(n);
			mv.visitInsn(Opcodes.FCMPG); // Use FCMPG, not FCMPL: NaN has to return false
			Label trueLabel = new Label();
			Label afterLabel = new Label();
			mv.visitJumpInsn(Opcodes.IFLT, trueLabel);
			mv.visitInsn(Opcodes.FCONST_0);
			mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
			mv.visitLabel(trueLabel);
			mv.visitInsn(Opcodes.FCONST_1);
			mv.visitLabel(afterLabel);
			context.storeVd(n);
		}
		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT("vslt", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VMOV = new InstructionAnonymousInnerClass172(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_CONSUMES_VFPU_PFXT | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass172 : Instruction
	{
		public InstructionAnonymousInnerClass172(int FLAG_USES_VFPU_PFXS) : base(169, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_CONSUMES_VFPU_PFXT | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VMOV";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVMOV(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileVFPUInstr(null, Opcodes.NOP, null);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vmov", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VABS = new InstructionAnonymousInnerClass173(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass173 : Instruction
	{
		public InstructionAnonymousInnerClass173(int FLAG_USES_VFPU_PFXS) : base(170, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VABS";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVABS(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileVFPUInstr(null, Opcodes.NOP, "abs");
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vabs", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VNEG = new InstructionAnonymousInnerClass174(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass174 : Instruction
	{
		public InstructionAnonymousInnerClass174(int FLAG_USES_VFPU_PFXS) : base(171, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VNEG";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVNEG(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileVFPUInstr(null, Opcodes.FNEG, null);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vneg", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VIDT = new InstructionAnonymousInnerClass175(FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass175 : Instruction
	{
		public InstructionAnonymousInnerClass175(int FLAG_USES_VFPU_PFXD) : base(172, FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VIDT";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;


					processor.cpu.doVIDT(1 + one + (two << 1), vd);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.startPfxCompiled();
		int vsize = context.Vsize;
		int id = context.VdRegisterIndex % vsize;
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStore(n);
			context.MethodVisitor.visitInsn(id == n ? Opcodes.FCONST_1 : Opcodes.FCONST_0);
			context.storeVd(n);
		}
		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;

	return Common.disasmVD("vidt", 1 + one + (two << 1), vd);
	}
	}
	public static readonly Instruction VSAT0 = new InstructionAnonymousInnerClass176(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass176 : Instruction
	{
		public InstructionAnonymousInnerClass176(int FLAG_USES_VFPU_PFXS) : base(173, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VSAT0";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVSAT0(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		MethodVisitor mv = context.MethodVisitor;
		context.startPfxCompiled();
		for (int n = 0; n < vsize; n++)
		{
			// float stackValue = vs[n];
			// if (stackValue <= 0.f) {
			//     stackValue = 0.f;
			// } else if (stackValue > 1.f) {
			//     stackValue = 1.f;
			// }
			// vd[n] = stackValue;
			context.prepareVdForStore(n);
			context.loadVs(n);
			mv.visitInsn(Opcodes.DUP);
			mv.visitLdcInsn(0.0f);
			mv.visitInsn(Opcodes.FCMPG); // Use FCMPG, not FCMPL: NaN has to be left unchanged
			Label limitLabel = new Label();
			Label afterLabel = new Label();
			mv.visitJumpInsn(Opcodes.IFGT, limitLabel);
			mv.visitInsn(Opcodes.POP);
			mv.visitLdcInsn(0.0f);
			mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
			mv.visitLabel(limitLabel);
			mv.visitInsn(Opcodes.DUP);
			mv.visitLdcInsn(1.0f);
			mv.visitInsn(Opcodes.FCMPL); // Use FCMPL, not FCMPG: NaN has to be left unchanged
			mv.visitJumpInsn(Opcodes.IFLE, afterLabel);
			mv.visitInsn(Opcodes.POP);
			mv.visitLdcInsn(1.0f);
			mv.visitLabel(afterLabel);
			context.storeVd(n);
		}
		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vsat0", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VSAT1 = new InstructionAnonymousInnerClass177(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass177 : Instruction
	{
		public InstructionAnonymousInnerClass177(int FLAG_USES_VFPU_PFXS) : base(174, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VSAT1";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVSAT1(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		MethodVisitor mv = context.MethodVisitor;
		context.startPfxCompiled();
		for (int n = 0; n < vsize; n++)
		{
			// float stackValue = vs[n];
			// if (stackValue <= -1.f) {
			//     stackValue = -1.f;
			// } else if (stackValue > 1.f) {
			//     stackValue = 1.f;
			// }
			// vd[n] = stackValue;
			context.prepareVdForStore(n);
			context.loadVs(n);
			mv.visitInsn(Opcodes.DUP);
			mv.visitLdcInsn(-1.0f);
			mv.visitInsn(Opcodes.FCMPG); // Use FCMPG, not FCMPL: NaN has to be left unchanged
			Label limitLabel = new Label();
			Label afterLabel = new Label();
			mv.visitJumpInsn(Opcodes.IFGT, limitLabel);
			mv.visitInsn(Opcodes.POP);
			mv.visitLdcInsn(-1.0f);
			mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
			mv.visitLabel(limitLabel);
			mv.visitInsn(Opcodes.DUP);
			mv.visitLdcInsn(1.0f);
			mv.visitInsn(Opcodes.FCMPL); // Use FCMPL, not FCMPG: NaN has to be left unchanged
			mv.visitJumpInsn(Opcodes.IFLE, afterLabel);
			mv.visitInsn(Opcodes.POP);
			mv.visitLdcInsn(1.0f);
			mv.visitLabel(afterLabel);
			context.storeVd(n);
		}
		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vsat1", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VZERO = new InstructionAnonymousInnerClass178(FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass178 : Instruction
	{
		public InstructionAnonymousInnerClass178(int FLAG_USES_VFPU_PFXD) : base(175, FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VZERO";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;


					processor.cpu.doVZERO(1 + one + (two << 1), vd);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.startPfxCompiled();
		int vsize = context.Vsize;
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStore(n);
			context.MethodVisitor.visitInsn(Opcodes.FCONST_0);
			context.storeVd(n);
		}
		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;

	return Common.disasmVD("vzero", 1 + one + (two << 1), vd);
	}
	}
	public static readonly Instruction VONE = new InstructionAnonymousInnerClass179(FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass179 : Instruction
	{
		public InstructionAnonymousInnerClass179(int FLAG_USES_VFPU_PFXD) : base(176, FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VONE";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;


					processor.cpu.doVONE(1 + one + (two << 1), vd);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.startPfxCompiled();
		int vsize = context.Vsize;
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStore(n);
			context.MethodVisitor.visitInsn(Opcodes.FCONST_1);
			context.storeVd(n);
		}
		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;

	return Common.disasmVD("vone", 1 + one + (two << 1), vd);
	}
	}
	public static readonly Instruction VRCP = new InstructionAnonymousInnerClass180(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass180 : Instruction
	{
		public InstructionAnonymousInnerClass180(int FLAG_USES_VFPU_PFXS) : base(177, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VRCP";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVRCP(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileVFPUInstr(1.0f, Opcodes.FDIV, null);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vrcp", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VRSQ = new InstructionAnonymousInnerClass181(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass181 : Instruction
	{
		public InstructionAnonymousInnerClass181(int FLAG_USES_VFPU_PFXS) : base(178, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VRSQ";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVRSQ(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileVFPUInstr(1.0f, Opcodes.FDIV, "Sqrt");
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vrsq", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VSIN = new InstructionAnonymousInnerClass182(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass182 : Instruction
	{
		public InstructionAnonymousInnerClass182(int FLAG_USES_VFPU_PFXS) : base(179, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VSIN";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVSIN(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vsin", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VCOS = new InstructionAnonymousInnerClass183(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass183 : Instruction
	{
		public InstructionAnonymousInnerClass183(int FLAG_USES_VFPU_PFXS) : base(180, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VCOS";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVCOS(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vcos", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VEXP2 = new InstructionAnonymousInnerClass184(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass184 : Instruction
	{
		public InstructionAnonymousInnerClass184(int FLAG_USES_VFPU_PFXS) : base(181, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VEXP2";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVEXP2(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vexp2", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VLOG2 = new InstructionAnonymousInnerClass185(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass185 : Instruction
	{
		public InstructionAnonymousInnerClass185(int FLAG_USES_VFPU_PFXS) : base(182, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VLOG2";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVLOG2(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vlog2", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VSQRT = new InstructionAnonymousInnerClass186(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass186 : Instruction
	{
		public InstructionAnonymousInnerClass186(int FLAG_USES_VFPU_PFXS) : base(183, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VSQRT";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVSQRT(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.compileVFPUInstr(null, Opcodes.NOP, "Sqrt");
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vsqrt", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VASIN = new InstructionAnonymousInnerClass187(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass187 : Instruction
	{
		public InstructionAnonymousInnerClass187(int FLAG_USES_VFPU_PFXS) : base(184, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VASIN";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVASIN(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vasin", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VNRCP = new InstructionAnonymousInnerClass188(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass188 : Instruction
	{
		public InstructionAnonymousInnerClass188(int FLAG_USES_VFPU_PFXS) : base(185, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VNRCP";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVNRCP(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vnrcp", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VNSIN = new InstructionAnonymousInnerClass189(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass189 : Instruction
	{
		public InstructionAnonymousInnerClass189(int FLAG_USES_VFPU_PFXS) : base(186, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VNSIN";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVNSIN(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vnsin", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VREXP2 = new InstructionAnonymousInnerClass190(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass190 : Instruction
	{
		public InstructionAnonymousInnerClass190(int FLAG_USES_VFPU_PFXS) : base(187, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VREXP2";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVREXP2(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vrexp2", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VRNDS = new InstructionAnonymousInnerClass191(FLAG_USES_VFPU_PFXS);

	private class InstructionAnonymousInnerClass191 : Instruction
	{
		public InstructionAnonymousInnerClass191(UnknownType FLAG_USES_VFPU_PFXS) : base(188, FLAG_USES_VFPU_PFXS)
		{
		}


	public override sealed string name()
	{
		return "VRNDS";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVRNDS(1 + one + (two << 1), vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVS("vrnds", 1 + one + (two << 1), vs);
	}
	}
	public static readonly Instruction VRNDI = new InstructionAnonymousInnerClass192(FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass192 : Instruction
	{
		public InstructionAnonymousInnerClass192(UnknownType FLAG_USES_VFPU_PFXD) : base(189, FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VRNDI";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;


					processor.cpu.doVRNDI(1 + one + (two << 1), vd);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;

	return Common.disasmVD("vrndi", 1 + one + (two << 1), vd);
	}
	}
	public static readonly Instruction VRNDF1 = new InstructionAnonymousInnerClass193(FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass193 : Instruction
	{
		public InstructionAnonymousInnerClass193(UnknownType FLAG_USES_VFPU_PFXD) : base(190, FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VRNDF1";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;


					processor.cpu.doVRNDF1(1 + one + (two << 1), vd);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;

	return Common.disasmVD("vrndf1", 1 + one + (two << 1), vd);
	}
	}
	public static readonly Instruction VRNDF2 = new InstructionAnonymousInnerClass194(FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass194 : Instruction
	{
		public InstructionAnonymousInnerClass194(UnknownType FLAG_USES_VFPU_PFXD) : base(191, FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VRNDF2";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;


					processor.cpu.doVRNDF2(1 + one + (two << 1), vd);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;

	return Common.disasmVD("vrndf2", 1 + one + (two << 1), vd);
	}
	}
	public static readonly Instruction VF2H = new InstructionAnonymousInnerClass195(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass195 : Instruction
	{
		public InstructionAnonymousInnerClass195(int FLAG_USES_VFPU_PFXS) : base(192, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VF2H";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVF2H(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vf2h", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VH2F = new InstructionAnonymousInnerClass196(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass196 : Instruction
	{
		public InstructionAnonymousInnerClass196(int FLAG_USES_VFPU_PFXS) : base(193, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VH2F";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVH2F(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vh2f", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VSBZ = new InstructionAnonymousInnerClass197();

	private class InstructionAnonymousInnerClass197 : Instruction
	{
		public InstructionAnonymousInnerClass197() : base(194)
		{
		}


	public override sealed string name()
	{
		return "VSBZ";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVSBZ(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("VSBZ", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VLGB = new InstructionAnonymousInnerClass198();

	private class InstructionAnonymousInnerClass198 : Instruction
	{
		public InstructionAnonymousInnerClass198() : base(195)
		{
		}


	public override sealed string name()
	{
		return "VLGB";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVLGB(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vlgb", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VUC2I = new InstructionAnonymousInnerClass199(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass199 : Instruction
	{
		public InstructionAnonymousInnerClass199(int FLAG_USES_VFPU_PFXS) : base(196, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VUC2I";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVUC2I(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vuc2i", 1 + one + (two << 1), 4, vd, vs);
	}
	}
	public static readonly Instruction VC2I = new InstructionAnonymousInnerClass200(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass200 : Instruction
	{
		public InstructionAnonymousInnerClass200(int FLAG_USES_VFPU_PFXS) : base(197, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VC2I";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVC2I(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("VC2I", 1 + one + (two << 1), 4, vd, vs);
	}
	}
	public static readonly Instruction VUS2I = new InstructionAnonymousInnerClass201(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass201 : Instruction
	{
		public InstructionAnonymousInnerClass201(int FLAG_USES_VFPU_PFXS) : base(198, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VUS2I";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVUS2I(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vus2i", 1 + one + (two << 1), 1 + (one << 1), vd, vs);
	}
	}
	public static readonly Instruction VS2I = new InstructionAnonymousInnerClass202(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass202 : Instruction
	{
		public InstructionAnonymousInnerClass202(int FLAG_USES_VFPU_PFXS) : base(199, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VS2I";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVS2I(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vs2i", 1 + one + (two << 1), 1 + (one << 1), vd, vs);
	}
	}
	public static readonly Instruction VI2UC = new InstructionAnonymousInnerClass203(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass203 : Instruction
	{
		public InstructionAnonymousInnerClass203(int FLAG_USES_VFPU_PFXS) : base(200, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VI2UC";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVI2UC(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vsize = context.getVsize();
		int vsize = context.Vsize;
		if (vsize == 4)
		{
			MethodVisitor mv = context.MethodVisitor;
			context.startPfxCompiled(false);
			context.prepareVdForStoreInt(1, 0);
			for (int n = 0; n < vsize; n++)
			{
				context.loadVsInt(n);
				mv.visitInsn(Opcodes.DUP);
				Label afterLabel = new Label();
				Label negativeLabel = new Label();
				mv.visitJumpInsn(Opcodes.IFLT, negativeLabel);
				context.loadImm(23);
				mv.visitInsn(Opcodes.ISHR);
				if (n > 0)
				{
					context.loadImm(n * 8);
					mv.visitInsn(Opcodes.ISHL);
					mv.visitInsn(Opcodes.IOR);
				}
				mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
				mv.visitLabel(negativeLabel);
				mv.visitInsn(Opcodes.POP);
				if (n == 0)
				{
					context.loadImm(0);
				}
				mv.visitLabel(afterLabel);
			}
			context.storeVdInt(1, 0);
			context.endPfxCompiled(1, false);
		}
		else
		{
			// Only supported VI2UC.Q
			context.compileInterpreterInstruction();
		}
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVD1VS("vi2uc", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VI2C = new InstructionAnonymousInnerClass204(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass204 : Instruction
	{
		public InstructionAnonymousInnerClass204(int FLAG_USES_VFPU_PFXS) : base(201, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VI2C";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVI2C(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVD1VS("vi2c", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VI2US = new InstructionAnonymousInnerClass205(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass205 : Instruction
	{
		public InstructionAnonymousInnerClass205(int FLAG_USES_VFPU_PFXS) : base(202, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VI2US";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVI2US(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vi2us", 1 + one + (two << 1), 1 + two, vd, vs);
	}
	}
	public static readonly Instruction VI2S = new InstructionAnonymousInnerClass206(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass206 : Instruction
	{
		public InstructionAnonymousInnerClass206(int FLAG_USES_VFPU_PFXS) : base(203, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VI2S";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVI2S(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vi2s", 1 + one + (two << 1), 1 + two, vd, vs);
	}
	}
	public static readonly Instruction VSRT1 = new InstructionAnonymousInnerClass207(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass207 : Instruction
	{
		public InstructionAnonymousInnerClass207(int FLAG_USES_VFPU_PFXS) : base(204, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VSRT1";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVSRT1(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vsrt1", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VSRT2 = new InstructionAnonymousInnerClass208(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass208 : Instruction
	{
		public InstructionAnonymousInnerClass208(int FLAG_USES_VFPU_PFXS) : base(205, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VSRT2";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVSRT2(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vsrt2", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VBFY1 = new InstructionAnonymousInnerClass209(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass209 : Instruction
	{
		public InstructionAnonymousInnerClass209(int FLAG_USES_VFPU_PFXS) : base(206, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VBFY1";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVBFY1(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vbfy1", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VBFY2 = new InstructionAnonymousInnerClass210(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass210 : Instruction
	{
		public InstructionAnonymousInnerClass210(int FLAG_USES_VFPU_PFXS) : base(207, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VBFY2";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVBFY2(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vbfy2", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VOCP = new InstructionAnonymousInnerClass211(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass211 : Instruction
	{
		public InstructionAnonymousInnerClass211(int FLAG_USES_VFPU_PFXS) : base(208, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VOCP";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVOCP(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vocp", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VSOCP = new InstructionAnonymousInnerClass212(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass212 : Instruction
	{
		public InstructionAnonymousInnerClass212(int FLAG_USES_VFPU_PFXS) : base(209, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VSOCP";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVSOCP(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vsocp", 1 + one + (two << 1), 1 + (one << 1), vd, vs);
	}
	}
	public static readonly Instruction VFAD = new InstructionAnonymousInnerClass213(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass213 : Instruction
	{
		public InstructionAnonymousInnerClass213(int FLAG_USES_VFPU_PFXS) : base(210, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VFAD";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVFAD(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVD1VS("vfad", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VAVG = new InstructionAnonymousInnerClass214(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass214 : Instruction
	{
		public InstructionAnonymousInnerClass214(int FLAG_USES_VFPU_PFXS) : base(211, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VAVG";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVAVG(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVD1VS("vavg", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VSRT3 = new InstructionAnonymousInnerClass215(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass215 : Instruction
	{
		public InstructionAnonymousInnerClass215(int FLAG_USES_VFPU_PFXS) : base(212, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VSRT3";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVSRT3(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vsrt3", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VSGN = new InstructionAnonymousInnerClass216(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass216 : Instruction
	{
		public InstructionAnonymousInnerClass216(int FLAG_USES_VFPU_PFXS) : base(251, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


		public override sealed string name()
		{
			return "VSGN";
		}
		public override sealed string category()
		{
			return "VFPU";
		}
		public override void interpret(Processor processor, int insn)
		{
			int vd = (insn >> 0) & 127;
			int one = (insn >> 7) & 1;
			int vs = (insn >> 8) & 127;
			int two = (insn >> 15) & 1;


						processor.cpu.doVSGN(1 + one + (two << 1), vd, vs);

		}
		public override void compile(ICompilerContext context, int insn)
		{
			base.compile(context, insn);
		}
		public override string disasm(int address, int insn)
		{
			int vd = (insn >> 0) & 127;
			int one = (insn >> 7) & 1;
			int vs = (insn >> 8) & 127;
			int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vsgn", 1 + one + (two << 1), vd, vs);
		}
	}
	public static readonly Instruction VSRT4 = new InstructionAnonymousInnerClass217(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass217 : Instruction
	{
		public InstructionAnonymousInnerClass217(int FLAG_USES_VFPU_PFXS) : base(213, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VSRT4";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVSRT4(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDVS("vsrt4", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VMFVC = new InstructionAnonymousInnerClass218(FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass218 : Instruction
	{
		public InstructionAnonymousInnerClass218(UnknownType FLAG_USES_VFPU_PFXD) : base(214, FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VMFVC";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int imm7 = (insn >> 8) & 127;


					processor.cpu.doVMFVC(vd, imm7);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int imm7 = (insn >> 8) & 127;

	return "Unimplemented VMFVC imm7=" + imm7 + ", vd=" + vd;
	}
	}
	public static readonly Instruction VMTVC = new InstructionAnonymousInnerClass219();

	private class InstructionAnonymousInnerClass219 : Instruction
	{
		public InstructionAnonymousInnerClass219() : base(215)
		{
		}


	public override sealed string name()
	{
		return "VMTVC";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm7 = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;


					processor.cpu.doVMTVC(vs, imm7);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm7 = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;

	return "Unimplemented VMTVC imm7=" + imm7 + ", vs=" + vs;
	}
	}
	public static readonly Instruction VT4444 = new InstructionAnonymousInnerClass220(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass220 : Instruction
	{
		public InstructionAnonymousInnerClass220(int FLAG_USES_VFPU_PFXS) : base(216, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VT4444";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVT4444(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vt4444", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VT5551 = new InstructionAnonymousInnerClass221(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass221 : Instruction
	{
		public InstructionAnonymousInnerClass221(int FLAG_USES_VFPU_PFXS) : base(217, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VT5551";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVT5551(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vt5551", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VT5650 = new InstructionAnonymousInnerClass222(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass222 : Instruction
	{
		public InstructionAnonymousInnerClass222(int FLAG_USES_VFPU_PFXS) : base(218, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VT5650";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVT5650(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

		return Common.disasmVDVS("vt5650", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VCST = new InstructionAnonymousInnerClass223(FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass223 : Instruction
	{
		public InstructionAnonymousInnerClass223(int FLAG_USES_VFPU_PFXD) : base(219, FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VCST";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;


					processor.cpu.doVCST(1 + one + (two << 1), vd, imm5);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int imm5 = (insn >> 16) & 31;

		float constant = 0.0f;
		if (imm5 < VfpuState.floatConstants.Length)
		{
			constant = VfpuState.floatConstants[imm5];
		}

		context.startPfxCompiled();
		int vsize = context.Vsize;
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStore(n);
			context.MethodVisitor.visitLdcInsn(constant);
			context.storeVd(n);
		}
		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;

	return Common.disasmVDCST("VCST", 1 + one + (two << 1), vd, imm5);
	}
	}
	public static readonly Instruction VF2IN = new InstructionAnonymousInnerClass224(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass224 : Instruction
	{
		public InstructionAnonymousInnerClass224(int FLAG_USES_VFPU_PFXS) : base(220, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VF2IN";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;


					processor.cpu.doVF2IN(1 + one + (two << 1), vd, vs, imm5);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		int imm5 = context.Imm5;
		MethodVisitor mv = context.MethodVisitor;
		context.startPfxCompiled(false);
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStoreInt(n);
			context.loadVs(n);
			if (imm5 != 0)
			{
				context.loadImm(imm5);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "scalb", "(FI)F");
			}
			Label afterLabel = new Label();
			Label notNaNValueLabel = new Label();
			mv.visitInsn(Opcodes.DUP);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Float)), "isNaN", "(F)Z");
			mv.visitJumpInsn(Opcodes.IFEQ, notNaNValueLabel);
			mv.visitInsn(Opcodes.POP);
			context.loadImm(0x7FFFFFFF);
			mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
			mv.visitLabel(notNaNValueLabel);
			mv.visitInsn(Opcodes.F2D);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "Round", "(D)D");
			mv.visitInsn(Opcodes.D2I);
			mv.visitLabel(afterLabel);
			context.storeVdInt(n);
		}
		context.endPfxCompiled(false);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;

		return Common.disasmVDVSIMM("vf2in", 1 + one + (two << 1), vd, vs, imm5);
	}
	}
	public static readonly Instruction VF2IZ = new InstructionAnonymousInnerClass225(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass225 : Instruction
	{
		public InstructionAnonymousInnerClass225(int FLAG_USES_VFPU_PFXS) : base(221, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VF2IZ";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;


					processor.cpu.doVF2IZ(1 + one + (two << 1), vd, vs, imm5);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		int imm5 = context.Imm5;
		MethodVisitor mv = context.MethodVisitor;
		context.startPfxCompiled(false);
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStoreInt(n);
			context.loadVs(n);
			mv.visitInsn(Opcodes.DUP);
			mv.visitLdcInsn(0.0f);
			mv.visitInsn(Opcodes.FCMPG); // Use FCMPG or FCMPL? Need to check handling of NaN value
			Label negativeLabel = new Label();
			Label afterSignTestLabel = new Label();
			mv.visitJumpInsn(Opcodes.IFLT, negativeLabel);
			if (imm5 != 0)
			{
				context.loadImm(imm5);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "scalb", "(FI)F");
			}
			mv.visitInsn(Opcodes.F2D);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "floor", "(D)D");
			mv.visitJumpInsn(Opcodes.GOTO, afterSignTestLabel);
			mv.visitLabel(negativeLabel);
			if (imm5 != 0)
			{
				context.loadImm(imm5);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "scalb", "(FI)F");
			}
			mv.visitInsn(Opcodes.F2D);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "ceil", "(D)D");
			mv.visitLabel(afterSignTestLabel);

			Label afterLabel = new Label();
			Label notNaNValueLabel = new Label();
			mv.visitInsn(Opcodes.DUP2);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Double)), "isNaN", "(D)Z");
			mv.visitJumpInsn(Opcodes.IFEQ, notNaNValueLabel);
			mv.visitInsn(Opcodes.POP2);
			context.loadImm(0x7FFFFFFF);
			mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
			mv.visitLabel(notNaNValueLabel);
			mv.visitInsn(Opcodes.D2I);
			mv.visitLabel(afterLabel);
			context.storeVdInt(n);
		}
		context.endPfxCompiled(false);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;

		return Common.disasmVDVSIMM("vf2iz", 1 + one + (two << 1), vd, vs, imm5);
	}
	}
	public static readonly Instruction VF2IU = new InstructionAnonymousInnerClass226(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass226 : Instruction
	{
		public InstructionAnonymousInnerClass226(int FLAG_USES_VFPU_PFXS) : base(222, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VF2IU";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;


					processor.cpu.doVF2IU(1 + one + (two << 1), vd, vs, imm5);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		int imm5 = context.Imm5;
		MethodVisitor mv = context.MethodVisitor;
		context.startPfxCompiled(false);
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStoreInt(n);
			context.loadVs(n);
			if (imm5 != 0)
			{
				context.loadImm(imm5);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "scalb", "(FI)F");
			}
			mv.visitInsn(Opcodes.F2D);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "ceil", "(D)D");
			Label afterLabel = new Label();
			Label notNaNValueLabel = new Label();
			mv.visitInsn(Opcodes.DUP2);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Double)), "isNaN", "(D)Z");
			mv.visitJumpInsn(Opcodes.IFEQ, notNaNValueLabel);
			mv.visitInsn(Opcodes.POP2);
			context.loadImm(0x7FFFFFFF);
			mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
			mv.visitLabel(notNaNValueLabel);
			mv.visitInsn(Opcodes.D2I);
			mv.visitLabel(afterLabel);
			context.storeVdInt(n);
		}
		context.endPfxCompiled(false);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;

		return Common.disasmVDVSIMM("vf2iu", 1 + one + (two << 1), vd, vs, imm5);
	}
	}
	public static readonly Instruction VF2ID = new InstructionAnonymousInnerClass227(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass227 : Instruction
	{
		public InstructionAnonymousInnerClass227(int FLAG_USES_VFPU_PFXS) : base(223, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VF2ID";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;


					processor.cpu.doVF2ID(1 + one + (two << 1), vd, vs, imm5);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		int imm5 = context.Imm5;
		MethodVisitor mv = context.MethodVisitor;
		context.startPfxCompiled(false);
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStoreInt(n);
			context.loadVs(n);
			if (imm5 != 0)
			{
				context.loadImm(imm5);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "scalb", "(FI)F");
			}
			mv.visitInsn(Opcodes.F2D);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "floor", "(D)D");
			Label afterLabel = new Label();
			Label notNaNValueLabel = new Label();
			mv.visitInsn(Opcodes.DUP2);
			mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Double)), "isNaN", "(D)Z");
			mv.visitJumpInsn(Opcodes.IFEQ, notNaNValueLabel);
			mv.visitInsn(Opcodes.POP2);
			context.loadImm(0x7FFFFFFF);
			mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
			mv.visitLabel(notNaNValueLabel);
			mv.visitInsn(Opcodes.D2I);
			mv.visitLabel(afterLabel);
			context.storeVdInt(n);
		}
		context.endPfxCompiled(false);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;

		return Common.disasmVDVSIMM("vf2id", 1 + one + (two << 1), vd, vs, imm5);
	}
	}
	public static readonly Instruction VI2F = new InstructionAnonymousInnerClass228(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass228 : Instruction
	{
		public InstructionAnonymousInnerClass228(int FLAG_USES_VFPU_PFXS) : base(224, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VI2F";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;


					processor.cpu.doVI2F(1 + one + (two << 1), vd, vs, imm5);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		int imm5 = context.Imm5;
		MethodVisitor mv = context.MethodVisitor;
		context.startPfxCompiled();
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStore(n);
			context.loadVsInt(n);
			mv.visitInsn(Opcodes.I2F);
			if (imm5 != 0)
			{
				context.loadImm(-imm5);
				mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "scalb", "(FI)F");
			}
			context.storeVd(n);
		}
		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;

		return Common.disasmVDVSIMM("vi2f", 1 + one + (two << 1), vd, vs, imm5);
	}
	}
	public static readonly Instruction VCMOVT = new InstructionAnonymousInnerClass229(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass229 : Instruction
	{
		public InstructionAnonymousInnerClass229(int FLAG_USES_VFPU_PFXS) : base(225, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VCMOVT";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm3 = (insn >> 16) & 7;


					processor.cpu.doVCMOVT(1 + one + (two << 1), imm3, vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		int imm3 = context.Imm3;
		MethodVisitor mv = context.MethodVisitor;
		if (imm3 < 6)
		{
			context.startPfxCompiled(false);
			Label notMoveLabel = new Label();
			Label afterLabel = new Label();
			context.loadVcrCc(imm3);
			mv.visitJumpInsn(Opcodes.IFEQ, notMoveLabel);
			for (int n = 0; n < vsize; n++)
			{
				context.prepareVdForStoreInt(n);
				context.loadVsInt(n);
				context.storeVdInt(n);
			}
			context.endPfxCompiled(false);
			mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
			mv.visitLabel(notMoveLabel);
			if (context.PfxdState.Known && context.PfxdState.pfxDst.enabled)
			{
				for (int n = 0; n < vsize; n++)
				{
					context.prepareVdForStoreInt(n);
					context.loadVdInt(n);
					context.storeVdInt(n);
				}
				context.endPfxCompiled(false);
			}
			mv.visitLabel(afterLabel);
		}
		else if (imm3 == 6)
		{
			context.startPfxCompiled(false);
			for (int n = 0; n < vsize; n++)
			{
				Label notMoveLabel = new Label();
				Label afterLabel = new Label();
				context.loadVcrCc(n);
				mv.visitJumpInsn(Opcodes.IFEQ, notMoveLabel);
				context.prepareVdForStoreInt(n);
				context.loadVsInt(n);
				context.storeVdInt(n);
				mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
				mv.visitLabel(notMoveLabel);
				if (context.PfxdState.Known && context.PfxdState.pfxDst.enabled)
				{
					context.prepareVdForStoreInt(n);
					context.loadVdInt(n);
					context.storeVdInt(n);
				}
				mv.visitLabel(afterLabel);
			}
			context.endPfxCompiled(false);
		}
		else
		{
			// Never copy
		}
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm3 = (insn >> 16) & 7;

		return Common.disasmVDVSIMM("VCMOVT", 1 + one + (two << 1), vd, vs, imm3);
	}
	}
	public static readonly Instruction VCMOVF = new InstructionAnonymousInnerClass230(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass230 : Instruction
	{
		public InstructionAnonymousInnerClass230(int FLAG_USES_VFPU_PFXS) : base(226, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VCMOVF";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm3 = (insn >> 16) & 7;


					processor.cpu.doVCMOVF(1 + one + (two << 1), imm3, vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		int imm3 = context.Imm3;
		MethodVisitor mv = context.MethodVisitor;
		context.startPfxCompiled(false);
		if (imm3 < 6)
		{
			Label notMoveLabel = new Label();
			Label afterLabel = new Label();
			context.loadVcrCc(imm3);
			mv.visitJumpInsn(Opcodes.IFNE, notMoveLabel);
			for (int n = 0; n < vsize; n++)
			{
				context.prepareVdForStoreInt(n);
				context.loadVsInt(n);
				context.storeVdInt(n);
			}
			mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
			mv.visitLabel(notMoveLabel);
			if (context.PfxdState.Known && context.PfxdState.pfxDst.enabled)
			{
				for (int n = 0; n < vsize; n++)
				{
					context.prepareVdForStoreInt(n);
					context.loadVdInt(n);
					context.storeVdInt(n);
				}
			}
			mv.visitLabel(afterLabel);
		}
		else if (imm3 == 6)
		{
			for (int n = 0; n < vsize; n++)
			{
				Label notMoveLabel = new Label();
				Label afterLabel = new Label();
				context.loadVcrCc(n);
				mv.visitJumpInsn(Opcodes.IFNE, notMoveLabel);
				context.prepareVdForStoreInt(n);
				context.loadVsInt(n);
				context.storeVdInt(n);
				mv.visitJumpInsn(Opcodes.GOTO, afterLabel);
				mv.visitLabel(notMoveLabel);
				if (context.PfxdState.Known && context.PfxdState.pfxDst.enabled)
				{
					context.prepareVdForStoreInt(n);
					context.loadVdInt(n);
					context.storeVdInt(n);
				}
				mv.visitLabel(afterLabel);
			}
		}
		else
		{
			// Always copy
			for (int n = 0; n < vsize; n++)
			{
				context.prepareVdForStoreInt(n);
				context.loadVsInt(n);
				context.storeVdInt(n);
			}
		}
		context.endPfxCompiled(false);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm3 = (insn >> 16) & 7;

		return Common.disasmVDVSIMM("VCMOVF", 1 + one + (two << 1), vd, vs, imm3);
	}
	}
	public static readonly Instruction VWBN = new InstructionAnonymousInnerClass231();

	private class InstructionAnonymousInnerClass231 : Instruction
	{
		public InstructionAnonymousInnerClass231() : base(227)
		{
		}


	public override sealed string name()
	{
		return "VWBN";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm8 = (insn >> 16) & 255;


					processor.cpu.doVWBN(1 + one + (two << 1), vd, vs, imm8);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm8 = (insn >> 16) & 255;

		return Common.disasmVDVSIMM("VWBN", 1 + one + (two << 1), vd, vs, imm8);
	}
	}
	public static readonly Instruction VPFXS = new InstructionAnonymousInnerClass232();

	private class InstructionAnonymousInnerClass232 : Instruction
	{
		public InstructionAnonymousInnerClass232() : base(228)
		{
		}


	public override sealed string name()
	{
		return "VPFXS";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int swzx = (insn >> 0) & 3;
		int swzy = (insn >> 2) & 3;
		int swzz = (insn >> 4) & 3;
		int swzw = (insn >> 6) & 3;
		int absx = (insn >> 8) & 1;
		int absy = (insn >> 9) & 1;
		int absz = (insn >> 10) & 1;
		int absw = (insn >> 11) & 1;
		int cstx = (insn >> 12) & 1;
		int csty = (insn >> 13) & 1;
		int cstz = (insn >> 14) & 1;
		int cstw = (insn >> 15) & 1;
		int negx = (insn >> 16) & 1;
		int negy = (insn >> 17) & 1;
		int negz = (insn >> 18) & 1;
		int negw = (insn >> 19) & 1;


					processor.cpu.doVPFXS(negw, negz, negy, negx, cstw, cstz, csty, cstx, absw, absz, absy, absx, swzw, swzz, swzy, swzx);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (context.isPfxConsumed(FLAG_USES_VFPU_PFXS))
		{
			context.PfxsState.Known = true;
			PfxSrc pfxSrc = context.PfxsState.pfxSrc;
			pfxSrc.swz[0] = (insn >> 0) & 3;
			pfxSrc.swz[1] = (insn >> 2) & 3;
			pfxSrc.swz[2] = (insn >> 4) & 3;
			pfxSrc.swz[3] = (insn >> 6) & 3;
			pfxSrc.abs[0] = ((insn >> 8) & 1) != 0;
			pfxSrc.abs[1] = ((insn >> 9) & 1) != 0;
			pfxSrc.abs[2] = ((insn >> 10) & 1) != 0;
			pfxSrc.abs[3] = ((insn >> 11) & 1) != 0;
			pfxSrc.cst[0] = ((insn >> 12) & 1) != 0;
			pfxSrc.cst[1] = ((insn >> 13) & 1) != 0;
			pfxSrc.cst[2] = ((insn >> 14) & 1) != 0;
			pfxSrc.cst[3] = ((insn >> 15) & 1) != 0;
			pfxSrc.neg[0] = ((insn >> 16) & 1) != 0;
			pfxSrc.neg[1] = ((insn >> 17) & 1) != 0;
			pfxSrc.neg[2] = ((insn >> 18) & 1) != 0;
			pfxSrc.neg[3] = ((insn >> 19) & 1) != 0;
			pfxSrc.enabled = true;
		}
		else
		{
			context.PfxsState.Known = false;
			context.compileInterpreterInstruction();
		}
	}
	public override string disasm(int address, int insn)
	{
		int swzx = (insn >> 0) & 3;
		int swzy = (insn >> 2) & 3;
		int swzz = (insn >> 4) & 3;
		int swzw = (insn >> 6) & 3;
		int absx = (insn >> 8) & 1;
		int absy = (insn >> 9) & 1;
		int absz = (insn >> 10) & 1;
		int absw = (insn >> 11) & 1;
		int cstx = (insn >> 12) & 1;
		int csty = (insn >> 13) & 1;
		int cstz = (insn >> 14) & 1;
		int cstw = (insn >> 15) & 1;
		int negx = (insn >> 16) & 1;
		int negy = (insn >> 17) & 1;
		int negz = (insn >> 18) & 1;
		int negw = (insn >> 19) & 1;

		int[] swz = new int[4];
		bool[] abs, cst, neg;
		abs = new bool[4];
		cst = new bool[4];
		neg = new bool[4];

		swz[0] = swzx;
		swz[1] = swzy;
		swz[2] = swzz;
		swz[3] = swzw;
		abs[0] = absx != 0;
		abs[1] = absy != 0;
		abs[2] = absz != 0;
		abs[3] = absw != 0;
		cst[0] = cstx != 0;
		cst[1] = csty != 0;
		cst[2] = cstz != 0;
		cst[3] = cstw != 0;
		neg[0] = negx != 0;
		neg[1] = negy != 0;
		neg[2] = negz != 0;
		neg[3] = negw != 0;

	return Common.disasmVPFX("VPFXS", swz, abs, cst, neg);
	}
	}
	public static readonly Instruction VPFXT = new InstructionAnonymousInnerClass233();

	private class InstructionAnonymousInnerClass233 : Instruction
	{
		public InstructionAnonymousInnerClass233() : base(229)
		{
		}


	public override sealed string name()
	{
		return "VPFXT";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int swzx = (insn >> 0) & 3;
		int swzy = (insn >> 2) & 3;
		int swzz = (insn >> 4) & 3;
		int swzw = (insn >> 6) & 3;
		int absx = (insn >> 8) & 1;
		int absy = (insn >> 9) & 1;
		int absz = (insn >> 10) & 1;
		int absw = (insn >> 11) & 1;
		int cstx = (insn >> 12) & 1;
		int csty = (insn >> 13) & 1;
		int cstz = (insn >> 14) & 1;
		int cstw = (insn >> 15) & 1;
		int negx = (insn >> 16) & 1;
		int negy = (insn >> 17) & 1;
		int negz = (insn >> 18) & 1;
		int negw = (insn >> 19) & 1;


					processor.cpu.doVPFXT(negw, negz, negy, negx, cstw, cstz, csty, cstx, absw, absz, absy, absx, swzw, swzz, swzy, swzx);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (context.isPfxConsumed(FLAG_USES_VFPU_PFXT))
		{
			context.PfxtState.Known = true;
			PfxSrc pfxSrc = context.PfxtState.pfxSrc;
			pfxSrc.swz[0] = (insn >> 0) & 3;
			pfxSrc.swz[1] = (insn >> 2) & 3;
			pfxSrc.swz[2] = (insn >> 4) & 3;
			pfxSrc.swz[3] = (insn >> 6) & 3;
			pfxSrc.abs[0] = ((insn >> 8) & 1) != 0;
			pfxSrc.abs[1] = ((insn >> 9) & 1) != 0;
			pfxSrc.abs[2] = ((insn >> 10) & 1) != 0;
			pfxSrc.abs[3] = ((insn >> 11) & 1) != 0;
			pfxSrc.cst[0] = ((insn >> 12) & 1) != 0;
			pfxSrc.cst[1] = ((insn >> 13) & 1) != 0;
			pfxSrc.cst[2] = ((insn >> 14) & 1) != 0;
			pfxSrc.cst[3] = ((insn >> 15) & 1) != 0;
			pfxSrc.neg[0] = ((insn >> 16) & 1) != 0;
			pfxSrc.neg[1] = ((insn >> 17) & 1) != 0;
			pfxSrc.neg[2] = ((insn >> 18) & 1) != 0;
			pfxSrc.neg[3] = ((insn >> 19) & 1) != 0;
			pfxSrc.enabled = true;
		}
		else
		{
			context.PfxtState.Known = false;
			context.compileInterpreterInstruction();
		}
	}
	public override string disasm(int address, int insn)
	{
		int swzx = (insn >> 0) & 3;
		int swzy = (insn >> 2) & 3;
		int swzz = (insn >> 4) & 3;
		int swzw = (insn >> 6) & 3;
		int absx = (insn >> 8) & 1;
		int absy = (insn >> 9) & 1;
		int absz = (insn >> 10) & 1;
		int absw = (insn >> 11) & 1;
		int cstx = (insn >> 12) & 1;
		int csty = (insn >> 13) & 1;
		int cstz = (insn >> 14) & 1;
		int cstw = (insn >> 15) & 1;
		int negx = (insn >> 16) & 1;
		int negy = (insn >> 17) & 1;
		int negz = (insn >> 18) & 1;
		int negw = (insn >> 19) & 1;

		int[] swz = new int[4];
		bool[] abs, cst, neg;
		abs = new bool[4];
		cst = new bool[4];
		neg = new bool[4];

		swz[0] = swzx;
		swz[1] = swzy;
		swz[2] = swzz;
		swz[3] = swzw;
		abs[0] = absx != 0;
		abs[1] = absy != 0;
		abs[2] = absz != 0;
		abs[3] = absw != 0;
		cst[0] = cstx != 0;
		cst[1] = csty != 0;
		cst[2] = cstz != 0;
		cst[3] = cstw != 0;
		neg[0] = negx != 0;
		neg[1] = negy != 0;
		neg[2] = negz != 0;
		neg[3] = negw != 0;

	return Common.disasmVPFX("VPFXT", swz, abs, cst, neg);
	}
	}
	public static readonly Instruction VPFXD = new InstructionAnonymousInnerClass234();

	private class InstructionAnonymousInnerClass234 : Instruction
	{
		public InstructionAnonymousInnerClass234() : base(230)
		{
		}


	public override sealed string name()
	{
		return "VPFXD";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int satx = (insn >> 0) & 3;
		int saty = (insn >> 2) & 3;
		int satz = (insn >> 4) & 3;
		int satw = (insn >> 6) & 3;
		int mskx = (insn >> 8) & 1;
		int msky = (insn >> 9) & 1;
		int mskz = (insn >> 10) & 1;
		int mskw = (insn >> 11) & 1;


					processor.cpu.doVPFXD(mskw, mskz, msky, mskx, satw, satz, saty, satx);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		if (context.isPfxConsumed(FLAG_USES_VFPU_PFXD))
		{
			context.PfxdState.Known = true;
			PfxDst pfxDst = context.PfxdState.pfxDst;
			pfxDst.sat[0] = (insn >> 0) & 3;
			pfxDst.sat[1] = (insn >> 2) & 3;
			pfxDst.sat[2] = (insn >> 4) & 3;
			pfxDst.sat[3] = (insn >> 6) & 3;
			pfxDst.msk[0] = ((insn >> 8) & 1) != 0;
			pfxDst.msk[1] = ((insn >> 9) & 1) != 0;
			pfxDst.msk[2] = ((insn >> 10) & 1) != 0;
			pfxDst.msk[3] = ((insn >> 11) & 1) != 0;
			pfxDst.enabled = true;
		}
		else
		{
			context.PfxdState.Known = false;
			context.compileInterpreterInstruction();
		}
	}
	public override string disasm(int address, int insn)
	{
		int satx = (insn >> 0) & 3;
		int saty = (insn >> 2) & 3;
		int satz = (insn >> 4) & 3;
		int satw = (insn >> 6) & 3;
		int mskx = (insn >> 8) & 1;
		int msky = (insn >> 9) & 1;
		int mskz = (insn >> 10) & 1;
		int mskw = (insn >> 11) & 1;

		int[] sat, msk;
		sat = new int[4];
		msk = new int[4];
		sat[0] = satx;
		sat[1] = saty;
		sat[2] = satz;
		sat[3] = satw;
		msk[0] = mskx;
		msk[1] = msky;
		msk[2] = mskz;
		msk[3] = mskw;

	return Common.disasmVPFXD("VPFXD", sat, msk);
	}
	}
	public static readonly Instruction VIIM = new InstructionAnonymousInnerClass235(FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass235 : Instruction
	{
		public InstructionAnonymousInnerClass235(int FLAG_USES_VFPU_PFXD) : base(231, FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VIIM";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int vd = (insn >> 16) & 127;


					processor.cpu.doVIIM(vd, (int)(short) imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		const int vsize = 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vd = context.getVtRegisterIndex();
		int vd = context.VtRegisterIndex; // vd index stored as vt
		int simm16 = context.getImm16(true);
		context.startPfxCompiled();
		context.prepareVdForStore(vsize, vd, 0);
		context.MethodVisitor.visitLdcInsn((float) simm16);
		context.storeVd(vsize, vd, 0);
		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int vd = (insn >> 16) & 127;

	return Common.disasmVDIIM("VIIM", 1, vd, imm16);
	}
	}
	public static readonly Instruction VFIM = new InstructionAnonymousInnerClass236(FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass236 : Instruction
	{
		public InstructionAnonymousInnerClass236(int FLAG_USES_VFPU_PFXD) : base(232, FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VFIM";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int vd = (insn >> 16) & 127;


					processor.cpu.doVFIM(vd, imm16);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		const int vsize = 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vd = context.getVtRegisterIndex();
		int vd = context.VtRegisterIndex; // vd index stored as vt
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int imm16 = context.getImm16(false);
		int imm16 = context.getImm16(false);
		int value = VfpuState.halffloatToFloat(imm16);
		context.startPfxCompiled();
		context.prepareVdForStoreInt(vsize, vd, 0);
		context.loadImm(value);
		context.storeVdInt(vsize, vd, 0);
		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int vd = (insn >> 16) & 127;

	return Common.disasmVDFIM("VFIM", 1, vd, imm16);
	}
	}
	public static readonly Instruction VMMUL = new InstructionAnonymousInnerClass237(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass237 : Instruction
	{
		public InstructionAnonymousInnerClass237(int FLAG_USES_VFPU_PFXS) : base(233, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VMMUL";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVMMUL(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		int vsize = context.Vsize;
		if (vsize > 1)
		{
			context.startPfxCompiled();
			int vs = context.VsRegisterIndex;
			int vt = context.VtRegisterIndex;
			int vd = context.VdRegisterIndex;
			for (int i = 0; i < vsize; i++)
			{
				for (int j = 0; j < vsize; j++)
				{
					context.prepareVdForStore(vsize, vd + i, j);
					context.loadVs(vsize, vs + j, 0);
					context.loadVt(vsize, vt + i, 0);
					context.MethodVisitor.visitInsn(Opcodes.FMUL);
					for (int n = 1; n < vsize; n++)
					{
						context.loadVs(vsize, vs + j, n);
						context.loadVt(vsize, vt + i, n);
						context.MethodVisitor.visitInsn(Opcodes.FMUL);
						context.MethodVisitor.visitInsn(Opcodes.FADD);
					}
					context.storeVd(vsize, vd + i, j);
				}
				context.flushPfxCompiled(vsize, vd + i, true);
			}
			context.endPfxCompiled(vsize, true, false);
		}
		else
		{
			// Unsupported VMMUL.S
			context.compileInterpreterInstruction();
		}
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDMVSMVTM("VMMUL", 1 + one + (two << 1), vd, vs ^ 32, vt);
	}
	}
	public static readonly Instruction VHTFM2 = new InstructionAnonymousInnerClass238(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass238 : Instruction
	{
		public InstructionAnonymousInnerClass238(int FLAG_USES_VFPU_PFXS) : base(234, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VHTFM2";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVHTFM2(vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;

		return Common.disasmVDVSMVT("VHTFM2", 2, vd, vs, vt);
	}
	}
	public static readonly Instruction VTFM2 = new InstructionAnonymousInnerClass239(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass239 : Instruction
	{
		public InstructionAnonymousInnerClass239(int FLAG_USES_VFPU_PFXS) : base(235, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VTFM2";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVTFM2(vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;

		return Common.disasmVDVSMVT("VTFM2", 2, vd, vs, vt);
	}
	}
	public static readonly Instruction VHTFM3 = new InstructionAnonymousInnerClass240(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass240 : Instruction
	{
		public InstructionAnonymousInnerClass240(int FLAG_USES_VFPU_PFXS) : base(236, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VHTFM3";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVHTFM3(vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;

		return Common.disasmVDVSMVT("VHTFM3", 3, vd, vs, vt);
	}
	}
	public static readonly Instruction VTFM3 = new InstructionAnonymousInnerClass241(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass241 : Instruction
	{
		public InstructionAnonymousInnerClass241(int FLAG_USES_VFPU_PFXS) : base(237, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VTFM3";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVTFM3(vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		const int vsize = 3;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vs = context.getVsRegisterIndex();
		int vs = context.VsRegisterIndex;
		MethodVisitor mv = context.MethodVisitor;
		context.loadVt(vsize, 0);
		context.storeFTmp1();
		context.loadVt(vsize, 1);
		context.storeFTmp2();
		context.loadVt(vsize, 2);
		context.storeFTmp3();
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStore(vsize, n);
			context.loadVs(vsize, vs + n, 0);
			context.loadFTmp1();
			mv.visitInsn(Opcodes.FMUL);
			context.loadVs(vsize, vs + n, 1);
			context.loadFTmp2();
			mv.visitInsn(Opcodes.FMUL);
			mv.visitInsn(Opcodes.FADD);
			context.loadVs(vsize, vs + n, 2);
			context.loadFTmp3();
			mv.visitInsn(Opcodes.FMUL);
			mv.visitInsn(Opcodes.FADD);
			context.storeVd(vsize, n);
		}
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;

		return Common.disasmVDVSMVT("VTFM3", 3, vd, vs, vt);
	}
	}
	public static readonly Instruction VHTFM4 = new InstructionAnonymousInnerClass242(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass242 : Instruction
	{
		public InstructionAnonymousInnerClass242(int FLAG_USES_VFPU_PFXS) : base(238, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VHTFM4";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVHTFM4(vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		const int vsize = 4;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vs = context.getVsRegisterIndex();
		int vs = context.VsRegisterIndex;
		MethodVisitor mv = context.MethodVisitor;
		context.loadVt(3, 0);
		context.storeFTmp1();
		context.loadVt(3, 1);
		context.storeFTmp2();
		context.loadVt(3, 2);
		context.storeFTmp3();
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStore(vsize, n);
			context.loadVs(vsize, vs + n, 0);
			context.loadFTmp1();
			mv.visitInsn(Opcodes.FMUL);
			context.loadVs(vsize, vs + n, 1);
			context.loadFTmp2();
			mv.visitInsn(Opcodes.FMUL);
			mv.visitInsn(Opcodes.FADD);
			context.loadVs(vsize, vs + n, 2);
			context.loadFTmp3();
			mv.visitInsn(Opcodes.FMUL);
			mv.visitInsn(Opcodes.FADD);
			context.loadVs(vsize, vs + n, 3);
			mv.visitInsn(Opcodes.FADD);
			context.storeVd(vsize, n);
		}
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;

		return Common.disasmVDVSMVT("VHTFM4", 4, vd, vs, vt);
	}
	}
	public static readonly Instruction VTFM4 = new InstructionAnonymousInnerClass243(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass243 : Instruction
	{
		public InstructionAnonymousInnerClass243(int FLAG_USES_VFPU_PFXS) : base(239, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VTFM4";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVTFM4(vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		const int vsize = 4;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vs = context.getVsRegisterIndex();
		int vs = context.VsRegisterIndex;
		MethodVisitor mv = context.MethodVisitor;
		context.loadVt(vsize, 0);
		context.storeFTmp1();
		context.loadVt(vsize, 1);
		context.storeFTmp2();
		context.loadVt(vsize, 2);
		context.storeFTmp3();
		context.loadVt(vsize, 3);
		context.storeFTmp4();
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStore(vsize, n);
			context.loadVs(vsize, vs + n, 0);
			context.loadFTmp1();
			mv.visitInsn(Opcodes.FMUL);
			context.loadVs(vsize, vs + n, 1);
			context.loadFTmp2();
			mv.visitInsn(Opcodes.FMUL);
			mv.visitInsn(Opcodes.FADD);
			context.loadVs(vsize, vs + n, 2);
			context.loadFTmp3();
			mv.visitInsn(Opcodes.FMUL);
			mv.visitInsn(Opcodes.FADD);
			context.loadVs(vsize, vs + n, 3);
			context.loadFTmp4();
			mv.visitInsn(Opcodes.FMUL);
			mv.visitInsn(Opcodes.FADD);
			context.storeVd(vsize, n);
		}
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;

		return Common.disasmVDVSMVT("VTFM4", 4, vd, vs, vt);
	}
	}
	public static readonly Instruction VMSCL = new InstructionAnonymousInnerClass244(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass244 : Instruction
	{
		public InstructionAnonymousInnerClass244(int FLAG_USES_VFPU_PFXS) : base(240, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VMSCL";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVMSCL(1 + one + (two << 1), vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vsize = context.getVsize();
		int vsize = context.Vsize;
		if (vsize > 1)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vs = context.getVsRegisterIndex();
			int vs = context.VsRegisterIndex;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vd = context.getVdRegisterIndex();
			int vd = context.VdRegisterIndex;
			context.startPfxCompiled();
			context.loadVt(1, 0);
			context.storeFTmp1();
			for (int i = 0; i < vsize; i++)
			{
				for (int n = 0; n < vsize; n++)
				{
					context.prepareVdForStore(vsize, vd + i, n);
					context.loadVs(vsize, vs + i, n);
					context.loadFTmp1();
					context.MethodVisitor.visitInsn(Opcodes.FMUL);
					context.storeVd(vsize, vd + i, n);
				}
				context.flushPfxCompiled(vsize, vd + i, true);
			}
			context.endPfxCompiled(vsize, true, false);
		}
		else
		{
			// Unsupported VMSCL.S
			context.compileInterpreterInstruction();
		}
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int vt = (insn >> 16) & 127;

		return Common.disasmVDVSVT("vmscl", 1 + one + (two << 1), vd, vs, vt);
	}
	}
	public static readonly Instruction VCRSP = new InstructionAnonymousInnerClass245(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass245 : Instruction
	{
		public InstructionAnonymousInnerClass245(int FLAG_USES_VFPU_PFXS) : base(241, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VCRSP";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVCRSP(vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.startPfxCompiled();
		const int vsize = 3;
		MethodVisitor mv = context.MethodVisitor;

		// v3[0] = +v1[1] * v2[2] - v1[2] * v2[1];
		context.prepareVdForStore(vsize, 0);
		context.loadVs(vsize, 1);
		context.loadVt(vsize, 2);
		mv.visitInsn(Opcodes.FMUL);
		context.loadVs(vsize, 2);
		context.loadVt(vsize, 1);
		mv.visitInsn(Opcodes.FMUL);
		mv.visitInsn(Opcodes.FSUB);
		context.storeVd(vsize, 0);

		// v3[1] = +v1[2] * v2[0] - v1[0] * v2[2];
		context.prepareVdForStore(vsize, 1);
		context.loadVs(vsize, 2);
		context.loadVt(vsize, 0);
		mv.visitInsn(Opcodes.FMUL);
		context.loadVs(vsize, 0);
		context.loadVt(vsize, 2);
		mv.visitInsn(Opcodes.FMUL);
		mv.visitInsn(Opcodes.FSUB);
		context.storeVd(vsize, 1);

		// v3[2] = +v1[0] * v2[1] - v1[1] * v2[0];
		context.prepareVdForStore(vsize, 2);
		context.loadVs(vsize, 0);
		context.loadVt(vsize, 1);
		mv.visitInsn(Opcodes.FMUL);
		context.loadVs(vsize, 1);
		context.loadVt(vsize, 0);
		mv.visitInsn(Opcodes.FMUL);
		mv.visitInsn(Opcodes.FSUB);
		context.storeVd(vsize, 2);

		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT("vcrsp", 3, vd, vs, vt);
	}
	}
	public static readonly Instruction VQMUL = new InstructionAnonymousInnerClass246(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD);

	private class InstructionAnonymousInnerClass246 : Instruction
	{
		public InstructionAnonymousInnerClass246(int FLAG_USES_VFPU_PFXS) : base(242, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXT | FLAG_USES_VFPU_PFXD)
		{
		}


	public override sealed string name()
	{
		return "VQMUL";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;


					processor.cpu.doVQMUL(vd, vs, vt);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int vs = (insn >> 8) & 127;
		int vt = (insn >> 16) & 127;

	return Common.disasmVDVSVT("VQMUL", 4, vd, vs, vt);
	}
	}
	public static readonly Instruction VMMOV = new InstructionAnonymousInnerClass247(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass247 : Instruction
	{
		public InstructionAnonymousInnerClass247(int FLAG_USES_VFPU_PFXS) : base(243, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VMMOV";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;


					processor.cpu.doVMMOV(1 + one + (two << 1), vd, vs);

	}
	public override void compile(ICompilerContext context, int insn)
	{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vsize = context.getVsize();
		int vsize = context.Vsize;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vd = context.getVdRegisterIndex();
		int vd = context.VdRegisterIndex;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int vs = context.getVsRegisterIndex();
		int vs = context.VsRegisterIndex;
		if (vsize == 4 && ((vd | vs) & (32 | 64)) == 0 && context.hasNoPfx())
		{
			// Simple case which can be implemented using an array copy.
			int vsVprIndex = VfpuState.getVprIndex((vs >> 2) & 7, vs & 3, 0);
			int vdVprIndex = VfpuState.getVprIndex((vd >> 2) & 7, vd & 3, 0);

			context.loadVprInt();
			context.loadImm(vsVprIndex);
			context.loadVprInt();
			context.loadImm(vdVprIndex);
			context.loadImm(16);
			context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(System)), "arraycopy", arraycopyDescriptor);

			context.loadVprFloat();
			context.loadImm(vsVprIndex);
			context.loadVprFloat();
			context.loadImm(vdVprIndex);
			context.loadImm(16);
			context.MethodVisitor.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(System)), "arraycopy", arraycopyDescriptor);
		}
		else
		{
			context.startPfxCompiled(false);
			for (int i = 0; i < vsize; i++)
			{
				for (int n = 0; n < vsize; n++)
				{
					context.prepareVdForStoreInt(vsize, vd + i, n);
					context.loadVsInt(vsize, vs + i, n);
					context.storeVdInt(vsize, vd + i, n);
				}
				context.flushPfxCompiled(vsize, vd + i, false);
			}
			context.endPfxCompiled(vsize, false, false);
		}
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;

	return Common.disasmVDMVSM("VMMOV", 1 + one + (two << 1), vd, vs);
	}
	}
	public static readonly Instruction VMIDT = new InstructionAnonymousInnerClass248(FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass248 : Instruction
	{
		public InstructionAnonymousInnerClass248(int FLAG_USES_VFPU_PFXD) : base(244, FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VMIDT";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;


					processor.cpu.doVMIDT(1 + one + (two << 1), vd);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.startPfxCompiled();
		int vsize = context.Vsize;
		int vd = context.VdRegisterIndex;
		for (int i = 0; i < vsize; i++)
		{
			for (int n = 0; n < vsize; n++)
			{
				context.prepareVdForStore(vsize, vd + i, n);
				context.MethodVisitor.visitInsn(i == n ? Opcodes.FCONST_1 : Opcodes.FCONST_0);
				context.storeVd(vsize, vd + i, n);
			}
			context.flushPfxCompiled(vsize, vd + i, true);
		}
		context.endPfxCompiled(vsize, true, false);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;

		return Common.disasmVDM("VMIDT", 1 + one + (two << 1), vd);
	}
	}
	public static readonly Instruction VMZERO = new InstructionAnonymousInnerClass249(FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass249 : Instruction
	{
		public InstructionAnonymousInnerClass249(int FLAG_USES_VFPU_PFXD) : base(245, FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VMZERO";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;


					processor.cpu.doVMZERO(1 + one + (two << 1), vd);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.startPfxCompiled();
		int vsize = context.Vsize;
		int vd = context.VdRegisterIndex;
		for (int i = 0; i < vsize; i++)
		{
			for (int n = 0; n < vsize; n++)
			{
				context.prepareVdForStore(vsize, vd + i, n);
				context.MethodVisitor.visitInsn(Opcodes.FCONST_0);
				context.storeVd(vsize, vd + i, n);
			}
			context.flushPfxCompiled(vsize, vd + i, true);
		}
		context.endPfxCompiled(vsize, true, false);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;

	return Common.disasmVDM("VMZERO", 1 + one + (two << 1), vd);
	}
	}
	public static readonly Instruction VMONE = new InstructionAnonymousInnerClass250(FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass250 : Instruction
	{
		public InstructionAnonymousInnerClass250(int FLAG_USES_VFPU_PFXD) : base(246, FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VMONE";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;


					processor.cpu.doVMONE(1 + one + (two << 1), vd);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.startPfxCompiled();
		int vsize = context.Vsize;
		int vd = context.VdRegisterIndex;
		for (int i = 0; i < vsize; i++)
		{
			for (int n = 0; n < vsize; n++)
			{
				context.prepareVdForStore(vsize, vd + i, n);
				context.MethodVisitor.visitInsn(Opcodes.FCONST_1);
				context.storeVd(vsize, vd + i, n);
			}
			context.flushPfxCompiled(vsize, vd + i, true);
		}
		context.endPfxCompiled(vsize, true, false);
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int two = (insn >> 15) & 1;

	return Common.disasmVDM("VMONE", 1 + one + (two << 1), vd);
	}
	}
	public static readonly Instruction VROT = new InstructionAnonymousInnerClass251(FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX);

	private class InstructionAnonymousInnerClass251 : Instruction
	{
		public InstructionAnonymousInnerClass251(int FLAG_USES_VFPU_PFXS) : base(247, FLAG_USES_VFPU_PFXS | FLAG_USES_VFPU_PFXD | FLAG_COMPILED_PFX)
		{
		}


	public override sealed string name()
	{
		return "VROT";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;


					processor.cpu.doVROT(1 + one + (two << 1), vd, vs, imm5);

	}
	public override void compile(ICompilerContext context, int insn)
	{
		context.startPfxCompiled();
		int vsize = context.Vsize;
		int imm5 = context.Imm5;
		int si = ((int)((uint)imm5 >> 2)) & 3;
		int ci = ((int)((uint)imm5 >> 0)) & 3;
		MethodVisitor mv = context.MethodVisitor;

		Label isNotZero = new Label();
		Label isNotOne = new Label();
		Label isNotTwo = new Label();
		Label computeAngle = new Label();
		Label computeResult = new Label();

		context.loadVs(1, 0);
		// Reduce the angle to [0..4[
		mv.visitInsn(Opcodes.DUP);
		mv.visitLdcInsn(0.25f);
		mv.visitInsn(Opcodes.FMUL);
		mv.visitInsn(Opcodes.F2D);
		mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "floor", "(D)D");
		mv.visitInsn(Opcodes.D2F);
		mv.visitLdcInsn(4f);
		mv.visitInsn(Opcodes.FMUL);
		mv.visitInsn(Opcodes.FSUB);
		// Special case 0.0
		mv.visitInsn(Opcodes.DUP);
		mv.visitInsn(Opcodes.FCONST_0);
		mv.visitInsn(Opcodes.FCMPG);
		mv.visitJumpInsn(Opcodes.IFNE, isNotZero);
		mv.visitInsn(Opcodes.POP);
		mv.visitInsn(Opcodes.FCONST_1);
		context.storeFTmp1();
		mv.visitInsn(Opcodes.FCONST_0);
		context.storeFTmp2();
		mv.visitJumpInsn(Opcodes.GOTO, computeResult);
		// Special case 1.0
		mv.visitLabel(isNotZero);
		mv.visitInsn(Opcodes.DUP);
		mv.visitInsn(Opcodes.FCONST_1);
		mv.visitInsn(Opcodes.FCMPG);
		mv.visitJumpInsn(Opcodes.IFNE, isNotOne);
		mv.visitInsn(Opcodes.POP);
		mv.visitInsn(Opcodes.FCONST_0);
		context.storeFTmp1();
		mv.visitInsn(Opcodes.FCONST_1);
		if ((imm5 & 16) != 0)
		{
			mv.visitInsn(Opcodes.FNEG);
		}
		context.storeFTmp2();
		mv.visitJumpInsn(Opcodes.GOTO, computeResult);
		// Special case 2.0
		mv.visitLabel(isNotOne);
		mv.visitInsn(Opcodes.DUP);
		mv.visitInsn(Opcodes.FCONST_2);
		mv.visitInsn(Opcodes.FCMPG);
		mv.visitJumpInsn(Opcodes.IFNE, isNotTwo);
		mv.visitInsn(Opcodes.POP);
		mv.visitInsn(Opcodes.FCONST_1);
		mv.visitInsn(Opcodes.FNEG);
		context.storeFTmp1();
		mv.visitInsn(Opcodes.FCONST_0);
		context.storeFTmp2();
		mv.visitJumpInsn(Opcodes.GOTO, computeResult);
		// Special case 3.0
		mv.visitLabel(isNotTwo);
		mv.visitInsn(Opcodes.DUP);
		mv.visitLdcInsn(3f);
		mv.visitInsn(Opcodes.FCMPG);
		mv.visitJumpInsn(Opcodes.IFNE, computeAngle);
		mv.visitInsn(Opcodes.POP);
		mv.visitInsn(Opcodes.FCONST_0);
		context.storeFTmp1();
		mv.visitInsn(Opcodes.FCONST_1);
		if ((imm5 & 16) == 0)
		{
			mv.visitInsn(Opcodes.FNEG);
		}
		context.storeFTmp2();
		mv.visitJumpInsn(Opcodes.GOTO, computeResult);
		// General case
		mv.visitLabel(computeAngle);
		mv.visitInsn(Opcodes.F2D);
		mv.visitLdcInsn(Math.PI * 0.5);
		mv.visitInsn(Opcodes.DMUL);

		// Compute cos(angle)
		mv.visitInsn(Opcodes.DUP2);
		mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "cos", "(D)D");
		mv.visitInsn(Opcodes.D2F);
		context.storeFTmp1();

		// Compute sin(angle)
		mv.visitMethodInsn(Opcodes.INVOKESTATIC, Type.getInternalName(typeof(Math)), "sin", "(D)D");
		mv.visitInsn(Opcodes.D2F);
		if ((imm5 & 16) != 0)
		{
			mv.visitInsn(Opcodes.FNEG);
		}
		context.storeFTmp2();

		mv.visitLabel(computeResult);
		for (int n = 0; n < vsize; n++)
		{
			context.prepareVdForStore(n);
			if (n == ci)
			{
				context.loadFTmp1();
			}
			else if (si == ci || n == si)
			{
				context.loadFTmp2();
			}
			else
			{
				mv.visitInsn(Opcodes.FCONST_0);
			}
			context.storeVd(n);
		}
		context.endPfxCompiled();
	}
	public override string disasm(int address, int insn)
	{
		int vd = (insn >> 0) & 127;
		int one = (insn >> 7) & 1;
		int vs = (insn >> 8) & 127;
		int two = (insn >> 15) & 1;
		int imm5 = (insn >> 16) & 31;

	return Common.disasmVROT("VROT", 1 + one + (two << 1), vd, vs, imm5);
	}
	}
	public static readonly Instruction VNOP = new InstructionAnonymousInnerClass252();

	private class InstructionAnonymousInnerClass252 : Instruction
	{
		public InstructionAnonymousInnerClass252() : base(248)
		{
		}


	public override sealed string name()
	{
		return "VNOP";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{



	}
	public override void compile(ICompilerContext context, int insn)
	{
	}
	public override string disasm(int address, int insn)
	{

	return "vnop";
	}
	}
	public static readonly Instruction VFLUSH = new InstructionAnonymousInnerClass253();

	private class InstructionAnonymousInnerClass253 : Instruction
	{
		public InstructionAnonymousInnerClass253() : base(249)
		{
		}


	public override sealed string name()
	{
		return "VFLUSH";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{



	}
	public override void compile(ICompilerContext context, int insn)
	{
		// Nothing to compile
	}
	public override string disasm(int address, int insn)
	{

	return "vflush";
	}
	}
	public static readonly Instruction VSYNC = new InstructionAnonymousInnerClass254();

	private class InstructionAnonymousInnerClass254 : Instruction
	{
		public InstructionAnonymousInnerClass254() : base(250)
		{
		}


	public override sealed string name()
	{
		return "VSYNC";
	}
	public override sealed string category()
	{
		return "VFPU";
	}
	public override void interpret(Processor processor, int insn)
	{



	}
	public override void compile(ICompilerContext context, int insn)
	{
		// Nothing to compile
	}
	public override string disasm(int address, int insn)
	{

	return "vsync";
	}
	}
	public static readonly Instruction DBREAK = new InstructionAnonymousInnerClass255();

	private class InstructionAnonymousInnerClass255 : Instruction
	{
		public InstructionAnonymousInnerClass255() : base(251)
		{
		}


	public override sealed string name()
	{
		return "DBREAK";
	}
	public override sealed string category()
	{
		return "ME";
	}
	public override void interpret(Processor processor, int insn)
	{
		if (processor.cp0.MainCpu)
		{
			processor.cpu.doUNK(string.Format("Unsupported dbreak instruction on the main processor: 0x{0:X8}: [0x{1:X8}]", processor.cpu.pc, insn));
		}
		else
		{
			processor.cpu.pc = unchecked((int)0xBFC01000);
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
	return "dbreak";
	}
	}
	public static readonly Instruction MTVME = new InstructionAnonymousInnerClass256();

	private class InstructionAnonymousInnerClass256 : Instruction
	{
		public InstructionAnonymousInnerClass256() : base(252)
		{
		}


	public override sealed string name()
	{
		return "MTVME";
	}
	public override sealed string category()
	{
		return "ME";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		if (processor.cp0.MainCpu)
		{
			processor.cpu.doUNK(string.Format("Unsupported mtvme instruction on the main processor: 0x{0:X8}: [0x{1:X8}]", processor.cpu.pc, insn));
		}
		else if (processor is MEProcessor)
		{
			((MEProcessor) processor).setVmeRegister(imm16, processor.cpu.getRegister(rt));
		}
		else
		{
			processor.cpu.doUNK(string.Format("Unsupported processor for mtvme instruction is not an MEProcessor: 0x{0:X8}: [0x{1:X8}]", processor.cpu.pc, insn));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		return Common.disasmRTVME("mtvme", rt, imm16);
	}
	}
	public static readonly Instruction MFVME = new InstructionAnonymousInnerClass257();

	private class InstructionAnonymousInnerClass257 : Instruction
	{
		public InstructionAnonymousInnerClass257() : base(253)
		{
		}


	public override sealed string name()
	{
		return "MFVME";
	}
	public override sealed string category()
	{
		return "ME";
	}
	public override void interpret(Processor processor, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		if (processor.cp0.MainCpu)
		{
			processor.cpu.doUNK(string.Format("Unsupported mfvme instruction on the main processor: 0x{0:X8}: [0x{1:X8}]", processor.cpu.pc, insn));
		}
		else if (processor is MEProcessor)
		{
			processor.cpu.setRegister(rt, ((MEProcessor) processor).getVmeRegister(imm16));
		}
		else
		{
			processor.cpu.doUNK(string.Format("Unsupported processor for mfvme instruction is not an MEProcessor: 0x{0:X8}: [0x{1:X8}]", processor.cpu.pc, insn));
		}
	}
	public override void compile(ICompilerContext context, int insn)
	{
		base.compile(context, insn);
	}
	public override string disasm(int address, int insn)
	{
		int imm16 = (insn >> 0) & 65535;
		int rt = (insn >> 16) & 31;
		return Common.disasmRTVME("mfvme", rt, imm16);
	}
	}
	}

}