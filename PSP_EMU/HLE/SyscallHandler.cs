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
namespace pspsharp.HLE
{
	using Logger = org.apache.log4j.Logger;

	using Instruction = pspsharp.Allegrex.Common.Instruction;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using Common = pspsharp.Allegrex.Common;
	using CpuState = pspsharp.Allegrex.CpuState;
	using Decoder = pspsharp.Allegrex.Decoder;
	using Managers = pspsharp.HLE.kernel.Managers;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using DeferredStub = pspsharp.format.DeferredStub;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using Settings = pspsharp.settings.Settings;
	using Utilities = pspsharp.util.Utilities;

	public class SyscallHandler
	{
		private static Logger log = Modules.log;
		public static bool ignoreUnmappedImports = false;
		public const int syscallUnmappedImport = 0xFFFFF;
		// Syscall number used by loadcore.prx to mark unmapped imports
		public const int syscallLoadCoreUnmappedImport = 0x00015;
		private static IgnoreUnmappedImportsSettingsListerner ignoreUnmappedImportsSettingsListerner;

		private class IgnoreUnmappedImportsSettingsListerner : AbstractBoolSettingsListener
		{
			protected internal override void settingsValueChanged(bool value)
			{
				EnableIgnoreUnmappedImports = value;
			}
		}

		private static bool EnableIgnoreUnmappedImports
		{
			get
			{
				return ignoreUnmappedImports;
			}
			set
			{
				ignoreUnmappedImports = value;
				if (value)
				{
					log.info("Ignore Unmapped Imports enabled");
				}
			}
		}


		private static void logMem(Memory mem, int address, string registerName)
		{
			if (Memory.isAddressGood(address))
			{
				log.error(string.Format("Memory at {0}:{1}", registerName, Utilities.getMemoryDump(address, 64)));
			}
		}

