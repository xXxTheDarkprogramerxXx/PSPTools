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
	//using Logger = org.apache.log4j.Logger;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using GeCommands = pspsharp.graphics.GeCommands;

	public class sceDmacplus : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceDmacplus");
		public static readonly int[] pixelFormatFromCode = new int[] {GeCommands.PSM_32BIT_ABGR8888, GeCommands.PSM_16BIT_BGR5650, GeCommands.PSM_16BIT_ABGR5551, GeCommands.PSM_16BIT_ABGR4444};

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE9B746F9, version = 150) public int sceDmacplusLcdcDisable()
		[HLEFunction(nid : 0xE9B746F9, version : 150)]
		public virtual int sceDmacplusLcdcDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xED849260, version = 150) public int sceDmacplusLcdcEnable()
		[HLEFunction(nid : 0xED849260, version : 150)]
		public virtual int sceDmacplusLcdcEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x88ACB6F1, version = 150) public int sceDmacplusLcdcSetFormat(int displayWidth, int displayFrameBufferWidth, int displayPixelFormatCoded)
		[HLEFunction(nid : 0x88ACB6F1, version : 150)]
		public virtual int sceDmacplusLcdcSetFormat(int displayWidth, int displayFrameBufferWidth, int displayPixelFormatCoded)
		{
			int pixelFormat = pixelFormatFromCode[displayPixelFormatCoded];
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceDmacplusLcdcSetFormat pixelFormat={0:D}", pixelFormat));
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA3AA8D00, version = 150) public int sceDmacplusLcdcSetBaseAddr(int frameBufferAddress)
		[HLEFunction(nid : 0xA3AA8D00, version : 150)]
		public virtual int sceDmacplusLcdcSetBaseAddr(int frameBufferAddress)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3438DA0B, version = 150) public int sceDmacplusSc2MeLLI(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=16, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 dmacParameters)
		[HLEFunction(nid : 0x3438DA0B, version : 150)]
		public virtual int sceDmacplusSc2MeLLI(TPointer32 dmacParameters)
		{
			int src = dmacParameters.getValue(0);
			int dst = dmacParameters.getValue(4);
			int next = dmacParameters.getValue(8);
			int attributes = dmacParameters.getValue(12);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceDmacplusSc2MeLLI src=0x{0:X8}, dst=0x{1:X8}, next=0x{2:X8}, attributes=0x{3:X}", src, dst, next, attributes));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x282CA0D7, version = 660) public int sceDmacplusSc2MeLLI_660(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=16, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 dmacParameters)
		[HLEFunction(nid : 0x282CA0D7, version : 660)]
		public virtual int sceDmacplusSc2MeLLI_660(TPointer32 dmacParameters)
		{
			return sceDmacplusSc2MeLLI(dmacParameters);
		}
	}

}