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

	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;

	public class sceMemab : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceMemab");
		private System.Random random = new System.Random();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6DD7339A, version = 150) public int sceMemab_6DD7339A()
		[HLEFunction(nid : 0x6DD7339A, version : 150)]
		public virtual int sceMemab_6DD7339A()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD57856A7, version = 150) public int sceMemab_D57856A7()
		[HLEFunction(nid : 0xD57856A7, version : 150)]
		public virtual int sceMemab_D57856A7()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF742F283, version = 150) public int sceMemab_F742F283(int unknown1, int unknown2, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=16, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown3, int unknown4)
		[HLEFunction(nid : 0xF742F283, version : 150)]
		public virtual int sceMemab_F742F283(int unknown1, int unknown2, TPointer unknown3, int unknown4)
		{
			unknown3.clear(16);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4B54EAAD, version = 150) public int sceMemab_4B54EAAD(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=192, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknownOutput1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=160, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknownOutput2)
		[HLEFunction(nid : 0x4B54EAAD, version : 150)]
		public virtual int sceMemab_4B54EAAD(TPointer unknownOutput1, TPointer unknownOutput2)
		{
			unknownOutput1.clear(192);
			RuntimeContext.debugMemory(unknownOutput1.Address, 192);
			unknownOutput2.clear(160);
			RuntimeContext.debugMemory(unknownOutput2.Address, 160);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9BF0C95D, version = 150) public int sceMemab_9BF0C95D(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=192, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknownOutput1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=160, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknownInput, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=272, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknownOutput2)
		[HLEFunction(nid : 0x9BF0C95D, version : 150)]
		public virtual int sceMemab_9BF0C95D(TPointer unknownOutput1, TPointer unknownInput, TPointer unknownOutput2)
		{
			RuntimeContext.debugMemory(unknownInput.Address, 160);
			unknownOutput1.clear(192);
			RuntimeContext.debugMemory(unknownOutput1.Address, 192);
			unknownOutput2.clear(272);
			RuntimeContext.debugMemory(unknownOutput2.Address, 272);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC3981EE1, version = 150) public int sceMemab_C3981EE1(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=192, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer unknownInputOuput, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=272, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknownInput, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=256, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknownOutput)
		[HLEFunction(nid : 0xC3981EE1, version : 150)]
		public virtual int sceMemab_C3981EE1(TPointer unknownInputOuput, TPointer unknownInput, TPointer unknownOutput)
		{
			unknownInputOuput.clear(192);
			RuntimeContext.debugMemory(unknownInputOuput.Address, 192);
			RuntimeContext.debugMemory(unknownInput.Address, 272);
			unknownOutput.clear(256);
			RuntimeContext.debugMemory(unknownOutput.Address, 256);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8ABE3445, version = 150) public int sceMemab_8ABE3445(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=192, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknownOutput, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=96, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknownInput)
		[HLEFunction(nid : 0x8ABE3445, version : 150)]
		public virtual int sceMemab_8ABE3445(TPointer unknownOutput, TPointer unknownInput)
		{
			unknownOutput.clear(192);
			RuntimeContext.debugMemory(unknownOutput.Address, 192);
			RuntimeContext.debugMemory(unknownInput.Address, 96);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x23E4659B, version = 150) public int sceMemab_23E4659B(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=192, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer unknownInputOutput1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=256, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer unknownInputOutput2, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=96, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknownOutput)
		[HLEFunction(nid : 0x23E4659B, version : 150)]
		public virtual int sceMemab_23E4659B(TPointer unknownInputOutput1, TPointer unknownInputOutput2, TPointer unknownOutput)
		{
			unknownInputOutput1.clear(192);
			RuntimeContext.debugMemory(unknownInputOutput1.Address, 192);
			unknownInputOutput2.clear(256);
			RuntimeContext.debugMemory(unknownInputOutput2.Address, 256);
			unknownOutput.clear(96);
			RuntimeContext.debugMemory(unknownOutput.Address, 96);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCB5D3916, version = 150) public int sceMemab_CB5D3916(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=60, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer unknownInputOutput, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknownInput, int inputLength, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=32, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknownOutput, int unknown1, int unknown2)
		[HLEFunction(nid : 0xCB5D3916, version : 150)]
		public virtual int sceMemab_CB5D3916(TPointer unknownInputOutput, TPointer unknownInput, int inputLength, TPointer unknownOutput, int unknown1, int unknown2)
		{
			unknownInputOutput.clear(60);
			RuntimeContext.debugMemory(unknownInputOutput.Address, 60);
			RuntimeContext.debugMemory(unknownInput.Address, inputLength);
			unknownOutput.clear(32);
			RuntimeContext.debugMemory(unknownOutput.Address, 32);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD47A50B1, version = 150) public int sceMemab_D47A50B1(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=76, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer unknownInputOutput, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknownInput, int inputLength, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=32, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknownOutput, int unknown1, int unknown2)
		[HLEFunction(nid : 0xD47A50B1, version : 150)]
		public virtual int sceMemab_D47A50B1(TPointer unknownInputOutput, TPointer unknownInput, int inputLength, TPointer unknownOutput, int unknown1, int unknown2)
		{
			unknownInputOutput.clear(76);
			RuntimeContext.debugMemory(unknownInputOutput.Address, 76);
			RuntimeContext.debugMemory(unknownInput.Address, inputLength);
			unknownOutput.clear(32);
			RuntimeContext.debugMemory(unknownOutput.Address, 32);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3C15BC8C, version = 150) public int sceMemab_3C15BC8C(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=68, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer unknownInputOutput, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknownInput, int inputLength, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=16, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknownInput2, int unknown2)
		[HLEFunction(nid : 0x3C15BC8C, version : 150)]
		public virtual int sceMemab_3C15BC8C(TPointer unknownInputOutput, TPointer unknownInput, int inputLength, TPointer unknownInput2, int unknown2)
		{
			unknownInputOutput.clear(68);
			RuntimeContext.debugMemory(unknownInputOutput.Address, 68);
			RuntimeContext.debugMemory(unknownInput.Address, inputLength);
			RuntimeContext.debugMemory(unknownInput2.Address, 16);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x16594684, version = 150) public int sceMemab_16594684(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=16, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0x16594684, version : 150)]
		public virtual int sceMemab_16594684(TPointer buffer)
		{
			// Generates 4 pseudo-random numbers (PSP_KIRK_CMD_PRNG)
			for (int i = 0; i < 4; i++)
			{
				buffer.setValue32(i << 2, random.Next());
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9DE8C8CD, version = 150) public int sceMemab_9DE8C8CD(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=16, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer xorKey, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer buffer, int bufferLength)
		[HLEFunction(nid : 0x9DE8C8CD, version : 150)]
		public virtual int sceMemab_9DE8C8CD(TPointer xorKey, TPointer buffer, int bufferLength)
		{
			// Encrypting (PSP_KIRK_CMD_ENCRYPT) the data in unknownInputOutput buffer
			RuntimeContext.debugMemory(buffer.Address, bufferLength);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9BF1A0A4, version = 150) public int sceMemab_9BF1A0A4(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=16, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer xorKey, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer buffer, int bufferLength)
		[HLEFunction(nid : 0x9BF1A0A4, version : 150)]
		public virtual int sceMemab_9BF1A0A4(TPointer xorKey, TPointer buffer, int bufferLength)
		{
			// Decrypting (PSP_KIRK_CMD_DECRYPT) the data in unknownInputOutput buffer
			RuntimeContext.debugMemory(buffer.Address, bufferLength);

			return 0;
		}
	}

}