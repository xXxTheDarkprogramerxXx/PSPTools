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

	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using pspUsbMicInputInitExParam = pspsharp.HLE.kernel.types.pspUsbMicInputInitExParam;

	public class sceUsbMic : HLEModule
	{
		public static Logger log = Modules.getLogger("sceUsbMic");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x06128E42, version = 260) public int sceUsbMicPollInputEnd()
		[HLEFunction(nid : 0x06128E42, version : 260)]
		public virtual int sceUsbMicPollInputEnd()
		{
			return 0;
		}

		[HLEFunction(nid : 0x2E6DCDCD, version : 260)]
		public virtual int sceUsbMicInputBlocking(int maxSamples, int frequency, TPointer buffer)
		{
			if (maxSamples <= 0 || (maxSamples & 0x3F) != 0)
			{
				return SceKernelErrors.ERROR_USBMIC_INVALID_MAX_SAMPLES;
			}

			if (frequency != 44100 && frequency != 22050 && frequency != 11025)
			{
				return SceKernelErrors.ERROR_USBMIC_INVALID_FREQUENCY;
			}

			return Modules.sceAudioModule.hleAudioInputBlocking(maxSamples, frequency, buffer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x45310F07, version = 260) public int sceUsbMicInputInitEx(@CanBeNull pspsharp.HLE.kernel.types.pspUsbMicInputInitExParam param)
		[HLEFunction(nid : 0x45310F07, version : 260)]
		public virtual int sceUsbMicInputInitEx(pspUsbMicInputInitExParam param)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5F7F368D, version = 260) public int sceUsbMicInput()
		[HLEFunction(nid : 0x5F7F368D, version : 260)]
		public virtual int sceUsbMicInput()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x63400E20, version = 260) public int sceUsbMicGetInputLength()
		[HLEFunction(nid : 0x63400E20, version : 260)]
		public virtual int sceUsbMicGetInputLength()
		{
			return Modules.sceAudioModule.hleAudioGetInputLength();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB8E536EB, version = 260) public int sceUsbMicInputInit(int unknown1, int inputVolume, int unknown2)
		[HLEFunction(nid : 0xB8E536EB, version : 260)]
		public virtual int sceUsbMicInputInit(int unknown1, int inputVolume, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF899001C, version = 260) public int sceUsbMicWaitInputEnd()
		[HLEFunction(nid : 0xF899001C, version : 260)]
		public virtual int sceUsbMicWaitInputEnd()
		{
			return 0;
		}
	}

}