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

	public class sceDNASCore : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceDNASCore");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFA571A75, version = 150) public int sceDNASCoreInit()
		[HLEFunction(nid : 0xFA571A75, version : 150)]
		public virtual int sceDNASCoreInit()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD5E80301, version = 150) public int sceDNASCoreTerm()
		[HLEFunction(nid : 0xD5E80301, version : 150)]
		public virtual int sceDNASCoreTerm()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x15096ECD, version = 150) public int sceDNASCoreGetHostname(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=128, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer hostname)
		[HLEFunction(nid : 0x15096ECD, version : 150)]
		public virtual int sceDNASCoreGetHostname(TPointer hostname)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2370130E, version = 150) public int sceDNASCoreCheckProxyResponse()
		[HLEFunction(nid : 0x2370130E, version : 150)]
		public virtual int sceDNASCoreCheckProxyResponse()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x26E1E2BD, version = 150) public int sceDNASCoreSetChallenge()
		[HLEFunction(nid : 0x26E1E2BD, version : 150)]
		public virtual int sceDNASCoreSetChallenge()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2B6C67EA, version = 150) public int sceDNASCoreCheckGameInfoFlag()
		[HLEFunction(nid : 0x2B6C67EA, version : 150)]
		public virtual int sceDNASCoreCheckGameInfoFlag()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4108128B, version = 150) public int sceDNASCoreMakeConnect()
		[HLEFunction(nid : 0x4108128B, version : 150)]
		public virtual int sceDNASCoreMakeConnect()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x80CEC43A, version = 150) public int sceDNASCoreMakeResponse()
		[HLEFunction(nid : 0x80CEC43A, version : 150)]
		public virtual int sceDNASCoreMakeResponse()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x822357BB, version = 150) public int sceDNASCoreGetResponse()
		[HLEFunction(nid : 0x822357BB, version : 150)]
		public virtual int sceDNASCoreGetResponse()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8309549E, version = 150) public int sceDNASCoreSetResult()
		[HLEFunction(nid : 0x8309549E, version : 150)]
		public virtual int sceDNASCoreSetResult()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB6C76A14, version = 150) public int sceDNASCoreCheckChallenge()
		[HLEFunction(nid : 0xB6C76A14, version : 150)]
		public virtual int sceDNASCoreCheckChallenge()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBA0A32CA, version = 150) public int sceDNASCoreCheckResult()
		[HLEFunction(nid : 0xBA0A32CA, version : 150)]
		public virtual int sceDNASCoreCheckResult()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBA0D27F8, version = 150) public int sceDNASCore_lib_BA0D27F8()
		[HLEFunction(nid : 0xBA0D27F8, version : 150)]
		public virtual int sceDNASCore_lib_BA0D27F8()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBF6A7475, version = 150) public int sceDNASCoreGetProductCode()
		[HLEFunction(nid : 0xBF6A7475, version : 150)]
		public virtual int sceDNASCoreGetProductCode()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC54657B7, version = 150) public int sceDNASCoreSetProxyResponse()
		[HLEFunction(nid : 0xC54657B7, version : 150)]
		public virtual int sceDNASCoreSetProxyResponse()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDA5939B4, version = 150) public int sceDNASCoreGetProxyRequest()
		[HLEFunction(nid : 0xDA5939B4, version : 150)]
		public virtual int sceDNASCoreGetProxyRequest()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF0EB4367, version = 150) public int sceDNASCoreGetConnect(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.returnValue, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer connectAddr, int Length)
		[HLEFunction(nid : 0xF0EB4367, version : 150)]
		public virtual int sceDNASCoreGetConnect(TPointer connectAddr, int Length)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF479F616, version = 150) public int sceDNASCoreGetHostnameBase(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=128, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer hostnameBase)
		[HLEFunction(nid : 0xF479F616, version : 150)]
		public virtual int sceDNASCoreGetHostnameBase(TPointer hostnameBase)
		{
			return 0;
		}
	}

}