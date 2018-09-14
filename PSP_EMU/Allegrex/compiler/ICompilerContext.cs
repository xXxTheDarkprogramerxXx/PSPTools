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
	using MethodVisitor = org.objectweb.asm.MethodVisitor;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public interface ICompilerContext
	{
		void compileInterpreterInstruction();
		void compileRTRSIMM(string method, bool signedImm);
		void compileRDRT(string method);
		void compileFDFSFT(string method);
		void compileSyscall();
		void loadRs();
		void loadRt();
		void loadRd();
		void loadRegister(int reg);
		void loadFs();
		void loadFt();
		void loadFd();
		void loadFRegister(int reg);
		void loadVs(int n);
		void loadVs(int vsize, int n);
		void loadVs(int vsize, int vs, int n);
		void loadVsInt(int n);
		void loadVsInt(int vsize, int n);
		void loadVsInt(int vsize, int vs, int n);
		void loadVt(int n);
		void loadVt(int vsize, int n);
		void loadVt(int vsize, int vt, int n);
		void loadVtInt(int n);
		void loadVtInt(int vsize, int n);
		void loadVtInt(int vsize, int vt, int n);
		void loadVd(int n);
		void loadVd(int vize, int n);
		void loadVd(int vize, int vd, int n);
		void loadVdInt(int n);
		void loadVdInt(int vsize, int n);
		void loadVdInt(int vsize, int vd, int n);
		void loadHilo();
		void loadSaValue();
		void loadFCr();
		void loadFcr31c();
		void loadVcrCc(int cc);
		void storeRd();
		void storeRt();
		void storeRd(int constantValue);
		void storeRt(int constantValue);
		void storeFd();
		void storeFt();
		void storeVd(int n);
		void storeVd(int vsize, int n);
		void storeVd(int vsize, int vd, int n);
		void storeVdInt(int n);
		void storeVdInt(int vsize, int n);
		void storeVdInt(int vsize, int vd, int n);
		void storeVt(int n);
		void storeVt(int vsize, int n);
		void storeVt(int vsize, int vt, int n);
		void storeVtInt(int n);
		void storeVtInt(int vsize, int n);
		void storeVtInt(int vsize, int vt, int n);
		void storeRegister(int reg, int constantValue);
		void storeHilo();
		void storeFCr();
		void storeFcr31c();
		void storeVcrCc(int cc);
		void prepareRdForStore();
		void prepareRtForStore();
		void prepareFdForStore();
		void prepareFtForStore();
		void prepareVdForStore(int n);
		void prepareVdForStore(int vsize, int n);
		void prepareVdForStore(int vsize, int vd, int n);
		void prepareVdForStoreInt(int n);
		void prepareVdForStoreInt(int vsize, int n);
		void prepareVdForStoreInt(int vsize, int vd, int n);
		void prepareVtForStore(int n);
		void prepareVtForStore(int vsize, int n);
		void prepareVtForStore(int vsize, int vt, int n);
		void prepareVtForStoreInt(int n);
		void prepareVtForStoreInt(int vsize, int n);
		void prepareVtForStoreInt(int vsize, int vt, int n);
		void prepareHiloForStore();
		void prepareFCrForStore();
		void prepareFcr31cForStore();
		void prepareVcrCcForStore(int cc);
		int RsRegisterIndex {get;}
		int RtRegisterIndex {get;}
		int RdRegisterIndex {get;}
		int FsRegisterIndex {get;}
		int FtRegisterIndex {get;}
		int FdRegisterIndex {get;}
		int VsRegisterIndex {get;}
		int VtRegisterIndex {get;}
		int VdRegisterIndex {get;}
		int Vsize {get;}
		int CrValue {get;}
		int SaValue {get;}
		bool RdRegister0 {get;}
		bool RtRegister0 {get;}
		bool RsRegister0 {get;}
		int getImm16(bool signedImm);
		int getImm14(bool signedImm);
		int Imm7 {get;}
		int Imm5 {get;}
		int Imm4 {get;}
		int Imm3 {get;}
		void loadImm(int imm);
		void loadImm16(bool signedImm);
		MethodVisitor MethodVisitor {get;}
		void memRead32(int registerIndex, int offset);
		void memRead16(int registerIndex, int offset);
		void memRead8(int registerIndex, int offset);
		void memWrite32(int registerIndex, int offset);
		void memWrite16(int registerIndex, int offset);
		void memWrite8(int registerIndex, int offset);
		void prepareMemWrite32(int registerIndex, int offset);
		void prepareMemWrite16(int registerIndex, int offset);
		void prepareMemWrite8(int registerIndex, int offset);
		void memWriteZero8(int registerIndex, int offset);
		void convertUnsignedIntToLong();
		void startPfxCompiled();
		void startPfxCompiled(bool isFloat);
		void endPfxCompiled();
		void endPfxCompiled(bool isFloat);
		void endPfxCompiled(int vsize);
		void endPfxCompiled(int vsize, bool isFloat);
		void endPfxCompiled(int vsize, bool isFloat, bool doFlush);
		void flushPfxCompiled(int vsize, int vd, bool isFloat);
		bool isPfxConsumed(int flag);
		void loadTmp1();
		void loadTmp2();
		void loadLTmp1();
		void loadFTmp1();
		void loadFTmp2();
		void loadFTmp3();
		void loadFTmp4();
		void storeTmp1();
		void storeTmp2();
		void storeLTmp1();
		void storeFTmp1();
		void storeFTmp2();
		void storeFTmp3();
		void storeFTmp4();
		VfpuPfxSrcState PfxsState {get;}
		VfpuPfxSrcState PfxtState {get;}
		VfpuPfxDstState PfxdState {get;}
		bool VsVdOverlap {get;}
		bool VtVdOverlap {get;}
		void compileVFPUInstr(object cstBefore, int opcode, string mathFunction);
		bool compileVFPULoad(int registerIndex, int offset, int vt, int count);
		bool compileVFPUStore(int registerIndex, int offset, int vt, int count);
		CodeInstruction CodeInstruction {get;}
		CodeInstruction getCodeInstruction(int address);
		void skipInstructions(int numberInstructionsToBeSkipped, bool skipDelaySlot);
		bool compileSWsequence(int baseRegister, int[] offsets, int[] registers);
		bool compileLWsequence(int baseRegister, int[] offsets, int[] registers);
		void storePc();
		void loadLocalVar(int localVar);
		void loadProcessor();
		bool hasNoPfx();
		void loadVprInt();
		void loadVprFloat();
	}

}