		private static int unsupportedSyscall(int code)
		{
			if (ignoreUnmappedImportsSettingsListerner == null)
			{
				ignoreUnmappedImportsSettingsListerner = new IgnoreUnmappedImportsSettingsListerner();
				Settings.Instance.registerSettingsListener("SyscallHandler", "emu.ignoreUnmappedImports", ignoreUnmappedImportsSettingsListerner);
			}

			NIDMapper nidMapper = NIDMapper.Instance;
			CpuState cpu = Emulator.Processor.cpu;
			int result = cpu._ra;

			if (code == syscallUnmappedImport)
			{ // special code for unmapped imports
				string description = string.Format("0x{0:X8}", cpu.pc);
				// Search for the module & NID to provide a better description
				foreach (SceModule module in Managers.modules.values())
				{
					foreach (DeferredStub deferredStub in module.unresolvedImports)
					{
						if (deferredStub.ImportAddress == cpu.pc || deferredStub.ImportAddress == cpu.pc - 4)
						{
							description = deferredStub.ToString();
							break;
						}
					}
				}

				if (EnableIgnoreUnmappedImports)
				{
					log.warn(string.Format("IGNORING: Unmapped import at {0} - $a0=0x{1:X8} $a1=0x{2:X8} $a2=0x{3:X8}", description, cpu._a0, cpu._a1, cpu._a2));
				}
				else
				{
					log.error(string.Format("Unmapped import at {0}:", description));
					log.error(string.Format("Registers: $a0=0x{0:X8}, $a1=0x{1:X8}, $a2=0x{2:X8}, $a3=0x{3:X8}", cpu._a0, cpu._a1, cpu._a2, cpu._a3));
					log.error(string.Format("           $t0=0x{0:X8}, $t1=0x{1:X8}, $t2=0x{2:X8}, $t3=0x{3:X8}", cpu._t0, cpu._t1, cpu._t2, cpu._t3));
					log.error(string.Format("           $ra=0x{0:X8}, $sp=0x{1:X8}", cpu._ra, cpu._sp));
					Memory mem = Emulator.Memory;
					log.error(string.Format("Caller code:"));
					for (int i = -96; i <= 40; i += 4)
					{
						int address = cpu._ra + i;
						int opcode = mem.read32(address);
						Instruction insn = Decoder.instruction(opcode);
						string disasm = insn.disasm(address, opcode);
						log.error(string.Format("{0} 0x{1:X8}:[{2:X8}]: {3}", i == -8 ? '>' : ' ', address, opcode, disasm));
					}
					logMem(mem, cpu._a0, Common.gprNames[Common._a0]);
					logMem(mem, cpu._a1, Common.gprNames[Common._a1]);
					logMem(mem, cpu._a2, Common.gprNames[Common._a2]);
					logMem(mem, cpu._a3, Common.gprNames[Common._a3]);
					logMem(mem, cpu._t0, Common.gprNames[Common._t0]);
					logMem(mem, cpu._t1, Common.gprNames[Common._t1]);
					logMem(mem, cpu._t2, Common.gprNames[Common._t2]);
					logMem(mem, cpu._t3, Common.gprNames[Common._t3]);
					Emulator.PauseEmu();
				}
				cpu._v0 = 0;
			}
			else if (code == syscallLoadCoreUnmappedImport)
			{
				string description = string.Format("0x{0:X8}", cpu.pc);
				// Search for the module & NID to provide a better description
				foreach (SceModule module in Managers.modules.values())
				{
					foreach (DeferredStub deferredStub in module.unresolvedImports)
					{
						if (deferredStub.ImportAddress == cpu.pc || deferredStub.ImportAddress == cpu.pc - 4)
						{
							description = deferredStub.ToString();
							break;
						}
					}
				}

				if (EnableIgnoreUnmappedImports)
				{
					log.warn(string.Format("IGNORING: Unmapped import at {0} - $a0=0x{1:X8} $a1=0x{2:X8} $a2=0x{3:X8}", description, cpu._a0, cpu._a1, cpu._a2));
				}
				else
				{
					log.error(string.Format("Unmapped import at {0}:", description));
					log.error(string.Format("Registers: $a0=0x{0:X8}, $a1=0x{1:X8}, $a2=0x{2:X8}, $a3=0x{3:X8}", cpu._a0, cpu._a1, cpu._a2, cpu._a3));
					log.error(string.Format("           $t0=0x{0:X8}, $t1=0x{1:X8}, $t2=0x{2:X8}, $t3=0x{3:X8}", cpu._t0, cpu._t1, cpu._t2, cpu._t3));
					log.error(string.Format("           $ra=0x{0:X8}, $sp=0x{1:X8}", cpu._ra, cpu._sp));
					Memory mem = Emulator.Memory;
					log.error(string.Format("Caller code:"));
					for (int i = -96; i <= 40; i += 4)
					{
						int address = cpu._ra + i;
						int opcode = mem.read32(address);
						Instruction insn = Decoder.instruction(opcode);
						string disasm = insn.disasm(address, opcode);
						log.error(string.Format("{0} 0x{1:X8}:[{2:X8}]: {3}", i == -8 ? '>' : ' ', address, opcode, disasm));
					}
					logMem(mem, cpu._a0, Common.gprNames[Common._a0]);
					logMem(mem, cpu._a1, Common.gprNames[Common._a1]);
					logMem(mem, cpu._a2, Common.gprNames[Common._a2]);
					logMem(mem, cpu._a3, Common.gprNames[Common._a3]);
					logMem(mem, cpu._t0, Common.gprNames[Common._t0]);
					logMem(mem, cpu._t1, Common.gprNames[Common._t1]);
					logMem(mem, cpu._t2, Common.gprNames[Common._t2]);
					logMem(mem, cpu._t3, Common.gprNames[Common._t3]);
					Emulator.PauseEmu();
				}
				cpu._v0 = 0;
			}
			else
			{
				int address = nidMapper.getAddressBySyscall(code);

				if (address != 0)
				{
					if (log.DebugEnabled)
					{
						string name = nidMapper.getNameBySyscall(code);
						int nid = nidMapper.getNidBySyscall(code);
						if (!string.ReferenceEquals(name, null))
						{
							log.debug(string.Format("Jumping to 0x{0:X8} instead of overwritten syscall {1}[0x{2:X8}]", address, name, nid));
						}
						else
						{
							log.debug(string.Format("Jumping to 0x{0:X8} instead of overwritten syscall NID 0x{1:X8}", address, nid));
						}
					}

					RuntimeContext.executeFunction(address);
				}
				else
				{
					// Check if this is the syscall
					// for an HLE function currently being uninstalled
					string name = nidMapper.getNameBySyscall(code);
					if (!string.ReferenceEquals(name, null))
					{
						log.error(string.Format("HLE Function {0} not activated by default for Firmware Version {1:D}", name, Emulator.Instance.FirmwareVersion));
					}
					else
					{
						int nid = nidMapper.getNidBySyscall(code);
						if (nid != 0)
						{
							log.error(string.Format("NID 0x{0:X8} not activated by default for Firmware Version {1:D}", nid, Emulator.Instance.FirmwareVersion));
						}
						else
						{
							log.error(string.Format("Unknown syscall 0x{0:X5}", code));
						}
					}
				}
			}

			return result;
		}

		public static int syscall(int code, bool inDelaySlot)
		{
			if (RuntimeContextLLE.LLEActive)
			{
				Processor processor = Emulator.Processor;
				if (log.DebugEnabled)
				{
					log.debug(string.Format("0x{0:X8} - syscall 0x{1:X5}", processor.cpu.pc, code));
				}

				// For LLE syscalls, trigger a syscall exception
				return RuntimeContextLLE.triggerSyscallException(processor, code, inDelaySlot);
			}

			// All HLE syscalls are now implemented natively in the compiler
			return unsupportedSyscall(code);
		}
	}
}