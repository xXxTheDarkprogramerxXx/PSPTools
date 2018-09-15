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

	using NativeCodeManager = pspsharp.Allegrex.compiler.nativeCode.NativeCodeManager;
	using NativeCodeSequence = pspsharp.Allegrex.compiler.nativeCode.NativeCodeSequence;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using Settings = pspsharp.settings.Settings;

	//using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class Profiler
	{

		//public static Logger log = Logger.getLogger("profiler");
		private static bool profilerEnabled = false;
		private static readonly Dictionary<int, long> callCounts = new Dictionary<int, long>();
		private static readonly Dictionary<int, long> instructionCounts = new Dictionary<int, long>();
		private static readonly Dictionary<int, long> backBranchCounts = new Dictionary<int, long>();
		private static readonly long? zero = new long?(0);
		private const int detailedCodeBlockLogThreshold = 50;
		private const int codeLogMaxLength = 700;
		private const int backBranchMaxLength = 100;
		private const int backBranchContextBefore = 5;
		private const int backBranchContextAfter = 3;
		private static ProfilerEnabledSettingsListerner profilerEnabledSettingsListerner;
		private static int compilationCount;
		private static long compilationTimeMicros;
		private static long longestCompilationTimeMicros;

		private class ProfilerEnabledSettingsListerner : AbstractBoolSettingsListener
		{
			protected internal override void settingsValueChanged(bool value)
			{
				ProfilerEnabled = value;
			}
		}

		public static void initialise()
		{
			if (profilerEnabledSettingsListerner == null)
			{
				profilerEnabledSettingsListerner = new ProfilerEnabledSettingsListerner();
				Settings.Instance.registerSettingsListener("Profiler", "emu.profiler", profilerEnabledSettingsListerner);
			}

			reset();
		}

		private static bool ProfilerEnabled
		{
			set
			{
				profilerEnabled = value;
			}
			get
			{
				return profilerEnabled;
			}
		}


		public static void reset()
		{
			if (!profilerEnabled)
			{
				return;
			}

			callCounts.Clear();
			instructionCounts.Clear();
			backBranchCounts.Clear();
			compilationCount = 0;
			compilationTimeMicros = 0;
			longestCompilationTimeMicros = 0;
		}

		public static void exit()
		{
			if (!profilerEnabled)
			{
				return;
			}
			IList<int> sortedBackBranches = new List<int>(backBranchCounts.Keys);
			sortedBackBranches.Sort(new BackBranchComparator());

			IList<CodeBlock> sortedCodeBlocks = new List<CodeBlock>(RuntimeContext.CodeBlocks.Values);
			sortedCodeBlocks.Sort(new CodeBlockComparator());

			long allCycles = 0;
			foreach (long? instructionCount in instructionCounts.Values)
			{
				allCycles += instructionCount.Value;
			}

			int count = 0;
			double avg = compilationCount == 0 ? 0.0 : compilationTimeMicros / (double) compilationCount / 1000;
			log.info(string.Format("Compilation time {0:D}ms, {1:D} calls, average {2:F1}ms, longest {3:D}ms", compilationTimeMicros / 1000, compilationCount, avg, longestCompilationTimeMicros / 1000));
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.info(String.format("CodeBlocks profiling information (%,d total cycles):", allCycles));
			log.info(string.Format("CodeBlocks profiling information (%,d total cycles):", allCycles));
			foreach (CodeBlock codeBlock in sortedCodeBlocks)
			{
				long callCount = getCallCount(codeBlock);
				long instructionCount = getInstructionCount(codeBlock);

				if (callCount == 0 && instructionCount == 0)
				{
					// This and the following CodeBlocks have not been called since
					// the Profiler has been reset. Skip them.
					break;
				}
				logCodeBlock(codeBlock, allCycles, instructionCount, callCount, count, sortedBackBranches);
				count++;
			}
		}

		private static void logCodeBlock(CodeBlock codeBlock, long allCycles, long instructionCount, long callCount, int count, IList<int> sortedBackBranches)
		{
			string name = codeBlock.ClassName;
			NativeCodeManager nativeCodeManager = Compiler.Instance.NativeCodeManager;
			NativeCodeSequence nativeCodeSequence = nativeCodeManager.getCompiledNativeCodeBlock(codeBlock.StartAddress);
			if (nativeCodeSequence != null)
			{
				name = string.Format("{0} ({1})", name, nativeCodeSequence.Name);
			}
			int lowestAddress = codeBlock.LowestAddress;
			int highestAddress = codeBlock.HighestAddress;
			int Length = (highestAddress - lowestAddress) / 4 + 1;
			double percentage = 0;
			if (allCycles != 0)
			{
				percentage = (instructionCount / (double) allCycles) * 100;
			}
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.info(String.format("%s %,d instructions (%2.3f%%), %,d calls (%08X - %08X, Length %d)", name, instructionCount, percentage, callCount, lowestAddress, highestAddress, Length));
			log.info(string.Format("%s %,d instructions (%2.3f%%), %,d calls (%08X - %08X, Length %d)", name, instructionCount, percentage, callCount, lowestAddress, highestAddress, Length));
			if (count < detailedCodeBlockLogThreshold && codeBlock.Length <= codeLogMaxLength)
			{
				logCode(codeBlock);
			}
			foreach (int address in sortedBackBranches)
			{
				if (address < lowestAddress || address > highestAddress)
				{
					continue;
				}
				CodeInstruction codeInstruction = codeBlock.getCodeInstruction(address);
				if (codeInstruction == null)
				{
					continue;
				}
				int branchingToAddress = codeInstruction.BranchingTo;
				// Add 2 for branch instruction itself and delay slot
				int backBranchLength = (address - branchingToAddress) / 4 + 2;
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.info(String.format("  Back Branch %08X %,d times (Length %d)", address, backBranchCounts.get(address), backBranchLength));
				log.info(string.Format("  Back Branch %08X %,d times (Length %d)", address, backBranchCounts[address], backBranchLength));
				if (count < detailedCodeBlockLogThreshold && backBranchLength <= backBranchMaxLength)
				{
					logCode(codeBlock, branchingToAddress, backBranchLength, backBranchContextBefore, backBranchContextAfter, address);
				}
			}
		}

		private static void logCode(CodeBlock codeBlock)
		{
			logCode(codeBlock, codeBlock.LowestAddress, codeBlock.Length, 0, 0, -1);
		}

		private static void logCode(SequenceCodeInstruction sequenceCodeInstruction, int highlightAddress)
		{
			foreach (CodeInstruction codeInstruction in sequenceCodeInstruction.CodeSequence.Instructions)
			{
				logCode(codeInstruction, highlightAddress);
			}
		}

		private static void logCode(CodeInstruction codeInstruction, int highlightAddress)
		{
			if (codeInstruction == null)
			{
				return;
			}
			int address = codeInstruction.Address;
			if (codeInstruction is SequenceCodeInstruction)
			{
				logCode((SequenceCodeInstruction) codeInstruction, highlightAddress);
			}
			else
			{
				int opcode = codeInstruction.Opcode;
				string prefix = "   ";
				if (address == highlightAddress)
				{
					prefix = "-->";
				}
				log.info(string.Format("{0} {1:X8}:[{2:X8}]: {3}", prefix, address, opcode, codeInstruction.disasm(address, opcode)));
			}
		}

		private static void logCode(CodeBlock codeBlock, int startAddress, int Length, int contextBefore, int contextAfter, int highlightAddress)
		{
			for (int i = -contextBefore; i < Length + contextAfter; i++)
			{
				int address = startAddress + (i * 4);
				CodeInstruction codeInstruction = codeBlock.getCodeInstruction(address);
				if (highlightAddress >= 0 && address == startAddress)
				{
					// Also highlight startAddress
					logCode(codeInstruction, startAddress);
				}
				else
				{
					logCode(codeInstruction, highlightAddress);
				}
			}
		}

		private static long getCallCount(CodeBlock codeBlock)
		{
			long? callCount = callCounts[codeBlock.StartAddress];
			if (callCount == null)
			{
				return 0;
			}

			return callCount.Value;
		}

		private static long getInstructionCount(CodeBlock codeBlock)
		{
			long? instructionCount = instructionCounts[codeBlock.StartAddress];
			if (instructionCount == null)
			{
				return 0;
			}

			return instructionCount.Value;
		}

		public static void addCall(int address)
		{
			long? callCount = callCounts[address];
			if (callCount == null)
			{
				callCount = zero;
			}

			callCounts[address] = callCount + 1;
		}

		public static void addInstructionCount(int count, int address)
		{
			long? instructionCount = instructionCounts[address];
			if (instructionCount == null)
			{
				instructionCount = zero;
			}

			instructionCounts[address] = instructionCount + count;
		}

		public static void addBackBranch(int address)
		{
			long? backBranchCount = backBranchCounts[address];
			if (backBranchCount == null)
			{
				backBranchCount = zero;
			}

			backBranchCounts[address] = backBranchCount + 1;
		}

		private class BackBranchComparator : Comparator<int>
		{

			public virtual int Compare(int? address1, int? address2)
			{
				long count1 = backBranchCounts[address1];
				long count2 = backBranchCounts[address2];

				if (count1 == count2)
				{
					return 0;
				}
				return (count2 > count1 ? 1 : -1);
			}
		}

		private class CodeBlockComparator : Comparator<CodeBlock>
		{

			public virtual int Compare(CodeBlock codeBlock1, CodeBlock codeBlock2)
			{
				long instructionCallCount1 = getInstructionCount(codeBlock1);
				long instructionCallCount2 = getInstructionCount(codeBlock2);

				if (instructionCallCount1 != instructionCallCount2)
				{
					return (instructionCallCount2 > instructionCallCount1 ? 1 : -1);
				}

				long callCount1 = getCallCount(codeBlock1);
				long callCount2 = getCallCount(codeBlock2);

				if (callCount1 != callCount2)
				{
					return (callCount2 > callCount1 ? 1 : -1);
				}

				return codeBlock2.StartAddress - codeBlock1.StartAddress;
			}
		}

		public static void addCompilation(long compilationTimeMicros)
		{
			compilationCount++;
			Profiler.compilationTimeMicros += compilationTimeMicros;
			if (compilationTimeMicros > longestCompilationTimeMicros)
			{
				longestCompilationTimeMicros = compilationTimeMicros;
			}
		}
	}

}