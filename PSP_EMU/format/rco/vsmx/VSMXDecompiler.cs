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
namespace pspsharp.format.rco.vsmx
{

	using Logger = org.apache.log4j.Logger;

	public class VSMXDecompiler
	{
		private static Logger log = VSMX.log;
		private VSMXMem mem;
		private string prefix;
		private Stack<int> blockEnd;
		private Stack<int> stack;
		private const int SWITCH_STATE_NONE = 0;
		private const int SWITCH_STATE_START = 1;
		private const int SWITCH_STATE_VALUE = 2;
		private const int SWITCH_STATE_CASE = 3;
		private const int SWITCH_STATE_MULTI_VALUE = 4;
		private int switchState;
		private int switchBreakLine;
		private int ignoreFunctionSet;
		private int statementStartLine;
		private ISet<int> needLineLabel;
		private StringBuilder booleanExpression;

		public VSMXDecompiler(VSMX vsmx)
		{
			mem = vsmx.Mem;
		}

		private void increaseIndent(int blockEndLine)
		{
			prefix += "  ";
			blockEnd.Push(blockEndLine);
		}

		private void decrementIndent()
		{
			blockEnd.Pop();
			prefix = prefix.Substring(0, prefix.Length - 2);
		}

		private void operator2(StringBuilder s, string @operator)
		{
			StringBuilder op1 = new StringBuilder();
			StringBuilder op2 = new StringBuilder();

			decompileOp(op1);
			decompileOp(op2);
			s.Append(string.Format("{0}{1}{2}", op2, @operator, op1));
		}

		private void operatorPre1(StringBuilder s, string @operator)
		{
			StringBuilder op = new StringBuilder();

			decompileOp(op);
			s.Append(string.Format("{0}{1}", @operator, op));
		}

		private void operatorPost1(StringBuilder s, string @operator)
		{
			StringBuilder op = new StringBuilder();

			decompileOp(op);
			s.Append(string.Format("{0}{1}", op, @operator));
		}

		private bool isBooleanExpression(StringBuilder s)
		{
			if (stack.Count == 0)
			{
				return false;
			}
			int i = stack.Peek().intValue();
			if (!mem.codes[i].isOpcode(VSMXCode.VID_STACK_COPY))
			{
				return false;
			}

			stack.Pop();
			decompileOp(s);

			return true;
		}

		private void addToBooleanExpression(StringBuilder s)
		{
			if (booleanExpression == null)
			{
				booleanExpression = new StringBuilder();
			}
			booleanExpression.Append(s.ToString());
		}

		private void addToBooleanExpression(StringBuilder s, bool isOr)
		{
			addToBooleanExpression(s);
			booleanExpression.Append(isOr ? " || " : " && ");
		}

		private void decompileStmt(StringBuilder s)
		{
			if (stack.Count == 0)
			{
				return;
			}
			int i = stack.Pop();
			decompileStmt(s, i);
		}

		private bool detectSwitch(StringBuilder s, int i)
		{
			if (mem.codes[i + 1].isOpcode(VSMXCode.VID_DEBUG_LINE))
			{
				i++;
			}
			if (!mem.codes[i + 1].isOpcode(VSMXCode.VID_JUMP))
			{
				return false;
			}

			if (blockEnd.Count > 0 && blockEnd.Peek().intValue() == i)
			{
				return false;
			}

			return true;
		}

		private bool isSwitch(int jumpLine)
		{
			if (switchState == SWITCH_STATE_NONE)
			{
				return false;
			}

			if (switchState == SWITCH_STATE_CASE && switchBreakLine >= 0 && jumpLine != switchBreakLine)
			{
				return false;
			}

			return true;
		}

		private int decompileSwitch(StringBuilder s, int i, int jumpLine)
		{
			StringBuilder op;
			switch (switchState)
			{
				case SWITCH_STATE_NONE:
					op = new StringBuilder();
					decompileOp(op);
					s.Append(string.Format("{0}switch ({1}) {{\n", prefix, op));
					switchState = SWITCH_STATE_START;
					switchBreakLine = -1;
					break;
				case SWITCH_STATE_START:
					if (switchBreakLine >= 0 && jumpLine == switchBreakLine)
					{
						switchState = SWITCH_STATE_NONE;
						i = switchBreakLine - 1;
					}
					else
					{
						switchState = SWITCH_STATE_VALUE;
					}
					break;
				case SWITCH_STATE_VALUE:
					op = new StringBuilder();
					decompileOp(op);
					if (mem.codes[i + 1].isOpcode(VSMXCode.VID_DEBUG_LINE))
					{
						i++;
					}
					if (mem.codes[i + 1].isOpcode(VSMXCode.VID_JUMP))
					{
						s.Append(string.Format("{0}case {1}:\n", prefix, op));
						switchState = SWITCH_STATE_MULTI_VALUE;
					}
					else
					{
						s.Append(string.Format("{0}case {1}: {{\n", prefix, op));
						switchState = SWITCH_STATE_CASE;
						increaseIndent(0);
					}
					break;
				case SWITCH_STATE_MULTI_VALUE:
					switchState = SWITCH_STATE_VALUE;
					break;
				case SWITCH_STATE_CASE:
					s.Append(string.Format("{0}break;\n", prefix));
					decrementIndent();
					s.Append(string.Format("{0}}}\n", prefix));
					switchBreakLine = jumpLine;
					switchState = SWITCH_STATE_START;
					break;
			}

			return i;
		}

