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
	public class VSMXCode
	{
		public const int VID_NOTHING = 0x0; // dummy
		public const int VID_OPERATOR_ASSIGN = 0x1;
		public const int VID_OPERATOR_ADD = 0x2;
		public const int VID_OPERATOR_SUBTRACT = 0x3;
		public const int VID_OPERATOR_MULTIPLY = 0x4;
		public const int VID_OPERATOR_DIVIDE = 0x5;
		public const int VID_OPERATOR_MOD = 0x6;
		public const int VID_OPERATOR_POSITIVE = 0x7;
		public const int VID_OPERATOR_NEGATE = 0x8;
		public const int VID_OPERATOR_NOT = 0x9;
		public const int VID_P_INCREMENT = 0xa;
		public const int VID_P_DECREMENT = 0xb;
		public const int VID_INCREMENT = 0xc;
		public const int VID_DECREMENT = 0xd;
		public const int VID_OPERATOR_EQUAL = 0xe;
		public const int VID_OPERATOR_NOT_EQUAL = 0xf;
		public const int VID_OPERATOR_IDENTITY = 0x10; // === operator; used in case labels
		public const int VID_OPERATOR_NON_IDENTITY = 0x11;
		public const int VID_OPERATOR_LT = 0x12;
		public const int VID_OPERATOR_LTE = 0x13;
		public const int VID_OPERATOR_GTE = 0x14;
		public const int VID_OPERATOR_GT = 0x15;

		public const int VID_OPERATOR_INSTANCEOF = 0x16;
		public const int VID_OPERATOR_IN = 0x17;
		public const int VID_OPERATOR_TYPEOF = 0x18;
		public const int VID_OPERATOR_B_AND = 0x19;
		public const int VID_OPERATOR_B_XOR = 0x1a;
		public const int VID_OPERATOR_B_OR = 0x1b;
		public const int VID_OPERATOR_B_NOT = 0x1c;
		public const int VID_OPERATOR_LSHIFT = 0x1d;
		public const int VID_OPERATOR_RSHIFT = 0x1e;
		public const int VID_OPERATOR_URSHIFT = 0x1f;
		public const int VID_STACK_COPY = 0x20;
		public const int VID_STACK_SWAP = 0x21;

		public const int VID_END_STMT = 0x22;
		public const int VID_CONST_NULL = 0x23;
		public const int VID_CONST_EMPTYARRAY = 0x24;
		public const int VID_CONST_BOOL = 0x25;
		public const int VID_CONST_INT = 0x26;
		public const int VID_CONST_FLOAT = 0x27;
		public const int VID_CONST_STRING = 0x28;
		public const int VID_CONST_OBJECT = 0x29;
		public const int VID_FUNCTION = 0x2a;
		public const int VID_ARRAY = 0x2b; // start an array constant
		public const int VID_THIS = 0x2c;
		public const int VID_UNNAMED_VAR = 0x2d;
		public const int VID_VARIABLE = 0x2e;
		public const int VID_PROPERTY = 0x2f;
		public const int VID_METHOD = 0x30;
		public const int VID_SET_ATTR = 0x31; // appears to be an object set; pops last two
		// items off the stack
		public const int VID_UNSET = 0x32; // guess; looks like above; but only with one
		// item
		public const int VID_OBJ_ADD_ATTR = 0x33;
		public const int VID_ARRAY_INDEX = 0x34;
		public const int VID_ARRAY_INDEX_KEEP_OBJ = 0x35;
		public const int VID_ARRAY_INDEX_ASSIGN = 0x36;
		public const int VID_ARRAY_DELETE = 0x37;
		public const int VID_ARRAY_PUSH = 0x38; // push something into array constant
		public const int VID_JUMP = 0x39; // jump statement; can indicate end of
															// function; end of else/for; or return to
															// beginning of loop
		public const int VID_JUMP_TRUE = 0x3a; // jump if previous value is true
		public const int VID_JUMP_FALSE = 0x3b;
		public const int VID_CALL_FUNC = 0x3c;
		public const int VID_CALL_METHOD = 0x3d;
		public const int VID_CALL_NEW = 0x3e;
		public const int VID_RETURN = 0x3f;

		public const int VID_THROW = 0x40;
		public const int VID_TRY_BLOCK_IN = 0x41;
		public const int VID_TRY_BLOCK_OUT = 0x42;
		public const int VID_CATCH_FINALLY_BLOCK_IN = 0x43;
		public const int VID_CATCH_FINALLY_BLOCK_OUT = 0x44;

		public const int VID_END = 0x45;
		public const int VID_DEBUG_FILE = 0x46;
		public const int VID_DEBUG_LINE = 0x47;

		public const int VID_MAKE_FLOAT_ARRAY = 0x49; // weird??

		public static readonly string[] VsmxDecOps = new string[] {"UNKNOWN_0", "ASSIGN", "ADD", "SUBTRACT", "MULTIPLY", "DIVIDE", "MODULUS", "POSITIVE", "NEGATE", "NOT", "PRE_INCREMENT", "PRE_DECREMENT", "INCREMENT", "DECREMENT", "TEST_EQUAL", "TEST_NOT_EQUAL", "TEST_IDENTITY", "TEST_NON_IDENTITY", "TEST_LESS_THAN", "TEST_LESS_EQUAL_THAN", "TEST_MORE_EQUAL_THAN", "TEST_MORE_THAN", "INSTANCEOF", "IN", "TYPEOF", "BINARY_AND", "BINARY_XOR", "BINARY_OR", "BINARY_NOT", "LSHIFT", "RSHIFT", "UNSIGNED_RSHIFT", "STACK_COPY", "STACK_SWAP", "END_STATEMENT", "CONST_NULL", "CONST_EMPTY_ARRAY", "CONST_BOOL", "CONST_INT", "CONST_FLOAT", "CONST_STRING", "CONST_OBJECT", "FUNCTION", "CONST_ARRAY", "THIS_OBJECT", "UNNAMED_VARIABLE", "NAME", "PROPERTY", "METHOD", "SET", "UNSET", "OBJECT_ADD_ATTRIBUTE", "ARRAY_INDEX", "ARRAY_INDEX_KEEP_OBJ", "ARRAY_INDEX_ASSIGN", "ARRAY_DELETE", "ARRAY_PUSH", "JUMP", "JUMP_IF_TRUE", "JUMP_IF_FALSE", "CALL_FUNCTION", "CALL_METHOD", "CALL_NEW", "RETURN", "THROW", "TRY_BLOCK_IN", "TRY_BLOCK_OUT", "CATCH_FINALLY_BLOCK_IN", "CATCH_FINALLY_BLOCK_OUT", "END_SCRIPT", "DEBUG_FILE", "DEBUG_LINE", "UNKNOWN_48", "MAKE_FLOAT_ARRAY"};
	}

}