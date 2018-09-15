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

	using Utilities = pspsharp.util.Utilities;

	using Element = org.w3c.dom.Element;
	using Node = org.w3c.dom.Node;
	using NodeList = org.w3c.dom.NodeList;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class NativeCodeManager
	{
		private static int defaultOpcodeMask = unchecked((int)0xFFFFFFFF);
		private Dictionary<int, IList<NativeCodeSequence>> nativeCodeSequencesByFirstOpcode;
		private IList<NativeCodeSequence> nativeCodeSequenceWithMaskInFirstOpcode;
		private Dictionary<int, NativeCodeSequence> compiledNativeCodeBlocks;

		public NativeCodeManager(Element configuration)
		{
			compiledNativeCodeBlocks = new Dictionary<int, NativeCodeSequence>();
			nativeCodeSequencesByFirstOpcode = new Dictionary<int, IList<NativeCodeSequence>>();
			nativeCodeSequenceWithMaskInFirstOpcode = new LinkedList<NativeCodeSequence>();

			load(configuration);
		}

		public virtual void reset()
		{
			compiledNativeCodeBlocks.Clear();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private Class<INativeCodeSequence> getNativeCodeSequenceClass(String className)
		private Type<INativeCodeSequence> getNativeCodeSequenceClass(string className)
		{
			try
			{
				return (Type<INativeCodeSequence>) Type.GetType(className);
			}
			catch (ClassNotFoundException e)
			{
				Compiler.Console.WriteLine(e);
				return null;
			}
		}

		private string getContent(Node node)
		{
			if (node.hasChildNodes())
			{
				return getContent(node.ChildNodes);
			}

			return node.NodeValue;
		}

		private string getContent(NodeList nodeList)
		{
			if (nodeList == null || nodeList.Length <= 0)
			{
				return null;
			}

			StringBuilder content = new StringBuilder();
			int n = nodeList.Length;
			for (int i = 0; i < n; i++)
			{
				Node node = nodeList.item(i);
				content.Append(getContent(node));
			}

			return content.ToString();
		}

		private void loadBeforeCodeInstructions(NativeCodeSequence nativeCodeSequence, string codeInstructions)
		{
			System.IO.StreamReader reader = new System.IO.StreamReader(new StringReader(codeInstructions));
			if (reader == null)
			{
				return;
			}

			Pattern codeInstructionPattern = Pattern.compile("\\s*(\\w+\\s*:?\\s*)?\\[(\\p{XDigit}+)\\].*");
			const int opcodeGroup = 2;
			const int address = 0;

			try
			{
				while (true)
				{
					string line = reader.ReadLine();
					if (string.ReferenceEquals(line, null))
					{
						break;
					}

					line = line.Trim();
					if (line.Length > 0)
					{
						try
						{
							Matcher codeInstructionMatcher = codeInstructionPattern.matcher(line);
							int opcode = 0;
							if (codeInstructionMatcher.matches())
							{
								opcode = Utilities.parseAddress(codeInstructionMatcher.group(opcodeGroup));
							}
							else
							{
								opcode = Utilities.parseAddress(line.Trim());
							}

							Common.Instruction insn = Decoder.instruction(opcode);
							CodeInstruction codeInstruction = new CodeInstruction(address, opcode, insn, false, false, 0);

							nativeCodeSequence.addBeforeCodeInstruction(codeInstruction);
						}
						catch (System.FormatException e)
						{
							Compiler.Console.WriteLine(e);
						}
					}
				}
			}
			catch (IOException e)
			{
				Compiler.Console.WriteLine(e);
			}
		}

		private void loadNativeCodeOpcodes(NativeCodeSequence nativeCodeSequence, string codeInstructions)
		{
			System.IO.StreamReader reader = new System.IO.StreamReader(new StringReader(codeInstructions));
			if (reader == null)
			{
				return;
			}

			Pattern codeInstructionPattern = Pattern.compile("\\s*((\\w+)\\s*:?\\s*)?\\[(\\p{XDigit}+)(/(\\p{XDigit}+))?\\].*");
			const int labelGroup = 2;
			const int opcodeGroup = 3;
			const int opcodeMaskGroup = 5;

			try
			{
				while (true)
				{
					string line = reader.ReadLine();
					if (string.ReferenceEquals(line, null))
					{
						break;
					}

					line = line.Trim();
					if (line.Length > 0)
					{
						try
						{
							Matcher codeInstructionMatcher = codeInstructionPattern.matcher(line);
							int opcode = 0;
							int mask = defaultOpcodeMask;
							string label = null;
							if (codeInstructionMatcher.matches())
							{
								opcode = Utilities.parseAddress(codeInstructionMatcher.group(opcodeGroup));
								string opcodeMaskString = codeInstructionMatcher.group(opcodeMaskGroup);
								if (!string.ReferenceEquals(opcodeMaskString, null))
								{
									mask = Utilities.parseAddress(opcodeMaskString);
								}
								label = codeInstructionMatcher.group(labelGroup);
							}
							else
							{
								opcode = Utilities.parseAddress(line.Trim());
							}

							nativeCodeSequence.addOpcode(opcode, mask, label);
						}
						catch (System.FormatException e)
						{
							Compiler.Console.WriteLine(e);
						}
					}
				}
			}
			catch (IOException e)
			{
				Compiler.Console.WriteLine(e);
			}
		}

		private void setParameter(NativeCodeSequence nativeCodeSequence, int parameter, string valueString)
		{
			if (string.ReferenceEquals(valueString, null) || valueString.Length <= 0)
			{
				nativeCodeSequence.setParameter(parameter, 0, false);
				return;
			}

			for (int i = 0; i < Common.gprNames.Length; i++)
			{
				if (Common.gprNames[i].Equals(valueString))
				{
					nativeCodeSequence.setParameter(parameter, i, false);
					return;
				}
			}

			for (int i = 0; i < Common.fprNames.Length; i++)
			{
				if (Common.fprNames[i].Equals(valueString))
				{
					nativeCodeSequence.setParameter(parameter, i, false);
					return;
				}
			}

			if (valueString.StartsWith("@", StringComparison.Ordinal))
			{
				string label = valueString.Substring(1);
				int labelIndex = nativeCodeSequence.getLabelIndex(label);
				if (labelIndex >= 0)
				{
					nativeCodeSequence.setParameter(parameter, labelIndex, true);
					return;
				}
			}

			try
			{
				int value;
				if (valueString.StartsWith("0x", StringComparison.Ordinal))
				{
					value = Convert.ToInt32(valueString.Substring(2), 16);
				}
				else
				{
					value = int.Parse(valueString);
				}
				nativeCodeSequence.setParameter(parameter, value, false);
			}
			catch (System.FormatException e)
			{
				Compiler.Console.WriteLine(e);
			}
		}

		private void loadNativeCodeSequence(Element element)
		{
			string name = element.getAttribute("name");
			string className = getContent(element.getElementsByTagName("Class"));

			Type<INativeCodeSequence> nativeCodeSequenceClass = getNativeCodeSequenceClass(className);
			if (nativeCodeSequenceClass == null)
			{
				return;
			}

			NativeCodeSequence nativeCodeSequence = new NativeCodeSequence(name, nativeCodeSequenceClass);

			string isReturningString = getContent(element.getElementsByTagName("IsReturning"));
			if (!string.ReferenceEquals(isReturningString, null))
			{
				nativeCodeSequence.Returning = bool.Parse(isReturningString);
			}

			string wholeCodeBlockString = getContent(element.getElementsByTagName("WholeCodeBlock"));
			if (!string.ReferenceEquals(wholeCodeBlockString, null))
			{
				nativeCodeSequence.WholeCodeBlock = bool.Parse(wholeCodeBlockString);
			}

			string methodName = getContent(element.getElementsByTagName("Method"));
			if (!string.ReferenceEquals(methodName, null))
			{
				nativeCodeSequence.MethodName = methodName;
			}

			string isHookString = getContent(element.getElementsByTagName("IsHook"));
			if (!string.ReferenceEquals(isHookString, null))
			{
				nativeCodeSequence.Hook = bool.Parse(isHookString);
			}

			string isMethodRetuningString = getContent(element.getElementsByTagName("IsMethodReturning"));
			if (!string.ReferenceEquals(isMethodRetuningString, null))
			{
				nativeCodeSequence.MethodReturning = bool.Parse(isMethodRetuningString);
			}

			string codeInstructions = getContent(element.getElementsByTagName("CodeInstructions"));
			loadNativeCodeOpcodes(nativeCodeSequence, codeInstructions);

			// The "Parameters" and "BranchInstruction" have to be parsed after "CodeInstructions"
			// because they are using them (e.g. instruction labels)
			string parametersList = getContent(element.getElementsByTagName("Parameters"));
			if (!string.ReferenceEquals(parametersList, null))
			{
				string[] parameters = parametersList.Split(" *, *", true);
				for (int parameter = 0; parameters != null && parameter < parameters.Length; parameter++)
				{
					setParameter(nativeCodeSequence, parameter, parameters[parameter].Trim());
				}
			}

			string branchInstructionLabel = getContent(element.getElementsByTagName("BranchInstruction"));
			if (!string.ReferenceEquals(branchInstructionLabel, null))
			{
				if (branchInstructionLabel.StartsWith("@", StringComparison.Ordinal))
				{
					branchInstructionLabel = branchInstructionLabel.Substring(1);
				}
				int branchInstructionOffset = nativeCodeSequence.getLabelIndex(branchInstructionLabel.Trim());
				if (branchInstructionOffset >= 0)
				{
					nativeCodeSequence.BranchInstruction = branchInstructionOffset;
				}
				else
				{
					Compiler.Console.WriteLine(string.Format("BranchInstruction: label '{0}' not found", branchInstructionLabel));
				}
			}

			string beforeCodeInstructions = getContent(element.getElementsByTagName("BeforeCodeInstructions"));
			if (!string.ReferenceEquals(beforeCodeInstructions, null))
			{
				loadBeforeCodeInstructions(nativeCodeSequence, beforeCodeInstructions);
			}

			addNativeCodeSequence(nativeCodeSequence);
		}

		private void load(Element configuration)
		{
			if (configuration == null)
			{
				return;
			}

			NodeList nativeCodeBlocks = configuration.getElementsByTagName("NativeCodeSequence");
			int n = nativeCodeBlocks.Length;
			for (int i = 0; i < n; i++)
			{
				Element nativeCodeSequence = (Element) nativeCodeBlocks.item(i);
				loadNativeCodeSequence(nativeCodeSequence);
			}
		}

		private void addNativeCodeSequence(NativeCodeSequence nativeCodeSequence)
		{
			if (nativeCodeSequence.NumOpcodes > 0)
			{
				int firstOpcodeMask = nativeCodeSequence.FirstOpcodeMask;
				if (firstOpcodeMask == defaultOpcodeMask)
				{
					// First opcode has not mask: fast lookup allowed
					int firstOpcode = nativeCodeSequence.FirstOpcode;

					if (!nativeCodeSequencesByFirstOpcode.ContainsKey(firstOpcode))
					{
						nativeCodeSequencesByFirstOpcode[firstOpcode] = new LinkedList<NativeCodeSequence>();
					}
					nativeCodeSequencesByFirstOpcode[firstOpcode].Add(nativeCodeSequence);
				}
				else
				{
					// First opcode has not mask: only slow lookup possible
					nativeCodeSequenceWithMaskInFirstOpcode.Add(nativeCodeSequence);
				}
			}
		}

		public virtual void setCompiledNativeCodeBlock(int address, NativeCodeSequence nativeCodeBlock)
		{
			compiledNativeCodeBlocks[address] = nativeCodeBlock;
		}

		public virtual NativeCodeSequence getCompiledNativeCodeBlock(int address)
		{
			return compiledNativeCodeBlocks[address];
		}

		public virtual void invalidateCompiledNativeCodeBlocks(int startAddress, int endAddress)
		{
			// Most common case: nothing to do.
			if (compiledNativeCodeBlocks.Count == 0)
			{
				return;
			}

			// What is the most efficient?
			// To scan all the addressed between startAddress and endAddres or
			// to scan all the compiledNativeCodeBlocks?
			if ((int)((uint)(endAddress - startAddress) >> 2) < compiledNativeCodeBlocks.Count)
			{
				for (int address = startAddress; address <= endAddress; address += 4)
				{
					compiledNativeCodeBlocks.Remove(address);
				}
			}
			else
			{
				IList<int> toBeRemoved = new LinkedList<int>();
				foreach (int? address in compiledNativeCodeBlocks.Keys)
				{
					if (startAddress >= address.Value && address.Value <= endAddress)
					{
						toBeRemoved.Add(address);
					}
				}
				foreach (int? address in toBeRemoved)
				{
					compiledNativeCodeBlocks.Remove(address);
				}
				toBeRemoved.Clear();
			}
		}

		private bool isNativeCodeSequence(NativeCodeSequence nativeCodeSequence, CodeInstruction codeInstruction, CodeBlock codeBlock)
		{
			int address = codeInstruction.Address;
			int numOpcodes = nativeCodeSequence.NumOpcodes;

			// Can this NativeCodeSequence only match a whole CodeBlock?
			if (nativeCodeSequence.WholeCodeBlock)
			{
				// Match only a whole CodeBlock: same StartAddress, same Length
				if (codeBlock.StartAddress != address)
				{
					return false;
				}

				if (codeBlock.Length != numOpcodes)
				{
					return false;
				}
			}

			for (int i = 0; i < numOpcodes; i++)
			{
				int opcode = codeBlock.getCodeInstructionOpcode(address + (i << 2));
				if (!nativeCodeSequence.isMatching(i, opcode))
				{
					return false;
				}
			}

			return true;
		}

		public virtual NativeCodeSequence getNativeCodeSequence(CodeInstruction codeInstruction, CodeBlock codeBlock)
		{
			int firstOpcode = codeInstruction.Opcode;

			// Fast lookup using the first opcode
			if (nativeCodeSequencesByFirstOpcode.ContainsKey(firstOpcode))
			{
				for (IEnumerator<NativeCodeSequence> it = nativeCodeSequencesByFirstOpcode[firstOpcode].GetEnumerator(); it.MoveNext();)
				{
					NativeCodeSequence nativeCodeSequence = it.Current;
					if (isNativeCodeSequence(nativeCodeSequence, codeInstruction, codeBlock))
					{
						return nativeCodeSequence;
					}
				}
			}

			// Slow lookup for sequences having an opcode mask in the first opcode
			foreach (NativeCodeSequence nativeCodeSequence in nativeCodeSequenceWithMaskInFirstOpcode)
			{
				if (isNativeCodeSequence(nativeCodeSequence, codeInstruction, codeBlock))
				{
					return nativeCodeSequence;
				}
			}

			return null;
		}
	}
}