		private bool isFunction(int i)
		{
			if (stack.Count == 0)
			{
				return false;
			}

			int prev = stack.Peek().intValue();
			VSMXGroup prevCode = mem.codes[prev];
			if (prevCode.isOpcode(VSMXCode.VID_OPERATOR_ASSIGN))
			{
				prev--;
				prevCode = mem.codes[prev];
			}

			if (!prevCode.isOpcode(VSMXCode.VID_FUNCTION) || prevCode.value != i + 1)
			{
				return false;
			}

			VSMXGroup prePrevCode = mem.codes[prev - 1];
			if (prePrevCode.isOpcode(VSMXCode.VID_PROPERTY))
			{
				prev--;
				prePrevCode = mem.codes[prev - 1];
			}

			if (!prePrevCode.isOpcode(VSMXCode.VID_VARIABLE))
			{
				return false;
			}

			return true;
		}

		private void decompileFunction(StringBuilder s, int setLine)
		{
			StringBuilder function = new StringBuilder();
			decompileOp(function);
			StringBuilder name = new StringBuilder();
			decompileOp(name);
			stack.Push(setLine);
			StringBuilder set = new StringBuilder();
			decompileOp(set);
			s.Append(string.Format("{0}{1}.{2} = {3}\n", prefix, name, set, function));

			increaseIndent(setLine);

			ignoreFunctionSet = setLine;
		}

