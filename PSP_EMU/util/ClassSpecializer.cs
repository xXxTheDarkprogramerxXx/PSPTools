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
namespace pspsharp.util
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.tree.AbstractInsnNode.JUMP_INSN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.tree.AbstractInsnNode.LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.tree.AbstractInsnNode.LINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.tree.AbstractInsnNode.LOOKUPSWITCH_INSN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.objectweb.asm.tree.AbstractInsnNode.TABLESWITCH_INSN;


	using Logger = org.apache.log4j.Logger;
	using ClassReader = org.objectweb.asm.ClassReader;
	using ClassVisitor = org.objectweb.asm.ClassVisitor;
	using ClassWriter = org.objectweb.asm.ClassWriter;
	using Opcodes = org.objectweb.asm.Opcodes;
	using AbstractInsnNode = org.objectweb.asm.tree.AbstractInsnNode;
	using ClassNode = org.objectweb.asm.tree.ClassNode;
	using FieldInsnNode = org.objectweb.asm.tree.FieldInsnNode;
	using FieldNode = org.objectweb.asm.tree.FieldNode;
	using InsnNode = org.objectweb.asm.tree.InsnNode;
	using IntInsnNode = org.objectweb.asm.tree.IntInsnNode;
	using JumpInsnNode = org.objectweb.asm.tree.JumpInsnNode;
	using LabelNode = org.objectweb.asm.tree.LabelNode;
	using LdcInsnNode = org.objectweb.asm.tree.LdcInsnNode;
	using LookupSwitchInsnNode = org.objectweb.asm.tree.LookupSwitchInsnNode;
	using MethodInsnNode = org.objectweb.asm.tree.MethodInsnNode;
	using MethodNode = org.objectweb.asm.tree.MethodNode;
	using TableSwitchInsnNode = org.objectweb.asm.tree.TableSwitchInsnNode;
	using Analyzer = org.objectweb.asm.tree.analysis.Analyzer;
	using AnalyzerException = org.objectweb.asm.tree.analysis.AnalyzerException;
	using BasicInterpreter = org.objectweb.asm.tree.analysis.BasicInterpreter;
	using Frame = org.objectweb.asm.tree.analysis.Frame;
	using TraceClassVisitor = org.objectweb.asm.util.TraceClassVisitor;

	/// <summary>
	/// @author gid15
	/// 
	/// Specialize a Java class by modifying its code to exclude part of it,
	/// based on a list of field values dynamically computed at runtime.
	/// The specialized class contains only the part of the code that will be
	/// executed for the given field values, without the overhead for testing these values.
	/// 
	/// For example, given the following class:
	///    public class Test {
	///      public static int testValue;
	/// 		int test(int parameter) {
	///        if (testValue == 0) {
	///          return 0;
	///        } else if (testValue < 0) {
	///          return -parameter;
	///        } else {
	///          return parameter;
	///        }
	///      }
	///    }
	/// 
	/// A specialized class for the following field values:
	///    testValue = 123;
	/// would be
	///    public class SpecialitedTest1 {
	///      int test(int parameter) {
	///        return parameter;
	///      }
	///    }
	/// 
	/// and for
	///    testValue = -123;
	/// it would be
	///    public class SpecialitedTest2 {
	///      int test(int parameter) {
	///        return -parameter;
	///      }
	///    }
	/// 
	/// The following code statements can be evaluated by the specializer:
	/// - if
	/// - switch
	/// - while
	/// on field values of the following types:
	/// - int
	/// - byte
	/// - short
	/// - boolean
	/// - float
	/// </summary>
	public class ClassSpecializer
	{
		private static Logger log = Logger.getLogger("classSpecializer");
		private static SpecializedClassLoader classLoader = new SpecializedClassLoader();
		private static HashSet<Type> tracedClasses = new HashSet<Type>();

		public virtual Type specialize(string name, Type c, Dictionary<string, object> variables)
		{
			ClassWriter cw = new ClassWriter(ClassWriter.COMPUTE_FRAMES | ClassWriter.COMPUTE_MAXS);
			ClassVisitor cv = cw;

			StringWriter debugOutput = null;
			if (log.TraceEnabled)
			{
				// Dump the class to be specialized (only once)
				if (!tracedClasses.Contains(c))
				{
					StringWriter classTrace = new StringWriter();
					ClassVisitor classTraceCv = new TraceClassVisitor(new PrintWriter(classTrace));
					try
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						ClassReader cr = new ClassReader(c.FullName.Replace('.', '/'));
						cr.accept(classTraceCv, 0);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						log.trace(string.Format("Dump of class to be specialized: {0}", c.FullName));
						log.trace(classTrace);
					}
					catch (IOException)
					{
						// Ignore Exception
					}
					tracedClasses.Add(c);
				}

				log.trace(string.Format("Specializing class {0}", name));
				string[] variableNames = variables.Keys.toArray(new string[variables.Count]);
				Arrays.sort(variableNames);
				foreach (string variableName in variableNames)
				{
					log.trace(string.Format("Variable {0}={1}", variableName, variables[variableName]));
				}

				debugOutput = new StringWriter();
				PrintWriter debugPrintWriter = new PrintWriter(debugOutput);
				cv = new TraceClassVisitor(cv, debugPrintWriter);
				//cv = new TraceClassVisitor(debugPrintWriter);
			}

			try
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				ClassReader cr = new ClassReader(c.FullName.Replace('.', '/'));
				ClassNode cn = new SpecializedClassVisitor(name, variables);
				cr.accept(cn, 0);
				cn.accept(cv);
			}
			catch (IOException e)
			{
				log.error("Cannot read class", e);
			}

			if (debugOutput != null)
			{
				log.trace(debugOutput.ToString());
			}

			Type specializedClass = null;
			try
			{
				specializedClass = classLoader.defineClass(name, cw.toByteArray());
			}
			catch (ClassFormatError e)
			{
				log.error("Error while defining specialized class", e);
			}

			return specializedClass;
		}

		private class SpecializedClassVisitor : ClassNode
		{
			internal readonly string specializedClassName;
			internal readonly Dictionary<string, object> variables;
			internal object value;
			internal AbstractInsnNode deleteUpToInsn;
			internal string className;
			internal string superClassName;

			public SpecializedClassVisitor(string specializedClassName, Dictionary<string, object> variables)
			{
				this.specializedClassName = specializedClassName;
				this.variables = variables;
			}

			public override void visit(int version, int access, string name, string signature, string superName, string[] interfaces)
			{
				className = name;
				superClassName = superName;
				// Define the specialized class as extending the original class
				base.visit(version, access, specializedClassName, signature, name, interfaces);
			}

			public override void visitEnd()
			{
				// Visit all the methods
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Iterator<?> it = methods.iterator(); it.hasNext();)
				for (IEnumerator<object> it = methods.GetEnumerator(); it.MoveNext();)
				{
					MethodNode method = (MethodNode) it.Current;
					visitMethod(method);
				}

				// Delete all the fields used as specialization variables
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.ListIterator<?> lit = fields.listIterator(); lit.hasNext();)
				for (IEnumerator<object> lit = fields.GetEnumerator(); lit.MoveNext();)
				{
					FieldNode field = (FieldNode) lit.Current;
					if ((field.access & Opcodes.ACC_STATIC) != 0 && variables.ContainsKey(field.name))
					{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						lit.remove();
					}
				}

				base.visitEnd();
			}

			internal virtual void visitMethod(MethodNode method)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean isConstructor = "<init>".equals(method.name);
				bool isConstructor = "<init>".Equals(method.name);

				deleteUpToInsn = null;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.ListIterator<?> lit = method.instructions.iterator(); lit.hasNext();)
				for (IEnumerator<object> lit = method.instructions.GetEnumerator(); lit.MoveNext();)
				{
					AbstractInsnNode insn = (AbstractInsnNode) lit.Current;

					if (deleteUpToInsn != null)
					{
						if (insn == deleteUpToInsn)
						{
							deleteUpToInsn = null;
						}
						else
						{
							// Do not delete labels, they could be used as a target from a previous jump.
							// Also keep line numbers for easier debugging.
							if (insn.Type != LABEL && insn.Type != LINE)
							{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
								lit.remove();
							}
							continue;
						}
					}

					if (insn.Type == AbstractInsnNode.FRAME)
					{
						// Remove all the FRAME information, they will be calculated
						// anew after the class specialization.
						lit.remove();
					}
					else if (insn.Opcode == Opcodes.GETSTATIC)
					{
						FieldInsnNode fieldInsn = (FieldInsnNode) insn;
						if (variables.ContainsKey(fieldInsn.name))
						{
							bool processed = false;
							value = variables[fieldInsn.name];
							AbstractInsnNode nextInsn = insn.Next;
							if (analyseIfTestInt(method, insn))
							{
								processed = true;
							}
							else if (nextInsn != null && nextInsn.Type == TABLESWITCH_INSN)
							{
								TableSwitchInsnNode switchInsn = (TableSwitchInsnNode) nextInsn;
								LabelNode label = null;
								if (isIntValue(value))
								{
									int n = getIntValue(value);
									if (n >= switchInsn.min && n <= switchInsn.max)
									{
										int i = n - switchInsn.min;
										if (i < switchInsn.labels.size())
										{
											label = (LabelNode) switchInsn.labels.get(i);
										}
									}
								}
								if (label == null)
								{
									label = switchInsn.dflt;
								}
								if (label != null)
								{
									// Replace the table switch instruction by a GOTO to the switch label
									method.instructions.set(insn, new JumpInsnNode(Opcodes.GOTO, label));
									processed = true;
								}
							}
							else if (nextInsn != null && nextInsn.Type == LOOKUPSWITCH_INSN)
							{
								LookupSwitchInsnNode switchInsn = (LookupSwitchInsnNode) nextInsn;
								LabelNode label = null;
								if (isIntValue(value))
								{
									int n = getIntValue(value);
									int i = 0;
									foreach (object value in switchInsn.keys)
									{
										if (value is int?)
										{
											if (((int?) value).Value == n)
											{
												label = (LabelNode) switchInsn.labels.get(i);
												break;
											}
										}
										i++;
									}
								}
								if (label == null)
								{
									label = switchInsn.dflt;
								}
								if (label != null)
								{
									// Replace the table switch instruction by a GOTO to the switch label
									method.instructions.set(insn, new JumpInsnNode(Opcodes.GOTO, label));
									processed = true;
								}
							}
							else if (nextInsn != null && nextInsn.Type == AbstractInsnNode.INSN)
							{
								int opcode = nextInsn.Opcode;
								int n = 0;
								float f = 0f;
								bool isIntConstant = false;
								bool isFloatConstant = false;
								switch (opcode)
								{
									case Opcodes.ICONST_M1:
										n = -1;
										isIntConstant = true;
										break;
									case Opcodes.ICONST_0:
										n = 0;
										isIntConstant = true;
										break;
									case Opcodes.ICONST_1:
										n = 1;
										isIntConstant = true;
										break;
									case Opcodes.ICONST_2:
										n = 2;
										isIntConstant = true;
										break;
									case Opcodes.ICONST_3:
										n = 3;
										isIntConstant = true;
										break;
									case Opcodes.ICONST_4:
										n = 4;
										isIntConstant = true;
										break;
									case Opcodes.ICONST_5:
										n = 5;
										isIntConstant = true;
										break;
									case Opcodes.FCONST_0:
										f = 0f;
										isFloatConstant = true;
										break;
									case Opcodes.FCONST_1:
										f = 1f;
										isFloatConstant = true;
										break;
									case Opcodes.FCONST_2:
										f = 2f;
										isFloatConstant = true;
										break;
								}
								if (isIntConstant)
								{
									if (analyseIfTestInt(method, insn, nextInsn, n))
									{
										processed = true;
									}
								}
								else if (isFloatConstant)
								{
									if (analyseIfTestFloat(method, insn, nextInsn, f))
									{
										processed = true;
									}
								}
							}
							else if (nextInsn != null && nextInsn.Type == AbstractInsnNode.INT_INSN)
							{
								IntInsnNode intInsn = (IntInsnNode) nextInsn;
								if (analyseIfTestInt(method, insn, nextInsn, intInsn.operand))
								{
									processed = true;
								}
							}
							else if (nextInsn != null && nextInsn.Type == AbstractInsnNode.LDC_INSN)
							{
								LdcInsnNode ldcInsn = (LdcInsnNode) nextInsn;
								if (isIntValue(ldcInsn.cst))
								{
									if (analyseIfTestInt(method, insn, nextInsn, getIntValue(ldcInsn.cst)))
									{
										processed = true;
									}
								}
								else if (isFloatValue(ldcInsn.cst))
								{
									if (analyseIfTestFloat(method, insn, nextInsn, getFloatValue(ldcInsn.cst)))
									{
										processed = true;
									}
								}
							}

							if (!processed)
							{
								// Replace the field access by its constant value
								AbstractInsnNode constantInsn = getConstantInsn(value);
								if (constantInsn != null)
								{
									method.instructions.set(insn, constantInsn);
								}
							}
						}
						else
						{
							if (fieldInsn.owner.Equals(className))
							{
								// Replace the class name by the specialized class name
								fieldInsn.owner = specializedClassName;
							}
						}
					}
					else if (insn.Opcode == Opcodes.PUTSTATIC)
					{
						FieldInsnNode fieldInsn = (FieldInsnNode) insn;
						if (!variables.ContainsKey(fieldInsn.name))
						{
							if (fieldInsn.owner.Equals(className))
							{
								// Replace the class name by the specialized class name
								fieldInsn.owner = specializedClassName;
							}
						}
					}
					else if (insn.Type == AbstractInsnNode.METHOD_INSN)
					{
						MethodInsnNode methodInsn = (MethodInsnNode) insn;
						if (methodInsn.owner.Equals(className))
						{
							// Replace the class name by the specialized class name
							methodInsn.owner = specializedClassName;
						}
						else if (isConstructor && methodInsn.owner.Equals(superClassName))
						{
							// Update the call to the constructor of the parent class
							methodInsn.owner = className;
						}
					}
				}

				// Delete all the information about local variables, they are no longer correct
				// (the class loader would complain).
				method.localVariables.clear();

				optimizeJumps(method);
				removeDeadCode(method);
				optimizeJumps(method);
				removeUnusedLabels(method);
				removeUselessLineNumbers(method);
			}

			/// <summary>
			/// Optimize the jumps from a method:
			/// - jumps to a "GOTO label" instruction
			///   are replaced with a direct jump to "label";
			/// - a GOTO to the next instruction is deleted;
			/// - a GOTO to a RETURN or ATHROW instruction
			///   is replaced with this RETURN or ATHROW instruction.
			/// </summary>
			/// <param name="method">  the method to be optimized </param>
			internal virtual void optimizeJumps(MethodNode method)
			{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.ListIterator<?> lit = method.instructions.iterator(); lit.hasNext();)
				for (IEnumerator<object> lit = method.instructions.GetEnumerator(); lit.MoveNext();)
				{
					AbstractInsnNode insn = (AbstractInsnNode) lit.Current;
					if (insn.Type == JUMP_INSN)
					{
						JumpInsnNode jumpInsn = (JumpInsnNode) insn;
						LabelNode label = jumpInsn.label;
						AbstractInsnNode target;
						// while target == goto l, replace label with l
						while (true)
						{
							target = label;
							while (target != null && target.Opcode < 0)
							{
								target = target.Next;
							}
							if (target != null && target.Opcode == Opcodes.GOTO)
							{
								label = ((JumpInsnNode) target).label;
							}
							else
							{
								break;
							}
						}

						// update target
						jumpInsn.label = label;

						bool removeJump = false;
						if (jumpInsn.Opcode == Opcodes.GOTO)
						{
							// Delete a GOTO to the next instruction
							AbstractInsnNode next = jumpInsn.Next;
							while (next != null)
							{
								if (next == label)
								{
									removeJump = true;
									break;
								}
								else if (next.Opcode >= 0)
								{
									break;
								}
								next = next.Next;
							}
						}

						if (removeJump)
						{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
							lit.remove();
						}
						else
						{
							// if possible, replace jump with target instruction
							if (jumpInsn.Opcode == Opcodes.GOTO && target != null)
							{
								switch (target.Opcode)
								{
									case Opcodes.IRETURN:
									case Opcodes.LRETURN:
									case Opcodes.FRETURN:
									case Opcodes.DRETURN:
									case Opcodes.ARETURN:
									case Opcodes.RETURN:
									case Opcodes.ATHROW:
										// replace instruction with clone of target
										method.instructions.set(insn, target.clone(null));
									break;
								}
							}
						}
					}
				}
			}

			/// <summary>
			/// Remove the dead code - or unreachable code - from a method.
			/// </summary>
			/// <param name="method">  the method to be updated </param>
			internal virtual void removeDeadCode(MethodNode method)
			{
				try
				{
					// Analyze the method using the BasicInterpreter.
					// As a result, the computed frames are null for instructions
					// that cannot be reached.
					Analyzer analyzer = new Analyzer(new BasicInterpreter());
					analyzer.analyze(specializedClassName, method);
					Frame[] frames = analyzer.Frames;
					AbstractInsnNode[] insns = method.instructions.toArray();
					for (int i = 0; i < frames.Length; i++)
					{
						AbstractInsnNode insn = insns[i];
						if (frames[i] == null && insn.Type != AbstractInsnNode.LABEL)
						{
							// This instruction was not reached by the analyzer
							method.instructions.remove(insn);
							insns[i] = null;
						}
					}
				}
				catch (AnalyzerException)
				{
					// Ignore error
				}
			}

			/// <summary>
			/// Remove unused labels, i.e. labels that are not referenced.
			/// </summary>
			/// <param name="method">  the method to be updated </param>
			internal virtual void removeUnusedLabels(MethodNode method)
			{
				// Scan for all the used labels
				ISet<LabelNode> usedLabels = new HashSet<LabelNode>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.ListIterator<?> lit = method.instructions.iterator(); lit.hasNext();)
				for (IEnumerator<object> lit = method.instructions.GetEnumerator(); lit.MoveNext();)
				{
					AbstractInsnNode insn = (AbstractInsnNode) lit.Current;
					if (insn.Type == JUMP_INSN)
					{
						JumpInsnNode jumpInsn = (JumpInsnNode) insn;
						usedLabels.Add(jumpInsn.label);
					}
					else if (insn.Type == TABLESWITCH_INSN)
					{
						TableSwitchInsnNode tableSwitchInsn = (TableSwitchInsnNode) insn;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Iterator<?> it = tableSwitchInsn.labels.iterator(); it.hasNext();)
						for (IEnumerator<object> it = tableSwitchInsn.labels.GetEnumerator(); it.MoveNext();)
						{
							LabelNode labelNode = (LabelNode) it.Current;
							if (labelNode != null)
							{
								usedLabels.Add(labelNode);
							}
						}
					}
					else if (insn.Type == LOOKUPSWITCH_INSN)
					{
						LookupSwitchInsnNode loopupSwitchInsn = (LookupSwitchInsnNode) insn;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Iterator<?> it = loopupSwitchInsn.labels.iterator(); it.hasNext();)
						for (IEnumerator<object> it = loopupSwitchInsn.labels.GetEnumerator(); it.MoveNext();)
						{
							LabelNode labelNode = (LabelNode) it.Current;
							if (labelNode != null)
							{
								usedLabels.Add(labelNode);
							}
						}
					}
				}

				// Remove all the label instructions not being identified in the scan
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.ListIterator<?> lit = method.instructions.iterator(); lit.hasNext();)
				for (IEnumerator<object> lit = method.instructions.GetEnumerator(); lit.MoveNext();)
				{
					AbstractInsnNode insn = (AbstractInsnNode) lit.Current;
					if (insn.Type == LABEL)
					{
						if (!usedLabels.Contains(insn))
						{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
							lit.remove();
						}
					}
				}
			}

			/// <summary>
			/// Remove unused line numbers, i.e. line numbers where there is no code.
			/// </summary>
			/// <param name="method">  the method to be updated </param>
			internal virtual void removeUselessLineNumbers(MethodNode method)
			{
				// Remove all the line numbers being immediately followed by another line number.
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.ListIterator<?> lit = method.instructions.iterator(); lit.hasNext();)
				for (IEnumerator<object> lit = method.instructions.GetEnumerator(); lit.MoveNext();)
				{
					AbstractInsnNode insn = (AbstractInsnNode) lit.Current;
					if (insn.Type == LINE)
					{
						AbstractInsnNode nextInsn = insn.Next;
						if (nextInsn != null && nextInsn.Type == LINE)
						{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
							lit.remove();
						}
					}
				}
			}

			internal virtual bool analyseIfTestInt(MethodNode method, AbstractInsnNode insn)
			{
				return analyseIfTestInt(method, insn, insn, null);
			}

			internal virtual bool analyseIfTestInt(MethodNode method, AbstractInsnNode insn, AbstractInsnNode valueInsn, int? testValue)
			{
				bool eliminateJump = false;

				AbstractInsnNode nextInsn = valueInsn.Next;
				if (nextInsn != null && nextInsn.Type == JUMP_INSN)
				{
					JumpInsnNode jumpInsn = (JumpInsnNode) nextInsn;
					bool doJump = false;
					switch (jumpInsn.Opcode)
					{
						case Opcodes.IFEQ:
							if (testValue == null && isIntValue(value))
							{
								doJump = getIntValue(value) == 0;
								eliminateJump = true;
							}
							break;
						case Opcodes.IFNE:
							if (testValue == null && isIntValue(value))
							{
								doJump = getIntValue(value) != 0;
								eliminateJump = true;
							}
							break;
						case Opcodes.IFLT:
							if (testValue == null && isIntValue(value))
							{
								doJump = getIntValue(value) < 0;
								eliminateJump = true;
							}
							break;
						case Opcodes.IFGE:
							if (testValue == null && isIntValue(value))
							{
								doJump = getIntValue(value) >= 0;
								eliminateJump = true;
							}
							break;
						case Opcodes.IFGT:
							if (testValue == null && isIntValue(value))
							{
								doJump = getIntValue(value) > 0;
								eliminateJump = true;
							}
							break;
						case Opcodes.IFLE:
							if (testValue == null && isIntValue(value))
							{
								doJump = getIntValue(value) <= 0;
								eliminateJump = true;
							}
							break;
						case Opcodes.IF_ICMPEQ:
							if (testValue != null && isIntValue(value))
							{
								doJump = getIntValue(value) == testValue.Value;
								eliminateJump = true;
							}
							break;
						case Opcodes.IF_ICMPNE:
							if (testValue != null && isIntValue(value))
							{
								doJump = getIntValue(value) != testValue.Value;
								eliminateJump = true;
							}
							break;
						case Opcodes.IF_ICMPLT:
							if (testValue != null && isIntValue(value))
							{
								doJump = getIntValue(value) < testValue.Value;
								eliminateJump = true;
							}
							break;
						case Opcodes.IF_ICMPGE:
							if (testValue != null && isIntValue(value))
							{
								doJump = getIntValue(value) >= testValue.Value;
								eliminateJump = true;
							}
							break;
						case Opcodes.IF_ICMPGT:
							if (testValue != null && isIntValue(value))
							{
								doJump = getIntValue(value) > testValue.Value;
								eliminateJump = true;
							}
							break;
						case Opcodes.IF_ICMPLE:
							if (testValue != null && isIntValue(value))
							{
								doJump = getIntValue(value) <= testValue.Value;
								eliminateJump = true;
							}
							break;
					}

					if (eliminateJump)
					{
						if (doJump)
						{
							// Replace the expression test by a fixed GOTO.
							// The skipped instructions will be eliminated by dead code analysis.
							method.instructions.set(insn, new JumpInsnNode(Opcodes.GOTO, jumpInsn.label));
						}
						else
						{
							method.instructions.remove(insn);
						}
						deleteUpToInsn = jumpInsn.Next;
					}
				}

				return eliminateJump;
			}

			internal virtual bool analyseIfTestFloat(MethodNode method, AbstractInsnNode insn, AbstractInsnNode valueInsn, float testValue)
			{
				bool eliminateJump = false;

				AbstractInsnNode nextInsn = valueInsn.Next;
				if (nextInsn != null && (nextInsn.Opcode == Opcodes.FCMPL || nextInsn.Opcode == Opcodes.FCMPG))
				{
					AbstractInsnNode nextNextInsn = nextInsn.Next;
					if (nextNextInsn != null && nextNextInsn.Type == JUMP_INSN)
					{
						JumpInsnNode jumpInsn = (JumpInsnNode) nextNextInsn;
						bool doJump = false;
						switch (jumpInsn.Opcode)
						{
							case Opcodes.IFEQ:
								if (isFloatValue(value))
								{
									doJump = getFloatValue(value) == testValue;
									eliminateJump = true;
								}
								break;
							case Opcodes.IFNE:
								if (isFloatValue(value))
								{
									doJump = getFloatValue(value) != testValue;
									eliminateJump = true;
								}
								break;
							case Opcodes.IFLT:
								if (isFloatValue(value))
								{
									doJump = getFloatValue(value) < testValue;
									eliminateJump = true;
								}
								break;
							case Opcodes.IFGE:
								if (isFloatValue(value))
								{
									doJump = getFloatValue(value) >= testValue;
									eliminateJump = true;
								}
								break;
							case Opcodes.IFGT:
								if (isFloatValue(value))
								{
									doJump = getFloatValue(value) > testValue;
									eliminateJump = true;
								}
								break;
							case Opcodes.IFLE:
								if (isFloatValue(value))
								{
									doJump = getFloatValue(value) <= testValue;
									eliminateJump = true;
								}
								break;
						}

						if (eliminateJump)
						{
							if (doJump)
							{
								// Replace the expression test by a fixed GOTO.
								// The skipped instructions will be eliminated by dead code analysis.
								method.instructions.set(insn, new JumpInsnNode(Opcodes.GOTO, jumpInsn.label));
							}
							else
							{
								method.instructions.remove(insn);
							}
							deleteUpToInsn = jumpInsn.Next;
						}
					}
				}

				return eliminateJump;
			}

			internal virtual bool isIntValue(object value)
			{
				return (value is int?) || (value is bool?);
			}

			internal virtual int getIntValue(object value)
			{
				if (value is int?)
				{
					return ((int?) value).Value;
				}
				if (value is bool?)
				{
					return value == false ? 0 : 1;
				}
				return 0;
			}

			internal virtual bool isFloatValue(object value)
			{
				return (value is float?);
			}

			internal virtual float getFloatValue(object value)
			{
				if (value is float?)
				{
					return ((float?) value).Value;
				}
				return 0f;
			}

			internal virtual AbstractInsnNode getConstantInsn(object value)
			{
				AbstractInsnNode constantInsn = null;

				if (isIntValue(value))
				{
					int n = getIntValue(value);
					// Find the optimum opcode to represent this integer value
					switch (n)
					{
						case -1:
							constantInsn = new InsnNode(Opcodes.ICONST_M1);
							break;
						case 0:
							constantInsn = new InsnNode(Opcodes.ICONST_0);
							break;
						case 1:
							constantInsn = new InsnNode(Opcodes.ICONST_1);
							break;
						case 2:
							constantInsn = new InsnNode(Opcodes.ICONST_2);
							break;
						case 3:
							constantInsn = new InsnNode(Opcodes.ICONST_3);
							break;
						case 4:
							constantInsn = new InsnNode(Opcodes.ICONST_4);
							break;
						case 5:
							constantInsn = new InsnNode(Opcodes.ICONST_5);
							break;
						default:
							if (sbyte.MinValue <= n && n < sbyte.MaxValue)
							{
								constantInsn = new IntInsnNode(Opcodes.BIPUSH, n);
							}
							else if (short.MinValue <= n && n < short.MaxValue)
							{
								constantInsn = new IntInsnNode(Opcodes.SIPUSH, n);
							}
							else
							{
								constantInsn = new LdcInsnNode(new int?(n));
							}
							break;
					}
				}

				return constantInsn;
			}
		}

		private class SpecializedClassLoader : ClassLoader
		{
			public virtual Type defineClass(string name, sbyte[] b)
			{
				return defineClass(name, b, 0, b.Length);
			}
		}
	}

}