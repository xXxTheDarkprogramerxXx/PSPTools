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
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;

	using Logger = org.apache.log4j.Logger;

	public class sceNpService : HLEModule
	{
		public static Logger log = Modules.getLogger("sceNpService");

		private bool initialized;
		private int npManagerMemSize; // Memory allocated by the NP Manager utility.
		private int npManagerMaxMemSize; // Maximum memory used by the NP Manager utility.
		private int npManagerFreeMemSize; // Free memory available to use by the NP Manager utility.
		private int dummyNumberFriends = 5;
		private int dummyNumberBlockList = 2;

		public override void start()
		{
			initialized = false;
			base.start();
		}

		protected internal virtual void checkInitialized()
		{
			if (!initialized)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_NPSERVICE_NOT_INIT);
			}
		}

		/// <summary>
		/// Initialization.
		/// </summary>
		/// <param name="poolSize"> </param>
		/// <param name="stackSize"> </param>
		/// <param name="threadPriority">
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0F8F5821, version = 150, checkInsideInterrupt = true) public int sceNpServiceInit(int poolSize, int stackSize, int threadPriority)
		[HLEFunction(nid : 0x0F8F5821, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpServiceInit(int poolSize, int stackSize, int threadPriority)
		{
			npManagerMemSize = poolSize;
			npManagerMaxMemSize = poolSize / 2; // Dummy
			npManagerFreeMemSize = poolSize - 16; // Dummy.

			initialized = true;

			return 0;
		}

		/// <summary>
		/// Termination.
		/// @return
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x00ACFAC3, version = 150, checkInsideInterrupt = true) public int sceNpServiceTerm()
		[HLEFunction(nid : 0x00ACFAC3, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpServiceTerm()
		{
			initialized = false;
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x250488F9, version = 150, checkInsideInterrupt = true) public int sceNpServiceGetMemoryStat(pspsharp.HLE.TPointer32 memStatAddr)
		[HLEFunction(nid : 0x250488F9, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpServiceGetMemoryStat(TPointer32 memStatAddr)
		{
			checkInitialized();

			memStatAddr.setValue(0, npManagerMemSize);
			memStatAddr.setValue(4, npManagerMaxMemSize);
			memStatAddr.setValue(8, npManagerFreeMemSize);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBE22EEA3, version = 150) public int sceNpRosterCreateRequest()
		[HLEFunction(nid : 0xBE22EEA3, version : 150)]
		public virtual int sceNpRosterCreateRequest()
		{
			int requestId = 0x1234; // Dummy value for testing purpose

			return requestId;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x66C64821, version = 150) public int sceNpRosterDeleteRequest(int requestId)
		[HLEFunction(nid : 0x66C64821, version : 150)]
		public virtual int sceNpRosterDeleteRequest(int requestId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x58251346, version = 150) public int sceNpRosterGetFriendListEntryCount(int requestId, @CanBeNull pspsharp.HLE.TPointer32 options)
		[HLEFunction(nid : 0x58251346, version : 150)]
		public virtual int sceNpRosterGetFriendListEntryCount(int requestId, TPointer32 options)
		{
			if (options.NotNull && options.getValue() != 4)
			{
				return -1;
			}

			return dummyNumberFriends;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4E851B10, version = 150) public int sceNpRosterGetFriendListEntry(int requestId, int numEntries, int startIndex, pspsharp.HLE.TPointer32 countAddr, pspsharp.HLE.TPointer32 numberRetrieved, pspsharp.HLE.TPointer buffer, @CanBeNull pspsharp.HLE.TPointer32 options)
		[HLEFunction(nid : 0x4E851B10, version : 150)]
		public virtual int sceNpRosterGetFriendListEntry(int requestId, int numEntries, int startIndex, TPointer32 countAddr, TPointer32 numberRetrieved, TPointer buffer, TPointer32 options)
		{
			if (options.NotNull && options.getValue() != 4)
			{
				return -1;
			}

			countAddr.setValue(dummyNumberFriends);
			numberRetrieved.setValue(dummyNumberFriends);

			for (int i = 0; i < dummyNumberFriends; i++)
			{
				buffer.setStringNZ(i * 36, 16, string.Format("DummyFriend#{0:D}", i));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x72A1CE0D, version = 150) public int sceNpRosterDeleteFriendListEntry()
		[HLEFunction(nid : 0x72A1CE0D, version : 150)]
		public virtual int sceNpRosterDeleteFriendListEntry()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x75DACB57, version = 150) public int sceNpRosterAcceptFriendListEntry()
		[HLEFunction(nid : 0x75DACB57, version : 150)]
		public virtual int sceNpRosterAcceptFriendListEntry()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x788F2B5E, version = 150) public int sceNpRosterAddFriendListEntry()
		[HLEFunction(nid : 0x788F2B5E, version : 150)]
		public virtual int sceNpRosterAddFriendListEntry()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA01443AA, version = 150) public int sceNpRosterGetBlockListEntryCount(int requestId, pspsharp.HLE.TPointer32 options)
		[HLEFunction(nid : 0xA01443AA, version : 150)]
		public virtual int sceNpRosterGetBlockListEntryCount(int requestId, TPointer32 options)
		{
			if (options.NotNull && options.getValue() != 4)
			{
				return -1;
			}

			return dummyNumberBlockList;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x506C318D, version = 150) public int sceNpRosterGetBlockListEntry(int requestId, int numEntries, int startIndex, pspsharp.HLE.TPointer32 countAddr, pspsharp.HLE.TPointer32 numberRetrieved, pspsharp.HLE.TPointer buffer, @CanBeNull pspsharp.HLE.TPointer32 options)
		[HLEFunction(nid : 0x506C318D, version : 150)]
		public virtual int sceNpRosterGetBlockListEntry(int requestId, int numEntries, int startIndex, TPointer32 countAddr, TPointer32 numberRetrieved, TPointer buffer, TPointer32 options)
		{
			if (options.NotNull && options.getValue() != 4)
			{
				return -1;
			}

			countAddr.setValue(dummyNumberBlockList);
			numberRetrieved.setValue(dummyNumberBlockList);

			for (int i = 0; i < dummyNumberBlockList; i++)
			{
				buffer.setStringNZ(i * 36, 16, string.Format("DummyBlocked#{0:D}", i));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x174D0D24, version = 150) public int sceNpRosterDeleteBlockListEntry()
		[HLEFunction(nid : 0x174D0D24, version : 150)]
		public virtual int sceNpRosterDeleteBlockListEntry()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFC0BC8DB, version = 150) public int sceNpRosterAddBlockListEntry()
		[HLEFunction(nid : 0xFC0BC8DB, version : 150)]
		public virtual int sceNpRosterAddBlockListEntry()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1DA3E950, version = 150) public int sceNpManagerSigninUpdateInitStart()
		[HLEFunction(nid : 0x1DA3E950, version : 150)]
		public virtual int sceNpManagerSigninUpdateInitStart()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x168B8DE5, version = 150) public int sceNpManagerSigninUpdateGetStatus()
		[HLEFunction(nid : 0x168B8DE5, version : 150)]
		public virtual int sceNpManagerSigninUpdateGetStatus()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x78802D5F, version = 150) public int sceNpManagerSigninUpdateShutdownStart()
		[HLEFunction(nid : 0x78802D5F, version : 150)]
		public virtual int sceNpManagerSigninUpdateShutdownStart()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x268C009D, version = 150) public int sceNpManagerSigninUpdateAbort()
		[HLEFunction(nid : 0x268C009D, version : 150)]
		public virtual int sceNpManagerSigninUpdateAbort()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5494274B, version = 150) public int sceNpLookupCreateTransactionCtx()
		[HLEFunction(nid : 0x5494274B, version : 150)]
		public virtual int sceNpLookupCreateTransactionCtx()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA670D3A3, version = 150) public int sceNpLookupDestroyTransactionCtx()
		[HLEFunction(nid : 0xA670D3A3, version : 150)]
		public virtual int sceNpLookupDestroyTransactionCtx()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x90E4DB6A, version = 150) public int sceNpLookupUserProfile()
		[HLEFunction(nid : 0x90E4DB6A, version : 150)]
		public virtual int sceNpLookupUserProfile()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x389A0D44, version = 150) public int sceNpLookupNpId()
		[HLEFunction(nid : 0x389A0D44, version : 150)]
		public virtual int sceNpLookupNpId()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4B4E4E71, version = 150) public int sceNpLookupAbortTransaction()
		[HLEFunction(nid : 0x4B4E4E71, version : 150)]
		public virtual int sceNpLookupAbortTransaction()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5F5E32AF, version = 150) public int sceNpRosterAbort()
		[HLEFunction(nid : 0x5F5E32AF, version : 150)]
		public virtual int sceNpRosterAbort()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA164CACC, version = 150) public int sceNpRosterGetFriendListMessage()
		[HLEFunction(nid : 0xA164CACC, version : 150)]
		public virtual int sceNpRosterGetFriendListMessage()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC76F55ED, version = 150) public int sceNpLookupTitleSmallStorage()
		[HLEFunction(nid : 0xC76F55ED, version : 150)]
		public virtual int sceNpLookupTitleSmallStorage()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB2EA4530, version = 150) public int sceNpService_B2EA4530(pspsharp.HLE.TPointer64 unknown)
		[HLEFunction(nid : 0xB2EA4530, version : 150)]
		public virtual int sceNpService_B2EA4530(TPointer64 unknown)
		{
			// The returned value seems to be the ticket validity duration
			unknown.Value = 10 * 60 * 1000;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x63644C02, version = 150) public int sceNpService_63644C02()
		[HLEFunction(nid : 0x63644C02, version : 150)]
		public virtual int sceNpService_63644C02()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x00041295, version = 150) public int sceNpService_00041295()
		[HLEFunction(nid : 0x00041295, version : 150)]
		public virtual int sceNpService_00041295()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x41E99DD1, version = 150) public int sceNpService_41E99DD1()
		[HLEFunction(nid : 0x41E99DD1, version : 150)]
		public virtual int sceNpService_41E99DD1()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x43E635D2, version = 150) public int sceNpService_43E635D2(int unknown1, int unknown2)
		[HLEFunction(nid : 0x43E635D2, version : 150)]
		public virtual int sceNpService_43E635D2(int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x53F01574, version = 150) public int sceNpService_53F01574()
		[HLEFunction(nid : 0x53F01574, version : 150)]
		public virtual int sceNpService_53F01574()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x570DB15B, version = 150) public int sceNpService_570DB15B()
		[HLEFunction(nid : 0x570DB15B, version : 150)]
		public virtual int sceNpService_570DB15B()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5B74DE06, version = 150) public int sceNpService_5B74DE06(pspsharp.HLE.PspString onlineId)
		[HLEFunction(nid : 0x5B74DE06, version : 150)]
		public virtual int sceNpService_5B74DE06(PspString onlineId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x61AB2062, version = 150) public int sceNpService_61AB2062()
		[HLEFunction(nid : 0x61AB2062, version : 150)]
		public virtual int sceNpService_61AB2062()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x66B3E216, version = 150) public int sceNpService_66B3E216()
		[HLEFunction(nid : 0x66B3E216, version : 150)]
		public virtual int sceNpService_66B3E216()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x68DE4402, version = 150) public int sceNpService_68DE4402()
		[HLEFunction(nid : 0x68DE4402, version : 150)]
		public virtual int sceNpService_68DE4402()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x76867C01, version = 150) public int sceNpService_76867C01()
		[HLEFunction(nid : 0x76867C01, version : 150)]
		public virtual int sceNpService_76867C01()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7EF4312E, version = 150) public int sceNpService_7EF4312E(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=16, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer onlineId, int unknown1, int unknown2)
		[HLEFunction(nid : 0x7EF4312E, version : 150)]
		public virtual int sceNpService_7EF4312E(TPointer onlineId, int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7FA900FA, version = 150) public int sceNpService_7FA900FA()
		[HLEFunction(nid : 0x7FA900FA, version : 150)]
		public virtual int sceNpService_7FA900FA()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9CD89895, version = 150) public int sceNpService_9CD89895()
		[HLEFunction(nid : 0x9CD89895, version : 150)]
		public virtual int sceNpService_9CD89895()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA88B932E, version = 150) public int sceNpService_A88B932E()
		[HLEFunction(nid : 0xA88B932E, version : 150)]
		public virtual int sceNpService_A88B932E()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAD2C0674, version = 150) public int sceNpService_AD2C0674()
		[HLEFunction(nid : 0xAD2C0674, version : 150)]
		public virtual int sceNpService_AD2C0674()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB9D7D1A3, version = 150) public int sceNpService_B9D7D1A3()
		[HLEFunction(nid : 0xB9D7D1A3, version : 150)]
		public virtual int sceNpService_B9D7D1A3()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC5094BE6, version = 150) public int sceNpService_C5094BE6()
		[HLEFunction(nid : 0xC5094BE6, version : 150)]
		public virtual int sceNpService_C5094BE6()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCAF9E5FD, version = 150) public int sceNpService_CAF9E5FD()
		[HLEFunction(nid : 0xCAF9E5FD, version : 150)]
		public virtual int sceNpService_CAF9E5FD()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD05205D6, version = 150) public int sceNpService_D05205D6()
		[HLEFunction(nid : 0xD05205D6, version : 150)]
		public virtual int sceNpService_D05205D6()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD6E95870, version = 150) public int sceNpService_D6E95870()
		[HLEFunction(nid : 0xD6E95870, version : 150)]
		public virtual int sceNpService_D6E95870()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD7A0CD2D, version = 150) public int sceNpService_D7A0CD2D()
		[HLEFunction(nid : 0xD7A0CD2D, version : 150)]
		public virtual int sceNpService_D7A0CD2D()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD97B509F, version = 150) public int sceNpService_D97B509F()
		[HLEFunction(nid : 0xD97B509F, version : 150)]
		public virtual int sceNpService_D97B509F()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDD8B5E53, version = 150) public int sceNpService_DD8B5E53()
		[HLEFunction(nid : 0xDD8B5E53, version : 150)]
		public virtual int sceNpService_DD8B5E53()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE00CDEBB, version = 150) public int sceNpService_E00CDEBB()
		[HLEFunction(nid : 0xE00CDEBB, version : 150)]
		public virtual int sceNpService_E00CDEBB()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEBC076F3, version = 150) public int sceNpService_EBC076F3()
		[HLEFunction(nid : 0xEBC076F3, version : 150)]
		public virtual int sceNpService_EBC076F3()
		{
			return 0;
		}
	}
}