		private void decompileOp(StringBuilder s)
		{
			if (stack.Count == 0)
			{
				return;
			}
			int i = stack.Pop();
			VSMXGroup code = mem.codes[i];
			int opcode = code.Opcode;
			int args;
			StringBuilder[] ops;
			StringBuilder op, op1, op2, method;
			switch (opcode)
			{
				case VSMXCode.VID_VARIABLE:
					s.Append(mem.names[code.value]);
					break;
				case VSMXCode.VID_UNNAMED_VAR:
					s.Append(string.Format("var{0:D}", code.value));
					break;
				case VSMXCode.VID_CONST_BOOL:
					if (code.value == 1)
					{
						s.Append("true");
					}
					else if (code.value == 0)
					{
						s.Append("false");
					}
					else
					{
						s.Append(string.Format("0x{0:X}", code.value));
					}
					break;
				case VSMXCode.VID_CONST_INT:
					s.Append(string.Format("{0:D}", code.value));
					break;
				case VSMXCode.VID_CONST_FLOAT:
					s.Append(string.Format("{0:F}", code.FloatValue));
					break;
				case VSMXCode.VID_CONST_STRING:
				case VSMXCode.VID_DEBUG_FILE:
					s.Append(string.Format("\"{0}\"", mem.texts[code.value]));
					break;
				case VSMXCode.VID_PROPERTY:
					op = new StringBuilder();
					decompileOp(op);
					s.Append(string.Format("{0}.{1}", op, mem.properties[code.value]));
					break;
				case VSMXCode.VID_METHOD:
				case VSMXCode.VID_SET_ATTR:
				case VSMXCode.VID_UNSET:
				case VSMXCode.VID_OBJ_ADD_ATTR:
					s.Append(mem.properties[code.value]);
					break;
				case VSMXCode.VID_FUNCTION:
					args = (code.id >> 8) & 0xFF;
					s.Append("function(");
					for (int n = 0; n < args; n++)
					{
						if (n > 0)
						{
							s.Append(", ");
						}
						s.Append(string.Format("var{0:D}", n + 1));
					}
					s.Append(string.Format(") {"));
					break;
				case VSMXCode.VID_CONST_EMPTYARRAY:
					s.Append("{}");
					break;
				case VSMXCode.VID_CONST_NULL:
					s.Append("null");
					break;
				case VSMXCode.VID_THIS:
					s.Append("this");
					break;
				case VSMXCode.VID_ARRAY_INDEX:
					op1 = new StringBuilder();
					decompileOp(op1);
					op2 = new StringBuilder();
					decompileOp(op2);
					s.Append(string.Format("{0}[{1}]", op2, op1));
					break;
				case VSMXCode.VID_ARRAY_INDEX_KEEP_OBJ:
					op1 = new StringBuilder();
					decompileOp(op1);
					i = stack.Peek();
					op2 = new StringBuilder();
					decompileOp(op2);
					stack.Push(i);
					s.Append(string.Format("{0}[{1}]", op2, op1));
					break;
				case VSMXCode.VID_CALL_NEW:
					args = code.value;
					ops = new StringBuilder[args];
					for (int n = args - 1; n >= 0; n--)
					{
						ops[n] = new StringBuilder();
						decompileOp(ops[n]);
					}
					op = new StringBuilder();
					decompileOp(op);
					s.Append(string.Format("new {0}(", op));
					for (int n = 0; n < args; n++)
					{
						if (n > 0)
						{
							s.Append(", ");
						}
						s.Append(ops[n]);
					}
					s.Append(")");
					break;
				case VSMXCode.VID_CALL_METHOD:
					args = code.value;
					ops = new StringBuilder[args];
					for (int n = args - 1; n >= 0; n--)
					{
						ops[n] = new StringBuilder();
						decompileOp(ops[n]);
					}
					method = new StringBuilder();
					decompileOp(method);
					op = new StringBuilder();
					decompileOp(op);
					s.Append(string.Format("{0}.{1}(", op, method));
					for (int n = 0; n < args; n++)
					{
						if (n > 0)
						{
							s.Append(", ");
						}
						s.Append(ops[n]);
					}
					s.Append(")");
					break;
				case VSMXCode.VID_CALL_FUNC:
					args = code.value;
					ops = new StringBuilder[args];
					for (int n = args - 1; n >= 0; n--)
					{
						ops[n] = new StringBuilder();
						decompileOp(ops[n]);
					}
					method = new StringBuilder();
					decompileOp(method);
					s.Append(string.Format("{0}(", method));
					for (int n = 0; n < args; n++)
					{
						if (n > 0)
						{
							s.Append(", ");
						}
						s.Append(ops[n]);
					}
					s.Append(")");
					break;
				case VSMXCode.VID_OPERATOR_EQUAL:
					operator2(s, " == ");
					break;
				case VSMXCode.VID_OPERATOR_NOT_EQUAL:
					operator2(s, " != ");
					break;
				case VSMXCode.VID_OPERATOR_GT:
					operator2(s, " > ");
					break;
				case VSMXCode.VID_OPERATOR_GTE:
					operator2(s, " >= ");
					break;
				case VSMXCode.VID_OPERATOR_LT:
					operator2(s, " < ");
					break;
				case VSMXCode.VID_OPERATOR_LTE:
					operator2(s, " <= ");
					break;
				case VSMXCode.VID_OPERATOR_NOT:
					operatorPre1(s, "!");
					break;
				case VSMXCode.VID_OPERATOR_NEGATE:
					operatorPre1(s, "-");
					break;
				case VSMXCode.VID_OPERATOR_ADD:
					operator2(s, " + ");
					break;
				case VSMXCode.VID_OPERATOR_SUBTRACT:
					operator2(s, " - ");
					break;
				case VSMXCode.VID_OPERATOR_MULTIPLY:
					operator2(s, " * ");
					break;
				case VSMXCode.VID_OPERATOR_DIVIDE:
					operator2(s, " / ");
					break;
				case VSMXCode.VID_OPERATOR_MOD:
					operator2(s, " % ");
					break;
				case VSMXCode.VID_OPERATOR_B_AND:
					operator2(s, " & ");
					break;
				case VSMXCode.VID_OPERATOR_B_XOR:
					operator2(s, " ^ ");
					break;
				case VSMXCode.VID_OPERATOR_B_OR:
					operator2(s, " | ");
					break;
				case VSMXCode.VID_OPERATOR_B_NOT:
					operatorPre1(s, "~");
					break;
				case VSMXCode.VID_OPERATOR_LSHIFT:
					operator2(s, " << ");
					break;
				case VSMXCode.VID_OPERATOR_RSHIFT:
					operator2(s, " >> ");
					break;
				case VSMXCode.VID_OPERATOR_URSHIFT:
					operator2(s, " >>> ");
					break;
				case VSMXCode.VID_INCREMENT:
					operatorPost1(s, "++");
					break;
				case VSMXCode.VID_DECREMENT:
					operatorPost1(s, "--");
					break;
				case VSMXCode.VID_P_INCREMENT:
					operatorPre1(s, "++");
					break;
				case VSMXCode.VID_P_DECREMENT:
					operatorPre1(s, "--");
					break;
				case VSMXCode.VID_ARRAY_PUSH:
					op1 = new StringBuilder();
					decompileOp(op1);
					if (stack.Count > 0 && mem.codes[stack.Peek().intValue()].isOpcode(VSMXCode.VID_ARRAY_PUSH))
					{
						// Display nicely an array initialization
						while (stack.Count > 0 && mem.codes[stack.Peek().intValue()].isOpcode(VSMXCode.VID_ARRAY_PUSH))
						{
							stack.Pop();
							op2 = new StringBuilder();
							decompileOp(op2);
							op1.Insert(0, string.Format(",\n{0}  ", prefix));
							op1.Insert(0, op2.ToString());
						}
						op2 = new StringBuilder();
						decompileOp(op2);
						s.Append(string.Format("{0} {{\n{1}  {2}\n{3}}}", op2, prefix, op1, prefix));
					}
					else
					{
						op2 = new StringBuilder();
						decompileOp(op2);
						s.Append(string.Format("{0}.push({1})", op2, op1));
					}
					break;
				case VSMXCode.VID_ARRAY:
					s.Append("new Array()");
					break;
				case VSMXCode.VID_OPERATOR_ASSIGN:
					op1 = new StringBuilder();
					decompileOp(op1);
					op2 = new StringBuilder();
					decompileOp(op2);
					s.Append(string.Format("{0} = {1}", op2, op1));
					break;
				case VSMXCode.VID_STACK_COPY:
					if (stack.Count > 0)
					{
						i = stack.Pop();
						stack.Push(i);
						stack.Push(i);
						decompileOp(s);
					}
					break;
				case VSMXCode.VID_DEBUG_LINE:
					// Ignore debug line
					decompileOp(s);
					break;
				default:
					log.warn(string.Format("Line #{0:D}: decompileOp({1}) unimplemented", i, VSMXCode.VsmxDecOps[opcode]));
					break;
			}
		}

