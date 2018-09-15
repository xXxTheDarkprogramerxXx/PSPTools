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
namespace pspsharp.format.rco.vsmx.interpreter
{

	//using Logger = org.apache.log4j.Logger;

	using IAction = pspsharp.HLE.kernel.types.IAction;

	public class VSMXInterpreter
	{
		private static readonly Logger log = VSMX.log;
		private VSMXMem mem;
		private int pc;
		private bool exit;
		private Stack<VSMXBaseObject> stack;
		private Stack<VSMXCallState> callStates;
		private VSMXCallState callState;
		private VSMXObject globalVariables;
		private string prefix;
		private string name;

		private class InterpretFunctionAction : IAction
		{
			private readonly VSMXInterpreter outerInstance;

			internal VSMXFunction function;
			internal VSMXBaseObject @object;
			internal VSMXBaseObject[] arguments;

			public InterpretFunctionAction(VSMXInterpreter outerInstance, VSMXFunction function, VSMXBaseObject @object, VSMXBaseObject[] arguments)
			{
				this.outerInstance = outerInstance;
				this.function = function;
				this.@object = @object;
				this.arguments = arguments;
			}

			public virtual void execute()
			{
				outerInstance.interpretFunction(function, @object, arguments);
			}
		}

		public VSMXInterpreter()
		{
		}

		public virtual VSMX VSMX
		{
			set
			{
				mem = value.Mem;
				name = value.Name;
			}
		}

		public virtual VSMXObject GlobalVariables
		{
			get
			{
				return globalVariables;
			}
		}

		private VSMXBaseObject[] popValues(int n)
		{
			VSMXBaseObject[] values = new VSMXBaseObject[n];
			for (int i = n - 1; i >= 0; i--)
			{
				values[i] = stack.Pop().Value;
			}

			return values;
		}

		private void pushCallState(VSMXBaseObject thisObject, int numberOfLocalVariables, bool returnThis, bool exitAfterCall)
		{
			if (callState != null)
			{
				callStates.Push(callState);
			}
			callState = new VSMXCallState(thisObject, numberOfLocalVariables, pc, returnThis, exitAfterCall);
			stack = callState.Stack;
			prefix += "  ";
		}

		private void popCallState()
		{
			if (callStates.Count == 0)
			{
				callState = null;
				stack = null;
				prefix = "";
			}
			else
			{
				callState = callStates.Pop();
				stack = callState.Stack;
				prefix = prefix.Substring(0, prefix.Length - 2);
			}
		}

