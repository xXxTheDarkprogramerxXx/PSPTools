using System.Threading;

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
namespace pspsharp.graphics.RE.externalge
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.RuntimeContext.setLog4jMDC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.END;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.FINISH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.NativeUtils.INTR_STAT_END;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.NativeUtils.INTR_STAT_FINISH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.NativeUtils.INTR_STAT_SIGNAL;


	using PspGeList = pspsharp.HLE.kernel.types.PspGeList;
	using sceGe_user = pspsharp.HLE.modules.sceGe_user;

	//using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class CoreThread : Thread
	{
		protected internal static Logger log = ExternalGE.log;
		private static CoreThread instance;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private volatile bool exit_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private Semaphore sync_Renamed;
		private volatile bool insideRendering;

		public static CoreThread Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CoreThread();
					instance.Daemon = true;
					instance.Name = "ExternalGE - Core Thread";
					instance.Start();
				}
    
				return instance;
			}
		}

		public static void exit()
		{
			if (instance != null)
			{
				instance.exit_Renamed = true;
				instance = null;
			}
		}

		private CoreThread()
		{
			sync_Renamed = new Semaphore(0);
		}

		public override void run()
		{
			setLog4jMDC();
			bool doCoreInterpret = false;

			while (!exit_Renamed)
			{
				PspGeList list = ExternalGE.CurrentList;

				if (list == null)
				{
					if (!Emulator.pause && log.DebugEnabled)
					{
						Console.WriteLine(string.Format("CoreThread no current list available... waiting"));
					}

					waitForSync(100);
				}
				else if (doCoreInterpret || list.waitForSync(100))
				{
					InsideRendering = true;

					doCoreInterpret = false;
					NativeUtils.CoreMadr = list.Pc;
					NativeUtils.updateMemoryUnsafeAddr();

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("CoreThread processing {0}", list));
					}

					while (NativeUtils.coreInterpret())
					{
						NativeUtils.updateMemoryUnsafeAddr();

						//if (log.DebugEnabled)
						{
							list.Pc = NativeUtils.CoreMadr;
							Console.WriteLine(string.Format("CoreThread looping {0}", list));
						}

						if (ExternalGE.numberRendererThread > 0 && NativeUtils.RendererIndexCount > 0)
						{
							break;
						}
					}

					list.Pc = NativeUtils.CoreMadr;

					int intrStat = NativeUtils.CoreIntrStat;
					if ((intrStat & INTR_STAT_END) != 0)
					{
						if ((intrStat & INTR_STAT_SIGNAL) != 0)
						{
							executeCommandSIGNAL(list);
						}
						if ((intrStat & INTR_STAT_FINISH) != 0)
						{
							executeCommandFINISH(list);
						}
						intrStat &= ~(INTR_STAT_END | INTR_STAT_SIGNAL | INTR_STAT_FINISH);
						NativeUtils.CoreIntrStat = intrStat;
					}

					if (ExternalGE.numberRendererThread > 0 && NativeUtils.RendererIndexCount > 0)
					{
						ExternalGE.render();
						doCoreInterpret = true;
					}

					InsideRendering = false;
				}
			}

			log.info(string.Format("CoreThread exited"));
		}

		public virtual void sync()
		{
			if (sync_Renamed != null)
			{
				sync_Renamed.release();
			}
		}

		private bool waitForSync(int millis)
		{
			while (true)
			{
				try
				{
					int availablePermits = sync_Renamed.drainPermits();
					if (availablePermits > 0)
					{
						break;
					}

					if (sync_Renamed.tryAcquire(millis, TimeUnit.MILLISECONDS))
					{
						break;
					}
					return false;
				}
				catch (InterruptedException e)
				{
					// Ignore exception and retry again
					Console.WriteLine(string.Format("CoreThread waitForSync {0}", e));
				}
			}

			return true;
		}

		private static int command(int instruction)
		{
			return (int)((uint)instruction >> 24);
		}

		private void executeCommandFINISH(PspGeList list)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("FINISH {0}", list));
			}

			list.clearRestart();
			list.finishList();
			list.pushFinishCallback(list.id, NativeUtils.getCoreCmdArray(GeCommands.FINISH) & 0x00FFFFFF);
			list.endList();
			list.status = sceGe_user.PSP_GE_LIST_DONE;
			ExternalGE.finishList(list);
		}

		private void executeCommandSIGNAL(PspGeList list)
		{
			int args = NativeUtils.getCoreCmdArray(GeCommands.SIGNAL) & 0x00FFFFFF;
			int behavior = (args >> 16) & 0xFF;
			int signal = args & 0xFFFF;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("SIGNAL (behavior={0:D}, signal=0x{1:X})", behavior, signal));
			}

			switch (behavior)
			{
				case sceGe_user.PSP_GE_SIGNAL_SYNC:
				{
					// Skip FINISH / END
					Memory mem = Memory.Instance;
					if (command(mem.read32(list.Pc)) == FINISH)
					{
						list.readNextInstruction();
						if (command(mem.read32(list.Pc)) == END)
						{
							list.readNextInstruction();
						}
					}
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("PSP_GE_SIGNAL_SYNC ignored PC: 0x{0:X8}", list.Pc));
					}
					break;
				}
				case sceGe_user.PSP_GE_SIGNAL_CALL:
				{
					// Call list using absolute address from SIGNAL + END.
					int hi16 = signal & 0x0FFF;
					int lo16 = NativeUtils.getCoreCmdArray(GeCommands.END) & 0xFFFF;
					int addr = (hi16 << 16) | lo16;
					int oldPc = list.Pc;
					list.callAbsolute(addr);
					int newPc = list.Pc;
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("PSP_GE_SIGNAL_CALL old PC: 0x{0:X8}, new PC: 0x{1:X8}", oldPc, newPc));
					}
					break;
				}
				case sceGe_user.PSP_GE_SIGNAL_RETURN:
				{
					// Return from PSP_GE_SIGNAL_CALL.
					int oldPc = list.Pc;
					list.ret();
					int newPc = list.Pc;
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("PSP_GE_SIGNAL_RETURN old PC: 0x{0:X8}, new PC: 0x{1:X8}", oldPc, newPc));
					}
					break;
				}
				case sceGe_user.PSP_GE_SIGNAL_TBP0_REL:
				case sceGe_user.PSP_GE_SIGNAL_TBP1_REL:
				case sceGe_user.PSP_GE_SIGNAL_TBP2_REL:
				case sceGe_user.PSP_GE_SIGNAL_TBP3_REL:
				case sceGe_user.PSP_GE_SIGNAL_TBP4_REL:
				case sceGe_user.PSP_GE_SIGNAL_TBP5_REL:
				case sceGe_user.PSP_GE_SIGNAL_TBP6_REL:
				case sceGe_user.PSP_GE_SIGNAL_TBP7_REL:
				{
					// Overwrite TBPn and TBPw with SIGNAL + END (uses relative address only).
					int hi16 = signal & 0xFFFF;
					int end = NativeUtils.getCoreCmdArray(GeCommands.END);
					int lo16 = end & 0xFFFF;
					int width = (end >> 16) & 0xFF;
					int addr = list.getAddressRel((hi16 << 16) | lo16);
					int tbpValue = (behavior - sceGe_user.PSP_GE_SIGNAL_TBP0_REL + GeCommands.TBP0) << 24 | (addr & 0x00FFFFFF);
					int tbwValue = (behavior - sceGe_user.PSP_GE_SIGNAL_TBP0_REL + GeCommands.TBW0) << 24 | ((addr >> 8) & 0x00FF0000) | (width & 0xFFFF);
					NativeUtils.interpretCoreCmd(command(tbpValue), tbpValue, NativeUtils.CoreMadr);
					NativeUtils.interpretCoreCmd(command(tbwValue), tbwValue, NativeUtils.CoreMadr);
					break;
				}
				case sceGe_user.PSP_GE_SIGNAL_TBP0_REL_OFFSET:
				case sceGe_user.PSP_GE_SIGNAL_TBP1_REL_OFFSET:
				case sceGe_user.PSP_GE_SIGNAL_TBP2_REL_OFFSET:
				case sceGe_user.PSP_GE_SIGNAL_TBP3_REL_OFFSET:
				case sceGe_user.PSP_GE_SIGNAL_TBP4_REL_OFFSET:
				case sceGe_user.PSP_GE_SIGNAL_TBP5_REL_OFFSET:
				case sceGe_user.PSP_GE_SIGNAL_TBP6_REL_OFFSET:
				case sceGe_user.PSP_GE_SIGNAL_TBP7_REL_OFFSET:
				{
					// Overwrite TBPn and TBPw with SIGNAL + END (uses relative address with offset).
					int hi16 = signal & 0xFFFF;
					// Read & skip END
					int end = NativeUtils.getCoreCmdArray(GeCommands.END);
					int lo16 = end & 0xFFFF;
					int width = (end >> 16) & 0xFF;
					int addr = list.getAddressRelOffset((hi16 << 16) | lo16);
					int tbpValue = (behavior - sceGe_user.PSP_GE_SIGNAL_TBP0_REL + GeCommands.TBP0) << 24 | (addr & 0x00FFFFFF);
					int tbwValue = (behavior - sceGe_user.PSP_GE_SIGNAL_TBP0_REL + GeCommands.TBW0) << 24 | ((addr >> 8) & 0x00FF0000) | (width & 0xFFFF);
					NativeUtils.interpretCoreCmd(command(tbpValue), tbpValue, NativeUtils.CoreMadr);
					NativeUtils.interpretCoreCmd(command(tbwValue), tbwValue, NativeUtils.CoreMadr);
					break;
				}
				case sceGe_user.PSP_GE_SIGNAL_HANDLER_SUSPEND:
				case sceGe_user.PSP_GE_SIGNAL_HANDLER_CONTINUE:
				case sceGe_user.PSP_GE_SIGNAL_HANDLER_PAUSE:
				{
					list.clearRestart();
					list.pushSignalCallback(list.id, behavior, signal);
					list.endList();
					list.status = sceGe_user.PSP_GE_LIST_END_REACHED;
					break;
				}
				default:
				{
					if (log.InfoEnabled)
					{
						Console.WriteLine(string.Format("SIGNAL (behavior={0:D}, signal=0x{1:X}) unknown behavior at 0x{2:X8}", behavior, signal, list.Pc - 4));
					}
				}
			break;
			}

			if (list.Drawing)
			{
				list.sync();
				NativeUtils.setCoreCtrlActive();
			}
		}

		public virtual bool InsideRendering
		{
			get
			{
				return insideRendering;
			}
			set
			{
				this.insideRendering = value;
			}
		}

	}

}