		private int decompileStmt(StringBuilder s, int i)
		{
			int initialLength = s.Length;
			VSMXGroup code = mem.codes[i];
			int opcode = code.Opcode;
			StringBuilder op1;
			StringBuilder op2;
			StringBuilder op3;
			switch (opcode)
			{
				case VSMXCode.VID_OPERATOR_ASSIGN:
					op1 = new StringBuilder();
					decompileOp(op1);
					op2 = new StringBuilder();
					decompileOp(op2);
					s.Append(string.Format("{0}{1} = {2}", prefix, op2, op1));
					break;
				case VSMXCode.VID_ARRAY_INDEX_ASSIGN:
					op1 = new StringBuilder();
					decompileOp(op1);
					op2 = new StringBuilder();
					decompileOp(op2);
					op3 = new StringBuilder();
					decompileOp(op3);
					s.Append(string.Format("{0}{1}[{2}] = {3}", prefix, op3, op2, op1));
					break;
				case VSMXCode.VID_CALL_FUNC:
				case VSMXCode.VID_CALL_METHOD:
					stack.Push(i);
					op1 = new StringBuilder();
					decompileOp(op1);
					s.Append(string.Format("{0}{1}", prefix, op1));
					break;
				case VSMXCode.VID_JUMP_TRUE:
					op1 = new StringBuilder();
					if (isBooleanExpression(op1))
					{
						addToBooleanExpression(op1, true);
					}
					else if (booleanExpression != null)
					{
						decompileOp(op1);
						addToBooleanExpression(op1, true);
						s.Append(string.Format("{0}if ({1}) {{", prefix, booleanExpression));
						increaseIndent(code.value);
						booleanExpression = null;
					}
					else
					{
						decompileOp(op1);
						// this is probably a "for" loop
						s.Append(string.Format("{0:D}:\n", statementStartLine));
						if (mem.codes[i + 1].isOpcode(VSMXCode.VID_JUMP))
						{
							int elseGoto = mem.codes[i + 1].value;
							s.Append(string.Format("{0}if ({1}) goto {2:D}; else goto {3:D}", prefix, op1, code.value, elseGoto));
							needLineLabel.Add(elseGoto);
							needLineLabel.Add(i + 2);
							i++;
						}
						else
						{
							s.Append(string.Format("{0}if ({1}) goto {2:D}", prefix, op1, code.value));
						}
						needLineLabel.Add(code.value);
					}
					break;
				case VSMXCode.VID_JUMP_FALSE:
					op1 = new StringBuilder();
					if (isBooleanExpression(op1))
					{
						addToBooleanExpression(op1, false);
					}
					else if (booleanExpression != null)
					{
						decompileOp(op1);
						addToBooleanExpression(op1);
						s.Append(string.Format("{0}if ({1}) {{", prefix, booleanExpression));
						increaseIndent(code.value);
						booleanExpression = null;
					}
					else
					{
						decompileOp(op1);
						s.Append(string.Format("{0}if ({1}) {{", prefix, op1));
						increaseIndent(code.value);
					}
					break;
				case VSMXCode.VID_RETURN:
					op1 = new StringBuilder();
					decompileOp(op1);
					s.Append(string.Format("{0}return {1}", prefix, op1));
					break;
				case VSMXCode.VID_SET_ATTR:
					if (i == ignoreFunctionSet)
					{
						ignoreFunctionSet = -1;
					}
					else
					{
						op1 = new StringBuilder();
						decompileOp(op1);
						op2 = new StringBuilder();
						decompileOp(op2);
						s.Append(string.Format("{0}{1}.{2} = {3}", prefix, op2, mem.properties[code.value], op1));
					}
					break;
				case VSMXCode.VID_INCREMENT:
					s.Append(prefix);
					operatorPost1(s, "++");
					break;
				case VSMXCode.VID_DECREMENT:
					s.Append(prefix);
					operatorPost1(s, "--");
					break;
				case VSMXCode.VID_P_INCREMENT:
					s.Append(prefix);
					operatorPre1(s, "++");
					break;
				case VSMXCode.VID_P_DECREMENT:
					s.Append(prefix);
					operatorPre1(s, "--");
					break;
				case VSMXCode.VID_OPERATOR_EQUAL:
					operator2(s, " == ");
					break;
				case VSMXCode.VID_JUMP:
					if (code.value > i)
					{
						increaseIndent(code.value);
					}
					else
					{
						// Backward loop
						s.Append(string.Format("{0}goto {1:D}", prefix, code.value));
					}
					break;
				case VSMXCode.VID_VARIABLE:
					s.Append(prefix);
					s.Append(mem.names[code.value]);
					break;
				case VSMXCode.VID_UNNAMED_VAR:
					s.Append(prefix);
					s.Append(string.Format("var{0:D}", code.value));
					break;
				case VSMXCode.VID_DEBUG_LINE:
					break;
				default:
					log.warn(string.Format("Line #{0:D}: decompileStmt({1}) unimplemented", i, VSMXCode.VsmxDecOps[opcode]));
					break;
			}

			if (s.Length != initialLength)
			{
				if (s[s.Length - 1] != '{')
				{
					s.Append(";");
				}
				s.Append(string.Format(" // line {0:D}", i));
				s.Append("\n");
			}

			return i;
		}

