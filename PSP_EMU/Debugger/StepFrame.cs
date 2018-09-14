using System;
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
namespace pspsharp.Debugger
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.GprState.NUMBER_REGISTERS;
	using Common = pspsharp.Allegrex.Common;
	using CpuState = pspsharp.Allegrex.CpuState;
	using Decoder = pspsharp.Allegrex.Decoder;
	using Modules = pspsharp.HLE.Modules;

	public class StepFrame
	{

		// Optimize for speed and memory, just store the raw details and calculate
		// the formatted message the first time getMessage it called.
		private int pc;
		private int[] gpr = new int[NUMBER_REGISTERS];

		private int opcode;
		private string asm;

		private int threadID;
		private string threadName;

		private bool dirty;
		private string message;

		public StepFrame()
		{
			dirty = false;
			message = "";
		}

		public virtual void make(CpuState cpu)
		{
			pc = cpu.pc;
			for (int i = 0; i < NUMBER_REGISTERS; i++)
			{
				gpr[i] = cpu.getRegister(i);
			}
			threadID = Modules.ThreadManForUserModule.CurrentThreadID;
			threadName = Modules.ThreadManForUserModule.getThreadName(threadID);

			Memory mem = MemoryViewer.Memory;
			if (MemoryViewer.isAddressGood(cpu.pc))
			{
				opcode = mem.read32(cpu.pc);
				Common.Instruction insn = Decoder.instruction(opcode);
				asm = insn.disasm(cpu.pc, opcode);
			}
			else
			{
				opcode = 0;
				asm = "?";
			}

			dirty = true;
		}

		private string ThreadInfo
		{
			get
			{
				// Thread ID - 0x04600843
				// Th Name   - user_main
				return string.Format("Thread ID - 0x{0:X8}\n", threadID) + "Th Name   - " + threadName + "\n";
			}
		}

		private string RegistersInfo
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < NUMBER_REGISTERS; i += 4)
				{
					sb.Append(string.Format("{0}:0x{1:X8} {2}:0x{3:X8} {4}:0x{5:X8} {6}:0x{7:X8}\n", Common.gprNames[i + 0].Substring(1), gpr[i + 0], Common.gprNames[i + 1].Substring(1), gpr[i + 1], Common.gprNames[i + 2].Substring(1), gpr[i + 2], Common.gprNames[i + 3].Substring(1), gpr[i + 3]));
				}
    
				return sb.ToString();
			}
		}

		private void makeMessage()
		{
			string address = string.Format("0x{0:X8}", pc);
			string rawdata = string.Format("0x{0:X8}", opcode);

			message = ThreadInfo + RegistersInfo + address + ": " + rawdata + " - " + asm;
		}

		public virtual string Message
		{
			get
			{
				if (dirty)
				{
					dirty = false;
					makeMessage();
				}
				return message;
			}
		}

		public virtual bool JAL
		{
			get
			{
				return (asm.IndexOf("jal", StringComparison.Ordinal) != -1);
			}
		}

		public virtual bool JRRA
		{
			get
			{
				return (asm.IndexOf("jr", StringComparison.Ordinal) != -1) && (asm.IndexOf("$ra", StringComparison.Ordinal) != -1);
			}
		}
	}
}