		private void interpret(VSMXGroup code)
		{
			VSMXBaseObject o1, o2, o3, o, r;
			VSMXBaseObject[] arguments;
			float f1, f2, f;
			string s1, s2, s;
			int i1, i2, i;
			bool b;
			switch (code.Opcode)
			{
				case VSMXCode.VID_NOTHING:
					break;
				case VSMXCode.VID_OPERATOR_ASSIGN:
					o1 = stack.Pop().Value;
					o2 = stack.Pop();
					if (o2 is VSMXReference)
					{
						if (log.TraceEnabled)
						{
							log.trace(string.Format("{0} = {1}", o2, o1));
						}
						((VSMXReference) o2).assign(o1);
						stack.Push(o1);
					}
					else
					{
						Console.WriteLine(string.Format("Line#{0:D} non-ref assignment {1}", pc - 1, code));
					}
					break;
				case VSMXCode.VID_OPERATOR_ADD:
					o1 = stack.Pop().Value;
					o2 = stack.Pop().Value;
					if (o1 is VSMXString || o2 is VSMXString)
					{
						s1 = o1.StringValue;
						s2 = o2.StringValue;
						s = s2 + s1;
						stack.Push(new VSMXString(this, s));
					}
					else
					{
						f1 = o1.FloatValue;
						f2 = o2.FloatValue;
						f = f2 + f1;
						stack.Push(new VSMXNumber(this, f));
					}
					break;
				case VSMXCode.VID_OPERATOR_SUBTRACT:
					f1 = stack.Pop().FloatValue;
					f2 = stack.Pop().FloatValue;
					f = f2 - f1;
					stack.Push(new VSMXNumber(this, f));
					break;
				case VSMXCode.VID_OPERATOR_MULTIPLY:
					f1 = stack.Pop().FloatValue;
					f2 = stack.Pop().FloatValue;
					f = f2 * f1;
					stack.Push(new VSMXNumber(this, f));
					break;
				case VSMXCode.VID_OPERATOR_DIVIDE:
					f1 = stack.Pop().FloatValue;
					f2 = stack.Pop().FloatValue;
					f = f2 / f1;
					stack.Push(new VSMXNumber(this, f));
					break;
				case VSMXCode.VID_OPERATOR_MOD:
					f1 = stack.Pop().FloatValue;
					f2 = stack.Pop().FloatValue;
					f = f2 % f1;
					stack.Push(new VSMXNumber(this, f));
					break;
				case VSMXCode.VID_OPERATOR_POSITIVE:
					f1 = stack.Pop().FloatValue;
					f = f1;
					stack.Push(new VSMXNumber(this, f));
					break;
				case VSMXCode.VID_OPERATOR_NEGATE:
					f1 = stack.Pop().FloatValue;
					f = -f1;
					stack.Push(new VSMXNumber(this, f));
					break;
				case VSMXCode.VID_OPERATOR_NOT:
					b = stack.Pop().BooleanValue;
					b = !b;
					stack.Push(VSMXBoolean.getValue(b));
					break;
				case VSMXCode.VID_P_INCREMENT:
					o = stack.Pop();
					f = o.FloatValue;
					f += 1f;
					stack.Push(new VSMXNumber(this, f));
					if (o is VSMXReference)
					{
						((VSMXReference) o).assign(new VSMXNumber(this, f));
					}
					else
					{
						Console.WriteLine(string.Format("Line#{0:D} non-ref increment {1}", pc - 1, code));
					}
					break;
				case VSMXCode.VID_P_DECREMENT:
					o = stack.Pop();
					f = o.FloatValue;
					f -= 1f;
					stack.Push(new VSMXNumber(this, f));
					if (o is VSMXReference)
					{
						((VSMXReference) o).assign(new VSMXNumber(this, f));
					}
					else
					{
						Console.WriteLine(string.Format("Line#{0:D} non-ref increment {1}", pc - 1, code));
					}
					break;
				case VSMXCode.VID_INCREMENT:
					o = stack.Pop();
					f = o.FloatValue;
					stack.Push(new VSMXNumber(this, f));
					if (o is VSMXReference)
					{
						((VSMXReference) o).assign(new VSMXNumber(this, f + 1f));
					}
					else
					{
						Console.WriteLine(string.Format("Line#{0:D} non-ref increment {1}", pc - 1, code));
					}
					break;
				case VSMXCode.VID_DECREMENT:
					o = stack.Pop();
					f = o.FloatValue;
					stack.Push(new VSMXNumber(this, f));
					if (o is VSMXReference)
					{
						((VSMXReference) o).assign(new VSMXNumber(this, f - 1f));
					}
					else
					{
						Console.WriteLine(string.Format("Line#{0:D} non-ref decrement {1}", pc - 1, code));
					}
					break;
				case VSMXCode.VID_OPERATOR_EQUAL:
					o1 = stack.Pop().Value;
					o2 = stack.Pop().Value;
					b = o1.Equals(o2);
					if (log.TraceEnabled)
					{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.trace(String.format("%s == %s: %b", o1, o2, b));
						log.trace(string.Format("%s == %s: %b", o1, o2, b));
					}
					stack.Push(VSMXBoolean.getValue(b));
					break;
				case VSMXCode.VID_OPERATOR_NOT_EQUAL:
					o1 = stack.Pop().Value;
					o2 = stack.Pop().Value;
					b = !o1.Equals(o2);
					if (log.TraceEnabled)
					{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.trace(String.format("%s != %s: %b", o1, o2, b));
						log.trace(string.Format("%s != %s: %b", o1, o2, b));
					}
					stack.Push(VSMXBoolean.getValue(b));
					break;
				case VSMXCode.VID_OPERATOR_IDENTITY:
					o1 = stack.Pop().Value;
					o2 = stack.Pop().Value;
					b = o1.identity(o2);
					stack.Push(VSMXBoolean.getValue(b));
					break;
				case VSMXCode.VID_OPERATOR_NON_IDENTITY:
					o1 = stack.Pop().Value;
					o2 = stack.Pop().Value;
					b = !o1.identity(o2);
					stack.Push(VSMXBoolean.getValue(b));
					break;
				case VSMXCode.VID_OPERATOR_LT:
					f1 = stack.Pop().FloatValue;
					f2 = stack.Pop().FloatValue;
					b = f2 < f1;
					stack.Push(VSMXBoolean.getValue(b));
					break;
				case VSMXCode.VID_OPERATOR_LTE:
					f1 = stack.Pop().FloatValue;
					f2 = stack.Pop().FloatValue;
					b = f2 <= f1;
					stack.Push(VSMXBoolean.getValue(b));
					break;
				case VSMXCode.VID_OPERATOR_GTE:
					f1 = stack.Pop().FloatValue;
					f2 = stack.Pop().FloatValue;
					b = f2 >= f1;
					stack.Push(VSMXBoolean.getValue(b));
					break;
				case VSMXCode.VID_OPERATOR_GT:
					f1 = stack.Pop().FloatValue;
					f2 = stack.Pop().FloatValue;
					b = f2 > f1;
					stack.Push(VSMXBoolean.getValue(b));
					break;
				case VSMXCode.VID_OPERATOR_INSTANCEOF:
					Console.WriteLine(string.Format("Line#{0:D} unimplemented {1}", pc - 1, code));
					break;
				case VSMXCode.VID_OPERATOR_IN:
					Console.WriteLine(string.Format("Line#{0:D} unimplemented {1}", pc - 1, code));
					break;
				case VSMXCode.VID_OPERATOR_TYPEOF:
					o = stack.Pop().Value;
					string typeOf = o.typeOf();
					stack.Push(new VSMXString(this, typeOf));
					break;
				case VSMXCode.VID_OPERATOR_B_AND:
					i1 = stack.Pop().IntValue;
					i2 = stack.Pop().IntValue;
					i = i1 & i2;
					stack.Push(new VSMXNumber(this, i));
					break;
				case VSMXCode.VID_OPERATOR_B_XOR:
					i1 = stack.Pop().IntValue;
					i2 = stack.Pop().IntValue;
					i = i1 ^ i2;
					stack.Push(new VSMXNumber(this, i));
					break;
				case VSMXCode.VID_OPERATOR_B_OR:
					i1 = stack.Pop().IntValue;
					i2 = stack.Pop().IntValue;
					i = i1 | i2;
					stack.Push(new VSMXNumber(this, i));
					break;
				case VSMXCode.VID_OPERATOR_B_NOT:
					i1 = stack.Pop().IntValue;
					i = ~i1;
					stack.Push(new VSMXNumber(this, i));
					break;
				case VSMXCode.VID_OPERATOR_LSHIFT:
					i1 = stack.Pop().IntValue;
					i2 = stack.Pop().IntValue;
					i = i2 << i1;
					stack.Push(new VSMXNumber(this, i));
					break;
				case VSMXCode.VID_OPERATOR_RSHIFT:
					i1 = stack.Pop().IntValue;
					i2 = stack.Pop().IntValue;
					i = i2 >> i1;
					stack.Push(new VSMXNumber(this, i));
					break;
				case VSMXCode.VID_OPERATOR_URSHIFT:
					i1 = stack.Pop().IntValue;
					i2 = stack.Pop().IntValue;
					i = (int)((uint)i2 >> i1);
					stack.Push(new VSMXNumber(this, i));
					break;
				case VSMXCode.VID_STACK_COPY:
					o1 = stack.Peek();
					stack.Push(o1);
					break;
				case VSMXCode.VID_STACK_SWAP:
					o1 = stack.Pop();
					o2 = stack.Pop();
					stack.Push(o1);
					stack.Push(o2);
					break;
				case VSMXCode.VID_END_STMT:
					stack.Clear();
					break;
				case VSMXCode.VID_CONST_NULL:
					stack.Push(VSMXNull.singleton);
					break;
				case VSMXCode.VID_CONST_EMPTYARRAY:
					o = new VSMXArray(this);
					stack.Push(o);
					break;
				case VSMXCode.VID_CONST_BOOL:
					stack.Push(VSMXBoolean.getValue(code.value));
					break;
				case VSMXCode.VID_CONST_INT:
					stack.Push(new VSMXNumber(this, code.value));
					break;
				case VSMXCode.VID_CONST_FLOAT:
					stack.Push(new VSMXNumber(this, code.FloatValue));
					break;
				case VSMXCode.VID_CONST_STRING:
					stack.Push(new VSMXString(this, mem.texts[code.value]));
					break;
				case VSMXCode.VID_CONST_OBJECT:
					break;
				case VSMXCode.VID_FUNCTION:
					stack.Push(new VSMXFunction(this, (code.id >> 8) & 0xFF, (code.id >> 24) & 0xFF, code.value));
					break;
				case VSMXCode.VID_ARRAY:
					stack.Push(new VSMXArray(this));
					break;
				case VSMXCode.VID_THIS:
					stack.Push(callState.ThisObject);
					break;
				case VSMXCode.VID_UNNAMED_VAR:
					stack.Push(new VSMXLocalVarReference(this, callState, code.value));
					break;
				case VSMXCode.VID_VARIABLE:
					stack.Push(new VSMXReference(this, globalVariables, mem.names[code.value]));
					if (log.TraceEnabled)
					{
						log.trace(string.Format("{0} '{1}'", VSMXCode.VsmxDecOps[code.Opcode], mem.names[code.value]));
					}
					break;
				case VSMXCode.VID_PROPERTY:
					o = stack.Pop().Value;
					if (o is VSMXObject)
					{
						stack.Push(new VSMXReference(this, (VSMXObject) o, mem.properties[code.value]));
						if (log.TraceEnabled)
						{
							log.trace(string.Format("{0} '{1}': {2}", VSMXCode.VsmxDecOps[code.Opcode], mem.properties[code.value], stack.Peek()));
						}
					}
					else
					{
						stack.Push(o.getPropertyValue(mem.properties[code.value]));
					}
					break;
				case VSMXCode.VID_METHOD:
					o = stack.Pop().Value;
					stack.Push(new VSMXMethod(this, o, mem.properties[code.value]));
					if (log.TraceEnabled)
					{
						log.trace(string.Format("{0} '{1}'", VSMXCode.VsmxDecOps[code.Opcode], mem.properties[code.value]));
					}
					break;
				case VSMXCode.VID_SET_ATTR:
					o1 = stack.Pop().Value;
					o2 = stack.Pop();
					o2.setPropertyValue(mem.properties[code.value], o1);
					if (log.TraceEnabled)
					{
						log.trace(string.Format("{0} {1}.{2} = {3}", VSMXCode.VsmxDecOps[code.Opcode], o2, mem.properties[code.value], o1));
					}
					break;
				case VSMXCode.VID_UNSET:
					o1 = stack.Pop();
					o1.deletePropertyValue(mem.properties[code.value]);
					break;
				case VSMXCode.VID_OBJ_ADD_ATTR:
					Console.WriteLine(string.Format("Line#{0:D} unimplemented {1}", pc - 1, code));
					break;
				case VSMXCode.VID_ARRAY_INDEX:
					o1 = stack.Pop();
					o2 = stack.Pop().Value;
					if (o2 is VSMXArray)
					{
						o = new VSMXReference(this, (VSMXObject) o2, o1.IntValue);
						if (log.TraceEnabled)
						{
							log.trace(string.Format("{0} VSMXArray {1}[{2:D}] = {3}", VSMXCode.VsmxDecOps[code.Opcode], o2, o1.IntValue, o));
						}
					}
					else if (o2 is VSMXObject)
					{
						o = new VSMXReference(this, (VSMXObject) o2, o1.StringValue);
						if (log.TraceEnabled)
						{
							log.trace(string.Format("{0} VSMXObject {1}[{2}] = {3}", VSMXCode.VsmxDecOps[code.Opcode], o2, o1.StringValue, o));
						}
					}
					else
					{
						o = o2.getPropertyValue(o1.StringValue);
						if (log.TraceEnabled)
						{
							log.trace(string.Format("{0} {1}[{2}] = {3}", VSMXCode.VsmxDecOps[code.Opcode], o2, o1.StringValue, o));
						}
					}
					stack.Push(o);
					break;
				case VSMXCode.VID_ARRAY_INDEX_KEEP_OBJ:
					o1 = stack.Pop();
					o2 = stack.Peek().Value;
					if (o2 is VSMXArray)
					{
						o = o2.getPropertyValue(o1.IntValue);
					}
					else
					{
						o = o2.getPropertyValue(o1.StringValue);
					}
					stack.Push(o);
					break;
				case VSMXCode.VID_ARRAY_INDEX_ASSIGN:
					o1 = stack.Pop().Value;
					o2 = stack.Pop();
					o3 = stack.Pop().Value;
					if (o3 is VSMXArray)
					{
						o3.setPropertyValue(o2.IntValue, o1);
					}
					else
					{
						Console.WriteLine(string.Format("Line#{0:D} non-array index assignment {1}", pc - 1, code));
					}
					break;
				case VSMXCode.VID_ARRAY_DELETE:
					o1 = stack.Pop();
					o2 = stack.Pop().Value;
					if (o2 is VSMXArray)
					{
						o2.deletePropertyValue(o1.IntValue);
					}
					else
					{
						Console.WriteLine(string.Format("Line#{0:D} non-array delete {1}", pc - 1, code));
					}
					break;
				case VSMXCode.VID_ARRAY_PUSH:
					o1 = stack.Pop().Value;
					o2 = stack.Pop().Value;
					if (o2 is VSMXArray)
					{
						int Length = ((VSMXArray) o2).Length;
						o2.setPropertyValue(Length, o1);
						stack.Push(o2);
					}
					else
					{
						Console.WriteLine(string.Format("Line#{0:D} non-array push {1}", pc - 1, code));
					}
					break;
				case VSMXCode.VID_JUMP:
					pc = code.value;
					break;
				case VSMXCode.VID_JUMP_TRUE:
					o1 = stack.Pop();
					b = o1.BooleanValue;
					if (b)
					{
						pc = code.value;
					}
					break;
				case VSMXCode.VID_JUMP_FALSE:
					o1 = stack.Pop();
					b = !o1.BooleanValue;
					if (b)
					{
						pc = code.value;
					}
					break;
				case VSMXCode.VID_CALL_FUNC:
					arguments = popValues(code.value);
					o = stack.Pop().getValueWithArguments(code.value);
					if (o is VSMXFunction)
					{
						VSMXFunction function = (VSMXFunction) o;

						callFunction(function, VSMXNull.singleton, arguments, code.value, false);
					}
					else
					{
						stack.Push(VSMXNull.singleton);
						Console.WriteLine(string.Format("Line#{0:D} non-function call {1}", pc - 1, code));
					}
					break;
				case VSMXCode.VID_CALL_METHOD:
					arguments = popValues(code.value);
					o = stack.Pop().getValueWithArguments(code.value);
					if (o is VSMXMethod)
					{
						VSMXMethod method = (VSMXMethod) o;
						VSMXFunction function = method.getFunction(code.value, arguments);

						if (function == null)
						{
							stack.Push(VSMXNull.singleton);
							Console.WriteLine(string.Format("Line#{0:D} non existing method {1}()", pc - 1, method.Name));
						}
						else
						{
							callFunction(function, method.ThisObject, method.Arguments, method.NumberOfArguments, false);
						}
					}
					else if (o is VSMXFunction)
					{
						VSMXFunction function = (VSMXFunction) o;
						o = stack.Pop().Value;
						callFunction(function, o, arguments, code.value, false);
					}
					else
					{
						stack.Push(VSMXNull.singleton);
						Console.WriteLine(string.Format("Line#{0:D} non-method call {1}", pc - 1, code));
					}
					break;
				case VSMXCode.VID_CALL_NEW:
					arguments = popValues(code.value);
					r = stack.Pop();
					o = r.Value;
					if (o is VSMXArray)
					{
						if (code.value == 0)
						{
							stack.Push(new VSMXArray(this));
						}
						else if (code.value == 1)
						{
							stack.Push(new VSMXArray(this, arguments[0].IntValue));
						}
						else
						{
							Console.WriteLine(string.Format("Line#{0:D} wrong number of arguments for new Array {1}", pc - 1, code));
						}
					}
					else if (o is VSMXFunction)
					{
						VSMXFunction function = (VSMXFunction) o;

						string className = null;
						if (r is VSMXReference)
						{
							className = ((VSMXReference) r).RefProperty;
						}
						VSMXObject thisObject = new VSMXObject(this, className);
						callFunction(function, thisObject, arguments, code.value, true);
					}
					else if (o is VSMXObject)
					{
						if (code.value == 0)
						{
							stack.Push(new VSMXObject(this, null));
						}
						else
						{
							Console.WriteLine(string.Format("Line#{0:D} wrong number of arguments for new Object {1}", pc - 1, code));
						}
					}
					else
					{
						stack.Push(new VSMXArray(this));
						Console.WriteLine(string.Format("Line#{0:D} unimplemented {1}", pc - 1, code));
					}
					break;
				case VSMXCode.VID_RETURN:
					o = stack.Pop().Value;
					if (callState.ReturnThis)
					{
						o = callState.ThisObject;
					}
					pc = callState.ReturnPc;
					if (callState.ExitAfterCall)
					{
						exit = true;
					}
					popCallState();
					if (callState == null)
					{
						exit = true;
					}
					else
					{
						stack.Push(o);
					}
					break;
				case VSMXCode.VID_THROW:
					Console.WriteLine(string.Format("Line#{0:D} unimplemented {1}", pc - 1, code));
					break;
				case VSMXCode.VID_TRY_BLOCK_IN:
					Console.WriteLine(string.Format("Line#{0:D} unimplemented {1}", pc - 1, code));
					break;
				case VSMXCode.VID_TRY_BLOCK_OUT:
					Console.WriteLine(string.Format("Line#{0:D} unimplemented {1}", pc - 1, code));
					break;
				case VSMXCode.VID_CATCH_FINALLY_BLOCK_IN:
					Console.WriteLine(string.Format("Line#{0:D} unimplemented {1}", pc - 1, code));
					break;
				case VSMXCode.VID_CATCH_FINALLY_BLOCK_OUT:
					Console.WriteLine(string.Format("Line#{0:D} unimplemented {1}", pc - 1, code));
					break;
				case VSMXCode.VID_END:
					exit = true;
					break;
				case VSMXCode.VID_DEBUG_FILE:
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("debug file '{0}'", mem.texts[code.value]));
					}
					break;
				case VSMXCode.VID_DEBUG_LINE:
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("debug line {0:D}", code.value));
					}
					break;
				case VSMXCode.VID_MAKE_FLOAT_ARRAY:
					Console.WriteLine(string.Format("Line#{0:D} unimplemented {1}", pc - 1, code));
					break;
				default:
					Console.WriteLine(string.Format("Line#{0:D} unimplemented {1}", pc - 1, code));
					break;
			}
		}

		private void interpret()
		{
			exit = false;

			while (!exit)
			{
				VSMXGroup code = mem.codes[pc];
				if (log.TraceEnabled)
				{
					log.trace(string.Format("{0}Interpret Line#{1:D}: {2}", prefix, pc, code));
				}
				pc++;
				interpret(code);
			}

			exit = false;
		}

		public virtual void run(VSMXObject globalVariables)
		{
			lock (this)
			{
				prefix = "";
				pc = 0;
				exit = false;
				callStates = new Stack<VSMXCallState>();
				pushCallState(VSMXNull.singleton, 0, false, true);
				this.globalVariables = globalVariables;
        
				VSMXBoolean.init(this);
        
				interpret();
        
				callStates.Clear();
				callState = null;
				prefix = "";
        
				if (log.TraceEnabled)
				{
					log.trace(string.Format("Global variables after run(): {0}", globalVariables));
				}
			}
		}

		private void callFunction(VSMXFunction function, VSMXBaseObject thisObject, VSMXBaseObject[] arguments, int numberArguments, bool returnThis)
		{
			pushCallState(thisObject, function.LocalVars + function.Args, returnThis, false);
			for (int i = 1; i <= function.Args && i <= numberArguments; i++)
			{
				callState.setLocalVar(i, arguments[i - 1]);
			}

			function.call(callState);

			int startLine = function.StartLine;
			if (startLine >= 0 && startLine < mem.codes.Length)
			{
				pc = startLine;
			}
			else
			{
				popCallState();

				VSMXBaseObject returnValue = function.ReturnValue;
				if (returnThis)
				{
					stack.Push(thisObject);
				}
				else if (returnValue != null)
				{
					stack.Push(returnValue);
				}
			}
		}

		public virtual void interpretFunction(VSMXFunction function, VSMXBaseObject @object, VSMXBaseObject[] arguments)
		{
			lock (this)
			{
				pushCallState(@object, function.LocalVars, false, true);
				for (int i = 1; i <= function.Args; i++)
				{
					if (arguments == null || i > arguments.Length)
					{
						callState.setLocalVar(i, VSMXNull.singleton);
					}
					else
					{
						callState.setLocalVar(i, arguments[i - 1]);
					}
				}
				pc = function.StartLine;
        
				interpret();
			}
		}

		public virtual void delayInterpretFunction(VSMXFunction function, VSMXBaseObject @object, VSMXBaseObject[] arguments)
		{
			lock (this)
			{
				IAction action = new InterpretFunctionAction(this, function, @object, arguments);
				Emulator.Scheduler.addAction(action);
			}
		}

		public virtual void interpretScript(VSMXBaseObject @object, string script)
		{
			lock (this)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("interpretScript {0} on {1}", script, @object));
				}
        
				if (string.ReferenceEquals(script, null))
				{
					return;
				}
        
				if (@object == null)
				{
					@object = VSMXNull.singleton;
				}
        
				string scriptPrefix = string.Format("script:/{0}/", name);
				if (script.StartsWith(scriptPrefix, StringComparison.Ordinal))
				{
					string functionName = script.Substring(scriptPrefix.Length);
					VSMXBaseObject functionObject = globalVariables.getPropertyValue(functionName);
					if (functionObject is VSMXFunction)
					{
						VSMXFunction function = (VSMXFunction) functionObject;
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("interpretScript function={0}", function));
						}
						interpretFunction(function, @object, null);
					}
				}
				else
				{
					Console.WriteLine(string.Format("interpretScript unknown script syntax '{0}'", script));
				}
			}
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();

			s.Append(string.Format("pc={0:D}", pc));
			s.Append(string.Format(", {0}", callState));

			return s.ToString();
		}
	}

}