		private string decompile()
		{
			StringBuilder s = new StringBuilder();

			prefix = "";
			stack = new Stack<int>();
			blockEnd = new Stack<int>();
			switchState = SWITCH_STATE_NONE;
			ignoreFunctionSet = -1;
			statementStartLine = 0;
			needLineLabel = new HashSet<int>();

			for (int i = 0; i < mem.codes.Length; i++)
			{
				VSMXGroup code = mem.codes[i];
				int opcode = code.Opcode;
				while (blockEnd.Count > 0 && blockEnd.Peek().intValue() == i)
				{
					decrementIndent();
					s.Append(string.Format("{0}}}\n", prefix));
				}
				if (needLineLabel.remove(i))
				{
					s.Append(string.Format("{0:D}:\n", i));
				}
				switch (opcode)
				{
					case VSMXCode.VID_END_STMT:
						decompileStmt(s);
						statementStartLine = i + 1;
						break;
					case VSMXCode.VID_RETURN:
					case VSMXCode.VID_JUMP_FALSE:
					case VSMXCode.VID_JUMP_TRUE:
						i = decompileStmt(s, i);
						break;
					case VSMXCode.VID_JUMP:
						if (isSwitch(code.value) || detectSwitch(s, i))
						{
							i = decompileSwitch(s, i, code.value);
						}
						else if (isFunction(i))
						{
							decompileFunction(s, code.value);
						}
						else
						{
							if (blockEnd.Count > 0 && blockEnd.Peek().intValue() == i + 1)
							{
								decrementIndent();
								s.Append(string.Format("{0}}} else {{\n", prefix));
							}
							i = decompileStmt(s, i);
						}
						break;
					default:
						stack.Push(i);
						break;
				}
			}

			return s.ToString();
		}

		public override string ToString()
		{
			return decompile();
		}
	}

}