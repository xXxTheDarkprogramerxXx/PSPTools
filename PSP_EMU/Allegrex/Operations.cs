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
	public interface Operations
	{

		void opUNK(string reason);

		void opNOP();

		void opSLL(int rd, int rt, int sa);

		void opSRL(int rd, int rt, int sa);

		void opSRA(int rd, int rt, int sa);

		void opSLLV(int rd, int rt, int rs);

		void opSRLV(int rd, int rt, int rs);

		void opSRAV(int rd, int rt, int rs);

		void opJR(int rs);

		void opJALR(int rd, int rs);

		void opMFHI(int rd);

		void opMTHI(int rs);

		void opMFLO(int rd);

		void opMTLO(int rs);

		void opMULT(int rs, int rt);

		void opMULTU(int rs, int rt);

		void opDIV(int rs, int rt);

		void opDIVU(int rs, int rt);

		void opADD(int rd, int rs, int rt);

		void opADDU(int rd, int rs, int rt);

		void opSUB(int rd, int rs, int rt);

		void opSUBU(int rd, int rs, int rt);

		void opAND(int rd, int rs, int rt);

		void opOR(int rd, int rs, int rt);

		void opXOR(int rd, int rs, int rt);

		void opNOR(int rd, int rs, int rt);

		void opSLT(int rd, int rs, int rt);

		void opSLTU(int rd, int rs, int rt);

		void opBLTZ(int rs, int simm16);

		void opBGEZ(int rs, int simm16);

		void opBLTZL(int rs, int simm16);

		void opBGEZL(int rs, int simm16);

		void opBLTZAL(int rs, int simm16);

		void opBGEZAL(int rs, int simm16);

		void opBLTZALL(int rs, int simm16);

		void opBGEZALL(int rs, int simm16);

		void opJ(int uimm26);

		void opJAL(int uimm26);

		void opBEQ(int rs, int rt, int simm16);

		void opBNE(int rs, int rt, int simm16);

		void opBLEZ(int rs, int simm16);

		void opBGTZ(int rs, int simm16);

		void opBEQL(int rs, int rt, int simm16);

		void opBNEL(int rs, int rt, int simm16);

		void opBLEZL(int rs, int simm16);

		void opBGTZL(int rs, int simm16);

		void opADDI(int rt, int rs, int simm16);

		void opADDIU(int rt, int rs, int simm16);

		void opSLTI(int rt, int rs, int simm16);

		void opSLTIU(int rt, int rs, int simm16);

		void opANDI(int rt, int rs, int uimm16);

		void opORI(int rt, int rs, int uimm16);

		void opXORI(int rt, int rs, int uimm16);

		void opLUI(int rt, int uimm16);

		void opHALT();

		void opMFIC(int rt);

		void opMTIC(int rt);

		void opMFC0(int rt, int c0dr);

		void opCFC0(int rt, int c0cr);

		void opMTC0(int rt, int c0dr);

		void opCTC0(int rt, int c0cr);

		void opERET();

		void opLB(int rt, int rs, int simm16);

		void opLBU(int rt, int rs, int simm16);

		void opLH(int rt, int rs, int simm16);

		void opLHU(int rt, int rs, int simm16);

		void opLWL(int rt, int rs, int simm16);

		void opLW(int rt, int rs, int simm16);

		void opLWR(int rt, int rs, int simm16);

		void opSB(int rt, int rs, int simm16);

		void opSH(int rt, int rs, int simm16);

		void opSWL(int rt, int rs, int simm16);

		void opSW(int rt, int rs, int simm16);

		void opSWR(int rt, int rs, int simm16);

		void opCACHE(int rt, int rs, int simm16);

		void opLL(int rt, int rs, int simm16);

		void opLWC1(int rt, int rs, int simm16);

		void opLVS(int vt, int rs, int simm14);

		void opSC(int rt, int rs, int simm16);

		void opSWC1(int rt, int rs, int simm16);

		void opSVS(int vt, int rs, int simm14);

		void opROTR(int rd, int rt, int sa);

		void opROTRV(int rd, int rt, int rs);

		void opMOVZ(int rd, int rs, int rt);

		void opMOVN(int rd, int rs, int rt);

		void opSYSCALL(int code);

		void opBREAK(int code);

		void opSYNC();

		void opCLZ(int rd, int rs);

		void opCLO(int rd, int rs);

		void opMADD(int rs, int rt);

		void opMADDU(int rs, int rt);

		void opMAX(int rd, int rs, int rt);

		void opMIN(int rd, int rs, int rt);

		void opMSUB(int rs, int rt);

		void opMSUBU(int rs, int rt);

		void opEXT(int rt, int rs, int rd, int sa);

		void opINS(int rt, int rs, int rd, int sa);

		void opWSBH(int rd, int rt);

		void opWSBW(int rd, int rt);

		void opSEB(int rd, int rt);

		void opBITREV(int rd, int rt);

		void opSEH(int rd, int rt);
		//COP1 instructions
		void opMFC1(int rt, int c1dr);

		void opCFC1(int rt, int c1cr);

		void opMTC1(int rt, int c1dr);

		void opCTC1(int rt, int c1cr);

		void opBC1F(int simm16);

		void opBC1T(int simm16);

		void opBC1FL(int simm16);

		void opBC1TL(int simm16);

		void opADDS(int fd, int fs, int ft);

		void opSUBS(int fd, int fs, int ft);

		void opMULS(int fd, int fs, int ft);

		void opDIVS(int fd, int fs, int ft);

		void opSQRTS(int fd, int fs);

		void opABSS(int fd, int fs);

		void opMOVS(int fd, int fs);

		void opNEGS(int fd, int fs);

		void opROUNDWS(int fd, int fs);

		void opTRUNCWS(int fd, int fs);

		void opCEILWS(int fd, int fs);

		void opFLOORWS(int fd, int fs);

		void opCVTSW(int fd, int fs);

		void opCVTWS(int fd, int fs);

		void opCCONDS(int fs, int ft, int cond);

		// VFPU0
		void opVADD(int vsize, int vd, int vs, int vt);

		void opVSUB(int vsize, int vd, int vs, int vt);

		void opVSBN(int vsize, int vd, int vs, int vt);

		void opVDIV(int vsize, int vd, int vs, int vt);

		// VFPU1
		void opVMUL(int vsize, int vd, int vs, int vt);

		void opVDOT(int vsize, int vd, int vs, int vt);

		void opVSCL(int vsize, int vd, int vs, int vt);

		void opVHDP(int vsize, int vd, int vs, int vt);

		void opVCRS(int vsize, int vd, int vs, int vt);

		void opVDET(int vsize, int vd, int vs, int vt);

		// VFPU3
		void opVCMP(int vsize, int vs, int vt, int cond);

		void opVMIN(int vsize, int vd, int vs, int vt);

		void opVMAX(int vsize, int vd, int vs, int vt);

		void opVSCMP(int vsize, int vd, int vs, int vt);

		void opVSGE(int vsize, int vd, int vs, int vt);

		void opVSLT(int vsize, int vd, int vs, int vt);

		// VFPU5
		void opVPFXS(int imm24);

		void opVPFXT(int imm24);

		void opVPFXD(int imm24);

		void opVIIM(int vs, int imm16);

		void opVFIM(int vs, int imm16);
	}
}