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
	/// 
	/// <summary>
	/// @author hli
	/// </summary>
	public class Opcodes
	{

	// CPU: encoded by opcode field.
	//
	//     31---------26---------------------------------------------------0
	//     |  opcode   |                                                   |
	//     ------6----------------------------------------------------------
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	// 000 | *1    | *2    | J     | JAL   | BEQ   | BNE   | BLEZ  | BGTZ  |
	// 001 | ADDI  | ADDIU | SLTI  | SLTIU | ANDI  | ORI   | XORI  | LUI   |
	// 010 | *3    | *4    | VFPU2 | ---   | BEQL  | BNEL  | BLEZL | BGTZL |
	// 011 | VFPU0 | VFPU1 |  ---  | VFPU3 | * 5   | ---   | ---   | *6    |
	// 100 | LB    | LH    | LWL   | LW    | LBU   | LHU   | LWR   | ---   |
	// 101 | SB    | SH    | SWL   | SW    | ---   | ---   | SWR   | CACHE |
	// 110 | LL    | LWC1  | LVS   | ---   | VFPU4 | ULVQ  | LVQ   | VFPU5 |
	// 111 | SC    | SWC1  | SVS   | ---   | VFPU6 | USVQ  | SVQ   | VFPU7 |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
	//
	//      *1 = SPECIAL, see SPECIAL list    *2 = REGIMM, see REGIMM list
	//      *3 = COP0                         *4 = COP1
	//      *5 = SPECIAL2 , see SPECIAL2      *6 = SPECIAL3 , see SPECIAL 3
	//      *ULVQ is buggy on PSP1000 PSP
	//      *VFPU0 check VFPU0 table
	//      *VFPU1 check VFPU1 table
	//      *VFPU2 check VFPU2 table
	//      *VFPU3 check VFPU3 table
	//      *VFPU4 check VFPU4 table
	//      *VFPU5 check VFPU5 table
	//      *VFPU6 check VFPU6 table
	//      *VFPU7 check VFPU7 table
		public const sbyte SPECIAL = 0x0;
		public const sbyte REGIMM = 0x1;
		public const sbyte J = 0x2; // Jump
		public const sbyte JAL = 0x3; // Jump And Link
		public const sbyte BEQ = 0x4; // Branch on Equal
		public const sbyte BNE = 0x5; // Branch on Not Equal
		public const sbyte BLEZ = 0x6; // Branch on Less Than or Equal to Zero
		public const sbyte BGTZ = 0x7; // Branch on Greater Than Zero
		public const sbyte ADDI = 0x8; // Add Immediate
		public const sbyte ADDIU = 0x9; // Add Immediate Unsigned
		public const sbyte SLTI = 0xa; // Set on Less Than Immediate
		public const sbyte SLTIU = 0xb; // Set on Less Than Immediate Unsigned
		public const sbyte ANDI = 0xc; // AND Immediate
		public const sbyte ORI = 0xd; // OR Immediate
		public const sbyte XORI = 0xe; // Exclusive OR Immediate
		public const sbyte LUI = 0xf; // Load Upper Immediate
		public const sbyte COP0 = 0x10; // Coprocessor Operation
		public const sbyte COP1 = 0x11; // Coprocessor Operation
		public const sbyte VFPU2 = 0x12;
		/*  0x13 reserved or unsupported */
		public const sbyte BEQL = 0x14; // Branch on Equal Likely
		public const sbyte BNEL = 0x15; // Branch on Not Equal Likely
		public const sbyte BLEZL = 0x16; // Branch on Less Than or Equal to Zero Likely
		public const sbyte BGTZL = 0x17; // Branch on Greater Than Zero Likely
		public const sbyte VFPU0 = 0x18;
		public const sbyte VFPU1 = 0x19;
		public const sbyte ME1 = 0x1A; // Only for the ME
		public const sbyte VFPU3 = 0x1b;
		public const sbyte SPECIAL2 = 0x1c; // Allegrex table
		/*  0x1d reserved or unsupported */
		/*  0x1e reserved or unsupported */
		public const sbyte SPECIAL3 = 0x1f; //special3 table
		public const sbyte LB = 0x20; //Load Byte
		public const sbyte LH = 0x21; // Load Halfword
		public const sbyte LWL = 0x22; // Load Word Left
		public const sbyte LW = 0x23; // Load Word
		public const sbyte LBU = 0x24; // Load Byte Unsigned
		public const sbyte LHU = 0x25; // Load Halfword Unsigned
		public const sbyte LWR = 0x26; // Load Word Right
		/*  0x27 reserved or unsupported */
		public const sbyte SB = 0x28; // Store Byte
		public const sbyte SH = 0x29; // Store Halfword
		public const sbyte SWL = 0x2A; // Store Word Left
		public const sbyte SW = 0x2B; // Store Word
		public const sbyte ME2 = 0x2C; // Only for the ME
		/*  0x2d reserved or unsupported */
		public const sbyte SWR = 0x2E; // Store Word Right
		public const sbyte CACHE = 0x2f; // Allegrex Cache Operation
		public const sbyte LL = 0x30; // Load Linked
		public const sbyte LWC1 = 0x31; // Load FPU Register
		public const sbyte LVS = 0x32; // Load Scalar VFPU Register
		/*  0x32 reserved or unsupported */
		/*  0x33 reserved or unsupported */
		public const sbyte VFPU4 = 0x34;
		public const sbyte ULVQ = 0x35; // Load Quad VFPU Register (Unaligned)
		public const sbyte LVQ = 0x36; // Load Quad VFPU Register
		public const sbyte VFPU5 = 0x37;
		public const sbyte SC = 0x38; // Store Conditionaly
		public const sbyte SWC1 = 0x39; // Store FPU Register
		public const sbyte SVS = 0x3a; // Store Scalar VFPU Register
		/*  0x3b reserved or unsupported */
		public const sbyte VFPU6 = 0x3c;
		public const sbyte USVQ = 0x3d; // Store Quad VFPU Register (Unaligned)
		public const sbyte SVQ = 0x3e; // Store Quad VFPU Register
		public const sbyte VFPU7 = 0x3f; // SPECIAL: encoded by function field when opcode field = SPECIAL
	//
	//     31---------26------------------------------------------5--------0
	//     |=   SPECIAL|                                         | function|
	//     ------6----------------------------------------------------6-----
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	// 000 | SLL   | ---   |SRLROR | SRA   | SLLV  |  ---  |SRLRORV| SRAV  |
	// 001 | JR    | JALR  | MOVZ  | MOVN  |SYSCALL| BREAK |  ---  | SYNC  |
	// 010 | MFHI  | MTHI  | MFLO  | MTLO  | ---   |  ---  |  CLZ  | CLO   |
	// 011 | MULT  | MULTU | DIV   | DIVU  | MADD  | MADDU | ----  | ----- |
	// 100 | ADD   | ADDU  | SUB   | SUBU  | AND   | OR    | XOR   | NOR   |
	// 101 | ---   |  ---  | SLT   | SLTU  | MAX   | MIN   | MSUB  | MSUBU |
	// 110 | ---   |  ---  | ---   | ---   | ---   |  ---  | ---   | ---   |
	// 111 | ---   |  ---  | ---   | ---   | ---   |  ---  | ---   | ---   |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
		public const sbyte SLL = 0x0; // Shift Left Logical
		/*  0x1 reserved or unsupported */
		public const sbyte SRLROR = 0x2; // Shift/Rotate Right Logical
		public const sbyte SRA = 0x3; // Shift Right Arithmetic
		public const sbyte SLLV = 0x4; // Shift Left Logical Variable
		/*  0x5 reserved or unsupported */
		public const sbyte SRLRORV = 0x6; // Shift/Rotate Right Logical Variable
		public const sbyte SRAV = 0x7; // Shift Right Arithmetic Variable
		public const sbyte JR = 0x8; // Jump Register
		public const sbyte JALR = 0x9; // Jump And Link Register
		public const sbyte MOVZ = 0xa; // Move If Zero
		public const sbyte MOVN = 0xb; // Move If Non-zero
		public const sbyte SYSCALL = 0xc; // System Call
		public const sbyte BREAK = 0xd; // Break
		/*  0xe reserved or unsupported */
		public const sbyte SYNC = 0xf; // Sync
		public const sbyte MFHI = 0x10; // Move From HI
		public const sbyte MTHI = 0x11; // Move To HI
		public const sbyte MFLO = 0x12; // Move From LO
		public const sbyte MTLO = 0x13; // Move To LO
		/*  0x14 reserved or unsupported */
		/*  0x15 reserved or unsupported */
		public const sbyte CLZ = 0x16; // Count Leading Zero
		public const sbyte CLO = 0x17; // Count Leading One
		public const sbyte MULT = 0x18; // Multiply
		public const sbyte MULTU = 0x19; // Multiply Unsigned
		public const sbyte DIV = 0x1a; // Divide
		public const sbyte DIVU = 0x1b; // Divide Unsigned
		public const sbyte MADD = 0x1c; // Multiply And Add
		public const sbyte MADDU = 0x1d; // Multiply And Add Unsigned
		/*  0x1e reserved or unsupported */
		/*  0x1f reserved or unsupported */
		public const sbyte ADD = 0x20; // Add
		public const sbyte ADDU = 0x21; // Add Unsigned
		public const sbyte SUB = 0x22; // Subtract
		public const sbyte SUBU = 0x23; // Subtract Unsigned
		public const sbyte AND = 0x24; // AND
		public const sbyte OR = 0x25; // OR
		public const sbyte XOR = 0x26; // Exclusive OR
		public const sbyte NOR = 0x27; // NOR
		/*  0x28 reserved or unsupported */
		/*  0x29 reserved or unsupported */
		public const sbyte SLT = 0x2a; // Set on Less Than
		public const sbyte SLTU = 0x2b; // Set on Less Than Unsigned
		public const sbyte MAX = 0x2c; // Move Max
		public const sbyte MIN = 0x2d; // Move Min
		public const sbyte MSUB = 0x2e; // Multiply And Substract
		public const sbyte MSUBU = 0x2f; // Multiply And Substract

	// SPECIAL rs : encoded by rs field when opcode/func field = SPECIAL/SRLROR
	//
	//     31---------26-----21-----------------------------------5--------0
	//     |=   SPECIAL| rs  |                                    |= SRLROR|
	//     ------6--------5-------------------------------------------6-----
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	// 000 | SRL   | ROTR  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 001 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 010 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 011 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
		public const sbyte SRL = 0x0;
		public const sbyte ROTR = 0x1; // SPECIAL sa : encoded by sa field when opcode/func field = SPECIAL/SRLRORV
	//
	//     31---------26------------------------------------10----5--------0
	//     |=   SPECIAL|                                    | sa  |=SRLRORV|
	//     ------6---------------------------------------------5------6-----
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	// 000 | SRLV  | ROTRV |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 001 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 010 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 011 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
		public const sbyte SRLV = 0x0;
		public const sbyte ROTRV = 0x1;
	//     REGIMM: encoded by the rt field when opcode field = REGIMM.
	//     31---------26----------20-------16------------------------------0
	//     |=    REGIMM|          |   rt    |                              |
	//     ------6---------------------5------------------------------------
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	//  00 | BLTZ  | BGEZ  | BLTZL | BGEZL |  ---  |  ---  |  ---  |  ---  |
	//  01 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  10 | BLTZAL| BGEZAL|BLTZALL|BGEZALL|  ---  |  ---  |  ---  |  ---  |
	//  11 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
		public const sbyte BLTZ = 0x0; // Branch on Less Than Zero
		public const sbyte BGEZ = 0x1; // Branch on Greater Than or Equal to Zero
		public const sbyte BLTZL = 0x2; // Branch on Less Than Zero Likely
		public const sbyte BGEZL = 0x3; // Branch on Greater Than or Equal to Zero Likely
		/*  0x4 reserved or unsupported */
		/*  0x5 reserved or unsupported */
		/*  0x6 reserved or unsupported */
		/*  0x7 reserved or unsupported */
		/*  0x8 reserved or unsupported */
		/*  0x9 reserved or unsupported */
		/*  0xa reserved or unsupported */
		/*  0xb reserved or unsupported */
		/*  0xc reserved or unsupported */
		/*  0xd reserved or unsupported */
		/*  0xe reserved or unsupported */
		/*  0xf reserved or unsupported */
		public const sbyte BLTZAL = 0x10; // Branch on Less Than Zero And Link
		public const sbyte BGEZAL = 0x11; // Branch on Greater Than or Equal to Zero And Link
		public const sbyte BLTZALL = 0x12; // Branch on Less Than Zero And Link Likely
		public const sbyte BGEZALL = 0x13; // Branch on Greater Than or Equal to Zero And Link Likely
	//     COP0: encoded by the rs field when opcode field = COP0.
	//     31---------26----------23-------31------------------------------0
	//     |=      COP0|          |   rs    |                              |
	//     ------6---------------------5------------------------------------
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	//  00 |  MFC0 |  ---  |  CFC0 |  ---  |  MTC0 |  ---  |  CTC0 |  ---  |
	//  01 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  10 |  *1   |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  11 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
	//
	//     *1 COP0 func
		public const sbyte MFC0 = 0x0; // Move from Coprocessor 0
		public const sbyte CFC0 = 0x2; // Move from Coprocessor 0
		public const sbyte MTC0 = 0x4; // Move to Coprocessor 0
		public const sbyte CTC0 = 0x6; // Move to Coprocessor 0
		public const sbyte COP0ERET = 0x10; //     COP0: encoded by the func field when opcode/rs field = COP0/10000.
	//     31---------26------------------------------------------5--------0
	//     |=      COP0|                                         | function|
	//     ------6----------------------------------------------------6-----
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	//  00 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  01 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  10 |  ERET |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  11 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
		public const sbyte ERET = 0x10; // Exception Return            */

	//     SPECIAL2 : encoded by function field when opcode field = SPECIAL2
	//     31---------26------------------------------------------5--------0
	//     |=  SPECIAL2|                                         | function|
	//     ------6----------------------------------------------------6-----
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	// 000 | HALT  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 001 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 010 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 011 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 100 |  ---  |  ---  |  ---  |  ---  | MFIC  |  ---  | MTIC  |  ---  |
	// 101 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 110 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 111 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
		public const sbyte HALT = 0x0; // halt execution until next interrupt
		public const sbyte MFIC = 0x24; // move from IC (Interrupt) register
		public const sbyte MTIC = 0x26; // move to IC (Interrupt) register
		public const sbyte DBREAK = 0x3F; // Debugging break, only for the ME

	//     SPECIAL3: encoded by function field when opcode field = SPECIAL3
	//     31---------26------------------------------------------5--------0
	//     |=  SPECIAL3|                                         | function|
	//     ------6----------------------------------------------------6-----
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	// 000 |  EXT  |  ---  |  ---  |  ---  |  INS  |  ---  |  ---  |  ---  |
	// 001 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 010 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 011 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 100 |  *1   |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 101 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 110 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 111 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
	//       * 1 BSHFL encoding based on sa field
		public const sbyte EXT = 0x0; // extract bit field
		public const sbyte INS = 0x4; // insert bit field
		public const sbyte BSHFL = 0x20; //BSHFL table
	//     BSHFL: encoded by the sa field.
	//     31---------26----------20-------16--------------8---6-----------0
	//     |          |          |         |               | sa|           |
	//     ------6---------------------5------------------------------------
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	//  00 |  ---  |  ---  | WSBH  | WSBW  |  ---  |  ---  |  ---  |  ---  |
	//  01 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  10 |  SEB  |  ---  |  ---  |  ---  |BITREV |  ---  |  ---  |  ---  |
	//  11 |  SEH  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
		public const sbyte WSBH = 0x02; // Swap Bytes In Each Half Word
		public const sbyte WSBW = 0x03; // Swap Bytes In Word
		public const sbyte SEB = 0x10; // Sign-Extend Byte
		public const sbyte BITREV = 0x14; // Revert Bits In Word
		public const sbyte SEH = 0x18; // Sign-Extend HalfWord

	//     COP1: encoded by the rs field when opcode field = COP1.
	//     31-------26------21---------------------------------------------0
	//     |=    COP1|  rs  |                                              |
	//     -----6-------5---------------------------------------------------
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	//  00 |  MFC1 |  ---  |  CFC1 |  ---  |  MTC1 |  ---  |  CTC1 |  ---  |
	//  01 |  *1   |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  10 |  *2   |  ---  |  ---  |  ---  |  *3   |  ---  |  ---  |  ---  |
	//  11 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
	//    *1 check COP1BC table
	//    *2 check COP1S table;
	//    *2 check COP1W table;
		public const sbyte MFC1 = 0x00;
		public const sbyte CFC1 = 0x02;
		public const sbyte MTC1 = 0x04;
		public const sbyte CTC1 = 0x06;
		public const sbyte COP1BC = 0x08;
		public const sbyte COP1S = 0x10;
		public const sbyte COP1W = 0x14;
	//     COP1BC: encoded by the rt field
	//     31---------21-------16------------------------------------------0
	//     |=    COP1BC|  rt   |                                           |
	//     ------11---------5-----------------------------------------------
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	//  00 |  BC1F | BC1T  | BC1FL | BC1TL |  ---  |  ---  |  ---  |  ---  |
	//  01 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  10 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  11 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
		public const sbyte BC1F = 0x00;
		public const sbyte BC1T = 0x01;
		public const sbyte BC1FL = 0x02;
		public const sbyte BC1TL = 0x03;
	//     COP1S: encoded by function field
	//     31---------21------------------------------------------5--------0
	//     |=  COP1S  |                                          | function|
	//     -----11----------------------------------------------------6-----
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	// 000 | add.s | sub.s | mul.s | div.s |sqrt.s | abs.s | mov.s | neg.s |
	// 001 |  ---  |  ---  |  ---  |  ---  |            <*1>.w.s           |
	// 010 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 011 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 100 |  ---  |  ---  |  ---  |  ---  |cvt.w.s|  ---  |  ---  |  ---  |
	// 101 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 110 |                            c.<*2>.s                           |
	// 110 |                            c.<*3>.s                           |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
	//
	// *1 : round.w.s | trunc.w.s | ceil.w.s | floor.w.s
	// *2 : c.f.s | c.un.s | c.eq.s | c.ueq.s | c.olt.s | c.ult.s | c.ole.s | c.ule.s
	// *3 : c.sf.s | c.ngle.s | c.seq.s | c.ngl.s | c.lt.s | c.nge.s | c.le.s  | c.ngt.s
	//
		public const sbyte ADDS = 0x00;
		public const sbyte SUBS = 0x01;
		public const sbyte MULS = 0x02;
		public const sbyte DIVS = 0x03;
		public const sbyte SQRTS = 0x04;
		public const sbyte ABSS = 0x05;
		public const sbyte MOVS = 0x06;
		public const sbyte NEGS = 0x07;
		public const sbyte ROUNDWS = 0xc;
		public const sbyte TRUNCWS = 0xd;
		public const sbyte CEILWS = 0xe;
		public const sbyte FLOORWS = 0xf;
		public const sbyte CVTWS = 0x24;
		public const sbyte CCONDS = 0x30;
		public const sbyte CF = 0x0;
		public const sbyte CUN = 0x1;
		public const sbyte CEQ = 0x2;
		public const sbyte CUEQ = 0x3;
		public const sbyte COLT = 0x4;
		public const sbyte CULT = 0x5;
		public const sbyte COLE = 0x6;
		public const sbyte CULE = 0x7;
		public const sbyte CSF = 0x8;
		public const sbyte CNGLE = 0x9;
		public const sbyte CSEQ = 0xa;
		public const sbyte CNGL = 0xb;
		public const sbyte CLT = 0xc;
		public const sbyte CNGE = 0xd;
		public const sbyte CLE = 0xe;
		public const sbyte CNGT = 0xf;
	//     COP1W: encoded by function field
	//     31---------21------------------------------------------5--------0
	//     |=  COP1W  |                                          | function|
	//     -----11----------------------------------------------------6-----
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--| lo
	// 000 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 001 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 010 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 011 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 100 |cvt.s.w|  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 101 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 110 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	// 110 |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//  hi |-------|-------|-------|-------|-------|-------|-------|-------|
		public const sbyte CVTSW = 0x20;
	// VFPU2: /* known as COP2 */
		public const sbyte MFVMFVC = 0x00;
		public const sbyte MTVMTVC = 0x04;
		public const sbyte VFPU2BC = 0x08;
		public const sbyte MFV = 0x0;
		public const sbyte MFVC = 0x1;
		public const sbyte MTV = 0x0;
		public const sbyte MTVC = 0x1;
	// VFPU0:
	//
	//     31---------26-----23--------------------------------------------0
	//     |=     VFPU0| VOP |                                             |
	//     ------6--------3-------------------------------------------------
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
	//     | VADD  | VSUB  | VSBN  |  ---  |  ---  |  ---  |  ---  | VDIV  |
	//     |-------|-------|-------|-------|-------|-------|-------|-------|
		public const sbyte VADD = 0x00;
		public const sbyte VSUB = 0x01;
		public const sbyte VSBN = 0x02;
		public const sbyte VDIV = 0x07;
	// VFPU1:
	//
	//     31---------26-----23--------------------------------------------0
	//     |=     VFPU1| VOP |                                             |
	//     ------6--------3-------------------------------------------------
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
	//     | VMUL  | VDOT  | VSCL  |  ---  | VHDP  | VCRS  | VDET  |  ---  |
	//     |-------|-------|-------|-------|-------|-------|-------|-------|
		public const sbyte VMUL = 0x00;
		public const sbyte VDOT = 0x01;
		public const sbyte VSCL = 0x02;
		public const sbyte VHDP = 0x04;
		public const sbyte VCRS = 0x05;
		public const sbyte VDET = 0x06;

	// VFPU3:
	//
	//     31---------26-----23--------------------------------------------0
	//     |=     VFPU3| VOP |                                             |
	//     ------6--------3-------------------------------------------------
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
	//     | VCMP  |  ---  | VMIN  | VMAX  |  ---  | VSCMP | VSGE  | VSLT  |
	//     |-------|-------|-------|-------|-------|-------|-------|-------|
		public const sbyte VCMP = 0x00;
		public const sbyte VMIN = 0x02;
		public const sbyte VMAX = 0x03;
		public const sbyte VSCMP = 0x05;
		public const sbyte VSGE = 0x06;
		public const sbyte VSLT = 0x07;
	// VFPU4:
	//     31---------26-----24--------------------------------------------0
	//     |=     VFPU4| VOP |                                             |
	//     ------6--------2-------------------------------------------------
	//     |-------00-------|-------01-------|------10------|------11------|
	//     |    VFPU4_0     |      ---       |    VFPU4_2   |     VWBN     |
	//     |----------------|----------------|--------------|--------------|
		public const sbyte VFPU4_0 = 0x0;
		public const sbyte VFPU4_1 = 0x1;
		public const sbyte VFPU4_2 = 0x2;
		public const sbyte VWBN = 0x3;

	// VFPU4_0:
	//     31---------26-----24--------------------------------------------0
	//     |=     VFPU4| 00  |                                             |
	//     ------6--------2-------------------------------------------------
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
	//     |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//     |-------|-------|-------|-------|-------|-------|-------|-------|
	//

	// VFPU4_2:
	//     31---------26-----24--------------------------------------------0
	//     |=     VFPU4| 10  |                                             |
	//     ------6--------2-------------------------------------------------
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
	//     | VF2IN | VF2IZ | VF2IU | VF2ID | VI2F  |   1*  |  ---  |  ---  |
	//     |-------|-------|-------|-------|-------|-------|-------|-------|
	//     *1 : VCMOVF/VCMOVT
		public const sbyte VF2IN = 0x0;
		public const sbyte VF2IZ = 0x1;
		public const sbyte VF2IU = 0x2;
		public const sbyte VF2ID = 0x3;
		public const sbyte VI2F = 0x4;
		public const sbyte VFPU4_2_2 = 0x5;

	// VFPU4_2_2:
	//     31---------26----24----19---------------------------------------0
	//     |=     VFPU4| 10 | 101 |                                        |
	//     ------6--------2----3--------------------------------------------
	//     |-------00-------|-------01-------|------10------|------11------|
	//     |     VCMOVF     |     VCMOVT     |     ----     |     ----     |
	//     |----------------|----------------|--------------|--------------|
		public const sbyte VCMOVF = 0x0;
		public const sbyte VCMOVT = 0x1;

	// VFPU5:
	//
	//     31---------26----24---------------------------------------------0
	//     |=     VFPU5| VOP |                                             |
	//     ------6--------2-------------------------------------------------
	//     |-------00-------|-------01-------|-------10-----|------11------|
	//     |     VPFXS      |     VPFXT      |     VPFXD    |  VIIM/VFIM   |
	//     |----------------|----------------|--------------|--------------|
		public const sbyte VPFXS = 0x00;
		public const sbyte VPFXT = 0x01;
		public const sbyte VPFXD = 0x02;
		public const sbyte VFPU5_3 = 0x03;
	//     31---------------23---------------------------------------------0
	//     |=   VFPU5/VIFM  |                                              |
	//     ---------8-------------------------------------------------------
	//     |----------------0----------------|--------------1--------------|
	//     |              VIIM               |            VFIM             |
	//     |---------------------------------|-----------------------------|
		public const sbyte VIIM = 0x0;
		public const sbyte VFIM = 0x1; // VFPU6:
	//
	//     31---------26-----23--------------------------------------------0
	//     |=     VFPU6| VOP |                                             |
	//     ------6--------3-------------------------------------------------
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
	//     |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//     |-------|-------|-------|-------|-------|-------|-------|-------|
	//

	// VFPU7:
	//
	//     31---------26-----23--------------------------------------------0
	//     |=     VFPU6| VOP |                                             |
	//     ------6--------3-------------------------------------------------
	//     |--000--|--001--|--010--|--011--|--100--|--101--|--110--|--111--|
	//     |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |  ---  |
	//     |-------|-------|-------|-------|-------|-------|-------|-------|
	//
	}
}