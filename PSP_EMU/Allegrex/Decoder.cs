namespace pspsharp.Allegrex
{
	using Instruction = pspsharp.Allegrex.Common.Instruction;
	using STUB = pspsharp.Allegrex.Common.STUB;

	public class Decoder
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public static final pspsharp.Allegrex.Common.Instruction table_0[] = { new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { return table_1[(insn >> 0) & 0x0000003f].instance(insn); } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { return table_2[(insn >> 16) & 0x00000003].instance(insn); } }, pspsharp.Allegrex.Instructions.J, pspsharp.Allegrex.Instructions.JAL, pspsharp.Allegrex.Instructions.BEQ, pspsharp.Allegrex.Instructions.BNE, pspsharp.Allegrex.Instructions.BLEZ, pspsharp.Allegrex.Instructions.BGTZ, pspsharp.Allegrex.Instructions.ADDI, pspsharp.Allegrex.Instructions.ADDIU, pspsharp.Allegrex.Instructions.SLTI, pspsharp.Allegrex.Instructions.SLTIU, pspsharp.Allegrex.Instructions.ANDI, pspsharp.Allegrex.Instructions.ORI, pspsharp.Allegrex.Instructions.XORI, pspsharp.Allegrex.Instructions.LUI, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { return table_3[(insn >> 22) & 0x00000003].instance(insn); } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { return table_4[(insn >> 23) & 0x00000007].instance(insn); } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00200000) == 0x00000000) { return table_7[(insn >> 16) & 0x00000003].instance(insn); } if((insn & 0x00000080) == 0x00000000) { if((insn & 0x00800000) == 0x00000000) { return pspsharp.Allegrex.Instructions.MFV; } return pspsharp.Allegrex.Instructions.MTV; } if((insn & 0x00800000) == 0x00000000) { return pspsharp.Allegrex.Instructions.MFVC; } return pspsharp.Allegrex.Instructions.MTVC; } }, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Instructions.BEQL, pspsharp.Allegrex.Instructions.BNEL, pspsharp.Allegrex.Instructions.BLEZL, pspsharp.Allegrex.Instructions.BGTZL, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { return table_8[(insn >> 23) & 0x00000003].instance(insn); } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { return table_9[(insn >> 23) & 0x00000007].instance(insn); } }, pspsharp.Allegrex.Instructions.MFVME, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { return table_10[(insn >> 23) & 0x00000007].instance(insn); } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00000002) == 0x00000000) { if((insn & 0x00000004) == 0x00000000) { return pspsharp.Allegrex.Instructions.HALT; } return pspsharp.Allegrex.Instructions.MFIC; } if((insn & 0x0000003F) == 0x0000003F) { return pspsharp.Allegrex.Instructions.DBREAK; } return pspsharp.Allegrex.Instructions.MTIC; } }, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00000020) == 0x00000020) { if((insn & 0x00000080) == 0x00000000) { if((insn & 0x00000100) == 0x00000000) { if((insn & 0x00000200) == 0x00000000) { return pspsharp.Allegrex.Instructions.SEB; } return pspsharp.Allegrex.Instructions.SEH; } return pspsharp.Allegrex.Instructions.BITREV; } if((insn & 0x00000040) == 0x00000000) { return pspsharp.Allegrex.Instructions.WSBH; } return pspsharp.Allegrex.Instructions.WSBW; } if((insn & 0x00000004) == 0x00000000) { return pspsharp.Allegrex.Instructions.EXT; } return pspsharp.Allegrex.Instructions.INS; } }, pspsharp.Allegrex.Instructions.LB, pspsharp.Allegrex.Instructions.LH, pspsharp.Allegrex.Instructions.LWL, pspsharp.Allegrex.Instructions.LW, pspsharp.Allegrex.Instructions.LBU, pspsharp.Allegrex.Instructions.LHU, pspsharp.Allegrex.Instructions.LWR, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Instructions.SB, pspsharp.Allegrex.Instructions.SH, pspsharp.Allegrex.Instructions.SWL, pspsharp.Allegrex.Instructions.SW, pspsharp.Allegrex.Instructions.MTVME, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Instructions.SWR, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { return table_11[(insn >> 17) & 0x0000000f].instance(insn); } }, pspsharp.Allegrex.Instructions.LL, pspsharp.Allegrex.Instructions.LWC1, pspsharp.Allegrex.Instructions.LVS, pspsharp.Allegrex.Common.UNK, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { return table_12[(insn >> 18) & 0x0000001f].instance(insn); } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00000002) == 0x00000000) { return pspsharp.Allegrex.Instructions.LVLQ; } return pspsharp.Allegrex.Instructions.LVRQ; } }, pspsharp.Allegrex.Instructions.LVQ, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { return table_13[(insn >> 24) & 0x00000003].instance(insn); } }, pspsharp.Allegrex.Instructions.SC, pspsharp.Allegrex.Instructions.SWC1, pspsharp.Allegrex.Instructions.SVS, pspsharp.Allegrex.Common.UNK, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { return table_14[(insn >> 23) & 0x00000007].instance(insn); } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00000002) == 0x00000000) { return pspsharp.Allegrex.Instructions.SVLQ; } return pspsharp.Allegrex.Instructions.SVRQ; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00000002) == 0x00000000) { return pspsharp.Allegrex.Instructions.SVQ; } return pspsharp.Allegrex.Instructions.VWB; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00000001) == 0x00000000) { if((insn & 0x00000020) == 0x00000000) { return pspsharp.Allegrex.Instructions.VNOP; } return pspsharp.Allegrex.Instructions.VSYNC; } return pspsharp.Allegrex.Instructions.VFLUSH; } }};
		public static readonly Instruction[] table_0 = new Instruction[]
		{
			new STUB()
			{
				public Instruction instance(int insn) {return table_1[(insn >> 0) & 0x0000003f].instance(insn);}
			},
			new STUB()
			{
				public Instruction instance(int insn) {return table_2[(insn >> 16) & 0x00000003].instance(insn);}
			},
			pspsharp.Allegrex.Instructions.J,
			pspsharp.Allegrex.Instructions.JAL,
			pspsharp.Allegrex.Instructions.BEQ,
			pspsharp.Allegrex.Instructions.BNE,
			pspsharp.Allegrex.Instructions.BLEZ,
			pspsharp.Allegrex.Instructions.BGTZ,
			pspsharp.Allegrex.Instructions.ADDI,
			pspsharp.Allegrex.Instructions.ADDIU,
			pspsharp.Allegrex.Instructions.SLTI,
			pspsharp.Allegrex.Instructions.SLTIU,
			pspsharp.Allegrex.Instructions.ANDI,
			pspsharp.Allegrex.Instructions.ORI,
			pspsharp.Allegrex.Instructions.XORI,
			pspsharp.Allegrex.Instructions.LUI,
			new STUB()
			{
				public Instruction instance(int insn) {return table_3[(insn >> 22) & 0x00000003].instance(insn);}
			},
			new STUB()
			{
				public Instruction instance(int insn) {return table_4[(insn >> 23) & 0x00000007].instance(insn);}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00200000) == 0x00000000) {return table_7[(insn >> 16) & 0x00000003].instance(insn);} if ((insn & 0x00000080) == 0x00000000)
					{
						if ((insn & 0x00800000) == 0x00000000) {return pspsharp.Allegrex.Instructions.MFV;} return pspsharp.Allegrex.Instructions.MTV;
					}
					if ((insn & 0x00800000) == 0x00000000) {return pspsharp.Allegrex.Instructions.MFVC;} return pspsharp.Allegrex.Instructions.MTVC;
				}
			},
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Instructions.BEQL,
			pspsharp.Allegrex.Instructions.BNEL,
			pspsharp.Allegrex.Instructions.BLEZL,
			pspsharp.Allegrex.Instructions.BGTZL,
			new STUB()
			{
				public Instruction instance(int insn) {return table_8[(insn >> 23) & 0x00000003].instance(insn);}
			},
			new STUB()
			{
				public Instruction instance(int insn) {return table_9[(insn >> 23) & 0x00000007].instance(insn);}
			},
			pspsharp.Allegrex.Instructions.MFVME,
			new STUB()
			{
				public Instruction instance(int insn) {return table_10[(insn >> 23) & 0x00000007].instance(insn);}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00000002) == 0x00000000)
					{
						if ((insn & 0x00000004) == 0x00000000) {return pspsharp.Allegrex.Instructions.HALT;} return pspsharp.Allegrex.Instructions.MFIC;
					}
					if ((insn & 0x0000003F) == 0x0000003F) {return pspsharp.Allegrex.Instructions.DBREAK;} return pspsharp.Allegrex.Instructions.MTIC;
				}
			},
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00000020) == 0x00000020)
					{
						if ((insn & 0x00000080) == 0x00000000)
						{
							if ((insn & 0x00000100) == 0x00000000)
							{
								if ((insn & 0x00000200) == 0x00000000) {return pspsharp.Allegrex.Instructions.SEB;} return pspsharp.Allegrex.Instructions.SEH;
							}
							return pspsharp.Allegrex.Instructions.BITREV;
						}
						if ((insn & 0x00000040) == 0x00000000) {return pspsharp.Allegrex.Instructions.WSBH;} return pspsharp.Allegrex.Instructions.WSBW;
					}
					if ((insn & 0x00000004) == 0x00000000) {return pspsharp.Allegrex.Instructions.EXT;} return pspsharp.Allegrex.Instructions.INS;
				}
			},
			pspsharp.Allegrex.Instructions.LB,
			pspsharp.Allegrex.Instructions.LH,
			pspsharp.Allegrex.Instructions.LWL,
			pspsharp.Allegrex.Instructions.LW,
			pspsharp.Allegrex.Instructions.LBU,
			pspsharp.Allegrex.Instructions.LHU,
			pspsharp.Allegrex.Instructions.LWR,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Instructions.SB,
			pspsharp.Allegrex.Instructions.SH,
			pspsharp.Allegrex.Instructions.SWL,
			pspsharp.Allegrex.Instructions.SW,
			pspsharp.Allegrex.Instructions.MTVME,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Instructions.SWR,
			new STUB()
			{
				public Instruction instance(int insn) {return table_11[(insn >> 17) & 0x0000000f].instance(insn);}
			},
			pspsharp.Allegrex.Instructions.LL,
			pspsharp.Allegrex.Instructions.LWC1,
			pspsharp.Allegrex.Instructions.LVS,
			pspsharp.Allegrex.Common.UNK,
			new STUB()
			{
				public Instruction instance(int insn) {return table_12[(insn >> 18) & 0x0000001f].instance(insn);}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00000002) == 0x00000000) {return pspsharp.Allegrex.Instructions.LVLQ;} return pspsharp.Allegrex.Instructions.LVRQ;
				}
			},
			pspsharp.Allegrex.Instructions.LVQ,
			new STUB()
			{
				public Instruction instance(int insn) {return table_13[(insn >> 24) & 0x00000003].instance(insn);}
			},
			pspsharp.Allegrex.Instructions.SC,
			pspsharp.Allegrex.Instructions.SWC1,
			pspsharp.Allegrex.Instructions.SVS,
			pspsharp.Allegrex.Common.UNK,
			new STUB()
			{
				public Instruction instance(int insn) {return table_14[(insn >> 23) & 0x00000007].instance(insn);}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00000002) == 0x00000000) {return pspsharp.Allegrex.Instructions.SVLQ;} return pspsharp.Allegrex.Instructions.SVRQ;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00000002) == 0x00000000) {return pspsharp.Allegrex.Instructions.SVQ;} return pspsharp.Allegrex.Instructions.VWB;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00000001) == 0x00000000)
					{
						if ((insn & 0x00000020) == 0x00000000) {return pspsharp.Allegrex.Instructions.VNOP;} return pspsharp.Allegrex.Instructions.VSYNC;
					}
					return pspsharp.Allegrex.Instructions.VFLUSH;
				}
			}
		};
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public static final pspsharp.Allegrex.Common.Instruction table_1[] = { new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x001fffc0) == 0x00000000) { return pspsharp.Allegrex.Instructions.NOP; } return pspsharp.Allegrex.Instructions.SLL; } }, pspsharp.Allegrex.Common.UNK, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00200000) == 0x00000000) { return pspsharp.Allegrex.Instructions.SRL; } return pspsharp.Allegrex.Instructions.ROTR; } }, pspsharp.Allegrex.Instructions.SRA, pspsharp.Allegrex.Instructions.SLLV, pspsharp.Allegrex.Common.UNK, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00000040) == 0x00000000) { return pspsharp.Allegrex.Instructions.SRLV; } return pspsharp.Allegrex.Instructions.ROTRV; } }, pspsharp.Allegrex.Instructions.SRAV, pspsharp.Allegrex.Instructions.JR, pspsharp.Allegrex.Instructions.JALR, pspsharp.Allegrex.Instructions.MOVZ, pspsharp.Allegrex.Instructions.MOVN, pspsharp.Allegrex.Instructions.SYSCALL, pspsharp.Allegrex.Instructions.BREAK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Instructions.SYNC, pspsharp.Allegrex.Instructions.MFHI, pspsharp.Allegrex.Instructions.MTHI, pspsharp.Allegrex.Instructions.MFLO, pspsharp.Allegrex.Instructions.MTLO, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Instructions.CLZ, pspsharp.Allegrex.Instructions.CLO, pspsharp.Allegrex.Instructions.MULT, pspsharp.Allegrex.Instructions.MULTU, pspsharp.Allegrex.Instructions.DIV, pspsharp.Allegrex.Instructions.DIVU, pspsharp.Allegrex.Instructions.MADD, pspsharp.Allegrex.Instructions.MADDU, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Instructions.ADD, pspsharp.Allegrex.Instructions.ADDU, pspsharp.Allegrex.Instructions.SUB, pspsharp.Allegrex.Instructions.SUBU, pspsharp.Allegrex.Instructions.AND, pspsharp.Allegrex.Instructions.OR, pspsharp.Allegrex.Instructions.XOR, pspsharp.Allegrex.Instructions.NOR, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Instructions.SLT, pspsharp.Allegrex.Instructions.SLTU, pspsharp.Allegrex.Instructions.MAX, pspsharp.Allegrex.Instructions.MIN, pspsharp.Allegrex.Instructions.MSUB, pspsharp.Allegrex.Instructions.MSUBU, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK};
		public static readonly Instruction[] table_1 = new Instruction[]
		{
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x001fffc0) == 0x00000000) {return pspsharp.Allegrex.Instructions.NOP;} return pspsharp.Allegrex.Instructions.SLL;
				}
			},
			pspsharp.Allegrex.Common.UNK,
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00200000) == 0x00000000) {return pspsharp.Allegrex.Instructions.SRL;} return pspsharp.Allegrex.Instructions.ROTR;
				}
			},
			pspsharp.Allegrex.Instructions.SRA,
			pspsharp.Allegrex.Instructions.SLLV,
			pspsharp.Allegrex.Common.UNK,
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00000040) == 0x00000000) {return pspsharp.Allegrex.Instructions.SRLV;} return pspsharp.Allegrex.Instructions.ROTRV;
				}
			},
			pspsharp.Allegrex.Instructions.SRAV,
			pspsharp.Allegrex.Instructions.JR,
			pspsharp.Allegrex.Instructions.JALR,
			pspsharp.Allegrex.Instructions.MOVZ,
			pspsharp.Allegrex.Instructions.MOVN,
			pspsharp.Allegrex.Instructions.SYSCALL,
			pspsharp.Allegrex.Instructions.BREAK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Instructions.SYNC,
			pspsharp.Allegrex.Instructions.MFHI,
			pspsharp.Allegrex.Instructions.MTHI,
			pspsharp.Allegrex.Instructions.MFLO,
			pspsharp.Allegrex.Instructions.MTLO,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Instructions.CLZ,
			pspsharp.Allegrex.Instructions.CLO,
			pspsharp.Allegrex.Instructions.MULT,
			pspsharp.Allegrex.Instructions.MULTU,
			pspsharp.Allegrex.Instructions.DIV,
			pspsharp.Allegrex.Instructions.DIVU,
			pspsharp.Allegrex.Instructions.MADD,
			pspsharp.Allegrex.Instructions.MADDU,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Instructions.ADD,
			pspsharp.Allegrex.Instructions.ADDU,
			pspsharp.Allegrex.Instructions.SUB,
			pspsharp.Allegrex.Instructions.SUBU,
			pspsharp.Allegrex.Instructions.AND,
			pspsharp.Allegrex.Instructions.OR,
			pspsharp.Allegrex.Instructions.XOR,
			pspsharp.Allegrex.Instructions.NOR,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Instructions.SLT,
			pspsharp.Allegrex.Instructions.SLTU,
			pspsharp.Allegrex.Instructions.MAX,
			pspsharp.Allegrex.Instructions.MIN,
			pspsharp.Allegrex.Instructions.MSUB,
			pspsharp.Allegrex.Instructions.MSUBU,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK
		};
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public static final pspsharp.Allegrex.Common.Instruction table_2[] = { new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00100000) == 0x00000000) { return pspsharp.Allegrex.Instructions.BLTZ; } return pspsharp.Allegrex.Instructions.BLTZAL; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00100000) == 0x00000000) { return pspsharp.Allegrex.Instructions.BGEZ; } return pspsharp.Allegrex.Instructions.BGEZAL; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00100000) == 0x00000000) { return pspsharp.Allegrex.Instructions.BLTZL; } return pspsharp.Allegrex.Instructions.BLTZALL; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00100000) == 0x00000000) { return pspsharp.Allegrex.Instructions.BGEZL; } return pspsharp.Allegrex.Instructions.BGEZALL; } }};
		public static readonly Instruction[] table_2 = new Instruction[]
		{
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00100000) == 0x00000000) {return pspsharp.Allegrex.Instructions.BLTZ;} return pspsharp.Allegrex.Instructions.BLTZAL;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00100000) == 0x00000000) {return pspsharp.Allegrex.Instructions.BGEZ;} return pspsharp.Allegrex.Instructions.BGEZAL;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00100000) == 0x00000000) {return pspsharp.Allegrex.Instructions.BLTZL;} return pspsharp.Allegrex.Instructions.BLTZALL;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00100000) == 0x00000000) {return pspsharp.Allegrex.Instructions.BGEZL;} return pspsharp.Allegrex.Instructions.BGEZALL;
				}
			}
		};
		public static readonly Instruction[] table_3 = new Instruction[]
		{
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00000008) == 0x00000000) {return pspsharp.Allegrex.Instructions.MFC0;} return pspsharp.Allegrex.Instructions.ERET;
				}
			},
			pspsharp.Allegrex.Instructions.CFC0,
			pspsharp.Allegrex.Instructions.MTC0,
			pspsharp.Allegrex.Instructions.CTC0
		};
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public static final pspsharp.Allegrex.Common.Instruction table_4[] = { new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00400000) == 0x00000000) { return pspsharp.Allegrex.Instructions.MFC1; } return pspsharp.Allegrex.Instructions.CFC1; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00400000) == 0x00000000) { return pspsharp.Allegrex.Instructions.MTC1; } return pspsharp.Allegrex.Instructions.CTC1; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { return table_5[(insn >> 16) & 0x00000003].instance(insn); } }, pspsharp.Allegrex.Common.UNK, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { return table_6[(insn >> 0) & 0x0000001f].instance(insn); } }, pspsharp.Allegrex.Instructions.CVT_S_W, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK};
		public static readonly Instruction[] table_4 = new Instruction[]
		{
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00400000) == 0x00000000) {return pspsharp.Allegrex.Instructions.MFC1;} return pspsharp.Allegrex.Instructions.CFC1;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00400000) == 0x00000000) {return pspsharp.Allegrex.Instructions.MTC1;} return pspsharp.Allegrex.Instructions.CTC1;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn) {return table_5[(insn >> 16) & 0x00000003].instance(insn);}
			},
			pspsharp.Allegrex.Common.UNK,
			new STUB()
			{
				public Instruction instance(int insn) {return table_6[(insn >> 0) & 0x0000001f].instance(insn);}
			},
			pspsharp.Allegrex.Instructions.CVT_S_W,
			pspsharp.Allegrex.Common.UNK,
			pspsharp.Allegrex.Common.UNK
		};
		public static readonly Instruction[] table_5 = new Instruction[] {pspsharp.Allegrex.Instructions.BC1F, pspsharp.Allegrex.Instructions.BC1T, pspsharp.Allegrex.Instructions.BC1FL, pspsharp.Allegrex.Instructions.BC1TL};
		public static readonly Instruction[] table_6 = new Instruction[]
		{
			pspsharp.Allegrex.Instructions.ADD_S, pspsharp.Allegrex.Instructions.SUB_S, pspsharp.Allegrex.Instructions.MUL_S, pspsharp.Allegrex.Instructions.DIV_S, new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00000020) == 0x00000000) {return pspsharp.Allegrex.Instructions.SQRT_S;} return pspsharp.Allegrex.Instructions.CVT_W_S;
				}
			},
			pspsharp.Allegrex.Instructions.ABS_S, pspsharp.Allegrex.Instructions.MOV_S, pspsharp.Allegrex.Instructions.NEG_S, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Instructions.ROUND_W_S, pspsharp.Allegrex.Instructions.TRUNC_W_S, pspsharp.Allegrex.Instructions.CEIL_W_S, pspsharp.Allegrex.Instructions.FLOOR_W_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S, pspsharp.Allegrex.Instructions.C_COND_S
		};
		public static readonly Instruction[] table_7 = new Instruction[] {pspsharp.Allegrex.Instructions.BVF, pspsharp.Allegrex.Instructions.BVT, pspsharp.Allegrex.Instructions.BVFL, pspsharp.Allegrex.Instructions.BVTL};
		public static readonly Instruction[] table_8 = new Instruction[] {pspsharp.Allegrex.Instructions.VADD, pspsharp.Allegrex.Instructions.VSUB, pspsharp.Allegrex.Instructions.VSBN, pspsharp.Allegrex.Instructions.VDIV};
		public static readonly Instruction[] table_9 = new Instruction[] {pspsharp.Allegrex.Instructions.VMUL, pspsharp.Allegrex.Instructions.VDOT, pspsharp.Allegrex.Instructions.VSCL, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Instructions.VHDP, pspsharp.Allegrex.Instructions.VCRS, pspsharp.Allegrex.Instructions.VDET, pspsharp.Allegrex.Common.UNK};
		public static readonly Instruction[] table_10 = new Instruction[] {pspsharp.Allegrex.Instructions.VCMP, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Instructions.VMIN, pspsharp.Allegrex.Instructions.VMAX, pspsharp.Allegrex.Common.UNK, pspsharp.Allegrex.Instructions.VSCMP, pspsharp.Allegrex.Instructions.VSGE, pspsharp.Allegrex.Instructions.VSLT};
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public static final pspsharp.Allegrex.Common.Instruction table_11[] = { pspsharp.Allegrex.Instructions.ICACHE, pspsharp.Allegrex.Instructions.ICACHE, pspsharp.Allegrex.Instructions.ICACHE_INDEX_INVALIDATE, pspsharp.Allegrex.Instructions.ICACHE_INDEX_UNLOCK, pspsharp.Allegrex.Instructions.ICACHE_HIT_INVALIDATE, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00010000) == 0x00000000) { return pspsharp.Allegrex.Instructions.ICACHE_FILL; } return pspsharp.Allegrex.Instructions.ICACHE_FILL_WITH_LOCK; } }, pspsharp.Allegrex.Instructions.ICACHE, pspsharp.Allegrex.Instructions.ICACHE, pspsharp.Allegrex.Instructions.DCACHE, pspsharp.Allegrex.Instructions.DCACHE, pspsharp.Allegrex.Instructions.DCACHE_INDEX_WRITEBACK_INVALIDATE, pspsharp.Allegrex.Instructions.DCACHE_INDEX_UNLOCK, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00010000) == 0x00000000) { return pspsharp.Allegrex.Instructions.DCACHE_CREATE_DIRTY_EXCLUSIVE; } return pspsharp.Allegrex.Instructions.DCACHE_HIT_INVALIDATE; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00010000) == 0x00000000) { return pspsharp.Allegrex.Instructions.DCACHE_HIT_WRITEBACK; } return pspsharp.Allegrex.Instructions.DCACHE_HIT_WRITEBACK_INVALIDATE; } }, pspsharp.Allegrex.Instructions.DCACHE_CREATE_DIRTY_EXCLUSIVE_WITH_LOCK, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00010000) == 0x00000000) { return pspsharp.Allegrex.Instructions.DCACHE_FILL; } return pspsharp.Allegrex.Instructions.DCACHE_FILL_WITH_LOCK; } }};
		public static readonly Instruction[] table_11 = new Instruction[]
		{
			pspsharp.Allegrex.Instructions.ICACHE, pspsharp.Allegrex.Instructions.ICACHE, pspsharp.Allegrex.Instructions.ICACHE_INDEX_INVALIDATE, pspsharp.Allegrex.Instructions.ICACHE_INDEX_UNLOCK, pspsharp.Allegrex.Instructions.ICACHE_HIT_INVALIDATE, new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00010000) == 0x00000000) {return pspsharp.Allegrex.Instructions.ICACHE_FILL;} return pspsharp.Allegrex.Instructions.ICACHE_FILL_WITH_LOCK;
				}
			},
			pspsharp.Allegrex.Instructions.ICACHE, pspsharp.Allegrex.Instructions.ICACHE, pspsharp.Allegrex.Instructions.DCACHE, pspsharp.Allegrex.Instructions.DCACHE, pspsharp.Allegrex.Instructions.DCACHE_INDEX_WRITEBACK_INVALIDATE, pspsharp.Allegrex.Instructions.DCACHE_INDEX_UNLOCK, new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00010000) == 0x00000000) {return pspsharp.Allegrex.Instructions.DCACHE_CREATE_DIRTY_EXCLUSIVE;} return pspsharp.Allegrex.Instructions.DCACHE_HIT_INVALIDATE;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00010000) == 0x00000000) {return pspsharp.Allegrex.Instructions.DCACHE_HIT_WRITEBACK;} return pspsharp.Allegrex.Instructions.DCACHE_HIT_WRITEBACK_INVALIDATE;
				}
			},
			pspsharp.Allegrex.Instructions.DCACHE_CREATE_DIRTY_EXCLUSIVE_WITH_LOCK, new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00010000) == 0x00000000) {return pspsharp.Allegrex.Instructions.DCACHE_FILL;} return pspsharp.Allegrex.Instructions.DCACHE_FILL_WITH_LOCK;
				}
			}
		};
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public static final pspsharp.Allegrex.Common.Instruction table_12[] = { new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00000000) { if((insn & 0x00020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VMOV; } return pspsharp.Allegrex.Instructions.VNEG; } if((insn & 0x02020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VABS; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VIDT; } if((insn & 0x01800000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IN; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VI2F; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00000000) { if((insn & 0x00020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VSAT0; } return pspsharp.Allegrex.Instructions.VZERO; } if((insn & 0x02020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VSAT1; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VONE; } if((insn & 0x01800000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IN; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VI2F; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x01800000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IN; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VI2F; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x01800000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IN; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VI2F; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00000000) { if((insn & 0x00020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VRCP; } return pspsharp.Allegrex.Instructions.VSIN; } if((insn & 0x02020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VRSQ; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VCOS; } if((insn & 0x01800000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IN; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VI2F; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00000000) { if((insn & 0x00020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VEXP2; } return pspsharp.Allegrex.Instructions.VSQRT; } if((insn & 0x02020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VLOG2; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VASIN; } if((insn & 0x01800000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IN; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VI2F; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VNRCP; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VNSIN; } if((insn & 0x01800000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IN; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VI2F; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VREXP2; } if((insn & 0x01800000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IN; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VI2F; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00000000) { if((insn & 0x00020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VRNDS; } return pspsharp.Allegrex.Instructions.VRNDF1; } if((insn & 0x02020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VRNDI; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VRNDF2; } if((insn & 0x01800000) == 0x00800000) { return pspsharp.Allegrex.Instructions.VCMOVT; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IZ; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x01800000) == 0x00800000) { return pspsharp.Allegrex.Instructions.VCMOVT; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IZ; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x01800000) == 0x00800000) { return pspsharp.Allegrex.Instructions.VCMOVF; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IZ; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x01800000) == 0x00800000) { return pspsharp.Allegrex.Instructions.VCMOVF; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IZ; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2H; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VH2F; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IZ; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VSBZ; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VLGB; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IZ; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00000000) { if((insn & 0x00020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VUC2I; } return pspsharp.Allegrex.Instructions.VUS2I; } if((insn & 0x02020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VC2I; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VS2I; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IZ; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00000000) { if((insn & 0x00020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VI2UC; } return pspsharp.Allegrex.Instructions.VI2US; } if((insn & 0x02020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VI2C; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VI2S; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IZ; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00000000) { if((insn & 0x00020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VSRT1; } return pspsharp.Allegrex.Instructions.VBFY1; } if((insn & 0x02020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VSRT2; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VBFY2; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IU; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00000000) { if((insn & 0x00020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VOCP; } return pspsharp.Allegrex.Instructions.VFAD; } if((insn & 0x02020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VSOCP; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VAVG; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IU; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00000000) { if((insn & 0x00020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VSRT3; } return pspsharp.Allegrex.Instructions.VSGN; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VSRT4; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IU; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IU; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VMFVC; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VMTVC; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IU; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IU; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02010000) == 0x00010000) { if((insn & 0x00020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VT4444; } return pspsharp.Allegrex.Instructions.VT5650; } if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VT5551; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IU; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2IU; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VCST; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2ID; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VCST; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2ID; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VCST; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2ID; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VCST; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2ID; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VCST; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2ID; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VCST; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2ID; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VCST; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2ID; } return pspsharp.Allegrex.Instructions.VWBN; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x02000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VCST; } if((insn & 0x01000000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VF2ID; } return pspsharp.Allegrex.Instructions.VWBN; } }};
		public static readonly Instruction[] table_12 = new Instruction[]
		{
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00000000)
					{
						if ((insn & 0x00020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VMOV;} return pspsharp.Allegrex.Instructions.VNEG;
					}
					if ((insn & 0x02020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VABS;} if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VIDT;} if ((insn & 0x01800000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IN;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VI2F;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00000000)
					{
						if ((insn & 0x00020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VSAT0;} return pspsharp.Allegrex.Instructions.VZERO;
					}
					if ((insn & 0x02020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VSAT1;} if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VONE;} if ((insn & 0x01800000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IN;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VI2F;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x01800000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IN;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VI2F;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x01800000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IN;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VI2F;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00000000)
					{
						if ((insn & 0x00020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VRCP;} return pspsharp.Allegrex.Instructions.VSIN;
					}
					if ((insn & 0x02020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VRSQ;} if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VCOS;} if ((insn & 0x01800000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IN;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VI2F;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00000000)
					{
						if ((insn & 0x00020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VEXP2;} return pspsharp.Allegrex.Instructions.VSQRT;
					}
					if ((insn & 0x02020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VLOG2;} if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VASIN;} if ((insn & 0x01800000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IN;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VI2F;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VNRCP;} if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VNSIN;} if ((insn & 0x01800000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IN;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VI2F;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VREXP2;} if ((insn & 0x01800000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IN;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VI2F;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00000000)
					{
						if ((insn & 0x00020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VRNDS;} return pspsharp.Allegrex.Instructions.VRNDF1;
					}
					if ((insn & 0x02020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VRNDI;} if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VRNDF2;} if ((insn & 0x01800000) == 0x00800000) {return pspsharp.Allegrex.Instructions.VCMOVT;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IZ;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x01800000) == 0x00800000) {return pspsharp.Allegrex.Instructions.VCMOVT;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IZ;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x01800000) == 0x00800000) {return pspsharp.Allegrex.Instructions.VCMOVF;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IZ;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x01800000) == 0x00800000) {return pspsharp.Allegrex.Instructions.VCMOVF;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IZ;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2H;} if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VH2F;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IZ;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VSBZ;} if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VLGB;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IZ;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00000000)
					{
						if ((insn & 0x00020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VUC2I;} return pspsharp.Allegrex.Instructions.VUS2I;
					}
					if ((insn & 0x02020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VC2I;} if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VS2I;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IZ;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00000000)
					{
						if ((insn & 0x00020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VI2UC;} return pspsharp.Allegrex.Instructions.VI2US;
					}
					if ((insn & 0x02020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VI2C;} if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VI2S;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IZ;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00000000)
					{
						if ((insn & 0x00020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VSRT1;} return pspsharp.Allegrex.Instructions.VBFY1;
					}
					if ((insn & 0x02020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VSRT2;} if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VBFY2;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IU;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00000000)
					{
						if ((insn & 0x00020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VOCP;} return pspsharp.Allegrex.Instructions.VFAD;
					}
					if ((insn & 0x02020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VSOCP;} if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VAVG;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IU;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00000000)
					{
						if ((insn & 0x00020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VSRT3;} return pspsharp.Allegrex.Instructions.VSGN;
					}
					if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VSRT4;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IU;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IU;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VMFVC;} if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VMTVC;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IU;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IU;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02010000) == 0x00010000)
					{
						if ((insn & 0x00020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VT4444;} return pspsharp.Allegrex.Instructions.VT5650;
					}
					if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VT5551;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IU;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2IU;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VCST;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2ID;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VCST;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2ID;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VCST;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2ID;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VCST;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2ID;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VCST;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2ID;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VCST;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2ID;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VCST;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2ID;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x02000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VCST;} if ((insn & 0x01000000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VF2ID;} return pspsharp.Allegrex.Instructions.VWBN;
				}
			}
		};
		public static readonly Instruction[] table_13 = new Instruction[]
		{
			pspsharp.Allegrex.Instructions.VPFXS, pspsharp.Allegrex.Instructions.VPFXT, pspsharp.Allegrex.Instructions.VPFXD, new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00800000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VIIM;} return pspsharp.Allegrex.Instructions.VFIM;
				}
			}
		};
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public static final pspsharp.Allegrex.Common.Instruction table_14[] = { pspsharp.Allegrex.Instructions.VMMUL, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00000080) == 0x00000000) { return pspsharp.Allegrex.Instructions.VHTFM2; } return pspsharp.Allegrex.Instructions.VTFM2; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00000080) == 0x00000000) { return pspsharp.Allegrex.Instructions.VTFM3; } return pspsharp.Allegrex.Instructions.VHTFM3; } }, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00000080) == 0x00000000) { return pspsharp.Allegrex.Instructions.VHTFM4; } return pspsharp.Allegrex.Instructions.VTFM4; } }, pspsharp.Allegrex.Instructions.VMSCL, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00000080) == 0x00000000) { return pspsharp.Allegrex.Instructions.VCRSP; } return pspsharp.Allegrex.Instructions.VQMUL; } }, pspsharp.Allegrex.Common.UNK, new pspsharp.Allegrex.Common.STUB() { @Override public pspsharp.Allegrex.Common.Instruction instance(int insn) { if((insn & 0x00210000) == 0x00000000) { if((insn & 0x00020000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VMMOV; } return pspsharp.Allegrex.Instructions.VMZERO; } if((insn & 0x00200000) == 0x00000000) { if((insn & 0x00040000) == 0x00000000) { return pspsharp.Allegrex.Instructions.VMIDT; } return pspsharp.Allegrex.Instructions.VMONE; } return pspsharp.Allegrex.Instructions.VROT; } }};
		public static readonly Instruction[] table_14 = new Instruction[]
		{
			pspsharp.Allegrex.Instructions.VMMUL, new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00000080) == 0x00000000) {return pspsharp.Allegrex.Instructions.VHTFM2;} return pspsharp.Allegrex.Instructions.VTFM2;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00000080) == 0x00000000) {return pspsharp.Allegrex.Instructions.VTFM3;} return pspsharp.Allegrex.Instructions.VHTFM3;
				}
			},
			new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00000080) == 0x00000000) {return pspsharp.Allegrex.Instructions.VHTFM4;} return pspsharp.Allegrex.Instructions.VTFM4;
				}
			},
			pspsharp.Allegrex.Instructions.VMSCL, new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00000080) == 0x00000000) {return pspsharp.Allegrex.Instructions.VCRSP;} return pspsharp.Allegrex.Instructions.VQMUL;
				}
			},
			pspsharp.Allegrex.Common.UNK, new STUB()
			{
				public Instruction instance(int insn)
				{
					if ((insn & 0x00210000) == 0x00000000)
					{
						if ((insn & 0x00020000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VMMOV;} return pspsharp.Allegrex.Instructions.VMZERO;
					}
					if ((insn & 0x00200000) == 0x00000000)
					{
						if ((insn & 0x00040000) == 0x00000000) {return pspsharp.Allegrex.Instructions.VMIDT;} return pspsharp.Allegrex.Instructions.VMONE;
					}
					return pspsharp.Allegrex.Instructions.VROT;
				}
			}
		};

		public static Instruction instruction(int insn)
		{
			return table_0[(insn >> 26) & 0x0000003f].instance(insn);
		}
	}
}