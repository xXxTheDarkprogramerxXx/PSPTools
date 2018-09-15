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
	using Usage = pspsharp.HLE.BufferInfo.Usage;

	//using Logger = org.apache.log4j.Logger;

	public class sceHprm : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceHprm");

		private bool enableRemote = false;
		private bool enableHeadphone = false;
		private bool enableMicrophone = false;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC7154136, version = 150) public int sceHprmRegisterCallback()
		[HLEFunction(nid : 0xC7154136, version : 150)]
		public virtual int sceHprmRegisterCallback()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x444ED0B7, version = 150) public int sceHprmUnregisterCallback()
		[HLEFunction(nid : 0x444ED0B7, version : 150)]
		public virtual int sceHprmUnregisterCallback()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x71B5FB67, version = 150) public int sceHprmGetHpDetect()
		[HLEFunction(nid : 0x71B5FB67, version : 150)]
		public virtual int sceHprmGetHpDetect()
		{
			return 0;
		}

		[HLEFunction(nid : 0x208DB1BD, version : 150)]
		public virtual bool sceHprmIsRemoteExist()
		{
			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("sceHprmIsRemoteExist returning %b", enableRemote));
				Console.WriteLine(string.Format("sceHprmIsRemoteExist returning %b", enableRemote));
			}

			return enableRemote;
		}

		[HLEFunction(nid : 0x7E69EDA4, version : 150)]
		public virtual bool sceHprmIsHeadphoneExist()
		{
			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("sceHprmIsHeadphoneExist returning %b", enableHeadphone));
				Console.WriteLine(string.Format("sceHprmIsHeadphoneExist returning %b", enableHeadphone));
			}

			return enableHeadphone;
		}

		[HLEFunction(nid : 0x219C58F1, version : 150)]
		public virtual bool sceHprmIsMicrophoneExist()
		{
			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("sceHprmIsMicrophoneExist returning %b", enableMicrophone));
				Console.WriteLine(string.Format("sceHprmIsMicrophoneExist returning %b", enableMicrophone));
			}

			return enableMicrophone;
		}

		[HLEFunction(nid : 0x1910B327, version : 150)]
		public virtual int sceHprmPeekCurrentKey(TPointer32 keyAddr)
		{
			keyAddr.setValue(0); // fake

			return 0; // check
		}

		[HLEFunction(nid : 0x2BCEC83E, version : 150)]
		public virtual int sceHprmPeekLatch(TPointer latchAddr)
		{
			return 0;
		}

		[HLEFunction(nid : 0x40D2F9F0, version : 150)]
		public virtual int sceHprmReadLatch(TPointer32 latchAddr)
		{
			// Return dummy values
			latchAddr.setValue(0, 0);
			latchAddr.setValue(4, 0);
			latchAddr.setValue(8, 0);
			latchAddr.setValue(12, 0);

			return 0;
		}

		/// <returns> 0 - Cable not connected </returns>
		/// <returns> 1 - S-Video Cable / AV (composite) cable </returns>
		/// <returns> 2 - D Terminal Cable / Component Cable </returns>
		/// <returns> < 0 - Error
		///  </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1528D408, version = 150) public int sceHprm_driver_1528D408()
		[HLEFunction(nid : 0x1528D408, version : 150)]
		public virtual int sceHprm_driver_1528D408()
		{
			return 1;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDC895B2B, version = 660) public int sceHprm_driver_DC895B2B()
		[HLEFunction(nid : 0xDC895B2B, version : 660)]
		public virtual int sceHprm_driver_DC895B2B()
		{
			return 0;
		}

		[HLEFunction(nid : 0xE9B776BE, version : 660)]
		public virtual int sceHprmReadLatch_660(TPointer32 latchAddr)
		{
			return sceHprmReadLatch(latchAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBAD0828E, version = 150) public int sceHprmGetModel(@CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown1, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown2, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown3, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown4)
		[HLEFunction(nid : 0xBAD0828E, version : 150)]
		public virtual int sceHprmGetModel(TPointer32 unknown1, TPointer32 unknown2, TPointer32 unknown3, TPointer32 unknown4)
		{
			// Return dummy values
			unknown1.setValue(0);
			unknown2.setValue(0);
			unknown3.setValue(0);
			unknown4.setValue(0);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0B83352B, version = 660) public int sceHprmGetModel_660(@CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown1, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown2, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown3, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown4)
		[HLEFunction(nid : 0x0B83352B, version : 660)]
		public virtual int sceHprmGetModel_660(TPointer32 unknown1, TPointer32 unknown2, TPointer32 unknown3, TPointer32 unknown4)
		{
			return sceHprmGetModel(unknown1, unknown2, unknown3, unknown4);
		}
	}
}