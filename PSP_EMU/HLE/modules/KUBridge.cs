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
namespace pspsharp.HLE.modules
{
	using Logger = org.apache.log4j.Logger;

	using SceKernelLMOption = pspsharp.HLE.kernel.types.SceKernelLMOption;
	using LoadModuleContext = pspsharp.HLE.modules.ModuleMgrForUser.LoadModuleContext;
	using Model = pspsharp.hardware.Model;

	public class KUBridge : HLEModule
	{
		public static Logger log = Modules.getLogger("KUBridge");

		/*
		 * Equivalent to sceKernelLoadModule()
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x4C25EA72, version = 150) public int kuKernelLoadModule(pspsharp.HLE.PspString path, int flags, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0x4C25EA72, version : 150)]
		public virtual int kuKernelLoadModule(PspString path, int flags, TPointer optionAddr)
		{
			SceKernelLMOption lmOption = null;
			if (optionAddr.NotNull)
			{
				lmOption = new SceKernelLMOption();
				lmOption.read(optionAddr);
				if (log.InfoEnabled)
				{
					log.info(string.Format("kuKernelLoadModule options: {0}", lmOption));
				}
			}

			LoadModuleContext loadModuleContext = new LoadModuleContext();
			loadModuleContext.fileName = path.String;
			loadModuleContext.flags = flags;
			loadModuleContext.lmOption = lmOption;
			loadModuleContext.needModuleInfo = true;
			loadModuleContext.allocMem = true;

			return Modules.ModuleMgrForUserModule.hleKernelLoadModule(loadModuleContext);
		}

		/*
		 * Equivalent to sceKernelGetModel()
		 */
		[HLEFunction(nid : 0x24331850, version : 150)]
		public virtual int kuKernelGetModel()
		{
			int result = Model.Model;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("kuKernelGetModel returning {0:D}({1})", result, Model.getModelName(result)));
			}

			return result;
		}
	}

}